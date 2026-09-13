using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace btr.portal.worker.admin.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var status = value?.ToString()?.ToLowerInvariant();
            return status switch
            {
                "ok" or "success" => new SolidColorBrush(Color.FromRgb(16, 185, 129)),
                "degraded" or "warning" => new SolidColorBrush(Color.FromRgb(245, 158, 11)),
                "refreshing" or "inprogress" => new SolidColorBrush(Color.FromRgb(59, 130, 246)),
                "failed" or "error" => new SolidColorBrush(Color.FromRgb(239, 68, 68)),
                _ => new SolidColorBrush(Color.FromRgb(107, 114, 128))
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
