namespace btr.application.ReportingContext.DashboardSnapshotAgg.UseCases
{
    public class RefreshDashboardSalesmanSnapshotRequest
    {
        public string TriggeredBy { get; set; } = "Scheduler";

        public RefreshDashboardSalesmanSnapshotResult Result { get; set; }
    }

    public class RefreshDashboardSalesmanSnapshotResult
    {
        public string RefreshLogId { get; set; }

        public int DurationMs { get; set; }
    }
}
