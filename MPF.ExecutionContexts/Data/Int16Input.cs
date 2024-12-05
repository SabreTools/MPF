using System.Globalization;
using System.Text;

namespace MPF.ExecutionContexts.Data
{
    /// <summary>
    /// Represents an Int16 flag with an optional trailing value
    /// </summary>
    public class Int16Input : Input<short?>
    {
        #region Constructors

        /// <inheritdoc/>
        public Int16Input(string name)
            : base(name) { }

        /// <inheritdoc/>
        public Int16Input(string name, bool required)
            : base(name, required) { }

        /// <inheritdoc/>
        public Int16Input(string shortName, string longName)
            : base(shortName, longName) { }

        /// <inheritdoc/>
        public Int16Input(string shortName, string longName, bool required)
            : base(shortName, longName, required) { }

        #endregion

        /// <inheritdoc/>
        public override string Format(bool useEquals)
        {
            // Do not output if there is no value
            if (Value == null)
                return string.Empty;

            // Do not output if required value is invalid
            if (_required && Value == short.MinValue)
                return string.Empty;

            // Build the output format
            var builder = new StringBuilder();

            // Flag name
            if (_longName != null)
                builder.Append(_longName);
            else
                builder.Append(Name);

            // Only output separator and value if needed
            if (_required || (!_required && Value != short.MinValue))
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
            if (parts[index] == Name || (_longName != null && parts[index] == _longName))
            {
                // Ensure the value exists
                if (index + 1 >= parts.Length)
                {
                    Value = _required ? null : short.MinValue;
                    return !_required;
                }

                // If the next value is valid
                if (ParseValue(parts[index + 1], out short? value) && value != null)
                {
                    index++;
                    Value = value;
                    return true;
                }

                // Return value based on required flag
                Value = _required ? null : short.MinValue;
                return !_required;
            }

            // Check for equal separated
            if (parts[index].StartsWith($"{Name}=") || (_longName != null && parts[index].StartsWith($"{_longName}=")))
            {
                // Split the string, using the first equal sign as the separator
                string[] tempSplit = parts[index].Split('=');
                string key = tempSplit[0];
                string val = string.Join("=", tempSplit, 1, tempSplit.Length - 1);

                // Ensure the value exists
                if (string.IsNullOrEmpty(val))
                {
                    Value = _required ? null : short.MinValue;
                    return !_required;
                }

                // If the next value is valid
                if (ParseValue(val, out short? value) && value != null)
                {
                    Value = value;
                    return true;
                }

                // Return value based on required flag
                Value = _required ? null : short.MinValue;
                return !_required;
            }

            return false;
        }
        
        /// <summary>
        /// Parse a value from a string
        /// </summary>
        private static bool ParseValue(string str, out short? output)
        {
            // If the next value is valid
            if (short.TryParse(str, out short value))
            {
                output = value;
                return true;
            }

            // Try to process as a formatted string
            string baseVal = ExtractFactorFromValue(str, out long factor);
            if (short.TryParse(baseVal, out value))
            {
                output = (short)(value * factor);
                return true;
            }

            // Try to process as a hex string
            string hexValue = RemoveHexIdentifier(baseVal);
            if (short.TryParse(hexValue, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value))
            {
                output = (short)(value * factor);
                return true;
            }

            // The value could not be parsed
            output = null;
            return false;
        }
    }
}