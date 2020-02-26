using System.Collections.Generic;
using System.Linq;
using DICUI.Data;
using DICUI.DiscImageCreator;
using DICUI.Utilities;
using Xunit;

namespace DICUI.Test.Utilities
{
    public class ParametersTest
    {
        [Theory]
        [InlineData(KnownSystem.MicrosoftXBOX, MediaType.CDROM, Command.CompactDisc)]
        [InlineData(KnownSystem.MicrosoftXBOX, MediaType.DVD, Command.XBOX)]
        [InlineData(KnownSystem.MicrosoftXBOX, MediaType.LaserDisc, Command.NONE)]
        [InlineData(KnownSystem.SegaNu, MediaType.BluRay, Command.BluRay)]
        [InlineData(KnownSystem.AppleMacintosh, MediaType.FloppyDisk, Command.Floppy)]
        [InlineData(KnownSystem.RawThrillsVarious, MediaType.GDROM, Command.NONE)]
        public void ParametersFromSystemAndTypeTest(KnownSystem? knownSystem, MediaType? mediaType, Command expected)
        {
            Parameters actual = new Parameters(knownSystem, mediaType, 'D', "disc.bin", 16, true, -1);
            Assert.Equal(expected, actual.Command);
        }

        [Theory]
        [InlineData(KnownSystem.AppleMacintosh, MediaType.LaserDisc, true, 20, null, null)]
        [InlineData(KnownSystem.NintendoGameCube, MediaType.NintendoGameCubeGameDisc, false, 20, null, new Flag[] { Flag.Raw })]
        [InlineData(KnownSystem.IBMPCCompatible, MediaType.DVD, false, 20, null, new Flag[] { })]
        /* paranoid mode tests */
        [InlineData(KnownSystem.IBMPCCompatible, MediaType.CDROM, true, 1000, 2, new Flag[] { Flag.C2Opcode, Flag.NoFixSubQSecuROM, Flag.ScanFileProtect, Flag.ScanSectorProtect, Flag.SubchannelReadLevel })]
        [InlineData(KnownSystem.AppleMacintosh, MediaType.CDROM, false, 20, null, new Flag[] { Flag.C2Opcode, Flag.NoFixSubQSecuROM, Flag.ScanFileProtect })]
        [InlineData(KnownSystem.IBMPCCompatible, MediaType.DVD, true, 500, null, new Flag[] { Flag.CopyrightManagementInformation, Flag.ScanFileProtect })]
        [InlineData(KnownSystem.HDDVDVideo, MediaType.HDDVD, true, 500, null, new Flag[] { Flag.CopyrightManagementInformation })]
        [InlineData(KnownSystem.IBMPCCompatible, MediaType.DVD, false, 500, null, new Flag[] { })]
        [InlineData(KnownSystem.HDDVDVideo, MediaType.HDDVD, false, 500, null, new Flag[] { })]
        /* reread c2 */
        [InlineData(KnownSystem.SegaDreamcast, MediaType.GDROM, false, 1000, null, new Flag[] { Flag.C2Opcode })]
        [InlineData(KnownSystem.SegaDreamcast, MediaType.GDROM, false, -1, null, new Flag[] { Flag.C2Opcode })]

        public void ParametersFromOptionsTest(KnownSystem? knownSystem, MediaType? mediaType, bool paranoid, int rereadC2, int? subchannelLevel, Flag[] expected)
        {
            Parameters actual = new Parameters(knownSystem, mediaType, 'D', "disc.bin", 16, paranoid, rereadC2);

            HashSet<Flag> expectedSet = new HashSet<Flag>(expected ?? new Flag[0]);
            HashSet<Flag> actualSet = new HashSet<Flag>(actual.Keys ?? new Flag[0]);
            Assert.Equal(expectedSet, actualSet);
            if (rereadC2 == -1 || !Validators.GetValidMediaTypes(knownSystem).Contains(mediaType))
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
            Parameters actual = new Parameters(parameters);
            Assert.Equal(expected, actual.IsValid());
        }
    }
}
