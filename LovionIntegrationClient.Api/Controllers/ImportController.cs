using LovionIntegrationClient.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LovionIntegrationClient.Core.Services; 

namespace LovionIntegrationClient.Api.Controllers;

[ApiController]
[Route("api/imports")]
public class ImportController : ControllerBase
{
    private readonly IntegrationDbContext _dbContext;

    public ImportController(IntegrationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // GET /api/imports
    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
    {
        var runs = await _dbContext.ImportRuns
            .AsNoTracking()
            .Select(r => new
            {
                r.Id,
                r.StartedAtUtc,
                r.CompletedAtUtc,
                r.Status,
                r.SourceSystem,
                ErrorCount = r.Errors.Count
            })
            .OrderByDescending(r => r.StartedAtUtc)
            .ToListAsync();

        return Ok(runs);
    }

    // GET /api/imports/{id}/errors
    [HttpGet("{id:guid}/errors")]
    public async Task<IActionResult> GetErrorsAsync(Guid id)
    {
        var exists = await _dbContext.ImportRuns
            .AsNoTracking()
            .AnyAsync(r => r.Id == id);

        if (!exists)
        {
            return NotFound();
        }

        var errors = await _dbContext.ImportErrors
            .AsNoTracking()
            .Where(e => e.ImportRunId == id)
            .Select(e => new
            {
                e.Id,
                e.ExternalWorkOrderId,
                e.ErrorType,
                e.ErrorMessage,
                e.Message,
                e.PayloadReference
            })
            .ToListAsync();

        return Ok(errors);
    }
    
    // POST /api/imports/run
    [HttpPost("run")]
    public async Task<IActionResult> RunAsync(
        [FromServices] IWorkOrderService workOrderService,
        CancellationToken cancellationToken)
    {
        await workOrderService.ImportFromSoapAsync(cancellationToken);
        return Ok(new { message = "Import run completed." });
    }

}