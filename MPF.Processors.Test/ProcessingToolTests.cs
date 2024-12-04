using System;
using System.IO;
using SabreTools.Models.Logiqx;
using SabreTools.Models.PIC;
using SabreTools.RedumpLib.Data;
using Xunit;

namespace MPF.Processors.Test
{
    // TODO: Write tests for PlayStation 3 specific tools
    // TODO: Write tests for Xbox and Xbox 360
    public class ProcessingToolTests
    {
        #region GenerateDatfile

        [Fact]
        public void GenerateDatfile_Null_Null()
        {
            Datafile? datafile = null;
            string? actual = ProcessingTool.GenerateDatfile(datafile);
            Assert.Null(actual);
        }

        [Fact]
        public void GenerateDatfile_Invalid_Null()
        {
            Datafile? datafile = new Datafile();
            string? actual = ProcessingTool.GenerateDatfile(datafile);
            Assert.Null(actual);
        }

        [Fact]
        public void GenerateDatfile_Valid_Filled()
        {
            string? expected = "<rom name=\"test\" size=\"12345\" crc=\"00000000\" md5=\"d41d8cd98f00b204e9800998ecf8427e\" sha1=\"da39a3ee5e6b4b0d3255bfef95601890afd80709\" />";
            Rom rom = new Rom
            {
                Name = "test",
                Size = "12345",
                CRC = "00000000",
                MD5 = "d41d8cd98f00b204e9800998ecf8427e",
                SHA1 = "da39a3ee5e6b4b0d3255bfef95601890afd80709",
            };
            Game game = new Game { Rom = [rom] };
            Datafile? datafile = new Datafile { Game = [game] };

            string? actual = ProcessingTool.GenerateDatfile(datafile);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetBase64

        [Fact]
        public void GetBase64_Null_Null()
        {
            string? content = null;
            string? actual = ProcessingTool.GetBase64(content);
            Assert.Null(actual);
        }

        [Fact]
        public void GetBase64_Empty_Null()
        {
            string? content = string.Empty;
            string? actual = ProcessingTool.GetBase64(content);
            Assert.Null(actual);
        }

        [Fact]
        public void GetBase64_Valid_Filled()
        {
            string? expected = "MTIzNDVBQkNERQ==";
            string? content = "12345ABCDE";
            string? actual = ProcessingTool.GetBase64(content);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetDatafile

        [Fact]
        public void GetDatafile_Null_Null()
        {
            string? dat = null;
            Datafile? actual = ProcessingTool.GetDatafile(dat);
            Assert.Null(actual);
        }

        [Fact]
        public void GetDatafile_Empty_Null()
        {
            string? dat = string.Empty;
            Datafile? actual = ProcessingTool.GetDatafile(dat);
            Assert.Null(actual);
        }

        [Fact]
        public void GetDatafile_Valid_Filled()
        {
            string? dat = Path.Combine(Environment.CurrentDirectory, "TestData", "ProcessingTool", "datfile.xml");
            Datafile? actual = ProcessingTool.GetDatafile(dat);

            // TODO: Add structure validation
            Assert.NotNull(actual);
        }

        #endregion

        #region GetDiscInformation

        // TODO: Figure out how to mock a PIC file

        #endregion

        #region GetFileModifiedDate

        // TODO: Figure out how to get a statically-dated file

        #endregion

        #region GetFullFile

        [Fact]
        public void GetFullFile_Empty_Null()
        {
            string filename = string.Empty;
            string? actual = ProcessingTool.GetFullFile(filename);
            Assert.Null(actual);
        }

        [Fact]
        public void GetFullFile_Invalid_Null()
        {
            string filename = "INVALID";
            string? actual = ProcessingTool.GetFullFile(filename);
            Assert.Null(actual);
        }

        [Fact]
        public void GetFullFile_ValidBinary_Filled()
        {
            string? expected = "544553542044415441";
            string filename = Path.Combine(Environment.CurrentDirectory, "TestData", "ProcessingTool", "textfile.txt");
            string? actual = ProcessingTool.GetFullFile(filename, binary: true);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GetFullFile_ValidText_Filled()
        {
            string? expected = "TEST DATA";
            string filename = Path.Combine(Environment.CurrentDirectory, "TestData", "ProcessingTool", "textfile.txt");
            string? actual = ProcessingTool.GetFullFile(filename, binary: false);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetISOHashValues

        [Fact]
        public void GetISOHashValues_Datafile_Null_Null()
        {
            Datafile? datafile = null;
            bool actual = ProcessingTool.GetISOHashValues(datafile,
                out long size,
                out string? crc32,
                out string? md5,
                out string? sha1);

            Assert.False(actual);
            Assert.Equal(-1, size);
            Assert.Null(crc32);
            Assert.Null(md5);
            Assert.Null(sha1);
        }

        [Fact]
        public void GetISOHashValues_Datafile_Empty_Null()
        {
            Datafile? datafile = new Datafile();
            bool actual = ProcessingTool.GetISOHashValues(datafile,
                out long size,
                out string? crc32,
                out string? md5,
                out string? sha1);

            Assert.False(actual);
            Assert.Equal(-1, size);
            Assert.Null(crc32);
            Assert.Null(md5);
            Assert.Null(sha1);
        }

        [Fact]
        public void GetISOHashValues_Datafile_Valid_Filled()
        {
            long expectedSize = 12345;
            string? expectedCrc32 = "00000000";
            string? expectedMd5 = "d41d8cd98f00b204e9800998ecf8427e";
            string? expectedSha1 = "da39a3ee5e6b4b0d3255bfef95601890afd80709";

            Rom rom = new Rom
            {
                Name = "test",
                Size = "12345",
                CRC = "00000000",
                MD5 = "d41d8cd98f00b204e9800998ecf8427e",
                SHA1 = "da39a3ee5e6b4b0d3255bfef95601890afd80709",
            };
            Game game = new Game { Rom = [rom] };
            Datafile? datafile = new Datafile { Game = [game] };

            bool actual = ProcessingTool.GetISOHashValues(datafile,
                out long size,
                out string? crc32,
                out string? md5,
                out string? sha1);

            Assert.True(actual);
            Assert.Equal(expectedSize, size);
            Assert.Equal(expectedCrc32, crc32);
            Assert.Equal(expectedMd5, md5);
            Assert.Equal(expectedSha1, sha1);
        }

        [Fact]
        public void GetISOHashValues_String_Null_Null()
        {
            string? hashData = null;
            bool actual = ProcessingTool.GetISOHashValues(hashData,
                out long size,
                out string? crc32,
                out string? md5,
                out string? sha1);

            Assert.False(actual);
            Assert.Equal(-1, size);
            Assert.Null(crc32);
            Assert.Null(md5);
            Assert.Null(sha1);
        }

        [Fact]
        public void GetISOHashValues_String_Empty_Null()
        {
            string? hashData = string.Empty;
            bool actual = ProcessingTool.GetISOHashValues(hashData,
                out long size,
                out string? crc32,
                out string? md5,
                out string? sha1);

            Assert.False(actual);
            Assert.Equal(-1, size);
            Assert.Null(crc32);
            Assert.Null(md5);
            Assert.Null(sha1);
        }

        [Fact]
        public void GetISOHashValues_String_Invalid_Filled()
        {
            string? hashData = "INVALID";
            bool actual = ProcessingTool.GetISOHashValues(hashData,
                out long size,
                out string? crc32,
                out string? md5,
                out string? sha1);

            Assert.False(actual);
            Assert.Equal(-1, size);
            Assert.Null(crc32);
            Assert.Null(md5);
            Assert.Null(sha1);
        }

        [Fact]
        public void GetISOHashValues_String_Valid_Filled()
        {
            long expectedSize = 12345;
            string? expectedCrc32 = "00000000";
            string? expectedMd5 = "d41d8cd98f00b204e9800998ecf8427e";
            string? expectedSha1 = "da39a3ee5e6b4b0d3255bfef95601890afd80709";

            string? hashData = "<rom name=\"test\" size=\"12345\" crc=\"00000000\" md5=\"d41d8cd98f00b204e9800998ecf8427e\" sha1=\"da39a3ee5e6b4b0d3255bfef95601890afd80709\" />";
            bool actual = ProcessingTool.GetISOHashValues(hashData,
                out long size,
                out string? crc32,
                out string? md5,
                out string? sha1);

            Assert.True(actual);
            Assert.Equal(expectedSize, size);
            Assert.Equal(expectedCrc32, crc32);
            Assert.Equal(expectedMd5, md5);
            Assert.Equal(expectedSha1, sha1);
        }

        #endregion

        #region GetLayerbreaks

        [Fact]
        public void GetLayerbreaks_Null_Null()
        {
            DiscInformation? di = null;
            bool actual = ProcessingTool.GetLayerbreaks(di,
                out long? layerbreak1,
                out long? layerbreak2,
                out long? layerbreak3);

            Assert.False(actual);
            Assert.Null(layerbreak1);
            Assert.Null(layerbreak2);
            Assert.Null(layerbreak3);
        }

        [Fact]
        public void GetLayerbreaks_Empty_Null()
        {
            DiscInformation? di = new DiscInformation();
            bool actual = ProcessingTool.GetLayerbreaks(di,
                out long? layerbreak1,
                out long? layerbreak2,
                out long? layerbreak3);

            Assert.False(actual);
            Assert.Null(layerbreak1);
            Assert.Null(layerbreak2);
            Assert.Null(layerbreak3);
        }

        [Fact]
        public void GetLayerbreaks_Valid_Filled()
        {
            long? expectedLayerbreak1 = 67372038;
            long? expectedLayerbreak2 = 134744076;
            long? expectedLayerbreak3 = 202116114;

            DiscInformationUnit layer0 = new DiscInformationUnit
            {
                Body = new DiscInformationUnitBody
                {
                    FormatDependentContents = [0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08],
                }
            };
            DiscInformationUnit layer1 = new DiscInformationUnit
            {
                Body = new DiscInformationUnitBody
                {
                    FormatDependentContents = [0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08],
                }
            };
            DiscInformationUnit layer2 = new DiscInformationUnit
            {
                Body = new DiscInformationUnitBody
                {
                    FormatDependentContents = [0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08],
                }
            };
            DiscInformationUnit layer3 = new DiscInformationUnit
            {
                Body = new DiscInformationUnitBody
                {
                    FormatDependentContents = [0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08],
                }
            };
            DiscInformation? di = new DiscInformation
            {
                Units = [layer0, layer1, layer2, layer3],
            };

            bool actual = ProcessingTool.GetLayerbreaks(di,
                out long? layerbreak1,
                out long? layerbreak2,
                out long? layerbreak3);

            Assert.True(actual);
            Assert.Equal(expectedLayerbreak1, layerbreak1);
            Assert.Equal(expectedLayerbreak2, layerbreak2);
            Assert.Equal(expectedLayerbreak3, layerbreak3);
        }

        #endregion

        #region GetPICIdentifier

        [Fact]
        public void GetPICIdentifier_Null_Null()
        {
            DiscInformation? di = null;
            string? actual = ProcessingTool.GetPICIdentifier(di);
            Assert.Null(actual);
        }

        [Fact]
        public void GetPICIdentifier_Empty_Null()
        {
            DiscInformation? di = new DiscInformation();
            string? actual = ProcessingTool.GetPICIdentifier(di);
            Assert.Null(actual);
        }

        [Fact]
        public void GetPICIdentifier_Valid_Filled()
        {
            string? expected = "UHD";
            DiscInformationUnit layer0 = new DiscInformationUnit
            {
                Body = new DiscInformationUnitBody
                {
                    DiscTypeIdentifier = "UHD",
                }
            };
            DiscInformation? di = new DiscInformation
            {
                Units = [layer0],
            };
            string? actual = ProcessingTool.GetPICIdentifier(di);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetUMDCategory

        [Theory]
        [InlineData(null, null)]
        [InlineData("", null)]
        [InlineData("GAME", DiscCategory.Games)]
        [InlineData("game", DiscCategory.Games)]
        [InlineData("VIDEO", DiscCategory.Video)]
        [InlineData("video", DiscCategory.Video)]
        [InlineData("AUDIO", DiscCategory.Audio)]
        [InlineData("audio", DiscCategory.Audio)]
        [InlineData("INVALID", null)]
        public void GetUMDCategoryTest(string? category, DiscCategory? expected)
        {
            DiscCategory? actual = ProcessingTool.GetUMDCategory(category);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetPlayStationRegion

        [Theory]
        [InlineData(null, null)]
        [InlineData("", null)]
        [InlineData("S_A", Region.Asia)]
        [InlineData("S_C", Region.China)]
        [InlineData("S_E", Region.Europe)]
        [InlineData("S_K", Region.SouthKorea)]
        [InlineData("S_U", Region.UnitedStatesOfAmerica)]
        [InlineData("S_PS_46", Region.SouthKorea)]
        [InlineData("S_PS_51", Region.Asia)]
        [InlineData("S_PS_56", Region.SouthKorea)]
        [InlineData("S_PS_55", Region.Asia)]
        [InlineData("S_PS_XX", Region.Japan)]
        [InlineData("S_PM_645", Region.SouthKorea)]
        [InlineData("S_PM_675", Region.SouthKorea)]
        [InlineData("S_PM_885", Region.SouthKorea)]
        [InlineData("S_PM_XXX", Region.Japan)]
        [InlineData("S_PX", Region.Japan)]
        [InlineData("PAPX", Region.Japan)]
        [InlineData("PABX", null)]
        [InlineData("PBPX", null)]
        [InlineData("PCBX", Region.Japan)]
        [InlineData("PCXC", Region.Japan)]
        [InlineData("PDBX", Region.Japan)]
        [InlineData("PEBX", Region.Europe)]
        [InlineData("PUBX", Region.UnitedStatesOfAmerica)]
        public void GetPlayStationRegionTest(string? serial, Region? expected)
        {
            Region? actual = ProcessingTool.GetPlayStationRegion(serial);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region GetXGDRegion

        [Theory]
        [InlineData(null, null)]
        [InlineData(' ', null)]
        [InlineData('W', Region.World)]
        [InlineData('A', Region.UnitedStatesOfAmerica)]
        [InlineData('J', Region.JapanAsia)]
        [InlineData('E', Region.Europe)]
        [InlineData('K', Region.USAJapan)]
        [InlineData('L', Region.USAEurope)]
        [InlineData('H', Region.JapanEurope)]
        [InlineData('X', null)]
        public void GetXGDRegionTest(char? region, Region? expected)
        {
            Region? actual = ProcessingTool.GetXGDRegion(region);
            Assert.Equal(expected, actual);
        }

        #endregion
    }
}
