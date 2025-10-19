using System;
using System.IO;
using SabreTools.RedumpLib.Data;
using Xunit;

namespace MPF.Processors.Test
{
    // TODO: Add tests for RecreateSS
    public class XboxBackupCreatorTests
    {
        #region DetermineMediaType

        [Fact]
        public void DetermineMediaType_Empty_DVD()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new XboxBackupCreator(RedumpSystem.MicrosoftXbox);
            var actual = processor.DetermineMediaType(outputDirectory, outputFilename);
            Assert.Equal(MediaType.DVD, actual);
        }

        [Fact]
        public void DetermineMediaType_Invalid_DVD()
        {
            string? outputDirectory = null;
            string outputFilename = "INVALID";
            var processor = new XboxBackupCreator(RedumpSystem.MicrosoftXbox);
            var actual = processor.DetermineMediaType(outputDirectory, outputFilename);
            Assert.Equal(MediaType.DVD, actual);
        }

        [Fact]
        public void DetermineMediaType_Valid_DVD()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "XboxBackupCreator", "DVD");
            string outputFilename = "test";
            var processor = new XboxBackupCreator(RedumpSystem.MicrosoftXbox);
            var actual = processor.DetermineMediaType(outputDirectory, outputFilename);
            Assert.Equal(MediaType.DVD, actual);
        }

        #endregion

        #region GetOutputFiles

        [Fact]
        public void GetOutputFiles_Null_Populated()
        {
            string? outputDirectory = null;
            string outputFilename = "test";
            var processor = new XboxBackupCreator(RedumpSystem.MicrosoftXbox);

            var actual = processor.GetOutputFiles(null, outputDirectory, outputFilename);
            Assert.Equal(7, actual.Count);
        }

        [Fact]
        public void GetOutputFiles_DVD_Populated()
        {
            string? outputDirectory = null;
            string outputFilename = "test";
            var processor = new XboxBackupCreator(RedumpSystem.MicrosoftXbox);

            var actual = processor.GetOutputFiles(MediaType.DVD, outputDirectory, outputFilename);
            Assert.Equal(7, actual.Count);
        }

        [Fact]
        public void GetOutputFiles_Other_Populated()
        {
            string? outputDirectory = null;
            string outputFilename = "test";
            var processor = new XboxBackupCreator(RedumpSystem.MicrosoftXbox);

            var actual = processor.GetOutputFiles(MediaType.ApertureCard, outputDirectory, outputFilename);
            Assert.Equal(7, actual.Count);
        }

        #endregion

        #region FoundAllFiles

        [Fact]
        public void FoundAllFiles_Invalid_Filled()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new XboxBackupCreator(RedumpSystem.MicrosoftXbox);
            var actual = processor.FoundAllFiles(MediaType.DVD, outputDirectory, outputFilename);
            Assert.Equal(5, actual.Count);
        }

        [Fact]
        public void FoundAllFiles_Valid_Empty()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "XboxBackupCreator", "DVD");
            string outputFilename = "test.iso";
            var processor = new XboxBackupCreator(RedumpSystem.MicrosoftXbox);
            var actual = processor.FoundAllFiles(MediaType.DVD, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void FoundAllFiles_ValidZip_Empty()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "XboxBackupCreator", "DVD-zip");
            string outputFilename = "test.iso";
            var processor = new XboxBackupCreator(RedumpSystem.MicrosoftXbox);
            var actual = processor.FoundAllFiles(MediaType.DVD, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        #endregion

        #region FoundAnyFiles

        [Fact]
        public void FoundAnyFiles_Invalid_Filled()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new XboxBackupCreator(RedumpSystem.MicrosoftXbox);
            var actual = processor.FoundAnyFiles(MediaType.DVD, outputDirectory, outputFilename);
            Assert.False(actual);
        }

        [Fact]
        public void FoundAnyFiles_Valid_Empty()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "XboxBackupCreator", "DVD");
            string outputFilename = "test.iso";
            var processor = new XboxBackupCreator(RedumpSystem.MicrosoftXbox);
            var actual = processor.FoundAnyFiles(MediaType.DVD, outputDirectory, outputFilename);
            Assert.True(actual);
        }

        #endregion

        #region GenerateArtifacts

        [Fact]
        public void GenerateArtifacts_Invalid_Empty()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new XboxBackupCreator(RedumpSystem.MicrosoftXbox);
            var actual = processor.GenerateArtifacts(MediaType.DVD, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void GenerateArtifacts_Valid_Filled()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "XboxBackupCreator", "DVD");
            string outputFilename = "test.iso";
            var processor = new XboxBackupCreator(RedumpSystem.MicrosoftXbox);
            var actual = processor.GenerateArtifacts(MediaType.DVD, outputDirectory, outputFilename);
            Assert.Equal(6, actual.Count);
        }

        #endregion

        #region GetDeleteableFilePaths

        [Fact]
        public void GetDeleteableFilePaths_Invalid_Empty()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new XboxBackupCreator(RedumpSystem.MicrosoftXbox);
            var actual = processor.GetDeleteableFilePaths(MediaType.DVD, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetDeleteableFilePaths_Valid_Empty()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "XboxBackupCreator", "DVD");
            string outputFilename = "test.iso";
            var processor = new XboxBackupCreator(RedumpSystem.MicrosoftXbox);
            var actual = processor.GetDeleteableFilePaths(MediaType.DVD, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        #endregion

        #region GetZippableFilePaths

        [Fact]
        public void GetZippableFilePaths_Invalid_Empty()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new XboxBackupCreator(RedumpSystem.MicrosoftXbox);
            var actual = processor.GetZippableFilePaths(MediaType.DVD, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetZippableFilePaths_Valid_Filled()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "XboxBackupCreator", "DVD");
            string outputFilename = "test.iso";
            var processor = new XboxBackupCreator(RedumpSystem.MicrosoftXbox);
            var actual = processor.GetZippableFilePaths(MediaType.DVD, outputDirectory, outputFilename);
            Assert.Equal(6, actual.Count);
        }

        #endregion

        #region GetPreservedFilePaths

        [Fact]
        public void GetPreservedFilePaths_Invalid_Empty()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new XboxBackupCreator(RedumpSystem.MicrosoftXbox);
            var actual = processor.GetPreservedFilePaths(MediaType.DVD, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetPreservedFilePaths_Valid_Empty()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "XboxBackupCreator", "DVD");
            string outputFilename = "test.iso";
            var processor = new XboxBackupCreator(RedumpSystem.MicrosoftXbox);
            var actual = processor.GetPreservedFilePaths(MediaType.DVD, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        #endregion

        #region GetLogName

        [Fact]
        public void GetLogName_EmptyDir_Null()
        {
            string baseDir = string.Empty;
            string? actual = XboxBackupCreator.GetLogName(baseDir);
            Assert.Null(actual);
        }

        [Fact]
        public void GetLogName_InvalidDir_Null()
        {
            string baseDir = "INVALID";
            string? actual = XboxBackupCreator.GetLogName(baseDir);
            Assert.Null(actual);
        }

        [Fact]
        public void GetLogName_ValidDirNoLog_Null()
        {
            string? baseDir = Path.Combine(Environment.CurrentDirectory, "TestData", "XboxBackupCreator");
            string? actual = XboxBackupCreator.GetLogName(baseDir);
            Assert.Null(actual);
        }

        [Fact]
        public void GetLogName_ValidDirLog_Path()
        {
            string? baseDir = Path.Combine(Environment.CurrentDirectory, "TestData", "XboxBackupCreator", "DVD");
            string? actual = XboxBackupCreator.GetLogName(baseDir);
            Assert.NotNull(actual);
        }

        #endregion

        #region GetVersion

        [Fact]
        public void GetVersion_Null_Null()
        {
            string? log = null;
            string? actual = XboxBackupCreator.GetVersion(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetVersion_Empty_Null()
        {
            string? log = string.Empty;
            string? actual = XboxBackupCreator.GetVersion(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetVersion_Invalid_Null()
        {
            string? log = "INVALID";
            string? actual = XboxBackupCreator.GetVersion(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetVersion_Valid_Filled()
        {
            string? expected = "v0.0 Build:0000 By Redline99";
            string? log = Path.Combine(Environment.CurrentDirectory, "TestData", "XboxBackupCreator", "DVD", "log.txt");
            string? actual = XboxBackupCreator.GetVersion(log);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetDrive

        [Fact]
        public void GetDrive_Null_Null()
        {
            string? log = null;
            string? actual = XboxBackupCreator.GetDrive(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetDrive_Empty_Null()
        {
            string? log = string.Empty;
            string? actual = XboxBackupCreator.GetDrive(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetDrive_Invalid_Null()
        {
            string? log = "INVALID";
            string? actual = XboxBackupCreator.GetDrive(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetDrive_Valid_Filled()
        {
            string? expected = "SH-D162D";
            string? log = Path.Combine(Environment.CurrentDirectory, "TestData", "XboxBackupCreator", "DVD", "log.txt");
            string? actual = XboxBackupCreator.GetDrive(log);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetLayerbreak

        [Fact]
        public void GetLayerbreak_Null_Null()
        {
            long expected = -1;
            string? log = null;
            bool actual = XboxBackupCreator.GetLayerbreak(log, out long layerbreak);

            Assert.False(actual);
            Assert.Equal(expected, layerbreak);
        }

        [Fact]
        public void GetLayerbreak_Empty_Null()
        {
            long expected = -1;
            string? log = string.Empty;
            bool actual = XboxBackupCreator.GetLayerbreak(log, out long layerbreak);

            Assert.False(actual);
            Assert.Equal(expected, layerbreak);
        }

        [Fact]
        public void GetLayerbreak_Invalid_Null()
        {
            long expected = -1;
            string? log = "INVALID";
            bool actual = XboxBackupCreator.GetLayerbreak(log, out long layerbreak);

            Assert.False(actual);
            Assert.Equal(expected, layerbreak);
        }

        [Fact]
        public void GetLayerbreak_Valid_Filled()
        {
            long expected = 1913776;
            string? log = Path.Combine(Environment.CurrentDirectory, "TestData", "XboxBackupCreator", "DVD", "log.txt");
            bool actual = XboxBackupCreator.GetLayerbreak(log, out long layerbreak);

            Assert.True(actual);
            Assert.Equal(expected, layerbreak);
        }

        #endregion

        #region GetReadErrors

        [Fact]
        public void GetReadErrors_Null_Null()
        {
            long expected = -1;
            string? log = null;
            var processor = new XboxBackupCreator(RedumpSystem.MicrosoftXbox);
            bool actual = processor.GetReadErrors(log, out long readErrors);

            Assert.False(actual);
            Assert.Equal(expected, readErrors);
        }

        [Fact]
        public void GetReadErrors_Empty_Null()
        {
            long expected = -1;
            string? log = string.Empty;
            var processor = new XboxBackupCreator(RedumpSystem.MicrosoftXbox);
            bool actual = processor.GetReadErrors(log, out long readErrors);

            Assert.False(actual);
            Assert.Equal(expected, readErrors);
        }

        [Fact]
        public void GetReadErrors_Invalid_Null()
        {
            long expected = -1;
            string? log = "INVALID";
            var processor = new XboxBackupCreator(RedumpSystem.MicrosoftXbox);
            bool actual = processor.GetReadErrors(log, out long readErrors);

            Assert.False(actual);
            Assert.Equal(expected, readErrors);
        }

        [Fact]
        public void GetReadErrors_Valid_Filled()
        {
            long expected = 0;
            string? log = Path.Combine(Environment.CurrentDirectory, "TestData", "XboxBackupCreator", "DVD", "log.txt");
            var processor = new XboxBackupCreator(RedumpSystem.MicrosoftXbox);
            bool actual = processor.GetReadErrors(log, out long readErrors);

            Assert.True(actual);
            Assert.Equal(expected, readErrors);
        }

        #endregion

        #region GetMediaID

        [Fact]
        public void GetMediaID_Null_Null()
        {
            string? log = null;
            var processor = new XboxBackupCreator(RedumpSystem.MicrosoftXbox360);
            string? actual = processor.GetMediaID(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetMediaID_Empty_Null()
        {
            string? log = string.Empty;
            var processor = new XboxBackupCreator(RedumpSystem.MicrosoftXbox360);
            string? actual = processor.GetMediaID(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetMediaID_Invalid_Null()
        {
            string? log = "INVALID";
            var processor = new XboxBackupCreator(RedumpSystem.MicrosoftXbox360);
            string? actual = processor.GetMediaID(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetMediaID_Valid_Filled()
        {
            string? expected = "8B62A812";
            string? log = Path.Combine(Environment.CurrentDirectory, "TestData", "XboxBackupCreator", "DVD", "log.txt");
            var processor = new XboxBackupCreator(RedumpSystem.MicrosoftXbox360);
            string? actual = processor.GetMediaID(log);
            Assert.Equal(expected, actual);
        }

        #endregion
    }
}