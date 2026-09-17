using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class EntityPeerDistributionSummaryTest
    {
        [Fact]
        public void BuildDistributionSummary_NoOverflow_ReturnsNull()
        {
            EntityPeerDistributionEngine.BuildDistributionSummary(
                    "Supplier", 20, 0, 0, null, null)
                .Should().BeNull();
        }

        [Fact]
        public void BuildDistributionSummary_Overflow_ReportsBulkAndOutlier()
        {
            var summary = EntityPeerDistributionEngine.BuildDistributionSummary(
                "Supplier", 20, 0, 1, null, "200");

            summary.Should().Be("19 of 20 suppliers below 200. 1 supplier exceeds 200.");
        }

        [Fact]
        public void BuildDistributionSummary_Underflow_ReportsBulkAndOutlier()
        {
            var summary = EntityPeerDistributionEngine.BuildDistributionSummary(
                "Supplier", 20, 1, 0, "50", null);

            summary.Should().Be("19 of 20 suppliers above 50. 1 supplier below 50.");
        }

        [Fact]
        public void BuildDistributionSummary_BothTails_UsesBetween()
        {
            var summary = EntityPeerDistributionEngine.BuildDistributionSummary(
                "Customer", 20, 1, 2, "50", "200");

            summary.Should().Be(
                "17 of 20 customers between 50 and 200. " +
                "2 customers exceed 200. " +
                "1 customer below 50.");
        }
    }
}
