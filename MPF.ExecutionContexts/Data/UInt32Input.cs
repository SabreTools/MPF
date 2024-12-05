using System.Globalization;
using System.Text;

namespace MPF.ExecutionContexts.Data
{
    /// <summary>
    /// Represents an UInt32 flag with an optional trailing value
    /// </summary>
    public class UInt32Input : Input<uint?>
    {
        #region Constructors

        /// <inheritdoc/>
        public UInt32Input(string name)
            : base(name) { }

        /// <inheritdoc/>
        public UInt32Input(string name, bool required)
            : base(name, required) { }

        /// <inheritdoc/>
        public UInt32Input(string shortName, string longName)
            : base(shortName, longName) { }

        /// <inheritdoc/>
        public UInt32Input(string shortName, string longName, bool required)
            : base(shortName, longName, required) { }

        #endregion

        /// <inheritdoc/>
        public override string Format(bool useEquals)
        {
            // Do not output if there is no value
            if (Value == null)
                return string.Empty;

            // Do not output if required value is invalid
            if (_required && Value == uint.MinValue)
                return string.Empty;

            // Build the output format
            var builder = new StringBuilder();

            // Flag name
            if (_longName != null)
                builder.Append(_longName);
            else
                builder.Append(Name);

            // Only output separator and value if needed
            if (_required || (!_required && Value != uint.MinValue))
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
            if (parts.Length == 0)
                return false;

            // Check the index
            if (index < 0 || index >= parts.Length)
                return false;

            // Check the name
            if (parts[index] != Name && (_longName != null && parts[index] != _longName))
                return false;

            // Ensure the value exists
            if (!DoesExist(parts, index + 1))
            {
                Value = _required ? null : uint.MinValue;
                return !_required;
            }

            // If the next value is valid
            if (uint.TryParse(parts[index + 1], out uint value))
            {
                index++;
                Value = value;
                return true;
            }

            // Try to process as a formatted string
            string baseVal = ExtractFactorFromValue(parts[index + 1], out long factor);
            if (uint.TryParse(baseVal, out value))
            {
                index++;
                Value = (uint)(value * factor);
                return true;
            }

            // Try to process as a hex string
            string hexValue = RemoveHexIdentifier(baseVal);
            if (uint.TryParse(hexValue, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out value))
            {
                index++;
                Value = (uint)(value * factor);
                return true;
            }

            // Return value based on required flag
            Value = _required ? null : uint.MinValue;
            return !_required;
        }
    }
}