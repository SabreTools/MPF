using DICUI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;


namespace DICUI.Test.Utilities
{
    public class DriveTest
    {
        [Fact]
        public void DriveConstructorsTest()
        {
            Assert.True(Drive.Floppy('a').IsFloppy);
            Assert.False(Drive.Optical('d', "test").IsFloppy);
        }
    }
}
