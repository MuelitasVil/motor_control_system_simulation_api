using MotorControlSystemSimulationApi.Models.Entities;
using MotorControlSystemSimulationApi.Repositories.Interfaces;

namespace MotorControlSystemSimulationApi.Repositories.InMemory;

public sealed class MotorRepository : InMemoryRepositoryBase<Motor>, IEntityRepository<Motor>
{
    public MotorRepository(InMemoryDataStore store)
        : base(store.Motors, x => x.MotorId, x => x.MotorId, _ => DateTimeOffset.MinValue)
    {
    }
}
