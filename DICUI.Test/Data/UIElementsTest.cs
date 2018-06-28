using System.Linq;
using DICUI.Data;
using Xunit;

namespace DICUI.Test.Data
{
    public class UIElementsTest
    {
        [Theory]
        [InlineData(MediaType.CD, 72)]
        [InlineData(MediaType.DVD, 24)]
        [InlineData(MediaType.BluRay, 16)]
        [InlineData(MediaType.LaserDisc, 72)] // TODO: Update when fully determined
        [InlineData(null, 72)] // TODO: Update when fully determined
        public void GetAllowedDriveSpeedForMediaTypeTest(MediaType? mediaType, int maxExpected)
        {
            var actual = UIElements.GetAllowedDriveSpeedsForMediaType(mediaType);
            Assert.Equal(maxExpected, actual.Last());
        }
    }
}
