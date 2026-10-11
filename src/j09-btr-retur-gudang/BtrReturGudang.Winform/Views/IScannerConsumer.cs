using System;

namespace BtrReturGudang.Winform.Views
{
    /// <summary>
    /// Interface implemented by views that consume normalized barcode scans directly from the shell window.
    /// Traceable to: ARCHITECTURE §5.2, §6, TD-011, P3-S07, P3-S08.
    /// </summary>
    public interface IScannerConsumer
    {
        /// <summary>
        /// Invoked when a barcode is scanned and normalized by ScannerWedgeListener.
        /// </summary>
        /// <param name="normalizedBarcode">Clean barcode string stripped of whitespace/CR/LF.</param>
        void OnBarcodeScanned(string normalizedBarcode);
    }
}
