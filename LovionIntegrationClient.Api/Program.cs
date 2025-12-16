using LovionIntegrationClient.Core.Services;
using LovionIntegrationClient.Infrastructure.Persistence;
using LovionIntegrationClient.Infrastructure.Repositories;
using LovionIntegrationClient.Infrastructure.Services;
using LovionIntegrationClient.Infrastructure.Configuration;
using LovionIntegrationClient.Infrastructure.Soap;
using Microsoft.EntityFrameworkCore;
using LovionIntegrationClient.Infrastructure.Xml;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Logging;



var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<SoapBackendSettings>(builder.Configuration.GetSection("SoapBackend"));

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
builder.Services.AddScoped<SoapWorkOrderClient>();

builder.Services.AddSingleton<XmlWorkOrderSerializer>();
builder.Services.AddSingleton<XmlWorkOrderValidator>();


var app = builder.Build();

// Seeder uitvoeren na het bouwen van de app, voor app.Run
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IntegrationDbContext>();
    // Zorg dat de database en schema bestaan voordat er geseed wordt.
    db.Database.Migrate();
    LovionIntegrationClient.Infrastructure.DataSeeder.Seed(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Dit blok gaat over Global Exeption Handling
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        // Zorg dat we altijd een 500 + JSON teruggeven
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        // Haal de exception + logger op
        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

        if (exceptionFeature != null)
        {
            logger.LogError(
                exceptionFeature.Error,
                "Unhandled exception while processing request {Path}",
                context.Request.Path
            );
        }

        // Nette response voor de client (geen stacktrace)
        var problem = new
        {
            type = "https://httpstatuses.com/500",
            title = "An unexpected error occurred.",
            status = 500,
            traceId = context.TraceIdentifier
        };

        await context.Response.WriteAsJsonAsync(problem);
    });
});


app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
