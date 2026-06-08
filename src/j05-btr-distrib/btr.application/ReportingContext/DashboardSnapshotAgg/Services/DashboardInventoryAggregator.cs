using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.InventoryContext.StokBalanceInfo;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public class DashboardInventoryAggregator
    {
        public const string InTransitWarehouseName = DashboardInventoryItemGroupBuilder.InTransitWarehouseName;
        public const string UnknownLabel = DashboardInventoryItemGroupBuilder.UnknownLabel;
        public const string DimensionCategory = "Category";
        public const string DimensionSupplier = "Supplier";

        public DashboardInventoryAggregateResult Aggregate(
            IEnumerable<StokBalanceView> rows,
            DateTime generatedAt)
        {
            var itemGroups = DashboardInventoryItemGroupBuilder.BuildItemGroups(rows);
            var categoryRollup = BuildDimensionRollup(itemGroups, g => g.CategoryName);
            var supplierRollup = BuildDimensionRollup(itemGroups, g => g.SupplierName);
            var topCategories = BuildTop10(categoryRollup);
            var topSuppliers = BuildTop10(supplierRollup);

            var breakdown = new List<DashboardInventoryBreakdownRow>();
            breakdown.AddRange(MapBreakdownRows(categoryRollup, topCategories, DimensionCategory));
            breakdown.AddRange(MapBreakdownRows(supplierRollup, topSuppliers, DimensionSupplier));

            return new DashboardInventoryAggregateResult
            {
                TotalInventoryValue = itemGroups.Sum(x => x.InventoryValue),
                TotalItem = itemGroups.Count,
                GeneratedAt = generatedAt,
                Breakdown = breakdown
            };
        }

        private static List<(string Name, decimal InventoryValue)> BuildDimensionRollup(
            List<DashboardInventoryItemGroup> itemGroups,
            Func<DashboardInventoryItemGroup, string> dimensionSelector)
        {
            return itemGroups
                .GroupBy(dimensionSelector, StringComparer.OrdinalIgnoreCase)
                .Select(g => (Name: g.Key, InventoryValue: g.Sum(x => x.InventoryValue)))
                .OrderByDescending(x => x.InventoryValue)
                .ThenBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static List<(string Name, decimal InventoryValue, int Rank)> BuildTop10(
            List<(string Name, decimal InventoryValue)> rollup)
        {
            return rollup
                .Take(10)
                .Select((x, index) => (x.Name, x.InventoryValue, Rank: index + 1))
                .ToList();
        }

        private static IEnumerable<DashboardInventoryBreakdownRow> MapBreakdownRows(
            List<(string Name, decimal InventoryValue)> rollup,
            List<(string Name, decimal InventoryValue, int Rank)> top10,
            string dimensionType)
        {
            var top10Lookup = top10.ToDictionary(
                x => x.Name,
                x => x.Rank,
                StringComparer.OrdinalIgnoreCase);

            return rollup.Select(row => new DashboardInventoryBreakdownRow
            {
                DimensionType = dimensionType,
                Name = row.Name,
                InventoryValue = row.InventoryValue,
                IsTop10 = top10Lookup.ContainsKey(row.Name),
                Top10Rank = top10Lookup.TryGetValue(row.Name, out var rank) ? rank : (int?)null
            });
        }
    }
}
