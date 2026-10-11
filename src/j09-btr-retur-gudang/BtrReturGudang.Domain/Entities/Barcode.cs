using System;
using BtrReturGudang.Domain.Exceptions;
using BtrReturGudang.Domain.ValueObjects;

namespace BtrReturGudang.Domain.Entities
{
    /// <summary>
    /// Local cache of an active or inactive barcode mapped to an Item.
    /// Represents BTRG_Barcode table.
    /// </summary>
    public class Barcode
    {
        public string BarcodeValue { get; private set; }
        public string BrgId { get; private set; }
        public string BrgName { get; private set; }
        public string Satuan { get; private set; }
        public decimal Pengali { get; private set; }
        public bool IsActive { get; private set; }

        public Barcode()
        {
        }

        public Barcode(
            string barcodeValue,
            string brgId,
            string brgName,
            string satuan,
            decimal pengali = 1.0m,
            bool isActive = true)
        {
            var normalized = BarcodeNormalization.Normalize(barcodeValue);
            if (string.IsNullOrWhiteSpace(normalized))
                throw new DomainException("Barcode value cannot be empty.");

            if (string.IsNullOrWhiteSpace(brgId))
                throw new DomainException("Item ID (BrgId) cannot be empty.");

            if (string.IsNullOrWhiteSpace(satuan))
                throw new DomainException("Packaging unit (Satuan) cannot be empty.");

            if (pengali <= 0)
                throw new DomainException($"Pengali multiplier must be greater than zero. Received: {pengali}");

            BarcodeValue = normalized;
            BrgId = brgId;
            BrgName = brgName ?? string.Empty;
            Satuan = satuan;
            Pengali = pengali;
            IsActive = isActive;
        }

        public void Activate() => IsActive = true;
        public void Deactivate() => IsActive = false;
    }
}
