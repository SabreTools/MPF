using System;
using System.Globalization;
using System.Text;

namespace MPF.ExecutionContexts.Data
{
    /// <summary>
    /// Represents an UInt64 flag with an optional trailing value
    /// </summary>
    public class UInt64Input : Input<ulong?>
    {
        #region Properties

        /// <summary>
        /// Indicates a minimum value (inclusive) for the flag
        /// </summary>
        public ulong? MinValue { get; set; } = null;

        /// <summary>
        /// Indicates a maximum value (inclusive) for the flag
        /// </summary>
        public ulong? MaxValue { get; set; } = null;

        #endregion

        #region Constructors

        /// <inheritdoc/>
        public UInt64Input(string name)
            : base(name) { }

        /// <inheritdoc/>
        public UInt64Input(string name, bool required)
            : base(name, required) { }

        /// <inheritdoc/>
        public UInt64Input(string shortName, string longName)
            : base(shortName, longName) { }

        /// <inheritdoc/>
        public UInt64Input(string shortName, string longName, bool required)
            : base(shortName, longName, required) { }

        /// <inheritdoc/>
        public UInt64Input(string[] names)
            : base(names) { }

        /// <inheritdoc/>
        public UInt64Input(string[] names, bool required)
            : base(names, required) { }

        #endregion

        /// <inheritdoc/>
        public override string Format(bool useEquals)
        {
            // Do not output if there is no value
            if (Value == null)
                return string.Empty;

            // Build the output format
            var builder = new StringBuilder();

            // Flag name
            builder.Append(Name);

            // Only output separator and value if needed
            if (_required || (!_required && Value != ulong.MinValue))
            {
                // Separator
                if (useEquals)
                    builder.Append("=");
                else
                    builder.Append(" ");

                // Value
                builder.Append(Value.ToString());
            }

            return builder.ToString();
        }

        /// <inheritdoc/>
        public override bool Process(string[] parts, ref int index)
        {
            // Check the parts array
            if (index < 0 || index >= parts.Length)
                return false;

            // Check for space-separated
            string part = parts[index];
            if (part == Name || (_altNames.Length > 0 && Array.FindIndex(_altNames, n => n == part) > -1))
            {
                // Ensure the value exists
                if (index + 1 >= parts.Length)
                {
                    Value = _required ? null : ulong.MinValue;
                    Value = (MinValue != null && Value < MinValue) ? MinValue : Value;
                    Value = (MaxValue != null && Value > MaxValue) ? MaxValue : Value;
                    return !_required;
                }

                // If the next value is valid
                if (ParseValue(parts[index + 1], out ulong? value) && value != null)
                {
                    index++;
                    Value = value;
                    Value = (MinValue != null && Value < MinValue) ? MinValue : Value;
                    Value = (MaxValue != null && Value > MaxValue) ? MaxValue : Value;
                    return true;
                }

                // Return value based on required flag
                Value = _required ? null : ulong.MinValue;
                Value = (MinValue != null && Value < MinValue) ? MinValue : Value;
                Value = (MaxValue != null && Value > MaxValue) ? MaxValue : Value;
                return !_required;
            }

            // Check for equal separated
            if (part.StartsWith($"{Name}=") || (_altNames.Length > 0 && Array.FindIndex(_altNames, n => part.StartsWith($"{n}=")) > -1))
            {
                // Split the string, using the first equal sign as the separator
                string[] tempSplit = part.Split('=');
                string key = tempSplit[0];
                string val = string.Join("=", tempSplit, 1, tempSplit.Length - 1);

                // Ensure the value exists
                if (string.IsNullOrEmpty(val))
                {
                    Value = _required ? null : ulong.MinValue;
                    Value = (MinValue != null && Value < MinValue) ? MinValue : Value;
                    Value = (MaxValue != null && Value > MaxValue) ? MaxValue : Value;
                    return !_required;
                }

                // If the next value is valid
                if (ParseValue(val, out ulong? value) && value != null)
                {
                    Value = value;
                    Value = (MinValue != null && Value < MinValue) ? MinValue : Value;
                    Value = (MaxValue != null && Value > MaxValue) ? MaxValue : Value;
                    return true;
                }

                // Return value based on required flag
                Value = _required ? null : ulong.MinValue;
                Value = (MinValue != null && Value < MinValue) ? MinValue : Value;
                Value = (MaxValue != null && Value > MaxValue) ? MaxValue : Value;
                return !_required;
            }

            return false;
        }

        /// <summary>
        /// Parse a value from a string
        /// </summary>
        private static bool ParseValue(string str, out ulong? output)
        {
            // If the next value is valid
            if (ulong.TryParse(str, out ulong value))
            {
                output = value;
                return true;
            }

            // Try to process as a formatted string
            string baseVal = ExtractFactorFromValue(str, out long factor);
            if (ulong.TryParse(baseVal, out value))
            {
                output = (ulong)(value * (ulong)factor);
                return true;
            }

            // Try to process as a hex string
            string hexValue = RemoveHexIdentifier(baseVal);
            if (ulong.TryParse(hexValue, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value))
            {
                output = (ulong)(value * (ulong)factor);
                return true;
            }

            // The value could not be parsed
            output = null;
            return false;
        }
    }
}