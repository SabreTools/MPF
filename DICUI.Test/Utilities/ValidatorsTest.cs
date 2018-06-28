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
            var actual = Validators.CreateListOfSystems();
            Assert.Equal(Enum.GetValues(typeof(KnownSystem)).Length + 4, actual.Count);
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

        [Fact]
        public void DetermineFlagsTest()
        {
            // TODO: Implement, maybe make Theory?
            Assert.True(true);
        }
    }
}
