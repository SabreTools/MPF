using System;
using System.Collections.Generic;
using System.Linq;
using SabreTools.RedumpLib.Data;
using Xunit;
using LogCompression = MPF.Processors.LogCompression;
using RedumperDriveType = MPF.ExecutionContexts.Redumper.DriveType;
using RedumperReadMethod = MPF.ExecutionContexts.Redumper.ReadMethod;
using RedumperSectorOrder = MPF.ExecutionContexts.Redumper.SectorOrder;

namespace MPF.Frontend.Test
{
    public class EnumExtensionsTests
    {
        #region Long Name

        [Theory]
        [InlineData(null, "Unknown")]
        [InlineData(InterfaceLanguage.AutoDetect, "Auto Detect")]
        [InlineData(InterfaceLanguage.English, "English")]
        [InlineData(InterfaceLanguage.French, "Français")]
        [InlineData(InterfaceLanguage.German, "Deutsch")]
        [InlineData(InterfaceLanguage.Italian, "Italiano")]
        [InlineData(InterfaceLanguage.Japanese, "日本語")]
        [InlineData(InterfaceLanguage.Korean, "한국어")]
        [InlineData(InterfaceLanguage.Polish, "Polski")]
        [InlineData(InterfaceLanguage.Russian, "Русский")]
        [InlineData(InterfaceLanguage.Spanish, "Español")]
        [InlineData(InterfaceLanguage.Swedish, "Svenska")]
        [InlineData(InterfaceLanguage.Ukrainian, "Українська")]
        [InlineData(InterfaceLanguage.L337, "L337")]
        public void LongName_InterfaceLanguage(InterfaceLanguage? lang, string? expected)
        {
            string? actual = lang.LongName();
            Assert.Equal(expected, actual);

            if (lang != null)
            {
                actual = EnumExtensions.GetLongName(lang);
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData(null, "Unknown")]
        [InlineData(InternalProgram.NONE, "Unknown")]
        [InlineData(InternalProgram.Aaru, "Aaru")]
        [InlineData(InternalProgram.DiscImageCreator, "DiscImageCreator")]
        [InlineData(InternalProgram.Redumper, "Redumper")]
        [InlineData(InternalProgram.CleanRip, "CleanRip")]
        [InlineData(InternalProgram.PS3CFW, "PS3 CFW")]
        [InlineData(InternalProgram.UmdImageCreator, "UmdImageCreator")]
        [InlineData(InternalProgram.XboxBackupCreator, "XboxBackupCreator")]
        public void LongName_InternalProgram(InternalProgram? prog, string? expected)
        {
            string? actual = prog.LongName();
            Assert.Equal(expected, actual);

            if (prog != null)
            {
                actual = EnumExtensions.GetLongName(prog);
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData(null, "Unknown")]
        [InlineData(LogCompression.DeflateDefault, "ZIP using Deflate (Level 5)")]
        [InlineData(LogCompression.DeflateMaximum, "ZIP using Deflate (Level 9)")]
        [InlineData(LogCompression.Zstd19, "ZIP using Zstd (Level 19)")]
        public void LongName_LogCompression(LogCompression? comp, string? expected)
        {
            string? actual = comp.LongName();
            Assert.Equal(expected, actual);

            if (comp != null)
            {
                actual = EnumExtensions.GetLongName(comp);
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData(null, "Unknown")]
        [InlineData(RedumperReadMethod.NONE, "Default")]
        [InlineData(RedumperReadMethod.D8, "D8")]
        [InlineData(RedumperReadMethod.BE, "BE")]
        public void LongName_RedumperReadMethod(RedumperReadMethod? method, string? expected)
        {
            string? actual = method.LongName();
            Assert.Equal(expected, actual);

            if (method != null)
            {
                actual = EnumExtensions.GetLongName(method);
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData(null, "Unknown")]
        [InlineData(RedumperSectorOrder.NONE, "Default")]
        [InlineData(RedumperSectorOrder.DATA_C2_SUB, "DATA_C2_SUB")]
        [InlineData(RedumperSectorOrder.DATA_SUB_C2, "DATA_SUB_C2")]
        [InlineData(RedumperSectorOrder.DATA_SUB, "DATA_SUB")]
        [InlineData(RedumperSectorOrder.DATA_C2, "DATA_C2")]
        public void LongName_RedumperSectorOrder(RedumperSectorOrder? order, string? expected)
        {
            string? actual = order.LongName();
            Assert.Equal(expected, actual);

            if (order != null)
            {
                actual = EnumExtensions.GetLongName(order);
                Assert.Equal(expected, actual);
            }
        }

        [Theory]
        [InlineData(null, "Unknown")]
        [InlineData(RedumperDriveType.NONE, "Default")]
        [InlineData(RedumperDriveType.GENERIC, "GENERIC")]
        [InlineData(RedumperDriveType.PLEXTOR, "PLEXTOR")]
        [InlineData(RedumperDriveType.LG_ASU8A, "LG_ASU8A")]
        [InlineData(RedumperDriveType.LG_ASU8B, "LG_ASU8B")]
        [InlineData(RedumperDriveType.LG_ASU8C, "LG_ASU8C")]
        [InlineData(RedumperDriveType.LG_ASU3, "LG_ASU3")]
        [InlineData(RedumperDriveType.LG_ASU2, "LG_ASU2")]
        public void LongName_RedumperDriveType(RedumperDriveType? type, string? expected)
        {
            string? actual = type.LongName();
            Assert.Equal(expected, actual);

            if (type != null)
            {
                actual = EnumExtensions.GetLongName(type);
                Assert.Equal(expected, actual);
            }
        }

        #endregion

        #region Short Name

        [Theory]
        [InlineData(null, "Unknown")]
        [InlineData(InterfaceLanguage.AutoDetect, "auto")]
        [InlineData(InterfaceLanguage.English, "eng")]
        [InlineData(InterfaceLanguage.French, "fra")]
        [InlineData(InterfaceLanguage.German, "deu")]
        [InlineData(InterfaceLanguage.Italian, "ita")]
        [InlineData(InterfaceLanguage.Japanese, "jpn")]
        [InlineData(InterfaceLanguage.Korean, "kor")]
        [InlineData(InterfaceLanguage.Polish, "pol")]
        [InlineData(InterfaceLanguage.Russian, "rus")]
        [InlineData(InterfaceLanguage.Spanish, "spa")]
        [InlineData(InterfaceLanguage.Swedish, "swe")]
        [InlineData(InterfaceLanguage.Ukrainian, "ukr")]
        [InlineData(InterfaceLanguage.L337, "l37")]
        public void ShortName_InterfaceLanguage(InterfaceLanguage? lang, string? expected)
        {
            string? actual = lang.ShortName();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null, "Unknown")]
        [InlineData(InternalProgram.NONE, "Unknown")]
        [InlineData(InternalProgram.Aaru, "aaru")]
        [InlineData(InternalProgram.DiscImageCreator, "dic")]
        [InlineData(InternalProgram.Redumper, "redumper")]
        [InlineData(InternalProgram.CleanRip, "cleanrip")]
        [InlineData(InternalProgram.PS3CFW, "ps3cfw")]
        [InlineData(InternalProgram.UmdImageCreator, "uic")]
        [InlineData(InternalProgram.XboxBackupCreator, "xbc")]
        public void ShortName_InternalProgram(InternalProgram? prog, string? expected)
        {
            string? actual = prog.ShortName();
            Assert.Equal(expected, actual);
        }

        #endregion

        #region From String

        [Theory]
        [InlineData(null, InterfaceLanguage.AutoDetect)]
        [InlineData("", InterfaceLanguage.AutoDetect)]
        [InlineData("auto", InterfaceLanguage.AutoDetect)]
        [InlineData("eng", InterfaceLanguage.English)]
        [InlineData("fra", InterfaceLanguage.French)]
        [InlineData("deu", InterfaceLanguage.German)]
        [InlineData("ita", InterfaceLanguage.Italian)]
        [InlineData("jpn", InterfaceLanguage.Japanese)]
        [InlineData("kor", InterfaceLanguage.Korean)]
        [InlineData("pol", InterfaceLanguage.Polish)]
        [InlineData("rus", InterfaceLanguage.Russian)]
        [InlineData("spa", InterfaceLanguage.Spanish)]
        [InlineData("swe", InterfaceLanguage.Swedish)]
        [InlineData("ukr", InterfaceLanguage.Ukrainian)]
        [InlineData("l37", InterfaceLanguage.L337)]
        public void ToInterfaceLanguageTest(string? interfaceLanguage, InterfaceLanguage expected)
        {
            InterfaceLanguage actual = interfaceLanguage.ToInterfaceLanguage();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null, InternalProgram.NONE)]
        [InlineData("", InternalProgram.NONE)]
        [InlineData("aaru", InternalProgram.Aaru)]
        [InlineData("dic", InternalProgram.DiscImageCreator)]
        [InlineData("redumper", InternalProgram.Redumper)]
        [InlineData("cleanrip", InternalProgram.CleanRip)]
        [InlineData("ps3cfw", InternalProgram.PS3CFW)]
        [InlineData("uic", InternalProgram.UmdImageCreator)]
        [InlineData("xbc", InternalProgram.XboxBackupCreator)]
        public void ToInternalProgramTest(string? internalProgram, InternalProgram expected)
        {
            InternalProgram actual = internalProgram.ToInternalProgram();
            Assert.Equal(expected, actual);
        }

        // TODO: Write remaining from-string tests

        #endregion

        #region Functionality Support

        private static readonly RedumpSystem?[] _antiModchipSystems =
        [
            RedumpSystem.SonyPlayStation,
        ];

        private static readonly RedumpSystem?[] _copyProtectionSystems =
        [
            RedumpSystem.AppleMacintosh,
            RedumpSystem.EnhancedCD ,
            RedumpSystem.IBMPCcompatible,
            RedumpSystem.PalmOS,
            RedumpSystem.PocketPC,
            RedumpSystem.RainbowDisc,
            RedumpSystem.SonyElectronicBook,
        ];

        [Theory]
        [MemberData(nameof(GenerateSupportsAntiModchipScansData))]
        public void SupportsAntiModchipScansTest(RedumpSystem? redumpSystem, bool expected)
        {
            bool actual = redumpSystem.SupportsAntiModchipScans();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(GenerateSupportsCopyProtectionScansData))]
        public void SupportsCopyProtectionScansTest(RedumpSystem? redumpSystem, bool expected)
        {
            bool actual = redumpSystem.SupportsCopyProtectionScans();
            Assert.Equal(expected, actual);
        }

        public static List<object?[]> GenerateSupportsAntiModchipScansData()
        {
            var testData = new List<object?[]>() { new object?[] { null, false } };
            foreach (RedumpSystem redumpSystem in Enum.GetValues(typeof(RedumpSystem)))
            {
                if (_antiModchipSystems.Contains(redumpSystem))
                    testData.Add([redumpSystem, true]);
                else
                    testData.Add([redumpSystem, false]);
            }

            return testData;
        }

        public static List<object?[]> GenerateSupportsCopyProtectionScansData()
        {
            var testData = new List<object?[]>() { new object?[] { null, false } };
            foreach (RedumpSystem redumpSystem in Enum.GetValues(typeof(RedumpSystem)))
            {
                if (_copyProtectionSystems.Contains(redumpSystem))
                    testData.Add([redumpSystem, true]);
                else
                    testData.Add([redumpSystem, false]);
            }

            return testData;
        }

        #endregion
    }
}
