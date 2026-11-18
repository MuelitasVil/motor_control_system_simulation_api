using Microsoft.AspNetCore.Mvc;
using MotorControlSystemSimulationApi.Models.Entities;
using MotorControlSystemSimulationApi.Repositories.Interfaces;
using MotorControlSystemSimulationApi.Services;

namespace MotorControlSystemSimulationApi.Controllers;

[ApiController]
[Route("api/motors")]
public sealed class MotorController(
    IEntityRepository<Motor> repository,
    MotorCreationService creationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MotorResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var motors = await repository.GetAllAsync(cancellationToken);
        return Ok(motors.Select(ToResponse).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MotorResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var motor = await repository.GetByIdAsync(id, cancellationToken);
        return motor is null ? NotFound() : Ok(ToResponse(motor));
    }

    [HttpPost]
    public async Task<ActionResult<MotorResponse>> Create(
        [FromBody] CreateMotorRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var motor = await creationService.CreateAsync(request, cancellationToken);
            var response = ToResponse(motor);
            return CreatedAtAction(nameof(GetById), new { id = motor.MotorId }, response);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid motor data", Detail = exception.Message });
        }
    }

    private static MotorResponse ToResponse(Motor motor) => new(
        motor.MotorId,
        motor.Model,
        motor.MaximumSpeed,
        motor.MaximumTemperature,
        motor.BatteryCapacityKwh,
        motor.MaximumPower,
        motor.AssociatedMotors.Select(x => x.AssociatedMotorId).ToArray());
}

public sealed record MotorResponse(
    Guid MotorId,
    string Model,
    decimal MaximumSpeed,
    decimal MaximumTemperature,
    decimal BatteryCapacityKwh,
    decimal MaximumPower,
    IReadOnlyList<Guid> AssociatedMotorIds);
