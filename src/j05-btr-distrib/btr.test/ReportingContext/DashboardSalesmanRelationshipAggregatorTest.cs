using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardSalesmanRelationshipAggregatorTest
    {
        [Fact]
        public void Aggregate_BuildsTopCustomersAndItemsPerSalesman()
        {
            var aggregator = new DashboardSalesmanRelationshipAggregator();
            var generatedAt = new DateTime(2026, 6, 24, 10, 0, 0);

            var result = aggregator.Aggregate(
                new List<SalesmanMtdItemRollupDto>
                {
                    Row("SP1", "SP01", "C001", "I1", "BRG1", "Item 1", 100m),
                    Row("SP1", "SP01", "C001", "I1", "BRG1", "Item 1", 50m),
                    Row("SP1", "SP01", "C002", "I2", "BRG2", "Item 2", 200m),
                    Row("SP2", "SP02", "C003", "I3", "BRG3", "Item 3", 75m)
                },
                generatedAt.Date,
                generatedAt);

            result.BySalesPersonId.Should().HaveCount(2);
            var sp1 = result.BySalesPersonId["SP1"];
            sp1.TopCustomers.Should().HaveCount(2);
            sp1.TopCustomers[0].CustomerCode.Should().Be("C002");
            sp1.TopCustomers[0].MetricValue.Should().Be(200m);
            sp1.TopItems.Should().HaveCount(2);
            sp1.TopItems[0].BrgCode.Should().Be("BRG2");
            sp1.TopItems[0].MetricValue.Should().Be(200m);
        }

        private static SalesmanMtdItemRollupDto Row(
            string salesPersonId,
            string salesPersonCode,
            string customerCode,
            string brgId,
            string brgCode,
            string brgName,
            decimal lineTotal)
        {
            return new SalesmanMtdItemRollupDto
            {
                SalesPersonId = salesPersonId,
                SalesPersonCode = salesPersonCode,
                CustomerCode = customerCode,
                BrgId = brgId,
                BrgCode = brgCode,
                BrgName = brgName,
                LineTotal = lineTotal
            };
        }
    }
}
