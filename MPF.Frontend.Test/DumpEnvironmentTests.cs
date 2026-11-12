using SabreTools.RedumpLib.Data;
using Xunit;

namespace MPF.Frontend.Test
{
    public class DumpEnvironmentTests
    {
        [Theory]
        [MemberData(nameof(GenerateParametersValidSegmentedTestData))]
        public void ParametersValidSegmentedTest(string? parameters, char letter, bool isFloppy, PhysicalMediaType? mediaType, bool expected)
        {
            var options = new Options();
            options.InternalProgram = InternalProgram.DiscImageCreator;

            // TODO: This relies on creating real objects for the drive. Can we mock this out instead?
            var drive = isFloppy
                ? Drive.Create(InternalDriveType.Floppy, letter.ToString())
                : Drive.Create(InternalDriveType.Optical, letter.ToString());

            var env = new DumpEnvironment(options,
                string.Empty,
                drive,
                PhysicalSystem.IBMPCcompatible,
                null);
            env.SetExecutionContext(mediaType, parameters);
            env.SetProcessor();

            bool actual = env.ParametersValid(mediaType);
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Generate a test set for ExtensionTest
        /// </summary>
        public static TheoryData<string?, char, bool, PhysicalMediaType?, bool> GenerateParametersValidSegmentedTestData()
        {
            return new TheoryData<string?, char, bool, PhysicalMediaType?, bool>
            {
                { null, 'D', false, null, false },
                { "", 'D', false, null, false },
                { "cd F test.bin 8 /c2 20", 'F', false, PhysicalMediaType.CDROM, true },
                { "cd F test.bin 8 /c2new 20", 'F', false, PhysicalMediaType.CDROM, true },
                { "fd A test.img", 'A', true, PhysicalMediaType.FloppyDisk, true },
                { "dvd X test.iso 8 /raw", 'X', false, PhysicalMediaType.FloppyDisk, false },
                { "stop D", 'D', false, PhysicalMediaType.DVD, true },
            };
        }
    }
}
