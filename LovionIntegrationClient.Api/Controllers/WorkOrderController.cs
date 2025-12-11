using LovionIntegrationClient.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace LovionIntegrationClient.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkOrderController : ControllerBase
{
    private readonly IWorkOrderService workOrderService;

    public WorkOrderController(IWorkOrderService workOrderService)
    {
        this.workOrderService = workOrderService;
    }

    // TODO: add endpoints for work order import, validation, and status queries.
    // TODO: add logging here later.
}



