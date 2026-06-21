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
        [InlineData(null, "ANY", null)]
        [InlineData("", "ANY", null)]
        [InlineData("INVALID", "ANY", null)]
        [InlineData("3\" floppy", "ANY", PhysicalMediaType.FloppyDisk)]
        [InlineData("3.5\" floppy", "ANY", PhysicalMediaType.FloppyDisk)]
        [InlineData("3.5\" magneto-optical", "ANY", PhysicalMediaType.Floptical)]
        [InlineData("3.5\" SyQuest cartridge", "ANY", null)]
        [InlineData("3.9\" SyQuest cartridge", "ANY", null)]
        [InlineData("5.25\" floppy", "ANY", PhysicalMediaType.FloppyDisk)]
        [InlineData("5.25\" magneto-optical", "ANY", PhysicalMediaType.Floptical)]
        [InlineData("5.25\" SyQuest cartridge", "ANY", null)]
        [InlineData("8\" floppy", "ANY", PhysicalMediaType.FloppyDisk)]
        [InlineData("300mm magneto optical", "ANY", PhysicalMediaType.Floptical)]
        [InlineData("356mm magneto-optical", "ANY", PhysicalMediaType.Floptical)]
        [InlineData("Advanced Digital Recording", "ANY", null)]
        [InlineData("Advanced Intelligent Tape", "ANY", null)]
        [InlineData("Archival Disc", "ANY", null)]
        [InlineData("BeeCard", "ANY", null)]
        [InlineData("Blu-ray", "Wii U Optical Disc", PhysicalMediaType.NintendoWiiUOpticalDisc)]
        [InlineData("Blu-ray", "ANY", PhysicalMediaType.BluRay)]
        [InlineData("Borsu", "ANY", null)]
        [InlineData("Compact Cassette", "ANY", PhysicalMediaType.Cassette)]
        [InlineData("Compact Disc", "ANY", PhysicalMediaType.CDROM)]
        [InlineData("Compact Flash", "ANY", PhysicalMediaType.CompactFlash)]
        [InlineData("CompacTape", "ANY", null)]
        [InlineData("CRVdisc", "ANY", null)]
        [InlineData("Data8", "ANY", null)]
        [InlineData("DataPlay", "ANY", null)]
        [InlineData("DataStore", "ANY", null)]
        [InlineData("DDCD", "ANY", PhysicalMediaType.CDROM)]
        [InlineData("DECtape", "ANY", null)]
        [InlineData("DemiDiskette", "ANY", null)]
        [InlineData("Digital Audio Tape", "ANY", null)]
        [InlineData("Digital Data Storage", "ANY", PhysicalMediaType.DataCartridge)]
        [InlineData("Digital Linear Tape", "ANY", null)]
        [InlineData("DIR", "ANY", null)]
        [InlineData("DST", "ANY", null)]
        [InlineData("DTF", "ANY", null)]
        [InlineData("DTF2", "ANY", null)]
        [InlineData("DV tape", "ANY", null)]
        [InlineData("DVD", "GameCube Game Disc", PhysicalMediaType.NintendoGameCubeGameDisc)]
        [InlineData("DVD", "Wii Optical Disc", PhysicalMediaType.NintendoWiiOpticalDisc)]
        [InlineData("DVD", "ANY", PhysicalMediaType.DVD)]
        [InlineData("EVD", "ANY", null)]
        [InlineData("Exatape", "ANY", null)]
        [InlineData("Express Card", "ANY", null)]
        [InlineData("FDDVD", "ANY", null)]
        [InlineData("Flextra", "ANY", null)]
        [InlineData("Floptical", "ANY", PhysicalMediaType.Floptical)]
        [InlineData("FVD", "ANY", null)]
        [InlineData("GD", "ANY", PhysicalMediaType.GDROM)]
        [InlineData("Hard Disk Drive", "ANY", PhysicalMediaType.HardDisk)]
        [InlineData("HD DVD", "ANY", PhysicalMediaType.HDDVD)]
        [InlineData("HD VMD", "ANY", null)]
        [InlineData("HiFD", "ANY", PhysicalMediaType.FloppyDisk)]
        [InlineData("HiTC", "ANY", null)]
        [InlineData("HuCard", "ANY", PhysicalMediaType.Cartridge)]
        [InlineData("HVD", "ANY", null)]
        [InlineData("HyperFlex", "ANY", null)]
        [InlineData("IBM 3470", "ANY", null)]
        [InlineData("IBM 3480", "ANY", null)]
        [InlineData("IBM 3490", "ANY", null)]
        [InlineData("IBM 3490E", "ANY", null)]
        [InlineData("IBM 3592", "ANY", null)]
        [InlineData("Iomega Bernoulli Box", "ANY", PhysicalMediaType.IomegaBernoulliDisk)]
        [InlineData("Iomega Bernoulli Box II", "ANY", PhysicalMediaType.IomegaBernoulliDisk)]
        [InlineData("Iomega Ditto", "ANY", null)]
        [InlineData("Iomega Jaz", "ANY", PhysicalMediaType.IomegaJaz)]
        [InlineData("Iomega PocketZip", "ANY", PhysicalMediaType.IomegaZip)]
        [InlineData("Iomega REV", "ANY", null)]
        [InlineData("Iomega ZIP", "ANY", PhysicalMediaType.IomegaZip)]
        [InlineData("Kodak Verbatim", "ANY", null)]
        [InlineData("LaserDisc", "ANY", PhysicalMediaType.LaserDisc)]
        [InlineData("Linear Tape-Open", "ANY", null)]
        [InlineData("LT1", "ANY", null)]
        [InlineData("Magneto-optical", "ANY", PhysicalMediaType.Floptical)]
        [InlineData("Memory Stick", "ANY", PhysicalMediaType.SDCard)]
        [InlineData("MiniCard", "ANY", null)]
        [InlineData("MiniDisc", "ANY", null)]
        [InlineData("MultiMediaCard", "ANY", PhysicalMediaType.SDCard)]
        [InlineData("Nintendo 3DS Game Card", "ANY", PhysicalMediaType.Cartridge)]
        [InlineData("Nintendo 64 Disk", "ANY", PhysicalMediaType.Nintendo64DD)]
        [InlineData("Nintendo 64 Game Pak", "ANY", PhysicalMediaType.Cartridge)]
        [InlineData("Nintendo Disk Card", "ANY", PhysicalMediaType.NintendoFamicomDiskSystem)]
        [InlineData("Nintendo DS Game Card", "ANY", PhysicalMediaType.Cartridge)]
        [InlineData("Nintendo DSi Game Card", "ANY", PhysicalMediaType.Cartridge)]
        [InlineData("Nintendo Entertainment System Game Pak", "ANY", PhysicalMediaType.Cartridge)]
        [InlineData("Nintendo Famicom Game Pak", "ANY", PhysicalMediaType.Cartridge)]
        [InlineData("Nintendo Game Boy Advance Game Pak", "ANY", PhysicalMediaType.Cartridge)]
        [InlineData("Nintendo Game Boy Game Pak", "ANY", PhysicalMediaType.Cartridge)]
        [InlineData("Nintendo Switch Game Card", "ANY", PhysicalMediaType.Cartridge)]
        [InlineData("Optical Disc Archive", "ANY", null)]
        [InlineData("Orb", "ANY", null)]
        [InlineData("PCMCIA Card", "ANY", null)]
        [InlineData("PD650", "ANY", null)]
        [InlineData("PlayStation Memory Card", "ANY", null)]
        [InlineData("Quarter-inch cartridge", "ANY", PhysicalMediaType.DataCartridge)]
        [InlineData("Quarter-inch mini cartridge", "ANY", PhysicalMediaType.DataCartridge)]
        [InlineData("QuickDisk", "ANY", null)]
        [InlineData("RDX", "ANY", null)]
        [InlineData("SACD", "ANY", PhysicalMediaType.DVD)]
        [InlineData("Scalable Linear Recording", "ANY", null)]
        [InlineData("Secure Digital", "ANY", PhysicalMediaType.SDCard)]
        [InlineData("SmartMedia", "ANY", PhysicalMediaType.SDCard)]
        [InlineData("Sony Professional Disc", "ANY", null)]
        [InlineData("Sony Professional Disc for DATA", "ANY", null)]
        [InlineData("STK 4480", "ANY", null)]
        [InlineData("STK 4490", "ANY", null)]
        [InlineData("STK 9490", "ANY", null)]
        [InlineData("STK T-9840", "ANY", null)]
        [InlineData("STK T-9940", "ANY", null)]
        [InlineData("STK T-10000", "ANY", null)]
        [InlineData("Super Advanced Intelligent Tape", "ANY", null)]
        [InlineData("Super Digital Linear Tape", "ANY", null)]
        [InlineData("Super Nintendo Game Pak", "ANY", PhysicalMediaType.Cartridge)]
        [InlineData("Super Nintendo Game Pak (US)", "ANY", PhysicalMediaType.Cartridge)]
        [InlineData("SuperDisk", "ANY", PhysicalMediaType.FloppyDisk)]
        [InlineData("SVOD", "ANY", null)]
        [InlineData("Travan", "ANY", null)]
        [InlineData("UDO", "ANY", null)]
        [InlineData("UHD144", "ANY", null)]
        [InlineData("UMD", "ANY", PhysicalMediaType.UMD)]
        [InlineData("Unknown", "ANY", null)]
        [InlineData("USB flash drive", "ANY", PhysicalMediaType.FlashDrive)]
        [InlineData("VCDHD", "ANY", null)]
        [InlineData("VideoFloppy", "ANY", null)]
        [InlineData("VideoNow", "ANY", PhysicalMediaType.CDROM)]
        [InlineData("VXA", "ANY", PhysicalMediaType.FlashDrive)]
        [InlineData("Wafer", "ANY", null)]
        [InlineData("xD", "ANY", null)]
        [InlineData("XQD", "ANY", null)]
        [InlineData("Zoned Hard Disk Drive", "ANY", PhysicalMediaType.HardDisk)]
        [InlineData("ZX Microdrive", "ANY", null)]
        public void GetDiscTypeFromStringsTest(string? discType, string? discSubType, PhysicalMediaType? expected)
        {
            var actual = Aaru.GetDiscTypeFromStrings(discType, discSubType);
            Assert.Equal(expected, actual);
        }

        #endregion
    }
}
