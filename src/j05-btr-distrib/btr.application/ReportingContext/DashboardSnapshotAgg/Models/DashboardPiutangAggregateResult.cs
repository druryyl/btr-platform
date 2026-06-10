using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Models
{
    public class DashboardPiutangAggregateResult
    {
        public decimal TotalPiutang { get; set; }

        public int TotalCustomer { get; set; }

        public DateTime GeneratedAt { get; set; }

        public int OverdueCustomer { get; set; }

        public decimal OverduePiutang { get; set; }

        public decimal AgingOver90Amount { get; set; }

        public decimal? AgingOver90Percent { get; set; }

        public decimal? Top10CustomerConcentrationPercent { get; set; }

        public decimal? Top20CustomerConcentrationPercent { get; set; }

        public int SkippedCustomerIdRowCount { get; set; }

        public List<DashboardPiutangAgingBucket> AgingBuckets { get; set; }
            = new List<DashboardPiutangAgingBucket>();

        public List<DashboardPiutangCustomerAgingRow> CustomerAging { get; set; }
            = new List<DashboardPiutangCustomerAgingRow>();

        public List<DashboardPiutangTopCustomerRiskRow> TopCustomerRisk { get; set; }
            = new List<DashboardPiutangTopCustomerRiskRow>();
    }
}
