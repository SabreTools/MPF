using System;
using System.Collections.Generic;
using System.Linq;
using RedumpLib.Data;
using Xunit;

namespace MPF.Test.RedumpLib
{
    // TODO: Add tests for string-to-enum conversion
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
        [MemberData(nameof(GenerateRedumpSystemMappingTestData))]
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
        [MemberData(nameof(GenerateMediaTypeMappingTestData))]
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
        [MemberData(nameof(GenerateDiscTypeMappingTestData))]
        public void ToMediaTypeTest(DiscType? discType, bool expectNull)
        {
            MediaType? actual = discType.ToMediaType();
            Assert.Equal(expectNull, actual == null);
        }

        /// <summary>
        /// Generate a test set of DiscType values
        /// </summary>
        /// <returns>MemberData-compatible list of DiscType values</returns>
        public static List<object[]> GenerateDiscTypeMappingTestData()
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
        public static List<object[]> GenerateRedumpSystemMappingTestData()
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
        public static List<object[]> GenerateMediaTypeMappingTestData()
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

        #region Disc Category

        /// <summary>
        /// Check that every DiscCategory has a long name provided
        /// </summary>
        /// <param name="discCategory">DiscCategory value to check</param>
        [Theory]
        [MemberData(nameof(GenerateDiscCategoryTestData))]
        public void DiscCategoryLongNameTest(DiscCategory? discCategory, bool expectNull)
        {
            string actual = discCategory.LongName();

            if (expectNull)
                Assert.Null(actual);
            else
                Assert.NotNull(actual);
        }

        /// <summary>
        /// Generate a test set of DiscCategory values
        /// </summary>
        /// <returns>MemberData-compatible list of DiscCategory values</returns>
        public static List<object[]> GenerateDiscCategoryTestData()
        {
            var testData = new List<object[]>() { new object[] { null, true } };
            foreach (DiscCategory? discCategory in Enum.GetValues(typeof(DiscCategory)))
            {
                testData.Add(new object[] { discCategory, false });
            }

            return testData;
        }

        #endregion

        #region Disc Type

        /// <summary>
        /// Check that every DiscType has a long name provided
        /// </summary>
        /// <param name="discType">DiscType value to check</param>
        /// <param name="expectNull">True to expect a null value, false otherwise</param>
        [Theory]
        [MemberData(nameof(GenerateDiscTypeTestData))]
        public void DiscTypeLongNameTest(DiscType? discType, bool expectNull)
        {
            string actual = discType.LongName();

            if (expectNull)
                Assert.Null(actual);
            else
                Assert.NotNull(actual);
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
                if (discType == DiscType.NONE)
                    testData.Add(new object[] { discType, true });
                else
                    testData.Add(new object[] { discType, false });
            }

            return testData;
        }

        #endregion

        #region Language

        /// <summary>
        /// Check that every Language has a long name provided
        /// </summary>
        /// <param name="language">Language value to check</param>
        /// <param name="expectNull">True to expect a null value, false otherwise</param>
        [Theory]
        [MemberData(nameof(GenerateLanguageTestData))]
        public void LanguageLongNameTest(Language? language, bool expectNull)
        {
            string actual = language.LongName();

            if (expectNull)
                Assert.Null(actual);
            else
                Assert.NotNull(actual);
        }

        /// <summary>
        /// Check that every Language has a short name provided
        /// </summary>
        /// <param name="language">Language value to check</param>
        /// <param name="expectNull">True to expect a null value, false otherwise</param>
        [Theory]
        [MemberData(nameof(GenerateLanguageTestData))]
        public void LanguageShortNameTest(Language? language, bool expectNull)
        {
            string actual = language.ShortName();

            if (expectNull)
                Assert.Null(actual);
            else
                Assert.NotNull(actual);
        }

        /// <summary>
        /// Generate a test set of Language values
        /// </summary>
        /// <returns>MemberData-compatible list of Language values</returns>
        public static List<object[]> GenerateLanguageTestData()
        {
            var testData = new List<object[]>() { new object[] { null, true } };
            foreach (Language? language in Enum.GetValues(typeof(Language)))
            {
                testData.Add(new object[] { language, false });
            }

            return testData;
        }

        #endregion

        #region Language Selection

        /// <summary>
        /// Check that every LanguageSelection has a long name provided
        /// </summary>
        /// <param name="languageSelection">LanguageSelection value to check</param>
        /// <param name="expectNull">True to expect a null value, false otherwise</param>
        [Theory]
        [MemberData(nameof(GenerateLanguageSelectionTestData))]
        public void LanguageSelectionLongNameTest(LanguageSelection? languageSelection, bool expectNull)
        {
            string actual = languageSelection.LongName();

            if (expectNull)
                Assert.Null(actual);
            else
                Assert.NotNull(actual);
        }

        /// <summary>
        /// Generate a test set of LanguageSelection values
        /// </summary>
        /// <returns>MemberData-compatible list of LanguageSelection values</returns>
        public static List<object[]> GenerateLanguageSelectionTestData()
        {
            var testData = new List<object[]>() { new object[] { null, true } };
            foreach (LanguageSelection? languageSelection in Enum.GetValues(typeof(LanguageSelection)))
            {
                testData.Add(new object[] { languageSelection, false });
            }

            return testData;
        }

        #endregion

        #region Media Type

        /// <summary>
        /// Check that every MediaType has a long name provided
        /// </summary>
        /// <param name="mediaType">MediaType value to check</param>
        /// <param name="expectNull">True to expect a null value, false otherwise</param>
        [Theory]
        [MemberData(nameof(GenerateMediaTypeTestData))]
        public void MediaTypeLongNameTest(MediaType? mediaType, bool expectNull)
        {
            string actual = mediaType.LongName();

            if (expectNull)
                Assert.Null(actual);
            else
                Assert.NotNull(actual);
        }

        /// <summary>
        /// Check that every MediaType has a short name provided
        /// </summary>
        /// <param name="mediaType">MediaType value to check</param>
        /// <param name="expectNull">True to expect a null value, false otherwise</param>
        [Theory]
        [MemberData(nameof(GenerateMediaTypeTestData))]
        public void MediaTypeShortNameTest(MediaType? mediaType, bool expectNull)
        {
            string actual = mediaType.ShortName();

            if (expectNull)
                Assert.Null(actual);
            else
                Assert.NotNull(actual);
        }

        /// <summary>
        /// Generate a test set of MediaType values
        /// </summary>
        /// <returns>MemberData-compatible list of MediaType values</returns>
        public static List<object[]> GenerateMediaTypeTestData()
        {
            var testData = new List<object[]>() { new object[] { null, true } };
            foreach (MediaType? mediaType in Enum.GetValues(typeof(MediaType)))
            {
                testData.Add(new object[] { mediaType, false });
            }

            return testData;
        }

        #endregion

        #region Region

        /// <summary>
        /// Check that every Region has a long name provided
        /// </summary>
        /// <param name="mediaType">Region value to check</param>
        /// <param name="expectNull">True to expect a null value, false otherwise</param>
        [Theory]
        [MemberData(nameof(GenerateRegionTestData))]
        public void RegionLongNameTest(Region? region, bool expectNull)
        {
            string actual = region.LongName();

            if (expectNull)
                Assert.Null(actual);
            else
                Assert.NotNull(actual);
        }

        /// <summary>
        /// Check that every Region has a short name provided
        /// </summary>
        /// <param name="region">Region value to check</param>
        /// <param name="expectNull">True to expect a null value, false otherwise</param>
        [Theory]
        [MemberData(nameof(GenerateRegionTestData))]
        public void RegionShortNameTest(Region? region, bool expectNull)
        {
            string actual = region.ShortName();

            if (expectNull)
                Assert.Null(actual);
            else
                Assert.NotNull(actual);
        }

        /// <summary>
        /// Generate a test set of Region values
        /// </summary>
        /// <returns>MemberData-compatible list of Region values</returns>
        public static List<object[]> GenerateRegionTestData()
        {
            var testData = new List<object[]>() { new object[] { null, true } };
            foreach (Region? region in Enum.GetValues(typeof(Region)))
            {
                testData.Add(new object[] { region, false });
            }

            return testData;
        }

        #endregion
    }
}
