namespace btr.application.ReportingContext.EntityAnalyticsAgg.Models
{
    public class PeerGroupRuleDefinition
    {
        public string RuleId { get; set; }

        public string EntityType { get; set; }

        public string DisplayLabel { get; set; }

        public string DimensionLabel { get; set; }

        public bool IsDefault { get; set; }
    }
}
