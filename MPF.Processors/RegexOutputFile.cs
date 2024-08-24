using System.IO;
#if NET452_OR_GREATER || NETCOREAPP
using System.IO.Compression;
#endif
using System.Linq;
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
            // Get list of all files in directory
            var directoryFiles = Directory.GetFiles(baseDirectory);
            foreach (string file in directoryFiles)
            {
                if (Filenames.Any(pattern => Regex.IsMatch(file, pattern)))
                    return true;
            }

            return false;
        }

#if NET452_OR_GREATER || NETCOREAPP
        /// <summary>
        /// Indicates if an output file exists in an archive
        /// </summary>
        /// <param name="archive">Zip archive to check in</param>
        public override bool Exists(ZipArchive? archive)
        {
            // If the archive is invalid
            if (archive == null)
                return false;

            // Get list of all files in archive
            var archiveFiles = archive.Entries.Select(e => e.Name).ToList();
            foreach (string file in archiveFiles)
            {
                if (Filenames.Any(pattern => Regex.IsMatch(file, pattern)))
                    return true;
            }

            return false;
        }
#endif
    }
}