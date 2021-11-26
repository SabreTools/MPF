using MPF.Core.Data;
using Xunit;

namespace MPF.Test.Core.Data
{
    public class ResultTests
    {
        [Fact]
        public void EmptySuccessTest()
        {
            var actual = Result.Success();
            Assert.True(actual);
            Assert.Empty(actual.Message);
        }

        [Fact]
        public void CustomMessageSuccessTest()
        {
            string message = "Success!";
            var actual = Result.Success(message);
            Assert.True(actual);
            Assert.Equal(message, actual.Message);
        }

        [Fact]
        public void EmptyFailureTest()
        {
            var actual = Result.Failure();
            Assert.False(actual);
            Assert.Empty(actual.Message);
        }

        [Fact]
        public void CustomMessageFailureTest()
        {
            string message = "Failure!";
            var actual = Result.Failure(message);
            Assert.False(actual);
            Assert.Equal(message, actual.Message);
        }
    }
}
