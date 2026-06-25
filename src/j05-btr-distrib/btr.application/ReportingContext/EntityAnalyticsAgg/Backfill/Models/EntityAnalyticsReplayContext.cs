using System;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models
{
    public sealed class EntityAnalyticsReplayContext
    {
        public int PeriodYear { get; set; }
        public int PeriodMonth { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public string EntityTypeCode { get; set; }
        public bool IsDryRun { get; set; }
        public EntityAnalyticsReplayResumeMode ResumeMode { get; set; }
        public bool SkipLiveMutexCheck { get; set; }
        public string BackfillJobId { get; set; }
    }
}
