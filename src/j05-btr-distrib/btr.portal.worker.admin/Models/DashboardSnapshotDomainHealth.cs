using System;

namespace btr.portal.worker.admin.Models
{
    public class DashboardSnapshotDomainHealth
    {
        public string Domain { get; set; }
        public int IntervalMinutes { get; set; }
        public DashboardSnapshotLastRefresh LastRefresh { get; set; }

        public bool IsStale
        {
            get
            {
                if (LastRefresh?.CompletedAt == null || IntervalMinutes <= 0)
                    return false;
                return (DateTime.UtcNow - LastRefresh.CompletedAt.Value).TotalMinutes > IntervalMinutes * 1.5;
            }
        }
    }

    public class DashboardSnapshotLastRefresh
    {
        public string RefreshLogId { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string Status { get; set; }
        public int DurationMs { get; set; }
        public string ErrorMessage { get; set; }
        public string TriggeredBy { get; set; }
    }
}
