using System;
using System.IO;

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
    public class OutputFile
    {
        /// <summary>
        /// Set of all filename variants
        /// </summary>
        public string[] Filenames { get; private set; }

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
        private readonly OutputFileFlags _flags;

        /// <summary>
        /// Create an OutputFile with a single filename
        /// </summary>
        public OutputFile(string filename, OutputFileFlags flags)
        {
            Filenames = [filename];
            _flags = flags;
        }

        /// <summary>
        /// Create an OutputFile with set of filenames
        /// </summary>
        public OutputFile(string[] filenames, OutputFileFlags flags)
        {
            Filenames = filenames;
            _flags = flags;
        }

        /// <summary>
        /// Indicates if an output file exists in a base directory
        /// </summary>
        /// <param name="baseDirectory">Base directory to check in</param>
        public bool Exists(string baseDirectory)
        {
            foreach (string filename in Filenames)
            {
                // Check for invalid filenames
                if (string.IsNullOrEmpty(filename))
                    continue;

                try
                {
                    string possiblePath = Path.Combine(baseDirectory, filename);
                    if (File.Exists(possiblePath))
                        return true;
                }
                catch { }
            }

            return false;
        }
    }
}