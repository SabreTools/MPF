using System.IO;
using MPF.Frontend.Tools;
using SabreTools.RedumpLib.Data;
using Xunit;

namespace MPF.Frontend.Test.Tools
{
    public class FrontendToolTests
    {
        #region GetDefaultSpeedForMediaType

        [Theory]
        [InlineData(null, 72)]
        [InlineData(MediaType.CDROM, 72)]
        [InlineData(MediaType.GDROM, 72)]
        [InlineData(MediaType.DVD, 24)]
        [InlineData(MediaType.NintendoGameCubeGameDisc, 24)]
        [InlineData(MediaType.NintendoWiiOpticalDisc, 24)]
        [InlineData(MediaType.HDDVD, 24)]
        [InlineData(MediaType.BluRay, 16)]
        [InlineData(MediaType.NintendoWiiUOpticalDisc, 16)]
        public void GetDefaultSpeedForMediaTypeSegmentedTest(MediaType? mediaType, int expected)
        {
            var options = new SegmentedOptions();
            options.Dumping.DumpSpeeds.PreferredCD = 72;
            options.Dumping.DumpSpeeds.PreferredDVD = 24;
            options.Dumping.DumpSpeeds.PreferredHDDVD = 24;
            options.Dumping.DumpSpeeds.PreferredBD = 16;

            int actual = FrontendTool.GetDefaultSpeedForMediaType(mediaType, options);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetRedumpSystemFromVolumeLabel

        [Theory]
        [InlineData(null, null)]
        [InlineData("", null)]
        [InlineData("Audio CD", RedumpSystem.AudioCD)]
        [InlineData("SEP13011042", RedumpSystem.MicrosoftXbox)]
        [InlineData("SEP13011042072", RedumpSystem.MicrosoftXbox)]
        [InlineData("XBOX360", RedumpSystem.MicrosoftXbox360)]
        [InlineData("XGD2DVD_NTSC", RedumpSystem.MicrosoftXbox360)]
        [InlineData("Sega_CD", RedumpSystem.SegaMegaCDSegaCD)]
        [InlineData("PS3VOLUME", RedumpSystem.SonyPlayStation3)]
        [InlineData("PS4VOLUME", RedumpSystem.SonyPlayStation4)]
        [InlineData("PS5VOLUME", RedumpSystem.SonyPlayStation5)]
        public void GetRedumpSystemFromVolumeLabelTest(string? volumeLabel, RedumpSystem? expected)
        {
            RedumpSystem? actual = FrontendTool.GetRedumpSystemFromVolumeLabel(volumeLabel);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region NormalizeDiscTitle

        // TODO: Write NormalizeDiscTitle(string?, Language?[]?) test
        // TODO: Write NormalizeDiscTitle(string?, Language?) test

        #endregion

        #region NormalizeOutputPaths

        [Theory]
        [InlineData(null, false, "")]
        [InlineData(null, true, "")]
        [InlineData("", false, "")]
        [InlineData("", true, "")]
        [InlineData("filename.bin", false, "filename.bin")]
        [InlineData("filename.bin", true, "filename.bin")]
        [InlineData("\"filename.bin\"", false, "filename.bin")]
        [InlineData("\"filename.bin\"", true, "filename.bin")]
        [InlineData("<filename.bin>", false, "filename.bin")]
        [InlineData("<filename.bin>", true, "filename.bin")]
        [InlineData("1.2.3.4..bin", false, "1.2.3.4..bin")]
        [InlineData("1.2.3.4..bin", true, "1.2.3.4..bin")]
        [InlineData("dir/filename.bin", false, "dir/filename.bin")]
        [InlineData("dir/filename.bin", true, "dir/filename.bin")]
        [InlineData(" dir / filename.bin", false, "dir/filename.bin")]
        [InlineData(" dir / filename.bin", true, "dir/filename.bin")]
        [InlineData("\0dir/\0filename.bin", false, "_dir/_filename.bin")]
        [InlineData("\0dir/\0filename.bin", true, "_dir/_filename.bin")]
        public void NormalizeOutputPathsTest(string? path, bool getFullPath, string expected)
        {
            // Modify expected to account for test data if necessary
            if (getFullPath && !string.IsNullOrEmpty(expected))
                expected = Path.GetFullPath(expected);

            string actual = FrontendTool.NormalizeOutputPaths(path, getFullPath);
            Assert.Equal(expected, actual);
        }

        #endregion
    }
}
