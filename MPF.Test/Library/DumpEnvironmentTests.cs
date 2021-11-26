using System.IO;
using MPF.Core.Data;
using MPF.Library;
using RedumpLib.Data;
using Xunit;

namespace MPF.Test.Library
{
    public class DumpEnvironmentTests
    {
        [Theory]
        [InlineData(null, 'D', false, MediaType.NONE, false)]
        [InlineData("", 'D', false, MediaType.NONE, false)]
        [InlineData("cd F test.bin 8 /c2 20", 'F', false, MediaType.CDROM, true)]
        [InlineData("fd A test.img", 'A', true, MediaType.FloppyDisk, true)]
        [InlineData("dvd X test.iso 8 /raw", 'X', false, MediaType.FloppyDisk, false)]
        [InlineData("stop D", 'D', false, MediaType.DVD, true)]
        public void ParametersValidTest(string parameters, char letter, bool isFloppy, MediaType? mediaType, bool expected)
        {
            var options = new Options() { InternalProgram = InternalProgram.DiscImageCreator };
            var drive = isFloppy
                ? new Drive(InternalDriveType.Floppy, new DriveInfo(letter.ToString()))
                : new Drive(InternalDriveType.Optical, new DriveInfo(letter.ToString()));

            var env = new DumpEnvironment(options, string.Empty, string.Empty, drive, RedumpSystem.IBMPCcompatible, mediaType, parameters);

            bool actual = env.ParametersValid();
            Assert.Equal(expected, actual);
        }
    }
}
