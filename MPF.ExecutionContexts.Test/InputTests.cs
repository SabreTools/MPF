using MPF.ExecutionContexts.Data;
using Xunit;

namespace MPF.ExecutionContexts.Test
{
    public class InputTests
    {
        #region FlagInput

        [Theory]
        // Invalid parts
        [InlineData("flag", new string[0], 0, false, false)]
        // Invalid index
        [InlineData("flag", new string[] { "flag" }, -1, false, false)]
        [InlineData("flag", new string[] { "flag" }, 1, false, false)]
        // Invalid name
        [InlineData("flag", new string[] { "" }, 0, false, false)]
        [InlineData("flag", new string[] { "flag2" }, 0, false, false)]
        // Valid
        [InlineData("flag", new string[] { "flag" }, 0, true, true)]
        public void FlagInputTest(string name, string[] parts, int index, bool success, bool expected)
        {
            FlagInput input = new FlagInput(name);
            bool actual = input.Process(parts, ref index);

            Assert.Equal(success, actual);
            Assert.Equal(expected, input.Value);
        }

        #endregion

        #region BooleanInput

        [Theory]
        // Invalid parts
        [InlineData("flag", true, new string[0], 0, false, null)]
        // Invalid index
        [InlineData("flag", true, new string[] { "flag" }, -1, false, null)]
        [InlineData("flag", true, new string[] { "flag" }, 1, false, null)]
        // Invalid name
        [InlineData("flag", true, new string[] { "" }, 0, false, null)]
        [InlineData("flag", true, new string[] { "flag2" }, 0, false, null)]
        // Valid name, no following
        [InlineData("flag", true, new string[] { "flag" }, 0, false, null)]
        [InlineData("flag", false, new string[] { "flag" }, 0, true, true)]
        // Valid name, invalid following
        [InlineData("flag", true, new string[] { "flag", "invalid" }, 0, false, null)]
        [InlineData("flag", false, new string[] { "flag", "invalid" }, 0, true, true)]
        // Valid name, valid following
        [InlineData("flag", true, new string[] { "flag", "true" }, 0, true, true)]
        [InlineData("flag", true, new string[] { "flag", "false" }, 0, true, false)]
        public void BooleanInputTest(string name, bool required, string[] parts, int index, bool success, bool? expected)
        {
            BooleanInput input = new BooleanInput(name, required);
            bool actual = input.Process(parts, ref index);

            Assert.Equal(success, actual);
            Assert.Equal(expected, input.Value);
        }

        #endregion

        #region Int8Input

        [Theory]
        // Invalid parts
        [InlineData("flag", true, new string[0], 0, false, null)]
        // Invalid index
        [InlineData("flag", true, new string[] { "flag" }, -1, false, null)]
        [InlineData("flag", true, new string[] { "flag" }, 1, false, null)]
        // Invalid name
        [InlineData("flag", true, new string[] { "" }, 0, false, null)]
        [InlineData("flag", true, new string[] { "flag2" }, 0, false, null)]
        // Valid name, no following
        [InlineData("flag", true, new string[] { "flag" }, 0, false, null)]
        [InlineData("flag", false, new string[] { "flag" }, 0, true, sbyte.MinValue)]
        // Valid name, invalid following
        [InlineData("flag", true, new string[] { "flag", "invalid" }, 0, false, null)]
        [InlineData("flag", false, new string[] { "flag", "invalid" }, 0, true, sbyte.MinValue)]
        // Valid name, valid following
        [InlineData("flag", true, new string[] { "flag", "1" }, 0, true, (sbyte)1)]
        [InlineData("flag", true, new string[] { "flag", "-1" }, 0, true, (sbyte)-1)]
        public void Int8InputTest(string name, bool required, string[] parts, int index, bool success, sbyte? expected)
        {
            Int8Input input = new Int8Input(name, required);
            bool actual = input.Process(parts, ref index);

            Assert.Equal(success, actual);
            Assert.Equal(expected, input.Value);
        }

        #endregion

        #region UInt8Input

        [Theory]
        // Invalid parts
        [InlineData("flag", true, new string[0], 0, false, null)]
        // Invalid index
        [InlineData("flag", true, new string[] { "flag" }, -1, false, null)]
        [InlineData("flag", true, new string[] { "flag" }, 1, false, null)]
        // Invalid name
        [InlineData("flag", true, new string[] { "" }, 0, false, null)]
        [InlineData("flag", true, new string[] { "flag2" }, 0, false, null)]
        // Valid name, no following
        [InlineData("flag", true, new string[] { "flag" }, 0, false, null)]
        [InlineData("flag", false, new string[] { "flag" }, 0, true, byte.MinValue)]
        // Valid name, invalid following
        [InlineData("flag", true, new string[] { "flag", "invalid" }, 0, false, null)]
        [InlineData("flag", false, new string[] { "flag", "invalid" }, 0, true, byte.MinValue)]
        // Valid name, valid following
        [InlineData("flag", true, new string[] { "flag", "1" }, 0, true, (byte)1)]
        public void UInt8InputTest(string name, bool required, string[] parts, int index, bool success, byte? expected)
        {
            UInt8Input input = new UInt8Input(name, required);
            bool actual = input.Process(parts, ref index);

            Assert.Equal(success, actual);
            Assert.Equal(expected, input.Value);
        }

        #endregion

        #region Int16Input

        [Theory]
        // Invalid parts
        [InlineData("flag", true, new string[0], 0, false, null)]
        // Invalid index
        [InlineData("flag", true, new string[] { "flag" }, -1, false, null)]
        [InlineData("flag", true, new string[] { "flag" }, 1, false, null)]
        // Invalid name
        [InlineData("flag", true, new string[] { "" }, 0, false, null)]
        [InlineData("flag", true, new string[] { "flag2" }, 0, false, null)]
        // Valid name, no following
        [InlineData("flag", true, new string[] { "flag" }, 0, false, null)]
        [InlineData("flag", false, new string[] { "flag" }, 0, true, short.MinValue)]
        // Valid name, invalid following
        [InlineData("flag", true, new string[] { "flag", "invalid" }, 0, false, null)]
        [InlineData("flag", false, new string[] { "flag", "invalid" }, 0, true, short.MinValue)]
        // Valid name, valid following
        [InlineData("flag", true, new string[] { "flag", "1" }, 0, true, (short)1)]
        [InlineData("flag", true, new string[] { "flag", "-1" }, 0, true, (short)-1)]
        public void Int16InputTest(string name, bool required, string[] parts, int index, bool success, short? expected)
        {
            Int16Input input = new Int16Input(name, required);
            bool actual = input.Process(parts, ref index);

            Assert.Equal(success, actual);
            Assert.Equal(expected, input.Value);
        }

        #endregion

        #region UInt16Input

        [Theory]
        // Invalid parts
        [InlineData("flag", true, new string[0], 0, false, null)]
        // Invalid index
        [InlineData("flag", true, new string[] { "flag" }, -1, false, null)]
        [InlineData("flag", true, new string[] { "flag" }, 1, false, null)]
        // Invalid name
        [InlineData("flag", true, new string[] { "" }, 0, false, null)]
        [InlineData("flag", true, new string[] { "flag2" }, 0, false, null)]
        // Valid name, no following
        [InlineData("flag", true, new string[] { "flag" }, 0, false, null)]
        [InlineData("flag", false, new string[] { "flag" }, 0, true, ushort.MinValue)]
        // Valid name, invalid following
        [InlineData("flag", true, new string[] { "flag", "invalid" }, 0, false, null)]
        [InlineData("flag", false, new string[] { "flag", "invalid" }, 0, true, ushort.MinValue)]
        // Valid name, valid following
        [InlineData("flag", true, new string[] { "flag", "1" }, 0, true, (ushort)1)]
        public void UInt16InputTest(string name, bool required, string[] parts, int index, bool success, ushort? expected)
        {
            UInt16Input input = new UInt16Input(name, required);
            bool actual = input.Process(parts, ref index);

            Assert.Equal(success, actual);
            Assert.Equal(expected, input.Value);
        }

        #endregion

        #region Int32Input

        [Theory]
        // Invalid parts
        [InlineData("flag", true, new string[0], 0, false, null)]
        // Invalid index
        [InlineData("flag", true, new string[] { "flag" }, -1, false, null)]
        [InlineData("flag", true, new string[] { "flag" }, 1, false, null)]
        // Invalid name
        [InlineData("flag", true, new string[] { "" }, 0, false, null)]
        [InlineData("flag", true, new string[] { "flag2" }, 0, false, null)]
        // Valid name, no following
        [InlineData("flag", true, new string[] { "flag" }, 0, false, null)]
        [InlineData("flag", false, new string[] { "flag" }, 0, true, int.MinValue)]
        // Valid name, invalid following
        [InlineData("flag", true, new string[] { "flag", "invalid" }, 0, false, null)]
        [InlineData("flag", false, new string[] { "flag", "invalid" }, 0, true, int.MinValue)]
        // Valid name, valid following
        [InlineData("flag", true, new string[] { "flag", "1" }, 0, true, (int)1)]
        [InlineData("flag", true, new string[] { "flag", "-1" }, 0, true, (int)-1)]
        public void Int32InputTest(string name, bool required, string[] parts, int index, bool success, int? expected)
        {
            Int32Input input = new Int32Input(name, required);
            bool actual = input.Process(parts, ref index);

            Assert.Equal(success, actual);
            Assert.Equal(expected, input.Value);
        }

        #endregion

        #region UInt32Input

        [Theory]
        // Invalid parts
        [InlineData("flag", true, new string[0], 0, false, null)]
        // Invalid index
        [InlineData("flag", true, new string[] { "flag" }, -1, false, null)]
        [InlineData("flag", true, new string[] { "flag" }, 1, false, null)]
        // Invalid name
        [InlineData("flag", true, new string[] { "" }, 0, false, null)]
        [InlineData("flag", true, new string[] { "flag2" }, 0, false, null)]
        // Valid name, no following
        [InlineData("flag", true, new string[] { "flag" }, 0, false, null)]
        [InlineData("flag", false, new string[] { "flag" }, 0, true, uint.MinValue)]
        // Valid name, invalid following
        [InlineData("flag", true, new string[] { "flag", "invalid" }, 0, false, null)]
        [InlineData("flag", false, new string[] { "flag", "invalid" }, 0, true, uint.MinValue)]
        // Valid name, valid following
        [InlineData("flag", true, new string[] { "flag", "1" }, 0, true, (uint)1)]
        public void UInt32InputTest(string name, bool required, string[] parts, int index, bool success, uint? expected)
        {
            UInt32Input input = new UInt32Input(name, required);
            bool actual = input.Process(parts, ref index);

            Assert.Equal(success, actual);
            Assert.Equal(expected, input.Value);
        }

        #endregion

        #region Int64Input

        [Theory]
        // Invalid parts
        [InlineData("flag", true, new string[0], 0, false, null)]
        // Invalid index
        [InlineData("flag", true, new string[] { "flag" }, -1, false, null)]
        [InlineData("flag", true, new string[] { "flag" }, 1, false, null)]
        // Invalid name
        [InlineData("flag", true, new string[] { "" }, 0, false, null)]
        [InlineData("flag", true, new string[] { "flag2" }, 0, false, null)]
        // Valid name, no following
        [InlineData("flag", true, new string[] { "flag" }, 0, false, null)]
        [InlineData("flag", false, new string[] { "flag" }, 0, true, long.MinValue)]
        // Valid name, invalid following
        [InlineData("flag", true, new string[] { "flag", "invalid" }, 0, false, null)]
        [InlineData("flag", false, new string[] { "flag", "invalid" }, 0, true, long.MinValue)]
        // Valid name, valid following
        [InlineData("flag", true, new string[] { "flag", "1" }, 0, true, (long)1)]
        [InlineData("flag", true, new string[] { "flag", "-1" }, 0, true, (long)-1)]
        public void Int64InputTest(string name, bool required, string[] parts, int index, bool success, long? expected)
        {
            Int64Input input = new Int64Input(name, required);
            bool actual = input.Process(parts, ref index);

            Assert.Equal(success, actual);
            Assert.Equal(expected, input.Value);
        }

        #endregion

        #region UInt64Input

        [Theory]
        // Invalid parts
        [InlineData("flag", true, new string[0], 0, false, null)]
        // Invalid index
        [InlineData("flag", true, new string[] { "flag" }, -1, false, null)]
        [InlineData("flag", true, new string[] { "flag" }, 1, false, null)]
        // Invalid name
        [InlineData("flag", true, new string[] { "" }, 0, false, null)]
        [InlineData("flag", true, new string[] { "flag2" }, 0, false, null)]
        // Valid name, no following
        [InlineData("flag", true, new string[] { "flag" }, 0, false, null)]
        [InlineData("flag", false, new string[] { "flag" }, 0, true, ulong.MinValue)]
        // Valid name, invalid following
        [InlineData("flag", true, new string[] { "flag", "invalid" }, 0, false, null)]
        [InlineData("flag", false, new string[] { "flag", "invalid" }, 0, true, ulong.MinValue)]
        // Valid name, valid following
        [InlineData("flag", true, new string[] { "flag", "1" }, 0, true, (ulong)1)]
        public void UInt64InputTest(string name, bool required, string[] parts, int index, bool success, ulong? expected)
        {
            UInt64Input input = new UInt64Input(name, required);
            bool actual = input.Process(parts, ref index);

            Assert.Equal(success, actual);
            Assert.Equal(expected, input.Value);
        }

        #endregion

        #region StringInput

        [Theory]
        // Invalid parts
        [InlineData("flag", true, new string[0], 0, false, null)]
        // Invalid index
        [InlineData("flag", true, new string[] { "flag" }, -1, false, null)]
        [InlineData("flag", true, new string[] { "flag" }, 1, false, null)]
        // Invalid name
        [InlineData("flag", true, new string[] { "" }, 0, false, null)]
        [InlineData("flag", true, new string[] { "flag2" }, 0, false, null)]
        // Valid name, no following
        [InlineData("flag", true, new string[] { "flag" }, 0, false, null)]
        [InlineData("flag", false, new string[] { "flag" }, 0, true, "")]
        // Valid name, following
        [InlineData("flag", true, new string[] { "flag", "value" }, 0, true, "value")]
        public void StringInputTest(string name, bool required, string[] parts, int index, bool success, string? expected)
        {
            StringInput input = new StringInput(name, required);
            bool actual = input.Process(parts, ref index);

            Assert.Equal(success, actual);
            Assert.Equal(expected, input.Value);
        }

        #endregion

        #region DoesExist

        [Fact]
        public void DoesExist_Empty_False()
        {
            string[] parts = [];
            int index = 0;
            bool actual = Input.DoesExist(parts, index);
            Assert.False(actual);
        }

        [Fact]
        public void DoesExist_Negative_False()
        {
            string[] parts = ["item"];
            int index = -1;
            bool actual = Input.DoesExist(parts, index);
            Assert.False(actual);
        }

        [Fact]
        public void DoesExist_Greater_False()
        {
            string[] parts = ["item"];
            int index = 1;
            bool actual = Input.DoesExist(parts, index);
            Assert.False(actual);
        }

        [Fact]
        public void DoesExist_Valid_True()
        {
            string[] parts = ["item"];
            int index = 0;
            bool actual = Input.DoesExist(parts, index);
            Assert.True(actual);
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
            string actual = Input.ExtractFactorFromValue(value, out long factor);
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
            string actual = Input.RemoveHexIdentifier(value);
            Assert.Equal(expected, actual);
        }

        #endregion
    }
}