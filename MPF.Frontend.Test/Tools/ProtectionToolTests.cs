using System;
using System.Collections.Generic;
using System.Linq;
using MPF.Frontend.Tools;
using Xunit;

namespace MPF.Frontend.Test.Tools
{
    public class ProtectionToolTests
    {
        [Fact]
        public void SanitizeFoundProtections_Exception()
        {
            List<string> protections =
            [
                "Anything Else Protection",
                "[Exception opening file",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal("Anything Else Protection, Exception occurred while scanning [RESCAN NEEDED]", sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_ActiveMARK()
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
        public void SanitizeFoundProtections_CactusDataShield()
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
        public void SanitizeFoundProtections_CactusDataShieldMacrovision()
        {
            List<string> protections =
            [
                "Anything Else Protection",
                "Cactus Data Shield 300 (Confirm presence of other CDS-300 files)",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal("Anything Else Protection, Cactus Data Shield 300", sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_CDCheck()
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
        public void SanitizeFoundProtections_CDCops()
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
        public void SanitizeFoundProtections_CDKey()
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
        public void SanitizeFoundProtections_EACdKey()
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
        public void SanitizeFoundProtections_EADRM()
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
        public void SanitizeFoundProtections_GFWL()
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
        public void SanitizeFoundProtections_GFWLZDPP()
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
        public void SanitizeFoundProtections_ImpulseReactor()
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
        public void SanitizeFoundProtections_JoWoodXProtTest(int skip)
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
        public void SanitizeFoundProtections_OnlineRegistration()
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
        [InlineData(0, "Macrovision Protected Application [SafeDisc 0.00.000], SafeDisc 0.00.000, SafeDisc Lite")]
        [InlineData(1, "Macrovision Protected Application [SafeDisc 0.00.000 / SRV Tool APP], SafeDisc 0.00.000, SafeDisc Lite")]
        [InlineData(2, "Macrovision Security Driver, Macrovision Security Driver [SafeDisc 1.11.111], SafeDisc 0.00.000, SafeDisc 0.00.000-1.11.111, SafeDisc Lite")]
        [InlineData(3, "Macrovision Security Driver, Macrovision Security Driver [SafeDisc 1.11.111], SafeDisc 0.00.000, SafeDisc Lite")]
        [InlineData(4, "Macrovision Security Driver, Macrovision Security Driver [SafeDisc 1.11.111], SafeDisc Lite")]
        [InlineData(5, "Macrovision Security Driver, SafeDisc Lite")]
        [InlineData(6, "Macrovision Protection File, SafeDisc 2+, SafeDisc 3+ (DVD), SafeDisc Lite")]
        [InlineData(7, "SafeDisc 3+ (DVD)")]
        [InlineData(8, "SafeDisc 2+")]
        public void SanitizeFoundProtections_SafeDisc(int skip, string expected)
        {
             List<string> protections =
            [
                "Macrovision Protected Application [SafeDisc 0.00.000]",
                "Macrovision Protected Application [SafeDisc 0.00.000 / SRV Tool APP]",
                "SafeDisc 0.00.000-1.11.111",
                "SafeDisc 0.00.000",
                "Macrovision Security Driver [SafeDisc 1.11.111]",
                "Macrovision Security Driver",
                "SafeDisc Lite",
                "SafeDisc 3+ (DVD)",
                "SafeDisc 2+",
                "Macrovision Protection File",
            ];

            // Safeguard for the future
            if (skip >= protections.Count)
                throw new ArgumentException("Invalid skip value", nameof(skip));

            // The list is in order of preference
            protections = protections.Skip(skip).ToList();

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal(expected, sanitized);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void SanitizeFoundProtections_StarForce(int skip)
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
        public void SanitizeFoundProtections_Sysiphus()
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
        public void SanitizeFoundProtections_XCP()
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
