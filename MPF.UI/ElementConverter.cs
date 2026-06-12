using System;
using System.Globalization;
using System.Windows.Data;

namespace MPF.UI
{
    internal class ElementConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => Frontend.ElementConverter.ConvertEnumToElement(value);

        /// <inheritdoc/>
        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => Frontend.ElementConverter.ConvertElementToEnum(value);
    }
}
