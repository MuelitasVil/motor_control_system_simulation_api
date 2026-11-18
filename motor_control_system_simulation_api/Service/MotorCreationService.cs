using MotorControlSystemSimulationApi.Models.Entities;
using MotorControlSystemSimulationApi.Repositories.Interfaces;

namespace MotorControlSystemSimulationApi.Services;

public sealed class MotorCreationService(IEntityRepository<Motor> motorRepository)
{
    public async Task<Motor> CreateAsync(
        CreateMotorRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.Model))
            throw new ArgumentException("El modelo del motor es obligatorio.", nameof(request));
        if (request.MaximumSpeed <= 0 || request.MaximumTemperature <= 0 ||
            request.BatteryCapacityKwh <= 0 || request.MaximumPower <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(request),
                "La velocidad, temperatura, capacidad de batería y potencia máximas deben ser mayores que cero.");
        }

        var motor = new Motor
        {
            MotorId = Guid.NewGuid(),
            Model = request.Model.Trim(),
            MaximumSpeed = request.MaximumSpeed,
            MaximumTemperature = request.MaximumTemperature,
            BatteryCapacityKwh = request.BatteryCapacityKwh,
            MaximumPower = request.MaximumPower
        };

        await motorRepository.AddAsync(motor, cancellationToken);
        return motor;
    }
}
