using System;
using System.Collections.Generic;
using System.IO;
#if NET35_OR_GREATER || NETCOREAPP
using System.Linq;
#endif
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BinaryObjectScanner;
using MPF.Frontend.FileSystems;
using SabreTools.Collections.Extensions;
using SabreTools.IO.Extensions;
using SabreTools.Wrappers;

#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'
namespace MPF.Frontend.Tools
{
    public static partial class ProtectionTool
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
            "Crunch",
            "dotFuscator",
            "Embedded",
            "EXE Stealth",
            "Gentee Installer",
            "GP-Install",
            "HyperTech CrackProof",
            "Inno Setup",
            "InstallAnywhere",
            "Installer VISE",
            "Intel Installation Framework",
            "Microsoft CAB SFX",
            "MPRESS",
            "NeoLite",
            "NSIS",
            "PE Compact",
            "PEtite",
            "Setup Factory",
            "Shrinker",
            "Spoon Installer",
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
        /// Determine if the drive path can be scanned as a set of files
        /// </summary>
        /// <param name="drive">Drive object representing the current drive</param>
        /// <returns>True if the drive path is a readable directory, false otherwise</returns>
        /// <remarks>Device paths such as /dev/sr0 or disk2 are not directories</remarks>
        public static bool CanScanDrivePath(Drive? drive)
            => Directory.Exists(drive?.DevicePath);

        /// <summary>
        /// Run comprehensive protection scans based on both the
        /// physical media as well as the image
        /// </summary>
        /// <param name="basePath">Base output image path</param>
        /// <param name="drive">Drive object representing the current drive</param>
        /// <param name="options">Options object that determines what to scan</param>
        /// <param name="protectionProgress">Optional progress callback</param>
        /// <param name="resultProgress">Optional callback noting where the scanned files are read from</param>
        /// <returns>Set of all detected copy protections, null if no files could be scanned</returns>
        public static async Task<Dictionary<string, List<string>>?> RunCombinedProtectionScans(string basePath,
            Drive? drive,
            Options options,
            IProgress<ProtectionProgress>? protectionProgress = null,
            IProgress<ResultEventArgs>? resultProgress = null)
        {
            // Setup the output protections dictionary
            Dictionary<string, List<string>> protections = [];

            // Scan the disc image, if possible
            foreach (string imagePath in DiscFileSystem.GetDumpImagePaths(basePath))
            {
                var imageProtections = await RunProtectionScanOnImage(imagePath, options, protectionProgress);
                protections.MergeWith(imageProtections);
            }

            // Scan the mounted drive path
            bool scanDumpImageFiles = options.Processing.ProtectionScanning.ScanDumpImageFiles;
            if (CanScanDrivePath(drive) && !scanDumpImageFiles)
            {
                resultProgress?.Report(ResultEventArgs.Neutral("Scanning the files of the disc in the drive for protection"));
                var driveProtections = await RunProtectionScanOnPath(drive!.DevicePath!, options, protectionProgress);
                protections.MergeWith(driveProtections);
                return protections;
            }

            // Otherwise scan the files of the dump image, without reading the disc again
            Dictionary<string, List<string>>? fileProtections;
            using (var imageFiles = DiscFileSystem.OpenDump(basePath))
            using (var driveFiles = imageFiles is null ? DiscFileSystem.OpenDrive(drive) : null)
            {
                if (imageFiles is not null)
                {
                    string reason = scanDumpImageFiles ? "set in options" : "no mounted disc path";
                    resultProgress?.Report(ResultEventArgs.Neutral($"Scanning the files of the dump image for protection ({reason})"));
                    fileProtections = await RunProtectionScanOnFiles(imageFiles, imageFiles.SourcePath!, options, protectionProgress);
                }
                else if (driveFiles is not null && driveFiles.DirectoryExists(string.Empty))
                {
                    // Without readable files in the dump image, the disc in the drive is scanned
                    resultProgress?.Report(ResultEventArgs.Neutral("Scanning the files of the disc in the drive for protection (no readable dump image)"));
                    fileProtections = await RunProtectionScanOnFiles(driveFiles, drive!.DevicePath!, options, protectionProgress);
                }
                else
                {
                    fileProtections = null;
                }
            }

            // An empty result is only meaningful if files were scanned
            if (fileProtections is null)
                return null;

            protections.MergeWith(fileProtections);
            return protections;
        }

        /// <summary>
        /// Run protection scan on the files of a disc
        /// </summary>
        /// <param name="files">Files of the disc to scan</param>
        /// <param name="sourcePath">Path of the disc image or device, put in front of the scanned file names</param>
        /// <param name="options">Options object that determines what to scan</param>
        /// <param name="progress">Optional progress callback</param>
        /// <returns>Set of all detected copy protections, null if the disc has no files</returns>
        /// <remarks>
        /// The files of a disc image or device are written to a temporary directory and scanned there,
        /// without extracting the whole disc: the scanner identifies a file by its first bytes and only
        /// parses files of known types that it can open as that type, so other files only get the bytes read
        /// to identify and open them and their length (as a sparse file), which keeps their name, size and
        /// first bytes for the path checks
        /// </remarks>
        public static async Task<Dictionary<string, List<string>>?> RunProtectionScanOnFiles(DiscFileSystem files,
            string sourcePath,
            Options options,
            IProgress<ProtectionProgress>? progress = null)
        {
            // Files in a directory are scanned in place
            if (files is FolderFileSystem folder)
                return await RunProtectionScanOnPath(folder.Root, options, progress);

            if (files is not ImageFileSystem image)
                return null;

            string mirrorPath = Path.GetFullPath(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));
            return await RunProtectionScanOnFiles(image, sourcePath, options, progress, mirrorPath);
        }

        /// <summary>
        /// Run protection scan on the files of a disc image or device in a temporary directory
        /// </summary>
        /// <param name="image">Disc image or device to scan</param>
        /// <param name="sourcePath">Path of the disc image or device, put in front of the scanned file names</param>
        /// <param name="options">Options object that determines what to scan</param>
        /// <param name="progress">Optional progress callback</param>
        /// <param name="mirrorPath">Temporary directory to write the files to, removed afterwards</param>
        /// <returns>Set of all detected copy protections, null if the disc has no files</returns>
        internal static async Task<Dictionary<string, List<string>>?> RunProtectionScanOnFiles(ImageFileSystem image,
            string sourcePath,
            Options options,
            IProgress<ProtectionProgress>? progress,
            string mirrorPath)
        {
            var entries = image.GetAllEntries();
            if (!entries.Exists(entry => !entry.IsDirectory))
                return null;

            Directory.CreateDirectory(mirrorPath);
            try
            {
#if NET40
                await Task.Factory.StartNew(() => CreateMirror(image, entries, mirrorPath));
#else
                await Task.Run(() => CreateMirror(image, entries, mirrorPath));
#endif
                var found = await RunProtectionScanOnPath(mirrorPath, options, progress);

                // Name the scanned files after the disc image or device instead of the temporary directory
                Dictionary<string, List<string>> protections = [];
                foreach (var kvp in found)
                {
                    protections[$"{sourcePath}{kvp.Key.Substring(mirrorPath.Length)}"] = kvp.Value;
                }

                return protections;
            }
            finally
            {
                Directory.Delete(mirrorPath, true);
            }
        }

        /// <summary>
        /// Write the files of a disc to a directory, in full only where the scanner parses them
        /// </summary>
        /// <param name="image">Disc image or device to read</param>
        /// <param name="entries">Files and directories of the disc</param>
        /// <param name="mirrorPath">Directory to write to</param>
        /// <returns>Number of files written</returns>
        internal static int CreateMirror(ImageFileSystem image, List<DiscEntry> entries, string mirrorPath)
        {
            int written = 0;
            var buffer = new byte[1024 * 1024];
            foreach (var entry in entries)
            {
                // Names that cannot be written as a path on this system are skipped
                if (!IsSafeRelativePath(entry.Path))
                    continue;

                string target = Path.Combine(mirrorPath, entry.Path.Replace('/', Path.DirectorySeparatorChar));
                if (entry.IsDirectory)
                {
                    Directory.CreateDirectory(target);
                    continue;
                }

                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                written++;
                using var output = File.Create(target);
                using var source = image.OpenEntry(entry);

                // Files that cannot be read, such as interleaved files, keep their length without their contents
                if (source is null)
                {
                    SetSparseLength(output, entry.Length);
                    continue;
                }

                // What is read to identify and open the file is written as it is read, so it is read only once
                var copying = new CopyingStream(source, output);

                // Read the first sector, which holds the bytes the scanner identifies the file by
                int headerLength = ReadFully(copying, buffer, 2048);

                byte[] magic = new byte[Math.Min(16, headerLength)];
                Array.Copy(buffer, magic, magic.Length);
                string extension = Path.GetExtension(entry.Name).ToLowerInvariant().TrimStart('.');
                bool readInFull = IsReadInFull(magic, extension, copying);

                // Opening a file as its type ignores errors, which stop the scan like those of other files
                if (copying.Error is not null)
                    throw new IOException($"{entry.Path} could not be copied", copying.Error);

                // Files the scanner does not read further keep their length without the rest of their contents
                if (!readInFull)
                {
                    SetSparseLength(output, entry.Length);
                    continue;
                }

                source.Seek(copying.Copied, SeekOrigin.Begin);
                int read;
                while ((read = source.Read(buffer, 0, buffer.Length)) > 0)
                {
                    output.Write(buffer, 0, read);
                }
            }

            return written;
        }

        /// <summary>
        /// Set the length of a file without writing the rest of it, which reads as zeros
        /// </summary>
        /// <param name="file">File to set the length of</param>
        /// <param name="length">Length to set</param>
        private static void SetSparseLength(FileStream file, long length)
        {
            // Windows only leaves out the rest of a file that is marked as sparse
            if (SystemEnvironment.IsWindows())
                MarkWindowsSparseFile(file);

            file.SetLength(length);
        }

        /// <summary>
        /// Determine if the scanner reads a file further than its first bytes
        /// </summary>
        /// <param name="magic">First bytes of the file</param>
        /// <param name="extension">Extension of the file, without the period</param>
        /// <param name="source">Stream of the file</param>
        /// <returns>True if the file is of a known type and can be opened as that type, or is a text file, false otherwise</returns>
        private static bool IsReadInFull(byte[] magic, string extension, Stream source)
        {
            var fileType = WrapperFactory.GetFileType(magic, extension);
            if (fileType == WrapperType.UNKNOWN)
                return false;

            // Text files are scanned without being opened as a type
            if (fileType == WrapperType.Textfile)
                return true;

            // Opening a file takes many small reads, which a drive answers faster in larger pieces
            source.Seek(0, SeekOrigin.Begin);
            return WrapperFactory.CreateWrapper(fileType, new BufferedStream(source, 64 * 1024)) is not null;
        }

        /// <summary>
        /// Read up to a number of bytes from the start of a stream
        /// </summary>
        /// <returns>Number of bytes read</returns>
        private static int ReadFully(Stream stream, byte[] buffer, int count)
        {
            int total = 0;
            while (total < count)
            {
                int read = stream.Read(buffer, total, count - total);
                if (read <= 0)
                    break;

                total += read;
            }

            return total;
        }

        /// <summary>
        /// Determine if a relative path from a disc can be written below a directory
        /// </summary>
        /// <param name="path">Relative path, separated by '/'</param>
        /// <returns>True if every name of the path is a valid file name, false otherwise</returns>
        internal static bool IsSafeRelativePath(string path)
        {
            char[] invalid = Path.GetInvalidFileNameChars();
            foreach (string name in path.Split('/'))
            {
                if (name.Length == 0 || name == "." || name == ".." || name.IndexOfAny(invalid) >= 0)
                    return false;
            }

            return true;
        }

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
                    options.Processing.ProtectionScanning.ScanArchivesForProtection,
                    scanContents: true, // Hardcoded value to avoid issues
                    scanPaths: true, // Hardcoded value to avoid issues
                    scanSubdirectories: true, // Hardcoded value to avoid issues
                    options.Processing.ProtectionScanning.IncludeDebugProtectionInformation,
                    progress);

                return scanner.GetProtections(Path.GetFullPath(path));
            });

            // If nothing was returned, return
            if (found is null || found.Count == 0)
                return [];

            // Return the filtered set of protections
            return found;
        }

        /// <summary>
        /// Run protection scan on a disc image
        /// </summary>
        /// <param name="image">Image path to scan for protection</param>
        /// <param name="options">Options object that determines what to scan</param>
        /// <param name="progress">Optional progress callback</param>
        /// <returns>Set of all detected copy protections with an optional error string</returns>
        public static async Task<Dictionary<string, List<string>>> RunProtectionScanOnImage(string image,
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
                    scanArchives: false, // Disable extracting disc images for now
                    scanContents: true, // Enabled for image scanning
                    scanPaths: false, // Disabled for image scanning
                    scanSubdirectories: false, // Disabled for image scanning
                    options.Processing.ProtectionScanning.IncludeDebugProtectionInformation,
                    progress);

                return scanner.GetProtections(Path.GetFullPath(image));
            });

            // If nothing was returned, return
            if (found is null || found.Count == 0)
                return [];

            // Return the filtered set of protections
            return found;
        }

        /// <summary>
        /// Format found protections to a deduplicated, ordered string
        /// </summary>
        /// <param name="protections">Dictionary of file to list of protection mappings, null if no files were scanned</param>
        /// <returns>Detected protections, if any</returns>
        public static string? FormatProtections(Dictionary<string, List<string>>? protections)
        {
            // If the filtered list is empty in some way, return
            if (protections is null)
                return "[EXTERNAL SCAN NEEDED]";
            else if (protections.Count == 0)
                return "No protections found [OMIT LINE FROM SUBMISSION]";

            // Sanitize context-sensitive protections
            var sanitized = SanitizeContextSensitiveProtections(protections);

            // Get a list of distinct found protections
#if NET20
            var protectionValues = new List<string>();
            foreach (var value in sanitized.Values)
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
            var protectionValues = sanitized
                .SelectMany(kvp => kvp.Value)
                .Distinct()
                .ToList();
#endif

            // Sanitize and join protections for writing
            string protectionString = SanitizeFoundProtections(protectionValues);
            if (string.IsNullOrEmpty(protectionString))
                return "No protections found [OMIT LINE FROM SUBMISSION]";

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
                    foreach (string file in path!.SafeGetFiles("*", SearchOption.AllDirectories))
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
                if (!protections.TryGetValue(path, out var values) || values is null || values.Count == 0)
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
                    if (!filtered.TryGetValue(path, out var values) || values is null || values.Count == 0)
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
                if (foundProtections.Exists(p => Regex.IsMatch(p, @"SafeDisc [0-9]\.[0-9]{2}\.[0-9]{3}", RegexOptions.Compiled)
                        && p.Contains("Macrovision Protected Application")
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
                    foundProtections = foundProtections.ConvertAll(p => p
                            .Replace("Macrovision Protected Application, ", string.Empty)
                            .Replace(", Macrovision Protected Application", string.Empty));
                }

                // Next best case for SafeDisc 2+: A full SafeDisc version is found from the "SafeDisc SRV Tool APP".
                else if (foundProtections.Exists(p => Regex.IsMatch(p, @"SafeDisc [0-9]\.[0-9]{2}\.[0-9]{3}", RegexOptions.Compiled)
                        && p.Contains("Macrovision Protected Application")
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
                else if (foundProtections.Exists(p => (p.StartsWith("Macrovision Security Driver")
                        && Regex.IsMatch(p, @"SafeDisc 1\.[0-9]{2}\.[0-9]{3}-[1-2]\.[0-9]{2}\.[0-9]{3}", RegexOptions.Compiled))
                        || Regex.IsMatch(p, @"SafeDisc 1\.[0-9]{2}\.[0-9]{3}$")))
                {
                    foundProtections = foundProtections.FindAll(p => !p.StartsWith("Macrovision Protection File"))
                        .FindAll(p => !p.StartsWith("Macrovision Protected Application [Version Expunged]"))
                        .FindAll(p => !Regex.IsMatch(p, @"SafeDisc [0-9]\.[0-9]{2}\.[0-9]{3}\+", RegexOptions.Compiled))
                        .FindAll(p => !Regex.IsMatch(p, @"SafeDisc 1\.[0-9]{2}\.[0-9]{3}-[0-9]\.[0-9]{2}\.[0-9]{3}", RegexOptions.Compiled))
                        .FindAll(p => p != "SafeDisc")
                        .FindAll(p => p != "SafeDisc 1")
                        .FindAll(p => p != "SafeDisc 1/Lite");
                    foundProtections = foundProtections.ConvertAll(p => p.StartsWith("Macrovision Security Driver") && p.Split('/').Length > 1
                        ? p.Split('/')[1].TrimStart()
                        : p);
                }

                // Next best case for SafeDisc 2+: A SafeDisc version range is found from "SECDRV.SYS".
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
                    foundProtections = foundProtections.ConvertAll(p => p.StartsWith("Macrovision Security Driver") && p.Split('/').Length > 1
                        ? p.Split('/')[1].TrimStart()
                        : p);
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

            if (foundProtections.Exists(p => p.StartsWith("StarForce Keyless")))
            {
                foundProtections = foundProtections.FindAll(p => !p.StartsWith("StarForce Keyless"));
                foundProtections.Add("StarForce Keyless");
            }

            // Steam
            if (foundProtections.Exists(p => p == "Steam")
                && foundProtections.Exists(p => p.StartsWith("Steam")
                    && p.Length > "Steam".Length))
            {
                foundProtections = foundProtections.FindAll(p => p != "Steam");
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
