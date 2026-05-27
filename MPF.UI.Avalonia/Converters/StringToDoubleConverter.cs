// MPF cross-platform (Avalonia) UI — contributed by Knutwurst (https://github.com/knutwurst)
using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace MPF.UI.Avalonia.Converters
{
    /// <summary>
    /// Converts a numeric string (e.g. "22") to a <see cref="double"/> for use in Height bindings.
    /// Used by <see cref="Controls.UserInput"/> to bridge the string-typed TextHeight styled property
    /// to the Avalonia TextBox's double Height property.
    /// </summary>
    public class StringToDoubleConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string s && double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
                return result;

            return global::Avalonia.AvaloniaProperty.UnsetValue;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double d)
                return d.ToString(CultureInfo.InvariantCulture);

            return global::Avalonia.AvaloniaProperty.UnsetValue;
        }
    }
}
