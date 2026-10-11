namespace BtrReturGudang.AppTier.DTOs
{
    /// <summary>
    /// Result model for a barcode scanning or lookup query.
    /// Provides information for active scan banners, audio triggers, and quick registration prompts.
    /// </summary>
    public class BarcodeLookupResult
    {
        public bool Found { get; set; }
        public string RawInput { get; set; }
        public string NormalizedBarcode { get; set; }
        public string BarcodeValue { get; set; }
        public string BrgId { get; set; }
        public string BrgName { get; set; }
        public string Satuan { get; set; }
        public decimal Pengali { get; set; } = 1.0m;
        public bool IsActive { get; set; }

        public static BarcodeLookupResult Hit(
            string rawInput,
            string normalizedBarcode,
            string brgId,
            string brgName,
            string satuan,
            decimal pengali,
            bool isActive)
        {
            return new BarcodeLookupResult
            {
                Found = true,
                RawInput = rawInput,
                NormalizedBarcode = normalizedBarcode,
                BarcodeValue = normalizedBarcode,
                BrgId = brgId,
                BrgName = brgName,
                Satuan = satuan,
                Pengali = pengali,
                IsActive = isActive
            };
        }

        public static BarcodeLookupResult Miss(string rawInput, string normalizedBarcode)
        {
            return new BarcodeLookupResult
            {
                Found = false,
                RawInput = rawInput,
                NormalizedBarcode = normalizedBarcode,
                BarcodeValue = normalizedBarcode,
                BrgId = null,
                BrgName = null,
                Satuan = null,
                Pengali = 1.0m,
                IsActive = false
            };
        }
    }
}
