using System;
using System.Collections.Generic;
using btr.application.FinanceContext.PiutangAgg.Contracts;
using btr.application.ReportingContext.DashboardPiutangAgg.Queries;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.SupportContext.TglJamAgg;
using btr.infrastructure.ReportingContext.DashboardPiutangAgg;
using btr.nuna.Domain;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardPiutangDalTest
    {
        private static readonly DateTime SnapshotGeneratedAt = new DateTime(2026, 6, 6, 8, 0, 0);
        private static readonly DateTime LiveGeneratedAt = new DateTime(2026, 6, 6, 14, 30, 0);

        [Fact]
        public void GetSummary_UsesSnapshot_WhenCurrentExists()
        {
            var snapshot = new DashboardPiutangAggregateResult
            {
                TotalPiutang = 1_000_000m,
                TotalCustomer = 3,
                GeneratedAt = SnapshotGeneratedAt,
                OverdueCustomer = 1
            };

            var dal = CreateDal(
                snapshot,
                liveRows: Array.Empty<PiutangSalesWilayahDto>(),
                allowLiveFallback: true);

            var result = dal.GetSummary();

            result.TotalPiutang.Should().Be(1_000_000m);
            result.GeneratedAt.Should().Be(SnapshotGeneratedAt);
        }

        [Fact]
        public void GetSummary_FallsBackToLive_WhenSnapshotMissingAndFallbackEnabled()
        {
            var dal = CreateDal(
                snapshot: null,
                liveRows: new[]
                {
                    LiveRow("C001", "Alpha", 750_000m),
                },
                allowLiveFallback: true);

            var result = dal.GetSummary();

            result.TotalPiutang.Should().Be(750_000m);
            result.GeneratedAt.Should().Be(LiveGeneratedAt);
        }

        [Fact]
        public void GetSummary_Throws_WhenSnapshotMissingAndFallbackDisabled()
        {
            var dal = CreateDal(
                snapshot: null,
                liveRows: new[] { LiveRow("C001", "Alpha", 750_000m) },
                allowLiveFallback: false);

            Action act = () => dal.GetSummary();

            act.Should().Throw<DashboardSnapshotUnavailableException>()
                .WithMessage("Dashboard data not yet available");
        }

        private static DashboardPiutangDal CreateDal(
            DashboardPiutangAggregateResult snapshot,
            IEnumerable<PiutangSalesWilayahDto> liveRows,
            bool allowLiveFallback)
        {
            var liveDal = new DashboardPiutangLiveDal(
                new StubPiutangSalesWilayahDal(liveRows),
                new StubTglJamDal(LiveGeneratedAt));

            return new DashboardPiutangDal(
                new StubSnapshotDal(snapshot),
                liveDal,
                Options.Create(new DashboardSnapshotOptions
                {
                    AllowLiveFallback = allowLiveFallback
                }));
        }

        private static PiutangSalesWilayahDto LiveRow(
            string customerCode,
            string customerName,
            decimal kurangBayar)
        {
            return new PiutangSalesWilayahDto
            {
                CustomerCode = customerCode,
                CustomerName = customerName,
                JatuhTempo = LiveGeneratedAt.Date.AddDays(30),
                KurangBayar = kurangBayar
            };
        }

        private sealed class StubSnapshotDal : IDashboardPiutangSnapshotDal
        {
            private readonly DashboardPiutangAggregateResult _snapshot;

            public StubSnapshotDal(DashboardPiutangAggregateResult snapshot)
            {
                _snapshot = snapshot;
            }

            public DashboardPiutangAggregateResult GetCurrent() => _snapshot;

            public void ReplaceCurrent(DashboardPiutangAggregateResult result, string refreshLogId)
            {
            }
        }

        private sealed class StubPiutangSalesWilayahDal : IPiutangSalesWilayahDal
        {
            private readonly IEnumerable<PiutangSalesWilayahDto> _rows;

            public StubPiutangSalesWilayahDal(IEnumerable<PiutangSalesWilayahDto> rows)
            {
                _rows = rows;
            }

            public IEnumerable<PiutangSalesWilayahDto> ListData(Periode periode) => _rows;
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
