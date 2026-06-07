namespace btr.application.ReportingContext.DashboardSnapshotAgg.UseCases
{
    public class RefreshDashboardPiutangSnapshotRequest
    {
        public string TriggeredBy { get; set; } = "Scheduler";

        public RefreshDashboardPiutangSnapshotResult Result { get; set; }
    }

    public class RefreshDashboardPiutangSnapshotResult
    {
        public string RefreshLogId { get; set; }

        public int DurationMs { get; set; }
    }
}
