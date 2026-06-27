using btr.application.ReportingContext.Shared;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class PortalMenuRegistryTest
    {
        [Fact]
        public void PortalMenuRegistry_ContainsTwentyFiveEntries()
        {
            PortalMenuRegistry.GetAllLinks().Should().HaveCount(25);
        }

        [Fact]
        public void GetDomainDashboardLinks_ExcludesAlertCenter()
        {
            var links = PortalMenuRegistry.GetDomainDashboardLinks();

            links.Should().HaveCount(19);
            links.Should().NotContain(link => link.Code == PortalMenuRegistry.AlertCenterCode);
        }

        [Theory]
        [InlineData("/reports/sales", "SA03 · Sales Report")]
        [InlineData("/dashboard/customer-portfolio", "CU04 · Customer Portfolio")]
        [InlineData("/dashboard/inventory-risk", "IN02 · Inventory Risk")]
        public void FormatMenuLabel_ReturnsCodeAndLabel(string route, string expected)
        {
            PortalMenuRegistry.FormatMenuLabel(route).Should().Be(expected);
        }

        [Fact]
        public void CollectionOptimization_IsCustomersGroupCode()
        {
            PortalMenuRegistry.FormatMenuLabel("/dashboard/collection-optimization")
                .Should()
                .Be("CU03 · Collection Optimization");
        }
    }
}
