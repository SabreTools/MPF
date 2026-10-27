using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using MPF.Processors.OutputFiles;
using SabreTools.Data.Models.Logiqx;
using SabreTools.Hashing;
using SabreTools.RedumpLib.Data;

namespace MPF.Processors
{
    /// <summary>
    /// Represents processing WiiU WUDD
    /// </summary>
    public sealed class WUDD : BaseProcessor
    {
        /// <inheritdoc/>
        public WUDD(PhysicalSystem? system) : base(system) { }

        #region BaseProcessor Implementations

        /// <inheritdoc/>
        public override PhysicalMediaType? DeterminePhysicalMediaType(string? outputDirectory, string outputFilename)
            => PhysicalMediaType.NintendoWiiUOpticalDisc;

        /// <inheritdoc/>
        public override void GenerateSubmissionInfo(SubmissionInfo info, PhysicalMediaType? mediaType, string basePath, bool redumpCompat)
        {
            // Get the logfile path
            string logPath = GetLogFilePath(basePath) ?? $"{basePath}.txt";

            info.DumpMetadata.Comments = string.Empty;

            // Get the dumping program and version
            info.DumpingInfo.DumpingProgram ??= string.Empty;
            info.DumpingInfo.DumpingProgram += $" {GetVersion(logPath) ?? "Unknown Version"}";
            info.DumpingInfo.DumpingDate = GetDumpStartDateTime(logPath);

            // Get the Datafile information
            var datafile = GenerateWUDDDatafile($"{basePath}.wud", logPath);
            info.DumpMetadata.Dat = ProcessingTool.GenerateDatfile(datafile);

            // Get disc ID
            info.DiscIdentifiers.DiscID = GetDiscID(logPath);

            // Get disc key
            info.DiscIdentifiers.DiscKey = GetDiscKey($"{basePath}.key");
        }

        /// <inheritdoc/>
        internal override List<OutputFile> GetOutputFiles(PhysicalMediaType? mediaType, string? outputDirectory, string outputFilename)
        {
            // Remove the extension by default
            outputFilename = Path.GetFileNameWithoutExtension(outputFilename);

            return [
                new($"{outputFilename}.wud", OutputFileFlags.Required),
                new RegexOutputFile ([$"{outputFilename}.txt", @"\d{4}-\d{2}-\d{2}-\d{2}-\d{2}-\d{2}\.txt"], OutputFileFlags.Required
                    | OutputFileFlags.Artifact
                    | OutputFileFlags.Zippable,
                    "log"),
                new ($"{outputFilename}.key", OutputFileFlags.Binary
                    | OutputFileFlags.Zippable,
                    "key"),
            ];
        }

        #endregion

        #region Private Extra Methods

        /// <summary>
        /// Get the log file path, if possible
        /// </summary>
        /// <param name="basePath">Base filename and path to use for checking</param>
        /// <returns>The path as a string, a default value on no match, null on error</returns>
        internal static string? GetLogFilePath(string basePath)
        {
            // If we have an invalid base path, we can do nothing
            if (string.IsNullOrEmpty(basePath))
                return null;

            // Generate the matching regex based on the base path
            var logFilenameRegex = new Regex(@"\d{4}-\d{2}-\d{2}-\d{2}-\d{2}-\d{2}\.txt");

            // Find the first match for the log file
            var parentDirectory = Path.GetDirectoryName(basePath);
            if (string.IsNullOrEmpty(parentDirectory))
                return null;

            var currentFiles = Directory.GetFiles(parentDirectory);
            string? logPath = Array.Find(currentFiles, f => logFilenameRegex.IsMatch(f));
            if (string.IsNullOrEmpty(value: logPath))
                return null;

            // Extract the version string
            var match = logFilenameRegex.Match(logPath);
            return match.Groups[1].Value;
        }

        #endregion

        #region Information Extraction Methods

        /// <summary>
        /// Get a formatted datfile from the WUDD output, if possible
        /// </summary>
        /// <param name="wud">Path to WUD file</param>
        /// <param name="log">Log file location</param>
        /// <returns></returns>
        internal static Datafile? GenerateWUDDDatafile(string wud, string log)
        {
            // Get the size from the WUD, if possible
            // TODO: Pull size from the log as a backup
            long size = !string.IsNullOrEmpty(wud) && File.Exists(wud)
                ? new FileInfo(wud).Length
                : -1;

            // Setup the hashes
            string crc = string.Empty;
            string md5 = string.Empty;
            string sha1 = string.Empty;
            string sha256 = string.Empty;

            try
            {
                // Make sure this file is a WUDD log
                if (!string.IsNullOrEmpty(log) && File.Exists(log))
                {
                    using var sr = File.OpenText(log);
                    if (sr.ReadLine()?.Contains("-WUDD log file-") != true)
                        return null;

                    // Fast forward to the PVD
                    while (sr.ReadLine()?.StartsWith("Hashes of uncompressed WUD") == false) ;

                    // Read all lines and gather dat information
                    while (!sr.EndOfStream)
                    {
                        var line = sr.ReadLine()?.Trim();
                        if (line!.StartsWith("CRC32"))
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
                            crc = line[7..].ToLowerInvariant();
#else
                            crc = line.Substring(7).ToLowerInvariant();
#endif
                        else if (line.StartsWith("MD5"))
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
                            md5 = line[5..];
#else
                            md5 = line.Substring(5);
#endif
                        else if (line.StartsWith("SHA1"))
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
                            sha1 = line[6..];
#else
                            sha1 = line.Substring(6);
#endif
                        else if (line.StartsWith("SHA256"))
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
                            sha256 = line[8..];
#else
                            sha256 = line.Substring(8);
#endif
                        else
                            break;
                    }
                }

                // Ensure all checksums were found in log
                if (crc == string.Empty || md5 == string.Empty || sha1 == string.Empty)
                {
                    if (HashTool.GetStandardHashes(wud, out long wudSize, out string? wudCRC, out string? wudMD5, out string? wudSHA1))
                    {
                        crc = wudCRC ?? crc;
                        md5 = wudMD5 ?? md5;
                        sha1 = wudSHA1 ?? sha1;
                    }
                }

                // If no information could be found
                if (size == -1 && crc == string.Empty && md5 == string.Empty && sha1 == string.Empty)
                    return null;

                return new Datafile
                {
                    Game = [new Game() { Rom = [new Rom { Name = Path.GetFileName(wud), Size = size, CRC = crc, MD5 = md5, SHA1 = sha1 }] }]
                };
            }
            catch
            {
                // Absorb the exception
                return null;
            }
        }

        /// <summary>
        /// Get the disc ID. if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>Disc ID if possible, null on error</returns>
        internal static string? GetDiscID(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (string.IsNullOrEmpty(log))
                return null;
            if (!File.Exists(log))
                return null;

            // Setup all of the individual pieces
            string? discId = null;
            using (var sr = File.OpenText(log))
            {
                try
                {
                    var line = sr.ReadLine()?.Trim();
                    while (line is not null && !sr.EndOfStream)
                    {
                        if (line.StartsWith("Disc ID:"))
                        {
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
                            discId = line["Disc ID: ".Length..];
#else
                            discId = line.Substring("Disc ID: ".Length);
#endif
                            break;
                        }

                        line = sr.ReadLine()?.Trim();
                    }
                }
                catch { }
            }

            return discId;
        }

        /// <summary>
        /// Get the disc key. if possible
        /// </summary>
        /// <param name="key">Key file location</param>
        /// <returns>Disc key if possible, null on error</returns>
        internal static string? GetDiscKey(string key)
        {
            // If the file doesn't exist, we can't get info from it
            if (string.IsNullOrEmpty(key))
                return null;
            if (!File.Exists(key))
                return null;

            try
            {
                return ProcessingTool.GetFullFile(key, true);
            }
            catch
            {
                // Absorb the exception right now
                return null;
            }
        }

        /// <summary>
        /// Get the dump start datetime, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>Dump start datetime if possible, null on error</returns>
        internal static string? GetDumpStartDateTime(string log)
        {
            // If one of the files doesn't exist, we can't get info from them
            if (string.IsNullOrEmpty(log))
                return null;
            if (!File.Exists(log))
                return null;

            // Setup all of the individual pieces
            string? dumpStartDateTime = null;
            using (var sr = File.OpenText(log))
            {
                try
                {
                    var line = sr.ReadLine()?.Trim();
                    while (line is not null && !sr.EndOfStream)
                    {
                        if (line.StartsWith("Dump start datetime:"))
                        {
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
                            dumpStartDateTime = line["Dump start datetime: ".Length..];
#else
                            dumpStartDateTime = line.Substring("Dump start datetime: ".Length);
#endif
                            break;
                        }

                        line = sr.ReadLine()?.Trim();
                    }
                }
                catch { }
            }

            return dumpStartDateTime;
        }

        /// <summary>
        /// Get the version. if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>Version if possible, null on error</returns>
        internal static string? GetVersion(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (string.IsNullOrEmpty(log))
                return null;
            if (!File.Exists(log))
                return null;

            // Samples:
            // WUDD version: v1.2.3 2026_08_21_12_17

            // Setup all of the individual pieces
            string? wuddVersion = null;
            using (var sr = File.OpenText(log))
            {
                try
                {
                    var line = sr.ReadLine()?.Trim();
                    while (line is not null && !sr.EndOfStream)
                    {
                        if (line.StartsWith("WUDD version:"))
                        {
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
                            wuddVersion = line["WUDD version: ".Length..];
#else
                            wuddVersion = line.Substring("WUDD version: ".Length);
#endif
                            break;
                        }

                        line = sr.ReadLine()?.Trim();
                    }
                }
                catch { }
            }

            return wuddVersion;
        }

        #endregion
    }
}
