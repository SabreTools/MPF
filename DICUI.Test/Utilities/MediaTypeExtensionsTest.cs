using DICUI.Data;
using DICUI.Utilities;
using Xunit;

namespace DICUI.Test.Utilities
{
    public class MediaTypeExtensionsTest
    {
        [Theory]
        [InlineData(MediaType.CDROM, "CD-ROM")]
        [InlineData(MediaType.LaserDisc, "LD-ROM / LV-ROM")]
        [InlineData(MediaType.NONE, "Unknown")]
        public void NameTest(MediaType? mediaType, string expected)
        {
            string actual = mediaType.LongName();

            Assert.NotNull(actual);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(MediaType.CDROM, ".bin")]
        [InlineData(MediaType.DVD, ".iso")]
        [InlineData(MediaType.LaserDisc, ".raw")]
        [InlineData(MediaType.FloppyDisk, ".img")]
        [InlineData(MediaType.NONE, null)]
        public void ExtensionTest(MediaType? mediaType, string expected)
        {
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
