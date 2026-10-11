using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BtrReturGudang.Winform.Hardware;
using Xunit;

namespace BtrReturGudang.Winform.Tests
{
    public class ScannerWedgeListenerTests
    {
        private static Message CreateKeyMessage(int msg, int wParam)
        {
            return Message.Create(IntPtr.Zero, msg, new IntPtr(wParam), IntPtr.Zero);
        }

        [Fact]
        public void ScannerWedge_FastBurst_InterceptsAndSuppressesEnter()
        {
            long mockTime = 1000;
            var listener = new ScannerWedgeListener(() => mockTime)
            {
                InterKeystrokeTimeoutMs = 50,
                MinBarcodeLength = 3
            };

            BarcodeScannedEventArgs receivedEvent = null;
            listener.BarcodeScanned += (s, e) => receivedEvent = e;

            string barcode = "8992753123456";

            // Feed characters with 10ms interval (<= 50ms)
            foreach (char c in barcode)
            {
                mockTime += 10;
                var wmChar = CreateKeyMessage(ScannerWedgeListener.WM_CHAR, c);
                bool filteredChar = listener.PreFilterMessage(ref wmChar);
                Assert.False(filteredChar); // printable characters pass through
            }

            // Feed VK_RETURN 10ms after last char
            mockTime += 10;
            var wmKeyDownEnter = CreateKeyMessage(ScannerWedgeListener.WM_KEYDOWN, ScannerWedgeListener.VK_RETURN);
            bool filteredEnter = listener.PreFilterMessage(ref wmKeyDownEnter);

            // Assertions
            Assert.True(filteredEnter); // VK_RETURN must be suppressed
            Assert.NotNull(receivedEvent);
            Assert.Equal("8992753123456", receivedEvent.RawBarcode);
            Assert.Equal("8992753123456", receivedEvent.NormalizedBarcode);
            Assert.Equal(0, listener.BufferedLength);

            // Subsequent WM_CHAR for Enter is also suppressed
            var wmCharEnter = CreateKeyMessage(ScannerWedgeListener.WM_CHAR, '\r');
            bool filteredCharEnter = listener.PreFilterMessage(ref wmCharEnter);
            Assert.True(filteredCharEnter);
        }

        [Fact]
        public void ScannerWedge_PreservesLeadingZerosInNormalizedBarcode()
        {
            long mockTime = 1000;
            var listener = new ScannerWedgeListener(() => mockTime);

            BarcodeScannedEventArgs receivedEvent = null;
            listener.BarcodeScanned += (s, e) => receivedEvent = e;

            string barcodeWithLeadingZeros = "000123987";
            foreach (char c in barcodeWithLeadingZeros)
            {
                mockTime += 5;
                var wmChar = CreateKeyMessage(ScannerWedgeListener.WM_CHAR, c);
                listener.PreFilterMessage(ref wmChar);
            }

            mockTime += 5;
            var wmKeyDown = CreateKeyMessage(ScannerWedgeListener.WM_KEYDOWN, ScannerWedgeListener.VK_RETURN);
            listener.PreFilterMessage(ref wmKeyDown);

            Assert.NotNull(receivedEvent);
            Assert.Equal("000123987", receivedEvent.NormalizedBarcode);
        }

        [Fact]
        public void ScannerWedge_SlowTyping_FlushesBufferAndDoesNotSuppressEnter()
        {
            long mockTime = 1000;
            var listener = new ScannerWedgeListener(() => mockTime)
            {
                InterKeystrokeTimeoutMs = 50
            };

            BarcodeScannedEventArgs receivedEvent = null;
            listener.BarcodeScanned += (s, e) => receivedEvent = e;

            // User types 'A'
            mockTime += 100;
            var msgA = CreateKeyMessage(ScannerWedgeListener.WM_CHAR, 'A');
            listener.PreFilterMessage(ref msgA);

            // User pauses 200ms (> 50ms) then types 'B'
            mockTime += 200;
            var msgB = CreateKeyMessage(ScannerWedgeListener.WM_CHAR, 'B');
            listener.PreFilterMessage(ref msgB);

            // Buffer was flushed on 'B' because interval > 50ms, so buffer has only "B"
            Assert.Equal("B", listener.CurrentBuffer);

            // User pauses 200ms (> 50ms) then presses Enter
            mockTime += 200;
            var msgEnter = CreateKeyMessage(ScannerWedgeListener.WM_KEYDOWN, ScannerWedgeListener.VK_RETURN);
            bool suppressed = listener.PreFilterMessage(ref msgEnter);

            // Enter should NOT be suppressed (human typing pass-through)
            Assert.False(suppressed);
            Assert.Null(receivedEvent);
            Assert.Equal(0, listener.BufferedLength);
        }

        [Fact]
        public void ScannerWedge_NavigationKey_FlushesBuffer()
        {
            long mockTime = 1000;
            var listener = new ScannerWedgeListener(() => mockTime);

            // Fast burst of 3 chars
            mockTime += 10;
            var msg1 = CreateKeyMessage(ScannerWedgeListener.WM_CHAR, '1');
            listener.PreFilterMessage(ref msg1);

            mockTime += 10;
            var msg2 = CreateKeyMessage(ScannerWedgeListener.WM_CHAR, '2');
            listener.PreFilterMessage(ref msg2);

            Assert.Equal(2, listener.BufferedLength);

            // User hits F2 (0x71)
            mockTime += 10;
            var msgF2 = CreateKeyMessage(ScannerWedgeListener.WM_KEYDOWN, 0x71);
            listener.PreFilterMessage(ref msgF2);

            Assert.Equal(0, listener.BufferedLength); // buffer flushed
        }

        [Fact]
        public void ScannerWedge_SimulateScan_FiresBarcodeScannedEvent()
        {
            var listener = new ScannerWedgeListener();
            BarcodeScannedEventArgs received = null;
            listener.BarcodeScanned += (s, e) => received = e;

            listener.SimulateScan("00987654321");

            Assert.NotNull(received);
            Assert.Equal("00987654321", received.NormalizedBarcode);
        }

        [Fact]
        public void ScannerWedge_WhenDisabled_PassesAllMessagesThrough()
        {
            long mockTime = 1000;
            var listener = new ScannerWedgeListener(() => mockTime)
            {
                IsEnabled = false
            };

            var msgEnter = CreateKeyMessage(ScannerWedgeListener.WM_KEYDOWN, ScannerWedgeListener.VK_RETURN);
            bool suppressed = listener.PreFilterMessage(ref msgEnter);

            Assert.False(suppressed);
        }
    }
}
