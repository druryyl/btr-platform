using System;
using BtrReturGudang.Domain.Enums;
using BtrReturGudang.Domain.Exceptions;

namespace BtrReturGudang.Domain.Entities
{
    /// <summary>
    /// Line item of a Return Order recording physical received goods.
    /// Composite key: (ReturnOrderId, NoUrut).
    /// </summary>
    public class ReturnOrderItem
    {
        public string ReturnOrderId { get; internal set; }
        public int NoUrut { get; internal set; }
        public string BrgId { get; private set; }
        public string BrgName { get; private set; }
        public string Barcode { get; private set; }
        public JenisRetur JenisRetur { get; private set; }
        public decimal Qty { get; private set; }
        public string SatId { get; private set; }
        public string Keterangan { get; private set; }

        /// <summary>
        /// Parameterless constructor for Dapper and deserializers.
        /// </summary>
        public ReturnOrderItem()
        {
        }

        public ReturnOrderItem(
            string returnOrderId,
            int noUrut,
            string brgId,
            string brgName,
            string barcode,
            JenisRetur jenisRetur,
            decimal qty,
            string satId,
            string keterangan = null)
        {
            ValidateValues(brgId, brgName, qty, satId);

            ReturnOrderId = returnOrderId;
            NoUrut = noUrut;
            BrgId = brgId;
            BrgName = brgName;
            Barcode = barcode;
            JenisRetur = jenisRetur;
            Qty = qty;
            SatId = satId;
            Keterangan = keterangan;
        }

        /// <summary>
        /// Factory method for new intake lines before sequential index assignment.
        /// </summary>
        public static ReturnOrderItem Create(
            string brgId,
            string brgName,
            string barcode,
            JenisRetur jenisRetur,
            decimal qty,
            string satId,
            string keterangan = null)
        {
            return new ReturnOrderItem(null, 0, brgId, brgName, barcode, jenisRetur, qty, satId, keterangan);
        }

        public void Update(decimal newQty, JenisRetur newJenisRetur, string newKeterangan = null)
        {
            if (newQty <= 0)
                throw new DomainException($"Return item quantity must be greater than zero. Received: {newQty}");

            Qty = newQty;
            JenisRetur = newJenisRetur;
            Keterangan = newKeterangan;
        }

        private static void ValidateValues(string brgId, string brgName, decimal qty, string satId)
        {
            if (string.IsNullOrWhiteSpace(brgId))
                throw new DomainException("Item ID (BrgId) is mandatory.");

            if (string.IsNullOrWhiteSpace(brgName))
                throw new DomainException("Item Name (BrgName) is mandatory.");

            if (qty <= 0)
                throw new DomainException($"Return item quantity must be greater than zero (BR-010). Received: {qty}");

            if (string.IsNullOrWhiteSpace(satId))
                throw new DomainException("Unit (SatId) is mandatory (BR-011).");
        }
    }
}
