using System;
using System.Collections.Generic;
using btr.application.ReportingContext.DashboardInventoryRiskAgg.Queries;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.infrastructure.ReportingContext.DashboardInventoryRiskAgg;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardInventoryRiskDalTest
    {
        private static readonly DateTime SnapshotGeneratedAt = new DateTime(2026, 6, 6, 8, 0, 0);

        [Fact]
        public void GetSummary_SupplierRiskExposure_NavigatesToPrincipalPerformance()
        {
            var snapshot = new DashboardInventoryRiskAggregateResult
            {
                GeneratedAt = SnapshotGeneratedAt,
                TotalInventoryValue = 1_000_000m,
                Breakdown = new List<DashboardInventoryRiskBreakdownRow>
                {
                    new DashboardInventoryRiskBreakdownRow
                    {
                        DimensionType = DashboardInventoryRiskAggregator.DimensionSupplier,
                        Name = "Sup A",
                        SupplierId = "S-01",
                        AtRiskValue = 500_000m,
                        ItemCount = 3,
                        Rank = 1,
                        PercentOfAtRisk = 50m
                    }
                }
            };

            var result = CreateDal(snapshot).GetSummary();

            result.IsAvailable.Should().BeTrue();

            var item = result.SupplierRiskExposure.Should().ContainSingle(x => x.Name == "Sup A").Subject;
            item.SupplierId.Should().Be("S-01");
            item.DashboardRoute.Should().Be("/dashboard/principal-performance");
        }

        [Fact]
        public void GetSummary_CategoryRiskExposure_DoesNotExposeSupplierNavigation()
        {
            var snapshot = new DashboardInventoryRiskAggregateResult
            {
                GeneratedAt = SnapshotGeneratedAt,
                TotalInventoryValue = 1_000_000m,
                Breakdown = new List<DashboardInventoryRiskBreakdownRow>
                {
                    new DashboardInventoryRiskBreakdownRow
                    {
                        DimensionType = DashboardInventoryRiskAggregator.DimensionCategory,
                        Name = "Cat A",
                        AtRiskValue = 500_000m,
                        ItemCount = 3,
                        Rank = 1,
                        PercentOfAtRisk = 50m
                    }
                }
            };

            var result = CreateDal(snapshot).GetSummary();

            var item = result.CategoryRiskExposure.Should().ContainSingle(x => x.Name == "Cat A").Subject;
            item.SupplierId.Should().Be(string.Empty);
            item.DashboardRoute.Should().BeNull();
        }

        private static DashboardInventoryRiskDal CreateDal(DashboardInventoryRiskAggregateResult snapshot)
        {
            return new DashboardInventoryRiskDal(
                new StubSnapshotDal(snapshot),
                Options.Create(new DashboardSnapshotOptions()));
        }

        private sealed class StubSnapshotDal : IDashboardInventoryRiskSnapshotDal
        {
            private readonly DashboardInventoryRiskAggregateResult _snapshot;

            public StubSnapshotDal(DashboardInventoryRiskAggregateResult snapshot)
            {
                _snapshot = snapshot;
            }

            public DashboardInventoryRiskAggregateResult GetCurrent() => _snapshot;

            public void ReplaceCurrent(DashboardInventoryRiskAggregateResult result, string refreshLogId)
            {
            }

            public void ReplaceCurrent(
                DashboardInventoryRiskAggregateResult result,
                DashboardInventoryForecastAggregateResult forecast,
                string refreshLogId)
            {
            }

            public void ReplaceCurrent(
                DashboardInventoryRiskAggregateResult result,
                DashboardInventoryForecastAggregateResult forecast,
                DashboardInventoryOptimizationAggregateResult optimization,
                string refreshLogId)
            {
            }
        }
    }
}