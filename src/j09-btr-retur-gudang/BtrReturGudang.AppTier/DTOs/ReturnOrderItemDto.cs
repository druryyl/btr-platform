using System;
using BtrReturGudang.Domain.Entities;
using BtrReturGudang.Domain.Enums;

namespace BtrReturGudang.AppTier.DTOs
{
    /// <summary>
    /// Data Transfer Object representing a line item in a Return Order.
    /// Used for persistence transfer, UI grid binding, and Cloud API transmission.
    /// </summary>
    public class ReturnOrderItemDto
    {
        public string ReturnOrderId { get; set; }
        public int NoUrut { get; set; }
        public string BrgId { get; set; }
        public string BrgCode { get; set; }
        public string BrgName { get; set; }
        public string Barcode { get; set; }
        public string JenisRetur { get; set; } = "BAGUS";
        public decimal Qty { get; set; }
        public string SatId { get; set; }
        public string Satuan { get => SatId; set => SatId = value; }
        public string Keterangan { get; set; }

        public ReturnOrderItemDto()
        {
        }

        public ReturnOrderItemDto(
            string returnOrderId,
            int noUrut,
            string brgId,
            string brgName,
            string barcode,
            string jenisRetur,
            decimal qty,
            string satId,
            string keterangan = null,
            string brgCode = null)
        {
            ReturnOrderId = returnOrderId;
            NoUrut = noUrut;
            BrgId = brgId;
            BrgCode = brgCode ?? brgId;
            BrgName = brgName;
            Barcode = barcode;
            JenisRetur = jenisRetur;
            Qty = qty;
            SatId = satId;
            Keterangan = keterangan;
        }

        public static ReturnOrderItemDto FromDomain(ReturnOrderItem item)
        {
            if (item == null) return null;
            return new ReturnOrderItemDto(
                item.ReturnOrderId,
                item.NoUrut,
                item.BrgId,
                item.BrgName,
                item.Barcode,
                item.JenisRetur.ToString(),
                item.Qty,
                item.SatId,
                item.Keterangan);
        }

        public ReturnOrderItem ToDomain()
        {
            var jenis = Enum.TryParse<JenisRetur>(JenisRetur, true, out var parsed)
                ? parsed
                : Domain.Enums.JenisRetur.BAGUS;

            return new ReturnOrderItem(
                ReturnOrderId,
                NoUrut,
                BrgId,
                BrgName,
                Barcode,
                jenis,
                Qty,
                SatId,
                Keterangan);
        }
    }
}
