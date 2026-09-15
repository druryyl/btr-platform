using System;

namespace btr.domain.BrgContext.BrgBarcodeAgg
{
    public class BrgBarcodeModel : IBrgBarcodeKey
    {
        public BrgBarcodeModel()
        {
        }

        public BrgBarcodeModel(string id) => BrgBarcodeId = id;

        public string BrgBarcodeId { get; set; }
        public string BarcodeValue { get; set; }
        public string BrgId { get; set; }
        public string BrgCode { get; set; }
        public string BrgName { get; set; }
        public string Satuan { get; set; } = string.Empty;
        public bool IsAktif { get; set; }

        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }

        public static string NormalizeBarcodeValue(string barcodeValue)
        {
            if (string.IsNullOrEmpty(barcodeValue))
                return string.Empty;

            var normalized = barcodeValue
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty)
                .Replace("\t", string.Empty);
            return normalized.Trim();
        }

        public static string BarcodeValueKey(string barcodeValue)
            => NormalizeBarcodeValue(barcodeValue).ToUpperInvariant();
    }
}
