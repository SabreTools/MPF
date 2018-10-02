using DICUI.Data;

namespace DICUI.Utilities
{
    /// <summary>
    /// Extensions for DICCommand for easier calling
    /// </summary>
    public static class DICCommandExtensions
    {
        public static string Name(this DICCommand command)
        {
            return Converters.DICCommandToString(command);
        }
    }

    /// <summary>
    /// Extensions for DICFlag for easier calling
    /// </summary>
    public static class DICFlagExtensions
    {
        public static string Name(this DICFlag command)
        {
            return Converters.DICFlagToString(command);
        }
    }

    /// <summary>
    /// Extensions for MediaType? for easier calling
    /// </summary>
    public static class MediaTypeExtensions
    {
        public static string Name(this MediaType? type)
        {
            return Converters.MediaTypeToString(type);
        }

        public static string Extension(this MediaType? type)
        {
            return Converters.MediaTypeToExtension(type);
        }

        public static bool DoesSupportDriveSpeed(this MediaType? type)
        {
            switch (type)
            {
                case MediaType.CDROM:
                case MediaType.DVD:
                case MediaType.GDROM:
                case MediaType.HDDVD:
                case MediaType.BluRay:
                case MediaType.NintendoGameCube:
                case MediaType.NintendoWiiOpticalDisc:
                    return true;
                default:
                    return false;
            }
        }
    }

    /// <summary>
    /// Extensions for KnownSystem? for easier calling
    /// </summary>
    public static class KnownSystemExtensions
    {
        public static string Name(this KnownSystem? system)
        {
            return Converters.KnownSystemToString(system);
        }

        public static KnownSystemCategory Category(this KnownSystem? system)
        {
            if (system < KnownSystem.MarkerConsoleEnd)
                return KnownSystemCategory.Console;
            else if (system < KnownSystem.MarkerComputerEnd)
                return KnownSystemCategory.Computer;
            else if (system < KnownSystem.MarkerArcadeEnd)
                return KnownSystemCategory.Arcade;
            else if (system < KnownSystem.MarkerOtherEnd)
                return KnownSystemCategory.Other;
            else
                return KnownSystemCategory.Custom;
        }

        public static bool IsMarker(this KnownSystem? system)
        {
            switch (system)
            {
                case KnownSystem.MarkerArcadeEnd:
                case KnownSystem.MarkerComputerEnd:
                case KnownSystem.MarkerConsoleEnd:
                case KnownSystem.MarkerOtherEnd:
                    return true;
                default:
                    return false;
            }
        }
    }

    /// <summary>
    /// Extensions for KnownSystemCategory?
    /// </summary>
    public static class KnownSystemCategoryExtensions
    {
        /// <summary>
        /// Get the string representation of a KnownSystemCategory
        /// </summary>
        public static string Name(this KnownSystemCategory? category)
        {
            switch (category)
            {
                case KnownSystemCategory.Arcade:
                    return "Arcade";
                case KnownSystemCategory.Computer:
                    return "Computers";
                case KnownSystemCategory.Console:
                    return "Consoles";
                case KnownSystemCategory.Other:
                    return "Other";
                case KnownSystemCategory.Custom:
                    return "Custom";
                default:
                    return "";
            }
        }
    }
}
