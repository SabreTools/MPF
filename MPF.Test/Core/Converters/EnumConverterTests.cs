using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if NETFRAMEWORK
using IMAPI2;
#endif
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

#if NETFRAMEWORK
        /// <summary>
        /// IMAPI_MEDIA_PHYSICAL_TYPE values that map to MediaType
        /// </summary>
        private static readonly IMAPI_MEDIA_PHYSICAL_TYPE[] _mappableImapiTypes = new IMAPI_MEDIA_PHYSICAL_TYPE[]
        {
            IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_UNKNOWN,
            IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_CDROM,
            IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_CDR,
            IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_CDRW,
            IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDROM,
            IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDRAM,
            IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDPLUSR,
            IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDPLUSRW,
            IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDPLUSR_DUALLAYER,
            IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDDASHR,
            IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDDASHRW,
            IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDDASHR_DUALLAYER,
            IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DISK,
            IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDPLUSRW_DUALLAYER,
            IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_HDDVDROM,
            IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_HDDVDR,
            IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_HDDVDRAM,
            IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_BDROM,
            IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_BDR,
            IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_BDRE,
        };
#endif

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

#if NETFRAMEWORK
        /// <summary>
        /// Check that every supported IMAPI_MEDIA_PHYSICAL_TYPE maps to an MediaType
        /// </summary>
        /// <param name="imapiType">IMAPI_MEDIA_PHYSICAL_TYPE value to check</param>
        /// <param name="expectNull">True to expect a null mapping, false otherwise</param>
        [Theory]
        [MemberData(nameof(GenerateImapiTypeMappingTestData))]
        public void IMAPIToMediaTypeTest(IMAPI_MEDIA_PHYSICAL_TYPE imapiType, bool expectNull)
        {
            var actual = imapiType.IMAPIToMediaType();

            if (expectNull)
                Assert.Null(actual);
            else
                Assert.NotNull(actual);
        }
#endif

        /// <summary>
        /// Generate a test set of DriveType values
        /// </summary>
        /// <returns>MemberData-compatible list of DriveType values</returns>
        public static List<object[]> GenerateDriveTypeMappingTestData()
        {
            var testData = new List<object[]>() { new object[] { null, true } };
            foreach (DriveType driveType in Enum.GetValues(typeof(DriveType)))
            {
                if (_mappableDriveTypes.Contains(driveType))
                    testData.Add(new object[] { driveType, false });
                else
                    testData.Add(new object[] { driveType, true });
            }

            return testData;
        }

#if NETFRAMEWORK
        /// <summary>
        /// Generate a test set of IMAPI_MEDIA_PHYSICAL_TYPE values
        /// </summary>
        /// <returns>MemberData-compatible list of IMAPI_MEDIA_PHYSICAL_TYPE values</returns>
        public static List<object[]> GenerateImapiTypeMappingTestData()
        {
            var testData = new List<object[]>() { new object[] { null, false } };
            foreach (IMAPI_MEDIA_PHYSICAL_TYPE imapiType in Enum.GetValues(typeof(IMAPI_MEDIA_PHYSICAL_TYPE)))
            {
                if (_mappableImapiTypes.Contains(imapiType))
                    testData.Add(new object[] { imapiType, false });
                else
                    testData.Add(new object[] { imapiType, true });
            }

            return testData;
        }
#endif

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
        public static List<object[]> GenerateInternalProgramTestData()
        {
            var testData = new List<object[]>() { new object[] { null } };
            foreach (InternalProgram? internalProgram in Enum.GetValues(typeof(InternalProgram)))
            {
                testData.Add(new object[] { internalProgram });
            }

            return testData;
        }

        #endregion

        // TODO: Add from-string tests
    }
}
