using btr.application.ReportingContext.DashboardSnapshotAgg;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardSnapshotHealthStatusResolverTest
    {
        [Fact]
        public void ResolveOverallStatus_WhenAllDomainsMissing_ReturnsUnknown()
        {
            var status = DashboardSnapshotHealthStatusResolver.ResolveOverallStatus(
                new string[] { null, null, null });

            status.Should().Be("unknown");
        }

        [Fact]
        public void ResolveOverallStatus_WhenAnyFailed_ReturnsDegraded()
        {
            var status = DashboardSnapshotHealthStatusResolver.ResolveOverallStatus(
                new string[] { "Success", "Failed", "Success" });

            status.Should().Be("degraded");
        }

        [Fact]
        public void ResolveOverallStatus_WhenAnyRunning_ReturnsRefreshing()
        {
            var status = DashboardSnapshotHealthStatusResolver.ResolveOverallStatus(
                new string[] { "Success", "Running", null });

            status.Should().Be("refreshing");
        }

        [Fact]
        public void ResolveOverallStatus_WhenAllSuccessful_ReturnsOk()
        {
            var status = DashboardSnapshotHealthStatusResolver.ResolveOverallStatus(
                new string[] { "Success", "Success", "Success" });

            status.Should().Be("ok");
        }

        [Fact]
        public void ResolveOverallStatus_WhenPartialHistory_ReturnsOk()
        {
            var status = DashboardSnapshotHealthStatusResolver.ResolveOverallStatus(
                new string[] { "Success", null, "Success" });

            status.Should().Be("ok");
        }
    }
}
