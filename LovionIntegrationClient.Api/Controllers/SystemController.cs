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
    
    [HttpGet("Paul")]

    public IActionResult GetPaul()
    {
        // TO: test
        return Ok("Je gelooft het niet maar: Deze tekst komt in de Responce body... ;)  ");
        
    }
}



