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
    public class DashboardItemRelationshipAggregatorTest
    {
        [Fact]
        public void Aggregate_GroupsTopCustomersAndSalesmenByBrgId()
        {
            var aggregator = new DashboardItemRelationshipAggregator();
            var generatedAt = new DateTime(2026, 6, 24, 10, 0, 0);
            var businessDate = new DateTime(2026, 6, 24);

            var result = aggregator.Aggregate(
                new List<SalesmanMtdItemRollupDto>
                {
                    Row("B001", "BRG1", "C001", "SP1", "SP01", 1000m),
                    Row("B001", "BRG1", "C002", "SP1", "SP01", 500m),
                    Row("B001", "BRG1", "C001", "SP2", "SP02", 300m),
                    Row("B002", "BRG2", "C003", "SP3", "SP03", 200m)
                },
                businessDate,
                generatedAt);

            result.ByBrgId.Should().ContainKey("B001");
            var itemOne = result.ByBrgId["B001"];
            itemOne.TopCustomers.Should().HaveCount(2);
            itemOne.TopCustomers[0].CustomerCode.Should().Be("C001");
            itemOne.TopCustomers[0].MetricValue.Should().Be(1300m);
            itemOne.TopSalesmen.Should().HaveCount(2);
            itemOne.TopSalesmen[0].SalesPersonCode.Should().Be("SP01");
            itemOne.TopSalesmen[0].MetricValue.Should().Be(1500m);

            result.ByBrgId["B002"].TopCustomers.Single().CustomerCode.Should().Be("C003");
        }

        private static SalesmanMtdItemRollupDto Row(
            string brgId,
            string brgCode,
            string customerCode,
            string salesPersonId,
            string salesPersonCode,
            decimal lineTotal)
        {
            return new SalesmanMtdItemRollupDto
            {
                BrgId = brgId,
                BrgCode = brgCode,
                BrgName = brgCode,
                CustomerCode = customerCode,
                SalesPersonId = salesPersonId,
                SalesPersonCode = salesPersonCode,
                LineTotal = lineTotal
            };
        }
    }
}
