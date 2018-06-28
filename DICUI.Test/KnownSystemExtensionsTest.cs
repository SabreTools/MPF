using DICUI.Data;
using DICUI.Utilities;
using Xunit;

namespace DICUI.Test
{
    public class KnownSystemExtensionsTest
    {
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
    }
}
