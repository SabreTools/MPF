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
        [MemberData(nameof(GeneratePhysicalSystemFromVolumeLabelData))]
        public void GetPhysicalSystemFromVolumeLabelTest(string? volumeLabel, PhysicalSystem? expected)
        {
            PhysicalSystem? actual = FrontendTool.GetPhysicalSystemFromVolumeLabel(volumeLabel);
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Generate a test set for GetPhysicalSystemFromVolumeLabelTest
        /// </summary>
        public static TheoryData<string?, PhysicalSystem?> GeneratePhysicalSystemFromVolumeLabelData()
        {
            return new TheoryData<string?, PhysicalSystem?>
            {
                { null, null },
                { "", null },
                { "Audio CD", PhysicalSystem.AudioCD },
                { "SEP13011042", PhysicalSystem.MicrosoftXbox },
                { "SEP13011042072", PhysicalSystem.MicrosoftXbox },
                { "XBOX360", PhysicalSystem.MicrosoftXbox360 },
                { "XGD2DVD_NTSC", PhysicalSystem.MicrosoftXbox360 },
                { "Sega_CD", PhysicalSystem.SegaMegaCDSegaCD },
                { "PS3VOLUME", PhysicalSystem.SonyPlayStation3 },
                { "PS4VOLUME", PhysicalSystem.SonyPlayStation4 },
                { "PS5VOLUME", PhysicalSystem.SonyPlayStation5 },
            };
        }

        #endregion

        #region NormalizeDiscTitle

        // TODO: Write NormalizeDiscTitle(string?, LanguageCode?[]?) test
        // TODO: Write NormalizeDiscTitle(string?, LanguageCode?) test

        #endregion
    }
}
