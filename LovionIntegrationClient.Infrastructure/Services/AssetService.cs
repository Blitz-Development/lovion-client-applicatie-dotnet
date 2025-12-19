using LovionIntegrationClient.Core.Dtos;
using LovionIntegrationClient.Core.Services;
using LovionIntegrationClient.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LovionIntegrationClient.Infrastructure.Services;

public class AssetService : IAssetService
{
    private readonly IntegrationDbContext dbContext;

    public AssetService(IntegrationDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task ImportFromSoapAsync(CancellationToken cancellationToken = default)
    {
        // TODO: generate and call SOAP client
        // TODO: validate XML
        // TODO: map and persist assets using dbContext
        // TODO: add logging here later.
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<AssetDto>> GetAllAssetsAsync()
    {
        var entities = await dbContext.Assets.AsNoTracking().ToListAsync();
        return entities
            .Select(a => new AssetDto
            {
                Id = a.Id,
                ExternalAssetRef = a.ExternalId,
                // Type / Description / Location niet aanwezig in het domeinmodel,
                // blijven dus null tenzij later uitgebreid.
                Type = null,
                Description = a.Name, // eventueel gebruiken als interim description
                Location = null
            })
            .ToList();
    }

    public async Task<AssetDto?> GetAssetByIdAsync(Guid id)
    {
        var entity = await dbContext.Assets
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (entity is null)
        {
            return null;
        }

        return new AssetDto
        {
            Id = entity.Id,
            ExternalAssetRef = entity.ExternalId,
            Type = null,
            Description = entity.Name,
            Location = null
        };
    }
}
