using System.Collections.Generic;
using System.Linq;
using MPF.Core.Data;
using MPF.Modules.DiscImageCreator;
using RedumpLib.Data;
using Xunit;

namespace MPF.Test.Utilities
{
    public class ParametersTest
    {
        [Theory]
        [InlineData(RedumpSystem.MicrosoftXbox, MediaType.CDROM, CommandStrings.CompactDisc)]
        [InlineData(RedumpSystem.MicrosoftXbox, MediaType.DVD, CommandStrings.XBOX)]
        [InlineData(RedumpSystem.MicrosoftXbox, MediaType.LaserDisc, null)]
        [InlineData(RedumpSystem.SonyPlayStation3, MediaType.BluRay, CommandStrings.BluRay)]
        [InlineData(RedumpSystem.AppleMacintosh, MediaType.FloppyDisk, CommandStrings.Floppy)]
        [InlineData(RedumpSystem.RawThrillsVarious, MediaType.GDROM, null)]
        public void ParametersFromSystemAndTypeTest(RedumpSystem? knownSystem, MediaType? mediaType, string expected)
        {
            var options = new Options { };
            var actual = new Parameters(knownSystem, mediaType, 'D', "disc.bin", 16, options);
            Assert.Equal(expected, actual.BaseCommand);
        }

        [Theory]
        [InlineData(RedumpSystem.AppleMacintosh, MediaType.LaserDisc, true, 20, null, null)]
        [InlineData(RedumpSystem.NintendoGameCube, MediaType.NintendoGameCubeGameDisc, false, 20, null, new string[] { FlagStrings.Raw })]
        [InlineData(RedumpSystem.IBMPCcompatible, MediaType.DVD, false, 20, null, new string[] { FlagStrings.CopyrightManagementInformation, FlagStrings.ScanFileProtect })]
        /* paranoid mode tests */
        [InlineData(RedumpSystem.IBMPCcompatible, MediaType.CDROM, true, 1000, 2, new string[] { FlagStrings.C2Opcode, FlagStrings.NoFixSubQSecuROM, FlagStrings.ScanFileProtect, FlagStrings.ScanSectorProtect, FlagStrings.SubchannelReadLevel })]
        [InlineData(RedumpSystem.AppleMacintosh, MediaType.CDROM, false, 20, null, new string[] { FlagStrings.C2Opcode, FlagStrings.NoFixSubQSecuROM, FlagStrings.ScanFileProtect, FlagStrings.ScanSectorProtect, FlagStrings.SubchannelReadLevel })]
        [InlineData(RedumpSystem.IBMPCcompatible, MediaType.DVD, true, 500, null, new string[] { FlagStrings.CopyrightManagementInformation, FlagStrings.ScanFileProtect })]
        [InlineData(RedumpSystem.HDDVDVideo, MediaType.HDDVD, true, 500, null, new string[] { FlagStrings.CopyrightManagementInformation })]
        [InlineData(RedumpSystem.IBMPCcompatible, MediaType.DVD, false, 500, null, new string[] { FlagStrings.CopyrightManagementInformation, FlagStrings.ScanFileProtect })]
        [InlineData(RedumpSystem.HDDVDVideo, MediaType.HDDVD, false, 500, null, new string[] { FlagStrings.CopyrightManagementInformation })]
        /* reread c2 */
        [InlineData(RedumpSystem.SegaDreamcast, MediaType.GDROM, false, 1000, null, new string[] { FlagStrings.C2Opcode })]
        [InlineData(RedumpSystem.SegaDreamcast, MediaType.GDROM, false, -1, null, new string[] { FlagStrings.C2Opcode })]

        public void ParametersFromOptionsTest(RedumpSystem? knownSystem, MediaType? mediaType, bool paranoid, int rereadC2, int? subchannelLevel, string[] expected)
        {
            var options = new Options { DICParanoidMode = paranoid, DICRereadCount = rereadC2 };
            var actual = new Parameters(knownSystem, mediaType, 'D', "disc.bin", 16, options);

            HashSet<string> expectedSet = new HashSet<string>(expected ?? new string[0]);
            HashSet<string> actualSet = new HashSet<string>(actual.Keys.Cast<string>() ?? new string[0]);
            Assert.Equal(expectedSet, actualSet);
            if (rereadC2 == -1 || !knownSystem.MediaTypes().Contains(mediaType))
                Assert.Null(actual.C2OpcodeValue[0]);
            else
                Assert.Equal(rereadC2, actual.C2OpcodeValue[0]);
            Assert.Equal(subchannelLevel, actual.SubchannelReadLevelValue);
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("cd F test.bin 8 /c2 20", true)]
        [InlineData("fd A test.img", true)]
        [InlineData("dvd X super\\test.iso 8 /raw", true)]
        [InlineData("bd D longer\\path_test.iso 16", true)]
        [InlineData("stop D", true)]
        [InlineData("ls", false)]
        public void ValidateParametersTest(string parameters, bool expected)
        {
            var actual = new Parameters(parameters);
            Assert.Equal(expected, actual.IsValid());
        }
    }
}
