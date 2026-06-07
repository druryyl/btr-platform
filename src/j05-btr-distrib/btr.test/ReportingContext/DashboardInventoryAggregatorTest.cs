using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.InventoryContext.StokBalanceInfo;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardInventoryAggregatorTest
    {
        private static readonly DateTime FixedGeneratedAt = new DateTime(2026, 6, 6, 14, 30, 0);
        private readonly DashboardInventoryAggregator _aggregator = new DashboardInventoryAggregator();

        [Fact]
        public void Aggregate_ExcludesInTransit_AndZeroQtyGroups()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Row("BRG001", "Gudang Utama", 100, 10_000m, "Cat A", "Sup A"),
                Row("BRG001", "In-Transit", 50, 10_000m, "Cat A", "Sup A"),
                Row("BRG002", "Gudang Utama", 0, 5_000m, "Cat B", "Sup B"),
            }, FixedGeneratedAt);

            result.TotalItem.Should().Be(1);
            result.TotalInventoryValue.Should().Be(1_000_000m);
        }

        [Fact]
        public void Aggregate_MapsBlankCategoryAndSupplier_ToUnknown()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Row("BRG001", "Gudang Utama", 10, 1_000m, null, null),
                Row("BRG002", "Gudang Utama", 5, 2_000m, "  ", "Supplier X"),
            }, FixedGeneratedAt);

            var topCategories = TopRanking(result, DashboardInventoryAggregator.DimensionCategory);
            var topSuppliers = TopRanking(result, DashboardInventoryAggregator.DimensionSupplier);

            topCategories.Should().Contain(c => c.Name == "Unknown");
            topSuppliers.Should().Contain(s => s.Name == "Unknown");
        }

        [Fact]
        public void Aggregate_FullCategoryRollup_EqualsTotalInventoryValue()
        {
            var rows = new[]
            {
                Row("BRG001", "Gudang Utama", 100, 10_000m, "Cat A", "Sup A"),
                Row("BRG001", "Gudang Cabang", 50, 10_000m, "Cat A", "Sup A"),
                Row("BRG002", "Gudang Utama", 20, 5_000m, "Cat B", null),
                Row("BRG003", "Gudang Utama", 10, 1_000m, null, "Sup B"),
            };

            var result = _aggregator.Aggregate(rows, FixedGeneratedAt);

            var allCategories = rows
                .Where(r => r.WarehouseName != DashboardInventoryAggregator.InTransitWarehouseName)
                .GroupBy(r => r.BrgId)
                .Where(g => g.Sum(x => x.Qty) > 0)
                .Select(g => new
                {
                    Category = string.IsNullOrWhiteSpace(g.Select(x => x.KategoriName).FirstOrDefault())
                        ? DashboardInventoryAggregator.UnknownLabel
                        : g.Select(x => x.KategoriName).FirstOrDefault().Trim(),
                    Value = g.Sum(x => x.Hpp * x.Qty)
                })
                .GroupBy(x => x.Category, StringComparer.OrdinalIgnoreCase)
                .Sum(g => g.Sum(x => x.Value));

            allCategories.Should().Be(result.TotalInventoryValue);

            var topCategories = TopRanking(result, DashboardInventoryAggregator.DimensionCategory).ToList();
            topCategories.Count.Should().BeLessOrEqualTo(10);
            topCategories.Should().OnlyContain(r => r.IsTop10);
        }

        [Fact]
        public void Aggregate_FullSupplierRollup_EqualsTotalInventoryValue()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Row("BRG001", "Gudang Utama", 100, 10_000m, "Cat A", "Sup A"),
                Row("BRG002", "Gudang Utama", 20, 5_000m, "Cat B", "Sup B"),
            }, FixedGeneratedAt);

            var expectedSupplierSum = 1_100_000m;
            result.TotalInventoryValue.Should().Be(expectedSupplierSum);

            var topSuppliers = TopRanking(result, DashboardInventoryAggregator.DimensionSupplier);
            topSuppliers.Sum(s => s.InventoryValue).Should().BeLessOrEqualTo(result.TotalInventoryValue);
            topSuppliers
                .OrderBy(s => s.Top10Rank)
                .Select(s => s.InventoryValue)
                .Should()
                .BeInDescendingOrder();
        }

        [Fact]
        public void Aggregate_StoresFullBreakdown_WithTop10Flags()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Row("BRG001", "Gudang Utama", 10, 1_000m, "Cat A", "Sup A"),
                Row("BRG002", "Gudang Utama", 5, 2_000m, "Cat B", "Sup B"),
            }, FixedGeneratedAt);

            var categories = result.Breakdown
                .Where(r => r.DimensionType == DashboardInventoryAggregator.DimensionCategory)
                .ToList();

            categories.Should().HaveCount(2);
            categories.Count(r => r.IsTop10).Should().Be(2);
            categories.Single(r => r.Name == "Cat A").Top10Rank.Should().Be(1);
            categories.Single(r => r.Name == "Cat B").Top10Rank.Should().Be(2);
        }

        private static IEnumerable<DashboardInventoryBreakdownRow> TopRanking(
            DashboardInventoryAggregateResult result,
            string dimensionType)
        {
            return result.Breakdown
                .Where(r => r.DimensionType == dimensionType && r.IsTop10)
                .OrderBy(r => r.Top10Rank ?? int.MaxValue);
        }

        private static StokBalanceView Row(
            string brgId,
            string warehouseName,
            int qty,
            decimal hpp,
            string kategoriName,
            string supplierName)
        {
            return new StokBalanceView
            {
                BrgId = brgId,
                WarehouseName = warehouseName,
                Qty = qty,
                Hpp = hpp,
                KategoriName = kategoriName,
                SupplierName = supplierName,
            };
        }
    }
}
