using System;
using System.Collections.Generic;
using System.IO;
#if NET462_OR_GREATER || NETCOREAPP
using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Generic;
#endif
using SabreTools.RedumpLib.Data;
using SabreTools.Text.Compare;

namespace MPF.Frontend
{
    /// <summary>
    /// Represents information for a single drive
    /// </summary>
    /// <remarks>
    /// TODO: Can the Aaru models be used instead of the ones I've created here?
    /// </remarks>
    public partial class Drive
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
            using var comparer = new NaturalComparer();
            drives.Sort((d1, d2) => comparer.Compare(d1?.Name, d2?.Name));
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
            {
                MarkWindowsFloppyDrives(drives);
            }
            else if (isUnix)
            {
                // Optical drives are removable dump targets, so surface them regardless of the
                // fixed-drive toggle. Floppy drives are hidden unless the user opts to show
                // fixed drives, matching Windows (where floppies enumerate as removable media
                // that is only queried alongside fixed drives) and because the default program,
                // Redumper, does not support them. Floppy runs before the fixed enumerator so a
                // USB floppy is tagged Floppy and then skipped there.
                drives = AppendUnixOpticalDrives(drives);
                if (!ignoreFixedDrives)
                {
                    drives = AppendUnixFloppyDrives(drives);
                    drives = AppendUnixFixedDrives(drives);
                }
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
                    uint? mediaType = properties["MediaType"]?.Value as uint?;
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
