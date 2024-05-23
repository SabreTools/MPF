using System;
using System.Globalization;
using System.Windows.Data;
using MPF.Core;
using MPF.Core.UI.ComboBoxItems;
using SabreTools.RedumpLib.Data;

namespace MPF.UI.Core
{
    internal class ElementConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                DiscCategory discCategory => new Element<DiscCategory>(discCategory),
                InternalProgram internalProgram => new Element<InternalProgram>(internalProgram),
                MediaType mediaType => new Element<MediaType>(mediaType),
                RedumperReadMethod readMethod => new Element<RedumperReadMethod>(readMethod),
                RedumperSectorOrder sectorOrder => new Element<RedumperSectorOrder>(sectorOrder),
                RedumpSystem redumpSystem => new RedumpSystemComboBoxItem(redumpSystem),
                Region region => new Element<Region>(region),

                // Null values are treated as a system value
                _ => new RedumpSystemComboBoxItem((RedumpSystem?)null),
            };
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // If it's an IElement but ends up null
            if (value is not IElement element)
                return null;

            return element switch
            {
                Element<DiscCategory> dcElement => dcElement.Value,
                Element<InternalProgram> ipElement => ipElement.Value,
                Element<MediaType> mtElement => mtElement.Value,
                Element<RedumperReadMethod> rmElement => rmElement.Value,
                Element<RedumperSectorOrder> soElement => soElement.Value,
                RedumpSystemComboBoxItem rsElement => rsElement.Value,
                Element<Region> reValue => reValue.Value,
                _ => null,
            };
        }
    }
}
