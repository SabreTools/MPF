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
    }
}
