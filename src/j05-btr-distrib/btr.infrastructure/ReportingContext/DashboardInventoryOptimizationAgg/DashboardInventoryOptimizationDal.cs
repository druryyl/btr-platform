using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using btr.application.ReportingContext.DashboardInventoryOptimizationAgg.Contracts;
using btr.application.ReportingContext.DashboardInventoryOptimizationAgg.Queries;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.infrastructure.Helpers;
using Dapper;
using Microsoft.Extensions.Options;

namespace btr.infrastructure.ReportingContext.DashboardInventoryOptimizationAgg
{
    public class DashboardInventoryOptimizationDal : IDashboardInventoryOptimizationDal
    {
        private const string SnapshotKey = "CURRENT";

        private readonly DatabaseOptions _opt;

        public DashboardInventoryOptimizationDal(IOptions<DatabaseOptions> opt)
        {
            _opt = opt.Value;
        }

        public DashboardInventoryOptimizationResponse GetSummary()
        {
            const string kpiSql = @"
SELECT GeneratedAt, BusinessDate, PlanningHorizonDays, BudgetCapIdr, InventoryHealthScore,
       CriticalActionCount, HighActionCount, MediumActionCount, LowActionCount,
       PurchaseNowCount, DelayCount, TransferCount, ClearanceCount, PostFirstCount, DeferCount,
       RequiredPurchaseBudgetIdr, RecommendedPurchaseBudgetIdr, DeferrableSpendIdr, RecoverableCapitalIdr
FROM BTRPD_InventoryOptimizationKpi
WHERE SnapshotKey = @SnapshotKey";

            using (var conn = new SqlConnection(ConnStringHelper.Get(_opt)))
            {
                var kpi = conn.QueryFirstOrDefault<OptimizationKpiRow>(kpiSql, new { SnapshotKey });
                if (kpi is null)
                {
                    return new DashboardInventoryOptimizationResponse
                    {
                        IsAvailable = false,
                        ExecutiveSummary = "Inventory optimization data not yet available."
                    };
                }

                var priorityRows = conn.Query<PriorityRow>(@"
SELECT Category, ActionCount, SortOrder
FROM BTRPD_InventoryOptimizationPriorityDist
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder", new { SnapshotKey }).ToList();

                var heatRows = conn.Query<HeatRow>(@"
SELECT ActionType, ActionLabel, Category, ActionCount
FROM BTRPD_InventoryOptimizationActionHeat
WHERE SnapshotKey = @SnapshotKey", new { SnapshotKey }).ToList();

                var actionRows = conn.Query<ActionRow>(@"
SELECT SortOrder, PriorityScore, Category, ActionType, ActionLabel, BrgId, BrgName, SupplierName,
       WarehouseFromName, WarehouseToName, Quantity, ImpactValueIdr, DaysOfSupply,
       ReasonText, RuleId, ReportRoute, DrillDownRoute
FROM BTRPD_InventoryOptimizationAction
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder", new { SnapshotKey }).ToList();

                var reorderRows = conn.Query<ReorderRow>(@"
SELECT SortOrder, PriorityScore, Category, BrgId, BrgCode, BrgName, SupplierName,
       RecommendedPurchaseQty, EstimatedCostIdr, DaysOfSupply, ReorderDate,
       ReasonText, RuleId, ReportRoute, DrillDownRoute
FROM BTRPD_InventoryOptimizationReorder
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder", new { SnapshotKey }).ToList();

                var transferRows = conn.Query<TransferRow>(@"
SELECT SortOrder, PriorityScore, Category, BrgId, BrgName,
       WarehouseFromName, WarehouseToName, TransferQty, DestDaysOfSupply,
       ReasonText, RuleId, ReportRoute, DrillDownRoute
FROM BTRPD_InventoryOptimizationTransfer
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder", new { SnapshotKey }).ToList();

                var delayRows = conn.Query<DelayRow>(@"
SELECT SortOrder, PriorityScore, Category, ActionType, ActionLabel, BrgId, BrgName, SupplierName,
       DaysOfSupply, MovementClass, SuggestedQty, ReasonText, RuleId, ReportRoute, DrillDownRoute
FROM BTRPD_InventoryOptimizationDelay
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder", new { SnapshotKey }).ToList();

                var clearanceRows = conn.Query<ClearanceRow>(@"
SELECT SortOrder, PriorityScore, Category, BrgId, BrgName, InventoryValueIdr, IdleDays,
       RecommendedAction, ReasonText, RuleId, ReportRoute, DrillDownRoute
FROM BTRPD_InventoryOptimizationClearance
WHERE SnapshotKey = @SnapshotKey
ORDER BY SortOrder", new { SnapshotKey }).ToList();

                var aggregate = new DashboardInventoryOptimizationAggregateResult
                {
                    GeneratedAt = kpi.GeneratedAt,
                    BusinessDate = kpi.BusinessDate,
                    PlanningHorizonDays = kpi.PlanningHorizonDays,
                    BudgetCapIdr = kpi.BudgetCapIdr,
                    InventoryHealthScore = kpi.InventoryHealthScore,
                    CriticalActionCount = kpi.CriticalActionCount,
                    HighActionCount = kpi.HighActionCount,
                    MediumActionCount = kpi.MediumActionCount,
                    LowActionCount = kpi.LowActionCount,
                    PurchaseNowCount = kpi.PurchaseNowCount,
                    DelayCount = kpi.DelayCount,
                    TransferCount = kpi.TransferCount,
                    ClearanceCount = kpi.ClearanceCount,
                    PostFirstCount = kpi.PostFirstCount,
                    DeferCount = kpi.DeferCount,
                    RequiredPurchaseBudgetIdr = kpi.RequiredPurchaseBudgetIdr,
                    RecommendedPurchaseBudgetIdr = kpi.RecommendedPurchaseBudgetIdr,
                    DeferrableSpendIdr = kpi.DeferrableSpendIdr,
                    RecoverableCapitalIdr = kpi.RecoverableCapitalIdr,
                    PurchaseImpactIdr = reorderRows.Sum(r => r.EstimatedCostIdr),
                    DelayImpactIdr = delayRows.Sum(r => r.SuggestedQty ?? 0m),
                    TransferSavingsIdr = transferRows.Sum(r => r.TransferQty),
                    TopActionSummary = actionRows.FirstOrDefault()?.ActionLabel
                };

                var response = new DashboardInventoryOptimizationResponse
                {
                    IsAvailable = true,
                    GeneratedAt = kpi.GeneratedAt,
                    BusinessDate = kpi.BusinessDate,
                    PlanningHorizonDays = kpi.PlanningHorizonDays,
                    BudgetCapIdr = kpi.BudgetCapIdr,
                    InventoryHealthScore = kpi.InventoryHealthScore,
                    CriticalActionCount = kpi.CriticalActionCount,
                    HighActionCount = kpi.HighActionCount,
                    MediumActionCount = kpi.MediumActionCount,
                    LowActionCount = kpi.LowActionCount,
                    PurchaseNowCount = kpi.PurchaseNowCount,
                    DelayCount = kpi.DelayCount,
                    TransferCount = kpi.TransferCount,
                    ClearanceCount = kpi.ClearanceCount,
                    PostFirstCount = kpi.PostFirstCount,
                    DeferCount = kpi.DeferCount,
                    RequiredPurchaseBudgetIdr = kpi.RequiredPurchaseBudgetIdr,
                    RecommendedPurchaseBudgetIdr = kpi.RecommendedPurchaseBudgetIdr,
                    DeferrableSpendIdr = kpi.DeferrableSpendIdr,
                    RecoverableCapitalIdr = kpi.RecoverableCapitalIdr,
                    PurchaseImpactIdr = aggregate.PurchaseImpactIdr,
                    DelayImpactIdr = aggregate.DelayImpactIdr,
                    TransferSavingsIdr = aggregate.TransferSavingsIdr,
                    PriorityDistribution = priorityRows.Select(r => new DashboardInventoryOptimizationPriorityDistItem
                    {
                        Category = r.Category,
                        ActionCount = r.ActionCount,
                        SortOrder = r.SortOrder
                    }).ToList(),
                    ActionHeatSummary = heatRows.Select(r => new DashboardInventoryOptimizationActionHeatItem
                    {
                        ActionType = r.ActionType,
                        ActionLabel = r.ActionLabel,
                        Category = r.Category,
                        ActionCount = r.ActionCount
                    }).ToList(),
                    TopActions = actionRows.Select(MapAction).ToList(),
                    ReorderList = reorderRows.Select(MapReorder).ToList(),
                    TransferList = transferRows.Select(MapTransfer).ToList(),
                    DelayList = delayRows.Select(MapDelay).ToList(),
                    ClearanceList = clearanceRows.Select(MapClearance).ToList()
                };

                response.ExecutiveSummary = InventoryOptimizationExecutiveSummaryBuilder.Build(
                    kpi.BusinessDate,
                    aggregate);

                return response;
            }
        }

        private static DashboardInventoryOptimizationActionItem MapAction(ActionRow r) =>
            new DashboardInventoryOptimizationActionItem
            {
                SortOrder = r.SortOrder,
                PriorityScore = r.PriorityScore,
                Category = r.Category,
                ActionType = r.ActionType,
                ActionLabel = r.ActionLabel,
                BrgId = r.BrgId,
                BrgName = r.BrgName,
                SupplierName = r.SupplierName,
                WarehouseFromName = r.WarehouseFromName,
                WarehouseToName = r.WarehouseToName,
                Quantity = r.Quantity,
                ImpactValueIdr = r.ImpactValueIdr,
                DaysOfSupply = r.DaysOfSupply,
                ReasonText = r.ReasonText,
                RuleId = r.RuleId,
                ReportRoute = r.ReportRoute,
                DrillDownRoute = r.DrillDownRoute
            };

        private static DashboardInventoryOptimizationReorderItem MapReorder(ReorderRow r) =>
            new DashboardInventoryOptimizationReorderItem
            {
                SortOrder = r.SortOrder,
                PriorityScore = r.PriorityScore,
                Category = r.Category,
                BrgId = r.BrgId,
                BrgCode = r.BrgCode,
                BrgName = r.BrgName,
                SupplierName = r.SupplierName,
                RecommendedPurchaseQty = r.RecommendedPurchaseQty,
                EstimatedCostIdr = r.EstimatedCostIdr,
                DaysOfSupply = r.DaysOfSupply,
                ReorderDate = r.ReorderDate,
                ReasonText = r.ReasonText,
                RuleId = r.RuleId,
                ReportRoute = r.ReportRoute,
                DrillDownRoute = r.DrillDownRoute
            };

        private static DashboardInventoryOptimizationTransferItem MapTransfer(TransferRow r) =>
            new DashboardInventoryOptimizationTransferItem
            {
                SortOrder = r.SortOrder,
                PriorityScore = r.PriorityScore,
                Category = r.Category,
                BrgId = r.BrgId,
                BrgName = r.BrgName,
                WarehouseFromName = r.WarehouseFromName,
                WarehouseToName = r.WarehouseToName,
                TransferQty = r.TransferQty,
                DestDaysOfSupply = r.DestDaysOfSupply,
                ReasonText = r.ReasonText,
                RuleId = r.RuleId,
                ReportRoute = r.ReportRoute,
                DrillDownRoute = r.DrillDownRoute
            };

        private static DashboardInventoryOptimizationDelayItem MapDelay(DelayRow r) =>
            new DashboardInventoryOptimizationDelayItem
            {
                SortOrder = r.SortOrder,
                PriorityScore = r.PriorityScore,
                Category = r.Category,
                ActionType = r.ActionType,
                ActionLabel = r.ActionLabel,
                BrgId = r.BrgId,
                BrgName = r.BrgName,
                SupplierName = r.SupplierName,
                DaysOfSupply = r.DaysOfSupply,
                MovementClass = r.MovementClass,
                SuggestedQty = r.SuggestedQty,
                ReasonText = r.ReasonText,
                RuleId = r.RuleId,
                ReportRoute = r.ReportRoute,
                DrillDownRoute = r.DrillDownRoute
            };

        private static DashboardInventoryOptimizationClearanceItem MapClearance(ClearanceRow r) =>
            new DashboardInventoryOptimizationClearanceItem
            {
                SortOrder = r.SortOrder,
                PriorityScore = r.PriorityScore,
                Category = r.Category,
                BrgId = r.BrgId,
                BrgName = r.BrgName,
                InventoryValueIdr = r.InventoryValueIdr,
                IdleDays = r.IdleDays,
                RecommendedAction = r.RecommendedAction,
                ReasonText = r.ReasonText,
                RuleId = r.RuleId,
                ReportRoute = r.ReportRoute,
                DrillDownRoute = r.DrillDownRoute
            };

        private sealed class OptimizationKpiRow
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
        }

        private sealed class PriorityRow
        {
            public string Category { get; set; }
            public int ActionCount { get; set; }
            public int SortOrder { get; set; }
        }

        private sealed class HeatRow
        {
            public string ActionType { get; set; }
            public string ActionLabel { get; set; }
            public string Category { get; set; }
            public int ActionCount { get; set; }
        }

        private sealed class ActionRow
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

        private sealed class ReorderRow
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

        private sealed class TransferRow
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

        private sealed class DelayRow
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

        private sealed class ClearanceRow
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
    }
}
