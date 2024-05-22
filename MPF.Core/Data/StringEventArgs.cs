using System;
using System.Text;

namespace MPF.Core.Data
{
    /// <summary>
    /// String wrapper for event arguments
    /// </summary>
    public class StringEventArgs : EventArgs
    {
        /// <summary>
        /// String represented by the event arguments
        /// </summary>
        private readonly string _value;

        /// <summary>
        /// Constructor for string values
        /// </summary>
        public StringEventArgs(string? value)
        {
            _value = value ?? string.Empty;
        }

        /// <summary>
        /// Constructor for StringBuilder values
        /// </summary>
        public StringEventArgs(StringBuilder? value)
        {
            _value = value?.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Results can be compared to boolean values based on the success value
        /// </summary>
        public static implicit operator string(StringEventArgs args) => args._value;
    }
}