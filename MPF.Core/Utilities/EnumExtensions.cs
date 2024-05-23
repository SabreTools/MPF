using System;
using System.Collections.Generic;
#if NET40_OR_GREATER || NETCOREAPP
using System.Collections.Concurrent;
#endif
using System.Reflection;
using SabreTools.RedumpLib.Data;

namespace MPF.Core.Utilities
{
    public static class EnumExtensions
    {
        #region Convert to Long Name

        /// <summary>
        /// Long name method cache
        /// </summary>
#if NET20 || NET35
        private static readonly Dictionary<Type, MethodInfo?> LongNameMethods = [];
#else
        private static readonly ConcurrentDictionary<Type, MethodInfo?> LongNameMethods = [];
#endif

        /// <summary>
        /// Get the string representation of a generic enumerable value
        /// </summary>
        /// <param name="value">Enum value to convert</param>
        /// <returns>String representation of that value if possible, empty string on error</returns>
        public static string GetLongName(Enum value)
        {
            try
            {
                var sourceType = value.GetType();
                sourceType = Nullable.GetUnderlyingType(sourceType) ?? sourceType;

                if (!LongNameMethods.TryGetValue(sourceType, out var method))
                {
                    method = typeof(Extensions).GetMethod("LongName", [typeof(Nullable<>).MakeGenericType(sourceType)]);
                    method ??= typeof(EnumExtensions).GetMethod("LongName", [typeof(Nullable<>).MakeGenericType(sourceType)]);

#if NET20 || NET35
                    LongNameMethods[sourceType] = method;
#else
                    LongNameMethods.TryAdd(sourceType, method);
#endif
                }

                if (method != null)
                    return method.Invoke(null, new[] { value }) as string ?? string.Empty;
                else
                    return string.Empty;
            }
            catch
            {
                // Converter is not implemented for the given type
                return string.Empty;
            }
        }

        /// <summary>
        /// Get the string representation of the InternalProgram enum values
        /// </summary>
        /// <param name="prog">InternalProgram value to convert</param>
        /// <returns>String representing the value, if possible</returns>
        public static string LongName(this InternalProgram? prog)
        {
            return (prog) switch
            {
                #region Dumping support

                InternalProgram.Aaru => "Aaru",
                InternalProgram.DiscImageCreator => "DiscImageCreator",
                InternalProgram.Redumper => "Redumper",

                #endregion

                #region Verification support only

                InternalProgram.CleanRip => "CleanRip",
                InternalProgram.PS3CFW => "PS3 CFW",
                InternalProgram.UmdImageCreator => "UmdImageCreator",
                InternalProgram.XboxBackupCreator => "XboxBackupCreator",

                #endregion

                InternalProgram.NONE => "Unknown",
                _ => "Unknown",
            };
        }

        /// <summary>
        /// Get the string representation of the RedumperReadMethod enum values
        /// </summary>
        /// <param name="method">RedumperReadMethod value to convert</param>
        /// <returns>String representing the value, if possible</returns>
        public static string LongName(this RedumperReadMethod? method)
        {
            return (method) switch
            {
                RedumperReadMethod.D8 => "D8",
                RedumperReadMethod.BE => "BE",
                RedumperReadMethod.BE_CDDA => "BE_CDDA",

                RedumperReadMethod.NONE => "Default",
                _ => "Unknown",
            };
        }

        /// <summary>
        /// Get the string representation of the RedumperSectorOrder enum values
        /// </summary>
        /// <param name="order">RedumperSectorOrder value to convert</param>
        /// <returns>String representing the value, if possible</returns>
        public static string LongName(this RedumperSectorOrder? order)
        {
            return (order) switch
            {
                RedumperSectorOrder.DATA_C2_SUB => "DATA_C2_SUB",
                RedumperSectorOrder.DATA_SUB_C2 => "DATA_SUB_C2",
                RedumperSectorOrder.DATA_SUB => "DATA_SUB",
                RedumperSectorOrder.DATA_C2 => "DATA_C2",

                RedumperSectorOrder.NONE => "Default",
                _ => "Unknown",
            };
        }

        #endregion

        #region Convert From String

        /// <summary>
        /// Get the InternalProgram enum value for a given string
        /// </summary>
        /// <param name="internalProgram">String value to convert</param>
        /// <returns>InternalProgram represented by the string, if possible</returns>
        public static InternalProgram ToInternalProgram(string? internalProgram)
        {
            return (internalProgram?.ToLowerInvariant()) switch
            {
                // Dumping support
                "aaru"
                    or "chef"
                    or "dichef"
                    or "discimagechef" => InternalProgram.Aaru,
                "creator"
                    or "dic"
                    or "dicreator"
                    or "discimagecreator" => InternalProgram.DiscImageCreator,
                "rd"
                    or "redumper" => InternalProgram.Redumper,

                // Verification support only
                "cleanrip"
                    or "cr" => InternalProgram.CleanRip,
                "ps3cfw"
                    or "ps3"
                    or "getkey"
                    or "managunz"
                    or "multiman" => InternalProgram.PS3CFW,
                "uic"
                    or "umd"
                    or "umdcreator"
                    or "umdimagecreator" => InternalProgram.UmdImageCreator,
                "xbc"
                    or "xbox"
                    or "xbox360"
                    or "xboxcreator"
                    or "xboxbackupcreator" => InternalProgram.XboxBackupCreator,

                _ => InternalProgram.NONE,
            };
        }

        /// <summary>
        /// Get the MediaType enum value for a given string
        /// </summary>
        /// <param name="type">String value to convert</param>
        /// <returns>MediaType represented by the string, if possible</returns>
        public static MediaType ToMediaType(string type)
        {
            return (type.ToLowerInvariant()) switch
            {
                #region Punched Media

                "aperture"
                    or "aperturecard"
                    or "aperture card" => MediaType.ApertureCard,
                "jacquardloom"
                    or "jacquardloomcard"
                    or "jacquard loom card" => MediaType.JacquardLoomCard,
                "magneticstripe"
                    or "magneticstripecard"
                    or "magnetic stripe card" => MediaType.MagneticStripeCard,
                "opticalphone"
                    or "opticalphonecard"
                    or "optical phonecard" => MediaType.OpticalPhonecard,
                "punchcard"
                    or "punchedcard"
                    or "punched card" => MediaType.PunchedCard,
                "punchtape"
                    or "punchedtape"
                    or "punched tape" => MediaType.PunchedTape,

                #endregion

                #region Tape

                "openreel"
                    or "openreeltape"
                    or "open reel tape" => MediaType.OpenReel,
                "datacart"
                    or "datacartridge"
                    or "datatapecartridge"
                    or "data tape cartridge" => MediaType.DataCartridge,
                "cassette"
                    or "cassettetape"
                    or "cassette tape" => MediaType.Cassette,

                #endregion

                #region Disc / Disc

                "bd"
                    or "bdrom"
                    or "bd-rom"
                    or "bluray" => MediaType.BluRay,
                "cd"
                    or "cdrom"
                    or "cd-rom" => MediaType.CDROM,
                "dvd"
                    or "dvd5"
                    or "dvd-5"
                    or "dvd9"
                    or "dvd-9"
                    or "dvdrom"
                    or "dvd-rom" => MediaType.DVD,
                "fd"
                    or "floppy"
                    or "floppydisk"
                    or "floppy disk"
                    or "floppy diskette" => MediaType.FloppyDisk,
                "floptical" => MediaType.Floptical,
                "gd"
                    or "gdrom"
                    or "gd-rom" => MediaType.GDROM,
                "hddvd"
                    or "hd-dvd"
                    or "hddvdrom"
                    or "hd-dvd-rom" => MediaType.HDDVD,
                "hdd"
                    or "harddisk"
                    or "hard disk" => MediaType.HardDisk,
                "bernoullidisk"
                    or "iomegabernoullidisk"
                    or "bernoulli disk"
                    or "iomega bernoulli disk" => MediaType.IomegaBernoulliDisk,
                "jaz"
                    or "iomegajaz"
                    or "iomega jaz" => MediaType.IomegaJaz,
                "zip"
                    or "zipdisk"
                    or "iomegazip"
                    or "iomega zip" => MediaType.IomegaZip,
                "ldrom"
                    or "lvrom"
                    or "ld-rom"
                    or "lv-rom"
                    or "laserdisc"
                    or "laservision"
                    or "ld-rom / lv-rom" => MediaType.LaserDisc,
                "64dd"
                    or "n64dd"
                    or "64dddisk"
                    or "n64dddisk"
                    or "64dd disk"
                    or "n64dd disk" => MediaType.Nintendo64DD,
                "fds"
                    or "famicom"
                    or "nfds"
                    or "nintendofamicom"
                    or "famicomdisksystem"
                    or "famicom disk system"
                    or "famicom disk system disk" => MediaType.NintendoFamicomDiskSystem,
                "gc"
                    or "gamecube"
                    or "nintendogamecube"
                    or "nintendo gamecube"
                    or "gamecube disc"
                    or "gamecube game disc" => MediaType.NintendoGameCubeGameDisc,
                "wii"
                    or "nintendowii"
                    or "nintendo wii"
                    or "nintendo wii disc"
                    or "wii optical disc" => MediaType.NintendoWiiOpticalDisc,
                "wiiu"
                    or "nintendowiiu"
                    or "nintendo wiiu"
                    or "nintendo wiiu disc"
                    or "wiiu optical disc"
                    or "wii u optical disc" => MediaType.NintendoWiiUOpticalDisc,
                "umd" => MediaType.UMD,

                #endregion

                // Unsorted Formats
                "cartridge" => MediaType.Cartridge,
                "ced"
                    or "rcaced"
                    or "rca ced"
                    or "videodisc"
                    or "rca videodisc" => MediaType.CED,

                _ => MediaType.NONE,
            };
        }

        /// <summary>
        /// Get the RedumperReadMethod enum value for a given string
        /// </summary>
        /// <param name="method">String value to convert</param>
        /// <returns>RedumperReadMethod represented by the string, if possible</returns>
        public static RedumperReadMethod ToRedumperReadMethod(string? method)
        {
            return (method?.ToLowerInvariant()) switch
            {
                "d8" => RedumperReadMethod.D8,
                "be" => RedumperReadMethod.BE,
                "be_cdda"
                    or "be cdda"
                    or "be-cdda"
                    or "becdda" => RedumperReadMethod.BE_CDDA,

                _ => RedumperReadMethod.NONE,
            };
        }

        /// <summary>
        /// Get the RedumperSectorOrder enum value for a given string
        /// </summary>
        /// <param name="order">String value to convert</param>
        /// <returns>RedumperSectorOrder represented by the string, if possible</returns>
        public static RedumperSectorOrder ToRedumperSectorOrder(string? order)
        {
            return (order?.ToLowerInvariant()) switch
            {
                "data_c2_sub"
                    or "data c2 sub"
                    or "data-c2-sub"
                    or "datac2sub" => RedumperSectorOrder.DATA_C2_SUB,
                "data_sub_c2"
                    or "data sub c2"
                    or "data-sub-c2"
                    or "datasubc2" => RedumperSectorOrder.DATA_SUB_C2,
                "data_sub"
                    or "data sub"
                    or "data-sub"
                    or "datasub" => RedumperSectorOrder.DATA_SUB,
                "data_c2"
                    or "data c2"
                    or "data-c2"
                    or "datac2" => RedumperSectorOrder.DATA_C2,

                _ => RedumperSectorOrder.NONE,
            };
        }

        #endregion

        #region Enum Helpers

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

        #endregion
    }
}
