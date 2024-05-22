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
        /// Event arguments are just the value of the string contained within
        /// </summary>
        public static implicit operator string(StringEventArgs args) => args._value;

        /// <summary>
        /// Event arguments are just the value of the string contained within
        /// </summary>
        public static implicit operator StringBuilder(StringEventArgs args) => new(args._value);

        /// <summary>
        /// Event arguments are just the value of the string contained within
        /// </summary>
        public static implicit operator StringEventArgs(string? str) => new(str);

        /// <summary>
        /// Event arguments are just the value of the string contained within
        /// </summary>
        public static implicit operator StringEventArgs(StringBuilder? sb) => new(sb);
    }
}