using System;
using DICUI.Data;
using DICUI.Utilities;
using Xunit;

namespace DICUI.Test.Utilities
{
    public class ValidatorsTest
    {
        [Theory]
        [InlineData(KnownSystem.BandaiApplePippin, MediaType.CD)]
        [InlineData(KnownSystem.MicrosoftXBOX, MediaType.DVD)]
        [InlineData(KnownSystem.NintendoGameCube, MediaType.GameCubeGameDisc)]
        [InlineData(KnownSystem.NintendoWii, MediaType.WiiOpticalDisc)]
        [InlineData(KnownSystem.NintendoWiiU, MediaType.WiiUOpticalDisc)]
        [InlineData(KnownSystem.SonyPlayStationPortable, MediaType.UMD)]
        public void GetValidMediaTypesTest(KnownSystem? knownSystem, MediaType? expected)
        {
            var actual = Validators.GetValidMediaTypes(knownSystem);
            Assert.Contains(expected, actual);
        }

        [Fact]
        public void CreateListOfSystemsTest()
        {
            int expected = Enum.GetValues(typeof(KnownSystem)).Length - 5; // - 4 -1 for markers categories and KnownSystem.NONE
            var actual = Validators.CreateListOfSystems();
            Assert.Equal(expected, actual.Count);
        }

        [Fact]
        public void CreateListOfDrivesTest()
        {
            // TODO: Implement
            Assert.True(true);
        }

        [Fact]
        public void GetDiscTypeTest()
        {
            // TODO: Implement
            Assert.True(true);
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
            bool actual = Validators.ValidateParameters(parameters);
            Assert.Equal(expected, actual);
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
            bool actual = Validators.DetermineFlags(parameters, out MediaType? actualMediaType, out KnownSystem? actualKnownSystem, out string actualDriveLetter, out string actualPath);
            Assert.Equal(expectedMediaType, actualMediaType);
            Assert.Equal(expectedKnownSystem, actualKnownSystem);
            Assert.Equal(expectedDriveLetter, actualDriveLetter);
            Assert.Equal(expectedPath, actualPath);
        }
    }
}
