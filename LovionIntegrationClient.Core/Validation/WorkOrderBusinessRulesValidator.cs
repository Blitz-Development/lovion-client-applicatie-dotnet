using LovionIntegrationClient.Core.Dtos;

namespace LovionIntegrationClient.Core.Validation;

public sealed class WorkOrderBusinessRulesValidator
{
    public ValidationResult Validate(SoapWorkOrderDto workOrder)
    {
        var result = new ValidationResult();

        // Rule 1:
        // Als Priority = "HIGH", dan moet ScheduledDate bestaan én binnen 7 dagen vanaf nu liggen.
        if (string.Equals(workOrder.Priority, "HIGH", StringComparison.OrdinalIgnoreCase))
        {
            if (workOrder.ScheduledDate is null)
            {
                result.Errors.Add("Priority HIGH requires ScheduledDate.");
            }
            else
            {
                var now = DateTime.UtcNow.Date;
                var scheduled = workOrder.ScheduledDate.Value.Date;

                if (scheduled < now)
                {
                    result.Errors.Add("Priority HIGH: ScheduledDate may not be in the past.");
                }
                else if (scheduled > now.AddDays(7))
                {
                    result.Errors.Add("Priority HIGH: ScheduledDate must be within 7 days.");
                }
            }
        }
        
        // Rule 2:
        // Als Status = "COMPLETED", dan moet Description bestaan én minimaal 10 tekens zijn.
        if (string.Equals(workOrder.Status, "COMPLETED", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(workOrder.Description))
            {
                result.Errors.Add("Status COMPLETED requires Description.");
            }
            else if (workOrder.Description.Trim().Length < 10)
            {
                result.Errors.Add("Status COMPLETED: Description must be at least 10 characters.");
            }
        }


        return result;
    }
}