using System;
using btr.application.ReportingContext.DashboardPiutangAgg.Queries;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using AggregateTopCustomerRiskRow = btr.application.ReportingContext.DashboardSnapshotAgg.Models.DashboardPiutangTopCustomerRiskRow;
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

        [Fact]
        public void GetSummary_MapsV2FieldsAndInvestigation()
        {
            var snapshot = new DashboardPiutangAggregateResult
            {
                TotalPiutang = 1_000_000m,
                TotalCustomer = 3,
                GeneratedAt = SnapshotGeneratedAt,
                OverdueCustomer = 1,
                OverduePiutang = 400_000m,
                AgingOver90Amount = 100_000m,
                AgingOver90Percent = 10m,
                Top10CustomerConcentrationPercent = 35m,
                Top20CustomerConcentrationPercent = 50m,
                TopCustomerRisk = new System.Collections.Generic.List<AggregateTopCustomerRiskRow>
                {
                    new AggregateTopCustomerRiskRow
                    {
                        Rank = 1,
                        CustomerCode = "C001",
                        CustomerName = "Alpha",
                        TotalPiutang = 350_000m,
                        CurrentAmount = 100_000m,
                        Aging30Amount = 50_000m,
                        Aging60Amount = 50_000m,
                        Aging90Amount = 50_000m,
                        AgingOver90Amount = 100_000m
                    }
                }
            };

            var result = CreateDal(snapshot).GetSummary();

            result.OverduePiutang.Should().Be(400_000m);
            result.AgingOver90Amount.Should().Be(100_000m);
            result.AgingOver90Percent.Should().Be(10m);
            result.Top10CustomerConcentrationPercent.Should().Be(35m);
            result.Top20CustomerConcentrationPercent.Should().Be(50m);
            result.TopCustomerRisk.Should().HaveCount(1);
            result.TopCustomerRisk[0].Investigation.Should().NotBeNull();
            result.TopCustomerRisk[0].TotalPiutang.Should().Be(350_000m);
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
