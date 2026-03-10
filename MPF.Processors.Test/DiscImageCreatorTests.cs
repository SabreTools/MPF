using System;
using System.Collections.Generic;
using System.IO;
using SabreTools.Data.Models.Logiqx;
using SabreTools.RedumpLib.Data;
using Xunit;

namespace MPF.Processors.Test
{
    // TODO: Add tests around remaining helper methods
    public class DiscImageCreatorTests
    {
        #region DetermineMediaType

        [Fact]
        public void DetermineMediaType_Null_Null()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new DiscImageCreator(RedumpSystem.IBMPCcompatible);
            var actual = processor.DetermineMediaType(outputDirectory, outputFilename);
            Assert.Null(actual);
        }

        [Fact]
        public void DetermineMediaType_Invalid_Null()
        {
            string? outputDirectory = null;
            string outputFilename = "INVALID";
            var processor = new DiscImageCreator(RedumpSystem.IBMPCcompatible);
            var actual = processor.DetermineMediaType(outputDirectory, outputFilename);
            Assert.Null(actual);
        }

        [Fact]
        public void DetermineMediaType_BD_Filled()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "DiscImageCreator", "BluRay");
            string outputFilename = "test";
            var processor = new DiscImageCreator(RedumpSystem.IBMPCcompatible);
            var actual = processor.DetermineMediaType(outputDirectory, outputFilename);
            Assert.Equal(MediaType.BluRay, actual);
        }

        [Fact]
        public void DetermineMediaType_CD_Filled()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "DiscImageCreator", "CDROM");
            string outputFilename = "test";
            var processor = new DiscImageCreator(RedumpSystem.IBMPCcompatible);
            var actual = processor.DetermineMediaType(outputDirectory, outputFilename);
            Assert.Equal(MediaType.CDROM, actual);
        }

        [Fact]
        public void DetermineMediaType_DVD_Filled()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "DiscImageCreator", "DVD");
            string outputFilename = "test";
            var processor = new DiscImageCreator(RedumpSystem.IBMPCcompatible);
            var actual = processor.DetermineMediaType(outputDirectory, outputFilename);
            Assert.Equal(MediaType.DVD, actual);
        }

        [Fact]
        public void DetermineMediaType_HDDVD_Filled()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "DiscImageCreator", "HDDVD");
            string outputFilename = "test";
            var processor = new DiscImageCreator(RedumpSystem.IBMPCcompatible);
            var actual = processor.DetermineMediaType(outputDirectory, outputFilename);
            Assert.Equal(MediaType.HDDVD, actual);
        }

        #endregion

        #region GetOutputFiles

        [Fact]
        public void GetOutputFiles_Null_Empty()
        {
            string? outputDirectory = null;
            string outputFilename = "test";
            var processor = new DiscImageCreator(RedumpSystem.IBMPCcompatible);

            var actual = processor.GetOutputFiles(null, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetOutputFiles_CDROM_Populated()
        {
            string? outputDirectory = null;
            string outputFilename = "test";
            var processor = new DiscImageCreator(RedumpSystem.IBMPCcompatible);

            var actual = processor.GetOutputFiles(MediaType.CDROM, outputDirectory, outputFilename);
            Assert.Equal(28, actual.Count);
        }

        [Fact]
        public void GetOutputFiles_GDROM_Populated()
        {
            string? outputDirectory = null;
            string outputFilename = "test";
            var processor = new DiscImageCreator(RedumpSystem.IBMPCcompatible);

            var actual = processor.GetOutputFiles(MediaType.GDROM, outputDirectory, outputFilename);
            Assert.Equal(10, actual.Count);
        }

        [Fact]
        public void GetOutputFiles_DVD_Populated()
        {
            string? outputDirectory = null;
            string outputFilename = "test";
            var processor = new DiscImageCreator(RedumpSystem.IBMPCcompatible);

            var actual = processor.GetOutputFiles(MediaType.DVD, outputDirectory, outputFilename);
            Assert.Equal(17, actual.Count);
        }

        [Fact]
        public void GetOutputFiles_NintendoGameCubeGameDisc_Populated()
        {
            string? outputDirectory = null;
            string outputFilename = "test";
            var processor = new DiscImageCreator(RedumpSystem.IBMPCcompatible);

            var actual = processor.GetOutputFiles(MediaType.NintendoGameCubeGameDisc, outputDirectory, outputFilename);
            Assert.Equal(17, actual.Count);
        }

        [Fact]
        public void GetOutputFiles_NintendoWiiOpticalDisc_Populated()
        {
            string? outputDirectory = null;
            string outputFilename = "test";
            var processor = new DiscImageCreator(RedumpSystem.IBMPCcompatible);

            var actual = processor.GetOutputFiles(MediaType.NintendoWiiOpticalDisc, outputDirectory, outputFilename);
            Assert.Equal(17, actual.Count);
        }

        [Fact]
        public void GetOutputFiles_HDDVD_Populated()
        {
            string? outputDirectory = null;
            string outputFilename = "test";
            var processor = new DiscImageCreator(RedumpSystem.IBMPCcompatible);

            var actual = processor.GetOutputFiles(MediaType.HDDVD, outputDirectory, outputFilename);
            Assert.Equal(15, actual.Count);
        }

        [Fact]
        public void GetOutputFiles_BluRay_Populated()
        {
            string? outputDirectory = null;
            string outputFilename = "test";
            var processor = new DiscImageCreator(RedumpSystem.IBMPCcompatible);

            var actual = processor.GetOutputFiles(MediaType.BluRay, outputDirectory, outputFilename);
            Assert.Equal(15, actual.Count);
        }

        [Fact]
        public void GetOutputFiles_FloppyDisk_Populated()
        {
            string? outputDirectory = null;
            string outputFilename = "test";
            var processor = new DiscImageCreator(RedumpSystem.IBMPCcompatible);

            var actual = processor.GetOutputFiles(MediaType.FloppyDisk, outputDirectory, outputFilename);
            Assert.Equal(4, actual.Count);
        }

        [Fact]
        public void GetOutputFiles_HardDisk_Populated()
        {
            string? outputDirectory = null;
            string outputFilename = "test";
            var processor = new DiscImageCreator(RedumpSystem.IBMPCcompatible);

            var actual = processor.GetOutputFiles(MediaType.HardDisk, outputDirectory, outputFilename);
            Assert.Equal(4, actual.Count);
        }

        [Fact]
        public void GetOutputFiles_Other_Empty()
        {
            string? outputDirectory = null;
            string outputFilename = "test";
            var processor = new DiscImageCreator(RedumpSystem.IBMPCcompatible);

            var actual = processor.GetOutputFiles(MediaType.ApertureCard, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        #endregion

        #region FoundAllFiles

        [Fact]
        public void FoundAllFiles_Invalid_Filled()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new DiscImageCreator(RedumpSystem.IBMPCcompatible);
            var actual = processor.FoundAllFiles(MediaType.CDROM, outputDirectory, outputFilename);
            Assert.Equal(14, actual.Count);
        }

        [Fact]
        public void FoundAllFiles_Valid_Empty()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "DiscImageCreator", "CDROM");
            string outputFilename = "test.cue";
            var processor = new DiscImageCreator(RedumpSystem.IBMPCcompatible);
            var actual = processor.FoundAllFiles(MediaType.CDROM, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        #endregion

        #region FoundAnyFiles

        [Fact]
        public void FoundAnyFiles_Invalid_Filled()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new DiscImageCreator(RedumpSystem.IBMPCcompatible);
            var actual = processor.FoundAnyFiles(MediaType.CDROM, outputDirectory, outputFilename);
            Assert.False(actual);
        }

        [Fact]
        public void FoundAnyFiles_Valid_Empty()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "DiscImageCreator", "CDROM");
            string outputFilename = "test.cue";
            var processor = new DiscImageCreator(RedumpSystem.IBMPCcompatible);
            var actual = processor.FoundAnyFiles(MediaType.CDROM, outputDirectory, outputFilename);
            Assert.True(actual);
        }

        [Fact]
        public void FoundAnyFiles_ValidZip_Empty()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "DiscImageCreator", "CDROM-zip");
            string outputFilename = "test.cue";
            var processor = new DiscImageCreator(RedumpSystem.IBMPCcompatible);
            var actual = processor.FoundAnyFiles(MediaType.CDROM, outputDirectory, outputFilename);
            Assert.True(actual);
        }

        #endregion

        #region GenerateArtifacts

        [Fact]
        public void GenerateArtifacts_Invalid_Empty()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new DiscImageCreator(RedumpSystem.IBMPCcompatible);
            var actual = processor.GenerateArtifacts(MediaType.CDROM, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void GenerateArtifacts_Valid_Filled()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "DiscImageCreator", "CDROM");
            string outputFilename = "test.cue";
            var processor = new DiscImageCreator(RedumpSystem.IBMPCcompatible);
            var actual = processor.GenerateArtifacts(MediaType.CDROM, outputDirectory, outputFilename);
            Assert.Equal(23, actual.Count);
        }

        #endregion

        #region GetDeleteableFilePaths

        [Fact]
        public void GetDeleteableFilePaths_Invalid_Empty()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new DiscImageCreator(RedumpSystem.IBMPCcompatible);
            var actual = processor.GetDeleteableFilePaths(MediaType.CDROM, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetDeleteableFilePaths_Valid_Filled()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "DiscImageCreator", "CDROM");
            string outputFilename = "test.cue";
            var processor = new DiscImageCreator(RedumpSystem.IBMPCcompatible);
            var actual = processor.GetDeleteableFilePaths(MediaType.CDROM, outputDirectory, outputFilename);
            Assert.Equal(2, actual.Count);
        }

        #endregion

        #region GetZippableFilePaths

        [Fact]
        public void GetZippableFilePaths_Invalid_Empty()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new DiscImageCreator(RedumpSystem.IBMPCcompatible);
            var actual = processor.GetZippableFilePaths(MediaType.CDROM, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetZippableFilePaths_Valid_Filled()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "DiscImageCreator", "CDROM");
            string outputFilename = "test.cue";
            var processor = new DiscImageCreator(RedumpSystem.IBMPCcompatible);
            var actual = processor.GetZippableFilePaths(MediaType.CDROM, outputDirectory, outputFilename);
            Assert.Equal(26, actual.Count);
        }

        #region GetPreservedFilePaths

        [Fact]
        public void GetPreservedFilePaths_Invalid_Empty()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new DiscImageCreator(RedumpSystem.IBMPCcompatible);
            var actual = processor.GetPreservedFilePaths(MediaType.CDROM, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetPreservedFilePaths_Valid_Filled()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "DiscImageCreator", "CDROM");
            string outputFilename = "test.cue";
            var processor = new DiscImageCreator(RedumpSystem.IBMPCcompatible);
            var actual = processor.GetPreservedFilePaths(MediaType.CDROM, outputDirectory, outputFilename);
            Assert.Single(actual);
        }

        #endregion

        #endregion

        #region GetCommandFilePathAndVersion

        [Fact]
        public void GetCommandFilePathAndVersion_Empty_Null()
        {
            string basePath = string.Empty;
            string? version = DiscImageCreator.GetCommandFilePathAndVersion(basePath, out string? commandPath);
            Assert.Null(version);
            Assert.Null(commandPath);
        }

        [Fact]
        public void GetCommandFilePathAndVersion_Invalid_Null()
        {
            string basePath = "INVALID";
            string? version = DiscImageCreator.GetCommandFilePathAndVersion(basePath, out string? commandPath);
            Assert.Null(version);
            Assert.Null(commandPath);
        }

        [Fact]
        public void GetCommandFilePathAndVersion_Valid_Filled()
        {
            string? expectedVersion = "19800101";
            string basePath = Path.Combine(Environment.CurrentDirectory, "TestData", "DiscImageCreator", "CDROM", "test");
            string? version = DiscImageCreator.GetCommandFilePathAndVersion(basePath, out string? commandPath);
            Assert.Equal(expectedVersion, version);
            Assert.NotNull(commandPath);
        }

        #endregion

        #region GetParameters

        [Fact]
        public void GetParameters_Empty_Null()
        {
            string basePath = string.Empty;
            string? parameters = DiscImageCreator.GetParameters(basePath);
            Assert.Null(parameters);
        }

        [Fact]
        public void GetParameters_Invalid_Null()
        {
            string basePath = "INVALID";
            string? parameters = DiscImageCreator.GetParameters(basePath);
            Assert.Null(parameters);
        }

        [Fact]
        public void GetParameters_Valid_Filled()
        {
            string? expectedParameters = "TEST DATA";
            string basePath = Path.Combine(Environment.CurrentDirectory, "TestData", "DiscImageCreator", "CDROM", "test");
            string? parameters = DiscImageCreator.GetParameters($"{basePath}_19800101T000000.txt");
            Assert.Equal(expectedParameters, parameters);
        }

        #endregion

        #region GetPlayStationEXEDate

        // TODO: This... is horrible

        #endregion

        #region GetDiscType

        [Fact]
        public void GetDiscType_Empty_Null()
        {
            string disc = string.Empty;
            bool actual = DiscImageCreator.GetDiscType(disc, out _);
            Assert.False(actual);
        }

        [Fact]
        public void GetDiscType_Invalid_Null()
        {
            string disc = "INVALID";
            bool actual = DiscImageCreator.GetDiscType(disc, out _);
            Assert.False(actual);
        }

        [Fact]
        public void GetDiscType_BD_Filled()
        {
            string? expected = "BDO";
            string disc = Path.Combine(Environment.CurrentDirectory, "TestData", "DiscImageCreator", "BluRay", "test_disc.txt");
            bool actual = DiscImageCreator.GetDiscType(disc, out string? discTypeOrBookType);
            Assert.True(actual);
            Assert.Equal(expected, discTypeOrBookType);
        }

        [Fact]
        public void GetDiscType_CD_Filled()
        {
            string? expected = "CD-ROM";
            string disc = Path.Combine(Environment.CurrentDirectory, "TestData", "DiscImageCreator", "CDROM", "test_disc.txt");
            bool actual = DiscImageCreator.GetDiscType(disc, out string? discTypeOrBookType);
            Assert.True(actual);
            Assert.Equal(expected, discTypeOrBookType);
        }

        [Fact]
        public void GetDiscType_DVD_Filled()
        {
            string? expected = "DVD-ROM";
            string disc = Path.Combine(Environment.CurrentDirectory, "TestData", "DiscImageCreator", "DVD", "test_disc.txt");
            bool actual = DiscImageCreator.GetDiscType(disc, out string? discTypeOrBookType);
            Assert.True(actual);
            Assert.Equal(expected, discTypeOrBookType);
        }

        [Fact]
        public void GetDiscType_HDDVD_Filled()
        {
            string? expected = "HD DVD-ROM";
            string disc = Path.Combine(Environment.CurrentDirectory, "TestData", "DiscImageCreator", "HDDVD", "test_disc.txt");
            bool actual = DiscImageCreator.GetDiscType(disc, out string? discTypeOrBookType);
            Assert.True(actual);
            Assert.Equal(expected, discTypeOrBookType);
        }

        #endregion

        #region GetDVDProtection

        [Fact]
        public void GetDVDProtection_BothEmpty_Null()
        {
            string cssKey = string.Empty;
            string disc = string.Empty;
            string? actual = DiscImageCreator.GetDVDProtection(cssKey, disc, includeAlways: true);
            Assert.Null(actual);
        }

        [Fact]
        public void GetDVDProtection_BothInvalid_Null()
        {
            string cssKey = "INVALID";
            string disc = "INVALID";
            string? actual = DiscImageCreator.GetDVDProtection(cssKey, disc, includeAlways: true);
            Assert.Null(actual);
        }

        [Fact]
        public void GetDVDProtection_DiscOnlyNotAlways_Empty()
        {
            string? expected = string.Empty;
            string cssKey = string.Empty;
            string disc = Path.Combine(Environment.CurrentDirectory, "TestData", "DiscImageCreator", "CDROM", "test_disc.txt");
            string? actual = DiscImageCreator.GetDVDProtection(cssKey, disc, includeAlways: false);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetDVDProtection_DiscOnlyAlways_Filled()
        {
            string? expected = "Region: 1 2 3 4 5 6 7 8\nCopyright Protection System Type: No\n";
            string cssKey = string.Empty;
            string disc = Path.Combine(Environment.CurrentDirectory, "TestData", "DiscImageCreator", "CDROM", "test_disc.txt");
            string? actual = DiscImageCreator.GetDVDProtection(cssKey, disc, includeAlways: true);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetDVDProtection_CSSOnly_Filled()
        {
            string? expected = "FILE Title Key: No Title Key\nDecrypted Disc Key: No Key\n";
            string cssKey = Path.Combine(Environment.CurrentDirectory, "TestData", "DiscImageCreator", "CDROM", "test_CSSKey.txt");
            string disc = string.Empty;
            string? actual = DiscImageCreator.GetDVDProtection(cssKey, disc, includeAlways: true);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetDVDProtection_BothNotAlways_Filled()
        {
            string? expected = "FILE Title Key: No Title Key\nDecrypted Disc Key: No Key\n";
            string cssKey = Path.Combine(Environment.CurrentDirectory, "TestData", "DiscImageCreator", "CDROM", "test_CSSKey.txt");
            string disc = Path.Combine(Environment.CurrentDirectory, "TestData", "DiscImageCreator", "CDROM", "test_disc.txt");
            string? actual = DiscImageCreator.GetDVDProtection(cssKey, disc, includeAlways: false);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetDVDProtection_BothAlways_Filled()
        {
            string? expected = "Region: 1 2 3 4 5 6 7 8\nCopyright Protection System Type: No\nFILE Title Key: No Title Key\nDecrypted Disc Key: No Key\n";
            string cssKey = Path.Combine(Environment.CurrentDirectory, "TestData", "DiscImageCreator", "CDROM", "test_CSSKey.txt");
            string disc = Path.Combine(Environment.CurrentDirectory, "TestData", "DiscImageCreator", "CDROM", "test_disc.txt");
            string? actual = DiscImageCreator.GetDVDProtection(cssKey, disc, includeAlways: true);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetErrorCount

        [Fact]
        public void GetErrorCount_Empty_Null()
        {
            long expected = -1;
            string edcecc = string.Empty;
            long actual = DiscImageCreator.GetErrorCount(edcecc);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetErrorCount_Invalid_Null()
        {
            long expected = -1;
            string edcecc = "INVALID";
            long actual = DiscImageCreator.GetErrorCount(edcecc);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetErrorCount_Valid_Filled()
        {
            long expected = 12346;
            string edcecc = Path.Combine(Environment.CurrentDirectory, "TestData", "DiscImageCreator", "CDROM", "test.img_EdcEcc.txt");
            long actual = DiscImageCreator.GetErrorCount(edcecc);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetGDROMBuildInfo

        [Fact]
        public void GetGDROMBuildInfo_Null_Null()
        {
            string? segaHeader = null;
            bool actual = DiscImageCreator.GetGDROMBuildInfo(segaHeader,
                out string? buildDate,
                out string? serial,
                out string? version);

            Assert.False(actual);
            Assert.Null(buildDate);
            Assert.Null(serial);
            Assert.Null(version);
        }

        [Fact]
        public void GetGDROMBuildInfo_Empty_Null()
        {
            string? segaHeader = string.Empty;
            bool actual = DiscImageCreator.GetGDROMBuildInfo(segaHeader,
                out string? buildDate,
                out string? serial,
                out string? version);

            Assert.False(actual);
            Assert.Null(buildDate);
            Assert.Null(serial);
            Assert.Null(version);
        }

        [Fact]
        public void GetGDROMBuildInfo_Invalid_Null()
        {
            string? segaHeader = "INVALID";
            bool actual = DiscImageCreator.GetGDROMBuildInfo(segaHeader,
                out string? buildDate,
                out string? serial,
                out string? version);

            Assert.False(actual);
            Assert.Null(buildDate);
            Assert.Null(serial);
            Assert.Null(version);
        }

        [Fact]
        public void GetGDROMBuildInfo_Valid_Filled()
        {
            string? expectedDate = "1980-01-01";
            string? expectedSerial = "serial";
            string? expectedVersion = "ersio";

            string? segaHeader = "LINE0\nLINE1\nLINE2\nLINE3\nLINE4XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXserial    versio\nLINE5XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX19800101";
            bool actual = DiscImageCreator.GetGDROMBuildInfo(segaHeader,
                 out string? serial,
                 out string? version,
                 out string? date);

            Assert.True(actual);
            Assert.Equal(expectedSerial, serial);
            Assert.Equal(expectedVersion, version);
            Assert.Equal(expectedDate, date);
        }

        #endregion

        #region GetHardwareInfo

        [Fact]
        public void GetHardwareInfo_Empty_Null()
        {
            string drive = string.Empty;
            bool actual = DiscImageCreator.GetHardwareInfo(drive,
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
            string drive = "INVALID";
            bool actual = DiscImageCreator.GetHardwareInfo(drive,
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

            string drive = Path.Combine(Environment.CurrentDirectory, "TestData", "DiscImageCreator", "CDROM", "test_drive.txt");
            bool actual = DiscImageCreator.GetHardwareInfo(drive,
                out string? manufacturer,
                out string? model,
                out string? firmware);

            Assert.True(actual);
            Assert.Equal(expectedManufacturer, manufacturer);
            Assert.Equal(expectedModel, model);
            Assert.Equal(expectedFirmware, firmware);
        }

        #endregion

        #region GetLayerbreak

        [Fact]
        public void GetLayerbreak_Empty_Null()
        {
            string disc = string.Empty;
            string? actual = DiscImageCreator.GetLayerbreak(disc, xgd: false);
            Assert.Null(actual);
        }

        [Fact]
        public void GetLayerbreak_Invalid_Null()
        {
            string disc = "INVALID";
            string? actual = DiscImageCreator.GetLayerbreak(disc, xgd: false);
            Assert.Null(actual);
        }

        [Fact]
        public void GetLayerbreak_ValidStandard_Filled()
        {
            string? expected = "12345";
            string disc = Path.Combine(Environment.CurrentDirectory, "TestData", "DiscImageCreator", "CDROM", "test_disc.txt");
            string? actual = DiscImageCreator.GetLayerbreak(disc, xgd: false);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetLayerbreak_ValidXgd_Filled()
        {
            string? expected = "23456";
            string disc = Path.Combine(Environment.CurrentDirectory, "TestData", "DiscImageCreator", "CDROM", "test_disc.txt");
            string? actual = DiscImageCreator.GetLayerbreak(disc, xgd: true);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetLibCryptDetected

        // TODO: Figure out how to mock the subfile

        #endregion

        #region GetMultisessionInformation

        [Fact]
        public void GetMultisessionInformation_Empty_Null()
        {
            string disc = string.Empty;
            string? actual = DiscImageCreator.GetMultisessionInformation(disc);
            Assert.Null(actual);
        }

        [Fact]
        public void GetMultisessionInformation_Invalid_Null()
        {
            string disc = "INVALID";
            string? actual = DiscImageCreator.GetMultisessionInformation(disc);
            Assert.Null(actual);
        }

        [Fact]
        public void GetMultisessionInformation_Valid_Filled()
        {
            string? expected = "Session 1: 0-12344\nSession 2: 12346-23456";
            string disc = Path.Combine(Environment.CurrentDirectory, "TestData", "DiscImageCreator", "CDROM", "test_disc.txt");
            string? actual = DiscImageCreator.GetMultisessionInformation(disc);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetPlayStationAntiModchipDetected

        [Fact]
        public void GetPlayStationAntiModchipDetected_Empty_Null()
        {
            string disc = string.Empty;
            bool? actual = DiscImageCreator.GetPlayStationAntiModchipDetected(disc);
            Assert.Null(actual);
        }

        [Fact]
        public void GetPlayStationAntiModchipDetected_Invalid_Null()
        {
            string disc = "INVALID";
            bool? actual = DiscImageCreator.GetPlayStationAntiModchipDetected(disc);
            Assert.Null(actual);
        }

        [Fact]
        public void GetPlayStationAntiModchipDetected_Valid_Filled()
        {
            string disc = Path.Combine(Environment.CurrentDirectory, "TestData", "DiscImageCreator", "CDROM", "test_disc.txt");
            bool? actual = DiscImageCreator.GetPlayStationAntiModchipDetected(disc);
            Assert.True(actual);
        }

        #endregion

        #region GetPlayStation3Info

        [Fact]
        public void GetPlayStation3Info_Empty_Null()
        {
            string disc = string.Empty;
            bool actual = DiscImageCreator.GetPlayStation3Info(disc,
                out string? serial,
                out string? version,
                out string? firmwareVersion);

            Assert.False(actual);
            Assert.Null(serial);
            Assert.Null(version);
            Assert.Null(firmwareVersion);
        }

        [Fact]
        public void GetPlayStation3Info_Invalid_Null()
        {
            string disc = "INVALID";
            bool actual = DiscImageCreator.GetPlayStation3Info(disc,
                out string? serial,
                out string? version,
                out string? firmwareVersion);

            Assert.False(actual);
            Assert.Null(serial);
            Assert.Null(version);
            Assert.Null(firmwareVersion);
        }

        [Fact]
        public void GetPlayStation3Info_Valid_Filled()
        {
            string? expectedSerial = "serial";
            string? expectedVersion = "version";
            string? expectedFirmwareVersion = "firmwareVersion";

            string disc = Path.Combine(Environment.CurrentDirectory, "TestData", "DiscImageCreator", "CDROM", "test_disc.txt");
            bool actual = DiscImageCreator.GetPlayStation3Info(disc,
                out string? serial,
                out string? version,
                out string? firmwareVersion);

            Assert.True(actual);
            Assert.Equal(expectedSerial, serial);
            Assert.Equal(expectedVersion, version);
            Assert.Equal(expectedFirmwareVersion, firmwareVersion);
        }

        #endregion

        #region GetPlayStationEDCStatus

        [Fact]
        public void GetPlayStationEDCStatus_Empty_Null()
        {
            string edcecc = string.Empty;
            bool? actual = DiscImageCreator.GetPlayStationEDCStatus(edcecc);
            Assert.Null(actual);
        }

        [Fact]
        public void GetPlayStationEDCStatus_Invalid_Null()
        {
            string edcecc = "INVALID";
            bool? actual = DiscImageCreator.GetPlayStationEDCStatus(edcecc);
            Assert.Null(actual);
        }

        [Fact]
        public void GetPlayStationEDCStatus_Valid_Filled()
        {
            string edcecc = Path.Combine(Environment.CurrentDirectory, "TestData", "DiscImageCreator", "CDROM", "test.img_EdcEcc.txt");
            bool? actual = DiscImageCreator.GetPlayStationEDCStatus(edcecc);
            Assert.True(actual);
        }

        #endregion

        #region GetPVD

        [Fact]
        public void GetPVD_Empty_Null()
        {
            string mainInfo = string.Empty;
            string? actual = DiscImageCreator.GetPVD(mainInfo);
            Assert.Null(actual);
        }

        [Fact]
        public void GetPVD_Invalid_Null()
        {
            string mainInfo = "INVALID";
            string? actual = DiscImageCreator.GetPVD(mainInfo);
            Assert.Null(actual);
        }

        [Fact]
        public void GetPVD_Valid_Filled()
        {
            string? expected = "0320 TEST DATA\n0330 TEST DATA\n0340 TEST DATA\n0350 TEST DATA\n0360 TEST DATA\n0370 TEST DATA";
            string mainInfo = Path.Combine(Environment.CurrentDirectory, "TestData", "DiscImageCreator", "CDROM", "test_mainInfo.txt");
            string? actual = DiscImageCreator.GetPVD(mainInfo);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetSaturnBuildInfo

        [Fact]
        public void GetSaturnBuildInfo_Null_Null()
        {
            string? segaHeader = null;
            bool actual = DiscImageCreator.GetSaturnBuildInfo(segaHeader,
                out string? serial,
                out string? version,
                out string? date);

            Assert.False(actual);
            Assert.Null(serial);
            Assert.Null(version);
            Assert.Null(date);
        }

        [Fact]
        public void GetSaturnBuildInfo_Empty_Null()
        {
            string? segaHeader = string.Empty;
            bool actual = DiscImageCreator.GetSaturnBuildInfo(segaHeader,
                out string? serial,
                out string? version,
                out string? date);

            Assert.False(actual);
            Assert.Null(serial);
            Assert.Null(version);
            Assert.Null(date);
        }

        [Fact]
        public void GetSaturnBuildInfo_Invalid_Null()
        {
            string? segaHeader = "INVALID";
            bool actual = DiscImageCreator.GetSaturnBuildInfo(segaHeader,
                out string? serial,
                out string? version,
                out string? date);

            Assert.False(actual);
            Assert.Null(serial);
            Assert.Null(version);
            Assert.Null(date);
        }

        [Fact]
        public void GetSaturnBuildInfo_Valid_Filled()
        {
            string? expectedSerial = "serial";
            string? expectedVersion = "ersio";
            string? expectedDate = "1980-01-01";

            string? segaHeader = "LINE0\nLINE1\nLINE2XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXserial    versio\nLINE3XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX19800101";
            bool actual = DiscImageCreator.GetSaturnBuildInfo(segaHeader,
                out string? serial,
                out string? version,
                out string? date);

            Assert.True(actual);
            Assert.Equal(expectedSerial, serial);
            Assert.Equal(expectedVersion, version);
            Assert.Equal(expectedDate, date);
        }

        #endregion

        #region GetSecuROMData

        [Fact]
        public void GetSecuROMData_Empty_Null()
        {
            string log = string.Empty;
            string? actual = DiscImageCreator.GetSecuROMData(log, out var securomScheme);
            Assert.Null(actual);
            Assert.Equal(SecuROMScheme.None, securomScheme);
        }

        [Fact]
        public void GetSecuROMData_Invalid_Null()
        {
            string log = "INVALID";
            string? actual = DiscImageCreator.GetSecuROMData(log, out var securomScheme);
            Assert.Null(actual);
            Assert.Equal(SecuROMScheme.None, securomScheme);
        }

        [Fact]
        public void GetSecuROMData_Valid_Filled()
        {
            string? expected = "MSF: 00\nMSF: 01\nMSF: 02";
            string log = Path.Combine(Environment.CurrentDirectory, "TestData", "DiscImageCreator", "CDROM", "test_subIntention.txt");
            string? actual = DiscImageCreator.GetSecuROMData(log, out var securomScheme);
            Assert.Equal(expected, actual);
            Assert.Equal(SecuROMScheme.Unknown, securomScheme);
        }

        #endregion

        #region GetSegaCDBuildInfo

        [Fact]
        public void GetSegaCDBuildInfo_Null_Null()
        {
            string? segaHeader = null;
            bool actual = DiscImageCreator.GetSegaCDBuildInfo(segaHeader,
                out string? serial,
                out string? date);

            Assert.False(actual);
            Assert.Null(serial);
            Assert.Null(date);
        }

        [Fact]
        public void GetSegaCDBuildInfo_Empty_Null()
        {
            string? segaHeader = string.Empty;
            bool actual = DiscImageCreator.GetSegaCDBuildInfo(segaHeader,
                out string? serial,
                out string? date);

            Assert.False(actual);
            Assert.Null(serial);
            Assert.Null(date);
        }

        [Fact]
        public void GetSegaCDBuildInfo_Invalid_Null()
        {
            string? segaHeader = "INVALID";
            bool actual = DiscImageCreator.GetSegaCDBuildInfo(segaHeader,
                out string? serial,
                out string? date);

            Assert.False(actual);
            Assert.Null(serial);
            Assert.Null(date);
        }

        [Fact]
        public void GetSegaCDBuildInfo_Valid_Filled()
        {
            string? expectedSerial = "serial";
            string? expectedDate = "1980-01-01";

            string? segaHeader = "LINE0\nLINE1XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX        1980.JAN.01\nLINE2\nLINE3\nLINE4\nLINE5\nLINE6\nLINE7\nLINE8XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX   serial  ";
            bool actual = DiscImageCreator.GetSegaCDBuildInfo(segaHeader,
                out string? serial,
                out string? date);

            Assert.True(actual);
            Assert.Equal(expectedSerial, serial);
            Assert.Equal(expectedDate, date);
        }

        #endregion

        #region GetSegaHeader

        [Fact]
        public void GetSegaHeader_Empty_Null()
        {
            string mainInfo = string.Empty;
            string? actual = DiscImageCreator.GetSegaHeader(mainInfo);
            Assert.Null(actual);
        }

        [Fact]
        public void GetSegaHeader_Invalid_Null()
        {
            string mainInfo = "INVALID";
            string? actual = DiscImageCreator.GetSegaHeader(mainInfo);
            Assert.Null(actual);
        }

        [Fact]
        public void GetSegaHeader_Valid_Filled()
        {
            string? expected = "0000 TEST DATA\n0010 TEST DATA\n0020 TEST DATA\n0030 TEST DATA\n0040 TEST DATA\n0050 TEST DATA\n0060 TEST DATA\n0070 TEST DATA\n0080 TEST DATA\n0090 TEST DATA\n00A0 TEST DATA\n00B0 TEST DATA\n00C0 TEST DATA\n00D0 TEST DATA\n00E0 TEST DATA\n00F0 TEST DATA\n0100 TEST DATA\n0110 TEST DATA\n0120 TEST DATA\n0130 TEST DATA\n0140 TEST DATA\n0150 TEST DATA\n0160 TEST DATA\n0170 TEST DATA\n0180 TEST DATA\n0190 TEST DATA\n01A0 TEST DATA\n01B0 TEST DATA\n01C0 TEST DATA\n01D0 TEST DATA\n01E0 TEST DATA\n01F0 TEST DATA";
            string mainInfo = Path.Combine(Environment.CurrentDirectory, "TestData", "DiscImageCreator", "CDROM", "test_mainInfo.txt");
            string? actual = DiscImageCreator.GetSegaHeader(mainInfo);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetVolumeLabels

        [Fact]
        public void GetVolumeLabels_Empty_Null()
        {
            string volDesc = string.Empty;
            bool actual = DiscImageCreator.GetVolumeLabels(volDesc, out Dictionary<string, List<string>> volLabels);

            Assert.False(actual);
            Assert.Empty(volLabels);
        }

        [Fact]
        public void GetVolumeLabels_Invalid_Null()
        {
            string volDesc = "INVALID";
            bool actual = DiscImageCreator.GetVolumeLabels(volDesc, out Dictionary<string, List<string>> volLabels);

            Assert.False(actual);
            Assert.Empty(volLabels);
        }

        [Fact]
        public void GetVolumeLabels_Valid_Filled()
        {
            string volDesc = Path.Combine(Environment.CurrentDirectory, "TestData", "DiscImageCreator", "CDROM", "test_volDesc.txt");
            bool actual = DiscImageCreator.GetVolumeLabels(volDesc, out Dictionary<string, List<string>> volLabels);

            Assert.True(actual);
            KeyValuePair<string, List<string>> labelPair = Assert.Single(volLabels);
            Assert.Equal("label", labelPair.Key);
            string filesystem = Assert.Single(labelPair.Value);
            Assert.Equal("UDF", filesystem);
        }

        #endregion

        #region GetWriteOffset

        [Fact]
        public void GetWriteOffset_Empty_Null()
        {
            string disc = string.Empty;
            string? actual = DiscImageCreator.GetWriteOffset(disc);
            Assert.Null(actual);
        }

        [Fact]
        public void GetWriteOffset_Invalid_Null()
        {
            string disc = "INVALID";
            string? actual = DiscImageCreator.GetWriteOffset(disc);
            Assert.Null(actual);
        }

        [Fact]
        public void GetWriteOffset_Valid_Filled()
        {
            string? expected = "offset";
            string disc = Path.Combine(Environment.CurrentDirectory, "TestData", "DiscImageCreator", "CDROM", "test_disc.txt");
            string? actual = DiscImageCreator.GetWriteOffset(disc);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetXGDAuxHashInfo

        [Fact]
        public void GetXGDAuxHashInfo_Null_Null()
        {
            Datafile? suppl = null;
            bool actual = DiscImageCreator.GetXGDAuxHashInfo(suppl,
                out string? dmihash,
                out string? pfihash,
                out string? sshash);

            Assert.False(actual);
            Assert.Null(dmihash);
            Assert.Null(pfihash);
            Assert.Null(sshash);
        }

        [Fact]
        public void GetXGDAuxHashInfo_Invalid_Null()
        {
            Datafile? suppl = new();
            bool actual = DiscImageCreator.GetXGDAuxHashInfo(suppl,
                out string? dmihash,
                out string? pfihash,
                out string? sshash);

            Assert.False(actual);
            Assert.Null(dmihash);
            Assert.Null(pfihash);
            Assert.Null(sshash);
        }

        [Fact]
        public void GetXGDAuxHashInfo_Valid_Filled()
        {
            Datafile? suppl = new()
            {
                Game =
                [
                    new Game
                    {
                        Rom =
                        [
                            new Rom { Name = "DMI.bin", CRC = "00000000" },
                            new Rom { Name = "PFI.bin", CRC = "00000000" },
                            new Rom { Name = "SS.bin", CRC = "00000000" },
                        ],
                    }
                ],
            };
            bool actual = DiscImageCreator.GetXGDAuxHashInfo(suppl,
                out string? dmihash,
                out string? pfihash,
                out string? sshash);

            Assert.True(actual);
            Assert.Equal("00000000", dmihash);
            Assert.Equal("00000000", pfihash);
            Assert.Equal("00000000", sshash);
        }

        #endregion

        #region GetXGDAuxInfo

        [Fact]
        public void GetXGDAuxInfo_Null_Null()
        {
            string disc = string.Empty;
            bool actual = DiscImageCreator.GetXGDAuxInfo(disc,
                out string? dmihash,
                out string? pfihash,
                out string? sshash,
                out string? ss);

            Assert.False(actual);
            Assert.Null(dmihash);
            Assert.Null(pfihash);
            Assert.Null(sshash);
            Assert.Null(ss);
        }

        [Fact]
        public void GetXGDAuxInfo_Invalid_Null()
        {
            string disc = "INVALID";
            bool actual = DiscImageCreator.GetXGDAuxInfo(disc,
                out string? dmihash,
                out string? pfihash,
                out string? sshash,
                out string? ss);

            Assert.False(actual);
            Assert.Null(dmihash);
            Assert.Null(pfihash);
            Assert.Null(sshash);
            Assert.Null(ss);
        }

        [Fact]
        public void GetXGDAuxInfo_Valid_Filled()
        {
            string? expectedSs = "0-12345";
            string disc = Path.Combine(Environment.CurrentDirectory, "TestData", "DiscImageCreator", "CDROM", "test_disc.txt");
            bool actual = DiscImageCreator.GetXGDAuxInfo(disc,
                out string? dmihash,
                out string? pfihash,
                out string? sshash,
                out string? ss);

            Assert.True(actual);
            Assert.Equal("00000000", dmihash);
            Assert.Equal("00000000", pfihash);
            Assert.Equal("00000000", sshash);
            Assert.Equal(expectedSs, ss);
        }

        #endregion
    }
}
