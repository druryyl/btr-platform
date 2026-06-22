using System;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class CustomerPortfolioLifecycleResolverTest
    {
        private static readonly DateTime BusinessDate = new DateTime(2026, 6, 22);
        private static readonly CustomerPortfolioOptions Options = new CustomerPortfolioOptions();

        [Fact]
        public void Resolve_NeverPurchased_WhenNoHistory()
        {
            var result = CustomerPortfolioLifecycleResolver.Resolve(
                hasPurchaseHistory: false,
                firstPurchaseDate: null,
                lastPurchaseDate: null,
                daysSinceLastFaktur: null,
                isActiveMtd: false,
                isDormant: false,
                forecast: null,
                mtdOmzet: 0,
                priorMonthOmzet: 0,
                BusinessDate,
                Options);

            result.Should().Be(CustomerPortfolioOptimizationPolicy.LifecycleNeverPurchased);
        }

        [Fact]
        public void Resolve_Dormant_UsesM17Threshold()
        {
            var result = CustomerPortfolioLifecycleResolver.Resolve(
                hasPurchaseHistory: true,
                firstPurchaseDate: BusinessDate.AddYears(-2),
                lastPurchaseDate: BusinessDate.AddDays(-100),
                daysSinceLastFaktur: 100,
                isActiveMtd: false,
                isDormant: true,
                forecast: null,
                mtdOmzet: 0,
                priorMonthOmzet: 1_000_000m,
                BusinessDate,
                Options);

            result.Should().Be(CustomerPortfolioOptimizationPolicy.LifecycleDormant);
        }

        [Fact]
        public void Resolve_New_WhenFirstPurchaseWithin90Days()
        {
            var result = CustomerPortfolioLifecycleResolver.Resolve(
                hasPurchaseHistory: true,
                firstPurchaseDate: BusinessDate.AddDays(-30),
                lastPurchaseDate: BusinessDate.AddDays(-5),
                daysSinceLastFaktur: 5,
                isActiveMtd: true,
                isDormant: false,
                forecast: Forecast(CustomerRiskForecastPolicy.CategoryHealthy),
                mtdOmzet: 500_000m,
                priorMonthOmzet: 0,
                BusinessDate,
                Options);

            result.Should().Be(CustomerPortfolioOptimizationPolicy.LifecycleNew);
        }

        [Fact]
        public void Resolve_Declining_WhenM29SevereDeclineSignal()
        {
            var forecast = Forecast(CustomerRiskForecastPolicy.CategoryAttention);
            forecast.Signals.Add(new CustomerRiskSignalContext
            {
                SignalKey = CustomerRiskSignalBuilder.SignalSevereDecline
            });

            var result = CustomerPortfolioLifecycleResolver.Resolve(
                hasPurchaseHistory: true,
                firstPurchaseDate: BusinessDate.AddYears(-1),
                lastPurchaseDate: BusinessDate.AddDays(-10),
                daysSinceLastFaktur: 10,
                isActiveMtd: true,
                isDormant: false,
                forecast,
                mtdOmzet: 500_000m,
                priorMonthOmzet: 2_000_000m,
                BusinessDate,
                Options);

            result.Should().Be(CustomerPortfolioOptimizationPolicy.LifecycleDeclining);
        }

        [Fact]
        public void Resolve_Precedence_DormantBeatsDeclining()
        {
            var forecast = Forecast(CustomerRiskForecastPolicy.CategoryAttention);
            forecast.Signals.Add(new CustomerRiskSignalContext
            {
                SignalKey = CustomerRiskSignalBuilder.SignalSevereDecline
            });

            var result = CustomerPortfolioLifecycleResolver.Resolve(
                hasPurchaseHistory: true,
                firstPurchaseDate: BusinessDate.AddYears(-1),
                lastPurchaseDate: BusinessDate.AddDays(-120),
                daysSinceLastFaktur: 120,
                isActiveMtd: false,
                isDormant: true,
                forecast,
                mtdOmzet: 0,
                priorMonthOmzet: 2_000_000m,
                BusinessDate,
                Options);

            result.Should().Be(CustomerPortfolioOptimizationPolicy.LifecycleDormant);
        }

        private static CustomerRiskForecastContext Forecast(string category) =>
            new CustomerRiskForecastContext { Category = category };
    }
}
