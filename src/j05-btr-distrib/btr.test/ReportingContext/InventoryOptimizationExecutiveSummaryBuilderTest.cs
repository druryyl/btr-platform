using System;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class InventoryOptimizationExecutiveSummaryBuilderTest
    {
        [Fact]
        public void Build_IncludesActionCountsAndBudget()
        {
            var result = new DashboardInventoryOptimizationAggregateResult
            {
                RequiredPurchaseBudgetIdr = 10_000_000m,
                DelayCount = 3,
                TransferCount = 2,
                PostFirstCount = 1,
                DeferCount = 4,
                ClearanceCount = 5,
                RecoverableCapitalIdr = 2_000_000m,
                TopActionSummary = "Purchase Item A (Critical)"
            };

            var summary = InventoryOptimizationExecutiveSummaryBuilder.Build(
                new DateTime(2026, 6, 15),
                result);

            summary.Should().Contain("Delay purchasing for 3 products");
            summary.Should().Contain("Transfer inventory for 2 warehouse pairs");
            summary.Should().Contain("Purchase Item A");
        }
    }
}
