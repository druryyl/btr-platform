using System;
using System.Linq;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.application.ReportingContext.SalesReportAgg;
using btr.application.ReportingContext.SalesReportAgg.Queries;
using btr.infrastructure.ReportingContext.PrincipalAnalyticsAgg;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class SalesReportPrincipalEvidenceTest
    {
        private static readonly DateTime PeriodFrom = new DateTime(2026, 9, 1);
        private static readonly DateTime PeriodTo = new DateTime(2026, 9, 30, 23, 59, 59);

        [Fact]
        public void Attach_AttributesEachLineToItsPrincipal_AndDoesNotSplitHeaderGrandTotal()
        {
            var report = HeaderReport(
                Row("FK-MIX", 1_000_000m),
                Row("FK-ONE", 250_000m));

            SalesReportPrincipalEvidenceComposer.Attach(
                report,
                new[]
                {
                    Line("FK-MIX", "FI-A", "SUPA", "Alpha", subTotal: 100_000m, discRp: 10_000m, headerGrandTotal: 1_000_000m),
                    Line("FK-MIX", "FI-B", "SUPB", "Beta", subTotal: 200_000m, discRp: 0m, headerGrandTotal: 1_000_000m),
                    Line("FK-ONE", "FI-C", "SUPA", "Alpha", subTotal: 50_000m, discRp: 5_000m, headerGrandTotal: 250_000m)
                },
                selectedSupplierId: "SUPA",
                selectedLines: new[]
                {
                    EvidenceLine("FK-MIX", "FI-A", "SUPA", "Alpha", subTotal: 100_000m, discRp: 10_000m),
                    EvidenceLine("FK-ONE", "FI-C", "SUPA", "Alpha", subTotal: 50_000m, discRp: 5_000m),
                    EvidenceLine("FK-MIX", "FI-B", "SUPB", "Beta", subTotal: 200_000m, discRp: 0m)
                });

            report.KpiId.Should().Be(PrincipalKpiCatalog.SalesOutId);
            report.KpiName.Should().Be("Principal Sales-Out");
            report.HeaderTotalLabel.Should().Be("Header Total");
            report.HeaderTotalAmount.Should().Be(1_250_000m);
            report.Rows.Should().HaveCount(2);
            report.Rows.Single(row => row.FakturCode == "FK-MIX").FakturTotal.Should().Be(1_000_000m);

            report.Principals.Select(row => row.SupplierId).Should().Equal("SUPB", "SUPA");
            report.Principals.Single(row => row.SupplierId == "SUPA").PrincipalSalesOutAmount.Should().Be(135_000m);
            report.Principals.Single(row => row.SupplierId == "SUPB").PrincipalSalesOutAmount.Should().Be(200_000m);
            report.Principals.Should().OnlyContain(row => row.KpiId == PrincipalKpiCatalog.SalesOutId);
            report.Principals.Should().NotContain(row => row.PrincipalSalesOutAmount == 500_000m);
            report.Principals.Sum(row => row.PrincipalSalesOutAmount).Should().NotBe(report.HeaderTotalAmount);

            report.EvidenceLines.Select(line => line.FakturItemId).Should().Equal("FI-A", "FI-C");
            report.EvidenceLines.Should().OnlyContain(line =>
                line.SupplierId == "SUPA"
                && line.KpiId == PrincipalKpiCatalog.SalesOutId
                && !string.IsNullOrWhiteSpace(line.FakturId)
                && !string.IsNullOrWhiteSpace(line.FakturItemId));
            report.SelectedPrincipalSalesOutAmount.Should().Be(135_000m);
            report.EvidenceLines.Sum(line => line.PrincipalSalesOutAmount).Should().Be(135_000m);
        }

        [Fact]
        public void Attach_ExcludesBlankAndUnknownPrincipalLines_AndStatesRequiredDisclosures()
        {
            var report = HeaderReport(Row("FK-1", 80_000m));

            SalesReportPrincipalEvidenceComposer.Attach(
                report,
                new[]
                {
                    Line("FK-1", "FI-1", "SUPA", "Alpha", subTotal: 40_000m, discRp: 0m, headerGrandTotal: 80_000m),
                    BlankLine("FK-1", "FI-2", subTotal: 15_000m, discRp: 0m),
                    UnknownLine("FK-1", "FI-3", subTotal: 20_000m, discRp: 0m)
                },
                selectedSupplierId: null,
                selectedLines: null);

            report.Principals.Should().ContainSingle(row => row.SupplierId == "SUPA");
            report.Principals.Single().PrincipalSalesOutAmount.Should().Be(40_000m);
            report.UnknownPrincipalExceptionCount.Should().Be(2);
            report.EvidenceLines.Should().BeEmpty();
            report.Disclosures.Should().Contain(
                SalesReportPrincipalEvidenceComposer.MeasureIsPrnSales001AndNotRequiredToEqualGrandTotal);
            report.Disclosures.Should().Contain(
                SalesReportPrincipalEvidenceComposer.ReturnsClaimsAndInventoryAdjustmentsAreNotDeducted);
            report.Disclosures.Should().Contain(
                SalesReportPrincipalEvidenceComposer.CompanyHeaderTotalsAreNotPrincipalSales);
            report.Disclosures.Should().Contain(
                "Returns, Claims, and Inventory Adjustments are not deducted.");
            report.Disclosures.Should().Contain(
                PrincipalSalesOutDisclosure.ReturnsDoNotReduceOrRedefinePrincipalSalesOut);
        }

        [Fact]
        public void Contract_DoesNotAddPrincipalOpenBalancePaymentOrReturnColumns()
        {
            var principalProperties = typeof(SalesReportPrincipalRow).GetProperties().Select(p => p.Name);
            var evidenceProperties = typeof(SalesReportPrincipalEvidenceLine).GetProperties().Select(p => p.Name);
            var forbidden = new[] { "OpenBalance", "Payment", "ReturnAmount", "KurangBayar", "NetSales" };

            principalProperties.Should().NotContain(forbidden);
            evidenceProperties.Should().NotContain(forbidden);
            evidenceProperties.Should().Contain("FakturItemId");
            evidenceProperties.Should().Contain("FakturId");
            PrincipalSalesOutEvidenceDal.ListFakturItemEvidenceSql.Should().Contain("b.SupplierId");
            PrincipalSalesOutEvidenceDal.ListFakturItemEvidenceSql.Should().Contain("fi.SubTotal");
            PrincipalSalesOutEvidenceDal.ListFakturItemEvidenceSql.Should().Contain("fi.DiscRp");
            PrincipalSalesOutEvidenceDal.ListFakturItemEvidenceForPrincipalSql.Should().Contain("fi.FakturItemId");
            PrincipalSalesOutEvidenceDal.ListFakturItemEvidenceSql.Should().NotContain("Retur");
            PrincipalSalesOutEvidenceDal.ListFakturItemEvidenceForPrincipalSql.Should().NotContain("Retur");
        }

        private static SalesReportResponse HeaderReport(params SalesReportRow[] rows)
        {
            return new SalesReportResponse
            {
                PeriodFrom = PeriodFrom,
                PeriodTo = PeriodTo,
                GeneratedAt = new DateTime(2026, 9, 9, 8, 0, 0),
                Rows = rows.ToList()
            };
        }

        private static SalesReportRow Row(string fakturCode, decimal headerTotal)
        {
            return new SalesReportRow
            {
                FakturCode = fakturCode,
                FakturDate = PeriodFrom,
                FakturTotal = headerTotal
            };
        }

        private static PrincipalSalesOutFakturItemEvidence Line(
            string fakturId,
            string fakturItemId,
            string supplierId,
            string supplierName,
            decimal subTotal,
            decimal discRp,
            decimal headerGrandTotal)
        {
            return new PrincipalSalesOutFakturItemEvidence
            {
                FakturId = fakturId,
                FakturItemId = fakturItemId,
                ItemSupplierId = supplierId,
                SupplierId = supplierId,
                SupplierName = supplierName,
                SubTotal = subTotal,
                DiscRp = discRp,
                HeaderGrandTotal = headerGrandTotal,
                LineTotal = subTotal - discRp + 11m,
                DppRp = subTotal - discRp - 1m
            };
        }

        private static PrincipalSalesOutFakturItemEvidence BlankLine(
            string fakturId,
            string fakturItemId,
            decimal subTotal,
            decimal discRp)
        {
            return new PrincipalSalesOutFakturItemEvidence
            {
                FakturId = fakturId,
                FakturItemId = fakturItemId,
                ItemSupplierId = string.Empty,
                SupplierId = string.Empty,
                SubTotal = subTotal,
                DiscRp = discRp
            };
        }

        private static PrincipalSalesOutFakturItemEvidence UnknownLine(
            string fakturId,
            string fakturItemId,
            decimal subTotal,
            decimal discRp)
        {
            return new PrincipalSalesOutFakturItemEvidence
            {
                FakturId = fakturId,
                FakturItemId = fakturItemId,
                ItemSupplierId = "MISSING",
                SupplierId = string.Empty,
                SubTotal = subTotal,
                DiscRp = discRp
            };
        }

        private static PrincipalSalesOutFakturItemEvidenceLine EvidenceLine(
            string fakturId,
            string fakturItemId,
            string supplierId,
            string supplierName,
            decimal subTotal,
            decimal discRp)
        {
            return new PrincipalSalesOutFakturItemEvidenceLine
            {
                FakturId = fakturId,
                FakturCode = fakturId,
                FakturDate = PeriodFrom,
                FakturItemId = fakturItemId,
                BrgId = "BRG-" + fakturItemId,
                ItemSupplierId = supplierId,
                SupplierId = supplierId,
                SupplierName = supplierName,
                SubTotal = subTotal,
                DiscRp = discRp
            };
        }
    }
}
