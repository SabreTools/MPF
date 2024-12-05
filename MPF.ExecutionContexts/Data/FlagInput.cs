namespace MPF.ExecutionContexts.Data
{
    /// <summary>
    /// Represents a boolean flag without a trailing value
    /// </summary>
    public class FlagInput : Input<bool>
    {
        #region Constructors

        /// <inheritdoc/>
        public FlagInput(string name)
            : base(name) { }

        /// <inheritdoc/>
        public FlagInput(string name, bool required)
            : base(name, required) { }

        /// <inheritdoc/>
        public FlagInput(string shortName, string longName)
            : base(shortName, longName) { }

        /// <inheritdoc/>
        public FlagInput(string shortName, string longName, bool required)
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

            Value = true;
            return true;
        }
    }
}