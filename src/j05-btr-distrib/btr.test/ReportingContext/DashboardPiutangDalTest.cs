using System;
using btr.application.ReportingContext.DashboardPiutangAgg.Queries;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.infrastructure.ReportingContext.DashboardPiutangAgg;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardPiutangDalTest
    {
        private static readonly DateTime SnapshotGeneratedAt = new DateTime(2026, 6, 6, 8, 0, 0);

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

            var dal = CreateDal(snapshot);
            var result = dal.GetSummary();

            result.TotalPiutang.Should().Be(1_000_000m);
            result.GeneratedAt.Should().Be(SnapshotGeneratedAt);
        }

        [Fact]
        public void GetSummary_Throws_WhenSnapshotMissing()
        {
            var dal = CreateDal(snapshot: null);

            Action act = () => dal.GetSummary();

            act.Should().Throw<DashboardSnapshotUnavailableException>()
                .WithMessage("Dashboard data not yet available");
        }

        private static DashboardPiutangDal CreateDal(DashboardPiutangAggregateResult snapshot)
        {
            return new DashboardPiutangDal(new StubSnapshotDal(snapshot));
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
    }
}
