using System;
using System.Collections.Generic;
using System.IO;
#if NET452_OR_GREATER || NETCOREAPP
using System.IO.Compression;
#endif
using System.Linq;
using System.Text.RegularExpressions;
using SabreTools.RedumpLib.Data;

namespace MPF.Processors
{
    public abstract class BaseProcessor
    {
        /// <summary>
        /// All found volume labels and their corresponding file systems
        /// </summary>
        public Dictionary<string, List<string>>? VolumeLabels;

        #region Metadata

        /// <summary>
        /// Currently represented system
        /// </summary>
        public RedumpSystem? System { get; private set; }

        /// <summary>
        /// Currently represented media type
        /// </summary>
        public MediaType? Type { get; private set; }

        #endregion

        /// <summary>
        /// Generate processor for a system and media type combination
        /// </summary>
        /// <param name="system">RedumpSystem value to use</param>
        /// <param name="type">MediaType value to use</param>
        public BaseProcessor(RedumpSystem? system, MediaType? type)
        {
            System = system;
            Type = type;
        }

        #region Abstract Methods

        /// <summary>
        /// Validate if all required output files exist
        /// </summary>
        /// <param name="basePath">Base filename and path to use for checking</param>
        /// <param name="preCheck">True if this is a check done before a dump, false if done after</param>
        /// <returns>Tuple of true if all required files exist, false otherwise and a list representing missing files</returns>
        public abstract (bool, List<string>) CheckAllOutputFilesExist(string basePath, bool preCheck);

        /// <summary>
        /// Generate artifacts and add to the SubmissionInfo
        /// </summary>
        /// <param name="submissionInfo">Base submission info to fill in specifics for</param>
        /// <param name="basePath">Base filename and path to use for checking</param>
        public abstract void GenerateArtifacts(SubmissionInfo submissionInfo, string basePath);

        /// <summary>
        /// Generate a SubmissionInfo for the output files
        /// </summary>
        /// <param name="submissionInfo">Base submission info to fill in specifics for</param>
        /// <param name="basePath">Base filename and path to use for checking</param>
        /// <param name="redumpCompat">Determines if outputs are processed according to Redump specifications</param>
        public abstract void GenerateSubmissionInfo(SubmissionInfo submissionInfo, string basePath, bool redumpCompat);

        /// <summary>
        /// Generate a list of all log files generated
        /// </summary>
        /// <param name="basePath">Base filename and path to use for checking</param>
        /// <returns>List of all log file paths, empty otherwise</returns>
        public abstract List<string> GetLogFilePaths(string basePath);

        /// <summary>
        /// Generate a list of all output files generated
        /// </summary>
        /// <param name="baseFilename">Base filename to use for checking</param>
        /// <returns>List of all output files, empty otherwise</returns>
        public abstract List<OutputFile> GetOutputFiles(string baseFilename);

        #endregion

        #region Virtual Methods

        /// <summary>
        /// Generate a list of all deleteable files generated
        /// </summary>
        /// <param name="basePath">Base filename and path to use for checking</param>
        /// <returns>List of all deleteable file paths, empty otherwise</returns>
        public virtual List<string> GetDeleteableFilePaths(string basePath) => [];

        #endregion

        #region Shared Methods

        /// <summary>
        /// Compress log files to save space
        /// </summary>
        /// <param name="outputDirectory">Output folder to write to</param>
        /// <param name="filenameSuffix">Output filename to use as the base path</param>
        /// <param name="outputFilename">Output filename to use as the base path</param>
        /// <param name="processor">Processor object representing how to process the outputs</param>
        /// <returns>True if the process succeeded, false otherwise</returns>
        public (bool, string) CompressLogFiles(string? outputDirectory, string? filenameSuffix, string outputFilename)
        {
#if NET20 || NET35 || NET40
            return (false, "Log compression is not available for this framework version");
#else

            // Prepare the necessary paths
            outputFilename = Path.GetFileNameWithoutExtension(outputFilename);
            string combinedBase;
            if (string.IsNullOrEmpty(outputDirectory))
                combinedBase = outputFilename;
            else
                combinedBase = Path.Combine(outputDirectory, outputFilename);

            string archiveName = combinedBase + "_logs.zip";

            // Get the list of log files from the parameters object
            var files = GetLogFilePaths(combinedBase);

            // Add on generated log files if they exist
            var mpfFiles = GetGeneratedFilePaths(outputDirectory, filenameSuffix);
            files.AddRange(mpfFiles);

            if (!files.Any())
                return (true, "No files to compress!");

            // If the file already exists, we want to delete the old one
            try
            {
                if (File.Exists(archiveName))
                    File.Delete(archiveName);
            }
            catch
            {
                return (false, "Could not delete old archive!");
            }

            // Add the log files to the archive and delete the uncompressed file after
            ZipArchive? zf = null;
            try
            {
                zf = ZipFile.Open(archiveName, ZipArchiveMode.Create);
                foreach (string file in files)
                {
                    if (string.IsNullOrEmpty(outputDirectory))
                    {
                        zf.CreateEntryFromFile(file, file, CompressionLevel.Optimal);
                    }
                    else
                    {
                        string entryName = file[outputDirectory!.Length..].TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

#if NETFRAMEWORK || NETCOREAPP3_1 || NET5_0
                        zf.CreateEntryFromFile(file, entryName, CompressionLevel.Optimal);
#else
                        zf.CreateEntryFromFile(file, entryName, CompressionLevel.SmallestSize);
#endif
                    }

                    // If the file is MPF-specific, don't delete
                    if (mpfFiles.Contains(file))
                        continue;

                    try
                    {
                        File.Delete(file);
                    }
                    catch { }
                }

                return (true, "Compression complete!");
            }
            catch (Exception ex)
            {
                return (false, $"Compression could not complete: {ex}");
            }
            finally
            {
                zf?.Dispose();
            }
#endif
        }

        /// <summary>
        /// Compress log files to save space
        /// </summary>
        /// <param name="outputDirectory">Output folder to write to</param>
        /// <param name="outputFilename">Output filename to use as the base path</param>
        /// <param name="processor">Processor object representing how to process the outputs</param>
        /// <returns>True if the process succeeded, false otherwise</returns>
        public (bool, string) DeleteUnnecessaryFiles(string? outputDirectory, string outputFilename)
        {
            // Prepare the necessary paths
            outputFilename = Path.GetFileNameWithoutExtension(outputFilename);
            string combinedBase;
            if (string.IsNullOrEmpty(outputDirectory))
                combinedBase = outputFilename;
            else
                combinedBase = Path.Combine(outputDirectory, outputFilename);

            // Get the list of deleteable files from the parameters object
            var files = GetDeleteableFilePaths(combinedBase);

            if (!files.Any())
                return (true, "No files to delete!");

            // Attempt to delete all of the files
            try
            {
                foreach (string file in files)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch { }
                }

                return (true, "Deletion complete!");
            }
            catch (Exception ex)
            {
                return (false, $"Deletion could not complete: {ex}");
            }
        }

        /// <summary>
        /// Ensures that all required output files have been created
        /// </summary>
        /// <param name="outputDirectory">Output folder to write to</param>
        /// <param name="outputFilename">Output filename to use as the base path</param>
        /// <param name="processor">Processor object representing how to process the outputs</param>
        /// <param name="preCheck">True if this is a check done before a dump, false if done after</param>
        /// <returns>Tuple of true if all required files exist, false otherwise and a list representing missing files</returns>
        public (bool, List<string>) FoundAllFiles(string? outputDirectory, string outputFilename, bool preCheck)
        {
            // First, sanitized the output filename to strip off any potential extension
            outputFilename = Path.GetFileNameWithoutExtension(outputFilename);

            // Then get the base path for all checking
            string basePath;
            if (string.IsNullOrEmpty(outputDirectory))
                basePath = outputFilename;
            else
                basePath = Path.Combine(outputDirectory, outputFilename);

            // Finally, let the parameters say if all files exist
            return CheckAllOutputFilesExist(basePath, preCheck);
        }

        /// <summary>
        /// Get the hex contents of the PIC file
        /// </summary>
        /// <param name="picPath">Path to the PIC.bin file associated with the dump</param>
        /// <param name="trimLength">Number of characters to trim the PIC to, if -1, ignored</param>
        /// <returns>PIC data as a hex string if possible, null on error</returns>
        /// <remarks>https://stackoverflow.com/questions/9932096/add-separator-to-string-at-every-n-characters</remarks>
        protected static string? GetPIC(string picPath, int trimLength = -1)
        {
            // If the file doesn't exist, we can't get the info
            if (!File.Exists(picPath))
                return null;

            try
            {
                var hex = ProcessingTool.GetFullFile(picPath, true);
                if (hex == null)
                    return null;

                if (trimLength > -1)
                    hex = hex.Substring(0, trimLength);

                // TODO: Check for non-zero values in discarded PIC

                return Regex.Replace(hex, ".{32}", "$0\n", RegexOptions.Compiled);
            }
            catch
            {
                // We don't care what the error was right now
                return null;
            }
        }

        /// <summary>
        /// Get a isobuster-formatted PVD from a 2048 byte-per-sector image, if possible
        /// </summary>
        /// <param name="isoPath">Path to ISO file</param>
        /// <param name="pvd">Formatted PVD string, otherwise null</param>
        /// <returns>True if PVD was successfully parsed, otherwise false</returns>
        protected static bool GetPVD(string isoPath, out string? pvd)
        {
            pvd = null;
            try
            {
                // Get PVD bytes from ISO file
                var buf = new byte[96];
                using (FileStream iso = File.OpenRead(isoPath))
                {
                    // TODO: Don't hardcode 0x8320
                    iso.Seek(0x8320, SeekOrigin.Begin);

                    int offset = 0;
                    while (offset < 96)
                    {
                        int read = iso.Read(buf, offset, buf.Length - offset);
                        if (read == 0)
                            throw new EndOfStreamException();
                        offset += read;
                    }
                }

                // Format PVD to isobuster standard
                char[] pvdCharArray = new char[96];
                for (int i = 0; i < 96; i++)
                {
                    if (buf[i] >= 0x20 && buf[i] <= 0x7E)
                        pvdCharArray[i] = (char)buf[i];
                    else
                        pvdCharArray[i] = '.';
                }
                string pvdASCII = new string(pvdCharArray, 0, 96);
                pvd = string.Empty;
                for (int i = 0; i < 96; i += 16)
                {
                    pvd += $"{(0x0320 + i):X4} : {buf[i]:X2} {buf[i + 1]:X2} {buf[i + 2]:X2} {buf[i + 3]:X2} {buf[i + 4]:X2} {buf[i + 5]:X2} {buf[i + 6]:X2} {buf[i + 7]:X2}  " +
                        $"{buf[i + 8]:X2} {buf[i + 9]:X2} {buf[i + 10]:X2} {buf[i + 11]:X2} {buf[i + 12]:X2} {buf[i + 13]:X2} {buf[i + 14]:X2} {buf[i + 15]:X2}   {pvdASCII.Substring(i, 16)}\n";
                }

                return true;
            }
            catch
            {
                // We don't care what the error is
                return false;
            }
        }

        /// <summary>
        /// Generate a list of all MPF-specific log files generated
        /// </summary>
        /// <param name="outputDirectory">Output folder to write to</param>
        /// <param name="filenameSuffix">Optional suffix to append to the filename</param>
        /// <returns>List of all log file paths, empty otherwise</returns>
        private static List<string> GetGeneratedFilePaths(string? outputDirectory, string? filenameSuffix)
        {
            var files = new List<string>();

            if (string.IsNullOrEmpty(outputDirectory) && string.IsNullOrEmpty(filenameSuffix))
            {
                if (File.Exists("!submissionInfo.txt"))
                    files.Add("!submissionInfo.txt");
                if (File.Exists("!submissionInfo.json"))
                    files.Add("!submissionInfo.json");
                if (File.Exists("!submissionInfo.json.gz"))
                    files.Add("!submissionInfo.json.gz");
                if (File.Exists("!protectionInfo.txt"))
                    files.Add("!protectionInfo.txt");
            }
            else if (string.IsNullOrEmpty(outputDirectory) && !string.IsNullOrEmpty(filenameSuffix))
            {
                if (File.Exists($"!submissionInfo_{filenameSuffix}.txt"))
                    files.Add($"!submissionInfo_{filenameSuffix}.txt");
                if (File.Exists($"!submissionInfo_{filenameSuffix}.json"))
                    files.Add($"!submissionInfo_{filenameSuffix}.json");
                if (File.Exists($"!submissionInfo_{filenameSuffix}.json.gz"))
                    files.Add($"!submissionInfo_{filenameSuffix}.json.gz");
                if (File.Exists($"!protectionInfo_{filenameSuffix}.txt"))
                    files.Add($"!protectionInfo_{filenameSuffix}.txt");
            }
            else if (!string.IsNullOrEmpty(outputDirectory) && string.IsNullOrEmpty(filenameSuffix))
            {
                if (File.Exists(Path.Combine(outputDirectory, "!submissionInfo.txt")))
                    files.Add(Path.Combine(outputDirectory, "!submissionInfo.txt"));
                if (File.Exists(Path.Combine(outputDirectory, "!submissionInfo.json")))
                    files.Add(Path.Combine(outputDirectory, "!submissionInfo.json"));
                if (File.Exists(Path.Combine(outputDirectory, "!submissionInfo.json.gz")))
                    files.Add(Path.Combine(outputDirectory, "!submissionInfo.json.gz"));
                if (File.Exists(Path.Combine(outputDirectory, "!protectionInfo.txt")))
                    files.Add(Path.Combine(outputDirectory, "!protectionInfo.txt"));
            }
            else if (!string.IsNullOrEmpty(outputDirectory) && !string.IsNullOrEmpty(filenameSuffix))
            {
                if (File.Exists(Path.Combine(outputDirectory, $"!submissionInfo_{filenameSuffix}.txt")))
                    files.Add(Path.Combine(outputDirectory, $"!submissionInfo_{filenameSuffix}.txt"));
                if (File.Exists(Path.Combine(outputDirectory, $"!submissionInfo_{filenameSuffix}.json")))
                    files.Add(Path.Combine(outputDirectory, $"!submissionInfo_{filenameSuffix}.json"));
                if (File.Exists(Path.Combine(outputDirectory, $"!submissionInfo_{filenameSuffix}.json.gz")))
                    files.Add(Path.Combine(outputDirectory, $"!submissionInfo_{filenameSuffix}.json.gz"));
                if (File.Exists(Path.Combine(outputDirectory, $"!protectionInfo_{filenameSuffix}.txt")))
                    files.Add(Path.Combine(outputDirectory, $"!protectionInfo_{filenameSuffix}.txt"));
            }

            return files;
        }

        #endregion
    }
}
