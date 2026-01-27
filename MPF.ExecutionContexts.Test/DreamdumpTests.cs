using System.Collections.Generic;
using MPF.ExecutionContexts.Dreamdump;
using SabreTools.RedumpLib.Data;
using Xunit;

namespace MPF.ExecutionContexts.Test
{
    public class DreamdumpTests
    {
        #region Default Values

        private static readonly Dictionary<string, string?> AllOptions = new()
        {
            [SettingConstants.RereadCount] = "1000",
            [SettingConstants.SectorOrder] = "DATA_C2_SUB",
        };

        // None of these scenarios are actually supported as all are treated like GD-ROM
        [Theory]
        [InlineData(null, null, null, "filename.bin", null, "--retries=20 --image-name=\"filename\" --sector-order=DATA_C2_SUB")]
        [InlineData(RedumpSystem.IBMPCcompatible, MediaType.CDROM, "/dev/sr0", "path/filename.bin", 2, "--retries=20 --image-name=\"filename\" --image-path=\"path\" --speed=2 --sector-order=DATA_C2_SUB --drive=/dev/sr0")]
        [InlineData(RedumpSystem.IBMPCcompatible, MediaType.DVD, "/dev/sr0", "path/filename.bin", 2, "--retries=20 --image-name=\"filename\" --image-path=\"path\" --speed=2 --sector-order=DATA_C2_SUB --drive=/dev/sr0")]
        [InlineData(RedumpSystem.NintendoGameCube, MediaType.NintendoGameCubeGameDisc, "/dev/sr0", "path/filename.bin", 2, "--retries=20 --image-name=\"filename\" --image-path=\"path\" --speed=2 --sector-order=DATA_C2_SUB --drive=/dev/sr0")]
        [InlineData(RedumpSystem.NintendoWii, MediaType.NintendoWiiOpticalDisc, "/dev/sr0", "path/filename.bin", 2, "--retries=20 --image-name=\"filename\" --image-path=\"path\" --speed=2 --sector-order=DATA_C2_SUB --drive=/dev/sr0")]
        [InlineData(RedumpSystem.HDDVDVideo, MediaType.HDDVD, "/dev/sr0", "path/filename.bin", 2, "--retries=20 --image-name=\"filename\" --image-path=\"path\" --speed=2 --sector-order=DATA_C2_SUB --drive=/dev/sr0")]
        [InlineData(RedumpSystem.BDVideo, MediaType.BluRay, "/dev/sr0", "path/filename.bin", 2, "--retries=20 --image-name=\"filename\" --image-path=\"path\" --speed=2 --sector-order=DATA_C2_SUB --drive=/dev/sr0")]
        [InlineData(RedumpSystem.NintendoWiiU, MediaType.NintendoWiiUOpticalDisc, "/dev/sr0", "path/filename.bin", 2, "--retries=20 --image-name=\"filename\" --image-path=\"path\" --speed=2 --sector-order=DATA_C2_SUB --drive=/dev/sr0")]
        public void DefaultValueTest(RedumpSystem? system,
            MediaType? type,
            string? drivePath,
            string filename,
            int? driveSpeed,
            string? expected)
        {
            var context = new ExecutionContext(system, type, drivePath, filename, driveSpeed, AllOptions);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        #endregion

        #region Default

        [Theory]
        [InlineData("--force-qtoc --train --retries=20 --image-name=image --image-path=path --read-offset=0 --read-at-once=0 --speed=8 --sector-order=so --drive=/dev/sr0")]
        public void DiscTest(string parameters)
        {
            string? expected = "--force-qtoc --train --retries=20 --image-name=\"image\" --image-path=\"path\" --read-offset=0 --read-at-once=0 --speed=8 --sector-order=so --drive=/dev/sr0";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
            Assert.True(context.IsDumpingCommand());
        }

        [Theory]
        [InlineData("--image-name=\"image name.bin\" --image-path=\"directory name\"")]
        public void SpacesTest(string parameters)
        {
            string? expected = "--image-name=\"image name.bin\" --image-path=\"directory name\"";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
            Assert.True(context.IsDumpingCommand());
        }

        #endregion
    }
}
