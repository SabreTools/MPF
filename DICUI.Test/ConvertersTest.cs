using DICUI.Data;
using DICUI.Utilities;
using Xunit;

namespace DICUI.Test
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
    }
}
