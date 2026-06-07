namespace btr.application.ReportingContext.DashboardSnapshotAgg.UseCases
{
    public class RefreshDashboardInventorySnapshotRequest
    {
        public string TriggeredBy { get; set; } = "Scheduler";

        public RefreshDashboardInventorySnapshotResult Result { get; set; }
    }

    public class RefreshDashboardInventorySnapshotResult
    {
        public string RefreshLogId { get; set; }

        public int DurationMs { get; set; }
    }
}
