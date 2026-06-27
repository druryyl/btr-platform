using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardSupplierRelationshipAggregatorTest
    {
        private readonly DashboardSupplierRelationshipAggregator _aggregator =
            new DashboardSupplierRelationshipAggregator();

        [Fact]
        public void Aggregate_GroupsTopCustomersSalesmenAndItemsBySupplierId()
        {
            var generatedAt = new DateTime(2026, 6, 24, 12, 0, 0);
            var result = _aggregator.Aggregate(new[]
            {
                Rollup("S1", "SUP1", "C1", "SP1", "SP01", "B1", "BC1", "Item 1", 100m, "Customer One", "Salesman One"),
                Rollup("S1", "SUP1", "C2", "SP2", "SP02", "B2", "BC2", "Item 2", 300m, "Customer Two", "Salesman Two"),
                Rollup("S1", "SUP1", "C1", "SP1", "SP01", "B3", "BC3", "Item 3", 50m, "Customer One", "Salesman One"),
                Rollup("S2", "SUP2", "C3", "SP3", "SP03", "B4", "BC4", "Item 4", 500m, "Customer Three", "Salesman Three")
            }, generatedAt.Date, generatedAt);

            result.BySupplierId.Should().HaveCount(2);
            var supplierOne = result.BySupplierId["S1"];
            supplierOne.TopCustomers.Should().HaveCount(2);
            supplierOne.TopCustomers[0].CustomerCode.Should().Be("C1");
            supplierOne.TopCustomers[0].CustomerName.Should().Be("Customer One");
            supplierOne.TopCustomers[0].MetricValue.Should().Be(150m);
            supplierOne.TopSalesmen.Should().ContainSingle(s => s.SalesPersonId == "SP2");
            supplierOne.TopSalesmen.Single(s => s.SalesPersonId == "SP2").SalesPersonName.Should().Be("Salesman Two");
            supplierOne.TopItems.Should().HaveCount(3);
            supplierOne.TopItems[0].BrgId.Should().Be("B2");
        }

        private static SupplierMtdItemRollupDto Rollup(
            string supplierId,
            string supplierCode,
            string customerCode,
            string salesPersonId,
            string salesPersonCode,
            string brgId,
            string brgCode,
            string brgName,
            decimal lineTotal,
            string customerName = null,
            string salesPersonName = null)
        {
            return new SupplierMtdItemRollupDto
            {
                SupplierId = supplierId,
                SupplierCode = supplierCode,
                CustomerCode = customerCode,
                CustomerName = customerName ?? customerCode,
                SalesPersonId = salesPersonId,
                SalesPersonCode = salesPersonCode,
                SalesPersonName = salesPersonName ?? salesPersonCode,
                BrgId = brgId,
                BrgCode = brgCode,
                BrgName = brgName,
                LineTotal = lineTotal
            };
        }
    }
}
