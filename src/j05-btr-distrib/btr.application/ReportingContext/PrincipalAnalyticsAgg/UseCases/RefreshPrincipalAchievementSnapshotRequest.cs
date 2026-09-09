namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.UseCases
{
    public class RefreshPrincipalAchievementSnapshotRequest
    {
        public string TriggeredBy { get; set; } = "Scheduler";

        public RefreshPrincipalAchievementSnapshotResult Result { get; set; }
    }

    public class RefreshPrincipalAchievementSnapshotResult
    {
        public string RefreshLogId { get; set; }

        public int DurationMs { get; set; }
    }
}
