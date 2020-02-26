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
            // Common
            if (value is MediaType?)
                return ((MediaType?)value).LongName();
            else if (value is KnownSystem?)
                return ((KnownSystem?)value).LongName();

            // DiscImageChef
            else if (value is DiscImageChef.Command)
                return ((DiscImageChef.Command)value).LongName();
            else if (value is DiscImageChef.Flag)
                return ((DiscImageChef.Flag)value).LongName();

            // DiscImageCreator
            else if (value is DiscImageCreator.Command)
                return ((DiscImageCreator.Command)value).LongName();
            else if (value is DiscImageCreator.Flag)
                return ((DiscImageCreator.Flag)value).LongName();

            // Default
            else
                return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }
}
