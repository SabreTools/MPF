using System.IO;
using SabreTools.IO;
using Xunit;

namespace MPF.Frontend.Test
{
    public class OptionsTests
    {
        /// <summary>
        /// A writable current directory keeps the relative default
        /// </summary>
        [Fact]
        public void GetDefaultOutputPath_WritableDirectory_ReturnsRelativePath()
        {
            string currentDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(currentDirectory);

            try
            {
                string actual = DumpSettings.GetDefaultOutputPath(currentDirectory);
                Assert.Equal("ISO", actual);
            }
            finally
            {
                Directory.Delete(currentDirectory);
            }
        }

        /// <summary>
        /// A current directory that can not hold a file falls back to the home directory
        /// </summary>
        /// <remarks>
        /// A file stands in for the directory because that is the only way of making the creation
        /// fail on every platform. Denied permissions end up in the same branch.
        /// </remarks>
        [Fact]
        public void GetDefaultOutputPath_UnwritableDirectory_ReturnsHomePath()
        {
            string currentDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            File.Create(currentDirectory).Dispose();

            try
            {
                string actual = DumpSettings.GetDefaultOutputPath(currentDirectory);
                Assert.Equal(Path.Combine(PathTool.GetHomeDirectory(), "ISO"), actual);
            }
            finally
            {
                File.Delete(currentDirectory);
            }
        }

        /// <summary>
        /// A missing current directory falls back to the home directory
        /// </summary>
        [Fact]
        public void GetDefaultOutputPath_MissingDirectory_ReturnsHomePath()
        {
            string currentDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            string actual = DumpSettings.GetDefaultOutputPath(currentDirectory);

            Assert.Equal(Path.Combine(PathTool.GetHomeDirectory(), "ISO"), actual);
        }

        /// <summary>
        /// Clearing the default output path restores the default instead of leaving it empty
        /// </summary>
        [Fact]
        public void DefaultOutputPath_SetToNull_RestoresDefault()
        {
            var options = new Options();

            options.Dumping.DefaultOutputPath = null;

            Assert.Equal(DumpSettings.DefaultOutputPathValue, options.Dumping.DefaultOutputPath);
        }
    }
}
