using DICUI.Data;

namespace DICUI.Utilities
{
    /// <summary>
    /// Extensions for MediaType? for easier calling
    /// </summary>
    public static class MediaTypeExtensions
    {
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
}
