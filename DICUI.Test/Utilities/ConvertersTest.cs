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
        [InlineData(DICCommandStrings.Audio, MediaType.CD)]
        [InlineData(DICCommandStrings.BluRay, MediaType.BluRay)]
        [InlineData(DICCommandStrings.Close, null)]
        [InlineData(DICCommandStrings.CompactDisc, MediaType.CD)]
        [InlineData(DICCommandStrings.Data, MediaType.CD)]
        [InlineData(DICCommandStrings.DigitalVideoDisc, MediaType.DVD)]
        [InlineData(DICCommandStrings.Eject, null)]
        [InlineData(DICCommandStrings.Floppy, MediaType.Floppy)]
        [InlineData(DICCommandStrings.GDROM, MediaType.GDROM)]
        [InlineData(DICCommandStrings.MDS, null)]
        [InlineData(DICCommandStrings.Reset, null)]
        [InlineData(DICCommandStrings.Start, null)]
        [InlineData(DICCommandStrings.Stop, null)]
        [InlineData(DICCommandStrings.Sub, null)]
        [InlineData(DICCommandStrings.Swap, MediaType.GDROM)]
        [InlineData(DICCommandStrings.XBOX, MediaType.DVD)]
        public void BaseCommandToMediaTypeTest(string command, MediaType? expected)
        {
            MediaType? actual = Converters.BaseCommmandToMediaType(command);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(DICCommandStrings.Audio, KnownSystem.AudioCD)]
        [InlineData(DICCommandStrings.BluRay, KnownSystem.SonyPlayStation3)]
        [InlineData(DICCommandStrings.Close, null)]
        [InlineData(DICCommandStrings.CompactDisc, KnownSystem.IBMPCCompatible)]
        [InlineData(DICCommandStrings.Data, KnownSystem.IBMPCCompatible)]
        [InlineData(DICCommandStrings.DigitalVideoDisc, KnownSystem.IBMPCCompatible)]
        [InlineData(DICCommandStrings.Eject, null)]
        [InlineData(DICCommandStrings.Floppy, KnownSystem.IBMPCCompatible)]
        [InlineData(DICCommandStrings.GDROM, KnownSystem.SegaDreamcast)]
        [InlineData(DICCommandStrings.MDS, null)]
        [InlineData(DICCommandStrings.Reset, null)]
        [InlineData(DICCommandStrings.Start, null)]
        [InlineData(DICCommandStrings.Stop, null)]
        [InlineData(DICCommandStrings.Sub, null)]
        [InlineData(DICCommandStrings.Swap, KnownSystem.SegaDreamcast)]
        [InlineData(DICCommandStrings.XBOX, KnownSystem.MicrosoftXBOX)]
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
        [InlineData(KnownSystem.MicrosoftXBOX, MediaType.CD, DICCommandStrings.XBOX)]
        [InlineData(KnownSystem.MicrosoftXBOX, MediaType.DVD, DICCommandStrings.XBOX)]
        [InlineData(KnownSystem.MicrosoftXBOX, MediaType.LaserDisc, null)]
        [InlineData(KnownSystem.SegaNu, MediaType.BluRay, DICCommandStrings.BluRay)]
        [InlineData(KnownSystem.AppleMacintosh, MediaType.Floppy, DICCommandStrings.Floppy)]
        [InlineData(KnownSystem.RawThrillsVarious, MediaType.GDROM, null)]
        public void KnownSystemAndMediaTypeToBaseCommandTest(KnownSystem? knownSystem, MediaType? mediaType, string expected)
        {
            string actual = Converters.KnownSystemAndMediaTypeToBaseCommand(knownSystem, mediaType);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(KnownSystem.AppleMacintosh, MediaType.CD, new string[] { DICFlagStrings.C2Opcode, "20", DICFlagStrings.NoFixSubQSecuROM, DICFlagStrings.ScanFileProtect })]
        [InlineData(KnownSystem.AppleMacintosh, MediaType.LaserDisc, null)]
        [InlineData(KnownSystem.NintendoGameCube, MediaType.GameCubeGameDisc, new string[] { DICFlagStrings.Raw })]
        [InlineData(KnownSystem.IBMPCCompatible, MediaType.DVD, new string[] { })]
        public void KnownSystemAndMediaTypeToParametersTest(KnownSystem? knownSystem, MediaType? mediaType, string[] expected)
        {
            List<string> actual = Converters.KnownSystemAndMediaTypeToParameters(knownSystem, mediaType);

            if (expected == null)
                Assert.Null(actual);

            else
                foreach (string param in expected)
                    Assert.Contains(param, actual);
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
