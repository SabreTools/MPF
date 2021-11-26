using MPF.Library;
using RedumpLib.Data;
using Xunit;

namespace MPF.Test.Library
{
    public class InfoToolTests
    {
        [Theory]
        [InlineData(null, 0, 0, 0, 0, null)]
        [InlineData(null, 12345, 0, 0, 0, null)]
        [InlineData(null, 12345, 1, 0, 0, null)]
        [InlineData(null, 12345, 1, 2, 0, null)]
        [InlineData(null, 12345, 1, 2, 3, null)]
        [InlineData(MediaType.CDROM, 0, 0, 0, 0, "CD-ROM")]
        [InlineData(MediaType.CDROM, 12345, 0, 0, 0, "CD-ROM")]
        [InlineData(MediaType.CDROM, 12345, 1, 0, 0, "CD-ROM")]
        [InlineData(MediaType.CDROM, 12345, 1, 2, 0, "CD-ROM")]
        [InlineData(MediaType.CDROM, 12345, 1, 2, 3, "CD-ROM")]
        [InlineData(MediaType.DVD, 0, 0, 0, 0, "DVD-ROM-5")]
        [InlineData(MediaType.DVD, 12345, 0, 0, 0, "DVD-ROM-5")]
        [InlineData(MediaType.DVD, 12345, 1, 0, 0, "DVD-ROM-9")]
        [InlineData(MediaType.DVD, 12345, 1, 2, 0, "DVD-ROM-9")]
        [InlineData(MediaType.DVD, 12345, 1, 2, 3, "DVD-ROM-9")]
        [InlineData(MediaType.BluRay, 0, 0, 0, 0, "BD-ROM-25")]
        [InlineData(MediaType.BluRay, 12345, 0, 0, 0, "BD-ROM-25")]
        [InlineData(MediaType.BluRay, 26_843_531_857, 0, 0, 0, "BD-ROM-33")]
        [InlineData(MediaType.BluRay, 12345, 1, 0, 0, "BD-ROM-50")]
        [InlineData(MediaType.BluRay, 53_687_063_713, 1, 0, 0, "BD-ROM-66")]
        [InlineData(MediaType.BluRay, 12345, 1, 2, 0, "BD-ROM-100")]
        [InlineData(MediaType.BluRay, 12345, 1, 2, 3, "BD-ROM-128")]
        [InlineData(MediaType.UMD, 0, 0, 0, 0, "UMD-SL")]
        [InlineData(MediaType.UMD, 12345, 0, 0, 0, "UMD-SL")]
        [InlineData(MediaType.UMD, 12345, 1, 0, 0, "UMD-DL")]
        [InlineData(MediaType.UMD, 12345, 1, 2, 0, "UMD-DL")]
        [InlineData(MediaType.UMD, 12345, 1, 2, 3, "UMD-DL")]
        public void GetFixedMediaTypeTest(
            MediaType? mediaType,
            long size,
            long layerbreak,
            long layerbreak2,
            long layerbreak3,
            string expected)
        {
            string actual = InfoTool.GetFixedMediaType(mediaType, size, layerbreak, layerbreak2, layerbreak3);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null, null, null, null)]
        [InlineData(" ", "", " ", "")]
        [InlineData("super", "blah.bin", "super", "blah.bin")]
        [InlineData("super\\hero", "blah.bin", "super\\hero", "blah.bin")]
        [InlineData("super.hero", "blah.bin", "super.hero", "blah.bin")]
        [InlineData("superhero", "blah.rev.bin", "superhero", "blah.rev.bin")]
        [InlineData("super&hero", "blah.bin", "super&hero", "blah.bin")]
        [InlineData("superhero", "blah&foo.bin", "superhero", "blah&foo.bin")]
        public void NormalizeOutputPathsTest(string outputDirectory, string outputFilename, string expectedOutputDirectory, string expectedOutputFilename)
        {
            (string actualOutputDirectory, string actualOutputFilename) = InfoTool.NormalizeOutputPaths(outputDirectory, outputFilename);
            Assert.Equal(expectedOutputDirectory, actualOutputDirectory);
            Assert.Equal(expectedOutputFilename, actualOutputFilename);
        }
    }
}
