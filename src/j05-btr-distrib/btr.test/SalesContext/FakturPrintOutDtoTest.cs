using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using btr.distrib.SalesContext.FakturAgg;
using btr.domain.SalesContext.CustomerAgg;
using btr.domain.SalesContext.FakturAgg;
using btr.domain.SupportContext.UserAgg;
using FluentAssertions;
using Xunit;

namespace btr.test.SalesContext
{
    public class FakturPrintOutDtoTest
    {
        private DateTime CalculateDefaultTglKirim(DateTime fakturDate)
        {
            var tglKirim = fakturDate.AddDays(1);
            if (tglKirim.DayOfWeek == DayOfWeek.Sunday)
                tglKirim = tglKirim.AddDays(1);
            return tglKirim;
        }

        private (FakturModel faktur, CustomerModel customer, UserModel user) CreateDummyContext(DateTime fakturDate, DateTime tglRencanaKirim)
        {
            var faktur = new FakturModel
            {
                FakturId = "F001",
                FakturCode = "FP-202611-0001",
                FakturDate = fakturDate,
                TglRencanaKirim = tglRencanaKirim,
                CustomerId = "C001",
                CustomerName = "Toko Sumber Rejeki",
                Address = "Jl. Magelang KM 5",
                DueDate = fakturDate.AddDays(30),
                SalesPersonName = "Budi Santoso",
                TermOfPayment = TermOfPaymentEnum.Credit,
                DriverName = "Joko",
                Note = "Pengiriman pagi",
                Total = 1000000,
                Discount = 0,
                GrandTotal = 1110000,
                ListItem = new List<FakturItemModel>
                {
                    new FakturItemModel
                    {
                        NoUrut = 1,
                        BrgCode = "BRG01",
                        BrgName = "Barang 01",
                        QtyBesar = 1,
                        SatBesar = "DUS",
                        HrgSatBesar = 1000000,
                        DppProsen = 100M,
                        DppRp = 1000000,
                        PpnRp = 110000,
                        PpnProsen = 11M,
                        Total = 1110000,
                        ListDiscount = new List<FakturDiscountModel>()
                    }
                }
            };

            var customer = new CustomerModel
            {
                CustomerId = "C001",
                CustomerName = "Toko Sumber Rejeki",
                Address1 = "Jl. Magelang KM 5",
                Address2 = "Sinduadi",
                Kota = "Sleman"
            };

            var user = new UserModel
            {
                UserId = "U001",
                UserName = "Admin Sales"
            };

            return (faktur, customer, user);
        }

        [Fact]
        public void FakturPrintOutDto_ShouldFormatFakturDateAndRencanaKirimDateCorrectly()
        {
            var prevCulture = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("id-ID");

                var fakturDate = new DateTime(2026, 11, 14); // Sabtu, 14 November 2026
                var tglRencanaKirim = new DateTime(2026, 11, 16); // Senin, 16 November 2026
                var (faktur, customer, user) = CreateDummyContext(fakturDate, tglRencanaKirim);

                var dto = new FakturPrintOutDto(faktur, customer, user, isKlaim: false);

                dto.FakturDate.Should().Be("Tgl: 14 November 2026");
                dto.RencanaKirimDate.Should().Be("Tgl: 16 November 2026");
                dto.RencanaKirimDate.Should().NotBe(dto.FakturDate);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = prevCulture;
            }
        }

        [Theory]
        [InlineData(2026, 11, 11, 2026, 11, 12)] // Rabu -> Kamis (H+1)
        [InlineData(2026, 11, 12, 2026, 11, 13)] // Kamis -> Jumat (H+1)
        [InlineData(2026, 11, 13, 2026, 11, 14)] // Jumat -> Sabtu (H+1)
        [InlineData(2026, 11, 14, 2026, 11, 16)] // Sabtu -> Senin (H+2, skip Minggu)
        [InlineData(2026, 11, 15, 2026, 11, 16)] // Minggu -> Senin (H+1)
        public void CalculateDefaultTglKirim_ShouldHandleWeekdayAndSkipSunday(
            int fYear, int fMonth, int fDay,
            int expYear, int expMonth, int expDay)
        {
            var fakturDate = new DateTime(fYear, fMonth, fDay);
            var expectedDeliveryDate = new DateTime(expYear, expMonth, expDay);

            var actualDeliveryDate = CalculateDefaultTglKirim(fakturDate);

            actualDeliveryDate.Should().Be(expectedDeliveryDate);
            actualDeliveryDate.DayOfWeek.Should().NotBe(DayOfWeek.Sunday);
        }

        [Fact]
        public void FakturPrintOutDto_WithCalculatedDate_ShouldReflectDifferentDates()
        {
            var prevCulture = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("id-ID");

                var fakturDate = new DateTime(2026, 11, 14); // Sabtu
                var tglKirim = CalculateDefaultTglKirim(fakturDate); // Senin 16 Nov
                var (faktur, customer, user) = CreateDummyContext(fakturDate, tglKirim);

                var dto = new FakturPrintOutDto(faktur, customer, user, isKlaim: false);

                dto.FakturDate.Should().Be("Tgl: 14 November 2026");
                dto.RencanaKirimDate.Should().Be("Tgl: 16 November 2026");
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = prevCulture;
            }
        }
    }
}
