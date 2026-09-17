using System;
using System.Linq;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Services;
using btr.infrastructure.ReportingContext.PrincipalAnalyticsAgg;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class PrincipalPurchaseInAggregatorTest
    {
        private static readonly DateTime GeneratedAt = new DateTime(2026, 9, 9, 8, 0, 0);

        private readonly PrincipalPurchaseInAggregator _aggregator = new PrincipalPurchaseInAggregator();

        [Fact]
        public void Aggregate_SumsPurchaseDetailTotals_ForTheSamePrincipal()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Line("INV01", "L1", "SUPA", "Alpha", 100m),
                Line("INV01", "L2", "SUPA", "Alpha", 25.5m),
                Line("INV02", "L3", "SUPB", "Beta", 40m)
            }, 2026, 9, GeneratedAt);

            result.KpiId.Should().Be(PrincipalKpiCatalog.PurchaseInId);
            result.PeriodYear.Should().Be(2026);
            result.PeriodMonth.Should().Be(9);
            result.Principals.Should().HaveCount(2);
            result.Principals.Should().ContainSingle(row => row.SupplierId == "SUPA")
                .Which.PurchaseInAmount.Should().Be(125.5m);
            result.Principals.Should().ContainSingle(row => row.SupplierId == "SUPA")
                .Which.LineCount.Should().Be(2);
            result.Principals.Should().ContainSingle(row => row.SupplierId == "SUPB")
                .Which.PurchaseInAmount.Should().Be(40m);
            result.Principals.Should().OnlyContain(row => row.KpiId == PrincipalKpiCatalog.PurchaseInId);
        }

        [Fact]
        public void Aggregate_UsesPurchaseDetailTotal_NotABeforeTaxSubstitute()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Line("INV01", "L1", "SUPA", "Alpha", 110m)
            }, 2026, 9, GeneratedAt);

            result.Principals.Should().ContainSingle()
                .Which.PurchaseInAmount.Should().Be(110m);
        }

        [Fact]
        public void Aggregate_SkipsBlankPrincipal_AndDoesNotInventAPrincipal()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Line("INV01", "L1", "SUPA", "Alpha", 80m),
                Line("INV02", "L2", "   ", "", 15m)
            }, 2026, 9, GeneratedAt);

            result.Principals.Should().ContainSingle()
                .Which.SupplierId.Should().Be("SUPA");
            result.Principals.Should().NotContain(row => string.IsNullOrWhiteSpace(row.SupplierId));
        }

        [Fact]
        public void Aggregate_DoesNotWriteSalesOutOrReadSalesOutHistory()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Line("INV01", "L1", "SUPA", "Alpha", 80m)
            }, 2026, 9, GeneratedAt);

            result.KpiId.Should().Be("PRN-PUR-001");
            result.Principals.Should().OnlyContain(row => row.KpiId == "PRN-PUR-001");

            var resultProperties = typeof(btr.application.ReportingContext.PrincipalAnalyticsAgg.Models.PrincipalPurchaseInAggregateResult)
                .GetProperties()
                .Select(property => property.Name);
            resultProperties.Should().NotContain("SalesOutAmount");
            resultProperties.Should().NotContain(name => name.IndexOf("SalesOut", StringComparison.OrdinalIgnoreCase) >= 0);

            var rowProperties = typeof(btr.application.ReportingContext.PrincipalAnalyticsAgg.Models.PrincipalPurchaseInRow)
                .GetProperties()
                .Select(property => property.Name);
            rowProperties.Should().NotContain("SalesOutAmount");

            var evidenceSql = PrincipalPurchaseInEvidenceDal.ListPurchaseDetailSql;
            evidenceSql.Should().Contain("BTR_InvoiceItem");
            evidenceSql.Should().Contain("BTR_Invoice");
            evidenceSql.Should().Contain("ii.Total");
            evidenceSql.Should().Contain("i.SupplierId");
            evidenceSql.Should().Contain("VoidDate = '3000-01-01'");
            evidenceSql.Should().NotContain("BTR_Faktur");
            evidenceSql.Should().NotContain("BTRPD_PrincipalSalesOut");
            evidenceSql.Should().NotContain("SalesOutAmount");
            evidenceSql.Should().NotContain("GrandTotal");
            evidenceSql.Should().NotContain("PRN-SALES-001");
            evidenceSql.Should().NotContain("PU-KPI-001");

            var writerSql = string.Join(
                " ",
                PrincipalPurchaseInSnapshotDal.WrittenTables,
                PrincipalPurchaseInSnapshotDal.DeletePrincipalSql,
                PrincipalPurchaseInSnapshotDal.MergeKpiSql,
                PrincipalPurchaseInSnapshotDal.InsertPrincipalSql);
            writerSql.Should().Contain("BTRPD_PrincipalPurchaseIn");
            writerSql.Should().Contain("PurchaseInAmount");
            writerSql.Should().NotContain("BTRPD_PrincipalSalesOut");
            writerSql.Should().NotContain("PRN-SALES-001");
            writerSql.Should().NotContain("SalesOutAmount");
            writerSql.Should().NotContain("PU-KPI-001");
            writerSql.Should().NotContain("BTRPD_Purchasing");
        }

        private static PurchaseDetailEvidence Line(
            string invoiceId,
            string invoiceItemId,
            string supplierId,
            string supplierName,
            decimal purchaseDetailTotal)
        {
            return new PurchaseDetailEvidence
            {
                InvoiceId = invoiceId,
                InvoiceItemId = invoiceItemId,
                SupplierId = supplierId,
                SupplierName = supplierName,
                PurchaseDetailTotal = purchaseDetailTotal
            };
        }
    }
}
