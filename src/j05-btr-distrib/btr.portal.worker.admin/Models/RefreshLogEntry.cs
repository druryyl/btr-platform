using System;

namespace btr.portal.worker.admin.Models
{
    public class RefreshLogEntry
    {
        public string Domain { get; set; }
        public string Status { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public int DurationMs { get; set; }
        public string TriggeredBy { get; set; }
        public string ErrorMessage { get; set; }
    }
}
