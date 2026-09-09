using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.Portal;
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
    public class PrincipalReturnHistoryComposerTest
    {
        private static readonly DateTime GeneratedAt = new DateTime(2026, 9, 9, 8, 0, 0);

        private readonly PrincipalReturnHistoryComposer _composer = new PrincipalReturnHistoryComposer();

        [Fact]
        public void Compose_WritesReturnAmountsOnly_AndOmitsMonthsWithoutSource()
        {
            var result = _composer.Compose(new[]
            {
                Month(2026, 9, "SUPA", "Alpha", 90m, 15m, 2),
                Month(2026, 8, "SUPA", "Alpha", 40m, 0m, 1),
                Month(2025, 9, "SUPB", "Beta", 0m, 25m, 1)
            }, currentSnapshot: null, GeneratedAt);

            result.GoodReturnKpiId.Should().Be(PrincipalKpiCatalog.GoodReturnAmountId);
            result.BrokenReturnKpiId.Should().Be(PrincipalKpiCatalog.BrokenReturnAmountId);
            result.TotalReturnKpiId.Should().Be(PrincipalKpiCatalog.TotalReturnAmountId);
            result.Months.Should().HaveCount(3);
            result.Months.Should().OnlyContain(row =>
                row.GoodReturnKpiId == PrincipalKpiCatalog.GoodReturnAmountId
                && row.BrokenReturnKpiId == PrincipalKpiCatalog.BrokenReturnAmountId
                && row.TotalReturnKpiId == PrincipalKpiCatalog.TotalReturnAmountId
                && row.PeriodYear > 0
                && row.PeriodMonth >= 1
                && row.PeriodMonth <= 12
                && !string.IsNullOrWhiteSpace(row.SupplierId));
            result.Months.Should().ContainSingle(row =>
                row.SupplierId == "SUPA" && row.PeriodYear == 2026 && row.PeriodMonth == 9)
                .Which.TotalReturnAmount.Should().Be(105m);
            result.Months.Should().NotContain(row => row.PeriodYear == 2026 && row.PeriodMonth == 7);

            typeof(PrincipalReturnHistoryResult).GetProperties()
                .Select(property => property.Name)
                .Should().NotContain(name => name.IndexOf("SalesOut", StringComparison.OrdinalIgnoreCase) >= 0);
            typeof(PrincipalReturnHistoryRow).GetProperties()
                .Select(property => property.Name)
                .Should().NotContain(name => name.IndexOf("SalesOut", StringComparison.OrdinalIgnoreCase) >= 0);
            typeof(PrincipalReturnHistoryResult).GetProperties()
                .Select(property => property.Name)
                .Should().NotContain(name => name.IndexOf("Growth", StringComparison.OrdinalIgnoreCase) >= 0);
            typeof(PrincipalReturnHistoryRow).GetProperties()
                .Select(property => property.Name)
                .Should().NotContain(name => name.IndexOf("Growth", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        [Fact]
        public void Compose_CurrentMonthEqualsCurrentSnapshot_AndUnknownIsNotAssignedToHistory()
        {
            var current = new PrincipalReturnAggregateResult
            {
                GoodReturnKpiId = PrincipalKpiCatalog.GoodReturnAmountId,
                BrokenReturnKpiId = PrincipalKpiCatalog.BrokenReturnAmountId,
                TotalReturnKpiId = PrincipalKpiCatalog.TotalReturnAmountId,
                PeriodYear = 2026,
                PeriodMonth = 9,
                Principals =
                {
                    new PrincipalReturnRow
                    {
                        GoodReturnKpiId = PrincipalKpiCatalog.GoodReturnAmountId,
                        BrokenReturnKpiId = PrincipalKpiCatalog.BrokenReturnAmountId,
                        TotalReturnKpiId = PrincipalKpiCatalog.TotalReturnAmountId,
                        SupplierId = "SUPA",
                        SupplierName = "Alpha",
                        GoodReturnAmount = 111m,
                        BrokenReturnAmount = 22m,
                        TotalReturnAmount = 133m,
                        LineCount = 4,
                        SortOrder = 1
                    }
                }
            };

            var result = _composer.Compose(new[]
            {
                Month(2026, 9, "SUPA", "Alpha", 999m, 999m, 9),
                Month(2026, 9, "SUPB", "Beta", 50m, 5m, 1),
                Month(2026, 9, "   ", "", 80m, 8m, 1),
                Month(2026, 8, "SUPA", "Alpha", 40m, 10m, 1),
                Month(2025, 9, "SUPA", "Alpha", 20m, 5m, 1)
            }, current, GeneratedAt);

            var currentMonth = result.Months.Where(row => row.PeriodYear == 2026 && row.PeriodMonth == 9).ToList();
            currentMonth.Should().ContainSingle();
            currentMonth[0].SupplierId.Should().Be("SUPA");
            currentMonth[0].GoodReturnAmount.Should().Be(111m);
            currentMonth[0].BrokenReturnAmount.Should().Be(22m);
            currentMonth[0].TotalReturnAmount.Should().Be(133m);
            currentMonth[0].TotalReturnAmount.Should().Be(current.Principals.Single().TotalReturnAmount);
            currentMonth[0].LineCount.Should().Be(4);
            result.Months.Should().NotContain(row => string.IsNullOrWhiteSpace(row.SupplierId));
            result.Months.Should().NotContain(row => row.PeriodYear == 2026 && row.PeriodMonth == 9 && row.SupplierId == "SUPB");
            result.Months.Should().ContainSingle(row => row.PeriodYear == 2026 && row.PeriodMonth == 8)
                .Which.TotalReturnAmount.Should().Be(50m);
            result.Months.Should().ContainSingle(row => row.PeriodYear == 2025 && row.PeriodMonth == 9)
                .Which.TotalReturnAmount.Should().Be(25m);
        }

        [Fact]
        public void HistoryEvidenceAndWriter_UseReturnItemGrain_AndDoNotWriteSalesOutOrGrowth()
        {
            var evidenceSql = PrincipalReturnHistoryEvidenceDal.ListMonthlyReturnHistorySql;
            evidenceSql.Should().Contain("BTR_ReturJualItem");
            evidenceSql.Should().Contain("BTR_ReturJual");
            evidenceSql.Should().Contain("ri.SubTotal");
            evidenceSql.Should().Contain("ri.DiscRp");
            evidenceSql.Should().Contain("b.SupplierId");
            evidenceSql.Should().Contain("BTR_Brg");
            evidenceSql.Should().Contain("VoidDate = '3000-01-01'");
            evidenceSql.Should().Contain("BAGUS");
            evidenceSql.Should().Contain("RUSAK");
            evidenceSql.Should().NotContain("BTR_Faktur");
            evidenceSql.Should().NotContain("BTRPD_PrincipalSalesOut");
            evidenceSql.Should().NotContain("PRN-SALES-001");
            evidenceSql.Should().NotContain("SalesOutAmount");
            evidenceSql.Should().NotContain("GrandTotal");
            evidenceSql.Should().NotContain("PpnRp");
            evidenceSql.Should().NotContain("PRN-GRW");
            evidenceSql.Should().NotContain("PRN-RET-004");

            var writerSql = string.Join(
                " ",
                PrincipalReturnHistoryDal.WrittenTables,
                PrincipalReturnHistoryDal.DeleteHistorySql,
                PrincipalReturnHistoryDal.MergeHeaderSql,
                PrincipalReturnHistoryDal.InsertHistorySql);
            writerSql.Should().Contain("BTRPD_PrincipalReturnHistory");
            writerSql.Should().Contain("GoodReturnAmount");
            writerSql.Should().Contain("BrokenReturnAmount");
            writerSql.Should().Contain("TotalReturnAmount");
            writerSql.Should().NotContain("BTRPD_PrincipalSalesOut");
            writerSql.Should().NotContain("PRN-SALES-001");
            writerSql.Should().NotContain("SalesOutAmount");
            writerSql.Should().NotContain("PRN-RET-004");
            writerSql.Should().NotContain("ReturnPercentage");
            writerSql.Should().NotContain("Growth");
            writerSql.Should().NotContain("BTR_Faktur");
        }

        [Fact]
        public void PersistReturnHistory_DoesNotChangeSalesOutHistory()
        {
            var salesOutHistory = new RecordingSalesOutHistoryStore();
            salesOutHistory.Store(2026, 9, "SUPA", 999.25m);
            var storedBefore = salesOutHistory.GetAmount(2026, 9, "SUPA");

            var historyDal = new RecordingReturnHistoryDal();
            var worker = new RefreshPrincipalReturnHistoryWorker(
                new StubReturnHistoryEvidenceDal(new[]
                {
                    Month(2026, 9, "SUPA", "Alpha", 90m, 15m, 2)
                }),
                new PrincipalReturnHistoryComposer(),
                new StubCurrentReturnSnapshotDal(new PrincipalReturnAggregateResult
                {
                    GoodReturnKpiId = PrincipalKpiCatalog.GoodReturnAmountId,
                    BrokenReturnKpiId = PrincipalKpiCatalog.BrokenReturnAmountId,
                    TotalReturnKpiId = PrincipalKpiCatalog.TotalReturnAmountId,
                    PeriodYear = 2026,
                    PeriodMonth = 9,
                    Principals =
                    {
                        new PrincipalReturnRow
                        {
                            GoodReturnKpiId = PrincipalKpiCatalog.GoodReturnAmountId,
                            BrokenReturnKpiId = PrincipalKpiCatalog.BrokenReturnAmountId,
                            TotalReturnKpiId = PrincipalKpiCatalog.TotalReturnAmountId,
                            SupplierId = "SUPA",
                            SupplierName = "Alpha",
                            GoodReturnAmount = 90m,
                            BrokenReturnAmount = 15m,
                            TotalReturnAmount = 105m,
                            LineCount = 2,
                            SortOrder = 1
                        }
                    }
                }),
                historyDal,
                new StubRefreshLogDal(),
                new StubTglJamDal(GeneratedAt));

            worker.Execute(new RefreshPrincipalReturnHistoryRequest { TriggeredBy = "Manual" });

            salesOutHistory.GetAmount(2026, 9, "SUPA").Should().Be(storedBefore);
            salesOutHistory.WriteCount.Should().Be(1);
            historyDal.WriteCount.Should().Be(1);
            historyDal.LastResult.Months.Should().ContainSingle()
                .Which.TotalReturnAmount.Should().Be(105m);
            historyDal.LastResult.GoodReturnKpiId.Should().Be("PRN-RET-001");
            historyDal.LastResult.BrokenReturnKpiId.Should().Be("PRN-RET-002");
            historyDal.LastResult.TotalReturnKpiId.Should().Be("PRN-RET-003");
        }

        private static PrincipalReturnHistoryMonthEvidence Month(
            int year,
            int month,
            string supplierId,
            string supplierName,
            decimal goodReturnAmount,
            decimal brokenReturnAmount,
            int lineCount)
        {
            return new PrincipalReturnHistoryMonthEvidence
            {
                PeriodYear = year,
                PeriodMonth = month,
                SupplierId = supplierId,
                SupplierName = supplierName,
                GoodReturnAmount = goodReturnAmount,
                BrokenReturnAmount = brokenReturnAmount,
                TotalReturnAmount = goodReturnAmount + brokenReturnAmount,
                LineCount = lineCount
            };
        }

        private sealed class RecordingSalesOutHistoryStore
        {
            private readonly Dictionary<string, decimal> _amounts =
                new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            public int WriteCount { get; private set; }

            public void Store(int year, int month, string supplierId, decimal salesOutAmount)
            {
                _amounts[Key(year, month, supplierId)] = salesOutAmount;
                WriteCount++;
            }

            public decimal GetAmount(int year, int month, string supplierId)
            {
                return _amounts[Key(year, month, supplierId)];
            }

            private static string Key(int year, int month, string supplierId)
            {
                return year + "-" + month + "-" + supplierId;
            }
        }

        private sealed class RecordingReturnHistoryDal : IPrincipalReturnHistoryDal
        {
            public int WriteCount { get; private set; }

            public PrincipalReturnHistoryResult LastResult { get; private set; }

            public PrincipalReturnHistoryResult GetHistory()
            {
                return LastResult;
            }

            public void ReplaceHistory(PrincipalReturnHistoryResult result, string refreshLogId)
            {
                LastResult = result;
                WriteCount++;
            }
        }

        private sealed class StubReturnHistoryEvidenceDal : IPrincipalReturnHistoryEvidenceDal
        {
            private readonly IReadOnlyList<PrincipalReturnHistoryMonthEvidence> _months;

            public StubReturnHistoryEvidenceDal(IEnumerable<PrincipalReturnHistoryMonthEvidence> months)
            {
                _months = months.ToList();
            }

            public IReadOnlyList<PrincipalReturnHistoryMonthEvidence> ListMonthlyReturnHistory()
            {
                return _months;
            }
        }

        private sealed class StubCurrentReturnSnapshotDal : IPrincipalReturnSnapshotDal
        {
            private readonly PrincipalReturnAggregateResult _current;

            public StubCurrentReturnSnapshotDal(PrincipalReturnAggregateResult current)
            {
                _current = current;
            }

            public PrincipalReturnAggregateResult GetCurrent()
            {
                return _current;
            }

            public void ReplaceCurrent(PrincipalReturnAggregateResult result, string refreshLogId)
            {
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
