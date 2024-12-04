using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MPF.ExecutionContexts.Test
{
    public class BaseExecutionContextTests
    {
        #region GetBooleanSetting

        [Fact]
        public void GetBooleanSetting_InvalidDict_Default()
        {
            Dictionary<string, string?> settings = [];
            string key = "key";
            bool defaultValue = false;

            bool actual = BaseExecutionContext.GetBooleanSetting(settings, key, defaultValue);
            Assert.False(actual);
        }

        [Fact]
        public void GetBooleanSetting_InvalidKey_Default()
        {
            Dictionary<string, string?> settings = new()
            {
                ["key2"] = "true",
            };
            string key = "key";
            bool defaultValue = false;

            bool actual = BaseExecutionContext.GetBooleanSetting(settings, key, defaultValue);
            Assert.False(actual);
        }

        [Fact]
        public void GetBooleanSetting_NullValue_Default()
        {
            Dictionary<string, string?> settings = new()
            {
                ["key"] = null,
            };
            string key = "key";
            bool defaultValue = false;

            bool actual = BaseExecutionContext.GetBooleanSetting(settings, key, defaultValue);
            Assert.False(actual);
        }

        [Fact]
        public void GetBooleanSetting_InvalidValue_Default()
        {
            Dictionary<string, string?> settings = new()
            {
                ["key"] = "invalid",
            };
            string key = "key";
            bool defaultValue = false;

            bool actual = BaseExecutionContext.GetBooleanSetting(settings, key, defaultValue);
            Assert.False(actual);
        }

        [Fact]
        public void GetBooleanSetting_ValidValue_Parsed()
        {
            Dictionary<string, string?> settings = new()
            {
                ["key"] = "true",
            };
            string key = "key";
            bool defaultValue = false;

            bool actual = BaseExecutionContext.GetBooleanSetting(settings, key, defaultValue);
            Assert.True(actual);
        }

        #endregion

        #region GetInt32Setting

        [Fact]
        public void GetInt32Setting_InvalidDict_Default()
        {
            Dictionary<string, string?> settings = [];
            string key = "key";
            int defaultValue = -1;

            int actual = BaseExecutionContext.GetInt32Setting(settings, key, defaultValue);
            Assert.Equal(-1, actual);
        }

        [Fact]
        public void GetInt32Setting_InvalidKey_Default()
        {
            Dictionary<string, string?> settings = new()
            {
                ["key2"] = "12345",
            };
            string key = "key";
            int defaultValue = -1;

            int actual = BaseExecutionContext.GetInt32Setting(settings, key, defaultValue);
            Assert.Equal(-1, actual);
        }

        [Fact]
        public void GetInt32Setting_NullValue_Default()
        {
            Dictionary<string, string?> settings = new()
            {
                ["key"] = null,
            };
            string key = "key";
            int defaultValue = -1;

            int actual = BaseExecutionContext.GetInt32Setting(settings, key, defaultValue);
            Assert.Equal(-1, actual);
        }

        [Fact]
        public void GetInt32Setting_InvalidValue_Default()
        {
            Dictionary<string, string?> settings = new()
            {
                ["key"] = "invalid",
            };
            string key = "key";
            int defaultValue = -1;

            int actual = BaseExecutionContext.GetInt32Setting(settings, key, defaultValue);
            Assert.Equal(-1, actual);
        }

        [Fact]
        public void GetInt32Setting_ValidValue_Parsed()
        {
            int expected = 12345;
            Dictionary<string, string?> settings = new()
            {
                ["key"] = "12345",
            };
            string key = "key";
            int defaultValue = -1;

            int actual = BaseExecutionContext.GetInt32Setting(settings, key, defaultValue);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetStringSetting

        [Fact]
        public void GetStringSetting_InvalidDict_Default()
        {
            Dictionary<string, string?> settings = [];
            string key = "key";
            string? defaultValue = null;

            string? actual = BaseExecutionContext.GetStringSetting(settings, key, defaultValue);
            Assert.Null(actual);
        }

        [Fact]
        public void GetStringSetting_InvalidKey_Default()
        {
            Dictionary<string, string?> settings = new()
            {
                ["key2"] = "12345",
            };
            string key = "key";
            string? defaultValue = null;

            string? actual = BaseExecutionContext.GetStringSetting(settings, key, defaultValue);
            Assert.Null(actual);
        }

        [Fact]
        public void GetStringSetting_ValidValue_Rturned()
        {
            string expected = "expected";
            Dictionary<string, string?> settings = new()
            {
                ["key"] = "expected",
            };
            string key = "key";
            string? defaultValue = null;

            string? actual = BaseExecutionContext.GetStringSetting(settings, key, defaultValue);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region SplitParameterString

        [Fact]
        public void SplitParameterString_Empty_Empty()
        {
            string parameters = string.Empty;
            string[] actual = BaseExecutionContext.SplitParameterString(parameters);
            Assert.Empty(actual);
        }

        [Fact]
        public void SplitParameterString_NoSplit_Single()
        {
            string expected = "single";
            string parameters = "single";
            string[] actual = BaseExecutionContext.SplitParameterString(parameters);

            var p0 = Assert.Single(actual);
            Assert.Equal(expected, p0);
        }

        [Fact]
        public void SplitParameterString_SplitNoEquals_Multiple()
        {
            string[] expected = ["-flag1", "value1", "-flag2"];
            string parameters = "-flag1 value1 -flag2";
            string[] actual = BaseExecutionContext.SplitParameterString(parameters);
            Assert.True(expected.SequenceEqual(actual));
        }

        [Fact]
        public void SplitParameterString_SplitEquals_Multiple()
        {
            string[] expected = ["-flag1=value1", "-flag2"];
            string parameters = "-flag1=value1 -flag2";
            string[] actual = BaseExecutionContext.SplitParameterString(parameters);
            Assert.True(expected.SequenceEqual(actual));
        }

        [Fact]
        public void SplitParameterString_SplitNoEqualsQuotes_Multiple()
        {
            string[] expected = ["-flag1", "\"value1 value2\"", "-flag2"];
            string parameters = "-flag1 \"value1 value2\" -flag2";
            string[] actual = BaseExecutionContext.SplitParameterString(parameters);
            Assert.True(expected.SequenceEqual(actual));
        }

        [Fact]
        public void SplitParameterString_SplitEqualsQuotes_Multiple()
        {
            string[] expected = ["-flag1=\"value1 value2\"", "-flag2"];
            string parameters = "-flag1=\"value1 value2\" -flag2";
            string[] actual = BaseExecutionContext.SplitParameterString(parameters);
            Assert.True(expected.SequenceEqual(actual));
        }

        #endregion

        #region DoesExist

        [Fact]
        public void DoesExist_Empty_False()
        {
            string[] parts = [];
            int index = 0;
            bool actual = BaseExecutionContext.DoesExist(parts, index);
            Assert.False(actual);
        }

        [Fact]
        public void DoesExist_Negative_False()
        {
            string[] parts = ["item"];
            int index = -1;
            bool actual = BaseExecutionContext.DoesExist(parts, index);
            Assert.False(actual);
        }

        [Fact]
        public void DoesExist_Greater_False()
        {
            string[] parts = ["item"];
            int index = 1;
            bool actual = BaseExecutionContext.DoesExist(parts, index);
            Assert.False(actual);
        }

        [Fact]
        public void DoesExist_Valid_True()
        {
            string[] parts = ["item"];
            int index = 0;
            bool actual = BaseExecutionContext.DoesExist(parts, index);
            Assert.True(actual);
        }

        #endregion

        #region IsValidBool

        [Theory]
        [InlineData("", false)]
        [InlineData("true", true)]
        [InlineData("True", true)]
        [InlineData("TRUE", true)]
        [InlineData("Yes", false)]
        [InlineData("false", true)]
        [InlineData("False", true)]
        [InlineData("FALSE", true)]
        [InlineData("No", false)]
        [InlineData("Invalid", false)]
        public void IsValidBoolTest(string parameter, bool expected)
        {
            bool actual = BaseExecutionContext.IsValidBool(parameter);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region IsValidInt8

        [Theory]
        [InlineData("", null, null, false)]
        [InlineData("", (sbyte)0, (sbyte)1, false)]
        [InlineData("", (sbyte)0, sbyte.MaxValue, false)]
        [InlineData("-2", null, null, true)]
        [InlineData("-2", (sbyte)0, (sbyte)1, false)]
        [InlineData("-2", (sbyte)0, sbyte.MaxValue, false)]
        [InlineData("0", null, null, true)]
        [InlineData("0", (sbyte)0, (sbyte)1, true)]
        [InlineData("0", (sbyte)0, sbyte.MaxValue, true)]
        [InlineData("2", null, null, true)]
        [InlineData("2", (sbyte)0, (sbyte)1, false)]
        [InlineData("2", (sbyte)0, sbyte.MaxValue, true)]
        [InlineData("Invalid", null, null, false)]
        [InlineData("Invalid", (sbyte)0, (sbyte)1, false)]
        [InlineData("Invalid", (sbyte)0, sbyte.MaxValue, false)]
        public void IsValidInt8Test(string parameter, sbyte? lowerBound, sbyte? upperBound, bool expected)
        {
            bool actual = BaseExecutionContext.IsValidInt8(parameter, lowerBound, upperBound);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region IsValidInt16

        [Theory]
        [InlineData("", null, null, false)]
        [InlineData("", (short)0, (short)1, false)]
        [InlineData("", (short)0, short.MaxValue, false)]
        [InlineData("-2", null, null, true)]
        [InlineData("-2", (short)0, (short)1, false)]
        [InlineData("-2", (short)0, short.MaxValue, false)]
        [InlineData("0", null, null, true)]
        [InlineData("0", (short)0, (short)1, true)]
        [InlineData("0", (short)0, short.MaxValue, true)]
        [InlineData("2", null, null, true)]
        [InlineData("2", (short)0, (short)1, false)]
        [InlineData("2", (short)0, short.MaxValue, true)]
        [InlineData("Invalid", null, null, false)]
        [InlineData("Invalid", (short)0, (short)1, false)]
        [InlineData("Invalid", (short)0, short.MaxValue, false)]
        public void IsValidInt16Test(string parameter, short? lowerBound, short? upperBound, bool expected)
        {
            bool actual = BaseExecutionContext.IsValidInt16(parameter, lowerBound, upperBound);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region IsValidInt32

        [Theory]
        [InlineData("", null, null, false)]
        [InlineData("", (int)0, (int)1, false)]
        [InlineData("", (int)0, int.MaxValue, false)]
        [InlineData("-2", null, null, true)]
        [InlineData("-2", (int)0, (int)1, false)]
        [InlineData("-2", (int)0, int.MaxValue, false)]
        [InlineData("0", null, null, true)]
        [InlineData("0", (int)0, (int)1, true)]
        [InlineData("0", (int)0, int.MaxValue, true)]
        [InlineData("2", null, null, true)]
        [InlineData("2", (int)0, (int)1, false)]
        [InlineData("2", (int)0, int.MaxValue, true)]
        [InlineData("Invalid", null, null, false)]
        [InlineData("Invalid", (int)0, (int)1, false)]
        [InlineData("Invalid", (int)0, int.MaxValue, false)]
        public void IsValidInt32Test(string parameter, int? lowerBound, int? upperBound, bool expected)
        {
            bool actual = BaseExecutionContext.IsValidInt32(parameter, lowerBound, upperBound);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region IsValidInt64

        [Theory]
        [InlineData("", null, null, false)]
        [InlineData("", (long)0, (long)1, false)]
        [InlineData("", (long)0, long.MaxValue, false)]
        [InlineData("-2", null, null, true)]
        [InlineData("-2", (long)0, (long)1, false)]
        [InlineData("-2", (long)0, long.MaxValue, false)]
        [InlineData("0", null, null, true)]
        [InlineData("0", (long)0, (long)1, true)]
        [InlineData("0", (long)0, long.MaxValue, true)]
        [InlineData("2", null, null, true)]
        [InlineData("2", (long)0, (long)1, false)]
        [InlineData("2", (long)0, long.MaxValue, true)]
        [InlineData("Invalid", null, null, false)]
        [InlineData("Invalid", (long)0, (long)1, false)]
        [InlineData("Invalid", (long)0, long.MaxValue, false)]
        public void IsValidInt64Test(string parameter, long? lowerBound, long? upperBound, bool expected)
        {
            bool actual = BaseExecutionContext.IsValidInt64(parameter, lowerBound, upperBound);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region ExtractFactorFromValue

        [Theory]
        [InlineData("1", "1", 1)]
        [InlineData("1c", "1", 1)]
        [InlineData("1w", "1", 2)]
        [InlineData("1d", "1", 4)]
        [InlineData("1q", "1", 8)]
        [InlineData("1k", "1", 1024)]
        [InlineData("1M", "1", 1024 * 1024)]
        [InlineData("1G", "1", 1024 * 1024 * 1024)]
        public void ExtractFactorFromValueTest(string value, string expected, long expectedFactor)
        {
            string actual = BaseExecutionContext.ExtractFactorFromValue(value, out long factor);
            Assert.Equal(expected, actual);
            Assert.Equal(expectedFactor, factor);
        }

        #endregion

        #region RemoveHexIdentifier

        [Theory]
        [InlineData("", "")]
        [InlineData("0", "0")]
        [InlineData("00", "00")]
        [InlineData("0x", "0x")]
        [InlineData("0X", "0X")]
        [InlineData("A", "A")]
        [InlineData("A0", "A0")]
        [InlineData("Ax", "Ax")]
        [InlineData("AX", "AX")]
        [InlineData("012345", "012345")]
        [InlineData("0x12345", "12345")]
        [InlineData("0X12345", "12345")]
        public void RemoveHexIdentifierTest(string value, string expected)
        {
            string actual = BaseExecutionContext.RemoveHexIdentifier(value);
            Assert.Equal(expected, actual);
        }

        #endregion
    }
}