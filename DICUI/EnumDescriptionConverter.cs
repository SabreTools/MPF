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
                return DiscImageChef.Converters.LongName((DiscImageChef.Command)value);
            else if (value is DiscImageChef.Flag)
                return DiscImageChef.Converters.LongName((DiscImageChef.Flag)value);

            // DiscImageCreator
            else if (value is DiscImageCreator.Command)
                return DiscImageCreator.Converters.LongName((DiscImageCreator.Command)value);
            else if (value is DiscImageCreator.Flag)
                return DiscImageCreator.Converters.LongName((DiscImageCreator.Flag)value);

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
