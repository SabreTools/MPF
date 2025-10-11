using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
#if NET462_OR_GREATER || NETCOREAPP
using SharpCompress.Archives;
using SharpCompress.Archives.Zip;
#endif

namespace MPF.Processors
{
    /// <summary>
    /// Represents a single output file with a Regex-matched name
    /// </summary>
    internal class RegexOutputFile : OutputFile
    {
        /// <summary>
        /// Create an OutputFile with a single filename
        /// </summary>
        public RegexOutputFile(string filename, OutputFileFlags flags)
            : base([filename], flags)
        {
        }

        /// <summary>
        /// Create an OutputFile with a single filename
        /// </summary>
        public RegexOutputFile(string filename, OutputFileFlags flags, string artifactKey)
            : base([filename], flags, artifactKey)
        {
        }

        /// <summary>
        /// Create an OutputFile with set of filenames
        /// </summary>
        public RegexOutputFile(string[] filenames, OutputFileFlags flags)
            : base(filenames, flags)
        {
        }

        /// <summary>
        /// Create an OutputFile with set of filenames
        /// </summary>
        public RegexOutputFile(string[] filenames, OutputFileFlags flags, string artifactKey)
            : base(filenames, flags, artifactKey)
        {
        }

        /// <inheritdoc/>
        public override bool Exists(string outputDirectory)
        {
            // Ensure the directory exists
            if (!Directory.Exists(outputDirectory))
                return false;

            // Get list of all files in directory
            var directoryFiles = Directory.GetFiles(outputDirectory);
            foreach (string file in directoryFiles)
            {
                if (Array.FindIndex(Filenames, pattern => Regex.IsMatch(file, pattern)) > -1)
                    return true;
            }

            return false;
        }

#if NET462_OR_GREATER || NETCOREAPP
        /// <inheritdoc/>
        public override bool Exists(ZipArchive? archive)
        {
            // If the archive is invalid
            if (archive == null)
                return false;

            // Get list of all files in archive
            foreach (var entry in archive.Entries)
            {
                if (entry.Key == null)
                    continue;

                if (Array.Exists(Filenames, pattern => Regex.IsMatch(entry.Key, pattern)))
                    return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public override bool Extract(ZipArchive? archive, string outputDirectory)
        {
            // If the archive is invalid
            if (archive == null)
                return false;

            // Get list of all files in archive
            foreach (var entry in archive.Entries)
            {
                if (entry.Key == null)
                    continue;

                var matches = Array.FindAll(Filenames, pattern => Regex.IsMatch(entry.Key, pattern));
                foreach (string match in matches)
                {
                    try
                    {
                        string outputPath = Path.Combine(outputDirectory, match);
                        entry.WriteToFile(outputPath);
                    }
                    catch { }
                }
            }

            return true;
        }
#endif

        /// <inheritdoc/>
        public override List<string> GetPaths(string outputDirectory)
        {
            // Ensure the directory exists
            if (!Directory.Exists(outputDirectory))
                return [];

            List<string> paths = [];

            // Get list of all files in directory
            var directoryFiles = Directory.GetFiles(outputDirectory);
            foreach (string file in directoryFiles)
            {
                var matches = Array.FindAll(Filenames, pattern => Regex.IsMatch(file, pattern));
                if (matches != null && matches.Length > 0)
                    paths.Add(file);
            }

            return paths;
        }
    }
}