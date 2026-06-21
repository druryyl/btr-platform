using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.InventoryContext.StokBalanceInfo;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.SalesContext.FakturInfo;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public class DashboardInventoryForecastAggregator
    {
        public DashboardInventoryForecastAggregateResult Aggregate(
            IEnumerable<StokBalanceView> rows,
            IEnumerable<BrgLastFakturDto> lastFakturRows,
            IEnumerable<BrgConsumptionDto> consumptionRows,
            IEnumerable<DailyCompanyConsumptionDto> dailyConsumptionRows,
            DashboardInventoryRiskAggregateResult riskResult,
            DateTime businessDate,
            DateTime generatedAt,
            int planningHorizonDays,
            int defaultLeadTimeDays,
            int coverageDays,
            int overstockDosDays,
            int minDosHealthy)
        {
            if (riskResult is null)
                throw new ArgumentNullException(nameof(riskResult));

            var asOfDate = businessDate.Date;
            var window30Start = asOfDate.AddDays(-(InventoryForecastPolicy.AdcWindow30Days - 1));
            var window90Start = asOfDate.AddDays(-(InventoryForecastPolicy.AdcWindow90Days - 1));
            var monthStart = new DateTime(asOfDate.Year, asOfDate.Month, 1);
            var daysElapsedInMonth = Math.Max(1, (asOfDate - monthStart).Days + 1);

            var itemGroups = DashboardInventoryItemGroupBuilder.BuildItemGroups(rows);
            var currentInventoryValue = itemGroups.Sum(x => x.InventoryValue);

            var lastFakturByBrgId = (lastFakturRows ?? Enumerable.Empty<BrgLastFakturDto>())
                .GroupBy(x => x.BrgId ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var consumptionByBrgId = (consumptionRows ?? Enumerable.Empty<BrgConsumptionDto>())
                .GroupBy(x => x.BrgId ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var dailyTotals = (dailyConsumptionRows ?? Enumerable.Empty<DailyCompanyConsumptionDto>())
                .GroupBy(x => x.FakturDate.Date)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.UnitsSold));

            var itemContexts = new List<ForecastItemContext>();
            decimal totalAdc = 0m;
            decimal totalQtyEligible = 0m;
            decimal totalAdcForDos = 0m;
            decimal projectedValue = 0m;
            decimal bestCaseValue = 0m;
            decimal worstCaseValue = 0m;
            decimal understockValue = 0m;
            decimal overstockValue = 0m;
            int stockOutCount = 0;
            int coverageEligibleCount = 0;
            int adequateCoverageCount = 0;

            var heat = new int[9];

            foreach (var item in itemGroups)
            {
                var brgId = item.BrgId ?? string.Empty;
                consumptionByBrgId.TryGetValue(brgId, out var consumption);
                lastFakturByBrgId.TryGetValue(brgId, out var lastFaktur);

                var movement = ClassifyMovement(item, lastFaktur, asOfDate);
                var isAktif = consumption?.IsAktif ?? true;
                var soldQty30 = consumption?.SoldQty30 ?? 0m;
                var soldQty90 = consumption?.SoldQty90 ?? 0m;
                var soldQtyMtd = consumption?.SoldQty30 ?? 0m;

                var isEligible = isAktif &&
                                 item.Qty > 0 &&
                                 movement.SignalKey != DashboardInventoryRiskAggregator.SignalNeverSold &&
                                 movement.SignalKey != DashboardInventoryRiskAggregator.SignalDeadStock;

                var unitHpp = item.Qty > 0 ? item.InventoryValue / item.Qty : 0m;
                var calc = InventoryForecastPolicy.ComputeItem(
                    item.Qty,
                    unitHpp,
                    soldQty30,
                    soldQty90,
                    planningHorizonDays,
                    defaultLeadTimeDays,
                    coverageDays,
                    asOfDate,
                    consumption?.FirstFakturDate,
                    soldQtyMtd,
                    daysElapsedInMonth);

                if (isEligible)
                {
                    totalAdc += calc.AdcUsed;
                    totalQtyEligible += item.Qty;
                    if (calc.AdcUsed > 0)
                        totalAdcForDos += calc.AdcUsed;

                    projectedValue += calc.ForecastValueAtHorizon;
                    bestCaseValue += calc.BestCaseForecastValue;
                    worstCaseValue += calc.WorstCaseForecastValue;

                    if (calc.AdcUsed > 0 &&
                        calc.DaysOfSupply.HasValue &&
                        calc.DaysOfSupply.Value <= planningHorizonDays)
                    {
                        understockValue += item.InventoryValue;
                        stockOutCount++;
                    }

                    if (calc.AdcUsed > 0 &&
                        calc.DaysOfSupply.HasValue &&
                        calc.DaysOfSupply.Value > overstockDosDays)
                    {
                        overstockValue += item.InventoryValue;
                    }

                    coverageEligibleCount++;
                    if (calc.DaysOfSupply.GetValueOrDefault() >= minDosHealthy)
                        adequateCoverageCount++;

                    IncrementHeatCell(heat, calc.DaysOfSupply, item.InventoryValue, currentInventoryValue);
                }

                itemContexts.Add(new ForecastItemContext
                {
                    Item = item,
                    Calculation = calc,
                    MovementSignalKey = movement.SignalKey,
                    DaysSinceLastFaktur = movement.DaysSinceLastFaktur,
                    IsForecastEligible = isEligible
                });
            }

            var companySoldQty30 = consumptionByBrgId.Values.Sum(x => x.SoldQty30);
            var weightedDos = totalAdcForDos > 0
                ? (decimal?)Math.Round(totalQtyEligible / totalAdcForDos, 2, MidpointRounding.AwayFromZero)
                : null;

            var forecastConsumption = Math.Round(totalAdc * planningHorizonDays, 4, MidpointRounding.AwayFromZero);
            decimal? coveragePercent = coverageEligibleCount > 0
                ? Math.Round((decimal)adequateCoverageCount / coverageEligibleCount * 100m, 4, MidpointRounding.AwayFromZero)
                : (decimal?)null;

            decimal? turnoverForecast = totalQtyEligible > 0
                ? Math.Round(forecastConsumption / totalQtyEligible, 4, MidpointRounding.AwayFromZero)
                : (decimal?)null;

            var stockOutRiskPct = currentInventoryValue > 0
                ? Math.Round(understockValue / currentInventoryValue * 100m, 4, MidpointRounding.AwayFromZero)
                : 0m;
            var overstockPct = currentInventoryValue > 0
                ? Math.Round(overstockValue / currentInventoryValue * 100m, 4, MidpointRounding.AwayFromZero)
                : 0m;
            var atRiskPct = riskResult.AtRiskInventoryPercent ?? 0m;
            var healthScore = InventoryForecastPolicy.ComputeHealthScore(stockOutRiskPct, overstockPct, atRiskPct);

            var adcReference = totalAdc;
            var dailyBuckets = InventoryConsumptionGrouper.BuildBuckets(window30Start, asOfDate);
            var dailyConsumption = dailyBuckets.Select(bucket => new DashboardInventoryForecastDailyConsumptionRow
            {
                ConsumptionDate = bucket.ConsumptionDate,
                DayIndex = bucket.DayIndex,
                UnitsSold = dailyTotals.TryGetValue(bucket.ConsumptionDate, out var units) ? units : 0m,
                AdcReference = adcReference
            }).ToList();

            var projectedLevel = new List<DashboardInventoryForecastLevelRow>();
            for (var day = 0; day <= planningHorizonDays; day++)
            {
                var dayValue = itemContexts
                    .Where(ctx => ctx.IsForecastEligible)
                    .Sum(ctx =>
                    {
                        var unitHpp = ctx.Item.Qty > 0 ? ctx.Item.InventoryValue / ctx.Item.Qty : 0m;
                        var qty = InventoryForecastPolicy.ComputeForecastQtyAtHorizon(
                            ctx.Item.Qty,
                            ctx.Calculation.AdcUsed,
                            day);
                        return InventoryForecastPolicy.ComputeProjectedValue(qty, unitHpp);
                    });

                projectedLevel.Add(new DashboardInventoryForecastLevelRow
                {
                    HorizonDay = day,
                    ProjectedInventoryValue = dayValue
                });
            }

            var topRisks = InventoryForecastRiskBuilder.BuildTopRisks(
                itemContexts,
                planningHorizonDays,
                overstockDosDays,
                asOfDate);

            var recommendations = InventoryForecastRiskBuilder.BuildPurchaseRecommendations(itemContexts, asOfDate);

            return new DashboardInventoryForecastAggregateResult
            {
                GeneratedAt = generatedAt,
                BusinessDate = asOfDate,
                PlanningHorizonDays = planningHorizonDays,
                CurrentInventoryValue = currentInventoryValue,
                ProjectedInventoryValue = projectedValue,
                BestCaseProjectedValue = bestCaseValue,
                WorstCaseProjectedValue = worstCaseValue,
                AverageDailyConsumptionUnits = totalAdc,
                WeightedAverageDaysOfSupply = weightedDos,
                UnderstockValue = understockValue,
                OverstockValue = overstockValue,
                StockOutRiskItemCount = stockOutCount,
                InventoryCoveragePercent = coveragePercent,
                InventoryTurnoverForecast = turnoverForecast,
                InventoryHealthScore = healthScore,
                ForecastConfidence = InventoryForecastPolicy.ResolveConfidence(
                    planningHorizonDays,
                    companySoldQty30,
                    daysElapsedInMonth),
                AtRiskInventoryPercent = riskResult.AtRiskInventoryPercent,
                ForecastConsumptionUnits = forecastConsumption,
                HeatCellLowLow = heat[0],
                HeatCellLowMed = heat[1],
                HeatCellLowHigh = heat[2],
                HeatCellMedLow = heat[3],
                HeatCellMedMed = heat[4],
                HeatCellMedHigh = heat[5],
                HeatCellHighLow = heat[6],
                HeatCellHighMed = heat[7],
                HeatCellHighHigh = heat[8],
                DailyConsumption = dailyConsumption,
                ProjectedLevel = projectedLevel,
                TopRisks = topRisks,
                PurchaseRecommendations = recommendations,
                ItemContexts = itemContexts
            };
        }

        private static MovementClassification ClassifyMovement(
            DashboardInventoryItemGroup item,
            BrgLastFakturDto lastFaktur,
            DateTime today)
        {
            if (lastFaktur is null)
            {
                return new MovementClassification
                {
                    SignalKey = DashboardInventoryRiskAggregator.SignalNeverSold,
                    DaysSinceLastFaktur = null
                };
            }

            var idleDays = (today - lastFaktur.LastFakturDate.Date).Days;
            if (idleDays >= DashboardInventoryRiskAggregator.DeadStockDaysThreshold)
            {
                return new MovementClassification
                {
                    SignalKey = DashboardInventoryRiskAggregator.SignalDeadStock,
                    DaysSinceLastFaktur = idleDays
                };
            }

            if (idleDays >= DashboardInventoryRiskAggregator.SlowMovingDaysThreshold)
            {
                return new MovementClassification
                {
                    SignalKey = DashboardInventoryRiskAggregator.SignalSlowMoving,
                    DaysSinceLastFaktur = idleDays
                };
            }

            return new MovementClassification
            {
                SignalKey = DashboardInventoryRiskAggregator.BucketActive,
                DaysSinceLastFaktur = idleDays
            };
        }

        private static void IncrementHeatCell(int[] heat, decimal? dos, decimal itemValue, decimal totalValue)
        {
            if (!dos.HasValue || totalValue <= 0)
                return;

            var dosBand = dos.Value <= 14 ? 0 : dos.Value <= 60 ? 1 : 2;
            var valueShare = itemValue / totalValue;
            var valueBand = valueShare < 0.01m ? 0 : valueShare < 0.05m ? 1 : 2;
            var index = dosBand * 3 + valueBand;
            heat[index]++;
        }

        private sealed class MovementClassification
        {
            public string SignalKey { get; set; }

            public int? DaysSinceLastFaktur { get; set; }
        }
    }
}
