using System;
using System.Globalization;
using System.Windows.Data;
using MPF.Utilities;

namespace MPF
{
    /// <summary>
    /// Used to provide a converter to XAML files to render comboboxes with enum values
    /// </summary>
    public class EnumDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sourceType = value.GetType();
            sourceType = Nullable.GetUnderlyingType(sourceType) ?? sourceType;

            var method = typeof(Converters).GetMethod("LongName", new[] { typeof(Nullable<>).MakeGenericType(sourceType) });

            if (method != null)
                return method.Invoke(null, new[] { value });
            else
                return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }
}
