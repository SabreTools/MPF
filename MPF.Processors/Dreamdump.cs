using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using MPF.Processors.OutputFiles;
using SabreTools.RedumpLib.Data;
#if NET462_OR_GREATER || NETCOREAPP
using SharpCompress.Archives.Zip;
#endif

namespace MPF.Processors
{
    /// <summary>
    /// Represents processing Dreamdump outputs
    /// </summary>
    public sealed class Dreamdump : BaseProcessor
    {
        /// <inheritdoc/>
        public Dreamdump(PhysicalSystem? system) : base(system) { }

        #region BaseProcessor Implementations

        /// <inheritdoc/>
        public override PhysicalMediaType? DeterminePhysicalMediaType(string? outputDirectory, string outputFilename)
            => PhysicalMediaType.GDROM;

        /// <inheritdoc/>
        public override void GenerateSubmissionInfo(SubmissionInfo info, PhysicalMediaType? mediaType, string basePath, bool redumpCompat)
        {
            info.DumpMetadata.Comments = string.Empty;

            // Get the dumping program and version
            info.DumpingInfo.DumpingProgram ??= string.Empty;
            info.DumpingInfo.DumpingProgram += $" {GetVersion($"{basePath}.log") ?? "Unknown Version"}";
            info.DumpingInfo.DumpingParameters = GetParameters($"{basePath}.log") ?? "Unknown Parameters";
            info.DumpingInfo.DumpingDate = ProcessingTool.GetFileModifiedDate($"{basePath}.log")?.ToString("yyyy-MM-dd HH:mm:ss");

            // Fill in the hardware data
            if (GetHardwareInfo($"{basePath}.log", out var manufacturer, out var model, out var firmware))
            {
                info.DumpingInfo.Manufacturer = manufacturer;
                info.DumpingInfo.Model = model;
                info.DumpingInfo.Firmware = firmware;
            }

            // Get the Datafile information
            info.DumpMetadata.Dat = GetDatfile($"{basePath}.log");

            // Get the write offset, if it exists
            string? writeOffset = GetWriteOffset($"{basePath}.log");
            info.RingCodes.WriteOffset = writeOffset;

            // Read the cuesheet from the log, falling back to the external file
            info.DumpMetadata.Cuesheet = GetCuesheet($"{basePath}.log")
                ?? ProcessingTool.GetFullFile($"{basePath}.cue")
                ?? string.Empty;

            // Read the GD-ROM header values
            info.DumpMetadata.Header = GetGDROMHeader($"{basePath}.log",
                                        out string? buildDate,
                                        out string? serial,
                                        out _,
                                        out string? version) ?? string.Empty;
            info.DumpMetadata.CommentsSpecialFields[SiteCode.InternalSerialName] = serial ?? string.Empty;
            info.DiscIdentifiers.EXEDate = buildDate ?? string.Empty;
            // TODO: Support region setting from parsed value
            info.DiscIdentifiers.Version = version ?? string.Empty;
        }

        /// <inheritdoc/>
        /// TODO: Figure out how to handle individual section hash, scram, and subq files
        internal override List<OutputFile> GetOutputFiles(PhysicalMediaType? mediaType, string? outputDirectory, string outputFilename)
        {
            // Remove the extension by default
            outputFilename = Path.GetFileNameWithoutExtension(outputFilename);

            // Assemble the base path
            string basePath = Path.GetFileNameWithoutExtension(outputFilename);
            if (!string.IsNullOrEmpty(outputDirectory))
                basePath = Path.Combine(outputDirectory, basePath);

            return [
                new($"{outputFilename}.cue", OutputFileFlags.Required
                    | OutputFileFlags.Preserve),
                new($"{outputFilename}.gdi", OutputFileFlags.Required
                    | OutputFileFlags.Preserve),
                new($"{outputFilename}.log", OutputFileFlags.Required
                    | OutputFileFlags.Artifact
                    | OutputFileFlags.Zippable
                    | OutputFileFlags.Preserve,
                    "log"),
                new CustomOutputFile([$"{outputFilename}.dat", $"{outputFilename}.log"], OutputFileFlags.Required,
                    DatfileExists),
                new([$"{outputFilename}.scram", $"{outputFilename}.scrap"], OutputFileFlags.Deleteable),
                new($"{outputFilename}.subq", OutputFileFlags.Binary
                    | OutputFileFlags.Zippable,
                    "subq"),
            ];
        }

        #endregion

        #region Private Extra Methods

        /// <summary>
        /// Get if the datfile exists in the log
        /// </summary>
        /// <param name="log">Log file location</param>
        private static bool DatfileExists(string log)
        {
            // Uncompressed outputs
            if (GetDatfile(log) is not null)
                return true;

            // Check for the log file
            string outputFilename = Path.GetFileName(log);
            string? outputDirectory = Path.GetDirectoryName(log);
            string basePath = Path.GetFileNameWithoutExtension(outputFilename);
            if (!string.IsNullOrEmpty(outputDirectory))
                basePath = Path.Combine(outputDirectory, basePath);

#if NET20 || NET35 || NET40 || NET452
            // Assume the zipfile has the file in it
            return File.Exists($"{basePath}_logs.zip");
#else
            // If the zipfile doesn't exist
            if (!File.Exists($"{basePath}_logs.zip"))
                return false;

            try
            {
                // Try to open the archive
                using ZipArchive archive = (ZipArchive)ZipArchive.OpenArchive($"{basePath}_logs.zip");

                // Get the log entry and check it, if possible
                ZipArchiveEntry? logEntry = null;
                foreach (var entry in archive.Entries)
                {
                    if (entry.Key == outputFilename)
                    {
                        logEntry = entry;
                        break;
                    }
                }

                if (logEntry is null)
                    return false;

                using var sr = new StreamReader(logEntry.OpenEntryStream());
                return GetDatfile(sr) is not null;
            }
            catch
            {
                return false;
            }
#endif
        }

        #endregion

        #region Information Extraction Methods

        /// <summary>
        /// Get the cuesheet from the input file, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>Newline-delimited cuesheet if possible, null on error</returns>
        internal static string? GetCuesheet(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (string.IsNullOrEmpty(log))
                return null;
            if (!File.Exists(log))
                return null;

            try
            {
                // Fast forward to the cuesheet line
                using var sr = File.OpenText(log);
                while (!sr.EndOfStream && sr.ReadLine()?.TrimStart()?.StartsWith("CUE [") == false) ;
                if (sr.EndOfStream)
                    return null;

                // Now that we're at the relevant entries, read each line in and concatenate
                var sb = new StringBuilder();
                string? line = sr.ReadLine();
                while (!string.IsNullOrEmpty(line?.Trim()))
                {
                    // TODO: Figure out how to use NormalizeShiftJIS here
                    sb.AppendLine(line?.TrimEnd());
                    line = sr.ReadLine();
                }

                return sb.ToString().TrimEnd('\r', '\n');
            }
            catch
            {
                // Absorb the exception
                return null;
            }
        }

        /// <summary>
        /// Get the datfile from the input file, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>Newline-delimited datfile if possible, null on error</returns>
        internal static string? GetDatfile(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (!File.Exists(log))
                return null;

            try
            {
                using var sr = File.OpenText(log);
                return GetDatfile(sr);
            }
            catch
            {
                // Absorb the exception
                return null;
            }
        }

        /// <summary>
        /// Get the datfile from the input file, if possible
        /// </summary>
        /// <param name="sr">StreamReader representing the input file</param>
        /// <returns>Newline-delimited datfile if possible, null on error</returns>
        internal static string? GetDatfile(StreamReader sr)
        {
            try
            {
                string? datString = null;

                // Find all occurrences of the hash information
                while (!sr.EndOfStream)
                {
                    // Fast forward to the dat line
                    while (!sr.EndOfStream && sr.ReadLine()?.TrimStart()?.StartsWith("dat:") == false) ;
                    if (sr.EndOfStream)
                        break;

                    // Now that we're at the relevant entries, read each line in and concatenate
                    datString = string.Empty;
                    var line = sr.ReadLine()?.Trim();
                    while (line?.StartsWith("<rom") == true)
                    {
                        datString += line + "\n";
                        if (sr.EndOfStream)
                            break;

                        line = sr.ReadLine()?.Trim();
                    }
                }

                return datString?.TrimEnd('\n');
            }
            catch
            {
                // Absorb the exception
                return null;
            }
        }

        /// <summary>
        /// Get the header from a GD-ROM LD area, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>Header as a string if possible, null on error</returns>
        internal static string? GetGDROMHeader(string log, out string? buildDate, out string? serial, out string? region, out string? version)
        {
            // Set the default values
            buildDate = null; serial = null; region = null; version = null;

            // If the file doesn't exist, we can't get info from it
            if (string.IsNullOrEmpty(log))
                return null;
            if (!File.Exists(log))
                return null;

            try
            {
                // Fast forward to the DC header line
                using var sr = File.OpenText(log);
                while (!sr.EndOfStream && sr.ReadLine()?.TrimStart()?.StartsWith("DC Header") == false) ;
                if (sr.EndOfStream)
                    return null;

                string? line, headerString = string.Empty;
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine()?.TrimStart();
                    if (line is null)
                        break;

                    if (line.StartsWith("build date:"))
                    {
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
                        buildDate = line["build date: ".Length..].Trim();
#else
                        buildDate = line.Substring("build date: ".Length).Trim();
#endif
                    }
                    else if (line.StartsWith("version:"))
                    {
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
                        version = line["version: ".Length..].Trim();
#else
                        version = line.Substring("version: ".Length).Trim();
#endif
                    }
                    else if (line.StartsWith("serial:"))
                    {
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
                        serial = line["serial: ".Length..].Trim();
#else
                        serial = line.Substring("serial: ".Length).Trim();
#endif
                    }
                    else if (line.StartsWith("region:"))
                    {
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
                        region = line["region: ".Length..].Trim();
#else
                        region = line.Substring("region: ".Length).Trim();
#endif
                    }
                    else if (line.StartsWith("regions:"))
                    {
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
                        region = line["regions: ".Length..].Trim();
#else
                        region = line.Substring("regions: ".Length).Trim();
#endif
                    }
                    else if (line.StartsWith("header:"))
                    {
                        line = sr.ReadLine()?.TrimStart();
                        while (line?.StartsWith("00") == true)
                        {
                            headerString += line + "\n";
                            line = sr.ReadLine()?.TrimStart();
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                return headerString.TrimEnd('\n');
            }
            catch
            {
                // Absorb the exception
                return null;
            }
        }

        /// <summary>
        /// Get hardware information from the input file, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>True if hardware info was set, false otherwise</returns>
        internal static bool GetHardwareInfo(string log, out string? manufacturer, out string? model, out string? firmware)
        {
            // Set the default values
            manufacturer = null; model = null; firmware = null;

            // If the file doesn't exist, we can't get info from it
            if (string.IsNullOrEmpty(log))
                return false;
            if (!File.Exists(log))
                return false;

            // Firmware can be split into different lines
            List<string> firmwarePieces = [];

            try
            {
                // If we find the hardware info line, return each value
                // drive: <vendor_id> - <product_id> (revision level: <product_revision_level>, vendor specific: <vendor_specific>)
                var regex = new Regex(@"Drive: (.+) - (.+) \(revision level: (.+), vendor specific: (.+)\)", RegexOptions.Compiled);

                // Fast forward to the drive information section
                using var sr = File.OpenText(log);
                do
                {
                    // Read the next line until EOF
                    string? line = sr.ReadLine()?.Trim();
                    if (line is null)
                        break;

                    // Check old version of line
                    var match = regex.Match(line);
                    if (match.Success)
                    {
                        manufacturer = match.Groups[1].Value;
                        model = match.Groups[2].Value;

                        string revisionLevel = match.Groups[3].Value;
                        revisionLevel += match.Groups[4].Value == "<empty>" ? "" : $" ({match.Groups[4].Value})";
                        firmwarePieces.Add(revisionLevel);
                    }

                    if (string.IsNullOrEmpty(line))
                    {
                        // An empty line indicates the end of the section
                        break;
                    }
                } while (true);

                // Assemble the firmware string, if needed
                firmware = string.Join(", ", [.. firmwarePieces]);

                // Return if the fields are filled
                return manufacturer is not null && model is not null && firmware is not null;
            }
            catch
            {
                // Absorb the exception
                return false;
            }
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
            // dreamdump (build: 0.2.0)

            try
            {
                // Skip first line (dump date)
                using var sr = File.OpenText(log);
                sr.ReadLine();

                // Get the next non-warning line
                string nextLine = sr.ReadLine()?.Trim() ?? string.Empty;
                if (nextLine.StartsWith("warning:", StringComparison.OrdinalIgnoreCase))
                    nextLine = sr.ReadLine()?.Trim() ?? string.Empty;

                // Generate regex
                var regex = new Regex(@"^dreamdump \((.+)\)", RegexOptions.Compiled);

                // Extract the version string
                var match = regex.Match(nextLine);
                var version = match.Groups[1].Value;
                return string.IsNullOrEmpty(version) ? null : version;
            }
            catch
            {
                // Absorb the exception
                return null;
            }
        }

        /// <summary>
        /// Get the dumping parameters, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>Dumping parameters if possible, null on error</returns>
        internal static string? GetParameters(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (string.IsNullOrEmpty(log))
                return null;
            if (!File.Exists(log))
                return null;

            // Samples:
            // arguments: disc --image-path=DC --image-name=DISCNAME --drive=/dev/sg9

            try
            {
                // If we find the arguments line, return the arguments
                using var sr = File.OpenText(log);
                while (!sr.EndOfStream)
                {
                    string? line = sr.ReadLine()?.TrimStart();
                    if (line?.StartsWith("arguments: ") == true)
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
                        return line["arguments: ".Length..].Trim();
#else
                        return line.Substring("arguments: ".Length).Trim();
#endif
                }

                // Required lines were not found
                return null;
            }
            catch
            {
                // Absorb the exception
                return null;
            }
        }

        /// <summary>
        /// Get the write offset from the input file, if possible
        /// </summary>
        /// <param name="log">Log file location</param>
        /// <returns>Sample write offset if possible, null on error</returns>
        internal static string? GetWriteOffset(string log)
        {
            // If the file doesn't exist, we can't get info from it
            if (string.IsNullOrEmpty(log))
                return null;
            if (!File.Exists(log))
                return null;

            try
            {
                // If we find the disc write offset line, return the offset
                using var sr = File.OpenText(log);
                while (!sr.EndOfStream)
                {
                    string? line = sr.ReadLine()?.TrimStart();
                    if (line?.StartsWith("Write Offset:") == true)
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
                        return line["Write Offset: ".Length..].Trim();
#else
                        return line.Substring("Write Offset: ".Length).Trim();
#endif
                }

                // Required lines were not found
                return null;
            }
            catch
            {
                // Absorb the exception
                return null;
            }
        }

        #endregion
    }
}
