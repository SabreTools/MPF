using System;
using System.Collections.Generic;
using System.Linq;
using SabreTools.RedumpLib.Data;
using Xunit;
using RedumperReadMethod = MPF.ExecutionContexts.Redumper.ReadMethod;
using RedumperSectorOrder = MPF.ExecutionContexts.Redumper.SectorOrder;

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
        [InlineData(RedumperReadMethod.BE_CDDA, "BE_CDDA")]
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

        #endregion

        #region From String

        // TODO: Write from-string tests

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