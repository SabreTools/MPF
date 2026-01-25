using System;
using System.Text;

namespace MPF.ExecutionContexts.Data
{
    /// <summary>
    /// Represents a string flag with an optional trailing value
    /// </summary>
    public class StringInput : Input<string>

    {
        #region Properties

        /// <summary>
        /// Indicates whether quotes are used in output or not
        /// </summary>
        public bool Quotes { get; set; } = false;

        #endregion

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

        /// <inheritdoc/>
        public StringInput(string[] names)
            : base(names) { }

        /// <inheritdoc/>
        public StringInput(string[] names, bool required)
            : base(names, required) { }

        #endregion

        /// <inheritdoc/>
        public override string Format(bool useEquals)
        {
            // Do not output if there is no value
            if (Value is null)
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
                    builder.Append('=');
                else
                    builder.Append(' ');

                // Value
                if (Quotes)
                    builder.Append($"\"{Value}\"");
                else
                    builder.Append(Value);
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
                    Value = _required ? null : string.Empty;
                    return !_required;
                }

                index++;
                Value = parts[index].Trim('"');
                return true;
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
                    Value = _required ? null : string.Empty;
                    return !_required;
                }

                Value = val.Trim('"');
                return true;
            }

            return false;
        }
    }
}
