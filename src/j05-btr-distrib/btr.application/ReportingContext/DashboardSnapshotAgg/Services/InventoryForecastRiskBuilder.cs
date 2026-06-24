using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public static class InventoryForecastRiskBuilder
    {
        public const string SignalCriticalStockOut = "CriticalStockOut";
        public const string SignalStockOutRisk = "StockOutRisk";
        public const string SignalFutureDeadStock = "FutureDeadStock";
        public const string SignalOverstockRisk = "OverstockRisk";
        public const string SignalFutureSlowMoving = "FutureSlowMoving";
        public const string SignalInsufficientHistory = "InsufficientHistory";

        private const string ReportRoute = "/reports/inventory";
        private const int MaxRows = 10;

        private sealed class RiskCandidate
        {
            public int RulePriority { get; set; }

            public DashboardInventoryForecastRiskRow Row { get; set; }
        }

        private sealed class RecommendationCandidate
        {
            public int UrgencyPriority { get; set; }

            public DashboardInventoryForecastRecommendationRow Row { get; set; }
        }

        public static List<DashboardInventoryForecastRiskRow> BuildTopRisks(
            IEnumerable<ForecastItemContext> items,
            int planningHorizonDays,
            int overstockDosDays,
            DateTime businessDate)
        {
            var candidates = new List<RiskCandidate>();
            var addedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var ctx in items ?? Enumerable.Empty<ForecastItemContext>())
            {
                if (ctx?.Item is null || ctx.Calculation is null)
                    continue;

                var item = ctx.Item;
                var calc = ctx.Calculation;
                var dos = calc.DaysOfSupply;
                var entityCode = item.BrgCode ?? item.BrgName ?? string.Empty;

                if (calc.AdcUsed > 0 && dos.HasValue && dos.Value <= InventoryForecastPolicy.AdcWindow30Days)
                {
                    if (dos.Value <= 7)
                    {
                        TryAddRisk(candidates, addedKeys, 1, item, SignalCriticalStockOut, "Critical Stock-Out",
                            calc, entityCode,
                            $"Days of supply {dos.Value:F1} — stock may run out within 7 days");
                    }
                    else if (dos.Value <= planningHorizonDays)
                    {
                        TryAddRisk(candidates, addedKeys, 2, item, SignalStockOutRisk, "Stock-Out Risk",
                            calc, entityCode,
                            $"Days of supply {dos.Value:F1} — projected stock-out within planning horizon");
                    }
                }

                if (ctx.MovementSignalKey == DashboardInventoryRiskAggregator.BucketActive &&
                    calc.AdcUsed <= 0 &&
                    ctx.DaysSinceLastFaktur.HasValue &&
                    ctx.DaysSinceLastFaktur.Value >= 60 &&
                    ctx.DaysSinceLastFaktur.Value <= 89)
                {
                    TryAddRisk(candidates, addedKeys, 3, item, SignalFutureDeadStock, "Future Dead Stock",
                        calc, entityCode,
                        $"No sales in 30 days; last sale {ctx.DaysSinceLastFaktur} days ago");
                }

                if (calc.AdcUsed > 0 && dos.HasValue && dos.Value > overstockDosDays)
                {
                    TryAddRisk(candidates, addedKeys, 4, item, SignalOverstockRisk, "Overstock Risk",
                        calc, entityCode,
                        $"Days of supply {dos.Value:F0} exceeds overstock threshold ({overstockDosDays} days)");
                }

                if (ctx.MovementSignalKey == DashboardInventoryRiskAggregator.BucketActive &&
                    calc.AdcUsed > 0 &&
                    dos.HasValue &&
                    dos.Value > 60)
                {
                    TryAddRisk(candidates, addedKeys, 5, item, SignalFutureSlowMoving, "Future Slow Moving",
                        calc, entityCode,
                        $"High days of supply ({dos.Value:F0}) with active movement — excess cover risk");
                }

                if (calc.IsInsufficientHistory && item.InventoryValue >= 1_000_000m)
                {
                    TryAddRisk(candidates, addedKeys, 7, item, SignalInsufficientHistory, "Insufficient History",
                        calc, entityCode,
                        "Limited sales history — forecast confidence reduced");
                }
            }

            return candidates
                .OrderBy(c => c.RulePriority)
                .ThenByDescending(c => c.Row.ValueAmount)
                .ThenBy(c => c.Row.BrgName, StringComparer.OrdinalIgnoreCase)
                .Take(MaxRows)
                .Select((c, index) =>
                {
                    c.Row.SortOrder = index + 1;
                    return c.Row;
                })
                .ToList();
        }

        public static List<DashboardInventoryForecastRecommendationRow> BuildPurchaseRecommendations(
            IEnumerable<ForecastItemContext> items,
            DateTime businessDate)
        {
            var urgencyRank = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                [InventoryForecastPolicy.UrgencyCritical] = 1,
                [InventoryForecastPolicy.UrgencyHigh] = 2,
                [InventoryForecastPolicy.UrgencyMedium] = 3,
                [InventoryForecastPolicy.UrgencyLow] = 4
            };

            var candidates = (items ?? Enumerable.Empty<ForecastItemContext>())
                .Where(ctx => ctx?.IsForecastEligible == true &&
                              ctx.Calculation != null &&
                              ctx.Calculation.AdcUsed > 0 &&
                              ctx.Calculation.RecommendedPurchaseQty > 0)
                .Select(ctx =>
                {
                    var urgency = ctx.Calculation.PurchaseUrgency ?? InventoryForecastPolicy.UrgencyLow;
                    return new RecommendationCandidate
                    {
                        UrgencyPriority = urgencyRank.TryGetValue(urgency, out var rank) ? rank : 5,
                        Row = new DashboardInventoryForecastRecommendationRow
                        {
                            BrgId = ctx.Item.BrgId,
                            BrgCode = ctx.Item.BrgCode,
                            BrgName = ctx.Item.BrgName,
                            SupplierName = ctx.Item.SupplierName,
                            ReorderDate = ctx.Calculation.ReorderDate,
                            RecommendedPurchaseQty = ctx.Calculation.RecommendedPurchaseQty,
                            AverageDailyConsumption = ctx.Calculation.AdcUsed,
                            CurrentQty = ctx.Item.Qty,
                            DaysOfSupply = ctx.Calculation.DaysOfSupply,
                            Urgency = urgency,
                            ReportRoute = ReportRoute,
                            EntityCode = ctx.Item.BrgCode ?? ctx.Item.BrgName ?? string.Empty
                        }
                    };
                })
                .OrderBy(c => c.UrgencyPriority)
                .ThenBy(c => c.Row.ReorderDate ?? businessDate.AddYears(1))
                .ThenByDescending(c => c.Row.RecommendedPurchaseQty)
                .Take(MaxRows)
                .Select((c, index) =>
                {
                    c.Row.SortOrder = index + 1;
                    return c.Row;
                })
                .ToList();

            return candidates;
        }

        private static void TryAddRisk(
            ICollection<RiskCandidate> candidates,
            ISet<string> addedKeys,
            int priority,
            DashboardInventoryItemGroup item,
            string signalKey,
            string signalLabel,
            InventoryForecastCalculation calc,
            string entityCode,
            string explanation)
        {
            var dedupeKey = $"{signalKey}|{item.BrgId}";
            if (!addedKeys.Add(dedupeKey))
                return;

            candidates.Add(new RiskCandidate
            {
                RulePriority = priority,
                Row = new DashboardInventoryForecastRiskRow
                {
                    SignalKey = signalKey,
                    SignalLabel = signalLabel,
                    BrgId = item.BrgId,
                    BrgCode = item.BrgCode,
                    BrgName = item.BrgName,
                    SupplierName = item.SupplierName,
                    DaysOfSupply = calc.DaysOfSupply,
                    StockOutDate = calc.ProjectedStockOutDate,
                    ValueAmount = item.InventoryValue,
                    Urgency = calc.PurchaseUrgency,
                    RuleExplanation = explanation,
                    ReportRoute = ReportRoute,
                    EntityCode = entityCode
                }
            });
        }
    }
}
