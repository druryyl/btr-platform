using System;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Models
{
    public class DashboardSnapshotRefreshStatusModel
    {
        public string Domain { get; set; }

        public string RefreshLogId { get; set; }

        public DateTime StartedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public string Status { get; set; }

        public int DurationMs { get; set; }

        public string ErrorMessage { get; set; }

        public string TriggeredBy { get; set; }
    }
}
