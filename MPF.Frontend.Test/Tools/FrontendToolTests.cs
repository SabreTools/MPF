using System;
using System.IO;
using MPF.Frontend.Tools;
using SabreTools.RedumpLib.Data;
using Xunit;

namespace MPF.Frontend.Test.Tools
{
    public class FrontendToolTests
    {
        #region GetDefaultSpeedForPhysicalMediaType

        [Theory]
        [InlineData(null, 72)]
        [InlineData(PhysicalMediaType.CDROM, 72)]
        [InlineData(PhysicalMediaType.GDROM, 72)]
        [InlineData(PhysicalMediaType.DVD, 24)]
        [InlineData(PhysicalMediaType.NintendoGameCubeGameDisc, 24)]
        [InlineData(PhysicalMediaType.NintendoWiiOpticalDisc, 24)]
        [InlineData(PhysicalMediaType.HDDVD, 24)]
        [InlineData(PhysicalMediaType.BluRay, 16)]
        [InlineData(PhysicalMediaType.NintendoWiiUOpticalDisc, 16)]
        public void GetDefaultSpeedForPhysicalMediaTypeSegmentedTest(PhysicalMediaType? mediaType, int expected)
        {
            var options = new Options();
            options.Dumping.DumpSpeeds.CD = 72;
            options.Dumping.DumpSpeeds.DVD = 24;
            options.Dumping.DumpSpeeds.HDDVD = 24;
            options.Dumping.DumpSpeeds.Bluray = 16;

            int actual = FrontendTool.GetDefaultSpeedForPhysicalMediaType(mediaType, options);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetPhysicalSystemFromVolumeLabel

        [Theory]
        [InlineData(null, null)]
        [InlineData("", null)]
        [InlineData("Audio CD", PhysicalSystem.AudioCD)]
        [InlineData("SEP13011042", PhysicalSystem.MicrosoftXbox)]
        [InlineData("SEP13011042072", PhysicalSystem.MicrosoftXbox)]
        [InlineData("XBOX360", PhysicalSystem.MicrosoftXbox360)]
        [InlineData("XGD2DVD_NTSC", PhysicalSystem.MicrosoftXbox360)]
        [InlineData("Sega_CD", PhysicalSystem.SegaMegaCDSegaCD)]
        [InlineData("PS3VOLUME", PhysicalSystem.SonyPlayStation3)]
        [InlineData("PS4VOLUME", PhysicalSystem.SonyPlayStation4)]
        [InlineData("PS5VOLUME", PhysicalSystem.SonyPlayStation5)]
        public void GetPhysicalSystemFromVolumeLabelTest(string? volumeLabel, PhysicalSystem? expected)
        {
            PhysicalSystem? actual = FrontendTool.GetPhysicalSystemFromVolumeLabel(volumeLabel);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region NormalizeDiscTitle

        // TODO: Write NormalizeDiscTitle(string?, Language?[]?) test
        // TODO: Write NormalizeDiscTitle(string?, Language?) test

        #endregion

        #region ResolveBinaryPath

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ResolveBinaryPath_NullOrEmpty_ReturnsNull(string? input)
        {
            Assert.Null(FrontendTool.ResolveBinaryPath(input));
        }

        [Fact]
        public void ResolveBinaryPath_AbsolutePathExists_ReturnsAsIs()
        {
            // Use this assembly's location — guaranteed to exist on disk.
            string self = typeof(FrontendToolTests).Assembly.Location;
            Assert.Equal(self, FrontendTool.ResolveBinaryPath(self));
        }

        [Fact]
        public void ResolveBinaryPath_AbsolutePathMissing_ReturnsNull()
        {
            string missing = Path.Combine(Path.GetTempPath(), $"mpf-test-{Guid.NewGuid():N}.bin");
            Assert.False(File.Exists(missing));
            Assert.Null(FrontendTool.ResolveBinaryPath(missing));
        }

        [Fact]
        public void ResolveBinaryPath_RelativePathSeparator_ReturnsNullWhenMissing()
        {
            // Contains a separator → treated as explicit path, not searched on PATH.
            string relative = Path.Combine("definitely-not-a-dir", $"mpf-test-{Guid.NewGuid():N}.bin");
            Assert.Null(FrontendTool.ResolveBinaryPath(relative));
        }

        [Fact]
        public void ResolveBinaryPath_BareName_FoundInRuntimeDirectory()
        {
            // Create a file in the test assembly's runtime directory; the bare-name lookup
            // should find it via the runtime-directory probe.
            string runtimeDir = AppContext.BaseDirectory;
            string fileName = $"mpf-test-{Guid.NewGuid():N}.bin";
            string fullPath = Path.Combine(runtimeDir, fileName);

            File.WriteAllBytes(fullPath, Array.Empty<byte>());
            try
            {
                string? resolved = FrontendTool.ResolveBinaryPath(fileName);
                Assert.Equal(fullPath, resolved);
            }
            finally
            {
                File.Delete(fullPath);
            }
        }

        [Fact]
        public void ResolveBinaryPath_BareName_FoundOnPath()
        {
            // Stage a file in a temporary directory, prepend it to PATH, and ensure
            // bare-name resolution picks it up after the runtime-dir probe misses.
            string tempDir = Path.Combine(Path.GetTempPath(), $"mpf-test-{Guid.NewGuid():N}");
            Directory.CreateDirectory(tempDir);
            string fileName = $"mpf-test-{Guid.NewGuid():N}.bin";
            string fullPath = Path.Combine(tempDir, fileName);
            File.WriteAllBytes(fullPath, Array.Empty<byte>());

            string? originalPath = Environment.GetEnvironmentVariable("PATH");
            try
            {
                Environment.SetEnvironmentVariable("PATH", tempDir + Path.PathSeparator + (originalPath ?? string.Empty));
                string? resolved = FrontendTool.ResolveBinaryPath(fileName);
                Assert.Equal(fullPath, resolved);
            }
            finally
            {
                Environment.SetEnvironmentVariable("PATH", originalPath);
                File.Delete(fullPath);
                Directory.Delete(tempDir);
            }
        }

        [Fact]
        public void ResolveBinaryPath_BareName_NotFound_ReturnsNull()
        {
            string fileName = $"mpf-test-{Guid.NewGuid():N}-not-real.bin";
            Assert.Null(FrontendTool.ResolveBinaryPath(fileName));
        }

        #endregion
    }
}
