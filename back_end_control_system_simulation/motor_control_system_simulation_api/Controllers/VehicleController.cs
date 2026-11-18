using Microsoft.AspNetCore.Mvc;
using MotorControlSystemSimulationApi.Models.Entities;
using MotorControlSystemSimulationApi.Repositories.Interfaces;
using MotorControlSystemSimulationApi.Services;

namespace MotorControlSystemSimulationApi.Controllers;

[ApiController]
[Route("api/vehicles")]
public sealed class VehicleController(
    IEntityRepository<Vehicle> repository,
    VehicleCreationService creationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<VehicleResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var vehicles = await repository.GetAllAsync(cancellationToken);
        return Ok(vehicles.Select(ToResponse).ToList());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VehicleResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var vehicle = await repository.GetByIdAsync(id, cancellationToken);
        return vehicle is null ? NotFound() : Ok(ToResponse(vehicle));
    }

    [HttpPost]
    public async Task<ActionResult<VehicleResponse>> Create(
        [FromBody] CreateVehicleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var vehicle = await creationService.CreateAsync(request, cancellationToken);
            var response = ToResponse(vehicle);
            return CreatedAtAction(nameof(GetById), new { id = vehicle.VehicleId }, response);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid vehicle data", Detail = exception.Message });
        }
    }

    private static VehicleResponse ToResponse(Vehicle vehicle) => new(
        vehicle.VehicleId,
        vehicle.Identifier,
        vehicle.Color,
        vehicle.MaximumWeight,
        vehicle.MaximumOccupants,
        vehicle.AssociatedMotors.Select(x => x.AssociatedMotorId).ToArray());
}

public sealed record VehicleResponse(
    Guid VehicleId,
    string Identifier,
    string Color,
    decimal MaximumWeight,
    int MaximumOccupants,
    IReadOnlyList<Guid> AssociatedMotorIds);
