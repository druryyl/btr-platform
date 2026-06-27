using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Models
{
    public class DashboardCustomerAggregateResult
    {
        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public decimal TotalOmzet { get; set; }

        public decimal TotalPiutang { get; set; }

        public int ActiveCustomerCount { get; set; }

        public int DormantCustomerCount { get; set; }

        public int OverdueCustomerCount { get; set; }

        public int PlafondBreachCount { get; set; }

        public int SuspendedWithSalesCount { get; set; }

        public decimal AgingOver90Amount { get; set; }

        public decimal? TopOmzetCustomerPercent { get; set; }

        public decimal? TopPiutangCustomerPercent { get; set; }

        public DateTime GeneratedAt { get; set; }

        public List<DashboardCustomerTopOmzetRow> TopOmzet { get; set; }
            = new List<DashboardCustomerTopOmzetRow>();

        public List<DashboardCustomerTopPiutangRow> TopPiutang { get; set; }
            = new List<DashboardCustomerTopPiutangRow>();

        public List<DashboardCustomerAttentionRow> AttentionList { get; set; }
            = new List<DashboardCustomerAttentionRow>();

        public List<DashboardCustomerSegmentationRow> Segmentation { get; set; }
            = new List<DashboardCustomerSegmentationRow>();
    }

    public class DashboardCustomerTopOmzetRow
    {
        public int Rank { get; set; }

        public string CustomerId { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public decimal OmzetAmount { get; set; }

        public decimal? PercentOfTotal { get; set; }
    }

    public class DashboardCustomerTopPiutangRow
    {
        public int Rank { get; set; }

        public string CustomerId { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public decimal OutstandingBalance { get; set; }

        public decimal? PercentOfTotal { get; set; }
    }

    public class DashboardCustomerAttentionRow
    {
        public string CustomerId { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public string SignalKey { get; set; }

        public string SignalLabel { get; set; }

        public decimal? ValueAmount { get; set; }

        public string ValueText { get; set; }

        public string WilayahName { get; set; }

        public int SortOrder { get; set; }
    }

    public class DashboardCustomerSegmentationRow
    {
        public string SegmentType { get; set; }

        public string SegmentKey { get; set; }

        public string SegmentLabel { get; set; }

        public int CustomerCount { get; set; }

        public int ActiveCount { get; set; }

        public int DormantCount { get; set; }

        public int SortOrder { get; set; }
    }
}
