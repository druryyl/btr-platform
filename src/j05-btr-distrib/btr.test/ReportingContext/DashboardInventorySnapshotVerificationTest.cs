using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.InventoryContext.StokBalanceInfo;
using btr.application.ReportingContext.DashboardInventoryAgg.Queries;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.SupportContext.TglJamAgg;
using btr.infrastructure.ReportingContext.DashboardInventoryAgg;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardInventorySnapshotVerificationTest
    {
        private static readonly DateTime FixedGeneratedAt = new DateTime(2026, 6, 6, 14, 30, 0);

        [Fact]
        public void Aggregator_MatchesLiveDal_ForEquivalentStokBalanceRows()
        {
            var rows = new[]
            {
                Row("BRG001", "Gudang Utama", 100, 10_000m, "Cat A", "Sup A"),
                Row("BRG001", "Gudang Cabang", 50, 10_000m, "Cat A", "Sup A"),
                Row("BRG001", "In-Transit", 25, 10_000m, "Cat A", "Sup A"),
                Row("BRG002", "Gudang Utama", 20, 5_000m, "Cat B", "Sup B"),
                Row("BRG003", "Gudang Utama", 10, 1_000m, null, "Sup C"),
                Row("BRG004", "Gudang Utama", 0, 8_000m, "Cat D", "Sup D"),
            };

            var aggregator = new DashboardInventoryAggregator();
            var aggregate = aggregator.Aggregate(rows, FixedGeneratedAt);

            var liveDal = new DashboardInventoryLiveDal(
                new StubStokBalanceViewDal(rows),
                new StubTglJamDal(FixedGeneratedAt),
                aggregator);

            var live = liveDal.GetSummary();

            aggregate.TotalInventoryValue.Should().Be(live.TotalInventoryValue);
            aggregate.TotalItem.Should().Be(live.TotalItem);

            var aggregateTopCategories = TopItems(aggregate, DashboardInventoryAggregator.DimensionCategory);
            live.TopCategories.Should().HaveCount(aggregateTopCategories.Count);
            for (var i = 0; i < live.TopCategories.Count; i++)
            {
                live.TopCategories[i].Name.Should().Be(aggregateTopCategories[i].Name);
                live.TopCategories[i].InventoryValue.Should().Be(aggregateTopCategories[i].InventoryValue);
                live.TopCategories[i].Rank.Should().Be(aggregateTopCategories[i].Rank);
            }

            live.CategoryBreakdown.Should().BeEquivalentTo(
                live.TopCategories.Select(t => new { t.Name, t.InventoryValue }),
                options => options.ExcludingMissingMembers());

            var aggregateTopSuppliers = TopItems(aggregate, DashboardInventoryAggregator.DimensionSupplier);
            live.TopSuppliers.Should().HaveCount(aggregateTopSuppliers.Count);
            for (var i = 0; i < live.TopSuppliers.Count; i++)
            {
                live.TopSuppliers[i].Name.Should().Be(aggregateTopSuppliers[i].Name);
                live.TopSuppliers[i].InventoryValue.Should().Be(aggregateTopSuppliers[i].InventoryValue);
                live.TopSuppliers[i].Rank.Should().Be(aggregateTopSuppliers[i].Rank);
            }
        }

        private static List<DashboardInventoryRankingItem> TopItems(
            DashboardInventoryAggregateResult aggregate,
            string dimensionType)
        {
            return aggregate.Breakdown
                .Where(r => r.DimensionType == dimensionType && r.IsTop10)
                .OrderBy(r => r.Top10Rank ?? int.MaxValue)
                .Select(r => new DashboardInventoryRankingItem
                {
                    Rank = r.Top10Rank ?? 0,
                    Name = r.Name,
                    InventoryValue = r.InventoryValue
                })
                .ToList();
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

        private sealed class StubStokBalanceViewDal : IStokBalanceViewDal
        {
            private readonly IEnumerable<StokBalanceView> _rows;

            public StubStokBalanceViewDal(IEnumerable<StokBalanceView> rows)
            {
                _rows = rows;
            }

            public IEnumerable<StokBalanceView> ListData() => _rows;
        }

        private sealed class StubTglJamDal : ITglJamDal
        {
            public StubTglJamDal(DateTime now)
            {
                Now = now;
            }

            public DateTime Now { get; }
        }
    }
}
