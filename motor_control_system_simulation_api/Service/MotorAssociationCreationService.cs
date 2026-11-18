using MotorControlSystemSimulationApi.Models.Entities;
using MotorControlSystemSimulationApi.Models.Enums;
using MotorControlSystemSimulationApi.Repositories.Interfaces;

namespace MotorControlSystemSimulationApi.Services;

public sealed class MotorAssociationCreationService(
    IEntityRepository<Vehicle> vehicleRepository,
    IEntityRepository<Motor> motorRepository,
    IEntityRepository<MotorAssociated> associatedMotorRepository)
{
    public async Task<MotorAssociated> CreateAsync(
        CreateMotorAssociationRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var vehicle = await vehicleRepository.GetByIdAsync(request.VehicleId, cancellationToken)
            ?? throw new KeyNotFoundException("No existe el vehículo indicado.");
        var motor = await motorRepository.GetByIdAsync(request.MotorId, cancellationToken)
            ?? throw new KeyNotFoundException("No existe el motor indicado.");

        var existingAssociations = await associatedMotorRepository.GetAllAsync(cancellationToken);
        if (existingAssociations.Any(x => x.MotorId == motor.MotorId))
            throw new InvalidOperationException("El motor ya está asociado a un vehículo.");

        var association = new MotorAssociated
        {
            AssociatedMotorId = Guid.NewGuid(),
            VehicleId = vehicle.VehicleId,
            Vehicle = vehicle,
            MotorId = motor.MotorId,
            Motor = motor,
            InstallationDate = DateTimeOffset.UtcNow,
            OperatingStatus = "Active",
            OperationState = MotorOperationState.Stopped,
            CurrentSpeed = 0,
            BatteryLevel = 100,
            CurrentTemperature = 25,
            CurrentRpm = 0,
            CurrentPower = 0,
            DrivingMode = DrivingMode.Normal
        };

        await associatedMotorRepository.AddAsync(association, cancellationToken);
        vehicle.AssociatedMotors.Add(association);
        motor.AssociatedMotors.Add(association);
        return association;
    }
}
