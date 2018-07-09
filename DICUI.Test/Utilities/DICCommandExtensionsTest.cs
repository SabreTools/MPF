using System;
using DICUI.Data;
using DICUI.Utilities;
using Xunit;

namespace DICUI.Test.Utilities
{
    public class DICCommandExtensionsTest
    {
        [Fact]
        public void NameTest()
        {
            var values = (DICCommand[])Enum.GetValues(typeof(DICCommand));
            foreach(var command in values)
            {
                string expected = Converters.DICCommandToString(command);
                string actual = command.Name();

                Assert.Equal(expected, actual);
            }
        }
    }
}
