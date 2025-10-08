using System.Collections.Generic;
using MPF.ExecutionContexts.Redumper;
using SabreTools.RedumpLib.Data;
using Xunit;

namespace MPF.ExecutionContexts.Test
{
    public class RedumperTests
    {
        #region Converters.Extension

        [Theory]
        [InlineData(null, null)]
        [InlineData(MediaType.CDROM, ".bin")]
        [InlineData(MediaType.GDROM, ".bin")]
        [InlineData(MediaType.DVD, ".iso")]
        [InlineData(MediaType.HDDVD, ".iso")]
        [InlineData(MediaType.BluRay, ".iso")]
        [InlineData(MediaType.NintendoWiiOpticalDisc, ".iso")]
        [InlineData(MediaType.NintendoGameCubeGameDisc, ".raw")]
        [InlineData(MediaType.NintendoWiiUOpticalDisc, ".wud")]
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
            [SettingConstants.EnableVerbose] = "true",
            [SettingConstants.LeadinRetryCount] = "1000",
            [SettingConstants.ReadMethod] = "BE",
            [SettingConstants.RereadCount] = "1000",
            [SettingConstants.SectorOrder] = "DATA_C2_SUB",
            [SettingConstants.DriveType] = "GENERIC",
        };

        [Theory]
        [InlineData(null, null, null, "filename.bin", null, "disc --verbose --skeleton --retries=1000 --image-name=\"filename\" --drive-type=GENERIC --drive-read-method=BE --drive-sector-order=DATA_C2_SUB --plextor-leadin-retries=1000")]
        [InlineData(RedumpSystem.IBMPCcompatible, MediaType.CDROM, "/dev/sr0", "path/filename.bin", 2, "disc --verbose --skeleton --drive=/dev/sr0 --speed=2 --retries=1000 --image-path=\"path\" --image-name=\"filename\" --drive-type=GENERIC --drive-read-method=BE --drive-sector-order=DATA_C2_SUB --plextor-leadin-retries=1000")]
        [InlineData(RedumpSystem.IBMPCcompatible, MediaType.DVD, "/dev/sr0", "path/filename.bin", 2, "disc --verbose --skeleton --drive=/dev/sr0 --speed=2 --retries=1000 --image-path=\"path\" --image-name=\"filename\" --drive-type=GENERIC --drive-read-method=BE --drive-sector-order=DATA_C2_SUB --plextor-leadin-retries=1000")]
        [InlineData(RedumpSystem.NintendoGameCube, MediaType.NintendoGameCubeGameDisc, "/dev/sr0", "path/filename.bin", 2, "disc --verbose --skeleton --drive=/dev/sr0 --speed=2 --retries=1000 --image-path=\"path\" --image-name=\"filename\" --drive-type=GENERIC --drive-read-method=BE --drive-sector-order=DATA_C2_SUB --plextor-leadin-retries=1000")]
        [InlineData(RedumpSystem.NintendoWii, MediaType.NintendoWiiOpticalDisc, "/dev/sr0", "path/filename.bin", 2, "disc --verbose --skeleton --drive=/dev/sr0 --speed=2 --retries=1000 --image-path=\"path\" --image-name=\"filename\" --drive-type=GENERIC --drive-read-method=BE --drive-sector-order=DATA_C2_SUB --plextor-leadin-retries=1000")]
        [InlineData(RedumpSystem.HDDVDVideo, MediaType.HDDVD, "/dev/sr0", "path/filename.bin", 2, "disc --verbose --skeleton --drive=/dev/sr0 --speed=2 --retries=1000 --image-path=\"path\" --image-name=\"filename\" --drive-type=GENERIC --drive-read-method=BE --drive-sector-order=DATA_C2_SUB --plextor-leadin-retries=1000")]
        [InlineData(RedumpSystem.BDVideo, MediaType.BluRay, "/dev/sr0", "path/filename.bin", 2, "disc --verbose --skeleton --drive=/dev/sr0 --speed=2 --retries=1000 --image-path=\"path\" --image-name=\"filename\" --drive-type=GENERIC --drive-read-method=BE --drive-sector-order=DATA_C2_SUB --plextor-leadin-retries=1000")]
        [InlineData(RedumpSystem.NintendoWiiU, MediaType.NintendoWiiUOpticalDisc, "/dev/sr0", "path/filename.bin", 2, "disc --verbose --skeleton --drive=/dev/sr0 --speed=2 --retries=1000 --image-path=\"path\" --image-name=\"filename\" --drive-type=GENERIC --drive-read-method=BE --drive-sector-order=DATA_C2_SUB --plextor-leadin-retries=1000")]
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

        #region Disc

        [Theory]
        [InlineData("disc -h --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=path --image-name=image --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs")]
        [InlineData("disc --help --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=path --image-name=image --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs")]
        public void DiscTest(string parameters)
        {
            string? expected = "disc --help --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=\"path\" --image-name=\"image\" --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
            Assert.True(context.IsDumpingCommand());
        }

        [Theory]
        [InlineData("disc --drive=dr --image-path=\"directory name\" --image-name=\"image name.bin\"")]
        public void SpacesTest(string parameters)
        {
            string? expected = "disc --drive=dr --image-path=\"directory name\" --image-name=\"image name.bin\"";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
            Assert.True(context.IsDumpingCommand());
        }

        #endregion

        #region Rings

        [Theory]
        [InlineData("rings -h --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=path --image-name=image --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs")]
        [InlineData("rings --help --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=path --image-name=image --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs")]
        public void RingsTest(string parameters)
        {
            string? expected = "rings --help --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=\"path\" --image-name=\"image\" --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
            Assert.False(context.IsDumpingCommand());
        }

        #endregion

        #region Dump

        [Theory]
        [InlineData("dump -h --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=path --image-name=image --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs")]
        [InlineData("dump --help --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=path --image-name=image --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs")]
        public void DumpTest(string parameters)
        {
            string? expected = "dump --help --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=\"path\" --image-name=\"image\" --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
            Assert.False(context.IsDumpingCommand());
        }

        #endregion

        #region DumpExtra

        [Theory]
        [InlineData("dump::extra -h --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=path --image-name=image --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs")]
        [InlineData("dump::extra --help --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=path --image-name=image --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs")]
        public void DumpExtraTest(string parameters)
        {
            string? expected = "dump::extra --help --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=\"path\" --image-name=\"image\" --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
            Assert.False(context.IsDumpingCommand());
        }

        #endregion

        #region Refine

        [Theory]
        [InlineData("refine -h --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=path --image-name=image --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs")]
        [InlineData("refine --help --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=path --image-name=image --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs")]
        public void RefineTest(string parameters)
        {
            string? expected = "refine --help --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=\"path\" --image-name=\"image\" --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
            Assert.False(context.IsDumpingCommand());
        }

        #endregion

        #region Verify

        [Theory]
        [InlineData("verify -h --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=path --image-name=image --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs")]
        [InlineData("verify --help --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=path --image-name=image --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs")]
        public void VerifyTest(string parameters)
        {
            string? expected = "verify --help --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=\"path\" --image-name=\"image\" --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
            Assert.False(context.IsDumpingCommand());
        }

        #endregion

        #region DVDKey

        [Theory]
        [InlineData("dvdkey -h --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=path --image-name=image --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs")]
        [InlineData("dvdkey --help --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=path --image-name=image --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs")]
        public void DVDKeyTest(string parameters)
        {
            string? expected = "dvdkey --help --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=\"path\" --image-name=\"image\" --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
            Assert.False(context.IsDumpingCommand());
        }

        #endregion

        #region Eject

        [Theory]
        [InlineData("eject -h --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=path --image-name=image --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs")]
        [InlineData("eject --help --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=path --image-name=image --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs")]
        public void EjectTest(string parameters)
        {
            string? expected = "eject --help --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=\"path\" --image-name=\"image\" --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
            Assert.False(context.IsDumpingCommand());
        }

        #endregion

        #region DVDIsoKey

        [Theory]
        [InlineData("dvdisokey -h --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=path --image-name=image --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs")]
        [InlineData("dvdisokey --help --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=path --image-name=image --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs")]
        public void DVDIsoKeyTest(string parameters)
        {
            string? expected = "dvdisokey --help --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=\"path\" --image-name=\"image\" --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
            Assert.False(context.IsDumpingCommand());
        }

        #endregion

        #region Protection

        [Theory]
        [InlineData("protection -h --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=path --image-name=image --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs")]
        [InlineData("protection --help --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=path --image-name=image --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs --disable-cdtext")]
        public void ProtectionTest(string parameters)
        {
            string? expected = "protection --help --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=\"path\" --image-name=\"image\" --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
            Assert.False(context.IsDumpingCommand());
        }

        #endregion

        #region Split

        [Theory]
        [InlineData("split -h --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=path --image-name=image --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs")]
        [InlineData("split --help --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=path --image-name=image --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs")]
        public void SplitTest(string parameters)
        {
            string? expected = "split --help --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=\"path\" --image-name=\"image\" --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
            Assert.False(context.IsDumpingCommand());
        }

        #endregion

        #region Hash

        [Theory]
        [InlineData("hash -h --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=path --image-name=image --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs")]
        [InlineData("hash --help --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=path --image-name=image --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs")]
        public void HashTest(string parameters)
        {
            string? expected = "hash --help --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=\"path\" --image-name=\"image\" --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
            Assert.False(context.IsDumpingCommand());
        }

        #endregion

        #region Info

        [Theory]
        [InlineData("info -h --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=path --image-name=image --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs")]
        [InlineData("info --help --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=path --image-name=image --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs")]
        public void InfoTest(string parameters)
        {
            string? expected = "info --help --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=\"path\" --image-name=\"image\" --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
            Assert.False(context.IsDumpingCommand());
        }

        #endregion

        #region Skeleton

        [Theory]
        [InlineData("skeleton -h --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=path --image-name=image --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs")]
        [InlineData("skeleton --help --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=path --image-name=image --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs")]
        public void SkeletonTest(string parameters)
        {
            string? expected = "skeleton --help --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=\"path\" --image-name=\"image\" --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
            Assert.False(context.IsDumpingCommand());
        }

        #endregion

        #region Subchannel

        [Theory]
        [InlineData("subchannel -h --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=path --image-name=image --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs")]
        [InlineData("subchannel --help --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=path --image-name=image --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs")]
        public void SubchannelTest(string parameters)
        {
            string? expected = "subchannel --help --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=\"path\" --image-name=\"image\" --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
            Assert.False(context.IsDumpingCommand());
        }

        #endregion

        #region Debug

        [Theory]
        [InlineData("debug -h --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=path --image-name=image --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs")]
        [InlineData("debug --help --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=path --image-name=image --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs")]
        public void DebugTest(string parameters)
        {
            string? expected = "debug --help --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=\"path\" --image-name=\"image\" --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
            Assert.False(context.IsDumpingCommand());
        }

        #endregion

        #region FixMSF

        [Theory]
        [InlineData("fixmsf -h --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=path --image-name=image --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs")]
        [InlineData("fixmsf --help --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=path --image-name=image --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs")]
        public void FixMSFTest(string parameters)
        {
            string? expected = "fixmsf --help --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=\"path\" --image-name=\"image\" --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        #endregion

        #region DebugFlip

        [Theory]
        [InlineData("debug::flip -h --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=path --image-name=image --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs")]
        [InlineData("debug::flip --help --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=path --image-name=image --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs")]
        public void DebugFlipTest(string parameters)
        {
            string? expected = "debug::flip --help --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=\"path\" --image-name=\"image\" --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        #endregion

        #region DriveTest

        [Theory]
        [InlineData("drive::test -h --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=path --image-name=image --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs")]
        [InlineData("drive::test --help --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=path --image-name=image --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs")]
        public void DriveTestTest(string parameters)
        {
            string? expected = "drive::test --help --version --verbose --auto-eject --skeleton --drive=dr --speed=8 --retries=0 --image-path=\"path\" --image-name=\"image\" --overwrite --drive-type=dt --drive-read-offset=0 --drive-c2-shift=0 --drive-pregap-start=0 --drive-read-method=drm --drive-sector-order=dso --plextor-skip-leadin --plextor-leadin-retries=0 --asus-skip-leadout --disable-cdtext --force-offset=0 --audio-silence-threshold=0 --correct-offset-shift --offset-shift-relocate --force-split --leave-unchanged --force-qtoc --skip-fill=0 --iso9660-trim --lba-start=0 --lba-end=0 --refine-subchannel --refine-sector-mode --skip=0 --dump-write-offset=0 --dump-read-size=0 --overread-leadout --force-unscrambled --legacy-subs";
            var context = new ExecutionContext(parameters);
            string? actual = context.GenerateParameters();
            Assert.Equal(expected, actual);
        }

        #endregion
    }
}
