using System;
using System.Linq;
using BtrReturGudang.AppTier.DTOs;
using BtrReturGudang.Domain.Entities;
using BtrReturGudang.Domain.Enums;
using Xunit;

namespace BtrReturGudang.AppTier.Tests
{
    public class ReturnOrderDtoTests
    {
        [Fact]
        public void FromDomain_MapsAllPropertiesAndCalculations()
        {
            var order = new ReturnOrder(
                "01JABCDEF01234567890ABCDEF",
                new DateTime(2026, 1, 10),
                "CUST-001",
                "Toko Rejeki",
                "WHS-01",
                "Operator-1",
                "DRV-01",
                "Budi",
                "SLS-01",
                "Agus",
                "Barang rusak saat pengiriman",
                StatusSync.Draft,
                customerCode: "C-32",
                wilayah: "YOGYAKARTA");

            order.AddItem("BRG-01", "Indomie Goreng", "8991234567890", JenisRetur.BAGUS, 10, "PCS");
            order.AddItem("BRG-02", "Sedaap Goreng", "8999876543210", JenisRetur.RUSAK, 5, "PCS");

            var dto = ReturnOrderDto.FromDomain(order);

            Assert.NotNull(dto);
            Assert.Equal("01JABCDEF01234567890ABCDEF", dto.ReturnOrderId);
            Assert.Equal("CUST-001", dto.CustId);
            Assert.Equal("CUST-001", dto.CustomerId);
            Assert.Equal("C-32", dto.CustomerCode);
            Assert.Equal("C-32", dto.CustCode);
            Assert.Equal("Toko Rejeki", dto.CustName);
            Assert.Equal("Toko Rejeki", dto.CustomerName);
            Assert.Equal("YOGYAKARTA", dto.Wilayah);
            Assert.Equal("WHS-01", dto.WarehouseCode);
            Assert.Equal("Operator-1", dto.OperatorName);
            Assert.Equal("DRV-01", dto.DriverId);
            Assert.Equal("Budi", dto.DriverName);
            Assert.Equal("SLS-01", dto.SalesPersonId);
            Assert.Equal("Agus", dto.SalesPersonName);
            Assert.Equal("Barang rusak saat pengiriman", dto.Keterangan);
            Assert.Equal("Barang rusak saat pengiriman", dto.Note);
            Assert.Equal("Draft", dto.StatusSync);
            Assert.False(dto.IsSynced);
            Assert.Equal(2, dto.TotalItemsCount);
            Assert.Equal(10m, dto.TotalQtyBagus);
            Assert.Equal(5m, dto.TotalQtyRusak);
            Assert.Equal(2, dto.Items.Count);
            Assert.Equal(2, dto.ListItem.Count());
        }

        [Fact]
        public void FromDomain_WithBlankOptionalIds_ClearsOptionalNames()
        {
            var order = new ReturnOrder(
                "01JABCDEF01234567890ABCDEF",
                new DateTime(2026, 1, 10),
                "CUST-001",
                "Toko Rejeki",
                "WHS-01",
                "Operator-1",
                null,
                "-- Pilih Driver (Opsional) --",
                null,
                "-- Pilih Salesman (Opsional) --",
                "Catatan",
                StatusSync.Draft);

            order.AddItem("BRG-01", "Indomie Goreng", "8991234567890", JenisRetur.BAGUS, 10, "PCS");

            var dto = ReturnOrderDto.FromDomain(order);

            Assert.True(string.IsNullOrEmpty(dto.DriverName));
            Assert.True(string.IsNullOrEmpty(dto.SalesPersonName));
        }

        [Fact]
        public void ToDomain_RestoresAggregateWithItems()
        {
            var dto = new ReturnOrderDto
            {
                ReturnOrderId = "01JABCDEF01234567890ABCDEF",
                ReturnOrderDate = new DateTime(2026, 1, 10),
                CustId = "CUST-001",
                CustName = "Toko Rejeki",
                CustomerCode = "C-32",
                Wilayah = "YOGYAKARTA",
                WarehouseCode = "WHS-01",
                OperatorName = "Operator-1",
                Keterangan = "Catatan",
                StatusSync = "Draft"
            };

            dto.Items.Add(new ReturnOrderItemDto(
                "01JABCDEF01234567890ABCDEF",
                1,
                "BRG-01",
                "Indomie Goreng",
                "8991234567890",
                "BAGUS",
                12,
                "PCS"));

            var domain = dto.ToDomain();

            Assert.Equal("01JABCDEF01234567890ABCDEF", domain.ReturnOrderId);
            Assert.Equal("CUST-001", domain.CustId);
            Assert.Equal("Toko Rejeki", domain.CustName);
            Assert.Equal("C-32", domain.CustomerCode);
            Assert.Equal("YOGYAKARTA", domain.Wilayah);
            Assert.Equal(StatusSync.Draft, domain.StatusSync);
            Assert.Single(domain.Items);
            Assert.Equal(12m, domain.TotalQtyBagus);
        }
    }
}
