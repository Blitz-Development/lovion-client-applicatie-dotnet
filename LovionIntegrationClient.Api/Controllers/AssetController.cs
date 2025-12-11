using LovionIntegrationClient.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace LovionIntegrationClient.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssetController : ControllerBase
{
    private readonly IAssetService assetService;

    public AssetController(IAssetService assetService)
    {
        this.assetService = assetService;
    }

    // TODO: add endpoints for asset retrieval and import workflow.
    // TODO: add logging here later.
}


