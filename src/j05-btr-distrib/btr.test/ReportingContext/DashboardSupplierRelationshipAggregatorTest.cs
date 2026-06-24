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
                Rollup("S1", "SUP1", "C1", "SP1", "SP01", "B1", "BC1", "Item 1", 100m),
                Rollup("S1", "SUP1", "C2", "SP2", "SP02", "B2", "BC2", "Item 2", 300m),
                Rollup("S1", "SUP1", "C1", "SP1", "SP01", "B3", "BC3", "Item 3", 50m),
                Rollup("S2", "SUP2", "C3", "SP3", "SP03", "B4", "BC4", "Item 4", 500m)
            }, generatedAt.Date, generatedAt);

            result.BySupplierId.Should().HaveCount(2);
            var supplierOne = result.BySupplierId["S1"];
            supplierOne.TopCustomers.Should().HaveCount(2);
            supplierOne.TopCustomers[0].CustomerCode.Should().Be("C1");
            supplierOne.TopCustomers[0].MetricValue.Should().Be(150m);
            supplierOne.TopSalesmen.Should().ContainSingle(s => s.SalesPersonId == "SP2");
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
            decimal lineTotal)
        {
            return new SupplierMtdItemRollupDto
            {
                SupplierId = supplierId,
                SupplierCode = supplierCode,
                CustomerCode = customerCode,
                SalesPersonId = salesPersonId,
                SalesPersonCode = salesPersonCode,
                BrgId = brgId,
                BrgCode = brgCode,
                BrgName = brgName,
                LineTotal = lineTotal
            };
        }
    }
}
