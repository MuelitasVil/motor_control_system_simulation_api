using MotorControlSystemSimulationApi.Models.Entities;
using MotorControlSystemSimulationApi.Repositories.Interfaces;

namespace MotorControlSystemSimulationApi.Repositories.InMemory;

public sealed class CrewMemberRepository : InMemoryRepositoryBase<CrewMember>, IEntityRepository<CrewMember>
{
    public CrewMemberRepository(InMemoryDataStore store)
        : base(store.CrewMembers, x => x.CrewMemberId, x => x.CrewMemberId, _ => DateTimeOffset.MinValue)
    {
    }
}
