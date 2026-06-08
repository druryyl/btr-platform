using System;
using System.Collections.Generic;
using btr.application.FinanceContext.PiutangAgg.Contracts;
using btr.application.ReportingContext.PiutangReportAgg;
using btr.application.SupportContext.TglJamAgg;
using btr.infrastructure.ReportingContext.PiutangReportAgg;
using btr.nuna.Domain;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class PiutangReportDalTest
    {
        private static readonly DateTime FixedToday = new DateTime(2026, 6, 6, 14, 30, 0);
        private static readonly Periode June2026 = new Periode(
            new DateTime(2026, 6, 1),
            new DateTime(2026, 6, 30));

        [Fact]
        public void GetReport_FiltersOutstandingRows_AndComputesSummary()
        {
            var dal = new PiutangReportDal(
                new StubPiutangSalesWilayahDal(new[]
                {
                    Row("C001", "Alpha Corp", "FK-001", 3_000_000m, 1m),
                    Row("C001", "Alpha Corp", "FK-002", 2_000_000m, 500_000m),
                    Row(null, "Beta Ltd", "FK-003", 1_000_000m, 250_000m),
                    Row("C002", "Gamma Inc", "FK-004", 800_000m, 0m),
                }),
                new StubTglJamDal(FixedToday));

            var result = dal.GetReport(June2026, PiutangReportDateField.DueDate);

            result.PeriodFrom.Should().Be(new DateTime(2026, 6, 1));
            result.PeriodTo.Should().Be(new DateTime(2026, 6, 30, 23, 59, 59));
            result.DateField.Should().Be("DueDate");
            result.GeneratedAt.Should().Be(FixedToday);
            result.Rows.Should().HaveCount(2);
            result.Rows.Should().OnlyContain(r => r.KurangBayar > 1);
            result.Summary.TotalPiutang.Should().Be(750_000m);
            result.Summary.TotalCustomer.Should().Be(2);
        }

        [Fact]
        public void GetReport_UsesCustomerCode_WhenPresent_ForDistinctCount()
        {
            var dal = new PiutangReportDal(
                new StubPiutangSalesWilayahDal(new[]
                {
                    Row("C001", "Different Name A", "FK-010", 1_000_000m, 100_000m),
                    Row("C001", "Different Name B", "FK-011", 1_000_000m, 200_000m),
                    Row(null, "Standalone", "FK-012", 1_000_000m, 300_000m),
                }),
                new StubTglJamDal(FixedToday));

            var result = dal.GetReport(June2026, PiutangReportDateField.DueDate);

            result.Summary.TotalCustomer.Should().Be(2);
        }

        [Fact]
        public void GetReport_OrdersRows_ByCustomerThenDateThenFaktur()
        {
            var dal = new PiutangReportDal(
                new StubPiutangSalesWilayahDal(new[]
                {
                    Row("C002", "Zeta", "FK-B", new DateTime(2026, 5, 10), 1_000_000m, 100_000m),
                    Row("C001", "Alpha", "FK-A", new DateTime(2026, 5, 15), 1_000_000m, 100_000m),
                }),
                new StubTglJamDal(FixedToday));

            var result = dal.GetReport(June2026, PiutangReportDateField.DueDate);

            result.Rows[0].CustomerName.Should().Be("Alpha");
            result.Rows[1].CustomerName.Should().Be("Zeta");
        }

        [Fact]
        public void GetReport_PassesDateField_ToUnderlyingDal()
        {
            var stub = new StubPiutangSalesWilayahDal(Array.Empty<PiutangSalesWilayahDto>());
            var dal = new PiutangReportDal(stub, new StubTglJamDal(FixedToday));

            dal.GetReport(June2026, PiutangReportDateField.PiutangDate);

            stub.LastDateField.Should().Be(PiutangReportDateField.PiutangDate);
        }

        private static PiutangSalesWilayahDto Row(
            string customerCode,
            string customerName,
            string fakturCode,
            decimal totalJual,
            decimal kurangBayar)
        {
            return Row(customerCode, customerName, fakturCode, new DateTime(2026, 5, 1), totalJual, kurangBayar);
        }

        private static PiutangSalesWilayahDto Row(
            string customerCode,
            string customerName,
            string fakturCode,
            DateTime fakturDate,
            decimal totalJual,
            decimal kurangBayar)
        {
            return new PiutangSalesWilayahDto
            {
                CustomerCode = customerCode,
                CustomerName = customerName,
                FakturCode = fakturCode,
                FakturDate = fakturDate,
                JatuhTempo = fakturDate.AddDays(30),
                TotalJual = totalJual,
                KurangBayar = kurangBayar,
            };
        }

        private sealed class StubPiutangSalesWilayahDal : IPiutangSalesWilayahDal
        {
            private readonly IEnumerable<PiutangSalesWilayahDto> _rows;

            public StubPiutangSalesWilayahDal(IEnumerable<PiutangSalesWilayahDto> rows)
            {
                _rows = rows;
            }

            public PiutangReportDateField? LastDateField { get; private set; }

            public IEnumerable<PiutangSalesWilayahDto> ListData(Periode periode) => _rows;

            public IEnumerable<PiutangSalesWilayahDto> ListData(Periode filter, PiutangReportDateField dateField)
            {
                LastDateField = dateField;
                return _rows;
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
