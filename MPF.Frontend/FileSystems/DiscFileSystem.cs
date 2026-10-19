using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace MPF.Frontend.FileSystems
{
    /// <summary>
    /// Read-only access to the files of a disc, independent of where they are read from
    /// </summary>
    /// <remarks>
    /// Paths are relative to the root of the disc and may use '/' or '\' as separators
    /// </remarks>
    public abstract class DiscFileSystem : IDisposable
    {
        /// <summary>
        /// Volume label of the disc, if known
        /// </summary>
        public abstract string? VolumeLabel { get; }

        /// <summary>
        /// Determine if a file exists
        /// </summary>
        /// <param name="path">Relative path of the file</param>
        /// <returns>True if the file exists, false otherwise</returns>
        public abstract bool FileExists(string path);

        /// <summary>
        /// Determine if a directory exists
        /// </summary>
        /// <param name="path">Relative path of the directory</param>
        /// <returns>True if the directory exists, false otherwise</returns>
        public abstract bool DirectoryExists(string path);

        /// <summary>
        /// Get the length of a file
        /// </summary>
        /// <param name="path">Relative path of the file</param>
        /// <returns>Length of the file in bytes, -1 if the file does not exist</returns>
        public abstract long GetFileLength(string path);

        /// <summary>
        /// Get the last modified time of a file
        /// </summary>
        /// <param name="path">Relative path of the file</param>
        /// <returns>Last modified time in UTC, null if the file does not exist or has no valid time</returns>
        public abstract DateTime? GetLastWriteTimeUtc(string path);

        /// <summary>
        /// Open a file for reading
        /// </summary>
        /// <param name="path">Relative path of the file</param>
        /// <returns>Seekable stream of the file contents, null if the file does not exist</returns>
        public abstract Stream? OpenFile(string path);

        /// <summary>
        /// Get the files in a directory
        /// </summary>
        /// <param name="path">Relative path of the directory, empty for the root directory</param>
        /// <param name="searchPattern">Pattern the file names must match, supporting '*' and '?'</param>
        /// <param name="recursive">True to include the files of all subdirectories, false otherwise</param>
        /// <returns>Relative paths of the matching files, empty if the directory does not exist</returns>
        public abstract string[] GetFiles(string path, string searchPattern, bool recursive);

        /// <summary>
        /// Get the subdirectories of a directory
        /// </summary>
        /// <param name="path">Relative path of the directory, empty for the root directory</param>
        /// <param name="searchPattern">Pattern the directory names must match, supporting '*' and '?'</param>
        /// <returns>Relative paths of the matching subdirectories, empty if the directory does not exist</returns>
        public abstract string[] GetDirectories(string path, string searchPattern);

        /// <summary>
        /// Read the first bytes of the first sector of the disc
        /// </summary>
        /// <param name="numBytes">Number of bytes to read, maximum of one sector (2048 bytes)</param>
        /// <returns>The requested bytes, null if the disc cannot be read sector by sector</returns>
        public virtual byte[]? ReadFirstBytes(int numBytes) => null;

        /// <summary>
        /// Open the files of a disc from a directory or from a disc image or device
        /// </summary>
        /// <param name="path">Directory, disc image or device to open</param>
        /// <returns>File system on success, null if the path cannot be read</returns>
        public static DiscFileSystem? Open(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            if (Directory.Exists(path))
                return new FolderFileSystem(path!);

            return ImageFileSystem.Create(path!);
        }

        /// <summary>
        /// Open the files of the disc in a drive
        /// </summary>
        /// <param name="drive">Drive to open</param>
        /// <returns>File system on success, null if the files of the disc cannot be read</returns>
        /// <remarks>
        /// A drive letter or another directory is read as a directory. On Linux, the disc in an
        /// optical drive is read from the device without mounting it. On macOS, it is read from
        /// the directory macOS mounted it at.
        /// </remarks>
        public static DiscFileSystem? OpenDrive(Drive? drive)
        {
            if (string.IsNullOrEmpty(drive?.DevicePath))
                return null;

            // Drive letters are kept even if the disc cannot be read as files, to read its first sector
            string devicePath = drive!.DevicePath!;
            if (Directory.Exists(devicePath) || FolderFileSystem.IsWindowsDriveRoot(devicePath))
                return new FolderFileSystem(devicePath);

            // Other drives are only read as optical discs, or if the type is unknown (a path given on the command line)
            if (drive.InternalDriveType is not null && drive.InternalDriveType != InternalDriveType.Optical)
                return null;

            if (SystemEnvironment.IsLinux())
                return ImageFileSystem.Create(devicePath);

            if (SystemEnvironment.IsMacOS())
            {
                string? mountPoint = Drive.GetMacOSMountPoint(devicePath);
                return mountPoint is null ? null : new FolderFileSystem(mountPoint);
            }

            return null;
        }

        /// <summary>
        /// Open the files of a dump from its image
        /// </summary>
        /// <param name="basePath">Base path of the dump, without extension</param>
        /// <returns>File system of the first image or track with readable files, null if there is none</returns>
        public static ImageFileSystem? OpenDump(string basePath)
        {
            foreach (string imagePath in GetDumpImagePaths(basePath))
            {
                var fileSystem = ImageFileSystem.Create(imagePath);
                if (fileSystem?.FileSystemName is not null)
                    return fileSystem;

                fileSystem?.Dispose();
            }

            return null;
        }

        /// <summary>
        /// Get the image files of a dump
        /// </summary>
        /// <param name="basePath">Base path of the dump, without extension</param>
        /// <returns>The ISO or BIN image if it exists, otherwise the existing tracks of the cuesheet</returns>
        public static List<string> GetDumpImagePaths(string basePath)
        {
            if (File.Exists($"{basePath}.iso"))
                return [$"{basePath}.iso"];
            else if (File.Exists($"{basePath}.bin"))
                return [$"{basePath}.bin"];
            else if (!File.Exists($"{basePath}.cue"))
                return [];

            List<string> trackPaths = [];
            string[] cueLines = File.ReadAllLines($"{basePath}.cue");
            foreach (string cueLine in cueLines)
            {
                // Skip all non-FILE lines
                if (!cueLine.StartsWith("FILE"))
                    continue;

                // Extract the information
                var match = Regex.Match(cueLine, @"FILE ""(.*?)"" BINARY");
                if (!match.Success || match.Groups.Count == 0)
                    continue;

                // Get the track name from the matches
                string trackName = match.Groups[1].Value;
                trackName = Path.GetFileNameWithoutExtension(trackName);
                string baseDir = Path.GetDirectoryName(basePath) ?? string.Empty;
                string trackPath = Path.Combine(baseDir, trackName);

                // Add the track, if it exists
                if (File.Exists($"{trackPath}.bin"))
                    trackPaths.Add($"{trackPath}.bin");
            }

            return trackPaths;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Release the resources of the file system
        /// </summary>
        /// <param name="disposing">True if called from Dispose, false otherwise</param>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        /// Determine if a name matches a search pattern
        /// </summary>
        /// <param name="name">Name to check</param>
        /// <param name="searchPattern">Pattern supporting '*' (any number of characters) and '?' (one character)</param>
        /// <returns>True if the name matches, false otherwise</returns>
        /// <remarks>Names are compared case-insensitively, like on a mounted disc on Windows</remarks>
        internal static bool MatchesPattern(string name, string searchPattern)
        {
            int nameIndex = 0, patternIndex = 0;
            int starIndex = -1, starNameIndex = 0;
            while (nameIndex < name.Length)
            {
                if (patternIndex < searchPattern.Length
                    && (searchPattern[patternIndex] == '?'
                        || char.ToUpperInvariant(searchPattern[patternIndex]) == char.ToUpperInvariant(name[nameIndex])))
                {
                    nameIndex++;
                    patternIndex++;
                }
                else if (patternIndex < searchPattern.Length && searchPattern[patternIndex] == '*')
                {
                    starIndex = patternIndex++;
                    starNameIndex = nameIndex;
                }
                else if (starIndex >= 0)
                {
                    patternIndex = starIndex + 1;
                    nameIndex = ++starNameIndex;
                }
                else
                {
                    return false;
                }
            }

            while (patternIndex < searchPattern.Length && searchPattern[patternIndex] == '*')
            {
                patternIndex++;
            }

            return patternIndex == searchPattern.Length;
        }

        /// <summary>
        /// Remove an ISO 9660 version suffix and a trailing period from a name
        /// </summary>
        internal static string TrimVersion(string name)
        {
            int versionSeparator = name.LastIndexOf(';');
            if (versionSeparator > 0)
                name = name.Substring(0, versionSeparator);

            if (name.Length > 1 && name.EndsWith("."))
                name = name.Substring(0, name.Length - 1);

            return name;
        }
    }
}
