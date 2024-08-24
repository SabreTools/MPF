using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MPF.Processors
{
    /// <summary>
    /// Represents a single output file with a Regex-matched name
    /// </summary>
    public class RegexOutputFile : OutputFile
    {
        /// <summary>
        /// Create an OutputFile with a single filename
        /// </summary>
        /// <remarks><paramref name="existFunc">is unused in this constructor</remarks>
        public RegexOutputFile(string filename, OutputFileFlags flags, Func<string, bool>? existsFunc = null)
            : base([filename], flags, existsFunc: null)
        {
        }

        /// <summary>
        /// Create an OutputFile with a single filename
        /// </summary>
        /// <remarks><paramref name="existFunc">is unused in this constructor</remarks>
        public RegexOutputFile(string filename, OutputFileFlags flags, string artifactKey, Func<string, bool>? existsFunc = null)
            : base([filename], flags, artifactKey, existsFunc: null)
        {
        }

        /// <summary>
        /// Create an OutputFile with set of filenames
        /// </summary>
        /// <remarks><paramref name="existFunc">is unused in this constructor</remarks>
        public RegexOutputFile(string[] filenames, OutputFileFlags flags, Func<string, bool>? existsFunc = null)
            : base(filenames, flags, existsFunc: null)
        {
        }

        /// <summary>
        /// Create an OutputFile with set of filenames
        /// </summary>
        /// <remarks><paramref name="existFunc">is unused in this constructor</remarks>
        public RegexOutputFile(string[] filenames, OutputFileFlags flags, string artifactKey, Func<string, bool>? existsFunc = null)
            : base(filenames, flags, artifactKey, existsFunc: null)
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
    }
}