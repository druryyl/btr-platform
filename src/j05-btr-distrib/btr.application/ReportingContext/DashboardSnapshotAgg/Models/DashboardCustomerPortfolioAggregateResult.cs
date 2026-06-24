using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Models
{
    public sealed class PortfolioCustomerContext
    {
        public string CustomerKey { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public string WilayahName { get; set; }

        public string Klasifikasi { get; set; }

        public string LifecycleStage { get; set; }

        public string PortfolioTier { get; set; }

        public string PrimaryActionKey { get; set; }

        public string ActionOwner { get; set; }

        public string ActionReasonText { get; set; }

        public string TriggeredRuleIds { get; set; }

        public bool IsAttention { get; set; }

        public int PortfolioPriorityScore { get; set; }

        public decimal MtdOmzet { get; set; }

        public decimal OpenBalance { get; set; }

        public decimal? OverdueBalance { get; set; }

        public DateTime? LastPurchaseDate { get; set; }

        public DateTime? FirstPurchaseDate { get; set; }

        public int FakturCount6Mo { get; set; }

        public bool IsActiveMtd { get; set; }

        public string M29Category { get; set; }

        public string M29PrimarySignalKey { get; set; }

        public string SalesPersonName { get; set; }

        public decimal? SalesmanAchievementPercent { get; set; }

        public bool SalesmanHighPiutangExposure { get; set; }

        public string M30LinkRoute { get; set; }

        public string CustomerReportRoute { get; set; }

        public string ValueDisclaimer { get; set; }
    }

    public class DashboardCustomerPortfolioAggregateResult
    {
        public DateTime GeneratedAt { get; set; }

        public DateTime BusinessDate { get; set; }

        public DashboardCustomerPortfolioKpiSnapshot Kpi { get; set; }
            = new DashboardCustomerPortfolioKpiSnapshot();

        public List<DashboardCustomerPortfolioDistRow> LifecycleDistribution { get; set; }
            = new List<DashboardCustomerPortfolioDistRow>();

        public List<DashboardCustomerPortfolioDistRow> TierDistribution { get; set; }
            = new List<DashboardCustomerPortfolioDistRow>();

        public List<DashboardCustomerPortfolioDistRow> ActionDistribution { get; set; }
            = new List<DashboardCustomerPortfolioDistRow>();

        public List<DashboardCustomerPortfolioPriorityRow> PriorityQueue { get; set; }
            = new List<DashboardCustomerPortfolioPriorityRow>();

        public List<DashboardCustomerPortfolioCustomerRow> Customers { get; set; }
            = new List<DashboardCustomerPortfolioCustomerRow>();

        public List<DashboardCustomerPortfolioConcentrationRow> Concentration { get; set; }
            = new List<DashboardCustomerPortfolioConcentrationRow>();

        public List<DashboardCustomerPortfolioWilayahRow> WilayahBreakdown { get; set; }
            = new List<DashboardCustomerPortfolioWilayahRow>();
    }

    public class DashboardCustomerPortfolioKpiSnapshot
    {
        public decimal PortfolioHealthScore { get; set; }

        public decimal PortfolioHealthyPercent { get; set; }

        public int TotalCustomerCount { get; set; }

        public int AttentionCustomerCount { get; set; }

        public int StrategicCustomerCount { get; set; }

        public int StrategicAtRiskCount { get; set; }

        public int CustomersAtRiskCount { get; set; }

        public decimal WorkingCapitalTiedAmount { get; set; }

        public decimal TotalMtdOmzet { get; set; }

        public decimal TotalOpenBalance { get; set; }

        public int NeverPurchasedCount { get; set; }

        public int DormantCount { get; set; }

        public int DecliningCount { get; set; }

        public string ExecutiveSummaryText { get; set; }

        public string ValueDisclaimerText { get; set; }
    }

    public class DashboardCustomerPortfolioDistRow
    {
        public string Key { get; set; }

        public string Label { get; set; }

        public int CustomerCount { get; set; }

        public int SortOrder { get; set; }
    }

    public class DashboardCustomerPortfolioPriorityRow
    {
        public int SortOrder { get; set; }

        public int PortfolioPriorityScore { get; set; }

        public string CustomerKey { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public string WilayahName { get; set; }

        public string Klasifikasi { get; set; }

        public string LifecycleStage { get; set; }

        public string LifecycleLabel { get; set; }

        public string PortfolioTier { get; set; }

        public string TierLabel { get; set; }

        public string PrimaryActionKey { get; set; }

        public string PrimaryActionLabel { get; set; }

        public string ActionOwner { get; set; }

        public string ActionReasonText { get; set; }

        public string TriggeredRuleIds { get; set; }

        public decimal MtdOmzet { get; set; }

        public decimal OpenBalance { get; set; }

        public decimal? OverdueBalance { get; set; }

        public string M29Category { get; set; }

        public string SalesPersonName { get; set; }

        public decimal? SalesmanAchievementPercent { get; set; }

        public bool SalesmanHighPiutangExposure { get; set; }

        public bool IsAttention { get; set; }

        public string M30LinkRoute { get; set; }

        public string CustomerReportRoute { get; set; }

        public string DrillDownRouteM17 { get; set; }

        public string DrillDownRouteM29 { get; set; }
    }

    public class DashboardCustomerPortfolioCustomerRow
    {
        public int SortOrder { get; set; }

        public string CustomerKey { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public string WilayahName { get; set; }

        public string Klasifikasi { get; set; }

        public string LifecycleStage { get; set; }

        public string LifecycleLabel { get; set; }

        public string PortfolioTier { get; set; }

        public string TierLabel { get; set; }

        public string PrimaryActionKey { get; set; }

        public string PrimaryActionLabel { get; set; }

        public string ActionOwner { get; set; }

        public string ActionReasonText { get; set; }

        public string TriggeredRuleIds { get; set; }

        public decimal MtdOmzet { get; set; }

        public decimal OpenBalance { get; set; }

        public decimal? OverdueBalance { get; set; }

        public int FakturCount6Mo { get; set; }

        public bool IsActiveMtd { get; set; }

        public DateTime? LastPurchaseDate { get; set; }

        public DateTime? FirstPurchaseDate { get; set; }

        public string M29Category { get; set; }

        public string M29PrimarySignalKey { get; set; }

        public string SalesPersonName { get; set; }

        public decimal? SalesmanAchievementPercent { get; set; }

        public bool SalesmanHighPiutangExposure { get; set; }

        public bool IsAttention { get; set; }

        public int PortfolioPriorityScore { get; set; }

        public string M30LinkRoute { get; set; }

        public string CustomerReportRoute { get; set; }

        public string DrillDownRouteM17 { get; set; }

        public string DrillDownRouteM29 { get; set; }

        public string ValueDisclaimer { get; set; }
    }

    public class DashboardCustomerPortfolioConcentrationRow
    {
        public string ConcentrationType { get; set; }

        public int SortOrder { get; set; }

        public int Rank { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public decimal Amount { get; set; }

        public decimal? PercentOfTotal { get; set; }
    }

    public class DashboardCustomerPortfolioWilayahRow
    {
        public int SortOrder { get; set; }

        public string WilayahName { get; set; }

        public int CustomerCount { get; set; }

        public int AttentionCustomerCount { get; set; }
    }
}
