using Microsoft.AspNetCore.Mvc;
using MotorControlSystemSimulationApi.Services;

namespace MotorControlSystemSimulationApi.Controllers;

[ApiController]
[Route("api/associated-motors")]
public sealed class MotorSimulationController(MotorSimulationService simulationService) : ControllerBase
{
    [HttpPost("{associatedMotorId:guid}/speed")]
    [ProducesResponseType<MotorStateResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MotorStateResponse>> UpdateSpeed(
        Guid associatedMotorId,
        [FromBody] UpdateSpeedRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await simulationService.UpdateSpeedAsync(associatedMotorId, request, cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }
        catch (MotorNotInTripException exception)
        {
            return Conflict(new ProblemDetails { Title = "Motor is not in a trip", Detail = exception.Message });
        }
        catch (ArgumentOutOfRangeException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid speed command", Detail = exception.Message });
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Motor configuration error", Detail = exception.Message });
        }
    }
}
