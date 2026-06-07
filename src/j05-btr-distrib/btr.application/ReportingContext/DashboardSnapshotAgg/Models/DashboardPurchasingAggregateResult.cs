using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Models
{
    public class DashboardPurchasingAggregateResult
    {
        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public decimal GrandTotalPurchase { get; set; }

        public int TotalInvoice { get; set; }

        public int PendingPostingInvoiceCount { get; set; }

        public DateTime GeneratedAt { get; set; }

        public List<DashboardPurchasingWeekTrendRow> WeekTrend { get; set; }
            = new List<DashboardPurchasingWeekTrendRow>();

        public List<DashboardPurchasingPostingStatusRow> PostingStatus { get; set; }
            = new List<DashboardPurchasingPostingStatusRow>();

        public List<DashboardPurchasingTopPrincipalRow> TopPrincipal { get; set; }
            = new List<DashboardPurchasingTopPrincipalRow>();
    }

    public class DashboardPurchasingWeekTrendRow
    {
        public DateTime WeekStart { get; set; }

        public DateTime WeekEnd { get; set; }

        public string WeekLabel { get; set; }

        public decimal PurchaseAmount { get; set; }
    }

    public class DashboardPurchasingPostingStatusRow
    {
        public string StatusKey { get; set; }

        public string StatusLabel { get; set; }

        public int SortOrder { get; set; }

        public decimal PurchaseAmount { get; set; }
    }

    public class DashboardPurchasingTopPrincipalRow
    {
        public int Rank { get; set; }

        public string PrincipalName { get; set; }

        public decimal PurchaseAmount { get; set; }
    }
}
