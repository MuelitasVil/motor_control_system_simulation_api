using MotorControlSystemSimulationApi.Models.Entities;
using MotorControlSystemSimulationApi.Repositories.Interfaces;

namespace MotorControlSystemSimulationApi.Services;

public sealed class VehicleCreationService(IEntityRepository<Vehicle> vehicleRepository)
{
    public async Task<Vehicle> CreateAsync(
        CreateVehicleRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.Identifier))
            throw new ArgumentException("El identificador del vehículo es obligatorio.", nameof(request));
        if (string.IsNullOrWhiteSpace(request.Color))
            throw new ArgumentException("El color del vehículo es obligatorio.", nameof(request));
        if (request.MaximumWeight <= 0)
            throw new ArgumentOutOfRangeException(nameof(request), "El peso máximo debe ser mayor que cero.");
        if (request.MaximumOccupants <= 0)
            throw new ArgumentOutOfRangeException(nameof(request), "La capacidad de ocupantes debe ser mayor que cero.");

        var vehicle = new Vehicle
        {
            VehicleId = Guid.NewGuid(),
            Identifier = request.Identifier.Trim(),
            Color = request.Color.Trim(),
            MaximumWeight = request.MaximumWeight,
            MaximumOccupants = request.MaximumOccupants
        };

        await vehicleRepository.AddAsync(vehicle, cancellationToken);
        return vehicle;
    }
}
