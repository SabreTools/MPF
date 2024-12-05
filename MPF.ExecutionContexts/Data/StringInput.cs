using System.Text;

namespace MPF.ExecutionContexts.Data
{
    /// <summary>
    /// Represents a string flag with an optional trailing value
    /// </summary>
    public class StringInput : Input<string>
    {
        #region Constructors

        /// <inheritdoc/>
        public StringInput(string name)
            : base(name) { }

        /// <inheritdoc/>
        public StringInput(string name, bool required)
            : base(name, required) { }

        /// <inheritdoc/>
        public StringInput(string shortName, string longName)
            : base(shortName, longName) { }

        /// <inheritdoc/>
        public StringInput(string shortName, string longName, bool required)
            : base(shortName, longName, required) { }

        #endregion

        /// <inheritdoc/>
        public override string Format(bool useEquals)
        {
            // Do not output if there is no value
            if (Value == null)
                return string.Empty;

            // Do not output if required value is invalid
            if (_required && Value == string.Empty)
                return string.Empty;

            // Build the output format
            var builder = new StringBuilder();

            // Flag name
            if (_longName != null)
                builder.Append(_longName);
            else
                builder.Append(Name);

            // Only output separator and value if needed
            if (_required || (!_required && Value != string.Empty))
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
                Value = _required ? null : string.Empty;
                return !_required;
            }

            index++;
            Value = parts[index];
            return true;
        }
    }
}