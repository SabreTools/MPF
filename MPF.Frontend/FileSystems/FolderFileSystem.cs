using System;
using System.Collections.Generic;
using System.IO;

namespace MPF.Frontend.FileSystems
{
    /// <summary>
    /// Files of a disc read from a directory, such as a mounted disc
    /// </summary>
    public sealed class FolderFileSystem : DiscFileSystem
    {
        /// <inheritdoc/>
        public override string? VolumeLabel => null;

        /// <summary>
        /// Root directory of the disc
        /// </summary>
        public string Root { get; }

        /// <param name="root">Root directory of the disc</param>
        public FolderFileSystem(string root)
        {
            Root = root;
        }

        /// <inheritdoc/>
        public override bool FileExists(string path)
            => File.Exists(ResolvePath(path));

        /// <inheritdoc/>
        public override bool DirectoryExists(string path)
            => Directory.Exists(ResolvePath(path));

        /// <inheritdoc/>
        public override long GetFileLength(string path)
        {
            string? fullPath = ResolvePath(path);
            if (fullPath is null || !File.Exists(fullPath))
                return -1;

            return new FileInfo(fullPath).Length;
        }

        /// <inheritdoc/>
        public override DateTime? GetLastWriteTimeUtc(string path)
        {
            string? fullPath = ResolvePath(path);
            if (fullPath is null || !File.Exists(fullPath))
                return null;

            return new FileInfo(fullPath).LastWriteTimeUtc;
        }

        /// <inheritdoc/>
        public override Stream? OpenFile(string path)
        {
            string? fullPath = ResolvePath(path);
            if (fullPath is null || !File.Exists(fullPath))
                return null;

            return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        /// <inheritdoc/>
        public override string[] GetFiles(string path, string searchPattern, bool recursive)
        {
            string? fullPath = ResolvePath(path);
            if (fullPath is null || !Directory.Exists(fullPath))
                return [];

#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
            // Hidden and system files are listed too, like Directory.GetFiles with a SearchOption does
            var options = new EnumerationOptions
            {
                AttributesToSkip = 0,
                MatchCasing = MatchCasing.CaseInsensitive,
                RecurseSubdirectories = recursive,
            };

            return GetRelativePaths(Directory.GetFiles(fullPath, searchPattern, options));
#else
            var option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            return GetRelativePaths(Directory.GetFiles(fullPath, searchPattern, option));
#endif
        }

        /// <inheritdoc/>
        public override string[] GetDirectories(string path, string searchPattern)
        {
            string? fullPath = ResolvePath(path);
            if (fullPath is null || !Directory.Exists(fullPath))
                return [];

#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
            var options = new EnumerationOptions
            {
                AttributesToSkip = 0,
                MatchCasing = MatchCasing.CaseInsensitive,
            };

            return GetRelativePaths(Directory.GetDirectories(fullPath, searchPattern, options));
#else
            return GetRelativePaths(Directory.GetDirectories(fullPath, searchPattern, SearchOption.TopDirectoryOnly));
#endif
        }

        /// <inheritdoc/>
        /// <remarks>Only possible for a drive letter on Windows</remarks>
        public override byte[]? ReadFirstBytes(int numBytes)
        {
            if (!IsWindowsDriveRoot(Root))
                return null;

            // Must read between 1 and 2048 bytes
            if (numBytes < 1)
                return null;
            else if (numBytes > 2048)
                numBytes = 2048;

            string drivePath = $"\\\\.\\{Root[0]}:";
            var firstSector = new byte[numBytes];
            try
            {
                // Open the drive as a raw device
                using var driveStream = new FileStream(drivePath, FileMode.Open, FileAccess.Read);

                // Read the first sector
                int bytesRead = driveStream.Read(firstSector, 0, numBytes);
                if (bytesRead < numBytes)
                    return null;
            }
            catch
            {
                // Absorb the exception
                return null;
            }

            return firstSector;
        }

        /// <summary>
        /// Determine if a path is the root of a drive letter on Windows
        /// </summary>
        /// <param name="path">Path to check</param>
        /// <returns>True if the path is a drive root such as "D:\", false otherwise</returns>
        internal static bool IsWindowsDriveRoot(string path)
        {
            if (!SystemEnvironment.IsWindows())
                return false;

            return path.Length >= 2
                && path.Length <= 3
                && char.IsLetter(path[0])
                && path[1] == ':'
                && (path.Length == 2 || path[2] == '\\' || path[2] == '/');
        }

        /// <summary>
        /// Find the full path of an entry below the root directory
        /// </summary>
        /// <param name="path">Relative path of the entry</param>
        /// <returns>Full path of the existing entry, null if there is none</returns>
        /// <remarks>
        /// Names are matched case-insensitively and without an ISO 9660 version suffix, like on a
        /// mounted disc on Windows. Linux shows "SYSTEM.CNF;1" as "system.cnf", for example.
        /// </remarks>
        private string? ResolvePath(string path)
        {
            string current = Root;
            foreach (string name in path.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries))
            {
                // Only a directory can hold the next name
                if (!Directory.Exists(current))
                    return null;

                // The name as given is used if it exists, so other directories are not listed
                string exact = Path.Combine(current, name);
                if (File.Exists(exact) || Directory.Exists(exact))
                {
                    current = exact;
                    continue;
                }

                string? match = null;
                foreach (string entry in Directory.GetFileSystemEntries(current))
                {
                    if (string.Equals(TrimVersion(Path.GetFileName(entry)), TrimVersion(name), StringComparison.OrdinalIgnoreCase))
                    {
                        match = entry;
                        break;
                    }
                }

                if (match is null)
                    return null;

                current = match;
            }

            return current;
        }

        /// <summary>
        /// Convert full paths below the root directory to relative paths
        /// </summary>
        private string[] GetRelativePaths(string[] fullPaths)
        {
            var relativePaths = new List<string>(fullPaths.Length);
            foreach (string fullPath in fullPaths)
            {
                string relative = fullPath.StartsWith(Root, StringComparison.Ordinal)
                    ? fullPath.Substring(Root.Length)
                    : fullPath;
                relativePaths.Add(relative.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            }

            return [.. relativePaths];
        }
    }
}
