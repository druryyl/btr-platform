using System;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot
{
    public class EntityAnalyticsCurrentRow
    {
        public string EntityAnalyticsCurrentId { get; set; }

        public string SnapshotKey { get; set; }

        public string EntityType { get; set; }

        public string EntityId { get; set; }

        public string EntityCode { get; set; }

        public string KpiId { get; set; }

        public decimal? NumericValue { get; set; }

        public string TextValue { get; set; }

        public int? DefinitionVersion { get; set; }

        public DateTime GeneratedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public string LastRefreshLogId { get; set; }
    }
}
