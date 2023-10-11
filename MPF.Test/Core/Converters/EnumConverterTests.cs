using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MPF.Core.Converters;
using MPF.Core.Data;
using Xunit;

namespace MPF.Test.Core.Converters
{
    public class EnumConverterTests
    {
        #region Cross-enumeration conversions

        /// <summary>
        /// DiscType values that map to InternalDriveType
        /// </summary>
        private static readonly DriveType[] _mappableDriveTypes = new DriveType[]
        {
            DriveType.CDRom,
            DriveType.Fixed,
            DriveType.Removable,
        };

        /// <summary>
        /// Check that every supported DriveType maps to an InternalDriveType
        /// </summary>
        /// <param name="driveType">DriveType value to check</param>
        /// <param name="expectNull">True to expect a null mapping, false otherwise</param>
        [Theory]
        [MemberData(nameof(GenerateDriveTypeMappingTestData))]
        public void ToInternalDriveTypeTest(DriveType driveType, bool expectNull)
        {
            var actual = driveType.ToInternalDriveType();

            if (expectNull)
                Assert.Null(actual);
            else
                Assert.NotNull(actual);
        }

        /// <summary>
        /// Generate a test set of DriveType values
        /// </summary>
        /// <returns>MemberData-compatible list of DriveType values</returns>
#if NET48
        public static List<object[]> GenerateDriveTypeMappingTestData()
#else
        public static List<object?[]> GenerateDriveTypeMappingTestData()
#endif
        {
#if NET48
            var testData = new List<object[]>() { new object[] { null, true } };
#else
            var testData = new List<object?[]>() { new object?[] { null, true } };
#endif
            foreach (DriveType driveType in Enum.GetValues(typeof(DriveType)))
            {
                if (_mappableDriveTypes.Contains(driveType))
#if NET48
                    testData.Add(new object[] { driveType, false });
#else
                    testData.Add(new object?[] { driveType, false });
#endif
                else
#if NET48
                    testData.Add(new object[] { driveType, true });
#else
                    testData.Add(new object?[] { driveType, true });
#endif
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
#if NET48
        public static List<object[]> GenerateInternalProgramTestData()
#else
        public static List<object?[]> GenerateInternalProgramTestData()
#endif
        {
#if NET48
            var testData = new List<object[]>() { new object[] { null } };
#else
            var testData = new List<object?[]>() { new object?[] { null } };
#endif
            foreach (InternalProgram? internalProgram in Enum.GetValues(typeof(InternalProgram)))
            {
#if NET48
                testData.Add(new object[] { internalProgram });
#else
                testData.Add(new object?[] { internalProgram });
#endif
            }

            return testData;
        }

#endregion

        // TODO: Add from-string tests
    }
}
