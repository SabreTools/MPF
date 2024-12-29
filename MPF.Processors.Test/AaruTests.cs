using System;
using System.IO;
using SabreTools.RedumpLib.Data;
using Schemas;
using Xunit;

#pragma warning disable CS0618 // Ignore "Type or member is obsolete"

namespace MPF.Processors.Test
{
    // TODO: Add tests around remaining helper methods
    public class AaruTests
    {
        #region GetOutputFiles

        [Fact]
        public void GetOutputFiles_Null_Empty()
        {
            string? outputDirectory = null;
            string outputFilename = "test";
            var processor = new Aaru(RedumpSystem.IBMPCcompatible, null);

            var actual = processor.GetOutputFiles(outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetOutputFiles_CDROM_Populated()
        {
            string? outputDirectory = null;
            string outputFilename = "test";
            var processor = new Aaru(RedumpSystem.IBMPCcompatible, MediaType.CDROM);

            var actual = processor.GetOutputFiles(outputDirectory, outputFilename);
            Assert.Equal(8, actual.Count);
        }

        [Fact]
        public void GetOutputFiles_DVD_Populated()
        {
            string? outputDirectory = null;
            string outputFilename = "test";
            var processor = new Aaru(RedumpSystem.IBMPCcompatible, MediaType.DVD);

            var actual = processor.GetOutputFiles(outputDirectory, outputFilename);
            Assert.Equal(7, actual.Count);
        }

        [Fact]
        public void GetOutputFiles_HDDVD_Populated()
        {
            string? outputDirectory = null;
            string outputFilename = "test";
            var processor = new Aaru(RedumpSystem.IBMPCcompatible, MediaType.HDDVD);

            var actual = processor.GetOutputFiles(outputDirectory, outputFilename);
            Assert.Equal(7, actual.Count);
        }

        [Fact]
        public void GetOutputFiles_BluRay_Populated()
        {
            string? outputDirectory = null;
            string outputFilename = "test";
            var processor = new Aaru(RedumpSystem.IBMPCcompatible, MediaType.BluRay);

            var actual = processor.GetOutputFiles(outputDirectory, outputFilename);
            Assert.Equal(7, actual.Count);
        }

        [Fact]
        public void GetOutputFiles_Other_Empty()
        {
            string? outputDirectory = null;
            string outputFilename = "test";
            var processor = new Aaru(RedumpSystem.IBMPCcompatible, MediaType.ApertureCard);

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
            var processor = new Aaru(RedumpSystem.IBMPCcompatible, MediaType.CDROM);
            var actual = processor.FoundAllFiles(outputDirectory, outputFilename);
            Assert.Equal(7, actual.Count);
        }

        [Fact]
        public void FoundAllFiles_Valid_Empty()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "Aaru", "CDROM");
            string outputFilename = "test.aaruf";
            var processor = new Aaru(RedumpSystem.IBMPCcompatible, MediaType.CDROM);
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
            var processor = new Aaru(RedumpSystem.IBMPCcompatible, MediaType.CDROM);
            var actual = processor.FoundAnyFiles(outputDirectory, outputFilename);
            Assert.False(actual);
        }

        [Fact]
        public void FoundAnyFiles_Valid_Empty()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "Aaru", "CDROM");
            string outputFilename = "test.aaruf";
            var processor = new Aaru(RedumpSystem.IBMPCcompatible, MediaType.CDROM);
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
            var processor = new Aaru(RedumpSystem.IBMPCcompatible, MediaType.CDROM);
            var actual = processor.GenerateArtifacts(outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void GenerateArtifacts_Valid_Filled()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "Aaru", "CDROM");
            string outputFilename = "test.aaruf";
            var processor = new Aaru(RedumpSystem.IBMPCcompatible, MediaType.CDROM);
            var actual = processor.GenerateArtifacts(outputDirectory, outputFilename);
            Assert.Equal(7, actual.Count);
        }

        #endregion

        #region GetDeleteableFilePaths

        [Fact]
        public void GetDeleteableFilePaths_Invalid_Empty()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new Aaru(RedumpSystem.IBMPCcompatible, MediaType.CDROM);
            var actual = processor.GetDeleteableFilePaths(outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetDeleteableFilePaths_Valid_Empty()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "Aaru", "CDROM");
            string outputFilename = "test.aaruf";
            var processor = new Aaru(RedumpSystem.IBMPCcompatible, MediaType.CDROM);
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
            var processor = new Aaru(RedumpSystem.IBMPCcompatible, MediaType.CDROM);
            var actual = processor.GetZippableFilePaths(outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetZippableFilePaths_Valid_Filled()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "Aaru", "CDROM");
            string outputFilename = "test.aaruf";
            var processor = new Aaru(RedumpSystem.IBMPCcompatible, MediaType.CDROM);
            var actual = processor.GetZippableFilePaths(outputDirectory, outputFilename);
            Assert.Equal(7, actual.Count);
        }

        #endregion

        #region GenerateCuesheet

        [Fact]
        public void GenerateCuesheet_Null_Null()
        {
            CICMMetadataType? cicmSidecar = null;
            string basePath = "test";
            string? actual = Aaru.GenerateCuesheet(cicmSidecar, basePath);
            Assert.Null(actual);
        }

        [Fact]
        public void GenerateCuesheet_Empty_Null()
        {
            CICMMetadataType? cicmSidecar = new CICMMetadataType();
            string basePath = "test";
            string? actual = Aaru.GenerateCuesheet(cicmSidecar, basePath);
            Assert.Null(actual);
        }

        [Fact]
        public void GenerateCuesheet_Valid_Filled()
        {
            TrackType trackType = new TrackType
            {
                BytesPerSector = 2352,
                Flags = new TrackFlagsType { Quadraphonic = true },
                Indexes = [new TrackIndexType { index = 1, Value = 0 }],
                ISRC = "isrc",
                Sequence = new TrackSequenceType { TrackNumber = 1 },
                TrackType1 = TrackTypeTrackType.mode1,
            };

            OpticalDiscType opticalDiscType = new OpticalDiscType
            {
                DiscType = "CD-ROM",
                MediaCatalogueNumber = "mcn",
                Track = [trackType],
                Tracks = [1],
            };

            CICMMetadataType? cicmSidecar = new CICMMetadataType
            {
                OpticalDisc = [opticalDiscType],
                Performer = ["performer"],
            };

            string basePath = "test";
            string? actual = Aaru.GenerateCuesheet(cicmSidecar, basePath);

            // TODO: Unexpected outcome -- Non-null but empty cuesheet generated
            // TODO: Add structure validation
            Assert.NotNull(actual);
        }

        #endregion

        #region GenerateDatafile

        [Fact]
        public void GenerateDatafile_Null_Null()
        {
            CICMMetadataType? cicmSidecar = null;
            string basePath = "test";
            var actual = Aaru.GenerateDatafile(cicmSidecar, basePath);
            Assert.Null(actual);
        }

        [Fact]
        public void GenerateDatafile_Empty_Null()
        {
            CICMMetadataType? cicmSidecar = new CICMMetadataType();
            string basePath = "test";
            var actual = Aaru.GenerateDatafile(cicmSidecar, basePath);
            Assert.Null(actual);
        }

        [Fact]
        public void GenerateDatafile_Valid_Filled()
        {
            TrackType trackType = new TrackType
            {
                Checksums =
                [
                    new ChecksumType { type = ChecksumTypeType.crc32, Value = "00000000" },
                    new ChecksumType { type = ChecksumTypeType.md5, Value = "d41d8cd98f00b204e9800998ecf8427e" },
                    new ChecksumType { type = ChecksumTypeType.sha1, Value = "da39a3ee5e6b4b0d3255bfef95601890afd80709" },
                ],
                Sequence = new TrackSequenceType { TrackNumber = 1 },
                Size = 12345,
            };

            OpticalDiscType opticalDiscType = new OpticalDiscType
            {
                DiscType = "CD-ROM",
                MediaCatalogueNumber = "mcn",
                Track = [trackType],
                Tracks = [1],
            };

            CICMMetadataType? cicmSidecar = new CICMMetadataType
            {
                OpticalDisc = [opticalDiscType],
            };

            string basePath = "test";
            var actual = Aaru.GenerateDatafile(cicmSidecar, basePath);

            // TODO: Add structure validation
            Assert.NotNull(actual);
        }

        #endregion

        #region GeneratePVD

        [Fact]
        public void GeneratePVD_Null_Null()
        {
            CICMMetadataType? cicmSidecar = null;
            var actual = Aaru.GeneratePVD(cicmSidecar);
            Assert.Null(actual);
        }

        [Fact]
        public void GeneratePVD_Empty_Null()
        {
            CICMMetadataType? cicmSidecar = new CICMMetadataType();
            var actual = Aaru.GeneratePVD(cicmSidecar);
            Assert.Null(actual);
        }

        [Fact]
        public void GeneratePVD_Valid_Filled()
        {
            FileSystemType fileSystemType = new FileSystemType
            {
                CreationDate = DateTime.UtcNow,
                CreationDateSpecified = true,
                ModificationDate = DateTime.UtcNow,
                ModificationDateSpecified = true,
                ExpirationDate = DateTime.UtcNow,
                ExpirationDateSpecified = true,
                EffectiveDate = DateTime.UtcNow,
                EffectiveDateSpecified = true,
            };

            PartitionType partitionType = new PartitionType
            {
                FileSystems = [fileSystemType],
            };

            TrackType trackType = new TrackType
            {
                FileSystemInformation = [partitionType],
            };

            OpticalDiscType opticalDiscType = new OpticalDiscType
            {
                Track = [trackType],
            };

            CICMMetadataType? cicmSidecar = new CICMMetadataType
            {
                OpticalDisc = [opticalDiscType],
            };

            string? actual = Aaru.GeneratePVD(cicmSidecar);

            // TODO: Add structure validation
            Assert.NotNull(actual);
        }

        #endregion
    }
}