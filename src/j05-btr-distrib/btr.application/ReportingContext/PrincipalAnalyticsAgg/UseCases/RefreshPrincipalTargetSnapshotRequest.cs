namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.UseCases
{
    public class RefreshPrincipalTargetSnapshotRequest
    {
        public string TriggeredBy { get; set; } = "Scheduler";

        public RefreshPrincipalTargetSnapshotResult Result { get; set; }
    }

    public class RefreshPrincipalTargetSnapshotResult
    {
        public string RefreshLogId { get; set; }

        public int DurationMs { get; set; }
    }
}
