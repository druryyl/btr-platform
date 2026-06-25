using System;
using System.Threading;
using btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Backfill.Models
{
    public sealed class EntityAnalyticsBackfillRequest
    {
        public string TriggeredBy { get; set; }
        public string RefreshLogId { get; set; }
        public string EntityTypeScope { get; set; } = "All";
        public int? FromPeriodYear { get; set; }
        public int? FromPeriodMonth { get; set; }
        public int? ToPeriodYear { get; set; }
        public int? ToPeriodMonth { get; set; }
        public string Layers { get; set; } = "L1,L2,L5";
        public bool Resume { get; set; } = true;
        public bool Restart { get; set; }
        public bool Force { get; set; }
        public bool DryRun { get; set; }
        public bool ContinueOnError { get; set; }
        public int BatchSize { get; set; } = 500;
        public string ConfirmToken { get; set; }
        public bool SkipLiveMutexCheck { get; set; }
        public CancellationToken CancellationToken { get; set; } = CancellationToken.None;

        public EntityAnalyticsBackfillResult Result { get; set; }
    }

    public sealed class EntityAnalyticsBackfillResult
    {
        public string BackfillJobId { get; set; }
        public string Status { get; set; }
        public int PeriodsProcessed { get; set; }
        public int PeriodsSkipped { get; set; }
        public int DurationMs { get; set; }
        public string ErrorMessage { get; set; }
    }
}
