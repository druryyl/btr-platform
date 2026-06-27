using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using btr.domain.SalesContext.CustomerAgg;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class EntityIdentityIntegrityTest
    {
        [Fact]
        public void CustomerIdentityResolver_MapsCustomerIdAndCode()
        {
            var lookup = EntityAnalyticsCustomerIdentityResolver.BuildLookup(new[]
            {
                new CustomerModel("CUST-42") { CustomerCode = "C042", CustomerName = "Acme" }
            });

            var identity = EntityAnalyticsCustomerIdentityResolver.Resolve("C042", lookup);

            identity.CustomerId.Should().Be("CUST-42");
            identity.CustomerCode.Should().Be("C042");
            identity.CustomerName.Should().Be("Acme");
        }

        [Fact]
        public void CustomerIdentityResolver_UsesExplicitCustomerIdWhenProvided()
        {
            var identity = EntityAnalyticsCustomerIdentityResolver.Resolve(
                "C042",
                lookup: null,
                customerId: "CUST-42",
                customerName: "Acme");

            identity.CustomerId.Should().Be("CUST-42");
            identity.CustomerCode.Should().Be("C042");
            identity.CustomerName.Should().Be("Acme");
        }

        [Fact]
        public void CustomerIdentityResolver_PrefersLookupNameWhenCandidateIsCode()
        {
            var lookup = EntityAnalyticsCustomerIdentityResolver.BuildLookup(new[]
            {
                new CustomerModel("CUST-42") { CustomerCode = "C042", CustomerName = "Acme" }
            });

            var identity = EntityAnalyticsCustomerIdentityResolver.Resolve(
                "C042",
                lookup,
                customerName: "C042");

            identity.CustomerName.Should().Be("Acme");
        }
    }
}
