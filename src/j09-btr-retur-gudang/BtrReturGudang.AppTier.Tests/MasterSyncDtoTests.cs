using System;
using System.Collections.Generic;
using BtrReturGudang.AppTier.DTOs;
using Xunit;

namespace BtrReturGudang.AppTier.Tests
{
    public class MasterSyncDtoTests
    {
        [Fact]
        public void MasterSyncDto_TracksAllCountsAccurately()
        {
            var dto = new MasterSyncDto
            {
                ServerId = "JOGJA",
                Items = new List<BarangDto>
                {
                    new BarangDto { BrgId = "B1", BrgName = "Item 1", SatBesar = "PCS" },
                    new BarangDto { BrgId = "B2", BrgName = "Item 2", SatBesar = "DUS" }
                },
                Barcodes = new List<BarcodeDto>
                {
                    new BarcodeDto { BarcodeValue = "111", BrgId = "B1", Satuan = "PCS" }
                },
                Customers = new List<CustomerDto>
                {
                    new CustomerDto { CustId = "C1", CustName = "Customer 1" }
                },
                Drivers = new List<DriverDto>
                {
                    new DriverDto { DriverId = "D1", DriverName = "Driver 1" }
                },
                SalesPersons = new List<SalesPersonDto>
                {
                    new SalesPersonDto { SalesPersonId = "S1", SalesPersonName = "Sales 1" }
                }
            };

            Assert.Equal(2, dto.TotalBarangCount);
            Assert.Equal(1, dto.TotalBarcodeCount);
            Assert.Equal(1, dto.TotalCustomerCount);
            Assert.Equal(1, dto.TotalDriverCount);
            Assert.Equal(1, dto.TotalSalesPersonCount);
            Assert.Equal("JOGJA", dto.ServerId);
        }

        [Fact]
        public void BarangDto_MapsToAndFromDomain_PreservingSatBesarAndSatKecil()
        {
            var barang = new BtrReturGudang.Domain.Entities.Barang(
                "BRG-001",
                "Indomie Goreng",
                "DUS",
                "PCS",
                konversi: 24,
                isActive: true);

            var dto = BarangDto.FromDomain(barang);

            Assert.NotNull(dto);
            Assert.Equal("BRG-001", dto.BrgId);
            Assert.Equal("Indomie Goreng", dto.BrgName);
            Assert.Equal("DUS", dto.SatBesar);
            Assert.Equal("PCS", dto.SatKecil);
            Assert.Equal(24, dto.Konversi);
            Assert.True(dto.IsActive);

            var roundTrip = dto.ToDomain();
            Assert.NotNull(roundTrip);
            Assert.Equal("BRG-001", roundTrip.BrgId);
            Assert.Equal("Indomie Goreng", roundTrip.BrgName);
            Assert.Equal("DUS", roundTrip.SatBesar);
            Assert.Equal("PCS", roundTrip.SatKecil);
            Assert.Equal(24, roundTrip.Konversi);
            Assert.True(roundTrip.IsActive);
        }

        [Fact]
        public void BarangDto_DefaultsKonversiToOne()
        {
            var dto = new BarangDto();
            Assert.Equal(1, dto.Konversi);
        }

        [Fact]
        public void CustomerDto_MapsToAndFromDomain_PreservingAllFieldsAndAliases()
        {
            var customer = new BtrReturGudang.Domain.Entities.Customer(
                "CS0001",
                "Toko Rejeki",
                "Jl. Malioboro No. 10",
                "C-32",
                "YOGYAKARTA");

            var dto = CustomerDto.FromDomain(customer);

            Assert.NotNull(dto);
            Assert.Equal("CS0001", dto.CustomerId);
            Assert.Equal("CS0001", dto.CustId);
            Assert.Equal("C-32", dto.CustomerCode);
            Assert.Equal("C-32", dto.CustCode);
            Assert.Equal("Toko Rejeki", dto.CustomerName);
            Assert.Equal("Toko Rejeki", dto.CustName);
            Assert.Equal("Jl. Malioboro No. 10", dto.Alamat);
            Assert.Equal("YOGYAKARTA", dto.Wilayah);

            var domainRoundTrip = dto.ToDomain();
            Assert.Equal("CS0001", domainRoundTrip.CustomerId);
            Assert.Equal("C-32", domainRoundTrip.CustomerCode);
            Assert.Equal("Toko Rejeki", domainRoundTrip.CustomerName);
            Assert.Equal("Jl. Malioboro No. 10", domainRoundTrip.Alamat);
            Assert.Equal("YOGYAKARTA", domainRoundTrip.Wilayah);
        }
    }
}
