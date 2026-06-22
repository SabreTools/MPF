using System.Collections.Generic;
using SabreTools.RedumpLib.Data;
using SabreTools.RedumpLib.Data.Sections;
using SabreTools.RedumpLib.Tools;
using Xunit;

namespace MPF.Frontend.Test.Tools
{
    public class InfoToolTests
    {
        [Fact]
        public void ProcessSpecialFields_Complete()
        {
            // Create a new SubmissionInfo object
            var info = new SubmissionInfo()
            {
                DumpMetadata = new DumpMetadataSection()
                {
                    Comments = "This is a comments line\n[T:ISBN] ISBN Value",
                    CommentsSpecialFields = new Dictionary<SiteCode, string>()
                    {
                        [SiteCode.VolumeLabel] = "VOLUME_LABEL",
                    },

                    Contents = "This is a contents line\n[T:GF] Game Footage",
                    ContentsSpecialFields = new Dictionary<SiteCode, string>()
                    {
                        [SiteCode.Patches] = "1.04 patch",
                    },
                }
            };

            // Process the special fields
            Formatter.ProcessSpecialFields(info);

            // Validate the basics
            Assert.NotNull(info.DumpMetadata.Comments);
            Assert.Empty(info.DumpMetadata.CommentsSpecialFields);
            Assert.NotNull(info.DumpMetadata.Contents);
            Assert.Empty(info.DumpMetadata.ContentsSpecialFields);

            // Split the values
            string[] splitComments = info.DumpMetadata.Comments.Split('\n');
            string[] splitContents = info.DumpMetadata.Contents.Split('\n');

            // Validate the lines
            Assert.Equal(3, splitComments.Length);
            Assert.Equal(4, splitContents.Length);
        }

        [Fact]
        public void ProcessSpecialFields_NullStrings()
        {
            // Create a new SubmissionInfo object
            var info = new SubmissionInfo()
            {
                DumpMetadata = new DumpMetadataSection()
                {
                    Comments = null,
                    CommentsSpecialFields = new Dictionary<SiteCode, string>()
                    {
                        [SiteCode.VolumeLabel] = "VOLUME_LABEL",
                    },

                    Contents = null,
                    ContentsSpecialFields = new Dictionary<SiteCode, string>()
                    {
                        [SiteCode.Patches] = "1.04 patch",
                    },
                }
            };

            // Process the special fields
            Formatter.ProcessSpecialFields(info);

            // Validate the basics
            Assert.NotNull(info.DumpMetadata.Comments);
            Assert.Empty(info.DumpMetadata.CommentsSpecialFields);
            Assert.NotNull(info.DumpMetadata.Contents);
            Assert.Empty(info.DumpMetadata.ContentsSpecialFields);

            // Split the values
            string[] splitComments = info.DumpMetadata.Comments.Split('\n');
            string[] splitContents = info.DumpMetadata.Contents.Split('\n');

            // Validate the lines
            Assert.Single(splitComments);
            Assert.Equal(2, splitContents.Length);
        }

        [Fact]
        public void ProcessSpecialFields_EmptyDictionaries()
        {
            // Create a new SubmissionInfo object
            var info = new SubmissionInfo()
            {
                DumpMetadata = new DumpMetadataSection()
                {
                    Comments = "This is a comments line\n[T:ISBN] ISBN Value",
                    CommentsSpecialFields = [],

                    Contents = "This is a contents line\n[T:GF] Game Footage",
                    ContentsSpecialFields = [],
                }
            };

            // Process the special fields
            Formatter.ProcessSpecialFields(info);

            // Validate the basics
            Assert.NotNull(info.DumpMetadata.Comments);
            Assert.Empty(info.DumpMetadata.CommentsSpecialFields);
            Assert.NotNull(info.DumpMetadata.Contents);
            Assert.Empty(info.DumpMetadata.ContentsSpecialFields);

            // Split the values
            string[] splitComments = info.DumpMetadata.Comments.Split('\n');
            string[] splitContents = info.DumpMetadata.Contents.Split('\n');

            // Validate the lines
            Assert.Equal(2, splitComments.Length);
            Assert.Equal(2, splitContents.Length);
        }
    }
}
