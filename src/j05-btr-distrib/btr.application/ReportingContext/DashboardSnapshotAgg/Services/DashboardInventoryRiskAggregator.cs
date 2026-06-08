using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.InventoryContext.StokBalanceInfo;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.SalesContext.FakturInfo;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public class DashboardInventoryRiskAggregator
    {
        public const int SlowMovingDaysThreshold = 90;
        public const int DeadStockDaysThreshold = 180;
        public const int TopItemCount = 10;

        public const string SignalDeadStock = "DeadStock";
        public const string SignalSlowMoving = "SlowMoving";
        public const string SignalNeverSold = "NeverSold";

        public const string BucketActive = "Active";
        public const string BucketSlowMoving = "SlowMoving";
        public const string BucketDeadStock = "DeadStock";
        public const string BucketNeverSold = "NeverSold";

        public const string DimensionCategory = "Category";
        public const string DimensionSupplier = "Supplier";

        private sealed class ClassifiedItem
        {
            public DashboardInventoryItemGroup Item { get; set; }

            public string SignalKey { get; set; }

            public string SignalLabel { get; set; }

            public int? DaysSinceLastFaktur { get; set; }

            public int SignalSortOrder { get; set; }
        }

        public DashboardInventoryRiskAggregateResult Aggregate(
            IEnumerable<StokBalanceView> rows,
            IEnumerable<BrgLastFakturDto> lastFakturRows,
            DateTime today,
            DateTime generatedAt)
        {
            var itemGroups = DashboardInventoryItemGroupBuilder.BuildItemGroups(rows);
            var lastFakturByBrgId = (lastFakturRows ?? Enumerable.Empty<BrgLastFakturDto>())
                .GroupBy(x => x.BrgId ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var classified = itemGroups
                .Select(item => ClassifyItem(item, lastFakturByBrgId, today))
                .ToList();

            var neverSold = classified.Where(x => x.SignalKey == SignalNeverSold).ToList();
            var slowMoving = classified.Where(x => x.SignalKey == SignalSlowMoving).ToList();
            var deadStock = classified.Where(x => x.SignalKey == SignalDeadStock).ToList();
            var active = classified.Where(x => x.SignalKey == BucketActive).ToList();

            var neverSoldValue = neverSold.Sum(x => x.Item.InventoryValue);
            var slowValue = slowMoving.Sum(x => x.Item.InventoryValue);
            var deadValue = deadStock.Sum(x => x.Item.InventoryValue);
            var atRiskValue = neverSoldValue + slowValue + deadValue;
            var totalInventoryValue = itemGroups.Sum(x => x.InventoryValue);

            decimal? atRiskPercent = totalInventoryValue > 0
                ? Math.Round(atRiskValue / totalInventoryValue * 100m, 4)
                : (decimal?)null;

            var requiresAttention = atRiskValue > 0
                || (atRiskPercent ?? 0) > 0
                || neverSold.Count > 0
                || slowMoving.Count > 0
                || deadStock.Count > 0;

            var agingBuckets = BuildAgingBuckets(active, slowMoving, deadStock, neverSold);
            var atRiskItems = classified.Where(x => x.SignalKey != BucketActive).ToList();
            var breakdown = BuildBreakdown(atRiskItems, atRiskValue);
            var topDead = BuildTopRankings(deadStock, deadValue);
            var topSlow = BuildTopRankings(slowMoving, slowValue);
            var attentionList = BuildAttentionList(atRiskItems);

            return new DashboardInventoryRiskAggregateResult
            {
                TotalInventoryValue = totalInventoryValue,
                TotalItem = itemGroups.Count,
                DeadStockItemCount = deadStock.Count,
                DeadStockValue = deadValue,
                SlowMovingItemCount = slowMoving.Count,
                SlowMovingValue = slowValue,
                NeverSoldItemCount = neverSold.Count,
                NeverSoldValue = neverSoldValue,
                AtRiskInventoryValue = atRiskValue,
                AtRiskInventoryPercent = atRiskPercent,
                RequiresAttention = requiresAttention,
                GeneratedAt = generatedAt,
                AgingBuckets = agingBuckets,
                AttentionList = attentionList,
                TopDead = topDead,
                TopSlow = topSlow,
                Breakdown = breakdown
            };
        }

        private static ClassifiedItem ClassifyItem(
            DashboardInventoryItemGroup item,
            IReadOnlyDictionary<string, BrgLastFakturDto> lastFakturByBrgId,
            DateTime today)
        {
            if (!lastFakturByBrgId.TryGetValue(item.BrgId ?? string.Empty, out var lastFaktur))
            {
                return new ClassifiedItem
                {
                    Item = item,
                    SignalKey = SignalNeverSold,
                    SignalLabel = "Never Sold",
                    DaysSinceLastFaktur = null,
                    SignalSortOrder = 3
                };
            }

            var idleDays = (today.Date - lastFaktur.LastFakturDate.Date).Days;

            if (idleDays >= DeadStockDaysThreshold)
            {
                return new ClassifiedItem
                {
                    Item = item,
                    SignalKey = SignalDeadStock,
                    SignalLabel = "Dead Stock",
                    DaysSinceLastFaktur = idleDays,
                    SignalSortOrder = 1
                };
            }

            if (idleDays >= SlowMovingDaysThreshold)
            {
                return new ClassifiedItem
                {
                    Item = item,
                    SignalKey = SignalSlowMoving,
                    SignalLabel = "Slow Moving",
                    DaysSinceLastFaktur = idleDays,
                    SignalSortOrder = 2
                };
            }

            return new ClassifiedItem
            {
                Item = item,
                SignalKey = BucketActive,
                SignalLabel = "Active",
                DaysSinceLastFaktur = idleDays,
                SignalSortOrder = 0
            };
        }

        private static IList<DashboardInventoryRiskAgingRow> BuildAgingBuckets(
            IList<ClassifiedItem> active,
            IList<ClassifiedItem> slowMoving,
            IList<ClassifiedItem> deadStock,
            IList<ClassifiedItem> neverSold)
        {
            return new List<DashboardInventoryRiskAgingRow>
            {
                CreateAgingRow(BucketActive, "Active (≤ 89 days)", active, 1),
                CreateAgingRow(BucketSlowMoving, "Slow Moving (90–179 days)", slowMoving, 2),
                CreateAgingRow(BucketDeadStock, "Dead Stock (≥ 180 days)", deadStock, 3),
                CreateAgingRow(BucketNeverSold, "Never Sold", neverSold, 4)
            };
        }

        private static DashboardInventoryRiskAgingRow CreateAgingRow(
            string bucketKey,
            string bucketLabel,
            IList<ClassifiedItem> items,
            int sortOrder)
        {
            return new DashboardInventoryRiskAgingRow
            {
                BucketKey = bucketKey,
                BucketLabel = bucketLabel,
                InventoryValue = items.Sum(x => x.Item.InventoryValue),
                ItemCount = items.Count,
                SortOrder = sortOrder
            };
        }

        private static IList<DashboardInventoryRiskBreakdownRow> BuildBreakdown(
            IList<ClassifiedItem> atRiskItems,
            decimal atRiskValue)
        {
            var breakdown = new List<DashboardInventoryRiskBreakdownRow>();
            breakdown.AddRange(BuildDimensionBreakdown(atRiskItems, DimensionCategory, g => g.Item.CategoryName, atRiskValue));
            breakdown.AddRange(BuildDimensionBreakdown(atRiskItems, DimensionSupplier, g => g.Item.SupplierName, atRiskValue));
            return breakdown;
        }

        private static IEnumerable<DashboardInventoryRiskBreakdownRow> BuildDimensionBreakdown(
            IList<ClassifiedItem> atRiskItems,
            string dimensionType,
            Func<ClassifiedItem, string> dimensionSelector,
            decimal atRiskValue)
        {
            var rollup = atRiskItems
                .GroupBy(dimensionSelector, StringComparer.OrdinalIgnoreCase)
                .Select(g => new
                {
                    Name = g.Key,
                    AtRiskValue = g.Sum(x => x.Item.InventoryValue),
                    ItemCount = g.Count()
                })
                .OrderByDescending(x => x.AtRiskValue)
                .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .Take(TopItemCount)
                .Select((x, index) => new DashboardInventoryRiskBreakdownRow
                {
                    DimensionType = dimensionType,
                    Name = x.Name,
                    AtRiskValue = x.AtRiskValue,
                    ItemCount = x.ItemCount,
                    Rank = index + 1,
                    PercentOfAtRisk = atRiskValue > 0
                        ? Math.Round(x.AtRiskValue / atRiskValue * 100m, 4)
                        : (decimal?)null
                })
                .ToList();

            return rollup;
        }

        private static IList<DashboardInventoryRiskTopRow> BuildTopRankings(
            IList<ClassifiedItem> items,
            decimal classValue)
        {
            return items
                .OrderByDescending(x => x.Item.InventoryValue)
                .ThenBy(x => x.Item.BrgName, StringComparer.OrdinalIgnoreCase)
                .Take(TopItemCount)
                .Select((x, index) => new DashboardInventoryRiskTopRow
                {
                    Rank = index + 1,
                    BrgId = x.Item.BrgId,
                    BrgCode = x.Item.BrgCode,
                    BrgName = x.Item.BrgName,
                    KategoriName = x.Item.CategoryName,
                    SupplierName = x.Item.SupplierName,
                    Qty = (int)x.Item.Qty,
                    InventoryValue = x.Item.InventoryValue,
                    DaysSinceLastFaktur = x.DaysSinceLastFaktur ?? 0,
                    PercentOfAtRisk = classValue > 0
                        ? Math.Round(x.Item.InventoryValue / classValue * 100m, 4)
                        : (decimal?)null
                })
                .ToList();
        }

        private static IList<DashboardInventoryRiskAttentionRow> BuildAttentionList(
            IList<ClassifiedItem> atRiskItems)
        {
            return atRiskItems
                .OrderBy(x => x.SignalSortOrder)
                .ThenByDescending(x => x.Item.InventoryValue)
                .ThenBy(x => x.Item.BrgName, StringComparer.OrdinalIgnoreCase)
                .Select((x, index) => new DashboardInventoryRiskAttentionRow
                {
                    BrgId = x.Item.BrgId,
                    BrgCode = x.Item.BrgCode,
                    BrgName = x.Item.BrgName,
                    KategoriName = x.Item.CategoryName,
                    SupplierName = x.Item.SupplierName,
                    Qty = (int)x.Item.Qty,
                    InventoryValue = x.Item.InventoryValue,
                    DaysSinceLastFaktur = x.DaysSinceLastFaktur,
                    SignalKey = x.SignalKey,
                    SignalLabel = x.SignalLabel,
                    SortOrder = index + 1
                })
                .ToList();
        }
    }
}
