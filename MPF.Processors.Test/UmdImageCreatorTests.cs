using System;
using System.Collections.Generic;
using System.IO;
using SabreTools.RedumpLib.Data;
using Xunit;

namespace MPF.Processors.Test
{
    public class UmdImageCreatorTests
    {
        #region DetermineMediaType

        [Fact]
        public void DetermineMediaType_Null_DVD()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new UmdImageCreator(RedumpSystem.SonyPlayStationPortable);
            var actual = processor.DetermineMediaType(outputDirectory, outputFilename);
            Assert.Equal(MediaType.UMD, actual);
        }

        [Fact]
        public void DetermineMediaType_Invalid_DVD()
        {
            string? outputDirectory = null;
            string outputFilename = "INVALID";
            var processor = new UmdImageCreator(RedumpSystem.SonyPlayStationPortable);
            var actual = processor.DetermineMediaType(outputDirectory, outputFilename);
            Assert.Equal(MediaType.UMD, actual);
        }

        [Fact]
        public void DetermineMediaType_Valid_DVD()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "UmdImageCreator", "UMD");
            string outputFilename = "test";
            var processor = new UmdImageCreator(RedumpSystem.SonyPlayStationPortable);
            var actual = processor.DetermineMediaType(outputDirectory, outputFilename);
            Assert.Equal(MediaType.UMD, actual);
        }

        #endregion

        #region GetOutputFiles

        [Fact]
        public void GetOutputFiles_Null_Populated()
        {
            string? outputDirectory = null;
            string outputFilename = "test";
            var processor = new UmdImageCreator(RedumpSystem.SonyPlayStationPortable);

            var actual = processor.GetOutputFiles(null, outputDirectory, outputFilename);
            Assert.Equal(7, actual.Count);
        }

        [Fact]
        public void GetOutputFiles_UMD_Populated()
        {
            string? outputDirectory = null;
            string outputFilename = "test";
            var processor = new UmdImageCreator(RedumpSystem.SonyPlayStationPortable);

            var actual = processor.GetOutputFiles(MediaType.UMD, outputDirectory, outputFilename);
            Assert.Equal(7, actual.Count);
        }

        [Fact]
        public void GetOutputFiles_Other_Populated()
        {
            string? outputDirectory = null;
            string outputFilename = "test";
            var processor = new UmdImageCreator(RedumpSystem.SonyPlayStationPortable);

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
            var processor = new UmdImageCreator(RedumpSystem.SonyPlayStationPortable);
            var actual = processor.FoundAllFiles(MediaType.UMD, outputDirectory, outputFilename);
            Assert.Equal(5, actual.Count);
        }

        [Fact]
        public void FoundAllFiles_Valid_Empty()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "UmdImageCreator", "UMD");
            string outputFilename = "test.iso";
            var processor = new UmdImageCreator(RedumpSystem.SonyPlayStationPortable);
            var actual = processor.FoundAllFiles(MediaType.UMD, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void FoundAllFiles_ValidZip_Empty()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "UmdImageCreator", "UMD-zip");
            string outputFilename = "test.iso";
            var processor = new UmdImageCreator(RedumpSystem.SonyPlayStationPortable);
            var actual = processor.FoundAllFiles(MediaType.UMD, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        #endregion

        #region FoundAnyFiles

        [Fact]
        public void FoundAnyFiles_Invalid_Filled()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new UmdImageCreator(RedumpSystem.SonyPlayStationPortable);
            var actual = processor.FoundAnyFiles(MediaType.UMD, outputDirectory, outputFilename);
            Assert.False(actual);
        }

        [Fact]
        public void FoundAnyFiles_Valid_Empty()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "UmdImageCreator", "UMD");
            string outputFilename = "test.iso";
            var processor = new UmdImageCreator(RedumpSystem.SonyPlayStationPortable);
            var actual = processor.FoundAnyFiles(MediaType.UMD, outputDirectory, outputFilename);
            Assert.True(actual);
        }

        #endregion

        #region GenerateArtifacts

        [Fact]
        public void GenerateArtifacts_Invalid_Empty()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new UmdImageCreator(RedumpSystem.SonyPlayStationPortable);
            var actual = processor.GenerateArtifacts(MediaType.UMD, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void GenerateArtifacts_Valid_Filled()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "UmdImageCreator", "UMD");
            string outputFilename = "test.iso";
            var processor = new UmdImageCreator(RedumpSystem.SonyPlayStationPortable);
            var actual = processor.GenerateArtifacts(MediaType.UMD, outputDirectory, outputFilename);
            Assert.Equal(6, actual.Count);
        }

        #endregion

        #region GetDeleteableFilePaths

        [Fact]
        public void GetDeleteableFilePaths_Invalid_Empty()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new UmdImageCreator(RedumpSystem.SonyPlayStationPortable);
            var actual = processor.GetDeleteableFilePaths(MediaType.UMD, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetDeleteableFilePaths_Valid_Empty()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "UmdImageCreator", "UMD");
            string outputFilename = "test.iso";
            var processor = new UmdImageCreator(RedumpSystem.SonyPlayStationPortable);
            var actual = processor.GetDeleteableFilePaths(MediaType.UMD, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        #endregion

        #region GetZippableFilePaths

        [Fact]
        public void GetZippableFilePaths_Invalid_Empty()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new UmdImageCreator(RedumpSystem.SonyPlayStationPortable);
            var actual = processor.GetZippableFilePaths(MediaType.UMD, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetZippableFilePaths_Valid_Filled()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "UmdImageCreator", "UMD");
            string outputFilename = "test.iso";
            var processor = new UmdImageCreator(RedumpSystem.SonyPlayStationPortable);
            var actual = processor.GetZippableFilePaths(MediaType.UMD, outputDirectory, outputFilename);
            Assert.Equal(6, actual.Count);
        }

        #endregion

        #region GetPVD

        [Fact]
        public void GetPVD_Empty_Null()
        {
            string mainInfo = string.Empty;
            string? actual = UmdImageCreator.GetPVD(mainInfo);
            Assert.Null(actual);
        }

        [Fact]
        public void GetPVD_Invalid_Null()
        {
            string mainInfo = "INVALID";
            string? actual = UmdImageCreator.GetPVD(mainInfo);
            Assert.Null(actual);
        }

        [Fact]
        public void GetPVD_Valid_Filled()
        {
            string? expected = "0320 TEST DATA\n0330 TEST DATA\n0340 TEST DATA\n0350 TEST DATA\n0360 TEST DATA\n0370 TEST DATA\n";
            string mainInfo = Path.Combine(Environment.CurrentDirectory, "TestData", "UmdImageCreator", "UMD", "test_mainInfo.txt");
            string? actual = UmdImageCreator.GetPVD(mainInfo);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetUMDAuxInfo

        [Fact]
        public void GetUMDAuxInfo_Empty_Null()
        {
            long expectedSize = -1;

            string disc = string.Empty;
            bool actual = UmdImageCreator.GetUMDAuxInfo(disc,
                out string? title,
                out DiscCategory? category,
                out string? serial,
                out string? version,
                out string? layer,
                out long size);

            Assert.False(actual);
            Assert.Null(title);
            Assert.Null(category);
            Assert.Null(serial);
            Assert.Null(version);
            Assert.Null(layer);
            Assert.Equal(expectedSize, size);
        }

        [Fact]
        public void GetUMDAuxInfo_Invalid_Null()
        {
            long expectedSize = -1;

            string disc = "INVALID";
            bool actual = UmdImageCreator.GetUMDAuxInfo(disc,
                out string? title,
                out DiscCategory? category,
                out string? serial,
                out string? version,
                out string? layer,
                out long size);

            Assert.False(actual);
            Assert.Null(title);
            Assert.Null(category);
            Assert.Null(serial);
            Assert.Null(version);
            Assert.Null(layer);
            Assert.Equal(expectedSize, size);
        }

        [Fact]
        public void GetUMDAuxInfo_Valid_Filled()
        {
            string? expectedTitle = "title";
            DiscCategory? expectedCategory = DiscCategory.Games;
            string? expectedSerial = "ABCD-12345";
            string? expectedVersion = "1.00";
            string? expectedLayer = "442560";
            long expectedSize = 1471807488;

            string disc = Path.Combine(Environment.CurrentDirectory, "TestData", "UmdImageCreator", "UMD", "test_disc.txt");
            bool actual = UmdImageCreator.GetUMDAuxInfo(disc,
                out string? title,
                out DiscCategory? category,
                out string? serial,
                out string? version,
                out string? layer,
                out long size);

            Assert.True(actual);
            Assert.Equal(expectedTitle, title);
            Assert.Equal(expectedCategory, category);
            Assert.Equal(expectedSerial, serial);
            Assert.Equal(expectedVersion, version);
            Assert.Equal(expectedLayer, layer);
            Assert.Equal(expectedSize, size);
        }

        #endregion

        #region GetVolumeLabels

        [Fact]
        public void GetVolumeLabels_Empty_Null()
        {
            string volDesc = string.Empty;
            bool actual = UmdImageCreator.GetVolumeLabels(volDesc, out Dictionary<string, List<string>> volLabels);

            Assert.False(actual);
            Assert.Empty(volLabels);
        }

        [Fact]
        public void GetVolumeLabels_Invalid_Null()
        {
            string volDesc = "INVALID";
            bool actual = UmdImageCreator.GetVolumeLabels(volDesc, out Dictionary<string, List<string>> volLabels);

            Assert.False(actual);
            Assert.Empty(volLabels);
        }

        [Fact]
        public void GetVolumeLabels_Valid_Filled()
        {
            string volDesc = Path.Combine(Environment.CurrentDirectory, "TestData", "UmdImageCreator", "UMD", "test_volDesc.txt");
            bool actual = UmdImageCreator.GetVolumeLabels(volDesc, out Dictionary<string, List<string>> volLabels);

            Assert.True(actual);
            KeyValuePair<string, List<string>> labelPair = Assert.Single(volLabels);
            Assert.Equal("label", labelPair.Key);
            string filesystem = Assert.Single(labelPair.Value);
            Assert.Equal("UDF", filesystem);
        }

        #endregion
    }
}