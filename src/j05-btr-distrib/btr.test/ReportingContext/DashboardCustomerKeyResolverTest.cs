using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardCustomerKeyResolverTest
    {
        [Fact]
        public void ResolveCodeFirst_PrefersCustomerCode()
        {
            DashboardCustomerKeyResolver.ResolveCodeFirst("C001", "Alpha")
                .Should().Be("C001");
        }

        [Fact]
        public void ResolveCodeFirst_FallsBackToName_WhenCodeEmpty()
        {
            DashboardCustomerKeyResolver.ResolveCodeFirst("", "Alpha")
                .Should().Be("Alpha");
            DashboardCustomerKeyResolver.ResolveCodeFirst(null, "Alpha")
                .Should().Be("Alpha");
        }

        [Fact]
        public void ResolveCodeFirst_TrimsWhitespace()
        {
            DashboardCustomerKeyResolver.ResolveCodeFirst("  C001  ", " Alpha ")
                .Should().Be("C001");
            DashboardCustomerKeyResolver.ResolveCodeFirst("  ", " Alpha ")
                .Should().Be("Alpha");
        }
    }
}
