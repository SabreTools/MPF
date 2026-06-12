using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace MPF.Avalonia
{
    internal sealed class ElementConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
            => Frontend.ElementConverter.ConvertEnumToElement(value);

        /// <inheritdoc/>
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => Frontend.ElementConverter.ConvertElementToEnum(value);
    }
}
