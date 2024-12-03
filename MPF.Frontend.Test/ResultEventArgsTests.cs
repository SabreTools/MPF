using MPF.Frontend;
using Xunit;

namespace MPF.Frontend.Test
{
    public class ResultEventArgsTests
    {
        [Fact]
        public void EmptySuccessTest()
        {
            var actual = ResultEventArgs.Success();
            Assert.True(actual);
            Assert.Empty(actual.Message);
        }

        [Fact]
        public void CustomMessageSuccessTest()
        {
            string message = "Success!";
            var actual = ResultEventArgs.Success(message);
            Assert.True(actual);
            Assert.Equal(message, actual.Message);
        }

        [Fact]
        public void EmptyFailureTest()
        {
            var actual = ResultEventArgs.Failure();
            Assert.False(actual);
            Assert.Empty(actual.Message);
        }

        [Fact]
        public void CustomMessageFailureTest()
        {
            string message = "Failure!";
            var actual = ResultEventArgs.Failure(message);
            Assert.False(actual);
            Assert.Equal(message, actual.Message);
        }
    }
}
