using System;
using System.Globalization;
using System.Windows.Data;
using DICUI.Data;
using DICUI.Utilities;

namespace DICUI
{
    /// <summary>
    /// Used to provide a converter to XAML files to render comboboxes with enum values
    /// </summary>
    public class EnumDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DICCommand)
                return ((DICCommand)value).LongName();
            else if (value is DICFlag)
                return ((DICFlag)value).LongName();
            else if (value is MediaType?)
                return ((MediaType?)value).LongName();
            else if (value is KnownSystem?)
                return ((KnownSystem?)value).LongName();
            else
                return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }
}
