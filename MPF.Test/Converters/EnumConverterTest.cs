using System;
using System.Collections.Generic;
using System.Linq;
using MPF.Data;
using MPF.Utilities;
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
        [InlineData(DiscImageCreator.CommandStrings.Audio, MediaType.CDROM)]
        [InlineData(DiscImageCreator.CommandStrings.BluRay, MediaType.BluRay)]
        [InlineData(DiscImageCreator.CommandStrings.Close, null)]
        [InlineData(DiscImageCreator.CommandStrings.CompactDisc, MediaType.CDROM)]
        [InlineData(DiscImageCreator.CommandStrings.Data, MediaType.CDROM)]
        [InlineData(DiscImageCreator.CommandStrings.DigitalVideoDisc, MediaType.DVD)]
        [InlineData(DiscImageCreator.CommandStrings.Eject, null)]
        [InlineData(DiscImageCreator.CommandStrings.Floppy, MediaType.FloppyDisk)]
        [InlineData(DiscImageCreator.CommandStrings.GDROM, MediaType.GDROM)]
        [InlineData(DiscImageCreator.CommandStrings.MDS, null)]
        [InlineData(DiscImageCreator.CommandStrings.Reset, null)]
        [InlineData(DiscImageCreator.CommandStrings.SACD, MediaType.CDROM)]
        [InlineData(DiscImageCreator.CommandStrings.Start, null)]
        [InlineData(DiscImageCreator.CommandStrings.Stop, null)]
        [InlineData(DiscImageCreator.CommandStrings.Sub, null)]
        [InlineData(DiscImageCreator.CommandStrings.Swap, MediaType.GDROM)]
        [InlineData(DiscImageCreator.CommandStrings.XBOX, MediaType.DVD)]
        public void BaseCommandToMediaTypeTest(string command, MediaType? expected)
        {
            MediaType? actual = DiscImageCreator.Converters.ToMediaType(command);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(DiscImageCreator.CommandStrings.Audio, RedumpSystem.AudioCD)]
        [InlineData(DiscImageCreator.CommandStrings.BluRay, RedumpSystem.SonyPlayStation3)]
        [InlineData(DiscImageCreator.CommandStrings.Close, null)]
        [InlineData(DiscImageCreator.CommandStrings.CompactDisc, RedumpSystem.IBMPCcompatible)]
        [InlineData(DiscImageCreator.CommandStrings.Data, RedumpSystem.IBMPCcompatible)]
        [InlineData(DiscImageCreator.CommandStrings.DigitalVideoDisc, RedumpSystem.IBMPCcompatible)]
        [InlineData(DiscImageCreator.CommandStrings.Eject, null)]
        [InlineData(DiscImageCreator.CommandStrings.Floppy, RedumpSystem.IBMPCcompatible)]
        [InlineData(DiscImageCreator.CommandStrings.GDROM, RedumpSystem.SegaDreamcast)]
        [InlineData(DiscImageCreator.CommandStrings.MDS, null)]
        [InlineData(DiscImageCreator.CommandStrings.Reset, null)]
        [InlineData(DiscImageCreator.CommandStrings.SACD, RedumpSystem.SuperAudioCD)]
        [InlineData(DiscImageCreator.CommandStrings.Start, null)]
        [InlineData(DiscImageCreator.CommandStrings.Stop, null)]
        [InlineData(DiscImageCreator.CommandStrings.Sub, null)]
        [InlineData(DiscImageCreator.CommandStrings.Swap, RedumpSystem.SegaDreamcast)]
        [InlineData(DiscImageCreator.CommandStrings.XBOX, RedumpSystem.MicrosoftXbox)]
        public void BaseCommandToRedumpSystemTest(string command, RedumpSystem? expected)
        {
            RedumpSystem? actual = DiscImageCreator.Converters.ToRedumpSystem(command);
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
            string actual = DiscImageCreator.Converters.Extension(mediaType);
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
