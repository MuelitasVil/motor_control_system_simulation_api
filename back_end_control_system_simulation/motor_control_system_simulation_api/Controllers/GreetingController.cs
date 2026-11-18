using Microsoft.AspNetCore.Mvc;

namespace motor_control_system_simulation_api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GreetingController : ControllerBase
{
    [HttpGet]
    public ActionResult<string> Get()
    {
        return Ok("Hello World");
    }
}