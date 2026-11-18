using MotorControlSystemSimulationApi.Models.Entities;
using MotorControlSystemSimulationApi.Repositories.Interfaces;

namespace MotorControlSystemSimulationApi.Repositories.InMemory;

public sealed class MotorStateHistoryRepository : InMemoryRepositoryBase<MotorStateHistory>, IEntityRepository<MotorStateHistory>
{
    public MotorStateHistoryRepository(InMemoryDataStore store)
        : base(store.MotorStateHistory, x => x.MotorStateId, x => x.AssociatedMotorId, x => new DateTimeOffset(DateTime.SpecifyKind(x.Timestamp, DateTimeKind.Utc)))
    {
    }
}
