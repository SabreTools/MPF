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
        // TODO: Create minimal sidecars for all other supported types
        #region DeterminePhysicalMediaType

        [Fact]
        public void DeterminePhysicalMediaType_Empty_Null()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new Aaru(PhysicalSystem.IBMPCcompatible);
            var actual = processor.DeterminePhysicalMediaType(outputDirectory, outputFilename);
            Assert.Null(actual);
        }

        [Fact]
        public void DeterminePhysicalMediaType_Invalid_Null()
        {
            string? outputDirectory = null;
            string outputFilename = "INVALID";
            var processor = new Aaru(PhysicalSystem.IBMPCcompatible);
            var actual = processor.DeterminePhysicalMediaType(outputDirectory, outputFilename);
            Assert.Null(actual);
        }

        [Fact]
        public void DeterminePhysicalMediaType_CD_Valid_CD()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "Aaru", "CDROM");
            string outputFilename = "test";
            var processor = new Aaru(PhysicalSystem.IBMPCcompatible);
            var actual = processor.DeterminePhysicalMediaType(outputDirectory, outputFilename);
            Assert.Equal(PhysicalMediaType.CDROM, actual);
        }

        #endregion

        #region GetOutputFiles

        [Fact]
        public void GetOutputFiles_Null_Empty()
        {
            string? outputDirectory = null;
            string outputFilename = "test";
            var processor = new Aaru(PhysicalSystem.IBMPCcompatible);

            var actual = processor.GetOutputFiles(null, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetOutputFiles_CDROM_Populated()
        {
            string? outputDirectory = null;
            string outputFilename = "test";
            var processor = new Aaru(PhysicalSystem.IBMPCcompatible);

            var actual = processor.GetOutputFiles(PhysicalMediaType.CDROM, outputDirectory, outputFilename);
            Assert.Equal(8, actual.Count);
        }

        [Fact]
        public void GetOutputFiles_DVD_Populated()
        {
            string? outputDirectory = null;
            string outputFilename = "test";
            var processor = new Aaru(PhysicalSystem.IBMPCcompatible);

            var actual = processor.GetOutputFiles(PhysicalMediaType.DVD, outputDirectory, outputFilename);
            Assert.Equal(7, actual.Count);
        }

        [Fact]
        public void GetOutputFiles_HDDVD_Populated()
        {
            string? outputDirectory = null;
            string outputFilename = "test";
            var processor = new Aaru(PhysicalSystem.IBMPCcompatible);

            var actual = processor.GetOutputFiles(PhysicalMediaType.HDDVD, outputDirectory, outputFilename);
            Assert.Equal(7, actual.Count);
        }

        [Fact]
        public void GetOutputFiles_BluRay_Populated()
        {
            string? outputDirectory = null;
            string outputFilename = "test";
            var processor = new Aaru(PhysicalSystem.IBMPCcompatible);

            var actual = processor.GetOutputFiles(PhysicalMediaType.BluRay, outputDirectory, outputFilename);
            Assert.Equal(7, actual.Count);
        }

        [Fact]
        public void GetOutputFiles_Other_Empty()
        {
            string? outputDirectory = null;
            string outputFilename = "test";
            var processor = new Aaru(PhysicalSystem.IBMPCcompatible);

            var actual = processor.GetOutputFiles(PhysicalMediaType.ApertureCard, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        #endregion

        #region FoundAllFiles

        [Fact]
        public void FoundAllFiles_Invalid_Filled()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new Aaru(PhysicalSystem.IBMPCcompatible);
            var actual = processor.FoundAllFiles(PhysicalMediaType.CDROM, outputDirectory, outputFilename);
            Assert.Equal(7, actual.Count);
        }

        [Fact]
        public void FoundAllFiles_Valid_Empty()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "Aaru", "CDROM");
            string outputFilename = "test.aaruf";
            var processor = new Aaru(PhysicalSystem.IBMPCcompatible);
            var actual = processor.FoundAllFiles(PhysicalMediaType.CDROM, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        #endregion

        #region FoundAnyFiles

        [Fact]
        public void FoundAnyFiles_Invalid_Filled()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new Aaru(PhysicalSystem.IBMPCcompatible);
            var actual = processor.FoundAnyFiles(PhysicalMediaType.CDROM, outputDirectory, outputFilename);
            Assert.False(actual);
        }

        [Fact]
        public void FoundAnyFiles_Valid_Empty()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "Aaru", "CDROM");
            string outputFilename = "test.aaruf";
            var processor = new Aaru(PhysicalSystem.IBMPCcompatible);
            var actual = processor.FoundAnyFiles(PhysicalMediaType.CDROM, outputDirectory, outputFilename);
            Assert.True(actual);
        }

        [Fact]
        public void FoundAnyFiles_ValidZip_Empty()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "Aaru", "CDROM-zip");
            string outputFilename = "test.aaruf";
            var processor = new Aaru(PhysicalSystem.IBMPCcompatible);
            var actual = processor.FoundAnyFiles(PhysicalMediaType.CDROM, outputDirectory, outputFilename);
            Assert.True(actual);
        }

        #endregion

        #region GenerateArtifacts

        [Fact]
        public void GenerateArtifacts_Invalid_Empty()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new Aaru(PhysicalSystem.IBMPCcompatible);
            var actual = processor.GenerateArtifacts(PhysicalMediaType.CDROM, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void GenerateArtifacts_Valid_Filled()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "Aaru", "CDROM");
            string outputFilename = "test.aaruf";
            var processor = new Aaru(PhysicalSystem.IBMPCcompatible);
            var actual = processor.GenerateArtifacts(PhysicalMediaType.CDROM, outputDirectory, outputFilename);
            Assert.Equal(7, actual.Count);
        }

        #endregion

        #region GetDeleteableFilePaths

        [Fact]
        public void GetDeleteableFilePaths_Invalid_Empty()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new Aaru(PhysicalSystem.IBMPCcompatible);
            var actual = processor.GetDeleteableFilePaths(PhysicalMediaType.CDROM, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetDeleteableFilePaths_Valid_Empty()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "Aaru", "CDROM");
            string outputFilename = "test.aaruf";
            var processor = new Aaru(PhysicalSystem.IBMPCcompatible);
            var actual = processor.GetDeleteableFilePaths(PhysicalMediaType.CDROM, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        #endregion

        #region GetZippableFilePaths

        [Fact]
        public void GetZippableFilePaths_Invalid_Empty()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new Aaru(PhysicalSystem.IBMPCcompatible);
            var actual = processor.GetZippableFilePaths(PhysicalMediaType.CDROM, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetZippableFilePaths_Valid_Filled()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "Aaru", "CDROM");
            string outputFilename = "test.aaruf";
            var processor = new Aaru(PhysicalSystem.IBMPCcompatible);
            var actual = processor.GetZippableFilePaths(PhysicalMediaType.CDROM, outputDirectory, outputFilename);
            Assert.Equal(7, actual.Count);
        }

        #endregion

        #region GetPreservedFilePaths

        [Fact]
        public void GetPreservedFilePaths_Invalid_Empty()
        {
            string? outputDirectory = null;
            string outputFilename = string.Empty;
            var processor = new Aaru(PhysicalSystem.IBMPCcompatible);
            var actual = processor.GetPreservedFilePaths(PhysicalMediaType.CDROM, outputDirectory, outputFilename);
            Assert.Empty(actual);
        }

        [Fact]
        public void GetPreservedFilePaths_Valid_Empty()
        {
            string? outputDirectory = Path.Combine(Environment.CurrentDirectory, "TestData", "Aaru", "CDROM");
            string outputFilename = "test.aaruf";
            var processor = new Aaru(PhysicalSystem.IBMPCcompatible);
            var actual = processor.GetPreservedFilePaths(PhysicalMediaType.CDROM, outputDirectory, outputFilename);
            Assert.Empty(actual);
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
            CICMMetadataType? cicmSidecar = new();
            string basePath = "test";
            string? actual = Aaru.GenerateCuesheet(cicmSidecar, basePath);
            Assert.Null(actual);
        }

        [Fact]
        public void GenerateCuesheet_Valid_Filled()
        {
            TrackType trackType = new()
            {
                BytesPerSector = 2352,
                Flags = new TrackFlagsType { Quadraphonic = true },
                Indexes = [new TrackIndexType { index = 1, Value = 0 }],
                ISRC = "isrc",
                Sequence = new TrackSequenceType { TrackNumber = 1 },
                TrackType1 = TrackTypeTrackType.mode1,
            };

            OpticalDiscType opticalDiscType = new()
            {
                DiscType = "CD-ROM",
                MediaCatalogueNumber = "mcn",
                Track = [trackType],
                Tracks = [1],
            };

            CICMMetadataType? cicmSidecar = new()
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
            CICMMetadataType? cicmSidecar = new();
            string basePath = "test";
            var actual = Aaru.GenerateDatafile(cicmSidecar, basePath);
            Assert.Null(actual);
        }

        [Fact]
        public void GenerateDatafile_Valid_Filled()
        {
            TrackType trackType = new()
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

            OpticalDiscType opticalDiscType = new()
            {
                DiscType = "CD-ROM",
                MediaCatalogueNumber = "mcn",
                Track = [trackType],
                Tracks = [1],
            };

            CICMMetadataType? cicmSidecar = new()
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
            CICMMetadataType? cicmSidecar = new();
            var actual = Aaru.GeneratePVD(cicmSidecar);
            Assert.Null(actual);
        }

        [Fact]
        public void GeneratePVD_Valid_Filled()
        {
            FileSystemType fileSystemType = new()
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

            PartitionType partitionType = new()
            {
                FileSystems = [fileSystemType],
            };

            TrackType trackType = new()
            {
                FileSystemInformation = [partitionType],
            };

            OpticalDiscType opticalDiscType = new()
            {
                Track = [trackType],
            };

            CICMMetadataType? cicmSidecar = new()
            {
                OpticalDisc = [opticalDiscType],
            };

            string? actual = Aaru.GeneratePVD(cicmSidecar);

            // TODO: Add structure validation
            Assert.NotNull(actual);
        }

        #endregion

        #region GetDiscTypeFromStrings

        [Theory]
        [MemberData(nameof(GenerateGetDiscTypeFromStringsTestData))]
        public void GetDiscTypeFromStringsTest(string? discType, string? discSubType, PhysicalMediaType? expected)
        {
            var actual = Aaru.GetDiscTypeFromStrings(discType, discSubType);
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Generate a test set for GetDiscTypeFromStringsTest
        /// </summary>
        public static TheoryData<string?, string?, PhysicalMediaType?> GenerateGetDiscTypeFromStringsTestData()
        {
            return new TheoryData<string?, string?, PhysicalMediaType?>
            {
                { null, "ANY", null },
                { "", "ANY", null },
                { "INVALID", "ANY", null },
                { "3\" floppy", "ANY", PhysicalMediaType.FloppyDisk },
                { "3.5\" floppy", "ANY", PhysicalMediaType.FloppyDisk },
                { "3.5\" magneto-optical", "ANY", PhysicalMediaType.Floptical },
                { "3.5\" SyQuest cartridge", "ANY", null },
                { "3.9\" SyQuest cartridge", "ANY", null },
                { "5.25\" floppy", "ANY", PhysicalMediaType.FloppyDisk },
                { "5.25\" magneto-optical", "ANY", PhysicalMediaType.Floptical },
                { "5.25\" SyQuest cartridge", "ANY", null },
                { "8\" floppy", "ANY", PhysicalMediaType.FloppyDisk },
                { "300mm magneto optical", "ANY", PhysicalMediaType.Floptical },
                { "356mm magneto-optical", "ANY", PhysicalMediaType.Floptical },
                { "Advanced Digital Recording", "ANY", null },
                { "Advanced Intelligent Tape", "ANY", null },
                { "Archival Disc", "ANY", null },
                { "BeeCard", "ANY", null },
                { "Blu-ray", "Wii U Optical Disc", PhysicalMediaType.NintendoWiiUOpticalDisc },
                { "Blu-ray", "ANY", PhysicalMediaType.BluRay },
                { "Borsu", "ANY", null },
                { "Compact Cassette", "ANY", PhysicalMediaType.Cassette },
                { "Compact Disc", "ANY", PhysicalMediaType.CDROM },
                { "Compact Flash", "ANY", PhysicalMediaType.CompactFlash },
                { "CompacTape", "ANY", null },
                { "CRVdisc", "ANY", null },
                { "Data8", "ANY", null },
                { "DataPlay", "ANY", null },
                { "DataStore", "ANY", null },
                { "DDCD", "ANY", PhysicalMediaType.CDROM },
                { "DECtape", "ANY", null },
                { "DemiDiskette", "ANY", null },
                { "Digital Audio Tape", "ANY", null },
                { "Digital Data Storage", "ANY", PhysicalMediaType.DataCartridge },
                { "Digital Linear Tape", "ANY", null },
                { "DIR", "ANY", null },
                { "DST", "ANY", null },
                { "DTF", "ANY", null },
                { "DTF2", "ANY", null },
                { "DV tape", "ANY", null },
                { "DVD", "GameCube Game Disc", PhysicalMediaType.NintendoGameCubeGameDisc },
                { "DVD", "Wii Optical Disc", PhysicalMediaType.NintendoWiiOpticalDisc },
                { "DVD", "ANY", PhysicalMediaType.DVD },
                { "EVD", "ANY", null },
                { "Exatape", "ANY", null },
                { "Express Card", "ANY", null },
                { "FDDVD", "ANY", null },
                { "Flextra", "ANY", null },
                { "Floptical", "ANY", PhysicalMediaType.Floptical },
                { "FVD", "ANY", null },
                { "GD", "ANY", PhysicalMediaType.GDROM },
                { "Hard Disk Drive", "ANY", PhysicalMediaType.HardDisk },
                { "HD DVD", "ANY", PhysicalMediaType.HDDVD },
                { "HD VMD", "ANY", null },
                { "HiFD", "ANY", PhysicalMediaType.FloppyDisk },
                { "HiTC", "ANY", null },
                { "HuCard", "ANY", PhysicalMediaType.Cartridge },
                { "HVD", "ANY", null },
                { "HyperFlex", "ANY", null },
                { "IBM 3470", "ANY", null },
                { "IBM 3480", "ANY", null },
                { "IBM 3490", "ANY", null },
                { "IBM 3490E", "ANY", null },
                { "IBM 3592", "ANY", null },
                { "Iomega Bernoulli Box", "ANY", PhysicalMediaType.IomegaBernoulliDisk },
                { "Iomega Bernoulli Box II", "ANY", PhysicalMediaType.IomegaBernoulliDisk },
                { "Iomega Ditto", "ANY", null },
                { "Iomega Jaz", "ANY", PhysicalMediaType.IomegaJaz },
                { "Iomega PocketZip", "ANY", PhysicalMediaType.IomegaZip },
                { "Iomega REV", "ANY", null },
                { "Iomega ZIP", "ANY", PhysicalMediaType.IomegaZip },
                { "Kodak Verbatim", "ANY", null },
                { "LaserDisc", "ANY", PhysicalMediaType.LaserDisc },
                { "Linear Tape-Open", "ANY", null },
                { "LT1", "ANY", null },
                { "Magneto-optical", "ANY", PhysicalMediaType.Floptical },
                { "Memory Stick", "ANY", PhysicalMediaType.SDCard },
                { "MiniCard", "ANY", null },
                { "MiniDisc", "ANY", null },
                { "MultiMediaCard", "ANY", PhysicalMediaType.SDCard },
                { "Nintendo 3DS Game Card", "ANY", PhysicalMediaType.Cartridge },
                { "Nintendo 64 Disk", "ANY", PhysicalMediaType.Nintendo64DD },
                { "Nintendo 64 Game Pak", "ANY", PhysicalMediaType.Cartridge },
                { "Nintendo Disk Card", "ANY", PhysicalMediaType.NintendoFamicomDiskSystem },
                { "Nintendo DS Game Card", "ANY", PhysicalMediaType.Cartridge },
                { "Nintendo DSi Game Card", "ANY", PhysicalMediaType.Cartridge },
                { "Nintendo Entertainment System Game Pak", "ANY", PhysicalMediaType.Cartridge },
                { "Nintendo Famicom Game Pak", "ANY", PhysicalMediaType.Cartridge },
                { "Nintendo Game Boy Advance Game Pak", "ANY", PhysicalMediaType.Cartridge },
                { "Nintendo Game Boy Game Pak", "ANY", PhysicalMediaType.Cartridge },
                { "Nintendo Switch Game Card", "ANY", PhysicalMediaType.Cartridge },
                { "Optical Disc Archive", "ANY", null },
                { "Orb", "ANY", null },
                { "PCMCIA Card", "ANY", null },
                { "PD650", "ANY", null },
                { "PlayStation Memory Card", "ANY", null },
                { "Quarter-inch cartridge", "ANY", PhysicalMediaType.DataCartridge },
                { "Quarter-inch mini cartridge", "ANY", PhysicalMediaType.DataCartridge },
                { "QuickDisk", "ANY", null },
                { "RDX", "ANY", null },
                { "SACD", "ANY", PhysicalMediaType.DVD },
                { "Scalable Linear Recording", "ANY", null },
                { "Secure Digital", "ANY", PhysicalMediaType.SDCard },
                { "SmartMedia", "ANY", PhysicalMediaType.SDCard },
                { "Sony Professional Disc", "ANY", null },
                { "Sony Professional Disc for DATA", "ANY", null },
                { "STK 4480", "ANY", null },
                { "STK 4490", "ANY", null },
                { "STK 9490", "ANY", null },
                { "STK T-9840", "ANY", null },
                { "STK T-9940", "ANY", null },
                { "STK T-10000", "ANY", null },
                { "Super Advanced Intelligent Tape", "ANY", null },
                { "Super Digital Linear Tape", "ANY", null },
                { "Super Nintendo Game Pak", "ANY", PhysicalMediaType.Cartridge },
                { "Super Nintendo Game Pak (US)", "ANY", PhysicalMediaType.Cartridge },
                { "SuperDisk", "ANY", PhysicalMediaType.FloppyDisk },
                { "SVOD", "ANY", null },
                { "Travan", "ANY", null },
                { "UDO", "ANY", null },
                { "UHD144", "ANY", null },
                { "UMD", "ANY", PhysicalMediaType.UMD },
                { "Unknown", "ANY", null },
                { "USB flash drive", "ANY", PhysicalMediaType.FlashDrive },
                { "VCDHD", "ANY", null },
                { "VideoFloppy", "ANY", null },
                { "VideoNow", "ANY", PhysicalMediaType.CDROM },
                { "VXA", "ANY", PhysicalMediaType.FlashDrive },
                { "Wafer", "ANY", null },
                { "xD", "ANY", null },
                { "XQD", "ANY", null },
                { "Zoned Hard Disk Drive", "ANY", PhysicalMediaType.HardDisk },
                { "ZX Microdrive", "ANY", null },
            };
        }

        #endregion
    }
}
