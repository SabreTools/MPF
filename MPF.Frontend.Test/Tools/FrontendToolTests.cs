using System;
using System.IO;
using MPF.Frontend.Tools;
using SabreTools.RedumpLib.Data;
using Xunit;

namespace MPF.Frontend.Test.Tools
{
    public class FrontendToolTests
    {
        #region GetAvailableFreeSpace

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void GetAvailableFreeSpace_Empty_ReturnsNull(string? path)
        {
            long? actual = FrontendTool.GetAvailableFreeSpace(path);
            Assert.Null(actual);
        }

        [Fact]
        public void GetAvailableFreeSpace_MissingDirectory_ReturnsFreeSpace()
        {
            string path = Path.Combine(Environment.CurrentDirectory, "TestData", "missing", "track");
            long? actual = FrontendTool.GetAvailableFreeSpace(path);
            Assert.NotNull(actual);
            Assert.True(actual > 0);
        }

        #endregion

        #region GetDefaultSpeedForPhysicalMediaType

        [Theory]
        [MemberData(nameof(GenerateGetDefaultSpeedForPhysicalMediaTypeSegmentedTestData))]
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

        /// <summary>
        /// Generate a test set for GetDefaultSpeedForPhysicalMediaTypeSegmentedTest
        /// </summary>
        public static TheoryData<PhysicalMediaType?, int> GenerateGetDefaultSpeedForPhysicalMediaTypeSegmentedTestData()
        {
            return new TheoryData<PhysicalMediaType?, int>
            {
                { null, 72 },
                { PhysicalMediaType.CDROM, 72 },
                { PhysicalMediaType.GDROM, 72 },
                { PhysicalMediaType.DVD, 24 },
                { PhysicalMediaType.NintendoGameCubeGameDisc, 24 },
                { PhysicalMediaType.NintendoWiiOpticalDisc, 24 },
                { PhysicalMediaType.XGD, 24 },
                { PhysicalMediaType.HDDVD, 24 },
                { PhysicalMediaType.BluRay, 16 },
                { PhysicalMediaType.NintendoWiiUOpticalDisc, 16 },
            };
        }

        #endregion

        #region GetFreeSpaceCheckPath

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void GetFreeSpaceCheckPath_Empty_ReturnsNull(string? path)
        {
            string? actual = FrontendTool.GetFreeSpaceCheckPath(path);
            Assert.Null(actual);
        }

        [Fact]
        public void GetFreeSpaceCheckPath_ExistingDirectory_ReturnsDirectory()
        {
            string directory = Path.Combine(Environment.CurrentDirectory, "TestData");
            string? actual = FrontendTool.GetFreeSpaceCheckPath(directory);
            Assert.Equal(directory, actual);
        }

        [Fact]
        public void GetFreeSpaceCheckPath_MissingDirectory_ReturnsClosestExisting()
        {
            string directory = Path.Combine(Environment.CurrentDirectory, "TestData");
            string path = Path.Combine(directory, "missing", "track");
            string? actual = FrontendTool.GetFreeSpaceCheckPath(path);
            Assert.Equal(directory, actual);
        }

        [Fact]
        public void GetFreeSpaceCheckPath_MissingRelativePath_ReturnsNull()
        {
            string path = Path.Combine($"mpf-test-missing-{Guid.NewGuid():N}", "track");
            string? actual = FrontendTool.GetFreeSpaceCheckPath(path);
            Assert.Null(actual);
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
