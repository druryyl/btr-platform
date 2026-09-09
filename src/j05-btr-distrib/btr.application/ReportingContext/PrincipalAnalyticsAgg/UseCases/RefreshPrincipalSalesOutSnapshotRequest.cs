namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.UseCases
{
    public class RefreshPrincipalSalesOutSnapshotRequest
    {
        public string TriggeredBy { get; set; } = "Scheduler";

        public RefreshPrincipalSalesOutSnapshotResult Result { get; set; }
    }

    public class RefreshPrincipalSalesOutSnapshotResult
    {
        public string RefreshLogId { get; set; }

        public int DurationMs { get; set; }
    }
}
