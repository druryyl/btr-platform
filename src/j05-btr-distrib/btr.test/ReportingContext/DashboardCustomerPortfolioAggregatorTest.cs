using System;
using System.Collections.Generic;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.domain.SalesContext.CustomerAgg;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardCustomerPortfolioAggregatorTest
    {
        private static readonly DateTime FixedToday = new DateTime(2026, 6, 22);

        [Fact]
        public void Aggregate_SyntheticPortfolio_PopulatesKpiAndDistributions()
        {
            var forecastContexts = new List<CustomerRiskForecastContext>
            {
                Forecast("C001", "Alpha Corp", CustomerRiskForecastPolicy.CategoryCritical, 10_000_000m),
                Forecast("C002", "Beta Ltd", CustomerRiskForecastPolicy.CategoryHealthy, 0m)
            };

            var forecastAggregate = new DashboardCustomerRiskForecastAggregateResult
            {
                Kpi = new DashboardCustomerRiskForecastKpiSnapshot
                {
                    HealthyCount = 1,
                    PortfolioHealthScore = 72.5m
                }
            };

            var customerAggregate = new DashboardCustomerAggregateResult
            {
                TopOmzet = new List<DashboardCustomerTopOmzetRow>
                {
                    new DashboardCustomerTopOmzetRow
                    {
                        Rank = 1,
                        CustomerCode = "C001",
                        CustomerName = "Alpha Corp",
                        OmzetAmount = 10_000_000m,
                        PercentOfTotal = 66.7m
                    }
                }
            };

            var customers = new List<CustomerModel>
            {
                new CustomerModel { CustomerCode = "C001", CustomerName = "Alpha Corp", WilayahName = "Jakarta", KlasifikasiName = "A" },
                new CustomerModel { CustomerCode = "C002", CustomerName = "Beta Ltd", WilayahName = "Bandung", KlasifikasiName = "B" }
            };

            var aggregator = new DashboardCustomerPortfolioAggregator();
            var result = aggregator.Aggregate(
                customerAggregate,
                forecastContexts,
                forecastAggregate,
                new Dictionary<string, CollectionOptimizationContext>(),
                salesmanSnapshot: null,
                customers,
                lastFakturWithSalesman: Array.Empty<btr.application.SalesContext.FakturInfo.CustomerLastFakturWithSalesmanDto>(),
                firstFakturRows: Array.Empty<btr.application.SalesContext.FakturInfo.CustomerFirstFakturDto>(),
                frequencyRows: Array.Empty<btr.application.SalesContext.FakturInfo.CustomerPurchaseFrequencyDto>(),
                currentMonthFakturRows: Array.Empty<btr.application.SalesContext.FakturInfo.FakturView>(),
                piutangRows: Array.Empty<PiutangOpenBalanceDto>(),
                FixedToday,
                FixedToday,
                new CustomerPortfolioOptions());

            result.Kpi.TotalCustomerCount.Should().Be(2);
            result.Kpi.AttentionCustomerCount.Should().BeGreaterThan(0);
            result.LifecycleDistribution.Should().NotBeEmpty();
            result.TierDistribution.Should().NotBeEmpty();
            result.ActionDistribution.Should().NotBeEmpty();
            result.Customers.Should().HaveCount(2);
            result.PriorityQueue.Should().NotBeEmpty();
            result.Kpi.AttentionCustomerCount.Should().Be(
                result.Customers.FindAll(c => c.IsAttention).Count);
        }

        private static CustomerRiskForecastContext Forecast(
            string code,
            string name,
            string category,
            decimal overdue)
        {
            return new CustomerRiskForecastContext
            {
                CustomerKey = code,
                CustomerCode = code,
                CustomerName = name,
                Category = category,
                OverdueBalance = overdue,
                OpenBalance = overdue + 1_000_000m,
                WilayahName = "Jakarta",
                SalesPersonName = "Rep A"
            };
        }
    }
}
