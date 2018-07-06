using DICUI.Utilities;
using Xunit;

namespace DICUI.Test
{
    public class ResultTest
    {
        [Fact]
        public void ResultSuccessTest()
        {
            Result actual = Result.Success();
            Assert.Empty(actual.Message);

            string message = "Success!";
            actual = Result.Success(message);
            Assert.Equal(message, actual.Message);

            message = "Success! {0}";
            string parameter = "Parameter";
            actual = Result.Success(message, parameter);
            Assert.Equal(string.Format(message, parameter), actual.Message);
        }

        [Fact]
        public void ResultFailureTest()
        {
            Result actual = Result.Failure();
            Assert.Empty(actual.Message);

            string message = "Failure!";
            actual = Result.Failure(message);
            Assert.Equal(message, actual.Message);

            message = "Failure! {0}";
            string parameter = "Parameter";
            actual = Result.Failure(message, parameter);
            Assert.Equal(string.Format(message, parameter), actual.Message);
        }
    }
}
