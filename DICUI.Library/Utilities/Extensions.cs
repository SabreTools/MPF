using DICUI.Data;

namespace DICUI.Utilities
{
    /// <summary>
    /// Extensions for Category?
    /// </summary>
    public static class CategoryExtensions
    {
        public static string Name(this Category? category)
        {
            return Converters.LongName(category);
        }
    }

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
            return Converters.LongName(type);
        }

        public static string ShortName(this MediaType? type)
        {
            return Converters.ShortName(type);
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
                case MediaType.NintendoGameCubeGameDisc:
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
            return Converters.LongName(system);
        }

        public static string ShortName(this KnownSystem? system)
        {
            return Converters.ShortName(system);
        }

        public static KnownSystemCategory Category(this KnownSystem? system)
        {
            if (system < KnownSystem.MarkerDiscBasedConsoleEnd)
                return KnownSystemCategory.DiscBasedConsole;
            /*
            else if (system < KnownSystem.MarkerOtherConsoleEnd)
                return KnownSystemCategory.OtherConsole;
            */
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
                case KnownSystem.MarkerDiscBasedConsoleEnd:
                // case KnownSystem.MarkerOtherConsoleEnd:
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
                case KnownSystemCategory.DiscBasedConsole:
                    return "Disc-Based Consoles";
                case KnownSystemCategory.OtherConsole:
                    return "Other Consoles";
                case KnownSystemCategory.Other:
                    return "Other";
                case KnownSystemCategory.Custom:
                    return "Custom";
                default:
                    return "";
            }
        }
    }

    /// <summary>
    /// Extensions for Language?
    /// </summary>
    public static class LanguageExtensions
    {
        public static string Name(this Language? lang)
        {
            return Converters.LongName(lang);
        }

        public static string ShortName(this Language? lang)
        {
            return Converters.ShortName(lang);
        }

    }

    /// <summary>
    /// Extensions for Region?
    /// </summary>
    public static class RegionExtensions
    {
        public static string Name(this Region? region)
        {
            return Converters.LongName(region);
        }

        public static string ShortName(this Region? region)
        {
            return Converters.ShortName(region);
        }

    }
    
    /// <summary>
    /// Extensions for YesNo
    /// </summary>
    public static class YesNoExtensions
    {
        public static string Name(this YesNo yesno)
        {
            return Converters.LongName(yesno);
        }
    }
}
