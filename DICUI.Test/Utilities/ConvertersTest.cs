using System;
using System.Collections.Generic;
using DICUI.Data;
using DICUI.Utilities;
using Xunit;

namespace DICUI.Test.Utilities
{
    public class ConvertersTest
    {
        [Theory]
        [InlineData(DICCommands.Audio, MediaType.CD)]
        [InlineData(DICCommands.BluRay, MediaType.BluRay)]
        [InlineData(DICCommands.Close, null)]
        [InlineData(DICCommands.CompactDisc, MediaType.CD)]
        [InlineData(DICCommands.Data, MediaType.CD)]
        [InlineData(DICCommands.DigitalVideoDisc, MediaType.DVD)]
        [InlineData(DICCommands.Eject, null)]
        [InlineData(DICCommands.Floppy, MediaType.Floppy)]
        [InlineData(DICCommands.GDROM, MediaType.GDROM)]
        [InlineData(DICCommands.MDS, null)]
        [InlineData(DICCommands.Reset, null)]
        [InlineData(DICCommands.Start, null)]
        [InlineData(DICCommands.Stop, null)]
        [InlineData(DICCommands.Sub, null)]
        [InlineData(DICCommands.Swap, MediaType.GDROM)]
        [InlineData(DICCommands.XBOX, MediaType.DVD)]
        public void BaseCommandToMediaTypeTest(string command, MediaType? expected)
        {
            MediaType? actual = Converters.BaseCommmandToMediaType(command);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(DICCommands.Audio, KnownSystem.AudioCD)]
        [InlineData(DICCommands.BluRay, KnownSystem.SonyPlayStation3)]
        [InlineData(DICCommands.Close, null)]
        [InlineData(DICCommands.CompactDisc, KnownSystem.IBMPCCompatible)]
        [InlineData(DICCommands.Data, KnownSystem.IBMPCCompatible)]
        [InlineData(DICCommands.DigitalVideoDisc, KnownSystem.IBMPCCompatible)]
        [InlineData(DICCommands.Eject, null)]
        [InlineData(DICCommands.Floppy, KnownSystem.IBMPCCompatible)]
        [InlineData(DICCommands.GDROM, KnownSystem.SegaDreamcast)]
        [InlineData(DICCommands.MDS, null)]
        [InlineData(DICCommands.Reset, null)]
        [InlineData(DICCommands.Start, null)]
        [InlineData(DICCommands.Stop, null)]
        [InlineData(DICCommands.Sub, null)]
        [InlineData(DICCommands.Swap, KnownSystem.SegaDreamcast)]
        [InlineData(DICCommands.XBOX, KnownSystem.MicrosoftXBOX)]
        public void BaseCommandToKnownSystemTest(string command, KnownSystem? expected)
        {
            KnownSystem? actual = Converters.BaseCommandToKnownSystem(command);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(MediaType.CD, ".bin")]
        [InlineData(MediaType.DVD, ".iso")]
        [InlineData(MediaType.LaserDisc, ".raw")]
        [InlineData(MediaType.WiiUOpticalDisc, ".wud")]
        [InlineData(MediaType.Floppy, ".img")]
        [InlineData(MediaType.Cassette, ".wav")]
        [InlineData(MediaType.NONE, null)]
        public void MediaTypeToExtensionTest(MediaType? mediaType, string expected)
        {
            string actual = Converters.MediaTypeToExtension(mediaType);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(MediaType.CD, "CD-ROM")]
        [InlineData(MediaType.LaserDisc, "LaserDisc")]
        [InlineData(MediaType.NONE, "Unknown")]
        public void MediaTypeToStringTest(MediaType? mediaType, string expected)
        {
            string actual = Converters.MediaTypeToString(mediaType);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(KnownSystem.MicrosoftXBOX, MediaType.CD, DICCommands.XBOX)]
        [InlineData(KnownSystem.MicrosoftXBOX, MediaType.DVD, DICCommands.XBOX)]
        [InlineData(KnownSystem.MicrosoftXBOX, MediaType.LaserDisc, null)]
        [InlineData(KnownSystem.SegaNu, MediaType.BluRay, DICCommands.BluRay)]
        [InlineData(KnownSystem.AppleMacintosh, MediaType.Floppy, DICCommands.Floppy)]
        [InlineData(KnownSystem.RawThrillsVarious, MediaType.GDROM, null)]
        public void KnownSystemAndMediaTypeToBaseCommandTest(KnownSystem? knownSystem, MediaType? mediaType, string expected)
        {
            string actual = Converters.KnownSystemAndMediaTypeToBaseCommand(knownSystem, mediaType);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(KnownSystem.AppleMacintosh, MediaType.LaserDisc, true, 20, null)]
        [InlineData(KnownSystem.NintendoGameCube, MediaType.GameCubeGameDisc, false, 20, new string[] { DICFlags.Raw })]
        [InlineData(KnownSystem.IBMPCCompatible, MediaType.DVD, false, 20, new string[] { })]
        /* paranoid mode tests */
        [InlineData(KnownSystem.IBMPCCompatible, MediaType.CD, true, 1000, new string[] { DICFlags.C2Opcode, "1000", DICFlags.NoFixSubQSecuROM, DICFlags.ScanFileProtect, DICFlags.ScanSectorProtect, DICFlags.SubchannelReadLevel, "2"})]
        [InlineData(KnownSystem.AppleMacintosh, MediaType.CD, false, 20, new string[] { DICFlags.C2Opcode, "20", DICFlags.NoFixSubQSecuROM, DICFlags.ScanFileProtect })]
        [InlineData(KnownSystem.IBMPCCompatible, MediaType.DVD, true, 500, new string[] { DICFlags.CMI })]
        [InlineData(KnownSystem.HDDVDVideo, MediaType.HDDVD, true, 500, new string[] { DICFlags.CMI })]
        [InlineData(KnownSystem.IBMPCCompatible, MediaType.DVD, false, 500, new string[] { })]
        [InlineData(KnownSystem.HDDVDVideo, MediaType.HDDVD, false, 500, new string[] { })]
        /* reread c2 */
        [InlineData(KnownSystem.SegaDreamcast, MediaType.GDROM, false, 1000, new string[] { DICFlags.C2Opcode, "1000", })]
        [InlineData(KnownSystem.SegaDreamcast, MediaType.GDROM, false, -1, new string[] { DICFlags.C2Opcode })]

        public void KnownSystemAndMediaTypeToParametersTest(KnownSystem? knownSystem, MediaType? mediaType, bool paranoid, int rereadC2, string[] expected)
        {
            List<string> actual = Converters.KnownSystemAndMediaTypeToParameters(knownSystem, mediaType, paranoid, rereadC2);

            HashSet<string> expectedSet = expected != null ? new HashSet<string>(expected) : null;
            HashSet<string> actualSet = actual != null ? new HashSet<string>(actual) : null;

            Assert.Equal(expectedSet, actualSet);
        }

        [Theory]
        [InlineData(KnownSystem.MicrosoftXBOX, "Microsoft XBOX")]
        [InlineData(KnownSystem.NECPC88, "NEC PC-88")]
        [InlineData(KnownSystem.KonamiPython, "Konami Python")]
        [InlineData(KnownSystem.HDDVDVideo, "HD-DVD-Video")]
        [InlineData(KnownSystem.Custom, "Custom Input")]
        [InlineData(KnownSystem.NONE, "Unknown")]
        public void KnownSystemToStringTest(KnownSystem? knownSystem, string expected)
        {
            string actual = Converters.KnownSystemToString(knownSystem);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void KnownSystemHasValidCategory()
        {
            var values = Validators.CreateListOfSystems();
            KnownSystem[] markers = { KnownSystem.MarkerArcadeEnd, KnownSystem.MarkerConsoleEnd, KnownSystem.MarkerComputerEnd, KnownSystem.MarkerOtherEnd };

            values.ForEach(system => {
                if (system == KnownSystem.NONE || system == KnownSystem.Custom)
                    return;

                // we check that the category is the first category value higher than the system
                KnownSystemCategory category = ((KnownSystem?)system).Category();
                KnownSystem marker = KnownSystem.NONE;

                switch (category)
                {
                    case KnownSystemCategory.Arcade: marker = KnownSystem.MarkerArcadeEnd; break;
                    case KnownSystemCategory.Console: marker = KnownSystem.MarkerConsoleEnd; break;
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
