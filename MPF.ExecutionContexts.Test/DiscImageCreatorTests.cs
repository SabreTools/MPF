using System.Collections.Generic;
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

        #region Default Values

        private static Dictionary<string, string?> AllOptions = new()
        {
            [SettingConstants.DVDRereadCount] = "1000",
            [SettingConstants.MultiSectorRead] = "true",
            [SettingConstants.MultiSectorReadValue] = "1000",
            [SettingConstants.ParanoidMode] = "true",
            [SettingConstants.QuietMode] = "true",
            [SettingConstants.RereadCount] = "1000",
            [SettingConstants.UseCMIFlag] = "true",
        };

        [Theory]
        [InlineData(null, null, null, "filename.bin", null, null)]
        [InlineData(RedumpSystem.AppleMacintosh, MediaType.CDROM, "/dev/sr0", "filename.bin", 2, "cd /dev/sr0 \"filename.bin\" 2 /c2 1000 /q /mr 1000 /ns /sf /ss /s 2")]
        [InlineData(RedumpSystem.IBMPCcompatible, MediaType.CDROM, "/dev/sr0", "filename.bin", 2, "cd /dev/sr0 \"filename.bin\" 2 /c2 1000 /q /mr 1000 /ns /sf /ss /s 2")]
        [InlineData(RedumpSystem.AtariJaguarCDInteractiveMultimediaSystem, MediaType.CDROM, "/dev/sr0", "filename.bin", 2, "cd /dev/sr0 \"filename.bin\" 2 /aj /c2 1000 /q /mr 1000")]
        [InlineData(RedumpSystem.HasbroVideoNow, MediaType.CDROM, "/dev/sr0", "filename.bin", 2, "cd /dev/sr0 \"filename.bin\" 2 /a 0 /c2 1000 /q /mr 1000")]
        [InlineData(RedumpSystem.HasbroVideoNowColor, MediaType.CDROM, "/dev/sr0", "filename.bin", 2, "cd /dev/sr0 \"filename.bin\" 2 /a 0 /c2 1000 /q /mr 1000")]
        [InlineData(RedumpSystem.HasbroVideoNowJr, MediaType.CDROM, "/dev/sr0", "filename.bin", 2, "cd /dev/sr0 \"filename.bin\" 2 /a 0 /c2 1000 /q /mr 1000")]
        [InlineData(RedumpSystem.HasbroVideoNowXP, MediaType.CDROM, "/dev/sr0", "filename.bin", 2, "cd /dev/sr0 \"filename.bin\" 2 /a 0 /c2 1000 /q /mr 1000")]
        [InlineData(RedumpSystem.SonyPlayStation, MediaType.CDROM, "/dev/sr0", "filename.bin", 2, "cd /dev/sr0 \"filename.bin\" 2 /c2 1000 /q /mr 1000 /nl /am")]
        [InlineData(RedumpSystem.IBMPCcompatible, MediaType.DVD, "/dev/sr0", "filename.bin", 2, "dvd /dev/sr0 \"filename.bin\" 2 /c /q /rr 1000 /sf")]
        [InlineData(RedumpSystem.MicrosoftXbox, MediaType.DVD, "/dev/sr0", "filename.bin", 2, "xbox /dev/sr0 \"filename.bin\" 2 /q /rr 1000")]
        [InlineData(RedumpSystem.MicrosoftXbox360, MediaType.DVD, "/dev/sr0", "filename.bin", 2, "xbox /dev/sr0 \"filename.bin\" 2 /q /rr 1000")]
        [InlineData(RedumpSystem.NintendoGameCube, MediaType.NintendoGameCubeGameDisc, "/dev/sr0", "filename.bin", 2, "dvd /dev/sr0 \"filename.bin\" 2 /q /raw")]
        [InlineData(RedumpSystem.NintendoWii, MediaType.NintendoWiiOpticalDisc, "/dev/sr0", "filename.bin", 2, "dvd /dev/sr0 \"filename.bin\" 2 /q /raw")]
        [InlineData(RedumpSystem.SegaDreamcast, MediaType.GDROM, "/dev/sr0", "filename.bin", 2, "gd /dev/sr0 \"filename.bin\" 2 /c2 1000 /q")]
        [InlineData(RedumpSystem.HDDVDVideo, MediaType.HDDVD, "/dev/sr0", "filename.bin", 2, "dvd /dev/sr0 \"filename.bin\" 2 /c /q /rr 1000")]
        [InlineData(RedumpSystem.BDVideo, MediaType.BluRay, "/dev/sr0", "filename.bin", 2, "bd /dev/sr0 \"filename.bin\" 2 /q /rr 1000")]
        [InlineData(RedumpSystem.NintendoWiiU, MediaType.NintendoWiiUOpticalDisc, "/dev/sr0", "filename.bin", 2, "bd /dev/sr0 \"filename.bin\" 2 /q")]
        [InlineData(RedumpSystem.IBMPCcompatible, MediaType.FloppyDisk, "/dev/sr0", "filename.bin", 2, "fd /dev/sr0 \"filename.bin\"")]
        public void DefaultValueTest(RedumpSystem? system,
            MediaType? type,
            string? drivePath,
            string filename,
            int? driveSpeed,
            string? expected)
        {
            var context = new ExecutionContext(system, type, drivePath, filename, driveSpeed, AllOptions);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        #endregion

        #region Audio

        [Theory]
        [InlineData("audio F filename.bin 0 1 2 /be raw /c2 1 2 3 1 5 6 /c2new 1 /d8 /d /q /f 0 /np /nq /nr /r /am /sf 1 /ss /sk 1 0 /s 0 /t")]
        public void AudioTest(string parameters)
        {
            string? expected = "audio F \"filename.bin\" 0 1 2 /be raw /c2 1 2 3 1 5 6 /c2new 1 /d8 /d /q /f 0 /np /nq /nr /r /am /sf 1 /ss /sk 1 0 /s 0 /t";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
            Assert.True(context.IsDumpingCommand());
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
            Assert.True(context.IsDumpingCommand());
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
            Assert.False(context.IsDumpingCommand());
        }

        #endregion

        #region CompactDisc

        [Theory]
        [InlineData("cd f filename.bin 0 /a 0 /p /aj /be raw /c2 1 2 3 1 5 6 /c2new 1 /d8 /d /q /mscf /fdesc sync edc /f 0 /fulltoc /mr 0 /np /nq /nl /ns /nr /am /sf 1 /ss /74 /s 0 /toc /trp /vn 0 /vnc /vnx")]
        public void CompactDiscTest(string parameters)
        {
            string? expected = "cd f \"filename.bin\" 0 /a 0 /p /aj /be raw /c2 1 2 3 1 5 6 /c2new 1 /d8 /d /q /mscf /fdesc sync edc /f 0 /fulltoc /mr 0 /np /nq /nl /ns /nr /am /sf 1 /ss /74 /s 0 /toc /trp /vn 0 /vnc /vnx";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
            Assert.True(context.IsDumpingCommand());
        }

        #endregion

        #region Data

        [Theory]
        [InlineData("data F filename.bin 0 1 2 /be raw /c2 1 2 3 1 5 6 /c2new 1 /d8 /d /q /f 0 /np /nq /nr /r /am /sf 1 /ss /sk 1 0 /s 0 /t")]
        public void DataTest(string parameters)
        {
            string? expected = "data F \"filename.bin\" 0 1 2 /be raw /c2 1 2 3 1 5 6 /c2new 1 /d8 /d /q /f 0 /np /nq /nr /r /am /sf 1 /ss /sk 1 0 /s 0 /t";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
            Assert.True(context.IsDumpingCommand());
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
            Assert.True(context.IsDumpingCommand());
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
            Assert.True(context.IsDumpingCommand());
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
            Assert.False(context.IsDumpingCommand());
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
            Assert.False(context.IsDumpingCommand());
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
            Assert.True(context.IsDumpingCommand());
        }

        #endregion

        #region GDROM

        [Theory]
        [InlineData("gd f filename.bin 0 /be raw /c2 1 2 3 1 5 6 /c2new 1 /d8 /d /q /f 0 /np /nq /nr /s 0")]
        public void GDROMTest(string parameters)
        {
            string? expected = "gd f \"filename.bin\" 0 /be raw /c2 1 2 3 1 5 6 /c2new 1 /d8 /d /q /f 0 /np /nq /nr /s 0";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
            Assert.True(context.IsDumpingCommand());
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
            Assert.False(context.IsDumpingCommand());
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
            Assert.False(context.IsDumpingCommand());
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
            Assert.False(context.IsDumpingCommand());
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
            Assert.True(context.IsDumpingCommand());
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
            Assert.False(context.IsDumpingCommand());
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
            Assert.False(context.IsDumpingCommand());
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
            Assert.False(context.IsDumpingCommand());
        }

        #endregion

        #region Swap

        [Theory]
        [InlineData("swap f filename.bin 0 /a 0 /be raw /c2 1 2 3 1 5 6 /c2new 1 /d8 /d /q /f 0 /np /nq /nl /ns /nr /am /sf 1 /ss /74 /s 0 /trp /vn 0 /vnc /vnx")]
        public void SwapTest(string parameters)
        {
            string? expected = "swap f \"filename.bin\" 0 /a 0 /be raw /c2 1 2 3 1 5 6 /c2new 1 /d8 /d /q /f 0 /np /nq /nl /ns /nr /am /sf 1 /ss /74 /s 0 /trp /vn 0 /vnc /vnx";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
            Assert.True(context.IsDumpingCommand());
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
            Assert.True(context.IsDumpingCommand());
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
            Assert.False(context.IsDumpingCommand());
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
            Assert.True(context.IsDumpingCommand());
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
            Assert.True(context.IsDumpingCommand());
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
            Assert.True(context.IsDumpingCommand());
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
            Assert.True(context.IsDumpingCommand());
        }

        #endregion
    }
}
