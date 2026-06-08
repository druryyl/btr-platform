namespace btr.application.ReportingContext.DashboardSnapshotAgg.UseCases
{
    public class RefreshDashboardInventoryRiskSnapshotRequest
    {
        public string TriggeredBy { get; set; } = "Scheduler";

        public RefreshDashboardInventoryRiskSnapshotResult Result { get; set; }
    }

    public class RefreshDashboardInventoryRiskSnapshotResult
    {
        public string RefreshLogId { get; set; }

        public int DurationMs { get; set; }
    }
}
