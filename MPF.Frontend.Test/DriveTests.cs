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

        #region EnumerateMacOSWholeDiskNodes

        [Fact]
        public void EnumerateMacOSWholeDiskNodes_NullOrEmpty_ReturnsEmpty()
        {
            Assert.Empty(Drive.EnumerateMacOSWholeDiskNodes(null!));
            Assert.Empty(Drive.EnumerateMacOSWholeDiskNodes(string.Empty));
        }

        [Fact]
        public void EnumerateMacOSWholeDiskNodes_MissingDirectory_ReturnsEmpty()
        {
            string missing = Path.Combine(Path.GetTempPath(), "mpf-test-missing-" + Guid.NewGuid().ToString("N"));
            Assert.Empty(Drive.EnumerateMacOSWholeDiskNodes(missing));
        }

        [Fact]
        public void EnumerateMacOSWholeDiskNodes_OnlyMatchesDiskFollowedByDigits()
        {
            string root = Path.Combine(Path.GetTempPath(), "mpf-test-dev-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);
            try
            {
                File.WriteAllText(Path.Combine(root, "disk0"), string.Empty);
                File.WriteAllText(Path.Combine(root, "disk1"), string.Empty);
                File.WriteAllText(Path.Combine(root, "disk10"), string.Empty);
                File.WriteAllText(Path.Combine(root, "disk0s1"), string.Empty); // partition, skipped
                File.WriteAllText(Path.Combine(root, "disk"), string.Empty);    // no trailing digits, skipped
                File.WriteAllText(Path.Combine(root, "diska"), string.Empty);   // name not all-digits, skipped
                File.WriteAllText(Path.Combine(root, "rdisk0"), string.Empty);  // raw character node, skipped

                var actual = Drive.EnumerateMacOSWholeDiskNodes(root);

                var actualNames = new List<string>();
                foreach (var p in actual)
                {
                    actualNames.Add(Path.GetFileName(p));
                }
                actualNames.Sort(StringComparer.Ordinal);

                Assert.Equal(new[] { "disk0", "disk1", "disk10" }, actualNames);
            }
            finally
            {
                Directory.Delete(root, recursive: true);
            }
        }

        #endregion

        #region ParseMacOSDiskutilInfo

        [Fact]
        public void ParseMacOSDiskutilInfo_NullOrEmpty_ReturnsNull()
        {
            Assert.Null(Drive.ParseMacOSDiskutilInfo(null!, "/dev/disk0"));
            Assert.Null(Drive.ParseMacOSDiskutilInfo(string.Empty, "/dev/disk0"));
        }

        [Fact]
        public void ParseMacOSDiskutilInfo_FixedSataDisk_ClassifiesHardDisk()
        {
            string info =
                """
                   Device Identifier:         disk0
                   Device Node:               /dev/disk0
                   Whole:                     Yes
                   Device / Media Name:       QEMU HARDDISK
                   Protocol:                  SATA
                   Disk Size:                 402.7 MB (402653184 Bytes) (exactly 786432 512-Byte-Units)
                   Device Location:           Internal
                   Removable Media:           Fixed
                   Virtual:                   No
                """;

            var actual = Drive.ParseMacOSDiskutilInfo(info, "/dev/disk0");

            Assert.NotNull(actual);
            Assert.Equal("/dev/disk0", actual!.DevicePath);
            Assert.Equal(InternalDriveType.HardDisk, actual.DriveType);
            Assert.Equal(402653184L, actual.TotalSize);
        }

        [Fact]
        public void ParseMacOSDiskutilInfo_SynthesizedOrDiskImage_ReturnsNull()
        {
            string virtualDisk =
                """
                   Device Node:               /dev/disk3
                   Device / Media Name:       Disk Image
                   Protocol:                  Disk Image
                   Disk Size:                 4.2 MB (4194304 Bytes) (exactly 8192 512-Byte-Units)
                   Removable Media:           Removable
                   Virtual:                   Yes
                """;

            Assert.Null(Drive.ParseMacOSDiskutilInfo(virtualDisk, "/dev/disk3"));
        }

        [Fact]
        public void ParseMacOSDiskutilInfo_RemovableFlashDrive_ClassifiesRemovable()
        {
            string info =
                """
                   Device Node:               /dev/disk5
                   Device / Media Name:       SanDisk Ultra
                   Protocol:                  USB
                   Disk Size:                 15.5 GB (15518924800 Bytes) (exactly 30310400 512-Byte-Units)
                   Device Location:           External
                   Removable Media:           Removable
                   Virtual:                   No
                """;

            var actual = Drive.ParseMacOSDiskutilInfo(info, "/dev/disk5");

            Assert.NotNull(actual);
            Assert.Equal(InternalDriveType.Removable, actual!.DriveType);
            Assert.Equal(15518924800L, actual.TotalSize);
        }

        [Fact]
        public void ParseMacOSDiskutilInfo_RemovableFloppySizedDisk_ClassifiesFloppy()
        {
            string info =
                """
                   Device Node:               /dev/disk5
                   Device / Media Name:       NEC USB UF000x
                   Protocol:                  USB
                   Disk Size:                 1.4 MB (1474560 Bytes) (exactly 2880 512-Byte-Units)
                   Device Location:           External
                   Removable Media:           Removable
                   Virtual:                   No
                """;

            var actual = Drive.ParseMacOSDiskutilInfo(info, "/dev/disk5");

            Assert.NotNull(actual);
            Assert.Equal(InternalDriveType.Floppy, actual!.DriveType);
            Assert.Equal(1474560L, actual.TotalSize);
        }

        [Fact]
        public void ParseMacOSDiskutilInfo_OpticalMedia_ClassifiesOptical()
        {
            string info =
                """
                   Device Node:               /dev/disk6
                   Device / Media Name:       PLEXTOR DVDR
                   Protocol:                  ATAPI
                   Optical Media Type:        CD-ROM
                   Disk Size:                 650.0 MB (681574400 Bytes) (exactly 1331200 512-Byte-Units)
                   Removable Media:           Removable
                   Virtual:                   No
                """;

            var actual = Drive.ParseMacOSDiskutilInfo(info, "/dev/disk6");

            Assert.NotNull(actual);
            Assert.Equal(InternalDriveType.Optical, actual!.DriveType);
        }

        /// <remarks>
        /// Sample captured verbatim from a real optical drive. It contradicts the two obvious
        /// assumptions about how such a drive reports itself: it is reached over "SATA" rather
        /// than "ATAPI" because it sits behind an AHCI controller, and it lists the optical-only
        /// fields with no value at all. Classification therefore keys off the presence of those
        /// fields and never their contents. The empty values are written here as bare keys, so
        /// that stripping trailing whitespace cannot quietly turn this case into a different one.
        /// </remarks>
        [Fact]
        public void ParseMacOSDiskutilInfo_OpticalFieldsPresentButValueless_ClassifiesOptical()
        {
            string info =
                """
                   Device Identifier:         disk1
                   Device Node:               /dev/disk1
                   Whole:                     Yes
                   Device / Media Name:       QEMU DVD-ROM
                   Content (IOContent):       CD_partition_scheme
                   Media Type:
                   Protocol:                  SATA
                   Disk Size:                 10.1 MB (10061856 Bytes) (19652 512-Byte-Units + 32 Byte(s))
                   Device Block Size:         2352 Bytes
                   Media Read-Only:           Yes
                   Removable Media:           Removable
                   Virtual:                   No
                   Optical Drive Type:        Info not available
                   Optical Media Type:
                   Optical Media Erasable:    No
                """;

            var actual = Drive.ParseMacOSDiskutilInfo(info, "/dev/disk1");

            Assert.NotNull(actual);
            Assert.Equal(InternalDriveType.Optical, actual!.DriveType);
            Assert.Equal("/dev/disk1", actual.DevicePath);
            Assert.Equal(10061856L, actual.TotalSize);
        }

        /// <remarks>
        /// Sample captured verbatim from a USB-attached optical drive, the configuration an
        /// external drive on a Mac uses. It pairs with the drive above to pin down why detection
        /// keys off the presence of the optical-only fields rather than any single value: this
        /// drive fills those fields in and reports the protocol as "USB", while the one above
        /// leaves them blank and reports "SATA". Reading either value, rather than testing for
        /// the field, would misclassify one of the two.
        /// </remarks>
        [Fact]
        public void ParseMacOSDiskutilInfo_UsbOpticalDrive_ClassifiesOptical()
        {
            string info =
                """
                   Device Identifier:         disk4
                   Device Node:               /dev/disk4
                   Whole:                     Yes
                   Device / Media Name:       PLEXTOR DVDR PX-760A
                   Volume Name:               Audio CD
                   Content (IOContent):       CD_partition_scheme
                   Media Type:
                   Protocol:                  USB
                   Disk Size:                 310.6 MB (310635696 Bytes) (606710 512-Byte-Units + 176 Byte(s))
                   Device Block Size:         2352 Bytes
                   Media Read-Only:           Yes
                   Device Location:           External
                   Removable Media:           Removable
                   Virtual:                   No
                   Optical Drive Type:        CD-ROM, CD-R, CD-RW, DVD-ROM, DVD-R, DVD-R DL, DVD-RW, DVD+R, DVD+R DL, DVD+RW
                   Optical Media Type:        CD-ROM
                   Optical Media Erasable:    No
                """;

            var actual = Drive.ParseMacOSDiskutilInfo(info, "/dev/disk4");

            Assert.NotNull(actual);
            Assert.Equal(InternalDriveType.Optical, actual!.DriveType);
            Assert.Equal("/dev/disk4", actual.DevicePath);
            Assert.Equal("disk4", actual.BsdName);
            Assert.Equal(310635696L, actual.TotalSize);
        }

        [Fact]
        public void ParseMacOSDiskutilInfo_MissingSize_DefaultsToFixedWithUnknownSize()
        {
            string info =
                """
                   Device Node:               /dev/disk9
                   Whole:                     Yes
                   Removable Media:           Fixed
                """;

            var actual = Drive.ParseMacOSDiskutilInfo(info, "/dev/disk9");

            Assert.NotNull(actual);
            Assert.Equal(InternalDriveType.HardDisk, actual!.DriveType);
            Assert.Equal(0L, actual.TotalSize);
        }

        [Fact]
        public void ParseMacOSDiskutilInfo_MissingDeviceNode_UsesFallbackPath()
        {
            string info =
                """
                   Whole:                     Yes
                   Removable Media:           Fixed
                """;

            var actual = Drive.ParseMacOSDiskutilInfo(info, "/dev/disk4");

            Assert.NotNull(actual);
            Assert.Equal("/dev/disk4", actual!.DevicePath);
            Assert.Equal("disk4", actual.BsdName);
        }

        /// <remarks>
        /// A dumping program on macOS takes a drive as its BSD name, not as a device node path:
        /// redumper matches the name against the BSD name IOKit publishes for the drive and
        /// fails with "failed to find matching SCSI authoring device" when given "/dev/disk4".
        /// The name reaches the program verbatim, so a prefixed name breaks every dump.
        /// </remarks>
        [Fact]
        public void ParseMacOSDiskutilInfo_BsdName_IsNotDeviceNodePrefixed()
        {
            string info =
                """
                   Device Identifier:         disk4
                   Device Node:               /dev/disk4
                   Whole:                     Yes
                   Removable Media:           Removable
                   Optical Media Type:        CD-ROM
                """;

            var actual = Drive.ParseMacOSDiskutilInfo(info, "/dev/disk4");

            Assert.NotNull(actual);
            Assert.DoesNotContain("/dev/", actual!.BsdName);
            Assert.Equal("disk4", actual.BsdName);
        }

        #endregion

        #region ParseDiskutilByteSize

        [Theory]
        [InlineData("402.7 MB (402653184 Bytes) (exactly 786432 512-Byte-Units)", 402653184L)]
        [InlineData("15.5 GB (15518924800 Bytes) (exactly 30310400 512-Byte-Units)", 15518924800L)]
        [InlineData("1.4 MB (1474560 Bytes) (exactly 2880 512-Byte-Units)", 1474560L)]
        [InlineData("Zero KB (0 Bytes) (exactly 0 512-Byte-Units)", 0L)]
        [InlineData("", 0L)]
        [InlineData("no size information", 0L)]
        public void ParseDiskutilByteSize_ExtractsExactBytes(string value, long expected)
        {
            Assert.Equal(expected, Drive.ParseDiskutilByteSize(value));
        }

        #endregion
    }
}
