using System;
using System.Collections.Generic;
using System.Data;
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
        /// <param name="expectNull">True to expect a null value, false otherwise</param>
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
        /// Ensure that every Language that has an ISO 639-1 code is unique
        /// </summary>
        [Fact]
        public void NoDuplicateTwoLetterCodeTest()
        {
            var fullLanguages = Enum.GetValues(typeof(Language)).Cast<Language?>().ToList();
            var filteredLanguages = new Dictionary<string, Language?>();

            int totalCount = 0;
            foreach (Language? language in fullLanguages)
            {
                string code = language.TwoLetterCode();
                if (string.IsNullOrWhiteSpace(code))
                    continue;

                // Throw if the code already exists
                if (filteredLanguages.ContainsKey(code))
                    throw new DuplicateNameException($"Code {code} already in dictionary");

                filteredLanguages[code] = language;
                totalCount++;
            }

            Assert.Equal(totalCount, filteredLanguages.Count);
        }

        /// <summary>
        /// Ensure that every Language that has a standard/bibliographic ISO 639-2 code is unique
        /// </summary>
        [Fact]
        public void NoDuplicateThreeLetterCodeTest()
        {
            var fullLanguages = Enum.GetValues(typeof(Language)).Cast<Language?>().ToList();
            var filteredLanguages = new Dictionary<string, Language?>();

            int totalCount = 0;
            foreach (Language? language in fullLanguages)
            {
                string code = language.ThreeLetterCode();
                if (string.IsNullOrWhiteSpace(code))
                    continue;

                // Throw if the code already exists
                if (filteredLanguages.ContainsKey(code))
                    throw new DuplicateNameException($"Code {code} already in dictionary");

                filteredLanguages[code] = language;
                totalCount++;
            }

            Assert.Equal(totalCount, filteredLanguages.Count);
        }

        /// <summary>
        /// Ensure that every Language that has a terminology ISO 639-2 code is unique
        /// </summary>
        [Fact]
        public void NoDuplicateThreeLetterCodeAltTest()
        {
            var fullLanguages = Enum.GetValues(typeof(Language)).Cast<Language?>().ToList();
            var filteredLanguages = new Dictionary<string, Language?>();

            int totalCount = 0;
            foreach (Language? language in fullLanguages)
            {
                string code = language.ThreeLetterCodeAlt();
                if (string.IsNullOrWhiteSpace(code))
                    continue;

                // Throw if the code already exists
                if (filteredLanguages.ContainsKey(code))
                    throw new DuplicateNameException($"Code {code} already in dictionary");

                filteredLanguages[code] = language;
                totalCount++;
            }

            Assert.Equal(totalCount, filteredLanguages.Count);
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
        /// <param name="region">Region value to check</param>
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

        #region Site Code

        /// <summary>
        /// Check that every SiteCode has a long name provided
        /// </summary>
        /// <param name="siteCode">SiteCode value to check</param>
        /// <param name="expectNull">True to expect a null value, false otherwise</param>
        [Theory]
        [MemberData(nameof(GenerateSiteCodeTestData))]
        public void SiteCodeLongNameTest(SiteCode? siteCode, bool expectNull)
        {
            string actual = siteCode.LongName();

            if (expectNull)
                Assert.Null(actual);
            else
                Assert.NotNull(actual);
        }

        /// <summary>
        /// Check that every SiteCode has a short name provided
        /// </summary>
        /// <param name="siteCode">SiteCode value to check</param>
        /// <param name="expectNull">True to expect a null value, false otherwise</param>
        [Theory]
        [MemberData(nameof(GenerateSiteCodeTestData))]
        public void SiteCodeShortNameTest(SiteCode? siteCode, bool expectNull)
        {
            string actual = siteCode.ShortName();

            if (expectNull)
                Assert.Null(actual);
            else
                Assert.NotNull(actual);
        }

        /// <summary>
        /// Generate a test set of SiteCode values
        /// </summary>
        /// <returns>MemberData-compatible list of SiteCode values</returns>
        public static List<object[]> GenerateSiteCodeTestData()
        {
            var testData = new List<object[]>() { new object[] { null, true } };
            foreach (SiteCode? siteCode in Enum.GetValues(typeof(SiteCode)))
            {
                testData.Add(new object[] { siteCode, false });
            }

            return testData;
        }

        #endregion

        #region System

        /// <summary>
        /// RedumpSystem values that are considered markers and not real systems
        /// </summary>
        private static readonly RedumpSystem?[] _markerSystemTypes = new RedumpSystem?[]
        {
            RedumpSystem.MarkerArcadeEnd,
            RedumpSystem.MarkerComputerEnd,
            RedumpSystem.MarkerDiscBasedConsoleEnd,
            RedumpSystem.MarkerOtherEnd,
        };

        /// <summary>
        /// Check that every RedumpSystem has a long name provided
        /// </summary>
        /// <param name="redumpSystem">RedumpSystem value to check</param>
        /// <param name="expectNull">True to expect a null value, false otherwise</param>
        [Theory]
        [MemberData(nameof(GenerateRedumpSystemTestData))]
        public void RedumpSystemLongNameTest(RedumpSystem? redumpSystem, bool expectNull)
        {
            string actual = redumpSystem.LongName();

            if (expectNull)
                Assert.Null(actual);
            else
                Assert.NotNull(actual);
        }

        // TODO: Re-enable the following test once non-Redump systems are accounted for

        /// <summary>
        /// Check that every RedumpSystem has a short name provided
        /// </summary>
        /// <param name="redumpSystem">RedumpSystem value to check</param>
        /// <param name="expectNull">True to expect a null value, false otherwise</param>
        //[Theory]
        //[MemberData(nameof(GenerateRedumpSystemTestData))]
        //public void RedumpSystemShortNameTest(RedumpSystem? redumpSystem, bool expectNull)
        //{
        //    string actual = redumpSystem.ShortName();

        //    if (expectNull)
        //        Assert.Null(actual);
        //    else
        //        Assert.NotNull(actual);
        //}

        // TODO: Test the other attributes as well
        // Most are bool checks so they're not as interesting to have unit tests around
        // SystemCategory always returns something as well, so is it worth testing?

        /// <summary>
        /// Generate a test set of RedumpSystem values
        /// </summary>
        /// <returns>MemberData-compatible list of RedumpSystem values</returns>
        public static List<object[]> GenerateRedumpSystemTestData()
        {
            var testData = new List<object[]>() { new object[] { null, true } };
            foreach (RedumpSystem? redumpSystem in Enum.GetValues(typeof(RedumpSystem)))
            {
                // We want to skip all markers for this
                if (_markerSystemTypes.Contains(redumpSystem))
                    continue;

                testData.Add(new object[] { redumpSystem, false });
            }

            return testData;
        }

        #endregion

        #region System Category

        /// <summary>
        /// Check that every SystemCategory has a long name provided
        /// </summary>
        /// <param name="systemCategory">SystemCategory value to check</param>
        /// <param name="expectNull">True to expect a null value, false otherwise</param>
        [Theory]
        [MemberData(nameof(GenerateSystemCategoryTestData))]
        public void SystemCategoryLongNameTest(SystemCategory? systemCategory, bool expectNull)
        {
            string actual = systemCategory.LongName();

            if (expectNull)
                Assert.Null(actual);
            else
                Assert.NotNull(actual);
        }

        /// <summary>
        /// Generate a test set of SystemCategory values
        /// </summary>
        /// <returns>MemberData-compatible list of SystemCategory values</returns>
        public static List<object[]> GenerateSystemCategoryTestData()
        {
            var testData = new List<object[]>() { new object[] { null, true } };
            foreach (SystemCategory? systemCategory in Enum.GetValues(typeof(SystemCategory)))
            {
                if (systemCategory == SystemCategory.NONE)
                    testData.Add(new object[] { systemCategory, true });
                else
                    testData.Add(new object[] { systemCategory, false });
            }

            return testData;
        }

        #endregion

        #region Yes/No

        /// <summary>
        /// Check that every YesNo has a long name provided
        /// </summary>
        /// <param name="yesNo">YesNo value to check</param>
        /// <param name="expectNull">True to expect a null value, false otherwise</param>
        [Theory]
        [MemberData(nameof(GenerateYesNoTestData))]
        public void YesNoLongNameTest(YesNo? yesNo, bool expectNull)
        {
            string actual = yesNo.LongName();

            if (expectNull)
                Assert.Null(actual);
            else
                Assert.NotNull(actual);
        }

        /// <summary>
        /// Generate a test set of YesNo values
        /// </summary>
        /// <returns>MemberData-compatible list of YesNo values</returns>
        public static List<object[]> GenerateYesNoTestData()
        {
            var testData = new List<object[]>() { new object[] { null, false } };
            foreach (YesNo? yesNo in Enum.GetValues(typeof(YesNo)))
            {
                testData.Add(new object[] { yesNo, false });
            }

            return testData;
        }

        #endregion
    }
}
