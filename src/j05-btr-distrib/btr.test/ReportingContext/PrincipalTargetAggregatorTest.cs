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
    public class PrincipalTargetAggregatorTest
    {
        private static readonly DateTime GeneratedAt = new DateTime(2026, 9, 9, 8, 0, 0);

        private readonly PrincipalTargetAggregator _aggregator = new PrincipalTargetAggregator();

        [Fact]
        public void Aggregate_SumsSalesmanPrincipalTargets_ForTheSamePrincipalAndMonth()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Target("SP01", "SUPA", "Alpha", 100m),
                Target("SP02", "SUPA", "Alpha", 250m),
                Target("SP01", "SUPB", "Beta", 40m)
            }, 2026, 9, GeneratedAt);

            result.KpiId.Should().Be(PrincipalKpiCatalog.TargetId);
            result.PeriodYear.Should().Be(2026);
            result.PeriodMonth.Should().Be(9);
            result.Principals.Should().HaveCount(2);
            result.Principals.Should().ContainSingle(row => row.SupplierId == "SUPA")
                .Which.TargetAmount.Should().Be(350m);
            result.Principals.Should().ContainSingle(row => row.SupplierId == "SUPA")
                .Which.SourceCount.Should().Be(2);
            result.Principals.Should().ContainSingle(row => row.SupplierId == "SUPB")
                .Which.TargetAmount.Should().Be(40m);
            result.Principals.Should().OnlyContain(row => row.KpiId == PrincipalKpiCatalog.TargetId);
        }

        [Fact]
        public void Aggregate_DoesNotCreateAPrincipalRow_WhenNoSalesmanTargetExists()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Target("SP01", "SUPA", "Alpha", 80m)
            }, 2026, 9, GeneratedAt);

            result.Principals.Should().ContainSingle()
                .Which.SupplierId.Should().Be("SUPA");
            result.Principals.Should().NotContain(row => row.SupplierId == "SUPC");
        }

        [Fact]
        public void Aggregate_SkipsBlankPrincipal_AndDoesNotInventAPrincipalTarget()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Target("SP01", "SUPA", "Alpha", 80m),
                Target("SP02", "   ", "", 15m)
            }, 2026, 9, GeneratedAt);

            result.Principals.Should().ContainSingle()
                .Which.SupplierId.Should().Be("SUPA");
            result.Principals.Should().NotContain(row => string.IsNullOrWhiteSpace(row.SupplierId));
        }

        [Fact]
        public void Aggregate_DoesNotWriteSalesOutAchievementOrReturnValues()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Target("SP01", "SUPA", "Alpha", 80m)
            }, 2026, 9, GeneratedAt);

            result.KpiId.Should().Be("PRN-TGT-001");
            result.Principals.Should().OnlyContain(row => row.KpiId == "PRN-TGT-001");

            var resultProperties = typeof(btr.application.ReportingContext.PrincipalAnalyticsAgg.Models.PrincipalTargetAggregateResult)
                .GetProperties()
                .Select(property => property.Name);
            resultProperties.Should().NotContain("SalesOutAmount");
            resultProperties.Should().NotContain(name => name.IndexOf("Ret", StringComparison.OrdinalIgnoreCase) >= 0);
            resultProperties.Should().NotContain(name => name.IndexOf("Achievement", StringComparison.OrdinalIgnoreCase) >= 0);

            var evidenceSql = PrincipalTargetEvidenceDal.ListSalesmanPrincipalTargetsSql;
            evidenceSql.Should().Contain("BTR_SalesPersonPrincipalTarget");
            evidenceSql.Should().Contain("TargetAmount");
            evidenceSql.Should().NotContain("BTR_SalesPersonSupplier");
            evidenceSql.Should().NotContain("PRN-SALES-001");
            evidenceSql.Should().NotContain("PRN-TGT-002");
            evidenceSql.Should().NotContain("PRN-TGT-003");
            evidenceSql.Should().NotContain("PRN-RET");
            evidenceSql.Should().NotContain("Retur");

            var writerSql = string.Join(
                " ",
                PrincipalTargetSnapshotDal.WrittenTables,
                PrincipalTargetSnapshotDal.DeletePrincipalSql,
                PrincipalTargetSnapshotDal.MergeKpiSql,
                PrincipalTargetSnapshotDal.InsertPrincipalSql);
            writerSql.Should().Contain("BTRPD_PrincipalTarget");
            writerSql.Should().Contain("KpiId");
            writerSql.Should().NotContain("BTR_SalesPersonPrincipalTarget");
            writerSql.Should().NotContain("BTR_SalesPersonSupplier");
            writerSql.Should().NotContain("BTRPD_PrincipalSalesOut");
            writerSql.Should().NotContain("PRN-SALES-001");
            writerSql.Should().NotContain("PRN-TGT-002");
            writerSql.Should().NotContain("PRN-TGT-003");
            writerSql.Should().NotContain("PRN-RET");
        }

        private static SalesmanPrincipalTargetEvidence Target(
            string salesPersonId,
            string supplierId,
            string supplierName,
            decimal targetAmount)
        {
            return new SalesmanPrincipalTargetEvidence
            {
                SalesPersonId = salesPersonId,
                SupplierId = supplierId,
                SupplierName = supplierName,
                TargetYear = 2026,
                TargetMonth = 9,
                TargetAmount = targetAmount
            };
        }
    }
}
