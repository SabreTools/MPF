using System;
using System.IO;
using SabreTools.RedumpLib.Data;
using Xunit;

namespace MPF.Processors.Test
{
    public class XboxBackupCreatorTests
    {
        #region GetOutputFiles

        [Fact]
        public void GetOutputFiles_Null_Empty()
        {
            string? baseDirectory = null;
            string baseFilename = "test";
            var processor = new XboxBackupCreator(RedumpSystem.MicrosoftXbox, null);

            var actual = processor.GetOutputFiles(baseDirectory, baseFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetOutputFiles_DVD_Populated()
        {
            string? baseDirectory = null;
            string baseFilename = "test";
            var processor = new XboxBackupCreator(RedumpSystem.MicrosoftXbox, MediaType.DVD);

            var actual = processor.GetOutputFiles(baseDirectory, baseFilename);
            Assert.Equal(7, actual.Count);
        }

        [Fact]
        public void GetOutputFiles_Other_Empty()
        {
            string? baseDirectory = null;
            string baseFilename = "test";
            var processor = new XboxBackupCreator(RedumpSystem.MicrosoftXbox, MediaType.ApertureCard);

            var actual = processor.GetOutputFiles(baseDirectory, baseFilename);
            Assert.Empty(actual);
        }

        #endregion

        #region GenerateArtifacts

        [Fact]
        public void GenerateArtifacts_Invalid_Empty()
        {
            string basePath = string.Empty;
            var processor = new XboxBackupCreator(RedumpSystem.MicrosoftXbox, MediaType.DVD);
            var actual = processor.GenerateArtifacts(basePath);
            Assert.Empty(actual);
        }

        [Fact]
        public void GenerateArtifacts_Valid_Filled()
        {
            string? basePath = Path.Combine(Environment.CurrentDirectory, "TestData", "XboxBackupCreator", "DVD", "test");
            var processor = new XboxBackupCreator(RedumpSystem.MicrosoftXbox, MediaType.DVD);
            var actual = processor.GenerateArtifacts(basePath);
            Assert.Equal(6, actual.Count);
        }

        #endregion

        #region CheckRequiredFiles

        [Fact]
        public void CheckRequiredFiles_Invalid_Filled()
        {
            string basePath = string.Empty;
            var processor = new XboxBackupCreator(RedumpSystem.MicrosoftXbox, MediaType.DVD);
            var actual = processor.CheckRequiredFiles(basePath);
            Assert.Equal(5, actual.Count);
        }

        [Fact]
        public void CheckRequiredFiles_Valid_Empty()
        {
            string? basePath = Path.Combine(Environment.CurrentDirectory, "TestData", "XboxBackupCreator", "DVD", "test");
            var processor = new XboxBackupCreator(RedumpSystem.MicrosoftXbox, MediaType.DVD);
            var actual = processor.CheckRequiredFiles(basePath);
            Assert.Empty(actual);
        }

        #endregion

        #region GetDeleteableFilePaths

        [Fact]
        public void GetDeleteableFilePaths_Invalid_Empty()
        {
            string basePath = string.Empty;
            var processor = new XboxBackupCreator(RedumpSystem.MicrosoftXbox, MediaType.DVD);
            var actual = processor.GetDeleteableFilePaths(basePath);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetDeleteableFilePaths_Valid_Empty()
        {
            string? basePath = Path.Combine(Environment.CurrentDirectory, "TestData", "XboxBackupCreator", "DVD", "test");
            var processor = new XboxBackupCreator(RedumpSystem.MicrosoftXbox, MediaType.DVD);
            var actual = processor.GetDeleteableFilePaths(basePath);
            Assert.Empty(actual);
        }

        #endregion

        #region GetZippableFilePaths

        [Fact]
        public void GetZippableFilePaths_Invalid_Empty()
        {
            string basePath = string.Empty;
            var processor = new XboxBackupCreator(RedumpSystem.MicrosoftXbox, MediaType.DVD);
            var actual = processor.GetZippableFilePaths(basePath);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetZippableFilePaths_Valid_Filled()
        {
            string? basePath = Path.Combine(Environment.CurrentDirectory, "TestData", "XboxBackupCreator", "DVD", "test");
            var processor = new XboxBackupCreator(RedumpSystem.MicrosoftXbox, MediaType.DVD);
            var actual = processor.GetZippableFilePaths(basePath);
            Assert.Equal(6, actual.Count);
        }

        #endregion
    }
}