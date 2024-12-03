using System;
using System.IO;
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
        public void GetGeneratedFilePaths_NullBaseDirectory_Empty()
        {
            string? baseDirectory = null;
            var actual = BaseProcessor.GetGeneratedFilePaths(baseDirectory, filenameSuffix: null);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetGeneratedFilePaths_EmptyBaseDirectory_Empty()
        {
            string? baseDirectory = string.Empty;
            var actual = BaseProcessor.GetGeneratedFilePaths(baseDirectory, filenameSuffix: null);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetGeneratedFilePaths_InvalidBaseDirectory_Empty()
        {
            string? baseDirectory = "INVALID";
            var actual = BaseProcessor.GetGeneratedFilePaths(baseDirectory, filenameSuffix: null);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetGeneratedFilePaths_ValidBaseDirectory_Empty()
        {
            string? baseDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "BaseProcessor");
            var actual = BaseProcessor.GetGeneratedFilePaths(baseDirectory, filenameSuffix: null);
            Assert.Equal(4, actual.Count);
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