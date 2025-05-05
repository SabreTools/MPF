using System;
using System.IO;
using SabreTools.Models.Logiqx;
using SabreTools.RedumpLib.Data;
using Xunit;

namespace MPF.Processors.Test
{
    public class PS3CFWTests
    {
        #region GetOutputFiles

        [Fact]
        public void GetOutputFiles_Null_Empty()
        {
            string? outputDirectory = null;
            string outputFilename = "test";
            var processor = new PS3CFW(RedumpSystem.SonyPlayStation3, null);

            var actual = processor.GetOutputFiles(outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetOutputFiles_BluRay_Populated()
        {
            string? outputDirectory = null;
            string outputFilename = "test";
            var processor = new PS3CFW(RedumpSystem.SonyPlayStation3, MediaType.BluRay);

            var actual = processor.GetOutputFiles(outputDirectory, outputFilename);
            Assert.Equal(4, actual.Count);
        }

        [Fact]
        public void GetOutputFiles_Other_Empty()
        {
            string? outputDirectory = null;
            string outputFilename = "test";
            var processor = new PS3CFW(RedumpSystem.SonyPlayStation3, MediaType.ApertureCard);

            var actual = processor.GetOutputFiles(outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        #endregion

        #region FoundAllFiles

        [Fact]
        public void FoundAllFiles_Invalid_Filled()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new PS3CFW(RedumpSystem.SonyPlayStation3, MediaType.BluRay);
            var actual = processor.FoundAllFiles(outputDirectory, outputFilename);
            Assert.Equal(3, actual.Count);
        }

        [Fact]
        public void FoundAllFiles_Valid_Empty()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "PS3CFW", "BluRay");
            string outputFilename = "test.iso";
            var processor = new PS3CFW(RedumpSystem.SonyPlayStation3, MediaType.BluRay);
            var actual = processor.FoundAllFiles(outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        #endregion

        #region FoundAnyFiles

        [Fact]
        public void FoundAnyFiles_Invalid_Filled()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new PS3CFW(RedumpSystem.SonyPlayStation3, MediaType.BluRay);
            var actual = processor.FoundAnyFiles(outputDirectory, outputFilename);
            Assert.False(actual);
        }

        [Fact]
        public void FoundAnyFiles_Valid_Empty()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "PS3CFW", "BluRay");
            string outputFilename = "test.iso";
            var processor = new PS3CFW(RedumpSystem.SonyPlayStation3, MediaType.BluRay);
            var actual = processor.FoundAnyFiles(outputDirectory, outputFilename);
            Assert.True(actual);
        }

        [Fact]
        public void FoundAnyFiles_ValidZip_Empty()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "PS3CFW", "BluRay-zip");
            string outputFilename = "test.iso";
            var processor = new PS3CFW(RedumpSystem.SonyPlayStation3, MediaType.BluRay);
            var actual = processor.FoundAnyFiles(outputDirectory, outputFilename);
            Assert.True(actual);
        }

        #endregion

        #region GenerateArtifacts

        [Fact]
        public void GenerateArtifacts_Invalid_Empty()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new PS3CFW(RedumpSystem.SonyPlayStation3, MediaType.BluRay);
            var actual = processor.GenerateArtifacts(outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void GenerateArtifacts_Valid_Filled()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "PS3CFW", "BluRay");
            string outputFilename = "test.iso";
            var processor = new PS3CFW(RedumpSystem.SonyPlayStation3, MediaType.BluRay);
            var actual = processor.GenerateArtifacts(outputDirectory, outputFilename);
            Assert.Equal(2, actual.Count);
        }

        #endregion

        #region GetDeleteableFilePaths

        [Fact]
        public void GetDeleteableFilePaths_Invalid_Empty()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new PS3CFW(RedumpSystem.SonyPlayStation3, MediaType.BluRay);
            var actual = processor.GetDeleteableFilePaths(outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetDeleteableFilePaths_Valid_Empty()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "PS3CFW", "BluRay");
            string outputFilename = "test.iso";
            var processor = new PS3CFW(RedumpSystem.SonyPlayStation3, MediaType.BluRay);
            var actual = processor.GetDeleteableFilePaths(outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        #endregion

        #region GetZippableFilePaths

        [Fact]
        public void GetZippableFilePaths_Invalid_Empty()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new PS3CFW(RedumpSystem.SonyPlayStation3, MediaType.BluRay);
            var actual = processor.GetZippableFilePaths(outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetZippableFilePaths_Valid_Filled()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "PS3CFW", "BluRay");
            string outputFilename = "test.iso";
            var processor = new PS3CFW(RedumpSystem.SonyPlayStation3, MediaType.BluRay);
            var actual = processor.GetZippableFilePaths(outputDirectory, outputFilename);
            Assert.Equal(2, actual.Count);
        }

        #endregion

        #region GeneratePS3CFWDatafile

        [Fact]
        public void GeneratePS3CFWDatafile_Empty_Null()
        {
            string iso = string.Empty;
            Datafile? actual = PS3CFW.GeneratePS3CFWDatafile(iso);
            Assert.Null(actual);
        }

        [Fact]
        public void GeneratePS3CFWDatafile_Invalid_Null()
        {
            string iso = "INVALID";
            Datafile? actual = PS3CFW.GeneratePS3CFWDatafile(iso);
            Assert.Null(actual);
        }

        [Fact]
        public void GeneratePS3CFWDatafile_Valid_Filled()
        {
            string iso = Path.Combine(Environment.CurrentDirectory, "TestData", "PS3CFW", "BluRay", "test.iso");
            var actual = PS3CFW.GeneratePS3CFWDatafile(iso);

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
    }
}