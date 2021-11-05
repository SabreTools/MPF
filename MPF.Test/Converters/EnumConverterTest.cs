using System;
using System.Collections.Generic;
using System.Linq;
using MPF.Modules.DiscImageCreator;
using RedumpLib.Data;
using Xunit;

namespace MPF.Test.Converters
{
    public class EnumConverterTest
    {
        /// <summary>
        /// Set of all known systems for testing
        /// </summary>
        public static IEnumerable<object[]> RedumpSystems = RedumpSystemComboBoxItem.GenerateElements().Select(e => new object[] { e });

        [Theory]
        [InlineData(CommandStrings.Audio, MediaType.CDROM)]
        [InlineData(CommandStrings.BluRay, MediaType.BluRay)]
        [InlineData(CommandStrings.Close, null)]
        [InlineData(CommandStrings.CompactDisc, MediaType.CDROM)]
        [InlineData(CommandStrings.Data, MediaType.CDROM)]
        [InlineData(CommandStrings.DigitalVideoDisc, MediaType.DVD)]
        [InlineData(CommandStrings.Eject, null)]
        [InlineData(CommandStrings.Floppy, MediaType.FloppyDisk)]
        [InlineData(CommandStrings.GDROM, MediaType.GDROM)]
        [InlineData(CommandStrings.MDS, null)]
        [InlineData(CommandStrings.Reset, null)]
        [InlineData(CommandStrings.SACD, MediaType.CDROM)]
        [InlineData(CommandStrings.Start, null)]
        [InlineData(CommandStrings.Stop, null)]
        [InlineData(CommandStrings.Sub, null)]
        [InlineData(CommandStrings.Swap, MediaType.GDROM)]
        [InlineData(CommandStrings.XBOX, MediaType.DVD)]
        public void BaseCommandToMediaTypeTest(string command, MediaType? expected)
        {
            MediaType? actual = MPF.Modules.DiscImageCreator.Converters.ToMediaType(command);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(CommandStrings.Audio, RedumpSystem.AudioCD)]
        [InlineData(CommandStrings.BluRay, RedumpSystem.SonyPlayStation3)]
        [InlineData(CommandStrings.Close, null)]
        [InlineData(CommandStrings.CompactDisc, RedumpSystem.IBMPCcompatible)]
        [InlineData(CommandStrings.Data, RedumpSystem.IBMPCcompatible)]
        [InlineData(CommandStrings.DigitalVideoDisc, RedumpSystem.IBMPCcompatible)]
        [InlineData(CommandStrings.Eject, null)]
        [InlineData(CommandStrings.Floppy, RedumpSystem.IBMPCcompatible)]
        [InlineData(CommandStrings.GDROM, RedumpSystem.SegaDreamcast)]
        [InlineData(CommandStrings.MDS, null)]
        [InlineData(CommandStrings.Reset, null)]
        [InlineData(CommandStrings.SACD, RedumpSystem.SuperAudioCD)]
        [InlineData(CommandStrings.Start, null)]
        [InlineData(CommandStrings.Stop, null)]
        [InlineData(CommandStrings.Sub, null)]
        [InlineData(CommandStrings.Swap, RedumpSystem.SegaDreamcast)]
        [InlineData(CommandStrings.XBOX, RedumpSystem.MicrosoftXbox)]
        public void BaseCommandToRedumpSystemTest(string command, RedumpSystem? expected)
        {
            RedumpSystem? actual = MPF.Modules.DiscImageCreator.Converters.ToRedumpSystem(command);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(MediaType.CDROM, ".bin")]
        [InlineData(MediaType.DVD, ".iso")]
        [InlineData(MediaType.LaserDisc, ".raw")]
        [InlineData(MediaType.NintendoWiiUOpticalDisc, ".wud")]
        [InlineData(MediaType.FloppyDisk, ".img")]
        [InlineData(MediaType.Cassette, ".wav")]
        [InlineData(MediaType.NONE, null)]
        public void MediaTypeToExtensionTest(MediaType? mediaType, string expected)
        {
            string actual = MPF.Modules.DiscImageCreator.Converters.Extension(mediaType);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(RedumpSystems))]
        public void RedumpSystemHasValidCategory(RedumpSystemComboBoxItem system)
        {
            RedumpSystem[] markers = { RedumpSystem.MarkerArcadeEnd, RedumpSystem.MarkerDiscBasedConsoleEnd, /* RedumpSystem.MarkerOtherConsoleEnd, */ RedumpSystem.MarkerComputerEnd, RedumpSystem.MarkerOtherEnd };

            // Non-system items won't map
            if (system.IsHeader)
                return;

            // Null will never map
            if (system?.Value == null)
                return;

            // we check that the category is the first category value higher than the system
            SystemCategory category = ((RedumpSystem?)system).GetCategory();
            RedumpSystem? marker = null;

            switch (category)
            {
                case SystemCategory.Arcade: marker = RedumpSystem.MarkerArcadeEnd; break;
                case SystemCategory.DiscBasedConsole: marker = RedumpSystem.MarkerDiscBasedConsoleEnd; break;
                /* case SystemCategory.OtherConsole: marker = RedumpSystem.MarkerOtherConsoleEnd; break; */
                case SystemCategory.Computer: marker = RedumpSystem.MarkerComputerEnd; break;
                case SystemCategory.Other: marker = RedumpSystem.MarkerOtherEnd; break;
            }

            Assert.NotNull(marker);
            Assert.True(marker > system);

            Array.ForEach(markers, mmarker =>
            {
                // a marker can be the same of the found one, or one of a category before or a category after but never in the middle between
                // the system and the mapped category
                Assert.True(mmarker == marker || mmarker < system || mmarker > marker);
            });
        }
    }
}
