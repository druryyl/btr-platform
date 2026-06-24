using System;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class InventoryOptimizationPolicyTest
    {
        private static readonly DateTime BusinessDate = new DateTime(2026, 6, 15);

        [Fact]
        public void ComputePriorityScore_IncludesCategoryWeightAndStrategicBoost()
        {
            var score = InventoryOptimizationPolicy.ComputePriorityScore(
                InventoryOptimizationPolicy.CategoryCritical,
                5_000_000m,
                5m,
                7,
                isStrategicItem: true,
                InventoryOptimizationPolicy.ActionPurchase);

            score.Should().BeGreaterOrEqualTo(1000);
        }

        [Fact]
        public void ResolveCategory_PurchaseCritical_WhenDosLow()
        {
            var category = InventoryOptimizationPolicy.ResolveCategory(
                InventoryOptimizationPolicy.ActionPurchase,
                5m,
                BusinessDate.AddDays(3),
                BusinessDate,
                DashboardInventoryRiskAggregator.BucketActive,
                1_000_000m,
                500_000m);

            category.Should().Be(InventoryOptimizationPolicy.CategoryCritical);
        }

        [Fact]
        public void ApplyBudgetCap_DeferPurchasesBeyondCap()
        {
            var purchases = new[]
            {
                new PurchaseRecommendationContext
                {
                    ActionType = InventoryOptimizationPolicy.ActionPurchase,
                    ImpactValueIdr = 60_000_000m,
                    PriorityScore = 2000,
                    Category = InventoryOptimizationPolicy.CategoryCritical
                },
                new PurchaseRecommendationContext
                {
                    ActionType = InventoryOptimizationPolicy.ActionPurchase,
                    ImpactValueIdr = 50_000_000m,
                    PriorityScore = 1500,
                    Category = InventoryOptimizationPolicy.CategoryHigh
                }
            };

            var result = InventoryOptimizationPolicy.ApplyBudgetCap(purchases, 80_000_000m).ToList();

            result.Should().HaveCount(2);
            result[1].ActionType.Should().Be(InventoryOptimizationPolicy.ActionDefer);
        }

        [Fact]
        public void ComputeTransferQty_ReturnsPositiveWhenGapExists()
        {
            var qty = InventoryOptimizationPolicy.ComputeTransferQty(500m, 5m, 10m, 10m, 7);
            qty.Should().BeGreaterThan(0m);
        }

        [Fact]
        public void IsDoNotReorder_WhenDeadStock_ReturnsTrue()
        {
            var ctx = new ForecastItemContext
            {
                Item = new DashboardInventoryItemGroup { BrgId = "B1" },
                MovementSignalKey = DashboardInventoryRiskAggregator.SignalDeadStock
            };

            InventoryOptimizationPolicy.IsDoNotReorder(ctx).Should().BeTrue();
        }
    }
}
