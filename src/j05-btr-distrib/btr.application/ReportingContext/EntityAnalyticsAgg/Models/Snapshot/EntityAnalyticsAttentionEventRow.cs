using System;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot
{
    /// <summary>L3 attention lifecycle row (M32.5+).</summary>
    public class EntityAnalyticsAttentionEventRow
    {
        public string EntityAnalyticsAttentionId { get; set; }

        public string EntityType { get; set; }

        public string EntityId { get; set; }

        public string EntityCode { get; set; }

        public string SignalCode { get; set; }

        public string SignalCategory { get; set; }

        public string SignalTitle { get; set; }

        public int FirstSeenPeriodYear { get; set; }

        public int FirstSeenPeriodMonth { get; set; }

        public int LastSeenPeriodYear { get; set; }

        public int LastSeenPeriodMonth { get; set; }

        public int ConsecutivePeriods { get; set; }

        public int TotalOccurrences { get; set; }

        public bool IsActive { get; set; }

        public DateTime GeneratedAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public string LastRefreshLogId { get; set; }
    }
}
