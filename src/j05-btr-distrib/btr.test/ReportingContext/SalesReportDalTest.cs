using System;
using System.Collections.Generic;
using btr.application.SalesContext.FakturInfo;
using btr.application.SupportContext.TglJamAgg;
using btr.infrastructure.ReportingContext.SalesReportAgg;
using btr.nuna.Domain;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class SalesReportDalTest
    {
        private static readonly DateTime FixedToday = new DateTime(2026, 6, 6, 14, 30, 0);
        private static readonly Periode June2026 = new Periode(
            new DateTime(2026, 6, 1),
            new DateTime(2026, 6, 30));

        [Fact]
        public void GetReport_UsesProvidedPeriod_AndMapsRows()
        {
            var dal = new SalesReportDal(
                new StubFakturViewDal(new[]
                {
                    Row("FK-002", new DateTime(2026, 6, 5), "Beta", 2_000_000m, 0),
                    Row("FK-001", new DateTime(2026, 6, 3), "Alpha", 1_000_000m, 2),
                }),
                new StubTglJamDal(FixedToday));

            var result = dal.GetReport(June2026);

            result.PeriodFrom.Should().Be(new DateTime(2026, 6, 1));
            result.PeriodTo.Should().Be(new DateTime(2026, 6, 30, 23, 59, 59));
            result.GeneratedAt.Should().Be(FixedToday);
            result.Rows.Should().HaveCount(2);
            result.Rows[0].FakturCode.Should().Be("FK-001");
            result.Rows[1].FakturCode.Should().Be("FK-002");
            result.Rows[1].Status.Should().Be("Kembali");
        }

        private static FakturView Row(
            string fakturCode,
            DateTime tgl,
            string customer,
            decimal grandTotal,
            int statusFaktur)
        {
            return new FakturView
            {
                FakturCode = fakturCode,
                Tgl = tgl,
                Customer = customer,
                SalesPersonName = "Sales A",
                GrandTotal = grandTotal,
                StatusFaktur = statusFaktur,
            };
        }

        private sealed class StubFakturViewDal : IFakturViewDal
        {
            private readonly IEnumerable<FakturView> _rows;

            public StubFakturViewDal(IEnumerable<FakturView> rows)
            {
                _rows = rows;
            }

            public IEnumerable<FakturView> ListData(Periode periode) => _rows;

            public IEnumerable<FakturView> ListTerhapus(Periode periode) => _rows;
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
