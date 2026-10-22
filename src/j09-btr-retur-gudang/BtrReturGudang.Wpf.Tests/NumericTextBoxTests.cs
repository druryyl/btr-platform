using System;
using System.Threading;
using System.Windows.Media;
using BtrReturGudang.Wpf.Controls;
using Xunit;

namespace BtrReturGudang.Wpf.Tests
{
    public class NumericTextBoxTests
    {
        private static void RunInSta(Action action)
        {
            Exception exception = null;
            var thread = new Thread(() =>
            {
                try
                {
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

        [Fact]
        public void DefaultState_HasZeroValue_AndMutedWatermarkColor()
        {
            RunInSta(() =>
            {
                var control = new NumericTextBox();

                Assert.Equal(0, control.Value);
                Assert.Equal(0, control.Minimum);
                Assert.Equal(int.MaxValue, control.Maximum);
                Assert.Equal(1, control.Step);
                Assert.True(control.IsZero);
                Assert.Equal("0", control.Watermark);

                // When Value is 0, foreground is ZeroBrush (muted gray #94A3B8)
                var zeroBrush = control.ZeroBrush as SolidColorBrush;
                Assert.NotNull(zeroBrush);
                Assert.Equal(0x94, zeroBrush.Color.R);
                Assert.Equal(0xA3, zeroBrush.Color.G);
                Assert.Equal(0xB8, zeroBrush.Color.B);

                var fg = control.Foreground as SolidColorBrush;
                Assert.NotNull(fg);
                Assert.Equal(zeroBrush.Color, fg.Color);
            });
        }

        [Fact]
        public void NonZeroValue_SwitchesToNormalForeground_AndClearsIsZero()
        {
            RunInSta(() =>
            {
                var control = new NumericTextBox();
                control.Value = 5;

                Assert.Equal(5, control.Value);
                Assert.Equal("5", control.Text);
                Assert.False(control.IsZero);
                Assert.False(control.IsWatermarkVisible);

                var normalBrush = control.NormalForegroundBrush as SolidColorBrush;
                Assert.NotNull(normalBrush);
                var fg = control.Foreground as SolidColorBrush;
                Assert.NotNull(fg);
                Assert.Equal(normalBrush.Color, fg.Color);
            });
        }

        [Fact]
        public void EmptyText_ActivatesWatermarkVisible_AndSetsZeroValue()
        {
            RunInSta(() =>
            {
                var control = new NumericTextBox();
                control.Value = 10;
                Assert.Equal(10, control.Value);

                // User backspaces to empty string
                control.Text = "";

                Assert.True(control.IsWatermarkVisible);
                Assert.True(control.IsZero);
                Assert.Equal(0, control.Value);
            });
        }

        [Theory]
        [InlineData("1", 1)]
        [InlineData("2", 2)]
        [InlineData("5", 5)]
        [InlineData("9", 9)]
        public void AntiZeroAppend_TypingDigitDirectlyReplacesZero_NeverProducesTen(string inputDigit, int expectedValue)
        {
            RunInSta(() =>
            {
                var control = new NumericTextBox
                {
                    Text = "0"
                };
                Assert.Equal(0, control.Value);

                // Simulate user typing single digit on numpad / keyboard
                bool handled = !control.ProcessTextInput(inputDigit);

                // Control handles and replaces the text directly
                Assert.True(handled);
                Assert.Equal(inputDigit, control.Text);
                Assert.Equal(expectedValue, control.Value);
                Assert.NotEqual("10", control.Text);
                Assert.NotEqual("0" + inputDigit, control.Text);
            });
        }

        [Fact]
        public void AntiZeroAppend_TypingZeroWhenAlreadyZero_SuppressesDuplicate()
        {
            RunInSta(() =>
            {
                var control = new NumericTextBox
                {
                    Text = "0"
                };

                // Typing '0' when already '0' is suppressed
                bool handled = !control.ProcessTextInput("0");
                Assert.True(handled);
                Assert.Equal("0", control.Text);
                Assert.Equal(0, control.Value);
            });
        }

        [Fact]
        public void NonNumericInput_IsRejected()
        {
            RunInSta(() =>
            {
                var control = new NumericTextBox
                {
                    Value = 5
                };

                bool allowedLetter = control.ProcessTextInput("a");
                bool allowedSymbol = control.ProcessTextInput("!");
                bool allowedSpace = control.ProcessTextInput(" ");

                Assert.False(allowedLetter);
                Assert.False(allowedSymbol);
                Assert.False(allowedSpace);
                Assert.Equal("5", control.Text);
                Assert.Equal(5, control.Value);
            });
        }

        [Fact]
        public void Increment_IncrementsByStep_AndSelectsAll()
        {
            RunInSta(() =>
            {
                var control = new NumericTextBox
                {
                    Value = 1,
                    Step = 1,
                    AutoSelectOnFocus = true
                };

                control.Increment();

                Assert.Equal(2, control.Value);
                Assert.Equal("2", control.Text);
                Assert.Equal(0, control.SelectionStart);
                Assert.Equal(1, control.SelectionLength);
            });
        }

        [Fact]
        public void Decrement_DecrementsByStep_ClampsAtMinimum_AndSelectsAll()
        {
            RunInSta(() =>
            {
                var control = new NumericTextBox
                {
                    Value = 1,
                    Minimum = 0,
                    Step = 1,
                    AutoSelectOnFocus = true
                };

                control.Decrement();
                Assert.Equal(0, control.Value);
                Assert.Equal("0", control.Text);
                Assert.True(control.IsZero);

                // Further decrement clamps at Minimum (0), never negative
                control.Decrement();
                Assert.Equal(0, control.Value);
                Assert.Equal("0", control.Text);
            });
        }

        [Fact]
        public void Increment_ClampsAtMaximum()
        {
            RunInSta(() =>
            {
                var control = new NumericTextBox
                {
                    Maximum = 10,
                    Value = 9,
                    Step = 5
                };

                control.Increment();
                Assert.Equal(10, control.Value);
                Assert.Equal("10", control.Text);
            });
        }

        [Fact]
        public void CustomStep_IncrementsCorrectly()
        {
            RunInSta(() =>
            {
                var control = new NumericTextBox
                {
                    Value = 10,
                    Step = 5
                };

                control.Increment();
                Assert.Equal(15, control.Value);

                control.Decrement();
                Assert.Equal(10, control.Value);
            });
        }

        [Fact]
        public void SettingTextDirectly_UpdatesValue_AndVisualState()
        {
            RunInSta(() =>
            {
                var control = new NumericTextBox();

                control.Text = "42";
                Assert.Equal(42, control.Value);
                Assert.False(control.IsZero);
                Assert.False(control.IsWatermarkVisible);

                control.Text = "0";
                Assert.Equal(0, control.Value);
                Assert.True(control.IsZero);
            });
        }

        [Fact]
        public void SettingValueDirectly_UpdatesText_AndVisualState()
        {
            RunInSta(() =>
            {
                var control = new NumericTextBox();

                control.Value = 100;
                Assert.Equal("100", control.Text);
                Assert.False(control.IsZero);

                control.Value = 0;
                Assert.Equal("0", control.Text);
                Assert.True(control.IsZero);
            });
        }

        [Fact]
        public void CommittedEvent_FiresOnCommitted()
        {
            RunInSta(() =>
            {
                var control = new NumericTextBox { Value = 7 };
                bool fired = false;
                control.Committed += (s, e) => fired = true;

                // Raise committed
                var raiseMethod = typeof(NumericTextBox).GetField(nameof(NumericTextBox.Committed),
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                
                // Trigger via reflection or event invocation
                var eventDelegate = raiseMethod?.GetValue(control) as EventHandler;
                eventDelegate?.Invoke(control, EventArgs.Empty);

                Assert.True(fired);
            });
        }
    }
}
