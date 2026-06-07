using System;
using System.Collections.Generic;
using btr.application.PurchaseContext.InvoiceInfo;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.SupportContext.TglJamAgg;
using btr.infrastructure.ReportingContext.PurchasingReportAgg;
using btr.nuna.Domain;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardPurchasingReportTraceabilityTest
    {
        private static readonly DateTime FixedToday = new DateTime(2026, 6, 6, 14, 30, 0);
        private static readonly Periode June2026 = new Periode(
            new DateTime(2026, 6, 1),
            new DateTime(2026, 6, 30));

        [Fact]
        public void Aggregator_GrandTotalPurchase_MatchesPurchasingReportFooter()
        {
            var rows = new[]
            {
                Row("INV-001", new DateTime(2026, 6, 3), 10_000_000m, "SUDAH"),
                Row("INV-002", new DateTime(2026, 6, 5), 5_000_000m, "BELUM"),
                Row("INV-003", new DateTime(2026, 6, 10), 2_500_000m, "SUDAH"),
            };

            var stubDal = new StubInvoiceViewDal(rows);
            var stubTglJam = new StubTglJamDal(FixedToday);
            var aggregator = new DashboardPurchasingInvoiceAggregator();
            var reportDal = new PurchasingReportDal(stubDal, stubTglJam);

            var aggregate = aggregator.Aggregate(rows, June2026, FixedToday);
            var report = reportDal.GetReport();

            aggregate.GrandTotalPurchase.Should().Be(report.Summary.GrandTotalPurchase);
            aggregate.TotalInvoice.Should().Be(report.Summary.TotalInvoice);
        }

        private static InvoiceView Row(
            string invoiceCode,
            DateTime tgl,
            decimal grandTotal,
            string postingStok)
        {
            return new InvoiceView
            {
                InvoiceCode = invoiceCode,
                Tgl = tgl,
                SupplierName = "Supplier",
                WarehouseName = "Gudang",
                Total = grandTotal,
                Disc = 0,
                Tax = 0,
                GrandTotal = grandTotal,
                PostingStok = postingStok,
            };
        }

        private sealed class StubInvoiceViewDal : IInvoiceViewDal
        {
            private readonly IEnumerable<InvoiceView> _rows;

            public StubInvoiceViewDal(IEnumerable<InvoiceView> rows)
            {
                _rows = rows;
            }

            public IEnumerable<InvoiceView> ListData(Periode periode) => _rows;
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
