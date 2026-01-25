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
                VolumeLabel = driveInfo.VolumeLabel;
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
            drives.Sort((d1, d2) =>
            {
                string d1Name = d1?.Name is null ? "\0" : d1.Name;
                string d2Name = d2?.Name is null ? "\0" : d2.Name;
                return d1Name.CompareTo(d2Name);
            });
            return [.. drives];
        }

        /// <summary>
        /// Get the current media type from drive letter
        /// </summary>
        /// <param name="system">Currently selected system</param>
        /// <returns>The detected media type, if possible</returns>
        public MediaType? GetMediaType(RedumpSystem? system)
        {
#pragma warning disable IDE0010
            // Take care of the non-optical stuff first
            switch (InternalDriveType)
            {
                case Frontend.InternalDriveType.Floppy:
                    return MediaType.FloppyDisk;
                case Frontend.InternalDriveType.HardDisk:
                    return MediaType.HardDisk;
                case Frontend.InternalDriveType.Removable:
                    return MediaType.FlashDrive;
            }

            // Some systems should default to certain media types
            switch (system)
            {
                // CD
                case RedumpSystem.Panasonic3DOInteractiveMultiplayer:
                case RedumpSystem.PhilipsCDi:
                case RedumpSystem.SegaDreamcast:
                case RedumpSystem.SegaSaturn:
                case RedumpSystem.SonyPlayStation:
                case RedumpSystem.VideoCD:
                    return MediaType.CDROM;

                // DVD
                case RedumpSystem.DVDAudio:
                case RedumpSystem.DVDVideo:
                case RedumpSystem.MicrosoftXbox:
                case RedumpSystem.MicrosoftXbox360:
                    return MediaType.DVD;

                // HD-DVD
                case RedumpSystem.HDDVDVideo:
                    return MediaType.HDDVD;

                // Blu-ray
                case RedumpSystem.BDVideo:
                case RedumpSystem.MicrosoftXboxOne:
                case RedumpSystem.MicrosoftXboxSeriesXS:
                case RedumpSystem.SonyPlayStation3:
                case RedumpSystem.SonyPlayStation4:
                case RedumpSystem.SonyPlayStation5:
                    return MediaType.BluRay;

                // GameCube
                case RedumpSystem.NintendoGameCube:
                    return MediaType.NintendoGameCubeGameDisc;

                // Wii
                case RedumpSystem.NintendoWii:
                    return MediaType.NintendoWiiOpticalDisc;

                // WiiU
                case RedumpSystem.NintendoWiiU:
                    return MediaType.NintendoWiiUOpticalDisc;

                // PSP
                case RedumpSystem.SonyPlayStationPortable:
                    return MediaType.UMD;
            }
#pragma warning restore IDE0010

            // Handle optical media by size and filesystem
            if (TotalSize >= 0 && TotalSize <= 800_000_000 && (DriveFormat == "CDFS" || DriveFormat == "UDF"))
                return MediaType.CDROM;
            else if (TotalSize > 800_000_000 && TotalSize <= 8_540_000_000 && (DriveFormat == "CDFS" || DriveFormat == "UDF"))
                return MediaType.DVD;
            else if (TotalSize > 8_540_000_000)
                return MediaType.BluRay;

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
            var desiredDriveTypes = new List<DriveType>() { DriveType.CDRom };
            if (!ignoreFixedDrives)
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

            // Find and update all floppy drives
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

            return [.. drives];
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
