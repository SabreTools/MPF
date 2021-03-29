using MPF.Data;
using Xunit;

namespace MPF.Test
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
        }

        [Fact]
        public void ResultFailureTest()
        {
            Result actual = Result.Failure();
            Assert.Empty(actual.Message);

            string message = "Failure!";
            actual = Result.Failure(message);
            Assert.Equal(message, actual.Message);
        }
    }
}
