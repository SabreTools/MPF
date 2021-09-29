using RedumpLib.Data;

namespace MPF.Core.Utilities
{
    public static class EnumExtensions
    {
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
        /// <param name="system">RedumpSystem value to check</param>
        /// <returns>True if the system is audio-only, false otherwise</returns>
        /// <remarks>
        /// Philips CD-i should NOT be in this list. It's being included until there's a
        /// reasonable distinction between CD-i and CD-i ready on the database side.
        /// </remarks>
        public static bool IsAudio(this RedumpSystem? system)
        {
            switch (system)
            {
                case RedumpSystem.AtariJaguarCDInteractiveMultimediaSystem:
                case RedumpSystem.AudioCD:
                case RedumpSystem.DVDAudio:
                case RedumpSystem.HasbroVideoNow:
                case RedumpSystem.HasbroVideoNowColor:
                case RedumpSystem.HasbroVideoNowJr:
                case RedumpSystem.HasbroVideoNowXP:
                case RedumpSystem.PhilipsCDi:
                case RedumpSystem.SuperAudioCD:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Determine if a system is a marker value
        /// </summary>
        /// <param name="system">RedumpSystem value to check</param>
        /// <returns>True if the system is a marker value, false otherwise</returns>
        public static bool IsMarker(this RedumpSystem? system)
        {
            switch (system)
            {
                case RedumpSystem.MarkerArcadeEnd:
                case RedumpSystem.MarkerComputerEnd:
                case RedumpSystem.MarkerDiscBasedConsoleEnd:
                // case RedumpSystem.MarkerOtherConsoleEnd:
                case RedumpSystem.MarkerOtherEnd:
                    return true;
                default:
                    return false;
            }
        }
       
        /// <summary>
        /// Determine if a system is considered XGD
        /// </summary>
        /// <param name="system">RedumpSystem value to check</param>
        /// <returns>True if the system is XGD, false otherwise</returns>
        public static bool IsXGD(this RedumpSystem? system)
        {
            switch (system)
            {
                case RedumpSystem.MicrosoftXbox:
                case RedumpSystem.MicrosoftXbox360:
                case RedumpSystem.MicrosoftXboxOne:
                case RedumpSystem.MicrosoftXboxSeriesXS:
                    return true;
                default:
                    return false;
            }
        }
    }
}
