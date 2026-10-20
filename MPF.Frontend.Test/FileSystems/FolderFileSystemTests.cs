using System;
using System.IO;
using MPF.Frontend.FileSystems;
using Xunit;

namespace MPF.Frontend.Test.FileSystems
{
    public class FolderFileSystemTests
    {
        /// <summary>
        /// Directory holding the files of the test disc
        /// </summary>
        private static readonly string Root = Path.Combine(Environment.CurrentDirectory, "TestData", "FileSystems", "Folder");

        [Fact]
        public void FileExists_RelativePath_True()
        {
            var fileSystem = new FolderFileSystem(Root);
            Assert.True(fileSystem.FileExists("README.TXT"));
            Assert.True(fileSystem.FileExists("INSTALL/BITPOOL.RSC"));
            Assert.True(fileSystem.FileExists("/INSTALL/BITPOOL.RSC"));
            Assert.False(fileSystem.FileExists("INSTALL"));
            Assert.False(fileSystem.FileExists("MISSING.TXT"));
        }

        [Fact]
        public void FileExists_OtherCaseOrVersion_True()
        {
            // A disc mounted on Linux shows "SYSTEM.CNF;1" as "system.cnf"
            var fileSystem = new FolderFileSystem(Path.Combine(Environment.CurrentDirectory, "TestData", "FileSystems", "LowerCase"));
            Assert.True(fileSystem.FileExists("SYSTEM.CNF"));
            Assert.True(fileSystem.FileExists("SLES_500.09;1"));
            Assert.True(fileSystem.FileExists("INSTALL/BITPOOL.RSC"));
            Assert.True(fileSystem.DirectoryExists("INSTALL"));
            Assert.Equal(15, fileSystem.GetFileLength("SLES_500.09;1"));
            Assert.Equal([Path.Combine("install", "bitpool.rsc")], fileSystem.GetFiles("INSTALL", "*", recursive: false));
            Assert.False(fileSystem.FileExists("INSTALL/MISSING.RSC"));
            Assert.False(fileSystem.FileExists("SYSTEM.CNF/MISSING"));
        }

        [Fact]
        public void GetFiles_HiddenAndOtherCase_Listed()
        {
            // A name starting with a period is hidden on Linux and macOS
            var fileSystem = new FolderFileSystem(Path.Combine(Environment.CurrentDirectory, "TestData", "FileSystems", "LowerCase"));
            Assert.Contains(".hidden", fileSystem.GetFiles(string.Empty, "*", recursive: false));
            Assert.Contains(".hidden.dir", fileSystem.GetDirectories(string.Empty, "*"));
            Assert.Equal(["install"], fileSystem.GetDirectories(string.Empty, "INSTALL"));
            Assert.Empty(fileSystem.GetFiles("SYSTEM.CNF", "*", recursive: false));
            Assert.Empty(fileSystem.GetDirectories("SYSTEM.CNF", "*"));
        }

        [Fact]
        public void DirectoryExists_RelativePath_True()
        {
            var fileSystem = new FolderFileSystem(Root);
            Assert.True(fileSystem.DirectoryExists(string.Empty));
            Assert.True(fileSystem.DirectoryExists("INSTALL"));
            Assert.False(fileSystem.DirectoryExists("README.TXT"));
        }

        [Fact]
        public void GetFileLength_File_Length()
        {
            var fileSystem = new FolderFileSystem(Root);
            Assert.Equal(7, fileSystem.GetFileLength("README.TXT"));
            Assert.Equal(-1, fileSystem.GetFileLength("MISSING.TXT"));
        }

        [Fact]
        public void GetLastWriteTimeUtc_File_TimeOfFile()
        {
            var fileSystem = new FolderFileSystem(Root);
            var expected = File.GetLastWriteTimeUtc(Path.Combine(Root, "README.TXT"));
            Assert.Equal(expected, fileSystem.GetLastWriteTimeUtc("README.TXT"));
            Assert.Null(fileSystem.GetLastWriteTimeUtc("MISSING.TXT"));
        }

        [Fact]
        public void OpenFile_File_Contents()
        {
            var fileSystem = new FolderFileSystem(Root);
            using (var stream = fileSystem.OpenFile("README.TXT"))
            {
                Assert.NotNull(stream);
                Assert.Equal("readme\n", new StreamReader(stream).ReadToEnd());
            }

            Assert.Null(fileSystem.OpenFile("MISSING.TXT"));
        }

        [Fact]
        public void GetFiles_Recursive_RelativePaths()
        {
            var fileSystem = new FolderFileSystem(Root);
            string expected = Path.Combine("INSTALL", "BITPOOL.RSC");
            Assert.Equal([expected], fileSystem.GetFiles(string.Empty, "?*.rsc", recursive: true));
            Assert.Equal(["README.TXT"], fileSystem.GetFiles(string.Empty, "*", recursive: false));
            Assert.Empty(fileSystem.GetFiles("MISSING", "*", recursive: false));
        }

        [Fact]
        public void GetDirectories_Pattern_RelativePaths()
        {
            var fileSystem = new FolderFileSystem(Root);
            Assert.Equal(["INSTALL"], fileSystem.GetDirectories(string.Empty, "???????"));
            Assert.Empty(fileSystem.GetDirectories(string.Empty, "????"));
            Assert.Empty(fileSystem.GetDirectories("MISSING", "*"));
        }

        [Fact]
        public void ReadFirstBytes_NotADriveLetter_Null()
        {
            var fileSystem = new FolderFileSystem(Root);
            Assert.Null(fileSystem.ReadFirstBytes(16));
        }
    }
}
