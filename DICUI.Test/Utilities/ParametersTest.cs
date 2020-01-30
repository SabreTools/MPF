using System.Collections.Generic;
using System.Linq;
using DICUI.Data;
using DICUI.Utilities;
using Xunit;

namespace DICUI.Test.Utilities
{
    public class ParametersTest
    {
        [Theory]
        [InlineData(KnownSystem.MicrosoftXBOX, MediaType.CDROM, CreatorCommand.CompactDisc)]
        [InlineData(KnownSystem.MicrosoftXBOX, MediaType.DVD, CreatorCommand.XBOX)]
        [InlineData(KnownSystem.MicrosoftXBOX, MediaType.LaserDisc, CreatorCommand.NONE)]
        [InlineData(KnownSystem.SegaNu, MediaType.BluRay, CreatorCommand.BluRay)]
        [InlineData(KnownSystem.AppleMacintosh, MediaType.FloppyDisk, CreatorCommand.Floppy)]
        [InlineData(KnownSystem.RawThrillsVarious, MediaType.GDROM, CreatorCommand.NONE)]
        public void ParametersFromSystemAndTypeTest(KnownSystem? knownSystem, MediaType? mediaType, CreatorCommand expected)
        {
            CreatorParameters actual = new CreatorParameters(knownSystem, mediaType, 'D', "disc.bin", 16, true, -1);
            Assert.Equal(expected, actual.Command);
        }

        [Theory]
        [InlineData(KnownSystem.AppleMacintosh, MediaType.LaserDisc, true, 20, null, null)]
        [InlineData(KnownSystem.NintendoGameCube, MediaType.NintendoGameCubeGameDisc, false, 20, null, new CreatorFlag[] { CreatorFlag.Raw })]
        [InlineData(KnownSystem.IBMPCCompatible, MediaType.DVD, false, 20, null, new CreatorFlag[] { })]
        /* paranoid mode tests */
        [InlineData(KnownSystem.IBMPCCompatible, MediaType.CDROM, true, 1000, 2, new CreatorFlag[] { CreatorFlag.C2Opcode, CreatorFlag.NoFixSubQSecuROM, CreatorFlag.ScanFileProtect, CreatorFlag.ScanSectorProtect, CreatorFlag.SubchannelReadLevel })]
        [InlineData(KnownSystem.AppleMacintosh, MediaType.CDROM, false, 20, null, new CreatorFlag[] { CreatorFlag.C2Opcode, CreatorFlag.NoFixSubQSecuROM, CreatorFlag.ScanFileProtect })]
        [InlineData(KnownSystem.IBMPCCompatible, MediaType.DVD, true, 500, null, new CreatorFlag[] { CreatorFlag.CopyrightManagementInformation, CreatorFlag.ScanFileProtect })]
        [InlineData(KnownSystem.HDDVDVideo, MediaType.HDDVD, true, 500, null, new CreatorFlag[] { CreatorFlag.CopyrightManagementInformation })]
        [InlineData(KnownSystem.IBMPCCompatible, MediaType.DVD, false, 500, null, new CreatorFlag[] { })]
        [InlineData(KnownSystem.HDDVDVideo, MediaType.HDDVD, false, 500, null, new CreatorFlag[] { })]
        /* reread c2 */
        [InlineData(KnownSystem.SegaDreamcast, MediaType.GDROM, false, 1000, null, new CreatorFlag[] { CreatorFlag.C2Opcode })]
        [InlineData(KnownSystem.SegaDreamcast, MediaType.GDROM, false, -1, null, new CreatorFlag[] { CreatorFlag.C2Opcode })]

        public void ParametersFromOptionsTest(KnownSystem? knownSystem, MediaType? mediaType, bool paranoid, int rereadC2, int? subchannelLevel, CreatorFlag[] expected)
        {
            CreatorParameters actual = new CreatorParameters(knownSystem, mediaType, 'D', "disc.bin", 16, paranoid, rereadC2);

            HashSet<CreatorFlag> expectedSet = new HashSet<CreatorFlag>(expected ?? new CreatorFlag[0]);
            HashSet<CreatorFlag> actualSet = new HashSet<CreatorFlag>(actual.Keys ?? new CreatorFlag[0]);
            Assert.Equal(expectedSet, actualSet);
            if (rereadC2 == -1 || !Validators.GetValidMediaTypes(knownSystem).Contains(mediaType))
                Assert.Null(actual.C2OpcodeValue[0]);
            else
                Assert.Equal(rereadC2, actual.C2OpcodeValue[0]);
            Assert.Equal(subchannelLevel, actual.SubchannelReadLevelValue);
        }

        [Theory]
        [InlineData(null, null, null, null, null)]
        [InlineData("", null, null, null, null)]
        [InlineData("cd F test.bin 8 /c2 20", MediaType.CDROM, KnownSystem.IBMPCCompatible, "F", "test.bin")]
        [InlineData("fd A blah\\test.img", MediaType.FloppyDisk, KnownSystem.IBMPCCompatible, "A", "blah\\test.img")]
        [InlineData("dvd X super\\blah\\test.iso 8 /raw", MediaType.NintendoGameCubeGameDisc, KnownSystem.NintendoGameCube, "X", "super\\blah\\test.iso")]
        [InlineData("stop D", null, null, "D", null)]
        public void DetermineFlagsTest(string parameters, MediaType? expectedMediaType, KnownSystem? expectedKnownSystem, string expectedDriveLetter, string expectedPath)
        {
            CreatorParameters actualParams = new CreatorParameters(parameters);
            bool actual = actualParams.DetermineFlags(out MediaType? actualMediaType, out KnownSystem? actualKnownSystem, out string actualDriveLetter, out string actualPath);
            Assert.Equal(expectedMediaType, actualMediaType);
            Assert.Equal(expectedKnownSystem, actualKnownSystem);
            Assert.Equal(expectedDriveLetter, actualDriveLetter);
            Assert.Equal(expectedPath, actualPath);
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
            CreatorParameters actual = new CreatorParameters(parameters);
            Assert.Equal(expected, actual.IsValid());
        }
    }
}
