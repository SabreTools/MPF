using System;
using System.Collections.Generic;
using MPF.Core.Converters;
using MPF.Core.Data;
using SabreTools.RedumpLib.Data;

namespace MPF.Core.Utilities
{
    public static class EnumExtensions
    {
        // Moved to RedumpLib
        /// <summary>
        /// Determine if a system is okay if it's not detected by Windows
        /// </summary>
        /// <param name="system">RedumpSystem value to check</param>
        /// <returns>True if Windows show see a disc when dumping, false otherwise</returns>
        public static bool DetectedByWindows(this RedumpSystem? system)
        {
            return system switch
            {
                RedumpSystem.AmericanLaserGames3DO
                    or RedumpSystem.AppleMacintosh
                    or RedumpSystem.Atari3DO
                    or RedumpSystem.AtariJaguarCDInteractiveMultimediaSystem
                    or RedumpSystem.NewJatreCDi
                    or RedumpSystem.NintendoGameCube
                    or RedumpSystem.NintendoWii
                    or RedumpSystem.NintendoWiiU
                    or RedumpSystem.PhilipsCDi
                    or RedumpSystem.PhilipsCDiDigitalVideo
                    or RedumpSystem.Panasonic3DOInteractiveMultiplayer
                    or RedumpSystem.PanasonicM2
                    or RedumpSystem.PioneerLaserActive
                    or RedumpSystem.SuperAudioCD => false,
                _ => true,
            };
        }

        /// <summary>
        /// Determine if the media supports drive speeds
        /// </summary>
        /// <param name="type">MediaType value to check</param>
        /// <returns>True if the media has variable dumping speeds, false otherwise</returns>
        public static bool DoesSupportDriveSpeed(this MediaType? type)
        {
            return type switch
            {
                MediaType.CDROM
                    or MediaType.DVD
                    or MediaType.GDROM
                    or MediaType.HDDVD
                    or MediaType.BluRay
                    or MediaType.NintendoGameCubeGameDisc
                    or MediaType.NintendoWiiOpticalDisc => true,
                _ => false,
            };
        }

        // Moved to RedumpLib
        /// <summary>
        /// Determine if a system has reversed ringcodes
        /// </summary>
        /// <param name="system">RedumpSystem value to check</param>
        /// <returns>True if the system has reversed ringcodes, false otherwise</returns>
        public static bool HasReversedRingcodes(this RedumpSystem? system)
        {
            return system switch
            {
                RedumpSystem.SonyPlayStation2
                    or RedumpSystem.SonyPlayStation3
                    or RedumpSystem.SonyPlayStation4
                    or RedumpSystem.SonyPlayStation5
                    or RedumpSystem.SonyPlayStationPortable => true,
                _ => false,
            };
        }

        // Moved to RedumpLib
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
            return system switch
            {
                RedumpSystem.AtariJaguarCDInteractiveMultimediaSystem
                    or RedumpSystem.AudioCD
                    or RedumpSystem.DVDAudio
                    or RedumpSystem.HasbroiONEducationalGamingSystem
                    or RedumpSystem.HasbroVideoNow
                    or RedumpSystem.HasbroVideoNowColor
                    or RedumpSystem.HasbroVideoNowJr
                    or RedumpSystem.HasbroVideoNowXP
                    or RedumpSystem.PhilipsCDi
                    or RedumpSystem.PlayStationGameSharkUpdates
                    or RedumpSystem.SuperAudioCD => true,
                _ => false,
            };
        }

        // Moved to RedumpLib
        /// <summary>
        /// Determine if a system is considered XGD
        /// </summary>
        /// <param name="system">RedumpSystem value to check</param>
        /// <returns>True if the system is XGD, false otherwise</returns>
        public static bool IsXGD(this RedumpSystem? system)
        {
            return system switch
            {
                RedumpSystem.MicrosoftXbox
                    or RedumpSystem.MicrosoftXbox360
                    or RedumpSystem.MicrosoftXboxOne
                    or RedumpSystem.MicrosoftXboxSeriesXS => true,
                _ => false,
            };
        }

        /// <summary>
        /// List all programs with their short usable names
        /// </summary>
        public static List<string> ListPrograms()
        {
            var programs = new List<string>();

            foreach (var val in Enum.GetValues(typeof(InternalProgram)))
            {
                if (((InternalProgram)val!) == InternalProgram.NONE)
                    continue;

                programs.Add($"{((InternalProgram?)val).LongName()}");
            }

            return programs;
        }
    }
}
