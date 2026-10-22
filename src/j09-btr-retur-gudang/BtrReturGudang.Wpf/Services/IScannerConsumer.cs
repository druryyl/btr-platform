namespace BtrReturGudang.Wpf.Services
{
    /// <summary>
    /// Interface implemented by view models or views that consume normalized barcode scans directly.
    /// Traceable to: ARCHITECTURE §5.2, P1-S03, P2-S04, P2-S06.
    /// </summary>
    public interface IScannerConsumer
    {
        /// <summary>
        /// Invoked when a barcode is scanned and normalized by WpfScannerWedgeListener.
        /// </summary>
        /// <param name="normalizedBarcode">Clean barcode string stripped of whitespace/CR/LF.</param>
        void OnBarcodeScanned(string normalizedBarcode);
    }
}
