using System;
using BtrReturGudang.Domain.Entities;
using BtrReturGudang.Domain.Enums;

namespace BtrReturGudang.AppTier.DTOs
{
    /// <summary>
    /// Data Transfer Object representing an on-the-spot barcode registration request.
    /// Compatible with both local persistence and Cloud API (POST api/barcode-registration).
    /// </summary>
    public class BarcodeRegistrationDto
    {
        public string RequestId { get; set; }
        public string ClientRequestId => RequestId; // Cloud API alias
        public string Barcode { get; set; }
        public string BarcodeValue => Barcode; // Cloud API alias
        public string BrgId { get; set; }
        public string BrgName { get; set; }
        public string Satuan { get; set; }
        public decimal Pengali { get; set; } = 1.0m;
        public string WarehouseCode { get; set; }
        public string OperatorName { get; set; }
        public string StatusSync { get; set; } = "Pending";
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public BarcodeRegistrationDto()
        {
        }

        public BarcodeRegistrationDto(
            string requestId,
            string barcode,
            string brgId,
            string satuan,
            string warehouseCode,
            string operatorName,
            string statusSync = "Pending",
            DateTime? createdDate = null,
            string brgName = null,
            decimal pengali = 1.0m)
        {
            RequestId = requestId;
            Barcode = barcode;
            BrgId = brgId;
            BrgName = brgName;
            Satuan = satuan;
            Pengali = pengali;
            WarehouseCode = warehouseCode;
            OperatorName = operatorName;
            StatusSync = statusSync;
            CreatedDate = createdDate ?? DateTime.Now;
        }

        public static BarcodeRegistrationDto FromDomain(BarcodeRegistration reg, string brgName = null, decimal pengali = 1.0m)
        {
            if (reg == null) return null;
            return new BarcodeRegistrationDto(
                reg.RequestId,
                reg.Barcode,
                reg.BrgId,
                reg.Satuan,
                reg.WarehouseCode,
                reg.OperatorName,
                reg.StatusSync.ToString(),
                reg.CreatedDate,
                brgName,
                pengali);
        }

        public BarcodeRegistration ToDomain()
        {
            var status = Enum.TryParse<BarcodeRegistrationStatus>(StatusSync, true, out var parsed)
                ? parsed
                : BarcodeRegistrationStatus.Pending;

            return new BarcodeRegistration(
                RequestId,
                Barcode,
                BrgId,
                Satuan,
                WarehouseCode,
                OperatorName,
                status,
                CreatedDate);
        }
    }
}
