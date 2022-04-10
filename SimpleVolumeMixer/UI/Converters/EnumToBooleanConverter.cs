using System;
using System.Globalization;
using System.Windows.Data;

namespace SimpleVolumeMixer.UI.Converters;

public class EnumToBooleanConverter : IValueConverter
{
    public Type? EnumType { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is string enumString)
        {
            var enumType = EnumType ?? throw new InvalidOperationException();
            if (Enum.IsDefined(enumType, value))
            {
                var enumValue = Enum.Parse(EnumType, enumString);
                return enumValue.Equals(value);
            }
        }

        return false;
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is string enumString)
        {
            var enumType = EnumType ?? throw new InvalidOperationException();
            return Enum.Parse(enumType, enumString);
        }

        return null;
    }
}