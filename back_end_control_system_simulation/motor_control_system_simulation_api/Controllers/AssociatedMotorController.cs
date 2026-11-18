using Microsoft.AspNetCore.Mvc;
using MotorControlSystemSimulationApi.Models.Entities;
using MotorControlSystemSimulationApi.Models.Enums;
using MotorControlSystemSimulationApi.Repositories.Interfaces;
using MotorControlSystemSimulationApi.Services;

namespace MotorControlSystemSimulationApi.Controllers;

[ApiController]
[Route("api/associated-motors")]
public sealed class AssociatedMotorController(
    IEntityRepository<MotorAssociated> repository,
    MotorAssociationCreationService creationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AssociatedMotorResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var associatedMotors = await repository.GetAllAsync(cancellationToken);
        return Ok(associatedMotors.Select(ToResponse).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AssociatedMotorResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var associatedMotor = await repository.GetByIdAsync(id, cancellationToken);
        return associatedMotor is null ? NotFound() : Ok(ToResponse(associatedMotor));
    }

    [HttpPost]
    public async Task<ActionResult<AssociatedMotorResponse>> Create(
        [FromBody] CreateMotorAssociationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var association = await creationService.CreateAsync(request, cancellationToken);
            return CreatedAtAction(
                nameof(GetById),
                new { id = association.AssociatedMotorId },
                ToResponse(association));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(new ProblemDetails { Title = "Related entity not found", Detail = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails { Title = "Motor association conflict", Detail = exception.Message });
        }
    }

    private static AssociatedMotorResponse ToResponse(MotorAssociated item) => new(
        item.AssociatedMotorId,
        item.VehicleId,
        item.MotorId,
        item.InstallationDate,
        item.OperatingStatus,
        item.CurrentSpeed,
        item.BatteryLevel,
        item.CurrentTemperature,
        item.CurrentRpm,
        item.CurrentPower,
        item.DrivingMode,
        item.OperationState,
        item.Trips.Select(x => x.TripId).ToArray());
}

public sealed record AssociatedMotorResponse(
    Guid AssociatedMotorId,
    Guid VehicleId,
    Guid MotorId,
    DateTimeOffset InstallationDate,
    string OperatingStatus,
    decimal CurrentSpeed,
    decimal BatteryLevel,
    decimal CurrentTemperature,
    int CurrentRpm,
    decimal CurrentPower,
    DrivingMode DrivingMode,
    MotorOperationState OperationState,
    IReadOnlyList<Guid> TripIds);
