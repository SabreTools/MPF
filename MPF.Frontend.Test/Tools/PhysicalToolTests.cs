using System;
using System.IO;
using System.IO.Compression;
using MPF.Frontend.FileSystems;
using MPF.Frontend.Tools;
using Xunit;

namespace MPF.Frontend.Test.Tools
{
    public class PhysicalToolTests
    {
        /// <summary>
        /// Open the files of the test disc from a directory or from the test image
        /// </summary>
        private static DiscFileSystem Open(bool image)
        {
            if (!image)
                return new FolderFileSystem(Path.Combine(Environment.CurrentDirectory, "TestData", "FileSystems", "PlayStation2"));

            string path = Path.Combine(Environment.CurrentDirectory, "TestData", "FileSystems", "playstation2.iso.gz");
            using var gzip = new GZipStream(File.OpenRead(path), CompressionMode.Decompress);
            var data = new MemoryStream();
            gzip.CopyTo(data);
            return ImageFileSystem.Create(data);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void GetPlayStationExecutableName_SystemCnf_BootFile(bool image)
        {
            using var files = Open(image);
            Assert.Equal("SLES_500.09;1", PhysicalTool.GetPlayStationExecutableName(files));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void GetPlayStationSerial_SystemCnf_Serial(bool image)
        {
            using var files = Open(image);
            Assert.Equal("SLES-50009", PhysicalTool.GetPlayStationSerial(files));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void GetPlayStation2Version_SystemCnf_Version(bool image)
        {
            using var files = Open(image);
            Assert.Equal("1.00", PhysicalTool.GetPlayStation2Version(files));
        }

        [Fact]
        public void GetFileDate_FolderBootFile_DateOfFile()
        {
            using var files = Open(image: false);
            string expected = files.GetLastWriteTimeUtc("SLES_500.09")!.Value.ToString("yyyy-MM-dd");
            Assert.Equal(expected, PhysicalTool.GetFileDate(files, "SLES_500.09"));
            Assert.Null(PhysicalTool.GetFileDate(files, "MISSING.EXE"));
        }

        [Fact]
        public void GetFileDate_ImageBootFileWithVersion_Date()
        {
            // The boot file name from SYSTEM.CNF carries the ISO 9660 version suffix
            using var files = Open(image: true);
            string? exeName = PhysicalTool.GetPlayStationExecutableName(files);
            Assert.Equal("2002-03-04", PhysicalTool.GetFileDate(files, exeName, fixTwoDigitYear: true));
        }

        [Fact]
        public void GetPlayStationSerial_LowerCaseFolder_SerialVersionAndDate()
        {
            // A disc mounted on Linux shows "SYSTEM.CNF;1" as "system.cnf"
            using var files = new FolderFileSystem(Path.Combine(Environment.CurrentDirectory, "TestData", "FileSystems", "LowerCase"));
            string? exeName = PhysicalTool.GetPlayStationExecutableName(files);
            string expected = File.GetLastWriteTimeUtc(Path.Combine(Environment.CurrentDirectory, "TestData", "FileSystems", "LowerCase", "sles_500.09")).ToString("yyyy-MM-dd");
            Assert.Equal("SLES-50009", PhysicalTool.GetPlayStationSerial(files));
            Assert.Equal("1.00", PhysicalTool.GetPlayStation2Version(files));
            Assert.Equal(expected, PhysicalTool.GetFileDate(files, exeName));
        }

        [Fact]
        public void PhysicalTool_NoFiles_Null()
        {
            Assert.Null(PhysicalTool.GetPlayStationExecutableName(null));
            Assert.Null(PhysicalTool.GetPlayStation2Version(null));
            Assert.Null(PhysicalTool.GetFileDate(null, "SLES_500.09"));
            Assert.Null(PhysicalTool.GetFirstBytes(null, 16));
            Assert.Null(PhysicalTool.DetectSegaSystem(null));
        }

        [Fact]
        public void DetectSegaSystem_Image_FromFirstSector()
        {
            var image = new byte[32 * 2048];
            byte[] header = System.Text.Encoding.ASCII.GetBytes("SEGA SEGASATURN ");
            Array.Copy(header, image, header.Length);

            using var files = ImageFileSystem.Create(new MemoryStream(image));
            Assert.Equal(SabreTools.RedumpLib.Data.PhysicalSystem.SegaSaturn, PhysicalTool.DetectSegaSystem(files));
        }
    }
}
