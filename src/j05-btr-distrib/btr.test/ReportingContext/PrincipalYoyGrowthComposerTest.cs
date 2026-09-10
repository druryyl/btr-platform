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
    public class PrincipalYoyGrowthComposerTest
    {
        private static readonly DateTime GeneratedAt = new DateTime(2026, 9, 10, 8, 0, 0);

        private readonly PrincipalYoyGrowthComposer _composer = new PrincipalYoyGrowthComposer();

        [Fact]
        public void Compose_DividesCurrentMinusPriorYearByPriorYear_WhenPriorYearMonthIsGreaterThanZero()
        {
            var result = _composer.Compose(
                History(
                    Month(2025, 9, "SUPA", "Alpha", 800m),
                    Month(2026, 9, "SUPA", "Alpha", 1000m),
                    Month(2025, 9, "SUPB", "Beta", 50m),
                    Month(2026, 9, "SUPB", "Beta", 40m)),
                GeneratedAt);

            result.YoyGrowthKpiId.Should().Be("PRN-GRW-002");
            result.SalesOutKpiId.Should().Be("PRN-SALES-001");
            result.PeriodYear.Should().Be(2026);
            result.PeriodMonth.Should().Be(9);
            result.PriorYear.Should().Be(2025);
            result.PriorMonth.Should().Be(9);

            var alpha = result.Principals.Single(row => row.SupplierId == "SUPA");
            alpha.YoyGrowthKpiId.Should().Be("PRN-GRW-002");
            alpha.SalesOutKpiId.Should().Be("PRN-SALES-001");
            alpha.CurrentSalesOutAmount.Should().Be(1000m);
            alpha.PriorSalesOutAmount.Should().Be(800m);
            alpha.YoyGrowthPercentage.Should().Be(0.250000m);

            var beta = result.Principals.Single(row => row.SupplierId == "SUPB");
            beta.CurrentSalesOutAmount.Should().Be(40m);
            beta.PriorSalesOutAmount.Should().Be(50m);
            beta.YoyGrowthPercentage.Should().Be(-0.200000m);
        }

        [Fact]
        public void Compose_ReturnsNull_WhenPriorYearMonthIsNotGreaterThanZero()
        {
            var result = _composer.Compose(
                History(
                    Month(2025, 9, "ZERO", "Zero", 0m),
                    Month(2026, 9, "ZERO", "Zero", 40m),
                    Month(2025, 9, "NEG", "Negative", -5m),
                    Month(2026, 9, "NEG", "Negative", 10m),
                    Month(2026, 9, "MISS", "Missing", 100m)),
                GeneratedAt);

            result.Principals.Single(row => row.SupplierId == "ZERO").YoyGrowthPercentage.Should().BeNull();
            result.Principals.Single(row => row.SupplierId == "NEG").YoyGrowthPercentage.Should().BeNull();
            var missing = result.Principals.Single(row => row.SupplierId == "MISS");
            missing.CurrentSalesOutAmount.Should().Be(100m);
            missing.PriorSalesOutAmount.Should().BeNull();
            missing.YoyGrowthPercentage.Should().BeNull();
        }

        [Fact]
        public void Compose_DoesNotUseNonAdjacentYear()
        {
            var result = _composer.Compose(
                History(
                    Month(2024, 9, "SUPA", "Alpha", 1000m),
                    Month(2026, 9, "SUPA", "Alpha", 1200m)),
                GeneratedAt);

            var alpha = result.Principals.Should().ContainSingle().Subject;
            alpha.CurrentSalesOutAmount.Should().Be(1200m);
            alpha.PriorSalesOutAmount.Should().BeNull();
            alpha.YoyGrowthPercentage.Should().BeNull();
        }

        [Fact]
        public void Compose_UsesSameMonthPriorYear_NotAdjacentMonth()
        {
            var result = _composer.Compose(
                History(
                    Month(2025, 9, "SUPA", "Alpha", 500m),
                    Month(2026, 8, "SUPA", "Alpha", 9999m),
                    Month(2026, 9, "SUPA", "Alpha", 600m)),
                GeneratedAt);

            result.PeriodYear.Should().Be(2026);
            result.PeriodMonth.Should().Be(9);
            result.PriorYear.Should().Be(2025);
            result.PriorMonth.Should().Be(9);
            result.Principals.Should().ContainSingle()
                .Which.YoyGrowthPercentage.Should().Be(0.200000m);
        }

        [Fact]
        public void PersistYoyGrowth_ReadsStoredHistory_AndDoesNotUpdateThoseRows()
        {
            var history = new RecordingSalesOutHistoryDal(History(
                Month(2025, 9, "SUPA", "Alpha", 800m),
                Month(2026, 9, "SUPA", "Alpha", 999.25m)));
            var growthDal = new RecordingYoyGrowthSnapshotDal();
            var storedCount = history.GetHistory().Months.Count;

            var worker = new RefreshPrincipalYoyGrowthSnapshotWorker(
                history,
                new PrincipalYoyGrowthComposer(),
                growthDal,
                new StubRefreshLogDal(),
                new StubTglJamDal(GeneratedAt));

            worker.Execute(new RefreshPrincipalYoyGrowthSnapshotRequest { TriggeredBy = "Manual" });

            history.GetHistory().Months.Should().HaveCount(storedCount);
            history.WriteCount.Should().Be(0);
            growthDal.WriteCount.Should().Be(1);
            growthDal.LastResult.YoyGrowthKpiId.Should().Be("PRN-GRW-002");
            growthDal.LastResult.SalesOutKpiId.Should().Be("PRN-SALES-001");
            growthDal.LastResult.PeriodYear.Should().Be(2026);
            growthDal.LastResult.PeriodMonth.Should().Be(9);
            growthDal.LastResult.PriorYear.Should().Be(2025);
            growthDal.LastResult.PriorMonth.Should().Be(9);
            growthDal.LastResult.Principals.Should().ContainSingle()
                .Which.YoyGrowthPercentage.Should().Be(
                    PrincipalYoyGrowthComposer.Calculate(999.25m, 800m));

            var writerSql = string.Join(
                " ",
                PrincipalYoyGrowthSnapshotDal.WrittenTables,
                PrincipalYoyGrowthSnapshotDal.DeletePrincipalSql,
                PrincipalYoyGrowthSnapshotDal.MergeKpiSql,
                PrincipalYoyGrowthSnapshotDal.InsertPrincipalSql);
            writerSql.Should().Contain("BTRPD_PrincipalYoyGrowth");
            writerSql.Should().Contain("YoyGrowthPercentage");
            writerSql.Should().NotContain("BTRPD_PrincipalSalesOut");
            writerSql.Should().NotContain("BTRPD_PrincipalSalesOutHistory");
            writerSql.Should().NotContain("INSERT INTO BTRPD_PrincipalSalesOut");
            writerSql.Should().NotContain("UPDATE BTRPD_PrincipalSalesOut");
            writerSql.Should().NotContain("DELETE FROM BTRPD_PrincipalSalesOut");
            writerSql.Should().NotContain("PRN-GRW-001");
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

        private sealed class RecordingYoyGrowthSnapshotDal : IPrincipalYoyGrowthSnapshotDal
        {
            public int WriteCount { get; private set; }

            public PrincipalYoyGrowthResult LastResult { get; private set; }

            public PrincipalYoyGrowthResult GetCurrent()
            {
                return LastResult;
            }

            public void ReplaceCurrent(PrincipalYoyGrowthResult result, string refreshLogId)
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
