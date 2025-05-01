using SabreTools.RedumpLib.Data;
using Xunit;

namespace MPF.Frontend.Test
{
    public class DumpEnvironmentTests
    {
        [Theory]
        [InlineData(null, 'D', false, MediaType.NONE, false)]
        [InlineData("", 'D', false, MediaType.NONE, false)]
        [InlineData("cd F test.bin 8 /c2 20", 'F', false, MediaType.CDROM, true)]
        [InlineData("cd F test.bin 8 /c2new 20", 'F', false, MediaType.CDROM, true)]
        [InlineData("fd A test.img", 'A', true, MediaType.FloppyDisk, true)]
        [InlineData("dvd X test.iso 8 /raw", 'X', false, MediaType.FloppyDisk, false)]
        [InlineData("stop D", 'D', false, MediaType.DVD, true)]
        public void ParametersValidTest(string? parameters, char letter, bool isFloppy, MediaType? mediaType, bool expected)
        {
            var options = new Options() { InternalProgram = InternalProgram.DiscImageCreator };

            // TODO: This relies on creating real objects for the drive. Can we mock this out instead?
            var drive = isFloppy
                ? Drive.Create(InternalDriveType.Floppy, letter.ToString())
                : Drive.Create(InternalDriveType.Optical, letter.ToString());

            var env = new DumpEnvironment(options, string.Empty, drive, RedumpSystem.IBMPCcompatible, mediaType, null, parameters);

            bool actual = env.ParametersValid();
            Assert.Equal(expected, actual);
        }
    }
}
