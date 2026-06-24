using System;
using System.Collections.Generic;
using btr.application.ReportingContext.DashboardCustomerAgg.Queries;
using btr.application.ReportingContext.DashboardSnapshotAgg;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.infrastructure.ReportingContext.DashboardCustomerAgg;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardCustomerDalTest
    {
        private static readonly DateTime SnapshotGeneratedAt = new DateTime(2026, 6, 6, 8, 0, 0);

        [Fact]
        public void GetSummary_MapsSnapshot_WhenCurrentExists()
        {
            var snapshot = new DashboardCustomerAggregateResult
            {
                GeneratedAt = SnapshotGeneratedAt,
                PeriodYear = 2026,
                PeriodMonth = 6,
                OverdueCustomerCount = 2,
                ActiveCustomerCount = 10,
                TopOmzet = new List<DashboardCustomerTopOmzetRow>
                {
                    new DashboardCustomerTopOmzetRow
                    {
                        Rank = 1,
                        CustomerCode = "C001",
                        CustomerName = "Alpha",
                        OmzetAmount = 500m,
                        PercentOfTotal = 50m
                    }
                }
            };

            var dal = CreateDal(snapshot);
            var result = dal.GetSummary();

            result.IsAvailable.Should().BeTrue();
            result.GeneratedAt.Should().Be(SnapshotGeneratedAt);
            result.AttentionCards.OverdueCustomerCount.Should().Be(2);
            result.AttentionCards.ActiveCustomerCount.Should().Be(10);
            result.Rankings.TopOmzet.Should().HaveCount(1);
            result.Rankings.TopOmzet[0].CustomerCode.Should().Be("C001");
            result.Rankings.TopOmzet[0].ReportRoute.Should().Be("/reports/sales");
            result.Navigation.SalesDashboardRoute.Should().Be("/dashboard/sales");
            result.Navigation.PiutangDashboardRoute.Should().Be("/dashboard/piutang");
        }

        [Fact]
        public void GetSummary_ReturnsUnavailable_WhenSnapshotMissing()
        {
            var dal = CreateDal(snapshot: null);
            var result = dal.GetSummary();

            result.IsAvailable.Should().BeFalse();
            result.IsDataFresh.Should().BeFalse();
            result.AttentionCards.Should().BeNull();
            result.Navigation.Should().NotBeNull();
            result.Navigation.SalesDashboardRoute.Should().Be("/dashboard/sales");
        }

        [Fact]
        public void GetSummary_MarksStale_WhenGeneratedAtExceedsInterval()
        {
            var snapshot = new DashboardCustomerAggregateResult
            {
                GeneratedAt = DateTime.UtcNow.AddHours(-2),
                PeriodYear = 2026,
                PeriodMonth = 6
            };

            var dal = CreateDal(snapshot, customerIntervalMinutes: 30);
            var result = dal.GetSummary();

            result.IsAvailable.Should().BeTrue();
            result.IsDataFresh.Should().BeFalse();
        }

        private static DashboardCustomerDal CreateDal(
            DashboardCustomerAggregateResult snapshot,
            int customerIntervalMinutes = 30)
        {
            var options = Options.Create(new DashboardSnapshotOptions
            {
                CustomerIntervalMinutes = customerIntervalMinutes
            });

            return new DashboardCustomerDal(new StubSnapshotDal(snapshot), options);
        }

        private sealed class StubSnapshotDal : IDashboardCustomerSnapshotDal
        {
            private readonly DashboardCustomerAggregateResult _snapshot;

            public StubSnapshotDal(DashboardCustomerAggregateResult snapshot)
            {
                _snapshot = snapshot;
            }

            public DashboardCustomerAggregateResult GetCurrent() => _snapshot;

            public void ReplaceCurrent(DashboardCustomerAggregateResult result, string refreshLogId)
            {
            }

            public void ReplaceCurrent(
                DashboardCustomerAggregateResult result,
                DashboardCustomerRiskForecastAggregateResult forecast,
                string refreshLogId)
            {
            }

            public void ReplaceCurrent(
                DashboardCustomerAggregateResult result,
                DashboardCustomerRiskForecastAggregateResult forecast,
                DashboardCollectionOptimizationAggregateResult optimization,
                string refreshLogId)
            {
            }

            public void ReplaceCurrent(
                DashboardCustomerAggregateResult result,
                DashboardCustomerRiskForecastAggregateResult forecast,
                DashboardCollectionOptimizationAggregateResult optimization,
                DashboardCustomerPortfolioAggregateResult portfolio,
                string refreshLogId)
            {
            }
        }
    }
}
