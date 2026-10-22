using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BtrReturGudang.Wpf.Controls
{
    /// <summary>
    /// Custom WPF numeric input control tailored for high-load warehouse intake operations.
    /// Eliminates sticky zero bugs, auto-selects all on focus, and provides tactile numpad ergonomics.
    /// </summary>
    public class NumericTextBox : TextBox
    {
        private bool _isUpdatingInternally;

        static NumericTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(NumericTextBox),
                new FrameworkPropertyMetadata(typeof(NumericTextBox)));
        }

        public NumericTextBox()
        {
            DataObject.AddPastingHandler(this, OnPasting);
            UpdateZeroAndWatermarkState();
        }

        #region Dependency Properties

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(int),
                typeof(NumericTextBox),
                new FrameworkPropertyMetadata(
                    0,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnValueChanged,
                    CoerceValue));

        public int Value
        {
            get => (int)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(
                nameof(Minimum),
                typeof(int),
                typeof(NumericTextBox),
                new FrameworkPropertyMetadata(0, OnMinMaxChanged));

        public int Minimum
        {
            get => (int)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(
                nameof(Maximum),
                typeof(int),
                typeof(NumericTextBox),
                new FrameworkPropertyMetadata(int.MaxValue, OnMinMaxChanged));

        public int Maximum
        {
            get => (int)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public static readonly DependencyProperty StepProperty =
            DependencyProperty.Register(
                nameof(Step),
                typeof(int),
                typeof(NumericTextBox),
                new FrameworkPropertyMetadata(1));

        public int Step
        {
            get => (int)GetValue(StepProperty);
            set => SetValue(StepProperty, value);
        }

        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.Register(
                nameof(Watermark),
                typeof(string),
                typeof(NumericTextBox),
                new FrameworkPropertyMetadata("0"));

        public string Watermark
        {
            get => (string)GetValue(WatermarkProperty);
            set => SetValue(WatermarkProperty, value);
        }

        public static readonly DependencyProperty AutoSelectOnFocusProperty =
            DependencyProperty.Register(
                nameof(AutoSelectOnFocus),
                typeof(bool),
                typeof(NumericTextBox),
                new FrameworkPropertyMetadata(true));

        public bool AutoSelectOnFocus
        {
            get => (bool)GetValue(AutoSelectOnFocusProperty);
            set => SetValue(AutoSelectOnFocusProperty, value);
        }

        private static readonly DependencyPropertyKey IsZeroPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(IsZero),
                typeof(bool),
                typeof(NumericTextBox),
                new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty IsZeroProperty =
            IsZeroPropertyKey.DependencyProperty;

        public bool IsZero => (bool)GetValue(IsZeroProperty);

        private static readonly DependencyPropertyKey IsWatermarkVisiblePropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(IsWatermarkVisible),
                typeof(bool),
                typeof(NumericTextBox),
                new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty IsWatermarkVisibleProperty =
            IsWatermarkVisiblePropertyKey.DependencyProperty;

        public bool IsWatermarkVisible => (bool)GetValue(IsWatermarkVisibleProperty);

        public static readonly DependencyProperty WatermarkBrushProperty =
            DependencyProperty.Register(
                nameof(WatermarkBrush),
                typeof(Brush),
                typeof(NumericTextBox),
                new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(0x94, 0xA3, 0xB8)))); // Slate-400

        public Brush WatermarkBrush
        {
            get => (Brush)GetValue(WatermarkBrushProperty);
            set => SetValue(WatermarkBrushProperty, value);
        }

        public static readonly DependencyProperty ZeroBrushProperty =
            DependencyProperty.Register(
                nameof(ZeroBrush),
                typeof(Brush),
                typeof(NumericTextBox),
                new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(0x94, 0xA3, 0xB8)))); // Slate-400

        public Brush ZeroBrush
        {
            get => (Brush)GetValue(ZeroBrushProperty);
            set => SetValue(ZeroBrushProperty, value);
        }

        public static readonly DependencyProperty NormalForegroundBrushProperty =
            DependencyProperty.Register(
                nameof(NormalForegroundBrush),
                typeof(Brush),
                typeof(NumericTextBox),
                new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(0x0F, 0x17, 0x2A)))); // Slate-900

        public Brush NormalForegroundBrush
        {
            get => (Brush)GetValue(NormalForegroundBrushProperty);
            set => SetValue(NormalForegroundBrushProperty, value);
        }

        #endregion

        #region Events

        public event EventHandler Committed;

        #endregion

        #region Callbacks & Overrides

        private static object CoerceValue(DependencyObject d, object baseValue)
        {
            var control = (NumericTextBox)d;
            int val = (int)baseValue;
            if (val < control.Minimum) return control.Minimum;
            if (val > control.Maximum) return control.Maximum;
            return val;
        }

        private static void OnMinMaxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.CoerceValue(ValueProperty);
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (NumericTextBox)d;
            control.OnValueChangedInternal((int)e.NewValue);
        }

        private void OnValueChangedInternal(int newValue)
        {
            if (_isUpdatingInternally) return;

            _isUpdatingInternally = true;
            try
            {
                Text = newValue.ToString();
                UpdateZeroAndWatermarkState();
                if (AutoSelectOnFocus && IsKeyboardFocusWithin)
                {
                    SelectAll();
                }
            }
            finally
            {
                _isUpdatingInternally = false;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            UpdateZeroAndWatermarkState();
        }

        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);
            if (AutoSelectOnFocus)
            {
                SelectAll();
            }
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (AutoSelectOnFocus && !IsKeyboardFocusWithin)
            {
                Focus();
                e.Handled = true;
            }
            base.OnPreviewMouseLeftButtonDown(e);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Up)
            {
                Increment();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Down)
            {
                Decrement();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Add || (e.Key == Key.OemPlus))
            {
                Increment();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
            {
                Decrement();
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Enter)
            {
                Committed?.Invoke(this, EventArgs.Empty);
                // Do not mark handled so Enter propagates to default button / command
            }

            base.OnPreviewKeyDown(e);
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            if (!ProcessTextInput(e.Text))
            {
                e.Handled = true;
                return;
            }

            base.OnPreviewTextInput(e);
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);

            if (_isUpdatingInternally) return;

            _isUpdatingInternally = true;
            try
            {
                if (string.IsNullOrEmpty(Text))
                {
                    Value = Minimum;
                }
                else if (int.TryParse(Text, out int parsed))
                {
                    int clamped = Math.Max(Minimum, Math.Min(Maximum, parsed));
                    if (clamped != parsed || (parsed != 0 && Text.StartsWith("0")))
                    {
                        Text = clamped.ToString();
                        CaretIndex = Text.Length;
                    }
                    Value = clamped;
                }
                else
                {
                    Text = Value.ToString();
                }

                UpdateZeroAndWatermarkState();
            }
            finally
            {
                _isUpdatingInternally = false;
            }
        }

        #endregion

        #region Input & Processing Methods

        /// <summary>
        /// Increments the numeric value by Step, clamped to Maximum, and selects all text.
        /// </summary>
        public void Increment()
        {
            int current = Value;
            int next = current + Step;
            if (next > Maximum) next = Maximum;
            Value = next;
            Text = Value.ToString();
            if (AutoSelectOnFocus)
            {
                SelectAll();
            }
        }

        /// <summary>
        /// Decrements the numeric value by Step, clamped to Minimum, and selects all text.
        /// </summary>
        public void Decrement()
        {
            int current = Value;
            int next = current - Step;
            if (next < Minimum) next = Minimum;
            Value = next;
            Text = Value.ToString();
            if (AutoSelectOnFocus)
            {
                SelectAll();
            }
        }

        /// <summary>
        /// Processes single or multi-character text input, enforcing numeric-only input and Anti-Zero replacement.
        /// Returns true if default WPF insertion should proceed; false if handled or rejected.
        /// </summary>
        public bool ProcessTextInput(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;

            // Reject non-numeric input
            if (!IsNumericOnly(input)) return false;

            // Anti-Zero Append: typing "1".."9" when current text is "0" directly replaces "0"
            if (Text == "0")
            {
                if (input == "0")
                {
                    // Typing "0" when already "0" maintains "0"
                    return false;
                }

                _isUpdatingInternally = true;
                try
                {
                    Text = input;
                    CaretIndex = Text.Length;
                    if (int.TryParse(Text, out int parsed))
                    {
                        Value = Math.Max(Minimum, Math.Min(Maximum, parsed));
                    }
                    UpdateZeroAndWatermarkState();
                }
                finally
                {
                    _isUpdatingInternally = false;
                }
                return false;
            }

            return true;
        }

        private void OnPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(DataFormats.UnicodeText, true))
            {
                string text = e.DataObject.GetData(DataFormats.UnicodeText, true) as string;
                if (!string.IsNullOrEmpty(text))
                {
                    string digits = FilterDigits(text);
                    if (string.IsNullOrEmpty(digits))
                    {
                        e.CancelCommand();
                        return;
                    }

                    if (Text == "0")
                    {
                        string trimmed = digits.TrimStart('0');
                        Text = string.IsNullOrEmpty(trimmed) ? "0" : trimmed;
                        CaretIndex = Text.Length;
                        e.CancelCommand();
                        return;
                    }

                    var newObj = new DataObject();
                    newObj.SetData(DataFormats.UnicodeText, digits);
                    e.DataObject = newObj;
                    return;
                }
            }
            e.CancelCommand();
        }

        public void UpdateZeroAndWatermarkState()
        {
            bool isZero = Value == 0;
            bool isWatermarkVisible = string.IsNullOrEmpty(Text);

            SetValue(IsZeroPropertyKey, isZero);
            SetValue(IsWatermarkVisiblePropertyKey, isWatermarkVisible);

            Foreground = isZero ? ZeroBrush : NormalForegroundBrush;
        }

        private static bool IsNumericOnly(string text)
        {
            foreach (char c in text)
            {
                if (!char.IsDigit(c)) return false;
            }
            return true;
        }

        private static string FilterDigits(string text)
        {
            var sb = new System.Text.StringBuilder();
            foreach (char c in text)
            {
                if (char.IsDigit(c)) sb.Append(c);
            }
            return sb.ToString();
        }

        #endregion
    }
}
