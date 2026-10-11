using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using BtrReturGudang.Domain.ValueObjects;

namespace BtrReturGudang.Winform.Hardware
{
    /// <summary>
    /// Global keyboard wedge message filter for USB HID barcode scanners.
    /// Intercepts rapid keystroke bursts (&lt;= 50ms inter-character interval) terminating in VK_RETURN (0x0D).
    /// Suppresses Enter and normalizes barcode string per VO-01 rules before dispatching BarcodeScanned event.
    /// Traceable to: TD-005, GAP-003, OQ-002, ARCHITECTURE §5.1, P2-S06.
    /// </summary>
    public class ScannerWedgeListener : IMessageFilter
    {
        public const int WM_KEYDOWN = 0x0100;
        public const int WM_KEYUP = 0x0101;
        public const int WM_CHAR = 0x0102;
        public const int VK_RETURN = 0x0D;

        private readonly StringBuilder _buffer = new StringBuilder();
        private readonly Stopwatch _stopwatch;
        private readonly Func<long> _timeProvider;

        private long _lastKeystrokeTimestamp = 0;
        private bool _suppressNextEnterChar = false;

        /// <summary>
        /// Fires when a complete barcode sequence is intercepted and normalized.
        /// </summary>
        public event EventHandler<BarcodeScannedEventArgs> BarcodeScanned;

        /// <summary>
        /// Maximum inter-keystroke interval in milliseconds to qualify as a scanner burst.
        /// Keystrokes arriving after this threshold flush the buffer (human typing pass-through).
        /// Default is 50ms.
        /// </summary>
        public int InterKeystrokeTimeoutMs { get; set; } = 50;

        /// <summary>
        /// Minimum characters buffered to be recognized as a valid barcode.
        /// Default is 1 character.
        /// </summary>
        public int MinBarcodeLength { get; set; } = 1;

        /// <summary>
        /// Master toggle to enable or disable wedge filtering.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Current length of accumulated characters in the buffer.
        /// </summary>
        public int BufferedLength => _buffer.Length;

        /// <summary>
        /// Current snapshot of characters in the buffer.
        /// </summary>
        public string CurrentBuffer => _buffer.ToString();

        public ScannerWedgeListener(Func<long> timeProvider = null)
        {
            if (timeProvider != null)
            {
                _timeProvider = timeProvider;
            }
            else
            {
                _stopwatch = new Stopwatch();
                _stopwatch.Start();
                _timeProvider = () => _stopwatch.ElapsedMilliseconds;
            }
        }

        /// <summary>
        /// Filters incoming Windows messages before dispatching to focused controls.
        /// Returns true to suppress the message; false to pass through.
        /// </summary>
        public bool PreFilterMessage(ref Message m)
        {
            if (!IsEnabled)
                return false;

            long now = _timeProvider();
            long elapsedSinceLast = _lastKeystrokeTimestamp > 0 ? (now - _lastKeystrokeTimestamp) : long.MaxValue;

            if (m.Msg == WM_KEYDOWN)
            {
                int vk = m.WParam.ToInt32();

                // Non-printable navigation / function keys indicate human navigation; flush buffer
                if (IsNavigationOrFunctionKey(vk))
                {
                    FlushBuffer();
                    _lastKeystrokeTimestamp = 0;
                    return false;
                }

                if (vk == VK_RETURN)
                {
                    // If buffer holds characters from a rapid burst arriving within timeout
                    if (_buffer.Length >= MinBarcodeLength && elapsedSinceLast <= InterKeystrokeTimeoutMs)
                    {
                        string raw = _buffer.ToString();
                        string normalized = BarcodeNormalization.Normalize(raw);

                        FlushBuffer();
                        _lastKeystrokeTimestamp = 0;
                        _suppressNextEnterChar = true;

                        OnBarcodeScanned(new BarcodeScannedEventArgs(raw, normalized));

                        // Suppress VK_RETURN from active child control
                        return true;
                    }

                    // Human typing pass-through: elapsed time exceeded timeout or empty buffer
                    FlushBuffer();
                    _lastKeystrokeTimestamp = 0;
                    return false;
                }
            }
            else if (m.Msg == WM_CHAR)
            {
                char c = (char)m.WParam.ToInt32();

                // Handle Enter character (CR / LF)
                if (c == '\r' || c == '\n')
                {
                    if (_suppressNextEnterChar)
                    {
                        _suppressNextEnterChar = false;
                        return true; // Suppress translated WM_CHAR of previously handled VK_RETURN
                    }

                    if (_buffer.Length >= MinBarcodeLength && elapsedSinceLast <= InterKeystrokeTimeoutMs)
                    {
                        string raw = _buffer.ToString();
                        string normalized = BarcodeNormalization.Normalize(raw);

                        FlushBuffer();
                        _lastKeystrokeTimestamp = 0;

                        OnBarcodeScanned(new BarcodeScannedEventArgs(raw, normalized));
                        return true;
                    }

                    FlushBuffer();
                    _lastKeystrokeTimestamp = 0;
                    return false;
                }

                _suppressNextEnterChar = false;

                // Printable character (ASCII 32 and above)
                if (c >= 32)
                {
                    if (elapsedSinceLast > InterKeystrokeTimeoutMs)
                    {
                        // Flushes buffer if interval exceeds 50ms (human typing pass-through)
                        FlushBuffer();
                    }

                    _buffer.Append(c);
                    _lastKeystrokeTimestamp = now;

                    // Allow normal keystrokes to pass through to the active control
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Clears the accumulated keystroke buffer.
        /// </summary>
        public void FlushBuffer()
        {
            _buffer.Clear();
        }

        /// <summary>
        /// Resets the buffer, timestamp, and suppression state.
        /// </summary>
        public void Reset()
        {
            FlushBuffer();
            _lastKeystrokeTimestamp = 0;
            _suppressNextEnterChar = false;
        }

        /// <summary>
        /// Simulates an incoming scan directly, dispatching BarcodeScanned.
        /// Useful for testing and manual barcode injection.
        /// </summary>
        public void SimulateScan(string rawBarcode)
        {
            string normalized = BarcodeNormalization.Normalize(rawBarcode);
            OnBarcodeScanned(new BarcodeScannedEventArgs(rawBarcode, normalized));
        }

        protected virtual void OnBarcodeScanned(BarcodeScannedEventArgs e)
        {
            BarcodeScanned?.Invoke(this, e);
        }

        private static bool IsNavigationOrFunctionKey(int vk)
        {
            // Backspace, Tab, Escape, Page Up/Down, End, Home, Left, Up, Right, Down, F1-F12
            return vk == 0x08 // Backspace
                || vk == 0x09 // Tab
                || vk == 0x1B // Escape
                || (vk >= 0x21 && vk <= 0x28) // PageUp..Down
                || (vk >= 0x70 && vk <= 0x7B); // F1..F12
        }
    }
}
