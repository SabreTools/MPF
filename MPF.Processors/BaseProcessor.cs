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
        /// Generate a SubmissionInfo for the output files
        /// </summary>
        /// <param name="submissionInfo">Base submission info to fill in specifics for</param>
        /// <param name="basePath">Base filename and path to use for checking</param>
        /// <param name="redumpCompat">Determines if outputs are processed according to Redump specifications</param>
        public abstract void GenerateSubmissionInfo(SubmissionInfo submissionInfo, string basePath, bool redumpCompat);

        // <summary>
        /// Generate a list of all output files generated
        /// </summary>
        /// <param name="baseFilename">Base filename to use for checking</param>
        /// <returns>List of all output files, empty otherwise</returns>
        internal abstract List<OutputFile> GetOutputFiles(string baseFilename);

        #endregion

        #region Output Files

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

            // Generate the archive filename
            string archiveName = $"{combinedBase}_logs.zip";

            // Get the lists of zippable files
            var zippableFiles = GetZippableFilePaths(combinedBase);
            var generatedFiles = GetGeneratedFilePaths(outputDirectory, filenameSuffix);

            // Don't create an archive if there are no paths
            if (!zippableFiles.Any() && !generatedFiles.Any())
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

                _ = AddToArchive(zf, zippableFiles, outputDirectory, true);
                _ = AddToArchive(zf, generatedFiles, outputDirectory, false);

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

        /// <summary>
        /// Ensures that all required output files have been created
        /// </summary>
        /// <param name="outputDirectory">Output folder to write to</param>
        /// <param name="outputFilename">Output filename to use as the base path</param>
        /// <param name="processor">Processor object representing how to process the outputs</param>
        /// <returns>Tuple of true if all required files exist, false otherwise and a list representing missing files</returns>
        public (bool, List<string>) FoundAllFiles(string? outputDirectory, string outputFilename)
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
            return CheckRequiredFiles(basePath);
        }

        /// <summary>
        /// Generate artifacts and return them as a dictionary
        /// </summary>
        /// <param name="basePath">Base filename and path to use for checking</param>
        /// <returns>Dictiionary of artifact keys to Base64-encoded values, if possible</param>
        public Dictionary<string, string> GenerateArtifacts(string basePath)
        {
            // Get the list of output files
            var outputFiles = GetOutputFiles(basePath);
            if (outputFiles.Count == 0)
                return [];

            // Create the artifacts dictionary
            var artifacts = new Dictionary<string, string>();

            // Only try to create artifacts for files that exist
            foreach (var outputFile in outputFiles)
            {
                // Skip non-artifact files
                if (!outputFile.IsArtifact || outputFile.ArtifactKey == null)
                    continue;

                // Skip non-existent files
                foreach (string filename in outputFile.Filenames)
                {
                    if (!File.Exists(filename))
                        continue;

                    // Get binary artifacts as a byte array
                    if (outputFile.IsBinaryArtifact)
                    {
                        byte[] data = File.ReadAllBytes(filename);
                        string str = Convert.ToBase64String(data);
                        artifacts.Add(outputFile.ArtifactKey, str);
                    }
                    else
                    {
                        string? data = ProcessingTool.GetFullFile(filename);
                        string str = ProcessingTool.GetBase64(data) ?? string.Empty;
                        artifacts.Add(outputFile.ArtifactKey, str);
                    }

                    break;
                }
            }

            return artifacts;
        }

#if NET452_OR_GREATER || NETCOREAPP
        /// <summary>
        /// Try to add a set of files to an existing archive
        /// </summary>
        /// <param name="archive">Archive to add the file to</param>
        /// <param name="files">Full path to a set of existing files</param>
        /// <param name="outputDirectory">Directory that the existing files live in</param>
        /// <param name="delete">Indicates if the files should be deleted after adding</param>
        /// <returns>True if all files were added successfully, false otherwise</returns>
        private static bool AddToArchive(ZipArchive archive, List<string> files, string? outputDirectory, bool delete)
        {
            // An empty list means success
            if (files.Count == 0)
                return true;

            // Loop through and add all files
            bool allAdded = true;
            foreach (string file in files)
            {
                allAdded &= AddToArchive(archive, file, outputDirectory, delete);
            }

            return allAdded;
        }

        /// <summary>
        /// Try to add a file to an existing archive
        /// </summary>
        /// <param name="archive">Archive to add the file to</param>
        /// <param name="file">Full path to an existing file</param>
        /// <param name="outputDirectory">Directory that the existing file lives in</param>
        /// <param name="delete">Indicates if the file should be deleted after adding</param>
        /// <returns>True if the file was added successfully, false otherwise</returns>
        private static bool AddToArchive(ZipArchive archive, string file, string? outputDirectory, bool delete)
        {
            // Check if the file exists
            if (!File.Exists(file))
                return false;

            // Get the entry name from the file
            string entryName = file;
            if (!string.IsNullOrEmpty(outputDirectory))
                entryName = entryName.Substring(outputDirectory!.Length);

            // Ensure the entry is formatted correctly
            entryName = entryName.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            // Create and add the entry
            try
            {
#if NETFRAMEWORK || NETCOREAPP3_1 || NET5_0
                archive.CreateEntryFromFile(file, entryName, CompressionLevel.Optimal);
#else
                archive.CreateEntryFromFile(file, entryName, CompressionLevel.SmallestSize);
#endif
            }
            catch
            {
                return false;
            }

            // Try to delete the file if requested
            if (delete)
            {
                try { File.Delete(file); } catch { }
            }

            return true;
        }
#endif

        /// <summary>
        /// Validate if all required output files exist
        /// </summary>
        /// <param name="basePath">Base filename and path to use for checking</param>
        /// <returns>Tuple of true if all required files exist, false otherwise and a list representing missing files</returns>
        private (bool, List<string>) CheckRequiredFiles(string basePath)
        {
            // Get the list of output files
            var outputFiles = GetOutputFiles(basePath);
            if (outputFiles.Count == 0)
                return (false, ["Media and system combination not supported"]);

            // Check for the log file
            bool logArchiveExists = false;
#if NET452_OR_GREATER || NETCOREAPP
            ZipArchive? logArchive = null;
#endif
            if (File.Exists($"{basePath}_logs.zip"))
            {
                logArchiveExists = true;
#if NET452_OR_GREATER || NETCOREAPP
                try
                {
                    // Try to open the archive
                    logArchive = ZipFile.OpenRead($"{basePath}_logs.zip");
                }
                catch
                {
                    logArchiveExists = false;
                }
#endif
            }

            // Get a list of all missing required files
            var missingFiles = new List<string>();
            foreach (var outputFile in outputFiles)
            {
                // Only check required files
                if (!outputFile.IsRequired)
                    continue;

                // Use the built-in existence function
                if (outputFile.Exists())
                    continue;

                // If the log archive doesn't exist
                if (!logArchiveExists)
                {
                    missingFiles.Add(outputFile.Filenames[0]);
                    continue;
                }

#if NET20 || NET35 || NET40
                // Assume the zipfile has the file in it
                continue;
#else
                // Check the log archive
                if (outputFile.Exists(logArchive))
                    continue;

                // Add the file to the missing list
                missingFiles.Add(outputFile.Filenames[0]);
#endif
            }

            return (!missingFiles.Any(), missingFiles);
        }

        /// <summary>
        /// Generate a list of all deleteable filenames
        /// </summary>
        /// <param name="basePath">Base filename and path to use for checking</param>
        /// <returns>List of all deleteable filenames, empty otherwise</returns>
        private List<string> GetDeleteableFilenames(string basePath)
        {
            // Get the list of output files
            var outputFiles = GetOutputFiles(basePath);
            if (outputFiles.Count == 0)
                return [];

            // Filter down to deleteable files
            var deleteableFiles = outputFiles.Where(of => of.IsDeleteable);
            return deleteableFiles.SelectMany(of => of.Filenames).ToList();
        }

        /// <summary>
        /// Generate a list of all deleteable file paths
        /// </summary>
        /// <param name="basePath">Base filename and path to use for checking</param>
        /// <returns>List of all deleteable file paths, empty otherwise</returns>
        private List<string> GetDeleteableFilePaths(string basePath)
        {
            // Get the list of deleteable files
            var deleteableFilenames = GetDeleteableFilenames(basePath);
            if (deleteableFilenames.Count == 0)
                return [];

            // Return only files that exist
            var deleteableFiles = new List<string>();
            foreach (var filename in deleteableFilenames)
            {
                // Skip non-existent files
                if (!File.Exists(filename))
                    continue;

                deleteableFiles.Add(filename);
            }

            return deleteableFiles;
        }

        /// <summary>
        /// Generate a list of all MPF-generated filenames
        /// </summary>
        /// <param name="filenameSuffix">Optional suffix to append to the filename</param>
        /// <returns>List of all MPF-generated filenames, empty otherwise</returns>
        private static List<string> GetGeneratedFilenames(string? filenameSuffix)
        {
            // Set the base file path names
            const string submissionInfoBase = "!submissionInfo";
            const string protectionInfoBase = "!protectionInfo";

            // Ensure the filename suffix is formatted correctly
            filenameSuffix = string.IsNullOrEmpty(filenameSuffix) ? string.Empty : $"_{filenameSuffix}";

            // Define the output filenames
            return [
                $"{protectionInfoBase}{filenameSuffix}.txt",
                $"{submissionInfoBase}{filenameSuffix}.json",
                $"{submissionInfoBase}{filenameSuffix}.json.gz",
                $"{submissionInfoBase}{filenameSuffix}.txt",
            ];
        }

        /// <summary>
        /// Generate a list of all MPF-specific log files generated
        /// </summary>
        /// <param name="outputDirectory">Output folder to write to</param>
        /// <param name="filenameSuffix">Optional suffix to append to the filename</param>
        /// <returns>List of all log file paths, empty otherwise</returns>
        private static List<string> GetGeneratedFilePaths(string? outputDirectory, string? filenameSuffix)
        {
            // Get the list of generated files
            var generatedFilenames = GetGeneratedFilenames(filenameSuffix);
            if (generatedFilenames.Count == 0)
                return [];

            // Ensure the output directory
            outputDirectory ??= string.Empty;

            // Return only files that exist
            var generatedFiles = new List<string>();
            foreach (var filename in generatedFilenames)
            {
                // Skip non-existent files
                string outputFilePath = Path.Combine(outputDirectory, filename);
                if (!File.Exists(outputFilePath))
                    continue;

                generatedFiles.Add(outputFilePath);
            }

            return generatedFiles;
        }

        /// <summary>
        /// Generate a list of all zippable filenames
        /// </summary>
        /// <param name="basePath">Base filename and path to use for checking</param>
        /// <returns>List of all zippable filenames, empty otherwise</returns>
        private List<string> GetZippableFilenames(string basePath)
        {
            // Get the list of output files
            var outputFiles = GetOutputFiles(basePath);
            if (outputFiles.Count == 0)
                return [];

            // Filter down to zippable files
            var zippableFiles = outputFiles.Where(of => of.IsZippable);
            return zippableFiles.SelectMany(of => of.Filenames).ToList();
        }

        /// <summary>
        /// Generate a list of all zippable file paths
        /// </summary>
        /// <param name="basePath">Base filename and path to use for checking</param>
        /// <returns>List of all zippable file paths, empty otherwise</returns>
        private List<string> GetZippableFilePaths(string basePath)
        {
            // Get the list of zippable files
            var zippableFilenames = GetZippableFilenames(basePath);
            if (zippableFilenames.Count == 0)
                return [];

            // Return only files that exist
            var zippableFiles = new List<string>();
            foreach (var filename in zippableFilenames)
            {
                // Skip non-existent files
                if (!File.Exists(filename))
                    continue;

                zippableFiles.Add(filename);
            }

            return zippableFiles;
        }

        #endregion

        #region Shared Methods

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

        #endregion
    }
}
