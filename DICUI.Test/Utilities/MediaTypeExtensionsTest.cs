using DICUI.Data;
using DICUI.Utilities;
using Xunit;

namespace DICUI.Test.Utilities
{
    public class MediaTypeExtensionsTest
    {
        [Theory]
        [InlineData(MediaType.CDROM)]
        [InlineData(MediaType.LaserDisc)]
        [InlineData(MediaType.NONE)]
        public void NameTest(MediaType? mediaType)
        {
            string expected = Converters.MediaTypeToString(mediaType);
            string actual = mediaType.Name();

            Assert.NotNull(actual);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(MediaType.CDROM)]
        [InlineData(MediaType.DVD)]
        [InlineData(MediaType.LaserDisc)]
        [InlineData(MediaType.FloppyDisk)]
        [InlineData(MediaType.NONE)]
        public void ExtensionTest(MediaType? mediaType)
        {
            string expected = Converters.MediaTypeToExtension(mediaType);
            string actual = mediaType.Extension();

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(MediaType.CDROM, true)]
        [InlineData(MediaType.DVD, true)]
        [InlineData(MediaType.FloppyDisk, false)]
        [InlineData(MediaType.BluRay, true)]
        [InlineData(MediaType.LaserDisc, false)]
        public void DriveSpeedSupportedTest(MediaType? mediaType, bool expected)
        {
            bool actual = mediaType.DoesSupportDriveSpeed();
            Assert.Equal(expected, actual);
        }
    }
}
