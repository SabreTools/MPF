using System.Collections.Generic;
using Xunit;

namespace MPF.Frontend.Test
{
    public class OptionsTests
    {
        [Theory]
        [InlineData("key2", null, "key", false, false)]
        [InlineData("key", null, "key", false, false)]
        [InlineData("key", "", "key", false, false)]
        [InlineData("key", "INVALID", "key", false, false)]
        [InlineData("key", "true", "key", false, true)]
        public void GetBooleanSettingTest(string key, string? value, string expectedKey, bool defaultValue, bool expectedValue)
        {
            Dictionary<string, string?> settings = new() { [key] = value };
            bool actual = Options.GetBooleanSetting(settings, expectedKey, defaultValue);
            Assert.Equal(expectedValue, actual);
        }

        [Theory]
        [InlineData("key2", null, "key", -1, -1)]
        [InlineData("key", null, "key", -1, -1)]
        [InlineData("key", "", "key", -1, -1)]
        [InlineData("key", "INVALID", "key", -1, -1)]
        [InlineData("key", "12345", "key", -1, 12345)]
        public void GetInt32SettingTest(string key, string? value, string expectedKey, int defaultValue, int expectedValue)
        {
            Dictionary<string, string?> settings = new() { [key] = value };
            int actual = Options.GetInt32Setting(settings, expectedKey, defaultValue);
            Assert.Equal(expectedValue, actual);
        }

        [Theory]
        [InlineData("key2", null, "key", null, null)]
        [InlineData("key", null, "key", null, null)]
        [InlineData("key", "", "key", null, "")]
        [InlineData("key", "INVALID", "key", null, "INVALID")]
        [InlineData("key", "String", "key", null, "String")]
        public void GetStringSettingTest(string key, string? value, string expectedKey, string? defaultValue, string? expectedValue)
        {
            Dictionary<string, string?> settings = new() { [key] = value };
            string? actual = Options.GetStringSetting(settings, expectedKey, defaultValue);
            Assert.Equal(expectedValue, actual);
        }
    }
}