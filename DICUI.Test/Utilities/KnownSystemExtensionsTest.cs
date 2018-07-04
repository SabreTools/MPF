using System;
using DICUI.Data;
using DICUI.Utilities;
using Xunit;

namespace DICUI.Test.Utilities
{
    public class KnownSystemExtensionsTest
    {
        [Fact]
        public void NameTest()
        {
            var values = (KnownSystem[])Enum.GetValues(typeof(KnownSystem));

            Array.ForEach(values, system =>
            {
                string expected = Converters.KnownSystemToString(system);
                string actual = ((KnownSystem?)system).Name();

                Assert.Equal(expected, actual);
            });            
        }

        [Theory]
        [InlineData(KnownSystem.AppleMacintosh, true)]
        [InlineData(KnownSystem.MicrosoftXBOX, false)]
        [InlineData(KnownSystem.MicrosoftXBOX360XDG2, false)]
        [InlineData(KnownSystem.MicrosoftXBOX360XDG3, false)]
        [InlineData(KnownSystem.SonyPlayStation3, true)]
        public void DriveSpeedSupportedTest(KnownSystem? knownSystem, bool expected)
        {
            bool actual = knownSystem.DoesSupportDriveSpeed();
            Assert.Equal(expected, actual);
        }


        [Fact]
        public void IsMarkerTest()
        {
            var values = (KnownSystem[])Enum.GetValues(typeof(KnownSystem));

            Array.ForEach(values, system =>
            {
                bool expected = system == KnownSystem.MarkerArcadeEnd || system == KnownSystem.MarkerComputerEnd ||
                                system == KnownSystem.MarkerOtherEnd || system == KnownSystem.MarkerConsoleEnd;

                bool actual = ((KnownSystem?)system).IsMarker();

                Assert.Equal(expected, actual);
            });
        }

        [Fact]
        public void CategoryNameNotEmptyTest()
        {
            var values = (KnownSystemCategory[])Enum.GetValues(typeof(KnownSystemCategory));

            Array.ForEach(values, system =>
            {
                string actual = ((KnownSystem?)system).Name();

                Assert.NotEqual("", actual);
            });
        }



    }
}
