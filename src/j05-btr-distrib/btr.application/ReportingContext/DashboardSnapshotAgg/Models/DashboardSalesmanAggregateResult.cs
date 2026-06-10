using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Models
{
    public class DashboardSalesmanAggregateResult
    {
        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public decimal TotalTeamOmzet { get; set; }

        public decimal TotalPiutang { get; set; }

        public int ActiveSalesmanCount { get; set; }

        public int BelowTargetCount { get; set; }

        public int MissingTargetSetupCount { get; set; }

        public int HighOverdueExposureCount { get; set; }

        public int HighPiutangExposureCount { get; set; }

        public int CustomerConcentrationCount { get; set; }

        public int DormantPortfolioCount { get; set; }

        public decimal? TopOmzetSalesmanPercent { get; set; }

        public decimal? TopPiutangSalesmanPercent { get; set; }

        public DateTime GeneratedAt { get; set; }

        public List<DashboardSalesmanTopOmzetRow> TopOmzet { get; set; }
            = new List<DashboardSalesmanTopOmzetRow>();

        public List<DashboardSalesmanTopAchievementRow> TopAchievement { get; set; }
            = new List<DashboardSalesmanTopAchievementRow>();

        public List<DashboardSalesmanTopPiutangRow> TopPiutang { get; set; }
            = new List<DashboardSalesmanTopPiutangRow>();

        public List<DashboardSalesmanAttentionRow> AttentionList { get; set; }
            = new List<DashboardSalesmanAttentionRow>();

        public List<DashboardSalesmanSegmentationRow> Segmentation { get; set; }
            = new List<DashboardSalesmanSegmentationRow>();

        public List<DashboardSalesmanPrincipalAchievementRow> PrincipalAchievement { get; set; }
            = new List<DashboardSalesmanPrincipalAchievementRow>();

        public List<DashboardSalesmanRepHistoryRow> RepHistory { get; set; }
            = new List<DashboardSalesmanRepHistoryRow>();
    }

    public class DashboardSalesmanTopOmzetRow
    {
        public int Rank { get; set; }

        public string SalesPersonId { get; set; }

        public string SalesPersonCode { get; set; }

        public string SalesPersonName { get; set; }

        public decimal CompletedOmzet { get; set; }

        public decimal? PercentOfTotal { get; set; }

        public bool IsActive { get; set; }
    }

    public class DashboardSalesmanTopAchievementRow
    {
        public int Rank { get; set; }

        public string SalesPersonId { get; set; }

        public string SalesPersonCode { get; set; }

        public string SalesPersonName { get; set; }

        public decimal? TargetAmount { get; set; }

        public decimal CompletedOmzet { get; set; }

        public decimal? AchievementPercent { get; set; }

        public decimal? PercentOfTotal { get; set; }

        public bool IsActive { get; set; }
    }

    public class DashboardSalesmanTopPiutangRow
    {
        public int Rank { get; set; }

        public string SalesPersonId { get; set; }

        public string SalesPersonCode { get; set; }

        public string SalesPersonName { get; set; }

        public decimal OutstandingBalance { get; set; }

        public decimal? PercentOfTotal { get; set; }

        public bool IsActive { get; set; }
    }

    public class DashboardSalesmanAttentionRow
    {
        public string SalesPersonId { get; set; }

        public string SalesPersonCode { get; set; }

        public string SalesPersonName { get; set; }

        public string SignalKey { get; set; }

        public string SignalLabel { get; set; }

        public decimal? ValueAmount { get; set; }

        public string ValueText { get; set; }

        public string WilayahName { get; set; }

        public int SortOrder { get; set; }

        public bool IsActive { get; set; }
    }

    public class DashboardSalesmanSegmentationRow
    {
        public string SegmentType { get; set; }

        public string SegmentKey { get; set; }

        public string SegmentLabel { get; set; }

        public int SalesmanCount { get; set; }

        public int ActiveCount { get; set; }

        public int InactiveCount { get; set; }

        public int SortOrder { get; set; }
    }

    public class DashboardSalesmanPrincipalAchievementRow
    {
        public string SalesPersonId { get; set; }

        public string SalesPersonCode { get; set; }

        public string SalesPersonName { get; set; }

        public string SupplierId { get; set; }

        public string SupplierName { get; set; }

        public decimal? TargetAmount { get; set; }

        public decimal CompletedOmzet { get; set; }

        public decimal? AchievementPercent { get; set; }

        public int SortOrder { get; set; }
    }

    public class DashboardSalesmanRepHistoryRow
    {
        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public string SalesPersonId { get; set; }

        public string SalesPersonCode { get; set; }

        public string SalesPersonName { get; set; }

        public decimal? TargetAmount { get; set; }

        public decimal CompletedOmzet { get; set; }

        public decimal? AchievementPercent { get; set; }

        public string AchievementBand { get; set; }

        public decimal OpenBalance { get; set; }

        public bool IsActive { get; set; }
    }
}
