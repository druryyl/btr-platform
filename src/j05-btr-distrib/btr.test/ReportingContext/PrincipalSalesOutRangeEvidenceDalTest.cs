using System;
using System.Linq;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.infrastructure.ReportingContext.PrincipalAnalyticsAgg;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class PrincipalSalesOutRangeEvidenceDalTest
    {
        [Fact]
        public void Contract_ExposesInclusiveBusinessDateRangeReturningPerPrincipalRows()
        {
            var method = typeof(IPrincipalSalesOutRangeEvidenceDal)
                .GetMethod(nameof(IPrincipalSalesOutRangeEvidenceDal.ListSalesOutByRange));

            method.Should().NotBeNull();
            method.ReturnType.Should().Be(typeof(System.Collections.Generic.IReadOnlyList<PrincipalSalesOutRangeEvidenceRow>));
            method.GetParameters().Select(parameter => parameter.ParameterType)
                .Should().Equal(typeof(DateTime), typeof(DateTime));

            typeof(PrincipalSalesOutRangeEvidenceRow).GetProperties().Select(property => property.Name)
                .Should().Contain(new[] { "SupplierId", "SupplierName", "SalesOutAmount", "LineCount" });
        }

        [Fact]
        public void RangeSql_MirrorsSalesOutAttribution_ExcludesVoidFaktur_AndGroupsByPrincipal()
        {
            var sql = PrincipalSalesOutRangeEvidenceDal.ListSalesOutByRangeSql;

            sql.Should().Contain("BTR_Faktur");
            sql.Should().Contain("BTR_FakturItem");
            sql.Should().Contain("BTR_Brg");
            sql.Should().Contain("BTR_Supplier");
            sql.Should().Contain("f.VoidDate = '3000-01-01'");
            sql.Should().Contain("f.FakturDate BETWEEN @StartDate AND @EndDate");
            sql.Should().Contain("fi.SubTotal");
            sql.Should().Contain("fi.DiscRp");
            sql.Should().Contain("b.SupplierId");
            sql.Should().Contain("sup.SupplierId");
            sql.Should().Contain("SUM(ISNULL(fi.SubTotal, 0) - ISNULL(fi.DiscRp, 0)) AS SalesOutAmount");
            sql.Should().Contain("GROUP BY LTRIM(RTRIM(sup.SupplierId))");
        }

        [Fact]
        public void RangeSql_IsReadOnly_AndDoesNotDeductReturnsOrFallBackToHeaderTotals()
        {
            var sql = PrincipalSalesOutRangeEvidenceDal.ListSalesOutByRangeSql;

            sql.Should().NotContain("INSERT");
            sql.Should().NotContain("DELETE");
            sql.Should().NotContain("UPDATE");
            sql.Should().NotContain("MERGE");
            sql.Should().NotContain("BTRPD_PrincipalSalesOutHistory");
            sql.Should().NotContain("CREATE INDEX");
            sql.Should().NotContain("Retur");
            sql.Should().NotContain("PRN-RET");
            sql.Should().NotContain("BTR_FakturItemKlaim");
            sql.Should().NotContain("GrandTotal");
            sql.Should().NotContain("PpnRp");
            sql.Should().NotContain("DppRp");
            sql.Should().NotContain("Purchasing");
        }
    }
}
