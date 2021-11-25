using System;
using System.Collections.Generic;
using System.Linq;
using RedumpLib.Data;
using Xunit;

namespace MPF.Test.RedumpLib
{
    public class ExtensionsTests
    {
        #region Cross-Enumeration

        /// <summary>
        /// DiscType values that map to MediaType
        /// </summary>
        private static readonly DiscType?[] _mappableDiscTypes = new DiscType?[]
        {
            DiscType.BD25,
            DiscType.BD50,
            DiscType.CD,
            DiscType.DVD5,
            DiscType.DVD9,
            DiscType.GDROM,
            DiscType.HDDVDSL,
            DiscType.NintendoGameCubeGameDisc,
            DiscType.NintendoWiiOpticalDiscSL,
            DiscType.NintendoWiiOpticalDiscDL,
            DiscType.NintendoWiiUOpticalDiscSL,
            DiscType.UMDSL,
            DiscType.UMDDL,
        };

        /// <summary>
        /// MediaType values that map to DiscType
        /// </summary>
        private static readonly MediaType?[] _mappableMediaTypes = new MediaType?[]
        {
            MediaType.BluRay,
            MediaType.CDROM,
            MediaType.DVD,
            MediaType.GDROM,
            MediaType.HDDVD,
            MediaType.NintendoGameCubeGameDisc,
            MediaType.NintendoWiiOpticalDisc,
            MediaType.NintendoWiiUOpticalDisc,
            MediaType.UMD,
        };

        /// <summary>
        /// Check that every supported system has some set of MediaTypes supported
        /// </summary>
        /// <param name="redumpSystem">RedumpSystem value to check</param>
        [Theory]
        [MemberData(nameof(GenerateRedumpSystemTestData))]
        public void MediaTypesTest(RedumpSystem? redumpSystem)
        {
            var actual = redumpSystem.MediaTypes();
            Assert.NotEmpty(actual);
        }

        /// <summary>
        /// Check that both mappable and unmappable media types output correctly
        /// </summary>
        /// <param name="mediaType">MediaType value to check</param>
        /// <param name="expectNull">True to expect a null mapping, false otherwise</param>
        [Theory]
        [MemberData(nameof(GenerateMediaTypeTestData))]
        public void ToDiscTypeTest(MediaType? mediaType, bool expectNull)
        {
            DiscType? actual = mediaType.ToDiscType();
            Assert.Equal(expectNull, actual == null);
        }

        /// <summary>
        /// Check that DiscType values all map to something appropriate
        /// </summary>
        /// <param name="discType">DiscType value to check</param>
        /// <param name="expectNull">True to expect a null mapping, false otherwise</param>
        [Theory]
        [MemberData(nameof(GenerateDiscTypeTestData))]
        public void ToMediaTypeTest(DiscType? discType, bool expectNull)
        {
            MediaType? actual = discType.ToMediaType();
            Assert.Equal(expectNull, actual == null);
        }

        /// <summary>
        /// Generate a test set of DiscType values
        /// </summary>
        /// <returns>MemberData-compatible list of DiscType values</returns>
        public static List<object[]> GenerateDiscTypeTestData()
        {
            var testData = new List<object[]>() { new object[] { null, true } };
            foreach (DiscType? discType in Enum.GetValues(typeof(DiscType)))
            {
                if (_mappableDiscTypes.Contains(discType))
                    testData.Add(new object[] { discType, false });
                else
                    testData.Add(new object[] { discType, true });
            }

            return testData;
        }

        /// <summary>
        /// Generate a test set of RedumpSystem values
        /// </summary>
        /// <returns>MemberData-compatible list of RedumpSystem values</returns>
        public static List<object[]> GenerateRedumpSystemTestData()
        {
            var testData = new List<object[]>() { new object[] { null } };
            foreach (RedumpSystem? redumpSystem in Enum.GetValues(typeof(RedumpSystem)))
            {
                testData.Add(new object[] { redumpSystem });
            }

            return testData;
        }

        /// <summary>
        /// Generate a test set of mappable media types
        /// </summary>
        /// <returns>MemberData-compatible list of MediaTypes</returns>
        public static List<object[]> GenerateMediaTypeTestData()
        {
            var testData = new List<object[]>() { new object[] { null, true } };

            foreach (MediaType? mediaType in Enum.GetValues(typeof(MediaType)))
            {
                if (_mappableMediaTypes.Contains(mediaType))
                    testData.Add(new object[] { mediaType, false });
                else
                    testData.Add(new object[] { mediaType, true });
            }

            return testData;
        }

        #endregion
    }
}
