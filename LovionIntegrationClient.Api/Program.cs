using LovionIntegrationClient.Core.Services;
using LovionIntegrationClient.Infrastructure.Persistence;
using LovionIntegrationClient.Infrastructure.Repositories;
using LovionIntegrationClient.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<IntegrationDbContext>(options =>
{
    // TODO: adjust provider/settings for other environments.
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<IWorkOrderRepository, WorkOrderRepository>();
builder.Services.AddScoped<IAssetRepository, AssetRepository>();

// TODO: add logging configuration when log sinks are defined.
builder.Services.AddScoped<IWorkOrderService, WorkOrderService>();
builder.Services.AddScoped<IAssetService, AssetService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
