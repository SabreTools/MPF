using MPF.ExecutionContexts.Aaru;
using SabreTools.RedumpLib.Data;
using Xunit;

namespace MPF.ExecutionContexts.Test
{
    public class AaruTests
    {
        #region Converters.Extension

        [Theory]
        [InlineData(null, ".aaruf")]
        [InlineData(MediaType.CDROM, ".aaruf")]
        [InlineData(MediaType.GDROM, ".aaruf")]
        [InlineData(MediaType.DVD, ".aaruf")]
        [InlineData(MediaType.HDDVD, ".aaruf")]
        [InlineData(MediaType.BluRay, ".aaruf")]
        [InlineData(MediaType.FloppyDisk, ".aaruf")]
        [InlineData(MediaType.HardDisk, ".aaruf")]
        [InlineData(MediaType.ApertureCard, ".aaruf")]
        public void ExtensionTest(MediaType? type, string expected)
        {
            string actual = Converters.Extension(type);
            Assert.Equal(expected, actual);
        }

        #endregion

        /// <summary>
        /// Ensure pre-command flags can all be set properly
        /// </summary>
        [Theory]
        [InlineData("--debug --help --verbose --version formats")]
        [InlineData("--debug true --help true --verbose true --version true formats")]
        public void PreCommandFlagsTest(string parameters)
        {
            string? expected = "--debug true --help true --verbose true --version true formats";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }
    }
}