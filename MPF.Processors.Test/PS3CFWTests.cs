using System;
using System.IO;
using SabreTools.RedumpLib.Data;
using Xunit;

namespace MPF.Processors.Test
{
    public class PS3CFWTests
    {
        #region DetermineMediaType

        [Fact]
        public void DetermineMediaType_Null_BluRay()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new PS3CFW(RedumpSystem.SonyPlayStation3);
            var actual = processor.DetermineMediaType(outputDirectory, outputFilename);
            Assert.Equal(MediaType.BluRay, actual);
        }

        [Fact]
        public void DetermineMediaType_Invalid_BluRay()
        {
            string? outputDirectory = null;
            string outputFilename = "INVALID";
            var processor = new PS3CFW(RedumpSystem.SonyPlayStation3);
            var actual = processor.DetermineMediaType(outputDirectory, outputFilename);
            Assert.Equal(MediaType.BluRay, actual);
        }

        [Fact]
        public void DetermineMediaType_Valid_BluRay()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "PS3CFW", "BluRay");
            string outputFilename = "test";
            var processor = new PS3CFW(RedumpSystem.SonyPlayStation3);
            var actual = processor.DetermineMediaType(outputDirectory, outputFilename);
            Assert.Equal(MediaType.BluRay, actual);
        }

        #endregion

        #region GetOutputFiles

        [Fact]
        public void GetOutputFiles_Null_Populated()
        {
            string? outputDirectory = null;
            string outputFilename = "test";
            var processor = new PS3CFW(RedumpSystem.SonyPlayStation3);

            var actual = processor.GetOutputFiles(null, outputDirectory, outputFilename);
            Assert.Equal(4, actual.Count);
        }

        [Fact]
        public void GetOutputFiles_BluRay_Populated()
        {
            string? outputDirectory = null;
            string outputFilename = "test";
            var processor = new PS3CFW(RedumpSystem.SonyPlayStation3);

            var actual = processor.GetOutputFiles(MediaType.BluRay, outputDirectory, outputFilename);
            Assert.Equal(4, actual.Count);
        }

        [Fact]
        public void GetOutputFiles_Other_Populated()
        {
            string? outputDirectory = null;
            string outputFilename = "test";
            var processor = new PS3CFW(RedumpSystem.SonyPlayStation3);

            var actual = processor.GetOutputFiles(MediaType.ApertureCard, outputDirectory, outputFilename);
            Assert.Equal(4, actual.Count);
        }

        #endregion

        #region FoundAllFiles

        [Fact]
        public void FoundAllFiles_Invalid_Filled()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new PS3CFW(RedumpSystem.SonyPlayStation3);
            var actual = processor.FoundAllFiles(MediaType.BluRay, outputDirectory, outputFilename);
            Assert.Equal(3, actual.Count);
        }

        [Fact]
        public void FoundAllFiles_Valid_Empty()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "PS3CFW", "BluRay");
            string outputFilename = "test.iso";
            var processor = new PS3CFW(RedumpSystem.SonyPlayStation3);
            var actual = processor.FoundAllFiles(MediaType.BluRay, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        #endregion

        #region FoundAnyFiles

        [Fact]
        public void FoundAnyFiles_Invalid_Filled()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new PS3CFW(RedumpSystem.SonyPlayStation3);
            var actual = processor.FoundAnyFiles(MediaType.BluRay, outputDirectory, outputFilename);
            Assert.False(actual);
        }

        [Fact]
        public void FoundAnyFiles_Valid_Empty()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "PS3CFW", "BluRay");
            string outputFilename = "test.iso";
            var processor = new PS3CFW(RedumpSystem.SonyPlayStation3);
            var actual = processor.FoundAnyFiles(MediaType.BluRay, outputDirectory, outputFilename);
            Assert.True(actual);
        }

        [Fact]
        public void FoundAnyFiles_ValidZip_Empty()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "PS3CFW", "BluRay-zip");
            string outputFilename = "test.iso";
            var processor = new PS3CFW(RedumpSystem.SonyPlayStation3);
            var actual = processor.FoundAnyFiles(MediaType.BluRay, outputDirectory, outputFilename);
            Assert.True(actual);
        }

        #endregion

        #region GenerateArtifacts

        [Fact]
        public void GenerateArtifacts_Invalid_Empty()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new PS3CFW(RedumpSystem.SonyPlayStation3);
            var actual = processor.GenerateArtifacts(MediaType.BluRay, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void GenerateArtifacts_Valid_Filled()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "PS3CFW", "BluRay");
            string outputFilename = "test.iso";
            var processor = new PS3CFW(RedumpSystem.SonyPlayStation3);
            var actual = processor.GenerateArtifacts(MediaType.BluRay, outputDirectory, outputFilename);
            Assert.Equal(2, actual.Count);
        }

        #endregion

        #region GetDeleteableFilePaths

        [Fact]
        public void GetDeleteableFilePaths_Invalid_Empty()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new PS3CFW(RedumpSystem.SonyPlayStation3);
            var actual = processor.GetDeleteableFilePaths(MediaType.BluRay, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetDeleteableFilePaths_Valid_Empty()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "PS3CFW", "BluRay");
            string outputFilename = "test.iso";
            var processor = new PS3CFW(RedumpSystem.SonyPlayStation3);
            var actual = processor.GetDeleteableFilePaths(MediaType.BluRay, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        #endregion

        #region GetZippableFilePaths

        [Fact]
        public void GetZippableFilePaths_Invalid_Empty()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new PS3CFW(RedumpSystem.SonyPlayStation3);
            var actual = processor.GetZippableFilePaths(MediaType.BluRay, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetZippableFilePaths_Valid_Filled()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "PS3CFW", "BluRay");
            string outputFilename = "test.iso";
            var processor = new PS3CFW(RedumpSystem.SonyPlayStation3);
            var actual = processor.GetZippableFilePaths(MediaType.BluRay, outputDirectory, outputFilename);
            Assert.Equal(2, actual.Count);
        }

        #endregion
    }
}