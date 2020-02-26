using System;
using DICUI.Data;
using DICUI.DiscImageCreator;
using DICUI.Utilities;
using Xunit;

namespace DICUI.Test.Utilities
{
    public class ConvertersTest
    {
        [Theory]
        [InlineData(Command.Audio, MediaType.CDROM)]
        [InlineData(Command.BluRay, MediaType.BluRay)]
        [InlineData(Command.Close, null)]
        [InlineData(Command.CompactDisc, MediaType.CDROM)]
        [InlineData(Command.Data, MediaType.CDROM)]
        [InlineData(Command.DigitalVideoDisc, MediaType.DVD)]
        [InlineData(Command.Eject, null)]
        [InlineData(Command.Floppy, MediaType.FloppyDisk)]
        [InlineData(Command.GDROM, MediaType.GDROM)]
        [InlineData(Command.MDS, null)]
        [InlineData(Command.Reset, null)]
        [InlineData(Command.SACD, MediaType.CDROM)]
        [InlineData(Command.Start, null)]
        [InlineData(Command.Stop, null)]
        [InlineData(Command.Sub, null)]
        [InlineData(Command.Swap, MediaType.GDROM)]
        [InlineData(Command.XBOX, MediaType.DVD)]
        public void BaseCommandToMediaTypeTest(Command command, MediaType? expected)
        {
            MediaType? actual = command.ToMediaType();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(Command.Audio, KnownSystem.AudioCD)]
        [InlineData(Command.BluRay, KnownSystem.SonyPlayStation3)]
        [InlineData(Command.Close, null)]
        [InlineData(Command.CompactDisc, KnownSystem.IBMPCCompatible)]
        [InlineData(Command.Data, KnownSystem.IBMPCCompatible)]
        [InlineData(Command.DigitalVideoDisc, KnownSystem.IBMPCCompatible)]
        [InlineData(Command.Eject, null)]
        [InlineData(Command.Floppy, KnownSystem.IBMPCCompatible)]
        [InlineData(Command.GDROM, KnownSystem.SegaDreamcast)]
        [InlineData(Command.MDS, null)]
        [InlineData(Command.Reset, null)]
        [InlineData(Command.SACD, KnownSystem.SuperAudioCD)]
        [InlineData(Command.Start, null)]
        [InlineData(Command.Stop, null)]
        [InlineData(Command.Sub, null)]
        [InlineData(Command.Swap, KnownSystem.SegaDreamcast)]
        [InlineData(Command.XBOX, KnownSystem.MicrosoftXBOX)]
        public void BaseCommandToKnownSystemTest(Command command, KnownSystem? expected)
        {
            KnownSystem? actual = Converters.ToKnownSystem(command);
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
