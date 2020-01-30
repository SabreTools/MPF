using System;
using DICUI.Data;
using DICUI.Utilities;
using Xunit;

namespace DICUI.Test.Utilities
{
    public class ConvertersTest
    {
        [Theory]
        [InlineData(CreatorCommand.Audio, MediaType.CDROM)]
        [InlineData(CreatorCommand.BluRay, MediaType.BluRay)]
        [InlineData(CreatorCommand.Close, null)]
        [InlineData(CreatorCommand.CompactDisc, MediaType.CDROM)]
        [InlineData(CreatorCommand.Data, MediaType.CDROM)]
        [InlineData(CreatorCommand.DigitalVideoDisc, MediaType.DVD)]
        [InlineData(CreatorCommand.Eject, null)]
        [InlineData(CreatorCommand.Floppy, MediaType.FloppyDisk)]
        [InlineData(CreatorCommand.GDROM, MediaType.GDROM)]
        [InlineData(CreatorCommand.MDS, null)]
        [InlineData(CreatorCommand.Reset, null)]
        [InlineData(CreatorCommand.SACD, MediaType.CDROM)]
        [InlineData(CreatorCommand.Start, null)]
        [InlineData(CreatorCommand.Stop, null)]
        [InlineData(CreatorCommand.Sub, null)]
        [InlineData(CreatorCommand.Swap, MediaType.GDROM)]
        [InlineData(CreatorCommand.XBOX, MediaType.DVD)]
        public void BaseCommandToMediaTypeTest(CreatorCommand command, MediaType? expected)
        {
            MediaType? actual = command.ToMediaType();
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(CreatorCommand.Audio, KnownSystem.AudioCD)]
        [InlineData(CreatorCommand.BluRay, KnownSystem.SonyPlayStation3)]
        [InlineData(CreatorCommand.Close, null)]
        [InlineData(CreatorCommand.CompactDisc, KnownSystem.IBMPCCompatible)]
        [InlineData(CreatorCommand.Data, KnownSystem.IBMPCCompatible)]
        [InlineData(CreatorCommand.DigitalVideoDisc, KnownSystem.IBMPCCompatible)]
        [InlineData(CreatorCommand.Eject, null)]
        [InlineData(CreatorCommand.Floppy, KnownSystem.IBMPCCompatible)]
        [InlineData(CreatorCommand.GDROM, KnownSystem.SegaDreamcast)]
        [InlineData(CreatorCommand.MDS, null)]
        [InlineData(CreatorCommand.Reset, null)]
        [InlineData(CreatorCommand.SACD, KnownSystem.SuperAudioCD)]
        [InlineData(CreatorCommand.Start, null)]
        [InlineData(CreatorCommand.Stop, null)]
        [InlineData(CreatorCommand.Sub, null)]
        [InlineData(CreatorCommand.Swap, KnownSystem.SegaDreamcast)]
        [InlineData(CreatorCommand.XBOX, KnownSystem.MicrosoftXBOX)]
        public void BaseCommandToKnownSystemTest(CreatorCommand command, KnownSystem? expected)
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
            string actual = Converters.Extension(mediaType);
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
