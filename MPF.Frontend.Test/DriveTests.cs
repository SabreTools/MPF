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

        #region EnumerateUnixOpticalDevicePaths

        [Fact]
        public void EnumerateUnixOpticalDevicePaths_NullOrEmpty_ReturnsEmpty()
        {
            Assert.Empty(Drive.EnumerateUnixOpticalDevicePaths(null!));
            Assert.Empty(Drive.EnumerateUnixOpticalDevicePaths(string.Empty));
        }

        [Fact]
        public void EnumerateUnixOpticalDevicePaths_MissingDirectory_ReturnsEmpty()
        {
            string missing = Path.Combine(Path.GetTempPath(), "mpf-test-missing-" + Guid.NewGuid().ToString("N"));
            Assert.Empty(Drive.EnumerateUnixOpticalDevicePaths(missing));
        }

        [Fact]
        public void EnumerateUnixOpticalDevicePaths_OnlyMatchesSrFollowedByDigits()
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

                var actual = Drive.EnumerateUnixOpticalDevicePaths(root);

                var actualNames = new List<string>();
                foreach (var p in actual)
                    actualNames.Add(Path.GetFileName(p));
                actualNames.Sort(StringComparer.Ordinal);

                Assert.Equal(new[] { "sr0", "sr1", "sr15" }, actualNames);
            }
            finally
            {
                Directory.Delete(root, recursive: true);
            }
        }

        #endregion
    }
}
