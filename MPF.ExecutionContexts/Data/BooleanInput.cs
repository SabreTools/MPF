using System.Text;

namespace MPF.ExecutionContexts.Data
{
    /// <summary>
    /// Represents a boolean flag with an optional trailing value
    /// </summary>
    public class BooleanInput : Input<bool?>
    {
        #region Constructors

        /// <inheritdoc/>
        public BooleanInput(string name)
            : base(name) { }

        /// <inheritdoc/>
        public BooleanInput(string name, bool required)
            : base(name, required) { }

        /// <inheritdoc/>
        public BooleanInput(string shortName, string longName)
            : base(shortName, longName) { }

        /// <inheritdoc/>
        public BooleanInput(string shortName, string longName, bool required)
            : base(shortName, longName, required) { }

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
            if (_longName != null)
                builder.Append(_longName);
            else
                builder.Append(Name);

            // Separator
            if (useEquals)
                builder.Append("=");
            else
                builder.Append(" ");

            // Value
            builder.Append(Value.ToString()?.ToLowerInvariant());

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
                    Value = _required ? null : true;
                    return !_required;
                }

                // If the next value is valid
                if (!bool.TryParse(parts[index + 1], out bool value))
                {
                    Value = _required ? null : true;
                    return !_required;
                }

                index++;
                Value = value;
                return true;
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
                    Value = _required ? null : true;
                    return !_required;
                }

                // If the next value is valid
                if (!bool.TryParse(val, out bool value))
                {
                    Value = _required ? null : true;
                    return !_required;
                }

                Value = value;
                return true;
            }

            return false;
        }
    }
}