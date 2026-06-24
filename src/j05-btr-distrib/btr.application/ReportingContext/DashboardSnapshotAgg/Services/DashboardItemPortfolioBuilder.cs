using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.SalesContext.FakturInfo;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public class DashboardItemPortfolioBuilder
    {
        public const int TrendEligibilityDays = 730;

        public IList<DashboardItemPortfolioRow> Build(
            IEnumerable<DashboardInventoryItemGroup> itemGroups,
            IEnumerable<ForecastItemContext> forecastContexts,
            DashboardInventoryRiskAggregateResult riskAggregate,
            IEnumerable<SalesmanMtdItemRollupDto> rollupRows,
            IEnumerable<BrgLastFakturDto> lastFakturRows,
            DateTime today)
        {
            var movementByBrgId = BuildMovementIndex(riskAggregate);
            var forecastByBrgId = (forecastContexts ?? Enumerable.Empty<ForecastItemContext>())
                .Where(x => x?.Item != null && !string.IsNullOrWhiteSpace(x.Item.BrgId))
                .GroupBy(x => x.Item.BrgId.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var customerCountByBrgId = (rollupRows ?? Enumerable.Empty<SalesmanMtdItemRollupDto>())
                .Where(r => !string.IsNullOrWhiteSpace(r.BrgId))
                .GroupBy(r => r.BrgId.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.CustomerCode).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
                    StringComparer.OrdinalIgnoreCase);

            var supplierByBrgId = (rollupRows ?? Enumerable.Empty<SalesmanMtdItemRollupDto>())
                .Where(r => !string.IsNullOrWhiteSpace(r.BrgId))
                .GroupBy(r => r.BrgId.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var lastFakturByBrgId = (lastFakturRows ?? Enumerable.Empty<BrgLastFakturDto>())
                .Where(x => !string.IsNullOrWhiteSpace(x.BrgId))
                .GroupBy(x => x.BrgId.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var portfolioByBrgId = new Dictionary<string, DashboardItemPortfolioRow>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in itemGroups ?? Enumerable.Empty<DashboardInventoryItemGroup>())
            {
                if (string.IsNullOrWhiteSpace(item.BrgId))
                    continue;

                var brgId = item.BrgId.Trim();
                portfolioByBrgId[brgId] = CreateRow(
                    item,
                    movementByBrgId,
                    forecastByBrgId,
                    customerCountByBrgId,
                    supplierByBrgId,
                    lastFakturByBrgId,
                    today);
            }

            foreach (var lastFaktur in lastFakturByBrgId.Values)
            {
                var brgId = lastFaktur.BrgId?.Trim();
                if (string.IsNullOrWhiteSpace(brgId) || portfolioByBrgId.ContainsKey(brgId))
                    continue;

                var idleDays = (today.Date - lastFaktur.LastFakturDate.Date).Days;
                if (idleDays > TrendEligibilityDays)
                    continue;

                supplierByBrgId.TryGetValue(brgId, out var supplier);

                portfolioByBrgId[brgId] = new DashboardItemPortfolioRow
                {
                    BrgId = brgId,
                    BrgCode = lastFaktur.BrgCode?.Trim() ?? brgId,
                    BrgName = lastFaktur.BrgName?.Trim() ?? brgId,
                    CategoryName = DashboardInventoryItemGroupBuilder.UnknownLabel,
                    SupplierName = supplier?.SupplierName ?? DashboardInventoryItemGroupBuilder.UnknownLabel,
                    SupplierId = supplier?.SupplierId?.Trim() ?? string.Empty,
                    SupplierCode = supplier?.SupplierCode?.Trim() ?? string.Empty,
                    Qty = 0,
                    InventoryValue = 0,
                    MovementClass = DashboardInventoryRiskAggregator.BucketActive,
                    DaysSinceLastFaktur = idleDays,
                    DaysOfSupply = forecastByBrgId.TryGetValue(brgId, out var forecast) ? forecast.Calculation?.DaysOfSupply : null,
                    RecommendedPurchaseQty = forecast?.Calculation?.RecommendedPurchaseQty,
                    DistinctCustomerCount = customerCountByBrgId.TryGetValue(brgId, out var count) ? count : 0,
                    IsTrendEligible = true,
                    IsActive = idleDays <= DashboardInventoryRiskAggregator.SlowMovingDaysThreshold
                };
            }

            return portfolioByBrgId.Values
                .OrderBy(x => x.BrgCode, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static DashboardItemPortfolioRow CreateRow(
            DashboardInventoryItemGroup item,
            IReadOnlyDictionary<string, string> movementByBrgId,
            IReadOnlyDictionary<string, ForecastItemContext> forecastByBrgId,
            IReadOnlyDictionary<string, int> customerCountByBrgId,
            IReadOnlyDictionary<string, SalesmanMtdItemRollupDto> supplierByBrgId,
            IReadOnlyDictionary<string, BrgLastFakturDto> lastFakturByBrgId,
            DateTime today)
        {
            var brgId = item.BrgId.Trim();
            movementByBrgId.TryGetValue(brgId, out var movementClass);
            forecastByBrgId.TryGetValue(brgId, out var forecast);
            supplierByBrgId.TryGetValue(brgId, out var supplier);

            int? daysSinceLastFaktur = forecast?.DaysSinceLastFaktur;
            if (!daysSinceLastFaktur.HasValue &&
                lastFakturByBrgId.TryGetValue(brgId, out var lastFaktur))
            {
                daysSinceLastFaktur = (today.Date - lastFaktur.LastFakturDate.Date).Days;
            }

            if (string.IsNullOrWhiteSpace(movementClass))
            {
                movementClass = forecast?.MovementSignalKey ?? DashboardInventoryRiskAggregator.BucketActive;
            }

            var isTrendEligible = item.Qty > 0 ||
                (daysSinceLastFaktur.HasValue && daysSinceLastFaktur.Value <= TrendEligibilityDays);

            return new DashboardItemPortfolioRow
            {
                BrgId = brgId,
                BrgCode = item.BrgCode?.Trim() ?? brgId,
                BrgName = item.BrgName?.Trim() ?? brgId,
                CategoryName = item.CategoryName,
                SupplierName = item.SupplierName,
                SupplierId = supplier?.SupplierId?.Trim() ?? string.Empty,
                SupplierCode = supplier?.SupplierCode?.Trim() ?? string.Empty,
                Qty = item.Qty,
                InventoryValue = item.InventoryValue,
                MovementClass = movementClass,
                DaysSinceLastFaktur = daysSinceLastFaktur,
                DaysOfSupply = forecast?.Calculation?.DaysOfSupply,
                RecommendedPurchaseQty = forecast?.Calculation?.RecommendedPurchaseQty,
                DistinctCustomerCount = customerCountByBrgId.TryGetValue(brgId, out var count) ? count : 0,
                IsTrendEligible = isTrendEligible,
                IsActive = string.Equals(
                    movementClass,
                    DashboardInventoryRiskAggregator.BucketActive,
                    StringComparison.OrdinalIgnoreCase)
            };
        }

        private static Dictionary<string, string> BuildMovementIndex(DashboardInventoryRiskAggregateResult riskAggregate)
        {
            var index = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (riskAggregate?.AttentionList == null)
                return index;

            foreach (var row in riskAggregate.AttentionList)
            {
                if (string.IsNullOrWhiteSpace(row.BrgId) || string.IsNullOrWhiteSpace(row.SignalKey))
                    continue;

                index[row.BrgId.Trim()] = row.SignalKey.Trim();
            }

            return index;
        }
    }
}
