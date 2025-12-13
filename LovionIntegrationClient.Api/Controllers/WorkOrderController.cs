using LovionIntegrationClient.Core.Dtos;
using LovionIntegrationClient.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace LovionIntegrationClient.Api.Controllers;

[ApiController]
[Route("api/workorders")]
public class WorkOrderController : ControllerBase
{
    private readonly IWorkOrderService workOrderService;

    public WorkOrderController(IWorkOrderService workOrderService)
    {
        this.workOrderService = workOrderService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkOrderDto>>> GetAllAsync()
    {
        var workOrders = await workOrderService.GetAllWorkOrdersAsync();
        return Ok(workOrders);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WorkOrderDto>> GetByIdAsync(Guid id)
    {
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
        var soapOrders = await workOrderService.FetchWorkOrdersFromSoapAsync();
        return Ok(soapOrders);
    }
}



