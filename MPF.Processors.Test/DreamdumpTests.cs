using System;
using System.IO;
using SabreTools.RedumpLib.Data;
using Xunit;

namespace MPF.Processors.Test
{
    public class DreamdumpTests
    {
        #region DeterminePhysicalMediaType

        [Fact]
        public void DeterminePhysicalMediaType_Constant()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new Dreamdump(PhysicalSystem.SegaDreamcast);
            var actual = processor.DeterminePhysicalMediaType(outputDirectory, outputFilename);
            Assert.Equal(PhysicalMediaType.GDROM, actual);
        }

        #endregion

        #region GetOutputFiles

        [Fact]
        public void GetOutputFiles_GDROM_Populated()
        {
            string? outputDirectory = null;
            string outputFilename = "test";
            var processor = new Dreamdump(PhysicalSystem.SegaDreamcast);

            var actual = processor.GetOutputFiles(PhysicalMediaType.GDROM, outputDirectory, outputFilename);
            Assert.Equal(126, actual.Count);
        }

        [Fact]
        public void GetOutputFiles_Other_Populated()
        {
            string? outputDirectory = null;
            string outputFilename = "test";
            var processor = new Dreamdump(PhysicalSystem.SegaDreamcast);

            var actual = processor.GetOutputFiles(PhysicalMediaType.ApertureCard, outputDirectory, outputFilename);
            Assert.Equal(126, actual.Count);
        }

        #endregion

        #region FoundAllFiles

        [Fact]
        public void FoundAllFiles_Invalid_Filled()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new Dreamdump(PhysicalSystem.SegaDreamcast);
            var actual = processor.FoundAllFiles(PhysicalMediaType.GDROM, outputDirectory, outputFilename);
            Assert.Equal(4, actual.Count);
        }

        [Fact]
        public void FoundAllFiles_Valid_Empty()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "Dreamdump", "GDROM");
            string outputFilename = "test.cue";
            var processor = new Dreamdump(PhysicalSystem.SegaDreamcast);
            var actual = processor.FoundAllFiles(PhysicalMediaType.GDROM, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void FoundAllFiles_ValidZip_Empty()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "Dreamdump", "GDROM-zip");
            string outputFilename = "test.cue";
            var processor = new Dreamdump(PhysicalSystem.SegaDreamcast);
            var actual = processor.FoundAllFiles(PhysicalMediaType.GDROM, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        #endregion

        #region FoundAnyFiles

        [Fact]
        public void FoundAnyFiles_Invalid_Filled()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new Dreamdump(PhysicalSystem.SegaDreamcast);
            var actual = processor.FoundAnyFiles(PhysicalMediaType.GDROM, outputDirectory, outputFilename);
            Assert.False(actual);
        }

        [Fact]
        public void FoundAnyFiles_Valid_Empty()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "Dreamdump", "GDROM");
            string outputFilename = "test.cue";
            var processor = new Dreamdump(PhysicalSystem.SegaDreamcast);
            var actual = processor.FoundAnyFiles(PhysicalMediaType.GDROM, outputDirectory, outputFilename);
            Assert.True(actual);
        }

        #endregion

        #region GenerateArtifacts

        [Fact]
        public void GenerateArtifacts_Invalid_Empty()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new Dreamdump(PhysicalSystem.SegaDreamcast);
            var actual = processor.GenerateArtifacts(PhysicalMediaType.GDROM, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void GenerateArtifacts_Valid_Filled()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "Dreamdump", "GDROM");
            string outputFilename = "test.cue";
            var processor = new Dreamdump(PhysicalSystem.SegaDreamcast);
            var actual = processor.GenerateArtifacts(PhysicalMediaType.GDROM, outputDirectory, outputFilename);
            Assert.Equal(2, actual.Count);
        }

        #endregion

        #region GetDeleteableFilePaths

        [Fact]
        public void GetDeleteableFilePaths_Invalid_Empty()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new Dreamdump(PhysicalSystem.SegaDreamcast);
            var actual = processor.GetDeleteableFilePaths(PhysicalMediaType.GDROM, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetDeleteableFilePaths_Valid_Single()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "Dreamdump", "GDROM");
            string outputFilename = "test.cue";
            var processor = new Dreamdump(PhysicalSystem.SegaDreamcast);
            var actual = processor.GetDeleteableFilePaths(PhysicalMediaType.GDROM, outputDirectory, outputFilename);
            Assert.Equal(41, actual.Count);
        }

        #endregion

        #region GetZippableFilePaths

        [Fact]
        public void GetZippableFilePaths_Invalid_Empty()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new Dreamdump(PhysicalSystem.SegaDreamcast);
            var actual = processor.GetZippableFilePaths(PhysicalMediaType.GDROM, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetZippableFilePaths_Valid_Filled()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "Dreamdump", "GDROM");
            string outputFilename = "test.cue";
            var processor = new Dreamdump(PhysicalSystem.SegaDreamcast);
            var actual = processor.GetZippableFilePaths(PhysicalMediaType.GDROM, outputDirectory, outputFilename);
            Assert.Equal(84, actual.Count);
        }

        #endregion

        #region GetPreservedFilePaths

        [Fact]
        public void GetPreservedFilePaths_Invalid_Empty()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new Dreamdump(PhysicalSystem.SegaDreamcast);
            var actual = processor.GetPreservedFilePaths(PhysicalMediaType.GDROM, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetPreservedFilePaths_Valid_Filled()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "Dreamdump", "GDROM");
            string outputFilename = "test.cue";
            var processor = new Dreamdump(PhysicalSystem.SegaDreamcast);
            var actual = processor.GetPreservedFilePaths(PhysicalMediaType.GDROM, outputDirectory, outputFilename);
            Assert.Equal(3, actual.Count);
            Assert.Contains(Path.Combine(outputDirectory, "test.cue"), actual);
            Assert.Contains(Path.Combine(outputDirectory, "test.gdi"), actual);
            Assert.Contains(Path.Combine(outputDirectory, "test.log"), actual);
        }

        #endregion

        #region GetCuesheet

        [Fact]
        public void GetCuesheet_Empty_Null()
        {
            string log = string.Empty;
            string? actual = Dreamdump.GetCuesheet(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetCuesheet_Invalid_Null()
        {
            string log = "INVALID";
            string? actual = Dreamdump.GetCuesheet(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetCuesheet_Valid_Filled()
        {
            string? expected = "    cuesheet";
            string log = Path.Combine(Environment.CurrentDirectory, "TestData", "Dreamdump", "GDROM", "test.log");
            string? actual = Dreamdump.GetCuesheet(log);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetDatfile

        [Fact]
        public void GetDatfile_Empty_Null()
        {
            string log = string.Empty;
            string? actual = Dreamdump.GetDatfile(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetDatfile_Invalid_Null()
        {
            string log = "INVALID";
            string? actual = Dreamdump.GetDatfile(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetDatfile_Valid_Filled()
        {
            string? expected = "<rom name=\"INVALID\" size=\"12345\" crc=\"00000000\" md5=\"d41d8cd98f00b204e9800998ecf8427e\" sha1=\"da39a3ee5e6b4b0d3255bfef95601890afd80709\" />";
            string log = Path.Combine(Environment.CurrentDirectory, "TestData", "Dreamdump", "GDROM", "test.log");
            string? actual = Dreamdump.GetDatfile(log);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetGDROMHeader

        [Fact]
        public void GetGDROMHeader_Empty_Null()
        {
            string log = string.Empty;
            string? actual = Dreamdump.GetGDROMHeader(log,
                out string? buildDate,
                out string? serial,
                out string? region,
                out string? version);

            Assert.Null(actual);
            Assert.Null(buildDate);
            Assert.Null(serial);
            Assert.Null(region);
            Assert.Null(version);
        }

        [Fact]
        public void GetGDROMHeader_Invalid_Null()
        {
            string log = "INVALID";
            string? actual = Dreamdump.GetGDROMHeader(log,
                out string? buildDate,
                out string? serial,
                out string? region,
                out string? version);

            Assert.Null(actual);
            Assert.Null(buildDate);
            Assert.Null(serial);
            Assert.Null(region);
            Assert.Null(version);
        }

        [Fact]
        public void GetGDROMHeader_Valid_Filled()
        {
            string? expected = "0000 TEST DATA\n0010 TEST DATA\n0020 TEST DATA\n0030 TEST DATA\n0040 TEST DATA\n0050 TEST DATA\n0060 TEST DATA\n0070 TEST DATA\n0080 TEST DATA\n0090 TEST DATA\n00A0 TEST DATA";
            string? expectedBuildDate = "date";
            string? expectedSerial = "serial";
            string? expectedRegion = "region";
            string? expectedVersion = "version";

            string log = Path.Combine(Environment.CurrentDirectory, "TestData", "Dreamdump", "GDROM", "test.log");
            string? actual = Dreamdump.GetGDROMHeader(log,
                out string? buildDate,
                out string? serial,
                out string? region,
                out string? version);

            Assert.Equal(expected, actual);
            Assert.Equal(expectedBuildDate, buildDate);
            Assert.Equal(expectedSerial, serial);
            Assert.Equal(expectedRegion, region);
            Assert.Equal(expectedVersion, version);
        }

        #endregion

        #region GetHardwareInfo

        [Fact]
        public void GetHardwareInfo_Empty_Null()
        {
            string log = string.Empty;
            bool actual = Dreamdump.GetHardwareInfo(log,
                out string? manufacturer,
                out string? model,
                out string? firmware);

            Assert.False(actual);
            Assert.Null(manufacturer);
            Assert.Null(model);
            Assert.Null(firmware);
        }

        [Fact]
        public void GetHardwareInfo_Invalid_Null()
        {
            string log = "INVALID";
            bool actual = Dreamdump.GetHardwareInfo(log,
                out string? manufacturer,
                out string? model,
                out string? firmware);

            Assert.False(actual);
            Assert.Null(manufacturer);
            Assert.Null(model);
            Assert.Null(firmware);
        }

        [Fact]
        public void GetHardwareInfo_Valid_Filled()
        {
            string? expectedManufacturer = "manufacturer";
            string? expectedModel = "model";
            string? expectedFirmware = "revision (vendor)";

            string log = Path.Combine(Environment.CurrentDirectory, "TestData", "Dreamdump", "GDROM", "test.log");
            bool actual = Dreamdump.GetHardwareInfo(log,
                out string? manufacturer,
                out string? model,
                out string? firmware);

            Assert.True(actual);
            Assert.Equal(expectedManufacturer, manufacturer);
            Assert.Equal(expectedModel, model);
            Assert.Equal(expectedFirmware, firmware);
        }

        #endregion

        #region GetParameters

        [Fact]
        public void GetParameters_Empty_Null()
        {
            string log = string.Empty;
            string? actual = Dreamdump.GetParameters(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetParameters_Invalid_Null()
        {
            string log = "INVALID";
            string? actual = Dreamdump.GetParameters(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetParameters_Valid_Filled()
        {
            string? expected = "cd --verbose";
            string log = Path.Combine(Environment.CurrentDirectory, "TestData", "Dreamdump", "GDROM", "test.log");
            string? actual = Dreamdump.GetParameters(log);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetVersion

        [Fact]
        public void GetVersion_Empty_Null()
        {
            string log = string.Empty;
            string? actual = Dreamdump.GetVersion(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetVersion_Invalid_Null()
        {
            string log = "INVALID";
            string? actual = Dreamdump.GetVersion(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetVersion_Valid_Filled()
        {
            string? expected = "build: 0.2.0";
            string log = Path.Combine(Environment.CurrentDirectory, "TestData", "Dreamdump", "GDROM", "test.log");
            string? actual = Dreamdump.GetVersion(log);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetWriteOffset

        [Fact]
        public void GetWriteOffset_Empty_Null()
        {
            string log = string.Empty;
            string? actual = Dreamdump.GetWriteOffset(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetWriteOffset_Invalid_Null()
        {
            string log = "INVALID";
            string? actual = Dreamdump.GetWriteOffset(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetWriteOffset_Valid_Filled()
        {
            string? expected = "+0";
            string log = Path.Combine(Environment.CurrentDirectory, "TestData", "Dreamdump", "GDROM", "test.log");
            string? actual = Dreamdump.GetWriteOffset(log);
            Assert.Equal(expected, actual);
        }

        #endregion
    }
}
