using System;
using System.Collections.Generic;
using System.Linq;
using MPF.Data;
using MPF.Utilities;
using Xunit;

namespace MPF.Test.Utilities
{
    public class ConvertersTest
    {
        /// <summary>
        /// Set of all known systems for testing
        /// </summary>
        public static IEnumerable<object[]> KnownSystems = KnownSystemComboBoxItem.GenerateElements().Select(e => new object[] { e });

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
        [InlineData(DiscImageCreator.CommandStrings.Audio, KnownSystem.AudioCD)]
        [InlineData(DiscImageCreator.CommandStrings.BluRay, KnownSystem.SonyPlayStation3)]
        [InlineData(DiscImageCreator.CommandStrings.Close, null)]
        [InlineData(DiscImageCreator.CommandStrings.CompactDisc, KnownSystem.IBMPCCompatible)]
        [InlineData(DiscImageCreator.CommandStrings.Data, KnownSystem.IBMPCCompatible)]
        [InlineData(DiscImageCreator.CommandStrings.DigitalVideoDisc, KnownSystem.IBMPCCompatible)]
        [InlineData(DiscImageCreator.CommandStrings.Eject, null)]
        [InlineData(DiscImageCreator.CommandStrings.Floppy, KnownSystem.IBMPCCompatible)]
        [InlineData(DiscImageCreator.CommandStrings.GDROM, KnownSystem.SegaDreamcast)]
        [InlineData(DiscImageCreator.CommandStrings.MDS, null)]
        [InlineData(DiscImageCreator.CommandStrings.Reset, null)]
        [InlineData(DiscImageCreator.CommandStrings.SACD, KnownSystem.SuperAudioCD)]
        [InlineData(DiscImageCreator.CommandStrings.Start, null)]
        [InlineData(DiscImageCreator.CommandStrings.Stop, null)]
        [InlineData(DiscImageCreator.CommandStrings.Sub, null)]
        [InlineData(DiscImageCreator.CommandStrings.Swap, KnownSystem.SegaDreamcast)]
        [InlineData(DiscImageCreator.CommandStrings.XBOX, KnownSystem.MicrosoftXBOX)]
        public void BaseCommandToKnownSystemTest(string command, KnownSystem? expected)
        {
            KnownSystem? actual = DiscImageCreator.Converters.ToKnownSystem(command);
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
        [InlineData(MediaType.CDROM, "CD-ROM")]
        [InlineData(MediaType.LaserDisc, "LD-ROM / LV-ROM")]
        [InlineData(MediaType.NONE, "Unknown")]
        public void MediaTypeToStringTest(MediaType? mediaType, string expected)
        {
            string actual = Converters.LongName(mediaType);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(KnownSystem.MicrosoftXBOX, "Microsoft XBOX")]
        [InlineData(KnownSystem.NECPC88, "NEC PC-88")]
        [InlineData(KnownSystem.KonamiPython, "Konami Python")]
        [InlineData(KnownSystem.HDDVDVideo, "HD-DVD-Video")]
        [InlineData(KnownSystem.NONE, "Unknown")]
        public void KnownSystemToStringTest(KnownSystem? knownSystem, string expected)
        {
            string actual = Converters.LongName(knownSystem);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [MemberData(nameof(KnownSystems))]
        public void KnownSystemHasValidCategory(KnownSystemComboBoxItem system)
        {
            KnownSystem[] markers = { KnownSystem.MarkerArcadeEnd, KnownSystem.MarkerDiscBasedConsoleEnd, /* KnownSystem.MarkerOtherConsoleEnd, */ KnownSystem.MarkerComputerEnd, KnownSystem.MarkerOtherEnd };

            // Non-system items won't map
            if (system.IsHeader)
                return;

            // NONE will never map
            if (system == KnownSystem.NONE)
                return;

            // we check that the category is the first category value higher than the system
            KnownSystemCategory category = ((KnownSystem?)system).Category();
            KnownSystem marker = KnownSystem.NONE;

            switch (category)
            {
                case KnownSystemCategory.Arcade: marker = KnownSystem.MarkerArcadeEnd; break;
                case KnownSystemCategory.DiscBasedConsole: marker = KnownSystem.MarkerDiscBasedConsoleEnd; break;
                /* case KnownSystemCategory.OtherConsole: marker = KnownSystem.MarkerOtherConsoleEnd; break; */
                case KnownSystemCategory.Computer: marker = KnownSystem.MarkerComputerEnd; break;
                case KnownSystemCategory.Other: marker = KnownSystem.MarkerOtherEnd; break;
            }

            Assert.NotEqual(KnownSystem.NONE, marker);
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
