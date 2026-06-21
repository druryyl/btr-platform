using System;
using System.Collections.Generic;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Models
{
    public class DashboardInventoryOptimizationAggregateResult
    {
        public DateTime GeneratedAt { get; set; }

        public DateTime BusinessDate { get; set; }

        public int PlanningHorizonDays { get; set; }

        public decimal? BudgetCapIdr { get; set; }

        public int InventoryHealthScore { get; set; }

        public int CriticalActionCount { get; set; }

        public int HighActionCount { get; set; }

        public int MediumActionCount { get; set; }

        public int LowActionCount { get; set; }

        public int PurchaseNowCount { get; set; }

        public int DelayCount { get; set; }

        public int TransferCount { get; set; }

        public int ClearanceCount { get; set; }

        public int PostFirstCount { get; set; }

        public int DeferCount { get; set; }

        public decimal RequiredPurchaseBudgetIdr { get; set; }

        public decimal RecommendedPurchaseBudgetIdr { get; set; }

        public decimal DeferrableSpendIdr { get; set; }

        public decimal RecoverableCapitalIdr { get; set; }

        public decimal PurchaseImpactIdr { get; set; }

        public decimal DelayImpactIdr { get; set; }

        public decimal TransferSavingsIdr { get; set; }

        public string TopActionSummary { get; set; }

        public List<DashboardInventoryOptimizationActionRow> TopActions { get; set; }
            = new List<DashboardInventoryOptimizationActionRow>();

        public List<DashboardInventoryOptimizationReorderRow> ReorderList { get; set; }
            = new List<DashboardInventoryOptimizationReorderRow>();

        public List<DashboardInventoryOptimizationTransferRow> TransferList { get; set; }
            = new List<DashboardInventoryOptimizationTransferRow>();

        public List<DashboardInventoryOptimizationDelayRow> DelayList { get; set; }
            = new List<DashboardInventoryOptimizationDelayRow>();

        public List<DashboardInventoryOptimizationClearanceRow> ClearanceList { get; set; }
            = new List<DashboardInventoryOptimizationClearanceRow>();

        public List<DashboardInventoryOptimizationPriorityDistRow> PriorityDistribution { get; set; }
            = new List<DashboardInventoryOptimizationPriorityDistRow>();

        public List<DashboardInventoryOptimizationActionHeatRow> ActionHeatSummary { get; set; }
            = new List<DashboardInventoryOptimizationActionHeatRow>();
    }

    public class DashboardInventoryOptimizationActionRow
    {
        public int SortOrder { get; set; }

        public int PriorityScore { get; set; }

        public string Category { get; set; }

        public string ActionType { get; set; }

        public string ActionLabel { get; set; }

        public string BrgId { get; set; }

        public string BrgName { get; set; }

        public string SupplierName { get; set; }

        public string WarehouseFromId { get; set; }

        public string WarehouseFromName { get; set; }

        public string WarehouseToId { get; set; }

        public string WarehouseToName { get; set; }

        public decimal? Quantity { get; set; }

        public decimal ImpactValueIdr { get; set; }

        public decimal? DaysOfSupply { get; set; }

        public string ReasonText { get; set; }

        public string RuleId { get; set; }

        public string ReportRoute { get; set; }

        public string DrillDownRoute { get; set; }
    }

    public class DashboardInventoryOptimizationReorderRow
    {
        public int SortOrder { get; set; }

        public int PriorityScore { get; set; }

        public string Category { get; set; }

        public string BrgId { get; set; }

        public string BrgCode { get; set; }

        public string BrgName { get; set; }

        public string SupplierName { get; set; }

        public decimal RecommendedPurchaseQty { get; set; }

        public decimal EstimatedCostIdr { get; set; }

        public decimal? DaysOfSupply { get; set; }

        public DateTime? ReorderDate { get; set; }

        public decimal AverageDailyConsumption { get; set; }

        public decimal CurrentQty { get; set; }

        public string ReasonText { get; set; }

        public string RuleId { get; set; }

        public string ReportRoute { get; set; }

        public string DrillDownRoute { get; set; }
    }

    public class DashboardInventoryOptimizationTransferRow
    {
        public int SortOrder { get; set; }

        public int PriorityScore { get; set; }

        public string Category { get; set; }

        public string BrgId { get; set; }

        public string BrgName { get; set; }

        public string WarehouseFromId { get; set; }

        public string WarehouseFromName { get; set; }

        public string WarehouseToId { get; set; }

        public string WarehouseToName { get; set; }

        public decimal TransferQty { get; set; }

        public decimal? DestDaysOfSupply { get; set; }

        public string ReasonText { get; set; }

        public string RuleId { get; set; }

        public string ReportRoute { get; set; }

        public string DrillDownRoute { get; set; }
    }

    public class DashboardInventoryOptimizationDelayRow
    {
        public int SortOrder { get; set; }

        public int PriorityScore { get; set; }

        public string Category { get; set; }

        public string ActionType { get; set; }

        public string ActionLabel { get; set; }

        public string BrgId { get; set; }

        public string BrgName { get; set; }

        public string SupplierName { get; set; }

        public decimal? DaysOfSupply { get; set; }

        public string MovementClass { get; set; }

        public decimal? SuggestedQty { get; set; }

        public string ReasonText { get; set; }

        public string RuleId { get; set; }

        public string ReportRoute { get; set; }

        public string DrillDownRoute { get; set; }
    }

    public class DashboardInventoryOptimizationClearanceRow
    {
        public int SortOrder { get; set; }

        public int PriorityScore { get; set; }

        public string Category { get; set; }

        public string BrgId { get; set; }

        public string BrgName { get; set; }

        public decimal InventoryValueIdr { get; set; }

        public int? IdleDays { get; set; }

        public string RecommendedAction { get; set; }

        public string ReasonText { get; set; }

        public string RuleId { get; set; }

        public string ReportRoute { get; set; }

        public string DrillDownRoute { get; set; }
    }

    public class DashboardInventoryOptimizationPriorityDistRow
    {
        public string Category { get; set; }

        public int ActionCount { get; set; }

        public int SortOrder { get; set; }
    }

    public class DashboardInventoryOptimizationActionHeatRow
    {
        public string ActionType { get; set; }

        public string ActionLabel { get; set; }

        public string Category { get; set; }

        public int ActionCount { get; set; }
    }

    public class PurchaseRecommendationContext
    {
        public ForecastItemContext ForecastItem { get; set; }

        public string Category { get; set; }

        public int PriorityScore { get; set; }

        public decimal ImpactValueIdr { get; set; }

        public string ActionType { get; set; }

        public string RuleId { get; set; }

        public string ReasonText { get; set; }
    }

    public class WarehouseTransferContext
    {
        public string BrgId { get; set; }

        public string BrgName { get; set; }

        public string WarehouseFromId { get; set; }

        public string WarehouseFromName { get; set; }

        public string WarehouseToId { get; set; }

        public string WarehouseToName { get; set; }

        public decimal SourceQty { get; set; }

        public decimal SourceAdc { get; set; }

        public decimal DestQty { get; set; }

        public decimal DestAdc { get; set; }

        public decimal? DestDaysOfSupply { get; set; }

        public decimal TransferQty { get; set; }

        public string Category { get; set; }

        public int PriorityScore { get; set; }

        public bool AvoidsCriticalPurchase { get; set; }
    }
}
