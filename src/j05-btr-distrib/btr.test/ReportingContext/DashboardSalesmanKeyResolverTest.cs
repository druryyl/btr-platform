using System.Collections.Generic;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.domain.SalesContext.SalesPersonAgg;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardSalesmanKeyResolverTest
    {
        [Fact]
        public void ResolveSalesPersonId_PrefersIdWhenPresent()
        {
            DashboardSalesmanKeyResolver.ResolveSalesPersonId("SP001", "Alpha")
                .Should().Be("SP001");
        }

        [Fact]
        public void ResolveSalesPersonId_ReturnsEmptyWhenIdBlank()
        {
            DashboardSalesmanKeyResolver.ResolveSalesPersonId("", "Alpha")
                .Should().BeEmpty();
        }

        [Fact]
        public void ResolveId_FallsBackToNameMapWhenIdBlank()
        {
            var lookup = DashboardSalesmanKeyResolver.BuildLookup(new[]
            {
                SalesPerson("SP001", "S001", "Alpha Rep"),
            });

            DashboardSalesmanKeyResolver.ResolveId("", "Alpha Rep", lookup)
                .Should().Be("SP001");
        }

        [Fact]
        public void ResolveId_ReturnsEmptyWhenNoMatch()
        {
            var lookup = DashboardSalesmanKeyResolver.BuildLookup(new[]
            {
                SalesPerson("SP001", "S001", "Alpha Rep"),
            });

            DashboardSalesmanKeyResolver.ResolveId("", "Unknown", lookup)
                .Should().BeEmpty();
        }

        private static SalesPersonModel SalesPerson(string id, string code, string name)
        {
            return new SalesPersonModel
            {
                SalesPersonId = id,
                SalesPersonCode = code,
                SalesPersonName = name
            };
        }
    }
}
