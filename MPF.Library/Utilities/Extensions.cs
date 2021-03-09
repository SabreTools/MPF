using MPF.Data;

namespace MPF.Utilities
{
    public static class Extensions
    {
        /// <summary>
        /// Determine the category based on the system
        /// </summary>
        /// <param name="system">KnownSystem value to check</param>
        /// <returns>KnownSystemCategory related to the system</returns>
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

        /// <summary>
        /// Determine if the media supports drive speeds
        /// </summary>
        /// <param name="type">MediaType value to check</param>
        /// <returns>True if the media has variable dumping speeds, false otherwise</returns>
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

        /// <summary>
        /// Determine if a system is considered audio-only
        /// </summary>
        /// <param name="system">KnownSystem value to check</param>
        /// <returns>True if the system is audio-only, false otherwise</returns>
        public static bool IsAudio(this KnownSystem? system)
        {
            switch (system)
            {
                case KnownSystem.AtariJaguarCD:
                case KnownSystem.AudioCD:
                case KnownSystem.DVDAudio:
                case KnownSystem.HasbroVideoNow:
                case KnownSystem.HasbroVideoNowColor:
                case KnownSystem.HasbroVideoNowJr:
                case KnownSystem.HasbroVideoNowXP:
                case KnownSystem.SuperAudioCD:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Determine if a system is a marker value
        /// </summary>
        /// <param name="system">KnownSystem value to check</param>
        /// <returns>True if the system is a marker value, false otherwise</returns>
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
