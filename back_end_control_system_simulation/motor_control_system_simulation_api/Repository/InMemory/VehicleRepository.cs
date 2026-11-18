using MotorControlSystemSimulationApi.Models.Entities;
using MotorControlSystemSimulationApi.Repositories.Interfaces;

namespace MotorControlSystemSimulationApi.Repositories.InMemory;

public sealed class VehicleRepository : InMemoryRepositoryBase<Vehicle>, IEntityRepository<Vehicle>
{
    public VehicleRepository(InMemoryDataStore store)
        : base(store.Vehicles, x => x.VehicleId, x => x.VehicleId, _ => DateTimeOffset.MinValue)
    {
    }
}
