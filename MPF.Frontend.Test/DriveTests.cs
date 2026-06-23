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

        #region EnumerateUnixOpticalBlockPaths

        [Fact]
        public void EnumerateUnixOpticalBlockPaths_NullOrEmpty_ReturnsEmpty()
        {
            Assert.Empty(Drive.EnumerateUnixOpticalBlockPaths(null!));
            Assert.Empty(Drive.EnumerateUnixOpticalBlockPaths(string.Empty));
        }

        [Fact]
        public void EnumerateUnixOpticalBlockPaths_MissingDirectory_ReturnsEmpty()
        {
            string missing = Path.Combine(Path.GetTempPath(), "mpf-test-missing-" + Guid.NewGuid().ToString("N"));
            Assert.Empty(Drive.EnumerateUnixOpticalBlockPaths(missing));
        }

        [Fact]
        public void EnumerateUnixOpticalBlockPaths_OnlyMatchesSrFollowedByDigits()
        {
            string root = Path.Combine(Path.GetTempPath(), "mpf-test-dev-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                File.WriteAllText(Path.Combine(root, "sr0"), string.Empty);
                File.WriteAllText(Path.Combine(root, "sr1"), string.Empty);
                File.WriteAllText(Path.Combine(root, "sr15"), string.Empty);
                File.WriteAllText(Path.Combine(root, "src"), string.Empty);
                File.WriteAllText(Path.Combine(root, "srm"), string.Empty);
                File.WriteAllText(Path.Combine(root, "serial0"), string.Empty);
                File.WriteAllText(Path.Combine(root, "sr"), string.Empty);
                File.WriteAllText(Path.Combine(root, "sr0a"), string.Empty);

                var actual = Drive.EnumerateUnixOpticalBlockPaths(root);

                var actualNames = new List<string>();
                foreach (var p in actual)
                {
                    actualNames.Add(Path.GetFileName(p));
                }
                actualNames.Sort(StringComparer.Ordinal);

                Assert.Equal(new[] { "sr0", "sr1", "sr15" }, actualNames);
            }
            finally
            {
                Directory.Delete(root, recursive: true);
            }
        }

        #endregion

        #region EnumerateUnixOpticalGenericPaths

        [Fact]
        public void EnumerateUnixOpticalGenericPaths_NullOrEmpty_ReturnsEmpty()
        {
            Assert.Empty(Drive.EnumerateUnixOpticalGenericPaths(null!, "/sys/class/scsi_generic"));
            Assert.Empty(Drive.EnumerateUnixOpticalGenericPaths(string.Empty, "/sys/class/scsi_generic"));
            Assert.Empty(Drive.EnumerateUnixOpticalGenericPaths("/dev", null!));
            Assert.Empty(Drive.EnumerateUnixOpticalGenericPaths("/dev", string.Empty));
        }

        [Fact]
        public void EnumerateUnixOpticalGenericPaths_MissingDirectory_ReturnsEmpty()
        {
            string missing = Path.Combine(Path.GetTempPath(), "mpf-test-missing-" + Guid.NewGuid().ToString("N"));
            Assert.Empty(Drive.EnumerateUnixOpticalGenericPaths("/dev", missing));
        }

        [Fact]
        public void EnumerateUnixOpticalGenericPaths_OnlyMatchesOpticalSgNodes()
        {
            string sysfs = Path.Combine(Path.GetTempPath(), "mpf-test-sysfs-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(sysfs);
            try
            {
                // Optical drives report SCSI peripheral device type 5
                WriteScsiGenericType(sysfs, "sg0", "5");   // optical, kept
                WriteScsiGenericType(sysfs, "sg2", "5\n");  // optical (trailing newline), kept
                WriteScsiGenericType(sysfs, "sg10", "5");  // optical (multi-digit), kept
                WriteScsiGenericType(sysfs, "sg1", "0");   // direct-access disk, skipped
                WriteScsiGenericType(sysfs, "sg3", "1");   // sequential tape, skipped
                WriteScsiGenericType(sysfs, "sga", "5");   // name not all-digits, skipped
                WriteScsiGenericType(sysfs, "sgX", "5");   // name not all-digits, skipped
                Directory.CreateDirectory(Path.Combine(sysfs, "sg4")); // no device/type, skipped

                var actual = Drive.EnumerateUnixOpticalGenericPaths("/dev", sysfs);

                var actualNames = new List<string>();
                foreach (var p in actual)
                {
                    actualNames.Add(Path.GetFileName(p));
                }
                actualNames.Sort(StringComparer.Ordinal);

                Assert.Equal(new[] { "sg0", "sg10", "sg2" }, actualNames);
                Assert.Contains(Path.Combine("/dev", "sg0"), actual);
            }
            finally
            {
                Directory.Delete(sysfs, recursive: true);
            }
        }

        /// <summary>
        /// Create a fake sysfs scsi_generic entry "<![CDATA[<root>/<name>/device/type]]>" with the given content.
        /// </summary>
        private static void WriteScsiGenericType(string root, string name, string type)
        {
            string deviceDir = Path.Combine(root, name, "device");
            Directory.CreateDirectory(deviceDir);
            File.WriteAllText(Path.Combine(deviceDir, "type"), type);
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
                    actualNames.Add(Path.GetFileName(device.DevicePath));
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

                var byPath = new Dictionary<string, Drive.UnixBlockDevice>();
                foreach (var device in actual)
                {
                    byPath[device.DevicePath] = device;
                }

                Drive.UnixBlockDevice sda = byPath[Path.Combine("/dev", "sda")];
                Assert.Equal(InternalDriveType.HardDisk, sda.DriveType);
                Assert.Equal(2048L * 512L, sda.TotalSize);

                Drive.UnixBlockDevice sdb = byPath[Path.Combine("/dev", "sdb")];
                Assert.Equal(InternalDriveType.Removable, sdb.DriveType);
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
                Assert.Equal(InternalDriveType.HardDisk, actual[0].DriveType);
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

        #region CompareDrivesForDisplay

        [Fact]
        public void CompareDrivesForDisplay_SrBeforeSg_WithNumericOrder()
        {
            var names = new List<string?>
            {
                "/dev/sg10", "/dev/sr2", "/dev/sg0", "/dev/sr10",
                "/dev/sr0", "/dev/sg1", "/dev/sr1", "/dev/sr15",
            };

            names.Sort(Drive.CompareDrivesForDisplay);

            Assert.Equal(new string?[]
            {
                "/dev/sr0", "/dev/sr1", "/dev/sr2", "/dev/sr10", "/dev/sr15",
                "/dev/sg0", "/dev/sg1", "/dev/sg10",
            }, names);
        }

        [Fact]
        public void CompareDrivesForDisplay_OpticalBeforeFixedAndRemovable()
        {
            var names = new List<string?>
            {
                "/dev/sda", "/dev/sr0", "/dev/nvme0n1", "/dev/sg0",
            };

            names.Sort(Drive.CompareDrivesForDisplay);

            Assert.Equal(new string?[]
            {
                "/dev/sr0", "/dev/sg0", "/dev/nvme0n1", "/dev/sda",
            }, names);
        }

        [Theory]
        [InlineData("sr2", "sr10", -1)]
        [InlineData("sr10", "sr2", 1)]
        [InlineData("sr2", "sr2", 0)]
        [InlineData("sr9", "sr10", -1)]
        [InlineData("sda", "sdb", -1)]
        public void NaturalCompare_TreatsDigitRunsAsNumbers(string a, string b, int expectedSign)
        {
            Assert.Equal(expectedSign, Math.Sign(Drive.NaturalCompare(a, b)));
        }

        #endregion
    }
}
