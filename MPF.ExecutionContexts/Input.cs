using System;
using System.Globalization;

namespace MPF.ExecutionContexts
{
    /// <summary>
    /// Indicates the input type
    /// </summary>
    public enum InputType
    {
        None,
        Flag,
        Boolean,
        Int8,
        UInt8,
        Int16,
        UInt16,
        Int32,
        UInt32,
        Int64,
        UInt64,
        String,
    }

    /// <summary>
    /// Represents a single input for an execution context
    /// </summary>
    public class Input
    {
        #region Properties

        /// <summary>
        /// Input type for parsing
        /// </summary>
        public readonly InputType InputType;

        /// <summary>
        /// Primary name for the input
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Verbose name for the input
        /// </summary>
        private readonly string? _longName;

        /// <summary>
        /// Indicates if the value following is required or not
        /// </summary>
        private readonly bool _required;

        #endregion

        #region Constructors

        /// <param name="type">Type of input for parsing</param>
        /// <param name="name">Flag name / value</param>
        public Input(InputType type, string name)
        {
            InputType = type;
            Name = name;
            _longName = null;
            _required = true;
        }

        /// <param name="type">Type of input for parsing</param>
        /// <param name="name">Flag name / value</param>
        /// <param name="required">Indicates if a following value is required</param>
        public Input(InputType type, string name, bool required)
        {
            InputType = type;
            Name = name;
            _longName = null;
            _required = required;
        }

        /// <param name="type">Type of input for parsing</param>
        /// <param name="shortName">Flag name / value</param>
        /// <param name="longName">Verbose flag name / value</param>
        public Input(InputType type, string shortName, string longName)
        {
            InputType = type;
            Name = shortName;
            _longName = longName;
            _required = true;
        }

        /// <param name="type">Type of input for parsing</param>
        /// <param name="shortName">Flag name / value</param>
        /// <param name="longName">Verbose flag name / value</param>
        /// <param name="required">Indicates if a following value is required</param>
        public Input(InputType type, string shortName, string longName, bool required)
        {
            InputType = type;
            Name = shortName;
            _longName = longName;
            _required = required;
        }

        #endregion

        #region Processors

        /// <summary>
        /// Process the input as a flag
        /// </summary>
        /// <param name="parts">Parts array to be referenced</param>
        /// <param name="index">Reference to the position in the parts</param>
        /// <returns>True if the flag was set, false otherwise</returns>
        public bool ProcessAsFlag(string[] parts, ref int index)
        {
            // Check the input type
            if (InputType != InputType.Flag)
                return false;

            // Check the parts array
            if (parts.Length == 0)
                return false;

            // Check the index
            if (index < 0 || index >= parts.Length)
                return false;

            // Check the name
            if (parts[index] == Name || (_longName != null && parts[index] == _longName))
                return true;

            return false;
        }

        /// <summary>
        /// Process the input as a boolean
        /// </summary>
        /// <param name="parts">Parts array to be referenced</param>
        /// <param name="index">Reference to the position in the parts</param>
        /// <returns>Boolean value if set, null otherwise</returns>
        public bool? ProcessAsBoolean(string[] parts, ref int index)
        {
            // Check the input type
            if (InputType != InputType.Boolean)
                return null;

            // Check the parts array
            if (parts == null || parts.Length == 0)
                return null;

            // Check the index
            if (index < 0 || index >= parts.Length)
                return null;

            // Check the name
            if (parts[index] == Name || (_longName != null && parts[index] == _longName))
            {
                // Ensure the value exists
                if (!DoesExist(parts, index + 1))
                    return _required ? null : true;

                // If the next value is valid
                if (!bool.TryParse(parts[index + 1], out bool value))
                    return _required ? null : true;

                index++;
                return value;
            }

            return null;
        }

        /// <summary>
        /// Process the input as an Int8
        /// </summary>
        /// <param name="parts">Parts array to be referenced</param>
        /// <param name="index">Reference to the position in the parts</param>
        /// <returns>Int8 value if set, minimum value if not required and missing, null otherwise</returns>
        public sbyte? ProcessAsInt8(string[] parts, ref int index)
        {
            // Check the input type
            if (InputType != InputType.Int8)
                return null;

            // Check the parts array
            if (parts == null || parts.Length == 0)
                return null;

            // Check the index
            if (index < 0 || index >= parts.Length)
                return null;

            // Check the name
            if (parts[index] == Name || (_longName != null && parts[index] == _longName))
            {
                // Ensure the value exists
                if (!DoesExist(parts, index + 1))
                    return _required ? null : sbyte.MinValue;

                // If the next value is valid
                if (sbyte.TryParse(parts[index + 1], out sbyte value))
                {
                    index++;
                    return value;
                }

                // Try to process as a formatted string
                string baseVal = ExtractFactorFromValue(parts[index + 1], out long factor);
                if (sbyte.TryParse(baseVal, out value))
                {
                    index++;
                    return (sbyte)(value * factor);
                }

                // Try to process as a hex string
                string hexValue = RemoveHexIdentifier(baseVal);
                if (sbyte.TryParse(hexValue, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value))
                {
                    index++;
                    return (sbyte)(value * factor);
                }

                // Return value based on required flag
                return _required ? null : sbyte.MinValue;
            }

            return null;
        }

        /// <summary>
        /// Process the input as a UInt8
        /// </summary>
        /// <param name="parts">Parts array to be referenced</param>
        /// <param name="index">Reference to the position in the parts</param>
        /// <returns>UInt8 value if set, minimum value if not required and missing, null otherwise</returns>
        public byte? ProcessAsUInt8(string[] parts, ref int index)
        {
            // Check the input type
            if (InputType != InputType.UInt8)
                return null;

            // Check the parts array
            if (parts == null || parts.Length == 0)
                return null;

            // Check the index
            if (index < 0 || index >= parts.Length)
                return null;

            // Check the name
            if (parts[index] == Name || (_longName != null && parts[index] == _longName))
            {
                // Ensure the value exists
                if (!DoesExist(parts, index + 1))
                    return _required ? null : byte.MinValue;

                // If the next value is valid
                if (byte.TryParse(parts[index + 1], out byte value))
                {
                    index++;
                    return value;
                }

                // Try to process as a formatted string
                string baseVal = ExtractFactorFromValue(parts[index + 1], out long factor);
                if (byte.TryParse(baseVal, out value))
                {
                    index++;
                    return (byte)(value * factor);
                }

                // Try to process as a hex string
                string hexValue = RemoveHexIdentifier(baseVal);
                if (byte.TryParse(hexValue, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value))
                {
                    index++;
                    return (byte)(value * factor);
                }

                // Return value based on required flag
                return _required ? null : byte.MinValue;
            }

            return null;
        }

        /// <summary>
        /// Process the input as an Int16
        /// </summary>
        /// <param name="parts">Parts array to be referenced</param>
        /// <param name="index">Reference to the position in the parts</param>
        /// <returns>Int16 value if set, minimum value if not required and missing, null otherwise</returns>
        public short? ProcessAsInt16(string[] parts, ref int index)
        {
            // Check the input type
            if (InputType != InputType.Int16)
                return null;

            // Check the parts array
            if (parts == null || parts.Length == 0)
                return null;

            // Check the index
            if (index < 0 || index >= parts.Length)
                return null;

            // Check the name
            if (parts[index] == Name || (_longName != null && parts[index] == _longName))
            {
                // Ensure the value exists
                if (!DoesExist(parts, index + 1))
                    return _required ? null : short.MinValue;

                // If the next value is valid
                if (short.TryParse(parts[index + 1], out short value))
                {
                    index++;
                    return value;
                }

                // Try to process as a formatted string
                string baseVal = ExtractFactorFromValue(parts[index + 1], out long factor);
                if (short.TryParse(baseVal, out value))
                {
                    index++;
                    return (short)(value * factor);
                }

                // Try to process as a hex string
                string hexValue = RemoveHexIdentifier(baseVal);
                if (short.TryParse(hexValue, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value))
                {
                    index++;
                    return (short)(value * factor);
                }

                // Return value based on required flag
                return _required ? null : short.MinValue;
            }

            return null;
        }

        /// <summary>
        /// Process the input as a UInt16
        /// </summary>
        /// <param name="parts">Parts array to be referenced</param>
        /// <param name="index">Reference to the position in the parts</param>
        /// <returns>UInt16 value if set, minimum value if not required and missing, null otherwise</returns>
        public ushort? ProcessAsUInt16(string[] parts, ref int index)
        {
            // Check the input type
            if (InputType != InputType.UInt16)
                return null;

            // Check the parts array
            if (parts == null || parts.Length == 0)
                return null;

            // Check the index
            if (index < 0 || index >= parts.Length)
                return null;

            // Check the name
            if (parts[index] == Name || (_longName != null && parts[index] == _longName))
            {
                // Ensure the value exists
                if (!DoesExist(parts, index + 1))
                    return _required ? null : ushort.MinValue;

                // If the next value is valid
                if (ushort.TryParse(parts[index + 1], out ushort value))
                {
                    index++;
                    return value;
                }

                // Try to process as a formatted string
                string baseVal = ExtractFactorFromValue(parts[index + 1], out long factor);
                if (ushort.TryParse(baseVal, out value))
                {
                    index++;
                    return (ushort)(value * factor);
                }

                // Try to process as a hex string
                string hexValue = RemoveHexIdentifier(baseVal);
                if (ushort.TryParse(hexValue, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value))
                {
                    index++;
                    return (ushort)(value * factor);
                }

                // Return value based on required flag
                return _required ? null : ushort.MinValue;
            }

            return null;
        }

        /// <summary>
        /// Process the input as an Int32
        /// </summary>
        /// <param name="parts">Parts array to be referenced</param>
        /// <param name="index">Reference to the position in the parts</param>
        /// <returns>Int32 value if set, minimum value if not required and missing, null otherwise</returns>
        public int? ProcessAsInt32(string[] parts, ref int index)
        {
            // Check the input type
            if (InputType != InputType.Int32)
                return null;

            // Check the parts array
            if (parts == null || parts.Length == 0)
                return null;

            // Check the index
            if (index < 0 || index >= parts.Length)
                return null;

            // Check the name
            if (parts[index] == Name || (_longName != null && parts[index] == _longName))
            {
                // Ensure the value exists
                if (!DoesExist(parts, index + 1))
                    return _required ? null : int.MinValue;

                // If the next value is valid
                if (int.TryParse(parts[index + 1], out int value))
                {
                    index++;
                    return value;
                }

                // Try to process as a formatted string
                string baseVal = ExtractFactorFromValue(parts[index + 1], out long factor);
                if (int.TryParse(baseVal, out value))
                {
                    index++;
                    return (int)(value * factor);
                }

                // Try to process as a hex string
                string hexValue = RemoveHexIdentifier(baseVal);
                if (int.TryParse(hexValue, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value))
                {
                    index++;
                    return (int)(value * factor);
                }

                // Return value based on required flag
                return _required ? null : int.MinValue;
            }

            return null;
        }

        /// <summary>
        /// Process the input as a UInt32
        /// </summary>
        /// <param name="parts">Parts array to be referenced</param>
        /// <param name="index">Reference to the position in the parts</param>
        /// <returns>UInt32 value if set, minimum value if not required and missing, null otherwise</returns>
        public uint? ProcessAsUInt32(string[] parts, ref int index)
        {
            // Check the input type
            if (InputType != InputType.UInt32)
                return null;

            // Check the parts array
            if (parts == null || parts.Length == 0)
                return null;

            // Check the index
            if (index < 0 || index >= parts.Length)
                return null;

            // Check the name
            if (parts[index] == Name || (_longName != null && parts[index] == _longName))
            {
                // Ensure the value exists
                if (!DoesExist(parts, index + 1))
                    return _required ? null : uint.MinValue;

                // If the next value is valid
                if (uint.TryParse(parts[index + 1], out uint value))
                {
                    index++;
                    return value;
                }

                // Try to process as a formatted string
                string baseVal = ExtractFactorFromValue(parts[index + 1], out long factor);
                if (uint.TryParse(baseVal, out value))
                {
                    index++;
                    return (uint)(value * factor);
                }

                // Try to process as a hex string
                string hexValue = RemoveHexIdentifier(baseVal);
                if (uint.TryParse(hexValue, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value))
                {
                    index++;
                    return (uint)(value * factor);
                }

                // Return value based on required flag
                return _required ? null : uint.MinValue;
            }

            return null;
        }

        /// <summary>
        /// Process the input as an Int64
        /// </summary>
        /// <param name="parts">Parts array to be referenced</param>
        /// <param name="index">Reference to the position in the parts</param>
        /// <returns>Int64 value if set, minimum value if not required and missing, null otherwise</returns>
        public long? ProcessAsInt64(string[] parts, ref int index)
        {
            // Check the input type
            if (InputType != InputType.Int64)
                return null;

            // Check the parts array
            if (parts == null || parts.Length == 0)
                return null;

            // Check the index
            if (index < 0 || index >= parts.Length)
                return null;

            // Check the name
            if (parts[index] == Name || (_longName != null && parts[index] == _longName))
            {
                // Ensure the value exists
                if (!DoesExist(parts, index + 1))
                    return _required ? null : long.MinValue;

                // If the next value is valid
                if (long.TryParse(parts[index + 1], out long value))
                {
                    index++;
                    return value;
                }

                // Try to process as a formatted string
                string baseVal = ExtractFactorFromValue(parts[index + 1], out long factor);
                if (long.TryParse(baseVal, out value))
                {
                    index++;
                    return (long)(value * factor);
                }

                // Try to process as a hex string
                string hexValue = RemoveHexIdentifier(baseVal);
                if (long.TryParse(hexValue, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value))
                {
                    index++;
                    return (long)(value * factor);
                }

                // Return value based on required flag
                return _required ? null : long.MinValue;
            }

            return null;
        }

        /// <summary>
        /// Process the input as a UInt64
        /// </summary>
        /// <param name="parts">Parts array to be referenced</param>
        /// <param name="index">Reference to the position in the parts</param>
        /// <returns>UInt64 value if set, minimum value if not required and missing, null otherwise</returns>
        public ulong? ProcessAsUInt64(string[] parts, ref int index)
        {
            // Check the input type
            if (InputType != InputType.UInt64)
                return null;

            // Check the parts array
            if (parts == null || parts.Length == 0)
                return null;

            // Check the index
            if (index < 0 || index >= parts.Length)
                return null;

            // Check the name
            if (parts[index] == Name || (_longName != null && parts[index] == _longName))
            {
                // Ensure the value exists
                if (!DoesExist(parts, index + 1))
                    return _required ? null : ulong.MinValue;

                // If the next value is valid
                if (ulong.TryParse(parts[index + 1], out ulong value))
                {
                    index++;
                    return value;
                }

                // Try to process as a formatted string
                string baseVal = ExtractFactorFromValue(parts[index + 1], out long factor);
                if (ulong.TryParse(baseVal, out value))
                {
                    index++;
                    return (ulong)(value * (ulong)factor);
                }

                // Try to process as a hex string
                string hexValue = RemoveHexIdentifier(baseVal);
                if (ulong.TryParse(hexValue, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value))
                {
                    index++;
                    return (ulong)(value * (ulong)factor);
                }

                // Return value based on required flag
                return _required ? null : ulong.MinValue;
            }

            return null;
        }

        /// <summary>
        /// Process the input as a UInt64
        /// </summary>
        /// <param name="parts">Parts array to be referenced</param>
        /// <param name="index">Reference to the position in the parts</param>
        /// <returns>String value if set, empty value if not required and missing, null otherwise</returns>
        public string? ProcessAsString(string[] parts, ref int index)
        {
            // Check the input type
            if (InputType != InputType.String)
                return null;

            // Check the parts array
            if (parts == null || parts.Length == 0)
                return null;

            // Check the index
            if (index < 0 || index >= parts.Length)
                return null;

            // Check the name
            if (parts[index] == Name || (_longName != null && parts[index] == _longName))
            {
                // Ensure the value exists
                if (!DoesExist(parts, index + 1))
                    return _required ? null : string.Empty;

                index++;
                return parts[index];
            }

            return null;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Returns whether or not the selected item exists
        /// </summary>
        /// <param name="parts">Parts array to be referenced</param>
        /// <param name="index">Current index</param>
        /// <returns>True if the next item exists, false otherwise</returns>
        internal static bool DoesExist(string[] parts, int index)
            => index >= 0 && index < parts.Length;

        /// <summary>
        /// Get the trimmed value and multiplication factor from a value
        /// </summary>
        /// <param name="value">String value to treat as suffixed number</param>
        /// <returns>Trimmed value and multiplication factor</returns>
        internal static string ExtractFactorFromValue(string value, out long factor)
        {
            value = value.Trim('"');
            factor = 1;

            // Characters
            if (value.EndsWith("c", StringComparison.Ordinal))
            {
                factor = 1;
                value = value.TrimEnd('c');
            }

            // Words
            else if (value.EndsWith("w", StringComparison.Ordinal))
            {
                factor = 2;
                value = value.TrimEnd('w');
            }

            // Double Words
            else if (value.EndsWith("d", StringComparison.Ordinal))
            {
                factor = 4;
                value = value.TrimEnd('d');
            }

            // Quad Words
            else if (value.EndsWith("q", StringComparison.Ordinal))
            {
                factor = 8;
                value = value.TrimEnd('q');
            }

            // Kilobytes
            else if (value.EndsWith("k", StringComparison.Ordinal))
            {
                factor = 1024;
                value = value.TrimEnd('k');
            }

            // Megabytes
            else if (value.EndsWith("M", StringComparison.Ordinal))
            {
                factor = 1024 * 1024;
                value = value.TrimEnd('M');
            }

            // Gigabytes
            else if (value.EndsWith("G", StringComparison.Ordinal))
            {
                factor = 1024 * 1024 * 1024;
                value = value.TrimEnd('G');
            }

            return value;
        }

        /// <summary>
        /// Removes a leading 0x if it exists, case insensitive
        /// </summary>
        /// <param name="value">String with removed leading 0x</param>
        /// <returns></returns>
        internal static string RemoveHexIdentifier(string value)
        {
            if (value.Length <= 2)
                return value;
            if (value[0] != '0')
                return value;
            if (value[1] != 'x' && value[1] != 'X')
                return value;

            return value.Substring(2);
        }

        #endregion
    }
}