using LovionIntegrationClient.Core.Dtos;
using LovionIntegrationClient.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LovionIntegrationClient.Api.Controllers;

[ApiController]
[Route("api/workorders")]
public class WorkOrderController : ControllerBase
{
    private readonly IWorkOrderService workOrderService;
    private readonly ILogger<WorkOrderController> _logger;

    public WorkOrderController(IWorkOrderService workOrderService)
    {
        this.workOrderService = workOrderService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkOrderDto>>> GetAllAsync([FromQuery] string? status)
    {
        _logger.LogInformation("GET /api/workorders called with status={Status}", status);
        
        var workOrders = await workOrderService.GetAllWorkOrdersAsync(status);
        return Ok(workOrders);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WorkOrderDto>> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("GET /api/workorders/{WorkOrderid} called", id);
        
        var workOrder = await workOrderService.GetWorkOrderByIdAsync(id);
        if (workOrder is null)
        {
            return NotFound();
        }

        return Ok(workOrder);
    }

    [HttpGet("soap-test")]
    public async Task<ActionResult<IEnumerable<SoapWorkOrderDto>>> GetFromSoapAsync()
    {
        _logger.LogInformation("GET /api/workorders/soap-test called"); // logging achteraf uitbreiden

        
        var soapOrders = await workOrderService.FetchWorkOrdersFromSoapAsync();
        return Ok(soapOrders);
    }
    
    [HttpPost("{id}/process")]
    public async Task<IActionResult> ProcessWorkOrder(Guid id)
    {
        _logger.LogInformation("POST /api/workorders/{WorkOrderid}/process called", id);
        
        // 1. Bestaat de workorder?
        var workOrder = await workOrderService.GetWorkOrderByIdAsync(id);

        if (workOrder is null)
        {
            return NotFound();
        }

        // 2. Status aanpassen
        await workOrderService.MarkAsProcessedAsync(id);

        // 3. HTTP 204 No Content (geslaagd, maar geen body)
        return NoContent();
    }

}



