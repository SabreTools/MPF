using System;
using System.IO;
using SabreTools.RedumpLib.Data;
using Xunit;

namespace MPF.Processors.Test
{
    public class WUDDTests
    {
        #region DeterminePhysicalMediaType

        [Fact]
        public void DeterminePhysicalMediaType_Null_NintendoWiiUOpticalDisc()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new WUDD(PhysicalSystem.NintendoWiiU);
            var actual = processor.DeterminePhysicalMediaType(outputDirectory, outputFilename);
            Assert.Equal(PhysicalMediaType.NintendoWiiUOpticalDisc, actual);
        }

        [Fact]
        public void DeterminePhysicalMediaType_Invalid_NintendoWiiUOpticalDisc()
        {
            string? outputDirectory = null;
            string outputFilename = "INVALID";
            var processor = new WUDD(PhysicalSystem.NintendoWiiU);
            var actual = processor.DeterminePhysicalMediaType(outputDirectory, outputFilename);
            Assert.Equal(PhysicalMediaType.NintendoWiiUOpticalDisc, actual);
        }

        [Fact]
        public void DeterminePhysicalMediaType_Valid_NintendoWiiUOpticalDisc()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "WUDD", "NintendoWiiUOpticalDisc");
            string outputFilename = "test";
            var processor = new WUDD(PhysicalSystem.NintendoWiiU);
            var actual = processor.DeterminePhysicalMediaType(outputDirectory, outputFilename);
            Assert.Equal(PhysicalMediaType.NintendoWiiUOpticalDisc, actual);
        }

        #endregion

        #region GetOutputFiles

        [Fact]
        public void GetOutputFiles_Null_Populated()
        {
            string? outputDirectory = null;
            string outputFilename = "test";
            var processor = new WUDD(PhysicalSystem.NintendoWiiU);

            var actual = processor.GetOutputFiles(null, outputDirectory, outputFilename);
            Assert.Equal(3, actual.Count);
        }

        [Fact]
        public void GetOutputFiles_NintendoWiiUOpticalDisc_Populated()
        {
            string? outputDirectory = null;
            string outputFilename = "test";
            var processor = new WUDD(PhysicalSystem.NintendoWiiU);

            var actual = processor.GetOutputFiles(PhysicalMediaType.NintendoWiiUOpticalDisc, outputDirectory, outputFilename);
            Assert.Equal(3, actual.Count);
        }

        [Fact]
        public void GetOutputFiles_Other_Populated()
        {
            string? outputDirectory = null;
            string outputFilename = "test";
            var processor = new WUDD(PhysicalSystem.NintendoWiiU);

            var actual = processor.GetOutputFiles(PhysicalMediaType.ApertureCard, outputDirectory, outputFilename);
            Assert.Equal(3, actual.Count);
        }

        #endregion

        #region FoundAllFiles

        [Fact]
        public void FoundAllFiles_Invalid_Filled()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new WUDD(PhysicalSystem.NintendoWiiU);
            var actual = processor.FoundAllFiles(PhysicalMediaType.NintendoWiiUOpticalDisc, outputDirectory, outputFilename);
            Assert.Equal(2, actual.Count);
        }

        [Fact]
        public void FoundAllFiles_Valid_Empty()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "WUDD", "NintendoWiiUOpticalDisc");
            string outputFilename = "test.wud";
            var processor = new WUDD(PhysicalSystem.NintendoWiiU);
            var actual = processor.FoundAllFiles(PhysicalMediaType.NintendoWiiUOpticalDisc, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        #endregion

        #region FoundAnyFiles

        [Fact]
        public void FoundAnyFiles_Invalid_Filled()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new WUDD(PhysicalSystem.NintendoWiiU);
            var actual = processor.FoundAnyFiles(PhysicalMediaType.NintendoWiiUOpticalDisc, outputDirectory, outputFilename);
            Assert.False(actual);
        }

        [Fact]
        public void FoundAnyFiles_Valid_Empty()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "WUDD", "NintendoWiiUOpticalDisc");
            string outputFilename = "test.wud";
            var processor = new WUDD(PhysicalSystem.NintendoWiiU);
            var actual = processor.FoundAnyFiles(PhysicalMediaType.NintendoWiiUOpticalDisc, outputDirectory, outputFilename);
            Assert.True(actual);
        }

        [Fact]
        public void FoundAnyFiles_ValidZip_Empty()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "WUDD", "NintendoWiiUOpticalDisc-zip");
            string outputFilename = "test.wud";
            var processor = new WUDD(PhysicalSystem.NintendoWiiU);
            var actual = processor.FoundAnyFiles(PhysicalMediaType.NintendoWiiUOpticalDisc, outputDirectory, outputFilename);
            Assert.True(actual);
        }

        #endregion

        #region GenerateArtifacts

        [Fact]
        public void GenerateArtifacts_Invalid_Empty()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new WUDD(PhysicalSystem.NintendoWiiU);
            var actual = processor.GenerateArtifacts(PhysicalMediaType.NintendoWiiUOpticalDisc, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void GenerateArtifacts_Valid_Filled()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "WUDD", "NintendoWiiUOpticalDisc");
            string outputFilename = "test.wud";
            var processor = new WUDD(PhysicalSystem.NintendoWiiU);
            var actual = processor.GenerateArtifacts(PhysicalMediaType.NintendoWiiUOpticalDisc, outputDirectory, outputFilename);
            Assert.Equal(2, actual.Count);
        }

        #endregion

        #region GetDeleteableFilePaths

        [Fact]
        public void GetDeleteableFilePaths_Invalid_Empty()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new WUDD(PhysicalSystem.NintendoWiiU);
            var actual = processor.GetDeleteableFilePaths(PhysicalMediaType.NintendoWiiUOpticalDisc, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetDeleteableFilePaths_Valid_Empty()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "WUDD", "NintendoWiiUOpticalDisc");
            string outputFilename = "test.wud";
            var processor = new WUDD(PhysicalSystem.NintendoWiiU);
            var actual = processor.GetDeleteableFilePaths(PhysicalMediaType.NintendoWiiUOpticalDisc, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        #endregion

        #region GetZippableFilePaths

        [Fact]
        public void GetZippableFilePaths_Invalid_Empty()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new WUDD(PhysicalSystem.NintendoWiiU);
            var actual = processor.GetZippableFilePaths(PhysicalMediaType.NintendoWiiUOpticalDisc, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetZippableFilePaths_Valid_Filled()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "WUDD", "NintendoWiiUOpticalDisc");
            string outputFilename = "test.wud";
            var processor = new WUDD(PhysicalSystem.NintendoWiiU);
            var actual = processor.GetZippableFilePaths(PhysicalMediaType.NintendoWiiUOpticalDisc, outputDirectory, outputFilename);
            Assert.Equal(2, actual.Count);
        }

        #endregion

        #region GetPreservedFilePaths

        [Fact]
        public void GetPreservedFilePaths_Invalid_Empty()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new WUDD(PhysicalSystem.NintendoWiiU);
            var actual = processor.GetPreservedFilePaths(PhysicalMediaType.NintendoWiiUOpticalDisc, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetPreservedFilePaths_Valid_Empty()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "WUDD", "NintendoWiiUOpticalDisc");
            string outputFilename = "test.wud";
            var processor = new WUDD(PhysicalSystem.NintendoWiiU);
            var actual = processor.GetPreservedFilePaths(PhysicalMediaType.NintendoWiiUOpticalDisc, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        #endregion

#region GetLogFilePath

        [Fact]
        public void GetLogFilePath_Empty_Null()
        {
            string basePath = string.Empty;
            string? logPath = WUDD.GetLogFilePath(basePath);
            Assert.Null(logPath);
        }

        [Fact]
        public void GetLogFilePath_Invalid_Null()
        {
            string basePath = "INVALID";
            string? logPath = WUDD.GetLogFilePath(basePath);
            Assert.Null(logPath);
        }

        [Fact]
        public void GetLogFilePath_Valid_Filled()
        {
            string basePath = Path.Combine(Environment.CurrentDirectory, "TestData", "WUDD", "NintendoWiiUOpticalDisc", "test");
            string? logPath = WUDD.GetLogFilePath(basePath);
            Assert.NotNull(logPath);
        }

        #endregion

        #region GenerateWUDDDatafile

        [Fact]
        public void GenerateWUDDDatafile_NoWudNoLog_Null()
        {
            string wud = "INVALID";
            string log = "INVALID";

            var actual = WUDD.GenerateWUDDDatafile(wud, log);
            Assert.Null(actual);
        }

        [Fact]
        public void GenerateWUDDDatafile_WudOnly_Filled()
        {
            string wud = Path.Combine(Environment.CurrentDirectory, "TestData", "WUDD", "NintendoWiiUOpticalDisc", "test.wud");
            string log = "INVALID";

            var actual = WUDD.GenerateWUDDDatafile(wud, log);
            Assert.NotNull(actual);

            Assert.NotNull(actual.Game);
            var game = Assert.Single(actual.Game);

            Assert.NotNull(game.Rom);
            var rom = Assert.Single(game.Rom);
            Assert.Equal(9, rom.Size);
            Assert.Equal("560b9f59", rom.CRC);
            Assert.Equal("edbb6676247e65c2245dd4883ed9fc24", rom.MD5);
            Assert.Equal("1b33ad54d78085be5ecb1cf1b3e9da821e708075", rom.SHA1);
        }

        [Fact]
        public void GenerateWUDDDatafile_DumpinfoOnly_Filled()
        {
            string wud = "INVALID";
            string log = Path.Combine(Environment.CurrentDirectory, "TestData", "WUDD", "NintendoWiiUOpticalDisc", "2026-09-29-10-33-33.txt");

            var actual = WUDD.GenerateWUDDDatafile(wud, log);
            Assert.NotNull(actual);

            Assert.NotNull(actual.Game);
            var game = Assert.Single(actual.Game);

            Assert.NotNull(game.Rom);
            var rom = Assert.Single(game.Rom);
            Assert.Equal(-1, rom.Size);
            Assert.Equal("00000000", rom.CRC);
            Assert.Equal("d41d8cd98f00b204e9800998ecf8427e", rom.MD5);
            Assert.Equal("da39a3ee5e6b4b0d3255bfef95601890afd80709", rom.SHA1);
        }

        [Fact]
        public void GenerateWUDDDatafile_BothValid_Filled()
        {
            string wud = Path.Combine(Environment.CurrentDirectory, "TestData", "WUDD", "NintendoWiiUOpticalDisc", "test.wud");
            string log = Path.Combine(Environment.CurrentDirectory, "TestData", "WUDD", "NintendoWiiUOpticalDisc", "2026-09-29-10-33-33.txt");

            var actual = WUDD.GenerateWUDDDatafile(wud, log);
            Assert.NotNull(actual);

            Assert.NotNull(actual.Game);
            var game = Assert.Single(actual.Game);

            Assert.NotNull(game.Rom);
            var rom = Assert.Single(game.Rom);
            Assert.Equal(9, rom.Size);
            Assert.Equal("00000000", rom.CRC);
            Assert.Equal("d41d8cd98f00b204e9800998ecf8427e", rom.MD5);
            Assert.Equal("da39a3ee5e6b4b0d3255bfef95601890afd80709", rom.SHA1);
        }

        #endregion

        #region GetDiscID

        [Fact]
        public void GetDiscID_NoLog_Null()
        {
            string log = "INVALID";

            var actual = WUDD.GetDiscID(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetDiscID_ValidLog_Filled()
        {
            string log = Path.Combine(Environment.CurrentDirectory, "TestData", "WUDD", "NintendoWiiUOpticalDisc", "2026-09-29-10-33-33.txt");

            var actual = WUDD.GetDiscID(log);
            Assert.Equal("WUP-Z-ABCD", actual);
        }

        #endregion

        #region GetDiscKey

        [Fact]
        public void GetDiscKey_NoLog_Null()
        {
            string key = "INVALID";

            var actual = WUDD.GetDiscKey(key);
            Assert.Null(actual);
        }

        [Fact]
        public void GetDiscKey_ValidLog_Filled()
        {
            string key = Path.Combine(Environment.CurrentDirectory, "TestData", "WUDD", "NintendoWiiUOpticalDisc", "test.key");

            var actual = WUDD.GetDiscKey(key);
            Assert.Equal("54455354204441203136204259544553", actual);
        }

        #endregion

        #region GetDumpStartDateTime

        [Fact]
        public void GetDumpStartDateTime_NoLog_Null()
        {
            string log = "INVALID";

            var actual = WUDD.GetDumpStartDateTime(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetDumpStartDateTime_ValidLog_Filled()
        {
            string log = Path.Combine(Environment.CurrentDirectory, "TestData", "WUDD", "NintendoWiiUOpticalDisc", "2026-09-29-10-33-33.txt");

            var actual = WUDD.GetDumpStartDateTime(log);
            Assert.Equal("2026-09-29 10:33:33", actual);
        }

        #endregion

        #region GetVersion

        [Fact]
        public void GetVersion_NoLog_Null()
        {
            string log = "INVALID";

            var actual = WUDD.GetVersion(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetVersion_ValidLog_Filled()
        {
            string log = Path.Combine(Environment.CurrentDirectory, "TestData", "WUDD", "NintendoWiiUOpticalDisc", "2026-09-29-10-33-33.txt");

            var actual = WUDD.GetVersion(log);
            Assert.Equal("v1.2.3 2026_08_21_12_17", actual);
        }

        #endregion
    }
}
