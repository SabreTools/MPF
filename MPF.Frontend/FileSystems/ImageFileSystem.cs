using System;
using System.Collections.Generic;
using System.IO;
using SabreTools.Data.Extensions;
using SabreTools.IO.Extensions;

namespace MPF.Frontend.FileSystems
{
    /// <summary>
    /// Files of a disc read from a disc image or an unmounted device, without mounting or extracting it
    /// </summary>
    /// <remarks>
    /// Reads the UDF file system if the disc has one and the ISO 9660 file system otherwise,
    /// which is the file system a mounted disc shows on Windows
    /// </remarks>
    public sealed partial class ImageFileSystem : DiscFileSystem
    {
        /// <summary>
        /// Number of bytes in a sector of user data
        /// </summary>
        private const int SectorSize = 2048;

        /// <summary>
        /// Number of bytes in a raw CD sector
        /// </summary>
        private const int RawSectorSize = 2352;

        /// <summary>
        /// Path of the disc image or device, null if opened from a stream
        /// </summary>
        public string? SourcePath { get; private set; }

        /// <summary>
        /// Name of the file system that is read, null if no file system was found
        /// </summary>
        public string? FileSystemName { get; private set; }

        /// <inheritdoc/>
        public override string? VolumeLabel => _volumeLabel;

        /// <summary>
        /// Stream of the user data of the disc, in 2048-byte sectors
        /// </summary>
        private readonly Stream _data;

        /// <summary>
        /// Volume label of the file system that is read
        /// </summary>
        private string? _volumeLabel;

        /// <summary>
        /// All entries by normalized path, once read
        /// </summary>
        private Dictionary<string, DiscEntry>? _index;

        /// <param name="data">Stream of the user data of the disc, in 2048-byte sectors</param>
        private ImageFileSystem(Stream data)
        {
            _data = data;
        }

        /// <summary>
        /// Open a disc image or an unmounted device
        /// </summary>
        /// <param name="path">Path to a disc image (2048 or raw 2352-byte sectors) or device</param>
        /// <returns>File system on success, null if the path cannot be opened</returns>
        /// <remarks>A disc without a readable file system still allows reading the first sector</remarks>
        public static ImageFileSystem? Create(string path)
        {
            Stream? stream = null;
            try
            {
                if (IsUnixDevice(path))
                {
                    // Random access avoids reading ahead into sectors that are slow or impossible to read
                    string blockDevice = GetUnixBlockDevice(path);
                    var device = new FileStream(blockDevice, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, SectorSize, FileOptions.RandomAccess);
                    stream = new VolumeStream(device, GetUnixDeviceLength(blockDevice));
                    var deviceFileSystem = Open(stream);
                    deviceFileSystem.SourcePath = path;
                    return deviceFileSystem;
                }
                else if (File.Exists(path))
                {
                    stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                    var imageFileSystem = Create(stream);
                    imageFileSystem.SourcePath = path;
                    return imageFileSystem;
                }

                return null;
            }
            catch
            {
                stream?.Dispose();
                return null;
            }
        }

        /// <summary>
        /// Open a disc image from a stream
        /// </summary>
        /// <param name="stream">Seekable stream of a disc image (2048 or raw 2352-byte sectors), disposed with the file system</param>
        /// <returns>File system, which may have no readable file system</returns>
        internal static ImageFileSystem Create(Stream stream)
        {
            var data = IsRawImage(stream) ? new CDROMExtensions.UserDataStream(stream) : stream;
            return Open(data);
        }

        /// <summary>
        /// Read the file system of the user data of a disc
        /// </summary>
        private static ImageFileSystem Open(Stream data)
        {
            // The ISO 9660 volume descriptors also limit the volume, so they are read first
            var fileSystem = new ImageFileSystem(data);
            fileSystem.OpenISO9660();
            fileSystem.OpenUDF();

            return fileSystem;
        }

        #region DiscFileSystem

        /// <inheritdoc/>
        public override bool FileExists(string path)
            => FindEntry(path) is DiscEntry entry && !entry.IsDirectory;

        /// <inheritdoc/>
        public override bool DirectoryExists(string path)
            => FindEntry(path) is DiscEntry entry && entry.IsDirectory;

        /// <inheritdoc/>
        public override long GetFileLength(string path)
            => FindEntry(path) is DiscEntry entry && !entry.IsDirectory ? entry.Length : -1;

        /// <inheritdoc/>
        public override DateTime? GetLastWriteTimeUtc(string path)
            => FindEntry(path) is DiscEntry entry && !entry.IsDirectory ? entry.LastWriteTimeUtc : null;

        /// <inheritdoc/>
        public override Stream? OpenFile(string path)
        {
            if (FindEntry(path) is not DiscEntry entry || entry.IsDirectory)
                return null;

            return OpenEntry(entry);
        }

        /// <inheritdoc/>
        public override string[] GetFiles(string path, string searchPattern, bool recursive)
        {
            List<string> paths = [];
            foreach (var entry in GetEntries(path, recursive))
            {
                if (!entry.IsDirectory && MatchesPattern(entry.Name, searchPattern))
                    paths.Add(entry.Path);
            }

            return [.. paths];
        }

        /// <inheritdoc/>
        public override string[] GetDirectories(string path, string searchPattern)
        {
            List<string> paths = [];
            foreach (var entry in GetEntries(path, recursive: false))
            {
                if (entry.IsDirectory && MatchesPattern(entry.Name, searchPattern))
                    paths.Add(entry.Path);
            }

            return [.. paths];
        }

        /// <inheritdoc/>
        public override byte[]? ReadFirstBytes(int numBytes)
        {
            // Must read between 1 and 2048 bytes
            if (numBytes < 1)
                return null;
            else if (numBytes > SectorSize)
                numBytes = SectorSize;

            try
            {
                var bytes = new byte[numBytes];
                _data.Seek(0, SeekOrigin.Begin);
                int read = 0;
                while (read < numBytes)
                {
                    int chunk = _data.Read(bytes, read, numBytes - read);
                    if (chunk <= 0)
                        return null;

                    read += chunk;
                }

                return bytes;
            }
            catch
            {
                // Absorb the exception
                return null;
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _data.Dispose();
        }

        #endregion

        #region Entries

        /// <summary>
        /// Get all files and directories of the disc, reading the whole directory hierarchy once
        /// </summary>
        /// <returns>Entries of all files and directories</returns>
        internal List<DiscEntry> GetAllEntries()
            => [.. GetIndex().Values];

        /// <summary>
        /// Open the contents of an entry
        /// </summary>
        /// <param name="entry">DiscEntry of a file</param>
        /// <returns>Seekable stream of the file contents, null if the file cannot be read</returns>
        internal Stream? OpenEntry(DiscEntry entry)
        {
            if (entry.EmbeddedData is not null)
                return new MemoryStream(entry.EmbeddedData, false);
            else if (entry.Extents is null)
                return null;

            return new ExtentStream(_data, entry.Extents, entry.Length);
        }

        /// <summary>
        /// Find the entry of a path
        /// </summary>
        /// <param name="path">Relative path</param>
        /// <returns>DiscEntry on success, null if the path does not exist</returns>
        private DiscEntry? FindEntry(string path)
        {
            if (FileSystemName is null)
                return null;

            if (_index is not null)
                return _index.TryGetValue(NormalizePath(path), out var indexed) ? indexed : null;

            try
            {
                return _udf is not null ? FindUDFEntry(path) : FindISO9660Entry(path);
            }
            catch
            {
                // Absorb the exception, the path cannot be read
                return null;
            }
        }

        /// <summary>
        /// Get the entries in a directory
        /// </summary>
        /// <param name="path">Relative path of the directory</param>
        /// <param name="recursive">True to include the entries of all subdirectories, false otherwise</param>
        /// <returns>Entries in the directory, empty if the directory does not exist</returns>
        private List<DiscEntry> GetEntries(string path, bool recursive)
        {
            List<DiscEntry> entries = [];
            if (FileSystemName is null)
                return entries;

            string directory = NormalizePath(path);
            string prefix = directory.Length == 0 ? string.Empty : $"{directory}/";

            // Recursive listings and listings after the whole hierarchy was read use the index
            if (recursive || _index is not null)
            {
                foreach (var kvp in GetIndex())
                {
                    if (!kvp.Key.StartsWith(prefix, StringComparison.Ordinal) || kvp.Key.Length == prefix.Length)
                        continue;
                    if (!recursive && kvp.Key.IndexOf('/', prefix.Length) >= 0)
                        continue;

                    entries.Add(kvp.Value);
                }

                return entries;
            }

            try
            {
                var entry = FindEntry(path);
                if (entry is null || !entry.IsDirectory)
                    return entries;

                return _udf is not null ? ListUDFDirectory(entry) : ListISO9660Directory(entry);
            }
            catch
            {
                // Absorb the exception, the directory cannot be read
                return entries;
            }
        }

        /// <summary>
        /// Get the index of all entries, reading the whole directory hierarchy if needed
        /// </summary>
        private Dictionary<string, DiscEntry> GetIndex()
        {
            if (_index is not null)
                return _index;

            var index = new Dictionary<string, DiscEntry>();
            try
            {
                if (_udf is not null)
                    IndexUDF(index);
                else if (FileSystemName is not null)
                    IndexISO9660(index);
            }
            catch
            {
                // Absorb the exception, keep what could be read
            }

            _index = index;
            return index;
        }

        /// <summary>
        /// Add an entry to the index, keeping the first entry of a path
        /// </summary>
        private static void AddToIndex(Dictionary<string, DiscEntry> index, DiscEntry entry)
        {
            string key = NormalizePath(entry.Path);
            if (!index.ContainsKey(key))
                index[key] = entry;
        }

        /// <summary>
        /// Combine a directory path and a name
        /// </summary>
        private static string CombinePath(string directory, string name)
            => directory.Length == 0 ? name : $"{directory}/{name}";

        /// <summary>
        /// Normalize a relative path for comparison
        /// </summary>
        /// <remarks>
        /// Separators become '/', names are compared case-insensitively and without
        /// an ISO 9660 version suffix (";1") or trailing period
        /// </remarks>
        private static string NormalizePath(string path)
        {
            string[] names = path.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < names.Length; i++)
            {
                names[i] = TrimVersion(names[i]).ToUpperInvariant();
            }

            return string.Join("/", names);
        }

        #endregion

        #region Opening

        /// <summary>
        /// Determine if a path is a device on Linux
        /// </summary>
        private static bool IsUnixDevice(string path)
            => Environment.OSVersion.Platform == PlatformID.Unix
                && path.StartsWith("/dev/", StringComparison.Ordinal);

        /// <summary>
        /// Get the block device of a SCSI generic device on Linux, since only block devices can be read by sector
        /// </summary>
        /// <returns>Path of the block device, the given path if it is no SCSI generic device</returns>
        private static string GetUnixBlockDevice(string path)
        {
            string name = Path.GetFileName(path);
            if (!name.StartsWith("sg", StringComparison.Ordinal))
                return path;

            string blockRoot = Path.Combine(Path.Combine(Path.Combine("/sys/class/scsi_generic", name), "device"), "block");
            string[] blockDevices = IOExtensions.SafeGetFileSystemEntries(blockRoot);
            if (blockDevices.Length == 0)
                return path;

            return Path.Combine("/dev", Path.GetFileName(blockDevices[0]));
        }

        /// <summary>
        /// Get the length of a device on Linux from sysfs
        /// </summary>
        /// <returns>Length of the device in bytes, long.MaxValue if unknown</returns>
        private static long GetUnixDeviceLength(string path)
        {
            string sizePath = Path.Combine(Path.Combine("/sys/class/block", Path.GetFileName(path)), "size");
            if (File.Exists(sizePath) && long.TryParse(File.ReadAllText(sizePath).Trim(), out long sectors) && sectors > 0)
                return sectors * 512;

            return long.MaxValue;
        }

        /// <summary>
        /// Determine if an image contains raw 2352-byte CD sectors
        /// </summary>
        /// <remarks>Sector 16 holds the first volume descriptor and always starts with a sync pattern</remarks>
        private static bool IsRawImage(Stream file)
        {
            if (file.Length < 17 * RawSectorSize || file.Length % RawSectorSize != 0)
                return false;

            var sync = new byte[12];
            file.Seek(16 * RawSectorSize, SeekOrigin.Begin);
            if (file.Read(sync, 0, sync.Length) != sync.Length)
                return false;

            if (sync[0] != 0x00 || sync[11] != 0x00)
                return false;

            for (int i = 1; i < 11; i++)
            {
                if (sync[i] != 0xFF)
                    return false;
            }

            return true;
        }

        #endregion
    }
}
