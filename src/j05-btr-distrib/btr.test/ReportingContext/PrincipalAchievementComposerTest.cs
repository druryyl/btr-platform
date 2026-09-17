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
    public class PrincipalAchievementComposerTest
    {
        private static readonly DateTime GeneratedAt = new DateTime(2026, 9, 9, 8, 0, 0);

        private readonly PrincipalAchievementComposer _composer = new PrincipalAchievementComposer();

        [Fact]
        public void Compose_PresentsStoredSalesOutVersusStoredTarget_WhenBothSourcesExist()
        {
            var result = _composer.Compose(
                Targets(2026, 9, TargetRow("SUPA", "Alpha", 800m), TargetRow("SUPB", "Beta", 50m)),
                SalesOut(2026, 9, SalesRow("SUPA", "Alpha", 1000m), SalesRow("SUPB", "Beta", 40m)),
                GeneratedAt);

            result.AchievementAmountKpiId.Should().Be("PRN-TGT-002");
            result.AchievementPercentageKpiId.Should().Be("PRN-TGT-003");
            result.SalesOutKpiId.Should().Be("PRN-SALES-001");
            result.TargetKpiId.Should().Be("PRN-TGT-001");
            result.PeriodYear.Should().Be(2026);
            result.PeriodMonth.Should().Be(9);

            var alpha = result.Principals.Single(row => row.SupplierId == "SUPA");
            alpha.AchievementAmountKpiId.Should().Be("PRN-TGT-002");
            alpha.AchievementPercentageKpiId.Should().Be("PRN-TGT-003");
            alpha.SalesOutAmount.Should().Be(1000m);
            alpha.TargetAmount.Should().Be(800m);
            alpha.AchievementAmount.Should().Be(200m);
            alpha.AchievementPercentage.Should().Be(1.250000m);

            var beta = result.Principals.Single(row => row.SupplierId == "SUPB");
            beta.AchievementAmount.Should().Be(-10m);
            beta.AchievementPercentage.Should().Be(0.800000m);
        }

        [Fact]
        public void Compose_ReturnsNull_WhenStoredTargetIsNotGreaterThanZero()
        {
            var result = _composer.Compose(
                Targets(
                    2026,
                    9,
                    TargetRow("ZERO", "Zero", 0m),
                    TargetRow("NEG", "Negative", -5m),
                    TargetRow("MISS", "Missing", 100m)),
                SalesOut(
                    2026, 9,
                    SalesRow("ZERO", "Zero", 40m),
                    SalesRow("NEG", "Negative", 10m)),
                GeneratedAt);

            result.Principals.Single(row => row.SupplierId == "ZERO").AchievementAmount.Should().BeNull();
            result.Principals.Single(row => row.SupplierId == "ZERO").AchievementPercentage.Should().BeNull();
            result.Principals.Single(row => row.SupplierId == "NEG").AchievementAmount.Should().BeNull();
            result.Principals.Single(row => row.SupplierId == "NEG").AchievementPercentage.Should().BeNull();
            var missing = result.Principals.Single(row => row.SupplierId == "MISS");
            missing.SalesOutAmount.Should().BeNull();
            missing.TargetAmount.Should().Be(100m);
            missing.AchievementAmount.Should().BeNull();
            missing.AchievementPercentage.Should().BeNull();
        }

        [Fact]
        public void Compose_IncludesSalesOnlyPrincipal_WithNullAchievement()
        {
            var result = _composer.Compose(
                Targets(2026, 9, TargetRow("SUPA", "Alpha", 800m)),
                SalesOut(2026, 9, SalesRow("SUPA", "Alpha", 1000m), SalesRow("SUPC", "Charlie", 300m)),
                GeneratedAt);

            var charlie = result.Principals.Single(row => row.SupplierId == "SUPC");
            charlie.SalesOutAmount.Should().Be(300m);
            charlie.TargetAmount.Should().BeNull();
            charlie.AchievementAmount.Should().BeNull();
            charlie.AchievementPercentage.Should().BeNull();
        }

        [Fact]
        public void Compose_DoesNotUseSalesOutFromADifferentPeriod()
        {
            var result = _composer.Compose(
                Targets(2026, 9, TargetRow("SUPA", "Alpha", 800m)),
                SalesOut(2026, 8, SalesRow("SUPA", "Alpha", 1000m)),
                GeneratedAt);

            var alpha = result.Principals.Should().ContainSingle().Subject;
            alpha.TargetAmount.Should().Be(800m);
            alpha.SalesOutAmount.Should().BeNull();
            alpha.AchievementAmount.Should().BeNull();
            alpha.AchievementPercentage.Should().BeNull();
        }

        [Fact]
        public void PersistAchievement_ReadsStoredSources_AndDoesNotUpdateThoseRows()
        {
            var salesOut = new RecordingSalesOutSnapshotDal(SalesOut(2026, 9, SalesRow("SUPA", "Alpha", 999.25m)));
            var targets = new RecordingTargetSnapshotDal(Targets(2026, 9, TargetRow("SUPA", "Alpha", 800m)));
            var achievementDal = new RecordingAchievementSnapshotDal();
            var storedSalesOut = salesOut.GetAmount("SUPA");
            var storedTarget = targets.GetAmount("SUPA");

            var worker = new RefreshPrincipalAchievementSnapshotWorker(
                targets,
                salesOut,
                new PrincipalAchievementComposer(),
                achievementDal,
                new StubRefreshLogDal(),
                new StubTglJamDal(GeneratedAt));

            worker.Execute(new RefreshPrincipalAchievementSnapshotRequest { TriggeredBy = "Manual" });

            salesOut.GetAmount("SUPA").Should().Be(storedSalesOut);
            targets.GetAmount("SUPA").Should().Be(storedTarget);
            salesOut.WriteCount.Should().Be(0);
            targets.WriteCount.Should().Be(0);
            achievementDal.WriteCount.Should().Be(1);
            achievementDal.LastResult.AchievementAmountKpiId.Should().Be("PRN-TGT-002");
            achievementDal.LastResult.AchievementPercentageKpiId.Should().Be("PRN-TGT-003");
            achievementDal.LastResult.Principals.Should().ContainSingle()
                .Which.AchievementAmount.Should().Be(
                    PrincipalAchievementComposer.CalculateAmount(999.25m, 800m));
            achievementDal.LastResult.Principals.Should().ContainSingle()
                .Which.AchievementPercentage.Should().Be(
                    PrincipalAchievementComposer.CalculatePercentage(999.25m, 800m));

            var writerSql = string.Join(
                " ",
                PrincipalAchievementSnapshotDal.WrittenTables,
                PrincipalAchievementSnapshotDal.DeletePrincipalSql,
                PrincipalAchievementSnapshotDal.MergeKpiSql,
                PrincipalAchievementSnapshotDal.InsertPrincipalSql);
            writerSql.Should().Contain("BTRPD_PrincipalAchievement");
            writerSql.Should().Contain("AchievementAmount");
            writerSql.Should().Contain("AchievementPercentage");
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
        }

        private static PrincipalTargetAggregateResult Targets(
            int year,
            int month,
            params PrincipalTargetRow[] rows)
        {
            return new PrincipalTargetAggregateResult
            {
                KpiId = PrincipalKpiCatalog.TargetId,
                PeriodYear = year,
                PeriodMonth = month,
                GeneratedAt = GeneratedAt,
                Principals = rows.ToList()
            };
        }

        private static PrincipalSalesOutAggregateResult SalesOut(
            int year,
            int month,
            params PrincipalSalesOutRow[] rows)
        {
            return new PrincipalSalesOutAggregateResult
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                PeriodYear = year,
                PeriodMonth = month,
                GeneratedAt = GeneratedAt,
                Principals = rows.ToList()
            };
        }

        private static PrincipalTargetRow TargetRow(string supplierId, string supplierName, decimal targetAmount)
        {
            return new PrincipalTargetRow
            {
                KpiId = PrincipalKpiCatalog.TargetId,
                SupplierId = supplierId,
                SupplierName = supplierName,
                TargetAmount = targetAmount
            };
        }

        private static PrincipalSalesOutRow SalesRow(string supplierId, string supplierName, decimal salesOutAmount)
        {
            return new PrincipalSalesOutRow
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                SupplierId = supplierId,
                SupplierName = supplierName,
                SalesOutAmount = salesOutAmount
            };
        }

        private sealed class RecordingSalesOutSnapshotDal : IPrincipalSalesOutSnapshotDal
        {
            private readonly PrincipalSalesOutAggregateResult _current;

            public RecordingSalesOutSnapshotDal(PrincipalSalesOutAggregateResult current)
            {
                _current = current;
            }

            public int WriteCount { get; private set; }

            public PrincipalSalesOutAggregateResult GetCurrent()
            {
                return _current;
            }

            public void ReplaceCurrent(PrincipalSalesOutAggregateResult result, string refreshLogId)
            {
                WriteCount++;
            }

            public decimal GetAmount(string supplierId)
            {
                return _current.Principals.Single(row => row.SupplierId == supplierId).SalesOutAmount;
            }
        }

        private sealed class RecordingTargetSnapshotDal : IPrincipalTargetSnapshotDal
        {
            private readonly PrincipalTargetAggregateResult _current;

            public RecordingTargetSnapshotDal(PrincipalTargetAggregateResult current)
            {
                _current = current;
            }

            public int WriteCount { get; private set; }

            public PrincipalTargetAggregateResult GetCurrent()
            {
                return _current;
            }

            public void ReplaceCurrent(PrincipalTargetAggregateResult result, string refreshLogId)
            {
                WriteCount++;
            }

            public decimal GetAmount(string supplierId)
            {
                return _current.Principals.Single(row => row.SupplierId == supplierId).TargetAmount;
            }
        }

        private sealed class RecordingAchievementSnapshotDal : IPrincipalAchievementSnapshotDal
        {
            public int WriteCount { get; private set; }

            public PrincipalAchievementResult LastResult { get; private set; }

            public PrincipalAchievementResult GetCurrent()
            {
                return LastResult;
            }

            public void ReplaceCurrent(PrincipalAchievementResult result, string refreshLogId)
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
    }
}
