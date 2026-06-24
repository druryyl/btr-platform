namespace btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot
{
    /// <summary>L1 population metric used by the ranking engine at refresh time.</summary>
    public class EntityAnalyticsPeriodMetricRow
    {
        public string EntityId { get; set; }

        public string EntityCode { get; set; }

        public decimal? NumericValue { get; set; }

        public bool IsActive { get; set; }
    }
}
