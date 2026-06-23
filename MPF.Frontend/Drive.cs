using System;
using System.Collections.Generic;
using System.IO;
#if NET462_OR_GREATER || NETCOREAPP
using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Generic;
#endif
using SabreTools.RedumpLib.Data;

namespace MPF.Frontend
{
    /// <summary>
    /// Represents information for a single drive
    /// </summary>
    /// <remarks>
    /// TODO: Can the Aaru models be used instead of the ones I've created here?
    /// </remarks>
    public class Drive
    {
        #region Fields

        /// <summary>
        /// Represents drive type
        /// </summary>
        public InternalDriveType? InternalDriveType { get; set; }

        /// <summary>
        /// Drive partition format
        /// </summary>
        public string? DriveFormat { get; private set; } = null;

        /// <summary>
        /// Windows drive path
        /// </summary>
        public string? Name { get; private set; } = null;

        /// <summary>
        /// Represents if Windows has marked the drive as active
        /// </summary>
        public bool MarkedActive { get; private set; } = false;

        /// <summary>
        /// Represents the total size of the drive
        /// </summary>
        public long TotalSize { get; private set; } = default;

        /// <summary>
        /// Media label as read by Windows
        /// </summary>
        /// <remarks>The try/catch is needed because Windows will throw an exception if the drive is not marked as active</remarks>
        public string? VolumeLabel { get; private set; } = null;

        #endregion

        #region Derived Fields

        /// <summary>
        /// Read-only access to the drive letter
        /// </summary>
        /// <remarks>Should only be used in UI applications</remarks>
        public char? Letter => Name?[0] ?? '\0';

        /// <summary>
        /// UI-friendly display name for the drive, cached after the first access
        /// </summary>
        public string DisplayName
        {
            get
            {
                if (field is not null)
                    return field;

                if (string.IsNullOrEmpty(Name))
                    return field = string.Empty;

                // Deal with non-Windows drive names
                if (Environment.OSVersion.Platform == PlatformID.Unix)
                {
                    string volumePath = Name!.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                    string volumeName = Path.GetFileName(volumePath);
                    if (!string.IsNullOrEmpty(volumeName))
                        return field = volumeName;
                }

                // Trim any trailing separators, falling back to the raw name if nothing remains
                string displayName = Name!.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                return field = string.IsNullOrEmpty(displayName) ? Name! : displayName;
            }
        }

        #endregion

        /// <summary>
        /// Protected constructor
        /// </summary>
        protected Drive() { }

        /// <summary>
        /// Create a new Drive object from a drive type and device path
        /// </summary>
        /// <param name="driveType">InternalDriveType value representing the drive type</param>
        /// <param name="devicePath">Path to the device according to the local machine</param>
        public static Drive? Create(InternalDriveType? driveType, string devicePath)
        {
            // Create a new, empty drive object
            var drive = new Drive()
            {
                InternalDriveType = driveType,
            };

            // If we have an invalid device path, return null
            if (string.IsNullOrEmpty(devicePath))
                return null;

            // Sanitize a Windows-formatted long device path
            if (devicePath.StartsWith("\\\\.\\"))
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
                devicePath = devicePath["\\\\.\\".Length..];
#else
                devicePath = devicePath.Substring("\\\\.\\".Length);
#endif

            // Create and validate the drive info object
            var driveInfo = new DriveInfo(devicePath);
            if (driveInfo is null || driveInfo == default)
                return null;

            // Fill in the rest of the data
            drive.PopulateFromDriveInfo(driveInfo);

            return drive;
        }

        /// <summary>
        /// Populate all fields from a DriveInfo object
        /// </summary>
        /// <param name="driveInfo">DriveInfo object to populate from</param>
        private void PopulateFromDriveInfo(DriveInfo? driveInfo)
        {
            // If we have an invalid DriveInfo, just return
            if (driveInfo is null || driveInfo == default)
                return;

            // Populate the data fields
            Name = driveInfo.Name;
            MarkedActive = driveInfo.IsReady;
            if (MarkedActive)
            {
                DriveFormat = driveInfo.DriveFormat;
                TotalSize = driveInfo.TotalSize;

                // DriveInfo.VolumeLabel is only available on Windows
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    VolumeLabel = driveInfo.VolumeLabel;
                else
                    VolumeLabel = string.Empty;
            }
            else
            {
                DriveFormat = string.Empty;
                TotalSize = default;
                VolumeLabel = string.Empty;
            }
        }

        #region Public Functionality

        /// <summary>
        /// Create a list of active drives matched to their volume labels
        /// </summary>
        /// <param name="ignoreFixedDrives">True to ignore fixed drives from population, false otherwise</param>
        /// <returns>Active drives, matched to labels, if possible</returns>
        public static List<Drive> CreateListOfDrives(bool ignoreFixedDrives)
        {
            var drives = GetDriveList(ignoreFixedDrives);
            drives.Sort((d1, d2) => CompareDrivesForDisplay(d1?.Name, d2?.Name));
            return [.. drives];
        }

        /// <summary>
        /// Get the current media type from drive letter
        /// </summary>
        /// <param name="system">Currently selected system</param>
        /// <returns>The detected media type, if possible</returns>
        public PhysicalMediaType? GetPhysicalMediaType(PhysicalSystem? system)
        {
#pragma warning disable IDE0010
            // Take care of the non-optical stuff first
            switch (InternalDriveType)
            {
                case Frontend.InternalDriveType.Floppy:
                    return PhysicalMediaType.FloppyDisk;
                case Frontend.InternalDriveType.HardDisk:
                    return PhysicalMediaType.HardDisk;
                case Frontend.InternalDriveType.Removable:
                    return PhysicalMediaType.FlashDrive;
            }

            // Some systems should default to certain media types
            switch (system)
            {
                // CD
                case PhysicalSystem.Panasonic3DOInteractiveMultiplayer:
                case PhysicalSystem.PhilipsCDi:
                case PhysicalSystem.SegaDreamcast:
                case PhysicalSystem.SegaSaturn:
                case PhysicalSystem.SonyPlayStation:
                case PhysicalSystem.VideoCD:
                    return PhysicalMediaType.CDROM;

                // DVD
                case PhysicalSystem.DVDAudio:
                case PhysicalSystem.DVDVideo:
                case PhysicalSystem.MicrosoftXbox:
                case PhysicalSystem.MicrosoftXbox360:
                    return PhysicalMediaType.DVD;

                // HD-DVD
                case PhysicalSystem.HDDVDVideo:
                    return PhysicalMediaType.HDDVD;

                // Blu-ray
                case PhysicalSystem.BDVideo:
                case PhysicalSystem.MicrosoftXboxOne:
                case PhysicalSystem.MicrosoftXboxSeriesXS:
                case PhysicalSystem.SonyPlayStation3:
                case PhysicalSystem.SonyPlayStation4:
                case PhysicalSystem.SonyPlayStation5:
                    return PhysicalMediaType.BluRay;

                // GameCube
                case PhysicalSystem.NintendoGameCube:
                    return PhysicalMediaType.NintendoGameCubeGameDisc;

                // Wii
                case PhysicalSystem.NintendoWii:
                    return PhysicalMediaType.NintendoWiiOpticalDisc;

                // WiiU
                case PhysicalSystem.NintendoWiiU:
                    return PhysicalMediaType.NintendoWiiUOpticalDisc;

                // PSP
                case PhysicalSystem.SonyPlayStationPortable:
                    return PhysicalMediaType.UMD;
            }
#pragma warning restore IDE0010

            // Handle optical media by size and filesystem
            if (TotalSize >= 0 && TotalSize <= 800_000_000 && (DriveFormat == "CDFS" || DriveFormat == "UDF"))
                return PhysicalMediaType.CDROM;
            else if (TotalSize > 800_000_000 && TotalSize <= 8_540_000_000 && (DriveFormat == "CDFS" || DriveFormat == "UDF"))
                return PhysicalMediaType.DVD;
            else if (TotalSize > 8_540_000_000)
                return PhysicalMediaType.BluRay;

            return null;
        }

        /// <summary>
        /// Refresh the current drive information based on path
        /// </summary>
        public void RefreshDrive()
        {
            var driveInfo = Array.Find(DriveInfo.GetDrives(), d => d?.Name == Name);
            PopulateFromDriveInfo(driveInfo);
        }

        /// <inheritdoc/>
        public override string ToString() => DisplayName;

        #endregion

        #region Helpers

        /// <summary>
        /// Get all current attached Drives
        /// </summary>
        /// <param name="ignoreFixedDrives">True to ignore fixed drives from population, false otherwise</param>
        /// <returns>List of drives, null on error</returns>
        /// <remarks>
        /// https://stackoverflow.com/questions/3060796/how-to-distinguish-between-usb-and-floppy-devices?utm_medium=organic&utm_source=google_rich_qa&utm_campaign=google_rich_qa
        /// https://msdn.microsoft.com/en-us/library/aa394173(v=vs.85).aspx
        /// </remarks>
        private static List<Drive> GetDriveList(bool ignoreFixedDrives)
        {
            // On Unix, fixed and removable media are enumerated from sysfs device nodes
            // below; DriveInfo only exposes mount points (e.g. "/"), which are not dumpable
            // devices, so they are deliberately left out of the DriveInfo query there.
            bool isUnix = Environment.OSVersion.Platform == PlatformID.Unix;

            var desiredDriveTypes = new List<DriveType>() { DriveType.CDRom };
            if (!ignoreFixedDrives && !isUnix)
            {
                desiredDriveTypes.Add(DriveType.Fixed);
                desiredDriveTypes.Add(DriveType.Removable);
            }

            // TODO: Reduce reliance on `DriveInfo`
            // https://github.com/aaru-dps/Aaru/blob/5164a154e2145941472f2ee0aeb2eff3338ecbb3/Aaru.Devices/Windows/ListDevices.cs#L66

            // Create an output drive array
            Drive[] drives = [];

            // Get all standard supported drive types
            try
            {
                var filteredDrives = Array.FindAll(DriveInfo.GetDrives(), d => desiredDriveTypes.Contains(d.DriveType));
                drives = Array.ConvertAll(filteredDrives, d => Create(ToInternalDriveType(d.DriveType), d.Name) ?? new Drive());
            }
            catch
            {
                return [.. drives];
            }

            // Apply OS-specific drive adjustments
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                MarkWindowsFloppyDrives(drives);
            else if (isUnix)
            {
                drives = AppendUnixOpticalDrives(drives);
                if (!ignoreFixedDrives)
                    drives = AppendUnixFixedDrives(drives);
            }

            return [.. drives];
        }

        /// <summary>
        /// Mark drives that Windows reports as floppy media via WMI.
        /// </summary>
        /// <param name="drives">Drives to update in place</param>
        private static void MarkWindowsFloppyDrives(Drive[] drives)
        {
#if NET462_OR_GREATER || NETCOREAPP
            try
            {
                CimSession session = CimSession.Create(null);
                var collection = session.QueryInstances("root\\CIMV2", "WQL", "SELECT * FROM Win32_LogicalDisk");

                foreach (CimInstance instance in collection)
                {
                    CimKeyedCollection<CimProperty> properties = instance.CimInstanceProperties;
                    uint? mediaType = properties["PhysicalMediaType"]?.Value as uint?;
                    if (mediaType is not null && ((mediaType > 0 && mediaType < 11) || (mediaType > 12 && mediaType < 22)))
                    {
                        char devId = (properties["Caption"].Value as string ?? string.Empty)[0];
                        Array.ForEach(drives, d =>
                        {
                            if (d?.Name is not null && d.Name[0] == devId)
                                d.InternalDriveType = Frontend.InternalDriveType.Floppy;
                        });
                    }
                }
            }
            catch
            {
                // No-op
            }
#endif
        }

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
            var existingNames = new HashSet<string>();
            foreach (var d in drives)
            {
                if (d?.Name is not null)
                    existingNames.Add(d.Name);
            }

            // Block nodes first, then their generic SCSI counterparts
            var devicePaths = new List<string>();
            devicePaths.AddRange(EnumerateUnixOpticalBlockPaths("/dev"));
            devicePaths.AddRange(EnumerateUnixOpticalGenericPaths("/dev", "/sys/class/scsi_generic"));

            var extra = new List<Drive>();
            foreach (var devicePath in devicePaths)
            {
                // Skip paths already surfaced by DriveInfo or an earlier enumerator
                if (!existingNames.Add(devicePath))
                    continue;

                try
                {
                    var d = Create(Frontend.InternalDriveType.Optical, devicePath);
                    if (d != null)
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
        /// Enumerate Linux optical block devices under a directory by matching
        /// the kernel convention "sr" followed by one or more digits (sr0, sr1, ...).
        /// </summary>
        /// <param name="devRoot">Root directory to scan (typically "/dev")</param>
        /// <returns>Device paths, or an empty list when the directory is unreadable</returns>
        internal static List<string> EnumerateUnixOpticalBlockPaths(string devRoot)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(devRoot) || !Directory.Exists(devRoot))
                return result;

            string[] candidates;
            try
            {
                candidates = Directory.GetFiles(devRoot, "sr*");
            }
            catch
            {
                return result;
            }

            foreach (var path in candidates)
            {
                if (HasDeviceIndexSuffix(Path.GetFileName(path), "sr"))
                    result.Add(path);
            }

            return result;
        }

        /// <summary>
        /// Enumerate Linux optical drives via their generic SCSI nodes (/dev/sg*).
        /// Unlike the block nodes, "sg" is not optical-specific, so each candidate is
        /// confirmed against sysfs: an optical drive reports SCSI peripheral device type 5.
        /// </summary>
        /// <param name="devRoot">Root directory the nodes live under (typically "/dev")</param>
        /// <param name="sysfsScsiGenericRoot">sysfs class directory (typically "/sys/class/scsi_generic")</param>
        /// <returns>Device paths, or an empty list when the sysfs directory is unreadable</returns>
        internal static List<string> EnumerateUnixOpticalGenericPaths(string devRoot, string sysfsScsiGenericRoot)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(devRoot) || string.IsNullOrEmpty(sysfsScsiGenericRoot))
                return result;
            if (!Directory.Exists(sysfsScsiGenericRoot))
                return result;

            string[] entries;
            try
            {
                entries = Directory.GetFileSystemEntries(sysfsScsiGenericRoot, "sg*");
            }
            catch
            {
                return result;
            }

            foreach (var entry in entries)
            {
                string name = Path.GetFileName(entry);
                if (!HasDeviceIndexSuffix(name, "sg"))
                    continue;

                // "sg" is generic SCSI; keep only optical drives (peripheral device type 5)
                string typePath = Path.Combine(Path.Combine(entry, "device"), "type");
                try
                {
                    if (!File.Exists(typePath))
                        continue;
                    if (!int.TryParse(File.ReadAllText(typePath).Trim(), out int scsiType) || scsiType != 5)
                        continue;
                }
                catch
                {
                    continue;
                }

                result.Add(Path.Combine(devRoot, name));
            }

            return result;
        }

        /// <summary>
        /// Check that a device node name is the given prefix followed by one or more digits
        /// (e.g. "sr0", "sg12"), rejecting names with trailing or embedded non-digit characters.
        /// </summary>
        /// <param name="name">Device node name to check</param>
        /// <param name="prefix">Required leading text (e.g. "sr", "sg")</param>
        /// <returns>True when the name is the prefix followed only by digits</returns>
        private static bool HasDeviceIndexSuffix(string name, string prefix)
        {
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
        /// Order drives for display: Linux optical block nodes (/dev/sr*) first, then their
        /// generic SCSI counterparts (/dev/sg*), then everything else. Within each group the
        /// trailing index is compared numerically, so e.g. sr2 sorts before sr10. Names that
        /// are not optical /dev nodes (Windows drive letters, fixed/removable device nodes)
        /// fall into the last group and keep a natural, digit-aware order.
        /// </summary>
        /// <param name="name1">First drive name (may be null)</param>
        /// <param name="name2">Second drive name (may be null)</param>
        /// <returns>Negative, zero, or positive per IComparer conventions</returns>
        internal static int CompareDrivesForDisplay(string? name1, string? name2)
        {
            string n1 = string.IsNullOrEmpty(name1) ? "\0" : name1!;
            string n2 = string.IsNullOrEmpty(name2) ? "\0" : name2!;

            int rank1 = OpticalDisplayRank(n1), rank2 = OpticalDisplayRank(n2);
            if (rank1 != rank2)
                return rank1 < rank2 ? -1 : 1;

            return NaturalCompare(n1, n2);
        }

        /// <summary>
        /// Display-order rank for a device name: /dev/sr* = 0, /dev/sg* = 1, anything else = 2.
        /// </summary>
        /// <param name="name">Drive name (device path)</param>
        /// <returns>0, 1, or 2</returns>
        private static int OpticalDisplayRank(string name)
        {
            string baseName = Path.GetFileName(name.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            if (HasDeviceIndexSuffix(baseName, "sr"))
                return 0;
            if (HasDeviceIndexSuffix(baseName, "sg"))
                return 1;
            return 2;
        }

        /// <summary>
        /// Compare two strings with runs of digits treated as numbers, so "sr2" sorts before
        /// "sr10" (and multi-digit indices order correctly). Non-digit characters are compared
        /// ordinally.
        /// </summary>
        /// <param name="a">First string</param>
        /// <param name="b">Second string</param>
        /// <returns>Negative, zero, or positive per IComparer conventions</returns>
        internal static int NaturalCompare(string a, string b)
        {
            int i = 0, j = 0;
            while (i < a.Length && j < b.Length)
            {
                char ca = a[i], cb = b[j];
                if (char.IsDigit(ca) && char.IsDigit(cb))
                {
                    // Compare the two digit runs as numbers, ignoring leading zeros
                    int startA = i, startB = j;
                    while (i < a.Length && char.IsDigit(a[i]))
                        i++;
                    while (j < b.Length && char.IsDigit(b[j]))
                        j++;

                    string numA = a.Substring(startA, i - startA).TrimStart('0');
                    string numB = b.Substring(startB, j - startB).TrimStart('0');

                    if (numA.Length != numB.Length)
                        return numA.Length < numB.Length ? -1 : 1;

                    int numCmp = string.CompareOrdinal(numA, numB);
                    if (numCmp != 0)
                        return numCmp;
                }
                else if (ca != cb)
                {
                    return ca < cb ? -1 : 1;
                }
                else
                {
                    i++;
                    j++;
                }
            }

            // Whichever string still has characters left sorts after the shorter one
            int restA = a.Length - i, restB = b.Length - j;
            return restA.CompareTo(restB);
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
            var existingNames = new HashSet<string>();
            foreach (var d in drives)
            {
                if (d?.Name is not null)
                    existingNames.Add(d.Name);
            }

            var extra = new List<Drive>();
            foreach (var device in EnumerateUnixFixedDevices("/sys/block", "/dev"))
            {
                // Skip paths already surfaced by DriveInfo or an earlier enumerator
                if (!existingNames.Add(device.DevicePath))
                    continue;

                try
                {
                    var d = Create(device.DriveType, device.DevicePath);
                    if (d != null)
                    {
                        // A raw /dev block node is never a mount point, so DriveInfo reports
                        // it as not-ready with no size. Mirror how Windows surfaces these: a
                        // fixed disk is always ready, while a removable drive is only ready
                        // when media is present (sysfs reports a non-zero size).
                        d.TotalSize = device.TotalSize;
                        d.MarkedActive = device.DriveType == Frontend.InternalDriveType.HardDisk
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
        internal static List<UnixBlockDevice> EnumerateUnixFixedDevices(string sysBlockRoot, string devRoot)
        {
            var result = new List<UnixBlockDevice>();
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

                result.Add(new UnixBlockDevice(Path.Combine(devRoot, name), driveType, ReadUnixBlockDeviceSize(entry)));
            }

            return result;
        }

        /// <summary>
        /// sysfs block-device name prefixes that are not user-facing dump targets: loopback
        /// images, ramdisks, device-mapper and software RAID virtual devices, floppy and
        /// network block devices, ZFS volumes, and optical drives (surfaced separately via
        /// the optical enumerator).
        /// </summary>
        private static readonly string[] _excludedUnixBlockPrefixes =
        [
            "loop",
            "ram",
            "zram",
            "dm-",
            "md",
            "sr",
            "fd",
            "nbd",
            "zd",
        ];

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
        /// A Linux fixed or removable block device discovered via sysfs.
        /// </summary>
        internal sealed class UnixBlockDevice
        {
            /// <summary>
            /// Device node path (e.g. "/dev/sda")
            /// </summary>
            public string DevicePath { get; }

            /// <summary>
            /// Drive type derived from the sysfs "removable" flag
            /// </summary>
            public InternalDriveType DriveType { get; }

            /// <summary>
            /// Device size in bytes, or 0 when unknown
            /// </summary>
            public long TotalSize { get; }

            public UnixBlockDevice(string devicePath, InternalDriveType driveType, long totalSize)
            {
                DevicePath = devicePath;
                DriveType = driveType;
                TotalSize = totalSize;
            }
        }

        /// <summary>
        /// Convert drive type to internal version, if possible
        /// </summary>
        /// <param name="driveType">DriveType value to check</param>
        /// <returns>InternalDriveType, if possible, null on error</returns>
        internal static InternalDriveType? ToInternalDriveType(DriveType driveType)
        {
#pragma warning disable IDE0072
            return driveType switch
            {
                DriveType.CDRom => (InternalDriveType?)Frontend.InternalDriveType.Optical,
                DriveType.Fixed => (InternalDriveType?)Frontend.InternalDriveType.HardDisk,
                DriveType.Removable => (InternalDriveType?)Frontend.InternalDriveType.Removable,
                _ => null,
            };
#pragma warning restore IDE0072
        }

        #endregion
    }
}
