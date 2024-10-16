using System;
using System.IO;
#if NET452_OR_GREATER || NETCOREAPP
using System.IO.Compression;
#endif

namespace MPF.Processors
{
    /// <summary>
    /// Represents a single output file with custom detection rules
    /// </summary>
    internal class CustomOutputFile : OutputFile
    {
        /// <summary>
        /// Optional func for determining if a file exists
        /// </summary>
        private readonly Func<string, bool> _existsFunc;

        /// <summary>
        /// Create an OutputFile with a single filename
        /// </summary>
        public CustomOutputFile(string filename, OutputFileFlags flags, Func<string, bool> existsFunc)
            : base([filename], flags)
        {
            _existsFunc = existsFunc;
        }

        /// <summary>
        /// Create an OutputFile with a single filename
        /// </summary>
        public CustomOutputFile(string filename, OutputFileFlags flags, string artifactKey, Func<string, bool> existsFunc)
            : base([filename], flags, artifactKey)
        {
            _existsFunc = existsFunc;
        }

        /// <summary>
        /// Create an OutputFile with set of filenames
        /// </summary>
        public CustomOutputFile(string[] filenames, OutputFileFlags flags, Func<string, bool> existsFunc)
            : base(filenames, flags)
        {
            _existsFunc = existsFunc;
        }

        /// <summary>
        /// Create an OutputFile with set of filenames
        /// </summary>
        public CustomOutputFile(string[] filenames, OutputFileFlags flags, string artifactKey, Func<string, bool> existsFunc)
            : base(filenames, flags, artifactKey)
        {
            _existsFunc = existsFunc;
        }

        /// <inheritdoc/>
        public override bool Exists(string baseDirectory)
        {
            // Ensure the directory exists
            if (!Directory.Exists(baseDirectory))
                return false;
            
            foreach (string filename in Filenames)
            {
                // Check for invalid filenames
                if (string.IsNullOrEmpty(filename))
                    continue;

                try
                {
                    string possibleFile = Path.Combine(baseDirectory, filename);
                    if (_existsFunc(possibleFile))
                        return true;
                }
                catch { }
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
            // Files aren't extracted so this check can't be done
            return false;
        }
#endif
    }
}