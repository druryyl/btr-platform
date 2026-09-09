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
    public class PrincipalReturnAggregatorTest
    {
        private static readonly DateTime GeneratedAt = new DateTime(2026, 9, 9, 8, 0, 0);

        private readonly PrincipalReturnAggregator _aggregator = new PrincipalReturnAggregator();

        [Fact]
        public void Aggregate_SplitsGoodAndBroken_AndStoresTotalAsTheirSum()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Line("R1", "L1", "BAGUS", "SUPA", "Alpha", 100m, 10m, 9m, 99m),
                Line("R1", "L2", "BAGUS", "SUPB", "Beta", 40m, 0m, 4m, 44m),
                Line("R2", "L3", "RUSAK", "SUPA", "Alpha", 20m, 5m, 1.5m, 16.5m),
                Line("R3", "L4", "OTHER", "SUPA", "Alpha", 50m, 0m, 5m, 55m)
            }, 2026, 9, GeneratedAt);

            result.GoodReturnKpiId.Should().Be(PrincipalKpiCatalog.GoodReturnAmountId);
            result.BrokenReturnKpiId.Should().Be(PrincipalKpiCatalog.BrokenReturnAmountId);
            result.TotalReturnKpiId.Should().Be(PrincipalKpiCatalog.TotalReturnAmountId);
            result.PeriodYear.Should().Be(2026);
            result.PeriodMonth.Should().Be(9);

            var alpha = result.Principals.Should().ContainSingle(row => row.SupplierId == "SUPA").Subject;
            alpha.GoodReturnAmount.Should().Be(90m);
            alpha.BrokenReturnAmount.Should().Be(15m);
            alpha.TotalReturnAmount.Should().Be(alpha.GoodReturnAmount + alpha.BrokenReturnAmount);
            alpha.LineCount.Should().Be(2);
            alpha.GoodReturnKpiId.Should().Be("PRN-RET-001");
            alpha.BrokenReturnKpiId.Should().Be("PRN-RET-002");
            alpha.TotalReturnKpiId.Should().Be("PRN-RET-003");

            var beta = result.Principals.Should().ContainSingle(row => row.SupplierId == "SUPB").Subject;
            beta.GoodReturnAmount.Should().Be(40m);
            beta.BrokenReturnAmount.Should().Be(0m);
            beta.TotalReturnAmount.Should().Be(40m);
        }

        [Fact]
        public void Aggregate_UsesLineAmountAfterDiscountAndBeforeTax()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Line("R1", "L1", "BAGUS", "SUPA", "Alpha", 100m, 10m, 9m, 99m)
            }, 2026, 9, GeneratedAt);

            result.Principals.Should().ContainSingle()
                .Which.GoodReturnAmount.Should().Be(90m);
            result.Principals.Should().ContainSingle()
                .Which.GoodReturnAmount.Should().NotBe(99m);
            result.Principals.Should().ContainSingle()
                .Which.GoodReturnAmount.Should().NotBe(109m);
        }

        [Fact]
        public void Aggregate_ExcludesBlankAndUnknownPrincipal_AndDoesNotInventAPrincipal()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Line("R1", "L1", "BAGUS", "SUPA", "Alpha", 80m, 0m, 8m, 88m),
                Line("R2", "L2", "RUSAK", "", "", 15m, 0m, 1m, 16m, itemSupplierId: ""),
                Line("R3", "L3", "BAGUS", "  ", "", 12m, 0m, 1m, 13m, itemSupplierId: "MISS")
            }, 2026, 9, GeneratedAt);

            result.Principals.Should().ContainSingle()
                .Which.SupplierId.Should().Be("SUPA");
            result.Principals.Should().NotContain(row => string.IsNullOrWhiteSpace(row.SupplierId));
        }

        [Fact]
        public void EvidenceQuery_ExcludesVoidReturns_AndDoesNotUseSalesmanOrSalesOut()
        {
            var evidenceSql = PrincipalReturnEvidenceDal.ListReturnItemEvidenceSql;
            evidenceSql.Should().Contain("BTR_ReturJualItem");
            evidenceSql.Should().Contain("BTR_ReturJual");
            evidenceSql.Should().Contain("ri.SubTotal");
            evidenceSql.Should().Contain("ri.DiscRp");
            evidenceSql.Should().Contain("VoidDate = '3000-01-01'");
            evidenceSql.Should().Contain("BAGUS");
            evidenceSql.Should().Contain("RUSAK");
            evidenceSql.Should().Contain("b.SupplierId");
            evidenceSql.Should().NotContain("SalesPersonId");
            evidenceSql.Should().NotContain("BTR_Faktur");
            evidenceSql.Should().NotContain("BTRPD_PrincipalSalesOut");
            evidenceSql.Should().NotContain("PRN-SALES-001");
            evidenceSql.Should().NotContain("GrandTotal");
            evidenceSql.Should().NotContain("PRN-RET-004");
        }

        [Fact]
        public void PersistReturns_DoesNotChangePreviouslyStoredSalesOutValue()
        {
            var salesOut = new RecordingSalesOutStore();
            salesOut.Store("SUPA", 999.25m);
            var storedBefore = salesOut.GetAmount("SUPA");

            var returnDal = new RecordingReturnSnapshotDal();
            var worker = new RefreshPrincipalReturnSnapshotWorker(
                new StubReturnEvidenceDal(new[]
                {
                    Line("R1", "L1", "BAGUS", "SUPA", "Alpha", 100m, 10m, 9m, 99m),
                    Line("R2", "L2", "RUSAK", "SUPA", "Alpha", 20m, 0m, 2m, 22m)
                }),
                new PrincipalReturnAggregator(),
                returnDal,
                new StubRefreshLogDal(),
                new StubTglJamDal(GeneratedAt),
                new StubBusinessDateProvider(new DateTime(2026, 9, 9)));

            worker.Execute(new RefreshPrincipalReturnSnapshotRequest { TriggeredBy = "Manual" });

            salesOut.GetAmount("SUPA").Should().Be(storedBefore);
            salesOut.WriteCount.Should().Be(1);
            returnDal.WriteCount.Should().Be(1);
            returnDal.LastResult.Principals.Should().ContainSingle()
                .Which.TotalReturnAmount.Should().Be(110m);
            returnDal.LastResult.GoodReturnKpiId.Should().Be("PRN-RET-001");
            returnDal.LastResult.BrokenReturnKpiId.Should().Be("PRN-RET-002");
            returnDal.LastResult.TotalReturnKpiId.Should().Be("PRN-RET-003");

            var writerSql = string.Join(
                " ",
                PrincipalReturnSnapshotDal.WrittenTables,
                PrincipalReturnSnapshotDal.DeletePrincipalSql,
                PrincipalReturnSnapshotDal.MergeKpiSql,
                PrincipalReturnSnapshotDal.InsertPrincipalSql);
            writerSql.Should().Contain("BTRPD_PrincipalReturn");
            writerSql.Should().Contain("GoodReturnAmount");
            writerSql.Should().Contain("BrokenReturnAmount");
            writerSql.Should().Contain("TotalReturnAmount");
            writerSql.Should().NotContain("BTRPD_PrincipalSalesOut");
            writerSql.Should().NotContain("PRN-SALES-001");
            writerSql.Should().NotContain("SalesOutAmount");
            writerSql.Should().NotContain("PRN-RET-004");
            writerSql.Should().NotContain("ReturnPercentage");
        }

        private static ReturnItemEvidence Line(
            string returJualId,
            string returJualItemId,
            string jenisRetur,
            string supplierId,
            string supplierName,
            decimal subTotal,
            decimal discRp,
            decimal ppnRp,
            decimal lineTotal,
            string itemSupplierId = null)
        {
            return new ReturnItemEvidence
            {
                ReturJualId = returJualId,
                ReturJualItemId = returJualItemId,
                JenisRetur = jenisRetur,
                ItemSupplierId = itemSupplierId ?? supplierId,
                SupplierId = supplierId,
                SupplierName = supplierName,
                SubTotal = subTotal,
                DiscRp = discRp,
                PpnRp = ppnRp,
                LineTotal = lineTotal
            };
        }

        private sealed class RecordingSalesOutStore
        {
            private readonly Dictionary<string, decimal> _amounts =
                new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            public int WriteCount { get; private set; }

            public void Store(string supplierId, decimal salesOutAmount)
            {
                _amounts[supplierId] = salesOutAmount;
                WriteCount++;
            }

            public decimal GetAmount(string supplierId)
            {
                return _amounts[supplierId];
            }
        }

        private sealed class RecordingReturnSnapshotDal : IPrincipalReturnSnapshotDal
        {
            public int WriteCount { get; private set; }

            public PrincipalReturnAggregateResult LastResult { get; private set; }

            public PrincipalReturnAggregateResult GetCurrent()
            {
                return LastResult;
            }

            public void ReplaceCurrent(PrincipalReturnAggregateResult result, string refreshLogId)
            {
                LastResult = result;
                WriteCount++;
            }
        }

        private sealed class StubReturnEvidenceDal : IPrincipalReturnEvidenceDal
        {
            private readonly IReadOnlyList<ReturnItemEvidence> _lines;

            public StubReturnEvidenceDal(IEnumerable<ReturnItemEvidence> lines)
            {
                _lines = lines.ToList();
            }

            public IReadOnlyList<ReturnItemEvidence> ListReturnItemEvidence(int year, int month)
            {
                return _lines;
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

        private sealed class StubBusinessDateProvider : IBusinessDateProvider
        {
            public StubBusinessDateProvider(DateTime today)
            {
                Today = today.Date;
            }

            public DateTime Today { get; }

            public bool IsPresentationActive { get { return false; } }
        }
    }
}
