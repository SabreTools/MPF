using System;
using System.Collections.Generic;
using DICUI.Data;
using DICUI.Utilities;
using Xunit;

namespace DICUI.Test.Utilities
{
    public class ParametersTest
    {
        [Theory]
        [InlineData(KnownSystem.MicrosoftXBOX, MediaType.CD, DICCommand.XBOX)]
        [InlineData(KnownSystem.MicrosoftXBOX, MediaType.DVD, DICCommand.XBOX)]
        [InlineData(KnownSystem.MicrosoftXBOX, MediaType.LaserDisc, DICCommand.NONE)]
        [InlineData(KnownSystem.SegaNu, MediaType.BluRay, DICCommand.BluRay)]
        [InlineData(KnownSystem.AppleMacintosh, MediaType.Floppy, DICCommand.Floppy)]
        [InlineData(KnownSystem.RawThrillsVarious, MediaType.GDROM, DICCommand.NONE)]
        public void ParametersFromSystemAndTypeTest(KnownSystem? knownSystem, MediaType? mediaType, DICCommand expected)
        {
            Parameters actual = new Parameters(knownSystem, mediaType, 'D', "disc.bin", 16, true, -1);
            Assert.Equal(expected, actual.Command);
        }

        [Theory]
        [InlineData(KnownSystem.AppleMacintosh, MediaType.LaserDisc, true, 20, null, null)]
        [InlineData(KnownSystem.NintendoGameCube, MediaType.GameCubeGameDisc, false, 20, null, new DICFlag[] { DICFlag.Raw })]
        [InlineData(KnownSystem.IBMPCCompatible, MediaType.DVD, false, 20, null, new DICFlag[] { })]
        /* paranoid mode tests */
        [InlineData(KnownSystem.IBMPCCompatible, MediaType.CD, true, 1000, 2, new DICFlag[] { DICFlag.C2Opcode, DICFlag.NoFixSubQSecuROM, DICFlag.ScanFileProtect, DICFlag.ScanSectorProtect, DICFlag.SubchannelReadLevel })]
        [InlineData(KnownSystem.AppleMacintosh, MediaType.CD, false, 20, null, new DICFlag[] { DICFlag.C2Opcode, DICFlag.NoFixSubQSecuROM, DICFlag.ScanFileProtect })]
        [InlineData(KnownSystem.IBMPCCompatible, MediaType.DVD, true, 500, null, new DICFlag[] { DICFlag.CopyrightManagementInformation })]
        [InlineData(KnownSystem.HDDVDVideo, MediaType.HDDVD, true, 500, null, new DICFlag[] { DICFlag.CopyrightManagementInformation })]
        [InlineData(KnownSystem.IBMPCCompatible, MediaType.DVD, false, 500, null, new DICFlag[] { })]
        [InlineData(KnownSystem.HDDVDVideo, MediaType.HDDVD, false, 500, null, new DICFlag[] { })]
        /* reread c2 */
        [InlineData(KnownSystem.SegaDreamcast, MediaType.GDROM, false, 1000, null, new DICFlag[] { DICFlag.C2Opcode })]
        [InlineData(KnownSystem.SegaDreamcast, MediaType.GDROM, false, -1, null, new DICFlag[] { DICFlag.C2Opcode })]

        public void ParametersFromOptionsTest(KnownSystem? knownSystem, MediaType? mediaType, bool paranoid, int rereadC2, int? subchannelLevel, DICFlag[] expected)
        {
            Parameters actual = new Parameters(knownSystem, mediaType, 'D', "disc.bin", 16, paranoid, rereadC2);

            HashSet<DICFlag> expectedSet = expected != null ? new HashSet<DICFlag>(expected) : null;
            HashSet<DICFlag> actualSet = actual != null ? new HashSet<DICFlag>(actual.Keys) : null;
            Assert.Equal(expectedSet, actualSet);
            Assert.Equal(rereadC2, actual.C2OpcodeValue[0]);
            Assert.Equal(subchannelLevel, actual.SubchannelReadLevelValue);
        }

        [Theory]
        [InlineData(null, null, null, null, null)]
        [InlineData("", null, null, null, null)]
        [InlineData("cd F test.bin 8 /c2 20", MediaType.CD, KnownSystem.IBMPCCompatible, "F", "test.bin")]
        [InlineData("fd A blah\\test.img", MediaType.Floppy, KnownSystem.IBMPCCompatible, "A", "blah\\test.img")]
        [InlineData("dvd X super\\blah\\test.iso 8 /raw", MediaType.GameCubeGameDisc, KnownSystem.NintendoGameCube, "X", "super\\blah\\test.iso")]
        [InlineData("stop D", null, null, "D", null)]
        public void DetermineFlagsTest(string parameters, MediaType? expectedMediaType, KnownSystem? expectedKnownSystem, string expectedDriveLetter, string expectedPath)
        {
            Parameters actualParams = new Parameters(parameters);
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
        [InlineData("bd D longer\\path_test.iso 16", false)]
        [InlineData("stop D", true)]
        [InlineData("ls", false)]
        public void ValidateParametersTest(string parameters, bool expected)
        {
            Parameters actual = new Parameters(parameters);
            Assert.Equal(expected, actual.IsValid());
        }
    }
}
