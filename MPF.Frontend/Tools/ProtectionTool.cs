using System;
using System.Collections.Generic;
using System.IO;
#if NET35_OR_GREATER || NETCOREAPP
using System.Linq;
#endif
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BinaryObjectScanner;
using SabreTools.IO.Extensions;

namespace MPF.Frontend.Tools
{
    public static class ProtectionTool
    {
        /// <summary>
        /// Set of protection prefixes to filter on
        /// </summary>
        /// <remarks>Based on Redump requirements</remarks>
        private static readonly string[] FilterPrefixes = [
            #region Game Engine

            "RenderWare",

            #endregion

            #region Packers

            ".NET Reactor",
            "7-Zip SFX",
            "ASPack",
            "AutoPlay Media Studio",
            "Caphyon Advanced Installer",
            "CExe",
            "dotFuscator",
            "Embedded",
            "EXE Stealth",
            "Gentee Installer",
            "HyperTech CrackProof",
            "Inno Setup",
            "InstallAnywhere",
            "Installer VISE",
            "Intel Installation Framework",
            "Microsoft CAB SFX",
            "NeoLite",
            "NSIS",
            "PE Compact",
            "PEtite",
            "Setup Factory",
            "Shrinker",
            "UPX",
            "WinRAR SFX",
            "WinZip SFX",
            "Wise Installation",

            #endregion

            #region Protections

            "CD-Key / Serial",
            "EA CdKey",
            "Executable-Based CD Check",
            "Executable-Based Online Registration",

            #endregion
        ];

        /// <summary>
        /// Run protection scan on a given path
        /// </summary>
        /// <param name="path">Path to scan for protection</param>
        /// <param name="options">Options object that determines what to scan</param>
        /// <param name="progress">Optional progress callback</param>
        /// <returns>Set of all detected copy protections with an optional error string</returns>
        public static async Task<Dictionary<string, List<string>>> RunProtectionScanOnPath(string path,
            Options options,
            IProgress<ProtectionProgress>? progress = null)
        {
#if NET40
            var found = await Task.Factory.StartNew(() =>
#else
            var found = await Task.Run(() =>
#endif
            {
                var scanner = new Scanner(
                    options.ScanArchivesForProtection,
                    scanContents: true, // Hardcoded value to avoid issues
                    scanPaths: true, // Hardcoded value to avoid issues
                    scanSubdirectories: true, // Hardcoded value to avoid issues
                    options.IncludeDebugProtectionInformation,
                    progress);

                return scanner.GetProtections(path);
            });

            // If nothing was returned, return
            if (found == null || found.Count == 0)
                return [];

            // Return the filtered set of protections
            return found;
        }

        /// <summary>
        /// Format found protections to a deduplicated, ordered string
        /// </summary>
        /// <param name="protections">Dictionary of file to list of protection mappings</param>
        /// <returns>Detected protections, if any</returns>
        public static string? FormatProtections(Dictionary<string, List<string>>? protections)
        {
            // If the filtered list is empty in some way, return
            if (protections == null)
                return "(CHECK WITH PROTECTIONID)";
            else if (protections.Count == 0)
                return "None found [OMIT FROM SUBMISSION]";

            // Sanitize context-sensitive protections
            protections = SanitizeContextSensitiveProtections(protections);

            // Get a list of distinct found protections
#if NET20
            var protectionValues = new List<string>();
            foreach (var value in protections.Values)
            {
                if (value.Count == 0)
                    continue;
                
                foreach (var prot in value)
                {
                    if (!protectionValues.Contains(prot))
                        protectionValues.Add(prot);
                }
            }
#else
            var protectionValues = protections
                .SelectMany(kvp => kvp.Value)
                .Distinct()
                .ToList();
#endif
            
            // Sanitize and join protections for writing
            string protectionString = SanitizeFoundProtections(protectionValues);
            if (string.IsNullOrEmpty(protectionString))
                return "None found [OMIT FROM SUBMISSION]";

            return protectionString;
        }

        /// <summary>
        /// Get the existence of an anti-modchip string from a PlayStation disc, if possible
        /// </summary>
        /// <param name="path">Path to scan for anti-modchip strings</param>
        /// <returns>Anti-modchip existence if possible, false on error</returns>
        public static async Task<bool> GetPlayStationAntiModchipDetected(string? path)
        {
            // If there is no valid path
            if (string.IsNullOrEmpty(path))
                return false;

#if NET40
            return await Task.Factory.StartNew(() =>
#else
            return await Task.Run(() =>
#endif
            {
                try
                {
                    var antiModchip = new BinaryObjectScanner.Protection.PSXAntiModchip();
                    foreach (string file in IOExtensions.SafeGetFiles(path!, "*", SearchOption.AllDirectories))
                    {
                        try
                        {
                            byte[] fileContent = File.ReadAllBytes(file);
                            var protection = antiModchip.CheckContents(file, fileContent, false);
                            if (!string.IsNullOrEmpty(protection))
                                return true;
                        }
                        catch { }
                    }
                }
                catch { }

                return false;
            });
        }

        /// <summary>
        /// Sanitize unnecessary protections where context matters
        /// </summary>
        /// <param name="protections">Dictionary of file to list of protection mappings</param>
        /// <returns>Dictionary with all necessary items filtered out</returns>
        public static Dictionary<string, List<string>> SanitizeContextSensitiveProtections(Dictionary<string, List<string>> protections)
        {
            // Setup the output dictionary
            Dictionary<string, List<string>> filtered = [];

            // Setup a list for keys that need additional processing
            List<string> foundKeys = [];

            // Loop through the keys and add relevant ones
            string[] paths = [.. protections.Keys];
            foreach (var path in paths)
            {
                if (!protections.TryGetValue(path, out var values) || values == null || values.Count == 0)
                    continue;

                // Always copy the values if they're valid
                filtered[path] = values;

                if (values.Exists(s => s.StartsWith("SecuROM Release Control -")))
                    foundKeys.Add(path);
            }

            // If there are no keys found
            if (foundKeys.Count == 0)
                return filtered;

            // Process the keys as necessary
            foreach (var key in foundKeys)
            {
                // Get all matching paths
                var matchingPaths = Array.FindAll(paths, s => s != key && s.StartsWith(key));
                if (matchingPaths.Length == 0)
                    continue;

                // Loop through the matching paths
                foreach (var path in matchingPaths)
                {
                    if (!filtered.TryGetValue(path, out var values) || values == null || values.Count == 0)
                        continue;

                    if (values.Exists(s => !s.Contains("GitHub") &&
                        (s.Contains("SecuROM 7")
                            || s.Contains("SecuROM 8")
                            || s.Contains("SecuROM Content Activation")
                            || s.Contains("SecuROM Data File Activation")
                            || s.Contains("Unlock"))))
                    {
                        filtered.Remove(path);
                    }
                }
            }

            return filtered;
        }

        /// <summary>
        /// Sanitize unnecessary protection duplication from output
        /// </summary>
        /// <param name="foundProtections">Enumerable of found protections</param>
        /// <remarks>
        /// This filtering only impacts the information that goes into the single-line
        /// protection field in the output submission info. The filtering performed by
        /// this method applies to the needs of Redump and not necessarily any other
        /// application. The full protection list should be used as a reference in all
        /// other cases.
        /// </remarks>
        public static string SanitizeFoundProtections(List<string> foundProtections)
        {
            // EXCEPTIONS
            if (foundProtections.Exists(p => p.StartsWith("[Exception opening file")))
            {
                foundProtections = foundProtections.FindAll(p => !p.StartsWith("[Exception opening file") && !p.StartsWith("[Access issue when opening file"));
                foundProtections.Add("Exception occurred while scanning [RESCAN NEEDED]");
            }
            if (foundProtections.Exists(p => p.StartsWith("[Access issue when opening file")))
            {
                foundProtections = foundProtections.FindAll(p => !p.StartsWith("[Exception opening file") && !p.StartsWith("[Access issue when opening file"));
                foundProtections.Add("Exception occurred while scanning [RESCAN NEEDED]");
            }

            // Filtered prefixes
            foreach (string prefix in FilterPrefixes)
            {
                foundProtections = foundProtections.FindAll(p => !p.StartsWith(prefix));
            }

            // ActiveMARK
            if (foundProtections.Exists(p => p == "ActiveMARK 5")
                && foundProtections.Exists(p => p == "ActiveMARK"))
            {
                foundProtections = foundProtections.FindAll(p => p != "ActiveMARK");
            }

            // Cactus Data Shield
            if (foundProtections.Exists(p => Regex.IsMatch(p, @"Cactus Data Shield [0-9]{3} .+", RegexOptions.Compiled))
                && foundProtections.Exists(p => p == "Cactus Data Shield 200"))
            {
                foundProtections = foundProtections.FindAll(p => p != "Cactus Data Shield 200");
            }

            // Cactus Data Shield / SafeDisc
            if (foundProtections.Exists(p => p == "Cactus Data Shield 300 (Confirm presence of other CDS-300 files)"))
            {
                foundProtections = foundProtections
                    .FindAll(p => p != "Cactus Data Shield 300 (Confirm presence of other CDS-300 files)");

                if (foundProtections.Exists(p => !p.StartsWith("SafeDisc")))
                    foundProtections.Add("Cactus Data Shield 300");
            }

            // CD-Cops
            if (foundProtections.Exists(p => p == "CD-Cops")
                && foundProtections.Exists(p => p.StartsWith("CD-Cops")
                    && p.Length > "CD-Cops".Length))
            {
                foundProtections = foundProtections.FindAll(p => p != "CD-Cops");
            }

            // Electronic Arts
            if (foundProtections.Exists(p => p == "EA DRM Protection")
                && foundProtections.Exists(p => p.StartsWith("EA DRM Protection")
                    && p.Length > "EA DRM Protection".Length))
            {
                foundProtections = foundProtections.FindAll(p => p != "EA DRM Protection");
            }

            // Games for Windows LIVE
            if (foundProtections.Exists(p => p == "Games for Windows LIVE")
                && foundProtections.Exists(p => p.StartsWith("Games for Windows LIVE")
                    && !p.Contains("Zero Day Piracy Protection")
                    && p.Length > "Games for Windows LIVE".Length))
            {
                foundProtections = foundProtections.FindAll(p => p != "Games for Windows LIVE");
            }

            // Impulse Reactor
            if (foundProtections.Exists(p => p.StartsWith("Impulse Reactor Core Module"))
                && foundProtections.Exists(p => p == "Impulse Reactor"))
            {
                foundProtections = foundProtections.FindAll(p => p != "Impulse Reactor");
            }

            // JoWood X-Prot
            if (foundProtections.Exists(p => p.StartsWith("JoWood X-Prot")))
            {
                if (foundProtections.Exists(p => Regex.IsMatch(p, @"JoWood X-Prot [0-9]\.[0-9]\.[0-9]\.[0-9]{2}", RegexOptions.Compiled)))
                {
                    foundProtections = foundProtections.FindAll(p => p != "JoWood X-Prot")
                        .FindAll(p => p != "JoWood X-Prot v1.0-v1.3")
                        .FindAll(p => p != "JoWood X-Prot v1.4+")
                        .FindAll(p => p != "JoWood X-Prot v2");
                }
                else if (foundProtections.Exists(p => p == "JoWood X-Prot v2"))
                {
                    foundProtections = foundProtections.FindAll(p => p != "JoWood X-Prot")
                        .FindAll(p => p != "JoWood X-Prot v1.0-v1.3")
                        .FindAll(p => p != "JoWood X-Prot v1.4+");
                }
                else if (foundProtections.Exists(p => p == "JoWood X-Prot v1.4+"))
                {
                    foundProtections = foundProtections.FindAll(p => p != "JoWood X-Prot")
                        .FindAll(p => p != "JoWood X-Prot v1.0-v1.3");
                }
                else if (foundProtections.Exists(p => p == "JoWood X-Prot v1.0-v1.3"))
                {
                    foundProtections = foundProtections.FindAll(p => p != "JoWood X-Prot");
                }
            }

            // LaserLok
            // TODO: Figure this one out

            // ProtectDISC / VOB ProtectCD/DVD
            // TODO: Figure this one out

            // SafeCast
            // TODO: Figure this one out

            // SafeDisc
            if (foundProtections.Exists(p => p.StartsWith("SafeDisc")))
            {
                // Confirmed this set of checks works with Redump entries 10430, 11347, 13230, 18614, 28257, 31149, 31824, 52606, 57721, 58455,
                // 58573, 62935, 63941, 64255, 65569, 66005, 70504, 73502, 74520, 78048, 79729, 83468, 98589, and 101261.

                // Best case scenario for SafeDisc 2+: A full SafeDisc version is found in a line starting with "Macrovision Protected Application".
                // All other SafeDisc detections can be safely scrubbed.
                // TODO: Scrub "Macrovision Protected Application, " from before the SafeDisc version.
                if (foundProtections.Exists(p => Regex.IsMatch(p, @"SafeDisc [0-9]\.[0-9]{2}\.[0-9]{3}", RegexOptions.Compiled)
                        && p.StartsWith("Macrovision Protected Application")
                        && !p.Contains("SRV Tool APP")))
                {
                    foundProtections = foundProtections.FindAll(p => !p.StartsWith("Macrovision Protection File"))
                        .FindAll(p => !p.StartsWith("Macrovision Security Driver"))
                        .FindAll(p => !p.Contains("SRV Tool APP"))
                        .FindAll(p => p != "SafeDisc")
                        .FindAll(p => !p.StartsWith("Macrovision Protected Application [Version Expunged]"))
                        .FindAll(p => !Regex.IsMatch(p, @"SafeDisc [0-9]\.[0-9]{2}\.[0-9]{3}-[0-9]\.[0-9]{2}\.[0-9]{3}", RegexOptions.Compiled))
                        .FindAll(p => !Regex.IsMatch(p, @"SafeDisc [0-9]\.[0-9]{2}\.[0-9]{3}\+", RegexOptions.Compiled))
                        .FindAll(p => !Regex.IsMatch(p, @"SafeDisc [0-9]\.[0-9]{2}\.[0-9]{3}\/4\+", RegexOptions.Compiled))
                        .FindAll(p => p != "SafeDisc 1/Lite")
                        .FindAll(p => p != "SafeDisc 2+")
                        .FindAll(p => p != "SafeDisc 3+ (DVD)");
                }

                // Next best case for SafeDisc 2+: A full SafeDisc version is found from the "SafeDisc SRV Tool APP".
                else if (foundProtections.Exists(p => Regex.IsMatch(p, @"SafeDisc [0-9]\.[0-9]{2}\.[0-9]{3}", RegexOptions.Compiled)
                        && p.StartsWith("Macrovision Protected Application")
                        && p.Contains("SRV Tool APP")))
                {
                    foundProtections = foundProtections.FindAll(p => !p.StartsWith("Macrovision Protection File"))
                        .FindAll(p => !p.StartsWith("Macrovision Security Driver"))
                        .FindAll(p => p != "SafeDisc")
                        .FindAll(p => !p.StartsWith("Macrovision Protected Application [Version Expunged]"))
                        .FindAll(p => !Regex.IsMatch(p, @"SafeDisc [0-9]\.[0-9]{2}\.[0-9]{3}-[0-9]\.[0-9]{2}\.[0-9]{3}", RegexOptions.Compiled))
                        .FindAll(p => !Regex.IsMatch(p, @"SafeDisc [0-9]\.[0-9]{2}\.[0-9]{3}\+", RegexOptions.Compiled))
                        .FindAll(p => !Regex.IsMatch(p, @"SafeDisc [0-9]\.[0-9]{2}\.[0-9]{3}\/4\+", RegexOptions.Compiled))
                        .FindAll(p => p != "SafeDisc 1/Lite")
                        .FindAll(p => p != "SafeDisc 2+")
                        .FindAll(p => p != "SafeDisc 3+ (DVD)");
                }

                // Covers specific edge cases where older drivers are erroneously placed in discs with a newer version of SafeDisc, and the specific SafeDisc version is expunged.
                else if (foundProtections.Exists(p => Regex.IsMatch(p, @"SafeDisc [1-2]\.[0-9]{2}\.[0-9]{3}-[1-2]\.[0-9]{2}\.[0-9]{3}$", RegexOptions.Compiled)
                        || Regex.IsMatch(p, @"SafeDisc [1-2]\.[0-9]{2}\.[0-9]{3}$", RegexOptions.Compiled))
                    && foundProtections.Exists(p => p == "SafeDisc 3+ (DVD)"))
                {
                    foundProtections = foundProtections.FindAll(p => !p.StartsWith("Macrovision Protection File"))
                    .FindAll(p => !p.StartsWith("Macrovision Protected Application [Version Expunged]"))
                    .FindAll(p => !p.StartsWith("Macrovision Security Driver"))
                    .FindAll(p => !Regex.IsMatch(p, @"SafeDisc [1-2]\.[0-9]{2}\.[0-9]{3}\+", RegexOptions.Compiled))
                    .FindAll(p => !Regex.IsMatch(p, @"SafeDisc [1-2]\.[0-9]{2}\.[0-9]{3}-[1-2]\.[0-9]{2}\.[0-9]{3}", RegexOptions.Compiled))
                    .FindAll(p => p != "SafeDisc")
                    .FindAll(p => p != "SafeDisc 1/Lite")
                    .FindAll(p => p != "SafeDisc 2+");
                }

                // Best case for SafeDisc 1.X: A full SafeDisc version is found that isn't part of a version range. 
                else if (foundProtections.Exists(p => Regex.IsMatch(p, @"SafeDisc 1\.[0-9]{2}\.[0-9]{3}$", RegexOptions.Compiled)
                        && !Regex.IsMatch(p, @"SafeDisc 1\.[0-9]{2}\.[0-9]{3}-[0-9]\.[0-9]{2}\.[0-9]{3}", RegexOptions.Compiled)))
                {
                    foundProtections = foundProtections.FindAll(p => !p.StartsWith("Macrovision Protection File"))
                        .FindAll(p => !p.StartsWith("Macrovision Security Driver"))
                        .FindAll(p => !Regex.IsMatch(p, @"SafeDisc [0-9]\.[0-9]{2}\.[0-9]{3}-[0-9]\.[0-9]{2}\.[0-9]{3}", RegexOptions.Compiled))
                        .FindAll(p => !Regex.IsMatch(p, @"SafeDisc [0-9]\.[0-9]{2}\.[0-9]{3}\+", RegexOptions.Compiled))
                        .FindAll(p => p != "SafeDisc")
                        .FindAll(p => p != "SafeDisc 1")
                        .FindAll(p => p != "SafeDisc 1/Lite");
                }

                // Next best case for SafeDisc 1: A SafeDisc version range is found from "SECDRV.SYS".
                // TODO: Scrub "Macrovision Security Driver {Version}" from before the SafeDisc version.
                else if (foundProtections.Exists(p => p.StartsWith("Macrovision Security Driver")
                        && Regex.IsMatch(p, @"SafeDisc 1\.[0-9]{2}\.[0-9]{3}-[1-2]\.[0-9]{2}\.[0-9]{3}", RegexOptions.Compiled)
                        || Regex.IsMatch(p, @"SafeDisc 1\.[0-9]{2}\.[0-9]{3}$")))
                {
                    foundProtections = foundProtections.FindAll(p => !p.StartsWith("Macrovision Protection File"))
                        .FindAll(p => !p.StartsWith("Macrovision Protected Application [Version Expunged]"))
                        .FindAll(p => !Regex.IsMatch(p, @"SafeDisc [0-9]\.[0-9]{2}\.[0-9]{3}\+", RegexOptions.Compiled))
                        .FindAll(p => !Regex.IsMatch(p, @"SafeDisc 1\.[0-9]{2}\.[0-9]{3}-[0-9]\.[0-9]{2}\.[0-9]{3}", RegexOptions.Compiled))
                        .FindAll(p => p != "SafeDisc")
                        .FindAll(p => p != "SafeDisc 1")
                        .FindAll(p => p != "SafeDisc 1/Lite");
                }

                // Next best case for SafeDisc 2+: A SafeDisc version range is found from "SECDRV.SYS".
                // TODO: Scrub "Macrovision Security Driver {Version}" from before the SafeDisc version.
                else if (foundProtections.Exists(p => p.StartsWith("Macrovision Security Driver")))
                {
                    foundProtections = foundProtections.FindAll(p => !p.StartsWith("Macrovision Protection File"))
                        .FindAll(p => !p.StartsWith("Macrovision Protected Application [Version Expunged]"))
                        .FindAll(p => !Regex.IsMatch(p, @"SafeDisc [0-9]\.[0-9]{2}\.[0-9]{3}\+", RegexOptions.Compiled))
                        .FindAll(p => !Regex.IsMatch(p, @"SafeDisc 1\.[0-9]{2}\.[0-9]{3}-[0-9]\.[0-9]{2}\.[0-9]{3}", RegexOptions.Compiled))
                        .FindAll(p => p != "SafeDisc")
                        .FindAll(p => p != "SafeDisc 1")
                        .FindAll(p => p != "SafeDisc 1/Lite")
                        .FindAll(p => p != "SafeDisc 2+")
                        .FindAll(p => p != "SafeDisc 3+ (DVD)");
                }

                // Only SafeDisc Lite is found.
                else if (foundProtections.Exists(p => p == "SafeDisc Lite"))
                {
                    foundProtections = foundProtections.FindAll(p => p != "SafeDisc")
                        .FindAll(p => !Regex.IsMatch(p, @"SafeDisc 1\.[0-9]{2}\.[0-9]{3}-1\.[0-9]{2}\.[0-9]{3}\/Lite", RegexOptions.Compiled));
                }

                // Only SafeDisc 3+ is found.
                else if (foundProtections.Exists(p => p == "SafeDisc 3+ (DVD)"))
                {
                    foundProtections = foundProtections.FindAll(p => p != "SafeDisc")
                        .FindAll(p => p != "SafeDisc 2+")
                        .FindAll(p => !p.StartsWith("Macrovision Protection File"))
                        .FindAll(p => !Regex.IsMatch(p, @"SafeDisc [0-9]\.[0-9]{2}\.[0-9]{3}\+", RegexOptions.Compiled));
                }

                // Only SafeDisc 2+ is found.
                else if (foundProtections.Exists(p => p == "SafeDisc 2+"))
                {
                    foundProtections = foundProtections.FindAll(p => p != "SafeDisc")
                        .FindAll(p => !p.StartsWith("Macrovision Protection File"))
                        .FindAll(p => !Regex.IsMatch(p, @"SafeDisc [0-9]\.[0-9]{2}\.[0-9]{3}\+", RegexOptions.Compiled));
                }
            }

            // SecuROM
            if (foundProtections.Exists(p => p.StartsWith("SecuROM Release Control")))
            {
                foundProtections = foundProtections.FindAll(p => !p.StartsWith("SecuROM Release Control"));
                foundProtections.Add("SecuROM Release Control");
            }

            // SolidShield
            // TODO: Figure this one out

            // StarForce
            if (foundProtections.Exists(p => p.StartsWith("StarForce")))
            {
                if (foundProtections.Exists(p => Regex.IsMatch(p, @"StarForce [0-9]+\..+", RegexOptions.Compiled)))
                {
                    foundProtections = foundProtections.FindAll(p => p != "StarForce")
                        .FindAll(p => p != "StarForce 3-5")
                        .FindAll(p => p != "StarForce 5")
                        .FindAll(p => p != "StarForce 5 [Protected Module]");
                }
                else if (foundProtections.Exists(p => p == "StarForce 5 [Protected Module]"))
                {
                    foundProtections = foundProtections.FindAll(p => p != "StarForce")
                        .FindAll(p => p != "StarForce 3-5")
                        .FindAll(p => p != "StarForce 5");
                }
                else if (foundProtections.Exists(p => p == "StarForce 5"))
                {
                    foundProtections = foundProtections.FindAll(p => p != "StarForce")
                        .FindAll(p => p != "StarForce 3-5");
                }
                else if (foundProtections.Exists(p => p == "StarForce 3-5"))
                {
                    foundProtections = foundProtections.FindAll(p => p != "StarForce");
                }
            }

            // Sysiphus
            if (foundProtections.Exists(p => p == "Sysiphus")
                && foundProtections.Exists(p => p.StartsWith("Sysiphus") && p.Length > "Sysiphus".Length))
            {
                foundProtections = foundProtections.FindAll(p => p != "Sysiphus");
            }

            // TAGES
            // TODO: Figure this one out

            // XCP
            if (foundProtections.Exists(p => p == "XCP")
                && foundProtections.Exists(p => p.StartsWith("XCP") && p.Length > "XCP".Length))
            {
                foundProtections = foundProtections.FindAll(p => p != "XCP");
            }

            // Sort and return the protections
            foundProtections.Sort();
            return string.Join(", ", [.. foundProtections]);
        }
    }
}
