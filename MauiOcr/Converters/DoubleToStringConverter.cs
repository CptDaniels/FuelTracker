using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace MauiOcr.Converters
{
    public class DoubleToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                return doubleValue.ToString(CultureInfo.InvariantCulture);
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                string[] parts = stringValue.Split('.', ',');

                if (parts.Length == 1)
                {
                    if (double.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
                    {
                        return result;
                    }
                }
                else if (parts.Length == 2 && int.TryParse(parts[0], out int integerPart) && int.TryParse(parts[1], out int fractionalPart))
                {
                    string cleanedValue = string.Join(".", integerPart, fractionalPart);

                    if (double.TryParse(cleanedValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
                    {
                        return result;
                    }
                }
            }
            return 0.0;
        }
    }
}