using System;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot
{
    /// <summary>L5 radar axis score row (M32.8).</summary>
    public class EntityAnalyticsRadarScoreRow
    {
        public string EntityType { get; set; }

        public string EntityId { get; set; }

        public string EntityCode { get; set; }

        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public string AxisKpiId { get; set; }

        public decimal? Score { get; set; }

        public string PeerGroupRuleId { get; set; }

        public int PeerGroupSize { get; set; }

        public string NormalizationMethod { get; set; }

        public DateTime GeneratedAt { get; set; }

        public string LastRefreshLogId { get; set; }
    }
}
