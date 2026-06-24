using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public static class InventoryOptimizationRecommendationBuilder
    {
        public static List<DashboardInventoryOptimizationActionRow> BuildUnifiedActions(
            IEnumerable<DashboardInventoryOptimizationActionRow> allActions,
            int maxRows)
        {
            return (allActions ?? Enumerable.Empty<DashboardInventoryOptimizationActionRow>())
                .OrderByDescending(a => a.PriorityScore)
                .ThenByDescending(a => a.ImpactValueIdr)
                .ThenBy(a => a.BrgName, StringComparer.OrdinalIgnoreCase)
                .Take(maxRows)
                .Select((a, index) =>
                {
                    a.SortOrder = index + 1;
                    return a;
                })
                .ToList();
        }

        public static DashboardInventoryOptimizationActionRow ToActionRow(
            string actionType,
            ForecastItemContext ctx,
            string category,
            int priorityScore,
            decimal impactValueIdr,
            string ruleId,
            string reasonText,
            decimal? quantity = null,
            WarehouseTransferContext transfer = null)
        {
            var item = ctx?.Item;
            return new DashboardInventoryOptimizationActionRow
            {
                PriorityScore = priorityScore,
                Category = category,
                ActionType = actionType,
                ActionLabel = InventoryOptimizationPolicy.ResolveActionLabel(actionType),
                BrgId = item?.BrgId ?? transfer?.BrgId ?? string.Empty,
                BrgName = item?.BrgName ?? transfer?.BrgName ?? string.Empty,
                SupplierName = item?.SupplierName ?? string.Empty,
                WarehouseFromId = transfer?.WarehouseFromId,
                WarehouseFromName = transfer?.WarehouseFromName,
                WarehouseToId = transfer?.WarehouseToId,
                WarehouseToName = transfer?.WarehouseToName,
                Quantity = quantity ?? transfer?.TransferQty,
                ImpactValueIdr = impactValueIdr,
                DaysOfSupply = ctx?.Calculation?.DaysOfSupply ?? transfer?.DestDaysOfSupply,
                ReasonText = reasonText,
                RuleId = ruleId,
                ReportRoute = InventoryOptimizationPolicy.DefaultReportRoute(actionType),
                DrillDownRoute = InventoryOptimizationPolicy.DefaultDrillDownRoute(actionType)
            };
        }

        public static DashboardInventoryOptimizationReorderRow ToReorderRow(
            PurchaseRecommendationContext purchase,
            ForecastItemContext ctx)
        {
            var item = ctx.Item;
            var calc = ctx.Calculation;
            return new DashboardInventoryOptimizationReorderRow
            {
                PriorityScore = purchase.PriorityScore,
                Category = purchase.Category,
                BrgId = item.BrgId,
                BrgCode = item.BrgCode,
                BrgName = item.BrgName,
                SupplierName = item.SupplierName,
                RecommendedPurchaseQty = calc.RecommendedPurchaseQty,
                EstimatedCostIdr = purchase.ImpactValueIdr,
                DaysOfSupply = calc.DaysOfSupply,
                ReorderDate = calc.ReorderDate,
                AverageDailyConsumption = calc.AdcUsed,
                CurrentQty = item.Qty,
                ReasonText = purchase.ReasonText,
                RuleId = purchase.RuleId,
                ReportRoute = InventoryOptimizationPolicy.DefaultReportRoute(purchase.ActionType),
                DrillDownRoute = InventoryOptimizationPolicy.DefaultDrillDownRoute(purchase.ActionType)
            };
        }

        public static DashboardInventoryOptimizationTransferRow ToTransferRow(
            WarehouseTransferContext transfer,
            string reasonText)
        {
            return new DashboardInventoryOptimizationTransferRow
            {
                PriorityScore = transfer.PriorityScore,
                Category = transfer.Category,
                BrgId = transfer.BrgId,
                BrgName = transfer.BrgName,
                WarehouseFromId = transfer.WarehouseFromId,
                WarehouseFromName = transfer.WarehouseFromName,
                WarehouseToId = transfer.WarehouseToId,
                WarehouseToName = transfer.WarehouseToName,
                TransferQty = transfer.TransferQty,
                DestDaysOfSupply = transfer.DestDaysOfSupply,
                ReasonText = reasonText,
                RuleId = WarehouseBalanceRecommendationBuilder.RuleId,
                ReportRoute = InventoryOptimizationPolicy.DefaultReportRoute(InventoryOptimizationPolicy.ActionTransfer),
                DrillDownRoute = InventoryOptimizationPolicy.DefaultDrillDownRoute(InventoryOptimizationPolicy.ActionTransfer)
            };
        }

        public static DashboardInventoryOptimizationDelayRow ToDelayRow(
            string actionType,
            ForecastItemContext ctx,
            string category,
            int priorityScore,
            decimal? suggestedQty,
            string ruleId,
            string reasonText)
        {
            return new DashboardInventoryOptimizationDelayRow
            {
                PriorityScore = priorityScore,
                Category = category,
                ActionType = actionType,
                ActionLabel = InventoryOptimizationPolicy.ResolveActionLabel(actionType),
                BrgId = ctx.Item.BrgId,
                BrgName = ctx.Item.BrgName,
                SupplierName = ctx.Item.SupplierName,
                DaysOfSupply = ctx.Calculation?.DaysOfSupply,
                MovementClass = ctx.MovementSignalKey,
                SuggestedQty = suggestedQty,
                ReasonText = reasonText,
                RuleId = ruleId,
                ReportRoute = InventoryOptimizationPolicy.DefaultReportRoute(actionType),
                DrillDownRoute = InventoryOptimizationPolicy.DefaultDrillDownRoute(actionType)
            };
        }

        public static DashboardInventoryOptimizationClearanceRow ToClearanceRow(
            ForecastItemContext ctx,
            string category,
            int priorityScore,
            string ruleId,
            string reasonText)
        {
            return new DashboardInventoryOptimizationClearanceRow
            {
                PriorityScore = priorityScore,
                Category = category,
                BrgId = ctx.Item.BrgId,
                BrgName = ctx.Item.BrgName,
                InventoryValueIdr = ctx.Item.InventoryValue,
                IdleDays = ctx.DaysSinceLastFaktur,
                RecommendedAction = InventoryOptimizationPolicy.ResolveActionLabel(InventoryOptimizationPolicy.ActionClearance),
                ReasonText = reasonText,
                RuleId = ruleId,
                ReportRoute = InventoryOptimizationPolicy.DefaultReportRoute(InventoryOptimizationPolicy.ActionClearance),
                DrillDownRoute = InventoryOptimizationPolicy.DefaultDrillDownRoute(InventoryOptimizationPolicy.ActionClearance)
            };
        }
    }
}
