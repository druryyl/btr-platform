using System;

namespace btr.distrib.InventoryContext.BrgBarcodeAgg
{
    public class BrgBarcodeFormBarcodeDto
    {
        public BrgBarcodeFormBarcodeDto()
        {
        }

        public BrgBarcodeFormBarcodeDto(string brgBarcodeId, string barcodeValue,
            string brgId, string brgCode, string brgName, string satuan,
            bool isAktif, string modifiedBy, DateTime modifiedDate)
        {
            BrgBarcodeId = brgBarcodeId;
            BarcodeValue = barcodeValue;
            BrgId = brgId;
            BrgCode = brgCode;
            BrgName = brgName;
            Satuan = satuan;
            IsAktif = isAktif;
            ModifiedBy = modifiedBy;
            ModifiedDate = modifiedDate;
        }

        public string BrgBarcodeId { get; set; }
        public string BarcodeValue { get; set; }
        public string BrgId { get; set; }
        public string BrgCode { get; set; }
        public string BrgName { get; set; }
        public string Satuan { get; set; }
        public bool IsAktif { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
