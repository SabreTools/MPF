using System;
using System.Linq;
using MPF.Data;
using MPF.Utilities;
using RedumpLib.Data;
using Xunit;

namespace MPF.Test.Utilities
{
    public class ValidatorsTest
    {
        [Theory]
        [InlineData(RedumpSystem.BandaiPippin, MediaType.CDROM)]
        [InlineData(RedumpSystem.MicrosoftXbox, MediaType.DVD)]
        [InlineData(RedumpSystem.NintendoGameCube, MediaType.NintendoGameCubeGameDisc)]
        [InlineData(RedumpSystem.NintendoWii, MediaType.NintendoWiiOpticalDisc)]
        [InlineData(RedumpSystem.NintendoWiiU, MediaType.NintendoWiiUOpticalDisc)]
        [InlineData(RedumpSystem.SonyPlayStationPortable, MediaType.UMD)]
        public void GetValidMediaTypesTest(RedumpSystem? knownSystem, MediaType? expected)
        {
            var actual = Validators.GetValidMediaTypes(knownSystem);
            Assert.Contains(expected, actual);
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
