using System;
#if NET20 || NET35
using System.Collections.Generic;
#else
using System.Collections.Concurrent;
#endif
using System.Reflection;
using SabreTools.RedumpLib.Data;
using RedumperReadMethod = MPF.ExecutionContexts.Redumper.ReadMethod;
using RedumperSectorOrder = MPF.ExecutionContexts.Redumper.SectorOrder;
using RedumperDriveType = MPF.ExecutionContexts.Redumper.DriveType;
namespace MPF.Frontend
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
                    return method.Invoke(null, [value]) as string ?? string.Empty;
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
            return prog switch
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
            return method switch
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
            return order switch
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

        #region Convert to Short Name

        /// <summary>
        /// Get the short string representation of the InternalProgram enum values
        /// </summary>
        /// <param name="prog">InternalProgram value to convert</param>
        /// <returns>String representing the value, if possible</returns>
        public static string ShortName(this InternalProgram? prog)
        {
            return prog switch
            {
                #region Dumping support

                InternalProgram.Aaru => "aaru",
                InternalProgram.DiscImageCreator => "dic",
                InternalProgram.Redumper => "redumper",

                #endregion

                #region Verification support only

                InternalProgram.CleanRip => "cleanrip",
                InternalProgram.PS3CFW => "ps3cfw",
                InternalProgram.UmdImageCreator => "uic",
                InternalProgram.XboxBackupCreator => "xbc",

                #endregion

                InternalProgram.NONE => "Unknown",
                _ => "Unknown",
            };
        }

        #endregion

        #region Convert from String

        /// <summary>
        /// Get the InternalProgram enum value for a given string
        /// </summary>
        /// <param name="internalProgram">String value to convert</param>
        /// <returns>InternalProgram represented by the string, if possible</returns>
        public static InternalProgram ToInternalProgram(this string? internalProgram)
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
        /// Get the RedumperReadMethod enum value for a given string
        /// </summary>
        /// <param name="method">String value to convert</param>
        /// <returns>RedumperReadMethod represented by the string, if possible</returns>
        public static RedumperReadMethod ToRedumperReadMethod(this string? method)
        {
            return (method?.ToLowerInvariant()) switch
            {
                "d8" => RedumperReadMethod.D8,
                "be" => RedumperReadMethod.BE,

                _ => RedumperReadMethod.NONE,
            };
        }

        /// <summary>
        /// Get the RedumperSectorOrder enum value for a given string
        /// </summary>
        /// <param name="order">String value to convert</param>
        /// <returns>RedumperSectorOrder represented by the string, if possible</returns>
        public static RedumperSectorOrder ToRedumperSectorOrder(this string? order)
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

        /// <summary>
        /// Get the RedumperDriveType enum value for a given string
        /// </summary>
        /// <param name="order">String value to convert</param>
        /// <returns>RedumperDriveType represented by the string, if possible</returns>
        public static RedumperDriveType ToRedumperDriveType(this string? type)
        {
            return (type?.ToLowerInvariant()) switch
            {
                "generic" => RedumperDriveType.GENERIC,
                "plextor" => RedumperDriveType.PLEXTOR,
                "lg_asus8a"
                    or "lg-asus8a"
                    or "lgasus8a"
                    or "lg_asus_8a"
                    or "lg-asus-8a" => RedumperDriveType.LG_ASU8A,
                "lg_asus8b"
                    or "lg-asus8b"
                    or "lgasus8b"
                    or "lg_asus_8b"
                    or "lg-asus-8b" => RedumperDriveType.LG_ASU8B,
                "lg_asus8c"
                    or "lg-asus8c"
                    or "lgasus8c"
                    or "lg_asus_8c"
                    or "lg-asus-8c" => RedumperDriveType.LG_ASU8C,
                "lg_asus3"
                    or "lg-asus3"
                    or "lgasus3"
                    or "lg_asus_3"
                    or "lg-asus-3" => RedumperDriveType.LG_ASU3,
                "lg_asus2"
                    or "lg-asus2"
                    or "lgasus2"
                    or "lg_asus_2"
                    or "lg-asus-2" => RedumperDriveType.LG_ASU2,

                _ => RedumperDriveType.NONE,
            };
        }

        #endregion

        #region Functionality Support

        /// <summary>
        /// Get if a system requires an anti-modchip scan
        /// </summary>
        public static bool SupportsAntiModchipScans(this RedumpSystem? system)
        {
            return system switch
            {
                RedumpSystem.SonyPlayStation => true,
                _ => false,
            };
        }

        /// <summary>
        /// Get if a system requires a copy protection scan
        /// </summary>
        public static bool SupportsCopyProtectionScans(this RedumpSystem? system)
        {
            return system switch
            {
                RedumpSystem.AppleMacintosh => true,
                RedumpSystem.EnhancedCD => true,
                RedumpSystem.IBMPCcompatible => true,
                RedumpSystem.PalmOS => true,
                RedumpSystem.PocketPC => true,
                RedumpSystem.RainbowDisc => true,
                RedumpSystem.SonyElectronicBook => true,
                _ => false,
            };
        }

        #endregion
    }
}
