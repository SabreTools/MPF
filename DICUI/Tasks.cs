using System.Linq;
using System.Threading.Tasks;
using DICUI.External;

namespace DICUI
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

    /// <summary>
    /// Class containing dumping tasks
    /// </summary>
    public class Tasks
    {
        /// <summary>
        /// Run protection scan on a given dump environment
        /// </summary>
        /// <param name="env">DumpEnvirionment containing all required information</param>
        /// <returns>Copy protection detected in the envirionment, if any</returns>
        public static async Task<string> RunProtectionScan(string path)
        {
            var found = await Task.Run(() =>
            {
                return ProtectionFind.Scan(path);
            });

            if (found == null)
                return "None found";

            return string.Join("\n", found.Select(kvp => kvp.Key + ": " + kvp.Value).ToArray());
        }
    }
}
