using System;
using System.IO;
using SabreTools.Data.Models.Logiqx;
using Xunit;

namespace MPF.Processors.Test
{
    public class BaseProcessorTests
    {
        #region GetGeneratedFilenames

        [Fact]
        public void GetGeneratedFilenames_NullSuffix_Standard()
        {
            string? filenameSuffix = null;
            var actual = BaseProcessor.GetGeneratedFilenames(filenameSuffix);

            Assert.Equal(4, actual.Count);
            Assert.Equal("!protectionInfo.txt", actual[0]);
            Assert.Equal("!submissionInfo.json", actual[1]);
            Assert.Equal("!submissionInfo.json.gz", actual[2]);
            Assert.Equal("!submissionInfo.txt", actual[3]);
        }

        [Fact]
        public void GetGeneratedFilenames_EmptySuffix_Standard()
        {
            string? filenameSuffix = string.Empty;
            var actual = BaseProcessor.GetGeneratedFilenames(filenameSuffix);

            Assert.Equal(4, actual.Count);
            Assert.Equal("!protectionInfo.txt", actual[0]);
            Assert.Equal("!submissionInfo.json", actual[1]);
            Assert.Equal("!submissionInfo.json.gz", actual[2]);
            Assert.Equal("!submissionInfo.txt", actual[3]);
        }

        [Fact]
        public void GetGeneratedFilenames_ValidSuffix_Modified()
        {
            string? filenameSuffix = "suffix";
            var actual = BaseProcessor.GetGeneratedFilenames(filenameSuffix);

            Assert.Equal(4, actual.Count);
            Assert.Equal("!protectionInfo_suffix.txt", actual[0]);
            Assert.Equal("!submissionInfo_suffix.json", actual[1]);
            Assert.Equal("!submissionInfo_suffix.json.gz", actual[2]);
            Assert.Equal("!submissionInfo_suffix.txt", actual[3]);
        }

        #endregion

        #region GetGeneratedFilePaths

        [Fact]
        public void GetGeneratedFilePaths_NulloutputDirectory_Empty()
        {
            string? outputDirectory = null;
            var actual = BaseProcessor.GetGeneratedFilePaths(outputDirectory, filenameSuffix: null);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetGeneratedFilePaths_EmptyoutputDirectory_Empty()
        {
            string? outputDirectory = string.Empty;
            var actual = BaseProcessor.GetGeneratedFilePaths(outputDirectory, filenameSuffix: null);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetGeneratedFilePaths_InvalidoutputDirectory_Empty()
        {
            string? outputDirectory = "INVALID";
            var actual = BaseProcessor.GetGeneratedFilePaths(outputDirectory, filenameSuffix: null);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetGeneratedFilePaths_ValidoutputDirectory_Empty()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "BaseProcessor");
            var actual = BaseProcessor.GetGeneratedFilePaths(outputDirectory, filenameSuffix: null);
            Assert.Equal(4, actual.Count);
        }

        #endregion

        #region GenerateDatafile

        [Fact]
        public void GenerateDatafile_Empty_Null()
        {
            string iso = string.Empty;
            Datafile? actual = BaseProcessor.GenerateDatafile(iso);
            Assert.Null(actual);
        }

        [Fact]
        public void GenerateDatafile_Invalid_Null()
        {
            string iso = "INVALID";
            Datafile? actual = BaseProcessor.GenerateDatafile(iso);
            Assert.Null(actual);
        }

        [Fact]
        public void GenerateDatafile_Valid_Filled()
        {
            string iso = Path.Combine(Environment.CurrentDirectory, "TestData", "PS3CFW", "BluRay", "test.iso");
            var actual = BaseProcessor.GenerateDatafile(iso);

            Assert.NotNull(actual);
            Assert.NotNull(actual.Game);
            var game = Assert.Single(actual.Game);
            Assert.NotNull(game.Rom);
            var rom = Assert.Single(game.Rom);
            Assert.Equal("9", rom.Size);
            Assert.Equal("560b9f59", rom.CRC);
            Assert.Equal("edbb6676247e65c2245dd4883ed9fc24", rom.MD5);
            Assert.Equal("1b33ad54d78085be5ecb1cf1b3e9da821e708075", rom.SHA1);
        }

        #endregion

        #region GetPIC

        [Fact]
        public void GetPIC_InvalidPath_Null()
        {
            string picPath = "INVALID";
            int trimLength = -1;

            string? actual = BaseProcessor.GetPIC(picPath, trimLength);
            Assert.Null(actual);
        }

        [Fact]
        public void GetPIC_ValidPathZeroTrim_Empty()
        {
            string picPath = Path.Combine(Environment.CurrentDirectory, "TestData", "BaseProcessor", "pic.bin");
            int trimLength = 0;

            string? actual = BaseProcessor.GetPIC(picPath, trimLength);
            Assert.NotNull(actual);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetPIC_ValidPathDefaultTrim_Formatted()
        {
            string expected = "000102030405060708090A0B0C0D0E0F\n000102030405060708090A0B0C0D0E0F\n000102030405060708090A0B0C0D0E0F\n000102030405060708090A0B0C0D0E0F\n000102030405060708090A0B0C0D0E0F\n000102030405060708090A0B0C0D0E0F\n000102030405060708090A0B0C0D0E0F\n000102030405060708090A0B0C0D0E0F\n000102030405060708090A0B0C0D0E0F\n000102030405060708090A0B0C0D0E0F\n";
            string picPath = Path.Combine(Environment.CurrentDirectory, "TestData", "BaseProcessor", "pic.bin");
            int trimLength = -1;

            string? actual = BaseProcessor.GetPIC(picPath, trimLength);
            Assert.NotNull(actual);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetPIC_ValidPathCustomTrim_Formatted()
        {
            string expected = "000102030405060708090A0B0C0D0E0F\n";
            string picPath = Path.Combine(Environment.CurrentDirectory, "TestData", "BaseProcessor", "pic.bin");
            int trimLength = 32;

            string? actual = BaseProcessor.GetPIC(picPath, trimLength);
            Assert.NotNull(actual);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetPVD

        // TODO: Create fake, 00-filled ISO for tests and implement

        #endregion

        #region IsAudio

        [Fact]
        public void IsAudio_Null_False()
        {
            string? cue = null;
            bool actual = BaseProcessor.IsAudio(cue);
            Assert.False(actual);
        }

        [Fact]
        public void IsAudio_Empty_False()
        {
            string? cue = string.Empty;
            bool actual = BaseProcessor.IsAudio(cue);
            Assert.False(actual);
        }

        [Fact]
        public void IsAudio_Invalid_False()
        {
            string? cue = @"INVALID";
            bool actual = BaseProcessor.IsAudio(cue);
            Assert.False(actual);
        }

        [Fact]
        public void IsAudio_NoAudio_False()
        {
            string? cue = @"FILE ""track (Track 1).bin"" BINARY
  TRACK 01 MODE1/2352
    INDEX 01 00:00:00
FILE ""track (Track 2).bin"" BINARY
  TRACK 02 MODE1/2352
    INDEX 01 00:00:00";
            bool actual = BaseProcessor.IsAudio(cue);
            Assert.False(actual);
        }

        [Fact]
        public void IsAudio_MixedTracks_False()
        {
            string? cue = @"FILE ""track (Track 1).bin"" BINARY
  TRACK 01 MODE1/2352
    INDEX 01 00:00:00
FILE ""track (Track 2).bin"" BINARY
  TRACK 02 AUDIO
    INDEX 00 00:00:00
    INDEX 01 00:02:00";
            bool actual = BaseProcessor.IsAudio(cue);
            Assert.False(actual);
        }

        [Fact]
        public void IsAudio_AllAudio_True()
        {
            string? cue = @"FILE ""track (Track 1).bin"" BINARY
  TRACK 01 AUDIO
    INDEX 00 00:00:00
    INDEX 01 00:02:00
FILE ""track (Track 2).bin"" BINARY
  TRACK 02 AUDIO
    INDEX 00 00:00:00
    INDEX 01 00:02:00";
            bool actual = BaseProcessor.IsAudio(cue);
            Assert.True(actual);
        }

        #endregion

        #region SplitString

        [Fact]
        public void SplitString_NullString_Empty()
        {
            string? str = null;
            int count = 0;
            bool trim = false;

            string actual = BaseProcessor.SplitString(str, count, trim);
            Assert.Empty(actual);
        }

        [Fact]
        public void SplitString_EmptyString_Empty()
        {
            string? str = string.Empty;
            int count = 0;
            bool trim = false;

            string actual = BaseProcessor.SplitString(str, count, trim);
            Assert.Empty(actual);
        }

        [Fact]
        public void SplitString_ValidStringInvalidCount_Original()
        {
            string expected = "VALID\n";
            string? str = "VALID";
            int count = 0;
            bool trim = false;

            string actual = BaseProcessor.SplitString(str, count, trim);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SplitString_ValidStringLessThanCount_Original()
        {
            string expected = "VALID\n";
            string? str = "VALID";
            int count = 10;
            bool trim = false;

            string actual = BaseProcessor.SplitString(str, count, trim);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SplitString_ValidStringGreaterThanCount_Split()
        {
            string expected = "VA\nLI\nD\n";
            string? str = "VALID";
            int count = 2;
            bool trim = false;

            string actual = BaseProcessor.SplitString(str, count, trim);
            Assert.Equal(expected, actual);
        }

        #endregion
    }
}
