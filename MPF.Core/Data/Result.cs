namespace MPF.Core.Data
{
    /// <summary>
    /// Generic success/failure result object, with optional message
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Internal representation of success
        /// </summary>
        private readonly bool success;

        /// <summary>
        /// Optional message for the result
        /// </summary>
        public string Message { get; private set; }

        private Result(bool success, string message)
        {
            this.success = success;
            this.Message = message;
        }

        /// <summary>
        /// Create a default success result with no message
        /// </summary>
        public static Result Success() => new Result(true, "");

        /// <summary>
        /// Create a success result with a custom message
        /// </summary>
        /// <param name="message">String to add as a message</param>
        public static Result Success(string message) => new Result(true, message);

        /// <summary>
        /// Create a default failure result with no message
        /// </summary>
        /// <returns></returns>
        public static Result Failure() => new Result(false, "");

        /// <summary>
        /// Create a failure result with a custom message
        /// </summary>
        /// <param name="message">String to add as a message</param>
        public static Result Failure(string message) => new Result(false, message);

        /// <summary>
        /// Results can be compared to boolean values based on the success value
        /// </summary>
        public static implicit operator bool(Result result) => result.success;
    }
}
