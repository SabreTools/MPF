using MPF.Frontend;
using SabreTools.RedumpLib.Data;
using Xunit;

namespace MPF.Test.Data
{
    public class UIElementsTest
    {
        [Theory]
        [InlineData(MediaType.CDROM, 72)]
        [InlineData(MediaType.DVD, 24)]
        [InlineData(MediaType.BluRay, 16)]
        [InlineData(MediaType.LaserDisc, 1)]
        [InlineData(null, 1)]
        public void GetAllowedDriveSpeedForMediaTypeTest(MediaType? mediaType, int maxExpected)
        {
            var actual = InterfaceConstants.GetSpeedsForMediaType(mediaType);
            Assert.Equal(maxExpected, actual[actual.Count - 1]);
        }
    }
}
