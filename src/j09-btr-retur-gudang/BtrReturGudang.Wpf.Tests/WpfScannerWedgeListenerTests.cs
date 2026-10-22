using System;
using System.Windows.Interop;
using BtrReturGudang.Wpf.Services;
using Xunit;

namespace BtrReturGudang.Wpf.Tests
{
    public class WpfScannerWedgeListenerTests
    {
        private static MSG CreateKeyMessage(int msg, int wParam)
        {
            return new MSG
            {
                hwnd = IntPtr.Zero,
                message = msg,
                wParam = new IntPtr(wParam),
                lParam = IntPtr.Zero,
                time = 0,
                pt_x = 0,
                pt_y = 0
            };
        }

        private class MockScannerConsumer : IScannerConsumer
        {
            public string LastScannedBarcode { get; private set; }
            public int ScanCount { get; private set; }

            public void OnBarcodeScanned(string barcode)
            {
                LastScannedBarcode = barcode;
                ScanCount++;
            }
        }

        [Fact]
        public void ScannerWedge_FastBurst_InterceptsAndSuppressesEnter()
        {
            long mockTime = 1000;
            var listener = new WpfScannerWedgeListener(() => mockTime)
            {
                InterKeystrokeTimeoutMs = 50,
                MinBarcodeLength = 3
            };

            var mockConsumer = new MockScannerConsumer();
            listener.ActiveConsumer = mockConsumer;

            string receivedBarcode = null;
            BarcodeScannedEventArgs detailedEvent = null;
            listener.BarcodeScanned += b => receivedBarcode = b;
            listener.BarcodeScannedDetailed += (s, e) => detailedEvent = e;

            string barcode = "8992753123456";

            // Feed characters with 10ms interval (<= 50ms)
            foreach (char c in barcode)
            {
                mockTime += 10;
                var wmChar = CreateKeyMessage(WpfScannerWedgeListener.WM_CHAR, c);
                bool handledChar = listener.ProcessMessage(ref wmChar);
                Assert.False(handledChar); // printable characters pass through
            }

            // Feed VK_RETURN 10ms after last char
            mockTime += 10;
            var wmKeyDownEnter = CreateKeyMessage(WpfScannerWedgeListener.WM_KEYDOWN, WpfScannerWedgeListener.VK_RETURN);
            bool handledEnter = listener.ProcessMessage(ref wmKeyDownEnter);

            // Assertions
            Assert.True(handledEnter); // VK_RETURN must be suppressed
            Assert.Equal("8992753123456", receivedBarcode);
            Assert.NotNull(detailedEvent);
            Assert.Equal("8992753123456", detailedEvent.RawBarcode);
            Assert.Equal("8992753123456", detailedEvent.NormalizedBarcode);
            Assert.Equal("8992753123456", mockConsumer.LastScannedBarcode);
            Assert.Equal(1, mockConsumer.ScanCount);
            Assert.Equal(0, listener.BufferedLength);

            // Subsequent WM_CHAR for Enter is also suppressed
            var wmCharEnter = CreateKeyMessage(WpfScannerWedgeListener.WM_CHAR, '\r');
            bool handledCharEnter = listener.ProcessMessage(ref wmCharEnter);
            Assert.True(handledCharEnter);
        }

        [Fact]
        public void ScannerWedge_PreservesLeadingZerosInNormalizedBarcode()
        {
            long mockTime = 1000;
            var listener = new WpfScannerWedgeListener(() => mockTime);

            string receivedBarcode = null;
            listener.BarcodeScanned += barcode => receivedBarcode = barcode;

            string barcodeWithLeadingZeros = "000123987";
            foreach (char c in barcodeWithLeadingZeros)
            {
                mockTime += 5;
                var wmChar = CreateKeyMessage(WpfScannerWedgeListener.WM_CHAR, c);
                listener.ProcessMessage(ref wmChar);
            }

            mockTime += 5;
            var wmKeyDown = CreateKeyMessage(WpfScannerWedgeListener.WM_KEYDOWN, WpfScannerWedgeListener.VK_RETURN);
            listener.ProcessMessage(ref wmKeyDown);

            Assert.Equal("000123987", receivedBarcode);
        }

        [Fact]
        public void ScannerWedge_SlowTyping_FlushesBufferAndDoesNotSuppressEnter()
        {
            long mockTime = 1000;
            var listener = new WpfScannerWedgeListener(() => mockTime)
            {
                InterKeystrokeTimeoutMs = 50
            };

            var mockConsumer = new MockScannerConsumer();
            listener.ActiveConsumer = mockConsumer;

            string receivedBarcode = null;
            listener.BarcodeScanned += barcode => receivedBarcode = barcode;

            // User types 'A'
            mockTime += 100;
            var msgA = CreateKeyMessage(WpfScannerWedgeListener.WM_CHAR, 'A');
            listener.ProcessMessage(ref msgA);

            // User pauses 200ms (> 50ms) then types 'B'
            mockTime += 200;
            var msgB = CreateKeyMessage(WpfScannerWedgeListener.WM_CHAR, 'B');
            listener.ProcessMessage(ref msgB);

            // Buffer was flushed on 'B' because interval > 50ms, so buffer has only "B"
            Assert.Equal("B", listener.CurrentBuffer);

            // User pauses 200ms (> 50ms) then presses Enter
            mockTime += 200;
            var msgEnter = CreateKeyMessage(WpfScannerWedgeListener.WM_KEYDOWN, WpfScannerWedgeListener.VK_RETURN);
            bool suppressed = listener.ProcessMessage(ref msgEnter);

            // Enter should NOT be suppressed (human typing pass-through)
            Assert.False(suppressed);
            Assert.Null(receivedBarcode);
            Assert.Equal(0, mockConsumer.ScanCount);
            Assert.Equal(0, listener.BufferedLength);
        }

        [Fact]
        public void ScannerWedge_NavigationKey_FlushesBuffer()
        {
            long mockTime = 1000;
            var listener = new WpfScannerWedgeListener(() => mockTime);

            // Fast burst of 2 chars
            mockTime += 10;
            var msg1 = CreateKeyMessage(WpfScannerWedgeListener.WM_CHAR, '1');
            listener.ProcessMessage(ref msg1);

            mockTime += 10;
            var msg2 = CreateKeyMessage(WpfScannerWedgeListener.WM_CHAR, '2');
            listener.ProcessMessage(ref msg2);

            Assert.Equal(2, listener.BufferedLength);

            // User hits F2 (0x71)
            mockTime += 10;
            var msgF2 = CreateKeyMessage(WpfScannerWedgeListener.WM_KEYDOWN, 0x71);
            listener.ProcessMessage(ref msgF2);

            Assert.Equal(0, listener.BufferedLength); // buffer flushed
        }

        [Fact]
        public void ScannerWedge_SimulateScan_FiresBarcodeScannedEvent()
        {
            var listener = new WpfScannerWedgeListener();
            var mockConsumer = new MockScannerConsumer();
            listener.ActiveConsumer = mockConsumer;

            string received = null;
            listener.BarcodeScanned += barcode => received = barcode;

            listener.SimulateScan("00987654321");

            Assert.Equal("00987654321", received);
            Assert.Equal("00987654321", mockConsumer.LastScannedBarcode);
            Assert.Equal(1, mockConsumer.ScanCount);
        }

        [Fact]
        public void ScannerWedge_WhenDisabled_PassesAllMessagesThrough()
        {
            long mockTime = 1000;
            var listener = new WpfScannerWedgeListener(() => mockTime)
            {
                IsEnabled = false
            };

            var msgEnter = CreateKeyMessage(WpfScannerWedgeListener.WM_KEYDOWN, WpfScannerWedgeListener.VK_RETURN);
            bool suppressed = listener.ProcessMessage(ref msgEnter);

            Assert.False(suppressed);
        }

        [Fact]
        public void ScannerWedge_MinBarcodeLength_Enforced()
        {
            long mockTime = 1000;
            var listener = new WpfScannerWedgeListener(() => mockTime)
            {
                MinBarcodeLength = 5
            };

            string received = null;
            listener.BarcodeScanned += barcode => received = barcode;

            // Fast burst of only 3 characters
            for (int i = 0; i < 3; i++)
            {
                mockTime += 10;
                var msgChar = CreateKeyMessage(WpfScannerWedgeListener.WM_CHAR, 'X');
                listener.ProcessMessage(ref msgChar);
            }

            mockTime += 10;
            var msgEnter = CreateKeyMessage(WpfScannerWedgeListener.WM_KEYDOWN, WpfScannerWedgeListener.VK_RETURN);
            bool suppressed = listener.ProcessMessage(ref msgEnter);

            // Below minimum length -> not recognized as barcode, Enter passes through
            Assert.False(suppressed);
            Assert.Null(received);
        }
    }
}
