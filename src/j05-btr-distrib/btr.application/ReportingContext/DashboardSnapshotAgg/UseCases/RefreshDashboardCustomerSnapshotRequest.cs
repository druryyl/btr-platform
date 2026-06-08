namespace btr.application.ReportingContext.DashboardSnapshotAgg.UseCases
{
    public class RefreshDashboardCustomerSnapshotRequest
    {
        public string TriggeredBy { get; set; } = "Scheduler";

        public RefreshDashboardCustomerSnapshotResult Result { get; set; }
    }

    public class RefreshDashboardCustomerSnapshotResult
    {
        public string RefreshLogId { get; set; }

        public int DurationMs { get; set; }
    }
}
