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
    public class PrincipalMomGrowthComposerTest
    {
        private static readonly DateTime GeneratedAt = new DateTime(2026, 9, 9, 8, 0, 0);

        private readonly PrincipalMomGrowthComposer _composer = new PrincipalMomGrowthComposer();

        [Fact]
        public void Compose_DividesCurrentMinusPriorByPrior_WhenPriorMonthIsGreaterThanZero()
        {
            var result = _composer.Compose(
                History(
                    Month(2026, 8, "SUPA", "Alpha", 800m),
                    Month(2026, 9, "SUPA", "Alpha", 1000m),
                    Month(2026, 8, "SUPB", "Beta", 50m),
                    Month(2026, 9, "SUPB", "Beta", 40m)),
                GeneratedAt);

            result.MomGrowthKpiId.Should().Be("PRN-GRW-001");
            result.SalesOutKpiId.Should().Be("PRN-SALES-001");
            result.PeriodYear.Should().Be(2026);
            result.PeriodMonth.Should().Be(9);
            result.PriorYear.Should().Be(2026);
            result.PriorMonth.Should().Be(8);

            var alpha = result.Principals.Single(row => row.SupplierId == "SUPA");
            alpha.MomGrowthKpiId.Should().Be("PRN-GRW-001");
            alpha.SalesOutKpiId.Should().Be("PRN-SALES-001");
            alpha.CurrentSalesOutAmount.Should().Be(1000m);
            alpha.PriorSalesOutAmount.Should().Be(800m);
            alpha.MomGrowthPercentage.Should().Be(0.250000m);

            var beta = result.Principals.Single(row => row.SupplierId == "SUPB");
            beta.CurrentSalesOutAmount.Should().Be(40m);
            beta.PriorSalesOutAmount.Should().Be(50m);
            beta.MomGrowthPercentage.Should().Be(-0.200000m);
        }

        [Fact]
        public void Compose_ReturnsNull_WhenPriorMonthIsNotGreaterThanZero()
        {
            var result = _composer.Compose(
                History(
                    Month(2026, 8, "ZERO", "Zero", 0m),
                    Month(2026, 9, "ZERO", "Zero", 40m),
                    Month(2026, 8, "NEG", "Negative", -5m),
                    Month(2026, 9, "NEG", "Negative", 10m),
                    Month(2026, 9, "MISS", "Missing", 100m)),
                GeneratedAt);

            result.Principals.Single(row => row.SupplierId == "ZERO").MomGrowthPercentage.Should().BeNull();
            result.Principals.Single(row => row.SupplierId == "NEG").MomGrowthPercentage.Should().BeNull();
            var missing = result.Principals.Single(row => row.SupplierId == "MISS");
            missing.CurrentSalesOutAmount.Should().Be(100m);
            missing.PriorSalesOutAmount.Should().BeNull();
            missing.MomGrowthPercentage.Should().BeNull();
        }

        [Fact]
        public void Compose_DoesNotUseNonAdjacentMonth()
        {
            var result = _composer.Compose(
                History(
                    Month(2026, 7, "SUPA", "Alpha", 1000m),
                    Month(2026, 9, "SUPA", "Alpha", 1200m)),
                GeneratedAt);

            var alpha = result.Principals.Should().ContainSingle().Subject;
            alpha.CurrentSalesOutAmount.Should().Be(1200m);
            alpha.PriorSalesOutAmount.Should().BeNull();
            alpha.MomGrowthPercentage.Should().BeNull();
        }

        [Fact]
        public void Compose_HandlesJanuaryPriorDecember()
        {
            var result = _composer.Compose(
                History(
                    Month(2025, 12, "SUPA", "Alpha", 500m),
                    Month(2026, 1, "SUPA", "Alpha", 600m)),
                GeneratedAt);

            result.PeriodYear.Should().Be(2026);
            result.PeriodMonth.Should().Be(1);
            result.PriorYear.Should().Be(2025);
            result.PriorMonth.Should().Be(12);
            result.Principals.Should().ContainSingle()
                .Which.MomGrowthPercentage.Should().Be(0.200000m);
        }

        [Fact]
        public void PersistMomGrowth_ReadsStoredHistory_AndDoesNotUpdateThoseRows()
        {
            var history = new RecordingSalesOutHistoryDal(History(
                Month(2026, 8, "SUPA", "Alpha", 800m),
                Month(2026, 9, "SUPA", "Alpha", 999.25m)));
            var growthDal = new RecordingMomGrowthSnapshotDal();
            var storedCount = history.GetHistory().Months.Count;

            var worker = new RefreshPrincipalMomGrowthSnapshotWorker(
                history,
                new PrincipalMomGrowthComposer(),
                growthDal,
                new StubRefreshLogDal(),
                new StubTglJamDal(GeneratedAt));

            worker.Execute(new RefreshPrincipalMomGrowthSnapshotRequest { TriggeredBy = "Manual" });

            history.GetHistory().Months.Should().HaveCount(storedCount);
            history.WriteCount.Should().Be(0);
            growthDal.WriteCount.Should().Be(1);
            growthDal.LastResult.MomGrowthKpiId.Should().Be("PRN-GRW-001");
            growthDal.LastResult.SalesOutKpiId.Should().Be("PRN-SALES-001");
            growthDal.LastResult.PeriodYear.Should().Be(2026);
            growthDal.LastResult.PeriodMonth.Should().Be(9);
            growthDal.LastResult.PriorYear.Should().Be(2026);
            growthDal.LastResult.PriorMonth.Should().Be(8);
            growthDal.LastResult.Principals.Should().ContainSingle()
                .Which.MomGrowthPercentage.Should().Be(
                    PrincipalMomGrowthComposer.Calculate(999.25m, 800m));

            var writerSql = string.Join(
                " ",
                PrincipalMomGrowthSnapshotDal.WrittenTables,
                PrincipalMomGrowthSnapshotDal.DeletePrincipalSql,
                PrincipalMomGrowthSnapshotDal.MergeKpiSql,
                PrincipalMomGrowthSnapshotDal.InsertPrincipalSql);
            writerSql.Should().Contain("BTRPD_PrincipalMomGrowth");
            writerSql.Should().Contain("MomGrowthPercentage");
            writerSql.Should().NotContain("BTRPD_PrincipalSalesOut");
            writerSql.Should().NotContain("BTRPD_PrincipalSalesOutHistory");
            writerSql.Should().NotContain("INSERT INTO BTRPD_PrincipalSalesOut");
            writerSql.Should().NotContain("UPDATE BTRPD_PrincipalSalesOut");
            writerSql.Should().NotContain("DELETE FROM BTRPD_PrincipalSalesOut");
            writerSql.Should().NotContain("PRN-GRW-002");
            writerSql.Should().NotContain("PRN-RET-");
            writerSql.Should().NotContain("PRN-PUR-");
            writerSql.Should().NotContain("PRN-INV-");
            writerSql.Should().NotContain("Net Sales");
        }

        private static PrincipalSalesOutHistoryResult History(params PrincipalSalesOutHistoryRow[] rows)
        {
            return new PrincipalSalesOutHistoryResult
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                HistoricalLimitation = PrincipalSalesOutHistory.HistoricalLimitation,
                GeneratedAt = GeneratedAt,
                Months = rows.ToList()
            };
        }

        private static PrincipalSalesOutHistoryRow Month(
            int year, int month, string supplierId, string supplierName, decimal salesOutAmount)
        {
            return new PrincipalSalesOutHistoryRow
            {
                KpiId = PrincipalKpiCatalog.SalesOutId,
                PeriodYear = year,
                PeriodMonth = month,
                SupplierId = supplierId,
                SupplierName = supplierName,
                SalesOutAmount = salesOutAmount
            };
        }

        private sealed class RecordingSalesOutHistoryDal : IPrincipalSalesOutHistoryDal
        {
            private readonly PrincipalSalesOutHistoryResult _history;

            public RecordingSalesOutHistoryDal(PrincipalSalesOutHistoryResult history)
            {
                _history = history;
            }

            public int WriteCount { get; private set; }

            public PrincipalSalesOutHistoryResult GetHistory()
            {
                return _history;
            }

            public void ReplaceHistory(PrincipalSalesOutHistoryResult result, string refreshLogId)
            {
                WriteCount++;
            }
        }

        private sealed class RecordingMomGrowthSnapshotDal : IPrincipalMomGrowthSnapshotDal
        {
            public int WriteCount { get; private set; }

            public PrincipalMomGrowthResult LastResult { get; private set; }

            public PrincipalMomGrowthResult GetCurrent()
            {
                return LastResult;
            }

            public void ReplaceCurrent(PrincipalMomGrowthResult result, string refreshLogId)
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
