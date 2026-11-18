using MotorControlSystemSimulationApi.Models.Entities;
using MotorControlSystemSimulationApi.Models.Enums;
using MotorControlSystemSimulationApi.Repositories.Interfaces;

namespace MotorControlSystemSimulationApi.Services;

public sealed class VehicleMaintenanceService(
    IEntityRepository<Vehicle> vehicleRepository,
    IEntityRepository<MotorAssociated> associatedMotorRepository,
    IEntityRepository<MotorStateHistory> stateHistoryRepository)
{
    public async Task<VehicleMaintenanceResponse?> RechargeAsync(
        Guid vehicleId,
        VehicleMaintenanceRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.TargetBatteryLevel is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(request), "El nivel de carga debe estar entre 0 y 100.");

        var vehicle = await vehicleRepository.GetByIdAsync(vehicleId, cancellationToken);
        if (vehicle is null)
            return null;

        var associations = await associatedMotorRepository.GetAllAsync(cancellationToken);
        var vehicleMotors = associations.Where(x => x.VehicleId == vehicleId).ToList();
        if (vehicleMotors.Count == 0)
            throw new InvalidOperationException("El vehículo no tiene motores asociados para recargar.");
        if (vehicleMotors.Any(x => x.OperationState == MotorOperationState.InTrip))
            throw new VehicleInTripException("No se puede recargar el vehículo mientras está en un viaje.");
        if (vehicleMotors.Any(x => request.TargetBatteryLevel < x.BatteryLevel))
            throw new ArgumentOutOfRangeException(
                nameof(request),
                "El nivel solicitado no puede ser menor que el nivel actual de la batería.");

        var now = DateTimeOffset.UtcNow;
        foreach (var association in vehicleMotors)
        {
            association.BatteryLevel = request.TargetBatteryLevel;
            await associatedMotorRepository.UpdateAsync(association, cancellationToken);

            var state = new MotorStateHistory
            {
                MotorStateId = Guid.NewGuid(),
                AssociatedMotorId = association.AssociatedMotorId,
                Timestamp = now.UtcDateTime,
                Speed = association.CurrentSpeed,
                Temperature = association.CurrentTemperature,
                Rpm = association.CurrentRpm,
                Power = association.CurrentPower,
                BatteryLevel = association.BatteryLevel
            };
            await stateHistoryRepository.AddAsync(state, cancellationToken);
            association.StateHistory.Add(state);
        }

        return new VehicleMaintenanceResponse(
            vehicle.VehicleId,
            vehicleMotors.Select(x => x.AssociatedMotorId).ToArray(),
            request.TargetBatteryLevel,
            now);
    }
}

public sealed record VehicleMaintenanceResponse(
    Guid VehicleId,
    IReadOnlyList<Guid> AssociatedMotorIds,
    decimal BatteryLevel,
    DateTimeOffset MaintainedAt);

public sealed class VehicleInTripException(string message) : InvalidOperationException(message)
{
}
