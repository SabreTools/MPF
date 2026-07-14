using MPF.ExecutionContexts.Dreamdump;
using SabreTools.RedumpLib.Data;
using Xunit;

namespace MPF.ExecutionContexts.Test
{
    public class DreamdumpTests
    {
        #region Default Values

        private static readonly BaseDumpSettings AllOptions = new DumpSettings()
        {
            RereadCount = 50,
            SectorOrder = SectorOrder.DATA_C2_SUB,
        };

        // None of these scenarios are actually supported as all are treated like GD-ROM
        [Theory]
        [MemberData(nameof(GenerateDefaultValueTestData))]
        public void DefaultValueTest(PhysicalSystem? system,
            PhysicalMediaType? type,
            string? drivePath,
            string filename,
            int? driveSpeed,
            string? expected)
        {
            var context = new ExecutionContext(system, type, drivePath, filename, driveSpeed, AllOptions);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Generate a test set for DefaultValueTest
        /// </summary>
        public static TheoryData<PhysicalSystem?, PhysicalMediaType?, string?, string, int?, string?> GenerateDefaultValueTestData()
        {
            return new TheoryData<PhysicalSystem?, PhysicalMediaType?, string?, string, int?, string?>
            {
                { null, null, null, "filename.bin", null, "disc --sector-order=DATA_C2_SUB --image-name=\"filename\" --retries=50" },
                { PhysicalSystem.IBMPCcompatible, PhysicalMediaType.CDROM, "/dev/sr0", "path/filename.bin", 2, "disc --drive=/dev/sr0 --sector-order=DATA_C2_SUB --speed=2 --image-name=\"filename\" --image-path=\"path\" --retries=50" },
                { PhysicalSystem.IBMPCcompatible, PhysicalMediaType.DVD, "/dev/sr0", "path/filename.bin", 2, "disc --drive=/dev/sr0 --sector-order=DATA_C2_SUB --speed=2 --image-name=\"filename\" --image-path=\"path\" --retries=50" },
                { PhysicalSystem.NintendoGameCube, PhysicalMediaType.NintendoGameCubeGameDisc, "/dev/sr0", "path/filename.bin", 2, "disc --drive=/dev/sr0 --sector-order=DATA_C2_SUB --speed=2 --image-name=\"filename\" --image-path=\"path\" --retries=50" },
                { PhysicalSystem.NintendoWii, PhysicalMediaType.NintendoWiiOpticalDisc, "/dev/sr0", "path/filename.bin", 2, "disc --drive=/dev/sr0 --sector-order=DATA_C2_SUB --speed=2 --image-name=\"filename\" --image-path=\"path\" --retries=50" },
                { PhysicalSystem.HDDVDVideo, PhysicalMediaType.HDDVD, "/dev/sr0", "path/filename.bin", 2, "disc --drive=/dev/sr0 --sector-order=DATA_C2_SUB --speed=2 --image-name=\"filename\" --image-path=\"path\" --retries=50" },
                { PhysicalSystem.BDVideo, PhysicalMediaType.BluRay, "/dev/sr0", "path/filename.bin", 2, "disc --drive=/dev/sr0 --sector-order=DATA_C2_SUB --speed=2 --image-name=\"filename\" --image-path=\"path\" --retries=50" },
                { PhysicalSystem.NintendoWiiU, PhysicalMediaType.NintendoWiiUOpticalDisc, "/dev/sr0", "path/filename.bin", 2, "disc --drive=/dev/sr0 --sector-order=DATA_C2_SUB --speed=2 --image-name=\"filename\" --image-path=\"path\" --retries=50" },
            };
        }

        #endregion

        #region Default

        [Theory]
        [InlineData("disc --drive=/dev/sr0 --sector-order=so --cutoff=1000 --read-offset=0 --speed=8 --image-name=image --image-path=path --force-qtoc --train --force-sector-order --read-at-once=21 --retries=35")]
        public void DiscTest(string parameters)
        {
            string? expected = "disc --drive=/dev/sr0 --sector-order=so --cutoff=1000 --read-offset=0 --speed=8 --image-name=\"image\" --image-path=\"path\" --force-qtoc --train --force-sector-order --read-at-once=21 --retries=35";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
            Assert.True(context.IsDumpingCommand());
        }

        [Theory]
        [InlineData("disc --image-name=\"image name.bin\" --image-path=\"directory name\"")]
        public void SpacesTest(string parameters)
        {
            string? expected = "disc --image-name=\"image name.bin\" --image-path=\"directory name\"";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
            Assert.True(context.IsDumpingCommand());
        }

        #endregion
    }
}
