namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.UseCases
{
    public class RefreshPrincipalInventorySnapshotRequest
    {
        public string TriggeredBy { get; set; } = "Scheduler";

        public RefreshPrincipalInventorySnapshotResult Result { get; set; }
    }

    public class RefreshPrincipalInventorySnapshotResult
    {
        public string RefreshLogId { get; set; }

        public int DurationMs { get; set; }
    }
}
