using System;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models
{
    public sealed class EntityAnalyticsBackfillJobModel
    {
        public string BackfillJobId { get; set; }
        public string EntityTypeScope { get; set; }
        public int FromPeriodYear { get; set; }
        public int FromPeriodMonth { get; set; }
        public int ToPeriodYear { get; set; }
        public int ToPeriodMonth { get; set; }
        public string Layers { get; set; }
        public string OptionsJson { get; set; }
        public string Status { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string TriggeredBy { get; set; }
        public string MachineName { get; set; }
        public string LastError { get; set; }
    }
}
