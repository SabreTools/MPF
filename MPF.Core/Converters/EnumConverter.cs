using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using MPF.Core.Data;
#if NET_FRAMEWORK
using IMAPI2;
#endif
using RedumpLib.Data;

namespace MPF.Core.Converters
{
    public static class EnumConverter
    {
        #region Cross-enumeration conversions

        /// <summary>
        /// Convert drive type to internal version, if possible
        /// </summary>
        /// <param name="driveType">DriveType value to check</param>
        /// <returns>InternalDriveType, if possible, null on error</returns>
        public static InternalDriveType? ToInternalDriveType(this DriveType driveType)
        {
            switch (driveType)
            {
                case DriveType.CDRom:
                    return InternalDriveType.Optical;
                case DriveType.Fixed:
                    return InternalDriveType.HardDisk;
                case DriveType.Removable:
                    return InternalDriveType.Removable;
                default:
                    return null;
            }
        }

#if NET_FRAMEWORK
        /// <summary>
        /// Convert IMAPI physical media type to a MediaType
        /// </summary>
        /// <param name="type">IMAPI_MEDIA_PHYSICAL_TYPE value to check</param>
        /// <returns>MediaType if possible, null on error</returns>
        public static MediaType? IMAPIToMediaType(this IMAPI_MEDIA_PHYSICAL_TYPE type)
        {
            switch (type)
            {
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_UNKNOWN:
                    return MediaType.NONE;
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_CDROM:
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_CDR:
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_CDRW:
                    return MediaType.CDROM;
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDROM:
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDRAM:
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDPLUSR:
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDPLUSRW:
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDPLUSR_DUALLAYER:
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDDASHR:
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDDASHRW:
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDDASHR_DUALLAYER:
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DISK:
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDPLUSRW_DUALLAYER:
                    return MediaType.DVD;
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_HDDVDROM:
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_HDDVDR:
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_HDDVDRAM:
                    return MediaType.HDDVD;
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_BDROM:
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_BDR:
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_BDRE:
                    return MediaType.BluRay;
                default:
                    return null;
            }
        }
#endif

        #endregion

        #region Convert to Long Name

        /// <summary>
        /// Long name method cache
        /// </summary>
        private static readonly ConcurrentDictionary<Type, MethodInfo> LongNameMethods = new ConcurrentDictionary<Type, MethodInfo>();

        /// <summary>
        /// Get the string representation of a generic enumerable value
        /// </summary>
        /// <param name="value">Enum value to convert</param>
        /// <returns>String representation of that value if possible, empty string on error</returns>
        public static string GetLongName(Enum value)
        {
            try
            {
                var sourceType = value?.GetType();
                sourceType = Nullable.GetUnderlyingType(sourceType) ?? sourceType;

                if (!LongNameMethods.TryGetValue(sourceType, out MethodInfo method))
                {
                    
                    method = typeof(RedumpLib.Data.Extensions).GetMethod("LongName", new[] { typeof(Nullable<>).MakeGenericType(sourceType) });
                    if (method == null)
                        method = typeof(EnumConverter).GetMethod("LongName", new[] { typeof(Nullable<>).MakeGenericType(sourceType) });


                    LongNameMethods.TryAdd(sourceType, method);
                }

                if (method != null)
                    return method.Invoke(null, new[] { value }) as string;
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
            switch (prog)
            {
                #region Dumping support

                case InternalProgram.Aaru:
                    return "Aaru";
                case InternalProgram.DD:
                    return "dd";
                case InternalProgram.DiscImageCreator:
                    return "DiscImageCreator";

                #endregion

                #region Verification support only

                case InternalProgram.CleanRip:
                    return "CleanRip";

                case InternalProgram.DCDumper:
                    return "DCDumper";

                case InternalProgram.UmdImageCreator:
                    return "UmdImageCreator";

                #endregion

                case InternalProgram.NONE:
                default:
                    return "Unknown";
            }
        }

        #endregion

        #region Convert to Short Name

        /// <summary>
        /// Short name method cache
        /// </summary>
        private static readonly ConcurrentDictionary<Type, MethodInfo> ShortNameMethods = new ConcurrentDictionary<Type, MethodInfo>();

        /// <summary>
        /// Get the short string representation of a generic enumerable value
        /// </summary>
        /// <param name="value">Enum value to convert</param>
        /// <returns>String representation of that value if possible, empty string on error</returns>
        public static string GetShortName(Enum value)
        {
            try
            {
                var sourceType = value.GetType();
                sourceType = Nullable.GetUnderlyingType(sourceType) ?? sourceType;

                if (!ShortNameMethods.TryGetValue(sourceType, out MethodInfo method))
                {
                    method = typeof(EnumConverter).GetMethod("ShortName", new[] { typeof(Nullable<>).MakeGenericType(sourceType) });
                    ShortNameMethods.TryAdd(sourceType, method);
                }

                if (method != null)
                    return method.Invoke(null, new[] { value }) as string;
                else
                    return string.Empty;
            }
            catch
            {
                // Converter is not implemented for the given type
                return string.Empty;
            }
        }

        #endregion

        #region Convert From String

        /// <summary>
        /// Get the InternalProgram enum value for a given string
        /// </summary>
        /// <param name="internalProgram">String value to convert</param>
        /// <returns>InternalProgram represented by the string, if possible</returns>
        public static InternalProgram ToInternalProgram(string internalProgram)
        {
            switch (internalProgram.ToLowerInvariant())
            {
                // Dumping support
                case "aaru":
                case "chef":
                case "dichef":
                case "discimagechef":
                    return InternalProgram.Aaru;
                case "creator":
                case "dic":
                case "dicreator":
                case "discimagecreator":
                    return InternalProgram.DiscImageCreator;
                case "dd":
                    return InternalProgram.DD;

                // Verification support only
                case "cleanrip":
                case "cr":
                    return InternalProgram.CleanRip;
                case "dc":
                case "dcd":
                case "dcdumper":
                    return InternalProgram.DCDumper;
                case "uic":
                case "umd":
                case "umdcreator":
                case "umdimagecreator":
                    return InternalProgram.UmdImageCreator;

                default:
                    return InternalProgram.NONE;
            }
        }

        /// <summary>
        /// Get the MediaType enum value for a given string
        /// </summary>
        /// <param name="type">String value to convert</param>
        /// <returns>MediaType represented by the string, if possible</returns>
        public static MediaType ToMediaType(string type)
        {
            switch (type.ToLowerInvariant())
            {
                #region Punched Media

                case "aperture":
                case "aperturecard":
                case "aperture card":
                    return MediaType.ApertureCard;
                case "jacquardloom":
                case "jacquardloomcard":
                case "jacquard loom card":
                    return MediaType.JacquardLoomCard;
                case "magneticstripe":
                case "magneticstripecard":
                case "magnetic stripe card":
                    return MediaType.MagneticStripeCard;
                case "opticalphone":
                case "opticalphonecard":
                case "optical phonecard":
                    return MediaType.OpticalPhonecard;
                case "punchcard":
                case "punchedcard":
                case "punched card":
                    return MediaType.PunchedCard;
                case "punchtape":
                case "punchedtape":
                case "punched tape":
                    return MediaType.PunchedTape;

                #endregion

                #region Tape

                case "openreel":
                case "openreeltape":
                case "open reel tape":
                    return MediaType.OpenReel;
                case "datacart":
                case "datacartridge":
                case "datatapecartridge":
                case "data tape cartridge":
                    return MediaType.DataCartridge;
                case "cassette":
                case "cassettetape":
                case "cassette tape":
                    return MediaType.Cassette;

                #endregion

                #region Disc / Disc

                case "bd":
                case "bdrom":
                case "bd-rom":
                case "bluray":
                    return MediaType.BluRay;
                case "cd":
                case "cdrom":
                case "cd-rom":
                    return MediaType.CDROM;
                case "dvd":
                case "dvd5":
                case "dvd-5":
                case "dvd9":
                case "dvd-9":
                case "dvdrom":
                case "dvd-rom":
                    return MediaType.DVD;
                case "fd":
                case "floppy":
                case "floppydisk":
                case "floppy disk":
                case "floppy diskette":
                    return MediaType.FloppyDisk;
                case "floptical":
                    return MediaType.Floptical;
                case "gd":
                case "gdrom":
                case "gd-rom":
                    return MediaType.GDROM;
                case "hddvd":
                case "hd-dvd":
                case "hddvdrom":
                case "hd-dvd-rom":
                    return MediaType.HDDVD;
                case "hdd":
                case "harddisk":
                case "hard disk":
                    return MediaType.HardDisk;
                case "bernoullidisk":
                case "iomegabernoullidisk":
                case "bernoulli disk":
                case "iomega bernoulli disk":
                    return MediaType.IomegaBernoulliDisk;
                case "jaz":
                case "iomegajaz":
                case "iomega jaz":
                    return MediaType.IomegaJaz;
                case "zip":
                case "zipdisk":
                case "iomegazip":
                case "iomega zip":
                    return MediaType.IomegaZip;
                case "ldrom":
                case "lvrom":
                case "ld-rom":
                case "lv-rom":
                case "laserdisc":
                case "laservision":
                case "ld-rom / lv-rom":
                    return MediaType.LaserDisc;
                case "64dd":
                case "n64dd":
                case "64dddisk":
                case "n64dddisk":
                case "64dd disk":
                case "n64dd disk":
                    return MediaType.Nintendo64DD;
                case "fds":
                case "famicom":
                case "nfds":
                case "nintendofamicom":
                case "famicomdisksystem":
                case "famicom disk system":
                case "famicom disk system disk":
                    return MediaType.NintendoFamicomDiskSystem;
                case "gc":
                case "gamecube":
                case "nintendogamecube":
                case "nintendo gamecube":
                case "gamecube disc":
                case "gamecube game disc":
                    return MediaType.NintendoGameCubeGameDisc;
                case "wii":
                case "nintendowii":
                case "nintendo wii":
                case "nintendo wii disc":
                case "wii optical disc":
                    return MediaType.NintendoWiiOpticalDisc;
                case "wiiu":
                case "nintendowiiu":
                case "nintendo wiiu":
                case "nintendo wiiu disc":
                case "wiiu optical disc":
                case "wii u optical disc":
                    return MediaType.NintendoWiiUOpticalDisc;
                case "umd":
                    return MediaType.UMD;

                #endregion

                // Unsorted Formats
                case "cartridge":
                    return MediaType.Cartridge;
                case "ced":
                case "rcaced":
                case "rca ced":
                case "videodisc":
                case "rca videodisc":
                    return MediaType.CED;

                default:
                    return MediaType.NONE;
            }
        }

        #endregion
    }
}
