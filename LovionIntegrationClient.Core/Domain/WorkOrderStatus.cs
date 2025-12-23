namespace LovionIntegrationClient.Core.Domain
{
    /// <summary>
    /// Bevat alle mogelijke statussen voor een WorkOrder.
    /// Gebruik deze constanten in plaats van losse strings in je code.
    /// </summary>
    public static class WorkOrderStatus
    {
        public const string Imported = "IMPORTED";
        public const string ValidationFailed = "VALIDATION_FAILED";
        public const string Ready = "READY";
        public const string Processed = "PROCESSED";
    }
}