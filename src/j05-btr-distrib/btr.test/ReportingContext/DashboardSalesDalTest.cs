using System;
using System.Collections.Generic;
using btr.application.ReportingContext.DashboardSalesAgg.Queries;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.SalesContext.FakturInfo;
using btr.application.SalesContext.SalesOmzetAgg.Contracts;
using btr.application.SupportContext.TglJamAgg;
using btr.infrastructure.ReportingContext.DashboardSalesAgg;
using btr.nuna.Domain;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardSalesDalTest
    {
        private static readonly DateTime SnapshotGeneratedAt = new DateTime(2026, 6, 6, 8, 0, 0);
        private static readonly DateTime LiveGeneratedAt = new DateTime(2026, 6, 6, 14, 30, 0);

        [Fact]
        public void GetSummary_UsesSnapshot_WhenCurrentExists()
        {
            var snapshot = new DashboardSalesAggregateResult
            {
                PeriodYear = 2026,
                PeriodMonth = 6,
                TotalOmzet = 5_000_000m,
                CompletedOmzet = 5_000_000m,
                PipelineOmzet = 0m,
                TotalFaktur = 10,
                TotalCustomer = 8,
                GeneratedAt = SnapshotGeneratedAt,
                TotalTarget = 6_000_000m,
                TotalAchievement = 5_000_000m,
                AchievementPercent = 83.3m,
                TopSalesman = new List<DashboardSalesTopSalesmanRow>
                {
                    new DashboardSalesTopSalesmanRow
                    {
                        Rank = 1,
                        SalesPersonName = "Alice",
                        CompletedOmzet = 3_000_000m
                    }
                }
            };

            var dal = CreateDal(
                snapshot,
                liveRows: Array.Empty<FakturView>(),
                allowLiveFallback: true);

            var result = dal.GetSummary();

            result.TotalOmzet.Should().Be(5_000_000m);
            result.PipelineOmzet.Should().Be(0m);
            result.GeneratedAt.Should().Be(SnapshotGeneratedAt);
            result.TopSalesmanRanking.Should().ContainSingle(r => r.SalesPersonName == "Alice");
        }

        [Fact]
        public void GetSummary_FallsBackToLive_WhenSnapshotMissingAndFallbackEnabled()
        {
            var dal = CreateDal(
                snapshot: null,
                liveRows: new[]
                {
                    Faktur("FK001", new DateTime(2026, 6, 3), "Alice", "C001", "Customer A", 1_000_000m),
                },
                allowLiveFallback: true,
                totalTarget: 2_000_000m);

            var result = dal.GetSummary();

            result.TotalOmzet.Should().Be(1_000_000m);
            result.GeneratedAt.Should().Be(LiveGeneratedAt);
        }

        [Fact]
        public void GetSummary_Throws_WhenSnapshotMissingAndFallbackDisabled()
        {
            var dal = CreateDal(
                snapshot: null,
                liveRows: new[] { Faktur("FK001", new DateTime(2026, 6, 3), "Alice", "C001", "A", 100m) },
                allowLiveFallback: false);

            Action act = () => dal.GetSummary();

            act.Should().Throw<DashboardSnapshotUnavailableException>()
                .WithMessage("Dashboard data not yet available");
        }

        private static DashboardSalesDal CreateDal(
            DashboardSalesAggregateResult snapshot,
            IEnumerable<FakturView> liveRows,
            bool allowLiveFallback,
            decimal totalTarget = 0m)
        {
            var liveDal = new DashboardSalesLiveDal(
                new StubFakturViewDal(liveRows),
                new StubTargetDal(totalTarget),
                new StubTglJamDal(LiveGeneratedAt),
                new DashboardSalesFakturAggregator());

            return new DashboardSalesDal(
                new StubSnapshotDal(snapshot),
                liveDal,
                Options.Create(new DashboardSnapshotOptions
                {
                    AllowLiveFallback = allowLiveFallback
                }));
        }

        private static FakturView Faktur(
            string code,
            DateTime tgl,
            string salesPerson,
            string customerCode,
            string customer,
            decimal grandTotal)
        {
            return new FakturView
            {
                FakturCode = code,
                Tgl = tgl,
                SalesPersonName = salesPerson,
                CustomerCode = customerCode,
                Customer = customer,
                GrandTotal = grandTotal,
            };
        }

        private sealed class StubSnapshotDal : IDashboardSalesSnapshotDal
        {
            private readonly DashboardSalesAggregateResult _snapshot;

            public StubSnapshotDal(DashboardSalesAggregateResult snapshot)
            {
                _snapshot = snapshot;
            }

            public DashboardSalesAggregateResult GetCurrent() => _snapshot;

            public void ReplaceCurrent(DashboardSalesAggregateResult result, string refreshLogId)
            {
            }
        }

        private sealed class StubFakturViewDal : IFakturViewDal
        {
            private readonly IEnumerable<FakturView> _rows;

            public StubFakturViewDal(IEnumerable<FakturView> rows)
            {
                _rows = rows;
            }

            public IEnumerable<FakturView> ListData(Periode filter) => _rows;

            public IEnumerable<FakturView> ListTerhapus(Periode periode) => Array.Empty<FakturView>();
        }

        private sealed class StubTargetDal : ISalesOmzetTargetDal
        {
            private readonly decimal _totalTarget;

            public StubTargetDal(decimal totalTarget)
            {
                _totalTarget = totalTarget;
            }

            public decimal SumTargetAmountForMonth(int year, int month) => _totalTarget;

            public decimal? GetTargetAmount(string salesPersonId, int year, int month) => null;
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
