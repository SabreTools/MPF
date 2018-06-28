using DICUI.Data;
using DICUI.Utilities;
using Xunit;

namespace DICUI.Test.Utilities
{
    public class DumpInformationTest
    {
        [Fact]
        public void GetFirstTrackTest()
        {
            // TODO: Implement
            Assert.True(true);
        }

        [Theory]
        [InlineData(MediaType.CD)]
        [InlineData(MediaType.DVD)]
        [InlineData(MediaType.Floppy)]
        [InlineData(MediaType.LaserDisc)]
        public void FoundAllFilesTest(MediaType? mediaTyp)
        {
            // TODO: Implement
            Assert.True(true);
        }

        [Theory]
        [InlineData(KnownSystem.AppleMacintosh, MediaType.CD)]
        [InlineData(KnownSystem.PhilipsCDi, MediaType.CD)]
        [InlineData(KnownSystem.SegaSaturn, MediaType.CD)]
        [InlineData(KnownSystem.SonyPlayStation, MediaType.CD)]
        [InlineData(KnownSystem.SonyPlayStation2, MediaType.CD)]
        [InlineData(KnownSystem.AppleMacintosh, MediaType.DVD)]
        [InlineData(KnownSystem.DVDVideo, MediaType.DVD)]
        [InlineData(KnownSystem.MicrosoftXBOX, MediaType.DVD)]
        [InlineData(KnownSystem.SonyPlayStation2, MediaType.DVD)]
        [InlineData(KnownSystem.BDVideo, MediaType.BluRay)]
        [InlineData(KnownSystem.AUSCOMSystem1, MediaType.Cassette)]
        public void ExtractOutputInformation(KnownSystem? knownSystem, MediaType? mediaType)
        {
            // TODO: Implement
            Assert.True(true);
        }

        [Fact]
        public void FormatOutputDataTest()
        {
            // TODO: Implement
            Assert.True(true);
        }

        [Fact]
        public void WriteOutputDataTest()
        {
            // TODO: Implement
            Assert.True(true);
        }
    }
}
