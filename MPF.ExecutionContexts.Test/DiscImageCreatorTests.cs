using MPF.ExecutionContexts.DiscImageCreator;
using SabreTools.RedumpLib.Data;
using Xunit;

namespace MPF.ExecutionContexts.Test
{
    public class DiscImageCreatorTests
    {
        // TODO: Add Converters.ToRedumpSystem test
        // TODO: Add Converters.ToMediaType test

        #region Converters.Extension

        [Theory]
        [InlineData(null, null)]
        [InlineData(MediaType.CDROM, ".bin")]
        [InlineData(MediaType.GDROM, ".bin")]
        [InlineData(MediaType.Cartridge, ".bin")]
        [InlineData(MediaType.HardDisk, ".bin")]
        [InlineData(MediaType.CompactFlash, ".bin")]
        [InlineData(MediaType.MMC, ".bin")]
        [InlineData(MediaType.SDCard, ".bin")]
        [InlineData(MediaType.FlashDrive, ".bin")]
        [InlineData(MediaType.DVD, ".iso")]
        [InlineData(MediaType.HDDVD, ".iso")]
        [InlineData(MediaType.BluRay, ".iso")]
        [InlineData(MediaType.NintendoWiiOpticalDisc, ".iso")]
        [InlineData(MediaType.LaserDisc, ".raw")]
        [InlineData(MediaType.NintendoGameCubeGameDisc, ".raw")]
        [InlineData(MediaType.NintendoWiiUOpticalDisc, ".wud")]
        [InlineData(MediaType.FloppyDisk, ".img")]
        [InlineData(MediaType.Cassette, ".wav")]
        [InlineData(MediaType.ApertureCard, null)]
        public void ExtensionTest(MediaType? type, string? expected)
        {
            string? actual = Converters.Extension(type);
            Assert.Equal(expected, actual);
        }

        #endregion

        #region Audio

        [Theory]
        [InlineData("audio F filename.bin 0 1 2 /be raw /c2 1 2 3 1 5 6 /d8 /d /q /f 0 /np /nq /nr /r /am /sf 1 /ss /sk 1 0 /s 0 /t")]
        public void AudioTest(string parameters)
        {
            string? expected = "audio F \"filename.bin\" 0 1 2 /be raw /c2 1 2 3 1 5 6 /d8 /d /q /f 0 /np /nq /nr /r /am /sf 1 /ss /sk 1 0 /s 0 /t";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        #endregion

        #region BluRay

        [Theory]
        [InlineData("bd F filename.bin 0 /d /q /rr 0 /f 0 /ra /avdp")]
        public void BluRayTest(string parameters)
        {
            string? expected = "bd F \"filename.bin\" 0 /d /q /rr 0 /f 0 /ra /avdp";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        #endregion

        #region Close

        [Theory]
        [InlineData("close f")]
        public void CloseTest(string parameters)
        {
            string? expected = "close f";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        #endregion

        #region CompactDisc

        [Theory]
        [InlineData("cd f filename.bin 0 /a 0 /p /aj /be raw /c2 1 2 3 1 5 6 /d8 /d /q /mscf /f 0 /mr 0 /np /nq /nl /ns /nr /am /sf 1 /ss /74 /s 0 /trp /vn 0 /vnc /vnx")]
        public void CompactDiscTest(string parameters)
        {
            string? expected = "cd f \"filename.bin\" 0 /a 0 /p /aj /be raw /c2 1 2 3 1 5 6 /d8 /d /q /mscf /f 0 /mr 0 /np /nq /nl /ns /nr /am /sf 1 /ss /74 /s 0 /trp /vn 0 /vnc /vnx";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        #endregion

        #region Data

        [Theory]
        [InlineData("data F filename.bin 0 1 2 /be raw /c2 1 2 3 1 5 6 /d8 /d /q /f 0 /np /nq /nr /r /am /sf 1 /ss /sk 1 0 /s 0 /t")]
        public void DataTest(string parameters)
        {
            string? expected = "data F \"filename.bin\" 0 1 2 /be raw /c2 1 2 3 1 5 6 /d8 /d /q /f 0 /np /nq /nr /r /am /sf 1 /ss /sk 1 0 /s 0 /t";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        #endregion

        #region DigitalVideoDisc

        [Theory]
        [InlineData("dvd F filename.bin 0 /c /d /q /rr 0 /fix 0 /ps 0 /ra 0 1 /raw /re /r 0 1 /sf 1 /sk 1 0 /avdp")]
        public void DigitalVideoDiscTest(string parameters)
        {
            string? expected = "dvd F \"filename.bin\" 0 /c /d /q /rr 0 /fix 0 /ps 0 /ra 0 1 /raw /re /r 0 1 /sf 1 /sk 1 0 /avdp";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        #endregion

        #region Disk

        [Theory]
        [InlineData("disk F filename.bin /d")]
        public void DiskTest(string parameters)
        {
            string? expected = "disk F \"filename.bin\" /d";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        #endregion

        #region DriveSpeed

        [Theory]
        [InlineData("ls f")]
        public void DriveSpeedTest(string parameters)
        {
            string? expected = "ls f";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        #endregion

        #region Eject

        [Theory]
        [InlineData("eject f")]
        public void EjectTest(string parameters)
        {
            string? expected = "eject f";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        #endregion

        #region Floppy

        [Theory]
        [InlineData("fd F filename.bin /d")]
        public void FloppyTest(string parameters)
        {
            string? expected = "fd F \"filename.bin\" /d";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        #endregion

        #region GDROM

        [Theory]
        [InlineData("gd f filename.bin 0 /be raw /c2 1 2 3 1 5 6 /d8 /d /q /f 0 /np /nq /nr /s 0")]
        public void GDROMTest(string parameters)
        {
            string? expected = "gd f \"filename.bin\" 0 /be raw /c2 1 2 3 1 5 6 /d8 /d /q /f 0 /np /nq /nr /s 0";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        #endregion

        #region MDS

        [Theory]
        [InlineData("mds filename.bin")]
        public void MDSTest(string parameters)
        {
            string? expected = "mds \"filename.bin\"";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        #endregion

        #region Merge

        [Theory]
        [InlineData("merge input1.bin input2.bin")]
        public void MergeTest(string parameters)
        {
            string? expected = "merge \"input1.bin\" \"input2.bin\"";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        #endregion

        #region Reset

        [Theory]
        [InlineData("reset f")]
        public void ResetTest(string parameters)
        {
            string? expected = "reset f";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        #endregion

        #region SACD

        [Theory]
        [InlineData("sacd f filename.bin 0 /d /q")]
        public void SACDTest(string parameters)
        {
            string? expected = "sacd f \"filename.bin\" 0 /d /q";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        #endregion

        #region Start

        [Theory]
        [InlineData("start f")]
        public void StartTest(string parameters)
        {
            string? expected = "start f";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        #endregion

        #region Stop

        [Theory]
        [InlineData("stop f")]
        public void StopTest(string parameters)
        {
            string? expected = "stop f";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        #endregion

        #region Sub

        [Theory]
        [InlineData("sub filename.bin")]
        public void SubTest(string parameters)
        {
            string? expected = "sub \"filename.bin\"";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        #endregion

        #region Swap

        [Theory]
        [InlineData("swap f filename.bin 0 /a 0 /be raw /c2 1 2 3 1 5 6 /d8 /d /q /f 0 /np /nq /nl /ns /nr /am /sf 1 /ss /74 /s 0 /trp /vn 0 /vnc /vnx")]
        public void SwapTest(string parameters)
        {
            string? expected = "swap f \"filename.bin\" 0 /a 0 /be raw /c2 1 2 3 1 5 6 /d8 /d /q /f 0 /np /nq /nl /ns /nr /am /sf 1 /ss /74 /s 0 /trp /vn 0 /vnc /vnx";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        #endregion

        #region Tape

        [Theory]
        [InlineData("tape filename.bin")]
        public void TapeTest(string parameters)
        {
            string? expected = "tape \"filename.bin\"";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        #endregion

        #region Version

        [Theory]
        [InlineData("/v")]
        public void VersionTest(string parameters)
        {
            string? expected = "/v";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        #endregion

        #region XBOX

        [Theory]
        [InlineData("xbox f filename.bin 0 /d /q /rr 0 /f 0 /nss 0")]
        public void XBOXTest(string parameters)
        {
            string? expected = "xbox f \"filename.bin\" 0 /d /q /rr 0 /f 0 /nss 0";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        #endregion

        #region XBOXSwap

        [Theory]
        [InlineData("xboxswap f filename.bin 0 /d /q /f 0 /nss 0")]
        public void XBOXSwapTest(string parameters)
        {
            string? expected = "xboxswap f \"filename.bin\" 0 /d /q /f 0 /nss 0";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        #endregion

        #region XGD2Swap

        [Theory]
        [InlineData("xgd2swap f filename.bin 0 /d /q /f 0 /nss 0")]
        public void XGD2SwapTest(string parameters)
        {
            string? expected = "xgd2swap f \"filename.bin\" 0 /d /q /f 0 /nss 0";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        #endregion

        #region XGD3Swap

        [Theory]
        [InlineData("xgd3swap f filename.bin 0 /d /q /f 0 /nss 0")]
        public void XGD3SwapTest(string parameters)
        {
            string? expected = "xgd3swap f \"filename.bin\" 0 /d /q /f 0 /nss 0";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        #endregion
    }
}
