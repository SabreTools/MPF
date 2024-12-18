using System;
using System.Collections.Generic;
using System.IO;
using SabreTools.RedumpLib.Data;
using Xunit;

namespace MPF.Processors.Test
{
    public class RedumperTests
    {
        #region GetOutputFiles

        [Fact]
        public void GetOutputFiles_Null_Empty()
        {
            string? baseDirectory = null;
            string baseFilename = "test";
            var processor = new Redumper(RedumpSystem.IBMPCcompatible, null);

            var actual = processor.GetOutputFiles(baseDirectory, baseFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetOutputFiles_CDROM_Populated()
        {
            string? baseDirectory = null;
            string baseFilename = "test";
            var processor = new Redumper(RedumpSystem.IBMPCcompatible, MediaType.CDROM);

            var actual = processor.GetOutputFiles(baseDirectory, baseFilename);
            Assert.Equal(14, actual.Count);
        }

        [Fact]
        public void GetOutputFiles_DVD_Populated()
        {
            string? baseDirectory = null;
            string baseFilename = "test";
            var processor = new Redumper(RedumpSystem.IBMPCcompatible, MediaType.DVD);

            var actual = processor.GetOutputFiles(baseDirectory, baseFilename);
            Assert.Equal(16, actual.Count);
        }

        [Fact]
        public void GetOutputFiles_HDDVD_Populated()
        {
            string? baseDirectory = null;
            string baseFilename = "test";
            var processor = new Redumper(RedumpSystem.IBMPCcompatible, MediaType.HDDVD);

            var actual = processor.GetOutputFiles(baseDirectory, baseFilename);
            Assert.Equal(10, actual.Count);
        }

        [Fact]
        public void GetOutputFiles_BluRay_Populated()
        {
            string? baseDirectory = null;
            string baseFilename = "test";
            var processor = new Redumper(RedumpSystem.IBMPCcompatible, MediaType.BluRay);

            var actual = processor.GetOutputFiles(baseDirectory, baseFilename);
            Assert.Equal(10, actual.Count);
        }

        [Fact]
        public void GetOutputFiles_Other_Empty()
        {
            string? baseDirectory = null;
            string baseFilename = "test";
            var processor = new Redumper(RedumpSystem.IBMPCcompatible, MediaType.ApertureCard);

            var actual = processor.GetOutputFiles(baseDirectory, baseFilename);
            Assert.Empty(actual);
        }

        #endregion

        #region GenerateArtifacts

        [Fact]
        public void GenerateArtifacts_Invalid_Empty()
        {
            string basePath = string.Empty;
            var processor = new Redumper(RedumpSystem.IBMPCcompatible, MediaType.CDROM);
            var actual = processor.GenerateArtifacts(basePath);
            Assert.Empty(actual);
        }

        [Fact]
        public void GenerateArtifacts_Valid_Filled()
        {
            string? basePath = Path.Combine(Environment.CurrentDirectory, "TestData", "Redumper", "CDROM", "test");
            var processor = new Redumper(RedumpSystem.IBMPCcompatible, MediaType.CDROM);
            var actual = processor.GenerateArtifacts(basePath);
            Assert.Equal(11, actual.Count);
        }

        #endregion

        #region CheckRequiredFiles

        [Fact]
        public void CheckRequiredFiles_Invalid_Filled()
        {
            string? baseDirectory = null;
            string baseFilename = string.Empty;
            var processor = new Redumper(RedumpSystem.IBMPCcompatible, MediaType.CDROM);
            var actual = processor.CheckRequiredFiles(baseDirectory, baseFilename);
            Assert.Equal(8, actual.Count);
        }

        [Fact]
        public void CheckRequiredFiles_Valid_Empty()
        {
            string? baseDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "Redumper", "CDROM");
            string baseFilename = "test";
            var processor = new Redumper(RedumpSystem.IBMPCcompatible, MediaType.CDROM);
            var actual = processor.CheckRequiredFiles(baseDirectory, baseFilename);
            Assert.Empty(actual);
        }

        #endregion

        #region GetDeleteableFilePaths

        [Fact]
        public void GetDeleteableFilePaths_Invalid_Empty()
        {
            string basePath = string.Empty;
            var processor = new Redumper(RedumpSystem.IBMPCcompatible, MediaType.CDROM);
            var actual = processor.GetDeleteableFilePaths(basePath);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetDeleteableFilePaths_Valid_Single()
        {
            string? basePath = Path.Combine(Environment.CurrentDirectory, "TestData", "Redumper", "CDROM", "test");
            var processor = new Redumper(RedumpSystem.IBMPCcompatible, MediaType.CDROM);
            var actual = processor.GetDeleteableFilePaths(basePath);
            Assert.Single(actual);
        }

        #endregion

        #region GetZippableFilePaths

        [Fact]
        public void GetZippableFilePaths_Invalid_Empty()
        {
            string basePath = string.Empty;
            var processor = new Redumper(RedumpSystem.IBMPCcompatible, MediaType.CDROM);
            var actual = processor.GetZippableFilePaths(basePath);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetZippableFilePaths_Valid_Filled()
        {
            string? basePath = Path.Combine(Environment.CurrentDirectory, "TestData", "Redumper", "CDROM", "test");
            var processor = new Redumper(RedumpSystem.IBMPCcompatible, MediaType.CDROM);
            var actual = processor.GetZippableFilePaths(basePath);
            Assert.Equal(11, actual.Count);
        }

        #endregion

        #region GetCuesheet

        [Fact]
        public void GetCuesheet_Empty_Null()
        {
            string log = string.Empty;
            string? actual = Redumper.GetCuesheet(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetCuesheet_Invalid_Null()
        {
            string log = "INVALID";
            string? actual = Redumper.GetCuesheet(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetCuesheet_Valid_Filled()
        {
            string? expected = "cuesheet";
            string log = Path.Combine(Environment.CurrentDirectory, "TestData", "Redumper", "CDROM", "test.log");
            string? actual = Redumper.GetCuesheet(log);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetDatfile

        [Fact]
        public void GetDatfile_Empty_Null()
        {
            string log = string.Empty;
            string? actual = Redumper.GetDatfile(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetDatfile_Invalid_Null()
        {
            string log = "INVALID";
            string? actual = Redumper.GetDatfile(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetDatfile_Valid_Filled()
        {
            string? expected = "<rom name=\"INVALID\" size=\"12345\" crc=\"00000000\" md5=\"d41d8cd98f00b204e9800998ecf8427e\" sha1=\"da39a3ee5e6b4b0d3255bfef95601890afd80709\" />";
            string log = Path.Combine(Environment.CurrentDirectory, "TestData", "Redumper", "CDROM", "test.log");
            string? actual = Redumper.GetDatfile(log);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetDiscType

        [Fact]
        public void GetDiscType_Empty_Null()
        {
            string log = string.Empty;
            bool actual = Redumper.GetDiscType(log, out string? discTypeOrBookType);
            Assert.False(actual);
        }

        [Fact]
        public void GetDiscType_Invalid_Null()
        {
            string log = "INVALID";
            bool actual = Redumper.GetDiscType(log, out string? discTypeOrBookType);
            Assert.False(actual);
        }

        [Fact]
        public void GetDiscType_Valid_Filled()
        {
            string? expected = "CD-ROM";
            string log = Path.Combine(Environment.CurrentDirectory, "TestData", "Redumper", "CDROM", "test.log");
            bool actual = Redumper.GetDiscType(log, out string? discTypeOrBookType);
            Assert.True(actual);
            Assert.Equal(expected, discTypeOrBookType);
        }

        #endregion

        #region GetDVDProtection

        [Fact]
        public void GetDVDProtection_Empty_Null()
        {
            string log = string.Empty;
            string? actual = Redumper.GetDVDProtection(log, includeAlways: true);
            Assert.Null(actual);
        }

        [Fact]
        public void GetDVDProtection_Invalid_Null()
        {
            string log = "INVALID";
            string? actual = Redumper.GetDVDProtection(log, includeAlways: true);
            Assert.Null(actual);
        }

        [Fact]
        public void GetDVDProtection_ValidNotAlways_Filled()
        {
            string? expected = "FILE Title Key: No Title Key\nDecrypted Disc Key: No Key\n";
            string log = Path.Combine(Environment.CurrentDirectory, "TestData", "Redumper", "CDROM", "test.log");
            string? actual = Redumper.GetDVDProtection(log, includeAlways: false);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetDVDProtection_ValidAlways_Filled()
        {
            string? expected = "Region: 1 2 3 4 5 6 7 8\nCopyright Protection System Type: No\nFILE Title Key: No Title Key\nDecrypted Disc Key: No Key\n";
            string log = Path.Combine(Environment.CurrentDirectory, "TestData", "Redumper", "CDROM", "test.log");
            string? actual = Redumper.GetDVDProtection(log, includeAlways: true);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetErrorCount

        [Fact]
        public void GetErrorCount_Empty_Null()
        {
            long expectedRedumpErrors = -1;
            long expectedC2Errors = -1;
            string log = string.Empty;
            bool actual = Redumper.GetErrorCount(log, out long redumpErrors, out long c2Errors);

            Assert.False(actual);
            Assert.Equal(expectedRedumpErrors, redumpErrors);
            Assert.Equal(expectedC2Errors, c2Errors);
        }

        [Fact]
        public void GetErrorCount_Invalid_Null()
        {
            long expectedRedumpErrors = -1;
            long expectedC2Errors = -1;
            string log = "INVALID";
            bool actual = Redumper.GetErrorCount(log, out long redumpErrors, out long c2Errors);

            Assert.False(actual);
            Assert.Equal(expectedRedumpErrors, redumpErrors);
            Assert.Equal(expectedC2Errors, c2Errors);
        }

        [Fact]
        public void GetErrorCount_Valid_Filled()
        {
            long expectedRedumpErrors = 12347;
            long expectedC2Errors = 12346;
            string log = Path.Combine(Environment.CurrentDirectory, "TestData", "Redumper", "CDROM", "test.log");
            bool actual = Redumper.GetErrorCount(log, out long redumpErrors, out long c2Errors);

            Assert.True(actual);
            Assert.Equal(expectedRedumpErrors, redumpErrors);
            Assert.Equal(expectedC2Errors, c2Errors);
        }

        #endregion

        #region GetGDROMHeader

        [Fact]
        public void GetGDROMHeader_Empty_Null()
        {
            string log = string.Empty;
            string? actual = Redumper.GetGDROMHeader(log,
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
            string? actual = Redumper.GetGDROMHeader(log,
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

            string log = Path.Combine(Environment.CurrentDirectory, "TestData", "Redumper", "CDROM", "test.log");
            string? actual = Redumper.GetGDROMHeader(log,
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
            bool actual = Redumper.GetHardwareInfo(log,
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
            bool actual = Redumper.GetHardwareInfo(log,
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

            string log = Path.Combine(Environment.CurrentDirectory, "TestData", "Redumper", "CDROM", "test.log");
            bool actual = Redumper.GetHardwareInfo(log,
                out string? manufacturer,
                out string? model,
                out string? firmware);

            Assert.True(actual);
            Assert.Equal(expectedManufacturer, manufacturer);
            Assert.Equal(expectedModel, model);
            Assert.Equal(expectedFirmware, firmware);
        }

        #endregion

        #region GetLayerbreaks

        [Fact]
        public void GetLayerbreaks_Empty_Null()
        {
            string log = string.Empty;
            bool actual = Redumper.GetLayerbreaks(log,
                out string? layerbreak1,
                out string? layerbreak2,
                out string? layerbreak3);

            Assert.False(actual);
            Assert.Null(layerbreak1);
            Assert.Null(layerbreak2);
            Assert.Null(layerbreak3);
        }

        [Fact]
        public void GetLayerbreaks_Invalid_Null()
        {
            string log = "INVALID";
            bool actual = Redumper.GetLayerbreaks(log,
                out string? layerbreak1,
                out string? layerbreak2,
                out string? layerbreak3);

            Assert.False(actual);
            Assert.Null(layerbreak1);
            Assert.Null(layerbreak2);
            Assert.Null(layerbreak3);
        }

        [Fact]
        public void GetLayerbreaks_Valid_Filled()
        {
            string? expectedLayerbreak1 = "12345";
            string? expectedLayerbreak2 = "23456";
            string? expectedLayerbreak3 = "34567";

            string log = Path.Combine(Environment.CurrentDirectory, "TestData", "Redumper", "CDROM", "test.log");
            bool actual = Redumper.GetLayerbreaks(log,
                out string? layerbreak1,
                out string? layerbreak2,
                out string? layerbreak3);

            Assert.True(actual);
            Assert.Equal(expectedLayerbreak1, layerbreak1);
            Assert.Equal(expectedLayerbreak2, layerbreak2);
            Assert.Equal(expectedLayerbreak3, layerbreak3);
        }

        #endregion

        #region GetMultisessionInformation

        [Fact]
        public void GetMultisessionInformation_Empty_Null()
        {
            string log = string.Empty;
            string? actual = Redumper.GetMultisessionInformation(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetMultisessionInformation_Invalid_Null()
        {
            string log = "INVALID";
            string? actual = Redumper.GetMultisessionInformation(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetMultisessionInformation_Valid_Filled()
        {
            string? expected = "Session 1: 0-12344\nSession 2: 12345-23456";
            string log = Path.Combine(Environment.CurrentDirectory, "TestData", "Redumper", "CDROM", "test.log");
            string? actual = Redumper.GetMultisessionInformation(log);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetPlayStationAntiModchipDetected

        [Fact]
        public void GetPlayStationAntiModchipDetected_Empty_Null()
        {
            string log = string.Empty;
            bool? actual = Redumper.GetPlayStationAntiModchipDetected(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetPlayStationAntiModchipDetected_Invalid_Null()
        {
            string log = "INVALID";
            bool? actual = Redumper.GetPlayStationAntiModchipDetected(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetPlayStationAntiModchipDetected_Valid_Filled()
        {
            string log = Path.Combine(Environment.CurrentDirectory, "TestData", "Redumper", "CDROM", "test.log");
            bool? actual = Redumper.GetPlayStationAntiModchipDetected(log);
            Assert.True(actual);
        }

        #endregion

        #region GetPlayStationEDCStatus

        [Fact]
        public void GetPlayStationEDCStatus_Empty_Null()
        {
            string log = string.Empty;
            bool? actual = Redumper.GetPlayStationEDCStatus(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetPlayStationEDCStatus_Invalid_Null()
        {
            string log = "INVALID";
            bool? actual = Redumper.GetPlayStationEDCStatus(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetPlayStationEDCStatus_Valid_Filled()
        {
            string log = Path.Combine(Environment.CurrentDirectory, "TestData", "Redumper", "CDROM", "test.log");
            bool? actual = Redumper.GetPlayStationEDCStatus(log);
            Assert.True(actual);
        }

        #endregion

        #region GetPlayStationInfo

        [Fact]
        public void GetPlayStationInfo_Empty_Null()
        {
            string log = string.Empty;
            bool actual = Redumper.GetPlayStationInfo(log,
                out string? exeDate,
                out string? serial,
                out string? version);

            Assert.False(actual);
            Assert.Null(exeDate);
            Assert.Null(serial);
            Assert.Null(version);
        }

        [Fact]
        public void GetPlayStationInfo_Invalid_Null()
        {
            string log = "INVALID";
            bool actual = Redumper.GetPlayStationInfo(log,
                out string? exeDate,
                out string? serial,
                out string? version);

            Assert.False(actual);
            Assert.Null(exeDate);
            Assert.Null(serial);
            Assert.Null(version);
        }

        [Fact]
        public void GetPlayStationInfo_Valid_Filled()
        {
            string? expectedExeDate = "date";
            string? expectedSerial = "serial";
            string? expectedVersion = "version";

            string log = Path.Combine(Environment.CurrentDirectory, "TestData", "Redumper", "CDROM", "test.log");
            bool actual = Redumper.GetPlayStationInfo(log,
                out string? exeDate,
                out string? serial,
                out string? version);

            Assert.True(actual);
            Assert.Equal(expectedExeDate, exeDate);
            Assert.Equal(expectedSerial, serial);
            Assert.Equal(expectedVersion, version);
        }

        #endregion

        #region GetPlayStationLibCryptData

        [Fact]
        public void GetPlayStationLibCryptData_Empty_Null()
        {
            string log = string.Empty;
            string? actual = Redumper.GetPlayStationLibCryptData(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetPlayStationLibCryptData_Invalid_Null()
        {
            string log = "INVALID";
            string? actual = Redumper.GetPlayStationLibCryptData(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetPlayStationLibCryptData_Valid_Filled()
        {
            string? expected = "MSF: 00\nMSF: 01\nMSF: 02";
            string log = Path.Combine(Environment.CurrentDirectory, "TestData", "Redumper", "CDROM", "test.log");
            string? actual = Redumper.GetPlayStationLibCryptData(log);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetPlayStationLibCryptStatus

        [Fact]
        public void GetPlayStationLibCryptStatus_Empty_Null()
        {
            string log = string.Empty;
            bool? actual = Redumper.GetPlayStationLibCryptStatus(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetPlayStationLibCryptStatus_Invalid_Null()
        {
            string log = "INVALID";
            bool? actual = Redumper.GetPlayStationLibCryptStatus(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetPlayStationLibCryptStatus_Valid_Filled()
        {
            string log = Path.Combine(Environment.CurrentDirectory, "TestData", "Redumper", "CDROM", "test.log");
            bool? actual = Redumper.GetPlayStationLibCryptStatus(log);
            Assert.True(actual);
        }

        #endregion

        #region GetPVD

        [Fact]
        public void GetPVD_Empty_Null()
        {
            string log = string.Empty;
            string? actual = Redumper.GetPVD(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetPVD_Invalid_Null()
        {
            string log = "INVALID";
            string? actual = Redumper.GetPVD(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetPVD_Valid_Filled()
        {
            string? expected = "0320 TEST DATA\n0330 TEST DATA\n0340 TEST DATA\n0350 TEST DATA\n0360 TEST DATA\n0370 TEST DATA";
            string log = Path.Combine(Environment.CurrentDirectory, "TestData", "Redumper", "CDROM", "test.log");
            string? actual = Redumper.GetPVD(log);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetRingNonZeroDataStart

        [Fact]
        public void GetRingNonZeroDataStart_Empty_Null()
        {
            string log = string.Empty;
            string? actual = Redumper.GetRingNonZeroDataStart(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetRingNonZeroDataStart_Invalid_Null()
        {
            string log = "INVALID";
            string? actual = Redumper.GetRingNonZeroDataStart(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetRingNonZeroDataStart_Valid_Filled()
        {
            string? expected = "12345";
            string log = Path.Combine(Environment.CurrentDirectory, "TestData", "Redumper", "CDROM", "test.log");
            string? actual = Redumper.GetRingNonZeroDataStart(log);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetSaturnBuildInfo

        [Fact]
        public void GetSaturnBuildInfo_Null_Null()
        {
            string? segaHeader = null;
            bool actual = Redumper.GetSaturnBuildInfo(segaHeader,
                out string? buildDate,
                out string? serial,
                out string? version);

            Assert.False(actual);
            Assert.Null(buildDate);
            Assert.Null(serial);
            Assert.Null(version);
        }

        [Fact]
        public void GetSaturnBuildInfo_Empty_Null()
        {
            string? segaHeader = string.Empty;
            bool actual = Redumper.GetSaturnBuildInfo(segaHeader,
                out string? buildDate,
                out string? serial,
                out string? version);

            Assert.False(actual);
            Assert.Null(buildDate);
            Assert.Null(serial);
            Assert.Null(version);
        }

        [Fact]
        public void GetSaturnBuildInfo_Invalid_Null()
        {
            string? segaHeader = "INVALID";
            bool actual = Redumper.GetSaturnBuildInfo(segaHeader,
                out string? buildDate,
                out string? serial,
                out string? version);

            Assert.False(actual);
            Assert.Null(buildDate);
            Assert.Null(serial);
            Assert.Null(version);
        }

        [Fact]
        public void GetSaturnBuildInfo_Valid_Filled()
        {
            string? expectedBuildDate = "1980-01-01";
            string? expectedSerial = "serial";
            string? expectedVersion = "ersio";

            string? segaHeader = "LINE0\nLINE1\nLINE2XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXserial    versio\nLINE3XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX19800101";
            bool actual = Redumper.GetSaturnBuildInfo(segaHeader,
                 out string? buildDate,
                 out string? serial,
                 out string? version);

            Assert.True(actual);
            Assert.Equal(expectedBuildDate, buildDate);
            Assert.Equal(expectedSerial, serial);
            Assert.Equal(expectedVersion, version);
        }

        #endregion

        #region GetSaturnHeader

        [Fact]
        public void GetSaturnHeader_Empty_Null()
        {
            string log = string.Empty;
            string? actual = Redumper.GetSaturnHeader(log,
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
        public void GetSaturnHeader_Invalid_Null()
        {
            string log = "INVALID";
            string? actual = Redumper.GetSaturnHeader(log,
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
        public void GetSaturnHeader_Valid_Filled()
        {
            string? expected = "0000 TEST DATA\n0010 TEST DATA\n0020 TEST DATA\n0030 TEST DATA\n0040 TEST DATA\n0050 TEST DATA\n0060 TEST DATA\n0070 TEST DATA\n0080 TEST DATA\n0090 TEST DATA\n00A0 TEST DATA\n00B0 TEST DATA\n00C0 TEST DATA\n00D0 TEST DATA\n00E0 TEST DATA\n00F0 TEST DATA";
            string? expectedBuildDate = "date";
            string? expectedSerial = "serial";
            string? expectedRegion = "region";
            string? expectedVersion = "version";

            string log = Path.Combine(Environment.CurrentDirectory, "TestData", "Redumper", "CDROM", "test.log");
            string? actual = Redumper.GetSaturnHeader(log,
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

        #region GetSecuROMData

        [Fact]
        public void GetSecuROMData_Empty_Null()
        {
            string log = string.Empty;
            string? actual = Redumper.GetSecuROMData(log, out var securomScheme);
            Assert.Null(actual);
        }

        [Fact]
        public void GetSecuROMData_Invalid_Null()
        {
            string log = "INVALID";
            string? actual = Redumper.GetSecuROMData(log, out var securomScheme);
            Assert.Null(actual);
        }

        [Fact]
        public void GetSecuROMData_Valid_Filled()
        {
            string? expected = "MSF: 00\nMSF: 01\nMSF: 02";
            string log = Path.Combine(Environment.CurrentDirectory, "TestData", "Redumper", "CDROM", "test.log");
            string? actual = Redumper.GetSecuROMData(log, out var securomScheme);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetSegaCDHeader

        [Fact]
        public void GetSegaCDHeader_Empty_Null()
        {
            string log = string.Empty;
            string? actual = Redumper.GetSegaCDHeader(log,
                out string? buildDate,
                out string? serial,
                out string? region);

            Assert.Null(actual);
            Assert.Null(buildDate);
            Assert.Null(serial);
            Assert.Null(region);
        }

        [Fact]
        public void GetSegaCDHeader_Invalid_Null()
        {
            string log = "INVALID";
            string? actual = Redumper.GetSegaCDHeader(log,
                out string? buildDate,
                out string? serial,
                out string? region);

            Assert.Null(actual);
            Assert.Null(buildDate);
            Assert.Null(serial);
            Assert.Null(region);
        }

        [Fact]
        public void GetSegaCDHeader_Valid_Filled()
        {
            string? expected = "0100 TEST DATA\n0110 TEST DATA\n0120 TEST DATA\n0130 TEST DATA\n0140 TEST DATA\n0150 TEST DATA\n0160 TEST DATA\n0170 TEST DATA\n0180 TEST DATA\n0190 TEST DATA\n01A0 TEST DATA";
            string? expectedBuildDate = "date";
            string? expectedSerial = "serial";
            string? expectedRegion = "region";

            string log = Path.Combine(Environment.CurrentDirectory, "TestData", "Redumper", "CDROM", "test.log");
            string? actual = Redumper.GetSegaCDHeader(log,
                out string? buildDate,
                out string? serial,
                out string? region);

            Assert.Equal(expected, actual);
            Assert.Equal(expectedBuildDate, buildDate);
            Assert.Equal(expectedSerial, serial);
            Assert.Equal(expectedRegion, region);
        }

        #endregion

        #region GetUniversalHash

        [Fact]
        public void GetUniversalHash_Empty_Null()
        {
            string log = string.Empty;
            string? actual = Redumper.GetUniversalHash(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetUniversalHash_Invalid_Null()
        {
            string log = "INVALID";
            string? actual = Redumper.GetUniversalHash(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetUniversalHash_Valid_Filled()
        {
            string? expected = "da39a3ee5e6b4b0d3255bfef95601890afd80709";
            string log = Path.Combine(Environment.CurrentDirectory, "TestData", "Redumper", "CDROM", "test.log");
            string? actual = Redumper.GetUniversalHash(log);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetParameters

        [Fact]
        public void GetParameters_Empty_Null()
        {
            string log = string.Empty;
            string? actual = Redumper.GetParameters(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetParameters_Invalid_Null()
        {
            string log = "INVALID";
            string? actual = Redumper.GetParameters(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetParameters_Valid_Filled()
        {
            string? expected = "cd --verbose";
            string log = Path.Combine(Environment.CurrentDirectory, "TestData", "Redumper", "CDROM", "test.log");
            string? actual = Redumper.GetParameters(log);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetVersion

        [Fact]
        public void GetVersion_Empty_Null()
        {
            string log = string.Empty;
            string? actual = Redumper.GetVersion(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetVersion_Invalid_Null()
        {
            string log = "INVALID";
            string? actual = Redumper.GetVersion(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetVersion_Valid_Filled()
        {
            string? expected = "v1980.01.01 build_00";
            string log = Path.Combine(Environment.CurrentDirectory, "TestData", "Redumper", "CDROM", "test.log");
            string? actual = Redumper.GetVersion(log);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetVolumeLabels

        [Fact]
        public void GetVolumeLabels_Empty_Null()
        {
            string log = string.Empty;
            bool actual = Redumper.GetVolumeLabels(log, out Dictionary<string, List<string>> volLabels);

            Assert.False(actual);
            Assert.Empty(volLabels);
        }

        [Fact]
        public void GetVolumeLabels_Invalid_Null()
        {
            string log = "INVALID";
            bool actual = Redumper.GetVolumeLabels(log, out Dictionary<string, List<string>> volLabels);

            Assert.False(actual);
            Assert.Empty(volLabels);
        }

        [Fact]
        public void GetVolumeLabels_Valid_Filled()
        {
            string log = Path.Combine(Environment.CurrentDirectory, "TestData", "Redumper", "CDROM", "test.log");
            bool actual = Redumper.GetVolumeLabels(log, out Dictionary<string, List<string>> volLabels);

            Assert.True(actual);
            KeyValuePair<string, List<string>> labelPair = Assert.Single(volLabels);
            Assert.Equal("label", labelPair.Key);
            string filesystem = Assert.Single(labelPair.Value);
            Assert.Equal("ISO", filesystem);
        }

        #endregion

        #region GetWriteOffset

        [Fact]
        public void GetWriteOffset_Empty_Null()
        {
            string log = string.Empty;
            string? actual = Redumper.GetWriteOffset(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetWriteOffset_Invalid_Null()
        {
            string log = "INVALID";
            string? actual = Redumper.GetWriteOffset(log);
            Assert.Null(actual);
        }

        [Fact]
        public void GetWriteOffset_Valid_Filled()
        {
            string? expected = "offset";
            string log = Path.Combine(Environment.CurrentDirectory, "TestData", "Redumper", "CDROM", "test.log");
            string? actual = Redumper.GetWriteOffset(log);
            Assert.Equal(expected, actual);
        }

        #endregion
    }
}