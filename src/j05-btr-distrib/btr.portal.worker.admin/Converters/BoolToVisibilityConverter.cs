using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace btr.portal.worker.admin.Converters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var boolValue = value is bool b && b;
            var invert = parameter?.ToString()?.ToLowerInvariant() == "invert";
            if (invert) boolValue = !boolValue;
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
