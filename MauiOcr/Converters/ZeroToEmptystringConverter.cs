using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace MauiOcr.Converters
{
    public class ZeroToEmptyStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue && doubleValue == 0)
            {
                return string.Empty;
            }
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(value as string))
            {
                return 0.0;
            }
            return double.TryParse(value as string, out double result) ? result : 0.0;
        }
    }
}
