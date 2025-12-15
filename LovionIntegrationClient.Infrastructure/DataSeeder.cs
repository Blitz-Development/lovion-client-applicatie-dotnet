using LovionIntegrationClient.Infrastructure.Persistence;
using LovionIntegrationClient.Core.Domain;

namespace LovionIntegrationClient.Infrastructure;

public static class DataSeeder
{
    public static void Seed(IntegrationDbContext dbContext)
    {
        // Asset seeding
        if (!dbContext.Assets.Any())
        {
            var asset1 = new Asset { Id = Guid.NewGuid(), ExternalId = "A-1001", Name = "Asset One" };
            var asset2 = new Asset { Id = Guid.NewGuid(), ExternalId = "A-1002", Name = "Asset Two" };
            var asset3 = new Asset { Id = Guid.NewGuid(), ExternalId = "A-1003", Name = "Asset Three" };
            dbContext.Assets.AddRange(asset1, asset2, asset3);
            dbContext.SaveChanges();
        }
        // WorkOrder seeding
        if (!dbContext.WorkOrders.Any())
        {
            var firstAsset = dbContext.Assets.First();
            var secondAsset = dbContext.Assets.Skip(1).FirstOrDefault() ?? firstAsset;
            dbContext.WorkOrders.AddRange(new[]
            {
                new WorkOrder { Id = Guid.NewGuid(), AssetId = firstAsset.Id, ExternalId = "WO-001", Description = "Controle asset one", ScheduledDate = DateTime.Now.AddDays(3) },
                new WorkOrder { Id = Guid.NewGuid(), AssetId = secondAsset.Id, ExternalId = "WO-002", Description = "Planning asset two", ScheduledDate = DateTime.Now.AddDays(7) },
                new WorkOrder { Id = Guid.NewGuid(), AssetId = firstAsset.Id, ExternalId = "WO-003", Description = "Herstel asset one", ScheduledDate = DateTime.Now.AddDays(10) },
            });
            dbContext.SaveChanges();
        }
    }
}
