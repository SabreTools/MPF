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
            var options = new Options();
            options.Dumping.DumpSpeeds.CD = 72;
            options.Dumping.DumpSpeeds.DVD = 24;
            options.Dumping.DumpSpeeds.HDDVD = 24;
            options.Dumping.DumpSpeeds.Bluray = 16;

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
    }
}
