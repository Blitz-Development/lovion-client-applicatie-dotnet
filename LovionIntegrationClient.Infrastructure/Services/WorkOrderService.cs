using LovionIntegrationClient.Core.Dtos;
using LovionIntegrationClient.Core.Services;
using LovionIntegrationClient.Infrastructure.Persistence;
using LovionIntegrationClient.Infrastructure.Soap;
using LovionIntegrationClient.Infrastructure.Xml;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using LovionIntegrationClient.Core.Domain; // voor ImportRun, ImportError, WorkOrder
using LovionIntegrationClient.Core.Validation;
using XmlValidationResult = LovionIntegrationClient.Infrastructure.Xml.ValidationResult;
using BusinessValidationResult = LovionIntegrationClient.Core.Validation.ValidationResult;



namespace LovionIntegrationClient.Infrastructure.Services;

public class WorkOrderService : IWorkOrderService
{
    private readonly IntegrationDbContext dbContext;
    private readonly SoapWorkOrderClient soapClient;
    private readonly XmlWorkOrderSerializer _xmlSerializer;
    private readonly XmlWorkOrderValidator _xmlValidator;
    private readonly WorkOrderBusinessRulesValidator _businessValidator;
    private readonly ILogger<WorkOrderService> _logger;

    public WorkOrderService(
        IntegrationDbContext dbContext,
        SoapWorkOrderClient soapClient,
        XmlWorkOrderSerializer xmlSerializer,
        XmlWorkOrderValidator xmlValidator,
        WorkOrderBusinessRulesValidator businessValidator,
        ILogger<WorkOrderService> logger)
    {
        this.dbContext = dbContext;
        this.soapClient = soapClient;
        _xmlSerializer = xmlSerializer;
        _xmlValidator = xmlValidator;
        _businessValidator = businessValidator;
        _logger = logger;
    }

    public async Task ImportFromSoapAsync(CancellationToken cancellationToken = default)
    {
        // 0. Nieuwe import-run registreren
        var importRun = new ImportRun
        {
            Id = Guid.NewGuid(),
            StartedAtUtc = DateTime.UtcNow,
            Status = "RUNNING",
            SourceSystem = "DummyLovionSoapBackend"
        };

        dbContext.ImportRuns.Add(importRun);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Logging (Fase 7)
        _logger.LogInformation(
            "Starting import run {ImportRunId}",
            importRun.Id
        );

        var total = 0;
        var invalid = 0;

        try
        {
            // 1. Haal werkorders op via SOAP-client
            var soapOrders = await soapClient.GetWorkOrdersAsync();

            foreach (var soapOrder in soapOrders)
            {
                cancellationToken.ThrowIfCancellationRequested();
                total++;

                // 2. Object -> XML string
                var xml = _xmlSerializer.ToXml(soapOrder);

                // 3. Valideer XML tegen XSD
                XmlValidationResult validation = _xmlValidator.Validate(xml);


                if (!validation.IsValid)
                {
                    invalid++;

                    var externalId = soapOrder.ExternalWorkOrderId;

                    _logger.LogWarning(
                        "Invalid SOAP workorder {ExternalId}: {Error}",
                        externalId ?? "(geen id)",
                        validation.Errors.FirstOrDefault() ?? "Unknown validation error"
                    );

                    // Errors -> Severity = Error
                    foreach (var err in validation.Errors)
                    {
                        dbContext.ImportErrors.Add(new ImportError
                        {
                            Id = Guid.NewGuid(),
                            ImportRunId = importRun.Id,
                            ExternalWorkOrderId = externalId,
                            Message = "XSD validation error for SOAP workorder.",
                            PayloadReference = null,
                            ErrorType = "XSD_VALIDATION",
                            ErrorMessage = err,
                            Severity = ErrorSeverity.Error
                        });
                    }

                    // Warnings -> Severity = Warning
                    foreach (var warn in validation.Warnings)
                    {
                        dbContext.ImportErrors.Add(new ImportError
                        {
                            Id = Guid.NewGuid(),
                            ImportRunId = importRun.Id,
                            ExternalWorkOrderId = externalId,
                            Message = "XSD validation warning for SOAP workorder.",
                            PayloadReference = null,
                            ErrorType = "XSD_VALIDATION",
                            ErrorMessage = warn,
                            Severity = ErrorSeverity.Warning
                        });
                    }

                    continue;
                }
                
                // 3b. Business-rule validatie (tweede laag)
                BusinessValidationResult businessValidation = _businessValidator.Validate(soapOrder);


                if (!businessValidation.IsValid)
                {
                    invalid++;

                    var externalId = soapOrder.ExternalWorkOrderId;

                    _logger.LogWarning(
                        "Business rule validation failed for SOAP workorder {ExternalId}: {Error}",
                        externalId ?? "(geen id)",
                        businessValidation.Errors.FirstOrDefault() ?? "Unknown business rule error"
                    );

                    foreach (var err in businessValidation.Errors)
                    {
                        dbContext.ImportErrors.Add(new ImportError
                        {
                            Id = Guid.NewGuid(),
                            ImportRunId = importRun.Id,
                            ExternalWorkOrderId = externalId,
                            Message = "Business rule validation error for SOAP workorder.",
                            PayloadReference = null,
                            ErrorType = "BUSINESS_RULE",
                            ErrorMessage = err,
                            Severity = ErrorSeverity.Error
                        });
                    }

                    foreach (var warn in businessValidation.Warnings)
                    {
                        dbContext.ImportErrors.Add(new ImportError
                        {
                            Id = Guid.NewGuid(),
                            ImportRunId = importRun.Id,
                            ExternalWorkOrderId = externalId,
                            Message = "Business rule validation warning for SOAP workorder.",
                            PayloadReference = null,
                            ErrorType = "BUSINESS_RULE",
                            ErrorMessage = warn,
                            Severity = ErrorSeverity.Warning
                        });
                    }

                    continue; // bij errors: niet mappen/opslaan
                }

                // Als er alleen warnings zouden zijn: opslaan maar wel doorgaan
                foreach (var warn in businessValidation.Warnings)
                {
                    dbContext.ImportErrors.Add(new ImportError
                    {
                        Id = Guid.NewGuid(),
                        ImportRunId = importRun.Id,
                        ExternalWorkOrderId = soapOrder.ExternalWorkOrderId,
                        Message = "Business rule validation warning for SOAP workorder.",
                        PayloadReference = null,
                        ErrorType = "BUSINESS_RULE",
                        ErrorMessage = warn,
                        Severity = ErrorSeverity.Warning
                    });
                }


                // Geldige workorder – nu echt opslaan als WorkOrder met Status = Imported
                _logger.LogInformation(
                    "Valid SOAP workorder {ExternalId} accepted for further processing.",
                    soapOrder.ExternalWorkOrderId ?? "(geen id)"
                );

                // 4. Zoek bijbehorend Asset op basis van ExternalAssetRef
                var asset = await dbContext.Assets
                    .FirstOrDefaultAsync(
                        a => a.ExternalId == soapOrder.ExternalAssetRef,
                        cancellationToken
                    );

                if (asset is null)
                {
                    // Geen asset gevonden → we kunnen deze workorder niet fatsoenlijk opslaan
                    _logger.LogWarning(
                        "No asset found for SOAP workorder {ExternalId} with assetRef {AssetRef}",
                        soapOrder.ExternalWorkOrderId ?? "(geen id)",
                        soapOrder.ExternalAssetRef ?? "(geen assetref)"
                    );

                    // we slaan deze over, maar tellen hem wel mee als geldig (geen XML-fout)
                    continue;
                }

                // 5. Bestaat deze workorder al (op basis van ExternalId)?
                var existing = await dbContext.WorkOrders
                    .FirstOrDefaultAsync(
                        w => w.ExternalId == soapOrder.ExternalWorkOrderId,
                        cancellationToken
                    );

                if (existing is null)
                {
                    // Nieuwe workorder aanmaken
                    existing = new WorkOrder
                    {
                        Id = Guid.NewGuid(),
                        AssetId = asset.Id,
                        ExternalId = soapOrder.ExternalWorkOrderId ?? string.Empty,
                        Description = soapOrder.Description,
                        ScheduledDate = soapOrder.ScheduledDate,
                        Status = WorkOrderStatus.Imported
                    };

                    dbContext.WorkOrders.Add(existing);
                }
                else
                {
                    // Dubbele ExternalWorkOrderId → we updaten bestaande record
                    _logger.LogWarning(
                        "Duplicate workorder {ExternalId} found. Updating existing WorkOrder {WorkOrderId}.",
                        soapOrder.ExternalWorkOrderId ?? "(geen id)",
                        existing.Id
                    );

                    // Bestaande workorder bijwerken
                    existing.AssetId = asset.Id;
                    existing.Description = soapOrder.Description;
                    existing.ScheduledDate = soapOrder.ScheduledDate;
                    existing.Status = WorkOrderStatus.Imported; // Haal status uit WorkOrderStatus
                }
            }

            // 5. Run-status bepalen (NA de foreach)
            if (total == 0)
            {
                importRun.Status = "EMPTY";
            }
            else if (invalid == 0)
            {
                importRun.Status = "SUCCESS";
            }
            else if (invalid == total)
            {
                importRun.Status = "FAILED";
            }
            else
            {
                importRun.Status = "PARTIAL_SUCCESS";
            }

            importRun.CompletedAtUtc = DateTime.UtcNow;

            // Einde-log met samenvatting
            _logger.LogInformation(
                "Finished import run {ImportRunId} with status {Status}. Total={Total}, Invalid={Invalid}",
                importRun.Id,
                importRun.Status,
                total,
                invalid
            );

            // 6. Wijzigingen opslaan (ImportRun + ImportErrors + WorkOrders)
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unexpected error during import run {ImportRunId}. ProcessedSoFar={Total}, InvalidSoFar={Invalid}",
                importRun.Id,
                total,
                invalid
            );

            // Voor nu: opnieuw gooien, zodat globale exception handler (Fase 7) ook z'n werk kan doen
            throw;
        }
    }


    public Task<IReadOnlyList<SoapWorkOrderDto>> FetchWorkOrdersFromSoapAsync()
    {
        _logger.LogInformation("Calling SOAP now...");
        return soapClient.GetWorkOrdersAsync();
    }

    public async Task<IReadOnlyList<WorkOrderDto>> GetAllWorkOrdersAsync(string? status = null)
    {
        // Start met een query i.p.v. directe ToListAsync
        IQueryable<WorkOrder> query = dbContext.WorkOrders.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(w => w.Status == status);
        }

        var entities = await query.ToListAsync();

        return entities
            .Select(w => new WorkOrderDto
            {
                Id = w.Id,
                ExternalWorkOrderId = w.ExternalId,
                WorkType = null,
                Priority = null,
                ScheduledDate = w.ScheduledDate,
                Status = w.Status,
                AssetId = w.AssetId
            })
            .ToList();
    }


    public async Task<WorkOrderDto?> GetWorkOrderByIdAsync(Guid id)
    {
        var entity = await dbContext.WorkOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == id);

        if (entity is null)
        {
            return null;
        }

        return new WorkOrderDto
        {
            Id = entity.Id,
            ExternalWorkOrderId = entity.ExternalId,
            WorkType = null,
            Priority = null,
            ScheduledDate = entity.ScheduledDate,
            Status = entity.Status,
            AssetId = entity.AssetId
        };
    }

    public async Task MarkAsProcessedAsync(Guid workOrderId)
    {
        // 1. Haal workorder op
        var workOrder = await dbContext.WorkOrders
            .FirstOrDefaultAsync(w => w.Id == workOrderId);

        if (workOrder is null)
        {
            // Voor nu alleen loggen. In de controller gaan we hier later 404 van maken.
            _logger.LogWarning(
                "Tried to mark non-existing workorder {WorkOrderId} as processed",
                workOrderId
            );
            return;
        }

        // 2. Status updaten naar PROCESSED
        workOrder.Status = WorkOrderStatus.Processed;

        // 3. Opslaan
        await dbContext.SaveChangesAsync();
    }
}