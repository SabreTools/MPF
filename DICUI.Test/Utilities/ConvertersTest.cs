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
        [InlineData(KnownSystem.AppleMacintosh, MediaType.CD, new string[] { DICFlags.C2Opcode, "20", DICFlags.NoFixSubQSecuROM, DICFlags.ScanFileProtect })]
        [InlineData(KnownSystem.AppleMacintosh, MediaType.LaserDisc, null)]
        [InlineData(KnownSystem.NintendoGameCube, MediaType.GameCubeGameDisc, new string[] { DICFlags.Raw })]
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
    }
}
