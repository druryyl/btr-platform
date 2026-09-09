using System;
using System.Linq;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Queries;
using btr.infrastructure.ReportingContext.PrincipalAnalyticsAgg;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class PrincipalPerformanceQueryTest
    {
        [Fact]
        public void Compose_RanksByStoredPrincipalSalesOut_AndCountsUnknownPrincipalExceptions()
        {
            var snapshot = new PrincipalSalesOutAggregateResult
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                PeriodYear = 2026,
                PeriodMonth = 9,
                GeneratedAt = new DateTime(2026, 9, 9, 8, 0, 0),
                Principals = new[]
                {
                    Row("SUPB", "Beta", 200m, 2),
                    Row("SUPA", "Alpha", 900m, 1)
                }.ToList(),
                DataQuality = new[]
                {
                    new PrincipalSalesOutDataQualityRow
                    {
                        ExceptionCode = PrincipalSalesOutSnapshot.BlankSupplierExceptionCode,
                        Amount = 50m,
                        LineCount = 2
                    },
                    new PrincipalSalesOutDataQualityRow
                    {
                        ExceptionCode = PrincipalSalesOutSnapshot.UnknownSupplierExceptionCode,
                        Amount = 25m,
                        LineCount = 1
                    }
                }.ToList()
            };

            var response = PrincipalPerformanceComposer.Compose(snapshot);

            response.IsAvailable.Should().BeTrue();
            response.KpiId.Should().Be(PrincipalKpiCatalog.SalesOutId);
            response.KpiName.Should().Be("Principal Sales-Out");
            response.PrincipalSalesOutAmount.Should().Be(1100m);
            response.UnknownPrincipalExceptionCount.Should().Be(3);
            response.Ranking.Select(row => row.SupplierId).Should().Equal("SUPA", "SUPB");
            response.Ranking.Should().OnlyContain(row => row.KpiId == PrincipalKpiCatalog.SalesOutId);
            response.Disclosures.Should().Contain(
                PrincipalSalesOutDisclosure.ReturnsDoNotReduceOrRedefinePrincipalSalesOut);
            response.Disclosures.Should().Contain(
                "The measure is Principal Sales-Out (DPP) from Faktur Item.");
            response.Disclosures.Should().Contain(
                "Unknown Principal and missing monthly target responsibility are visible exceptions, not silent drops of Principal Sales-Out.");
        }

        [Fact]
        public void Compose_WhenSnapshotMissing_DoesNotInventAPrincipalSalesOutFigure()
        {
            var response = PrincipalPerformanceComposer.Compose(null);

            response.IsAvailable.Should().BeFalse();
            response.PrincipalSalesOutAmount.Should().Be(0m);
            response.Ranking.Should().BeEmpty();
            response.KpiId.Should().Be(PrincipalKpiCatalog.SalesOutId);
            response.Disclosures.Should().NotBeEmpty();
        }

        [Fact]
        public void EvidenceComposer_UsesFakturItemSalesOut_AndKeepsPrincipalIdentity()
        {
            var response = PrincipalSalesOutEvidenceComposer.Compose(
                "SUPA",
                2026,
                9,
                new[]
                {
                    Line("FK002", "FI002", "SUPA", "Alpha", subTotal: 400m, discRp: 40m, fakturDate: new DateTime(2026, 9, 3)),
                    Line("FK001", "FI001", "SUPA", "Alpha", subTotal: 1000m, discRp: 100m, fakturDate: new DateTime(2026, 9, 2)),
                    Line("FK003", "FI003", "SUPB", "Beta", subTotal: 500m, discRp: 0m, fakturDate: new DateTime(2026, 9, 4))
                });

            response.KpiId.Should().Be(PrincipalKpiCatalog.SalesOutId);
            response.PrincipalName.Should().Be("Alpha");
            response.Lines.Should().HaveCount(2);
            response.Lines.Select(line => line.FakturItemId).Should().Equal("FI001", "FI002");
            response.Lines.Should().OnlyContain(line => line.KpiId == PrincipalKpiCatalog.SalesOutId);
            response.PrincipalSalesOutAmount.Should().Be(1260m);
            response.Lines.Should().OnlyContain(line => line.SupplierId == "SUPA");
        }

        [Fact]
        public void EvidenceSql_ReadsFakturItem_NotPurchasingManagementSalesOutAmount()
        {
            PrincipalSalesOutEvidenceDal.ListFakturItemEvidenceForPrincipalSql.Should().Contain("BTR_FakturItem");
            PrincipalSalesOutEvidenceDal.ListFakturItemEvidenceForPrincipalSql.Should().Contain("fi.SubTotal");
            PrincipalSalesOutEvidenceDal.ListFakturItemEvidenceForPrincipalSql.Should().Contain("fi.DiscRp");
            PrincipalSalesOutEvidenceDal.ListFakturItemEvidenceForPrincipalSql.Should().Contain("f.VoidDate = '3000-01-01'");
            PrincipalSalesOutEvidenceDal.ListFakturItemEvidenceForPrincipalSql.Should().Contain("b.SupplierId");
            PrincipalSalesOutEvidenceDal.ListFakturItemEvidenceForPrincipalSql.Should().NotContain("Retur");
            PrincipalSalesOutEvidenceDal.ListFakturItemEvidenceForPrincipalSql.Should().NotContain("PRN-RET");
            PrincipalSalesOutEvidenceDal.ListFakturItemEvidenceForPrincipalSql.Should().NotContain("SalesOutAmount");
            PrincipalSalesOutEvidenceDal.ListFakturItemEvidenceForPrincipalSql.Should().NotContain("Purchasing");
        }

        private static PrincipalSalesOutRow Row(
            string supplierId,
            string supplierName,
            decimal salesOutAmount,
            int sortOrder)
        {
            return new PrincipalSalesOutRow
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                SupplierId = supplierId,
                SupplierName = supplierName,
                SalesOutAmount = salesOutAmount,
                LineCount = 1,
                SortOrder = sortOrder
            };
        }

        private static PrincipalSalesOutFakturItemEvidenceLine Line(
            string fakturId,
            string fakturItemId,
            string supplierId,
            string supplierName,
            decimal subTotal,
            decimal discRp,
            DateTime fakturDate)
        {
            return new PrincipalSalesOutFakturItemEvidenceLine
            {
                FakturId = fakturId,
                FakturCode = fakturId,
                FakturDate = fakturDate,
                FakturItemId = fakturItemId,
                BrgId = "BRG1",
                ItemSupplierId = supplierId,
                SupplierId = supplierId,
                SupplierName = supplierName,
                SubTotal = subTotal,
                DiscRp = discRp
            };
        }
    }
}
