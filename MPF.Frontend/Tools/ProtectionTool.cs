using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BinaryObjectScanner;

#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.

namespace MPF.Frontend.Tools
{
    public static class ProtectionTool
    {
        /// <summary>
        /// Get the current detected copy protection(s), if possible
        /// </summary>
        /// <param name="drive">Drive object representing the current drive</param>
        /// <param name="options">Options object that determines what to scan</param>
        /// <param name="progress">Optional progress callback</param>
        /// <returns>Detected copy protection(s) if possible, null on error</returns>
        public static async Task<(string?, Dictionary<string, List<string>>?)> GetCopyProtection(Drive? drive,
            Frontend.Options options,
            IProgress<ProtectionProgress>? progress = null)
        {
            if (options.ScanForProtection && drive?.Name != null)
            {
                (var protection, _) = await RunProtectionScanOnPath(drive.Name, options, progress);
                return (FormatProtections(protection), protection);
            }

            return ("(CHECK WITH PROTECTIONID)", null);
        }

        /// <summary>
        /// Run protection scan on a given path
        /// </summary>
        /// <param name="path">Path to scan for protection</param>
        /// <param name="options">Options object that determines what to scan</param>
        /// <param name="progress">Optional progress callback</param>
        /// <returns>Set of all detected copy protections with an optional error string</returns>
        public static async Task<(Dictionary<string, List<string>>?, string?)> RunProtectionScanOnPath(string path,
            Frontend.Options options,
            IProgress<ProtectionProgress>? progress = null)
        {
            try
            {
#if NET40
                var found = await Task.Factory.StartNew(() =>
                {
                    var scanner = new Scanner(
                        options.ScanArchivesForProtection,
                        scanContents: true, // Hardcoded value to avoid issues
                        scanGameEngines: false, // Hardcoded value to avoid issues
                        options.ScanPackersForProtection,
                        scanPaths: true, // Hardcoded value to avoid issues
                        options.IncludeDebugProtectionInformation,
                        progress);

                    return scanner.GetProtections(path);
                });
#else
                var found = await Task.Run(() =>
                {
                    var scanner = new Scanner(
                        options.ScanArchivesForProtection,
                        scanContents: true, // Hardcoded value to avoid issues
                        scanGameEngines: false, // Hardcoded value to avoid issues
                        options.ScanPackersForProtection,
                        scanPaths: true, // Hardcoded value to avoid issues
                        options.IncludeDebugProtectionInformation,
                        progress);

                    return scanner.GetProtections(path);
                });
#endif

                // If nothing was returned, return
#if NET20 || NET35
                if (found == null || found.Count == 0)
#else
                if (found == null || found.IsEmpty)
#endif
                    return (null, null);

                // Filter out any empty protections
                var filteredProtections = found
#if NET20 || NET35
                    .Where(kvp => kvp.Value != null && kvp.Value.Count > 0)
#else
                    .Where(kvp => kvp.Value != null && !kvp.Value.IsEmpty)
#endif
                    .ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.OrderBy(s => s).ToList());

                // Return the filtered set of protections
                return (filteredProtections, null);
            }
            catch (Exception ex)
            {
                return (null, ex.ToString());
            }
        }

        /// <summary>
        /// Format found protections to a deduplicated, ordered string
        /// </summary>
        /// <param name="protections">Dictionary of file to list of protection mappings</param>
        /// <returns>Detected protections, if any</returns>
        public static string? FormatProtections(Dictionary<string, List<string>>? protections)
        {
            // If the filtered list is empty in some way, return
            if (protections == null || !protections.Any())
                return "None found [OMIT FROM SUBMISSION]";

            // Get an ordered list of distinct found protections
            var orderedDistinctProtections = protections
                .SelectMany(kvp => kvp.Value)
                .Distinct()
                .OrderBy(p => p);

            // Sanitize and join protections for writing
            string protectionString = SanitizeFoundProtections(orderedDistinctProtections);
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
            {
                try
                {
                    var antiModchip = new BinaryObjectScanner.Protection.PSXAntiModchip();
                    foreach (string file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
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
#else
            return await Task.Run(() =>
            {
                try
                {
                    var antiModchip = new BinaryObjectScanner.Protection.PSXAntiModchip();
#if NET20 || NET35
                    foreach (string file in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
#else
                    foreach (string file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
#endif
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
#endif
        }

        /// <summary>
        /// Sanitize unnecessary protection duplication from output
        /// </summary>
        /// <param name="foundProtections">Enumerable of found protections</param>
        public static string SanitizeFoundProtections(IEnumerable<string> foundProtections)
        {
            // EXCEPTIONS
            if (foundProtections.Any(p => p.StartsWith("[Exception opening file")))
            {
                foundProtections = foundProtections.Where(p => !p.StartsWith("[Exception opening file"));
#if NET20 || NET35 || NET40 || NET452 || NET462
                var tempList = new List<string> { "Exception occurred while scanning [RESCAN NEEDED]" };
                tempList.AddRange(foundProtections);
                foundProtections = tempList.OrderBy(p => p);
#else
                foundProtections = foundProtections
                    .Prepend("Exception occurred while scanning [RESCAN NEEDED]")
                    .OrderBy(p => p);
#endif
            }

            // ActiveMARK
            if (foundProtections.Any(p => p == "ActiveMARK 5") && foundProtections.Any(p => p == "ActiveMARK"))
                foundProtections = foundProtections.Where(p => p != "ActiveMARK");

            // Cactus Data Shield
            if (foundProtections.Any(p => Regex.IsMatch(p, @"Cactus Data Shield [0-9]{3} .+", RegexOptions.Compiled)) && foundProtections.Any(p => p == "Cactus Data Shield 200"))
                foundProtections = foundProtections.Where(p => p != "Cactus Data Shield 200");

            // CD-Check
            foundProtections = foundProtections.Where(p => p != "Executable-Based CD Check");

            // CD-Cops
            if (foundProtections.Any(p => p == "CD-Cops") && foundProtections.Any(p => p.StartsWith("CD-Cops") && p.Length > "CD-Cops".Length))
                foundProtections = foundProtections.Where(p => p != "CD-Cops");

            // CD-Key / Serial
            foundProtections = foundProtections.Where(p => p != "CD-Key / Serial");

            // Electronic Arts
            if (foundProtections.Any(p => p == "EA CdKey Registration Module") && foundProtections.Any(p => p.StartsWith("EA CdKey Registration Module") && p.Length > "EA CdKey Registration Module".Length))
                foundProtections = foundProtections.Where(p => p != "EA CdKey Registration Module");
            if (foundProtections.Any(p => p == "EA DRM Protection") && foundProtections.Any(p => p.StartsWith("EA DRM Protection") && p.Length > "EA DRM Protection".Length))
                foundProtections = foundProtections.Where(p => p != "EA DRM Protection");

            // Games for Windows LIVE
            if (foundProtections.Any(p => p == "Games for Windows LIVE") && foundProtections.Any(p => p.StartsWith("Games for Windows LIVE") && !p.Contains("Zero Day Piracy Protection") && p.Length > "Games for Windows LIVE".Length))
                foundProtections = foundProtections.Where(p => p != "Games for Windows LIVE");

            // Impulse Reactor
            if (foundProtections.Any(p => p.StartsWith("Impulse Reactor Core Module")) && foundProtections.Any(p => p == "Impulse Reactor"))
                foundProtections = foundProtections.Where(p => p != "Impulse Reactor");

            // JoWood X-Prot
            if (foundProtections.Any(p => p.StartsWith("JoWood X-Prot")))
            {
                if (foundProtections.Any(p => Regex.IsMatch(p, @"JoWood X-Prot [0-9]\.[0-9]\.[0-9]\.[0-9]{2}", RegexOptions.Compiled)))
                {
                    foundProtections = foundProtections.Where(p => p != "JoWood X-Prot")
                        .Where(p => p != "JoWood X-Prot v1.0-v1.3")
                        .Where(p => p != "JoWood X-Prot v1.4+")
                        .Where(p => p != "JoWood X-Prot v2");
                }
                else if (foundProtections.Any(p => p == "JoWood X-Prot v2"))
                {
                    foundProtections = foundProtections.Where(p => p != "JoWood X-Prot")
                        .Where(p => p != "JoWood X-Prot v1.0-v1.3")
                        .Where(p => p != "JoWood X-Prot v1.4+");
                }
                else if (foundProtections.Any(p => p == "JoWood X-Prot v1.4+"))
                {
                    foundProtections = foundProtections.Where(p => p != "JoWood X-Prot")
                        .Where(p => p != "JoWood X-Prot v1.0-v1.3");
                }
                else if (foundProtections.Any(p => p == "JoWood X-Prot v1.0-v1.3"))
                {
                    foundProtections = foundProtections.Where(p => p != "JoWood X-Prot");
                }
            }

            // LaserLok
            // TODO: Figure this one out

            // Online Registration
            foundProtections = foundProtections.Where(p => !p.StartsWith("Executable-Based Online Registration"));

            // ProtectDISC / VOB ProtectCD/DVD
            // TODO: Figure this one out

            // SafeCast
            // TODO: Figure this one out

            // Cactus Data Shield / SafeDisc
            if (foundProtections.Any(p => p == "Cactus Data Shield 300 (Confirm presence of other CDS-300 files)"))
            {
                foundProtections = foundProtections
                    .Where(p => p != "Cactus Data Shield 300 (Confirm presence of other CDS-300 files)");

                if (foundProtections.Any(p => !p.StartsWith("SafeDisc")))
                {
#if NET20 || NET35 || NET40 || NET452 || NET462
                    var tempList = new List<string>();
                    tempList.AddRange(foundProtections);
                    tempList.Add("Cactus Data Shield 300");
                    foundProtections = tempList;
#else
                    foundProtections = foundProtections.Append("Cactus Data Shield 300");
#endif
                }
            }

            // SafeDisc
            if (foundProtections.Any(p => p.StartsWith("SafeDisc")))
            {
                // Best case scenario for new SafeDisc 2+: A full SafeDisc version is found in a line starting with "Macrovision Protect Application". All other SafeDisc detections can be safely scrubbed.
                // TODO: Scrub "Macrovision Protected Application, " from before the SafeDisc version. Also scrub the "SafeDisc SRV Tool APP" portion of the detection when not needed.
                if (foundProtections.Any(p => Regex.IsMatch(p, @"SafeDisc [0-9]\.[0-9]{2}\.[0-9]{3}", RegexOptions.Compiled) && p.StartsWith("Macrovision Protected Application")))
                {
                    foundProtections = foundProtections.Where(p => !p.StartsWith("Macrovision Protection File"))
                        .Where(p => !p.StartsWith("Macrovision Security Driver"))
                        .Where(p => p != "SafeDisc")
                        .Where(p => !p.StartsWith("Macrovision Protected Application, Macrovision Protected Application [Version Expunged]"))
                        .Where(p => !(Regex.IsMatch(p, @"SafeDisc [0-9]\.[0-9]{2}\.[0-9]{3}-[0-9]\.[0-9]{2}\.[0-9]{3}", RegexOptions.Compiled)))
                        .Where(p => !(Regex.IsMatch(p, @"SafeDisc [0-9]\.[0-9]{2}\.[0-9]{3}\+", RegexOptions.Compiled)))
                        .Where(p => !(Regex.IsMatch(p, @"SafeDisc [0-9]\.[0-9]{2}\.[0-9]{3}\/4\+", RegexOptions.Compiled)))
                        .Where(p => p != "SafeDisc 1/Lite")
                        .Where(p => p != "SafeDisc 2+")
                        .Where(p => p != "SafeDisc 3+ (DVD)");
                }
                // Best case for SafeDisc 1.X: A full SafeDisc version is found.
                // TODO: Scrub unneeded detections better
                else if (foundProtections.Any(p => Regex.IsMatch(p, @"SafeDisc 1\.[0-9]{2}\.[0-9]{3}", RegexOptions.Compiled)))
                {
                    foundProtections = foundProtections.Where(p => !p.StartsWith("Macrovision Protection File"))
                        .Where(p => !p.StartsWith("Macrovision Security Driver"))
                        .Where(p => !(Regex.IsMatch(p, @"SafeDisc [0-9]\.[0-9]{2}\.[0-9]{3}\+", RegexOptions.Compiled)))
                        .Where(p => p != "SafeDisc")
                        .Where(p => p != "SafeDisc 1/Lite");

                }
                // Next best case: A SafeDisc version range is found from "SECDRV.SYS".  
                else if (foundProtections.Any(p => !p.StartsWith("Macrovision Security Driver")))
                {
                    foundProtections = foundProtections.Where(p => !p.StartsWith("Macrovision Protection File"))
                        .Where(p => !p.StartsWith("Macrovision Protected Application, Macrovision Protected Application [Version Expunged]"))
                        .Where(p => !(Regex.IsMatch(p, @"SafeDisc [0-9]\.[0-9]{2}\.[0-9]{3}\+", RegexOptions.Compiled)))
                        .Where(p => p != "SafeDisc")
                        .Where(p => p != "SafeDisc 1/Lite")
                        .Where(p => p != "SafeDisc 2+")
                        .Where(p => p != "SafeDisc 3+ (DVD)");
                }
                // TODO: Make sure edge cases, like SafeDisc Lite, are supported properly.
                // TODO: Organize and re-add the following cases if applicable: 
                /*
                else if (foundProtections.Any(p => Regex.IsMatch(p, @"SafeDisc [0-9]\.[0-9]{2}\.[0-9]{3}", RegexOptions.Compiled) && !p.StartsWith("Macrovision Protection File")))
                {
                    foundProtections = foundProtections.Where(p => !p.StartsWith("Macrovision Protected Application"))
                        .Where(p => !p.StartsWith("Macrovision Protection File"))
                        .Where(p => !p.StartsWith("Macrovision Security Driver"))
                        .Where(p => p != "SafeDisc")
                        .Where(p => !(Regex.IsMatch(p, @"SafeDisc [0-9]\.[0-9]{2}\.[0-9]{3}-[0-9]\.[0-9]{2}\.[0-9]{3}", RegexOptions.Compiled)))
                        .Where(p => !(Regex.IsMatch(p, @"SafeDisc [0-9]\.[0-9]{2}\.[0-9]{3}/+", RegexOptions.Compiled)))
                        .Where(p => p != "SafeDisc 1/Lite")
                        .Where(p => p != "SafeDisc 2+");
                }
                else if (foundProtections.Any(p => Regex.IsMatch(p, @"SafeDisc [0-9]\.[0-9]{2}\.[0-9]{3}-[0-9]\.[0-9]{2}\.[0-9]{3}", RegexOptions.Compiled) && !p.StartsWith("Macrovision Protection File")))
                {
                    foundProtections = foundProtections.Where(p => !p.StartsWith("Macrovision Protected Application"))
                        .Where(p => !p.StartsWith("Macrovision Protection File"))
                        .Where(p => !p.StartsWith("Macrovision Security Driver"))
                        .Where(p => p != "SafeDisc")
                        .Where(p => !(Regex.IsMatch(p, @"SafeDisc [0-9]\.[0-9]{2}\.[0-9]{3}/+", RegexOptions.Compiled)))
                        .Where(p => p != "SafeDisc 1/Lite")
                        .Where(p => p != "SafeDisc 2+");
                }
                else if (foundProtections.Any(p => Regex.IsMatch(p, @"SafeDisc [0-9]\.[0-9]{2}\.[0-9]{3}/+", RegexOptions.Compiled) && !p.StartsWith("Macrovision Protection File")))
                {
                    foundProtections = foundProtections.Where(p => !p.StartsWith("Macrovision Protected Application"))
                        .Where(p => !p.StartsWith("Macrovision Protection File"))
                        .Where(p => !p.StartsWith("Macrovision Security Driver"))
                        .Where(p => p != "SafeDisc")
                        .Where(p => p != "SafeDisc 1/Lite")
                        .Where(p => p != "SafeDisc 2+");
                }
                else if (foundProtections.Any(p => p.StartsWith("Macrovision Security Driver")))
                {
                    foundProtections = foundProtections.Where(p => !p.StartsWith("Macrovision Protected Application"))
                        .Where(p => !p.StartsWith("Macrovision Protection File"))
                        .Where(p => p != "SafeDisc")
                        .Where(p => p != "SafeDisc 1/Lite")
                        .Where(p => p != "SafeDisc 2+");
                }
                else if (foundProtections.Any(p => p == "SafeDisc 2+"))
                {
                    foundProtections = foundProtections.Where(p => !p.StartsWith("Macrovision Protected Application"))
                        .Where(p => !p.StartsWith("Macrovision Protection File"))
                        .Where(p => p != "SafeDisc");
                }
                else if (foundProtections.Any(p => p == "SafeDisc 1/Lite"))
                {
                    foundProtections = foundProtections.Where(p => !p.StartsWith("Macrovision Protected Application"))
                        .Where(p => !p.StartsWith("Macrovision Protection File"))
                        .Where(p => p != "SafeDisc");
                }*/
            }

            // SecuROM
            // TODO: Figure this one out

            // SolidShield
            // TODO: Figure this one out

            // StarForce
            if (foundProtections.Any(p => p.StartsWith("StarForce")))
            {
                if (foundProtections.Any(p => Regex.IsMatch(p, @"StarForce [0-9]+\..+", RegexOptions.Compiled)))
                {
                    foundProtections = foundProtections.Where(p => p != "StarForce")
                        .Where(p => p != "StarForce 3-5")
                        .Where(p => p != "StarForce 5")
                        .Where(p => p != "StarForce 5 [Protected Module]");
                }
                else if (foundProtections.Any(p => p == "StarForce 5 [Protected Module]"))
                {
                    foundProtections = foundProtections.Where(p => p != "StarForce")
                        .Where(p => p != "StarForce 3-5")
                        .Where(p => p != "StarForce 5");
                }
                else if (foundProtections.Any(p => p == "StarForce 5"))
                {
                    foundProtections = foundProtections.Where(p => p != "StarForce")
                        .Where(p => p != "StarForce 3-5");
                }
                else if (foundProtections.Any(p => p == "StarForce 3-5"))
                {
                    foundProtections = foundProtections.Where(p => p != "StarForce");
                }
            }

            // Sysiphus
            if (foundProtections.Any(p => p == "Sysiphus") && foundProtections.Any(p => p.StartsWith("Sysiphus") && p.Length > "Sysiphus".Length))
                foundProtections = foundProtections.Where(p => p != "Sysiphus");

            // TAGES
            // TODO: Figure this one out

            // XCP
            if (foundProtections.Any(p => p == "XCP") && foundProtections.Any(p => p.StartsWith("XCP") && p.Length > "XCP".Length))
                foundProtections = foundProtections.Where(p => p != "XCP");

            return string.Join(", ", foundProtections.ToArray());
        }
    }
}
