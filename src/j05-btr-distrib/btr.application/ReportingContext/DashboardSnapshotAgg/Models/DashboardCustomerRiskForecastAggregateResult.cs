using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Models
{
    public sealed class CustomerRiskSignalContext
    {
        public string SignalKey { get; set; }

        public string Severity { get; set; }

        public string RuleId { get; set; }

        public string Explanation { get; set; }
    }

    public sealed class CustomerRiskForecastContext
    {
        public string CustomerKey { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public string WilayahName { get; set; }

        public string SalesPersonName { get; set; }

        public string SalesPersonId { get; set; }

        public decimal OpenBalance { get; set; }

        public decimal OverdueBalance { get; set; }

        public decimal DueWithinHorizon { get; set; }

        public int MinDaysUntilDue { get; set; }

        public decimal MtdOmzet { get; set; }

        public decimal PriorMonthOmzet { get; set; }

        public decimal? DeclineRatio { get; set; }

        public int? DaysSinceLastFaktur { get; set; }

        public decimal? AvgPaymentLagDays { get; set; }

        public int? DaysSinceLastPayment { get; set; }

        public decimal Plafond { get; set; }

        public decimal ProjectedOpenBalance { get; set; }

        public bool IsActiveThisMonth { get; set; }

        public bool IsCurrentlyPlafondBreach { get; set; }

        public bool HasChronicOverdue { get; set; }

        public bool IsSuspended { get; set; }

        public string Category { get; set; }

        public int RiskPriorityScore { get; set; }

        public List<CustomerRiskSignalContext> Signals { get; set; } = new List<CustomerRiskSignalContext>();
    }

    public class DashboardCustomerRiskForecastAggregateResult
    {
        public DateTime GeneratedAt { get; set; }

        public DateTime BusinessDate { get; set; }

        public int PeriodYear { get; set; }

        public int PeriodMonth { get; set; }

        public int DaysInMonth { get; set; }

        public int DaysElapsed { get; set; }

        public DashboardCustomerRiskForecastKpiSnapshot Kpi { get; set; }
            = new DashboardCustomerRiskForecastKpiSnapshot();

        public List<DashboardCustomerRiskForecastDistRow> CategoryDistribution { get; set; }
            = new List<DashboardCustomerRiskForecastDistRow>();

        public List<DashboardCustomerRiskForecastWilayahRow> TopWilayah { get; set; }
            = new List<DashboardCustomerRiskForecastWilayahRow>();

        public List<DashboardCustomerRiskForecastSignalMixRow> SignalMix { get; set; }
            = new List<DashboardCustomerRiskForecastSignalMixRow>();

        public List<DashboardCustomerRiskForecastCustomerRow> TopCustomers { get; set; }
            = new List<DashboardCustomerRiskForecastCustomerRow>();

        public List<DashboardCustomerRiskForecastAttentionRow> AttentionList { get; set; }
            = new List<DashboardCustomerRiskForecastAttentionRow>();

        public List<DashboardCustomerRiskForecastRecommendationRow> Recommendations { get; set; }
            = new List<DashboardCustomerRiskForecastRecommendationRow>();

        public List<CustomerRiskForecastContext> Contexts { get; set; }
            = new List<CustomerRiskForecastContext>();
    }

    public sealed class DashboardCustomerRiskForecastKpiSnapshot
    {
        public int HorizonDays { get; set; }

        public int CustomersForecastedAtRisk { get; set; }

        public int HighRiskCustomerCount { get; set; }

        public int CriticalCustomerCount { get; set; }

        public decimal ElevatedRiskReceivable { get; set; }

        public decimal? ElevatedRiskReceivablePercent { get; set; }

        public decimal PortfolioHealthScore { get; set; }

        public decimal TotalPiutang { get; set; }

        public string ForecastConfidence { get; set; }

        public int PaymentDelaySignalCount { get; set; }

        public int CreditLimitSignalCount { get; set; }

        public int InactivitySignalCount { get; set; }

        public int PurchaseDeclineSignalCount { get; set; }

        public int CollectionRiskSignalCount { get; set; }

        public int HealthyCount { get; set; }

        public int WatchCount { get; set; }

        public int AttentionCount { get; set; }

        public int HighRiskCount { get; set; }

        public int CriticalCount { get; set; }

        public string ExecutiveSummaryText { get; set; }
    }

    public sealed class DashboardCustomerRiskForecastDistRow
    {
        public string Category { get; set; }

        public string CategoryLabel { get; set; }

        public int CustomerCount { get; set; }

        public int SortOrder { get; set; }
    }

    public sealed class DashboardCustomerRiskForecastWilayahRow
    {
        public string WilayahName { get; set; }

        public int ElevatedRiskCustomerCount { get; set; }

        public int SortOrder { get; set; }
    }

    public sealed class DashboardCustomerRiskForecastSignalMixRow
    {
        public string SignalFamilyKey { get; set; }

        public string SignalFamilyLabel { get; set; }

        public int CustomerCount { get; set; }

        public int SortOrder { get; set; }
    }

    public sealed class DashboardCustomerRiskForecastCustomerRow
    {
        public int SortOrder { get; set; }

        public int RiskPriorityScore { get; set; }

        public string Category { get; set; }

        public string CategoryLabel { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public string WilayahName { get; set; }

        public string SalesPersonName { get; set; }

        public decimal OpenBalance { get; set; }

        public decimal OverdueBalance { get; set; }

        public decimal DueWithinHorizon { get; set; }

        public decimal Plafond { get; set; }

        public decimal ProjectedOpenBalance { get; set; }

        public decimal MtdOmzet { get; set; }

        public decimal PriorMonthOmzet { get; set; }

        public decimal? DeclineRatio { get; set; }

        public int? DaysSinceLastFaktur { get; set; }

        public decimal? AvgPaymentLagDays { get; set; }

        public string PrimarySignalKey { get; set; }

        public string PrimarySignalLabel { get; set; }

        public string ReasonText { get; set; }

        public string RecommendationKey { get; set; }

        public string RecommendationLabel { get; set; }

        public string ReportRoute { get; set; }

        public string DrillDownRoute { get; set; }
    }

    public sealed class DashboardCustomerRiskForecastAttentionRow
    {
        public int SortOrder { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public string SignalKey { get; set; }

        public string SignalLabel { get; set; }

        public string Severity { get; set; }

        public decimal? Amount { get; set; }

        public string HorizonText { get; set; }

        public string RuleId { get; set; }

        public string Explanation { get; set; }

        public string ReportRoute { get; set; }
    }

    public sealed class DashboardCustomerRiskForecastRecommendationRow
    {
        public int SortOrder { get; set; }

        public string RecommendationKey { get; set; }

        public string RecommendationLabel { get; set; }

        public string CustomerCode { get; set; }

        public string CustomerName { get; set; }

        public string Category { get; set; }

        public string ReasonText { get; set; }

        public string RuleId { get; set; }

        public string ReportRoute { get; set; }

        public string DrillDownRoute { get; set; }
    }

    public sealed class CustomerRiskSignalRow
    {
        public string CustomerKey { get; set; }

        public string SignalKey { get; set; }

        public string SignalLabel { get; set; }

        public string Severity { get; set; }

        public string RuleId { get; set; }

        public string Explanation { get; set; }

        public decimal? Amount { get; set; }
    }
}
