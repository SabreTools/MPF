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
            // TODO: Implement
            Assert.True(true);
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

        [Fact]
        public void ValidateParametersTest()
        {
            // TODO: Implement, maybe make Theory?
            Assert.True(true);
        }

        [Fact]
        public void DetermineFlagsTest()
        {
            // TODO: Implement, maybe make Theory?
            Assert.True(true);
        }
    }
}
