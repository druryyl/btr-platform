using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class EntityComparisonCalculatorTest
    {
        [Fact]
        public void ComputeDelta_BothValues_ReturnsDifference()
        {
            EntityComparisonCalculator.ComputeDelta(120m, 100m).Should().Be(20m);
        }

        [Fact]
        public void ComputeDelta_MissingValue_ReturnsNull()
        {
            EntityComparisonCalculator.ComputeDelta(120m, null).Should().BeNull();
            EntityComparisonCalculator.ComputeDelta(null, 100m).Should().BeNull();
        }

        [Fact]
        public void ComputeGrowthPercent_PositivePrior_ReturnsPercent()
        {
            EntityComparisonCalculator.ComputeGrowthPercent(120m, 100m).Should().Be(20m);
        }

        [Fact]
        public void ComputeGrowthPercent_ZeroPrior_ReturnsNull()
        {
            EntityComparisonCalculator.ComputeGrowthPercent(120m, 0m).Should().BeNull();
        }

        [Fact]
        public void ComputeGrowthPercent_NegativePrior_UsesAbsoluteDenominator()
        {
            EntityComparisonCalculator.ComputeGrowthPercent(50m, -100m).Should().Be(150m);
        }

        [Fact]
        public void ShiftMonth_CrossesYearBoundary()
        {
            EntityComparisonCalculator.ShiftMonth(2026, 1, -1).Should().Be((2025, 12));
        }
    }
}
