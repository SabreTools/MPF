using System;

namespace MPF.Core
{
    /// <summary>
    /// Generic success/failure result object, with optional message
    /// </summary>
    public class ResultEventArgs : EventArgs
    {
        /// <summary>
        /// Internal representation of success
        /// </summary>
        private readonly bool _success;

        /// <summary>
        /// Optional message for the result
        /// </summary>
        public string Message { get; }

        private ResultEventArgs(bool success, string message)
        {
            _success = success;
            Message = message;
        }

        /// <summary>
        /// Create a default success result with no message
        /// </summary>
        public static ResultEventArgs Success() => new(true, string.Empty);

        /// <summary>
        /// Create a success result with a custom message
        /// </summary>
        /// <param name="message">String to add as a message</param>
        public static ResultEventArgs Success(string? message) => new(true, message ?? string.Empty);

        /// <summary>
        /// Create a default failure result with no message
        /// </summary>
        /// <returns></returns>
        public static ResultEventArgs Failure() => new(false, string.Empty);

        /// <summary>
        /// Create a failure result with a custom message
        /// </summary>
        /// <param name="message">String to add as a message</param>
        public static ResultEventArgs Failure(string? message) => new(false, message ?? string.Empty);

        /// <summary>
        /// Results can be compared to boolean values based on the success value
        /// </summary>
        public static implicit operator bool(ResultEventArgs result) => result._success;

        /// <summary>
        /// Results can be compared to boolean values based on the success value
        /// </summary>
        public static implicit operator ResultEventArgs(bool bval) => new(bval, string.Empty);
    }
}
