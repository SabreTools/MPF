using System;
using System.Collections.Generic;
using System.IO;
using SabreTools.IO.Extensions;

namespace MPF.Frontend
{
    // Linux-specific drive enumeration helpers for the Drive class. Kept in a platform
    // partial so macOS (and any future OS) can add its own counterpart (e.g. Drive.MacOS.cs).
    public partial class Drive
    {
        #region Fields

        /// <summary>
        /// sysfs block-device name prefixes that the fixed-drive enumerator does not surface:
        /// loopback images, ramdisks, device-mapper and software RAID virtual devices, network
        /// block devices, ZFS volumes, and the floppy and optical drives (each surfaced
        /// separately by its own enumerator rather than as a fixed or removable disk).
        /// </summary>
        private static readonly string[] _excludedUnixBlockPrefixes =
        [
            "dm-",  // device-mapper (LVM/LUKS, etc.) virtual devices
            "fd",   // floppy drive (surfaced separately by the floppy enumerator)
            "loop", // loopback-mounted image
            "md",   // software RAID (mdadm)
            "nbd",  // network block device
            "ram",  // ramdisk
            "sr",   // optical drive (surfaced separately by the optical enumerator)
            "zd",   // ZFS volume (zvol)
            "zram", // compressed ramdisk
        ];

        #endregion

        #region Linux Helpers

        /// <summary>
        /// Append Linux optical devices that DriveInfo did not surface.
        /// DriveInfo.GetDrives() returns mount points and typically does not list unmounted
        /// optical drives, so they are enumerated directly to let users dump discs without
        /// mounting them first. Both the block nodes (/dev/sr*) and their generic SCSI
        /// counterparts (/dev/sg*) are surfaced, block nodes first, because dumping back ends
        /// differ in which node they expect (e.g. redumper opens the generic node).
        /// </summary>
        /// <param name="drives">Drives already discovered via DriveInfo</param>
        /// <returns>The drive array, extended with any optical devices not already present</returns>
        private static Drive[] AppendUnixOpticalDrives(Drive[] drives)
        {
            // Defensive: this reads Linux /dev and sysfs paths, so never run it off-Unix
            if (Environment.OSVersion.Platform != PlatformID.Unix)
                return drives;

            var existingNames = new HashSet<string>();
            foreach (var d in drives)
            {
                if (d?.DevicePath is not null)
                    existingNames.Add(d.DevicePath);
            }

            // Block nodes first, then their generic SCSI counterparts
            var devicePaths = new List<string>();
            devicePaths.AddRange(IOExtensions.EnumerateUnixOpticalBlockPaths("/dev"));
            devicePaths.AddRange(IOExtensions.EnumerateUnixOpticalGenericPaths("/dev", "/sys/class/scsi_generic"));

            var extra = new List<Drive>();
            foreach (var devicePath in devicePaths)
            {
                // Skip paths already surfaced by DriveInfo or an earlier enumerator
                if (!existingNames.Add(devicePath))
                    continue;

                try
                {
                    var d = Create(Frontend.InternalDriveType.Optical, devicePath);
                    if (d is not null)
                    {
                        // A Linux optical /dev node is never a mount point, so DriveInfo always
                        // reports it as not-ready. Mark it active so it matches how Windows
                        // surfaces optical drives; whether a disc is actually loaded is left to
                        // the dumping program rather than probing the device here.
                        d.MarkedActive = true;
                        extra.Add(d);
                    }
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
        /// Append Linux floppy drives that DriveInfo did not surface. Two kinds are covered:
        /// legacy on-board floppy nodes (/dev/fd0, /dev/fd1, ...) and USB floppy drives, which
        /// the kernel exposes as ordinary SCSI disks (/dev/sd*). Neither is ever a mount point,
        /// so DriveInfo does not list them; they are enumerated directly so users can dump disks
        /// without mounting them first. Like optical drives, floppy drives are surfaced
        /// regardless of the fixed-drive toggle, mirroring how Windows tags floppy media
        /// (Win32_LogicalDisk.MediaType) in MarkWindowsFloppyDrives.
        /// </summary>
        /// <param name="drives">Drives already discovered via DriveInfo</param>
        /// <returns>The drive array, extended with any floppy drives not already present</returns>
        private static Drive[] AppendUnixFloppyDrives(Drive[] drives)
        {
            // Defensive: this reads Linux /dev paths, so never run it off-Unix
            if (Environment.OSVersion.Platform != PlatformID.Unix)
                return drives;

            var existingNames = new HashSet<string>();
            foreach (var d in drives)
            {
                if (d?.DevicePath is not null)
                    existingNames.Add(d.DevicePath);
            }

            // Legacy /dev/fd* nodes plus USB floppy drives exposed as SCSI disks (/dev/sd*)
            var devicePaths = new List<string>();
            devicePaths.AddRange(IOExtensions.EnumerateUnixFloppyBlockPaths("/dev"));
            devicePaths.AddRange(IOExtensions.EnumerateUnixUsbFloppyBlockPaths("/sys/block", "/dev"));

            var extra = new List<Drive>();
            foreach (var devicePath in devicePaths)
            {
                // Skip paths already surfaced by DriveInfo or an earlier enumerator
                if (!existingNames.Add(devicePath))
                    continue;

                try
                {
                    var d = Create(Frontend.InternalDriveType.Floppy, devicePath);
                    if (d != null)
                    {
                        // A Linux floppy /dev node is never a mount point, so DriveInfo always
                        // reports it as not-ready. Mark it active like optical drives; whether a
                        // disk is actually inserted is left to the dumping program rather than
                        // probing the device here, since floppy media state is not reliably
                        // exposed through sysfs.
                        d.MarkedActive = true;
                        extra.Add(d);
                    }
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
        /// Append Linux fixed and removable block devices that DriveInfo did not surface.
        /// On Unix, DriveInfo.GetDrives() only reports mount points (e.g. "/"), not the raw
        /// device nodes (/dev/sda, /dev/nvme0n1, ...) needed for dumping, so whole-disk
        /// block devices are enumerated directly from sysfs instead. Enumeration relies only
        /// on world-readable sysfs and does not require elevated privileges; only the later
        /// raw read during a dump does.
        /// </summary>
        /// <param name="drives">Drives already discovered via DriveInfo</param>
        /// <returns>The drive array, extended with any block devices not already present</returns>
        private static Drive[] AppendUnixFixedDrives(Drive[] drives)
        {
            // Defensive: this reads Linux /dev and sysfs paths, so never run it off-Unix
            if (Environment.OSVersion.Platform != PlatformID.Unix)
                return drives;

            var existingNames = new HashSet<string>();
            foreach (var d in drives)
            {
                if (d?.DevicePath is not null)
                    existingNames.Add(d.DevicePath);
            }

            var extra = new List<Drive>();
            foreach (var device in EnumerateUnixFixedDevices("/sys/block", "/dev"))
            {
                // Skip paths already surfaced by DriveInfo or an earlier enumerator
                if (!existingNames.Add(device.DevicePath!))
                    continue;

                try
                {
                    var d = Create(device.InternalDriveType, device.DevicePath!);
                    if (d is not null)
                    {
                        // A raw /dev block node is never a mount point, so DriveInfo reports
                        // it as not-ready with no size. Mirror how Windows surfaces these: a
                        // fixed disk is always ready, while a removable drive is only ready
                        // when media is present (sysfs reports a non-zero size).
                        d.TotalSize = device.TotalSize;
                        d.MarkedActive = device.InternalDriveType == Frontend.InternalDriveType.HardDisk
                            || device.TotalSize > 0;
                        extra.Add(d);
                    }
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
        /// Enumerate Linux fixed and removable block devices from sysfs. Each entry under
        /// <paramref name="sysBlockRoot"/> is a whole device (e.g. "sda", "nvme0n1");
        /// partitions are nested and therefore not listed here. Virtual and non-target
        /// devices (loopback, ramdisks, device-mapper, software RAID, optical, ...) are
        /// skipped, and the sysfs "removable" flag selects between a fixed hard disk and a
        /// removable drive.
        /// </summary>
        /// <param name="sysBlockRoot">sysfs block directory (typically "/sys/block")</param>
        /// <param name="devRoot">Root directory device nodes live under (typically "/dev")</param>
        /// <returns>Discovered devices, or an empty list when the directory is unreadable</returns>
        internal static List<Drive> EnumerateUnixFixedDevices(string sysBlockRoot, string devRoot)
        {
            var result = new List<Drive>();
            if (string.IsNullOrEmpty(sysBlockRoot) || string.IsNullOrEmpty(devRoot))
                return result;
            if (!Directory.Exists(sysBlockRoot))
                return result;

            string[] entries;
            try
            {
                entries = Directory.GetFileSystemEntries(sysBlockRoot);
            }
            catch
            {
                return result;
            }

            foreach (var entry in entries)
            {
                string name = Path.GetFileName(entry);
                if (IsExcludedUnixBlockDevice(name))
                    continue;

                var driveType = ReadUnixRemovableFlag(entry)
                    ? Frontend.InternalDriveType.Removable
                    : Frontend.InternalDriveType.HardDisk;

                long totalSize = ReadUnixBlockDeviceSize(entry);

                result.Add(new Drive()
                {
                    InternalDriveType = driveType,
                    DevicePath = Path.Combine(devRoot, name),
                    TotalSize = totalSize,
                });
            }

            return result;
        }

        /// <summary>
        /// Check whether a sysfs block-device name is a virtual or non-target device that
        /// should not be surfaced as a dump target.
        /// </summary>
        /// <param name="name">Block-device name (e.g. "sda", "loop0")</param>
        /// <returns>True when the device should be skipped</returns>
        private static bool IsExcludedUnixBlockDevice(string name)
        {
            if (string.IsNullOrEmpty(name))
                return true;

            foreach (var prefix in _excludedUnixBlockPrefixes)
            {
                if (name.StartsWith(prefix, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Read the sysfs "removable" flag for a block device.
        /// </summary>
        /// <param name="sysBlockEntry">Path to the device directory under the sysfs block root</param>
        /// <returns>True when the device reports itself as removable; false otherwise</returns>
        /// TODO: Remove when IO is updated
        private static bool ReadUnixRemovableFlag(string sysBlockEntry)
        {
            string removablePath = Path.Combine(sysBlockEntry, "removable");
            try
            {
                if (!File.Exists(removablePath))
                    return false;

                return File.ReadAllText(removablePath).Trim() == "1";
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Read the device size from sysfs (reported in 512-byte sectors) and convert to bytes.
        /// </summary>
        /// <param name="sysBlockEntry">Path to the device directory under the sysfs block root</param>
        /// <returns>The device size in bytes, or 0 when it cannot be determined</returns>
        /// TODO: Remove when IO is updated
        private static long ReadUnixBlockDeviceSize(string sysBlockEntry)
        {
            string sizePath = Path.Combine(sysBlockEntry, "size");
            try
            {
                if (!File.Exists(sizePath))
                    return 0;

                if (long.TryParse(File.ReadAllText(sizePath).Trim(), out long sectors) && sectors > 0)
                    return sectors * 512L;
            }
            catch
            {
                // Unreadable size; report it as unknown
            }

            return 0;
        }

        #endregion
    }
}
