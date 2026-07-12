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
                    | OutputFileFlags.Zippable),

                // 44990-55279
                new($"{outputFilename}-44990-55279.hash", OutputFileFlags.Zippable),
                new($"{outputFilename}-44990-55279.scram", OutputFileFlags.Deleteable),
                new($"{outputFilename}-44990-55279.subq", OutputFileFlags.Zippable),

                // 55279-65568
                new($"{outputFilename}-55279-65568.hash", OutputFileFlags.Zippable),
                new($"{outputFilename}-55279-65568.scram", OutputFileFlags.Deleteable),
                new($"{outputFilename}-55279-65568.subq", OutputFileFlags.Zippable),

                // 65568-75857
                new($"{outputFilename}-65568-75857.hash", OutputFileFlags.Zippable),
                new($"{outputFilename}-65568-75857.scram", OutputFileFlags.Deleteable),
                new($"{outputFilename}-65568-75857.subq", OutputFileFlags.Zippable),

                // 75857-86146
                new($"{outputFilename}-75857-86146.hash", OutputFileFlags.Zippable),
                new($"{outputFilename}-75857-86146.scram", OutputFileFlags.Deleteable),
                new($"{outputFilename}-75857-86146.subq", OutputFileFlags.Zippable),

                // 86146-96435
                new($"{outputFilename}-86146-96435.hash", OutputFileFlags.Zippable),
                new($"{outputFilename}-86146-96435.scram", OutputFileFlags.Deleteable),
                new($"{outputFilename}-86146-96435.subq", OutputFileFlags.Zippable),

                // 96435-106724
                new($"{outputFilename}-96435-106724.hash", OutputFileFlags.Zippable),
                new($"{outputFilename}-96435-106724.scram", OutputFileFlags.Deleteable),
                new($"{outputFilename}-96435-106724.subq", OutputFileFlags.Zippable),

                // 106724-117013
                new($"{outputFilename}-106724-117013.hash", OutputFileFlags.Zippable),
                new($"{outputFilename}-106724-117013.scram", OutputFileFlags.Deleteable),
                new($"{outputFilename}-106724-117013.subq", OutputFileFlags.Zippable),

                // 117013-127302
                new($"{outputFilename}-117013-127302.hash", OutputFileFlags.Zippable),
                new($"{outputFilename}-117013-127302.scram", OutputFileFlags.Deleteable),
                new($"{outputFilename}-117013-127302.subq", OutputFileFlags.Zippable),

                // 127302-137591
                new($"{outputFilename}-127302-137591.hash", OutputFileFlags.Zippable),
                new($"{outputFilename}-127302-137591.scram", OutputFileFlags.Deleteable),
                new($"{outputFilename}-127302-137591.subq", OutputFileFlags.Zippable),

                // 137591-147880
                new($"{outputFilename}-137591-147880.hash", OutputFileFlags.Zippable),
                new($"{outputFilename}-137591-147880.scram", OutputFileFlags.Deleteable),
                new($"{outputFilename}-137591-147880.subq", OutputFileFlags.Zippable),

                // 147880-158169
                new($"{outputFilename}-147880-158169.hash", OutputFileFlags.Zippable),
                new($"{outputFilename}-147880-158169.scram", OutputFileFlags.Deleteable),
                new($"{outputFilename}-147880-158169.subq", OutputFileFlags.Zippable),

                // 158169-168458
                new($"{outputFilename}-158169-168458.hash", OutputFileFlags.Zippable),
                new($"{outputFilename}-158169-168458.scram", OutputFileFlags.Deleteable),
                new($"{outputFilename}-158169-168458.subq", OutputFileFlags.Zippable),

                // 168458-178747
                new($"{outputFilename}-168458-178747.hash", OutputFileFlags.Zippable),
                new($"{outputFilename}-168458-178747.scram", OutputFileFlags.Deleteable),
                new($"{outputFilename}-168458-178747.subq", OutputFileFlags.Zippable),

                // 178747-189036
                new($"{outputFilename}-178747-189036.hash", OutputFileFlags.Zippable),
                new($"{outputFilename}-178747-189036.scram", OutputFileFlags.Deleteable),
                new($"{outputFilename}-178747-189036.subq", OutputFileFlags.Zippable),

                // 189036-199325
                new($"{outputFilename}-189036-199325.hash", OutputFileFlags.Zippable),
                new($"{outputFilename}-189036-199325.scram", OutputFileFlags.Deleteable),
                new($"{outputFilename}-189036-199325.subq", OutputFileFlags.Zippable),

                // 199325-209614
                new($"{outputFilename}-199325-209614.hash", OutputFileFlags.Zippable),
                new($"{outputFilename}-199325-209614.scram", OutputFileFlags.Deleteable),
                new($"{outputFilename}-199325-209614.subq", OutputFileFlags.Zippable),

                // 209614-219903
                new($"{outputFilename}-209614-219903.hash", OutputFileFlags.Zippable),
                new($"{outputFilename}-209614-219903.scram", OutputFileFlags.Deleteable),
                new($"{outputFilename}-209614-219903.subq", OutputFileFlags.Zippable),

                // 219903-230192
                new($"{outputFilename}-219903-230192.hash", OutputFileFlags.Zippable),
                new($"{outputFilename}-219903-230192.scram", OutputFileFlags.Deleteable),
                new($"{outputFilename}-219903-230192.subq", OutputFileFlags.Zippable),

                // 230192-240481
                new($"{outputFilename}-230192-240481.hash", OutputFileFlags.Zippable),
                new($"{outputFilename}-230192-240481.scram", OutputFileFlags.Deleteable),
                new($"{outputFilename}-230192-240481.subq", OutputFileFlags.Zippable),

                // 240481-250770
                new($"{outputFilename}-240481-250770.hash", OutputFileFlags.Zippable),
                new($"{outputFilename}-240481-250770.scram", OutputFileFlags.Deleteable),
                new($"{outputFilename}-240481-250770.subq", OutputFileFlags.Zippable),

                // 250770-261059
                new($"{outputFilename}-250770-261059.hash", OutputFileFlags.Zippable),
                new($"{outputFilename}-250770-261059.scram", OutputFileFlags.Deleteable),
                new($"{outputFilename}-250770-261059.subq", OutputFileFlags.Zippable),

                // 261059-271348
                new($"{outputFilename}-261059-271348.hash", OutputFileFlags.Zippable),
                new($"{outputFilename}-261059-271348.scram", OutputFileFlags.Deleteable),
                new($"{outputFilename}-261059-271348.subq", OutputFileFlags.Zippable),

                // 271348-281637
                new($"{outputFilename}-271348-281637.hash", OutputFileFlags.Zippable),
                new($"{outputFilename}-271348-281637.scram", OutputFileFlags.Deleteable),
                new($"{outputFilename}-271348-281637.subq", OutputFileFlags.Zippable),

                // 281637-291926
                new($"{outputFilename}-281637-291926.hash", OutputFileFlags.Zippable),
                new($"{outputFilename}-281637-291926.scram", OutputFileFlags.Deleteable),
                new($"{outputFilename}-281637-291926.subq", OutputFileFlags.Zippable),

                // 291926-302215
                new($"{outputFilename}-291926-302215.hash", OutputFileFlags.Zippable),
                new($"{outputFilename}-291926-302215.scram", OutputFileFlags.Deleteable),
                new($"{outputFilename}-291926-302215.subq", OutputFileFlags.Zippable),

                // 302215-312504
                new($"{outputFilename}-302215-312504.hash", OutputFileFlags.Zippable),
                new($"{outputFilename}-302215-312504.scram", OutputFileFlags.Deleteable),
                new($"{outputFilename}-302215-312504.subq", OutputFileFlags.Zippable),

                // 312504-322793
                new($"{outputFilename}-312504-322793.hash", OutputFileFlags.Zippable),
                new($"{outputFilename}-312504-322793.scram", OutputFileFlags.Deleteable),
                new($"{outputFilename}-312504-322793.subq", OutputFileFlags.Zippable),

                // 322793-333082
                new($"{outputFilename}-322793-333082.hash", OutputFileFlags.Zippable),
                new($"{outputFilename}-322793-333082.scram", OutputFileFlags.Deleteable),
                new($"{outputFilename}-322793-333082.subq", OutputFileFlags.Zippable),

                // 333082-343371
                new($"{outputFilename}-333082-343371.hash", OutputFileFlags.Zippable),
                new($"{outputFilename}-333082-343371.scram", OutputFileFlags.Deleteable),
                new($"{outputFilename}-333082-343371.subq", OutputFileFlags.Zippable),

                // 343371-353660
                new($"{outputFilename}-343371-353660.hash", OutputFileFlags.Zippable),
                new($"{outputFilename}-343371-353660.scram", OutputFileFlags.Deleteable),
                new($"{outputFilename}-343371-353660.subq", OutputFileFlags.Zippable),

                // 353660-363949
                new($"{outputFilename}-353660-363949.hash", OutputFileFlags.Zippable),
                new($"{outputFilename}-353660-363949.scram", OutputFileFlags.Deleteable),
                new($"{outputFilename}-353660-363949.subq", OutputFileFlags.Zippable),

                // 363949-374238
                new($"{outputFilename}-363949-374238.hash", OutputFileFlags.Zippable),
                new($"{outputFilename}-363949-374238.scram", OutputFileFlags.Deleteable),
                new($"{outputFilename}-363949-374238.subq", OutputFileFlags.Zippable),

                // 374238-384527
                new($"{outputFilename}-374238-384527.hash", OutputFileFlags.Zippable),
                new($"{outputFilename}-374238-384527.scram", OutputFileFlags.Deleteable),
                new($"{outputFilename}-374238-384527.subq", OutputFileFlags.Zippable),

                // 384527-394816
                new($"{outputFilename}-384527-394816.hash", OutputFileFlags.Zippable),
                new($"{outputFilename}-384527-394816.scram", OutputFileFlags.Deleteable),
                new($"{outputFilename}-384527-394816.subq", OutputFileFlags.Zippable),

                // 394816-405105
                new($"{outputFilename}-394816-405105.hash", OutputFileFlags.Zippable),
                new($"{outputFilename}-394816-405105.scram", OutputFileFlags.Deleteable),
                new($"{outputFilename}-394816-405105.subq", OutputFileFlags.Zippable),

                // 405105-415394
                new($"{outputFilename}-405105-415394.hash", OutputFileFlags.Zippable),
                new($"{outputFilename}-405105-415394.scram", OutputFileFlags.Deleteable),
                new($"{outputFilename}-405105-415394.subq", OutputFileFlags.Zippable),

                // 415394-425683
                new($"{outputFilename}-415394-425683.hash", OutputFileFlags.Zippable),
                new($"{outputFilename}-415394-425683.scram", OutputFileFlags.Deleteable),
                new($"{outputFilename}-415394-425683.subq", OutputFileFlags.Zippable),

                // 425683-435972
                new($"{outputFilename}-425683-435972.hash", OutputFileFlags.Zippable),
                new($"{outputFilename}-425683-435972.scram", OutputFileFlags.Deleteable),
                new($"{outputFilename}-425683-435972.subq", OutputFileFlags.Zippable),

                // 435972-446261
                new($"{outputFilename}-435972-446261.hash", OutputFileFlags.Zippable),
                new($"{outputFilename}-435972-446261.scram", OutputFileFlags.Deleteable),
                new($"{outputFilename}-435972-446261.subq", OutputFileFlags.Zippable),

                // 446261-549152
                new($"{outputFilename}-446261-549152.hash", OutputFileFlags.Zippable),
                new($"{outputFilename}-446261-549152.scram", OutputFileFlags.Deleteable),
                new($"{outputFilename}-446261-549152.subq", OutputFileFlags.Zippable),
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
