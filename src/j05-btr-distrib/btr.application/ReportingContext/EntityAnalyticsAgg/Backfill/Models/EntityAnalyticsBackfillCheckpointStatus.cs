namespace btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models
{
    public static class EntityAnalyticsBackfillCheckpointStatus
    {
        public const string Pending = "Pending";
        public const string Running = "Running";
        public const string Completed = "Completed";
        public const string Failed = "Failed";
        public const string Skipped = "Skipped";
        public const string Cancelled = "Cancelled";
        public const string DryRunCompleted = "DryRunCompleted";
    }
}
