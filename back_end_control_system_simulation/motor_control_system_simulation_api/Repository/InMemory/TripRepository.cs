using MotorControlSystemSimulationApi.Models.Entities;
using MotorControlSystemSimulationApi.Repositories.Interfaces;

namespace MotorControlSystemSimulationApi.Repositories.InMemory;

public sealed class TripRepository : InMemoryRepositoryBase<Trip>, IEntityRepository<Trip>
{
    public TripRepository(InMemoryDataStore store)
        : base(store.Trips, x => x.TripId, x => x.AssociatedMotorId, x => x.StartDate ?? DateTimeOffset.MinValue)
    {
    }
}
