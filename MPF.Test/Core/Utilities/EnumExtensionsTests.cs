using System;
using System.Collections.Generic;
using System.Linq;
using MPF.Core.Utilities;
using SabreTools.RedumpLib.Data;
using Xunit;

namespace MPF.Test.Core.Utilities
{
    public class EnumExtensionsTests
    {
        /// <summary>
        /// MediaType values that support drive speeds
        /// </summary>
        private static readonly MediaType?[] _supportDriveSpeeds = new MediaType?[]
        {
            MediaType.CDROM,
            MediaType.DVD,
            MediaType.GDROM,
            MediaType.HDDVD,
            MediaType.BluRay,
            MediaType.NintendoGameCubeGameDisc,
            MediaType.NintendoWiiOpticalDisc,
        };

        /// <summary>
        /// RedumpSystem values that are considered Audio
        /// </summary>
        private static readonly RedumpSystem?[] _audioSystems = new RedumpSystem?[]
        {
            RedumpSystem.AtariJaguarCDInteractiveMultimediaSystem,
            RedumpSystem.AudioCD,
            RedumpSystem.DVDAudio,
            RedumpSystem.HasbroiONEducationalGamingSystem,
            RedumpSystem.HasbroVideoNow,
            RedumpSystem.HasbroVideoNowColor,
            RedumpSystem.HasbroVideoNowJr,
            RedumpSystem.HasbroVideoNowXP,
            RedumpSystem.PlayStationGameSharkUpdates,
            RedumpSystem.PhilipsCDi,
            RedumpSystem.SuperAudioCD,
        };

        /// <summary>
        /// RedumpSystem values that are considered markers
        /// </summary>
        private static readonly RedumpSystem?[] _markerSystems = new RedumpSystem?[]
        {
            RedumpSystem.MarkerArcadeEnd,
            RedumpSystem.MarkerComputerEnd,
            RedumpSystem.MarkerDiscBasedConsoleEnd,
            RedumpSystem.MarkerOtherEnd,
        };

        /// <summary>
        /// RedumpSystem values that are have reversed ringcodes
        /// </summary>
        private static readonly RedumpSystem?[] _reverseRingcodeSystems = new RedumpSystem?[]
        {
            RedumpSystem.SonyPlayStation2,
            RedumpSystem.SonyPlayStation3,
            RedumpSystem.SonyPlayStation4,
            RedumpSystem.SonyPlayStationPortable,
        };

        /// <summary>
        /// RedumpSystem values that are considered XGD
        /// </summary>
        private static readonly RedumpSystem?[] _xgdSystems = new RedumpSystem?[]
        {
            RedumpSystem.MicrosoftXbox,
            RedumpSystem.MicrosoftXbox360,
            RedumpSystem.MicrosoftXboxOne,
            RedumpSystem.MicrosoftXboxSeriesXS,
        };

        /// <summary>
        /// Check that all optical media support drive speeds
        /// </summary>
        /// <param name="mediaType">DriveType value to check</param>
        /// <param name="expected">The expected value to come from the check</param>
        [Theory]
        [MemberData(nameof(GenerateSupportDriveSpeedsTestData))]
        public void DoesSupportDriveSpeedTest(MediaType? mediaType, bool expected)
        {
            bool actual = mediaType.DoesSupportDriveSpeed();
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Check that all systems with reversed ringcodes are marked properly
        /// </summary>
        /// <param name="redumpSystem">RedumpSystem value to check</param>
        /// <param name="expected">The expected value to come from the check</param>
        [Theory]
        [MemberData(nameof(GenerateReversedRingcodeSystemsTestData))]
        public void HasReversedRingcodesTest(RedumpSystem? redumpSystem, bool expected)
        {
            bool actual = redumpSystem.HasReversedRingcodes();
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Check that all audio systems are marked properly
        /// </summary>
        /// <param name="redumpSystem">RedumpSystem value to check</param>
        /// <param name="expected">The expected value to come from the check</param>
        [Theory]
        [MemberData(nameof(GenerateAudioSystemsTestData))]
        public void IsAudioTest(RedumpSystem? redumpSystem, bool expected)
        {
            bool actual = redumpSystem.IsAudio();
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Check that all marker systems are marked properly
        /// </summary>
        /// <param name="redumpSystem">RedumpSystem value to check</param>
        /// <param name="expected">The expected value to come from the check</param>
        [Theory]
        [MemberData(nameof(GenerateMarkerSystemsTestData))]
        public void IsMarkerTest(RedumpSystem? redumpSystem, bool expected)
        {
            bool actual = redumpSystem.IsMarker();
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Check that all XGD systems are marked properly
        /// </summary>
        /// <param name="redumpSystem">RedumpSystem value to check</param>
        /// <param name="expected">The expected value to come from the check</param>
        [Theory]
        [MemberData(nameof(GenerateXGDSystemsTestData))]
        public void IsXGDTest(RedumpSystem? redumpSystem, bool expected)
        {
            bool actual = redumpSystem.IsXGD();
            Assert.Equal(expected, actual);
        }

        /// <summary>
        /// Generate a test set of MediaType values that support drive speeds
        /// </summary>
        /// <returns>MemberData-compatible list of MediaType values</returns>
        public static List<object[]> GenerateSupportDriveSpeedsTestData()
        {
            var testData = new List<object[]>() { new object[] { null, false } };
            foreach (MediaType mediaType in Enum.GetValues(typeof(MediaType)))
            {
                if (_supportDriveSpeeds.Contains(mediaType))
                    testData.Add(new object[] { mediaType, true });
                else
                    testData.Add(new object[] { mediaType, false });
            }

            return testData;
        }

        /// <summary>
        /// Generate a test set of RedumpSystem values that are considered Audio
        /// </summary>
        /// <returns>MemberData-compatible list of RedumpSystem values</returns>
        public static List<object[]> GenerateAudioSystemsTestData()
        {
            var testData = new List<object[]>() { new object[] { null, false } };
            foreach (RedumpSystem redumpSystem in Enum.GetValues(typeof(RedumpSystem)))
            {
                if (_audioSystems.Contains(redumpSystem))
                    testData.Add(new object[] { redumpSystem, true });
                else
                    testData.Add(new object[] { redumpSystem, false });
            }

            return testData;
        }

        /// <summary>
        /// Generate a test set of RedumpSystem values that are considered markers
        /// </summary>
        /// <returns>MemberData-compatible list of RedumpSystem values</returns>
        public static List<object[]> GenerateMarkerSystemsTestData()
        {
            var testData = new List<object[]>() { new object[] { null, false } };
            foreach (RedumpSystem redumpSystem in Enum.GetValues(typeof(RedumpSystem)))
            {
                if (_markerSystems.Contains(redumpSystem))
                    testData.Add(new object[] { redumpSystem, true });
                else
                    testData.Add(new object[] { redumpSystem, false });
            }

            return testData;
        }

        /// <summary>
        /// Generate a test set of RedumpSystem values that are considered markers
        /// </summary>
        /// <returns>MemberData-compatible list of RedumpSystem values</returns>
        public static List<object[]> GenerateReversedRingcodeSystemsTestData()
        {
            var testData = new List<object[]>() { new object[] { null, false } };
            foreach (RedumpSystem redumpSystem in Enum.GetValues(typeof(RedumpSystem)))
            {
                if (_reverseRingcodeSystems.Contains(redumpSystem))
                    testData.Add(new object[] { redumpSystem, true });
                else
                    testData.Add(new object[] { redumpSystem, false });
            }

            return testData;
        }

        /// <summary>
        /// Generate a test set of RedumpSystem values that are considered XGD
        /// </summary>
        /// <returns>MemberData-compatible list of RedumpSystem values</returns>
        public static List<object[]> GenerateXGDSystemsTestData()
        {
            var testData = new List<object[]>() { new object[] { null, false } };
            foreach (RedumpSystem redumpSystem in Enum.GetValues(typeof(RedumpSystem)))
            {
                if (_xgdSystems.Contains(redumpSystem))
                    testData.Add(new object[] { redumpSystem, true });
                else
                    testData.Add(new object[] { redumpSystem, false });
            }

            return testData;
        }
    }
}
