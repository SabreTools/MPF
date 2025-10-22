using System.Collections.Generic;
using SabreTools.RedumpLib;
using SabreTools.RedumpLib.Data;
using SabreTools.RedumpLib.Data.Sections;
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
                CommonDiscInfo = new CommonDiscInfoSection()
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
            Assert.NotNull(info.CommonDiscInfo.Comments);
            Assert.Empty(info.CommonDiscInfo.CommentsSpecialFields);
            Assert.NotNull(info.CommonDiscInfo.Contents);
            Assert.Empty(info.CommonDiscInfo.ContentsSpecialFields);

            // Split the values
            string[] splitComments = info.CommonDiscInfo.Comments.Split('\n');
            string[] splitContents = info.CommonDiscInfo.Contents.Split('\n');

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
                CommonDiscInfo = new CommonDiscInfoSection()
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
            Assert.NotNull(info.CommonDiscInfo.Comments);
            Assert.Empty(info.CommonDiscInfo.CommentsSpecialFields);
            Assert.NotNull(info.CommonDiscInfo.Contents);
            Assert.Empty(info.CommonDiscInfo.ContentsSpecialFields);

            // Split the values
            string[] splitComments = info.CommonDiscInfo.Comments.Split('\n');
            string[] splitContents = info.CommonDiscInfo.Contents.Split('\n');

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
                CommonDiscInfo = new CommonDiscInfoSection()
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
            Assert.NotNull(info.CommonDiscInfo.Comments);
            Assert.Empty(info.CommonDiscInfo.CommentsSpecialFields);
            Assert.NotNull(info.CommonDiscInfo.Contents);
            Assert.Empty(info.CommonDiscInfo.ContentsSpecialFields);

            // Split the values
            string[] splitComments = info.CommonDiscInfo.Comments.Split('\n');
            string[] splitContents = info.CommonDiscInfo.Contents.Split('\n');

            // Validate the lines
            Assert.Equal(2, splitComments.Length);
            Assert.Equal(2, splitContents.Length);
        }
    }
}
