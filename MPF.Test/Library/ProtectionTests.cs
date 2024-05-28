using System;
using System.Collections.Generic;
using System.Linq;
using MPF.Frontend.Tools;
using Xunit;

namespace MPF.Test.Library
{
    public class ProtectionTests
    {
        [Fact]
        public void SanitizeFoundProtectionsActiveMARKTest()
        {
            List<string> protections =
            [
                "ActiveMARK",
                "ActiveMARK 5",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal("ActiveMARK 5", sanitized);
        }

        [Fact]
        public void SanitizeFoundProtectionsCactusDataShieldTest()
        {
            List<string> protections =
            [
                "Cactus Data Shield 200",
                "Cactus Data Shield 200 (Build 3.0.100a)",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal("Cactus Data Shield 200 (Build 3.0.100a)", sanitized);
        }

        [Fact]
        public void SanitizeFoundProtectionsCDCheckTest()
        {
            List<string> protections =
            [
                "Anything Else Protection",
                "Executable-Based CD Check",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal("Anything Else Protection", sanitized);
        }

        [Fact]
        public void SanitizeFoundProtectionsCDCopsTest()
        {
            List<string> protections =
            [
                "CD-Cops",
                "CD-Cops v1.2.0",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal("CD-Cops v1.2.0", sanitized);
        }

        [Fact]
        public void SanitizeFoundProtectionsCDKeyTest()
        {
            List<string> protections =
            [
                "Anything Else Protection",
                "CD-Key / Serial",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal("Anything Else Protection", sanitized);
        }

        [Fact]
        public void SanitizeFoundProtectionsEACdKeyTest()
        {
            List<string> protections =
            [
                "EA CdKey Registration Module",
                "EA CdKey Registration Module v1.2.0",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal("EA CdKey Registration Module v1.2.0", sanitized);
        }

        [Fact]
        public void SanitizeFoundProtectionsEADRMTest()
        {
            List<string> protections =
            [
                "EA DRM Protection",
                "EA DRM Protection v1.2.0",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal("EA DRM Protection v1.2.0", sanitized);
        }

        [Fact]
        public void SanitizeFoundProtectionsGFWLTest()
        {
            List<string> protections =
            [
                "Games for Windows LIVE",
                "Games for Windows LIVE v1.2.0",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal("Games for Windows LIVE v1.2.0", sanitized);
        }

        [Fact]
        public void SanitizeFoundProtectionsGFWLZDPPTest()
        {
            List<string> protections =
            [
                "Games for Windows LIVE",
                "Games for Windows LIVE Zero Day Piracy Protection",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal("Games for Windows LIVE, Games for Windows LIVE Zero Day Piracy Protection", sanitized);
        }

        [Fact]
        public void SanitizeFoundProtectionsImpulseReactorTest()
        {
            List<string> protections =
            [
                "Impulse Reactor",
                "Impulse Reactor Core Module v1.2.0",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal("Impulse Reactor Core Module v1.2.0", sanitized);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void SanitizeFoundProtectionsJoWoodXProtTest(int skip)
        {
            List<string> protections =
            [
                "JoWood X-Prot 1.2.0.00",
                "JoWood X-Prot v2",
                "JoWood X-Prot v1.4+",
                "JoWood X-Prot v1.0-v1.3",
                "JoWood X-Prot",
            ];

            // Safeguard for the future
            if (skip >= protections.Count)
                throw new ArgumentException("Invalid skip value", nameof(skip));

            // The list is in order of preference
            protections = protections.Skip(skip).ToList();

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal(protections[0], sanitized);
        }

        [Fact]
        public void SanitizeFoundProtectionsOnlineRegistrationTest()
        {
            List<string> protections =
            [
                "Anything Else Protection",
                "Executable-Based Online Registration",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal("Anything Else Protection", sanitized);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void SanitizeFoundProtectionStarForceTest(int skip)
        {
            List<string> protections =
            [
                "StarForce 1.20.000.000",
                "StarForce 5 [Protected Module]",
                "StarForce 5",
                "StarForce 3-5",
                "StarForce",
            ];

            // Safeguard for the future
            if (skip >= protections.Count)
                throw new ArgumentException("Invalid skip value", nameof(skip));

            // The list is in order of preference
            protections = protections.Skip(skip).ToList();

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal(protections[0], sanitized);
        }

        [Fact]
        public void SanitizeFoundProtectionsSysiphusTest()
        {
            List<string> protections =
            [
                "Sysiphus",
                "Sysiphus v1.2.0",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal("Sysiphus v1.2.0", sanitized);
        }

        [Fact]
        public void SanitizeFoundProtectionsXCPTest()
        {
            List<string> protections =
            [
                "XCP",
                "XCP v1.2.0",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal("XCP v1.2.0", sanitized);
        }
    }
}
