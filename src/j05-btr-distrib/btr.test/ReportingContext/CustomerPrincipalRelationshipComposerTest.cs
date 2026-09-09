using System;
using System.Linq;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Services;
using btr.infrastructure.ReportingContext.PrincipalAnalyticsAgg;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class CustomerPrincipalRelationshipComposerTest
    {
        private static readonly DateTime AsOfDate = new DateTime(2026, 9, 9);
        private static readonly DateTime GeneratedAt = new DateTime(2026, 9, 9, 8, 0, 0);

        private readonly CustomerPrincipalRelationshipComposer _composer =
            new CustomerPrincipalRelationshipComposer();

        [Fact]
        public void Compose_RetainsDormantPairs_AndDoesNotDeleteHistoryBecauseOfInactivity()
        {
            var result = _composer.Compose(new[]
            {
                Pair("CUS01", "Alpha", "SUPA", "Principal A", new DateTime(2025, 2, 7), new DateTime(2025, 8, 1), 400m, 2),
                Pair("CUS02", "Beta", "SUPA", "Principal A", new DateTime(2026, 4, 1), new DateTime(2026, 6, 24), 900m, 3)
            }, AsOfDate, GeneratedAt);

            result.Pairs.Should().HaveCount(2);
            result.Pairs.Should().ContainSingle(row => row.CustomerId == "CUS01" && row.SupplierId == "SUPA")
                .Which.RelationshipStatus.Should().Be(CustomerPrincipalRelationship.StatusDormant);
            result.Pairs.Should().OnlyContain(row => row.FirstTransactionDate < VoidSentinelDate);
            result.Pairs.Should().NotContain(row => row.CustomerId == "CUS01" && row.LastTransactionDate == default(DateTime));
        }

        [Fact]
        public void Compose_UsesSixMonthLastTransactionRule_InclusiveOfCutoff()
        {
            var cutoff = CustomerPrincipalRelationshipComposer.ActiveCutoff(AsOfDate);
            cutoff.Should().Be(new DateTime(2026, 3, 9));

            var result = _composer.Compose(new[]
            {
                Pair("CUSON", "On Cutoff", "SUPA", "Principal A", new DateTime(2025, 2, 7), cutoff, 100m, 1),
                Pair("CUSOFF", "Before Cutoff", "SUPA", "Principal A", new DateTime(2025, 2, 7), cutoff.AddDays(-1), 80m, 1),
                Pair("CUSIN", "Inside Window", "SUPB", "Principal B", new DateTime(2026, 1, 1), new DateTime(2026, 6, 24), 50m, 1)
            }, AsOfDate, GeneratedAt);

            result.AsOfDate.Should().Be(AsOfDate);
            result.Pairs.Should().ContainSingle(row => row.CustomerId == "CUSON")
                .Which.RelationshipStatus.Should().Be(CustomerPrincipalRelationship.StatusActive);
            result.Pairs.Should().ContainSingle(row => row.CustomerId == "CUSOFF")
                .Which.RelationshipStatus.Should().Be(CustomerPrincipalRelationship.StatusDormant);
            result.Pairs.Should().ContainSingle(row => row.CustomerId == "CUSIN")
                .Which.RelationshipStatus.Should().Be(CustomerPrincipalRelationship.StatusActive);
        }

        [Fact]
        public void Compose_StoresPairAttributedSalesOut_AndDoesNotLimitToTopNOrCurrentMonth()
        {
            var pairs = Enumerable.Range(1, 12)
                .Select(index => Pair(
                    "C" + index.ToString("00"),
                    "Customer " + index,
                    "SUPA",
                    "Principal A",
                    new DateTime(2025, 2, 7),
                    new DateTime(2025, 6, 1).AddMonths(index),
                    10m * index,
                    index))
                .ToArray();

            var result = _composer.Compose(pairs, AsOfDate, GeneratedAt);

            result.KpiId.Should().Be(PrincipalKpiCatalog.SalesOutId);
            result.Pairs.Should().HaveCount(12);
            result.Pairs.Should().OnlyContain(row => row.KpiId == PrincipalKpiCatalog.SalesOutId);
            result.Pairs.Single(row => row.CustomerId == "C01").SalesOutAmount.Should().Be(10m);
            result.Pairs.Should().Contain(row => row.RelationshipStatus == CustomerPrincipalRelationship.StatusDormant);
            result.Pairs.Should().Contain(row => row.RelationshipStatus == CustomerPrincipalRelationship.StatusActive);
        }

        [Fact]
        public void Compose_DoesNotWriteCustomerOrReturnKpis_AndConsumerQueriesAreNotPartOfThisSlice()
        {
            var result = _composer.Compose(new[]
            {
                Pair("CUS01", "Alpha", "SUPA", "Principal A", new DateTime(2026, 4, 1), new DateTime(2026, 6, 24), 850m, 1)
            }, AsOfDate, GeneratedAt);

            result.KpiId.Should().Be("PRN-SALES-001");
            result.Pairs.Should().OnlyContain(row => row.KpiId == "PRN-SALES-001");
            result.Pairs.Single().SalesOutAmount.Should().Be(850m);

            var resultProperties = typeof(CustomerPrincipalRelationshipResult)
                .GetProperties()
                .Select(property => property.Name)
                .Concat(typeof(CustomerPrincipalRelationshipRow).GetProperties().Select(property => property.Name))
                .ToList();
            resultProperties.Should().NotContain("PRN-CUS-001");
            resultProperties.Should().NotContain("PRN-CUS-002");
            resultProperties.Should().NotContain(name => name.IndexOf("Return", StringComparison.OrdinalIgnoreCase) >= 0);
            resultProperties.Should().NotContain(name => name.IndexOf("Coverage", StringComparison.OrdinalIgnoreCase) >= 0);
            resultProperties.Should().NotContain("ActiveCustomerCount");

            CustomerPrincipalRelationshipEvidenceDal.ListHistoricalPairEvidenceSql.Should().Contain("f.VoidDate = '3000-01-01'");
            CustomerPrincipalRelationshipEvidenceDal.ListHistoricalPairEvidenceSql.Should().Contain("fi.SubTotal");
            CustomerPrincipalRelationshipEvidenceDal.ListHistoricalPairEvidenceSql.Should().Contain("fi.DiscRp");
            CustomerPrincipalRelationshipEvidenceDal.ListHistoricalPairEvidenceSql.Should().Contain("b.SupplierId");
            CustomerPrincipalRelationshipEvidenceDal.ListHistoricalPairEvidenceSql.Should().Contain("GROUP BY");
            CustomerPrincipalRelationshipEvidenceDal.ListHistoricalPairEvidenceSql.Should().NotContain("TOP ");
            CustomerPrincipalRelationshipEvidenceDal.ListHistoricalPairEvidenceSql.Should().NotContain("Retur");
            CustomerPrincipalRelationshipEvidenceDal.ListHistoricalPairEvidenceSql.Should().NotContain("PRN-RET");
            CustomerPrincipalRelationshipEvidenceDal.ListHistoricalPairEvidenceSql.Should().NotContain("PRN-CUS");
            CustomerPrincipalRelationshipEvidenceDal.ListHistoricalPairEvidenceSql.Should().NotContain("BTR_FakturItemKlaim");
            CustomerPrincipalRelationshipEvidenceDal.ListHistoricalPairEvidenceSql.Should().NotContain("BETWEEN");

            var writerSql = string.Join(
                " ",
                CustomerPrincipalRelationshipDal.WrittenTables,
                CustomerPrincipalRelationshipDal.DeleteRelationshipSql,
                CustomerPrincipalRelationshipDal.MergeHeaderSql,
                CustomerPrincipalRelationshipDal.InsertRelationshipSql);
            writerSql.Should().Contain("BTRPD_CustomerPrincipalRelationship");
            writerSql.Should().Contain("KpiId");
            PrincipalKpiCatalog.SalesOutId.Should().Be("PRN-SALES-001");
            writerSql.Should().NotContain("PRN-CUS-001");
            writerSql.Should().NotContain("PRN-CUS-002");
            writerSql.Should().NotContain("PRN-RET");
            writerSql.Should().NotContain("BTRPD_EntityAnalytics");
            writerSql.Should().NotContain("BTR_FakturItem");
        }

        private static CustomerPrincipalRelationshipPairEvidence Pair(
            string customerId,
            string customerName,
            string supplierId,
            string supplierName,
            DateTime firstTransactionDate,
            DateTime lastTransactionDate,
            decimal salesOutAmount,
            int lineCount)
        {
            return new CustomerPrincipalRelationshipPairEvidence
            {
                CustomerId = customerId,
                CustomerName = customerName,
                SupplierId = supplierId,
                SupplierName = supplierName,
                FirstTransactionDate = firstTransactionDate,
                LastTransactionDate = lastTransactionDate,
                SalesOutAmount = salesOutAmount,
                LineCount = lineCount
            };
        }

        private static readonly DateTime VoidSentinelDate = new DateTime(3000, 1, 1);
    }
}
