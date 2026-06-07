using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Models
{
    public class DashboardSalesAggregateResult
    {
        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public decimal TotalOmzet { get; set; }

        public decimal CompletedOmzet { get; set; }

        public decimal PipelineOmzet { get; set; }

        public int TotalFaktur { get; set; }

        public int TotalCustomer { get; set; }

        public DateTime GeneratedAt { get; set; }

        public decimal TotalTarget { get; set; }

        public decimal TotalAchievement { get; set; }

        public decimal? AchievementPercent { get; set; }

        public List<DashboardSalesWeekTrendRow> WeekTrend { get; set; }
            = new List<DashboardSalesWeekTrendRow>();

        public List<DashboardSalesTopSalesmanRow> TopSalesman { get; set; }
            = new List<DashboardSalesTopSalesmanRow>();
    }

    public class DashboardSalesWeekTrendRow
    {
        public DateTime WeekStart { get; set; }

        public DateTime WeekEnd { get; set; }

        public string WeekLabel { get; set; }

        public decimal RecognizedAmount { get; set; }
    }

    public class DashboardSalesTopSalesmanRow
    {
        public int Rank { get; set; }

        public string SalesPersonName { get; set; }

        public decimal CompletedOmzet { get; set; }
    }
}
