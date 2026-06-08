using System;

using System.Collections.Generic;

using btr.application.PurchaseContext.InvoiceInfo;

using btr.application.SupportContext.TglJamAgg;

using btr.infrastructure.ReportingContext.PurchasingReportAgg;

using btr.nuna.Domain;

using FluentAssertions;

using Xunit;



namespace btr.test.ReportingContext

{

    public class PurchasingReportDalTest

    {

        private static readonly DateTime FixedToday = new DateTime(2026, 6, 6, 14, 30, 0);
        private static readonly Periode June2026 = new Periode(
            new DateTime(2026, 6, 1),
            new DateTime(2026, 6, 30));



        [Fact]

        public void GetReport_UsesCurrentMonthPeriod_AndComputesSummary()

        {

            var dal = new PurchasingReportDal(

                new StubInvoiceViewDal(new[]

                {

                    Row("INV-001", new DateTime(2026, 6, 3), 10_000_000m, "SUDAH"),

                    Row("INV-002", new DateTime(2026, 6, 5), 5_000_000m, "BELUM"),

                }),

                new StubTglJamDal(FixedToday));



            var result = dal.GetReport(June2026);



            result.PeriodFrom.Should().Be(new DateTime(2026, 6, 1));

            result.PeriodTo.Should().Be(new DateTime(2026, 6, 30, 23, 59, 59));

            result.GeneratedAt.Should().Be(FixedToday);

            result.Rows.Should().HaveCount(2);

            result.Summary.TotalInvoice.Should().Be(2);

            result.Summary.GrandTotalPurchase.Should().Be(15_000_000m);

        }



        [Fact]

        public void GetReport_PassesThroughPostingStok()

        {

            var dal = new PurchasingReportDal(

                new StubInvoiceViewDal(new[]

                {

                    Row("INV-010", new DateTime(2026, 6, 1), 1_000_000m, "SUDAH"),

                    Row("INV-011", new DateTime(2026, 6, 2), 2_000_000m, "BELUM"),

                }),

                new StubTglJamDal(FixedToday));



            var result = dal.GetReport(June2026);



            result.Rows[0].PostingStok.Should().Be("SUDAH");

            result.Rows[1].PostingStok.Should().Be("BELUM");

        }



        [Fact]

        public void GetReport_OrdersRows_ByDateThenInvoiceCode()

        {

            var dal = new PurchasingReportDal(

                new StubInvoiceViewDal(new[]

                {

                    Row("INV-B", new DateTime(2026, 6, 10), 1_000_000m, "SUDAH"),

                    Row("INV-A", new DateTime(2026, 6, 10), 1_000_000m, "BELUM"),

                    Row("INV-C", new DateTime(2026, 6, 5), 1_000_000m, "SUDAH"),

                }),

                new StubTglJamDal(FixedToday));



            var result = dal.GetReport(June2026);



            result.Rows[0].InvoiceCode.Should().Be("INV-C");

            result.Rows[1].InvoiceCode.Should().Be("INV-A");

            result.Rows[2].InvoiceCode.Should().Be("INV-B");

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


