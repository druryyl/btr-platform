using System;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models
{
    public sealed class EntityAnalyticsBackfillCheckpointModel
    {
        public string BackfillCheckpointId { get; set; }
        public string BackfillJobId { get; set; }
        public string EntityType { get; set; }
        public int PeriodYear { get; set; }
        public int PeriodMonth { get; set; }
        public string Status { get; set; }
        public string LayersCompleted { get; set; }
        public int EntityCount { get; set; }
        public string RowCountsJson { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string LastError { get; set; }
        public string LastRefreshLogId { get; set; }
    }
}
