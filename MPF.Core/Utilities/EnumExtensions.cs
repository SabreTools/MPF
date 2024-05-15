using System;
using System.Collections.Generic;
using MPF.Core.Converters;
using MPF.Core.Data;
using SabreTools.RedumpLib.Data;

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

        /// <summary>
        /// List all site codes with their short usable names
        /// </summary>
        public static List<string> ListSiteCodes()
        {
            var siteCodes = new List<string>();

            foreach (var val in Enum.GetValues(typeof(SiteCode)))
            {
                string? shortName = ((SiteCode?)val).ShortName()?.TrimEnd(':');
                string? longName = ((SiteCode?)val).LongName()?.TrimEnd(':');
                bool multiline = ((SiteCode?)val).IsMultiLine();

                // Invalid codes should be skipped
                if (shortName == null || longName == null)
                    continue;

                // Handle site tags
                string siteCode;
                if (shortName == longName)
                    siteCode = "***".PadRight(16, ' ');
                else
                    siteCode = shortName.PadRight(16, ' ');

                // Handle expanded tags
                siteCode += longName.PadRight(32, ' ');

                // Include multiline indicator if necessary
                if (multiline)
                    siteCode += "[Multiline]";

                // Add the formatted site code
                siteCodes.Add(siteCode);
            }

            return siteCodes;
        }
    }
}
