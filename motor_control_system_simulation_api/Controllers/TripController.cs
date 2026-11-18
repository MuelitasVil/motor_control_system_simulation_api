using Microsoft.AspNetCore.Mvc;
using MotorControlSystemSimulationApi.Models.Entities;
using MotorControlSystemSimulationApi.Repositories.Interfaces;

namespace MotorControlSystemSimulationApi.Controllers;

[ApiController]
[Route("api/trips")]
public sealed class TripController(IEntityRepository<Trip> repository) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TripResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var trips = await repository.GetAllAsync(cancellationToken);
        return Ok(trips.Select(ToResponse).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TripResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var trip = await repository.GetByIdAsync(id, cancellationToken);
        return trip is null ? NotFound() : Ok(ToResponse(trip));
    }

    private static TripResponse ToResponse(Trip trip) => new(
        trip.TripId,
        trip.AssociatedMotorId,
        trip.StartPoint,
        trip.EndPoint,
        trip.TotalDistance,
        trip.CurrentDistance,
        trip.CargoWeight,
        trip.StartDate,
        trip.EndDate);
}

public sealed record TripResponse(
    Guid TripId,
    Guid AssociatedMotorId,
    string StartPoint,
    string EndPoint,
    decimal TotalDistance,
    decimal CurrentDistance,
    decimal CargoWeight,
    DateTimeOffset? StartDate,
    DateTimeOffset? EndDate);
