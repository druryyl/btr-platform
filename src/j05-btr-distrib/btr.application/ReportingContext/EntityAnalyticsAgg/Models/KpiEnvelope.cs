namespace btr.application.ReportingContext.EntityAnalyticsAgg.Models
{
    public class KpiEnvelope
    {
        public string KpiId { get; set; }

        public EntityKpiCategory Category { get; set; }

        public string DisplayName { get; set; }

        public decimal? Value { get; set; }

        public string TextValue { get; set; }

        public string FormattedValue { get; set; }

        public string Unit { get; set; }

        public string Direction { get; set; }

        public string PeriodLabel { get; set; }

        public string EvidenceRoute { get; set; }
    }
}
