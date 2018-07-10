using DICUI.Utilities;
using Xunit;

namespace DICUI.Test.Utilities
{
    public class DriveTest
    {
        [Fact]
        public void DriveConstructorsTest()
        {
            Assert.True(Drive.Floppy('a').IsFloppy);
            Assert.False(Drive.Optical('d', "test", true).IsFloppy);
        }
    }
}
