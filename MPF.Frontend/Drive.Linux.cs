using System.Collections.Generic;
using System.IO;
using SabreTools.IO.Extensions;

namespace MPF.Frontend
{
    public partial class Drive
    {
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
                .. DeviceExtensions.EnumerateUnixOpticalBlockPaths(),
                .. DeviceExtensions.EnumerateUnixOpticalGenericPaths(),
            ];

            var extra = new List<Drive>();
            foreach (var devicePath in devicePaths)
            {
                // Skip paths already surfaced by DriveInfo or an earlier enumerator
                if (!existingNames.Add(devicePath))
                    continue;

                // Force marking the drive as active if it's found
                var drive = Create(Frontend.InternalDriveType.Optical, devicePath);
                if (drive is not null)
                {
                    drive.MarkedActive = true;
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
                .. DeviceExtensions.EnumerateUnixFloppyBlockPaths(),
                .. DeviceExtensions.EnumerateUnixUsbFloppyBlockPaths(),
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
                if (DeviceExtensions.IsExcludedUnixBlockDevice(name))
                    continue;

                var driveType = DeviceExtensions.ReadUnixRemovableFlag(entry)
                    ? Frontend.InternalDriveType.Removable
                    : Frontend.InternalDriveType.HardDisk;

                long totalSize = DeviceExtensions.ReadUnixBlockDeviceSize(entry);

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
            string? label = DeviceExtensions.GetLabelGeneric(DevicePath);
            if (label is not null)
            {
                VolumeLabel = label;
                return;
            }

            // Try to process the drive as a block device
            label = DeviceExtensions.GetLabelLink(DevicePath);
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
            string? mountPoint = DeviceExtensions.GetMountPointGeneric(DevicePath);
            if (mountPoint is not null)
            {
                MountedPath = mountPoint;
                return;
            }

            // Try to process the drive as a block device
            mountPoint = DeviceExtensions.GetMountPointMounts(DevicePath);
            if (mountPoint is not null)
                MountedPath = mountPoint;
        }

        #endregion
    }
}
