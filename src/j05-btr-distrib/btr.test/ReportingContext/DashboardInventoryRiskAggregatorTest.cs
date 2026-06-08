using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.InventoryContext.StokBalanceInfo;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.SalesContext.FakturInfo;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardInventoryRiskAggregatorTest
    {
        private static readonly DateTime FixedToday = new DateTime(2026, 6, 8);
        private static readonly DateTime FixedGeneratedAt = new DateTime(2026, 6, 8, 14, 30, 0);
        private readonly DashboardInventoryRiskAggregator _aggregator = new DashboardInventoryRiskAggregator();
        private readonly DashboardInventoryAggregator _inventoryAggregator = new DashboardInventoryAggregator();

        [Fact]
        public void Aggregate_BuildItemGroupsParity_MatchesInventoryAggregatorTotalValue()
        {
            var rows = new[]
            {
                Row("BRG001", "G001", "Item A", "Gudang Utama", 100, 10_000m, "Cat A", "Sup A"),
                Row("BRG002", "G002", "Item B", "Gudang Utama", 20, 5_000m, "Cat B", "Sup B"),
            };

            var inventoryResult = _inventoryAggregator.Aggregate(rows, FixedGeneratedAt);
            var riskResult = _aggregator.Aggregate(rows, Enumerable.Empty<BrgLastFakturDto>(), FixedToday, FixedGeneratedAt);

            riskResult.TotalInventoryValue.Should().Be(inventoryResult.TotalInventoryValue);
            riskResult.TotalItem.Should().Be(inventoryResult.TotalItem);
        }

        [Fact]
        public void Aggregate_NeverSold_WhenNoLastFakturHistory()
        {
            var rows = new[] { Row("BRG001", "G001", "Item A", "Gudang Utama", 10, 1_000m, "Cat A", "Sup A") };

            var result = _aggregator.Aggregate(rows, Enumerable.Empty<BrgLastFakturDto>(), FixedToday, FixedGeneratedAt);

            result.NeverSoldItemCount.Should().Be(1);
            result.NeverSoldValue.Should().Be(10_000m);
            result.SlowMovingItemCount.Should().Be(0);
            result.DeadStockItemCount.Should().Be(0);
            result.AttentionList.Single().SignalKey.Should().Be(DashboardInventoryRiskAggregator.SignalNeverSold);
        }

        [Fact]
        public void Aggregate_SlowMovingBoundary_AtExactly90Days()
        {
            var rows = new[] { Row("BRG001", "G001", "Item A", "Gudang Utama", 10, 1_000m, "Cat A", "Sup A") };
            var lastFaktur = new[] { LastFaktur("BRG001", FixedToday.AddDays(-90)) };

            var result = _aggregator.Aggregate(rows, lastFaktur, FixedToday, FixedGeneratedAt);

            result.SlowMovingItemCount.Should().Be(1);
            result.DeadStockItemCount.Should().Be(0);
            result.AttentionList.Single().SignalKey.Should().Be(DashboardInventoryRiskAggregator.SignalSlowMoving);
        }

        [Fact]
        public void Aggregate_DeadStockBoundary_AtExactly180Days()
        {
            var rows = new[] { Row("BRG001", "G001", "Item A", "Gudang Utama", 10, 1_000m, "Cat A", "Sup A") };
            var lastFaktur = new[] { LastFaktur("BRG001", FixedToday.AddDays(-180)) };

            var result = _aggregator.Aggregate(rows, lastFaktur, FixedToday, FixedGeneratedAt);

            result.DeadStockItemCount.Should().Be(1);
            result.SlowMovingItemCount.Should().Be(0);
            result.AttentionList.Single().SignalKey.Should().Be(DashboardInventoryRiskAggregator.SignalDeadStock);
        }

        [Fact]
        public void Aggregate_ActiveExclusion_At89Days()
        {
            var rows = new[] { Row("BRG001", "G001", "Item A", "Gudang Utama", 10, 1_000m, "Cat A", "Sup A") };
            var lastFaktur = new[] { LastFaktur("BRG001", FixedToday.AddDays(-89)) };

            var result = _aggregator.Aggregate(rows, lastFaktur, FixedToday, FixedGeneratedAt);

            result.SlowMovingItemCount.Should().Be(0);
            result.DeadStockItemCount.Should().Be(0);
            result.NeverSoldItemCount.Should().Be(0);
            result.AtRiskInventoryValue.Should().Be(0);
            result.AttentionList.Should().BeEmpty();
            result.RequiresAttention.Should().BeFalse();
        }

        [Fact]
        public void Aggregate_MutualExclusivity_ItemCountsDoNotOverlap()
        {
            var rows = new[]
            {
                Row("BRG001", "G001", "Never", "Gudang Utama", 10, 1_000m, "Cat A", "Sup A"),
                Row("BRG002", "G002", "Slow", "Gudang Utama", 10, 1_000m, "Cat A", "Sup A"),
                Row("BRG003", "G003", "Dead", "Gudang Utama", 10, 1_000m, "Cat A", "Sup A"),
                Row("BRG004", "G004", "Active", "Gudang Utama", 10, 1_000m, "Cat A", "Sup A"),
            };
            var lastFaktur = new[]
            {
                LastFaktur("BRG002", FixedToday.AddDays(-90)),
                LastFaktur("BRG003", FixedToday.AddDays(-180)),
                LastFaktur("BRG004", FixedToday.AddDays(-30)),
            };

            var result = _aggregator.Aggregate(rows, lastFaktur, FixedToday, FixedGeneratedAt);

            result.NeverSoldItemCount.Should().Be(1);
            result.SlowMovingItemCount.Should().Be(1);
            result.DeadStockItemCount.Should().Be(1);
            (result.NeverSoldItemCount + result.SlowMovingItemCount + result.DeadStockItemCount)
                .Should().BeLessOrEqualTo(result.TotalItem);
        }

        [Fact]
        public void Aggregate_AtRiskValue_EqualsSumOfDisjointClasses()
        {
            var rows = new[]
            {
                Row("BRG001", "G001", "Never", "Gudang Utama", 10, 1_000m, "Cat A", "Sup A"),
                Row("BRG002", "G002", "Slow", "Gudang Utama", 20, 1_000m, "Cat A", "Sup A"),
                Row("BRG003", "G003", "Dead", "Gudang Utama", 30, 1_000m, "Cat A", "Sup A"),
            };
            var lastFaktur = new[]
            {
                LastFaktur("BRG002", FixedToday.AddDays(-100)),
                LastFaktur("BRG003", FixedToday.AddDays(-200)),
            };

            var result = _aggregator.Aggregate(rows, lastFaktur, FixedToday, FixedGeneratedAt);

            result.AtRiskInventoryValue.Should().Be(
                result.NeverSoldValue + result.SlowMovingValue + result.DeadStockValue);
        }

        [Fact]
        public void Aggregate_AtRiskPercent_NullWhenTotalInventoryValueIsZero()
        {
            var result = _aggregator.Aggregate(
                Enumerable.Empty<StokBalanceView>(),
                Enumerable.Empty<BrgLastFakturDto>(),
                FixedToday,
                FixedGeneratedAt);

            result.TotalInventoryValue.Should().Be(0);
            result.AtRiskInventoryPercent.Should().BeNull();
        }

        [Fact]
        public void Aggregate_ExcludesInTransitWarehouse()
        {
            var rows = new[]
            {
                Row("BRG001", "G001", "Item A", "Gudang Utama", 100, 10_000m, "Cat A", "Sup A"),
                Row("BRG001", "G001", "Item A", "In-Transit", 50, 10_000m, "Cat A", "Sup A"),
            };

            var result = _aggregator.Aggregate(rows, Enumerable.Empty<BrgLastFakturDto>(), FixedToday, FixedGeneratedAt);

            result.TotalItem.Should().Be(1);
            result.NeverSoldValue.Should().Be(1_000_000m);
        }

        [Fact]
        public void Aggregate_ExcludesZeroQtyGroups()
        {
            var rows = new[]
            {
                Row("BRG001", "G001", "Item A", "Gudang Utama", 0, 10_000m, "Cat A", "Sup A"),
            };

            var result = _aggregator.Aggregate(rows, Enumerable.Empty<BrgLastFakturDto>(), FixedToday, FixedGeneratedAt);

            result.TotalItem.Should().Be(0);
            result.RequiresAttention.Should().BeFalse();
        }

        [Fact]
        public void Aggregate_Top10Cap_LimitsRankings()
        {
            var rows = Enumerable.Range(1, 15)
                .Select(i => Row(
                    $"BRG{i:D3}",
                    $"G{i:D3}",
                    $"Item {i}",
                    "Gudang Utama",
                    10,
                    1_000m,
                    "Cat A",
                    "Sup A"))
                .ToArray();
            var lastFaktur = rows
                .Select(r => LastFaktur(r.BrgId, FixedToday.AddDays(-200)))
                .ToArray();

            var result = _aggregator.Aggregate(rows, lastFaktur, FixedToday, FixedGeneratedAt);

            result.TopDead.Count.Should().Be(10);
        }

        [Fact]
        public void Aggregate_AttentionList_OneRowPerAtRiskItemWithCorrectSignal()
        {
            var rows = new[]
            {
                Row("BRG001", "G001", "Never", "Gudang Utama", 10, 1_000m, "Cat A", "Sup A"),
                Row("BRG002", "G002", "Slow", "Gudang Utama", 10, 1_000m, "Cat A", "Sup A"),
            };
            var lastFaktur = new[] { LastFaktur("BRG002", FixedToday.AddDays(-95)) };

            var result = _aggregator.Aggregate(rows, lastFaktur, FixedToday, FixedGeneratedAt);

            result.AttentionList.Should().HaveCount(2);
            result.AttentionList[0].SignalKey.Should().Be(DashboardInventoryRiskAggregator.SignalSlowMoving);
            result.AttentionList[1].SignalKey.Should().Be(DashboardInventoryRiskAggregator.SignalNeverSold);
        }

        private static BrgLastFakturDto LastFaktur(string brgId, DateTime lastFakturDate) =>
            new BrgLastFakturDto
            {
                BrgId = brgId,
                BrgCode = brgId,
                BrgName = brgId,
                LastFakturDate = lastFakturDate
            };

        private static StokBalanceView Row(
            string brgId,
            string brgCode,
            string brgName,
            string warehouseName,
            int qty,
            decimal hpp,
            string kategoriName,
            string supplierName)
        {
            return new StokBalanceView
            {
                BrgId = brgId,
                BrgCode = brgCode,
                BrgName = brgName,
                WarehouseName = warehouseName,
                Qty = qty,
                Hpp = hpp,
                KategoriName = kategoriName,
                SupplierName = supplierName,
            };
        }
    }
}
