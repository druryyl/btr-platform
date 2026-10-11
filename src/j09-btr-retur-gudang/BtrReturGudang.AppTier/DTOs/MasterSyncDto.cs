using System;
using System.Collections.Generic;

namespace BtrReturGudang.AppTier.DTOs
{
    /// <summary>
    /// Master Sync transfer payload containing reference deltas pulled from Cloud API.
    /// </summary>
    public class MasterSyncDto
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string ServerId { get; set; }

        public List<BarangDto> Items { get; set; } = new List<BarangDto>();
        public List<BarcodeDto> Barcodes { get; set; } = new List<BarcodeDto>();
        public List<CustomerDto> Customers { get; set; } = new List<CustomerDto>();
        public List<DriverDto> Drivers { get; set; } = new List<DriverDto>();
        public List<SalesPersonDto> SalesPersons { get; set; } = new List<SalesPersonDto>();

        public int TotalBarangCount => Items?.Count ?? 0;
        public int TotalBarcodeCount => Barcodes?.Count ?? 0;
        public int TotalCustomerCount => Customers?.Count ?? 0;
        public int TotalDriverCount => Drivers?.Count ?? 0;
        public int TotalSalesPersonCount => SalesPersons?.Count ?? 0;
    }
}
