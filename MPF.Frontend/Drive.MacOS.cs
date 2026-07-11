using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
#if NETCOREAPP
using System.Runtime.InteropServices;
#endif

namespace MPF.Frontend
{
    // macOS-specific drive enumeration helpers for the Drive class. Kept in a platform partial
    // alongside Drive.Linux.cs. Where Linux reads per-category sysfs trees, macOS exposes all
    // storage through the diskutil command-line tool, so a single sweep classifies fixed,
    // removable, floppy, and optical devices from one query.
    public partial class Drive
    {
        #region macOS Detection

        /// <summary>
        /// Whether the current process is running on macOS. On modern .NET,
        /// Environment.OSVersion.Platform reports PlatformID.Unix for both Linux and macOS, so
        /// the two are distinguished here rather than through PlatformID. .NET Framework never
        /// runs on macOS, so this is always false on those targets.
        /// </summary>
        /// <returns>True when running on macOS; false otherwise</returns>
        private static bool IsMacOS()
        {
#if NETCOREAPP
            return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
#else
            return false;
#endif
        }

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
        /// <returns>The drive array, extended with any devices not already present</returns>
        private static Drive[] AppendMacOSDrives(Drive[] drives, bool ignoreFixedDrives)
        {
            // Defensive: this shells out to macOS-only tooling, so never run it off macOS
            if (!IsMacOS())
                return drives;

            var existingNames = new HashSet<string>();
            foreach (var d in drives)
            {
                if (d?.Name is not null)
                    existingNames.Add(d.Name);
            }

            var extra = new List<Drive>();
            foreach (var device in EnumerateMacOSDevices())
            {
                // Optical drives are always surfaced; everything else follows the toggle
                if (device.DriveType != Frontend.InternalDriveType.Optical && ignoreFixedDrives)
                    continue;

                // Skip names already surfaced by DriveInfo or an earlier device
                if (!existingNames.Add(device.BsdName))
                    continue;

                try
                {
                    var d = CreateMacOSDrive(device);
                    if (d is not null)
                        extra.Add(d);
                }
                catch
                {
                    // Skip devices that can't be opened
                }
            }

            if (extra.Count == 0)
                return drives;

            var combined = new Drive[drives.Length + extra.Count];
            Array.Copy(drives, combined, drives.Length);
            extra.CopyTo(combined, drives.Length);
            return combined;
        }

        /// <summary>
        /// Build a Drive from a macOS device, mirroring how the Linux enumerator marks activity.
        /// The drive is named by its BSD name rather than its device node: that is the form a
        /// macOS dumping program expects, the same way the name is a drive letter on Windows and
        /// a device node on Linux. Redumper resolves it against the BSD name IOKit publishes for
        /// the drive and rejects a "/dev/"-prefixed path.
        /// </summary>
        /// <param name="device">Device discovered via diskutil</param>
        /// <returns>The populated Drive, or null when it cannot be created</returns>
        private static Drive? CreateMacOSDrive(MacOSBlockDevice device)
        {
            var d = Create(device.DriveType, device.BsdName);
            if (d is null)
                return null;

            // A BSD name is never a mount point, so DriveInfo reports it as not-ready with no
            // size. Mirror how Windows and the Linux enumerator surface these: optical, floppy,
            // and fixed drives are always ready, while a removable drive is only ready when
            // media is present (a non-zero size).
            d.TotalSize = device.TotalSize;
            d.MarkedActive = device.DriveType != Frontend.InternalDriveType.Removable || device.TotalSize > 0;
            return d;
        }

        /// <summary>
        /// Enumerate macOS whole-disk devices via diskutil. Each whole-disk node under
        /// <paramref name="devRoot"/> (e.g. "/dev/disk0") is queried with "diskutil info" and
        /// classified; partitions (e.g. "disk0s1") and the raw variants ("/dev/rdisk*") are not
        /// whole-device targets and are excluded by the node filter. Synthesized volumes and
        /// disk images are dropped during parsing.
        /// </summary>
        /// <returns>Discovered physical devices, or an empty list when none are found</returns>
        internal static List<MacOSBlockDevice> EnumerateMacOSDevices()
        {
            var result = new List<MacOSBlockDevice>();
            foreach (var node in EnumerateMacOSWholeDiskNodes("/dev"))
            {
                string info = RunDiskutilInfo(Path.GetFileName(node));
                if (string.IsNullOrEmpty(info))
                    continue;

                var device = ParseMacOSDiskutilInfo(info, node);
                if (device is not null)
                    result.Add(device);
            }

            return result;
        }

        /// <summary>
        /// Enumerate macOS whole-disk device nodes under a directory by matching the kernel
        /// convention "disk" followed by one or more digits (disk0, disk1, ...). This rejects
        /// partition nodes ("disk0s1", which contain a non-digit slice suffix) and the raw
        /// character nodes ("rdisk0", which do not start with "disk").
        /// </summary>
        /// <param name="devRoot">Root directory to scan (typically "/dev")</param>
        /// <returns>Device node paths, or an empty list when the directory is unreadable</returns>
        internal static List<string> EnumerateMacOSWholeDiskNodes(string devRoot)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(devRoot) || !Directory.Exists(devRoot))
                return result;

            string[] candidates;
            try
            {
                candidates = Directory.GetFiles(devRoot, "disk*");
            }
            catch
            {
                return result;
            }

            foreach (var path in candidates)
            {
                if (HasDeviceIndexSuffix(Path.GetFileName(path), "disk"))
                    result.Add(path);
            }

            return result;
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
        internal static MacOSBlockDevice? ParseMacOSDiskutilInfo(string diskutilInfoOutput, string devicePath)
        {
            if (string.IsNullOrEmpty(diskutilInfoOutput))
                return null;

            var fields = ParseDiskutilFields(diskutilInfoOutput);

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

            // The BSD name is what a macOS dumping program takes for a drive, so it becomes the
            // drive name. diskutil reports it directly; the node's file name is the same string.
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
            else if (removable && _unixFloppyMediaSizes.Contains(size))
                driveType = Frontend.InternalDriveType.Floppy;
            else if (removable)
                driveType = Frontend.InternalDriveType.Removable;
            else
                driveType = Frontend.InternalDriveType.HardDisk;

            return new MacOSBlockDevice(node, bsdName, driveType, size);
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
        /// Run "diskutil info &lt;deviceName&gt;" and return its standard output, or an empty
        /// string on any failure. The device name comes from the internal node enumeration, so
        /// it is a trusted "diskN" token rather than external input.
        /// </summary>
        /// <param name="deviceName">Whole-disk identifier (e.g. "disk0")</param>
        /// <returns>The command's standard output, or an empty string on failure</returns>
        private static string RunDiskutilInfo(string deviceName)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "diskutil",
                    Arguments = "info " + deviceName,
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
