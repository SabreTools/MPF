using System;
using System.IO;
using MPF.Frontend.FileSystems;
using Xunit;

namespace MPF.Frontend.Test.FileSystems
{
    public class DiscFileSystemTests
    {
        #region MatchesPattern

        [Theory]
        [InlineData("STEAM.SIS", "?*.sis", true)]
        [InlineData(".sis", "?*.sis", false)]
        [InlineData("CUSA12345", "?????????", true)]
        [InlineData("CUSA1234", "?????????", false)]
        [InlineData("anything", "*", true)]
        [InlineData("data1.cab", "data*.CAB", true)]
        [InlineData("data1.hdr", "data*.cab", false)]
        public void MatchesPattern_NameAndPattern_Expected(string name, string pattern, bool expected)
        {
            Assert.Equal(expected, DiscFileSystem.MatchesPattern(name, pattern));
        }

        #endregion

        #region OpenDrive

        [Fact]
        public void OpenDrive_Directory_FolderFileSystem()
        {
            var drive = Drive.Create(InternalDriveType.Optical, GetTestPath("FileSystems", "Folder"));
            using var fileSystem = DiscFileSystem.OpenDrive(drive);
            Assert.IsType<FolderFileSystem>(fileSystem);
        }

        [Fact]
        public void OpenDrive_MountedPath_FolderFileSystem()
        {
            // A mounted filesystem path that is a folder is read instead of the device
            var drive = Drive.Create(InternalDriveType.Optical, GetTestPath("ProtectionTool", "test.iso"));
            drive!.MountedPath = GetTestPath("FileSystems", "Folder");
            using var fileSystem = DiscFileSystem.OpenDrive(drive);
            var folder = Assert.IsType<FolderFileSystem>(fileSystem);
            Assert.Equal(drive.MountedPath, folder.Root);
        }

        [Fact]
        public void OpenDrive_Null_Null()
        {
            Assert.Null(DiscFileSystem.OpenDrive(null));
        }

        #endregion

        #region OpenDump

        [Fact]
        public void OpenDump_Iso_ImageFileSystem()
        {
            using var fileSystem = DiscFileSystem.OpenDump(GetTestPath("ProtectionTool", "test"));
            Assert.NotNull(fileSystem);
            Assert.Equal(GetTestPath("ProtectionTool", "test.iso"), fileSystem.SourcePath);
        }

        [Fact]
        public void OpenDump_CueTracks_FirstTrackWithFiles()
        {
            // The first track has no file system, like an audio track, and the third track is missing
            string basePath = GetTestPath("FileSystems", "Cue", "disc");
            Assert.Equal(2, DiscFileSystem.GetDumpImagePaths(basePath).Count);

            using var fileSystem = DiscFileSystem.OpenDump(basePath);
            Assert.NotNull(fileSystem);
            Assert.Equal(GetTestPath("FileSystems", "Cue", "disc (Track 2).bin"), fileSystem.SourcePath);
            Assert.True(fileSystem.FileExists("INSTALL/BITPOOL.RSC"));
        }

        [Fact]
        public void OpenDump_NoImage_Null()
        {
            string basePath = GetTestPath("FileSystems", "missing");
            Assert.Empty(DiscFileSystem.GetDumpImagePaths(basePath));
            Assert.Null(DiscFileSystem.OpenDump(basePath));
        }

        [Fact]
        public void OpenDump_NoFileSystem_Null()
        {
            Assert.Null(DiscFileSystem.OpenDump(GetTestPath("ProtectionTool", "unreadable")));
        }

        #endregion

        /// <summary>
        /// Get the path of test data
        /// </summary>
        private static string GetTestPath(params string[] names)
        {
            string path = Path.Combine(Environment.CurrentDirectory, "TestData");
            foreach (string name in names)
            {
                path = Path.Combine(path, name);
            }

            return path;
        }
    }
}
