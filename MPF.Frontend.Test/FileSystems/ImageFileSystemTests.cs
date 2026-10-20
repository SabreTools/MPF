using System;
using System.IO;
using System.IO.Compression;
using MPF.Frontend.FileSystems;
using Xunit;

namespace MPF.Frontend.Test.FileSystems
{
    public class ImageFileSystemTests
    {
        #region Create

        [Theory]
        [InlineData("Bridge", "UDF")]
        [InlineData("Joliet", "ISO 9660")]
        [InlineData("Primary", "ISO 9660")]
        [InlineData("UDFOnly", "UDF")]
        [InlineData("Raw", "UDF")]
        public void Create_Variant_FileSystemName(string variant, string expected)
        {
            using var fileSystem = Open(variant);
            Assert.Equal(expected, fileSystem.FileSystemName);
            Assert.Equal("TESTDISC", fileSystem.VolumeLabel);
        }

        [Theory]
        [InlineData("Bridge", 301L * 2048)]
        [InlineData("Joliet", 451L * 2048)]
        [InlineData("Primary", 451L * 2048)]
        [InlineData("UDFOnly", 301L * 2048)]
        [InlineData("Raw", 301L * 2048)]
        public void Create_Variant_VolumeSize(string variant, long expected)
        {
            // The ISO 9660 volume space is 451 sectors, the UDF partition starts at sector 257 and is 44 sectors long
            using var fileSystem = Open(variant);
            Assert.Equal(expected, fileSystem.VolumeSize);
        }

        [Theory]
        [InlineData("Bridge", 512, "UDF", 301L * 2048)]
        [InlineData("Bridge", 2048, "UDF", 301L * 2048)]
        [InlineData("Primary", 2048, "ISO 9660", 451L * 2048)]
        [InlineData("Raw", 2352, "UDF", 301L * 2048)]
        public void Create_WholeBlockDevice_Variant(string variant, int blockSize, string expectedName, long expectedSize)
        {
            byte[] image = Load(variant);
            var device = new VolumeStream(new WholeBlockStream(image, blockSize), image.Length, blockSize);

            using var fileSystem = ImageFileSystem.Create(device);
            Assert.Equal(expectedName, fileSystem.FileSystemName);
            Assert.Equal(expectedSize, fileSystem.VolumeSize);
            Assert.Equal("TESTDISC", fileSystem.VolumeLabel);
        }

        [Fact]
        public void Create_MissingWholeBlockDevice_Null()
        {
            Assert.Null(ImageFileSystem.Create("/dev/mpf-test-missing-device", 2048 * 2048, 2048));
        }

        [Fact]
        public void Create_NoFileSystem_FirstBytesReadable()
        {
            var image = new byte[32 * 2048];
            image[0] = 0x53;

            using var fileSystem = ImageFileSystem.Create(new MemoryStream(image));
            Assert.Null(fileSystem.FileSystemName);
            Assert.Equal(0, fileSystem.VolumeSize);
            Assert.False(fileSystem.FileExists("README.TXT"));
            Assert.Empty(fileSystem.GetFiles(string.Empty, "*", recursive: true));
            Assert.Equal([0x53, 0x00], fileSystem.ReadFirstBytes(2));
        }

        [Fact]
        public void Create_MissingPath_Null()
        {
            string path = Path.Combine(Environment.CurrentDirectory, "TestData", "FileSystems", "missing.iso");
            Assert.Null(ImageFileSystem.Create(path));
        }

        #endregion

        #region Lookup

        [Theory]
        [InlineData("Bridge")]
        [InlineData("Joliet")]
        [InlineData("Primary")]
        [InlineData("UDFOnly")]
        [InlineData("Raw")]
        public void FileExists_AnyCaseAndVersion_True(string variant)
        {
            using var fileSystem = Open(variant);
            Assert.True(fileSystem.FileExists("README.TXT"));
            Assert.True(fileSystem.FileExists("readme.txt"));
            Assert.True(fileSystem.FileExists("README.TXT;1"));
            Assert.True(fileSystem.FileExists("/INSTALL\\BITPOOL.RSC"));
            Assert.False(fileSystem.FileExists("INSTALL"));
            Assert.False(fileSystem.FileExists("MISSING.TXT"));
        }

        [Theory]
        [InlineData("Bridge")]
        [InlineData("Joliet")]
        [InlineData("Primary")]
        [InlineData("UDFOnly")]
        [InlineData("Raw")]
        public void DirectoryExists_Directory_True(string variant)
        {
            using var fileSystem = Open(variant);
            Assert.True(fileSystem.DirectoryExists(string.Empty));
            Assert.True(fileSystem.DirectoryExists("install"));
            Assert.True(fileSystem.DirectoryExists("EMPTY"));
            Assert.False(fileSystem.DirectoryExists("README.TXT"));
            Assert.False(fileSystem.DirectoryExists("MISSING"));
        }

        [Theory]
        [InlineData("Bridge", true)]
        [InlineData("Joliet", true)]
        [InlineData("Primary", false)]
        [InlineData("UDFOnly", true)]
        public void FileExists_LongName_OnlyWithLongNames(string variant, bool expected)
        {
            using var fileSystem = Open(variant);
            Assert.Equal(expected, fileSystem.FileExists("Long Directory Name/Long File Name.txt"));
        }

        [Theory]
        [InlineData("Bridge")]
        [InlineData("Joliet")]
        [InlineData("Primary")]
        [InlineData("UDFOnly")]
        [InlineData("Raw")]
        public void GetLastWriteTimeUtc_File_RecordedTimeInUtc(string variant)
        {
            // The image was created with a local time 9 hours ahead of UTC
            using var fileSystem = Open(variant);
            Assert.Equal(new DateTime(2001, 2, 3, 4, 5, 6, DateTimeKind.Utc), fileSystem.GetLastWriteTimeUtc("README.TXT"));
            Assert.Null(fileSystem.GetLastWriteTimeUtc("MISSING.TXT"));
        }

        #endregion

        #region Contents

        [Theory]
        [InlineData("Bridge")]
        [InlineData("Joliet")]
        [InlineData("Primary")]
        [InlineData("UDFOnly")]
        [InlineData("Raw")]
        public void OpenFile_MultipleSectors_Contents(string variant)
        {
            using var fileSystem = Open(variant);
            Assert.Equal(5000, fileSystem.GetFileLength("DATA/LARGE.BIN"));

            using var stream = fileSystem.OpenFile("DATA/LARGE.BIN");
            Assert.NotNull(stream);

            var actual = new byte[6000];
            int read = 0, chunk;
            while ((chunk = stream.Read(actual, read, 1000)) > 0)
            {
                read += chunk;
            }

            Assert.Equal(5000, read);
            for (int i = 0; i < 5000; i++)
            {
                Assert.Equal((byte)(i * 7 % 251), actual[i]);
            }
        }

        [Theory]
        [InlineData("Bridge")]
        [InlineData("Primary")]
        public void OpenFile_Seek_ContentsAtPosition(string variant)
        {
            using var fileSystem = Open(variant);
            using var stream = fileSystem.OpenFile("DATA/LARGE.BIN");
            Assert.NotNull(stream);

            stream.Seek(-10, SeekOrigin.End);
            Assert.Equal(4990 * 7 % 251, stream.ReadByte());
        }

        [Fact]
        public void OpenFile_Directory_Null()
        {
            using var fileSystem = Open("Bridge");
            Assert.Null(fileSystem.OpenFile("INSTALL"));
            Assert.Equal(-1, fileSystem.GetFileLength("INSTALL"));
        }

        #endregion

        #region Listing

        [Theory]
        [InlineData("Bridge")]
        [InlineData("Joliet")]
        [InlineData("Primary")]
        [InlineData("UDFOnly")]
        public void GetFiles_Recursive_AllMatches(string variant)
        {
            using var fileSystem = Open(variant);
            Assert.Equal(["INSTALL/BITPOOL.RSC"], fileSystem.GetFiles(string.Empty, "?*.rsc", recursive: true));
            Assert.Equal(5, fileSystem.GetFiles(string.Empty, "*", recursive: true).Length);

            // Lookups after the whole hierarchy was read
            Assert.True(fileSystem.FileExists("data/large.bin;1"));
            Assert.Equal(5000, fileSystem.GetFileLength("DATA/LARGE.BIN"));
        }

        [Theory]
        [InlineData("Bridge")]
        [InlineData("Joliet")]
        [InlineData("Primary")]
        [InlineData("UDFOnly")]
        public void GetFiles_TopDirectory_DirectMatches(string variant)
        {
            using var fileSystem = Open(variant);
            Assert.Equal(["README.TXT"], fileSystem.GetFiles(string.Empty, "*", recursive: false));
            Assert.Equal(["INSTALL/BITPOOL.RSC"], fileSystem.GetFiles("INSTALL", "*", recursive: false));
            Assert.Empty(fileSystem.GetFiles("EMPTY", "*", recursive: false));
            Assert.Empty(fileSystem.GetFiles("MISSING", "*", recursive: false));
        }

        [Theory]
        [InlineData("Bridge")]
        [InlineData("Joliet")]
        [InlineData("Primary")]
        [InlineData("UDFOnly")]
        public void GetDirectories_Pattern_Matches(string variant)
        {
            using var fileSystem = Open(variant);
            Assert.Equal(4, fileSystem.GetDirectories(string.Empty, "*").Length);
            Assert.Equal(["EMPTY"], fileSystem.GetDirectories(string.Empty, "?????"));
            Assert.Empty(fileSystem.GetDirectories("EMPTY", "*"));
        }

        #endregion

        #region Damaged Discs

        [Fact]
        public void Create_UnreadableStream_FirstBytesNull()
        {
            using var fileSystem = ImageFileSystem.Create(new FailingStream(32 * 2048, failFrom: 0));
            Assert.Null(fileSystem.FileSystemName);
            Assert.Null(fileSystem.ReadFirstBytes(16));
        }

        [Fact]
        public void Create_MissingDevice_Null()
        {
            Assert.Null(ImageFileSystem.Create("/dev/mpf-test-missing-device"));
        }

        [Fact]
        public void Create_NoUDFAnnounced_AnchorNotRead()
        {
            // Reading a sector that is not needed can take minutes on a damaged disc
            var stream = new FailingStream(Load("Joliet"), failFrom: long.MaxValue);
            using var fileSystem = ImageFileSystem.Create(stream);
            Assert.True(fileSystem.FileExists("README.TXT"));
            Assert.DoesNotContain(256, stream.SectorsRead);
        }

        [Fact]
        public void FileExists_DirectoriesUnreadable_False()
        {
            // The root directory lies in the first sector that cannot be read
            using var fileSystem = ImageFileSystem.Create(new FailingStream(Load("Primary"), failFrom: GetRootSector() * 2048L));
            Assert.Equal("ISO 9660", fileSystem.FileSystemName);
            Assert.False(fileSystem.FileExists("README.TXT"));
            Assert.Empty(fileSystem.GetFiles(string.Empty, "*", recursive: true));
        }

        [Fact]
        public void GetFiles_SubdirectoryUnreadable_Empty()
        {
            // Only the root directory can be read
            using var fileSystem = ImageFileSystem.Create(new FailingStream(Load("Primary"), failFrom: (GetRootSector() + 1) * 2048L));
            Assert.True(fileSystem.DirectoryExists("INSTALL"));
            Assert.Empty(fileSystem.GetFiles("INSTALL", "*", recursive: false));
        }

        [Fact]
        public void GetFiles_DirectoryLoop_ReadOnce()
        {
            // INSTALL points back to the root directory
            byte[] image = Load("Primary");
            int record = FindRootRecord(image, "INSTALL");
            WriteBothEndian(image, record + 2, GetRootSector());

            using var fileSystem = ImageFileSystem.Create(new MemoryStream(image));
            Assert.Equal(4, fileSystem.GetFiles(string.Empty, "*", recursive: true).Length);
        }

        [Fact]
        public void GetFiles_UDFDirectoryLoop_ReadOnce()
        {
            // INSTALL points back to the root directory
            byte[] image = LoadImage();
            PointUDFIdentifierToRoot(image, "INSTALL");

            using var fileSystem = ImageFileSystem.Create(new MemoryStream(image));
            Assert.Equal("UDF", fileSystem.FileSystemName);
            Assert.Equal(4, fileSystem.GetFiles(string.Empty, "*", recursive: true).Length);
        }

        [Fact]
        public void GetFiles_LittleEndianLocationInvalid_BigEndianLocation()
        {
            // The little-endian location of INSTALL points beyond the volume
            byte[] image = Load("Primary");
            int record = FindRootRecord(image, "INSTALL");
            WriteInt32(image, record + 2, (image.Length / 2048) + 1000);

            using var fileSystem = ImageFileSystem.Create(new MemoryStream(image));
            Assert.Equal(["INSTALL/BITPOOL.RSC"], fileSystem.GetFiles("INSTALL", "*", recursive: false));
            Assert.Equal(["INSTALL/BITPOOL.RSC"], fileSystem.GetFiles(string.Empty, "?*.rsc", recursive: true));
        }

        [Fact]
        public void GetFileLength_LittleEndianLengthInvalid_BigEndianLength()
        {
            byte[] image = Load("Primary");
            int record = FindRootRecord(image, "README.TXT;1");
            WriteInt32(image, record + 10, 0);

            using var fileSystem = ImageFileSystem.Create(new MemoryStream(image));
            Assert.Equal(7, fileSystem.GetFileLength("README.TXT"));
        }

        [Fact]
        public void GetFileLength_DeviceLengthBeyondVolume_BigEndianLength()
        {
            // A device also holds the tracks after the volume, such as audio tracks
            byte[] image = Load("Primary");
            int volumeSectors = BitConverter.ToInt32(image, (16 * 2048) + 80);
            Array.Resize(ref image, (volumeSectors + 100) * 2048);

            // The little-endian length of README.TXT reaches into these tracks
            int record = FindRootRecord(image, "README.TXT;1");
            int fileSector = BitConverter.ToInt32(image, record + 2);
            WriteInt32(image, record + 10, (volumeSectors - fileSector + 10) * 2048);

            using var fileSystem = ImageFileSystem.Create(new VolumeStream(new MemoryStream(image), image.Length));
            Assert.Equal(7, fileSystem.GetFileLength("README.TXT"));
        }

        [Fact]
        public void FileExists_DeviceVolumeSizeZero_True()
        {
            byte[] image = Load("Primary");
            WriteBothEndian(image, (16 * 2048) + 80, 0);

            using var fileSystem = ImageFileSystem.Create(new VolumeStream(new MemoryStream(image), image.Length));
            Assert.True(fileSystem.FileExists("README.TXT"));
        }

        [Fact]
        public void OpenFile_Interleaved_Null()
        {
            byte[] image = Load("Primary");
            int record = FindRootRecord(image, "README.TXT;1");
            image[record + 26] = 1;

            using var fileSystem = ImageFileSystem.Create(new MemoryStream(image));
            Assert.Equal(7, fileSystem.GetFileLength("README.TXT"));
            Assert.Null(fileSystem.OpenFile("README.TXT"));
        }

        [Fact]
        public void GetISO9660Time_InvalidTime_Null()
        {
            Assert.Null(ImageFileSystem.GetISO9660Time(new SabreTools.Data.Models.ISO9660.DirectoryRecordDateTime()));
        }

        [Fact]
        public void GetUDFTime_InvalidTime_Null()
        {
            Assert.Null(ImageFileSystem.GetUDFTime(new SabreTools.Data.Models.UDF.Timestamp()));
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Get the sector of the primary root directory
        /// </summary>
        private static int GetRootSector()
            => BitConverter.ToInt32(LoadImage(), (16 * 2048) + 156 + 2);

        /// <summary>
        /// Find a directory record in the primary root directory
        /// </summary>
        /// <returns>Offset of the directory record in the image</returns>
        private static int FindRootRecord(byte[] image, string identifier)
        {
            int offset = GetRootSector() * 2048;
            while (image[offset] != 0)
            {
                int nameLength = image[offset + 32];
                if (System.Text.Encoding.ASCII.GetString(image, offset + 33, nameLength) == identifier)
                    return offset;

                offset += image[offset];
            }

            throw new InvalidOperationException(identifier);
        }

        /// <summary>
        /// Write a value as both-endian 32-bit integer
        /// </summary>
        private static void WriteBothEndian(byte[] image, int offset, int value)
        {
            WriteInt32(image, offset, value);
            image[offset + 4] = (byte)(value >> 24);
            image[offset + 5] = (byte)(value >> 16);
            image[offset + 6] = (byte)(value >> 8);
            image[offset + 7] = (byte)value;
        }

        /// <summary>
        /// Write a little-endian 32-bit integer
        /// </summary>
        private static void WriteInt32(byte[] image, int offset, int value)
            => Array.Copy(BitConverter.GetBytes(value), 0, image, offset, 4);

        /// <summary>
        /// Point the UDF file identifier of a name to the root directory, updating the descriptor CRC
        /// </summary>
        private static void PointUDFIdentifierToRoot(byte[] image, string name)
        {
            // The File Set Descriptor (tag 256) holds the root directory ICB at offset 400
            int fileSet = FindUDFTag(image, 256, 0);
            int rootBlock = BitConverter.ToInt32(image, fileSet + 400 + 4);

            // File Identifier Descriptors (tag 257) hold the identifier after 38 bytes and implementation use
            for (int offset = FindUDFTag(image, 257, 0); offset >= 0; offset = FindUDFTag(image, 257, offset + 4))
            {
                int identifierLength = image[offset + 19];
                int implementationLength = BitConverter.ToUInt16(image, offset + 36);
                int identifier = offset + 38 + implementationLength;
                if (identifierLength != name.Length + 1 || image[identifier] != 8)
                    continue;
                if (System.Text.Encoding.ASCII.GetString(image, identifier + 1, name.Length) != name)
                    continue;

                WriteInt32(image, offset + 20 + 4, rootBlock);
                int crcLength = BitConverter.ToUInt16(image, offset + 10);
                ushort crc = Crc16(image, offset + 16, crcLength);
                image[offset + 8] = (byte)crc;
                image[offset + 9] = (byte)(crc >> 8);

                int checksum = 0;
                for (int i = 0; i < 16; i++)
                {
                    if (i != 4)
                        checksum += image[offset + i];
                }

                image[offset + 4] = (byte)checksum;
                return;
            }

            throw new InvalidOperationException(name);
        }

        /// <summary>
        /// Find a UDF descriptor tag, searching 4-byte aligned positions
        /// </summary>
        /// <returns>Offset of the tag, -1 if not found</returns>
        private static int FindUDFTag(byte[] image, ushort identifier, int start)
        {
            for (int offset = start; offset + 16 <= image.Length; offset += 4)
            {
                if (BitConverter.ToUInt16(image, offset) != identifier || BitConverter.ToUInt16(image, offset + 2) < 2)
                    continue;

                int checksum = 0;
                for (int i = 0; i < 16; i++)
                {
                    if (i != 4)
                        checksum += image[offset + i];
                }

                if ((byte)checksum == image[offset + 4])
                    return offset;
            }

            return -1;
        }

        /// <summary>
        /// CRC-ITU-T as used by UDF descriptor tags
        /// </summary>
        private static ushort Crc16(byte[] data, int offset, int length)
        {
            int crc = 0;
            for (int i = 0; i < length; i++)
            {
                crc ^= data[offset + i] << 8;
                for (int bit = 0; bit < 8; bit++)
                {
                    crc = (crc & 0x8000) != 0 ? (crc << 1) ^ 0x1021 : crc << 1;
                }
            }

            return (ushort)crc;
        }

        /// <summary>
        /// Open a variant of the test image, each leaving a different file system to read
        /// </summary>
        /// <param name="variant">
        /// Bridge (UDF with Joliet and primary ISO 9660), Joliet (no UDF announced), Primary (no UDF, no Joliet),
        /// UDFOnly (no ISO 9660 volume descriptors) or Raw (the bridge disc in raw 2352-byte sectors)
        /// </param>
        private static ImageFileSystem Open(string variant)
            => ImageFileSystem.Create(new MemoryStream(Load(variant)));

        /// <summary>
        /// Load a variant of the test image
        /// </summary>
        /// <param name="variant">Variant as described for Open</param>
        private static byte[] Load(string variant)
        {
            byte[] image = LoadImage();
            switch (variant)
            {
                case "Joliet":
                    // Break the identifier of the NSR descriptor in the volume recognition sequence
                    image[(20 * 2048) + 1] = (byte)'X';
                    break;

                case "Primary":
                    image[(20 * 2048) + 1] = (byte)'X';

                    // Clear the escape sequences that mark the supplementary descriptor as Joliet
                    Array.Clear(image, (17 * 2048) + 88, 32);
                    break;

                case "UDFOnly":
                    // Move the volume recognition sequence over the ISO 9660 volume descriptors
                    Array.Copy(image, 19 * 2048, image, 16 * 2048, 3 * 2048);
                    Array.Clear(image, 19 * 2048, 3 * 2048);
                    break;

                case "Raw":
                    image = ToRawSectors(image);
                    break;

                default:
                    break;
            }

            return image;
        }

        /// <summary>
        /// Load the test image, a UDF bridge disc created with genisoimage
        /// </summary>
        private static byte[] LoadImage()
        {
            string path = Path.Combine(Environment.CurrentDirectory, "TestData", "FileSystems", "udf-bridge.iso.gz");
            using var gzip = new GZipStream(File.OpenRead(path), CompressionMode.Decompress);
            using var image = new MemoryStream();
            gzip.CopyTo(image);
            return image.ToArray();
        }

        /// <summary>
        /// Convert 2048-byte sectors to raw 2352-byte Mode 1 sectors (without EDC/ECC)
        /// </summary>
        private static byte[] ToRawSectors(byte[] image)
        {
            int sectors = image.Length / 2048;
            var raw = new byte[sectors * 2352];
            for (int i = 0; i < sectors; i++)
            {
                int offset = i * 2352;
                for (int j = 1; j < 11; j++)
                {
                    raw[offset + j] = 0xFF;
                }

                // Header with the address (MSF, BCD) and the mode
                int lba = i + 150;
                raw[offset + 12] = ToBcd(lba / 4500);
                raw[offset + 13] = ToBcd(lba / 75 % 60);
                raw[offset + 14] = ToBcd(lba % 75);
                raw[offset + 15] = 0x01;

                Array.Copy(image, i * 2048, raw, offset + 16, 2048);
            }

            return raw;
        }

        /// <summary>
        /// Convert a number below 100 to binary-coded decimal
        /// </summary>
        private static byte ToBcd(int value)
            => (byte)(((value / 10) << 4) | (value % 10));

        #endregion
    }
}
