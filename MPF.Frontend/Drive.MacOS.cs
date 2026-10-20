using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using MPF.Frontend.FileSystems;
using SabreTools.IO.Extensions;

namespace MPF.Frontend
{
    public partial class Drive
    {
        #region Fields

        /// <summary>
        /// Floppy media sizes in bytes for the standard 5.25" and 3.5" PC formats
        /// (360 KB, 720 KB, 1.2 MB, 1.44 MB, 2.88 MB). A removable disk reporting one of these
        /// exact capacities is a floppy drive with media inserted; no other removable storage
        /// uses these sizes, so this doubles as the floppy identity check.
        /// </summary>
        private static readonly HashSet<long> _macOSFloppyMediaSizes =
        [
            368640,   // 360 KB (5.25" DD)
            737280,   // 720 KB (3.5" DD)
            1228800,  // 1.2 MB (5.25" HD)
            1474560,  // 1.44 MB (3.5" HD)
            2949120,  // 2.88 MB (3.5" ED)
        ];

        #endregion

        #region macOS Helpers

        /// <summary>
        /// Append macOS storage devices that DriveInfo did not surface. On macOS,
        /// DriveInfo.GetDrives() only reports mounted volumes (e.g. "/"), not the raw device
        /// nodes (/dev/disk*) needed for dumping, so whole-disk devices are enumerated directly
        /// from diskutil instead. Optical drives are surfaced regardless of the fixed-drive
        /// toggle because they are removable dump targets; floppy and fixed/removable disks are
        /// hidden unless the user opts to show fixed drives, matching Windows and the Linux
        /// enumerator. Enumeration relies only on read-only diskutil queries and does not
        /// require elevated privileges; only the later raw read during a dump does.
        /// </summary>
        /// <param name="drives">Drives already discovered via DriveInfo</param>
        /// <param name="ignoreFixedDrives">True to ignore fixed and floppy drives, false otherwise</param>
        /// <returns>The drive list, with mounted volumes replaced by the devices behind them</returns>
        private static Drive[] AppendMacOSDrives(Drive[] drives, bool ignoreFixedDrives)
        {
            // Ensure this only runs on MacOS
            if (!SystemEnvironment.IsMacOS())
                return drives;

            var kept = new List<Drive>();
            var existingPaths = new HashSet<string>();
            foreach (var drive in drives)
            {
                // Ignore MacOS mount points
                if (IsMacOSMountPoint(drive.DevicePath))
                    continue;

                kept.Add(drive);
                if (drive.DevicePath is not null)
                    existingPaths.Add(drive.DevicePath);
            }

            foreach (var device in EnumerateMacOSDevices("/dev"))
            {
                // Optical drives are always surfaced; everything else follows the toggle
                if (device.InternalDriveType != Frontend.InternalDriveType.Optical && ignoreFixedDrives)
                    continue;

                // Skip devices already surfaced by an earlier entry
                if (!existingPaths.Add(device.DevicePath!))
                    continue;

                // Force marking the drive as active if it's removable or has a non-zero size
                var d = Create(device.InternalDriveType, device.DevicePath!);
                if (d is not null)
                {
                    d.DriveFormat = device.DriveFormat;
                    d.TotalSize = device.TotalSize;
                    d.MarkedActive = device.InternalDriveType != Frontend.InternalDriveType.Removable
                        || device.TotalSize > 0;
                    kept.Add(d);
                }
            }

            return [.. kept];
        }

        /// <summary>
        /// Whether a device path is a macOS mount point rather than a dumpable device. Devices
        /// discovered here are named by their BSD name ("disk4"), which never contains a path
        /// separator, while DriveInfo reports mounted volumes as paths ("/Volumes/LABEL", "/").
        /// </summary>
        /// <param name="devicePath">Device path to check</param>
        /// <returns>True when the path is a mount point; false otherwise</returns>
        private static bool IsMacOSMountPoint(string? devicePath)
        {
            if (string.IsNullOrEmpty(devicePath))
                return false;

            return devicePath!.Contains(Path.DirectorySeparatorChar.ToString())
                || devicePath.Contains(Path.AltDirectorySeparatorChar.ToString());
        }

        /// <summary>
        /// Enumerate macOS whole-disk devices via diskutil. Each whole-disk node under
        /// <paramref name="devRoot"/> (e.g. "/dev/disk0") is queried with "diskutil info" and
        /// classified; partitions (e.g. "disk0s1") and the raw variants ("/dev/rdisk*") are not
        /// whole-device targets and are excluded by the node filter. Synthesized volumes and
        /// disk images are dropped during parsing.
        /// </summary>
        /// <param name="devRoot">Root directory device nodes live under (typically "/dev")</param>
        /// <returns>Discovered physical devices, or an empty list when none are found</returns>
        internal static List<Drive> EnumerateMacOSDevices(string devRoot)
        {
            var result = new List<Drive>();
            foreach (var node in EnumerateMacOSWholeDiskNodes(devRoot))
            {
                string info = RunDiskutilInfo(Path.GetFileName(node));
                if (string.IsNullOrEmpty(info))
                    continue;

                var device = ParseMacOSDiskutilInfo(info, node);
                if (device is null)
                    continue;

                if (device.InternalDriveType == Frontend.InternalDriveType.Optical)
                    device.PopulateFromMacOSDevice(info);

                result.Add(device);
            }

            return result;
        }

        /// <summary>
        /// Populate the file system and size of the disc in an optical drive on macOS, as Windows reports them
        /// </summary>
        /// <param name="diskutilInfoOutput">Raw "diskutil info" output for the drive</param>
        /// <remarks>
        /// The disc is read from the raw device of the whole disc, which belongs to the user logged in at the
        /// console, unlike the devices of its tracks. The block device of the whole disc cannot be opened when the
        /// disc has audio tracks, as macOS mounts them on it. The raw device can only be read in whole blocks, which
        /// are raw 2352-byte sectors on a CD.
        /// </remarks>
        private void PopulateFromMacOSDevice(string diskutilInfoOutput)
        {
            var fields = ParseDiskutilFields(diskutilInfoOutput);
            long size = fields.TryGetValue("Disk Size", out var diskSize)
                ? ParseDiskutilByteSize(diskSize)
                : 0;
            int blockSize = fields.TryGetValue("Device Block Size", out var deviceBlockSize)
                ? ParseDiskutilBlockSize(deviceBlockSize)
                : 0;

            using var files = ImageFileSystem.Create($"/dev/r{DevicePath}", size, blockSize);
            DriveFormat = GetWindowsDriveFormat(files?.FileSystemName);
            TotalSize = files?.VolumeSize ?? default;
        }

        /// <summary>
        /// Enumerate macOS whole-disk device nodes under a directory by matching the kernel
        /// convention "disk" followed by one or more digits (disk0, disk1, ...). This rejects
        /// partition nodes ("disk0s1", which contain a non-digit slice suffix) and the raw
        /// character nodes ("rdisk0", which do not start with "disk").
        /// </summary>
        /// <param name="devRoot">Root directory to scan (typically "/dev")</param>
        /// <returns>Device node paths, or an empty list when the directory is unreadable</returns>
        internal static List<string> EnumerateMacOSWholeDiskNodes(string? devRoot)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(devRoot) || !Directory.Exists(devRoot!))
                return result;

            string[] candidates = IOExtensions.SafeGetFileSystemEntries(devRoot!, "disk*");
            foreach (var path in candidates)
            {
                if (IsMacOSWholeDiskNode(Path.GetFileName(path)))
                    result.Add(path);
            }

            return result;
        }

        /// <summary>
        /// Whether a device node name is a macOS whole disk: the literal "disk" followed by one
        /// or more digits and nothing else.
        /// </summary>
        /// <param name="name">Device node file name (e.g. "disk0")</param>
        /// <returns>True when the name is a whole-disk node; false otherwise</returns>
        private static bool IsMacOSWholeDiskNode(string name)
        {
            const string prefix = "disk";
            if (name.Length <= prefix.Length || !name.StartsWith(prefix, StringComparison.Ordinal))
                return false;

            for (int i = prefix.Length; i < name.Length; i++)
            {
                if (!char.IsDigit(name[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Parse the output of "diskutil info &lt;device&gt;" into a classified device, or null
        /// when the entry is not a physical dump target. Synthesized volumes (APFS containers)
        /// and disk images report Virtual = Yes and are skipped. Classification order matters:
        /// optical media is checked first, then floppy media (a removable disk reporting a
        /// standard floppy capacity), then the "Removable Media" flag distinguishes a removable
        /// drive from a fixed hard disk.
        /// </summary>
        /// <param name="diskutilInfoOutput">Raw "diskutil info" output for a single device</param>
        /// <param name="devicePath">Fallback device node path when the output omits one</param>
        /// <returns>The classified device, or null when it should not be surfaced</returns>
        internal static Drive? ParseMacOSDiskutilInfo(string? diskutilInfoOutput, string devicePath)
        {
            if (string.IsNullOrEmpty(diskutilInfoOutput))
                return null;

            var fields = ParseDiskutilFields(diskutilInfoOutput!);

            // Only physical devices are dump targets; drop synthesized volumes and disk images
            if (fields.TryGetValue("Virtual", out var virtualFlag)
                && string.Equals(virtualFlag, "Yes", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (fields.TryGetValue("Protocol", out var protocol)
                && string.Equals(protocol, "Disk Image", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            // Prefer the node diskutil reports, falling back to the enumerated one
            string node = fields.TryGetValue("Device Node", out var deviceNode) && !string.IsNullOrEmpty(deviceNode)
                ? deviceNode
                : devicePath;

            // A macOS dumping program takes the BSD name for a drive, so that becomes the device
            // path, the same way it is a drive letter on Windows and a device node on Linux.
            // Redumper resolves it against the BSD name IOKit publishes and rejects a
            // "/dev/"-prefixed path. diskutil reports it directly; the node's file name matches.
            string bsdName = fields.TryGetValue("Device Identifier", out var identifier) && !string.IsNullOrEmpty(identifier)
                ? identifier
                : Path.GetFileName(node);

            long size = fields.TryGetValue("Disk Size", out var diskSize)
                ? ParseDiskutilByteSize(diskSize)
                : 0;

            bool removable = fields.TryGetValue("Removable Media", out var removableFlag)
                && string.Equals(removableFlag, "Removable", StringComparison.OrdinalIgnoreCase);

            // Optical drives are identified by diskutil's optical-only fields, which no other
            // device type reports. Presence of the key is the signal, not its value: a drive
            // lists the key with an empty value when it cannot describe the media, so the value
            // is deliberately not inspected. The protocol is not a usable marker on its own —
            // an optical drive behind an AHCI controller reports "SATA", not "ATAPI" — but ATAPI
            // is still accepted for the drives that do report it.
            bool optical = fields.ContainsKey("Optical Media Type")
                || fields.ContainsKey("Optical Drive Type")
                || (protocol is not null && string.Equals(protocol, "ATAPI", StringComparison.OrdinalIgnoreCase));

            InternalDriveType driveType;
            if (optical)
                driveType = Frontend.InternalDriveType.Optical;
            else if (removable && _macOSFloppyMediaSizes.Contains(size))
                driveType = Frontend.InternalDriveType.Floppy;
            else if (removable)
                driveType = Frontend.InternalDriveType.Removable;
            else
                driveType = Frontend.InternalDriveType.HardDisk;

            return new Drive()
            {
                InternalDriveType = driveType,
                DevicePath = bsdName,
                TotalSize = size,
            };
        }

        /// <summary>
        /// Parse the exact byte count from a diskutil "Disk Size" value, which formats the size
        /// as "402.7 MB (402653184 Bytes) (exactly 786432 512-Byte-Units)". The value inside the
        /// first "(... Bytes)" group is returned.
        /// </summary>
        /// <param name="diskSizeValue">The "Disk Size" field value</param>
        /// <returns>The size in bytes, or 0 when it cannot be determined</returns>
        internal static long ParseDiskutilByteSize(string diskSizeValue)
        {
            if (string.IsNullOrEmpty(diskSizeValue))
                return 0;

            int open = diskSizeValue.IndexOf('(');
            while (open >= 0)
            {
                int close = diskSizeValue.IndexOf(')', open + 1);
                if (close < 0)
                    break;

                string inside = diskSizeValue.Substring(open + 1, close - open - 1).Trim();
                if (inside.EndsWith("Bytes", StringComparison.OrdinalIgnoreCase))
                {
                    string number = inside.Substring(0, inside.Length - "Bytes".Length).Trim();
                    if (long.TryParse(number, out long bytes) && bytes > 0)
                        return bytes;
                }

                open = diskSizeValue.IndexOf('(', close + 1);
            }

            return 0;
        }

        /// <summary>
        /// Parse the number of bytes in a block from a diskutil "Device Block Size" value, which formats it as
        /// "2048 Bytes"
        /// </summary>
        /// <param name="blockSizeValue">The "Device Block Size" field value</param>
        /// <returns>The block size in bytes, or 0 when it cannot be determined</returns>
        internal static int ParseDiskutilBlockSize(string blockSizeValue)
        {
            if (string.IsNullOrEmpty(blockSizeValue))
                return 0;

            string number = blockSizeValue.Trim();
            if (number.EndsWith("Bytes", StringComparison.OrdinalIgnoreCase))
                number = number.Substring(0, number.Length - "Bytes".Length).Trim();

            return int.TryParse(number, out int bytes) && bytes > 0 ? bytes : 0;
        }

        /// <summary>
        /// Parse "diskutil info" output into a case-insensitive map of field name to value.
        /// Each line is "Key:   Value"; the key is split on the first colon so values that
        /// themselves contain colons are preserved.
        /// </summary>
        /// <param name="diskutilInfoOutput">Raw "diskutil info" output</param>
        /// <returns>Field name to value map</returns>
        private static Dictionary<string, string> ParseDiskutilFields(string diskutilInfoOutput)
        {
            var fields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var rawLine in diskutilInfoOutput.Split('\n'))
            {
                int separator = rawLine.IndexOf(':');
                if (separator <= 0)
                    continue;

                string key = rawLine.Substring(0, separator).Trim();
                string value = rawLine.Substring(separator + 1).Trim();
                if (key.Length == 0)
                    continue;

                // Keep the first occurrence; diskutil lists the whole-device fields first
                if (!fields.ContainsKey(key))
                    fields[key] = value;
            }

            return fields;
        }

        /// <summary>
        /// Get the directory macOS mounted the disc in a drive at
        /// </summary>
        /// <param name="devicePath">BSD name of the drive (e.g. "disk4")</param>
        /// <returns>The mount point, or null when the disc is not mounted</returns>
        /// <remarks>
        /// The mount table is read with "mount", which answers at once, while "diskutil info"
        /// takes about half a second per device
        /// </remarks>
        internal static string? GetMacOSMountPoint(string? devicePath)
        {
            if (string.IsNullOrEmpty(devicePath) || IsMacOSMountPoint(devicePath))
                return null;

            return ParseMacOSMountPoint(RunMacOSCommand("mount", string.Empty), devicePath!);
        }

        /// <summary>
        /// Parse the mount point of a drive from the output of "mount"
        /// </summary>
        /// <param name="mountOutput">Raw "mount" output</param>
        /// <param name="devicePath">BSD name of the drive (e.g. "disk4")</param>
        /// <returns>The mount point, or null when the disc is not mounted</returns>
        /// <remarks>
        /// "mount" lists one mounted file system per line as "/dev/disk4s0 on /Volumes/LABEL
        /// (cd9660, local, read-only)". The file system of a disc may sit on the whole disk or
        /// on one of its slices ("disk4s0")
        /// </remarks>
        internal static string? ParseMacOSMountPoint(string? mountOutput, string devicePath)
        {
            if (string.IsNullOrEmpty(mountOutput))
                return null;

            // The whole disk or one of its slices, but not another disk ("disk41")
            var device = new Regex($"^/dev/{Regex.Escape(devicePath)}(s[0-9]+)?$");
            foreach (var rawLine in mountOutput!.Split('\n'))
            {
                int on = rawLine.IndexOf(" on ", StringComparison.Ordinal);
                if (on <= 0 || !device.IsMatch(rawLine.Substring(0, on)))
                    continue;

                // The mount point may contain parentheses itself, the options are the last ones
                int options = rawLine.LastIndexOf(" (", StringComparison.Ordinal);
                if (options <= on)
                    continue;

                return rawLine.Substring(on + " on ".Length, options - on - " on ".Length);
            }

            return null;
        }

        /// <summary>
        /// Run "diskutil info &lt;deviceName&gt;" and return its standard output, or an empty
        /// string on any failure. The device name comes from the internal node enumeration, so
        /// it is a trusted "diskN" token rather than external input.
        /// </summary>
        /// <param name="deviceName">Whole-disk identifier (e.g. "disk0")</param>
        /// <returns>The command's standard output, or an empty string on failure</returns>
        /// TODO: Figure out if invoking diskutil is the only way of listing all drives
        private static string RunDiskutilInfo(string deviceName)
            => RunMacOSCommand("diskutil", $"info {deviceName}");

        /// <summary>
        /// Run a command and return its standard output, or an empty string on any failure
        /// </summary>
        /// <param name="fileName">Command to run</param>
        /// <param name="arguments">Arguments of the command</param>
        /// <returns>The command's standard output, or an empty string on failure</returns>
        private static string RunMacOSCommand(string fileName, string arguments)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                using var process = Process.Start(startInfo);
                if (process is null)
                    return string.Empty;

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                return output;
            }
            catch
            {
                return string.Empty;
            }
        }

        #endregion
    }
}
