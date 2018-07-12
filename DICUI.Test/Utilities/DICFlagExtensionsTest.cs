using System;
using DICUI.Data;
using DICUI.Utilities;
using Xunit;

namespace DICUI.Test.Utilities
{
    public class DICFlagExtensionsTest
    {
        [Fact]
        public void NameTest()
        {
            var values = (DICFlag[])Enum.GetValues(typeof(DICFlag));
            foreach(var command in values)
            {
                string expected = Converters.DICFlagToString(command);
                string actual = command.Name();

                Assert.Equal(expected, actual);
            }
        }
    }
}
