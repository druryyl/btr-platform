using System;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Progress
{
    public sealed class WorkerProgressStepInfo
    {
        public int? RecordCount { get; set; }

        public TimeSpan? Duration { get; set; }

        public string Detail { get; set; }
    }
}
