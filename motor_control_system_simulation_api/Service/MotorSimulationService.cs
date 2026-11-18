using MotorControlSystemSimulationApi.Models.Entities;
using MotorControlSystemSimulationApi.Models.Enums;
using MotorControlSystemSimulationApi.Repositories.Interfaces;

namespace MotorControlSystemSimulationApi.Services;

/// <summary>
/// Procesa órdenes de velocidad, actualiza el estado del motor asociado y
/// registra tanto la telemetría como el comando recibido.
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
    private const decimal SpeedReductionPerUpdateWhenBatteryEmpty = 10m;

    public async Task<MotorStateResponse?> UpdateSpeedAsync(
        Guid associatedMotorId,
        UpdateSpeedRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidateInputSpeed(request);

        var associatedMotor = await GetAssociatedMotorAsync(associatedMotorId, cancellationToken);
        if (associatedMotor is null)
            return null;

        var previousSpeed = associatedMotor.CurrentSpeed;
        var updatedAt = DateTimeOffset.UtcNow;

        var update = await UpdateAssociateMotorAsync(
            associatedMotor,
            request,
            updatedAt,
            cancellationToken);

        var stateHistory = CreateMotorStateHistory(associatedMotor, update, updatedAt);
        await stateHistoryRepository.AddAsync(stateHistory, cancellationToken);
        associatedMotor.StateHistory.Add(stateHistory);

        var commandType = GetCommandType(
            previousSpeed,
            update.AppliedSpeed,
            request.RequestedSpeed);

        var command = CreateCommand(
            associatedMotor,
            request,
            update.AppliedSpeed,
            commandType,
            updatedAt);

        await commandRepository.AddAsync(command, cancellationToken);
        associatedMotor.Commands.Add(command);

        return CreateMotorStateResponse(associatedMotor, request, update, updatedAt);
    }

    /// <summary>Valida la solicitud y lanza una excepción si los datos no son válidos.</summary>
    private static void ValidateInputSpeed(UpdateSpeedRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.RequestedSpeed < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                "La velocidad solicitada no puede ser negativa.");
        }

        if (!Enum.IsDefined(request.DrivingMode))
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                "El modo de conducción no es válido.");
        }
    }

    /// <summary>Busca el motor asociado y valida que tenga una configuración de motor utilizable.</summary>
    private async Task<MotorAssociated?> GetAssociatedMotorAsync(
        Guid associatedMotorId,
        CancellationToken cancellationToken)
    {
        var associatedMotor = await associatedMotorRepository.GetByIdAsync(
            associatedMotorId,
            cancellationToken);

        if (associatedMotor is null)
            return null;

        if (associatedMotor.Motor is null || associatedMotor.Motor.MaximumSpeed <= 0)
        {
            throw new InvalidOperationException(
                "El motor asociado no tiene un motor válido con velocidad máxima configurada.");
        }

        if (associatedMotor.OperationState != MotorOperationState.InTrip)
        {
            throw new MotorNotInTripException(
                "La velocidad solo se puede modificar cuando el motor asociado está en un viaje.");
        }

        return associatedMotor;
    }

    /// <summary>Calcula los valores del estado y persiste los cambios en el motor asociado.</summary>
    private async Task<MotorUpdateValues> UpdateAssociateMotorAsync(
        MotorAssociated associatedMotor,
        UpdateSpeedRequest request,
        DateTimeOffset updatedAt,
        CancellationToken cancellationToken)
    {
        var motor = associatedMotor.Motor!;
        var speedLimit = CalculateSpeedLimit(motor, request.DrivingMode);
        var appliedSpeed = associatedMotor.BatteryLevel <= 0
            ? CalculateBatteryDepletionSpeed(associatedMotor.CurrentSpeed)
            : CalculateAppliedSpeed(request.RequestedSpeed, speedLimit);

        var speedRatio = CalculateSpeedRatio(appliedSpeed, motor.MaximumSpeed);
        var rpm = CalculateRpm(speedRatio);
        var power = CalculatePower(motor, speedRatio);
        var temperature = CalculateTemperature(
            associatedMotor.CurrentTemperature,
            motor.MaximumTemperature,
            speedRatio);
        var batteryLevel = CalculateBatteryLevel(
            associatedMotor.BatteryLevel,
            motor.BatteryCapacityKwh,
            power);

        // Si el cálculo de consumo agota la batería en esta actualización, comienza
        // la desaceleración en el mismo ciclo. Las siguientes actualizaciones la llevan a cero.
        if (batteryLevel <= 0 && associatedMotor.BatteryLevel > 0 && appliedSpeed > 0)
        {
            appliedSpeed = CalculateBatteryDepletionSpeed(associatedMotor.CurrentSpeed);
            speedRatio = CalculateSpeedRatio(appliedSpeed, motor.MaximumSpeed);
            rpm = CalculateRpm(speedRatio);
            power = CalculatePower(motor, speedRatio);
            temperature = CalculateTemperature(
                associatedMotor.CurrentTemperature,
                motor.MaximumTemperature,
                speedRatio);
            batteryLevel = 0;
        }

        associatedMotor.CurrentSpeed = appliedSpeed;
        associatedMotor.CurrentTemperature = temperature;
        associatedMotor.CurrentRpm = rpm;
        associatedMotor.CurrentPower = power;
        associatedMotor.BatteryLevel = batteryLevel;
        associatedMotor.DrivingMode = request.DrivingMode;

        await associatedMotorRepository.UpdateAsync(associatedMotor, cancellationToken);

        return new MotorUpdateValues(
            speedLimit,
            appliedSpeed,
            temperature,
            rpm,
            power,
            batteryLevel);
    }

    private static decimal CalculateSpeedLimit(Motor motor, DrivingMode drivingMode)
    {
        var modeSpeedLimit = motor.MaximumSpeed * GetModeFactor(drivingMode);
        return Math.Min(motor.MaximumSpeed, modeSpeedLimit);
    }

    private static decimal CalculateAppliedSpeed(decimal requestedSpeed, decimal speedLimit) =>
        Math.Min(requestedSpeed, speedLimit);

    private static decimal CalculateBatteryDepletionSpeed(decimal currentSpeed) =>
        Math.Max(0m, currentSpeed - SpeedReductionPerUpdateWhenBatteryEmpty);

    private static decimal CalculateSpeedRatio(decimal appliedSpeed, decimal maximumSpeed) =>
        appliedSpeed / maximumSpeed;

    private static int CalculateRpm(decimal speedRatio) =>
        (int)Math.Round(
            ReferenceMaximumRpm * speedRatio,
            MidpointRounding.AwayFromZero);

    private static decimal CalculatePower(Motor motor, decimal speedRatio) =>
        Math.Round(
            motor.MaximumPower * speedRatio,
            2,
            MidpointRounding.AwayFromZero);

    private static decimal CalculateTemperature(
        decimal currentTemperature,
        decimal maximumTemperature,
        decimal speedRatio)
    {
        var estimatedTemperature = Math.Round(
            currentTemperature + speedRatio,
            2,
            MidpointRounding.AwayFromZero);

        return Math.Min(maximumTemperature, estimatedTemperature);
    }

    private static decimal CalculateBatteryLevel(
        decimal currentBatteryLevel,
        decimal batteryCapacityKwh,
        decimal power)
    {
        var estimatedEnergyKwh = power * ReferenceOperatingMinutesPerCommand / 60m;
        var batteryUsePercent = batteryCapacityKwh > 0
            ? estimatedEnergyKwh / batteryCapacityKwh * 100m
            : 0m;

        return Math.Max(
            0m,
            Math.Round(
                currentBatteryLevel - batteryUsePercent,
                2,
                MidpointRounding.AwayFromZero));
    }

    /// <summary>Crea una nueva muestra de telemetría a partir de los valores calculados.</summary>
    private static MotorStateHistory CreateMotorStateHistory(
        MotorAssociated associatedMotor,
        MotorUpdateValues update,
        DateTimeOffset timestamp) => new()
    {
        MotorStateId = Guid.NewGuid(),
        AssociatedMotorId = associatedMotor.AssociatedMotorId,
        Timestamp = timestamp.UtcDateTime,
        Speed = update.AppliedSpeed,
        Temperature = update.Temperature,
        Rpm = update.Rpm,
        Power = update.Power,
        BatteryLevel = update.BatteryLevel
    };

    /// <summary>Determina el tipo de comando comparando la velocidad anterior con la aplicada.</summary>
    private static CommandType GetCommandType(
        decimal previousSpeed,
        decimal appliedSpeed,
        decimal requestedSpeed)
    {
        if (requestedSpeed == 0)
            return CommandType.EmergencyStop;

        if (appliedSpeed > previousSpeed)
            return CommandType.IncreaseSpeed;

        if (appliedSpeed < previousSpeed)
            return CommandType.DecreaseSpeed;

        return CommandType.MaintainSpeed;
    }

    /// <summary>Crea el comando que se almacenará como resultado de la solicitud.</summary>
    private static Command CreateCommand(
        MotorAssociated associatedMotor,
        UpdateSpeedRequest request,
        decimal appliedSpeed,
        CommandType commandType,
        DateTimeOffset timestamp) => new()
    {
        AssociatedMotorId = associatedMotor.AssociatedMotorId,
        AssociatedMotor = associatedMotor,
        Type = commandType,
        Parameters = $"requestedSpeed={request.RequestedSpeed};appliedSpeed={appliedSpeed};mode={request.DrivingMode}",
        Result = associatedMotor.BatteryLevel <= 0
            ? $"Battery empty; speed reduced automatically to {appliedSpeed} km/h."
            : request.RequestedSpeed == appliedSpeed
                ? "Command processed."
                : $"Speed capped at {appliedSpeed} km/h for the selected driving mode.",
        SentAt = timestamp,
        ProcessedAt = timestamp
    };

    private static MotorStateResponse CreateMotorStateResponse(
        MotorAssociated associatedMotor,
        UpdateSpeedRequest request,
        MotorUpdateValues update,
        DateTimeOffset updatedAt) => new(
            associatedMotor.AssociatedMotorId,
            request.RequestedSpeed,
            update.AppliedSpeed,
            update.SpeedLimit,
            update.Temperature,
            update.Rpm,
            update.Power,
            update.BatteryLevel,
            request.DrivingMode,
            updatedAt);

    private static decimal GetModeFactor(DrivingMode mode) => mode switch
    {
        DrivingMode.Eco => EcoSpeedFactor,
        DrivingMode.Normal => NormalSpeedFactor,
        DrivingMode.Sport => SportSpeedFactor,
        _ => throw new ArgumentOutOfRangeException(nameof(mode))
    };

    private sealed record MotorUpdateValues(
        decimal SpeedLimit,
        decimal AppliedSpeed,
        decimal Temperature,
        int Rpm,
        decimal Power,
        decimal BatteryLevel);
}

public sealed class MotorNotInTripException(string message) : InvalidOperationException(message)
{
}
