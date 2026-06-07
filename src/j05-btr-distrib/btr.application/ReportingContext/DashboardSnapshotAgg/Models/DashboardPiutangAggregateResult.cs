using System;
using System.Collections.Generic;
using btr.application.ReportingContext.DashboardPiutangAgg.Queries;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Models
{
    public class DashboardPiutangAggregateResult
    {
        public decimal TotalPiutang { get; set; }

        public int TotalCustomer { get; set; }

        public DateTime GeneratedAt { get; set; }

        public int OverdueCustomer { get; set; }

        public List<DashboardPiutangAgingBucket> AgingBuckets { get; set; }
            = new List<DashboardPiutangAgingBucket>();

        public List<DashboardPiutangTopCustomer> TopCustomers { get; set; }
            = new List<DashboardPiutangTopCustomer>();
    }
}
