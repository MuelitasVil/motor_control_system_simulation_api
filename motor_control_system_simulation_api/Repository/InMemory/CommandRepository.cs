using MotorControlSystemSimulationApi.Models.Entities;
using MotorControlSystemSimulationApi.Repositories.Interfaces;

namespace MotorControlSystemSimulationApi.Repositories.InMemory;

public sealed class CommandRepository : InMemoryRepositoryBase<Command>, IEntityRepository<Command>
{
    public CommandRepository(InMemoryDataStore store)
        : base(store.Commands, x => x.CommandId, x => x.AssociatedMotorId, x => x.ProcessedAt ?? x.SentAt)
    {
    }
}
