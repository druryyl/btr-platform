namespace btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot
{
    using System;

    /// <summary>L4 relationship rollup row (M32.6+).</summary>
    public class EntityAnalyticsRelationshipRow
    {
        public string SourceEntityType { get; set; }

        public string SourceEntityId { get; set; }

        public string SourceEntityCode { get; set; }

        public string RelationshipCode { get; set; }

        public string TargetEntityType { get; set; }

        public string TargetEntityId { get; set; }

        public string TargetEntityCode { get; set; }

        public string TargetDisplayName { get; set; }

        public decimal? MetricValue { get; set; }

        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public int Rank { get; set; }

        public DateTime GeneratedAt { get; set; }
    }
}
