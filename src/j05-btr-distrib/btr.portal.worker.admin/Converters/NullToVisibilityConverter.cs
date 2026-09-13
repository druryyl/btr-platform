using System;
using System.Globalization;
using System.Windows.Data;

namespace btr.portal.worker.admin.Converters
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var invert = parameter?.ToString()?.ToLowerInvariant() == "invert";
            var isNull = value == null;
            var visible = invert ? !isNull : isNull;
            return visible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
