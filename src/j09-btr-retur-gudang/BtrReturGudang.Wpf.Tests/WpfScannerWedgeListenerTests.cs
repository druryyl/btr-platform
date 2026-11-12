using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using BtrReturGudang.Wpf.Services;
using Xunit;

namespace BtrReturGudang.Wpf.Tests
{
    public class WpfScannerWedgeListenerTests
    {
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr SetActiveWindow(IntPtr hWnd);
        private static readonly object _staLock = new object();

        private static void RunInSta(Action action)
        {
            Exception exception = null;
            var thread = new Thread(() =>
            {
                try
                {
                    lock (_staLock)
                    {
                        if (System.Windows.Application.Current == null)
                        {
                            try
                            {
                                new System.Windows.Application();
                            }
                            catch (InvalidOperationException)
                            {
                            }
                        }
                    }
                    action();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            if (exception != null)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception).Throw();
            }
        }

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
            for (int i = 0; i < barcode.Length; i++)
            {
                mockTime += 10;
                var wmChar = CreateKeyMessage(WpfScannerWedgeListener.WM_CHAR, barcode[i]);
                bool handledChar = listener.ProcessMessage(ref wmChar);
                if (i == 0)
                {
                    Assert.False(handledChar); // Char 1 passes through
                }
                else
                {
                    Assert.True(handledChar);  // Chars 2..N are suppressed during burst
                }
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
        public void ScannerWedge_FastBurst_WhenTextBoxFocused_PerformsUndoAndClearsFocus()
        {
            RunInSta(() =>
            {
                var window = new Window
                {
                    Width = 300,
                    Height = 200
                };
                var tb = new TextBox
                {
                    IsUndoEnabled = true,
                    Text = "ABC"
                };
                window.Content = tb;
                window.Show();
                window.Activate();

                try
                {
                    tb.Focus();
                    Keyboard.Focus(tb);
                    if (Keyboard.FocusedElement != tb)
                    {
                        // In non-interactive or background test runners, force WPF input manager activation
                        var hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
                        SetForegroundWindow(hwnd);
                        SetActiveWindow(hwnd);
                        Keyboard.Focus(tb);
                    }

                    // Simulate character 1 typing into textbox creating an undo action
                    tb.CaretIndex = tb.Text.Length;
                    tb.SelectedText = "8";
                    Assert.Equal("ABC8", tb.Text);
                    Assert.True(tb.CanUndo);

                    long mockTime = 1000;
                    var listener = new WpfScannerWedgeListener(() => mockTime)
                    {
                        InterKeystrokeTimeoutMs = 50,
                        MinBarcodeLength = 3
                    };

                    // Feed fast burst of barcode characters starting with '8'
                    string barcode = "8992753123456";
                    for (int i = 0; i < barcode.Length; i++)
                    {
                        mockTime += 10;
                        var wmChar = CreateKeyMessage(WpfScannerWedgeListener.WM_CHAR, barcode[i]);
                        listener.ProcessMessage(ref wmChar);
                    }

                    // Feed VK_RETURN burst completion
                    mockTime += 10;
                    var wmKeyDown = CreateKeyMessage(WpfScannerWedgeListener.WM_KEYDOWN, WpfScannerWedgeListener.VK_RETURN);
                    bool handled = listener.ProcessMessage(ref wmKeyDown);

                    Assert.True(handled);
                    Assert.Equal("ABC", tb.Text); // Rolled back via tb.Undo()
                    Assert.Null(Keyboard.FocusedElement); // Focus cleared via Keyboard.ClearFocus()
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void ScannerWedge_SlowTyping_DoesNotSuppressCharactersOrEnter()
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
            listener.BarcodeScanned += b => receivedBarcode = b;

            string slowInput = "12345";
            foreach (char c in slowInput)
            {
                mockTime += 100; // > 50ms interval (human typing)
                var wmChar = CreateKeyMessage(WpfScannerWedgeListener.WM_CHAR, c);
                bool handledChar = listener.ProcessMessage(ref wmChar);
                Assert.False(handledChar); // Characters must not be suppressed
            }

            mockTime += 100; // > 50ms interval
            var wmKeyDownEnter = CreateKeyMessage(WpfScannerWedgeListener.WM_KEYDOWN, WpfScannerWedgeListener.VK_RETURN);
            bool handledEnter = listener.ProcessMessage(ref wmKeyDownEnter);
            Assert.False(handledEnter); // Enter must not be suppressed

            Assert.Null(receivedBarcode);
            Assert.Equal(0, mockConsumer.ScanCount);
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
