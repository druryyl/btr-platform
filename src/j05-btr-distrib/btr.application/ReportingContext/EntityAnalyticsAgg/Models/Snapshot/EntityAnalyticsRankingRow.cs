using System;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot
{
    /// <summary>L2 ranking history row (M32.4+).</summary>
    public class EntityAnalyticsRankingRow
    {
        public string EntityType { get; set; }

        public string EntityId { get; set; }

        public string EntityCode { get; set; }

        public string RankMetricKpiId { get; set; }

        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public int RankPosition { get; set; }

        public int PopulationSize { get; set; }

        public decimal Percentile { get; set; }

        public DateTime GeneratedAt { get; set; }

        public string LastRefreshLogId { get; set; }
    }
}
