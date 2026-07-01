using System.Collections.Generic;
using MPF.Frontend.Tools;
using Xunit;

namespace MPF.Frontend.Test.Tools
{
    public class SubmissionGeneratorTests
    {
        #region FormatVolumeLabels

        [Fact]
        public void FormatVolumeLabels_BothNull_Null()
        {
            string? driveLabel = null;
            Dictionary<string, List<string>>? labels = null;

            string? actual = SubmissionGenerator.FormatVolumeLabels(driveLabel, labels);
            Assert.Null(actual);
        }

        [Fact]
        public void FormatVolumeLabels_EmptyLabelNullDict_Null()
        {
            string? driveLabel = string.Empty;
            Dictionary<string, List<string>>? labels = null;

            string? actual = SubmissionGenerator.FormatVolumeLabels(driveLabel, labels);
            Assert.Null(actual);
        }

        [Fact]
        public void FormatVolumeLabels_EmptyLabelEmptyDict_Null()
        {
            string? driveLabel = string.Empty;
            Dictionary<string, List<string>>? labels = [];

            string? actual = SubmissionGenerator.FormatVolumeLabels(driveLabel, labels);
            Assert.Null(actual);
        }

        [Theory]
        [InlineData("path/label")]
        [InlineData("path\\label")]
        public void FormatVolumeLabels_PathLabelNullDict_Null(string driveLabel)
        {
            Dictionary<string, List<string>>? labels = null;

            string? actual = SubmissionGenerator.FormatVolumeLabels(driveLabel, labels);
            Assert.Null(actual);
        }

        [Theory]
        [InlineData("path/label")]
        [InlineData("path\\label")]
        public void FormatVolumeLabels_PathLabelEmptyDict_Null(string driveLabel)
        {
            Dictionary<string, List<string>>? labels = [];

            string? actual = SubmissionGenerator.FormatVolumeLabels(driveLabel, labels);
            Assert.Null(actual);
        }

        [Fact]
        public void FormatVolumeLabels_SystemLabelNullDict_Null()
        {
            string? driveLabel = "PS3VOLUME";
            Dictionary<string, List<string>>? labels = null;

            string? actual = SubmissionGenerator.FormatVolumeLabels(driveLabel, labels);
            Assert.Null(actual);
        }

        [Fact]
        public void FormatVolumeLabels_SystemLabelEmptyDict_Null()
        {
            string? driveLabel = "PS3VOLUME";
            Dictionary<string, List<string>>? labels = [];

            string? actual = SubmissionGenerator.FormatVolumeLabels(driveLabel, labels);
            Assert.Null(actual);
        }

        [Theory]
        [InlineData("DVD_ROM")]
        [InlineData("CD_ROM")]
        public void FormatVolumeLabels_GenericLabelNullDict_Null(string driveLabel)
        {
            Dictionary<string, List<string>>? labels = null;

            string? actual = SubmissionGenerator.FormatVolumeLabels(driveLabel, labels);
            Assert.Null(actual);
        }

        [Theory]
        [InlineData("DVD_ROM")]
        [InlineData("CD_ROM")]
        public void FormatVolumeLabels_GenericLabelEmptyDict_Null(string driveLabel)
        {
            Dictionary<string, List<string>>? labels = [];

            string? actual = SubmissionGenerator.FormatVolumeLabels(driveLabel, labels);
            Assert.Null(actual);
        }

        [Fact]
        public void FormatVolumeLabels_ValidLabelNullDict_Valid()
        {
            string? driveLabel = "MyLabel";
            Dictionary<string, List<string>>? labels = null;

            string? actual = SubmissionGenerator.FormatVolumeLabels(driveLabel, labels);
            Assert.Equal("MyLabel", actual);
        }

        [Fact]
        public void FormatVolumeLabels_ValidLabelEmptyDict_Valid()
        {
            string? driveLabel = "MyLabel";
            Dictionary<string, List<string>>? labels = [];

            string? actual = SubmissionGenerator.FormatVolumeLabels(driveLabel, labels);
            Assert.Equal("MyLabel", actual);
        }

        // TODO: Write tests about default label generation

        [Fact]
        public void FormatVolumeLabels_ValidLabelInvalidDictEntries_Valid()
        {
            string? driveLabel = "MyLabel";
            Dictionary<string, List<string>>? labels = [];
            labels["DVD_ROM"] = ["ISO"];
            labels["CD_ROM"] = ["ISO"];

            string? actual = SubmissionGenerator.FormatVolumeLabels(driveLabel, labels);
            Assert.Equal("MyLabel", actual);
        }

        [Fact]
        public void FormatVolumeLabels_ValidLabelDuplicateSystemDictEntries_Valid()
        {
            string? driveLabel = "PS3VOLUME";
            Dictionary<string, List<string>>? labels = [];
            labels["PS3VOLUME"] = ["ISO"];

            string? actual = SubmissionGenerator.FormatVolumeLabels(driveLabel, labels);
            Assert.Null(actual);
        }

        [Theory]
        [InlineData("DVD_ROM")]
        [InlineData("CD_ROM")]
        public void FormatVolumeLabels_ValidLabelDuplicateInvalidDictEntries_Valid(string driveLabel)
        {
            Dictionary<string, List<string>>? labels = [];
            labels[driveLabel] = ["ISO"];

            string? actual = SubmissionGenerator.FormatVolumeLabels(driveLabel, labels);
            Assert.Null(actual);
        }

        [Fact]
        public void FormatVolumeLabels_ValidLabelDuplicateValidDictEntries_Valid()
        {
            string? driveLabel = "MyLabel";
            Dictionary<string, List<string>>? labels = [];
            labels["MyLabel"] = ["ISO"];

            string? actual = SubmissionGenerator.FormatVolumeLabels(driveLabel, labels);
            Assert.Equal("MyLabel", actual);
        }

        [Fact]
        public void FormatVolumeLabels_ValidLabelSystemDictEntries_Valid()
        {
            string? driveLabel = "MyLabel";
            Dictionary<string, List<string>>? labels = [];
            labels["PS3VOLUME"] = ["ISO"];

            string? actual = SubmissionGenerator.FormatVolumeLabels(driveLabel, labels);
            Assert.Equal("MyLabel", actual);
        }

        // TODO: Write tests about multiple label output generation

        #endregion
    }
}
