using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Microsoft.Win32.SafeHandles;
using MPF.Frontend.FileSystems;
using SabreTools.IO.Extensions;

namespace MPF.Frontend
{
    public partial class Drive
    {
        #region Fields

        /// <summary>
        /// Flags to open a device on Linux for reading without waiting for a disc (O_RDONLY | O_NONBLOCK | O_CLOEXEC)
        /// </summary>
        private const int UnixOpenReadNonBlocking = 0x0000 | 0x0800 | 0x80000;

        /// <summary>
        /// Advice that a file on Linux is read in random order (POSIX_FADV_RANDOM)
        /// </summary>
        private const int UnixAdviceRandom = 1;

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
                    drive.PopulateFromUnixDevice(devicePath);
                    extra.Add(drive);
                }
            }

            return [.. drives, .. extra];
        }

        /// <summary>
        /// Populate the file system and size of the disc in an optical drive on Linux, as Windows reports them
        /// </summary>
        /// <param name="devicePath">Block or SCSI generic device path of the drive</param>
        /// <remarks>DriveInfo does not report them, since the disc is not mounted</remarks>
        private void PopulateFromUnixDevice(string devicePath)
        {
            // This should only run on Linux
            if (!SystemEnvironment.IsLinux())
                return;

            using var files = ImageFileSystem.Create(devicePath);
            DriveFormat = GetWindowsDriveFormat(files?.FileSystemName);
            TotalSize = files?.VolumeSize ?? default;
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

                var drive = new Drive
                {
                    InternalDriveType = driveType,
                    DevicePath = Path.Combine(devRoot!, name),
                    MountedPath = Path.Combine(devRoot, name),
                    TotalSize = totalSize,
                };
                drive.SetMountPoint();
#if NET6_0_OR_GREATER
                drive.SetLabel();
#endif

                result.Add(drive);
            }

            return result;
        }

#if NET6_0_OR_GREATER
        /// <summary>
        /// Set the label as defined in /dev/disk/by-label/ or from generic SCSI
        /// </summary>
        private void SetLabel()
        {
            // Try to process the drive as generic SCSI
            string? label = GetLabelGeneric(DevicePath);
            if (label is not null)
            {
                VolumeLabel = label;
                return;
            }

            // Try to process the drive as a block device
            label = GetLabelLink(DevicePath);
            if (label is not null)
                VolumeLabel = label;
        }
#endif

        /// <summary>
        /// Set the mount point as defined in /proc/mounts or from generic SCSI
        /// </summary>
        private void SetMountPoint()
        {
            // Try to process the drive as generic SCSI
            string? mountPoint = GetMountPointGeneric(DevicePath);
            if (mountPoint is not null)
            {
                MountedPath = mountPoint;
                return;
            }

            // Try to process the drive as a block device
            mountPoint = GetMountPointMounts(DevicePath);
            if (mountPoint is not null)
                MountedPath = mountPoint;
        }

        #region TODO: Remove when IO is updated

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

#if NET6_0_OR_GREATER
        /// <summary>
        /// Get the label based on the block device mapping for generic SCSI
        /// </summary>
        /// <param name="devicePath">Device path to check</param>
        /// <returns>Label for the generic device on success, null otherwise</returns>
        private static string? GetLabelGeneric(string? devicePath)
        {
            // If the device path is invalid
            if (devicePath is null)
                return null;

            // Get the mapped device
            var mappedDevice = GetMappedDeviceForGeneric(devicePath);
            if (mappedDevice is null)
                return null;

            // Return the label for that device
            return GetLabelLink(mappedDevice);
        }

        /// <summary>
        /// Get the label based on the softlinks in /dev/disk/by-label/
        /// </summary>
        /// <param name="devicePath">Device path to check</param>
        /// <returns>Label for the device on success, null otherwise</returns>
        private static string? GetLabelLink(string? devicePath)
        {
            // If the device path is invalid
            if (devicePath is null)
                return null;

            // Try looping through the by-label directory
            string byLabelPath = "/dev/disk/by-label/";
            try
            {
                if (!Directory.Exists(byLabelPath))
                    return null;

                var entries = Directory.GetFileSystemEntries("/dev/disk/by-label/");
                foreach (var entry in entries)
                {
                    // Ensure the entry is a file
                    if (!File.Exists(entry))
                        continue;

                    // Extract the link target from the file
                    var linkTarget = new FileInfo(entry).LinkTarget;
                    if (linkTarget is null)
                        continue;

                    // Reformat the link target based on relative paths
                    linkTarget = linkTarget.Replace("../../", "/dev/");

                    // If the drive does not match
                    if (!string.Equals(devicePath, linkTarget))
                        continue;

                    // Return the mount point
                    return Path.GetFileName(entry)
                        .Replace("\\x20", " ") // Replace hex-encoded space characters
                        .Replace("\\020", " "); // Replace octal-encoded space characters
                }

                // No matches were found
                return null;
            }
            catch
            {
                // Absorb errors for now
                return null;
            }
        }
#endif

        /// <summary>
        /// Get the mount point based on the block device mapping for generic SCSI
        /// </summary>
        /// <param name="devicePath">Device path to check</param>
        /// <returns>Mount point for the generic device on success, null otherwise</returns>
        private static string? GetMountPointGeneric(string? devicePath)
        {
            // If the device path is invalid
            if (devicePath is null)
                return null;

            // Get the mapped device
            var mappedDevice = GetMappedDeviceForGeneric(devicePath);
            if (mappedDevice is null)
                return null;

            // Return the mount point for that device
            return GetMountPointMounts(mappedDevice);
        }

        /// <summary>
        /// Get the mount point based a mapping to /proc/mounts
        /// </summary>
        /// <param name="devicePath">Device path to check</param>
        /// <returns>Mount point for the device on success, null otherwise</returns>
        private static string? GetMountPointMounts(string? devicePath)
        {
            // If the device path is invalid
            if (devicePath is null)
                return null;

            // Try reading the mounts file for information
            string mountsPath = "/proc/mounts";
            try
            {
                if (!File.Exists(mountsPath))
                    return null;

                // Read all lines from the mounts file
                string[] mountsLines = File.ReadAllLines(mountsPath);
                foreach (string mount in mountsLines)
                {
                    // Split the whitespace-separated mount entry line
#if NETFRAMEWORK || NETSTANDARD
                    string[] entry = mount.Split(' ');
#else
                    string[] entry = mount.Split(' ', StringSplitOptions.RemoveEmptyEntries);
#endif
                    if (entry.Length < 2)
                        continue;

                    // Extract the two required pieces
                    string entryDevicePath = entry[0];
                    string entryMountPoint = entry[1];

                    // If the drive does not match
                    if (!string.Equals(devicePath, entryDevicePath))
                        continue;

                    // Return the mount point
                    return entryMountPoint;
                }

                // No matches were found
                return null;
            }
            catch
            {
                // Absorb errors for now
                return null;
            }
        }

        /// <summary>
        /// Read the device size from sysfs (reported in 512-byte sectors) and convert to bytes
        /// </summary>
        /// <param name="sysBlockEntry">Path to the device directory under the sysfs block root</param>
        /// <returns>The device size in bytes, 0 on error</returns>
        private static long ReadUnixBlockDeviceSize(string sysBlockEntry)
        {
            string sizePath = Path.Combine(sysBlockEntry, "size");
            try
            {
                if (!File.Exists(sizePath))
                    return 0;

                string sizeString = File.ReadAllText(sizePath).Trim();
                if (long.TryParse(sizeString, out long sectors) && sectors > 0)
                    return sectors * 512;
            }
            catch
            {
                // Unreadable size; report it as unknown
            }

            return 0;
        }

        /// <summary>
        /// Read the sysfs "removable" flag for a block device
        /// </summary>
        /// <param name="sysBlockEntry">Path to the device directory under the sysfs block root</param>
        /// <returns>True when the device reports itself as removable, false otherwise</returns>
        private static bool ReadUnixRemovableFlag(string sysBlockEntry)
        {
            string removablePath = Path.Combine(sysBlockEntry, "removable");
            try
            {
                if (!File.Exists(removablePath))
                    return false;

                string removableString = File.ReadAllText(removablePath).Trim();
                return removableString == "1";
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Map a generic (/dev/sg*) device to its corresponding block (/dev/sr*) device
        /// </summary>
        /// <param name="devicePath">Device path to check</param>
        /// <returns>The mapped device path on success, null otherwise</returns>
        private static string? GetMappedDeviceForGeneric(string? devicePath)
        {
            // If the device path is invalid
            if (devicePath is null)
                return null;

#pragma warning disable SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.
            // If the device does not match the generic pattern
            if (!Regex.IsMatch(devicePath, @"/dev/sg.+"))
                return null;
#pragma warning restore SYSLIB1045 // Convert to 'GeneratedRegexAttribute'.

            // Get the SCSI name from the path by trimming `/dev/`
#if NETFRAMEWORK || NETSTANDARD
            string scsiName = devicePath.Substring(5);
#else
            string scsiName = devicePath[5..];
#endif

            // Build the path to the block directory
            string blockDirectory = $"/sys/class/scsi_generic/{scsiName}/device/block/";
            if (!Directory.Exists(blockDirectory))
                return null;

            // If there are no block mappings
            var blockDevices = IOExtensions.SafeGetDirectories(blockDirectory);
            if (blockDevices.Length == 0)
                return null;

            // Take the first found block mapping and reformat to `/dev/sr*`
            string mappedDevice = blockDevices[0];
#if NETFRAMEWORK || NETSTANDARD
            mappedDevice = mappedDevice.Substring($"/sys/class/scsi_generic/{scsiName}/device/block/".Length);
#else
            mappedDevice = mappedDevice[$"/sys/class/scsi_generic/{scsiName}/device/block/".Length..];
#endif
            return $"/dev/{mappedDevice}";
        }

        #endregion

        /// <summary>
        /// Get the block device of a SCSI generic device on Linux, since only block devices can be read by sector
        /// </summary>
        /// <returns>Path of the block device, the given path if it is no SCSI generic device</returns>
        internal static string GetUnixBlockDevice(string path)
            => GetMappedDeviceForGeneric(path) ?? path;

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

        /// <summary>
        /// Open the block device of an optical drive on Linux for reading
        /// </summary>
        /// <param name="devicePath">Block device path of the drive (e.g. "/dev/sr0")</param>
        /// <returns>Handle of the device</returns>
        /// <remarks>
        /// Opening the device of a drive with an open tray makes the kernel close the tray, also
        /// in the seconds before udev noticed that the tray was opened. With O_NONBLOCK, the tray
        /// stays open and reads fail while there is no disc
        /// </remarks>
        internal static SafeFileHandle OpenUnixDevice(string devicePath)
        {
            int descriptor = open(devicePath, UnixOpenReadNonBlocking);
            if (descriptor < 0)
                throw new IOException($"Could not open {devicePath}, error {Marshal.GetLastWin32Error()}");

            // Random access avoids reading ahead into sectors that are slow or impossible to read
            var handle = new SafeFileHandle(new IntPtr(descriptor), true);
            posix_fadvise(descriptor, 0, 0, UnixAdviceRandom);
            return handle;
        }

        #endregion

        #region Native Interop

        [DllImport("libc", SetLastError = true)]
        private static extern int open(string pathname, int flags);

        [DllImport("libc")]
        private static extern int posix_fadvise(int fd, long offset, long len, int advice);

        #endregion
    }
}
