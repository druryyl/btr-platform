using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.PurchaseContext.InvoiceInfo;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.SalesContext.SalesOmzetAgg.Services;
using btr.nuna.Domain;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardPurchasingInvoiceAggregatorTest
    {
        private static readonly DateTime FixedGeneratedAt = new DateTime(2026, 6, 6, 14, 30, 0);
        private static readonly Periode June2026 = new Periode(
            new DateTime(2026, 6, 1),
            new DateTime(2026, 6, 30));

        private readonly DashboardPurchasingInvoiceAggregator _aggregator =
            new DashboardPurchasingInvoiceAggregator();

        [Fact]
        public void GrandTotalPurchase_MatchesSumOfGrandTotal()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Invoice("INV-001", new DateTime(2026, 6, 3), "Supplier A", 10_000_000m, "SUDAH"),
                Invoice("INV-002", new DateTime(2026, 6, 5), "Supplier B", 5_000_000m, "BELUM"),
            }, June2026, FixedGeneratedAt);

            result.GrandTotalPurchase.Should().Be(15_000_000m);
            result.PeriodYear.Should().Be(2026);
            result.PeriodMonth.Should().Be(6);
        }

        [Fact]
        public void TotalInvoice_MatchesRowCount()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Invoice("INV-001", new DateTime(2026, 6, 3), "Supplier A", 1_000m, "SUDAH"),
                Invoice("INV-002", new DateTime(2026, 6, 5), "Supplier B", 2_000m, "BELUM"),
                Invoice("INV-003", new DateTime(2026, 6, 7), "Supplier C", 3_000m, "SUDAH"),
            }, June2026, FixedGeneratedAt);

            result.TotalInvoice.Should().Be(3);
        }

        [Fact]
        public void PendingPostingInvoiceCount_CountsBelumOnly()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Invoice("INV-001", new DateTime(2026, 6, 1), "A", 1_000m, "SUDAH"),
                Invoice("INV-002", new DateTime(2026, 6, 2), "B", 2_000m, "BELUM"),
                Invoice("INV-003", new DateTime(2026, 6, 3), "C", 3_000m, "BELUM"),
            }, June2026, FixedGeneratedAt);

            result.PendingPostingInvoiceCount.Should().Be(2);
        }

        [Fact]
        public void WeekTrend_SumsMatchGrandTotal()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Invoice("INV-001", new DateTime(2026, 6, 1), "A", 100m, "SUDAH"),
                Invoice("INV-002", new DateTime(2026, 6, 2), "B", 200m, "BELUM"),
                Invoice("INV-003", new DateTime(2026, 6, 8), "C", 500m, "SUDAH"),
            }, June2026, FixedGeneratedAt);

            result.WeekTrend.Should().NotBeEmpty();
            result.WeekTrend.Sum(w => w.PurchaseAmount).Should().Be(800m);
            result.WeekTrend.Sum(w => w.PurchaseAmount).Should().BeLessOrEqualTo(result.GrandTotalPurchase);
        }

        [Fact]
        public void PostingStatus_SudahBelum_SumToGrandTotal()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Invoice("INV-001", new DateTime(2026, 6, 1), "A", 1_000m, "SUDAH"),
                Invoice("INV-002", new DateTime(2026, 6, 2), "B", 2_000m, "BELUM"),
                Invoice("INV-003", new DateTime(2026, 6, 3), "C", 3_000m, "SUDAH"),
            }, June2026, FixedGeneratedAt);

            result.PostingStatus.Should().HaveCount(2);
            result.PostingStatus.Sum(s => s.PurchaseAmount).Should().Be(result.GrandTotalPurchase);

            var belum = result.PostingStatus.First(s => s.StatusKey == "BELUM");
            belum.PurchaseAmount.Should().Be(2_000m);
            belum.SortOrder.Should().Be(1);

            var sudah = result.PostingStatus.First(s => s.StatusKey == "SUDAH");
            sudah.PurchaseAmount.Should().Be(4_000m);
            sudah.SortOrder.Should().Be(2);
        }

        [Fact]
        public void TopPrincipal_BlankSupplierMapsToUnknown()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Invoice("INV-001", new DateTime(2026, 6, 1), "", 5_000m, "SUDAH"),
                Invoice("INV-002", new DateTime(2026, 6, 2), null, 3_000m, "BELUM"),
                Invoice("INV-003", new DateTime(2026, 6, 3), "  ", 2_000m, "SUDAH"),
            }, June2026, FixedGeneratedAt);

            result.TopPrincipal.Should().ContainSingle();
            result.TopPrincipal[0].PrincipalName.Should().Be("Unknown");
            result.TopPrincipal[0].PurchaseAmount.Should().Be(10_000m);
        }

        [Fact]
        public void TopPrincipal_Take10_OrderedByPurchaseAmount()
        {
            var rows = Enumerable.Range(1, 12)
                .Select(i => Invoice(
                    $"INV-{i:D3}",
                    new DateTime(2026, 6, 1),
                    $"Principal {i}",
                    i * 100_000m,
                    "SUDAH"))
                .ToArray();

            var result = _aggregator.Aggregate(rows, June2026, FixedGeneratedAt);

            result.TopPrincipal.Should().HaveCount(DashboardPurchasingInvoiceAggregator.TopPrincipalCount);
            result.TopPrincipal[0].PrincipalName.Should().Be("Principal 12");
            result.TopPrincipal[0].PurchaseAmount.Should().Be(1_200_000m);
            result.TopPrincipal[9].PrincipalName.Should().Be("Principal 3");
            result.TopPrincipal.Select(r => r.Rank).Should().BeEquivalentTo(Enumerable.Range(1, 10));
        }

        [Fact]
        public void WeekTrendLabels_MatchWeekGrouper()
        {
            var result = _aggregator.Aggregate(new[]
            {
                Invoice("INV-001", new DateTime(2026, 6, 1), "A", 100m, "SUDAH"),
            }, June2026, FixedGeneratedAt);

            var expectedBuckets = SalesOmzetChartWeekGrouper.BuildBuckets(June2026);
            result.WeekTrend.Should().HaveCount(expectedBuckets.Count);
            result.WeekTrend[0].WeekLabel.Should().Be(expectedBuckets[0].WeekLabel);
        }

        private static InvoiceView Invoice(
            string code,
            DateTime tgl,
            string supplierName,
            decimal grandTotal,
            string postingStok)
        {
            return new InvoiceView
            {
                InvoiceCode = code,
                Tgl = tgl,
                SupplierName = supplierName,
                WarehouseName = "Gudang",
                Total = grandTotal,
                Disc = 0,
                Tax = 0,
                GrandTotal = grandTotal,
                PostingStok = postingStok,
            };
        }
    }
}
