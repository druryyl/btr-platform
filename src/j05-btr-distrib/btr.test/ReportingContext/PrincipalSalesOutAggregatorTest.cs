using System;
using System.Linq;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Services;
using btr.infrastructure.ReportingContext.PrincipalAnalyticsAgg;
using btr.nuna.Domain;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class PrincipalSalesOutAggregatorTest
    {
        private static readonly DateTime GeneratedAt = new DateTime(2026, 9, 9, 8, 0, 0);
        private static readonly Periode September2026 = new Periode(
            new DateTime(2026, 9, 1),
            new DateTime(2026, 9, 30));

        private readonly PrincipalSalesOutAggregator _aggregator = new PrincipalSalesOutAggregator();

        [Fact]
        public void Aggregate_AttributesMixedPrincipalFaktur_ByLinePrincipal_NotHeaderGrandTotal()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Line("FK001", "FI001", "SUPA", "Alpha", subTotal: 1000m, discRp: 100m, headerGrandTotal: 5000m),
                Line("FK001", "FI002", "SUPB", "Beta", subTotal: 2000m, discRp: 0m, headerGrandTotal: 5000m)
            }, September2026, GeneratedAt);

            result.KpiId.Should().Be(PrincipalKpiCatalog.SalesOutId);
            result.Principals.Should().HaveCount(2);
            result.Principals.Should().ContainSingle(row => row.SupplierId == "SUPA")
                .Which.SalesOutAmount.Should().Be(900m);
            result.Principals.Should().ContainSingle(row => row.SupplierId == "SUPB")
                .Which.SalesOutAmount.Should().Be(2000m);
            result.Principals.Sum(row => row.SalesOutAmount).Should().NotBe(5000m);
            result.Principals.Should().OnlyContain(row => row.KpiId == PrincipalKpiCatalog.SalesOutId);
        }

        [Fact]
        public void Aggregate_UsesSubTotalMinusDiscRp_AndExcludesTaxTotalDppAndGrandTotal()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Line(
                    "FK010",
                    "FI010",
                    "SUPA",
                    "Alpha",
                    subTotal: 1000m,
                    discRp: 150m,
                    ppnRp: 93.5m,
                    lineTotal: 943.5m,
                    dppRp: 935m,
                    headerGrandTotal: 1200m)
            }, September2026, GeneratedAt);

            var principal = result.Principals.Should().ContainSingle().Subject;
            principal.SalesOutAmount.Should().Be(850m);
            principal.SalesOutAmount.Should().NotBe(93.5m);
            principal.SalesOutAmount.Should().NotBe(943.5m);
            principal.SalesOutAmount.Should().NotBe(935m);
            principal.SalesOutAmount.Should().NotBe(1200m);
        }

        [Fact]
        public void Aggregate_ExcludesBlankAndUnknownPrincipal_FromPrincipalRows_AndWritesDataQualityAmountAndCount()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Line("FK020", "FI020", "SUPA", "Alpha", subTotal: 400m, discRp: 0m),
                Line("FK021", "FI021", itemSupplierId: "", supplierId: "", supplierName: "", subTotal: 100m, discRp: 10m),
                Line("FK022", "FI022", itemSupplierId: "   ", supplierId: "", supplierName: "", subTotal: 50m, discRp: 0m),
                Line("FK023", "FI023", itemSupplierId: "MISS", supplierId: "", supplierName: "", subTotal: 80m, discRp: 5m)
            }, September2026, GeneratedAt);

            result.Principals.Should().ContainSingle()
                .Which.SupplierId.Should().Be("SUPA");
            result.Principals.Should().NotContain(row => string.IsNullOrWhiteSpace(row.SupplierId));
            result.Principals.Should().NotContain(row => row.SupplierId == "MISS");

            var blank = result.DataQuality.Should().ContainSingle(row =>
                row.ExceptionCode == PrincipalSalesOutSnapshot.BlankSupplierExceptionCode).Subject;
            blank.Amount.Should().Be(140m);
            blank.LineCount.Should().Be(2);

            var unknown = result.DataQuality.Should().ContainSingle(row =>
                row.ExceptionCode == PrincipalSalesOutSnapshot.UnknownSupplierExceptionCode).Subject;
            unknown.Amount.Should().Be(75m);
            unknown.LineCount.Should().Be(1);
        }

        [Fact]
        public void Aggregate_DoesNotWriteReturnValues_AndDoesNotReduceSalesOut()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Line("FK030", "FI030", "SUPA", "Alpha", subTotal: 1000m, discRp: 0m)
            }, September2026, GeneratedAt);

            result.KpiId.Should().Be("PRN-SALES-001");
            result.Principals.Should().OnlyContain(row => row.KpiId == "PRN-SALES-001");
            result.Principals.Single().SalesOutAmount.Should().Be(1000m);
            result.DataQuality.Should().BeEmpty();

            var resultProperties = typeof(btr.application.ReportingContext.PrincipalAnalyticsAgg.Models.PrincipalSalesOutAggregateResult)
                .GetProperties()
                .Select(property => property.Name);
            resultProperties.Should().NotContain(name => name.IndexOf("Ret", StringComparison.OrdinalIgnoreCase) >= 0);
            resultProperties.Should().NotContain(name => name.IndexOf("Return", StringComparison.OrdinalIgnoreCase) >= 0);

            PrincipalSalesOutEvidenceDal.ListFakturItemEvidenceSql.Should().Contain("f.VoidDate = '3000-01-01'");
            PrincipalSalesOutEvidenceDal.ListFakturItemEvidenceSql.Should().Contain("fi.SubTotal");
            PrincipalSalesOutEvidenceDal.ListFakturItemEvidenceSql.Should().Contain("fi.DiscRp");
            PrincipalSalesOutEvidenceDal.ListFakturItemEvidenceSql.Should().NotContain("Retur");
            PrincipalSalesOutEvidenceDal.ListFakturItemEvidenceSql.Should().NotContain("PRN-RET");
            PrincipalSalesOutEvidenceDal.ListFakturItemEvidenceSql.Should().NotContain("BTR_FakturItemKlaim");

            var writerSql = string.Join(
                " ",
                PrincipalSalesOutSnapshotDal.WrittenTables,
                PrincipalSalesOutSnapshotDal.DeletePrincipalSql,
                PrincipalSalesOutSnapshotDal.DeleteDataQualitySql,
                PrincipalSalesOutSnapshotDal.MergeKpiSql,
                PrincipalSalesOutSnapshotDal.InsertPrincipalSql,
                PrincipalSalesOutSnapshotDal.InsertDataQualitySql);
            writerSql.Should().NotContain("PRN-RET");
            writerSql.Should().NotContain("Retur");
            writerSql.Should().NotContain("BTRPD_SalesmanPrincipalAchievement");
            writerSql.Should().Contain("KpiId");
            writerSql.Should().Contain("BTRPD_PrincipalSalesOut");
            writerSql.Should().NotContain("BTR_FakturItem");
        }

        private static PrincipalSalesOutFakturItemEvidence Line(
            string fakturId,
            string fakturItemId,
            string supplierId,
            string supplierName,
            decimal subTotal,
            decimal discRp,
            decimal ppnRp = 0m,
            decimal lineTotal = 0m,
            decimal dppRp = 0m,
            decimal headerGrandTotal = 0m)
        {
            return Line(
                fakturId,
                fakturItemId,
                itemSupplierId: supplierId,
                supplierId: supplierId,
                supplierName: supplierName,
                subTotal: subTotal,
                discRp: discRp,
                ppnRp: ppnRp,
                lineTotal: lineTotal,
                dppRp: dppRp,
                headerGrandTotal: headerGrandTotal);
        }

        private static PrincipalSalesOutFakturItemEvidence Line(
            string fakturId,
            string fakturItemId,
            string itemSupplierId,
            string supplierId,
            string supplierName,
            decimal subTotal,
            decimal discRp,
            decimal ppnRp = 0m,
            decimal lineTotal = 0m,
            decimal dppRp = 0m,
            decimal headerGrandTotal = 0m)
        {
            return new PrincipalSalesOutFakturItemEvidence
            {
                FakturId = fakturId,
                FakturItemId = fakturItemId,
                ItemSupplierId = itemSupplierId,
                SupplierId = supplierId,
                SupplierName = supplierName,
                SubTotal = subTotal,
                DiscRp = discRp,
                PpnRp = ppnRp,
                LineTotal = lineTotal,
                DppRp = dppRp,
                HeaderGrandTotal = headerGrandTotal
            };
        }
    }
}
