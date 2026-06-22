using System.Collections.Generic;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class CustomerPortfolioOptimizationPolicyTest
    {
        [Fact]
        public void ResolvePrimaryAction_M30Actionable_ReturnsCollect()
        {
            var optimization = new CollectionOptimizationContext
            {
                ActionCategoryKey = CollectionOptimizationPolicy.ActionImmediateCollection
            };

            var action = CustomerPortfolioActionBuilder.ResolvePrimaryAction(
                CustomerPortfolioOptimizationPolicy.LifecycleMature,
                CustomerPortfolioOptimizationPolicy.TierStrategic,
                CustomerRiskForecastPolicy.CategoryWatch,
                new CustomerRiskForecastContext { CustomerKey = "C001", CustomerName = "Alpha" },
                optimization,
                hasM17PlafondBreach: false,
                m29RecommendationKey: string.Empty);

            action.PrimaryActionKey.Should().Be(CustomerPortfolioOptimizationPolicy.ActionCollect);
            action.M30LinkRoute.Should().Contain("/dashboard/collection-optimization");
        }

        [Fact]
        public void ResolvePrimaryAction_PlafondBreachWithoutM30Collect_ReturnsReviewCredit()
        {
            var action = CustomerPortfolioActionBuilder.ResolvePrimaryAction(
                CustomerPortfolioOptimizationPolicy.LifecycleMature,
                CustomerPortfolioOptimizationPolicy.TierHighValue,
                CustomerRiskForecastPolicy.CategoryHealthy,
                new CustomerRiskForecastContext { CustomerKey = "C002", CustomerName = "Beta" },
                optimization: null,
                hasM17PlafondBreach: true,
                m29RecommendationKey: string.Empty);

            action.PrimaryActionKey.Should().Be(CustomerPortfolioOptimizationPolicy.ActionReviewCredit);
        }

        [Fact]
        public void ResolvePrimaryAction_StrategicWatch_ReturnsProtect()
        {
            var action = CustomerPortfolioActionBuilder.ResolvePrimaryAction(
                CustomerPortfolioOptimizationPolicy.LifecycleMature,
                CustomerPortfolioOptimizationPolicy.TierStrategic,
                CustomerRiskForecastPolicy.CategoryWatch,
                new CustomerRiskForecastContext { CustomerKey = "C003", CustomerName = "Gamma" },
                optimization: null,
                hasM17PlafondBreach: false,
                m29RecommendationKey: string.Empty);

            action.PrimaryActionKey.Should().Be(CustomerPortfolioOptimizationPolicy.ActionProtect);
        }

        [Fact]
        public void ResolvePrimaryAction_LowTierCritical_ReturnsExitReview()
        {
            var action = CustomerPortfolioActionBuilder.ResolvePrimaryAction(
                CustomerPortfolioOptimizationPolicy.LifecycleDeclining,
                CustomerPortfolioOptimizationPolicy.TierLowValue,
                CustomerRiskForecastPolicy.CategoryCritical,
                new CustomerRiskForecastContext { CustomerKey = "C004", CustomerName = "Delta" },
                optimization: null,
                hasM17PlafondBreach: false,
                m29RecommendationKey: string.Empty);

            action.PrimaryActionKey.Should().Be(CustomerPortfolioOptimizationPolicy.ActionExitReview);
        }

        [Fact]
        public void QualifiesForAttention_NonMonitorAction_ReturnsTrue()
        {
            var ctx = new PortfolioCustomerContext
            {
                CustomerKey = "C005",
                PrimaryActionKey = CustomerPortfolioOptimizationPolicy.ActionGrow,
                LifecycleStage = CustomerPortfolioOptimizationPolicy.LifecycleMature,
                PortfolioTier = CustomerPortfolioOptimizationPolicy.TierMediumValue,
                M29Category = CustomerRiskForecastPolicy.CategoryHealthy
            };

            CustomerPortfolioOptimizationPolicy.QualifiesForAttention(ctx, new HashSet<string>())
                .Should().BeTrue();
        }

        [Fact]
        public void ComputePortfolioPriorityScore_CollectOutranksMonitor()
        {
            var collect = new PortfolioCustomerContext
            {
                PrimaryActionKey = CustomerPortfolioOptimizationPolicy.ActionCollect,
                PortfolioTier = CustomerPortfolioOptimizationPolicy.TierStrategic,
                M29Category = CustomerRiskForecastPolicy.CategoryCritical,
                OpenBalance = 10_000_000m,
                MtdOmzet = 10_000_000m
            };

            var monitor = new PortfolioCustomerContext
            {
                PrimaryActionKey = CustomerPortfolioOptimizationPolicy.ActionMonitor,
                PortfolioTier = CustomerPortfolioOptimizationPolicy.TierLowValue,
                M29Category = CustomerRiskForecastPolicy.CategoryHealthy,
                OpenBalance = 0m,
                MtdOmzet = 0m
            };

            CustomerPortfolioOptimizationPolicy.ComputePortfolioPriorityScore(collect)
                .Should().BeGreaterThan(
                    CustomerPortfolioOptimizationPolicy.ComputePortfolioPriorityScore(monitor));
        }
    }
}
