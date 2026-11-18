using Microsoft.AspNetCore.Mvc;
using MotorControlSystemSimulationApi.Models.Entities;
using MotorControlSystemSimulationApi.Repositories.Interfaces;
using MotorControlSystemSimulationApi.Services;

namespace MotorControlSystemSimulationApi.Controllers;

[ApiController]
[Route("api/trips")]
public sealed class TripController(
    IEntityRepository<Trip> repository,
    TripManagementService tripManagementService) : ControllerBase
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

    [HttpPost]
    public async Task<ActionResult<TripResponse>> Create(
        [FromBody] CreateTripRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var trip = await tripManagementService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = trip.TripId }, ToResponse(trip));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new ProblemDetails { Title = "Related entity not found", Detail = exception.Message });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid trip data", Detail = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails { Title = "Trip conflict", Detail = exception.Message });
        }
    }

    [HttpPost("{id:guid}/finish")]
    public async Task<ActionResult<TripResponse>> Finish(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var trip = await tripManagementService.FinishAsync(id, cancellationToken);
            return trip is null ? NotFound() : Ok(ToResponse(trip));
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails { Title = "Trip conflict", Detail = exception.Message });
        }
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
        trip.EndDate,
        trip.CrewMembers.Select(x => x.CrewMemberId).ToArray());
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
    DateTimeOffset? EndDate,
    IReadOnlyList<Guid> CrewMemberIds);
