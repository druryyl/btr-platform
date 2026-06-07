using System;
using System.Collections.Generic;
using btr.application.PurchaseContext.InvoiceInfo;
using btr.application.ReportingContext.DashboardSnapshotAgg.Services;
using btr.application.SupportContext.TglJamAgg;
using btr.infrastructure.ReportingContext.DashboardPurchasingAgg;
using btr.nuna.Domain;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class DashboardPurchasingSnapshotVerificationTest
    {
        private static readonly DateTime FixedToday = new DateTime(2026, 6, 6, 14, 30, 0);

        [Fact]
        public void Aggregator_MatchesLiveDal_ForEquivalentInvoiceRows()
        {
            var rows = new[]
            {
                Row("INV-001", new DateTime(2026, 6, 3), "Principal A", 10_000_000m, "SUDAH"),
                Row("INV-002", new DateTime(2026, 6, 5), "Principal B", 5_000_000m, "BELUM"),
            };

            var stubDal = new StubInvoiceViewDal(rows);
            var stubTglJam = new StubTglJamDal(FixedToday);
            var aggregator = new DashboardPurchasingInvoiceAggregator();
            var liveDal = new DashboardPurchasingLiveDal(stubDal, stubTglJam, aggregator);

            var live = liveDal.GetSummary();
            var aggregate = aggregator.Aggregate(
                rows,
                new Periode(new DateTime(2026, 6, 1), new DateTime(2026, 6, 30)),
                FixedToday);

            live.GrandTotalPurchase.Should().Be(aggregate.GrandTotalPurchase);
            live.TotalInvoice.Should().Be(aggregate.TotalInvoice);
            live.PendingPostingInvoiceCount.Should().Be(aggregate.PendingPostingInvoiceCount);
        }

        private static InvoiceView Row(
            string invoiceCode,
            DateTime tgl,
            string supplierName,
            decimal grandTotal,
            string postingStok)
        {
            return new InvoiceView
            {
                InvoiceCode = invoiceCode,
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
