using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MPF.Core;
using MPF.Core.Utilities;
using Xunit;

namespace MPF.Test.Core.Converters
{
    public class EnumConverterTests
    {
        #region Cross-enumeration conversions

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
            foreach (DriveType driveType in Enum.GetValues(typeof(DriveType)))
            {
                if (_mappableDriveTypes.Contains(driveType))
                    testData.Add([driveType, false]);
                else
                    testData.Add([driveType, true]);
            }

            return testData;
        }

        #endregion

        #region Convert to Long Name

        // TODO: Maybe add a test for the generic "GetLongName" method

        /// <summary>
        /// Check that every InternalProgram has a long name provided
        /// </summary>
        /// <param name="internalProgram">InternalProgram value to check</param>
        [Theory]
        [MemberData(nameof(GenerateInternalProgramTestData))]
        public void InternalProgramLongNameTest(InternalProgram? internalProgram)
        {
            string actual = internalProgram.LongName();
            Assert.NotNull(actual);
        }

        /// <summary>
        /// Generate a test set of InternalProgram values
        /// </summary>
        /// <returns>MemberData-compatible list of InternalProgram values</returns>
        public static List<object?[]> GenerateInternalProgramTestData()
        {
            var testData = new List<object?[]>() { new object?[] { null } };
            foreach (InternalProgram? internalProgram in Enum.GetValues(typeof(InternalProgram)))
            {
                testData.Add([internalProgram]);
            }

            return testData;
        }

        #endregion

        // TODO: Add from-string tests
    }
}
