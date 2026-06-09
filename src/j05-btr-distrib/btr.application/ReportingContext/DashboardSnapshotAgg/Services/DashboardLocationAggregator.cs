using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.InventoryContext.StokBalanceInfo;
using btr.application.PurchaseContext.InvoiceInfo;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.SalesContext.FakturInfo;
using btr.domain.InventoryContext.WarehouseAgg;
using btr.nuna.Domain;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public class DashboardLocationAggregator
    {
        public const int TopRankingCount = 10;

        public const string EntityTypeWarehouse = "Warehouse";
        public const string InTransitWarehouseName = DashboardLocationKeyResolver.InTransitWarehouseName;

        public const string SignalWarehouseInventoryConcentration = "WarehouseInventoryConcentration";
        public const string SignalWarehouseAtRiskConcentration = "WarehouseAtRiskConcentration";
        public const string SignalWarehouseSalesConcentration = "WarehouseSalesConcentration";
        public const string SignalWarehousePurchasingConcentration = "WarehousePurchasingConcentration";
        public const string SignalWarehouseNoSalesWithInventory = "WarehouseNoSalesWithInventory";
        public const string SignalWarehouseInactiveWithStock = "WarehouseInactiveWithStock";

        private const string InventoryReportRoute = "/reports/inventory";
        private const string CollectionDashboardRoute = "/dashboard/collection";

        private static readonly Dictionary<string, int> SignalPriority =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
            {
                { SignalWarehouseInactiveWithStock, 1 },
                { SignalWarehouseNoSalesWithInventory, 2 },
                { SignalWarehouseAtRiskConcentration, 3 },
                { SignalWarehouseInventoryConcentration, 4 },
                { SignalWarehouseSalesConcentration, 5 },
                { SignalWarehousePurchasingConcentration, 6 },
            };

        private static readonly Dictionary<string, string> SignalLabels =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { SignalWarehouseInventoryConcentration, "Inventory Concentration" },
                { SignalWarehouseAtRiskConcentration, "At-Risk Concentration" },
                { SignalWarehouseSalesConcentration, "Sales Concentration" },
                { SignalWarehousePurchasingConcentration, "Purchasing Concentration" },
                { SignalWarehouseNoSalesWithInventory, "Stock Without Sales" },
                { SignalWarehouseInactiveWithStock, "Inactive With Stock" },
            };

        private sealed class WarehouseBucket
        {
            public string GroupKey { get; set; }

            public string WarehouseId { get; set; }

            public string WarehouseName { get; set; }

            public decimal InventoryValue { get; set; }

            public decimal AtRiskValue { get; set; }

            public decimal MtdOmzet { get; set; }

            public decimal MtdPurchase { get; set; }

            public bool IsAktif { get; set; } = true;

            public bool IsSpecial { get; set; }

            public bool IsRankingEligible { get; set; }
        }

        public DashboardLocationAggregateResult Aggregate(
            IEnumerable<StokBalanceView> stokRows,
            IEnumerable<BrgLastFakturDto> lastFakturRows,
            IEnumerable<FakturView> fakturRows,
            IEnumerable<InvoiceView> invoiceRows,
            IEnumerable<WarehouseModel> warehouses,
            DashboardInventoryAggregateResult inventorySnapshot,
            DashboardInventoryRiskAggregateResult inventoryRiskSnapshot,
            DashboardSalesAggregateResult salesSnapshot,
            DashboardPurchasingAggregateResult purchasingSnapshot,
            Periode periode,
            DateTime today,
            DateTime generatedAt)
        {
            if (periode is null)
                throw new ArgumentNullException(nameof(periode));

            var warehouseMaster = (warehouses ?? Enumerable.Empty<WarehouseModel>()).ToList();
            var masterById = warehouseMaster
                .Where(w => !string.IsNullOrWhiteSpace(w.WarehouseId))
                .ToDictionary(w => w.WarehouseId.Trim(), w => w, StringComparer.OrdinalIgnoreCase);
            var masterByName = warehouseMaster
                .Where(w => !string.IsNullOrWhiteSpace(w.WarehouseName))
                .GroupBy(w => DashboardLocationKeyResolver.ResolveWarehouseGroupKey(w.WarehouseName, w.WarehouseId),
                    StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var itemGroups = DashboardInventoryItemGroupBuilder.BuildItemGroups(stokRows);
            var atRiskBrgIds = DashboardInventoryRiskClassifier.BuildAtRiskBrgIdSet(
                itemGroups,
                lastFakturRows,
                today);

            var buckets = new Dictionary<string, WarehouseBucket>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in stokRows ?? Enumerable.Empty<StokBalanceView>())
            {
                if (row.Qty <= 0)
                    continue;

                if (DashboardLocationKeyResolver.IsInTransitWarehouse(row.WarehouseName))
                    continue;

                var bucket = GetOrCreateBucket(buckets, row.WarehouseName, row.WarehouseId, masterById, masterByName);
                bucket.InventoryValue += row.Hpp * row.Qty;

                if (atRiskBrgIds.Contains(row.BrgId ?? string.Empty))
                    bucket.AtRiskValue += row.Hpp * row.Qty;
            }

            foreach (var row in fakturRows ?? Enumerable.Empty<FakturView>())
            {
                var bucket = GetOrCreateBucket(buckets, row.WarehouseName, null, masterById, masterByName);
                bucket.MtdOmzet += row.GrandTotal;
            }

            foreach (var row in invoiceRows ?? Enumerable.Empty<InvoiceView>())
            {
                var bucket = GetOrCreateBucket(buckets, row.WarehouseName, null, masterById, masterByName);
                bucket.MtdPurchase += row.GrandTotal;
            }

            var totalInventoryValue = inventorySnapshot?.TotalInventoryValue
                ?? buckets.Values.Sum(b => b.InventoryValue);
            var totalAtRiskValue = inventoryRiskSnapshot?.AtRiskInventoryValue
                ?? buckets.Values.Sum(b => b.AtRiskValue);
            var totalOmzet = salesSnapshot?.TotalOmzet
                ?? buckets.Values.Sum(b => b.MtdOmzet);
            var totalPurchase = purchasingSnapshot?.GrandTotalPurchase
                ?? buckets.Values.Sum(b => b.MtdPurchase);

            var wilayahOmzet = (fakturRows ?? Enumerable.Empty<FakturView>())
                .GroupBy(
                    r => DashboardLocationKeyResolver.ResolveWilayahName(r.WilayahName),
                    StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Sum(r => r.GrandTotal), StringComparer.OrdinalIgnoreCase);

            var rankingBuckets = buckets.Values
                .Where(b => b.IsRankingEligible)
                .ToList();

            var topWarehouseInventory = BuildTopWarehouseInventory(rankingBuckets, totalInventoryValue);
            var topWarehouseAtRisk = BuildTopWarehouseAtRisk(rankingBuckets, totalAtRiskValue);
            var topWarehouseSales = BuildTopWarehouseSales(rankingBuckets, totalOmzet);
            var topWarehousePurchasing = BuildTopWarehousePurchasing(rankingBuckets, totalPurchase);
            var topWilayahSales = BuildTopWilayahSales(wilayahOmzet, totalOmzet);

            var inactiveWarehouseWithStockCount = buckets.Values
                .Count(b => !b.IsAktif
                    && b.InventoryValue > 0
                    && !DashboardLocationKeyResolver.IsInTransitWarehouse(b.WarehouseName));

            var warehouseNoSalesWithInventoryCount = rankingBuckets
                .Count(b => b.InventoryValue > 0 && b.MtdOmzet == 0);

            var top1WarehouseInventoryPercent = ComputePercent(
                topWarehouseInventory.FirstOrDefault()?.InventoryValue,
                totalInventoryValue);
            var top3WarehouseInventoryPercent = totalInventoryValue > 0 && topWarehouseInventory.Count > 0
                ? Math.Round(
                    topWarehouseInventory.Take(3).Sum(x => x.InventoryValue) / totalInventoryValue * 100m,
                    4)
                : (decimal?)null;
            var top1WarehouseAtRiskPercent = ComputePercent(
                topWarehouseAtRisk.FirstOrDefault()?.AtRiskValue,
                totalAtRiskValue);
            var top1WarehouseSalesPercent = ComputePercent(
                topWarehouseSales.FirstOrDefault()?.MtdOmzet,
                totalOmzet);
            var top1WilayahSalesPercent = ComputePercent(
                topWilayahSales.FirstOrDefault()?.MtdOmzet,
                totalOmzet);

            var attentionList = BuildAttentionList(
                topWarehouseInventory,
                topWarehouseAtRisk,
                topWarehouseSales,
                topWarehousePurchasing,
                rankingBuckets,
                buckets.Values);

            return new DashboardLocationAggregateResult
            {
                PeriodYear = periode.Tgl1.Year,
                PeriodMonth = periode.Tgl1.Month,
                Top1WarehouseInventoryPercent = top1WarehouseInventoryPercent,
                Top3WarehouseInventoryPercent = top3WarehouseInventoryPercent,
                Top1WarehouseAtRiskPercent = top1WarehouseAtRiskPercent,
                Top1WarehouseSalesPercent = top1WarehouseSalesPercent,
                Top1WilayahSalesPercent = top1WilayahSalesPercent,
                InactiveWarehouseWithStockCount = inactiveWarehouseWithStockCount,
                WarehouseNoSalesWithInventoryCount = warehouseNoSalesWithInventoryCount,
                TotalInventoryValue = totalInventoryValue,
                TotalAtRiskValue = totalAtRiskValue,
                TotalOmzet = totalOmzet,
                TotalPurchase = totalPurchase,
                GeneratedAt = generatedAt,
                TopWarehouseInventory = topWarehouseInventory,
                TopWarehouseAtRisk = topWarehouseAtRisk,
                TopWarehouseSales = topWarehouseSales,
                TopWarehousePurchasing = topWarehousePurchasing,
                TopWilayahSales = topWilayahSales,
                AttentionList = attentionList
            };
        }

        private static WarehouseBucket GetOrCreateBucket(
            IDictionary<string, WarehouseBucket> buckets,
            string warehouseName,
            string warehouseId,
            IReadOnlyDictionary<string, WarehouseModel> masterById,
            IReadOnlyDictionary<string, WarehouseModel> masterByName)
        {
            WarehouseModel master = null;
            if (!string.IsNullOrWhiteSpace(warehouseId) && masterById.TryGetValue(warehouseId.Trim(), out var byId))
                master = byId;

            var groupKey = DashboardLocationKeyResolver.ResolveWarehouseGroupKey(
                master?.WarehouseName ?? warehouseName,
                master?.WarehouseId ?? warehouseId);

            if (string.IsNullOrWhiteSpace(groupKey))
                groupKey = warehouseId?.Trim() ?? string.Empty;

            if (master == null && !string.IsNullOrWhiteSpace(groupKey))
                masterByName.TryGetValue(groupKey, out master);

            if (!buckets.TryGetValue(groupKey, out var bucket))
            {
                bucket = new WarehouseBucket
                {
                    GroupKey = groupKey,
                    WarehouseId = master?.WarehouseId?.Trim() ?? warehouseId?.Trim() ?? string.Empty,
                    WarehouseName = DashboardLocationKeyResolver.ResolveWarehouseDisplayName(
                        master?.WarehouseName ?? warehouseName),
                    IsAktif = master?.IsAktif ?? true,
                    IsSpecial = master?.IsSpecial ?? false
                };
                ApplyEligibility(bucket);
                buckets[groupKey] = bucket;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(bucket.WarehouseId) && !string.IsNullOrWhiteSpace(warehouseId))
                    bucket.WarehouseId = warehouseId.Trim();

                if (string.IsNullOrWhiteSpace(bucket.WarehouseName) && !string.IsNullOrWhiteSpace(warehouseName))
                    bucket.WarehouseName = DashboardLocationKeyResolver.ResolveWarehouseDisplayName(warehouseName);

                if (master != null)
                {
                    bucket.IsAktif = master.IsAktif;
                    bucket.IsSpecial = master.IsSpecial;
                    if (string.IsNullOrWhiteSpace(bucket.WarehouseId))
                        bucket.WarehouseId = master.WarehouseId?.Trim() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(bucket.WarehouseName))
                        bucket.WarehouseName = DashboardLocationKeyResolver.ResolveWarehouseDisplayName(master.WarehouseName);
                    ApplyEligibility(bucket);
                }
            }

            return bucket;
        }

        private static void ApplyEligibility(WarehouseBucket bucket)
        {
            bucket.IsRankingEligible = DashboardLocationKeyResolver.IsRankingEligible(
                bucket.WarehouseName,
                bucket.IsAktif,
                bucket.IsSpecial);
        }

        private static List<DashboardLocationTopWarehouseInventoryRow> BuildTopWarehouseInventory(
            IEnumerable<WarehouseBucket> buckets,
            decimal totalInventoryValue)
        {
            return buckets
                .Where(b => b.InventoryValue > 0)
                .OrderByDescending(b => b.InventoryValue)
                .ThenBy(b => b.WarehouseName, StringComparer.OrdinalIgnoreCase)
                .Take(TopRankingCount)
                .Select((b, index) => new DashboardLocationTopWarehouseInventoryRow
                {
                    Rank = index + 1,
                    WarehouseId = b.WarehouseId,
                    WarehouseName = b.WarehouseName,
                    InventoryValue = b.InventoryValue,
                    PercentOfTotal = ComputePercent(b.InventoryValue, totalInventoryValue),
                    ReportRoute = InventoryReportRoute
                })
                .ToList();
        }

        private static List<DashboardLocationTopWarehouseAtRiskRow> BuildTopWarehouseAtRisk(
            IEnumerable<WarehouseBucket> buckets,
            decimal totalAtRiskValue)
        {
            return buckets
                .Where(b => b.AtRiskValue > 0)
                .OrderByDescending(b => b.AtRiskValue)
                .ThenBy(b => b.WarehouseName, StringComparer.OrdinalIgnoreCase)
                .Take(TopRankingCount)
                .Select((b, index) => new DashboardLocationTopWarehouseAtRiskRow
                {
                    Rank = index + 1,
                    WarehouseId = b.WarehouseId,
                    WarehouseName = b.WarehouseName,
                    AtRiskValue = b.AtRiskValue,
                    PercentOfTotal = ComputePercent(b.AtRiskValue, totalAtRiskValue),
                    ReportRoute = InventoryReportRoute
                })
                .ToList();
        }

        private static List<DashboardLocationTopWarehouseSalesRow> BuildTopWarehouseSales(
            IEnumerable<WarehouseBucket> buckets,
            decimal totalOmzet)
        {
            return buckets
                .Where(b => b.MtdOmzet > 0)
                .OrderByDescending(b => b.MtdOmzet)
                .ThenBy(b => b.WarehouseName, StringComparer.OrdinalIgnoreCase)
                .Take(TopRankingCount)
                .Select((b, index) => new DashboardLocationTopWarehouseSalesRow
                {
                    Rank = index + 1,
                    WarehouseId = b.WarehouseId,
                    WarehouseName = b.WarehouseName,
                    MtdOmzet = b.MtdOmzet,
                    PercentOfTotal = ComputePercent(b.MtdOmzet, totalOmzet),
                    ReportRoute = null
                })
                .ToList();
        }

        private static List<DashboardLocationTopWarehousePurchasingRow> BuildTopWarehousePurchasing(
            IEnumerable<WarehouseBucket> buckets,
            decimal totalPurchase)
        {
            return buckets
                .Where(b => b.MtdPurchase > 0)
                .OrderByDescending(b => b.MtdPurchase)
                .ThenBy(b => b.WarehouseName, StringComparer.OrdinalIgnoreCase)
                .Take(TopRankingCount)
                .Select((b, index) => new DashboardLocationTopWarehousePurchasingRow
                {
                    Rank = index + 1,
                    WarehouseId = b.WarehouseId,
                    WarehouseName = b.WarehouseName,
                    MtdPurchaseAmount = b.MtdPurchase,
                    PercentOfTotal = ComputePercent(b.MtdPurchase, totalPurchase),
                    ReportRoute = null
                })
                .ToList();
        }

        private static List<DashboardLocationTopWilayahSalesRow> BuildTopWilayahSales(
            Dictionary<string, decimal> wilayahOmzet,
            decimal totalOmzet)
        {
            return wilayahOmzet
                .Where(p => p.Value > 0)
                .OrderByDescending(p => p.Value)
                .ThenBy(p => p.Key, StringComparer.OrdinalIgnoreCase)
                .Take(TopRankingCount)
                .Select((p, index) => new DashboardLocationTopWilayahSalesRow
                {
                    Rank = index + 1,
                    WilayahId = null,
                    WilayahName = p.Key,
                    MtdOmzet = p.Value,
                    PercentOfTotal = ComputePercent(p.Value, totalOmzet),
                    DashboardRoute = CollectionDashboardRoute
                })
                .ToList();
        }

        private static List<DashboardLocationAttentionRow> BuildAttentionList(
            IList<DashboardLocationTopWarehouseInventoryRow> topInventory,
            IList<DashboardLocationTopWarehouseAtRiskRow> topAtRisk,
            IList<DashboardLocationTopWarehouseSalesRow> topSales,
            IList<DashboardLocationTopWarehousePurchasingRow> topPurchasing,
            IList<WarehouseBucket> rankingBuckets,
            IEnumerable<WarehouseBucket> allBuckets)
        {
            var rows = new List<DashboardLocationAttentionRow>();

            foreach (var bucket in allBuckets.Where(b =>
                !b.IsAktif
                && b.InventoryValue > 0
                && !DashboardLocationKeyResolver.IsInTransitWarehouse(b.WarehouseName)))
            {
                rows.Add(CreateAttentionRow(
                    bucket,
                    SignalWarehouseInactiveWithStock,
                    bucket.InventoryValue,
                    null));
            }

            foreach (var bucket in rankingBuckets.Where(b => b.InventoryValue > 0 && b.MtdOmzet == 0))
            {
                rows.Add(CreateAttentionRow(
                    bucket,
                    SignalWarehouseNoSalesWithInventory,
                    bucket.InventoryValue,
                    null));
            }

            AddConcentrationRows(
                rows,
                topAtRisk,
                SignalWarehouseAtRiskConcentration,
                r => r.AtRiskValue,
                r => r.WarehouseId,
                r => r.WarehouseName,
                r => r.PercentOfTotal);
            AddConcentrationRows(
                rows,
                topInventory,
                SignalWarehouseInventoryConcentration,
                r => r.InventoryValue,
                r => r.WarehouseId,
                r => r.WarehouseName,
                r => r.PercentOfTotal);
            AddConcentrationRows(
                rows,
                topSales,
                SignalWarehouseSalesConcentration,
                r => r.MtdOmzet,
                r => r.WarehouseId,
                r => r.WarehouseName,
                r => r.PercentOfTotal);
            AddConcentrationRows(
                rows,
                topPurchasing,
                SignalWarehousePurchasingConcentration,
                r => r.MtdPurchaseAmount,
                r => r.WarehouseId,
                r => r.WarehouseName,
                r => r.PercentOfTotal);

            return rows
                .OrderBy(r => SignalPriority.TryGetValue(r.SignalKey ?? string.Empty, out var priority) ? priority : 99)
                .ThenByDescending(r => r.ValueAmount ?? 0m)
                .ThenBy(r => r.EntityName, StringComparer.OrdinalIgnoreCase)
                .Select((r, index) =>
                {
                    r.SortOrder = index + 1;
                    return r;
                })
                .ToList();
        }

        private static void AddConcentrationRows<T>(
            ICollection<DashboardLocationAttentionRow> rows,
            IEnumerable<T> topRows,
            string signalKey,
            Func<T, decimal> amountSelector,
            Func<T, string> warehouseIdSelector,
            Func<T, string> warehouseNameSelector,
            Func<T, decimal?> percentSelector)
        {
            foreach (var row in topRows ?? Enumerable.Empty<T>())
            {
                var percent = percentSelector(row);
                rows.Add(new DashboardLocationAttentionRow
                {
                    EntityType = EntityTypeWarehouse,
                    EntityCode = warehouseIdSelector(row),
                    EntityName = warehouseNameSelector(row),
                    SignalKey = signalKey,
                    SignalLabel = SignalLabels[signalKey],
                    ValueAmount = amountSelector(row),
                    ValueText = percent.HasValue ? $"{percent.Value:0.####}% of total" : null,
                    ReportRoute = InventoryReportRoute
                });
            }
        }

        private static DashboardLocationAttentionRow CreateAttentionRow(
            WarehouseBucket bucket,
            string signalKey,
            decimal valueAmount,
            string valueText)
        {
            return new DashboardLocationAttentionRow
            {
                EntityType = EntityTypeWarehouse,
                EntityCode = bucket.WarehouseId,
                EntityName = bucket.WarehouseName,
                SignalKey = signalKey,
                SignalLabel = SignalLabels[signalKey],
                ValueAmount = valueAmount,
                ValueText = valueText,
                ReportRoute = InventoryReportRoute
            };
        }

        private static decimal? ComputePercent(decimal? amount, decimal total)
        {
            if (!amount.HasValue || total <= 0)
                return null;

            return Math.Round(amount.Value / total * 100m, 4);
        }

        private static decimal? ComputePercent(decimal amount, decimal total)
        {
            return ComputePercent((decimal?)amount, total);
        }
    }
}
