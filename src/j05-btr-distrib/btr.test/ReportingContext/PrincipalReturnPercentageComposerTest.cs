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
    public class PrincipalReturnPercentageComposerTest
    {
        private static readonly DateTime GeneratedAt = new DateTime(2026, 9, 9, 8, 0, 0);

        private readonly PrincipalReturnPercentageComposer _composer = new PrincipalReturnPercentageComposer();

        [Fact]
        public void Compose_DividesStoredTotalReturnByStoredSalesOut_WhenSalesOutIsGreaterThanZero()
        {
            var result = _composer.Compose(
                Returns(2026, 9, ReturnRow("SUPA", "Alpha", 110m), ReturnRow("SUPB", "Beta", 25m)),
                SalesOut(2026, 9, SalesRow("SUPA", "Alpha", 1000m), SalesRow("SUPB", "Beta", 50m)),
                GeneratedAt);

            result.ReturnPercentageKpiId.Should().Be("PRN-RET-004");
            result.SalesOutKpiId.Should().Be("PRN-SALES-001");
            result.TotalReturnKpiId.Should().Be("PRN-RET-003");
            result.PeriodYear.Should().Be(2026);
            result.PeriodMonth.Should().Be(9);

            var alpha = result.Principals.Single(row => row.SupplierId == "SUPA");
            alpha.ReturnPercentageKpiId.Should().Be("PRN-RET-004");
            alpha.TotalReturnAmount.Should().Be(110m);
            alpha.SalesOutAmount.Should().Be(1000m);
            alpha.ReturnPercentage.Should().Be(0.110000m);

            var beta = result.Principals.Single(row => row.SupplierId == "SUPB");
            beta.ReturnPercentage.Should().Be(0.500000m);
        }

        [Fact]
        public void Compose_ReturnsNull_WhenStoredSalesOutIsNotGreaterThanZero()
        {
            var result = _composer.Compose(
                Returns(
                    2026,
                    9,
                    ReturnRow("ZERO", "Zero", 40m),
                    ReturnRow("NEG", "Negative", 10m),
                    ReturnRow("MISS", "Missing", 15m)),
                SalesOut(2026, 9, SalesRow("ZERO", "Zero", 0m), SalesRow("NEG", "Negative", -5m)),
                GeneratedAt);

            result.Principals.Single(row => row.SupplierId == "ZERO").ReturnPercentage.Should().BeNull();
            result.Principals.Single(row => row.SupplierId == "ZERO").SalesOutAmount.Should().Be(0m);
            result.Principals.Single(row => row.SupplierId == "NEG").ReturnPercentage.Should().BeNull();
            result.Principals.Single(row => row.SupplierId == "NEG").SalesOutAmount.Should().Be(-5m);
            result.Principals.Single(row => row.SupplierId == "MISS").ReturnPercentage.Should().BeNull();
            result.Principals.Single(row => row.SupplierId == "MISS").SalesOutAmount.Should().BeNull();
        }

        [Fact]
        public void Compose_DoesNotUseSalesOutFromADifferentPeriod()
        {
            var result = _composer.Compose(
                Returns(2026, 9, ReturnRow("SUPA", "Alpha", 110m)),
                SalesOut(2026, 8, SalesRow("SUPA", "Alpha", 1000m)),
                GeneratedAt);

            var alpha = result.Principals.Should().ContainSingle().Subject;
            alpha.ReturnPercentage.Should().BeNull();
            alpha.SalesOutAmount.Should().BeNull();
            alpha.TotalReturnAmount.Should().Be(110m);
        }

        [Fact]
        public void PersistReturnPercentage_ReadsStoredSources_AndDoesNotUpdateThoseRows()
        {
            var salesOut = new RecordingSalesOutSnapshotDal(SalesOut(2026, 9, SalesRow("SUPA", "Alpha", 999.25m)));
            var returns = new RecordingReturnSnapshotDal(Returns(2026, 9, ReturnRow("SUPA", "Alpha", 110m)));
            var percentageDal = new RecordingPercentageSnapshotDal();
            var storedSalesOut = salesOut.GetAmount("SUPA");
            var storedReturn = returns.GetAmount("SUPA");

            var worker = new RefreshPrincipalReturnPercentageSnapshotWorker(
                returns,
                salesOut,
                new PrincipalReturnPercentageComposer(),
                percentageDal,
                new StubRefreshLogDal(),
                new StubTglJamDal(GeneratedAt));

            worker.Execute(new RefreshPrincipalReturnPercentageSnapshotRequest { TriggeredBy = "Manual" });

            salesOut.GetAmount("SUPA").Should().Be(storedSalesOut);
            returns.GetAmount("SUPA").Should().Be(storedReturn);
            salesOut.WriteCount.Should().Be(0);
            returns.WriteCount.Should().Be(0);
            percentageDal.WriteCount.Should().Be(1);
            percentageDal.LastResult.ReturnPercentageKpiId.Should().Be("PRN-RET-004");
            percentageDal.LastResult.Principals.Should().ContainSingle()
                .Which.ReturnPercentage.Should().Be(
                    PrincipalReturnPercentageComposer.Calculate(110m, 999.25m));

            var writerSql = string.Join(
                " ",
                PrincipalReturnPercentageSnapshotDal.WrittenTables,
                PrincipalReturnPercentageSnapshotDal.DeletePrincipalSql,
                PrincipalReturnPercentageSnapshotDal.MergeKpiSql,
                PrincipalReturnPercentageSnapshotDal.InsertPrincipalSql);
            writerSql.Should().Contain("BTRPD_PrincipalReturnPercentage");
            writerSql.Should().Contain("ReturnPercentage");
            writerSql.Should().NotContain("BTRPD_PrincipalSalesOut");
            writerSql.Should().NotContain("INSERT INTO BTRPD_PrincipalReturn ");
            writerSql.Should().NotContain("UPDATE BTRPD_PrincipalReturn ");
            writerSql.Should().NotContain("DELETE FROM BTRPD_PrincipalReturn ");
            writerSql.Should().NotContain("GoodReturnAmount");
            writerSql.Should().NotContain("Net Sales");
        }

        private static PrincipalReturnAggregateResult Returns(
            int year,
            int month,
            params PrincipalReturnRow[] rows)
        {
            return new PrincipalReturnAggregateResult
            {
                GoodReturnKpiId = PrincipalKpiCatalog.GoodReturnAmountId,
                BrokenReturnKpiId = PrincipalKpiCatalog.BrokenReturnAmountId,
                TotalReturnKpiId = PrincipalKpiCatalog.TotalReturnAmountId,
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

        private static PrincipalReturnRow ReturnRow(string supplierId, string supplierName, decimal totalReturnAmount)
        {
            return new PrincipalReturnRow
            {
                GoodReturnKpiId = PrincipalKpiCatalog.GoodReturnAmountId,
                BrokenReturnKpiId = PrincipalKpiCatalog.BrokenReturnAmountId,
                TotalReturnKpiId = PrincipalKpiCatalog.TotalReturnAmountId,
                SupplierId = supplierId,
                SupplierName = supplierName,
                TotalReturnAmount = totalReturnAmount
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

        private sealed class RecordingReturnSnapshotDal : IPrincipalReturnSnapshotDal
        {
            private readonly PrincipalReturnAggregateResult _current;

            public RecordingReturnSnapshotDal(PrincipalReturnAggregateResult current)
            {
                _current = current;
            }

            public int WriteCount { get; private set; }

            public PrincipalReturnAggregateResult GetCurrent()
            {
                return _current;
            }

            public void ReplaceCurrent(PrincipalReturnAggregateResult result, string refreshLogId)
            {
                WriteCount++;
            }

            public decimal GetAmount(string supplierId)
            {
                return _current.Principals.Single(row => row.SupplierId == supplierId).TotalReturnAmount;
            }
        }

        private sealed class RecordingPercentageSnapshotDal : IPrincipalReturnPercentageSnapshotDal
        {
            public int WriteCount { get; private set; }

            public PrincipalReturnPercentageResult LastResult { get; private set; }

            public PrincipalReturnPercentageResult GetCurrent()
            {
                return LastResult;
            }

            public void ReplaceCurrent(PrincipalReturnPercentageResult result, string refreshLogId)
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
