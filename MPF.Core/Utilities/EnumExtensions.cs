using System;
using System.Collections.Generic;
using MPF.Core.Converters;
using MPF.Core.Data;
using RedumpLib.Data;

namespace MPF.Core.Utilities
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Determine if a system is okay if it's not detected by Windows
        /// </summary>
        /// <param name="system">RedumpSystem value to check</param>
        /// <returns>True if Windows show see a disc when dumping, false otherwise</returns>
        public static bool DetectedByWindows(this RedumpSystem? system)
        {
            switch (system)
            {
                case RedumpSystem.AmericanLaserGames3DO:
                case RedumpSystem.AppleMacintosh:
                case RedumpSystem.Atari3DO:
                case RedumpSystem.AtariJaguarCDInteractiveMultimediaSystem:
                case RedumpSystem.NewJatreCDi:
                case RedumpSystem.NintendoGameCube:
                case RedumpSystem.NintendoWii:
                case RedumpSystem.NintendoWiiU:
                case RedumpSystem.PhilipsCDi:
                case RedumpSystem.PhilipsCDiDigitalVideo:
                case RedumpSystem.Panasonic3DOInteractiveMultiplayer:
                case RedumpSystem.PanasonicM2:
                case RedumpSystem.PioneerLaserActive:
                case RedumpSystem.SuperAudioCD:
                    return false;
                default:
                    return true;
            }
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
        /// Determine if a system has reversed ringcodes
        /// </summary>
        /// <param name="system">RedumpSystem value to check</param>
        /// <returns>True if the system has reversed ringcodes, false otherwise</returns>
        public static bool HasReversedRingcodes(this RedumpSystem? system)
        {
            switch (system)
            {
                case RedumpSystem.SonyPlayStation2:
                case RedumpSystem.SonyPlayStation3:
                case RedumpSystem.SonyPlayStation4:
                //case RedumpSystem.SonyPlayStation5:
                case RedumpSystem.SonyPlayStationPortable:
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
                case RedumpSystem.HasbroiONEducationalGamingSystem:
                case RedumpSystem.HasbroVideoNow:
                case RedumpSystem.HasbroVideoNowColor:
                case RedumpSystem.HasbroVideoNowJr:
                case RedumpSystem.HasbroVideoNowXP:
                case RedumpSystem.PhilipsCDi:
                case RedumpSystem.PlayStationGameSharkUpdates:
                case RedumpSystem.SuperAudioCD:
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

        /// <summary>
        /// List all programs with their short usable names
        /// </summary>
        public static List<string> ListPrograms()
        {
            var programs = new List<string>();

            foreach (var val in Enum.GetValues(typeof(InternalProgram)))
            {
                if (((InternalProgram)val) == InternalProgram.NONE)
                    continue;

                programs.Add($"{((InternalProgram?)val).LongName()}");
            }

            return programs;
        }
    }
}
