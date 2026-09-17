namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.UseCases
{
    public class RefreshCustomerPrincipalRelationshipRequest
    {
        public string TriggeredBy { get; set; } = "Scheduler";

        public RefreshCustomerPrincipalRelationshipResult Result { get; set; }
    }

    public class RefreshCustomerPrincipalRelationshipResult
    {
        public string RefreshLogId { get; set; }

        public int DurationMs { get; set; }
    }
}
