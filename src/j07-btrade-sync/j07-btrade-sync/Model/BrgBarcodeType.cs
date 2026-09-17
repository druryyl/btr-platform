using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace j07_btrade_sync.Model
{
    public class BrgBarcodeType
    {
        public BrgBarcodeType(string brgBarcodeId, string barcodeValue, string brgId,
            string brgCode, string brgName, string satuan, bool isAktif, byte[] rowVer)
        {
            BrgBarcodeId = brgBarcodeId;
            BarcodeValue = barcodeValue;
            BrgId = brgId;
            BrgCode = brgCode;
            BrgName = brgName;
            Satuan = satuan;
            IsAktif = isAktif;
            RowVer = rowVer;
        }
        public string BrgBarcodeId { get; set; }
        public string BarcodeValue { get; set; }
        public string BrgId { get; set; }
        public string BrgCode { get; set; }
        public string BrgName { get; set; }
        public string Satuan { get; set; }
        public bool IsAktif { get; set; }
        public byte[] RowVer { get; set; }
    }
}
