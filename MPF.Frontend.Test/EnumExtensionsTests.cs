using System;
using System.Collections.Generic;
using System.Linq;
using SabreTools.RedumpLib.Data;
using Xunit;
using RedumperReadMethod = MPF.ExecutionContexts.Redumper.ReadMethod;
using RedumperSectorOrder = MPF.ExecutionContexts.Redumper.SectorOrder;
using RedumperDriveType = MPF.ExecutionContexts.Redumper.DriveType;

namespace MPF.Frontend.Test
{
    public class EnumExtensionsTests
    {
        #region Long Name

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
        }

        #endregion

        #region Short Name

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
