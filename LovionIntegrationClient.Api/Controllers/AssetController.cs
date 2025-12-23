using LovionIntegrationClient.Core.Services;
using Microsoft.AspNetCore.Mvc;
using LovionIntegrationClient.Core.Dtos;

namespace LovionIntegrationClient.Api.Controllers;

[ApiController]
[Route("api/assets")]
public class AssetController : ControllerBase
{
    private readonly IAssetService assetService;

    public AssetController(IAssetService assetService)
    {
        this.assetService = assetService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AssetDto>>> GetAllAsync()
    {
        var assets = await assetService.GetAllAssetsAsync();
        return Ok(assets);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AssetDto>> GetByIdAsync(Guid id)
    {
        var asset = await assetService.GetAssetByIdAsync(id);

        if (asset is null)
        {
            return NotFound();
        }

        return Ok(asset);
    }
}