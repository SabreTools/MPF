using SabreTools.RedumpLib.Data;
using Xunit;

namespace MPF.Frontend.Test
{
    public class InterfaceConstantsTests
    {
        [Theory]
        [InlineData(PhysicalMediaType.CDROM, 72)]
        [InlineData(PhysicalMediaType.DVD, 24)]
        [InlineData(PhysicalMediaType.NintendoGameCubeGameDisc, 24)]
        [InlineData(PhysicalMediaType.NintendoWiiOpticalDisc, 24)]
        [InlineData(PhysicalMediaType.HDDVD, 24)]
        [InlineData(PhysicalMediaType.BluRay, 16)]
        [InlineData(PhysicalMediaType.NintendoWiiUOpticalDisc, 16)]
        [InlineData(PhysicalMediaType.LaserDisc, 72)]
        [InlineData(null, 72)]
        public void GetAllowedDriveSpeedForPhysicalMediaTypeTest(PhysicalMediaType? mediaType, int maxExpected)
        {
            var actual = InterfaceConstants.GetSpeedsForPhysicalMediaType(mediaType);
            Assert.Equal(maxExpected, actual[^1]);
        }
    }
}
