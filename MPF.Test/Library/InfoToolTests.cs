using System.Collections.Generic;
using System.IO;
using MPF.Core;
using SabreTools.RedumpLib.Data;
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
#if NET48
            string expected)
#else
            string? expected)
#endif
        {
            // TODO: Add tests around BDU
            var actual = InfoTool.GetFixedMediaType(mediaType, null, size, layerbreak, layerbreak2, layerbreak3);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(null, "")]
        [InlineData(" ", "")]
        [InlineData("super\\blah.bin", "super\\blah.bin")]
        [InlineData("super\\hero\\blah.bin", "super\\hero\\blah.bin")]
        [InlineData("super.hero\\blah.bin", "super.hero\\blah.bin")]
        [InlineData("superhero\\blah.rev.bin", "superhero\\blah.rev.bin")]
        [InlineData("super&hero\\blah.bin", "super&hero\\blah.bin")]
        [InlineData("superhero\\blah&foo.bin", "superhero\\blah&foo.bin")]
#if NET48
        public void NormalizeOutputPathsTest(string outputPath, string expectedPath)
#else
        public void NormalizeOutputPathsTest(string? outputPath, string? expectedPath)
#endif
        {
            if (!string.IsNullOrWhiteSpace(expectedPath))
                expectedPath = Path.GetFullPath(expectedPath);

            string actualPath = InfoTool.NormalizeOutputPaths(outputPath, true);
            Assert.Equal(expectedPath, actualPath);
        }

        [Fact]
        public void ProcessSpecialFieldsCompleteTest()
        {
            // Create a new SubmissionInfo object
            var info = new SubmissionInfo()
            {
                CommonDiscInfo = new CommonDiscInfoSection()
                {
                    Comments = "This is a comments line\n[T:ISBN] ISBN Value",
#if NET48
                    CommentsSpecialFields = new Dictionary<SiteCode?, string>()
#else
                    CommentsSpecialFields = new Dictionary<SiteCode, string>()
#endif
                    {
                        [SiteCode.VolumeLabel] = "VOLUME_LABEL",
                    },

                    Contents = "This is a contents line\n[T:GF] Game Footage",
#if NET48
                    ContentsSpecialFields = new Dictionary<SiteCode?, string>()
#else
                    ContentsSpecialFields = new Dictionary<SiteCode, string>()
#endif
                    {
                        [SiteCode.Patches] = "1.04 patch",
                    },
                }
            };

            // Process the special fields
            InfoTool.ProcessSpecialFields(info);

            // Validate the basics
            Assert.NotNull(info.CommonDiscInfo.Comments);
            Assert.Null(info.CommonDiscInfo.CommentsSpecialFields);
            Assert.NotNull(info.CommonDiscInfo.Contents);
            Assert.Null(info.CommonDiscInfo.ContentsSpecialFields);

            // Split the values
            string[] splitComments = info.CommonDiscInfo.Comments.Split('\n');
            string[] splitContents = info.CommonDiscInfo.Contents.Split('\n');

            // Validate the lines
            Assert.Equal(3, splitComments.Length);
            Assert.Equal(5, splitContents.Length);
        }

        [Fact]
        public void ProcessSpecialFieldsNullObjectTest()
        {
            // Create a new SubmissionInfo object
            var info = new SubmissionInfo()
            {
                CommonDiscInfo = null,
            };

            // Process the special fields
            InfoTool.ProcessSpecialFields(info);

            // Validate
            Assert.Null(info.CommonDiscInfo);
        }

        [Fact]
        public void ProcessSpecialFieldsNullCommentsContentsTest()
        {
            // Create a new SubmissionInfo object
            var info = new SubmissionInfo()
            {
                CommonDiscInfo = new CommonDiscInfoSection()
                {
                    Comments = null,
#if NET48
                    CommentsSpecialFields = new Dictionary<SiteCode?, string>()
#else
                    CommentsSpecialFields = new Dictionary<SiteCode, string>()
#endif
                    {
                        [SiteCode.VolumeLabel] = "VOLUME_LABEL",
                    },

                    Contents = null,
#if NET48
                    ContentsSpecialFields = new Dictionary<SiteCode?, string>()
#else
                    ContentsSpecialFields = new Dictionary<SiteCode, string>()
#endif
                    {
                        [SiteCode.Patches] = "1.04 patch",
                    },
                }
            };

            // Process the special fields
            InfoTool.ProcessSpecialFields(info);

            // Validate the basics
            Assert.NotNull(info.CommonDiscInfo.Comments);
            Assert.Null(info.CommonDiscInfo.CommentsSpecialFields);
            Assert.NotNull(info.CommonDiscInfo.Contents);
            Assert.Null(info.CommonDiscInfo.ContentsSpecialFields);

            // Split the values
            string[] splitComments = info.CommonDiscInfo.Comments.Split('\n');
            string[] splitContents = info.CommonDiscInfo.Contents.Split('\n');

            // Validate the lines
            Assert.Single(splitComments);
            Assert.Equal(2, splitContents.Length);
        }

        [Fact]
        public void ProcessSpecialFieldsNullDictionariesTest()
        {
            // Create a new SubmissionInfo object
            var info = new SubmissionInfo()
            {
                CommonDiscInfo = new CommonDiscInfoSection()
                {
                    Comments = "This is a comments line\n[T:ISBN] ISBN Value",
                    CommentsSpecialFields = null,

                    Contents = "This is a contents line\n[T:GF] Game Footage",
                    ContentsSpecialFields = null,
                }
            };

            // Process the special fields
            InfoTool.ProcessSpecialFields(info);

            // Validate the basics
            Assert.NotNull(info.CommonDiscInfo.Comments);
            Assert.Null(info.CommonDiscInfo.CommentsSpecialFields);
            Assert.NotNull(info.CommonDiscInfo.Contents);
            Assert.Null(info.CommonDiscInfo.ContentsSpecialFields);

            // Split the values
            string[] splitComments = info.CommonDiscInfo.Comments.Split('\n');
            string[] splitContents = info.CommonDiscInfo.Contents.Split('\n');

            // Validate the lines
            Assert.Equal(2, splitComments.Length);
            Assert.Equal(2, splitContents.Length);
        }
    }
}
