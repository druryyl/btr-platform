namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.UseCases
{
    public class RefreshPrincipalMomGrowthSnapshotRequest
    {
        public string TriggeredBy { get; set; } = "Scheduler";

        public RefreshPrincipalMomGrowthSnapshotResult Result { get; set; }
    }

    public class RefreshPrincipalMomGrowthSnapshotResult
    {
        public string RefreshLogId { get; set; }

        public int DurationMs { get; set; }
    }
}
