using System;
using MPF.Converters;
using MPF.Data;
using MPF.Utilities;
using RedumpLib.Data;
using Xunit;

namespace MPF.Test.Converters
{
    public class RedumpSystemExtensionsTest
    {
        [Theory]
        [InlineData(RedumpSystem.MicrosoftXbox, "Microsoft Xbox")]
        [InlineData(RedumpSystem.NECPC88series, "NEC PC-88 series")]
        [InlineData(RedumpSystem.KonamiPython, "Konami Python")]
        [InlineData(RedumpSystem.HDDVDVideo, "HD DVD-Video")]
        [InlineData(null, null)]
        public void RedumpSystemToStringTest(RedumpSystem? knownSystem, string expected)
        {
            string actual = Extensions.LongName(knownSystem);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsMarkerTest()
        {
            var values = (RedumpSystem[])Enum.GetValues(typeof(RedumpSystem));
            foreach(var system in values)
            {
                bool expected = system == RedumpSystem.MarkerArcadeEnd || system == RedumpSystem.MarkerComputerEnd ||
                                system == RedumpSystem.MarkerOtherEnd || system == RedumpSystem.MarkerDiscBasedConsoleEnd;
                                // || system == RedumpSystem.MarkerOtherConsoleEnd;

                bool actual = ((RedumpSystem?)system).IsMarker();

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void CategoryNameNotEmptyTest()
        {
            var values = (SystemCategory[])Enum.GetValues(typeof(SystemCategory));
            foreach (var system in values)
            {
                string actual = ((RedumpSystem?)system).LongName();
                Assert.NotEqual("", actual);
            }
        }
    }
}
