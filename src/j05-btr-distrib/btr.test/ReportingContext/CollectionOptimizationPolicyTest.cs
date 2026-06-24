using System.Collections.Generic;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class CollectionOptimizationPolicyTest
    {
        [Fact]
        public void ResolveActionCategory_OverdueCritical_ReturnsImmediateCollection()
        {
            var forecast = Forecast(
                CustomerRiskForecastPolicy.CategoryCritical,
                overdueBalance: 1_000_000m);

            CollectionOptimizationPolicy.ResolveActionCategory(
                    forecast,
                    new CollectionOptimizationEnrichment { HasChronicOverdue = true },
                    CustomerRiskRecommendationBuilder.ManagementReview,
                    500_000m)
                .Should().Be(CollectionOptimizationPolicy.ActionImmediateCollection);
        }

        [Fact]
        public void ResolveActionCategory_SalesRecovery_LowOverdue_ReturnsSalesRecoveryVisit()
        {
            var forecast = Forecast(
                CustomerRiskForecastPolicy.CategoryWatch,
                overdueBalance: 0m);

            CollectionOptimizationPolicy.ResolveActionCategory(
                    forecast,
                    new CollectionOptimizationEnrichment(),
                    CustomerRiskRecommendationBuilder.SalesRecovery,
                    500_000m)
                .Should().Be(CollectionOptimizationPolicy.ActionSalesRecoveryVisit);
        }

        [Fact]
        public void ResolveActionCategory_SalesRecovery_HighOverdue_BlockedByImmediate()
        {
            var forecast = Forecast(
                CustomerRiskForecastPolicy.CategoryHighRisk,
                overdueBalance: 2_000_000m);

            CollectionOptimizationPolicy.ResolveActionCategory(
                    forecast,
                    new CollectionOptimizationEnrichment(),
                    CustomerRiskRecommendationBuilder.SalesRecovery,
                    500_000m)
                .Should().Be(CollectionOptimizationPolicy.ActionImmediateCollection);
        }

        [Fact]
        public void ResolveActionCategory_ProactiveReminder_CurrentDueSoonWatch()
        {
            var forecast = Forecast(
                CustomerRiskForecastPolicy.CategoryWatch,
                overdueBalance: 0m,
                minDaysUntilDue: 10);

            CollectionOptimizationPolicy.ResolveActionCategory(
                    forecast,
                    new CollectionOptimizationEnrichment(),
                    CustomerRiskRecommendationBuilder.NoAction,
                    500_000m)
                .Should().Be(CollectionOptimizationPolicy.ActionProactiveReminder);
        }

        [Fact]
        public void ResolveActionCategory_Defer_HealthyDueLater()
        {
            var forecast = Forecast(
                CustomerRiskForecastPolicy.CategoryHealthy,
                overdueBalance: 0m,
                minDaysUntilDue: 20);

            CollectionOptimizationPolicy.ResolveActionCategory(
                    forecast,
                    new CollectionOptimizationEnrichment(),
                    CustomerRiskRecommendationBuilder.NoAction,
                    500_000m)
                .Should().Be(CollectionOptimizationPolicy.ActionDeferCollection);
        }

        [Fact]
        public void ResolveActionCategory_StrategicWatch_ReturnsRelationshipMonitor()
        {
            var forecast = Forecast(
                CustomerRiskForecastPolicy.CategoryWatch,
                overdueBalance: 0m,
                minDaysUntilDue: 20);

            CollectionOptimizationPolicy.ResolveActionCategory(
                    forecast,
                    new CollectionOptimizationEnrichment { IsTop10MtdOmzet = true },
                    CustomerRiskRecommendationBuilder.NoAction,
                    500_000m)
                .Should().Be(CollectionOptimizationPolicy.ActionRelationshipMonitor);
        }

        [Fact]
        public void ComputeCollectionPriorityScore_HighImpactRanksAboveLow()
        {
            var forecast = Forecast(CustomerRiskForecastPolicy.CategoryHighRisk, 50_000_000m);
            var enrichment = new CollectionOptimizationEnrichment();

            var high = CollectionOptimizationPolicy.ComputeCollectionPriorityScore(
                CollectionOptimizationPolicy.ActionImmediateCollection,
                forecast,
                enrichment,
                50_000_000m);
            var low = CollectionOptimizationPolicy.ComputeCollectionPriorityScore(
                CollectionOptimizationPolicy.ActionImmediateCollection,
                forecast,
                enrichment,
                100_000m);

            high.Should().BeGreaterThan(low);
        }

        [Fact]
        public void ComputeCollectionImpactAmount_SumsOverdueAndDue7d()
        {
            CollectionOptimizationPolicy.ComputeCollectionImpactAmount(5_000_000m, 2_000_000m)
                .Should().Be(7_000_000m);
        }

        [Fact]
        public void ResolveActionOwner_SalesRecovery_ReturnsSales()
        {
            CollectionOptimizationPolicy.ResolveActionOwner(CollectionOptimizationPolicy.ActionSalesRecoveryVisit)
                .Should().Be(CollectionOptimizationPolicy.OwnerSales);
        }

        [Fact]
        public void ResolveActionOwner_CreditReview_ReturnsFinance()
        {
            CollectionOptimizationPolicy.ResolveActionOwner(CollectionOptimizationPolicy.ActionCreditReview)
                .Should().Be(CollectionOptimizationPolicy.OwnerFinance);
        }

        private static CustomerRiskForecastContext Forecast(
            string category,
            decimal overdueBalance = 0m,
            int minDaysUntilDue = -1)
        {
            return new CustomerRiskForecastContext
            {
                Category = category,
                OverdueBalance = overdueBalance,
                MinDaysUntilDue = minDaysUntilDue,
                OpenBalance = overdueBalance + 1_000_000m,
                HasChronicOverdue = overdueBalance > 0,
                Signals = new List<CustomerRiskSignalContext>()
            };
        }
    }
}
