namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.UseCases
{
    public class RefreshPrincipalReturnHistoryRequest
    {
        public string TriggeredBy { get; set; } = "Scheduler";

        public RefreshPrincipalReturnHistoryResult Result { get; set; }
    }

    public class RefreshPrincipalReturnHistoryResult
    {
        public string RefreshLogId { get; set; }

        public int DurationMs { get; set; }
    }
}
