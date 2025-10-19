using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MPF.Processors.OutputFiles;
using SabreTools.Hashing;
using SabreTools.Data.Models.Logiqx;
using SabreTools.RedumpLib.Data;
#if NET462_OR_GREATER || NETCOREAPP
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;
using SharpCompress.Compressors.ZStandard;
using SharpCompress.Writers.Zip;
#endif

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

        #endregion

        /// <summary>
        /// Generate processor for a system and media type combination
        /// </summary>
        /// <param name="system">RedumpSystem value to use</param>
        public BaseProcessor(RedumpSystem? system)
        {
            System = system;
        }

        #region Abstract Methods

        /// <summary>
        /// Determine the media type based on the provided files
        /// </summary>
        /// <param name="outputDirectory">Output folder to use as the base path</param>
        /// <param name="outputFilename">Output filename to use as the base path</param>
        /// <returns>MediaType that was determined, if possible</returns>
        public abstract MediaType? DetermineMediaType(string? outputDirectory, string outputFilename);

        /// <summary>
        /// Generate a SubmissionInfo for the output files
        /// </summary>
        /// <param name="submissionInfo">Base submission info to fill in specifics for</param>
        /// <param name="mediaType">Media type for specific information gathering</param>
        /// <param name="basePath">Base filename and path to use for checking</param>
        /// <param name="redumpCompat">Determines if outputs are processed according to Redump specifications</param>
        public abstract void GenerateSubmissionInfo(SubmissionInfo submissionInfo, MediaType? mediaType, string basePath, bool redumpCompat);

        /// <summary>
        /// Generate a list of all output files generated
        /// </summary>
        /// <param name="mediaType">Media type for controlling expected file sets</param>
        /// <param name="outputDirectory">Output folder to use as the base path</param>
        /// <param name="outputFilename">Output filename to use as the base path</param>
        /// <returns>List of all output files, empty otherwise</returns>
        /// <remarks>Assumes filename has an extension</remarks>
        internal abstract List<OutputFile> GetOutputFiles(MediaType? mediaType, string? outputDirectory, string outputFilename);

        #endregion

        #region Output Files

        /// <summary>
        /// Compress log files to save space
        /// </summary>
        /// <param name="mediaType">Media type for controlling expected file sets</param>
        /// <param name="logCompression">Compression type to use for logs</param>
        /// <param name="outputDirectory">Output folder to use as the base path</param>
        /// <param name="outputFilename">Output filename to use as the base path</param>
        /// <param name="filenameSuffix">Output filename to use as the base path</param>
        /// <param name="processor">Processor object representing how to process the outputs</param>
        /// <returns>True if the process succeeded, false otherwise</returns>
        /// <remarks>Assumes filename has an extension</remarks>
        public bool CompressLogFiles(MediaType? mediaType,
            LogCompression logCompression,
            string? outputDirectory,
            string outputFilename,
            string? filenameSuffix,
            out string status)
        {
#if NET20 || NET35 || NET40 || NET452
            status = "Log compression is not available for this framework version";
            return false;
#else
            // Assemble a base path
            string basePath = Path.GetFileNameWithoutExtension(outputFilename);
            if (!string.IsNullOrEmpty(outputDirectory))
                basePath = Path.Combine(outputDirectory, basePath);

            // Generate the archive filename
            string archiveName = $"{basePath}_logs.zip";

            // Get the lists of zippable files
            var zippableFiles = GetZippableFilePaths(mediaType, outputDirectory, outputFilename);
            var preservedFiles = GetPreservedFilePaths(mediaType, outputDirectory, outputFilename);
            var generatedFiles = GetGeneratedFilePaths(outputDirectory, filenameSuffix);

            // Don't create an archive if there are no paths
            if (zippableFiles.Count == 0 && generatedFiles.Count == 0)
            {
                status = "No files to compress!";
                return true;
            }

            // If the file already exists, change the archive name
            if (File.Exists(archiveName))
            {
                string now = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                archiveName = $"{basePath}_logs_{now}.zip";
            }

            // Add the log files to the archive and delete the uncompressed file after
            ZipArchive? zf = null;
            bool disposed = false;
            try
            {
                zf = ZipArchive.Create();

                List<string> successful = AddToArchive(zf, zippableFiles, outputDirectory);
                _ = AddToArchive(zf, generatedFiles, outputDirectory);

                switch (logCompression)
                {
                    case LogCompression.DeflateMaximum:
                        zf.SaveTo(archiveName, new ZipWriterOptions(CompressionType.Deflate, CompressionLevel.BestCompression) { UseZip64 = true });
                        break;
                    case LogCompression.Zstd19:
                        zf.SaveTo(archiveName, new ZipWriterOptions(CompressionType.ZStandard, (CompressionLevel)19) { UseZip64 = true });
                        break;
                    case LogCompression.DeflateDefault:
                    default:
                        zf.SaveTo(archiveName, new ZipWriterOptions(CompressionType.Deflate, CompressionLevel.Default) { UseZip64 = true });
                        break;
                }

                // Dispose the archive
                zf?.Dispose();
                disposed = true;

                // Delete all successful files
                foreach (string file in successful)
                {
                    // Ignore files that are preserved
                    if (preservedFiles.Contains(file))
                        continue;
                    
                    try { File.Delete(file); } catch { }
                }

                status = "Compression complete!";
                return true;
            }
            catch (Exception ex)
            {
                status = $"Compression could not complete: {ex}";
                return false;
            }
            finally
            {
                if (!disposed)
                    zf?.Dispose();
            }
#endif
        }

        /// <summary>
        /// Compress log files to save space
        /// </summary>
        /// <param name="mediaType">Media type for controlling expected file sets</param>
        /// <param name="outputDirectory">Output folder to use as the base path</param>
        /// <param name="outputFilename">Output filename to use as the base path</param>
        /// <param name="processor">Processor object representing how to process the outputs</param>
        /// <returns>True if the process succeeded, false otherwise</returns>
        /// <remarks>Assumes filename has an extension</remarks>
        public bool DeleteUnnecessaryFiles(MediaType? mediaType,
            string? outputDirectory,
            string outputFilename,
            out string status)
        {
            // Get the list of deleteable files from the parameters object
            var files = GetDeleteableFilePaths(mediaType, outputDirectory, outputFilename);

            if (files.Count == 0)
            {
                status = "No files to delete!";
                return true;
            }

            // Attempt to delete all of the files
            foreach (string file in files)
            {
                try
                {
                    File.Delete(file);
                }
                catch { }
            }

            status = "Deletion complete!";
            return true;
        }

        /// <summary>
        /// Ensures that all required output files have been created
        /// </summary>
        /// <param name="mediaType">Media type for controlling expected file sets</param>
        /// <param name="outputDirectory">Output folder to use as the base path</param>
        /// <param name="outputFilename">Output filename to use as the base path</param>
        /// <returns>A list representing missing files, empty if none</returns>
        /// <remarks>Assumes filename has an extension</remarks>
        public List<string> FoundAllFiles(MediaType? mediaType, string? outputDirectory, string outputFilename)
        {
            // Assemble a base path
            string basePath = Path.GetFileNameWithoutExtension(outputFilename);
            if (!string.IsNullOrEmpty(outputDirectory))
                basePath = Path.Combine(outputDirectory, basePath);

            // Get the list of output files
            var outputFiles = GetOutputFiles(mediaType, outputDirectory, outputFilename);
            if (outputFiles.Count == 0)
                return ["Media and system combination not supported"];

            // Check for the log file
            bool logArchiveExists = false;
#if NET462_OR_GREATER || NETCOREAPP
            ZipArchive? logArchive = null;
#endif
            if (File.Exists($"{basePath}_logs.zip"))
            {
                logArchiveExists = true;
#if NET462_OR_GREATER || NETCOREAPP
                try
                {
                    // Try to open the archive
                    logArchive = ZipArchive.Open($"{basePath}_logs.zip");
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
                if (outputFile.Exists(outputDirectory ?? string.Empty))
                    continue;

                // If the log archive doesn't exist
                if (!logArchiveExists)
                {
                    missingFiles.Add(outputFile.Filenames[0]);
                    continue;
                }

#if NET20 || NET35 || NET40 || NET452
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

#if NET462_OR_GREATER || NETCOREAPP
            // Close the log archive, if it exists
            logArchive?.Dispose();
#endif

            return missingFiles;
        }

        /// <summary>
        /// Extracts found files from a found archive if it exists
        /// </summary>
        /// <param name="mediaType">Media type for controlling expected file sets</param>
        /// <param name="outputDirectory">Output folder to use as the base path</param>
        /// <param name="outputFilename">Output filename to use as the base path</param>
        /// <remarks>Assumes filename has an extension</remarks>
#if NET462_OR_GREATER || NETCOREAPP
        public void ExtractFromLogs(MediaType? mediaType, string? outputDirectory, string outputFilename)
        {
            // Get the list of output files
            var outputFiles = GetOutputFiles(mediaType, outputDirectory, outputFilename);
            if (outputFiles.Count == 0)
                return;

            // Assemble a base path
            string basePath = Path.GetFileNameWithoutExtension(outputFilename);
            if (!string.IsNullOrEmpty(outputDirectory))
                basePath = Path.Combine(outputDirectory, basePath);

            // Check for the log archive
            if (!File.Exists($"{basePath}_logs.zip"))
                return;

            // Extract all found output files from the archive
            ZipArchive? logArchive = null;
            try
            {
                logArchive = ZipArchive.Open($"{basePath}_logs.zip");
                foreach (var outputFile in outputFiles)
                {
                    outputFile.Extract(logArchive, outputDirectory ?? string.Empty);
                }
            }
            catch
            {
                // Absorb the exception
                return;
            }
            finally
            {
                logArchive?.Dispose();
            }
        }
#endif

        /// <summary>
        /// Ensures that no potential output files have been created
        /// </summary>
        /// <param name="mediaType">Media type for controlling expected file sets</param>
        /// <param name="outputDirectory">Output folder to use as the base path</param>
        /// <param name="outputFilename">Output filename to use as the base path</param>
        /// <returns>True if any dumping files exist, False if none</returns>
        /// <remarks>Assumes filename has an extension</remarks>
        public bool FoundAnyFiles(MediaType? mediaType, string? outputDirectory, string outputFilename)
        {
            // Assemble a base path
            string basePath = Path.GetFileNameWithoutExtension(outputFilename);
            if (!string.IsNullOrEmpty(outputDirectory))
                basePath = Path.Combine(outputDirectory, basePath);

            // Get the list of output files
            var outputFiles = GetOutputFiles(mediaType, outputDirectory, outputFilename);
            if (outputFiles.Count == 0)
                return false;

            // Check for the log file
            if (File.Exists($"{basePath}_logs.zip"))
                return true;

            // Check all output files
            foreach (var outputFile in outputFiles)
            {
                // Use the built-in existence function
                if (outputFile.Exists(outputDirectory ?? string.Empty))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Generate artifacts and return them as a dictionary
        /// </summary>
        /// <param name="mediaType">Media type for controlling expected file sets</param>
        /// <param name="outputDirectory">Output folder to use as the base path</param>
        /// <param name="outputFilename">Output filename to use as the base path</param>
        /// <returns>Dictiionary of artifact keys to Base64-encoded values, if possible</param>
        /// <remarks>Assumes filename has an extension</remarks>
        public Dictionary<string, string> GenerateArtifacts(MediaType? mediaType, string? outputDirectory, string outputFilename)
        {
            // Get the list of output files
            var outputFiles = GetOutputFiles(mediaType, outputDirectory, outputFilename);
            if (outputFiles.Count == 0)
                return [];

            try
            {
                // Create the artifacts dictionary
                var artifacts = new Dictionary<string, string>();

                // Only try to create artifacts for files that exist
                foreach (var outputFile in outputFiles)
                {
                    // Skip non-artifact files
                    if (!outputFile.IsArtifact || outputFile.ArtifactKey == null)
                        continue;

                    // Skip non-existent files
                    if (!outputFile.Exists(outputDirectory ?? string.Empty))
                        continue;

                    // Skip non-existent files
                    foreach (var filePath in outputFile.GetPaths(outputDirectory ?? string.Empty))
                    {
                        // Get binary artifacts as a byte array
                        if (outputFile.IsBinaryArtifact)
                        {
                            byte[] data = File.ReadAllBytes(filePath);
                            string str = Convert.ToBase64String(data);
                            artifacts[outputFile.ArtifactKey] = str;
                        }
                        else
                        {
                            string? data = ProcessingTool.GetFullFile(filePath);
                            string str = ProcessingTool.GetBase64(data) ?? string.Empty;
                            artifacts[outputFile.ArtifactKey] = str;
                        }

                        break;
                    }
                }

                return artifacts;
            }
            catch
            {
                // Any issues shouldn't stop processing
                return [];
            }
        }

#if NET462_OR_GREATER || NETCOREAPP
        /// <summary>
        /// Try to add a set of files to an existing archive
        /// </summary>
        /// <param name="archive">Archive to add the file to</param>
        /// <param name="files">Full path to a set of existing files</param>
        /// <param name="outputDirectory">Directory that the existing files live in</param>
        /// <returns>List of all files that were successfully added</returns>
        private static List<string> AddToArchive(ZipArchive archive, List<string> files, string? outputDirectory)
        {
            // An empty list means success
            if (files.Count == 0)
                return [];

            // Loop through and add all files
            List<string> added = [];
            foreach (string file in files)
            {
                if (AddToArchive(archive, file, outputDirectory))
                    added.Add(file);
            }

            return added;
        }

        /// <summary>
        /// Try to add a file to an existing archive
        /// </summary>
        /// <param name="archive">Archive to add the file to</param>
        /// <param name="file">Full path to an existing file</param>
        /// <param name="outputDirectory">Directory that the existing file lives in</param>
        /// <returns>True if the file was added successfully, false otherwise</returns>
        private static bool AddToArchive(ZipArchive archive, string file, string? outputDirectory)
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
                archive.AddEntry(entryName, file);
            }
            catch
            {
                return false;
            }

            return true;
        }
#endif

        /// <summary>
        /// Generate a list of all deleteable file paths
        /// </summary>
        /// <param name="mediaType">Media type for controlling expected file sets</param>
        /// <param name="outputDirectory">Output folder to use as the base path</param>
        /// <param name="outputFilename">Output filename to use as the base path</param>
        /// <returns>List of all deleteable file paths, empty otherwise</returns>
        /// <remarks>Assumes filename has an extension</remarks>
        internal List<string> GetDeleteableFilePaths(MediaType? mediaType, string? outputDirectory, string outputFilename)
        {
            // Get the list of output files
            var outputFiles = GetOutputFiles(mediaType, outputDirectory, outputFilename);
            if (outputFiles.Count == 0)
                return [];

            // Filter down to deleteable files
            var deleteable = outputFiles.FindAll(of => of.IsDeleteable);

            // Get all paths that exist
            var deleteablePaths = new List<string>();
            foreach (var file in deleteable)
            {
                var paths = file.GetPaths(outputDirectory ?? string.Empty);
                paths = paths.FindAll(File.Exists);
                deleteablePaths.AddRange(paths);
            }

            return deleteablePaths;
        }

        /// <summary>
        /// Generate a list of all MPF-generated filenames
        /// </summary>
        /// <param name="filenameSuffix">Optional suffix to append to the filename</param>
        /// <returns>List of all MPF-generated filenames, empty otherwise</returns>
        internal static List<string> GetGeneratedFilenames(string? filenameSuffix)
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
        /// <param name="outputDirectory">Output folder to use as the base path</param>
        /// <param name="filenameSuffix">Optional suffix to append to the filename</param>
        /// <returns>List of all log file paths, empty otherwise</returns>
        internal static List<string> GetGeneratedFilePaths(string? outputDirectory, string? filenameSuffix)
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
                string possiblePath = Path.Combine(outputDirectory, filename);
                if (!File.Exists(possiblePath))
                    continue;

                generatedFiles.Add(possiblePath);
            }

            return generatedFiles;
        }

        /// <summary>
        /// Generate a list of all zippable file paths
        /// </summary>
        /// <param name="mediaType">Media type for controlling expected file sets</param>
        /// <param name="outputDirectory">Output folder to use as the base path</param>
        /// <param name="outputFilename">Output filename to use as the base path</param>
        /// <returns>List of all zippable file paths, empty otherwise</returns>
        /// <remarks>Assumes filename has an extension</remarks>
        internal List<string> GetZippableFilePaths(MediaType? mediaType, string? outputDirectory, string outputFilename)
        {
            // Get the list of output files
            var outputFiles = GetOutputFiles(mediaType, outputDirectory, outputFilename);
            if (outputFiles.Count == 0)
                return [];

            // Filter down to zippable files
            var zippable = outputFiles.FindAll(of => of.IsZippable);

            // Get all paths that exist
            var zippablePaths = new List<string>();
            foreach (var file in zippable)
            {
                var paths = file.GetPaths(outputDirectory ?? string.Empty);
                paths = paths.FindAll(File.Exists);
                zippablePaths.AddRange(paths);
            }

            return zippablePaths;
        }

        /// <summary>
        /// Generate a list of all preserved file paths
        /// </summary>
        /// <param name="mediaType">Media type for controlling expected file sets</param>
        /// <param name="outputDirectory">Output folder to use as the base path</param>
        /// <param name="outputFilename">Output filename to use as the base path</param>
        /// <returns>List of all preserved file paths, empty otherwise</returns>
        /// <remarks>Assumes filename has an extension</remarks>
        internal List<string> GetPreservedFilePaths(MediaType? mediaType, string? outputDirectory, string outputFilename)
        {
            // Get the list of output files
            var outputFiles = GetOutputFiles(mediaType, outputDirectory, outputFilename);
            if (outputFiles.Count == 0)
                return [];

            // Filter down to zippable files
            var preserved = outputFiles.FindAll(of => of.IsPreserved);

            // Get all paths that exist
            var preservedPaths = new List<string>();
            foreach (var file in preserved)
            {
                var paths = file.GetPaths(outputDirectory ?? string.Empty);
                paths = paths.FindAll(File.Exists);
                preservedPaths.AddRange(paths);
            }

            return preservedPaths;
        }

        #endregion

        #region Shared Methods

        /// <summary>
        /// Attempt to compress a file to Zstandard, removing the original on success
        /// </summary>
        /// <param name="file">Full path to an existing file</param>
        /// <returns>True if the compression was a success, false otherwise</returns>
        internal static bool CompressZstandard(string file)
        {
#if NET20 || NET35 || NET40 || NET452
            // Compression is not available for this framework version
            return false;
#else
            // Ensure the file exists
            if (!File.Exists(file))
                return false;

            try
            {
                // Prepare the input and output streams
                using var ifs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var ofs = File.Open($"{file}.zst", FileMode.Create, FileAccess.Write, FileShare.None);
                using var zst = new ZStandardStream(ofs, CompressionMode.Compress, compressionLevel: 19, leaveOpen: true);

                // Compress and write in blocks
                int read = 0;
                do
                {
                    byte[] buffer = new byte[3 * 1024 * 1024];

                    read = ifs.Read(buffer, 0, buffer.Length);
                    if (read == 0)
                        break;

                    zst.Write(buffer, 0, read);
                    zst.Flush();
                } while (read > 0);
            }
            catch
            {
                // Try to delete the incomplete
                try { File.Delete($"{file}.zst"); } catch { }

                return false;
            }

            // Try to delete the file
            try { File.Delete(file); } catch { }

            return true;
#endif
        }

        /// <summary>
        /// Generate a CMP XML datfile string based on a single input file
        /// </summary>
        /// <param name="file">File to generate a datfile for</param>
        /// <returns>Datafile containing the hash information, null on error</returns>
        internal static Datafile? GenerateDatafile(string file)
        {
            // If the file is invalid
            if (string.IsNullOrEmpty(file))
                return null;
            if (!File.Exists(file))
                return null;

            // Attempt to get the hashes
            if (!HashTool.GetStandardHashes(file, out long size, out var crc32, out var md5, out var sha1))
                return null;

            // Generate and return the Datafile
            var rom = new Rom
            {
                Name = string.Empty,
                Size = size.ToString(),
                CRC = crc32,
                MD5 = md5,
                SHA1 = sha1,
            };
            var game = new Game { Rom = [rom] };
            var datafile = new Datafile { Game = [game] };

            return datafile;
        }

        /// <summary>
        /// Get the hex contents of the PIC file
        /// </summary>
        /// <param name="picPath">Path to the PIC.bin file associated with the dump</param>
        /// <param name="trimLength">Number of characters to trim the PIC to, if -1, ignored</param>
        /// <returns>PIC data as a hex string if possible, null on error</returns>
        /// <remarks>https://stackoverflow.com/questions/9932096/add-separator-to-string-at-every-n-characters</remarks>
        internal static string? GetPIC(string? picPath, int trimLength = -1)
        {
            // If the file is invalid
            if (picPath == null)
                return null;

            // If the file doesn't exist, we can't get the info
            if (!File.Exists(picPath))
                return null;

            // If the trim length is 0, no data will be returned
            if (trimLength == 0)
                return string.Empty;

            try
            {
                var hex = ProcessingTool.GetFullFile(picPath, true);
                if (hex == null)
                    return null;

                if (trimLength > -1 && trimLength < hex.Length)
                    hex = hex.Substring(0, trimLength);

                // TODO: Check for non-zero values in discarded PIC
                return SplitString(hex, 32);
            }
            catch
            {
                // Absorb the exception right now
                return null;
            }
        }

        /// <summary>
        /// Get a isobuster-formatted PVD from a 2048 byte-per-sector image, if possible
        /// </summary>
        /// <param name="isoPath">Path to ISO file</param>
        /// <param name="pvd">Formatted PVD string, otherwise null</param>
        /// <returns>True if PVD was successfully parsed, otherwise false</returns>
        internal static bool GetPVD(string isoPath, out string? pvd)
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
                // Absorb the exception
                return false;
            }
        }

        /// <summary>
        /// Returns true if a Cuesheet consists of only Audio tracks, false otherwise 
        /// </summary>
        internal static bool IsAudio(string? cue)
        {
            // Ignore invalid inputs
            if (string.IsNullOrEmpty(cue))
                return false;

            bool tracksExist = false;
            foreach (string cueLine in cue!.Split(["\r\n", "\n", "\r"], StringSplitOptions.None))
            {
                string line = cueLine.Trim();
                if (line.Length == 0)
                    continue;

                string[] tokens = line.Split([" ", "\t"], StringSplitOptions.RemoveEmptyEntries);

                if (tokens.Length < 3 || !tokens[0].Equals("TRACK", StringComparison.OrdinalIgnoreCase))
                    continue;

                tracksExist = true;
                string trackType = tokens[2].ToUpperInvariant();
                if (trackType != "AUDIO" && trackType != "CDG")
                    return false;
            }

            return tracksExist;
        }

        /// <summary>
        /// Split a string with newlines every <paramref name="count"/> characters
        /// </summary>
        internal static string SplitString(string? str, int count, bool trim = false)
        {
            // Ignore invalid inputs
            if (str == null || str.Length == 0)
                return string.Empty;

            // Handle non-modifying counts
            if (count < 1 || count > str.Length)
                return $"{str}\n";

            // Build the output string
            var sb = new StringBuilder();
            for (int i = 0; i < str.Length; i += count)
            {
                int lineSize = Math.Min(count, str.Length - i);
                string line = str.Substring(i, lineSize);

                if (trim)
                    line = line.Trim();

                sb.Append(line);
                sb.Append('\n');
            }

            return sb.ToString();
        }

        #endregion
    }
}
