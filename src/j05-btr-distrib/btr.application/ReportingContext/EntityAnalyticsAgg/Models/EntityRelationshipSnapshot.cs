namespace btr.application.ReportingContext.EntityAnalyticsAgg.Models
{
    /// <summary>Normalized relationship row emitted by an entity producer at refresh time.</summary>
    public class EntityRelationshipSnapshot
    {
        public string SourceEntityId { get; set; }

        public string SourceEntityCode { get; set; }

        public string RelationshipCode { get; set; }

        public string TargetEntityType { get; set; }

        public string TargetEntityId { get; set; }

        public string TargetEntityCode { get; set; }

        public string TargetDisplayName { get; set; }

        public decimal? MetricValue { get; set; }
    }
}
