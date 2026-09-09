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
    public class PrincipalSalesOutHistoryComposerTest
    {
        private static readonly DateTime GeneratedAt = new DateTime(2026, 9, 9, 8, 0, 0);

        private readonly PrincipalSalesOutHistoryComposer _composer = new PrincipalSalesOutHistoryComposer();

        [Fact]
        public void Compose_WritesPrincipalYearMonthSalesOutOnly_AndOmitsMonthsWithoutSource()
        {
            var result = _composer.Compose(new[]
            {
                Month(2026, 9, "SUPA", "Alpha", 900m, 2),
                Month(2026, 8, "SUPA", "Alpha", 400m, 1),
                Month(2025, 9, "SUPB", "Beta", 250m, 1)
            }, currentSnapshot: null, GeneratedAt);

            result.KpiId.Should().Be(PrincipalKpiCatalog.SalesOutId);
            result.HistoricalLimitation.Should().Be(PrincipalSalesOutHistory.HistoricalLimitation);
            result.Months.Should().HaveCount(3);
            result.Months.Should().OnlyContain(row =>
                row.KpiId == PrincipalKpiCatalog.SalesOutId
                && row.PeriodYear > 0
                && row.PeriodMonth >= 1
                && row.PeriodMonth <= 12
                && !string.IsNullOrWhiteSpace(row.SupplierId));
            result.Months.Should().ContainSingle(row =>
                row.SupplierId == "SUPA" && row.PeriodYear == 2026 && row.PeriodMonth == 9)
                .Which.SalesOutAmount.Should().Be(900m);
            result.Months.Should().ContainSingle(row =>
                row.SupplierId == "SUPA" && row.PeriodYear == 2026 && row.PeriodMonth == 8);
            result.Months.Should().ContainSingle(row =>
                row.SupplierId == "SUPB" && row.PeriodYear == 2025 && row.PeriodMonth == 9);
            result.Months.Should().NotContain(row => row.PeriodYear == 2026 && row.PeriodMonth == 7);

            typeof(PrincipalSalesOutHistoryResult).GetProperties()
                .Select(property => property.Name)
                .Should().NotContain(name => name.IndexOf("Ret", StringComparison.OrdinalIgnoreCase) >= 0);
            typeof(PrincipalSalesOutHistoryRow).GetProperties()
                .Select(property => property.Name)
                .Should().NotContain(name => name.IndexOf("Ret", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        [Fact]
        public void Compose_CurrentMonthEqualsCurrentSnapshot_AndUnknownIsNotAssignedToHistory()
        {
            var current = new PrincipalSalesOutAggregateResult
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                PeriodYear = 2026,
                PeriodMonth = 9,
                Principals =
                {
                    new PrincipalSalesOutRow
                    {
                        KpiId = PrincipalKpiCatalog.SalesOutId,
                        SupplierId = "SUPA",
                        SupplierName = "Alpha",
                        SalesOutAmount = 1110m,
                        LineCount = 4,
                        SortOrder = 1
                    }
                }
            };

            var result = _composer.Compose(new[]
            {
                Month(2026, 9, "SUPA", "Alpha", 999m, 9),
                Month(2026, 9, "SUPB", "Beta", 50m, 1),
                Month(2026, 9, "   ", "", 80m, 1),
                Month(2026, 8, "SUPA", "Alpha", 400m, 1),
                Month(2025, 9, "SUPA", "Alpha", 250m, 1)
            }, current, GeneratedAt);

            var currentMonth = result.Months.Where(row => row.PeriodYear == 2026 && row.PeriodMonth == 9).ToList();
            currentMonth.Should().ContainSingle();
            currentMonth[0].SupplierId.Should().Be("SUPA");
            currentMonth[0].SalesOutAmount.Should().Be(1110m);
            currentMonth[0].SalesOutAmount.Should().Be(current.Principals.Single().SalesOutAmount);
            currentMonth[0].LineCount.Should().Be(4);
            result.Months.Should().NotContain(row => string.IsNullOrWhiteSpace(row.SupplierId));
            result.Months.Should().NotContain(row => row.PeriodYear == 2026 && row.PeriodMonth == 9 && row.SupplierId == "SUPB");
            result.Months.Should().ContainSingle(row => row.PeriodYear == 2026 && row.PeriodMonth == 8)
                .Which.SalesOutAmount.Should().Be(400m);
            result.Months.Should().ContainSingle(row => row.PeriodYear == 2025 && row.PeriodMonth == 9)
                .Which.SalesOutAmount.Should().Be(250m);
        }

        [Fact]
        public void HistoryEvidenceAndWriter_DoNotRequireInvoiceTimeSupplierId_AndDoNotReadPurchasingOrReturns()
        {
            var evidenceSql = PrincipalSalesOutHistoryEvidenceDal.ListMonthlySalesOutHistorySql;
            evidenceSql.Should().Contain("fi.SubTotal");
            evidenceSql.Should().Contain("fi.DiscRp");
            evidenceSql.Should().Contain("b.SupplierId");
            evidenceSql.Should().Contain("BTR_Brg");
            evidenceSql.Should().Contain("f.VoidDate = '3000-01-01'");
            evidenceSql.Should().NotContain("fi.SupplierId");
            evidenceSql.Should().NotContain("BTR_FakturItem.SupplierId");
            evidenceSql.Should().NotContain("Purchasing");
            evidenceSql.Should().NotContain("BTRPD_Purchasing");
            evidenceSql.Should().NotContain("Retur");
            evidenceSql.Should().NotContain("PRN-RET");
            evidenceSql.Should().NotContain("GrandTotal");
            evidenceSql.Should().NotContain("PpnRp");
            evidenceSql.Should().NotContain("DppRp");

            var writerSql = string.Join(
                " ",
                PrincipalSalesOutHistoryDal.WrittenTables,
                PrincipalSalesOutHistoryDal.DeleteHistorySql,
                PrincipalSalesOutHistoryDal.MergeHeaderSql,
                PrincipalSalesOutHistoryDal.InsertHistorySql);
            writerSql.Should().Contain("BTRPD_PrincipalSalesOutHistory");
            writerSql.Should().Contain("HistoricalLimitation");
            writerSql.Should().Contain("KpiId");
            writerSql.Should().NotContain("PRN-RET");
            writerSql.Should().NotContain("Retur");
            writerSql.Should().NotContain("BTRPD_PrincipalSalesOutDataQuality");
            writerSql.Should().NotContain("BTR_FakturItem");
            PrincipalSalesOutHistory.HistoricalLimitation.Should().Be(
                "Item Principal is current Item master and historical reconstruction may be limited.");
        }

        private static PrincipalSalesOutHistoryMonthEvidence Month(
            int year,
            int month,
            string supplierId,
            string supplierName,
            decimal salesOutAmount,
            int lineCount)
        {
            return new PrincipalSalesOutHistoryMonthEvidence
            {
                PeriodYear = year,
                PeriodMonth = month,
                SupplierId = supplierId,
                SupplierName = supplierName,
                SalesOutAmount = salesOutAmount,
                LineCount = lineCount
            };
        }
    }
}
