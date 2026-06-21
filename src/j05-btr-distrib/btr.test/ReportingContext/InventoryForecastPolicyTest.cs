using System;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class InventoryForecastPolicyTest
    {
        private static readonly DateTime BusinessDate = new DateTime(2026, 6, 15);

        [Fact]
        public void ComputeAdc30_DividesSoldQtyBy30()
        {
            InventoryForecastPolicy.ComputeAdc30(300m).Should().Be(10m);
        }

        [Fact]
        public void ComputeDaysOfSupply_WhenAdcPositive_ReturnsQtyOverAdc()
        {
            InventoryForecastPolicy.ComputeDaysOfSupply(100m, 10m).Should().Be(10m);
        }

        [Fact]
        public void ComputeDaysOfSupply_WhenAdcZero_ReturnsNull()
        {
            InventoryForecastPolicy.ComputeDaysOfSupply(100m, 0m).Should().BeNull();
        }

        [Fact]
        public void ComputeItem_StandardCase_CalculatesStockOutAndReorder()
        {
            var result = InventoryForecastPolicy.ComputeItem(
                currentQty: 100m,
                unitHpp: 1_000m,
                soldQty30: 300m,
                soldQty90: 900m,
                planningHorizonDays: 30,
                defaultLeadTimeDays: 7,
                coverageDays: 14,
                businessDate: BusinessDate);

            result.AdcUsed.Should().Be(10m);
            result.DaysOfSupply.Should().Be(10m);
            result.ProjectedStockOutDate.Should().Be(BusinessDate.AddDays(10));
            result.ReorderDate.Should().Be(BusinessDate.AddDays(3));
            result.RecommendedPurchaseQty.Should().Be(110m);
            result.ForecastQtyAtHorizon.Should().Be(0m);
            result.ForecastValueAtHorizon.Should().Be(0m);
            result.PurchaseUrgency.Should().Be(InventoryForecastPolicy.UrgencyHigh);
        }

        [Fact]
        public void ComputeBestCaseAdc_UsesMinimumPace()
        {
            InventoryForecastPolicy.ComputeBestCaseAdc(10m, 8m).Should().Be(8m);
            InventoryForecastPolicy.ComputeWorstCaseAdc(10m, 8m).Should().Be(10m);
        }

        [Fact]
        public void ComputeHealthScore_SubtractsWeightedPenalties()
        {
            var score = InventoryForecastPolicy.ComputeHealthScore(10m, 10m, 10m);
            score.Should().Be(90);
        }

        [Theory]
        [InlineData(5, InventoryForecastPolicy.SeverityCritical)]
        [InlineData(10, InventoryForecastPolicy.SeverityWarning)]
        [InlineData(20, InventoryForecastPolicy.SeverityNormal)]
        public void ResolveDosSeverity_UsesConfiguredBands(decimal dos, string expected)
        {
            InventoryForecastPolicy.ResolveDosSeverity(dos).Should().Be(expected);
        }

        [Fact]
        public void ResolveConfidence_LowWhenSparseCompanyConsumption()
        {
            InventoryForecastPolicy.ResolveConfidence(30, 0m, 10)
                .Should().Be(InventoryForecastPolicy.ConfidenceLow);
        }
    }
}
