using MotorControlSystemSimulationApi.Models.Entities;
using MotorControlSystemSimulationApi.Models.Enums;
using MotorControlSystemSimulationApi.Repositories.Interfaces;

namespace MotorControlSystemSimulationApi.Services;

/// <summary>
/// Caso de uso que aplica una orden de velocidad y guarda el nuevo estado,
/// una muestra de telemetría y el comando procesado.
/// Las ecuaciones de telemetría son heurísticas de simulación, no un modelo físico.
/// </summary>
public sealed class MotorSimulationService(
    IEntityRepository<MotorAssociated> associatedMotorRepository,
    IEntityRepository<MotorStateHistory> stateHistoryRepository,
    IEntityRepository<Command> commandRepository)
{
    private const decimal EcoSpeedFactor = 0.60m;
    private const decimal NormalSpeedFactor = 0.80m;
    private const decimal SportSpeedFactor = 1.00m;
    private const decimal ReferenceMaximumRpm = 6000m;
    private const decimal ReferenceOperatingMinutesPerCommand = 1m;

    public async Task<MotorStateResponse?> UpdateSpeedAsync(
        Guid associatedMotorId,
        UpdateSpeedRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.RequestedSpeed < 0)
            throw new ArgumentOutOfRangeException(nameof(request), "La velocidad solicitada no puede ser negativa.");
        if (!Enum.IsDefined(request.DrivingMode))
            throw new ArgumentOutOfRangeException(nameof(request), "El modo de conducción no es válido.");

        var associatedMotor = await associatedMotorRepository.GetByIdAsync(associatedMotorId, cancellationToken);
        if (associatedMotor is null)
            return null;
        if (associatedMotor.Motor is null || associatedMotor.Motor.MaximumSpeed <= 0)
            throw new InvalidOperationException("El motor asociado no tiene un motor válido con velocidad máxima configurada.");

        var motor = associatedMotor.Motor;
        var previousSpeed = associatedMotor.CurrentSpeed;
        var speedLimit = Math.Min(motor.MaximumSpeed, motor.MaximumSpeed * GetModeFactor(request.DrivingMode));
        var appliedSpeed = Math.Min(request.RequestedSpeed, speedLimit);
        var powerRatio = appliedSpeed / motor.MaximumSpeed;
        var rpm = (int)Math.Round(ReferenceMaximumRpm * powerRatio, MidpointRounding.AwayFromZero);
        var power = Math.Round(motor.MaximumPower * powerRatio, 2, MidpointRounding.AwayFromZero);
        var temperature = Math.Min(
            motor.MaximumTemperature,
            Math.Round(associatedMotor.CurrentTemperature + powerRatio, 2, MidpointRounding.AwayFromZero));
        var estimatedEnergyKwh = power * ReferenceOperatingMinutesPerCommand / 60m;
        var batteryUsePercent = motor.BatteryCapacityKwh > 0
            ? estimatedEnergyKwh / motor.BatteryCapacityKwh * 100m
            : 0m;
        var batteryLevel = Math.Max(0m, Math.Round(associatedMotor.BatteryLevel - batteryUsePercent, 2, MidpointRounding.AwayFromZero));
        var now = DateTimeOffset.UtcNow;

        associatedMotor.CurrentSpeed = appliedSpeed;
        associatedMotor.CurrentTemperature = temperature;
        associatedMotor.CurrentRpm = rpm;
        associatedMotor.CurrentPower = power;
        associatedMotor.BatteryLevel = batteryLevel;
        associatedMotor.DrivingMode = request.DrivingMode;
        await associatedMotorRepository.UpdateAsync(associatedMotor, cancellationToken);

        var state = new MotorStateHistory
        {
            MotorStateId = Guid.NewGuid(),
            AssociatedMotorId = associatedMotor.AssociatedMotorId,
            Timestamp = now.UtcDateTime,
            Speed = appliedSpeed,
            Temperature = temperature,
            Rpm = rpm,
            Power = power,
            BatteryLevel = batteryLevel
        };
        await stateHistoryRepository.AddAsync(state, cancellationToken);
        associatedMotor.StateHistory.Add(state);

        var commandType = appliedSpeed > previousSpeed
            ? CommandType.IncreaseSpeed
            : appliedSpeed < previousSpeed
                ? CommandType.DecreaseSpeed
                : request.RequestedSpeed == 0 ? CommandType.EmergencyStop : CommandType.MaintainSpeed;
        var command = new Command
        {
            AssociatedMotorId = associatedMotor.AssociatedMotorId,
            AssociatedMotor = associatedMotor,
            Type = commandType,
            Parameters = $"requestedSpeed={request.RequestedSpeed};appliedSpeed={appliedSpeed};mode={request.DrivingMode}",
            Result = request.RequestedSpeed == appliedSpeed
                ? "Command processed."
                : $"Speed capped at {appliedSpeed} km/h for the selected driving mode.",
            SentAt = now,
            ProcessedAt = now
        };
        await commandRepository.AddAsync(command, cancellationToken);
        associatedMotor.Commands.Add(command);

        return new MotorStateResponse(
            associatedMotor.AssociatedMotorId,
            request.RequestedSpeed,
            appliedSpeed,
            speedLimit,
            temperature,
            rpm,
            power,
            batteryLevel,
            request.DrivingMode,
            now);
    }

    private static decimal GetModeFactor(DrivingMode mode) => mode switch
    {
        DrivingMode.Eco => EcoSpeedFactor,
        DrivingMode.Normal => NormalSpeedFactor,
        DrivingMode.Sport => SportSpeedFactor,
        _ => throw new ArgumentOutOfRangeException(nameof(mode))
    };
}
