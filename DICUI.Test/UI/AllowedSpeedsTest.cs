using System.Linq;
using DICUI.Data;
using DICUI.UI;
using Xunit;

namespace DICUI.Test.Data
{
    public class UIElementsTest
    {
        [Theory]
        [InlineData(MediaType.CDROM, 72)]
        [InlineData(MediaType.DVD, 24)]
        [InlineData(MediaType.BluRay, 16)]
        [InlineData(MediaType.LaserDisc, 72)] // TODO: Update when fully determined
        [InlineData(null, 72)] // TODO: Update when fully determined
        public void GetAllowedDriveSpeedForMediaTypeTest(MediaType? mediaType, int maxExpected)
        {
            var actual = AllowedSpeeds.GetForMediaType(mediaType);
            Assert.Equal(maxExpected, actual.Last());
        }
    }
}
