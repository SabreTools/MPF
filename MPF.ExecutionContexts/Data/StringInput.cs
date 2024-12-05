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
                builder.Append($"\"{Value}\"");
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
            if (parts[index] == Name || (_shortName != null && parts[index] == _shortName))
            {
                // Ensure the value exists
                if (index + 1 >= parts.Length)
                {
                    Value = _required ? null : string.Empty;
                    return !_required;
                }

                index++;
                Value = parts[index];
                return true;
            }

            // Check for equal separated
            if (parts[index].StartsWith($"{Name}=") || (_shortName != null && parts[index].StartsWith($"{_shortName}=")))
            {
                // Split the string, using the first equal sign as the separator
                string[] tempSplit = parts[index].Split('=');
                string key = tempSplit[0];
                string val = string.Join("=", tempSplit, 1, tempSplit.Length - 1);

                // Ensure the value exists
                if (string.IsNullOrEmpty(val))
                {
                    Value = _required ? null : string.Empty;
                    return !_required;
                }

                Value = val;
                return true;
            }

            return false;
        }
    }
}