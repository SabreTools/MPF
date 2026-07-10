using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace MPF.Frontend.Test
{
    public class DriveTests
    {
        #region ToInternalDriveType

        /// <summary>
        /// DiscType values that map to InternalDriveType
        /// </summary>
        private static readonly DriveType[] _mappableDriveTypes =
        [
            DriveType.CDRom,
            DriveType.Fixed,
            DriveType.Removable,
        ];

        /// <summary>
        /// Check that every supported DriveType maps to an InternalDriveType
        /// </summary>
        /// <param name="driveType">DriveType value to check</param>
        /// <param name="expectNull">True to expect a null mapping, false otherwise</param>
        [Theory]
        [MemberData(nameof(GenerateDriveTypeMappingTestData))]
        public void ToInternalDriveTypeTest(DriveType driveType, bool expectNull)
        {
            var actual = Drive.ToInternalDriveType(driveType);

            if (expectNull)
                Assert.Null(actual);
            else
                Assert.NotNull(actual);
        }

        /// <summary>
        /// Generate a test set of DriveType values
        /// </summary>
        /// <returns>MemberData-compatible list of DriveType values</returns>
        public static List<object?[]> GenerateDriveTypeMappingTestData()
        {
            var testData = new List<object?[]>() { new object?[] { null, true } };
            foreach (DriveType driveType in Enum.GetValues<DriveType>())
            {
                if (Array.IndexOf(_mappableDriveTypes, driveType) > -1)
                    testData.Add([driveType, false]);
                else
                    testData.Add([driveType, true]);
            }

            return testData;
        }

        #endregion

        #region EnumerateUnixFixedDevices

        [Fact]
        public void EnumerateUnixFixedDevices_NullOrEmpty_ReturnsEmpty()
        {
            Assert.Empty(Drive.EnumerateUnixFixedDevices(null!, "/dev"));
            Assert.Empty(Drive.EnumerateUnixFixedDevices(string.Empty, "/dev"));
            Assert.Empty(Drive.EnumerateUnixFixedDevices("/sys/block", null!));
            Assert.Empty(Drive.EnumerateUnixFixedDevices("/sys/block", string.Empty));
        }

        [Fact]
        public void EnumerateUnixFixedDevices_MissingDirectory_ReturnsEmpty()
        {
            string missing = Path.Combine(Path.GetTempPath(), "mpf-test-missing-" + Guid.NewGuid().ToString("N"));
            Assert.Empty(Drive.EnumerateUnixFixedDevices(missing, "/dev"));
        }

        [Fact]
        public void EnumerateUnixFixedDevices_SkipsVirtualDevices()
        {
            string sysfs = Path.Combine(Path.GetTempPath(), "mpf-test-sysblock-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(sysfs);
            try
            {
                // Real dump targets
                WriteSysBlockDevice(sysfs, "sda", removable: "0", sizeSectors: "2048");
                WriteSysBlockDevice(sysfs, "nvme0n1", removable: "0", sizeSectors: "4096");
                WriteSysBlockDevice(sysfs, "sdb", removable: "1", sizeSectors: "100");

                // Virtual or non-target devices, all skipped
                WriteSysBlockDevice(sysfs, "loop0", removable: "0", sizeSectors: "8");
                WriteSysBlockDevice(sysfs, "ram0", removable: "0", sizeSectors: "8");
                WriteSysBlockDevice(sysfs, "zram0", removable: "0", sizeSectors: "8");
                WriteSysBlockDevice(sysfs, "dm-0", removable: "0", sizeSectors: "8");
                WriteSysBlockDevice(sysfs, "md0", removable: "0", sizeSectors: "8");
                WriteSysBlockDevice(sysfs, "sr0", removable: "1", sizeSectors: "8"); // optical, surfaced elsewhere
                WriteSysBlockDevice(sysfs, "fd0", removable: "1", sizeSectors: "8");

                var actual = Drive.EnumerateUnixFixedDevices(sysfs, "/dev");

                var actualNames = new List<string>();
                foreach (var device in actual)
                {
                    actualNames.Add(Path.GetFileName(device.DevicePath!));
                }

                actualNames.Sort(StringComparer.Ordinal);

                Assert.Equal(new[] { "nvme0n1", "sda", "sdb" }, actualNames);
            }
            finally
            {
                Directory.Delete(sysfs, recursive: true);
            }
        }

        [Fact]
        public void EnumerateUnixFixedDevices_ClassifiesRemovableAndSize()
        {
            string sysfs = Path.Combine(Path.GetTempPath(), "mpf-test-sysblock-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(sysfs);
            try
            {
                WriteSysBlockDevice(sysfs, "sda", removable: "0", sizeSectors: "2048");   // fixed hard disk
                WriteSysBlockDevice(sysfs, "sdb", removable: "1\n", sizeSectors: "100\n"); // removable (trailing newline)

                var actual = Drive.EnumerateUnixFixedDevices(sysfs, "/dev");

                var byPath = new Dictionary<string, Drive>();
                foreach (var device in actual)
                {
                    byPath[device.DevicePath!] = device;
                }

                Drive sda = byPath[Path.Combine("/dev", "sda")];
                Assert.Equal(InternalDriveType.HardDisk, sda.InternalDriveType);
                Assert.Equal(2048L * 512L, sda.TotalSize);

                Drive sdb = byPath[Path.Combine("/dev", "sdb")];
                Assert.Equal(InternalDriveType.Removable, sdb.InternalDriveType);
                Assert.Equal(100L * 512L, sdb.TotalSize);
            }
            finally
            {
                Directory.Delete(sysfs, recursive: true);
            }
        }

        [Fact]
        public void EnumerateUnixFixedDevices_MissingMetadata_DefaultsToFixedWithUnknownSize()
        {
            string sysfs = Path.Combine(Path.GetTempPath(), "mpf-test-sysblock-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(sysfs);
            try
            {
                // A bare device directory with neither "removable" nor "size"
                Directory.CreateDirectory(Path.Combine(sysfs, "sda"));

                var actual = Drive.EnumerateUnixFixedDevices(sysfs, "/dev");

                Assert.Single(actual);
                Assert.Equal(Path.Combine("/dev", "sda"), actual[0].DevicePath);
                Assert.Equal(InternalDriveType.HardDisk, actual[0].InternalDriveType);
                Assert.Equal(0L, actual[0].TotalSize);
            }
            finally
            {
                Directory.Delete(sysfs, recursive: true);
            }
        }

        /// <summary>
        /// Create a fake sysfs block-device entry "&lt;root&gt;/&lt;name&gt;" with "removable" and "size" files.
        /// </summary>
        private static void WriteSysBlockDevice(string root, string name, string removable, string sizeSectors)
        {
            string deviceDir = Path.Combine(root, name);
            Directory.CreateDirectory(deviceDir);
            File.WriteAllText(Path.Combine(deviceDir, "removable"), removable);
            File.WriteAllText(Path.Combine(deviceDir, "size"), sizeSectors);
        }

        #endregion
    }
}
