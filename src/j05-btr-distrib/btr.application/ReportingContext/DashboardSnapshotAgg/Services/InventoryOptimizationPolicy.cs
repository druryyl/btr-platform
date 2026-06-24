using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public static class InventoryOptimizationPolicy
    {
        public const string ActionPurchase = "PurchaseProduct";
        public const string ActionDefer = "DeferPurchase";
        public const string ActionDelay = "DelayPurchase";
        public const string ActionReduceQty = "ReducePurchaseQuantity";
        public const string ActionTransfer = "TransferInventory";
        public const string ActionPostFirst = "PostPurchaseFirst";
        public const string ActionPromote = "PromoteCampaignReview";
        public const string ActionBundle = "BundleProductsReview";
        public const string ActionClearance = "ClearanceReview";
        public const string ActionDoNotReorder = "DoNotReorder";

        public const string CategoryCritical = "Critical";
        public const string CategoryHigh = "High";
        public const string CategoryMedium = "Medium";
        public const string CategoryLow = "Low";

        private const int CategoryWeightCritical = 1000;
        private const int CategoryWeightHigh = 750;
        private const int CategoryWeightMedium = 500;
        private const int CategoryWeightLow = 250;

        private const string ReportRouteInventory = "/reports/inventory";
        private const string ReportRoutePurchasing = "/reports/purchasing";
        private const string DrillDownForecast = "/dashboard/inventory-forecast";
        private const string DrillDownRisk = "/dashboard/inventory-risk";
        private const string DrillDownPurchasingMgmt = "/dashboard/purchasing-management";

        public static readonly IReadOnlyDictionary<string, string> ActionLabels =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                [ActionPurchase] = "Purchase Product",
                [ActionDefer] = "Defer Purchase",
                [ActionDelay] = "Delay Purchase",
                [ActionReduceQty] = "Reduce Purchase Quantity",
                [ActionTransfer] = "Transfer Inventory",
                [ActionPostFirst] = "Post Purchase First",
                [ActionPromote] = "Promote / Campaign Review",
                [ActionBundle] = "Bundle Products Review",
                [ActionClearance] = "Clearance Review",
                [ActionDoNotReorder] = "Do Not Reorder"
            };

        public static int GetCategoryWeight(string category)
        {
            if (string.Equals(category, CategoryCritical, StringComparison.OrdinalIgnoreCase))
                return CategoryWeightCritical;
            if (string.Equals(category, CategoryHigh, StringComparison.OrdinalIgnoreCase))
                return CategoryWeightHigh;
            if (string.Equals(category, CategoryMedium, StringComparison.OrdinalIgnoreCase))
                return CategoryWeightMedium;
            return CategoryWeightLow;
        }

        public static int ComputePriorityScore(
            string category,
            decimal impactValueIdr,
            decimal? daysOfSupply,
            int defaultLeadTimeDays,
            bool isStrategicItem,
            string actionType,
            bool transferAvoidsCriticalPurchase = false)
        {
            var score = GetCategoryWeight(category);
            score += Math.Min(500, (int)Math.Floor(impactValueIdr / 1_000_000m) * 10);

            if (daysOfSupply.HasValue && daysOfSupply.Value <= defaultLeadTimeDays)
                score += 200;

            if (isStrategicItem)
                score += 100;

            if (string.Equals(actionType, ActionPostFirst, StringComparison.OrdinalIgnoreCase))
                score += 150;
            else if (string.Equals(actionType, ActionTransfer, StringComparison.OrdinalIgnoreCase) && transferAvoidsCriticalPurchase)
                score += 100;
            else if (string.Equals(actionType, ActionClearance, StringComparison.OrdinalIgnoreCase))
                score += 50;

            return score;
        }

        public static string ResolveCategory(
            string actionType,
            decimal? daysOfSupply,
            DateTime? reorderDate,
            DateTime businessDate,
            string movementClass,
            decimal impactValueIdr,
            decimal deadStockP75,
            int planningHorizonDays = 30)
        {
            if (string.Equals(actionType, ActionDefer, StringComparison.OrdinalIgnoreCase))
                return CategoryMedium;

            if (string.Equals(actionType, ActionDoNotReorder, StringComparison.OrdinalIgnoreCase))
                return CategoryMedium;

            if (string.Equals(actionType, ActionBundle, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(actionType, ActionPromote, StringComparison.OrdinalIgnoreCase))
            {
                return impactValueIdr >= deadStockP75 ? CategoryHigh : CategoryMedium;
            }

            if (string.Equals(actionType, ActionClearance, StringComparison.OrdinalIgnoreCase))
            {
                if (impactValueIdr >= deadStockP75)
                    return CategoryCritical;
                return CategoryHigh;
            }

            if (string.Equals(actionType, ActionDelay, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(actionType, ActionReduceQty, StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(movementClass, DashboardInventoryRiskAggregator.SignalSlowMoving, StringComparison.OrdinalIgnoreCase) &&
                    impactValueIdr >= deadStockP75)
                    return CategoryHigh;
                return CategoryMedium;
            }

            if (string.Equals(actionType, ActionPostFirst, StringComparison.OrdinalIgnoreCase))
            {
                if (daysOfSupply.HasValue && daysOfSupply.Value <= 7)
                    return CategoryCritical;
                if (daysOfSupply.HasValue && daysOfSupply.Value <= 14)
                    return CategoryHigh;
                return CategoryMedium;
            }

            if (string.Equals(actionType, ActionTransfer, StringComparison.OrdinalIgnoreCase))
            {
                if (daysOfSupply.HasValue && daysOfSupply.Value <= 7)
                    return CategoryCritical;
                if (daysOfSupply.HasValue && daysOfSupply.Value <= 14)
                    return CategoryHigh;
                if (daysOfSupply.HasValue && daysOfSupply.Value <= planningHorizonDays)
                    return CategoryMedium;
                return CategoryLow;
            }

            if (string.Equals(actionType, ActionPurchase, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(actionType, ActionDefer, StringComparison.OrdinalIgnoreCase))
            {
                var urgency = InventoryForecastPolicy.ResolvePurchaseUrgency(daysOfSupply, reorderDate, businessDate);
                if (string.Equals(urgency, InventoryForecastPolicy.UrgencyCritical, StringComparison.OrdinalIgnoreCase))
                    return CategoryCritical;
                if (string.Equals(urgency, InventoryForecastPolicy.UrgencyHigh, StringComparison.OrdinalIgnoreCase))
                    return CategoryHigh;
                if (string.Equals(urgency, InventoryForecastPolicy.UrgencyMedium, StringComparison.OrdinalIgnoreCase))
                    return CategoryMedium;
                return CategoryLow;
            }

            return CategoryMedium;
        }

        public static decimal ComputeRecommendedBudget(
            IEnumerable<PurchaseRecommendationContext> purchases,
            params string[] categories)
        {
            var categorySet = new HashSet<string>(categories ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            return (purchases ?? Enumerable.Empty<PurchaseRecommendationContext>())
                .Where(p => categorySet.Contains(p.Category) &&
                            (string.Equals(p.ActionType, ActionPurchase, StringComparison.OrdinalIgnoreCase) ||
                             string.Equals(p.ActionType, ActionDefer, StringComparison.OrdinalIgnoreCase)))
                .Sum(p => p.ImpactValueIdr);
        }

        public static IEnumerable<PurchaseRecommendationContext> ApplyBudgetCap(
            IEnumerable<PurchaseRecommendationContext> sortedPurchases,
            decimal? budgetCapIdr)
        {
            if (!budgetCapIdr.HasValue || budgetCapIdr.Value <= 0)
                return sortedPurchases ?? Enumerable.Empty<PurchaseRecommendationContext>();

            var result = new List<PurchaseRecommendationContext>();
            var cumulative = 0m;

            foreach (var purchase in sortedPurchases ?? Enumerable.Empty<PurchaseRecommendationContext>())
            {
                if (!string.Equals(purchase.ActionType, ActionPurchase, StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(purchase);
                    continue;
                }

                if (cumulative + purchase.ImpactValueIdr <= budgetCapIdr.Value)
                {
                    cumulative += purchase.ImpactValueIdr;
                    result.Add(purchase);
                }
                else
                {
                    result.Add(new PurchaseRecommendationContext
                    {
                        ForecastItem = purchase.ForecastItem,
                        Category = CategoryMedium,
                        PriorityScore = purchase.PriorityScore,
                        ImpactValueIdr = purchase.ImpactValueIdr,
                        ActionType = ActionDefer,
                        RuleId = "IO-BUDGET",
                        ReasonText = "Exceeds configured review budget — lower priority than items above."
                    });
                }
            }

            return result;
        }

        public static decimal ComputeTransferQty(
            decimal sourceQty,
            decimal sourceAdc,
            decimal destQty,
            decimal destAdc,
            int leadTimeDays)
        {
            if (destAdc <= 0 || sourceQty <= 0)
                return 0m;

            var shortageGap = Math.Ceiling(destAdc * (leadTimeDays + 3) - destQty);
            if (shortageGap <= 0)
                return 0m;

            var sourceExcess = sourceQty - sourceAdc * 60;
            if (sourceExcess <= 0)
                return 0m;

            return Math.Min(sourceExcess, shortageGap);
        }

        public static string BuildReasonText(
            string actionType,
            ForecastItemContext item,
            WarehouseTransferContext transfer = null,
            bool supplierBacklog = false)
        {
            if (item?.Item is null || item.Calculation is null)
                return string.Empty;

            var calc = item.Calculation;
            var dos = calc.DaysOfSupply;

            if (string.Equals(actionType, ActionPurchase, StringComparison.OrdinalIgnoreCase))
            {
                var stockOutDays = dos.HasValue ? $"{dos.Value:F0}" : "unknown";
                return $"Projected stock-out in {stockOutDays} days; reorder review may be overdue.";
            }

            if (string.Equals(actionType, ActionDefer, StringComparison.OrdinalIgnoreCase))
                return "Exceeds configured review budget — lower priority than items above.";

            if (string.Equals(actionType, ActionDelay, StringComparison.OrdinalIgnoreCase))
                return $"Hold replenishment — days of supply {(dos.HasValue ? dos.Value.ToString("F0", CultureInfo.InvariantCulture) : "high")} exceeds demand warrant.";

            if (string.Equals(actionType, ActionReduceQty, StringComparison.OrdinalIgnoreCase))
                return "MTD purchase exists for supplier — consider reduced replenishment to limit overstock.";

            if (string.Equals(actionType, ActionPostFirst, StringComparison.OrdinalIgnoreCase))
                return supplierBacklog
                    ? "Post pending invoice first — goods may already be in pipeline."
                    : "Complete posting before placing new order.";

            if (string.Equals(actionType, ActionPromote, StringComparison.OrdinalIgnoreCase))
                return "Slow-moving active stock — consider promotion to improve turnover.";

            if (string.Equals(actionType, ActionBundle, StringComparison.OrdinalIgnoreCase))
                return "Multiple overstock SKUs from same supplier — review as bundle.";

            if (string.Equals(actionType, ActionClearance, StringComparison.OrdinalIgnoreCase))
                return $"Dead stock ({item.DaysSinceLastFaktur ?? 0} days idle) — clearance or return review.";

            if (string.Equals(actionType, ActionDoNotReorder, StringComparison.OrdinalIgnoreCase))
                return "Do not buy — dead stock, never sold, or inactive item.";

            if (string.Equals(actionType, ActionTransfer, StringComparison.OrdinalIgnoreCase) && transfer != null)
                return $"Transfer from {transfer.WarehouseFromName} to {transfer.WarehouseToName} — destination low cover.";

            return string.Empty;
        }

        public static string ResolveActionLabel(string actionType)
        {
            return ActionLabels.TryGetValue(actionType ?? string.Empty, out var label)
                ? label
                : actionType ?? string.Empty;
        }

        public static string DefaultReportRoute(string actionType)
        {
            if (string.Equals(actionType, ActionPostFirst, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(actionType, ActionReduceQty, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(actionType, ActionBundle, StringComparison.OrdinalIgnoreCase))
                return ReportRoutePurchasing;

            return ReportRouteInventory;
        }

        public static string DefaultDrillDownRoute(string actionType)
        {
            if (string.Equals(actionType, ActionPostFirst, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(actionType, ActionBundle, StringComparison.OrdinalIgnoreCase))
                return DrillDownPurchasingMgmt;

            if (string.Equals(actionType, ActionDelay, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(actionType, ActionClearance, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(actionType, ActionDoNotReorder, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(actionType, ActionPromote, StringComparison.OrdinalIgnoreCase))
                return DrillDownRisk;

            if (string.Equals(actionType, ActionDefer, StringComparison.OrdinalIgnoreCase))
                return DrillDownForecast;

            return DrillDownForecast;
        }

        public static decimal ComputeUnitHpp(decimal qty, decimal inventoryValue) =>
            qty > 0 ? inventoryValue / qty : 0m;

        public static decimal ComputePurchaseCost(decimal qty, decimal unitHpp) =>
            Math.Round(qty * unitHpp, 2, MidpointRounding.AwayFromZero);

        public static bool IsDoNotReorder(ForecastItemContext ctx)
        {
            if (ctx?.Item is null)
                return true;

            var signal = ctx.MovementSignalKey ?? string.Empty;
            return string.Equals(signal, DashboardInventoryRiskAggregator.SignalDeadStock, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(signal, DashboardInventoryRiskAggregator.SignalNeverSold, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsOverstock(decimal? daysOfSupply, int overstockDosDays) =>
            daysOfSupply.HasValue && daysOfSupply.Value > overstockDosDays;

        public static bool IsPurchaseCandidate(
            ForecastItemContext ctx,
            DateTime businessDate,
            int planningHorizonDays)
        {
            if (ctx?.Item is null || ctx.Calculation is null || !ctx.IsForecastEligible)
                return false;

            if (IsDoNotReorder(ctx))
                return false;

            var calc = ctx.Calculation;
            if (calc.AdcUsed <= 0 || calc.RecommendedPurchaseQty <= 0)
                return false;

            if (IsOverstock(calc.DaysOfSupply, 90))
                return false;

            var dos = calc.DaysOfSupply;
            var reorderSoon = calc.ReorderDate.HasValue && calc.ReorderDate.Value.Date <= businessDate.Date.AddDays(7);
            var withinHorizon = dos.HasValue && dos.Value <= planningHorizonDays;

            return withinHorizon || reorderSoon;
        }
    }
}
