using System;
using System.Collections.Generic;
using System.IO;
using SabreTools.IO.Extensions;

namespace MPF.Frontend
{
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
            // This should only run on Linux
            if (!SystemEnvironment.IsLinux())
                return drives;

            var existingNames = new HashSet<string>();
            foreach (var drive in drives)
            {
                if (drive.DevicePath is not null)
                    existingNames.Add(drive.DevicePath);
            }

            // Block nodes first, then their generic SCSI counterparts
            List<string> devicePaths =
            [
                .. IOExtensions.EnumerateUnixOpticalBlockPaths("/dev"),
                .. IOExtensions.EnumerateUnixOpticalGenericPaths("/dev", "/sys/class/scsi_generic"),
            ];

            var extra = new List<Drive>();
            foreach (var devicePath in devicePaths)
            {
                // Skip paths already surfaced by DriveInfo or an earlier enumerator
                if (!existingNames.Add(devicePath))
                    continue;

                // Mark the drive as active unless udev saw no disc in it, like a drive that is not ready on Windows
                var drive = Create(Frontend.InternalDriveType.Optical, devicePath);
                if (drive is not null)
                {
                    drive.MarkedActive = MayContainUnixDisc(devicePath);
                    extra.Add(drive);
                }
            }

            return [.. drives, .. extra];
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
            // This should only run on Linux
            if (!SystemEnvironment.IsLinux())
                return drives;

            var existingNames = new HashSet<string>();
            foreach (var drive in drives)
            {
                if (drive.DevicePath is not null)
                    existingNames.Add(drive.DevicePath);
            }

            // Legacy /dev/fd* nodes plus USB floppy drives exposed as SCSI disks (/dev/sd*)
            List<string>? devicePaths =
            [
                .. IOExtensions.EnumerateUnixFloppyBlockPaths("/dev"),
                .. IOExtensions.EnumerateUnixUsbFloppyBlockPaths("/sys/block", "/dev"),
            ];

            var extra = new List<Drive>();
            foreach (var devicePath in devicePaths)
            {
                // Skip paths already surfaced by DriveInfo or an earlier enumerator
                if (!existingNames.Add(devicePath))
                    continue;

                // Force marking the drive as active if it's found
                var drive = Create(Frontend.InternalDriveType.Floppy, devicePath);
                if (drive != null)
                {
                    drive.MarkedActive = true;
                    extra.Add(drive);
                }
            }

            return [.. drives, .. extra];
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
            // This should only run on Linux
            if (!SystemEnvironment.IsLinux())
                return drives;

            var existingNames = new HashSet<string>();
            foreach (var drive in drives)
            {
                if (drive.DevicePath is not null)
                    existingNames.Add(drive.DevicePath);
            }

            var extra = new List<Drive>();
            foreach (var device in EnumerateUnixFixedDevices("/sys/block", "/dev"))
            {
                // Skip paths already surfaced by DriveInfo or an earlier enumerator
                if (!existingNames.Add(device.DevicePath!))
                    continue;

                // Force marking the drive as active if it's HDD or has a non-zero size
                var drive = Create(device.InternalDriveType, device.DevicePath!);
                if (drive is not null)
                {
                    drive.TotalSize = device.TotalSize;
                    drive.MarkedActive = device.InternalDriveType == Frontend.InternalDriveType.HardDisk
                        || device.TotalSize > 0;
                    extra.Add(drive);
                }
            }

            return [.. drives, .. extra];
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
        internal static List<Drive> EnumerateUnixFixedDevices(string? sysBlockRoot, string? devRoot)
        {
            var result = new List<Drive>();
            if (string.IsNullOrEmpty(sysBlockRoot) || string.IsNullOrEmpty(devRoot))
                return result;
            if (!Directory.Exists(sysBlockRoot!))
                return result;

            string[] entries = IOExtensions.SafeGetFileSystemEntries(sysBlockRoot!);
            foreach (var entry in entries)
            {
                string name = Path.GetFileName(entry);
                if (IsExcludedUnixBlockDevice(name))
                    continue;

                var driveType = ReadUnixRemovableFlag(entry)
                    ? Frontend.InternalDriveType.Removable
                    : Frontend.InternalDriveType.HardDisk;

                long totalSize = ReadUnixBlockDeviceSize(entry);

                result.Add(new Drive
                {
                    InternalDriveType = driveType,
                    DevicePath = Path.Combine(devRoot!, name),
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
        /// TODO: Remove when IO is updated
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

        /// <summary>
        /// Get the block device of a SCSI generic device on Linux, since only block devices can be read by sector
        /// </summary>
        /// <returns>Path of the block device, the given path if it is no SCSI generic device</returns>
        internal static string GetUnixBlockDevice(string path)
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
        /// Determine if an optical drive on Linux may contain a disc
        /// </summary>
        /// <param name="devicePath">Block or SCSI generic device path of the drive</param>
        /// <returns>False if udev saw no disc in the drive, true otherwise</returns>
        private static bool MayContainUnixDisc(string devicePath)
            => ReadUnixDiscPresence(GetUnixBlockDevice(devicePath), "/sys/class/block", "/run/udev/data") != false;

        /// <summary>
        /// Read whether udev saw a disc in an optical drive
        /// </summary>
        /// <param name="devicePath">Device path of the drive (e.g. "/dev/sr0")</param>
        /// <param name="sysBlockRoot">Root of the sysfs block devices (normally "/sys/class/block")</param>
        /// <param name="udevDataRoot">Root of the udev database (normally "/run/udev/data")</param>
        /// <returns>True if udev saw a disc, false if it saw none, null if udev did not check the drive</returns>
        /// <remarks>
        /// Opening the device of a drive with an open tray makes the kernel close the tray. udev
        /// checks an optical drive when a disc is inserted or removed and sets ID_CDROM=1 in the
        /// entry of the drive, and ID_CDROM_MEDIA=1 while a disc is in it. Reading the entry does
        /// not access the drive
        /// </remarks>
        internal static bool? ReadUnixDiscPresence(string devicePath, string sysBlockRoot, string udevDataRoot)
        {
            try
            {
                // The entry is named after the major and minor number of the device (e.g. "b11:0")
                string numberPath = Path.Combine(Path.Combine(sysBlockRoot, Path.GetFileName(devicePath)), "dev");
                string[] entry = File.ReadAllLines(Path.Combine(udevDataRoot, $"b{File.ReadAllText(numberPath).Trim()}"));
                if (Array.IndexOf(entry, "E:ID_CDROM=1") < 0)
                    return null;

                return Array.IndexOf(entry, "E:ID_CDROM_MEDIA=1") >= 0;
            }
            catch
            {
                // Without a udev entry for the drive, nothing is known about the disc
                return null;
            }
        }

        #endregion
    }
}
