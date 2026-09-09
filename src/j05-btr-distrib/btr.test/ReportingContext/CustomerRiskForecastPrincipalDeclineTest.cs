using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using btr.application.ReportingContext.DashboardCustomerRiskForecastAgg;
using btr.application.ReportingContext.DashboardCustomerRiskForecastAgg.Contracts;
using btr.application.ReportingContext.DashboardCustomerRiskForecastAgg.Queries;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;
using btr.infrastructure.ReportingContext.PrincipalAnalyticsAgg;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class CustomerRiskForecastPrincipalDeclineTest
    {
        [Fact]
        public void Compose_ListsProjectionPairs_WithStoredStatusAndPairSalesOut()
        {
            var forecast = Forecast(Customer("C0001", "CLANDYS JAMAL", 100m));
            var projection = Projection(
                Pair("CS010B", "C0001", "SUPA", "Principal A", CustomerPrincipalRelationship.StatusActive, 300m),
                Pair("CS010B", "C0001", "SUPB", "Principal B", CustomerPrincipalRelationship.StatusDormant, 100m),
                Pair("OTHER", "OTHER", "SUPC", "Other Principal", CustomerPrincipalRelationship.StatusActive, 999m));

            var decline = CustomerRiskForecastPrincipalDeclineComposer.Compose(forecast, projection);

            decline.IsAvailable.Should().BeTrue();
            decline.KpiId.Should().Be(PrincipalKpiCatalog.SalesOutId);
            decline.Customers.Select(row => row.CustomerCode).Should().Equal("C0001");

            var customer = decline.Customers.Single();
            customer.Principals.Select(row => row.SupplierId).Should().Equal("SUPA", "SUPB");
            customer.Principals.Select(row => row.RelationshipStatus).Should().Equal(
                CustomerPrincipalRelationship.StatusActive,
                CustomerPrincipalRelationship.StatusDormant);
            customer.Principals.Select(row => row.PairSalesOutAmount).Should().Equal(300m, 100m);
            customer.Principals.Should().OnlyContain(row => row.KpiId == PrincipalKpiCatalog.SalesOutId);
            customer.Principals.Should().NotContain(row => row.SupplierId == "SUPC");

            forecast.TopCustomers[0].MtdOmzet.Should().Be(100m);
            decline.Disclosures.Should().Contain(CustomerRiskForecastPrincipalDeclineComposer.CustomerLevelNote);
            decline.Disclosures.Should().Contain(CustomerRiskForecastPrincipalDeclineComposer.ProjectionOnlyPairsNote);
        }

        [Fact]
        public void Compose_KeepsDifferentPairStatuses_ForOneCustomerAtTheSameTime()
        {
            var forecast = Forecast(Customer("CT", "INTI MART", 50m));
            var projection = Projection(
                Pair("CS04D7", "CT", "SUPA", "Principal A", CustomerPrincipalRelationship.StatusActive, 60m),
                Pair("CS04D7", "CT", "SUPB", "Principal B", CustomerPrincipalRelationship.StatusDormant, 40m));

            var decline = CustomerRiskForecastPrincipalDeclineComposer.Compose(forecast, projection);

            var statuses = decline.Customers.Single().Principals
                .Select(row => row.RelationshipStatus)
                .ToList();
            statuses.Should().Contain(CustomerPrincipalRelationship.StatusActive);
            statuses.Should().Contain(CustomerPrincipalRelationship.StatusDormant);
        }

        [Fact]
        public void Compose_IsUnavailable_WhenProjectionIsMissing_AndLeavesCustomerTotalsUnchanged()
        {
            var forecast = Forecast(Customer("C0001", "CLANDYS JAMAL", 100m));

            var decline = CustomerRiskForecastPrincipalDeclineComposer.Compose(forecast, projection: null);

            decline.IsAvailable.Should().BeFalse();
            decline.Customers.Should().BeEmpty();
            forecast.TopCustomers[0].MtdOmzet.Should().Be(100m);
        }

        [Fact]
        public void ListPairsForCustomerCodes_ReadsProjectionOnly()
        {
            var sql = CustomerPrincipalRelationshipDal.ListPairsForCustomerCodesSql;

            sql.Should().Contain("BTRPD_CustomerPrincipalRelationship");
            sql.Should().Contain("c.CustomerCode IN @CustomerCodes");
            sql.Should().Contain("RelationshipStatus");
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
        public async Task Handle_AttachesProjectionDecline_WithoutChangingCustomerTotals()
        {
            var dal = new StubForecastDal(Forecast(Customer("C0001", "CLANDYS JAMAL", 100m)));
            var relationshipDal = new StubRelationshipDal(Projection(
                Pair("CS010B", "C0001", "SUPA", "Principal A", CustomerPrincipalRelationship.StatusDormant, 80m)));
            var handler = new GetDashboardCustomerRiskForecastHandler(dal, relationshipDal);

            var result = await handler.Handle(new GetDashboardCustomerRiskForecastQuery(), CancellationToken.None);

            result.TopCustomers[0].MtdOmzet.Should().Be(100m);
            result.PrincipalDecline.IsAvailable.Should().BeTrue();
            result.PrincipalDecline.Customers.Single().Principals.Single().PairSalesOutAmount.Should().Be(80m);
            result.PrincipalDecline.Customers.Single().Principals.Single().RelationshipStatus.Should()
                .Be(CustomerPrincipalRelationship.StatusDormant);
            relationshipDal.RequestedCustomerCodes.Should().Equal("C0001");
        }

        private static DashboardCustomerRiskForecastResponse Forecast(
            params DashboardCustomerRiskForecastCustomerDto[] customers)
        {
            return new DashboardCustomerRiskForecastResponse
            {
                IsAvailable = true,
                TopCustomers = customers.ToList()
            };
        }

        private static DashboardCustomerRiskForecastCustomerDto Customer(
            string customerCode,
            string customerName,
            decimal mtdOmzet)
        {
            return new DashboardCustomerRiskForecastCustomerDto
            {
                CustomerCode = customerCode,
                CustomerName = customerName,
                MtdOmzet = mtdOmzet,
                PriorMonthOmzet = 200m,
                SalesPersonName = "Salesman X"
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
            string customerCode,
            string supplierId,
            string supplierName,
            string status,
            decimal salesOutAmount)
        {
            return new CustomerPrincipalRelationshipRow
            {
                CustomerId = customerId,
                CustomerCode = customerCode,
                SupplierId = supplierId,
                SupplierName = supplierName,
                RelationshipStatus = status,
                KpiId = PrincipalKpiCatalog.SalesOutId,
                SalesOutAmount = salesOutAmount
            };
        }

        private sealed class StubForecastDal : IDashboardCustomerRiskForecastDal
        {
            private readonly DashboardCustomerRiskForecastResponse _response;

            public StubForecastDal(DashboardCustomerRiskForecastResponse response)
            {
                _response = response;
            }

            public DashboardCustomerRiskForecastResponse GetCurrent() => _response;
        }

        private sealed class StubRelationshipDal : ICustomerPrincipalRelationshipDal
        {
            private readonly CustomerPrincipalRelationshipResult _projection;

            public StubRelationshipDal(CustomerPrincipalRelationshipResult projection)
            {
                _projection = projection;
            }

            public IList<string> RequestedCustomerCodes { get; private set; }
                = new List<string>();

            public CustomerPrincipalRelationshipResult GetProjection() => _projection;

            public CustomerPrincipalRelationshipResult ListPairsForCustomers(IEnumerable<string> customerIds)
            {
                return _projection;
            }

            public CustomerPrincipalRelationshipResult ListPairsForCustomerCodes(IEnumerable<string> customerCodes)
            {
                RequestedCustomerCodes = (customerCodes ?? Enumerable.Empty<string>()).ToList();
                return _projection;
            }

            public void ReplaceProjection(CustomerPrincipalRelationshipResult result, string refreshLogId)
            {
            }
        }
    }
}
