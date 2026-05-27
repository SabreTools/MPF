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

namespace MPF.UI.Avalonia.Converters
{
    /// <summary>
    /// Avalonia port of MPF.UI.ElementConverter.
    /// Converts enum/value-type model values to their ComboBox item wrapper and back.
    /// NOTE: BooleanToVisibilityConverter was intentionally skipped — Avalonia uses
    /// the bool IsVisible property directly, so no converter is needed.
    /// </summary>
    public class ElementConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
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

                // Null values are treated as a system value
                _ => new RedumpSystemComboBoxItem((RedumpSystem?)null),
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // If it's an IElement but ends up null
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
                Element<Region> reValue => reValue.Value,
                _ => null,
            };
        }
    }
}
