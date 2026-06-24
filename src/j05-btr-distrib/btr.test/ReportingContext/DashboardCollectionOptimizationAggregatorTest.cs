using System;
using System.Collections.Generic;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardCollectionOptimizationAggregatorTest
    {
        private static readonly DateTime FixedToday = new DateTime(2026, 6, 22);

        [Fact]
        public void Aggregate_SyntheticPortfolio_PopulatesKpiAndPriorityQueue()
        {
            var result = RunAggregate(
                new List<CustomerRiskForecastContext>
                {
                    Context("C001", "Alpha Corp", CustomerRiskForecastPolicy.CategoryCritical, 10_000_000m, 0),
                    Context("C002", "Beta Ltd", CustomerRiskForecastPolicy.CategoryWatch, 0m, 7),
                    Context("C003", "Gamma Inc", CustomerRiskForecastPolicy.CategoryHealthy, 0m, 30)
                },
                collectionSnapshot: new DashboardCollectionAggregateResult
                {
                    OverdueExposure = 10_000_000m,
                    RecoveryVsBillingPercent = 72.5m
                });

            result.Kpi.ActionsTodayCount.Should().BeGreaterThan(0);
            result.PriorityQueue.Should().NotBeEmpty();
            result.Kpi.RecoveryVsBillingPercent.Should().Be(72.5m);
            result.PriorityQueue[0].SortOrder.Should().Be(1);
        }

        [Fact]
        public void Aggregate_M20Null_GracefulDegrade()
        {
            var result = RunAggregate(
                new List<CustomerRiskForecastContext>
                {
                    Context("C001", "Alpha Corp", CustomerRiskForecastPolicy.CategoryHighRisk, 5_000_000m, 0)
                },
                collectionSnapshot: null);

            result.Kpi.CollectionContextUnavailable.Should().BeTrue();
            result.Kpi.RecoveryVsBillingPercent.Should().BeNull();
            result.Kpi.ExecutiveSummaryText.Should().Contain("Collection context unavailable");
        }

        [Fact]
        public void Aggregate_PriorityCap_Enforced()
        {
            var contexts = new List<CustomerRiskForecastContext>();
            for (var i = 0; i < 40; i++)
            {
                contexts.Add(Context($"C{i:000}", $"Customer {i}", CustomerRiskForecastPolicy.CategoryAttention, 1_000_000m, 0));
            }

            var result = RunAggregate(contexts, maxPriorityRows: 30);

            result.PriorityQueue.Should().HaveCount(30);
        }

        [Fact]
        public void Aggregate_TopPriority_MatchesRankOne()
        {
            var result = RunAggregate(
                new List<CustomerRiskForecastContext>
                {
                    Context("C001", "Small Overdue", CustomerRiskForecastPolicy.CategoryAttention, 100_000m, 0),
                    Context("C002", "Large Critical", CustomerRiskForecastPolicy.CategoryCritical, 50_000_000m, 0)
                });

            result.PriorityQueue[0].CustomerCode.Should().Be("C002");
            result.Kpi.ExecutiveSummaryText.Should().Contain("Large Critical");
        }

        private static DashboardCollectionOptimizationAggregateResult RunAggregate(
            List<CustomerRiskForecastContext> contexts,
            DashboardCollectionAggregateResult collectionSnapshot = null,
            int maxPriorityRows = 30)
        {
            var aggregator = new DashboardCollectionOptimizationAggregator();
            var forecastAggregate = new DashboardCustomerRiskForecastAggregateResult
            {
                DaysElapsed = 15,
                BusinessDate = FixedToday
            };

            return aggregator.Aggregate(
                contexts,
                forecastAggregate,
                collectionSnapshot,
                Array.Empty<PiutangOpenBalanceDto>(),
                Array.Empty<btr.application.SalesContext.FakturInfo.FakturView>(),
                Array.Empty<btr.domain.SalesContext.CustomerAgg.CustomerModel>(),
                FixedToday,
                FixedToday,
                new CollectionOptimizationOptions { MaxPriorityRows = maxPriorityRows });
        }

        private static CustomerRiskForecastContext Context(
            string code,
            string name,
            string category,
            decimal overdue,
            int minDaysUntilDue)
        {
            return new CustomerRiskForecastContext
            {
                CustomerKey = code,
                CustomerCode = code,
                CustomerName = name,
                Category = category,
                OverdueBalance = overdue,
                OpenBalance = overdue + 500_000m,
                MinDaysUntilDue = minDaysUntilDue,
                HasChronicOverdue = overdue > 0,
                WilayahName = "Jakarta",
                SalesPersonName = "Rep A",
                Signals = overdue > 0
                    ? new List<CustomerRiskSignalContext>
                    {
                        new CustomerRiskSignalContext
                        {
                            SignalKey = CustomerRiskSignalBuilder.SignalChronicTrajectory,
                            Severity = CustomerRiskForecastPolicy.SeverityStrong,
                            RuleId = "CRF-L03"
                        }
                    }
                    : new List<CustomerRiskSignalContext>()
            };
        }
    }
}
