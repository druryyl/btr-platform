using System;
using System.Collections.Generic;

namespace btr.portal.worker.admin.Models
{
    public class DashboardSnapshotHealthData
    {
        public string Status { get; set; }
        public DateTime CheckedAtUtc { get; set; }
        public IList<DashboardSnapshotDomainHealth> Domains { get; set; }
    }
}
