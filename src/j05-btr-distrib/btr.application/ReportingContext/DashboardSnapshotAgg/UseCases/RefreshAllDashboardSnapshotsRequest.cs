using System.Collections.Generic;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.UseCases
{
    public class RefreshAllDashboardSnapshotsRequest
    {
        public string TriggeredBy { get; set; } = "Scheduler";

        public RefreshAllDashboardSnapshotsResult Result { get; set; }
    }

    public class RefreshAllDashboardSnapshotsResult
    {
        public IList<RefreshDashboardDomainResult> Domains { get; set; }
            = new List<RefreshDashboardDomainResult>();
    }

    public class RefreshDashboardDomainResult
    {
        public string Domain { get; set; }

        public string RefreshLogId { get; set; }

        public int DurationMs { get; set; }
    }
}
