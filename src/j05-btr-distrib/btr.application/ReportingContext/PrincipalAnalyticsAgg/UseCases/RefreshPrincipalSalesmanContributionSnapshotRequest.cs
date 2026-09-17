namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.UseCases
{
    public class RefreshPrincipalSalesmanContributionSnapshotRequest
    {
        public string TriggeredBy { get; set; }

        public RefreshPrincipalSalesmanContributionSnapshotResult Result { get; set; }
    }

    public class RefreshPrincipalSalesmanContributionSnapshotResult
    {
        public string RefreshLogId { get; set; }

        public int DurationMs { get; set; }
    }
}
