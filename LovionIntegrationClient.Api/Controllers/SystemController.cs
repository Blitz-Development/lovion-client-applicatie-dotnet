using Microsoft.AspNetCore.Mvc;

namespace LovionIntegrationClient.Api.Controllers;

[ApiController]
[Route("api/system")]
public class SystemController : ControllerBase
{
    [HttpGet("health")]
    public IActionResult GetHealth()
    {
        // TODO: add logging here later.
        return Ok("OK");
    }
}



