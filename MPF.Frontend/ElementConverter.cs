using MPF.Frontend.ComboBoxItems;
using SabreTools.RedumpLib.Data;
using DreamdumpSectorOrder = MPF.ExecutionContexts.Dreamdump.SectorOrder;
using LogCompression = MPF.Processors.LogCompression;
using RedumperDriveType = MPF.ExecutionContexts.Redumper.DriveType;
using RedumperReadMethod = MPF.ExecutionContexts.Redumper.ReadMethod;
using RedumperSectorOrder = MPF.ExecutionContexts.Redumper.SectorOrder;

namespace MPF.Frontend
{
    /// <summary>
    /// Convert Element types to and from their corresponding enums
    /// </summary>
    public static class ElementConverter
    {
        /// <summary>
        /// Convert from an enum value to an Element type
        /// </summary>
        public static object ConvertEnumToElement(object? value)
        {
            return value switch
            {
                DiscCategory discCategory => new Element<DiscCategory>(discCategory),
                DreamdumpSectorOrder sectorOrder => new Element<DreamdumpSectorOrder>(sectorOrder),
                InterfaceLanguage interfaceLanguage => new Element<InterfaceLanguage>(interfaceLanguage),
                InternalProgram internalProgram => new Element<InternalProgram>(internalProgram),
                LogCompression logCompression => new Element<LogCompression>(logCompression),
                PhysicalMediaType mediaType => new Element<PhysicalMediaType>(mediaType),
                RedumperReadMethod readMethod => new Element<RedumperReadMethod>(readMethod),
                RedumperSectorOrder sectorOrder => new Element<RedumperSectorOrder>(sectorOrder),
                RedumperDriveType driveType => new Element<RedumperDriveType>(driveType),
                PhysicalSystem redumpSystem => new PhysicalSystemComboBoxItem(redumpSystem),
                Region region => new Element<Region>(region),

                // Null values are treated as a system value
                _ => new PhysicalSystemComboBoxItem((PhysicalSystem?)null),
            };
        }

        /// <summary>
        /// Convert an Element type to an enum value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static object? ConvertElementToEnum(object? value)
        {
            // If it's an IElement but ends up null
            if (value is not IElement element)
                return null;

            return element switch
            {
                Element<DiscCategory> dcElement => dcElement.Value,
                Element<DreamdumpSectorOrder> soElement => soElement.Value,
                Element<InterfaceLanguage> ilElement => ilElement.Value,
                Element<InternalProgram> ipElement => ipElement.Value,
                Element<LogCompression> lcElement => lcElement.Value,
                Element<PhysicalMediaType> mtElement => mtElement.Value,
                Element<RedumperReadMethod> rmElement => rmElement.Value,
                Element<RedumperSectorOrder> soElement => soElement.Value,
                Element<RedumperDriveType> dtElement => dtElement.Value,
                PhysicalSystemComboBoxItem rsElement => rsElement.Value,
                Element<Region> reValue => reValue.Value,
                _ => null,
            };
        }
    }
}
