using System;
using System.Collections.Generic;
using MPF.Core;
using MPF.ExecutionContexts.DiscImageCreator;
using SabreTools.RedumpLib.Data;
using Xunit;

namespace MPF.Test.Modules
{
    public class DiscImageCreatorTests
    {
        #region Old Tests

        [Theory]
        [InlineData(RedumpSystem.MicrosoftXbox, MediaType.CDROM, CommandStrings.CompactDisc)]
        [InlineData(RedumpSystem.MicrosoftXbox, MediaType.DVD, CommandStrings.XBOX)]
        [InlineData(RedumpSystem.MicrosoftXbox, MediaType.LaserDisc, null)]
        [InlineData(RedumpSystem.SonyPlayStation3, MediaType.BluRay, CommandStrings.BluRay)]
        [InlineData(RedumpSystem.AppleMacintosh, MediaType.FloppyDisk, CommandStrings.Floppy)]
        [InlineData(RedumpSystem.RawThrillsVarious, MediaType.GDROM, null)]
        public void ParametersFromSystemAndTypeTest(RedumpSystem? knownSystem, MediaType? mediaType, string? expected)
        {
            var options = new Options();
            var actual = new ExecutionContext(knownSystem, mediaType, "D:\\", "disc.bin", 16, options.Settings);
            Assert.Equal(expected, actual.BaseCommand);
        }

        [Theory]
        [InlineData(RedumpSystem.AppleMacintosh, MediaType.LaserDisc, null)] // Deliberately unsupported
        [InlineData(RedumpSystem.IBMPCcompatible, MediaType.CDROM, new string[] { FlagStrings.C2Opcode, FlagStrings.NoFixSubQSecuROM, FlagStrings.ScanFileProtect })]
        [InlineData(RedumpSystem.NintendoGameCube, MediaType.NintendoGameCubeGameDisc, new string[] { FlagStrings.Raw })]
        public void ParametersFromOptionsSpecialDefaultTest(RedumpSystem? knownSystem, MediaType? mediaType, string[]? expected)
        {
            var options = new Options();
            var actual = new ExecutionContext(knownSystem, mediaType, "D:\\", "disc.bin", 16, options.Settings);

            var expectedSet = new HashSet<string>(expected ?? Array.Empty<string>());
            HashSet<string> actualSet = GenerateUsedKeys(actual);
            Assert.Equal(expectedSet, actualSet);
        }

        [Theory]
        [InlineData(RedumpSystem.SegaDreamcast, MediaType.GDROM, 1000, new string[] { FlagStrings.C2Opcode })]
        [InlineData(RedumpSystem.SegaDreamcast, MediaType.GDROM, -1, new string[] { FlagStrings.C2Opcode })]
        public void ParametersFromOptionsC2RereadTest(RedumpSystem? knownSystem, MediaType? mediaType, int rereadC2, string[] expected)
        {
            var options = new Options { DICRereadCount = rereadC2 };
            var actual = new ExecutionContext(knownSystem, mediaType, "D:\\", "disc.bin", 16, options.Settings);

            var expectedSet = new HashSet<string>(expected ?? Array.Empty<string>());
            HashSet<string> actualSet = GenerateUsedKeys(actual);

            Assert.Equal(expectedSet, actualSet);
            if (rereadC2 == -1 || !knownSystem.MediaTypes().Contains(mediaType))
                Assert.Null(actual.C2OpcodeValue[0]);
            else
                Assert.Equal(rereadC2, actual.C2OpcodeValue[0]);
        }

        [Theory]
        [InlineData(RedumpSystem.IBMPCcompatible, MediaType.DVD, 1000, new string[] { FlagStrings.DVDReread })]
        [InlineData(RedumpSystem.IBMPCcompatible, MediaType.DVD, -1, new string[] { FlagStrings.DVDReread })]
        [InlineData(RedumpSystem.IBMPCcompatible, MediaType.BluRay, 1000, new string[] { FlagStrings.DVDReread })]
        [InlineData(RedumpSystem.IBMPCcompatible, MediaType.BluRay, -1, new string[] { FlagStrings.DVDReread })]
        public void ParametersFromOptionsDVDRereadTest(RedumpSystem? knownSystem, MediaType? mediaType, int rereadDVDBD, string[] expected)
        {
            var options = new Options { DICDVDRereadCount = rereadDVDBD };
            var actual = new ExecutionContext(knownSystem, mediaType, "D:\\", "disc.bin", 16, options.Settings);

            var expectedSet = new HashSet<string>(expected ?? Array.Empty<string>());
            HashSet<string> actualSet = GenerateUsedKeys(actual);

            Assert.Equal(expectedSet, actualSet);
            if (rereadDVDBD == -1 || !knownSystem.MediaTypes().Contains(mediaType))
                Assert.Null(actual.DVDRereadValue);
            else
                Assert.Equal(rereadDVDBD, actual.DVDRereadValue);
        }

        [Theory]
        [InlineData(RedumpSystem.BDVideo, MediaType.BluRay, true, new string[] { FlagStrings.DVDReread })]
        [InlineData(RedumpSystem.BDVideo, MediaType.BluRay, false, new string[] { FlagStrings.DVDReread })]
        [InlineData(RedumpSystem.IBMPCcompatible, MediaType.CDROM, true, new string[] { FlagStrings.C2Opcode, FlagStrings.NoFixSubQSecuROM, FlagStrings.MultiSectorRead, FlagStrings.ScanFileProtect })]
        [InlineData(RedumpSystem.IBMPCcompatible, MediaType.CDROM, false, new string[] { FlagStrings.C2Opcode, FlagStrings.NoFixSubQSecuROM, FlagStrings.ScanFileProtect })]
        [InlineData(RedumpSystem.IBMPCcompatible, MediaType.DVD, true, new string[] { FlagStrings.DVDReread })]
        [InlineData(RedumpSystem.IBMPCcompatible, MediaType.DVD, false, new string[] { FlagStrings.DVDReread })]
        [InlineData(RedumpSystem.HDDVDVideo, MediaType.HDDVD, true, new string[] { FlagStrings.DVDReread })]
        [InlineData(RedumpSystem.HDDVDVideo, MediaType.HDDVD, false, new string[] { FlagStrings.DVDReread })]
        public void ParametersFromOptionsMultiSectorReadTest(RedumpSystem? knownSystem, MediaType? mediaType, bool multiSectorRead, string[] expected)
        {
            var options = new Options { DICMultiSectorRead = multiSectorRead };
            var actual = new ExecutionContext(knownSystem, mediaType, "D:\\", "disc.bin", 16, options.Settings);

            var expectedSet = new HashSet<string>(expected ?? Array.Empty<string>());
            HashSet<string> actualSet = GenerateUsedKeys(actual);
            Assert.Equal(expectedSet, actualSet);
            if (expectedSet.Count != 1 && multiSectorRead)
                Assert.Equal(0, actual.MultiSectorReadValue);
        }

        [Theory]
        [InlineData(RedumpSystem.BDVideo, MediaType.BluRay, true, new string[] { FlagStrings.DVDReread })]
        [InlineData(RedumpSystem.BDVideo, MediaType.BluRay, false, new string[] { FlagStrings.DVDReread })]
        [InlineData(RedumpSystem.IBMPCcompatible, MediaType.CDROM, true, new string[] { FlagStrings.C2Opcode, FlagStrings.NoFixSubQSecuROM, FlagStrings.ScanFileProtect, FlagStrings.ScanSectorProtect, FlagStrings.SubchannelReadLevel })]
        [InlineData(RedumpSystem.IBMPCcompatible, MediaType.CDROM, false, new string[] { FlagStrings.C2Opcode, FlagStrings.NoFixSubQSecuROM, FlagStrings.ScanFileProtect })]
        [InlineData(RedumpSystem.IBMPCcompatible, MediaType.DVD, true, new string[] { FlagStrings.DVDReread, FlagStrings.ScanFileProtect })]
        [InlineData(RedumpSystem.IBMPCcompatible, MediaType.DVD, false, new string[] { FlagStrings.DVDReread })]
        [InlineData(RedumpSystem.HDDVDVideo, MediaType.HDDVD, true, new string[] { FlagStrings.DVDReread })]
        [InlineData(RedumpSystem.HDDVDVideo, MediaType.HDDVD, false, new string[] { FlagStrings.DVDReread })]
        public void ParametersFromOptionsParanoidModeTest(RedumpSystem? knownSystem, MediaType? mediaType, bool paranoidMode, string[] expected)
        {
            var options = new Options { DICParanoidMode = paranoidMode };
            var actual = new ExecutionContext(knownSystem, mediaType, "D:\\", "disc.bin", 16, options.Settings);

            var expectedSet = new HashSet<string>(expected ?? Array.Empty<string>());
            HashSet<string> actualSet = GenerateUsedKeys(actual);
            Assert.Equal(expectedSet, actualSet);
            if (paranoidMode)
            {
                if (actualSet.Contains(FlagStrings.ScanSectorProtect))
                    Assert.True(actual[FlagStrings.ScanSectorProtect]);

                if (actualSet.Contains(FlagStrings.SubchannelReadLevel))
                {
                    Assert.True(actual[FlagStrings.SubchannelReadLevel]);
                    Assert.Equal(2, actual.SubchannelReadLevelValue);
                }
            }
            else
            {
                if (actualSet.Contains(FlagStrings.ScanSectorProtect))
                    Assert.False(actual[FlagStrings.ScanSectorProtect]);

                if (actualSet.Contains(FlagStrings.SubchannelReadLevel))
                    Assert.False(actual[FlagStrings.SubchannelReadLevel]);

                Assert.Null(actual.SubchannelReadLevelValue);
            }
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("cd F test.bin 8 /c2 20", true)]
        [InlineData("fd A test.img", true)]
        [InlineData("dvd X super\\test.iso 8 /raw", true)]
        [InlineData("bd D longer\\path_test.iso 16", true)]
        [InlineData("stop D", true)]
        [InlineData("ls", false)]
        public void ValidateParametersTest(string? parameters, bool expected)
        {
            var actual = new ExecutionContext(parameters);
            Assert.Equal(expected, actual.IsValid());
        }

        [Theory]
        [InlineData(MediaType.CDROM, ".bin")]
        [InlineData(MediaType.DVD, ".iso")]
        [InlineData(MediaType.LaserDisc, ".raw")]
        [InlineData(MediaType.NintendoWiiUOpticalDisc, ".wud")]
        [InlineData(MediaType.FloppyDisk, ".img")]
        [InlineData(MediaType.Cassette, ".wav")]
        [InlineData(MediaType.NONE, null)]
        public void MediaTypeToExtensionTest(MediaType? mediaType, string? expected)
        {
            var actual = Converters.Extension(mediaType);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(CommandStrings.Audio, MediaType.CDROM)]
        [InlineData(CommandStrings.BluRay, MediaType.BluRay)]
        [InlineData(CommandStrings.Close, null)]
        [InlineData(CommandStrings.CompactDisc, MediaType.CDROM)]
        [InlineData(CommandStrings.Data, MediaType.CDROM)]
        [InlineData(CommandStrings.DigitalVideoDisc, MediaType.DVD)]
        [InlineData(CommandStrings.Eject, null)]
        [InlineData(CommandStrings.Floppy, MediaType.FloppyDisk)]
        [InlineData(CommandStrings.GDROM, MediaType.GDROM)]
        [InlineData(CommandStrings.MDS, null)]
        [InlineData(CommandStrings.Reset, null)]
        [InlineData(CommandStrings.SACD, MediaType.CDROM)]
        [InlineData(CommandStrings.Start, null)]
        [InlineData(CommandStrings.Stop, null)]
        [InlineData(CommandStrings.Sub, null)]
        [InlineData(CommandStrings.Swap, MediaType.GDROM)]
        [InlineData(CommandStrings.XBOX, MediaType.DVD)]
        public void BaseCommandToMediaTypeTest(string command, MediaType? expected)
        {
            MediaType? actual = Converters.ToMediaType(command);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(CommandStrings.Audio, RedumpSystem.AudioCD)]
        [InlineData(CommandStrings.BluRay, RedumpSystem.SonyPlayStation3)]
        [InlineData(CommandStrings.Close, null)]
        [InlineData(CommandStrings.CompactDisc, RedumpSystem.IBMPCcompatible)]
        [InlineData(CommandStrings.Data, RedumpSystem.IBMPCcompatible)]
        [InlineData(CommandStrings.DigitalVideoDisc, RedumpSystem.IBMPCcompatible)]
        [InlineData(CommandStrings.Eject, null)]
        [InlineData(CommandStrings.Floppy, RedumpSystem.IBMPCcompatible)]
        [InlineData(CommandStrings.GDROM, RedumpSystem.SegaDreamcast)]
        [InlineData(CommandStrings.MDS, null)]
        [InlineData(CommandStrings.Reset, null)]
        [InlineData(CommandStrings.SACD, RedumpSystem.SuperAudioCD)]
        [InlineData(CommandStrings.Start, null)]
        [InlineData(CommandStrings.Stop, null)]
        [InlineData(CommandStrings.Sub, null)]
        [InlineData(CommandStrings.Swap, RedumpSystem.SegaDreamcast)]
        [InlineData(CommandStrings.XBOX, RedumpSystem.MicrosoftXbox)]
        public void BaseCommandToRedumpSystemTest(string command, RedumpSystem? expected)
        {
            RedumpSystem? actual = Converters.ToRedumpSystem(command);
            Assert.Equal(expected, actual);
        }

        #endregion

        [Fact]
        public void DiscImageCreatorAudioParametersTest()
        {
            string originalParameters = "audio F \"ISO\\Audio CD\\Audio CD.bin\" 72 -5 0";

            // Validate that a common audio commandline is parsed
            var executionContext = new ExecutionContext(originalParameters);
            Assert.NotNull(executionContext);

            // Validate that the same set of parameters are generated on the output
            var newParameters = executionContext.GenerateParameters();
            Assert.NotNull(newParameters);
            Assert.Equal(originalParameters, newParameters);
        }

        [Fact]
        public void DiscImageCreatorDataParametersTest()
        {
            string originalParameters = "data F \"ISO\\Data CD\\Data CD.bin\" 72 -5 0";

            // Validate that a common audio commandline is parsed
            var executionContext = new ExecutionContext(originalParameters);
            Assert.NotNull(executionContext);

            // Validate that the same set of parameters are generated on the output
            var newParameters = executionContext.GenerateParameters();
            Assert.NotNull(newParameters);
            Assert.Equal(originalParameters, newParameters);
        }

        /// <summary>
        /// Generate a HashSet of keys that are considered to be set
        /// </summary>
        /// <param name="executionContext">ExecutionContext object representing how to invoke the internal program</param>
        /// <returns>HashSet representing the strings</returns>
        private static HashSet<string> GenerateUsedKeys(ExecutionContext executionContext)
        {
            var usedKeys = new HashSet<string>();
            if (executionContext?.Keys == null)
                return usedKeys;

            foreach (string key in executionContext.Keys)
            {
                if (executionContext[key] == true)
                    usedKeys.Add(key);
            }

            return usedKeys;
        }
    }
}
