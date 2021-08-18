using System;
using MPF.Converters;
using MPF.Data;
using MPF.Utilities;
using Xunit;

namespace MPF.Test.Converters
{
    public class KnownSystemExtensionsTest
    {
        [Theory]
        [InlineData(KnownSystem.MicrosoftXBOX, "Microsoft XBOX")]
        [InlineData(KnownSystem.NECPC88, "NEC PC-88")]
        [InlineData(KnownSystem.KonamiPython, "Konami Python")]
        [InlineData(KnownSystem.HDDVDVideo, "HD-DVD-Video")]
        [InlineData(KnownSystem.NONE, "Unknown")]
        public void KnownSystemToStringTest(KnownSystem? knownSystem, string expected)
        {
            string actual = EnumConverter.LongName(knownSystem);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsMarkerTest()
        {
            var values = (KnownSystem[])Enum.GetValues(typeof(KnownSystem));
            foreach(var system in values)
            {
                bool expected = system == KnownSystem.MarkerArcadeEnd || system == KnownSystem.MarkerComputerEnd ||
                                system == KnownSystem.MarkerOtherEnd || system == KnownSystem.MarkerDiscBasedConsoleEnd;
                                // || system == KnownSystem.MarkerOtherConsoleEnd;

                bool actual = ((KnownSystem?)system).IsMarker();

                Assert.Equal(expected, actual);
            }
        }

        [Fact]
        public void CategoryNameNotEmptyTest()
        {
            var values = (KnownSystemCategory[])Enum.GetValues(typeof(KnownSystemCategory));
            foreach (var system in values)
            {
                string actual = ((KnownSystem?)system).LongName();
                Assert.NotEqual("", actual);
            }
        }
    }
}
