using Xunit;

namespace MPF.ExecutionContexts.Test
{
    public class InputTests
    {
        #region ProcessAsFlag

        [Theory]
        // Invalid type
        [InlineData(InputType.None, "flag", new string[] { "flag" }, 0, false)]
        [InlineData(InputType.Boolean, "flag", new string[] { "flag" }, 0, false)]
        [InlineData(InputType.Int8, "flag", new string[] { "flag" }, 0, false)]
        [InlineData(InputType.UInt8, "flag", new string[] { "flag" }, 0, false)]
        [InlineData(InputType.Int16, "flag", new string[] { "flag" }, 0, false)]
        [InlineData(InputType.UInt16, "flag", new string[] { "flag" }, 0, false)]
        [InlineData(InputType.Int32, "flag", new string[] { "flag" }, 0, false)]
        [InlineData(InputType.UInt32, "flag", new string[] { "flag" }, 0, false)]
        [InlineData(InputType.Int64, "flag", new string[] { "flag" }, 0, false)]
        [InlineData(InputType.UInt64, "flag", new string[] { "flag" }, 0, false)]
        [InlineData(InputType.String, "flag", new string[] { "flag" }, 0, false)]
        // Invalid parts
        [InlineData(InputType.Flag, "flag", new string[0], 0, false)]
        // Invalid index
        [InlineData(InputType.Flag, "flag", new string[] { "flag" }, -1, false)]
        [InlineData(InputType.Flag, "flag", new string[] { "flag" }, 1, false)]
        // Invalid name
        [InlineData(InputType.Flag, "flag", new string[] { "" }, 0, false)]
        [InlineData(InputType.Flag, "flag", new string[] { "flag2" }, 0, false)]
        // Valid
        [InlineData(InputType.Flag, "flag", new string[] { "flag" }, 0, true)]
        public void ProcessAsFlagTest(InputType type, string name, string[] parts, int index, bool expected)
        {
            Input input = new Input(type, name);
            bool actual = input.ProcessAsFlag(parts, ref index);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region ProcessAsBoolean

        [Theory]
        // Invalid type
        [InlineData(InputType.None, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Flag, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Int8, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.UInt8, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Int16, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.UInt16, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Int32, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.UInt32, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Int64, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.UInt64, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.String, "flag", true, new string[] { "flag" }, 0, null)]
        // Invalid parts
        [InlineData(InputType.Boolean, "flag", true, new string[0], 0, null)]
        // Invalid index
        [InlineData(InputType.Boolean, "flag", true, new string[] { "flag" }, -1, null)]
        [InlineData(InputType.Boolean, "flag", true, new string[] { "flag" }, 1, null)]
        // Invalid name
        [InlineData(InputType.Boolean, "flag", true, new string[] { "" }, 0, null)]
        [InlineData(InputType.Boolean, "flag", true, new string[] { "flag2" }, 0, null)]
        // Valid name, no following
        [InlineData(InputType.Boolean, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Boolean, "flag", false, new string[] { "flag" }, 0, true)]
        // Valid name, invalid following
        [InlineData(InputType.Boolean, "flag", true, new string[] { "flag", "invalid" }, 0, null)]
        [InlineData(InputType.Boolean, "flag", false, new string[] { "flag", "invalid" }, 0, true)]
        // Valid name, valid following
        [InlineData(InputType.Boolean, "flag", true, new string[] { "flag", "true" }, 0, true)]
        [InlineData(InputType.Boolean, "flag", true, new string[] { "flag", "false" }, 0, false)]
        public void ProcessAsBooleanTest(InputType type, string name, bool required, string[] parts, int index, bool? expected)
        {
            Input input = new Input(type, name, required);
            bool? actual = input.ProcessAsBoolean(parts, ref index);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region ProcessAsInt8

        [Theory]
        // Invalid type
        [InlineData(InputType.None, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Flag, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Boolean, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.UInt8, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Int16, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.UInt16, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Int32, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.UInt32, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Int64, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.UInt64, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.String, "flag", true, new string[] { "flag" }, 0, null)]
        // Invalid parts
        [InlineData(InputType.Int8, "flag", true, new string[0], 0, null)]
        // Invalid index
        [InlineData(InputType.Int8, "flag", true, new string[] { "flag" }, -1, null)]
        [InlineData(InputType.Int8, "flag", true, new string[] { "flag" }, 1, null)]
        // Invalid name
        [InlineData(InputType.Int8, "flag", true, new string[] { "" }, 0, null)]
        [InlineData(InputType.Int8, "flag", true, new string[] { "flag2" }, 0, null)]
        // Valid name, no following
        [InlineData(InputType.Int8, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Int8, "flag", false, new string[] { "flag" }, 0, sbyte.MinValue)]
        // Valid name, invalid following
        [InlineData(InputType.Int8, "flag", true, new string[] { "flag", "invalid" }, 0, null)]
        [InlineData(InputType.Int8, "flag", false, new string[] { "flag", "invalid" }, 0, sbyte.MinValue)]
        // Valid name, valid following
        [InlineData(InputType.Int8, "flag", true, new string[] { "flag", "1" }, 0, (sbyte)1)]
        [InlineData(InputType.Int8, "flag", true, new string[] { "flag", "-1" }, 0, (sbyte)-1)]
        public void ProcessAsInt8Test(InputType type, string name, bool required, string[] parts, int index, sbyte? expected)
        {
            Input input = new Input(type, name, required);
            sbyte? actual = input.ProcessAsInt8(parts, ref index);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region ProcessAsUInt8

        [Theory]
        // Invalid type
        [InlineData(InputType.None, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Flag, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Boolean, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Int8, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Int16, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.UInt16, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Int32, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.UInt32, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Int64, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.UInt64, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.String, "flag", true, new string[] { "flag" }, 0, null)]
        // Invalid parts
        [InlineData(InputType.UInt8, "flag", true, new string[0], 0, null)]
        // Invalid index
        [InlineData(InputType.UInt8, "flag", true, new string[] { "flag" }, -1, null)]
        [InlineData(InputType.UInt8, "flag", true, new string[] { "flag" }, 1, null)]
        // Invalid name
        [InlineData(InputType.UInt8, "flag", true, new string[] { "" }, 0, null)]
        [InlineData(InputType.UInt8, "flag", true, new string[] { "flag2" }, 0, null)]
        // Valid name, no following
        [InlineData(InputType.UInt8, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.UInt8, "flag", false, new string[] { "flag" }, 0, byte.MinValue)]
        // Valid name, invalid following
        [InlineData(InputType.UInt8, "flag", true, new string[] { "flag", "invalid" }, 0, null)]
        [InlineData(InputType.UInt8, "flag", false, new string[] { "flag", "invalid" }, 0, byte.MinValue)]
        // Valid name, valid following
        [InlineData(InputType.UInt8, "flag", true, new string[] { "flag", "1" }, 0, (byte)1)]
        public void ProcessAsUInt8Test(InputType type, string name, bool required, string[] parts, int index, byte? expected)
        {
            Input input = new Input(type, name, required);
            byte? actual = input.ProcessAsUInt8(parts, ref index);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region ProcessAsInt16

        [Theory]
        // Invalid type
        [InlineData(InputType.None, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Flag, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Boolean, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Int8, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.UInt8, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.UInt16, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Int32, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.UInt32, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Int64, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.UInt64, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.String, "flag", true, new string[] { "flag" }, 0, null)]
        // Invalid parts
        [InlineData(InputType.Int16, "flag", true, new string[0], 0, null)]
        // Invalid index
        [InlineData(InputType.Int16, "flag", true, new string[] { "flag" }, -1, null)]
        [InlineData(InputType.Int16, "flag", true, new string[] { "flag" }, 1, null)]
        // Invalid name
        [InlineData(InputType.Int16, "flag", true, new string[] { "" }, 0, null)]
        [InlineData(InputType.Int16, "flag", true, new string[] { "flag2" }, 0, null)]
        // Valid name, no following
        [InlineData(InputType.Int16, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Int16, "flag", false, new string[] { "flag" }, 0, short.MinValue)]
        // Valid name, invalid following
        [InlineData(InputType.Int16, "flag", true, new string[] { "flag", "invalid" }, 0, null)]
        [InlineData(InputType.Int16, "flag", false, new string[] { "flag", "invalid" }, 0, short.MinValue)]
        // Valid name, valid following
        [InlineData(InputType.Int16, "flag", true, new string[] { "flag", "1" }, 0, (short)1)]
        [InlineData(InputType.Int16, "flag", true, new string[] { "flag", "-1" }, 0, (short)-1)]
        public void ProcessAsInt16Test(InputType type, string name, bool required, string[] parts, int index, short? expected)
        {
            Input input = new Input(type, name, required);
            short? actual = input.ProcessAsInt16(parts, ref index);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region ProcessAsUInt16

        [Theory]
        // Invalid type
        [InlineData(InputType.None, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Flag, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Boolean, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Int8, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.UInt8, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Int16, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Int32, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.UInt32, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Int64, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.UInt64, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.String, "flag", true, new string[] { "flag" }, 0, null)]
        // Invalid parts
        [InlineData(InputType.UInt16, "flag", true, new string[0], 0, null)]
        // Invalid index
        [InlineData(InputType.UInt16, "flag", true, new string[] { "flag" }, -1, null)]
        [InlineData(InputType.UInt16, "flag", true, new string[] { "flag" }, 1, null)]
        // Invalid name
        [InlineData(InputType.UInt16, "flag", true, new string[] { "" }, 0, null)]
        [InlineData(InputType.UInt16, "flag", true, new string[] { "flag2" }, 0, null)]
        // Valid name, no following
        [InlineData(InputType.UInt16, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.UInt16, "flag", false, new string[] { "flag" }, 0, ushort.MinValue)]
        // Valid name, invalid following
        [InlineData(InputType.UInt16, "flag", true, new string[] { "flag", "invalid" }, 0, null)]
        [InlineData(InputType.UInt16, "flag", false, new string[] { "flag", "invalid" }, 0, ushort.MinValue)]
        // Valid name, valid following
        [InlineData(InputType.UInt16, "flag", true, new string[] { "flag", "1" }, 0, (ushort)1)]
        public void ProcessAsUInt16Test(InputType type, string name, bool required, string[] parts, int index, ushort? expected)
        {
            Input input = new Input(type, name, required);
            ushort? actual = input.ProcessAsUInt16(parts, ref index);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region ProcessAsInt32

        [Theory]
        // Invalid type
        [InlineData(InputType.None, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Flag, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Boolean, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Int8, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.UInt8, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Int16, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.UInt16, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.UInt32, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Int64, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.UInt64, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.String, "flag", true, new string[] { "flag" }, 0, null)]
        // Invalid parts
        [InlineData(InputType.Int32, "flag", true, new string[0], 0, null)]
        // Invalid index
        [InlineData(InputType.Int32, "flag", true, new string[] { "flag" }, -1, null)]
        [InlineData(InputType.Int32, "flag", true, new string[] { "flag" }, 1, null)]
        // Invalid name
        [InlineData(InputType.Int32, "flag", true, new string[] { "" }, 0, null)]
        [InlineData(InputType.Int32, "flag", true, new string[] { "flag2" }, 0, null)]
        // Valid name, no following
        [InlineData(InputType.Int32, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Int32, "flag", false, new string[] { "flag" }, 0, int.MinValue)]
        // Valid name, invalid following
        [InlineData(InputType.Int32, "flag", true, new string[] { "flag", "invalid" }, 0, null)]
        [InlineData(InputType.Int32, "flag", false, new string[] { "flag", "invalid" }, 0, int.MinValue)]
        // Valid name, valid following
        [InlineData(InputType.Int32, "flag", true, new string[] { "flag", "1" }, 0, (int)1)]
        [InlineData(InputType.Int32, "flag", true, new string[] { "flag", "-1" }, 0, (int)-1)]
        public void ProcessAsInt32Test(InputType type, string name, bool required, string[] parts, int index, int? expected)
        {
            Input input = new Input(type, name, required);
            int? actual = input.ProcessAsInt32(parts, ref index);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region ProcessAsUInt32

        [Theory]
        // Invalid type
        [InlineData(InputType.None, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Flag, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Boolean, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Int8, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.UInt8, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Int16, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.UInt16, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Int32, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Int64, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.UInt64, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.String, "flag", true, new string[] { "flag" }, 0, null)]
        // Invalid parts
        [InlineData(InputType.UInt32, "flag", true, new string[0], 0, null)]
        // Invalid index
        [InlineData(InputType.UInt32, "flag", true, new string[] { "flag" }, -1, null)]
        [InlineData(InputType.UInt32, "flag", true, new string[] { "flag" }, 1, null)]
        // Invalid name
        [InlineData(InputType.UInt32, "flag", true, new string[] { "" }, 0, null)]
        [InlineData(InputType.UInt32, "flag", true, new string[] { "flag2" }, 0, null)]
        // Valid name, no following
        [InlineData(InputType.UInt32, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.UInt32, "flag", false, new string[] { "flag" }, 0, uint.MinValue)]
        // Valid name, invalid following
        [InlineData(InputType.UInt32, "flag", true, new string[] { "flag", "invalid" }, 0, null)]
        [InlineData(InputType.UInt32, "flag", false, new string[] { "flag", "invalid" }, 0, uint.MinValue)]
        // Valid name, valid following
        [InlineData(InputType.UInt32, "flag", true, new string[] { "flag", "1" }, 0, (uint)1)]
        public void ProcessAsUInt32Test(InputType type, string name, bool required, string[] parts, int index, uint? expected)
        {
            Input input = new Input(type, name, required);
            uint? actual = input.ProcessAsUInt32(parts, ref index);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region ProcessAsInt64

        [Theory]
        // Invalid type
        [InlineData(InputType.None, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Flag, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Boolean, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Int8, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.UInt8, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Int16, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.UInt16, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Int32, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.UInt32, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.UInt64, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.String, "flag", true, new string[] { "flag" }, 0, null)]
        // Invalid parts
        [InlineData(InputType.Int64, "flag", true, new string[0], 0, null)]
        // Invalid index
        [InlineData(InputType.Int64, "flag", true, new string[] { "flag" }, -1, null)]
        [InlineData(InputType.Int64, "flag", true, new string[] { "flag" }, 1, null)]
        // Invalid name
        [InlineData(InputType.Int64, "flag", true, new string[] { "" }, 0, null)]
        [InlineData(InputType.Int64, "flag", true, new string[] { "flag2" }, 0, null)]
        // Valid name, no following
        [InlineData(InputType.Int64, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Int64, "flag", false, new string[] { "flag" }, 0, long.MinValue)]
        // Valid name, invalid following
        [InlineData(InputType.Int64, "flag", true, new string[] { "flag", "invalid" }, 0, null)]
        [InlineData(InputType.Int64, "flag", false, new string[] { "flag", "invalid" }, 0, long.MinValue)]
        // Valid name, valid following
        [InlineData(InputType.Int64, "flag", true, new string[] { "flag", "1" }, 0, (long)1)]
        [InlineData(InputType.Int64, "flag", true, new string[] { "flag", "-1" }, 0, (long)-1)]
        public void ProcessAsInt64Test(InputType type, string name, bool required, string[] parts, int index, long? expected)
        {
            Input input = new Input(type, name, required);
            long? actual = input.ProcessAsInt64(parts, ref index);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region ProcessAsUInt64

        [Theory]
        // Invalid type
        [InlineData(InputType.None, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Flag, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Boolean, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Int8, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.UInt8, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Int16, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.UInt16, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Int32, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.UInt32, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Int64, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.String, "flag", true, new string[] { "flag" }, 0, null)]
        // Invalid parts
        [InlineData(InputType.UInt64, "flag", true, new string[0], 0, null)]
        // Invalid index
        [InlineData(InputType.UInt64, "flag", true, new string[] { "flag" }, -1, null)]
        [InlineData(InputType.UInt64, "flag", true, new string[] { "flag" }, 1, null)]
        // Invalid name
        [InlineData(InputType.UInt64, "flag", true, new string[] { "" }, 0, null)]
        [InlineData(InputType.UInt64, "flag", true, new string[] { "flag2" }, 0, null)]
        // Valid name, no following
        [InlineData(InputType.UInt64, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.UInt64, "flag", false, new string[] { "flag" }, 0, ulong.MinValue)]
        // Valid name, invalid following
        [InlineData(InputType.UInt64, "flag", true, new string[] { "flag", "invalid" }, 0, null)]
        [InlineData(InputType.UInt64, "flag", false, new string[] { "flag", "invalid" }, 0, ulong.MinValue)]
        // Valid name, valid following
        [InlineData(InputType.UInt64, "flag", true, new string[] { "flag", "1" }, 0, (ulong)1)]
        public void ProcessAsUInt64Test(InputType type, string name, bool required, string[] parts, int index, ulong? expected)
        {
            Input input = new Input(type, name, required);
            ulong? actual = input.ProcessAsUInt64(parts, ref index);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region ProcessAsString

        [Theory]
        // Invalid type
        [InlineData(InputType.None, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Flag, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Boolean, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Int8, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.UInt8, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Int16, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.UInt16, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Int32, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.UInt32, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.Int64, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.UInt64, "flag", true, new string[] { "flag" }, 0, null)]
        // Invalid parts
        [InlineData(InputType.String, "flag", true, new string[0], 0, null)]
        // Invalid index
        [InlineData(InputType.String, "flag", true, new string[] { "flag" }, -1, null)]
        [InlineData(InputType.String, "flag", true, new string[] { "flag" }, 1, null)]
        // Invalid name
        [InlineData(InputType.String, "flag", true, new string[] { "" }, 0, null)]
        [InlineData(InputType.String, "flag", true, new string[] { "flag2" }, 0, null)]
        // Valid name, no following
        [InlineData(InputType.String, "flag", true, new string[] { "flag" }, 0, null)]
        [InlineData(InputType.String, "flag", false, new string[] { "flag" }, 0, "")]
        // Valid name, following
        [InlineData(InputType.String, "flag", true, new string[] { "flag", "value" }, 0, "value")]
        public void ProcessAsString(InputType type, string name, bool required, string[] parts, int index, string? expected)
        {
            Input input = new Input(type, name, required);
            string? actual = input.ProcessAsString(parts, ref index);
            Assert.Equal(expected, actual);
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