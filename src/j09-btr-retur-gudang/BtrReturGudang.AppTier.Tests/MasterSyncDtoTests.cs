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
                    new BarangDto { BrgId = "B1", BrgName = "Item 1", Satuan1 = "PCS" },
                    new BarangDto { BrgId = "B2", BrgName = "Item 2", Satuan1 = "DUS" }
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
    }
}
