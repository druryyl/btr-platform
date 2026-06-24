using System;
using System.Collections.Generic;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardCustomerRelationshipAggregatorTest
    {
        [Fact]
        public void Aggregate_BuildsTopItemsAndPrincipalsPerCustomer()
        {
            var aggregator = new DashboardCustomerRelationshipAggregator();
            var generatedAt = new DateTime(2026, 6, 24, 10, 0, 0);

            var result = aggregator.Aggregate(
                new List<CustomerMtdItemRollupDto>
                {
                    Row("C001", "I1", "BRG1", "Item 1", "S1", "SUP1", "Principal 1", 100m),
                    Row("C001", "I1", "BRG1", "Item 1", "S1", "SUP1", "Principal 1", 50m),
                    Row("C001", "I2", "BRG2", "Item 2", "S2", "SUP2", "Principal 2", 200m),
                    Row("C002", "I3", "BRG3", "Item 3", "S3", "SUP3", "Principal 3", 75m)
                },
                generatedAt.Date,
                generatedAt);

            result.ByCustomerCode.Should().HaveCount(2);
            var c001 = result.ByCustomerCode["C001"];
            c001.TopItems.Should().HaveCount(2);
            c001.TopItems[0].BrgCode.Should().Be("BRG2");
            c001.TopItems[0].MetricValue.Should().Be(200m);
            c001.TopPrincipals.Should().HaveCount(2);
            c001.TopPrincipals[0].SupplierName.Should().Be("Principal 2");
            c001.TopPrincipals[0].MetricValue.Should().Be(200m);
        }

        private static CustomerMtdItemRollupDto Row(
            string customerCode,
            string brgId,
            string brgCode,
            string brgName,
            string supplierId,
            string supplierCode,
            string supplierName,
            decimal lineTotal)
        {
            return new CustomerMtdItemRollupDto
            {
                CustomerCode = customerCode,
                BrgId = brgId,
                BrgCode = brgCode,
                BrgName = brgName,
                SupplierId = supplierId,
                SupplierCode = supplierCode,
                SupplierName = supplierName,
                LineTotal = lineTotal
            };
        }
    }
}
