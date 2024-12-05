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