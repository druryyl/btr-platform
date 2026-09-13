using System;
using System.Linq;
using System.Text.RegularExpressions;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.infrastructure.ReportingContext.PrincipalAnalyticsAgg;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class PrincipalSalesOutRangeEvidenceDalTest
    {
        private static readonly string[] SharedSalesOutAggregationFragments =
        {
            "FROM BTR_Faktur f",
            "INNER JOIN BTR_FakturItem fi ON f.FakturId = fi.FakturId",
            "INNER JOIN BTR_Brg b ON fi.BrgId = b.BrgId",
            "INNER JOIN BTR_Supplier sup ON b.SupplierId = sup.SupplierId",
            "f.VoidDate = '3000-01-01'",
            "LTRIM(RTRIM(ISNULL(b.SupplierId, ''))) <> ''",
            "LTRIM(RTRIM(ISNULL(sup.SupplierId, ''))) <> ''",
            "LTRIM(RTRIM(sup.SupplierId)) AS SupplierId",
            "MAX(LTRIM(RTRIM(ISNULL(sup.SupplierName, '')))) AS SupplierName",
            "SUM(ISNULL(fi.SubTotal, 0) - ISNULL(fi.DiscRp, 0)) AS SalesOutAmount",
            "COUNT(*) AS LineCount"
        };

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

        [Fact]
        public void FullMonthRange_ReconcilesToMonthGrainHistory_OnSharedAttributionAndAggregation()
        {
            var rangeSql = Normalize(PrincipalSalesOutRangeEvidenceDal.ListSalesOutByRangeSql);
            var historySql = Normalize(PrincipalSalesOutHistoryEvidenceDal.ListMonthlySalesOutHistorySql);

            foreach (var fragment in SharedSalesOutAggregationFragments)
            {
                rangeSql.Should().Contain(fragment);
                historySql.Should().Contain(fragment);
            }
        }

        [Fact]
        public void FullMonthRange_DiffersFromHistoryOnlyByInclusiveWindowAndMonthGrouping()
        {
            var rangeSql = Normalize(PrincipalSalesOutRangeEvidenceDal.ListSalesOutByRangeSql);
            var historySql = Normalize(PrincipalSalesOutHistoryEvidenceDal.ListMonthlySalesOutHistorySql);

            rangeSql.Should().Contain("f.FakturDate BETWEEN @StartDate AND @EndDate");
            rangeSql.Should().NotContain("f.FakturDate < '3000-01-01'");
            historySql.Should().Contain("f.FakturDate < '3000-01-01'");
            historySql.Should().NotContain("@StartDate");
            historySql.Should().NotContain("@EndDate");

            rangeSql.Should().Contain("GROUP BY LTRIM(RTRIM(sup.SupplierId))");
            historySql.Should().Contain(
                "GROUP BY YEAR(f.FakturDate), MONTH(f.FakturDate), LTRIM(RTRIM(sup.SupplierId))");
        }

        private static string Normalize(string sql)
        {
            return Regex.Replace(sql ?? string.Empty, @"\s+", " ").Trim();
        }
    }
}
