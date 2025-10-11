using System;
using System.Collections.Generic;
using System.IO;
#if NET462_OR_GREATER || NETCOREAPP
using System.Linq;
using SharpCompress.Archives.Zip;
#endif

namespace MPF.Processors
{
    /// <summary>
    /// Represents attributes about an <see cref="OutputFile"/>
    /// </summary>
    [Flags]
    public enum OutputFileFlags : ushort
    {
        /// <summary>
        /// Default state
        /// </summary>
        None = 0x0000,

        /// <summary>
        /// File is required to exist
        /// </summary>
        Required = 0x0001,

        /// <summary>
        /// File is included as an artifact
        /// </summary>
        Artifact = 0x0002,

        /// <summary>
        /// File is included as a binary artifact
        /// </summary>
        Binary = 0x0004,

        /// <summary>
        /// File can be deleted after processing
        /// </summary>
        Deleteable = 0x0008,

        /// <summary>
        /// File can be zipped after processing
        /// </summary>
        Zippable = 0x0010,
    }

    /// <summary>
    /// Represents a single output file
    /// </summary>
    internal class OutputFile
    {
        /// <summary>
        /// Set of all filename variants
        /// </summary>
        public string[] Filenames { get; private set; }

        /// <summary>
        /// Key used when creating an artifact
        /// </summary>
        public string? ArtifactKey { get; private set; }

        /// <summary>
        /// Indicates if the file is required
        /// </summary>
        public bool IsRequired
        {
            get
            {
#if NET20 || NET35
                return (_flags & OutputFileFlags.Required) != 0;
#else
                return _flags.HasFlag(OutputFileFlags.Required);
#endif
            }
        }

        /// <summary>
        /// Indicates if the file is an artifact
        /// </summary>
        public bool IsArtifact
        {
            get
            {
#if NET20 || NET35
                return (_flags & OutputFileFlags.Artifact) != 0
                    || (_flags & OutputFileFlags.Binary) != 0;
#else
                return _flags.HasFlag(OutputFileFlags.Artifact)
                    || _flags.HasFlag(OutputFileFlags.Binary);
#endif
            }
        }

        /// <summary>
        /// Indicates if the file is a binary artifact
        /// </summary>
        public bool IsBinaryArtifact
        {
            get
            {
#if NET20 || NET35
                return (_flags & OutputFileFlags.Binary) != 0;
#else
                return _flags.HasFlag(OutputFileFlags.Binary);
#endif
            }
        }

        /// <summary>
        /// Indicates if the file is deleteable after processing
        /// </summary>
        public bool IsDeleteable
        {
            get
            {
#if NET20 || NET35
                return (_flags & OutputFileFlags.Deleteable) != 0;
#else
                return _flags.HasFlag(OutputFileFlags.Deleteable);
#endif
            }
        }

        /// <summary>
        /// Indicates if the file is zippable after processing
        /// </summary>
        public bool IsZippable
        {
            get
            {
#if NET20 || NET35
                return (_flags & OutputFileFlags.Zippable) != 0;
#else
                return _flags.HasFlag(OutputFileFlags.Zippable);
#endif
            }
        }

        /// <summary>
        /// Represents attributes about the current file
        /// </summary>
        protected readonly OutputFileFlags _flags;

        /// <summary>
        /// Create an OutputFile with a single filename
        /// </summary>
        public OutputFile(string filename, OutputFileFlags flags)
            : this([filename], flags)
        {
        }

        /// <summary>
        /// Create an OutputFile with a single filename
        /// </summary>
        public OutputFile(string filename, OutputFileFlags flags, string artifactKey)
            : this([filename], flags, artifactKey)
        {
        }

        /// <summary>
        /// Create an OutputFile with set of filenames
        /// </summary>
        public OutputFile(string[] filenames, OutputFileFlags flags)
        {
            Filenames = filenames;
            ArtifactKey = null;
            _flags = flags;

            // Validate the inputs
            if (filenames.Length == 0)
                throw new ArgumentException($"{nameof(filenames)} must contain at least one value");
            if (IsArtifact && string.IsNullOrEmpty(ArtifactKey))
                throw new ArgumentException($"{nameof(flags)} should not contain the Artifact or Binary flag");
        }

        /// <summary>
        /// Create an OutputFile with set of filenames
        /// </summary>
        public OutputFile(string[] filenames, OutputFileFlags flags, string artifactKey)
        {
            Filenames = filenames;
            ArtifactKey = artifactKey;
            _flags = flags;

            // Validate the inputs
            if (filenames.Length == 0)
                throw new ArgumentException($"{nameof(filenames)} must contain at least one value");
            if (IsArtifact && string.IsNullOrEmpty(ArtifactKey))
                throw new ArgumentException($"{nameof(flags)} should not contain the Artifact or Binary flag");
        }

        /// <summary>
        /// Indicates if an output file exists in a base directory
        /// </summary>
        /// <param name="outputDirectory">Base directory to check in</param>
        public virtual bool Exists(string outputDirectory)
        {
            // Ensure the directory exists
            if (!Directory.Exists(outputDirectory))
                return false;

            foreach (string filename in Filenames)
            {
                // Check for invalid filenames
                if (string.IsNullOrEmpty(filename))
                    continue;

                try
                {
                    string possibleFile = Path.Combine(outputDirectory, filename);
                    if (File.Exists(possibleFile))
                        return true;
                }
                catch { }
            }

            return false;
        }

#if NET462_OR_GREATER || NETCOREAPP
        /// <summary>
        /// Indicates if an output file exists in an archive
        /// </summary>
        /// <param name="archive">Zip archive to check in</param>
        public virtual bool Exists(ZipArchive? archive)
        {
            // If the archive is invalid
            if (archive == null)
                return false;

            foreach (string filename in Filenames)
            {
                // Check for invalid filenames
                if (string.IsNullOrEmpty(filename))
                    continue;

                try
                {
                    // Check all entries on filename alone
                    if (archive.Entries.Any(e => e.Key == filename))
                        return true;
                }
                catch { }
            }

            return false;
        }
#endif

#if NET462_OR_GREATER || NETCOREAPP
        /// <summary>
        /// Extracts an output file from a zip archive
        /// </summary>
        /// <param name="archive">Zip archive to check in</param>
        /// <param name="outputDirectory">Base directory to extract to</param>
        /// <returns>True if file extracted, False otherwise</returns>
        public virtual bool Extract(ZipArchive? archive, string outputDirectory)
        {
            // If the archive is invalid
            if (archive == null)
                return false;

            foreach (string filename in Filenames)
            {
                // Check for invalid filenames
                if (string.IsNullOrEmpty(filename))
                    continue;

                try
                {
                    // Check all entries on filename alone
                    var outputFile = archive.Entries.FirstOrDefault(e => e.Key == filename);
                    if (outputFile == null || outputFile.IsDirectory)
                        continue;

                    string outputPath = Path.Combine(outputDirectory, filename);

                    using var entryStream = outputFile.OpenEntryStream();
                    using var fileStream = File.Create(outputPath);
                    entryStream.CopyTo(fileStream);
                }
                catch { }
            }

            return false;
        }
#endif

        /// <summary>
        /// Get all matching paths for the file
        /// </summary>
        /// <param name="outputDirectory">Base directory to check in</param>
        public virtual List<string> GetPaths(string outputDirectory)
        {
            List<string> paths = [];

            foreach (string filename in Filenames)
            {
                string possibleFile = Path.Combine(outputDirectory, filename);
                if (!File.Exists(possibleFile))
                    continue;

                paths.Add(possibleFile);
            }

            return paths;
        }
    }
}