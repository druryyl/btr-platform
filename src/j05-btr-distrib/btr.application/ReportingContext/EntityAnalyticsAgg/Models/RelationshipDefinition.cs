namespace btr.application.ReportingContext.EntityAnalyticsAgg.Models
{
    public class RelationshipDefinition
    {
        public string RelationshipCode { get; set; }

        public string DisplayName { get; set; }

        public string TargetEntityType { get; set; }

        public string MetricKpiId { get; set; }

        public string PeriodSemantics { get; set; }

        public int TopN { get; set; } = 10;
    }
}
