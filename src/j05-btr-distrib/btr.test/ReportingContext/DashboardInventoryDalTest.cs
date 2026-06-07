using System;
using System.Collections.Generic;
using btr.application.InventoryContext.StokBalanceInfo;
using btr.application.ReportingContext.DashboardInventoryAgg.Queries;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.SupportContext.TglJamAgg;
using btr.infrastructure.ReportingContext.DashboardInventoryAgg;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardInventoryDalTest
    {
        private static readonly DateTime SnapshotGeneratedAt = new DateTime(2026, 6, 6, 8, 0, 0);
        private static readonly DateTime LiveGeneratedAt = new DateTime(2026, 6, 6, 14, 30, 0);

        [Fact]
        public void GetSummary_UsesSnapshot_WhenCurrentExists()
        {
            var snapshot = new DashboardInventoryAggregateResult
            {
                TotalInventoryValue = 2_500_000m,
                TotalItem = 5,
                GeneratedAt = SnapshotGeneratedAt,
                Breakdown = new List<DashboardInventoryBreakdownRow>
                {
                    new DashboardInventoryBreakdownRow
                    {
                        DimensionType = DashboardInventoryAggregator.DimensionCategory,
                        Name = "Cat A",
                        InventoryValue = 2_500_000m,
                        IsTop10 = true,
                        Top10Rank = 1
                    }
                }
            };

            var dal = CreateDal(
                snapshot,
                liveRows: Array.Empty<StokBalanceView>(),
                allowLiveFallback: true);

            var result = dal.GetSummary();

            result.TotalInventoryValue.Should().Be(2_500_000m);
            result.GeneratedAt.Should().Be(SnapshotGeneratedAt);
            result.TopCategories.Should().ContainSingle(c => c.Name == "Cat A");
        }

        [Fact]
        public void GetSummary_FallsBackToLive_WhenSnapshotMissingAndFallbackEnabled()
        {
            var dal = CreateDal(
                snapshot: null,
                liveRows: new[]
                {
                    Row("BRG001", "Gudang Utama", 100, 10_000m, "Cat A", "Sup A"),
                },
                allowLiveFallback: true);

            var result = dal.GetSummary();

            result.TotalInventoryValue.Should().Be(1_000_000m);
            result.GeneratedAt.Should().Be(LiveGeneratedAt);
        }

        [Fact]
        public void GetSummary_Throws_WhenSnapshotMissingAndFallbackDisabled()
        {
            var dal = CreateDal(
                snapshot: null,
                liveRows: new[] { Row("BRG001", "Gudang Utama", 100, 10_000m, "Cat A", "Sup A") },
                allowLiveFallback: false);

            Action act = () => dal.GetSummary();

            act.Should().Throw<DashboardSnapshotUnavailableException>()
                .WithMessage("Dashboard data not yet available");
        }

        private static DashboardInventoryDal CreateDal(
            DashboardInventoryAggregateResult snapshot,
            IEnumerable<StokBalanceView> liveRows,
            bool allowLiveFallback)
        {
            var liveDal = new DashboardInventoryLiveDal(
                new StubStokBalanceViewDal(liveRows),
                new StubTglJamDal(LiveGeneratedAt),
                new DashboardInventoryAggregator());

            return new DashboardInventoryDal(
                new StubSnapshotDal(snapshot),
                liveDal,
                Options.Create(new DashboardSnapshotOptions
                {
                    AllowLiveFallback = allowLiveFallback
                }));
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

        private sealed class StubSnapshotDal : IDashboardInventorySnapshotDal
        {
            private readonly DashboardInventoryAggregateResult _snapshot;

            public StubSnapshotDal(DashboardInventoryAggregateResult snapshot)
            {
                _snapshot = snapshot;
            }

            public DashboardInventoryAggregateResult GetCurrent() => _snapshot;

            public void ReplaceCurrent(DashboardInventoryAggregateResult result, string refreshLogId)
            {
            }
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
