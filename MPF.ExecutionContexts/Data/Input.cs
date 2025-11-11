namespace MPF.ExecutionContexts.Data
{
    /// <summary>
    /// Represents a single input for an execution context
    /// </summary>
    public abstract class Input
    {
        #region Properties

        /// <summary>
        /// Primary name for the input
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Alternative name for the input
        /// </summary>
        protected readonly string[] _altNames;

        /// <summary>
        /// Indicates if the value following is required or not
        /// </summary>
        protected readonly bool _required;

        /// <summary>
        /// Indicates if a value has been set
        /// </summary>
        public abstract bool ValueSet { get; }

        #endregion

        #region Constructors

        /// <param name="name">Flag name / value</param>
        public Input(string name)
        {
            Name = name;
            _altNames = [];
            _required = true;
        }

        /// <param name="name">Flag name / value</param>
        /// <param name="required">Indicates if a following value is required</param>
        public Input(string name, bool required)
        {
            Name = name;
            _altNames = [];
            _required = required;
        }

        /// <param name="shortName">Flag name / value</param>
        /// <param name="longName">Verbose flag name / value</param>
        public Input(string shortName, string longName)
        {
            Name = longName;
            _altNames = [shortName];
            _required = true;
        }

        /// <param name="shortName">Flag name / value</param>
        /// <param name="longName">Verbose flag name / value</param>
        /// <param name="required">Indicates if a following value is required</param>
        public Input(string shortName, string longName, bool required)
        {
            Name = longName;
            _altNames = [shortName];
            _required = required;
        }

        /// <param name="names">Set of names to use</param>
        public Input(string[] names)
        {
            Name = names.Length > 0 ? names[0] : string.Empty;
            _altNames = names;
            _required = true;
        }

        /// <param name="names">Set of names to use</param>
        /// <param name="required">Indicates if a following value is required</param>
        public Input(string[] names, bool required)
        {
            Name = names.Length > 0 ? names[0] : string.Empty;
            _altNames = names;
            _required = required;
        }

        #endregion

        #region Functionality

        /// <summary>
        /// Clear any accumulated value
        /// </summary>
        public abstract void ClearValue();

        /// <summary>
        /// Create a formatted representation of the input and possible value
        /// </summary>
        /// <param name="useEquals">Use an equal sign as a separator on output</param>
        public abstract string Format(bool useEquals);

        /// <summary>
        /// Process the current index, if possible
        /// </summary>
        /// <param name="parts">Parts array to be referenced</param>
        /// <param name="index">Reference to the position in the parts</param>
        /// <returns>True if a value could be determined, false otherwise</returns>
        public abstract bool Process(string[] parts, ref int index);

        #endregion

        #region Helpers

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
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
            if (value.EndsWith('c'))
#else
            if (value.EndsWith("c", System.StringComparison.Ordinal))
#endif
            {
                factor = 1;
                value = value.TrimEnd('c');
            }

            // Words
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
            else if (value.EndsWith('w'))
#else
            else if (value.EndsWith("w", System.StringComparison.Ordinal))
#endif
            {
                factor = 2;
                value = value.TrimEnd('w');
            }

            // Double Words
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
            else if (value.EndsWith('d'))
#else
            else if (value.EndsWith("d", System.StringComparison.Ordinal))
#endif
            {
                factor = 4;
                value = value.TrimEnd('d');
            }

            // Quad Words
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
            else if (value.EndsWith('q'))
#else
            else if (value.EndsWith("q", System.StringComparison.Ordinal))
#endif
            {
                factor = 8;
                value = value.TrimEnd('q');
            }

            // Kilobytes
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
            else if (value.EndsWith('k'))
#else
            else if (value.EndsWith("k", System.StringComparison.Ordinal))
#endif
            {
                factor = 1024;
                value = value.TrimEnd('k');
            }

            // Megabytes
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
            else if (value.EndsWith('M'))
#else
            else if (value.EndsWith("M", System.StringComparison.Ordinal))
#endif
            {
                factor = 1024 * 1024;
                value = value.TrimEnd('M');
            }

            // Gigabytes
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
            else if (value.EndsWith('G'))
#else
            else if (value.EndsWith("G", System.StringComparison.Ordinal))
#endif
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

#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
            return value[2..];
#else
            return value.Substring(2);
#endif
        }

        #endregion
    }

    /// <summary>
    /// Represents a single input for an execution context
    /// </summary>
    public abstract class Input<T> : Input
    {
        #region Properties

        /// <summary>
        /// Represents the last value stored
        /// </summary>
        public T? Value { get; protected set; }

        /// <inheritdoc/>
        public override bool ValueSet => Value != null;

        #endregion

        #region Constructors

        /// <inheritdoc/>
        public Input(string name)
            : base(name) { }

        /// <inheritdoc/>
        public Input(string name, bool required)
            : base(name, required) { }

        /// <inheritdoc/>
        public Input(string shortName, string longName)
            : base(shortName, longName) { }

        /// <inheritdoc/>
        public Input(string shortName, string longName, bool required)
            : base(shortName, longName, required) { }

        /// <inheritdoc/>
        public Input(string[] names)
            : base(names) { }

        /// <inheritdoc/>
        public Input(string[] names, bool required)
            : base(names, required) { }

        #endregion

        #region Functionality

        /// <inheritdoc/>
        public override void ClearValue()
        {
            Value = default;
        }

        /// <summary>
        /// Set a new value
        /// </summary>
        public void SetValue(T value)
        {
            Value = value;
        }

        #endregion
    }
}
