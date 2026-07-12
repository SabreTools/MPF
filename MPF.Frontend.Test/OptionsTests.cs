using System;
using System.IO;
using Xunit;

namespace MPF.Frontend.Test
{
    public class OptionsTests
    {
        /// <summary>
        /// The default output path must not be relative anywhere but Windows
        /// </summary>
        /// <remarks>
        /// A relative path resolves against the working directory, not against the application
        /// directory. That is what the portable Windows layout wants, where the folder sits next to
        /// the executable. An installed application has no such folder, so a relative default sends
        /// dumps wherever MPF happened to be started from.
        /// </remarks>
        [Fact]
        public void DefaultOutputPath_OutsideWindows_IsRooted()
        {
            var options = new Options();

            string? actual = options.Dumping.DefaultOutputPath;

            if (Environment.OSVersion.Platform == PlatformID.Unix
                || Environment.OSVersion.Platform == PlatformID.MacOSX)
                Assert.True(Path.IsPathRooted(actual), $"Expected a rooted path, found '{actual}'");
            else
                Assert.Equal("ISO", actual);
        }

        /// <summary>
        /// Clearing the default output path restores the platform default, not a relative one
        /// </summary>
        [Fact]
        public void DefaultOutputPath_SetToNull_RestoresPlatformDefault()
        {
            var options = new Options();

            options.Dumping.DefaultOutputPath = null;

            string? actual = options.Dumping.DefaultOutputPath;
            if (Environment.OSVersion.Platform == PlatformID.Unix
                || Environment.OSVersion.Platform == PlatformID.MacOSX)
                Assert.True(Path.IsPathRooted(actual), $"Expected a rooted path, found '{actual}'");
            else
                Assert.Equal("ISO", actual);
        }
    }
}
