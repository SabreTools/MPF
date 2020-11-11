using System;
using MPF.Data;
using MPF.Utilities;
using Xunit;

namespace MPF.Test.Utilities
{
    public class ConvertersTest
    {
        [Theory]
        [InlineData(DiscImageCreator.Command.Audio, MediaType.CDROM)]
        [InlineData(DiscImageCreator.Command.BluRay, MediaType.BluRay)]
        [InlineData(DiscImageCreator.Command.Close, null)]
        [InlineData(DiscImageCreator.Command.CompactDisc, MediaType.CDROM)]
        [InlineData(DiscImageCreator.Command.Data, MediaType.CDROM)]
        [InlineData(DiscImageCreator.Command.DigitalVideoDisc, MediaType.DVD)]
        [InlineData(DiscImageCreator.Command.Eject, null)]
        [InlineData(DiscImageCreator.Command.Floppy, MediaType.FloppyDisk)]
        [InlineData(DiscImageCreator.Command.GDROM, MediaType.GDROM)]
        [InlineData(DiscImageCreator.Command.MDS, null)]
        [InlineData(DiscImageCreator.Command.Reset, null)]
        [InlineData(DiscImageCreator.Command.SACD, MediaType.CDROM)]
        [InlineData(DiscImageCreator.Command.Start, null)]
        [InlineData(DiscImageCreator.Command.Stop, null)]
        [InlineData(DiscImageCreator.Command.Sub, null)]
        [InlineData(DiscImageCreator.Command.Swap, MediaType.GDROM)]
        [InlineData(DiscImageCreator.Command.XBOX, MediaType.DVD)]
        public void BaseCommandToMediaTypeTest(DiscImageCreator.Command command, MediaType? expected)
        {
            MediaType? actual = DiscImageCreator.Converters.ToMediaType(command);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(DiscImageCreator.Command.Audio, KnownSystem.AudioCD)]
        [InlineData(DiscImageCreator.Command.BluRay, KnownSystem.SonyPlayStation3)]
        [InlineData(DiscImageCreator.Command.Close, null)]
        [InlineData(DiscImageCreator.Command.CompactDisc, KnownSystem.IBMPCCompatible)]
        [InlineData(DiscImageCreator.Command.Data, KnownSystem.IBMPCCompatible)]
        [InlineData(DiscImageCreator.Command.DigitalVideoDisc, KnownSystem.IBMPCCompatible)]
        [InlineData(DiscImageCreator.Command.Eject, null)]
        [InlineData(DiscImageCreator.Command.Floppy, KnownSystem.IBMPCCompatible)]
        [InlineData(DiscImageCreator.Command.GDROM, KnownSystem.SegaDreamcast)]
        [InlineData(DiscImageCreator.Command.MDS, null)]
        [InlineData(DiscImageCreator.Command.Reset, null)]
        [InlineData(DiscImageCreator.Command.SACD, KnownSystem.SuperAudioCD)]
        [InlineData(DiscImageCreator.Command.Start, null)]
        [InlineData(DiscImageCreator.Command.Stop, null)]
        [InlineData(DiscImageCreator.Command.Sub, null)]
        [InlineData(DiscImageCreator.Command.Swap, KnownSystem.SegaDreamcast)]
        [InlineData(DiscImageCreator.Command.XBOX, KnownSystem.MicrosoftXBOX)]
        public void BaseCommandToKnownSystemTest(DiscImageCreator.Command command, KnownSystem? expected)
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

        [Fact]
        public void KnownSystemHasValidCategory()
        {
            var values = Validators.CreateListOfSystems();
            KnownSystem[] markers = { KnownSystem.MarkerArcadeEnd, KnownSystem.MarkerDiscBasedConsoleEnd, /* KnownSystem.MarkerOtherConsoleEnd, */ KnownSystem.MarkerComputerEnd, KnownSystem.MarkerOtherEnd };

            values.ForEach(system => {
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
            });
        }
    }
}
