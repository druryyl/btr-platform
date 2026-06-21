using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.InventoryContext.StokBalanceInfo;
using btr.application.InventoryContext.WarehouseAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.SalesContext.FakturInfo;
using btr.domain.InventoryContext.WarehouseAgg;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public class DashboardInventoryOptimizationAggregator
    {
        public DashboardInventoryOptimizationAggregateResult Aggregate(
            IEnumerable<ForecastItemContext> forecastItems,
            IEnumerable<StokBalanceView> balanceRows,
            IEnumerable<BrgWarehouseConsumptionDto> warehouseConsumption,
            IEnumerable<WarehouseModel> warehouses,
            DashboardInventoryRiskAggregateResult riskResult,
            DashboardPurchasingManagementAggregateResult purchasingMgmt,
            DashboardInventoryForecastAggregateResult forecastResult,
            DateTime businessDate,
            DateTime generatedAt,
            DashboardSnapshotOptions options)
        {
            if (forecastResult is null)
                throw new ArgumentNullException(nameof(forecastResult));

            options = options ?? new DashboardSnapshotOptions();
            var items = (forecastItems ?? Enumerable.Empty<ForecastItemContext>()).ToList();
            var asOfDate = businessDate.Date;

            var planningHorizonDays = options.InventoryForecastPlanningHorizonDays;
            var leadTimeDays = options.InventoryForecastDefaultLeadTimeDays;
            var overstockDosDays = options.InventoryForecastOverstockDosDays;
            var budgetCap = options.InventoryOptimizationDefaultBudgetCapIdr;

            var qualifiedBacklogSuppliers = BuildSupplierSet(
                purchasingMgmt,
                DashboardPurchasingManagementAggregator.SignalQualifiedBacklog);

            var mtdPurchaseSuppliers = (purchasingMgmt?.TopPrincipal ?? new List<DashboardPurchasingManagementTopPrincipalRow>())
                .Where(p => p.MtdPurchaseAmount > 0)
                .Select(p => NormalizeSupplier(p.PrincipalName))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var principalNoPurchaseSuppliers = (purchasingMgmt?.TopPrincipal ?? new List<DashboardPurchasingManagementTopPrincipalRow>())
                .Where(p => p.IsInventoryNoPurchase)
                .Select(p => NormalizeSupplier(p.PrincipalName))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var strategicBrgIds = items
                .OrderByDescending(ctx => (ctx.Calculation?.AdcUsed ?? 0m) * InventoryOptimizationPolicy.ComputeUnitHpp(ctx.Item.Qty, ctx.Item.InventoryValue))
                .Take(10)
                .Select(ctx => ctx.Item.BrgId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var deadStockValues = items
                .Where(ctx => string.Equals(ctx.MovementSignalKey, DashboardInventoryRiskAggregator.SignalDeadStock, StringComparison.OrdinalIgnoreCase))
                .Select(ctx => ctx.Item.InventoryValue)
                .OrderByDescending(v => v)
                .ToList();

            var deadStockP75 = deadStockValues.Count > 0
                ? deadStockValues[Math.Min(deadStockValues.Count - 1, (int)Math.Floor(deadStockValues.Count * 0.75))]
                : 0m;

            var suppressedPurchaseBrgIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var allActions = new List<DashboardInventoryOptimizationActionRow>();
            var delayRows = new List<DashboardInventoryOptimizationDelayRow>();
            var clearanceRows = new List<DashboardInventoryOptimizationClearanceRow>();
            var purchaseCandidates = new List<PurchaseRecommendationContext>();

            foreach (var ctx in items)
            {
                if (ctx?.Item is null || ctx.Calculation is null)
                    continue;

                var unitHpp = InventoryOptimizationPolicy.ComputeUnitHpp(ctx.Item.Qty, ctx.Item.InventoryValue);
                var isStrategic = strategicBrgIds.Contains(ctx.Item.BrgId);
                var supplierKey = NormalizeSupplier(ctx.Item.SupplierName);

                if (InventoryOptimizationPolicy.IsDoNotReorder(ctx))
                {
                    var category = InventoryOptimizationPolicy.ResolveCategory(
                        InventoryOptimizationPolicy.ActionDoNotReorder,
                        ctx.Calculation.DaysOfSupply,
                        ctx.Calculation.ReorderDate,
                        asOfDate,
                        ctx.MovementSignalKey,
                        ctx.Item.InventoryValue,
                        deadStockP75);

                    var score = InventoryOptimizationPolicy.ComputePriorityScore(
                        category, ctx.Item.InventoryValue, ctx.Calculation.DaysOfSupply, leadTimeDays, isStrategic,
                        InventoryOptimizationPolicy.ActionDoNotReorder);

                    var reason = InventoryOptimizationPolicy.BuildReasonText(InventoryOptimizationPolicy.ActionDoNotReorder, ctx);
                    suppressedPurchaseBrgIds.Add(ctx.Item.BrgId);

                    allActions.Add(InventoryOptimizationRecommendationBuilder.ToActionRow(
                        InventoryOptimizationPolicy.ActionDoNotReorder, ctx, category, score, ctx.Item.InventoryValue,
                        "IO-13", reason));

                    if (string.Equals(ctx.MovementSignalKey, DashboardInventoryRiskAggregator.SignalDeadStock, StringComparison.OrdinalIgnoreCase))
                    {
                        clearanceRows.Add(InventoryOptimizationRecommendationBuilder.ToClearanceRow(
                            ctx, category, score, "IO-20", reason));
                    }

                    continue;
                }

                var isOverstock = InventoryOptimizationPolicy.IsOverstock(ctx.Calculation.DaysOfSupply, overstockDosDays);
                var isSlowOverstock = string.Equals(ctx.MovementSignalKey, DashboardInventoryRiskAggregator.SignalSlowMoving, StringComparison.OrdinalIgnoreCase) &&
                                      ctx.Calculation.DaysOfSupply.HasValue &&
                                      ctx.Calculation.DaysOfSupply.Value > 60;

                if (isOverstock || isSlowOverstock)
                {
                    var delayCategory = InventoryOptimizationPolicy.ResolveCategory(
                        InventoryOptimizationPolicy.ActionDelay,
                        ctx.Calculation.DaysOfSupply,
                        ctx.Calculation.ReorderDate,
                        asOfDate,
                        ctx.MovementSignalKey,
                        ctx.Item.InventoryValue,
                        deadStockP75);

                    if (principalNoPurchaseSuppliers.Contains(supplierKey))
                        delayCategory = InventoryOptimizationPolicy.CategoryHigh;

                    var delayScore = InventoryOptimizationPolicy.ComputePriorityScore(
                        delayCategory, ctx.Item.InventoryValue, ctx.Calculation.DaysOfSupply, leadTimeDays, isStrategic,
                        InventoryOptimizationPolicy.ActionDelay);

                    var delayReason = InventoryOptimizationPolicy.BuildReasonText(InventoryOptimizationPolicy.ActionDelay, ctx);
                    suppressedPurchaseBrgIds.Add(ctx.Item.BrgId);

                    delayRows.Add(InventoryOptimizationRecommendationBuilder.ToDelayRow(
                        InventoryOptimizationPolicy.ActionDelay, ctx, delayCategory, delayScore, null, "IO-10", delayReason));

                    allActions.Add(InventoryOptimizationRecommendationBuilder.ToActionRow(
                        InventoryOptimizationPolicy.ActionDelay, ctx, delayCategory, delayScore, ctx.Item.InventoryValue,
                        isOverstock ? "IO-10" : "IO-11", delayReason));

                    if (mtdPurchaseSuppliers.Contains(supplierKey))
                    {
                        var reducedQty = Math.Max(0m, Math.Ceiling(ctx.Calculation.RecommendedPurchaseQty * options.InventoryOptimizationReduceQtyFactor));
                        var reduceCategory = InventoryOptimizationPolicy.CategoryMedium;
                        var reduceScore = InventoryOptimizationPolicy.ComputePriorityScore(
                            reduceCategory, ctx.Item.InventoryValue, ctx.Calculation.DaysOfSupply, leadTimeDays, isStrategic,
                            InventoryOptimizationPolicy.ActionReduceQty);
                        var reduceReason = InventoryOptimizationPolicy.BuildReasonText(InventoryOptimizationPolicy.ActionReduceQty, ctx);

                        delayRows.Add(InventoryOptimizationRecommendationBuilder.ToDelayRow(
                            InventoryOptimizationPolicy.ActionReduceQty, ctx, reduceCategory, reduceScore, reducedQty, "IO-12", reduceReason));

                        allActions.Add(InventoryOptimizationRecommendationBuilder.ToActionRow(
                            InventoryOptimizationPolicy.ActionReduceQty, ctx, reduceCategory, reduceScore, ctx.Item.InventoryValue,
                            "IO-12", reduceReason, reducedQty));
                    }

                    if (isSlowOverstock && ctx.Calculation.AdcUsed > 0)
                    {
                        var promoteCategory = InventoryOptimizationPolicy.ResolveCategory(
                            InventoryOptimizationPolicy.ActionPromote,
                            ctx.Calculation.DaysOfSupply,
                            ctx.Calculation.ReorderDate,
                            asOfDate,
                            ctx.MovementSignalKey,
                            ctx.Item.InventoryValue,
                            deadStockP75);

                        var promoteScore = InventoryOptimizationPolicy.ComputePriorityScore(
                            promoteCategory, ctx.Item.InventoryValue, ctx.Calculation.DaysOfSupply, leadTimeDays, isStrategic,
                            InventoryOptimizationPolicy.ActionPromote);

                        var promoteReason = InventoryOptimizationPolicy.BuildReasonText(InventoryOptimizationPolicy.ActionPromote, ctx);
                        allActions.Add(InventoryOptimizationRecommendationBuilder.ToActionRow(
                            InventoryOptimizationPolicy.ActionPromote, ctx, promoteCategory, promoteScore, ctx.Item.InventoryValue,
                            "IO-11", promoteReason));
                    }
                }

                if (qualifiedBacklogSuppliers.Contains(supplierKey) &&
                    InventoryOptimizationPolicy.IsPurchaseCandidate(ctx, asOfDate, planningHorizonDays))
                {
                    var postCategory = InventoryOptimizationPolicy.ResolveCategory(
                        InventoryOptimizationPolicy.ActionPostFirst,
                        ctx.Calculation.DaysOfSupply,
                        ctx.Calculation.ReorderDate,
                        asOfDate,
                        ctx.MovementSignalKey,
                        ctx.Item.InventoryValue,
                        deadStockP75);

                    var postScore = InventoryOptimizationPolicy.ComputePriorityScore(
                        postCategory, ctx.Item.InventoryValue, ctx.Calculation.DaysOfSupply, leadTimeDays, isStrategic,
                        InventoryOptimizationPolicy.ActionPostFirst);

                    var postReason = InventoryOptimizationPolicy.BuildReasonText(
                        InventoryOptimizationPolicy.ActionPostFirst, ctx, supplierBacklog: true);

                    allActions.Add(InventoryOptimizationRecommendationBuilder.ToActionRow(
                        InventoryOptimizationPolicy.ActionPostFirst, ctx, postCategory, postScore, ctx.Item.InventoryValue,
                        "IO-22", postReason));
                }

                if (!suppressedPurchaseBrgIds.Contains(ctx.Item.BrgId) &&
                    InventoryOptimizationPolicy.IsPurchaseCandidate(ctx, asOfDate, planningHorizonDays))
                {
                    var purchaseCategory = InventoryOptimizationPolicy.ResolveCategory(
                        InventoryOptimizationPolicy.ActionPurchase,
                        ctx.Calculation.DaysOfSupply,
                        ctx.Calculation.ReorderDate,
                        asOfDate,
                        ctx.MovementSignalKey,
                        InventoryOptimizationPolicy.ComputePurchaseCost(ctx.Calculation.RecommendedPurchaseQty, unitHpp),
                        deadStockP75);

                    var purchaseCost = InventoryOptimizationPolicy.ComputePurchaseCost(ctx.Calculation.RecommendedPurchaseQty, unitHpp);
                    var purchaseScore = InventoryOptimizationPolicy.ComputePriorityScore(
                        purchaseCategory, purchaseCost, ctx.Calculation.DaysOfSupply, leadTimeDays, isStrategic,
                        InventoryOptimizationPolicy.ActionPurchase);

                    var purchaseReason = InventoryOptimizationPolicy.BuildReasonText(InventoryOptimizationPolicy.ActionPurchase, ctx);

                    purchaseCandidates.Add(new PurchaseRecommendationContext
                    {
                        ForecastItem = ctx,
                        Category = purchaseCategory,
                        PriorityScore = purchaseScore,
                        ImpactValueIdr = purchaseCost,
                        ActionType = InventoryOptimizationPolicy.ActionPurchase,
                        RuleId = "IO-PURCHASE",
                        ReasonText = purchaseReason
                    });
                }
            }

            var criticalPurchaseBrgIds = purchaseCandidates
                .Where(p => string.Equals(p.Category, InventoryOptimizationPolicy.CategoryCritical, StringComparison.OrdinalIgnoreCase))
                .Select(p => p.ForecastItem.Item.BrgId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var transferPairs = WarehouseBalanceRecommendationBuilder.BuildTransferPairs(
                balanceRows,
                warehouseConsumption,
                warehouses,
                criticalPurchaseBrgIds,
                options.InventoryOptimizationWarehouseShortageDosDays,
                options.InventoryOptimizationWarehouseExcessDosDays,
                leadTimeDays,
                options.InventoryOptimizationMaxTransferRows);

            var transferBrgIds = transferPairs
                .Where(t => t.AvoidsCriticalPurchase)
                .Select(t => t.BrgId)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            purchaseCandidates = purchaseCandidates
                .Where(p => !transferBrgIds.Contains(p.ForecastItem.Item.BrgId))
                .ToList();

            var sortedPurchases = purchaseCandidates
                .OrderByDescending(p => p.PriorityScore)
                .ThenByDescending(p => p.ImpactValueIdr)
                .ToList();

            var cappedPurchases = InventoryOptimizationPolicy.ApplyBudgetCap(sortedPurchases, budgetCap).ToList();

            foreach (var purchase in cappedPurchases)
            {
                allActions.Add(InventoryOptimizationRecommendationBuilder.ToActionRow(
                    purchase.ActionType,
                    purchase.ForecastItem,
                    purchase.Category,
                    purchase.PriorityScore,
                    purchase.ImpactValueIdr,
                    purchase.RuleId,
                    purchase.ReasonText,
                    purchase.ForecastItem.Calculation.RecommendedPurchaseQty));
            }

            var transferRows = transferPairs.Select(t =>
            {
                var reason = $"Transfer from {t.WarehouseFromName} to {t.WarehouseToName} — destination DOS {(t.DestDaysOfSupply.HasValue ? t.DestDaysOfSupply.Value.ToString("F0") : "low")}.";
                allActions.Add(InventoryOptimizationRecommendationBuilder.ToActionRow(
                    InventoryOptimizationPolicy.ActionTransfer,
                    null,
                    t.Category,
                    t.PriorityScore,
                    InventoryOptimizationPolicy.ComputePurchaseCost(t.TransferQty, 0m),
                    WarehouseBalanceRecommendationBuilder.RuleId,
                    reason,
                    t.TransferQty,
                    t));
                return InventoryOptimizationRecommendationBuilder.ToTransferRow(t, reason);
            }).ToList();

            var overstockBySupplier = items
                .Where(ctx => InventoryOptimizationPolicy.IsOverstock(ctx.Calculation?.DaysOfSupply, overstockDosDays))
                .GroupBy(ctx => NormalizeSupplier(ctx.Item.SupplierName), StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() >= 2)
                .ToList();

            foreach (var supplierGroup in overstockBySupplier)
            {
                var sample = supplierGroup.First();
                var itemNames = string.Join(", ", supplierGroup.Take(3).Select(x => x.Item.BrgName));
                var bundleCategory = InventoryOptimizationPolicy.CategoryMedium;
                var bundleScore = InventoryOptimizationPolicy.ComputePriorityScore(
                    bundleCategory, supplierGroup.Sum(x => x.Item.InventoryValue), null, leadTimeDays, false,
                    InventoryOptimizationPolicy.ActionBundle);

                var bundleReason = $"Multiple overstock SKUs from {supplierGroup.Key}: {itemNames}.";
                allActions.Add(InventoryOptimizationRecommendationBuilder.ToActionRow(
                    InventoryOptimizationPolicy.ActionBundle,
                    sample,
                    bundleCategory,
                    bundleScore,
                    supplierGroup.Sum(x => x.Item.InventoryValue),
                    "IO-BUNDLE",
                    bundleReason));
            }

            var topActions = InventoryOptimizationRecommendationBuilder.BuildUnifiedActions(
                allActions,
                options.InventoryOptimizationMaxTopActions);

            var reorderList = cappedPurchases
                .Where(p => string.Equals(p.ActionType, InventoryOptimizationPolicy.ActionPurchase, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(p => p.PriorityScore)
                .ThenByDescending(p => p.ImpactValueIdr)
                .Take(options.InventoryOptimizationMaxReorderRows)
                .Select(p => InventoryOptimizationRecommendationBuilder.ToReorderRow(p, p.ForecastItem))
                .Select((r, index) => { r.SortOrder = index + 1; return r; })
                .ToList();

            delayRows = delayRows
                .OrderByDescending(r => r.PriorityScore)
                .Take(10)
                .Select((r, index) => { r.SortOrder = index + 1; return r; })
                .ToList();

            clearanceRows = clearanceRows
                .OrderByDescending(r => r.PriorityScore)
                .ThenByDescending(r => r.InventoryValueIdr)
                .Take(10)
                .Select((r, index) => { r.SortOrder = index + 1; return r; })
                .ToList();

            transferRows = transferRows
                .Select((r, index) => { r.SortOrder = index + 1; return r; })
                .ToList();

            var requiredBudget = InventoryOptimizationPolicy.ComputeRecommendedBudget(
                cappedPurchases,
                InventoryOptimizationPolicy.CategoryCritical,
                InventoryOptimizationPolicy.CategoryHigh);

            var recommendedBudget = requiredBudget + InventoryOptimizationPolicy.ComputeRecommendedBudget(
                cappedPurchases,
                InventoryOptimizationPolicy.CategoryMedium);

            var deferrableSpend = cappedPurchases
                .Where(p => string.Equals(p.ActionType, InventoryOptimizationPolicy.ActionDefer, StringComparison.OrdinalIgnoreCase))
                .Sum(p => p.ImpactValueIdr) +
                delayRows.Sum(d => d.SuggestedQty.HasValue
                    ? InventoryOptimizationPolicy.ComputePurchaseCost(d.SuggestedQty.Value,
                        InventoryOptimizationPolicy.ComputeUnitHpp(d.SuggestedQty.Value, 0m))
                    : 0m);

            var recoverableCapital = clearanceRows.Sum(c => c.InventoryValueIdr) +
                items.Where(ctx => string.Equals(ctx.MovementSignalKey, DashboardInventoryRiskAggregator.SignalSlowMoving, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(ctx => ctx.Item.InventoryValue)
                    .Take(10)
                    .Sum(ctx => ctx.Item.InventoryValue);

            var categoryCounts = topActions
                .GroupBy(a => a.Category, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Count(), StringComparer.OrdinalIgnoreCase);

            var priorityDistribution = new[]
            {
                InventoryOptimizationPolicy.CategoryCritical,
                InventoryOptimizationPolicy.CategoryHigh,
                InventoryOptimizationPolicy.CategoryMedium,
                InventoryOptimizationPolicy.CategoryLow
            }.Select((cat, index) => new DashboardInventoryOptimizationPriorityDistRow
            {
                Category = cat,
                ActionCount = categoryCounts.TryGetValue(cat, out var count) ? count : 0,
                SortOrder = index + 1
            }).ToList();

            var heatActionTypes = new[]
            {
                InventoryOptimizationPolicy.ActionPurchase,
                InventoryOptimizationPolicy.ActionDelay,
                InventoryOptimizationPolicy.ActionTransfer,
                InventoryOptimizationPolicy.ActionPostFirst,
                InventoryOptimizationPolicy.ActionClearance
            };

            var actionHeat = new List<DashboardInventoryOptimizationActionHeatRow>();
            foreach (var actionType in heatActionTypes)
            {
                foreach (var cat in priorityDistribution.Select(p => p.Category))
                {
                    var count = topActions.Count(a =>
                        string.Equals(a.ActionType, actionType, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(a.Category, cat, StringComparison.OrdinalIgnoreCase));

                    if (count > 0)
                    {
                        actionHeat.Add(new DashboardInventoryOptimizationActionHeatRow
                        {
                            ActionType = actionType,
                            ActionLabel = InventoryOptimizationPolicy.ResolveActionLabel(actionType),
                            Category = cat,
                            ActionCount = count
                        });
                    }
                }
            }

            var topSummary = topActions.FirstOrDefault();
            var topActionSummary = topSummary != null
                ? $"{topSummary.ActionLabel} {topSummary.BrgName} ({topSummary.Category})"
                : "No critical actions at this time.";

            return new DashboardInventoryOptimizationAggregateResult
            {
                GeneratedAt = generatedAt,
                BusinessDate = asOfDate,
                PlanningHorizonDays = planningHorizonDays,
                BudgetCapIdr = budgetCap,
                InventoryHealthScore = forecastResult.InventoryHealthScore,
                CriticalActionCount = categoryCounts.TryGetValue(InventoryOptimizationPolicy.CategoryCritical, out var crit) ? crit : 0,
                HighActionCount = categoryCounts.TryGetValue(InventoryOptimizationPolicy.CategoryHigh, out var high) ? high : 0,
                MediumActionCount = categoryCounts.TryGetValue(InventoryOptimizationPolicy.CategoryMedium, out var med) ? med : 0,
                LowActionCount = categoryCounts.TryGetValue(InventoryOptimizationPolicy.CategoryLow, out var low) ? low : 0,
                PurchaseNowCount = cappedPurchases.Count(p => string.Equals(p.ActionType, InventoryOptimizationPolicy.ActionPurchase, StringComparison.OrdinalIgnoreCase)),
                DelayCount = delayRows.Count,
                TransferCount = transferRows.Count,
                ClearanceCount = clearanceRows.Count,
                PostFirstCount = topActions.Count(a => string.Equals(a.ActionType, InventoryOptimizationPolicy.ActionPostFirst, StringComparison.OrdinalIgnoreCase)),
                DeferCount = cappedPurchases.Count(p => string.Equals(p.ActionType, InventoryOptimizationPolicy.ActionDefer, StringComparison.OrdinalIgnoreCase)),
                RequiredPurchaseBudgetIdr = requiredBudget,
                RecommendedPurchaseBudgetIdr = recommendedBudget,
                DeferrableSpendIdr = deferrableSpend,
                RecoverableCapitalIdr = recoverableCapital,
                PurchaseImpactIdr = reorderList.Sum(r => r.EstimatedCostIdr),
                DelayImpactIdr = delayRows.Sum(d => d.SuggestedQty ?? 0m),
                TransferSavingsIdr = transferRows.Sum(t => t.TransferQty),
                TopActionSummary = topActionSummary,
                TopActions = topActions,
                ReorderList = reorderList,
                TransferList = transferRows,
                DelayList = delayRows,
                ClearanceList = clearanceRows,
                PriorityDistribution = priorityDistribution,
                ActionHeatSummary = actionHeat
            };
        }

        private static HashSet<string> BuildSupplierSet(
            DashboardPurchasingManagementAggregateResult purchasingMgmt,
            string signalKey)
        {
            return (purchasingMgmt?.AttentionList ?? new List<DashboardPurchasingManagementAttentionRow>())
                .Where(a => string.Equals(a.SignalKey, signalKey, StringComparison.OrdinalIgnoreCase))
                .Select(a => NormalizeSupplier(a.EntityName))
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private static string NormalizeSupplier(string name) =>
            string.IsNullOrWhiteSpace(name) ? string.Empty : name.Trim();
    }
}
