using System;
using BtrReturGudang.Domain.Common;
using BtrReturGudang.Domain.Enums;
using BtrReturGudang.Domain.Exceptions;
using BtrReturGudang.Domain.ValueObjects;

namespace BtrReturGudang.Domain.Entities
{
    /// <summary>
    /// Represents an on-the-spot barcode registration request queued locally for Cloud API upload.
    /// Represents BTRG_BarcodeRegistration table.
    /// </summary>
    public class BarcodeRegistration
    {
        public string RequestId { get; private set; }
        public string Barcode { get; private set; }
        public string BrgId { get; private set; }
        public string Satuan { get; private set; }
        public string WarehouseCode { get; private set; }
        public string OperatorName { get; private set; }
        public BarcodeRegistrationStatus StatusSync { get; private set; }
        public DateTime CreatedDate { get; private set; }

        public BarcodeRegistration()
        {
        }

        public BarcodeRegistration(
            string requestId,
            string barcode,
            string brgId,
            string satuan,
            string warehouseCode,
            string operatorName,
            BarcodeRegistrationStatus statusSync = BarcodeRegistrationStatus.Pending,
            DateTime? createdDate = null)
        {
            var normalized = BarcodeNormalization.Normalize(barcode);
            if (string.IsNullOrWhiteSpace(normalized))
                throw new DomainException("Barcode value cannot be empty.");

            if (string.IsNullOrWhiteSpace(brgId))
                throw new DomainException("Item ID (BrgId) cannot be empty.");

            if (string.IsNullOrWhiteSpace(satuan))
                throw new DomainException("Unit (Satuan) cannot be empty.");

            if (string.IsNullOrWhiteSpace(warehouseCode))
                throw new DomainException("Warehouse Code cannot be empty.");

            if (string.IsNullOrWhiteSpace(operatorName))
                throw new DomainException("Operator Name cannot be empty.");

            RequestId = string.IsNullOrWhiteSpace(requestId) ? UlidGenerator.NewUlid() : requestId;
            Barcode = normalized;
            BrgId = brgId;
            Satuan = satuan;
            WarehouseCode = warehouseCode;
            OperatorName = operatorName;
            StatusSync = statusSync;
            CreatedDate = createdDate ?? DateTime.Now;
        }

        public static BarcodeRegistration Create(
            string barcode,
            string brgId,
            string satuan,
            string warehouseCode,
            string operatorName)
        {
            return new BarcodeRegistration(
                UlidGenerator.NewUlid(),
                barcode,
                brgId,
                satuan,
                warehouseCode,
                operatorName,
                BarcodeRegistrationStatus.Pending,
                DateTime.Now);
        }

        public void MarkUploaded()
        {
            StatusSync = BarcodeRegistrationStatus.Uploaded;
        }
    }
}
