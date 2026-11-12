using System;
using System.Collections.Generic;
using System.Text;
using SabreTools.Data.Extensions;
using SabreTools.Data.Models.UDF;
using UDFReader = SabreTools.Serialization.Readers.UDF;

namespace MPF.Frontend.FileSystems
{
    public sealed partial class ImageFileSystem
    {
        /// <summary>
        /// UDF volume that is read, null if the disc has no readable UDF file system
        /// </summary>
        private Volume? _udf;

        /// <summary>
        /// Read the UDF volume and file set structures
        /// </summary>
        /// <returns>True if a UDF file system was found, false otherwise</returns>
        private bool OpenUDF()
        {
            try
            {
                // The anchor at sector 256 is only read if the disc announces a UDF file system,
                // since reading a sector that is not needed can take minutes on a damaged or protected CD
                bool announced = false;
                foreach (var vsd in UDFReader.ParseVolumeRecognitionSequence(_data))
                {
                    string identifier = Encoding.ASCII.GetString(vsd.StandardIdentifier);
                    if (identifier == "NSR02" || identifier == "NSR03")
                        announced = true;
                }

                if (!announced)
                    return false;

                var volume = new UDFReader().Deserialize(_data, includeDirectories: false);
                if (volume?.FileSetDescriptor is null)
                    return false;

                _udf = volume;
                string? label = volume.GetLogicalVolumeDescriptor()?.LogicalVolumeIdentifier.DecodeDString();
                if (!string.IsNullOrEmpty(label))
                    _volumeLabel = label;

                FileSystemName = "UDF";
                return true;
            }
            catch
            {
                // Absorb the exception, there is no readable UDF file system
                return false;
            }
        }

        /// <summary>
        /// Find the entry of a path in the UDF directory hierarchy
        /// </summary>
        private DiscEntry? FindUDFEntry(string path)
        {
            string[] names = path.Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries);
            var fileEntry = UDFReader.FindFileEntry(_data, _udf!, path, out long key);

            // Paths taken from ISO 9660 data, such as boot file names, may carry a version suffix
            if (fileEntry is null && names.Length > 0 && TrimVersion(names[names.Length - 1]) != names[names.Length - 1])
            {
                names[names.Length - 1] = TrimVersion(names[names.Length - 1]);
                fileEntry = UDFReader.FindFileEntry(_data, _udf!, string.Join("/", names), out key);
            }

            if (fileEntry is null)
                return null;
            return CreateUDFEntry(string.Join("/", names), key, fileEntry);
        }

        /// <summary>
        /// List the entries of a UDF directory
        /// </summary>
        /// <remarks>Listed files carry only their path and type</remarks>
        private List<DiscEntry> ListUDFDirectory(DiscEntry directory)
        {
            List<DiscEntry> entries = [];
            if (!_udf!.Directories.TryGetValue(directory.Key, out var identifiers))
                return entries;

            foreach (var identifier in identifiers)
            {
                if (!TryGetUDFName(identifier, out string name))
                    continue;

                bool isDirectory = (identifier.FileCharacteristics & FileCharacteristics.DIRECTORY) == FileCharacteristics.DIRECTORY;
                entries.Add(new DiscEntry(CombinePath(directory.Path, name),
                    isDirectory,
                    length: -1,
                    lastWriteTimeUtc: null,
                    extents: null,
                    embeddedData: null,
                    records: null,
                    identifier.ICB.ExtentLocation.GetKey()));
            }

            return entries;
        }

        /// <summary>
        /// Read the whole UDF directory hierarchy into the index
        /// </summary>
        private void IndexUDF(Dictionary<string, DiscEntry> index)
        {
            var volume = new UDFReader().Deserialize(_data, includeDirectories: true);
            if (volume?.FileSetDescriptor is null)
                return;

            _udf = volume;
            long root = volume.FileSetDescriptor.RootDirectoryICB.ExtentLocation.GetKey();
            IndexUDFDirectory(index, string.Empty, root, [root]);
        }

        /// <summary>
        /// Add the entries of a UDF directory and its subdirectories to the index
        /// </summary>
        private void IndexUDFDirectory(Dictionary<string, DiscEntry> index, string path, long key, HashSet<long> visited)
        {
            if (!_udf!.Directories.TryGetValue(key, out var identifiers))
                return;

            foreach (var identifier in identifiers)
            {
                if (!TryGetUDFName(identifier, out string name))
                    continue;

                long childKey = identifier.ICB.ExtentLocation.GetKey();
                if (!_udf.FileEntries.TryGetValue(childKey, out var fileEntry))
                    continue;

                var entry = CreateUDFEntry(CombinePath(path, name), childKey, fileEntry);
                AddToIndex(index, entry);

                // Directories reachable twice are read once
                if (entry.IsDirectory && visited.Add(childKey))
                    IndexUDFDirectory(index, entry.Path, childKey, visited);
            }
        }

        /// <summary>
        /// Create the entry for a UDF file entry
        /// </summary>
        private DiscEntry CreateUDFEntry(string path, long key, FileEntry fileEntry)
        {
            bool isDirectory = fileEntry.ICBTag.FileType == FileType.DIRECTORY;
            long length = (long)fileEntry.InformationLength;

            byte[]? embeddedData = null;
            Extent[]? extents = null;
            if (!isDirectory && fileEntry.ICBTag.GetAllocationDescriptorType() == AllocationDescriptorType.EMBEDDED)
            {
                // The file contents are stored in place of the allocation descriptors
                embeddedData = new byte[length];
                Array.Copy(fileEntry.AllocationDescriptors, embeddedData, length);
            }
            else if (!isDirectory)
            {
                extents = CreateUDFExtents(key, fileEntry, length);
            }

            return new DiscEntry(path,
                isDirectory,
                length,
                GetUDFTime(fileEntry.ModificationDateAndTime),
                extents,
                embeddedData,
                records: null,
                key);
        }

        /// <summary>
        /// Map the allocation descriptors of a file to extents on the volume
        /// </summary>
        /// <returns>Extents of the file, null if they cannot be mapped</returns>
        private Extent[]? CreateUDFExtents(long key, FileEntry fileEntry, long length)
        {
            var volume = _udf!;
            var descriptors = volume.GetAllocationDescriptors(key, fileEntry);
            if (descriptors is null)
                return null;

            List<Extent> extents = [];
            long total = 0;
            foreach (var descriptor in descriptors)
            {
                if (total >= length)
                    break;

                long extentLength = Math.Min(UDFExtensions.GetExtentLength(descriptor.ExtentLength), length - total);
                bool recorded = UDFExtensions.GetExtentType(descriptor.ExtentLength) == ExtentType.RECORDED_AND_ALLOCATED;

                long sector = 0;
                if (recorded && !volume.TryGetSector(descriptor.ExtentLocation.GetKey(), out sector))
                    return null;

                extents.Add(new Extent(sector * SectorSize, extentLength, recorded));
                total += extentLength;
            }

            return [.. extents];
        }

        /// <summary>
        /// Decode the name of a file identifier, skipping deleted entries and the parent directory
        /// </summary>
        private static bool TryGetUDFName(FileIdentifierDescriptor identifier, out string name)
        {
            name = string.Empty;
            if ((identifier.FileCharacteristics & (FileCharacteristics.DELETED | FileCharacteristics.PARENT)) != 0)
                return false;

            string? decoded = identifier.FileIdentifier.DecodeDCharacters();
            if (string.IsNullOrEmpty(decoded))
                return false;

            name = decoded!;
            return true;
        }

        /// <summary>
        /// Convert a UDF timestamp to UTC
        /// </summary>
        /// <returns>Time in UTC, null if the recorded time is not valid</returns>
        internal static DateTime? GetUDFTime(Timestamp time)
        {
            try
            {
                var local = new DateTime(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second, DateTimeKind.Unspecified);

                // Local time carries its offset from UTC in minutes, as a 12-bit signed value
                int type = time.TypeAndTimezone >> 12;
                int offset = time.TypeAndTimezone & 0x0FFF;
                if (offset >= 0x800)
                    offset -= 0x1000;

                if (type == 1 && offset != -2047)
                    local = local.AddMinutes(-offset);

                return DateTime.SpecifyKind(local, DateTimeKind.Utc);
            }
            catch
            {
                // Absorb the exception, the recorded time is not valid
                return null;
            }
        }
    }
}
