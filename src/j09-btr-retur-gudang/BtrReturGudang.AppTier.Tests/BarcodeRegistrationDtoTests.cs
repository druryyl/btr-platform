using System;
using BtrReturGudang.AppTier.DTOs;
using BtrReturGudang.Domain.Entities;
using BtrReturGudang.Domain.Enums;
using Xunit;

namespace BtrReturGudang.AppTier.Tests
{
    public class BarcodeRegistrationDtoTests
    {
        [Fact]
        public void FromDomain_MapsPropertiesAndCloudAliases()
        {
            var entity = new BarcodeRegistration(
                "01JABCDEF01234567890BCREG1",
                "8991234567890",
                "BRG-01",
                "DUS",
                "WHS-01",
                "Operator-1",
                BarcodeRegistrationStatus.Pending);

            var dto = BarcodeRegistrationDto.FromDomain(entity, "Indomie Goreng", 24m);

            Assert.NotNull(dto);
            Assert.Equal("01JABCDEF01234567890BCREG1", dto.RequestId);
            Assert.Equal("01JABCDEF01234567890BCREG1", dto.ClientRequestId);
            Assert.Equal("8991234567890", dto.Barcode);
            Assert.Equal("8991234567890", dto.BarcodeValue);
            Assert.Equal("BRG-01", dto.BrgId);
            Assert.Equal("Indomie Goreng", dto.BrgName);
            Assert.Equal("DUS", dto.Satuan);
            Assert.Equal(24m, dto.Pengali);
            Assert.Equal("Pending", dto.StatusSync);
        }

        [Fact]
        public void ToDomain_ConvertsToValidBarcodeRegistration()
        {
            var dto = new BarcodeRegistrationDto
            {
                RequestId = "01JABCDEF01234567890BCREG2",
                Barcode = "8999876543210",
                BrgId = "BRG-02",
                Satuan = "PCS",
                WarehouseCode = "WHS-01",
                OperatorName = "Operator-1",
                StatusSync = "Pending"
            };

            var domain = dto.ToDomain();

            Assert.Equal("01JABCDEF01234567890BCREG2", domain.RequestId);
            Assert.Equal("8999876543210", domain.Barcode);
            Assert.Equal("BRG-02", domain.BrgId);
            Assert.Equal("PCS", domain.Satuan);
            Assert.Equal(BarcodeRegistrationStatus.Pending, domain.StatusSync);
        }
    }
}
