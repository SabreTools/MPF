using MPF.Core.Data;
using Xunit;

namespace MPF.Test.Core.Data
{
    public class XgdInfoTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("1234567")]
        [InlineData("1234567\0")]
        [InlineData("123456789")]
        public void UnmatchedStringTests(string invalidString)
        {
            XgdInfo xgdInfo = new XgdInfo(invalidString);
            Assert.False(xgdInfo.Initialized);
        }

        [Theory]
        [InlineData("AV00100W", "AV", "001", "00", 'W')]
        [InlineData("AV00100W\0", "AV", "001", "00", 'W')]
        public void XGD1ValidTests(string validString, string publisher, string gameId, string version, char regionIdentifier)
        {
            XgdInfo xgdInfo = new XgdInfo(validString);

            Assert.True(xgdInfo.Initialized);
            Assert.Equal(publisher, xgdInfo.XMID.PublisherIdentifier);
            Assert.Equal(gameId, xgdInfo.XMID.GameID);
            Assert.Equal(version, xgdInfo.XMID.VersionNumber);
            Assert.Equal(regionIdentifier, xgdInfo.XMID.RegionIdentifier);
        }

        [Theory]
        // Invalid publisher identifier
        [InlineData("ZZ000000")]
        [InlineData("ZZ000000\0")]
        // Invalid region identifier
        [InlineData("AV00000Z")]
        [InlineData("AV00000Z\0")]
        public void XGD1InvalidTests(string invalidString)
        {
            XgdInfo xgdInfo = new XgdInfo(invalidString);
            Assert.False(xgdInfo.Initialized);
        }

        [Theory]
        [InlineData("AV200100W0F11", "AV", "001", "00", 'W', "0", 'F', "11", null)]
        [InlineData("AV200100W0F11\0", "AV", "001", "00", 'W', "0", 'F', "11", null)]
        [InlineData("AV200100W01F11", "AV", "001", "00", 'W', "01", 'F', "11", null)]
        [InlineData("AV200100W01F11\0", "AV", "001", "00", 'W', "01", 'F', "11", null)]
        [InlineData("AV200100W0F11DEADBEEF", "AV", "001", "00", 'W', "0", 'F', "11", "DEADBEEF")]
        [InlineData("AV200100W0F11DEADBEEF\0", "AV", "001", "00", 'W', "0", 'F', "11", "DEADBEEF")]
        [InlineData("AV200100W01F11DEADBEEF", "AV", "001", "00", 'W', "01", 'F', "11", "DEADBEEF")]
        [InlineData("AV200100W01F11DEADBEEF\0", "AV", "001", "00", 'W', "01", 'F', "11", "DEADBEEF")]
        public void XGD23ValidTests(string validString, string publisher, string gameId, string sku, char regionIdentifier, string baseVersion, char mediaSubtype, string discNumber, string cert)
        {
            XgdInfo xgdInfo = new XgdInfo(validString);

            Assert.True(xgdInfo.Initialized);
            Assert.Equal(publisher, xgdInfo.XeMID.PublisherIdentifier);
            Assert.Equal('2', xgdInfo.XeMID.PlatformIdentifier);
            Assert.Equal(gameId, xgdInfo.XeMID.GameID);
            Assert.Equal(sku, xgdInfo.XeMID.SKU);
            Assert.Equal(regionIdentifier, xgdInfo.XeMID.RegionIdentifier);
            Assert.Equal(baseVersion, xgdInfo.XeMID.BaseVersion);
            Assert.Equal(mediaSubtype, xgdInfo.XeMID.MediaSubtypeIdentifier);
            Assert.Equal(discNumber, xgdInfo.XeMID.DiscNumberIdentifier);
            Assert.Equal(cert, xgdInfo.XeMID.CertificationSubmissionIdentifier);
        }

        [Theory]
        // Invalid publisher identifier
        [InlineData("ZZ00000000000")]
        [InlineData("ZZ00000000000\0")]
        [InlineData("ZZ000000000000")]
        [InlineData("ZZ000000000000\0")]
        [InlineData("ZZ0000000000000000000")]
        [InlineData("ZZ0000000000000000000\0")]
        [InlineData("ZZ00000000000000000000")]
        [InlineData("ZZ00000000000000000000\0")]
        // Invalid platform identifier
        [InlineData("AV90000000000")]
        [InlineData("AV90000000000\0")]
        [InlineData("AV900000000000")]
        [InlineData("AV900000000000\0")]
        [InlineData("AV9000000000000000000")]
        [InlineData("AV9000000000000000000\0")]
        [InlineData("AV90000000000000000000")]
        [InlineData("AV90000000000000000000\0")]
        // Invalid region identifier
        [InlineData("AV200000Z0000")]
        [InlineData("AV200000Z0000\0")]
        [InlineData("AV200000Z00000")]
        [InlineData("AV200000Z00000\0")]
        [InlineData("AV200000Z000000000000")]
        [InlineData("AV200000Z000000000000\0")]
        [InlineData("AV200000Z0000000000000")]
        [InlineData("AV200000Z0000000000000\0")]
        // Invalid media subtype identifier
        [InlineData("AV200000W0A00")]
        [InlineData("AV200000W0A00\0")]
        [InlineData("AV200000W00A00")]
        [InlineData("AV200000W00A00\0")]
        [InlineData("AV200000W00A000000000")]
        [InlineData("AV200000W00A000000000\0")]
        [InlineData("AV200000W00A0000000000")]
        [InlineData("AV200000W00A0000000000\0")]
        public void XGD23InvalidTests(string invalidString)
        {
            XgdInfo xgdInfo = new XgdInfo(invalidString);
            Assert.False(xgdInfo.Initialized);
        }
    }
}
