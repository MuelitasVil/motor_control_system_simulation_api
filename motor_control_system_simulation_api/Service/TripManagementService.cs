using MotorControlSystemSimulationApi.Models.Entities;
using MotorControlSystemSimulationApi.Models.Enums;
using MotorControlSystemSimulationApi.Repositories.InMemory;
using MotorControlSystemSimulationApi.Repositories.Interfaces;

namespace MotorControlSystemSimulationApi.Services;

public sealed class TripManagementService(
    IEntityRepository<Trip> tripRepository,
    IEntityRepository<MotorAssociated> associatedMotorRepository,
    IEntityRepository<CrewMember> crewMemberRepository,
    IEntityRepository<TripHistory> tripHistoryRepository,
    TripCrewMemberRepository tripCrewMemberRepository)
{
    public async Task<Trip> CreateAsync(
        CreateTripRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateTripRequest(request);

        var associatedMotor = await associatedMotorRepository.GetByIdAsync(
            request.AssociatedMotorId,
            cancellationToken)
            ?? throw new KeyNotFoundException("No existe el motor asociado indicado.");

        if (associatedMotor.OperationState == MotorOperationState.InTrip)
            throw new InvalidOperationException("El motor asociado ya se encuentra en un viaje.");

        var crewMembers = await GetCrewMembersAsync(request.CrewMemberIds, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var trip = new Trip
        {
            TripId = Guid.NewGuid(),
            AssociatedMotorId = associatedMotor.AssociatedMotorId,
            AssociatedMotor = associatedMotor,
            StartPoint = request.StartPoint.Trim(),
            EndPoint = request.EndPoint.Trim(),
            TotalDistance = request.TotalDistance,
            CurrentDistance = 0,
            CargoWeight = request.CargoWeight,
            StartDate = now
        };

        await tripRepository.AddAsync(trip, cancellationToken);
        associatedMotor.Trips.Add(trip);
        associatedMotor.OperationState = MotorOperationState.InTrip;
        await associatedMotorRepository.UpdateAsync(associatedMotor, cancellationToken);

        foreach (var crewMember in crewMembers)
        {
            var membership = new TripCrewMember
            {
                TripId = trip.TripId,
                Trip = trip,
                CrewMemberId = crewMember.CrewMemberId,
                CrewMember = crewMember
            };

            await tripCrewMemberRepository.AddAsync(membership, cancellationToken);
            trip.CrewMembers.Add(membership);
            crewMember.Trips.Add(membership);
        }

        var history = new TripHistory
        {
            TripHistoryId = Guid.NewGuid(),
            TripId = trip.TripId,
            Trip = trip,
            Status = "Started",
            Timestamp = now,
            Details = crewMembers.Count == 0
                ? "Viaje creado sin tripulantes."
                : $"Viaje creado con {crewMembers.Count} tripulante(s)."
        };
        await tripHistoryRepository.AddAsync(history, cancellationToken);
        trip.History.Add(history);

        return trip;
    }

    public async Task<Trip?> FinishAsync(Guid tripId, CancellationToken cancellationToken = default)
    {
        var trip = await tripRepository.GetByIdAsync(tripId, cancellationToken);
        if (trip is null)
            return null;
        if (trip.EndDate is not null)
            throw new InvalidOperationException("El viaje ya se encuentra finalizado.");

        var associatedMotor = await associatedMotorRepository.GetByIdAsync(
            trip.AssociatedMotorId,
            cancellationToken)
            ?? throw new InvalidOperationException("No se encontró el motor asociado al viaje.");

        var now = DateTimeOffset.UtcNow;
        trip.EndDate = now;
        await tripRepository.UpdateAsync(trip, cancellationToken);

        associatedMotor.OperationState = MotorOperationState.Stopped;
        associatedMotor.CurrentSpeed = 0;
        associatedMotor.CurrentRpm = 0;
        associatedMotor.CurrentPower = 0;
        await associatedMotorRepository.UpdateAsync(associatedMotor, cancellationToken);

        var history = new TripHistory
        {
            TripHistoryId = Guid.NewGuid(),
            TripId = trip.TripId,
            Trip = trip,
            Status = "Finished",
            Timestamp = now,
            Details = "Viaje finalizado."
        };
        await tripHistoryRepository.AddAsync(history, cancellationToken);
        trip.History.Add(history);

        return trip;
    }

    private async Task<List<CrewMember>> GetCrewMembersAsync(
        IReadOnlyList<Guid>? crewMemberIds,
        CancellationToken cancellationToken)
    {
        if (crewMemberIds is null || crewMemberIds.Count == 0)
            return [];
        if (crewMemberIds.Count != crewMemberIds.Distinct().Count())
            throw new ArgumentException("La lista contiene tripulantes repetidos.", nameof(crewMemberIds));

        var crewMembers = new List<CrewMember>(crewMemberIds.Count);
        foreach (var crewMemberId in crewMemberIds)
        {
            var crewMember = await crewMemberRepository.GetByIdAsync(crewMemberId, cancellationToken);
            if (crewMember is null)
                throw new KeyNotFoundException($"No existe el tripulante {crewMemberId}.");
            crewMembers.Add(crewMember);
        }

        return crewMembers;
    }

    private static void ValidateTripRequest(CreateTripRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.StartPoint))
            throw new ArgumentException("El punto de inicio es obligatorio.", nameof(request));
        if (string.IsNullOrWhiteSpace(request.EndPoint))
            throw new ArgumentException("El punto de destino es obligatorio.", nameof(request));
        if (request.TotalDistance <= 0)
            throw new ArgumentOutOfRangeException(nameof(request), "La distancia total debe ser mayor que cero.");
        if (request.CargoWeight < 0)
            throw new ArgumentOutOfRangeException(nameof(request), "El peso de carga no puede ser negativo.");
    }
}
