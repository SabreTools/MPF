using System;
using System.Globalization;
using System.Windows.Data;
using DICUI.Data;
using DICUI.Utilities;

namespace DICUI.Forms
{
    /// <summary>
    /// Used to provide a converter to XAML files to render comboboxes with enum values
    /// </summary>
    public class EnumDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DICCommand)
                return ((DICCommand)value).Name();
            else if (value is DICFlag)
                return ((DICFlag)value).Name();
            else if (value is MediaType?)
                return ((MediaType?)value).Name();
            else if (value is KnownSystem?)
                return ((KnownSystem?)value).Name();
            else
                return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }
}
