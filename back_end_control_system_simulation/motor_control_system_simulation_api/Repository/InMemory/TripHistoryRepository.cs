using MotorControlSystemSimulationApi.Models.Entities;
using MotorControlSystemSimulationApi.Repositories.Interfaces;

namespace MotorControlSystemSimulationApi.Repositories.InMemory;

public sealed class TripHistoryRepository : InMemoryRepositoryBase<TripHistory>, IEntityRepository<TripHistory>
{
    public TripHistoryRepository(InMemoryDataStore store)
        : base(store.TripHistory, x => x.TripHistoryId, x => x.TripId, x => x.Timestamp)
    {
    }
}
