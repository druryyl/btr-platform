using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.InventoryContext.StokBalanceInfo;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.SalesContext.FakturInfo;
using btr.domain.InventoryContext.WarehouseAgg;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public static class WarehouseBalanceRecommendationBuilder
    {
        public const string RuleId = "IO-TRANSFER";

        public static List<WarehouseTransferContext> BuildTransferPairs(
            IEnumerable<StokBalanceView> balanceRows,
            IEnumerable<BrgWarehouseConsumptionDto> warehouseConsumption,
            IEnumerable<WarehouseModel> warehouses,
            HashSet<string> criticalPurchaseBrgIds,
            int shortageDosDays,
            int excessDosDays,
            int leadTimeDays,
            int maxRows)
        {
            var warehouseById = (warehouses ?? Enumerable.Empty<WarehouseModel>())
                .GroupBy(w => w.WarehouseId ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            bool IsEligible(string warehouseId, string warehouseName)
            {
                if (DashboardLocationKeyResolver.IsInTransitWarehouse(warehouseName))
                    return false;

                if (warehouseById.TryGetValue(warehouseId ?? string.Empty, out var wh))
                    return DashboardLocationKeyResolver.IsRankingEligible(wh);

                return DashboardLocationKeyResolver.IsRankingEligible(warehouseName, true, false);
            }

            var consumptionByKey = (warehouseConsumption ?? Enumerable.Empty<BrgWarehouseConsumptionDto>())
                .GroupBy(c => $"{c.BrgId}|{c.WarehouseId}", StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var rowsByBrg = (balanceRows ?? Enumerable.Empty<StokBalanceView>())
                .Where(r => !DashboardLocationKeyResolver.IsInTransitWarehouse(r.WarehouseName))
                .GroupBy(r => r.BrgId ?? string.Empty, StringComparer.OrdinalIgnoreCase);

            var pairs = new List<WarehouseTransferContext>();

            foreach (var group in rowsByBrg)
            {
                var brgId = group.Key;
                if (string.IsNullOrWhiteSpace(brgId))
                    continue;

                var warehouseStates = group
                    .Where(r => IsEligible(r.WarehouseId, r.WarehouseName))
                    .Select(r =>
                    {
                        consumptionByKey.TryGetValue($"{r.BrgId}|{r.WarehouseId}", out var cons);
                        var soldQty30 = cons?.SoldQty30 ?? 0m;
                        var adc = InventoryForecastPolicy.ComputeAdc30(soldQty30);
                        var dos = InventoryForecastPolicy.ComputeDaysOfSupply(r.Qty, adc);
                        return new
                        {
                            Row = r,
                            Adc = adc,
                            Dos = dos,
                            Qty = (decimal)r.Qty
                        };
                    })
                    .Where(x => x.Adc > 0)
                    .ToList();

                var shortages = warehouseStates
                    .Where(x => x.Dos.HasValue && x.Dos.Value <= shortageDosDays)
                    .ToList();

                var excesses = warehouseStates
                    .Where(x => x.Dos.HasValue && x.Dos.Value >= excessDosDays)
                    .ToList();

                foreach (var dest in shortages)
                {
                    foreach (var source in excesses)
                    {
                        if (string.Equals(source.Row.WarehouseId, dest.Row.WarehouseId, StringComparison.OrdinalIgnoreCase))
                            continue;

                        var transferQty = InventoryOptimizationPolicy.ComputeTransferQty(
                            source.Qty,
                            source.Adc,
                            dest.Qty,
                            dest.Adc,
                            leadTimeDays);

                        if (transferQty < Math.Ceiling(dest.Adc * 7))
                            continue;

                        var avoidsCritical = criticalPurchaseBrgIds != null &&
                                             criticalPurchaseBrgIds.Contains(brgId);

                        var category = InventoryOptimizationPolicy.ResolveCategory(
                            InventoryOptimizationPolicy.ActionTransfer,
                            dest.Dos,
                            null,
                            DateTime.Today,
                            DashboardInventoryRiskAggregator.BucketActive,
                            dest.Row.NilaiSediaan,
                            0m);

                        var impact = InventoryOptimizationPolicy.ComputePurchaseCost(
                            transferQty,
                            InventoryOptimizationPolicy.ComputeUnitHpp(dest.Qty, dest.Row.NilaiSediaan));

                        var score = InventoryOptimizationPolicy.ComputePriorityScore(
                            category,
                            impact,
                            dest.Dos,
                            leadTimeDays,
                            false,
                            InventoryOptimizationPolicy.ActionTransfer,
                            avoidsCritical);

                        pairs.Add(new WarehouseTransferContext
                        {
                            BrgId = brgId,
                            BrgName = dest.Row.BrgName,
                            WarehouseFromId = source.Row.WarehouseId,
                            WarehouseFromName = source.Row.WarehouseName,
                            WarehouseToId = dest.Row.WarehouseId,
                            WarehouseToName = dest.Row.WarehouseName,
                            SourceQty = source.Qty,
                            SourceAdc = source.Adc,
                            DestQty = dest.Qty,
                            DestAdc = dest.Adc,
                            DestDaysOfSupply = dest.Dos,
                            TransferQty = transferQty,
                            Category = category,
                            PriorityScore = score,
                            AvoidsCriticalPurchase = avoidsCritical
                        });
                    }
                }
            }

            return pairs
                .OrderByDescending(p => p.PriorityScore)
                .ThenByDescending(p => p.TransferQty)
                .Take(maxRows)
                .ToList();
        }
    }
}
