using System;
using System.Globalization;
using System.Windows.Data;
using MPF.Core.Data;
using MPF.Core.UI.ComboBoxItems;
using SabreTools.RedumpLib.Data;

namespace MPF.UI.Core
{
    internal class ElementConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case DiscCategory discCategory:
                    return new Element<DiscCategory>(discCategory);
                case InternalProgram internalProgram:
                    return new Element<InternalProgram>(internalProgram);
                case MediaType mediaType:
                    return new Element<MediaType>(mediaType);
                case RedumpSystem redumpSystem:
                    return new RedumpSystemComboBoxItem(redumpSystem);
                case Region region:
                    return new Element<Region>(region);

                // Null values are treated as a system value
                default:
                    return new RedumpSystemComboBoxItem((RedumpSystem?)null);
            }
        }

#if NET48
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
#else
        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
#endif
        {
            // If it's an IElement but ends up null
#if NET48
            if (!(value is IElement element))
#else
            if (value is not IElement element)
#endif
                return null;

            switch (element)
            {
                case Element<DiscCategory> dcElement:
                    return dcElement.Value;
                case Element<InternalProgram> ipElement:
                    return ipElement.Value;
                case Element<MediaType> mtElement:
                    return mtElement.Value;
                case RedumpSystemComboBoxItem rsElement:
                    return rsElement.Value;
                case Element<Region> reValue:
                    return reValue.Value;

                default: return null;
            }
        }
    }
}
