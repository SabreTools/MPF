namespace DICUI.Utilities
{
    /// <summary>
    /// Generic success/failure result object, with optional message
    /// </summary>
    public class Result
    {
        private bool success;
        public string Message { get; private set; }

        private Result(bool success, string message)
        {
            this.success = success;
            this.Message = message;
        }

        public static Result Success() => new Result(true, "");
        public static Result Success(string message) => new Result(true, message);
        public static Result Success(string message, params object[] args) => new Result(true, string.Format(message, args));

        public static Result Failure() => new Result(false, "");
        public static Result Failure(string message) => new Result(false, message);
        public static Result Failure(string message, params object[] args) => new Result(false, string.Format(message, args));

        public static implicit operator bool(Result result) => result.success;
    }
}
