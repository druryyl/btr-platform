using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class CustomerPortfolioActionBuilderTest
    {
        [Fact]
        public void BuildReasonText_IncludesTierAndLifecycle()
        {
            var text = CustomerPortfolioActionBuilder.BuildReasonText(
                CustomerPortfolioOptimizationPolicy.LifecycleGrowing,
                CustomerPortfolioOptimizationPolicy.TierHighValue,
                CustomerRiskForecastPolicy.CategoryHealthy,
                CustomerPortfolioOptimizationPolicy.ActionGrow);

            text.Should().Contain("Grow");
            text.Should().Contain("Growing");
            text.Should().Contain("High Value");
            text.Should().Contain("Healthy");
        }

        [Fact]
        public void ResolvePrimaryAction_Collect_IncludesM30Link()
        {
            var action = CustomerPortfolioActionBuilder.ResolvePrimaryAction(
                CustomerPortfolioOptimizationPolicy.LifecycleMature,
                CustomerPortfolioOptimizationPolicy.TierStrategic,
                CustomerRiskForecastPolicy.CategoryAttention,
                new CustomerRiskForecastContext { CustomerKey = "C001", CustomerName = "Alpha" },
                new CollectionOptimizationContext
                {
                    ActionCategoryKey = CollectionOptimizationPolicy.ActionImmediateCollection,
                    TriggeredRuleIds = "COL-OPT-CAT-01"
                },
                hasM17PlafondBreach: false,
                m29RecommendationKey: string.Empty);

            action.PrimaryActionKey.Should().Be(CustomerPortfolioOptimizationPolicy.ActionCollect);
            action.M30LinkRoute.Should().Contain("customerKey=C001");
            action.TriggeredRuleIds.Should().Contain("CPO-41");
        }

        [Fact]
        public void ResolvePrimaryAction_Dormant_IncludesTriggeredRuleIds()
        {
            var action = CustomerPortfolioActionBuilder.ResolvePrimaryAction(
                CustomerPortfolioOptimizationPolicy.LifecycleDormant,
                CustomerPortfolioOptimizationPolicy.TierMediumValue,
                CustomerRiskForecastPolicy.CategoryHealthy,
                new CustomerRiskForecastContext { CustomerKey = "C002", CustomerName = "Beta" },
                optimization: null,
                hasM17PlafondBreach: false,
                m29RecommendationKey: string.Empty);

            action.PrimaryActionKey.Should().Be(CustomerPortfolioOptimizationPolicy.ActionRecover);
            action.TriggeredRuleIds.Should().Be(DashboardCustomerAggregator.SignalDormant);
        }
    }
}
