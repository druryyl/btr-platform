namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.UseCases
{
    public class RefreshPrincipalSalesOutHistoryRequest
    {
        public string TriggeredBy { get; set; } = "Scheduler";

        public RefreshPrincipalSalesOutHistoryResult Result { get; set; }
    }

    public class RefreshPrincipalSalesOutHistoryResult
    {
        public string RefreshLogId { get; set; }

        public int DurationMs { get; set; }
    }
}
