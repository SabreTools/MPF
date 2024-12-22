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
            string? baseDirectory = null;
            string baseFilename = string.Empty;
            var processor = new CleanRip(RedumpSystem.NintendoGameCube, MediaType.DVD);
            var actual = processor.CheckRequiredFiles(baseDirectory, baseFilename);
            Assert.Equal(3, actual.Count);
        }

        [Fact]
        public void CheckRequiredFiles_Valid_Empty()
        {
            string? baseDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "CleanRip", "DVD");
            string baseFilename = "test";
            var processor = new CleanRip(RedumpSystem.NintendoGameCube, MediaType.DVD);
            var actual = processor.CheckRequiredFiles(baseDirectory, baseFilename);
            Assert.Empty(actual);
        }

        #endregion

        #region CheckExistingFiles

        [Fact]
        public void CheckExistingFiles_Invalid_Filled()
        {
            string? baseDirectory = null;
            string baseFilename = string.Empty;
            var processor = new CleanRip(RedumpSystem.NintendoGameCube, MediaType.DVD);
            var actual = processor.CheckExistingFiles(baseDirectory, baseFilename);
            Assert.False(actual);
        }

        [Fact]
        public void CheckExistingFiles_Valid_Empty()
        {
            string? baseDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "CleanRip", "DVD");
            string baseFilename = "test";
            var processor = new CleanRip(RedumpSystem.NintendoGameCube, MediaType.DVD);
            var actual = processor.CheckExistingFiles(baseDirectory, baseFilename);
            Assert.True(actual);
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

        #region GenerateCleanripDatafile

        [Fact]
        public void GenerateCleanripDatafile_NoIsoNoDumpinfo_Null()
        {
            string iso = "INVALID";
            string dumpinfo = "INVALID";

            var actual = CleanRip.GenerateCleanripDatafile(iso, dumpinfo);
            Assert.Null(actual);
        }

        [Fact]
        public void GenerateCleanripDatafile_IsoOnly_Filled()
        {
            string iso = Path.Combine(Environment.CurrentDirectory, "TestData", "CleanRip", "DVD", "test.iso");
            string dumpinfo = "INVALID";

            var actual = CleanRip.GenerateCleanripDatafile(iso, dumpinfo);
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

        [Fact]
        public void GenerateCleanripDatafile_DumpinfoOnly_Filled()
        {
            string iso = "INVALID";
            string dumpinfo = Path.Combine(Environment.CurrentDirectory, "TestData", "CleanRip", "DVD", "test-dumpinfo.txt");

            var actual = CleanRip.GenerateCleanripDatafile(iso, dumpinfo);
            Assert.NotNull(actual);

            Assert.NotNull(actual.Game);
            var game = Assert.Single(actual.Game);

            Assert.NotNull(game.Rom);
            var rom = Assert.Single(game.Rom);
            Assert.Equal("-1", rom.Size);
            Assert.Equal("00000000", rom.CRC);
            Assert.Equal("d41d8cd98f00b204e9800998ecf8427e", rom.MD5);
            Assert.Equal("da39a3ee5e6b4b0d3255bfef95601890afd80709", rom.SHA1);
        }

        [Fact]
        public void GenerateCleanripDatafile_BothValid_Filled()
        {
            string iso = Path.Combine(Environment.CurrentDirectory, "TestData", "CleanRip", "DVD", "test.iso");
            string dumpinfo = Path.Combine(Environment.CurrentDirectory, "TestData", "CleanRip", "DVD", "test-dumpinfo.txt");

            var actual = CleanRip.GenerateCleanripDatafile(iso, dumpinfo);
            Assert.NotNull(actual);

            Assert.NotNull(actual.Game);
            var game = Assert.Single(actual.Game);

            Assert.NotNull(game.Rom);
            var rom = Assert.Single(game.Rom);
            Assert.Equal("9", rom.Size);
            Assert.Equal("00000000", rom.CRC);
            Assert.Equal("d41d8cd98f00b204e9800998ecf8427e", rom.MD5);
            Assert.Equal("da39a3ee5e6b4b0d3255bfef95601890afd80709", rom.SHA1);
        }

        #endregion

        #region GetBCA

        [Fact]
        public void GetBCA_InvalidPath_Null()
        {
            string bcaPath = "INVALID";
            string? actual = CleanRip.GetBCA(bcaPath);
            Assert.Null(actual);
        }

        [Fact]
        public void GetBCA_ValidPath_Formatted()
        {
            string expected = "0001 0203 0405 0607 0809 0A0B 0C0D 0E0F\n0001 0203 0405 0607 0809 0A0B 0C0D 0E0F\n0001 0203 0405 0607 0809 0A0B 0C0D 0E0F\n0001 0203 0405 0607 0809 0A0B 0C0D 0E0F\n0001 0203 0405 0607 0809 0A0B 0C0D 0E0F\n0001 0203 0405 0607 0809 0A0B 0C0D 0E0F\n0001 0203 0405 0607 0809 0A0B 0C0D 0E0F\n0001 0203 0405 0607 0809 0A0B 0C0D 0E0F\n0001 0203 0405 0607 0809 0A0B 0C0D 0E0F\n0001 0203 0405 0607 0809 0A0B 0C0D 0E0F\n";
            string bcaPath = Path.Combine(Environment.CurrentDirectory, "TestData", "CleanRip", "DVD", "test.bca");

            string? actual = CleanRip.GetBCA(bcaPath);
            Assert.NotNull(actual);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetGameCubeWiiInformation

        [Fact]
        public void GetGameCubeWiiInformation_NoFile_False()
        {
            string dumpinfo = string.Empty;
            bool actual = CleanRip.GetGameCubeWiiInformation(dumpinfo, out Region? region, out string? version, out string? name, out string? serial);

            Assert.False(actual);
            Assert.Null(region);
            Assert.Null(version);
            Assert.Null(name);
            Assert.Null(serial);
        }

        [Fact]
        public void GetGameCubeWiiInformation_Filled_True()
        {
            Region? expectedRegion = Region.World;
            string? expectedVersion = "version";
            string? expectedName = "name";
            string? expectedSerial = "000A00";

            string dumpinfo = Path.Combine(Environment.CurrentDirectory, "TestData", "CleanRip", "DVD", "test-dumpinfo.txt");
            bool actual = CleanRip.GetGameCubeWiiInformation(dumpinfo, out Region? region, out string? version, out string? name, out string? serial);

            Assert.True(actual);
            Assert.Equal(expectedRegion, region);
            Assert.Equal(expectedVersion, version);
            Assert.Equal(expectedName, name);
            Assert.Equal(expectedSerial, serial);
        }

        #endregion
    }
}