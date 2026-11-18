using MotorControlSystemSimulationApi.Models.Entities;

namespace MotorControlSystemSimulationApi.Repositories.InMemory;

/// <summary>TripCrewMember usa clave compuesta (TripId, CrewMemberId), por eso no implementa IEntityRepository&lt;T&gt; basado en Guid.</summary>
public sealed class TripCrewMemberRepository(InMemoryDataStore store)
{
    public Task<IReadOnlyList<TripCrewMember>> GetByTripIdAsync(Guid tripId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<TripCrewMember> result = store.TripCrewMembers.Where(x => x.TripId == tripId).ToList();
        return Task.FromResult(result);
    }

    public Task AddAsync(TripCrewMember membership, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(membership);
        cancellationToken.ThrowIfCancellationRequested();
        if (store.TripCrewMembers.Any(x => x.TripId == membership.TripId && x.CrewMemberId == membership.CrewMemberId))
            throw new InvalidOperationException("Este tripulante ya está asociado al viaje.");
        store.TripCrewMembers.Add(membership);
        return Task.CompletedTask;
    }
}
