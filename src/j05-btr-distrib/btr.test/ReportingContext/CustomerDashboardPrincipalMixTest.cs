using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.DashboardCustomerAgg;
using btr.application.ReportingContext.DashboardCustomerAgg.Contracts;
using btr.application.ReportingContext.DashboardCustomerAgg.Queries;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;
using btr.infrastructure.ReportingContext.PrincipalAnalyticsAgg;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class CustomerDashboardPrincipalMixTest
    {
        [Fact]
        public void Compose_ListsOnlyProjectionPrincipals_AndDoesNotAllocateCustomerTotals()
        {
            var customer = CustomerResponse(
                omzet: Ranking("C001", "C-001", "Alpha", 500m),
                piutang: Ranking("C002", "C-002", "Beta", 900m));
            var projection = Projection(
                Pair("C001", "Alpha", "SUPA", "Principal A", 300m),
                Pair("C001", "Alpha", "SUPB", "Principal B", 100m),
                Pair("OTHER", "Other", "SUPC", "Other Principal", 999m));

            var mix = CustomerDashboardPrincipalMixComposer.Compose(customer, projection);

            mix.IsAvailable.Should().BeTrue();
            mix.KpiId.Should().Be(PrincipalKpiCatalog.SalesOutId);
            mix.Customers.Select(row => row.CustomerId).Should().Equal("C001", "C002");

            var alpha = mix.Customers.Single(row => row.CustomerId == "C001");
            alpha.Principals.Select(row => row.SupplierId).Should().Equal("SUPA", "SUPB");
            alpha.Principals.Select(row => row.PairSalesOutAmount).Should().Equal(300m, 100m);
            alpha.Principals.Should().OnlyContain(row => row.KpiId == PrincipalKpiCatalog.SalesOutId);
            alpha.Principals.Should().OnlyContain(row => row.PairSalesOutAmount != 500m);
            alpha.Principals.Sum(row => row.PairSalesOutAmount).Should().Be(400m);
            alpha.Principals.Single(row => row.SupplierId == "SUPA").PercentOfPairSalesOut.Should().Be(75m);
            alpha.Principals.Should().NotContain(row => row.SupplierId == "SUPC");

            var beta = mix.Customers.Single(row => row.CustomerId == "C002");
            beta.Principals.Should().BeEmpty();
            beta.Principals.Should().NotContain(row => row.PairSalesOutAmount == 900m);

            customer.Rankings.TopOmzet[0].Amount.Should().Be(500m);
            customer.Rankings.TopPiutang[0].Amount.Should().Be(900m);
            mix.Disclosures.Should().Contain(CustomerDashboardPrincipalMixComposer.CustomerLevelNote);
            mix.Disclosures.Should().Contain(CustomerDashboardPrincipalMixComposer.ProjectionOnlyPrincipalsNote);
        }

        [Fact]
        public void Compose_DoesNotReadAPrincipalAbsentFromThatCustomerProjection()
        {
            var customer = CustomerResponse(omzet: Ranking("C001", "C-001", "Alpha", 100m));
            var projection = Projection(
                Pair("C001", "Alpha", "SUPA", "Principal A", 40m));

            var mix = CustomerDashboardPrincipalMixComposer.Compose(customer, projection);

            mix.Customers.Single().Principals.Select(row => row.SupplierId).Should().Equal("SUPA");
        }

        [Fact]
        public void Compose_IsUnavailable_WhenProjectionIsMissing_AndLeavesCustomerTotalsUnchanged()
        {
            var customer = CustomerResponse(omzet: Ranking("C001", "C-001", "Alpha", 500m));

            var mix = CustomerDashboardPrincipalMixComposer.Compose(customer, projection: null);

            mix.IsAvailable.Should().BeFalse();
            mix.Customers.Should().BeEmpty();
            customer.Rankings.TopOmzet[0].Amount.Should().Be(500m);
        }

        [Fact]
        public void ListPairsForCustomers_ReadsProjectionOnly()
        {
            var sql = CustomerPrincipalRelationshipDal.ListPairsForCustomersSql;

            sql.Should().Contain("BTRPD_CustomerPrincipalRelationship");
            sql.Should().Contain("CustomerId IN @CustomerIds");
            sql.Should().Contain("SalesOutAmount");
            sql.Should().NotContain("BTR_Faktur");
            sql.Should().NotContain("BTR_FakturItem");
            sql.Should().NotContain("BTR_Brg");
            sql.Should().NotContain("BTR_SalesPersonSupplier");
            sql.Should().NotContain("Retur");
            sql.Should().NotContain("PRN-CUS");
            sql.Should().NotContain("PRN-RET");
            sql.Should().NotContain("TOP ");
        }

        [Fact]
        public async Task Handle_AttachesProjectionMix_WithoutChangingCustomerTotals()
        {
            var dal = new StubCustomerDal(CustomerResponse(
                omzet: Ranking("C001", "C-001", "Alpha", 500m)));
            var relationshipDal = new StubRelationshipDal(Projection(
                Pair("C001", "Alpha", "SUPA", "Principal A", 80m)));
            var handler = new GetDashboardCustomerHandler(dal, relationshipDal);

            var result = await handler.Handle(new GetDashboardCustomerQuery(), CancellationToken.None);

            result.Rankings.TopOmzet[0].Amount.Should().Be(500m);
            result.PrincipalMix.IsAvailable.Should().BeTrue();
            result.PrincipalMix.Customers.Single().Principals.Single().PairSalesOutAmount.Should().Be(80m);
            relationshipDal.RequestedCustomerIds.Should().Equal("C001");
        }

        private static DashboardCustomerResponse CustomerResponse(
            DashboardCustomerRankingRow omzet = null,
            DashboardCustomerRankingRow piutang = null)
        {
            return new DashboardCustomerResponse
            {
                IsAvailable = true,
                Rankings = new DashboardCustomerRankings
                {
                    TopOmzet = omzet == null
                        ? new List<DashboardCustomerRankingRow>()
                        : new List<DashboardCustomerRankingRow> { omzet },
                    TopPiutang = piutang == null
                        ? new List<DashboardCustomerRankingRow>()
                        : new List<DashboardCustomerRankingRow> { piutang }
                }
            };
        }

        private static DashboardCustomerRankingRow Ranking(
            string customerId,
            string customerCode,
            string customerName,
            decimal amount)
        {
            return new DashboardCustomerRankingRow
            {
                CustomerId = customerId,
                CustomerCode = customerCode,
                CustomerName = customerName,
                Amount = amount
            };
        }

        private static CustomerPrincipalRelationshipResult Projection(
            params CustomerPrincipalRelationshipRow[] pairs)
        {
            return new CustomerPrincipalRelationshipResult
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                Pairs = pairs.ToList()
            };
        }

        private static CustomerPrincipalRelationshipRow Pair(
            string customerId,
            string customerName,
            string supplierId,
            string supplierName,
            decimal salesOutAmount)
        {
            return new CustomerPrincipalRelationshipRow
            {
                CustomerId = customerId,
                CustomerName = customerName,
                SupplierId = supplierId,
                SupplierName = supplierName,
                KpiId = PrincipalKpiCatalog.SalesOutId,
                SalesOutAmount = salesOutAmount
            };
        }

        private sealed class StubCustomerDal : IDashboardCustomerDal
        {
            private readonly DashboardCustomerResponse _response;

            public StubCustomerDal(DashboardCustomerResponse response)
            {
                _response = response;
            }

            public DashboardCustomerResponse GetSummary() => _response;
        }

        private sealed class StubRelationshipDal : ICustomerPrincipalRelationshipDal
        {
            private readonly CustomerPrincipalRelationshipResult _projection;

            public StubRelationshipDal(CustomerPrincipalRelationshipResult projection)
            {
                _projection = projection;
            }

            public IList<string> RequestedCustomerIds { get; private set; }
                = new List<string>();

            public CustomerPrincipalRelationshipResult GetProjection() => _projection;

            public CustomerPrincipalRelationshipResult ListPairsForCustomers(IEnumerable<string> customerIds)
            {
                RequestedCustomerIds = (customerIds ?? Enumerable.Empty<string>()).ToList();
                return _projection;
            }

            public CustomerPrincipalRelationshipResult ListPairsForCustomerCodes(IEnumerable<string> customerCodes)
            {
                return _projection;
            }

            public void ReplaceProjection(CustomerPrincipalRelationshipResult result, string refreshLogId)
            {
            }
        }
    }
}
