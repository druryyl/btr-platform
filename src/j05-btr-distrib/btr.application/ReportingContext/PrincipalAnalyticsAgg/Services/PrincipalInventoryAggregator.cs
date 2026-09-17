using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.InventoryContext.StokBalanceInfo;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;
using btr.application.SalesContext.FakturInfo;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Services
{
    /// <summary>
    /// Maps PRN-INV-001 and PRN-INV-002 from Inventory Snapshot evidence.
    /// Inventory Days uses the existing Average Days of Supply measure. No new days-of-cover algorithm.
    /// </summary>
    public class PrincipalInventoryAggregator
    {
        public PrincipalInventoryAggregateResult Aggregate(
            IEnumerable<StokBalanceView> stockRows,
            IEnumerable<BrgLastFakturDto> lastFakturRows,
            IEnumerable<BrgConsumptionDto> consumptionRows,
            DateTime businessDate,
            DateTime generatedAt,
            int planningHorizonDays,
            int defaultLeadTimeDays,
            int coverageDays)
        {
            var asOfDate = businessDate.Date;
            var monthStart = new DateTime(asOfDate.Year, asOfDate.Month, 1);
            var daysElapsedInMonth = Math.Max(1, (asOfDate - monthStart).Days + 1);
            var items = BuildSnapshotItems(stockRows);

            var lastFakturByBrgId = (lastFakturRows ?? Enumerable.Empty<BrgLastFakturDto>())
                .Where(row => row != null && !string.IsNullOrWhiteSpace(row.BrgId))
                .GroupBy(row => row.BrgId.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

            var consumptionByBrgId = (consumptionRows ?? Enumerable.Empty<BrgConsumptionDto>())
                .Where(row => row != null && !string.IsNullOrWhiteSpace(row.BrgId))
                .GroupBy(row => row.BrgId.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

            var measured = new List<MeasuredItem>();
            foreach (var item in items)
            {
                consumptionByBrgId.TryGetValue(item.BrgId, out var consumption);
                lastFakturByBrgId.TryGetValue(item.BrgId, out var lastFaktur);

                var signalKey = ClassifyMovement(lastFaktur, asOfDate);
                var isAktif = consumption?.IsAktif ?? true;
                var soldQty30 = consumption?.SoldQty30 ?? 0m;
                var soldQty90 = consumption?.SoldQty90 ?? 0m;
                var isEligible = isAktif &&
                                 item.Qty > 0 &&
                                 signalKey != DashboardInventoryRiskAggregator.SignalNeverSold &&
                                 signalKey != DashboardInventoryRiskAggregator.SignalDeadStock;

                var unitHpp = item.Qty > 0 ? item.InventoryValue / item.Qty : 0m;
                var calculation = InventoryForecastPolicy.ComputeItem(
                    item.Qty,
                    unitHpp,
                    soldQty30,
                    soldQty90,
                    planningHorizonDays,
                    defaultLeadTimeDays,
                    coverageDays,
                    asOfDate,
                    consumption?.FirstFakturDate,
                    soldQty30,
                    daysElapsedInMonth);

                measured.Add(new MeasuredItem
                {
                    SupplierId = item.SupplierId,
                    SupplierName = item.SupplierName,
                    InventoryValue = item.InventoryValue,
                    Qty = item.Qty,
                    IsEligible = isEligible,
                    AdcUsed = calculation.AdcUsed
                });
            }

            var principals = measured
                .GroupBy(row => row.SupplierId, StringComparer.OrdinalIgnoreCase)
                .Select(group => MapPrincipal(group))
                .OrderByDescending(row => row.InventoryValue)
                .ThenBy(row => row.SupplierId, StringComparer.OrdinalIgnoreCase)
                .Select((row, index) =>
                {
                    row.SortOrder = index + 1;
                    return row;
                })
                .ToList();

            return new PrincipalInventoryAggregateResult
            {
                InventoryValueKpiId = PrincipalKpiCatalog.InventoryValueId,
                InventoryDaysKpiId = PrincipalKpiCatalog.InventoryDaysId,
                BusinessDate = asOfDate,
                GeneratedAt = generatedAt,
                Principals = principals
            };
        }

        private static PrincipalInventoryRow MapPrincipal(IGrouping<string, MeasuredItem> group)
        {
            var eligible = group.Where(row => row.IsEligible).ToList();
            var totalQtyEligible = eligible.Sum(row => row.Qty);
            var totalAdcForDos = eligible.Where(row => row.AdcUsed > 0).Sum(row => row.AdcUsed);
            decimal? inventoryDays = totalAdcForDos > 0
                ? (decimal?)Math.Round(totalQtyEligible / totalAdcForDos, 2, MidpointRounding.AwayFromZero)
                : null;

            return new PrincipalInventoryRow
            {
                InventoryValueKpiId = PrincipalKpiCatalog.InventoryValueId,
                InventoryDaysKpiId = PrincipalKpiCatalog.InventoryDaysId,
                SupplierId = group.Key,
                SupplierName = group
                    .Select(row => row.SupplierName)
                    .FirstOrDefault(name => !string.IsNullOrEmpty(name)) ?? string.Empty,
                InventoryValue = group.Sum(row => row.InventoryValue),
                InventoryDays = inventoryDays,
                ItemCount = group.Count()
            };
        }

        private static List<SnapshotItem> BuildSnapshotItems(IEnumerable<StokBalanceView> stockRows)
        {
            var filtered = (stockRows ?? Enumerable.Empty<StokBalanceView>())
                .Where(row => row != null)
                .Where(row => !string.Equals(
                    row.WarehouseName,
                    DashboardInventoryItemGroupBuilder.InTransitWarehouseName,
                    StringComparison.OrdinalIgnoreCase));

            return (
                from row in filtered
                group row by row.BrgId ?? string.Empty into itemGroup
                let qty = itemGroup.Sum(row => (decimal)row.Qty)
                where qty > 0
                let supplierId = itemGroup
                    .Select(row => (row.SupplierId ?? string.Empty).Trim())
                    .FirstOrDefault(id => id.Length > 0)
                where !string.IsNullOrEmpty(supplierId)
                select new SnapshotItem
                {
                    BrgId = (itemGroup.Key ?? string.Empty).Trim(),
                    SupplierId = supplierId,
                    SupplierName = itemGroup
                        .Select(row => (row.SupplierName ?? string.Empty).Trim())
                        .FirstOrDefault(name => name.Length > 0) ?? string.Empty,
                    Qty = qty,
                    InventoryValue = itemGroup.Sum(row => row.Hpp * row.Qty)
                }).ToList();
        }

        private static string ClassifyMovement(BrgLastFakturDto lastFaktur, DateTime asOfDate)
        {
            if (lastFaktur is null)
                return DashboardInventoryRiskAggregator.SignalNeverSold;

            var idleDays = (asOfDate - lastFaktur.LastFakturDate.Date).Days;
            if (idleDays >= DashboardInventoryRiskAggregator.DeadStockDaysThreshold)
                return DashboardInventoryRiskAggregator.SignalDeadStock;

            return string.Empty;
        }

        private sealed class SnapshotItem
        {
            public string BrgId { get; set; }

            public string SupplierId { get; set; }

            public string SupplierName { get; set; }

            public decimal Qty { get; set; }

            public decimal InventoryValue { get; set; }
        }

        private sealed class MeasuredItem
        {
            public string SupplierId { get; set; }

            public string SupplierName { get; set; }

            public decimal InventoryValue { get; set; }

            public decimal Qty { get; set; }

            public bool IsEligible { get; set; }

            public decimal AdcUsed { get; set; }
        }
    }
}
