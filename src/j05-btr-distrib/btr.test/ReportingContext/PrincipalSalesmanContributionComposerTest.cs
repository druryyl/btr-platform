using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.DashboardSnapshotAgg.Contracts;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;
using btr.application.ReportingContext.PrincipalAnalyticsAgg;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Models;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.Services;
using btr.application.ReportingContext.PrincipalAnalyticsAgg.UseCases;
using btr.application.SupportContext.TglJamAgg;
using btr.infrastructure.ReportingContext.PrincipalAnalyticsAgg;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class PrincipalSalesmanContributionComposerTest
    {
        private static readonly DateTime GeneratedAt = new DateTime(2026, 9, 9, 8, 0, 0);

        private readonly PrincipalSalesmanContributionComposer _composer = new PrincipalSalesmanContributionComposer();

        [Fact]
        public void Compose_GroupsByPrincipalAndFakturSalesman_WithSubTotalMinusDiscRp()
        {
            var result = _composer.Compose(
                new[]
                {
                    Line("F1", "FI1", "SP1", "SUPA", 100_000m, 10_000m),
                    Line("F1", "FI2", "SP1", "SUPA", 50_000m, 0m),
                    Line("F2", "FI3", "SP2", "SUPA", 200_000m, 20_000m),
                    Line("F3", "FI4", "SP1", "SUPB", 70_000m, 5_000m)
                },
                new[]
                {
                    Target("SP1", "SUPA", 2026, 9),
                    Target("SP2", "SUPA", 2026, 9),
                    Target("SP1", "SUPB", 2026, 9)
                },
                2026,
                9,
                GeneratedAt);

            result.SourceSalesOutKpiId.Should().Be("PRN-SALES-001");
            result.PeriodYear.Should().Be(2026);
            result.PeriodMonth.Should().Be(9);
            result.Contributions.Should().HaveCount(3);

            var alphaSp1 = result.Contributions
                .Single(row => row.SupplierId == "SUPA" && row.SalesPersonId == "SP1");
            alphaSp1.ContributionAmount.Should().Be(140_000m);
            alphaSp1.LineCount.Should().Be(2);
            alphaSp1.HasTargetResponsibility.Should().BeTrue();
            alphaSp1.SourceSalesOutKpiId.Should().Be("PRN-SALES-001");

            var alphaSp2 = result.Contributions
                .Single(row => row.SupplierId == "SUPA" && row.SalesPersonId == "SP2");
            alphaSp2.ContributionAmount.Should().Be(180_000m);
            alphaSp2.HasTargetResponsibility.Should().BeTrue();

            result.Exceptions.Should().BeEmpty();
        }

        [Fact]
        public void Compose_ListsMissingTargetPairAsException_WithoutDroppingTheSale()
        {
            var result = _composer.Compose(
                new[]
                {
                    Line("F1", "FI1", "SP1", "SUPA", 100_000m, 0m),
                    Line("F2", "FI2", "SP9", "SUPA", 40_000m, 0m)
                },
                new[] { Target("SP1", "SUPA", 2026, 9) },
                2026,
                9,
                GeneratedAt);

            result.Contributions.Should().HaveCount(2);

            var missing = result.Contributions
                .Single(row => row.SalesPersonId == "SP9");
            missing.ContributionAmount.Should().Be(40_000m);
            missing.HasTargetResponsibility.Should().BeFalse();

            var exception = result.Exceptions.Should().ContainSingle().Subject;
            exception.SupplierId.Should().Be("SUPA");
            exception.SalesPersonId.Should().Be("SP9");
            exception.ContributionAmount.Should().Be(40_000m);
            exception.LineCount.Should().Be(1);
            exception.TargetYear.Should().Be(2026);
            exception.TargetMonth.Should().Be(9);
        }

        [Fact]
        public void Compose_IgnoresTargetFromADifferentMonth_WhenCheckingResponsibility()
        {
            var result = _composer.Compose(
                new[] { Line("F1", "FI1", "SP1", "SUPA", 100_000m, 0m) },
                new[] { Target("SP1", "SUPA", 2026, 8) },
                2026,
                9,
                GeneratedAt);

            result.Contributions.Should().ContainSingle()
                .Which.HasTargetResponsibility.Should().BeFalse();
            result.Exceptions.Should().ContainSingle();
        }

        [Fact]
        public void Compose_ExcludesBlankAndUnknownPrincipalLines_FromContribution()
        {
            var result = _composer.Compose(
                new[]
                {
                    Line("F1", "FI1", "SP1", "SUPA", 100_000m, 0m),
                    BlankPrincipalLine("F2", "FI2", "SP1", 30_000m),
                    UnknownPrincipalLine("F3", "FI3", "SP1", 20_000m)
                },
                new[] { Target("SP1", "SUPA", 2026, 9) },
                2026,
                9,
                GeneratedAt);

            result.Contributions.Should().ContainSingle()
                .Which.ContributionAmount.Should().Be(100_000m);
            result.Exceptions.Should().BeEmpty();
        }

        [Fact]
        public void Compose_DoesNotMintARegistryKpi_ForContribution()
        {
            var result = _composer.Compose(
                new[] { Line("F1", "FI1", "SP1", "SUPA", 100_000m, 0m) },
                new[] { Target("SP1", "SUPA", 2026, 9) },
                2026,
                9,
                GeneratedAt);

            var serialized = string.Join(
                " ",
                result.SourceSalesOutKpiId,
                result.Contributions.Select(row => row.SourceSalesOutKpiId));
            serialized.Should().NotContain("PRN-RET-");
            serialized.Should().NotContain("PRN-TGT-");
            serialized.Should().NotContain("PRN-PUR-");
            serialized.Should().NotContain("PRN-INV-");
            serialized.Should().NotContain("PRN-CUS-");
            serialized.Should().NotContain("PRN-GRW-");
            serialized.Should().NotContain("PR-KPI-");
            serialized.Should().NotContain("CP-KPI-");
            serialized.Should().NotContain("Net Sales");

            PrincipalKpiCatalog.TryGet("PRN-SALES-001", out _).Should().BeTrue();
            result.Contributions.Should().OnlyContain(row =>
                string.Equals(row.SourceSalesOutKpiId, PrincipalKpiCatalog.SalesOutId, StringComparison.Ordinal));
        }

        [Fact]
        public void PersistContribution_WritesOnlyContributionTables_AndLeavesSalesOutUntouched()
        {
            var evidence = new RecordingContributionEvidenceDal(new[]
            {
                Line("F1", "FI1", "SP1", "SUPA", 100_000m, 10_000m),
                Line("F2", "FI2", "SP9", "SUPA", 40_000m, 0m)
            });
            var targets = new RecordingTargetEvidenceDal(new[]
            {
                Target("SP1", "SUPA", 2026, 9)
            });
            var snapshotDal = new RecordingContributionSnapshotDal();

            var worker = new RefreshPrincipalSalesmanContributionSnapshotWorker(
                evidence,
                targets,
                new PrincipalSalesmanContributionComposer(),
                snapshotDal,
                new StubRefreshLogDal(),
                new StubTglJamDal(GeneratedAt),
                new StubBusinessDateProvider(new DateTime(2026, 9, 9)));

            worker.Execute(new RefreshPrincipalSalesmanContributionSnapshotRequest { TriggeredBy = "Manual" });

            snapshotDal.WriteCount.Should().Be(1);
            snapshotDal.LastResult.Contributions.Should().HaveCount(2);
            snapshotDal.LastResult.Exceptions.Should().ContainSingle()
                .Which.SalesPersonId.Should().Be("SP9");
            snapshotDal.LastResult.SourceSalesOutKpiId.Should().Be("PRN-SALES-001");

            var writerSql = string.Join(
                " ",
                PrincipalSalesmanContributionSnapshotDal.WrittenTables,
                PrincipalSalesmanContributionSnapshotDal.DeleteContributionSql,
                PrincipalSalesmanContributionSnapshotDal.DeleteExceptionSql,
                PrincipalSalesmanContributionSnapshotDal.MergeKpiSql,
                PrincipalSalesmanContributionSnapshotDal.InsertContributionSql,
                PrincipalSalesmanContributionSnapshotDal.InsertExceptionSql);
            writerSql.Should().Contain("BTRPD_PrincipalContribution");
            writerSql.Should().Contain("BTRPD_PrincipalContributionException");
            writerSql.Should().NotContain("BTRPD_PrincipalSalesOut");
            writerSql.Should().NotContain("BTRPD_PrincipalTarget ");
            writerSql.Should().NotContain("INSERT INTO BTRPD_PrincipalSalesOut");
            writerSql.Should().NotContain("UPDATE BTRPD_PrincipalSalesOut");
            writerSql.Should().NotContain("DELETE FROM BTRPD_PrincipalSalesOut");
            writerSql.Should().NotContain("INSERT INTO BTRPD_PrincipalTarget ");
            writerSql.Should().NotContain("UPDATE BTRPD_PrincipalTarget ");
            writerSql.Should().NotContain("DELETE FROM BTRPD_PrincipalTarget ");
            writerSql.Should().NotContain("PRN-RET-");
            writerSql.Should().NotContain("Net Sales");

            var evidenceSql = PrincipalContributionEvidenceDal.ListContributionEvidenceSql;
            evidenceSql.Should().Contain("f.SalesPersonId");
            evidenceSql.Should().Contain("fi.SubTotal");
            evidenceSql.Should().Contain("fi.DiscRp");
            evidenceSql.Should().Contain("b.SupplierId");
            evidenceSql.Should().Contain("f.VoidDate = '3000-01-01'");
            evidenceSql.Should().NotContain("Retur");
        }

        private static PrincipalContributionEvidenceLine Line(
            string fakturId,
            string fakturItemId,
            string salesPersonId,
            string supplierId,
            decimal subTotal,
            decimal discRp)
        {
            return new PrincipalContributionEvidenceLine
            {
                FakturId = fakturId,
                FakturItemId = fakturItemId,
                SalesPersonId = salesPersonId,
                SalesPersonCode = "CODE-" + salesPersonId,
                SalesPersonName = "Name-" + salesPersonId,
                ItemSupplierId = supplierId,
                SupplierId = supplierId,
                SupplierName = "Principal-" + supplierId,
                SubTotal = subTotal,
                DiscRp = discRp
            };
        }

        private static PrincipalContributionEvidenceLine BlankPrincipalLine(
            string fakturId,
            string fakturItemId,
            string salesPersonId,
            decimal subTotal)
        {
            var line = Line(fakturId, fakturItemId, salesPersonId, string.Empty, subTotal, 0m);
            line.ItemSupplierId = string.Empty;
            line.SupplierId = string.Empty;
            return line;
        }

        private static PrincipalContributionEvidenceLine UnknownPrincipalLine(
            string fakturId,
            string fakturItemId,
            string salesPersonId,
            decimal subTotal)
        {
            var line = Line(fakturId, fakturItemId, salesPersonId, "SUPX", subTotal, 0m);
            line.SupplierId = string.Empty;
            return line;
        }

        private static SalesmanPrincipalTargetEvidence Target(
            string salesPersonId,
            string supplierId,
            int year,
            int month)
        {
            return new SalesmanPrincipalTargetEvidence
            {
                SalesPersonId = salesPersonId,
                SupplierId = supplierId,
                TargetYear = year,
                TargetMonth = month,
                TargetAmount = 1000m
            };
        }

        private sealed class RecordingContributionEvidenceDal : IPrincipalContributionEvidenceDal
        {
            private readonly IReadOnlyList<PrincipalContributionEvidenceLine> _lines;

            public RecordingContributionEvidenceDal(IReadOnlyList<PrincipalContributionEvidenceLine> lines)
            {
                _lines = lines;
            }

            public IReadOnlyList<PrincipalContributionEvidenceLine> ListContributionEvidence(btr.nuna.Domain.Periode periode)
            {
                return _lines;
            }
        }

        private sealed class RecordingTargetEvidenceDal : IPrincipalTargetEvidenceDal
        {
            private readonly IReadOnlyList<SalesmanPrincipalTargetEvidence> _targets;

            public RecordingTargetEvidenceDal(IReadOnlyList<SalesmanPrincipalTargetEvidence> targets)
            {
                _targets = targets;
            }

            public IReadOnlyList<SalesmanPrincipalTargetEvidence> ListSalesmanPrincipalTargets(int year, int month)
            {
                return _targets;
            }
        }

        private sealed class RecordingContributionSnapshotDal : IPrincipalSalesmanContributionSnapshotDal
        {
            public int WriteCount { get; private set; }

            public PrincipalSalesmanContributionResult LastResult { get; private set; }

            public PrincipalSalesmanContributionResult GetCurrent()
            {
                return LastResult;
            }

            public void ReplaceCurrent(PrincipalSalesmanContributionResult result, string refreshLogId)
            {
                LastResult = result;
                WriteCount++;
            }
        }

        private sealed class StubRefreshLogDal : IDashboardSnapshotRefreshLogDal
        {
            public void InsertRunning(DashboardSnapshotRefreshLogModel model)
            {
            }

            public void MarkSuccess(string refreshLogId, int durationMs)
            {
            }

            public void MarkFailed(string refreshLogId, int durationMs, string errorMessage)
            {
            }

            public IReadOnlyList<DashboardSnapshotRefreshStatusModel> GetLatestPerDomain()
            {
                return new List<DashboardSnapshotRefreshStatusModel>();
            }
        }

        private sealed class StubTglJamDal : ITglJamDal
        {
            public StubTglJamDal(DateTime now)
            {
                Now = now;
            }

            public DateTime Now { get; }
        }

        private sealed class StubBusinessDateProvider : btr.application.Portal.IBusinessDateProvider
        {
            private readonly DateTime _today;

            public StubBusinessDateProvider(DateTime today)
            {
                _today = today;
            }

            public DateTime Today { get { return _today.Date; } }

            public bool IsPresentationActive { get { return false; } }
        }
    }
}
