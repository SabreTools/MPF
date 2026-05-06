using System;
using System.Globalization;
using Avalonia.Data.Converters;
using MPF.Frontend;
using MPF.Frontend.ComboBoxItems;
using SabreTools.RedumpLib.Data;
using LogCompression = MPF.Processors.LogCompression;
using RedumperDriveType = MPF.ExecutionContexts.Redumper.DriveType;
using RedumperReadMethod = MPF.ExecutionContexts.Redumper.ReadMethod;
using RedumperSectorOrder = MPF.ExecutionContexts.Redumper.SectorOrder;

namespace MPF.Avalonia
{
    internal sealed class ElementConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value switch
            {
                DiscCategory discCategory => new Element<DiscCategory>(discCategory),
                InterfaceLanguage interfaceLanguage => new Element<InterfaceLanguage>(interfaceLanguage),
                InternalProgram internalProgram => new Element<InternalProgram>(internalProgram),
                LogCompression logCompression => new Element<LogCompression>(logCompression),
                MediaType mediaType => new Element<MediaType>(mediaType),
                RedumperReadMethod readMethod => new Element<RedumperReadMethod>(readMethod),
                RedumperSectorOrder sectorOrder => new Element<RedumperSectorOrder>(sectorOrder),
                RedumperDriveType driveType => new Element<RedumperDriveType>(driveType),
                RedumpSystem redumpSystem => new RedumpSystemComboBoxItem(redumpSystem),
                Region region => new Element<Region>(region),
                _ => new RedumpSystemComboBoxItem((RedumpSystem?)null),
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not IElement element)
                return null;

            return element switch
            {
                Element<DiscCategory> dcElement => dcElement.Value,
                Element<InterfaceLanguage> ilElement => ilElement.Value,
                Element<InternalProgram> ipElement => ipElement.Value,
                Element<LogCompression> lcElement => lcElement.Value,
                Element<MediaType> mtElement => mtElement.Value,
                Element<RedumperReadMethod> rmElement => rmElement.Value,
                Element<RedumperSectorOrder> soElement => soElement.Value,
                Element<RedumperDriveType> dtElement => dtElement.Value,
                RedumpSystemComboBoxItem rsElement => rsElement.Value,
                Element<Region> reElement => reElement.Value,
                _ => null,
            };
        }
    }
}
