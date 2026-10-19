using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using MPF.Frontend.FileSystems;
using MPF.Frontend.Test.FileSystems;
using MPF.Frontend.Tools;
using Xunit;

namespace MPF.Frontend.Test.Tools
{
    public class ProtectionToolTests
    {
        #region CanScanDrivePath

        [Fact]
        public void CanScanDrivePath_Null_False()
        {
            bool actual = ProtectionTool.CanScanDrivePath(null);
            Assert.False(actual);
        }

        [Fact]
        public void CanScanDrivePath_Directory_True()
        {
            string path = Path.Combine(Environment.CurrentDirectory, "TestData", "MacOSDev");
            var drive = Drive.Create(null, path);

            bool actual = ProtectionTool.CanScanDrivePath(drive);
            Assert.True(actual);
        }

        #endregion

        #region RunCombinedProtectionScans

        [Fact]
        public async Task RunCombinedProtectionScans_NoImageNoDrive_Null()
        {
            string basePath = Path.Combine(Environment.CurrentDirectory, "TestData", "ProtectionTool", "missing");

            var actual = await ProtectionTool.RunCombinedProtectionScans(basePath, null, new Options());
            Assert.Null(actual);
        }

        [Fact]
        public async Task RunCombinedProtectionScans_UnreadableImageNoDrive_Null()
        {
            string basePath = Path.Combine(Environment.CurrentDirectory, "TestData", "ProtectionTool", "unreadable");

            var actual = await ProtectionTool.RunCombinedProtectionScans(basePath, null, new Options());
            Assert.Null(actual);
        }

        [Fact]
        public async Task RunCombinedProtectionScans_ImageNoDrive_ImageFilesScanned()
        {
            string basePath = Path.Combine(Environment.CurrentDirectory, "TestData", "ProtectionTool", "test");

            var actual = await ProtectionTool.RunCombinedProtectionScans(basePath, null, new Options());
            Assert.Equal("Bitpool", ProtectionTool.FormatProtections(actual));
        }

        [Fact]
        public async Task RunCombinedProtectionScans_ImageDirectoryDrive_ImageFilesNotScanned()
        {
            string basePath = Path.Combine(Environment.CurrentDirectory, "TestData", "ProtectionTool", "test");
            var drive = Drive.Create(null, Path.Combine(Environment.CurrentDirectory, "TestData", "MacOSDev"));

            var actual = await ProtectionTool.RunCombinedProtectionScans(basePath, drive, new Options());
            Assert.Equal("No protections found [OMIT LINE FROM SUBMISSION]", ProtectionTool.FormatProtections(actual));
        }

        [Fact]
        public async Task RunCombinedProtectionScans_ImageDirectoryDriveOptionSet_ImageFilesScanned()
        {
            string basePath = Path.Combine(Environment.CurrentDirectory, "TestData", "ProtectionTool", "test");
            var drive = Drive.Create(null, Path.Combine(Environment.CurrentDirectory, "TestData", "MacOSDev"));
            var options = new Options();
            options.Processing.ProtectionScanning.ScanDumpImageFiles = true;

            var actual = await ProtectionTool.RunCombinedProtectionScans(basePath, drive, options);
            Assert.Equal("Bitpool", ProtectionTool.FormatProtections(actual));
        }

        #endregion

        #region RunProtectionScanOnFiles

        [Fact]
        public async Task RunProtectionScanOnFiles_Image_KeysNamedAfterImage()
        {
            using var files = OpenTestImage();
            string sourcePath = Path.Combine("dumps", "disc.iso");

            var actual = await ProtectionTool.RunProtectionScanOnFiles(files, sourcePath, new Options());
            Assert.NotNull(actual);
            Assert.Equal("Bitpool", ProtectionTool.FormatProtections(actual));
            Assert.All(actual.Keys, key => Assert.StartsWith(sourcePath, key));
        }

        [Fact]
        public async Task RunProtectionScanOnFiles_NoFiles_Null()
        {
            using var files = ImageFileSystem.Create(new MemoryStream(new byte[32 * 2048]));

            var actual = await ProtectionTool.RunProtectionScanOnFiles(files, "disc.iso", new Options());
            Assert.Null(actual);
        }

        [Fact]
        public async Task RunProtectionScanOnFiles_UnreadableFile_TemporaryFilesRemoved()
        {
            // Directories can be read, file contents cannot
            byte[] image = LoadTestImage();
            int firstFileSector = BitConverter.ToInt32(image, (16 * 2048) + 156 + 2) + 5;
            using var files = ImageFileSystem.Create(new FailingStream(image, failFrom: firstFileSector * 2048L));
            string mirrorPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            await Assert.ThrowsAsync<IOException>(() => ProtectionTool.RunProtectionScanOnFiles(files, "disc.iso", new Options(), null, mirrorPath));
            Assert.False(Directory.Exists(mirrorPath));
        }

        [Fact]
        public async Task RunProtectionScanOnFiles_Folder_ScannedInPlace()
        {
            string path = Path.Combine(Environment.CurrentDirectory, "TestData", "MacOSDev");

            var actual = await ProtectionTool.RunProtectionScanOnFiles(new FolderFileSystem(path), "ignored", new Options());
            Assert.NotNull(actual);
            Assert.Empty(actual);
        }

        #endregion

        #region CreateMirror

        [Fact]
        public void CreateMirror_UnknownType_FirstSectorAndLength()
        {
            using var files = OpenTestImage();
            string mirrorPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                ProtectionTool.CreateMirror(files, files.GetAllEntries(), mirrorPath);

                // Unknown file types keep their first sector and their length, the rest reads as zeros
                byte[] large = File.ReadAllBytes(Path.Combine(mirrorPath, "DATA", "LARGE.BIN"));
                Assert.Equal(5000, large.Length);
                for (int i = 0; i < 2048; i++)
                {
                    Assert.Equal((byte)(i * 7 % 251), large[i]);
                }

                for (int i = 2048; i < large.Length; i++)
                {
                    Assert.Equal(0, large[i]);
                }

                // Known file types are copied in full
                byte[] tool = File.ReadAllBytes(Path.Combine(mirrorPath, "DATA", "TOOL.EXE"));
                Assert.Equal(3000, tool.Length);
                Assert.Equal((byte)(2999 * 13 % 256), tool[2999]);

                // Directories are kept, even if empty
                Assert.True(Directory.Exists(Path.Combine(mirrorPath, "EMPTY")));
                Assert.True(File.Exists(Path.Combine(mirrorPath, "Long Directory Name", "Long File Name.txt")));
            }
            finally
            {
                Directory.Delete(mirrorPath, true);
            }
        }

        [Fact]
        public void CreateMirror_UnknownType_SparseOnWindows()
        {
            using var files = OpenTestImage();
            string mirrorPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                ProtectionTool.CreateMirror(files, files.GetAllEntries(), mirrorPath);

                // Windows only leaves out the zeros after the first sector in a file marked as sparse
                var attributes = File.GetAttributes(Path.Combine(mirrorPath, "DATA", "LARGE.BIN"));
                Assert.Equal(SystemEnvironment.IsWindows(), (attributes & FileAttributes.SparseFile) != 0);
            }
            finally
            {
                Directory.Delete(mirrorPath, true);
            }
        }

        [Fact]
        public void CreateMirror_NotOpenableAsType_ReadPartAndLength()
        {
            // Named like an archive without being one, such as a dummy file, so the scanner cannot open it
            byte[] image = LoadTestImage();
            var counting = new FailingStream(image, failFrom: image.Length);
            using var files = ImageFileSystem.Create(counting);
            List<DiscEntry> entries = [new DiscEntry("DATA/DUMMY.ZIP", false, 163840, null, [new Extent(0, 163840, true)], null, null, 0)];
            string mirrorPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                long before = counting.BytesRead;
                Assert.Equal(1, ProtectionTool.CreateMirror(files, entries, mirrorPath));
                Assert.True(counting.BytesRead - before < 163840);

                // What was read to identify and open the file is kept, such as the volume descriptor in sector 16
                byte[] dummy = File.ReadAllBytes(Path.Combine(mirrorPath, "DATA", "DUMMY.ZIP"));
                Assert.Equal(163840, dummy.Length);
                Assert.Equal(image[32769], dummy[32769]);
            }
            finally
            {
                Directory.Delete(mirrorPath, true);
            }
        }

        [Fact]
        public void CreateMirror_OpenedAsType_ReadOnce()
        {
            byte[] image = LoadTestImage();
            var counting = new FailingStream(image, failFrom: image.Length);
            using var files = ImageFileSystem.Create(counting);
            var tool = files.GetAllEntries().Find(entry => entry.Path == "DATA/TOOL.EXE")!;
            string mirrorPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                long before = counting.BytesRead;
                Assert.Equal(1, ProtectionTool.CreateMirror(files, [tool], mirrorPath));

                // Only the first sector is read twice, to identify the file and to open it as its type
                Assert.Equal(tool.Length + 2048, counting.BytesRead - before);
                byte[] copy = File.ReadAllBytes(Path.Combine(mirrorPath, "DATA", "TOOL.EXE"));
                Assert.Equal(3000, copy.Length);
                Assert.Equal((byte)(2999 * 13 % 256), copy[2999]);
            }
            finally
            {
                Directory.Delete(mirrorPath, true);
            }
        }

        [Fact]
        public void CreateMirror_OpenedAsTypeReturningToStart_Full()
        {
            // An executable followed by 160 KB of other data, which opening it reads in part before returning to its start
            byte[] image = LoadTestImage();
            using var files = ImageFileSystem.Create(new MemoryStream(image));
            var tool = files.GetAllEntries().Find(entry => entry.Path == "DATA/TOOL.EXE")!;
            long offset = tool.Extents![0].Offset;
            List<DiscEntry> entries = [new DiscEntry("DATA/BIG.EXE", false, 163000, null, [new Extent(offset, 3000, true), new Extent(0, 160000, true)], null, null, 0)];
            string mirrorPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                Assert.Equal(1, ProtectionTool.CreateMirror(files, entries, mirrorPath));

                byte[] expected = [.. image.Skip((int)offset).Take(3000), .. image.Take(160000)];
                Assert.Equal(expected, File.ReadAllBytes(Path.Combine(mirrorPath, "DATA", "BIG.EXE")));
            }
            finally
            {
                Directory.Delete(mirrorPath, true);
            }
        }

        [Fact]
        public void CreateMirror_TextFile_Full()
        {
            // Text files are scanned without being opened as a type
            using var files = OpenTestImage();
            var large = files.GetAllEntries().Find(entry => entry.Path == "DATA/LARGE.BIN")!;
            List<DiscEntry> entries = [new DiscEntry("DATA/LARGE.TXT", false, large.Length, null, large.Extents, null, null, 0)];
            string mirrorPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                Assert.Equal(1, ProtectionTool.CreateMirror(files, entries, mirrorPath));

                byte[] text = File.ReadAllBytes(Path.Combine(mirrorPath, "DATA", "LARGE.TXT"));
                Assert.Equal(5000, text.Length);
                Assert.Equal((byte)(4999 * 7 % 251), text[4999]);
            }
            finally
            {
                Directory.Delete(mirrorPath, true);
            }
        }

        [Fact]
        public void CreateMirror_UnreadableWhileOpening_Throws()
        {
            // An archive header that claims more data than the first sector holds, and the next sector cannot be read
            byte[] image = LoadTestImage();
            DiscEntry large;
            using (var files = ImageFileSystem.Create(new MemoryStream(image)))
            {
                large = files.GetAllEntries().Find(entry => entry.Path == "DATA/LARGE.BIN")!;
            }

            int offset = (int)large.Extents![0].Offset;
            Array.Clear(image, offset, 30);
            image[offset] = 0x50;
            image[offset + 1] = 0x4B;
            image[offset + 2] = 0x03;
            image[offset + 3] = 0x04;
            BitConverter.GetBytes(4000).CopyTo(image, offset + 18);

            using var damaged = ImageFileSystem.Create(new FailingStream(image, failFrom: offset + 2048));
            string mirrorPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                Assert.Throws<IOException>(() => ProtectionTool.CreateMirror(damaged, [large], mirrorPath));
            }
            finally
            {
                Directory.Delete(mirrorPath, true);
            }
        }

        [Fact]
        public void CreateMirror_UnreadableFile_LengthOnly()
        {
            // Interleaved files have no extents to read
            using var files = OpenTestImage();
            List<DiscEntry> entries = [new DiscEntry("DATA/INTERLEAVED.EXE", false, 3000, null, null, null, null, 0)];
            string mirrorPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                Assert.Equal(1, ProtectionTool.CreateMirror(files, entries, mirrorPath));
                Assert.Equal(new byte[3000], File.ReadAllBytes(Path.Combine(mirrorPath, "DATA", "INTERLEAVED.EXE")));
            }
            finally
            {
                Directory.Delete(mirrorPath, true);
            }
        }

        [Fact]
        public void CreateMirror_UnsafePath_Skipped()
        {
            using var files = OpenTestImage();
            string outsideName = $"{Guid.NewGuid()}.txt";
            List<DiscEntry> entries = [new DiscEntry($"../{outsideName}", false, 7, null, null, null, null, 0)];
            string mirrorPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            string outsidePath = Path.Combine(Path.GetTempPath(), outsideName);
            try
            {
                Assert.Equal(0, ProtectionTool.CreateMirror(files, entries, mirrorPath));
                Assert.False(File.Exists(outsidePath));
            }
            finally
            {
                File.Delete(outsidePath);
            }
        }

        [Theory]
        [InlineData("INSTALL/BITPOOL.RSC", true)]
        [InlineData("Long Directory Name/Long File Name.txt", true)]
        [InlineData("../outside.txt", false)]
        [InlineData("DATA/./FILE", false)]
        [InlineData("DATA//FILE", false)]
        [InlineData("DATA/FI\0LE", false)]
        public void IsSafeRelativePath_Path_Expected(string path, bool expected)
        {
            Assert.Equal(expected, ProtectionTool.IsSafeRelativePath(path));
        }

        /// <summary>
        /// Open the UDF bridge test image
        /// </summary>
        private static ImageFileSystem OpenTestImage()
            => ImageFileSystem.Create(new MemoryStream(LoadTestImage()));

        /// <summary>
        /// Load the UDF bridge test image
        /// </summary>
        private static byte[] LoadTestImage()
        {
            string path = Path.Combine(Environment.CurrentDirectory, "TestData", "FileSystems", "udf-bridge.iso.gz");
            using var gzip = new GZipStream(File.OpenRead(path), CompressionMode.Decompress);
            using var data = new MemoryStream();
            gzip.CopyTo(data);
            return data.ToArray();
        }

        #endregion

        #region FormatProtections

        [Fact]
        public void FormatProtections_Null_ExternalScanNeeded()
        {
            string? actual = ProtectionTool.FormatProtections(null);
            Assert.Equal("[EXTERNAL SCAN NEEDED]", actual);
        }

        [Fact]
        public void FormatProtections_Empty_NoProtectionsFound()
        {
            string? actual = ProtectionTool.FormatProtections([]);
            Assert.Equal("No protections found [OMIT LINE FROM SUBMISSION]", actual);
        }

        #endregion

        #region SanitizeContextSensitiveProtections

        [Fact]
        public void SanitizeContextSensitiveProtections_Empty_NoException()
        {
            Dictionary<string, List<string>>? protections = [];
            var actual = ProtectionTool.SanitizeContextSensitiveProtections(protections);
            Assert.NotNull(actual);
            Assert.Empty(actual);
        }

        [Fact]
        public void SanitizeContextSensitiveProtections_NoMatch_NoChange()
        {
            Dictionary<string, List<string>>? protections = [];
            protections["File1"] = ["Protection 1", "Protection 2"];

            var actual = ProtectionTool.SanitizeContextSensitiveProtections(protections);
            Assert.NotNull(actual);

            string[] keys = [.. actual.Keys];
            Assert.Contains("File1", keys);
        }

        [Fact]
        public void SanitizeContextSensitiveProtections_Match_NoSub_NoChange()
        {
            Dictionary<string, List<string>>? protections = [];
            protections["File1"] = ["Protection 1", "Protection 2"];
            protections["File2"] = ["SecuROM Release Control - ANYTHING", "Protection 2"];
            protections["File3"] = ["Protection 1", "SecuROM Release Control -"];

            var actual = ProtectionTool.SanitizeContextSensitiveProtections(protections);
            Assert.NotNull(actual);

            string[] keys = [.. actual.Keys];
            Assert.Contains("File1", keys);
            Assert.Contains("File2", keys);
            Assert.Contains("File3", keys);
        }

        [Fact]
        public void SanitizeContextSensitiveProtections_Match_Sub_Change()
        {
            Dictionary<string, List<string>>? protections = [];
            protections["File1"] = ["Protection 1", "Protection 2"];
            protections["File2"] = ["SecuROM Release Control - ANYTHING"];
            protections["File2/FileA"] = ["ANYTHING GitHub ANYTHING"];
            protections["File2/FileB"] = ["SecuROM 7"];
            protections["File2/FileC"] = ["SecuROM 8"];
            protections["File2/FileD"] = ["SecuROM Content Activation"];
            protections["File2/FileE"] = ["SecuROM Data File Activation"];
            protections["File2/FileF"] = ["Unlock"];

            var actual = ProtectionTool.SanitizeContextSensitiveProtections(protections);
            Assert.NotNull(actual);

            string[] keys = [.. actual.Keys];
            Assert.Contains("File1", keys);
            Assert.Contains("File2", keys);
            Assert.Contains("File2/FileA", keys);
            Assert.DoesNotContain("File2/FileB", keys);
            Assert.DoesNotContain("File2/FileC", keys);
            Assert.DoesNotContain("File2/FileD", keys);
            Assert.DoesNotContain("File2/FileE", keys);
            Assert.DoesNotContain("File2/FileF", keys);
        }

        [Fact]
        public void SanitizeContextSensitiveProtections_MultiMatch_Sub_Change()
        {
            Dictionary<string, List<string>>? protections = [];
            protections["File1"] = ["Protection 1", "Protection 2"];
            protections["File2"] = ["SecuROM Release Control - ANYTHING"];
            protections["File2/FileA"] = ["ANYTHING GitHub ANYTHING"];
            protections["File2/FileB"] = ["SecuROM 7"];
            protections["File2/FileC"] = ["SecuROM 8"];
            protections["File3"] = ["SecuROM Release Control - ANYTHING"];
            protections["File3/FileD"] = ["SecuROM Content Activation"];
            protections["File3/FileE"] = ["SecuROM Data File Activation"];
            protections["File3/FileF"] = ["Unlock"];

            var actual = ProtectionTool.SanitizeContextSensitiveProtections(protections);
            Assert.NotNull(actual);

            string[] keys = [.. actual.Keys];
            Assert.Contains("File1", keys);
            Assert.Contains("File2", keys);
            Assert.Contains("File2/FileA", keys);
            Assert.DoesNotContain("File2/FileB", keys);
            Assert.DoesNotContain("File2/FileC", keys);
            Assert.Contains("File3", keys);
            Assert.DoesNotContain("File3/FileD", keys);
            Assert.DoesNotContain("File3/FileE", keys);
            Assert.DoesNotContain("File3/FileF", keys);
        }

        #endregion

        #region SanitizeFoundProtections

        [Fact]
        public void SanitizeFoundProtections_Exception()
        {
            List<string> protections =
            [
                "Anything Else Protection",
                "[Access issue when opening file",
                "[Exception opening file",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal("Anything Else Protection, Exception occurred while scanning [RESCAN NEEDED]", sanitized);
        }

        #region Game Engine

        [Fact]
        public void SanitizeFoundProtections_RenderWare()
        {
            List<string> protections =
            [
                "RenderWare",
                "RenderWare ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        #endregion

        #region Packers

        [Fact]
        public void SanitizeFoundProtections_dotNetReactor()
        {
            List<string> protections =
            [
                ".NET Reactor",
                ".NET Reactor ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_SevenZipSFX()
        {
            List<string> protections =
            [
                "7-Zip SFX",
                "7-Zip SFX ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_ASPack()
        {
            List<string> protections =
            [
                "ASPack",
                "ASPack ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_AutoPlayMediaStudio()
        {
            List<string> protections =
            [
                "AutoPlay Media Studio",
                "AutoPlay Media Studio ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_CaphyonAdvancedInstaller()
        {
            List<string> protections =
            [
                "Caphyon Advanced Installer",
                "Caphyon Advanced Installer ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_CExe()
        {
            List<string> protections =
            [
                "CExe",
                "CExe ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_Crunch()
        {
            List<string> protections =
            [
                "Crunch",
                "Crunch ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_dotFuscator()
        {
            List<string> protections =
            [
                "dotFuscator",
                "dotFuscator ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_EmbeddedArchive()
        {
            List<string> protections =
            [
                "Embedded 7-zip Archive",
                "Embedded PKZIP Archive",
                "Embedded RAR Archive",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_EmbeddedExecutable()
        {
            List<string> protections =
            [
                "Embedded Executable",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_EXEStealth()
        {
            List<string> protections =
            [
                "EXE Stealth",
                "EXE Stealth ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_GenteeInstaller()
        {
            List<string> protections =
            [
                "Gentee Installer",
                "Gentee Installer ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_GPInstall()
        {
            List<string> protections =
            [
                "GP-Install",
                "GP-Install ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_HyperTechCrackProof()
        {
            List<string> protections =
            [
                "HyperTech CrackProof",
                "HyperTech CrackProof ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_InnoSetup()
        {
            List<string> protections =
            [
                "Inno Setup",
                "Inno Setup ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_InstallAnywhere()
        {
            List<string> protections =
            [
                "InstallAnywhere",
                "InstallAnywhere ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_InstallerVISE()
        {
            List<string> protections =
            [
                "Installer VISE",
                "Installer VISE ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_IntelInstallationFramework()
        {
            List<string> protections =
            [
                "Intel Installation Framework",
                "Intel Installation Framework ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_MicrosoftCABSFX()
        {
            List<string> protections =
            [
                "Microsoft CAB SFX",
                "Microsoft CAB SFX ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_MPRESS()
        {
            List<string> protections =
            [
                "MPRESS",
                "MPRESS ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_NeoLite()
        {
            List<string> protections =
            [
                "NeoLite",
                "NeoLite ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_NSIS()
        {
            List<string> protections =
            [
                "NSIS",
                "NSIS ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_PECompact()
        {
            List<string> protections =
            [
                "PE Compact",
                "PE Compact ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_PEtite()
        {
            List<string> protections =
            [
                "PEtite",
                "PEtite ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_SetupFactory()
        {
            List<string> protections =
            [
                "Setup Factory",
                "Setup Factory ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_Shrinker()
        {
            List<string> protections =
            [
                "Shrinker",
                "Shrinker ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_SpoonInstaller()
        {
            List<string> protections =
            [
                "Spoon Installer",
                "Spoon Installer ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_UPX()
        {
            List<string> protections =
            [
                "UPX",
                "UPX ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_WinRARSFX()
        {
            List<string> protections =
            [
                "WinRAR SFX",
                "WinRAR SFX ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_WinZipSFX()
        {
            List<string> protections =
            [
                "WinZip SFX",
                "WinZip SFX ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_WiseInstaller()
        {
            List<string> protections =
            [
                "Wise Installation",
                "Wise Installation ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        #endregion

        #region Protections

        [Fact]
        public void SanitizeFoundProtections_ActiveMARK()
        {
            List<string> protections =
            [
                "ActiveMARK",
                "ActiveMARK 5",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal("ActiveMARK 5", sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_CactusDataShield()
        {
            List<string> protections =
            [
                "Cactus Data Shield 200",
                "Cactus Data Shield 200 (Build 3.0.100a)",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal("Cactus Data Shield 200 (Build 3.0.100a)", sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_CactusDataShieldMacrovision()
        {
            List<string> protections =
            [
                "Anything Else Protection",
                "Cactus Data Shield 300 (Confirm presence of other CDS-300 files)",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal("Anything Else Protection, Cactus Data Shield 300", sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_CDCheck()
        {
            List<string> protections =
            [
                "Anything Else Protection",
                "Executable-Based CD Check",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal("Anything Else Protection", sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_CDCops()
        {
            List<string> protections =
            [
                "CD-Cops",
                "CD-Cops v1.2.0",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal("CD-Cops v1.2.0", sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_CDKey()
        {
            List<string> protections =
            [
                "Anything Else Protection",
                "CD-Key / Serial",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal("Anything Else Protection", sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_EACdKey()
        {
            List<string> protections =
            [
                "EA CdKey Registration Module",
                "EA CdKey Registration Module v1.2.0",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_EADRM()
        {
            List<string> protections =
            [
                "EA DRM Protection",
                "EA DRM Protection v1.2.0",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal("EA DRM Protection v1.2.0", sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_GFWL()
        {
            List<string> protections =
            [
                "Games for Windows LIVE",
                "Games for Windows LIVE v1.2.0",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal("Games for Windows LIVE v1.2.0", sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_GFWLZDPP()
        {
            List<string> protections =
            [
                "Games for Windows LIVE",
                "Games for Windows LIVE Zero Day Piracy Protection",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal("Games for Windows LIVE, Games for Windows LIVE Zero Day Piracy Protection", sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_ImpulseReactor()
        {
            List<string> protections =
            [
                "Impulse Reactor",
                "Impulse Reactor Core Module v1.2.0",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal("Impulse Reactor Core Module v1.2.0", sanitized);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void SanitizeFoundProtections_JoWoodXProtTest(int skip)
        {
            List<string> protections =
            [
                "JoWood X-Prot 1.2.0.00",
                "JoWood X-Prot v2",
                "JoWood X-Prot v1.4+",
                "JoWood X-Prot v1.0-v1.3",
                "JoWood X-Prot",
            ];

            // Safeguard for the future
            if (skip >= protections.Count)
                throw new ArgumentException("Invalid skip value", nameof(skip));

            // The list is in order of preference
            protections = [.. protections.Skip(skip)];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal(protections[0], sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_OnlineRegistration()
        {
            List<string> protections =
            [
                "Anything Else Protection",
                "Executable-Based Online Registration",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal("Anything Else Protection", sanitized);
        }

        [Theory]
        [InlineData(0, "Macrovision Protected Application [SafeDisc 0.00.000], SafeDisc 0.00.000, SafeDisc Lite")]
        [InlineData(1, "Macrovision Protected Application [SafeDisc 0.00.000 / SRV Tool APP], SafeDisc 0.00.000, SafeDisc Lite")]
        [InlineData(2, "SafeDisc 0.00.000, SafeDisc 0.00.000-1.11.111, SafeDisc 3+ (DVD), SafeDisc Lite")]
        [InlineData(3, "SafeDisc 0.00.000, SafeDisc 3+ (DVD), SafeDisc Lite")]
        [InlineData(4, "SafeDisc 3+ (DVD), SafeDisc Lite")]
        [InlineData(5, "Macrovision Security Driver, SafeDisc Lite")]
        [InlineData(6, "Macrovision Protection File, SafeDisc 2+, SafeDisc 3+ (DVD), SafeDisc Lite")]
        [InlineData(7, "SafeDisc 3+ (DVD)")]
        [InlineData(8, "SafeDisc 2+")]
        public void SanitizeFoundProtections_SafeDisc(int skip, string expected)
        {
            List<string> protections =
           [
                "Macrovision Protected Application [SafeDisc 0.00.000]",
                "Macrovision Protected Application [SafeDisc 0.00.000 / SRV Tool APP]",
                "SafeDisc 0.00.000-1.11.111",
                "SafeDisc 0.00.000",
                "Macrovision Security Driver 1.11.111 / SafeDisc 1.11.111",
                "Macrovision Security Driver",
                "SafeDisc Lite",
                "SafeDisc 3+ (DVD)",
                "SafeDisc 2+",
                "Macrovision Protection File",
            ];

            // Safeguard for the future
            if (skip >= protections.Count)
                throw new ArgumentException("Invalid skip value", nameof(skip));

            // The list is in order of preference
            protections = [.. protections.Skip(skip)];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal(expected, sanitized);
        }

        [Fact]
        public void SanitizeFoundProtectsion_SafeDisc_MacrovisionSecurityDriver()
        {
            List<string> protections =
           [
                "Macrovision Protection File [Likely indicates either SafeDisc 1.45.011+ (CD) or CDS-300]",
                "Macrovision Security Driver 4.00.060 / SafeDisc 4.00.000-4.70.000",
                "SafeDisc 4.00.000-4.00.003",
                "SafeDisc 4.00.002, Macrovision Protected Application"
            ];

            string expected = "SafeDisc 4.00.002";

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal(expected, sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_SecuROM()
        {
            List<string> protections =
            [
                "SecuROM Release Control",
                "SecuROM Release Control - ANYTHING",
                "SecuROM Release Control - ANYTHING ELSE",
                "SecuROM Release Control - EVEN MORE",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal("SecuROM Release Control", sanitized);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void SanitizeFoundProtections_StarForce(int skip)
        {
            List<string> protections =
            [
                "StarForce 1.20.000.000",
                "StarForce 5 [Protected Module]",
                "StarForce 5",
                "StarForce 3-5",
                "StarForce",
            ];

            // Safeguard for the future
            if (skip >= protections.Count)
                throw new ArgumentException("Invalid skip value", nameof(skip));

            // The list is in order of preference
            protections = [.. protections.Skip(skip)];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal(protections[0], sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_StarForce_Keyless()
        {
            List<string> protections =
            [
                "StarForce Keyless",
                "StarForce Keyless - ",
                "StarForce Keyless - ESPY97LS8FHNYR52JDWL8D6FJ",
                "StarForce Keyless - UAYA-RPMVHJ-ZRY45G-ETUJ36-WL922B",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal("StarForce Keyless", sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_Steam()
        {
            List<string> protections =
            [
                "Steam",
                "Steam (.sis/.csm/.csd)",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal("Steam (.sis/.csm/.csd)", sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_Sysiphus()
        {
            List<string> protections =
            [
                "Sysiphus",
                "Sysiphus v1.2.0",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal("Sysiphus v1.2.0", sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_XCP()
        {
            List<string> protections =
            [
                "XCP",
                "XCP v1.2.0",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal("XCP v1.2.0", sanitized);
        }

        #endregion

        #endregion
    }
}
