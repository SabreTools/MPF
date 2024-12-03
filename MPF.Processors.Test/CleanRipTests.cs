using System;
using System.IO;
using SabreTools.RedumpLib.Data;
using Xunit;

namespace MPF.Processors.Test
{
    public class CleanRipTests
    {
        #region GetOutputFiles

        [Fact]
        public void GetOutputFiles_Null_Empty()
        {
            string? baseDirectory = null;
            string baseFilename = "test";
            var processor = new CleanRip(RedumpSystem.NintendoGameCube, null);

            var actual = processor.GetOutputFiles(baseDirectory, baseFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetOutputFiles_DVD_Populated()
        {
            string? baseDirectory = null;
            string baseFilename = "test";
            var processor = new CleanRip(RedumpSystem.NintendoGameCube, MediaType.DVD);

            var actual = processor.GetOutputFiles(baseDirectory, baseFilename);
            Assert.Equal(3, actual.Count);
        }

        [Fact]
        public void GetOutputFiles_NintendoGameCubeGameDisc_Populated()
        {
            string? baseDirectory = null;
            string baseFilename = "test";
            var processor = new CleanRip(RedumpSystem.NintendoGameCube, MediaType.NintendoGameCubeGameDisc);

            var actual = processor.GetOutputFiles(baseDirectory, baseFilename);
            Assert.Equal(3, actual.Count);
        }

        [Fact]
        public void GetOutputFiles_NintendoWiiOpticalDisc_Populated()
        {
            string? baseDirectory = null;
            string baseFilename = "test";
            var processor = new CleanRip(RedumpSystem.NintendoGameCube, MediaType.NintendoWiiOpticalDisc);

            var actual = processor.GetOutputFiles(baseDirectory, baseFilename);
            Assert.Equal(3, actual.Count);
        }

        [Fact]
        public void GetOutputFiles_Other_Empty()
        {
            string? baseDirectory = null;
            string baseFilename = "test";
            var processor = new CleanRip(RedumpSystem.NintendoGameCube, MediaType.ApertureCard);

            var actual = processor.GetOutputFiles(baseDirectory, baseFilename);
            Assert.Empty(actual);
        }

        #endregion

        #region GenerateArtifacts

        [Fact]
        public void GenerateArtifacts_Invalid_Empty()
        {
            string basePath = string.Empty;
            var processor = new CleanRip(RedumpSystem.NintendoGameCube, MediaType.DVD);
            var actual = processor.GenerateArtifacts(basePath);
            Assert.Empty(actual);
        }

        [Fact]
        public void GenerateArtifacts_Valid_Filled()
        {
            string? basePath = Path.Combine(Environment.CurrentDirectory, "TestData", "CleanRip", "DVD", "test");
            var processor = new CleanRip(RedumpSystem.NintendoGameCube, MediaType.DVD);
            var actual = processor.GenerateArtifacts(basePath);
            Assert.Equal(2, actual.Count);
        }

        #endregion

        #region CheckRequiredFiles

        [Fact]
        public void CheckRequiredFiles_Invalid_Filled()
        {
            string basePath = string.Empty;
            var processor = new CleanRip(RedumpSystem.NintendoGameCube, MediaType.DVD);
            var actual = processor.CheckRequiredFiles(basePath);
            Assert.Equal(3, actual.Count);
        }

        [Fact]
        public void CheckRequiredFiles_Valid_Empty()
        {
            string? basePath = Path.Combine(Environment.CurrentDirectory, "TestData", "CleanRip", "DVD", "test");
            var processor = new CleanRip(RedumpSystem.NintendoGameCube, MediaType.DVD);
            var actual = processor.CheckRequiredFiles(basePath);
            Assert.Empty(actual);
        }

        #endregion

        #region GetDeleteableFilePaths

        [Fact]
        public void GetDeleteableFilePaths_Invalid_Empty()
        {
            string basePath = string.Empty;
            var processor = new CleanRip(RedumpSystem.NintendoGameCube, MediaType.DVD);
            var actual = processor.GetDeleteableFilePaths(basePath);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetDeleteableFilePaths_Valid_Empty()
        {
            string? basePath = Path.Combine(Environment.CurrentDirectory, "TestData", "CleanRip", "DVD", "test");
            var processor = new CleanRip(RedumpSystem.NintendoGameCube, MediaType.DVD);
            var actual = processor.GetDeleteableFilePaths(basePath);
            Assert.Empty(actual);
        }

        #endregion

        #region GetZippableFilePaths

        [Fact]
        public void GetZippableFilePaths_Invalid_Empty()
        {
            string basePath = string.Empty;
            var processor = new CleanRip(RedumpSystem.NintendoGameCube, MediaType.DVD);
            var actual = processor.GetZippableFilePaths(basePath);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetZippableFilePaths_Valid_Filled()
        {
            string? basePath = Path.Combine(Environment.CurrentDirectory, "TestData", "CleanRip", "DVD", "test");
            var processor = new CleanRip(RedumpSystem.NintendoGameCube, MediaType.DVD);
            var actual = processor.GetZippableFilePaths(basePath);
            Assert.Equal(2, actual.Count);
        }

        #endregion
    }
}