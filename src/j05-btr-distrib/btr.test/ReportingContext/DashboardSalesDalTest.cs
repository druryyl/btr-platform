using System;
using System.Collections.Generic;
using btr.application.ReportingContext.DashboardSalesAgg.Queries;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.infrastructure.ReportingContext.DashboardSalesAgg;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardSalesDalTest
    {
        private static readonly DateTime SnapshotGeneratedAt = new DateTime(2026, 6, 6, 8, 0, 0);

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

            var dal = CreateDal(snapshot);
            var result = dal.GetSummary();

            result.TotalOmzet.Should().Be(5_000_000m);
            result.PipelineOmzet.Should().Be(0m);
            result.GeneratedAt.Should().Be(SnapshotGeneratedAt);
            result.TopSalesmanRanking.Should().ContainSingle(r => r.SalesPersonName == "Alice");
        }

        [Fact]
        public void GetSummary_Throws_WhenSnapshotMissing()
        {
            var dal = CreateDal(snapshot: null);

            Action act = () => dal.GetSummary();

            act.Should().Throw<DashboardSnapshotUnavailableException>()
                .WithMessage("Dashboard data not yet available");
        }

        private static DashboardSalesDal CreateDal(DashboardSalesAggregateResult snapshot)
        {
            return new DashboardSalesDal(new StubSnapshotDal(snapshot));
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
    }
}
