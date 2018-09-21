using System;
using DICUI.Data;
using DICUI.Utilities;
using Xunit;

namespace DICUI.Test.Utilities
{
    public class ValidatorsTest
    {
        [Theory]
        [InlineData(KnownSystem.BandaiApplePippin, MediaType.CDROM)]
        [InlineData(KnownSystem.MicrosoftXBOX, MediaType.DVD)]
        [InlineData(KnownSystem.NintendoGameCube, MediaType.NintendoGameCube)]
        [InlineData(KnownSystem.NintendoWii, MediaType.NintendoWiiOpticalDisc)]
        [InlineData(KnownSystem.NintendoWiiU, MediaType.NintendoWiiUOpticalDisc)]
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
    }
}
