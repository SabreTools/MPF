using System;
using System.Collections.Generic;
using System.Linq;
using MPF.Frontend.Tools;
using Xunit;

namespace MPF.Frontend.Test.Tools
{
    public class ProtectionToolTests
    {
        #region SanitizeContextSensitiveProtections

        [Fact]
        public void SanitizeContextSensitiveProtections_Empty_NoException()
        {
            Dictionary<string, List<string>>? protections = [];
            var actual = ProtectionTool.SanitizeContextSensitiveProtections(protections);
            Assert.NotNull(actual);
            Assert.Empty(actual);
        }

        [Fact]
        public void SanitizeContextSensitiveProtections_NoMatch_NoChange()
        {
            Dictionary<string, List<string>>? protections = [];
            protections["File1"] = ["Protection 1", "Protection 2"];

            var actual = ProtectionTool.SanitizeContextSensitiveProtections(protections);
            Assert.NotNull(actual);

            string[] keys = [.. actual.Keys];
            Assert.Contains("File1", keys);
        }

        [Fact]
        public void SanitizeContextSensitiveProtections_Match_NoSub_NoChange()
        {
            Dictionary<string, List<string>>? protections = [];
            protections["File1"] = ["Protection 1", "Protection 2"];
            protections["File2"] = ["SecuROM Release Control - ANYTHING", "Protection 2"];
            protections["File3"] = ["Protection 1", "SecuROM Release Control -"];

            var actual = ProtectionTool.SanitizeContextSensitiveProtections(protections);
            Assert.NotNull(actual);

            string[] keys = [.. actual.Keys];
            Assert.Contains("File1", keys);
            Assert.Contains("File2", keys);
            Assert.Contains("File3", keys);
        }

        [Fact]
        public void SanitizeContextSensitiveProtections_Match_Sub_Change()
        {
            Dictionary<string, List<string>>? protections = [];
            protections["File1"] = ["Protection 1", "Protection 2"];
            protections["File2"] = ["SecuROM Release Control - ANYTHING"];
            protections["File2/FileA"] = ["ANYTHING GitHub ANYTHING"];
            protections["File2/FileB"] = ["SecuROM 7"];
            protections["File2/FileC"] = ["SecuROM 8"];
            protections["File2/FileD"] = ["SecuROM Content Activation"];
            protections["File2/FileE"] = ["SecuROM Data File Activation"];
            protections["File2/FileF"] = ["Unlock"];

            var actual = ProtectionTool.SanitizeContextSensitiveProtections(protections);
            Assert.NotNull(actual);

            string[] keys = [.. actual.Keys];
            Assert.Contains("File1", keys);
            Assert.Contains("File2", keys);
            Assert.Contains("File2/FileA", keys);
            Assert.DoesNotContain("File2/FileB", keys);
            Assert.DoesNotContain("File2/FileC", keys);
            Assert.DoesNotContain("File2/FileD", keys);
            Assert.DoesNotContain("File2/FileE", keys);
            Assert.DoesNotContain("File2/FileF", keys);
        }

        [Fact]
        public void SanitizeContextSensitiveProtections_MultiMatch_Sub_Change()
        {
            Dictionary<string, List<string>>? protections = [];
            protections["File1"] = ["Protection 1", "Protection 2"];
            protections["File2"] = ["SecuROM Release Control - ANYTHING"];
            protections["File2/FileA"] = ["ANYTHING GitHub ANYTHING"];
            protections["File2/FileB"] = ["SecuROM 7"];
            protections["File2/FileC"] = ["SecuROM 8"];
            protections["File3"] = ["SecuROM Release Control - ANYTHING"];
            protections["File3/FileD"] = ["SecuROM Content Activation"];
            protections["File3/FileE"] = ["SecuROM Data File Activation"];
            protections["File3/FileF"] = ["Unlock"];

            var actual = ProtectionTool.SanitizeContextSensitiveProtections(protections);
            Assert.NotNull(actual);

            string[] keys = [.. actual.Keys];
            Assert.Contains("File1", keys);
            Assert.Contains("File2", keys);
            Assert.Contains("File2/FileA", keys);
            Assert.DoesNotContain("File2/FileB", keys);
            Assert.DoesNotContain("File2/FileC", keys);
            Assert.Contains("File3", keys);
            Assert.DoesNotContain("File3/FileD", keys);
            Assert.DoesNotContain("File3/FileE", keys);
            Assert.DoesNotContain("File3/FileF", keys);
        }

        #endregion

        #region SanitizeFoundProtections

        [Fact]
        public void SanitizeFoundProtections_Exception()
        {
            List<string> protections =
            [
                "Anything Else Protection",
                "[Access issue when opening file",
                "[Exception opening file",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal("Anything Else Protection, Exception occurred while scanning [RESCAN NEEDED]", sanitized);
        }

        #region Game Engine

        [Fact]
        public void SanitizeFoundProtections_RenderWare()
        {
            List<string> protections =
            [
                "RenderWare",
                "RenderWare ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        #endregion

        #region Packers

        [Fact]
        public void SanitizeFoundProtections_dotNetReactor()
        {
            List<string> protections =
            [
                ".NET Reactor",
                ".NET Reactor ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_SevenZipSFX()
        {
            List<string> protections =
            [
                "7-Zip SFX",
                "7-Zip SFX ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_ASPack()
        {
            List<string> protections =
            [
                "ASPack",
                "ASPack ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_AutoPlayMediaStudio()
        {
            List<string> protections =
            [
                "AutoPlay Media Studio",
                "AutoPlay Media Studio ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_CaphyonAdvancedInstaller()
        {
            List<string> protections =
            [
                "Caphyon Advanced Installer",
                "Caphyon Advanced Installer ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_CExe()
        {
            List<string> protections =
            [
                "CExe",
                "CExe ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_dotFuscator()
        {
            List<string> protections =
            [
                "dotFuscator",
                "dotFuscator ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_EmbeddedArchive()
        {
            List<string> protections =
            [
                "Embedded 7-zip Archive",
                "Embedded PKZIP Archive",
                "Embedded RAR Archive",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_EmbeddedExecutable()
        {
            List<string> protections =
            [
                "Embedded Executable",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_EXEStealth()
        {
            List<string> protections =
            [
                "EXE Stealth",
                "EXE Stealth ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_GenteeInstaller()
        {
            List<string> protections =
            [
                "Gentee Installer",
                "Gentee Installer ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_GPInstall()
        {
            List<string> protections =
            [
                "GP-Install",
                "GP-Install ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_HyperTechCrackProof()
        {
            List<string> protections =
            [
                "HyperTech CrackProof",
                "HyperTech CrackProof ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_InnoSetup()
        {
            List<string> protections =
            [
                "Inno Setup",
                "Inno Setup ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_InstallAnywhere()
        {
            List<string> protections =
            [
                "InstallAnywhere",
                "InstallAnywhere ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_InstallerVISE()
        {
            List<string> protections =
            [
                "Installer VISE",
                "Installer VISE ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_IntelInstallationFramework()
        {
            List<string> protections =
            [
                "Intel Installation Framework",
                "Intel Installation Framework ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_MicrosoftCABSFX()
        {
            List<string> protections =
            [
                "Microsoft CAB SFX",
                "Microsoft CAB SFX ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_MPRESS()
        {
            List<string> protections =
            [
                "MPRESS",
                "MPRESS ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_NeoLite()
        {
            List<string> protections =
            [
                "NeoLite",
                "NeoLite ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_NSIS()
        {
            List<string> protections =
            [
                "NSIS",
                "NSIS ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_PECompact()
        {
            List<string> protections =
            [
                "PE Compact",
                "PE Compact ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_PEtite()
        {
            List<string> protections =
            [
                "PEtite",
                "PEtite ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_SetupFactory()
        {
            List<string> protections =
            [
                "Setup Factory",
                "Setup Factory ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_Shrinker()
        {
            List<string> protections =
            [
                "Shrinker",
                "Shrinker ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_UPX()
        {
            List<string> protections =
            [
                "UPX",
                "UPX ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_WinRARSFX()
        {
            List<string> protections =
            [
                "WinRAR SFX",
                "WinRAR SFX ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_WinZipSFX()
        {
            List<string> protections =
            [
                "WinZip SFX",
                "WinZip SFX ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_WiseInstaller()
        {
            List<string> protections =
            [
                "Wise Installation",
                "Wise Installation ANYTHING",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Empty(sanitized);
        }

        #endregion

        #region Protections

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
            Assert.Empty(sanitized);
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
            protections = [.. protections.Skip(skip)];

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
        [InlineData(2, "SafeDisc 0.00.000, SafeDisc 0.00.000-1.11.111, SafeDisc 3+ (DVD), SafeDisc Lite")]
        [InlineData(3, "SafeDisc 0.00.000, SafeDisc 3+ (DVD), SafeDisc Lite")]
        [InlineData(4, "SafeDisc 3+ (DVD), SafeDisc Lite")]
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
                "Macrovision Security Driver 1.11.111 / SafeDisc 1.11.111",
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
            protections = [.. protections.Skip(skip)];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal(expected, sanitized);
        }

        [Fact]
        public void SanitizeFoundProtectsion_SafeDisc_MacrovisionSecurityDriver()
        {
            List<string> protections =
           [
                "Macrovision Protection File [Likely indicates either SafeDisc 1.45.011+ (CD) or CDS-300]",
                "Macrovision Security Driver 4.00.060 / SafeDisc 4.00.000-4.70.000",
                "SafeDisc 4.00.000-4.00.003",
                "SafeDisc 4.00.002, Macrovision Protected Application"
            ];

            string expected = "SafeDisc 4.00.002";

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal(expected, sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_SecuROM()
        {
            List<string> protections =
            [
                "SecuROM Release Control",
                "SecuROM Release Control - ANYTHING",
                "SecuROM Release Control - ANYTHING ELSE",
                "SecuROM Release Control - EVEN MORE",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal("SecuROM Release Control", sanitized);
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
            protections = [.. protections.Skip(skip)];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal(protections[0], sanitized);
        }

        [Fact]
        public void SanitizeFoundProtections_StarForce_Keyless()
        {
            List<string> protections =
            [
                "StarForce Keyless",
                "StarForce Keyless - ",
                "StarForce Keyless - ESPY97LS8FHNYR52JDWL8D6FJ",
                "StarForce Keyless - UAYA-RPMVHJ-ZRY45G-ETUJ36-WL922B",
            ];

            string sanitized = ProtectionTool.SanitizeFoundProtections(protections);
            Assert.Equal("StarForce Keyless", sanitized);
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

        #endregion

        #endregion
    }
}
