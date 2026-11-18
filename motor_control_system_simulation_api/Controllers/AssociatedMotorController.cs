using Microsoft.AspNetCore.Mvc;
using MotorControlSystemSimulationApi.Models.Entities;
using MotorControlSystemSimulationApi.Models.Enums;
using MotorControlSystemSimulationApi.Repositories.Interfaces;

namespace MotorControlSystemSimulationApi.Controllers;

[ApiController]
[Route("api/associated-motors")]
public sealed class AssociatedMotorController(IEntityRepository<MotorAssociated> repository) : ControllerBase
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
    IReadOnlyList<Guid> TripIds);
