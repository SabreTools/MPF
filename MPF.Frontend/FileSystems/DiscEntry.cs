using System;

namespace MPF.Frontend.FileSystems
{
    /// <summary>
    /// File or directory of the disc
    /// </summary>
    internal sealed class DiscEntry
    {
        /// <summary>
        /// Relative path, separated by '/'
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Name as recorded, without an ISO 9660 version suffix
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// True if the entry is a directory, false otherwise
        /// </summary>
        public bool IsDirectory { get; }

        /// <summary>
        /// Length of the file in bytes
        /// </summary>
        public long Length { get; }

        /// <summary>
        /// Last modified time in UTC, if valid
        /// </summary>
        public DateTime? LastWriteTimeUtc { get; }

        /// <summary>
        /// Extents of the file, null if the file cannot be read
        /// </summary>
        public Extent[]? Extents { get; }

        /// <summary>
        /// Contents of a file stored inside its file entry
        /// </summary>
        public byte[]? EmbeddedData { get; }

        /// <summary>
        /// Directory records of an ISO 9660 entry
        /// </summary>
        public SabreTools.Data.Models.ISO9660.DirectoryRecord[]? Records { get; }

        /// <summary>
        /// Logical block address key of a UDF entry
        /// </summary>
        public long Key { get; }

        public DiscEntry(string path,
            bool isDirectory,
            long length,
            DateTime? lastWriteTimeUtc,
            Extent[]? extents,
            byte[]? embeddedData,
            SabreTools.Data.Models.ISO9660.DirectoryRecord[]? records,
            long key)
        {
            Path = path;
            int separator = path.LastIndexOf('/');
            Name = DiscFileSystem.TrimVersion(separator >= 0 ? path.Substring(separator + 1) : path);
            IsDirectory = isDirectory;
            Length = length;
            LastWriteTimeUtc = lastWriteTimeUtc;
            Extents = extents;
            EmbeddedData = embeddedData;
            Records = records;
            Key = key;
        }
    }
}
