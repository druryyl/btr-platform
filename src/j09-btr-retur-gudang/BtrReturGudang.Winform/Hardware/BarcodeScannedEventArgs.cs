using System;

namespace BtrReturGudang.Winform.Hardware
{
    /// <summary>
    /// Event arguments payload for barcode scanner interception.
    /// Traceable to: TD-005, ARCHITECTURE §5.1, P2-S06.
    /// </summary>
    public class BarcodeScannedEventArgs : EventArgs
    {
        /// <summary>
        /// Raw scanned string before normalization.
        /// </summary>
        public string RawBarcode { get; }

        /// <summary>
        /// Normalized barcode string satisfying VO-01 rules.
        /// </summary>
        public string NormalizedBarcode { get; }

        /// <summary>
        /// Timestamp when scan was intercepted.
        /// </summary>
        public DateTime Timestamp { get; }

        public BarcodeScannedEventArgs(string rawBarcode, string normalizedBarcode)
        {
            RawBarcode = rawBarcode ?? string.Empty;
            NormalizedBarcode = normalizedBarcode ?? string.Empty;
            Timestamp = DateTime.UtcNow;
        }

        public override string ToString() => NormalizedBarcode;
    }
}
