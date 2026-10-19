using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SabreTools.Data.Extensions;
using SabreTools.Data.Models.ISO9660;
using ISO9660Reader = SabreTools.Serialization.Readers.ISO9660;

namespace MPF.Frontend.FileSystems
{
    public sealed partial class ImageFileSystem
    {
        /// <summary>
        /// Volume descriptor of the ISO 9660 directory hierarchy that is read
        /// </summary>
        private VolumeDescriptor? _iso9660;

        /// <summary>
        /// Number of bytes in a logical block of the ISO 9660 volume
        /// </summary>
        private short _iso9660BlockLength = SectorSize;

        /// <summary>
        /// Encoding of the ISO 9660 file identifiers
        /// </summary>
        private Encoding _iso9660Encoding = Encoding.UTF8;

        /// <summary>
        /// Read the ISO 9660 volume descriptors, preferring the Joliet directory hierarchy
        /// </summary>
        /// <returns>True if an ISO 9660 file system was found, false otherwise</returns>
        private bool OpenISO9660()
        {
            try
            {
                _data.Seek(16 * SectorSize, SeekOrigin.Begin);
                var vdSet = ISO9660Reader.ParseVolumeDescriptorSet(_data, SectorSize);

                PrimaryVolumeDescriptor? pvd = null;
                SupplementaryVolumeDescriptor? joliet = null;
                foreach (var vd in vdSet)
                {
                    if (vd is PrimaryVolumeDescriptor primary && pvd is null)
                        pvd = primary;
                    else if (vd is SupplementaryVolumeDescriptor supplementary && joliet is null && IsJoliet(supplementary))
                        joliet = supplementary;
                }

                if (pvd is null)
                    return false;

                // Keep reads within the volume, since a disc may continue with audio tracks after it
                if (_data is VolumeStream)
                {
                    long volumeLength = (long)pvd.VolumeSpaceSize.LittleEndian * pvd.GetLogicalBlockSize(SectorSize);
                    if (volumeLength > 0)
                        _data.SetLength(volumeLength);
                }

                if (joliet is not null)
                {
                    _iso9660 = joliet;
                    _iso9660Encoding = Encoding.BigEndianUnicode;
                    _volumeLabel = _iso9660Encoding.GetString(joliet.VolumeIdentifier).TrimEnd(' ', '\0');
                }
                else
                {
                    _iso9660 = pvd;
                    _volumeLabel = Encoding.ASCII.GetString(pvd.VolumeIdentifier).TrimEnd(' ', '\0');
                }

                _iso9660BlockLength = _iso9660.GetLogicalBlockSize(SectorSize);
                FileSystemName = "ISO 9660";
                return true;
            }
            catch
            {
                // Absorb the exception, there is no readable ISO 9660 file system
                return false;
            }
        }

        /// <summary>
        /// Find the entry of a path in the ISO 9660 directory hierarchy
        /// </summary>
        private DiscEntry? FindISO9660Entry(string path)
        {
            string[] names = path.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries);
            var records = ISO9660Reader.FindDirectoryRecords(_data, SectorSize, _iso9660!, path);
            if (records is null || records.Length == 0)
                return null;

            if (names.Length == 0)
                return CreateISO9660Entry(string.Empty, records);

            string directory = string.Join("/", names, 0, names.Length - 1);
            return CreateISO9660Entry(CombinePath(directory, GetISO9660Name(records[0])), records);
        }

        /// <summary>
        /// List the entries of an ISO 9660 directory
        /// </summary>
        private List<DiscEntry> ListISO9660Directory(DiscEntry directory)
        {
            var record = directory.Records![0];
            var directories = ISO9660Reader.ParseDirectory(_data, SectorSize, _iso9660BlockLength, record, false);
            int location = record.ExtentLocation.LittleEndian;

            // If the extent location field is ambiguous, the big-endian location may hold the directory
            if ((directories is null || !directories.ContainsKey(location)) && !record.ExtentLocation.IsValid)
            {
                directories = ISO9660Reader.ParseDirectory(_data, SectorSize, _iso9660BlockLength, record, true);
                location = record.ExtentLocation.BigEndian;
            }

            if (directories is null || !directories.TryGetValue(location, out var extent))
                return [];

            return extent is DirectoryExtent directoryExtent
                ? CreateISO9660Entries(directory.Path, directoryExtent.DirectoryRecords)
                : [];
        }

        /// <summary>
        /// Read the whole ISO 9660 directory hierarchy into the index
        /// </summary>
        private void IndexISO9660(Dictionary<string, DiscEntry> index)
        {
            var directories = ISO9660Reader.ParseDirectoryDescriptors(_data, SectorSize, [_iso9660!]);
            if (directories is null)
                return;

            var root = _iso9660 is SupplementaryVolumeDescriptor svd
                ? svd.RootDirectoryRecord
                : ((PrimaryVolumeDescriptor)_iso9660!).RootDirectoryRecord;

            IndexISO9660Directory(index, directories, string.Empty, root, []);
        }

        /// <summary>
        /// Add the entries of an ISO 9660 directory and its subdirectories to the index
        /// </summary>
        private void IndexISO9660Directory(Dictionary<string, DiscEntry> index, Dictionary<int, FileExtent> directories, string path, DirectoryRecord record, HashSet<int> visited)
        {
            // If the extent location field is ambiguous, the big-endian location may hold the directory
            int location = record.ExtentLocation.LittleEndian;
            if (!directories.ContainsKey(location) && !record.ExtentLocation.IsValid)
                location = record.ExtentLocation.BigEndian;

            // Directories reachable twice are read once
            if (!directories.TryGetValue(location, out var extent) || extent is not DirectoryExtent directoryExtent || !visited.Add(location))
                return;

            foreach (var entry in CreateISO9660Entries(path, directoryExtent.DirectoryRecords))
            {
                AddToIndex(index, entry);
                if (entry.IsDirectory)
                    IndexISO9660Directory(index, directories, entry.Path, entry.Records![0], visited);
            }
        }

        /// <summary>
        /// Create the entries for the directory records of one directory
        /// </summary>
        private List<DiscEntry> CreateISO9660Entries(string path, DirectoryRecord[] records)
        {
            List<DiscEntry> entries = [];
            for (int i = 0; i < records.Length; i++)
            {
                // Skip the directory itself and its parent
                if (records[i].FileIdentifier.Length == 1 && records[i].FileIdentifier[0] <= 1)
                    continue;

                // A file recorded in several extents continues in the following records, up to the final one
                var fileRecords = new List<DirectoryRecord> { records[i] };
                while ((records[i].FileFlags & FileFlags.MULTI_EXTENT) == FileFlags.MULTI_EXTENT && i + 1 < records.Length)
                {
                    i++;
                    fileRecords.Add(records[i]);
                }

                entries.Add(CreateISO9660Entry(CombinePath(path, GetISO9660Name(fileRecords[0])), [.. fileRecords]));
            }

            return entries;
        }

        /// <summary>
        /// Create the entry for the directory records of one file or directory
        /// </summary>
        private DiscEntry CreateISO9660Entry(string path, DirectoryRecord[] records)
        {
            bool isDirectory = (records[0].FileFlags & FileFlags.DIRECTORY) == FileFlags.DIRECTORY;

            long length = 0;
            bool readable = !isDirectory;
            List<Extent> extents = [];
            foreach (var record in records)
            {
                // Interleaved files are not supported
                if (record.FileUnitSize != 0 || record.InterleaveGapSize != 0)
                    readable = false;

                // File data follows the extended attribute record, if any
                long offset = ((long)record.ExtentLocation.LittleEndian + record.ExtendedAttributeRecordLength) * _iso9660BlockLength;
                long extentLength = record.ExtentLength.LittleEndian;
                if (!record.ExtentLength.IsValid && (extentLength == 0 || offset + extentLength > _data.Length))
                    extentLength = record.ExtentLength.BigEndian;

                extents.Add(new Extent(offset, extentLength, recorded: true));
                length += extentLength;
            }

            return new DiscEntry(path,
                isDirectory,
                length,
                GetISO9660Time(records[0].RecordingDateTime),
                readable ? [.. extents] : null,
                embeddedData: null,
                records,
                key: -1);
        }

        /// <summary>
        /// Decode the name of a directory record
        /// </summary>
        private string GetISO9660Name(DirectoryRecord record)
            => TrimVersion(_iso9660Encoding.GetString(record.FileIdentifier));

        /// <summary>
        /// Convert the recording time of a directory record to UTC
        /// </summary>
        /// <returns>Recording time in UTC, null if the recorded time is not valid</returns>
        internal static DateTime? GetISO9660Time(DirectoryRecordDateTime time)
        {
            try
            {
                // Years are counted from 1900, the offset from UTC in 15-minute intervals
                var local = new DateTime(1900 + time.YearsSince1990, time.Month, time.Day, time.Hour, time.Minute, time.Second, DateTimeKind.Unspecified);
                var utc = local.AddMinutes(-15 * (sbyte)time.TimezoneOffset);
                return DateTime.SpecifyKind(utc, DateTimeKind.Utc);
            }
            catch
            {
                // Absorb the exception, the recorded time is not valid
                return null;
            }
        }

        /// <summary>
        /// Determine if a supplementary volume descriptor describes a Joliet directory hierarchy
        /// </summary>
        private static bool IsJoliet(SupplementaryVolumeDescriptor svd)
        {
            byte[] escape = svd.EscapeSequences;
            return escape.Length >= 3
                && escape[0] == 0x25
                && escape[1] == 0x2F
                && (escape[2] == 0x40 || escape[2] == 0x43 || escape[2] == 0x45);
        }
    }
}
