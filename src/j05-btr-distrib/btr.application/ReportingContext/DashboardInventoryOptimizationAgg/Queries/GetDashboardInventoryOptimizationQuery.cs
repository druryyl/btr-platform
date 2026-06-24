using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.DashboardInventoryOptimizationAgg.Contracts;
using MediatR;

namespace btr.application.ReportingContext.DashboardInventoryOptimizationAgg.Queries
{
    public class GetDashboardInventoryOptimizationQuery : IRequest<DashboardInventoryOptimizationResponse>
    {
    }

    public class DashboardInventoryOptimizationResponse
    {
        public bool IsAvailable { get; set; }

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

        public string ExecutiveSummary { get; set; }

        public DashboardInventoryOptimizationTraceability Traceability { get; set; }
            = new DashboardInventoryOptimizationTraceability();

        public List<DashboardInventoryOptimizationPriorityDistItem> PriorityDistribution { get; set; }
            = new List<DashboardInventoryOptimizationPriorityDistItem>();

        public List<DashboardInventoryOptimizationActionHeatItem> ActionHeatSummary { get; set; }
            = new List<DashboardInventoryOptimizationActionHeatItem>();

        public List<DashboardInventoryOptimizationActionItem> TopActions { get; set; }
            = new List<DashboardInventoryOptimizationActionItem>();

        public List<DashboardInventoryOptimizationReorderItem> ReorderList { get; set; }
            = new List<DashboardInventoryOptimizationReorderItem>();

        public List<DashboardInventoryOptimizationTransferItem> TransferList { get; set; }
            = new List<DashboardInventoryOptimizationTransferItem>();

        public List<DashboardInventoryOptimizationDelayItem> DelayList { get; set; }
            = new List<DashboardInventoryOptimizationDelayItem>();

        public List<DashboardInventoryOptimizationClearanceItem> ClearanceList { get; set; }
            = new List<DashboardInventoryOptimizationClearanceItem>();
    }

    public class DashboardInventoryOptimizationTraceability
    {
        public string InventoryForecastRoute { get; set; } = "/dashboard/inventory-forecast";

        public string InventoryRiskRoute { get; set; } = "/dashboard/inventory-risk";

        public string PurchasingManagementRoute { get; set; } = "/dashboard/purchasing-management";

        public string InventoryReportRoute { get; set; } = "/reports/inventory";

        public string PurchasingReportRoute { get; set; } = "/reports/purchasing";

        public string Disclaimer { get; set; } =
            "Recommendations are indicative decision support. BTR Portal does not create purchases or warehouse transfers. Confirm stock, pending postings, and supplier terms in BTR Desktop before acting.";
    }

    public class DashboardInventoryOptimizationPriorityDistItem
    {
        public string Category { get; set; }

        public int ActionCount { get; set; }

        public int SortOrder { get; set; }
    }

    public class DashboardInventoryOptimizationActionHeatItem
    {
        public string ActionType { get; set; }

        public string ActionLabel { get; set; }

        public string Category { get; set; }

        public int ActionCount { get; set; }
    }

    public class DashboardInventoryOptimizationActionItem
    {
        public int SortOrder { get; set; }

        public int PriorityScore { get; set; }

        public string Category { get; set; }

        public string ActionType { get; set; }

        public string ActionLabel { get; set; }

        public string BrgId { get; set; }

        public string BrgName { get; set; }

        public string SupplierName { get; set; }

        public string WarehouseFromName { get; set; }

        public string WarehouseToName { get; set; }

        public decimal? Quantity { get; set; }

        public decimal ImpactValueIdr { get; set; }

        public decimal? DaysOfSupply { get; set; }

        public string ReasonText { get; set; }

        public string RuleId { get; set; }

        public string ReportRoute { get; set; }

        public string DrillDownRoute { get; set; }
    }

    public class DashboardInventoryOptimizationReorderItem
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

        public string ReasonText { get; set; }

        public string RuleId { get; set; }

        public string ReportRoute { get; set; }

        public string DrillDownRoute { get; set; }
    }

    public class DashboardInventoryOptimizationTransferItem
    {
        public int SortOrder { get; set; }

        public int PriorityScore { get; set; }

        public string Category { get; set; }

        public string BrgId { get; set; }

        public string BrgName { get; set; }

        public string WarehouseFromName { get; set; }

        public string WarehouseToName { get; set; }

        public decimal TransferQty { get; set; }

        public decimal? DestDaysOfSupply { get; set; }

        public string ReasonText { get; set; }

        public string RuleId { get; set; }

        public string ReportRoute { get; set; }

        public string DrillDownRoute { get; set; }
    }

    public class DashboardInventoryOptimizationDelayItem
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

    public class DashboardInventoryOptimizationClearanceItem
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

    public class GetDashboardInventoryOptimizationHandler
        : IRequestHandler<GetDashboardInventoryOptimizationQuery, DashboardInventoryOptimizationResponse>
    {
        private readonly IDashboardInventoryOptimizationDal _dal;

        public GetDashboardInventoryOptimizationHandler(IDashboardInventoryOptimizationDal dal)
        {
            _dal = dal;
        }

        public Task<DashboardInventoryOptimizationResponse> Handle(
            GetDashboardInventoryOptimizationQuery request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_dal.GetSummary());
        }
    }
}
