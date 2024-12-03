using System;
using System.Collections.Generic;
using System.IO;
#if NET452_OR_GREATER || NETCOREAPP
using System.IO.Compression;
using System.Linq;
#endif
using System.Text.RegularExpressions;

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
        public override bool Exists(string baseDirectory)
        {
            // Ensure the directory exists
            if (!Directory.Exists(baseDirectory))
                return false;
            
            // Get list of all files in directory
            var directoryFiles = Directory.GetFiles(baseDirectory);
            foreach (string file in directoryFiles)
            {
                if (Array.FindIndex(Filenames, pattern => Regex.IsMatch(file, pattern)) > -1)
                    return true;
            }

            return false;
        }

#if NET452_OR_GREATER || NETCOREAPP
        /// <inheritdoc/>
        public override bool Exists(ZipArchive? archive)
        {
            // If the archive is invalid
            if (archive == null)
                return false;

            // Get list of all files in archive
            var archiveFiles = archive.Entries.Select(e => e.Name).ToList();
            foreach (string file in archiveFiles)
            {
                if (Array.Exists(Filenames, pattern => Regex.IsMatch(file, pattern)))
                    return true;
            }

            return false;
        }
#endif

        /// <inheritdoc/>
        public override List<string> GetPaths(string baseDirectory)
        {
            // Ensure the directory exists
            if (!Directory.Exists(baseDirectory))
                return [];

            List<string> paths = [];
            
            // Get list of all files in directory
            var directoryFiles = Directory.GetFiles(baseDirectory);
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