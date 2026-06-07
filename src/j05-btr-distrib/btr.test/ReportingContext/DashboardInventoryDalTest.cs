using System;
using System.Collections.Generic;
using btr.application.ReportingContext.DashboardInventoryAgg.Queries;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.infrastructure.ReportingContext.DashboardInventoryAgg;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardInventoryDalTest
    {
        private static readonly DateTime SnapshotGeneratedAt = new DateTime(2026, 6, 6, 8, 0, 0);

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

            var dal = CreateDal(snapshot);
            var result = dal.GetSummary();

            result.TotalInventoryValue.Should().Be(2_500_000m);
            result.GeneratedAt.Should().Be(SnapshotGeneratedAt);
            result.TopCategories.Should().ContainSingle(c => c.Name == "Cat A");
        }

        [Fact]
        public void GetSummary_Throws_WhenSnapshotMissing()
        {
            var dal = CreateDal(snapshot: null);

            Action act = () => dal.GetSummary();

            act.Should().Throw<DashboardSnapshotUnavailableException>()
                .WithMessage("Dashboard data not yet available");
        }

        private static DashboardInventoryDal CreateDal(DashboardInventoryAggregateResult snapshot)
        {
            return new DashboardInventoryDal(new StubSnapshotDal(snapshot));
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
    }
}
