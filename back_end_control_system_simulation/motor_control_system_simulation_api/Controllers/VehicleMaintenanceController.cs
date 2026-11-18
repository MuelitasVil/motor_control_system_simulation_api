using Microsoft.AspNetCore.Mvc;
using MotorControlSystemSimulationApi.Services;

namespace MotorControlSystemSimulationApi.Controllers;

[ApiController]
[Route("api/vehicles/{vehicleId:guid}/maintenance")]
public sealed class VehicleMaintenanceController(VehicleMaintenanceService maintenanceService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<VehicleMaintenanceResponse>> Recharge(
        Guid vehicleId,
        [FromBody] VehicleMaintenanceRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await maintenanceService.RechargeAsync(vehicleId, request, cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }
        catch (VehicleInTripException exception)
        {
            return Conflict(new ProblemDetails { Title = "Vehicle is in a trip", Detail = exception.Message });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid maintenance data", Detail = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails { Title = "Maintenance cannot be performed", Detail = exception.Message });
        }
    }
}
