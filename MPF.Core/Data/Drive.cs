using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if NET462_OR_GREATER || NETCOREAPP
using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Generic;
#endif
using MPF.Core.Converters;
using SabreTools.RedumpLib.Data;

namespace MPF.Core.Data
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
        /// Media label as read by Windows, formatted to avoid odd outputs
        /// If no volume label present, use PSX or PS2 serial if valid
        /// Otherwise, use "track" as volume label
        /// </summary>
        public string? FormattedVolumeLabel
        {
            get
            {
                string? volumeLabel = Template.DiscNotDetected;
                if (this.MarkedActive)
                {
                    if (!string.IsNullOrEmpty(this.VolumeLabel))
                        volumeLabel = this.VolumeLabel;
                    else
                    {
                        RedumpSystem? system = this.GetRedumpSystem(null);
                        if (system == RedumpSystem.SonyPlayStation || system == RedumpSystem.SonyPlayStation2)
                        {
                            InfoTool.GetPlayStationExecutableInfo(this.Name, out string? serial, out _, out _);
                            volumeLabel = serial ?? "track";
                        }
                        else
                        {
                            volumeLabel = "track";
                        }
                    }
                }

                foreach (char c in Path.GetInvalidFileNameChars())
                    volumeLabel = volumeLabel?.Replace(c, '_');

                return volumeLabel;
            }
        }

        /// <summary>
        /// Read-only access to the drive letter
        /// </summary>
        /// <remarks>Should only be used in UI applications</remarks>
        public char? Letter => this.Name?[0] ?? '\0';

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
                devicePath = devicePath.Substring("\\\\.\\".Length);

            // Create and validate the drive info object
            var driveInfo = new DriveInfo(devicePath);
            if (driveInfo == null || driveInfo == default)
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
            if (driveInfo == null || driveInfo == default)
                return;

            // Populate the data fields
            this.Name = driveInfo.Name;
            this.MarkedActive = driveInfo.IsReady;
            if (this.MarkedActive)
            {
                this.DriveFormat = driveInfo.DriveFormat;
                this.TotalSize = driveInfo.TotalSize;
                this.VolumeLabel = driveInfo.VolumeLabel;
            }
            else
            {
                this.DriveFormat = string.Empty;
                this.TotalSize = default;
                this.VolumeLabel = string.Empty;
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
            drives = [.. drives.OrderBy(i => i == null ? "\0" : i.Name)];
            return drives;
        }

        /// <summary>
        /// Get the current media type from drive letter
        /// </summary>
        /// <param name="system"></param>
        /// <returns></returns>
        public (MediaType?, string?) GetMediaType(RedumpSystem? system)
        {
            // Take care of the non-optical stuff first
            switch (this.InternalDriveType)
            {
                case Data.InternalDriveType.Floppy:
                    return (MediaType.FloppyDisk, null);
                case Data.InternalDriveType.HardDisk:
                    return (MediaType.HardDisk, null);
                case Data.InternalDriveType.Removable:
                    return (MediaType.FlashDrive, null);
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
                    return (MediaType.CDROM, null);

                // DVD
                case RedumpSystem.DVDAudio:
                case RedumpSystem.DVDVideo:
                case RedumpSystem.MicrosoftXbox:
                case RedumpSystem.MicrosoftXbox360:
                    return (MediaType.DVD, null);

                // HD-DVD
                case RedumpSystem.HDDVDVideo:
                    return (MediaType.HDDVD, null);

                // Blu-ray
                case RedumpSystem.BDVideo:
                case RedumpSystem.MicrosoftXboxOne:
                case RedumpSystem.MicrosoftXboxSeriesXS:
                case RedumpSystem.SonyPlayStation3:
                case RedumpSystem.SonyPlayStation4:
                case RedumpSystem.SonyPlayStation5:
                    return (MediaType.BluRay, null);

                // GameCube
                case RedumpSystem.NintendoGameCube:
                    return (MediaType.NintendoGameCubeGameDisc, null);

                // Wii
                case RedumpSystem.NintendoWii:
                    return (MediaType.NintendoWiiOpticalDisc, null);

                // WiiU
                case RedumpSystem.NintendoWiiU:
                    return (MediaType.NintendoWiiUOpticalDisc, null);

                // PSP
                case RedumpSystem.SonyPlayStationPortable:
                    return (MediaType.UMD, null);
            }

            // Handle optical media by size and filesystem
            if (this.TotalSize >= 0 && this.TotalSize <= 800_000_000 && (this.DriveFormat == "CDFS" || this.DriveFormat == "UDF"))
                return (MediaType.CDROM, null);
            else if (this.TotalSize > 800_000_000 && this.TotalSize <= 8_540_000_000 && (this.DriveFormat == "CDFS" || this.DriveFormat == "UDF"))
                return (MediaType.DVD, null);
            else if (this.TotalSize > 8_540_000_000)
                return (MediaType.BluRay, null);

            return (null, "Could not determine media type!");
        }

        /// <summary>
        /// Get the current system from drive
        /// </summary>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public RedumpSystem? GetRedumpSystem(RedumpSystem? defaultValue)
        {
            // If we can't read the media in that drive, we can't do anything
            if (string.IsNullOrEmpty(this.Name) || !Directory.Exists(this.Name))
                return defaultValue;

            // We're going to assume for floppies, HDDs, and removable drives
            if (this.InternalDriveType != Data.InternalDriveType.Optical)
                return RedumpSystem.IBMPCcompatible;

            // Check volume labels first
            RedumpSystem? systemFromLabel = GetRedumpSystemFromVolumeLabel();
            if (systemFromLabel != null)
                return systemFromLabel;

            // Get a list of files for quicker checking
            #region Consoles

            // Bandai Playdia Quick Interactive System
            try
            {
#if NET20 || NET35
                List<string> files = [.. Directory.GetFiles(this.Name, "*", SearchOption.TopDirectoryOnly)];
#else
                List<string> files = Directory.EnumerateFiles(this.Name, "*", SearchOption.TopDirectoryOnly).ToList();
#endif

                if (files.Any(f => f.EndsWith(".AJS", StringComparison.OrdinalIgnoreCase))
                    && files.Any(f => f.EndsWith(".GLB", StringComparison.OrdinalIgnoreCase)))
                {
                    return RedumpSystem.BandaiPlaydiaQuickInteractiveSystem;
                }
            }
            catch { }

            // Bandai Pippin
            if (File.Exists(Path.Combine(this.Name, "PippinAuthenticationFile")))
            {
                return RedumpSystem.BandaiPippin;
            }

            // Mattel Fisher-Price iXL
#if NET20 || NET35
            if (File.Exists(Path.Combine(Path.Combine(this.Name, "iXL"), "iXLUpdater.exe")))
#else
            if (File.Exists(Path.Combine(this.Name, "iXL", "iXLUpdater.exe")))
#endif
            {
                return RedumpSystem.MattelFisherPriceiXL;
            }

            // Microsoft Xbox 360
            try
            {
                if (Directory.Exists(Path.Combine(this.Name, "$SystemUpdate"))
#if NET20 || NET35
                    && Directory.GetFiles(Path.Combine(this.Name, "$SystemUpdate")).Any()
#else
                    && Directory.EnumerateFiles(Path.Combine(this.Name, "$SystemUpdate")).Any()
#endif
                    && this.TotalSize <= 500_000_000)
                {
                    return RedumpSystem.MicrosoftXbox360;
                }
            }
            catch { }

            // Microsoft Xbox One
            try
            {
                if (Directory.Exists(Path.Combine(this.Name, "MSXC"))
#if NET20 || NET35
                    && Directory.GetFiles(Path.Combine(this.Name, "MSXC")).Any())
#else
                    && Directory.EnumerateFiles(Path.Combine(this.Name, "MSXC")).Any())
#endif
                {
                    return RedumpSystem.MicrosoftXboxOne;
                }
            }
            catch { }

            // Sega Dreamcast
            if (File.Exists(Path.Combine(this.Name, "IP.BIN")))
            {
                return RedumpSystem.SegaDreamcast;
            }

            // Sega Mega-CD / Sega-CD
#if NET20 || NET35
            if (File.Exists(Path.Combine(Path.Combine(this.Name, "_BOOT"), "IP.BIN"))
                || File.Exists(Path.Combine(Path.Combine(this.Name, "_BOOT"), "SP.BIN"))
                || File.Exists(Path.Combine(Path.Combine(this.Name, "_BOOT"), "SP_AS.BIN"))
                || File.Exists(Path.Combine(this.Name, "FILESYSTEM.BIN")))
#else
            if (File.Exists(Path.Combine(this.Name, "_BOOT", "IP.BIN"))
                || File.Exists(Path.Combine(this.Name, "_BOOT", "SP.BIN"))
                || File.Exists(Path.Combine(this.Name, "_BOOT", "SP_AS.BIN"))
                || File.Exists(Path.Combine(this.Name, "FILESYSTEM.BIN")))
#endif
            {
                return RedumpSystem.SegaMegaCDSegaCD;
            }

            // Sony PlayStation and Sony PlayStation 2
            string psxExePath = Path.Combine(this.Name, "PSX.EXE");
            string systemCnfPath = Path.Combine(this.Name, "SYSTEM.CNF");
            if (File.Exists(systemCnfPath))
            {
                // Check for either BOOT or BOOT2
                var systemCnf = new IniFile(systemCnfPath);
                if (systemCnf.ContainsKey("BOOT"))
                    return RedumpSystem.SonyPlayStation;
                else if (systemCnf.ContainsKey("BOOT2"))
                    return RedumpSystem.SonyPlayStation2;
            }
            else if (File.Exists(psxExePath))
            {
                return RedumpSystem.SonyPlayStation;
            }

            // Sony PlayStation 3
            try
            {
                if (Directory.Exists(Path.Combine(this.Name, "PS3_GAME"))
                    || Directory.Exists(Path.Combine(this.Name, "PS3_UPDATE"))
                    || File.Exists(Path.Combine(this.Name, "PS3_DISC.SFB")))
                {
                    return RedumpSystem.SonyPlayStation3;
                }
            }
            catch { }

            // Sony PlayStation 4
            // There are more possible paths that could be checked.
            //  There are some entries that can be found on most PS4 discs:
            //    "/app/GAME_SERIAL/app.pkg"
            //    "/bd/param.sfo"
            //    "/license/rif"
            // There are also extra files that can be found on some discs:
            //    "/patch/GAME_SERIAL/patch.pkg" can be found in Redump entry 66816.
            //        Originally on disc as "/patch/CUSA11302/patch.pkg".
            //        Is used as an on-disc update for the base game app without needing to get update from the internet.
            //    "/addcont/GAME_SERIAL/CONTENT_ID/ac.pkg" can be found in Redump entry 97619.
            //        Originally on disc as "/addcont/CUSA00288/FFXIVEXPS400001A/ac.pkg".
#if NET20 || NET35
            if (File.Exists(Path.Combine(Path.Combine(Path.Combine(this.Name, "PS4"), "UPDATE"), "PS4UPDATE.PUP")))
#else
            if (File.Exists(Path.Combine(this.Name, "PS4", "UPDATE", "PS4UPDATE.PUP")))
#endif
            {
                return RedumpSystem.SonyPlayStation4;
            }

            // V.Tech V.Flash / V.Smile Pro
            if (File.Exists(Path.Combine(this.Name, "0SYSTEM")))
            {
                return RedumpSystem.VTechVFlashVSmilePro;
            }

#endregion

            #region Computers

            // Sharp X68000
            if (File.Exists(Path.Combine(this.Name, "COMMAND.X")))
            {
                return RedumpSystem.SharpX68000;
            }

            #endregion

            #region Video Formats

            // BD-Video
            if (Directory.Exists(Path.Combine(this.Name, "BDMV")))
            {
                // Technically BD-Audio has this as well, but it's hard to split that out right now
                return RedumpSystem.BDVideo;
            }

            // DVD-Audio and DVD-Video
            try
            {
                if (Directory.Exists(Path.Combine(this.Name, "AUDIO_TS"))
#if NET20 || NET35
                    && Directory.GetFiles(Path.Combine(this.Name, "AUDIO_TS")).Any())
#else
                    && Directory.EnumerateFiles(Path.Combine(this.Name, "AUDIO_TS")).Any())
#endif
                {
                    return RedumpSystem.DVDAudio;
                }

                else if (Directory.Exists(Path.Combine(this.Name, "VIDEO_TS"))
#if NET20 || NET35
                    && Directory.GetFiles(Path.Combine(this.Name, "VIDEO_TS")).Any())
#else
                    && Directory.EnumerateFiles(Path.Combine(this.Name, "VIDEO_TS")).Any())
#endif
                {
                    return RedumpSystem.DVDVideo;
                }
            }
            catch { }

            // HD-DVD-Video
            try
            {
                if (Directory.Exists(Path.Combine(this.Name, "HVDVD_TS"))
#if NET20 || NET35
                    && Directory.GetFiles(Path.Combine(this.Name, "HVDVD_TS")).Any())
#else
                    && Directory.EnumerateFiles(Path.Combine(this.Name, "HVDVD_TS")).Any())
#endif
                {
                    return RedumpSystem.HDDVDVideo;
                }
            }
            catch { }

            // VCD
            try
            {
                if (Directory.Exists(Path.Combine(this.Name, "VCD"))
#if NET20 || NET35
                    && Directory.GetFiles(Path.Combine(this.Name, "VCD")).Any())
#else
                    && Directory.EnumerateFiles(Path.Combine(this.Name, "VCD")).Any())
#endif
                {
                    return RedumpSystem.VideoCD;
                }
            }
            catch { }

#endregion

            // Default return
            return defaultValue;
        }

        /// <summary>
        /// Get the current system from the drive volume label
        /// </summary>
        /// <returns>The system based on volume label, null if none detected</returns>
        public RedumpSystem? GetRedumpSystemFromVolumeLabel()
        {
            // If the volume label is empty, we can't do anything
            if (string.IsNullOrEmpty(this.VolumeLabel))
                return null;

            // Audio CD
            if (this.VolumeLabel!.Equals("Audio CD", StringComparison.OrdinalIgnoreCase))
                return RedumpSystem.AudioCD;

            // Microsoft Xbox
            if (this.VolumeLabel.Equals("SEP13011042", StringComparison.OrdinalIgnoreCase))
                return RedumpSystem.MicrosoftXbox;
            else if (this.VolumeLabel.Equals("SEP13011042072", StringComparison.OrdinalIgnoreCase))
                return RedumpSystem.MicrosoftXbox;

            // Microsoft Xbox 360
            if (this.VolumeLabel.Equals("XBOX360", StringComparison.OrdinalIgnoreCase))
                return RedumpSystem.MicrosoftXbox360;
            else if (this.VolumeLabel.Equals("XGD2DVD_NTSC", StringComparison.OrdinalIgnoreCase))
                return RedumpSystem.MicrosoftXbox360;

            // Microsoft Xbox 360 - Too overly broad even if a lot of discs use this
            //if (this.VolumeLabel.Equals("CD_ROM", StringComparison.OrdinalIgnoreCase))
            //    return RedumpSystem.MicrosoftXbox360; // Also for Xbox One?
            //if (this.VolumeLabel.Equals("DVD_ROM", StringComparison.OrdinalIgnoreCase))
            //    return RedumpSystem.MicrosoftXbox360;

            // Sega Mega-CD / Sega-CD
            if (this.VolumeLabel.Equals("Sega_CD", StringComparison.OrdinalIgnoreCase))
                return RedumpSystem.SegaMegaCDSegaCD;

            // Sony PlayStation 3
            if (this.VolumeLabel.Equals("PS3VOLUME", StringComparison.OrdinalIgnoreCase))
                return RedumpSystem.SonyPlayStation3;

            // Sony PlayStation 4
            if (this.VolumeLabel.Equals("PS4VOLUME", StringComparison.OrdinalIgnoreCase))
                return RedumpSystem.SonyPlayStation4;

            // Sony PlayStation 5
            if (this.VolumeLabel.Equals("PS5VOLUME", StringComparison.OrdinalIgnoreCase))
                return RedumpSystem.SonyPlayStation5;

            return null;
        }

        /// <summary>
        /// Refresh the current drive information based on path
        /// </summary>
        public void RefreshDrive()
        {
            var driveInfo = DriveInfo.GetDrives().FirstOrDefault(d => d?.Name == this.Name);
            this.PopulateFromDriveInfo(driveInfo);
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

            // Create an output drive list
            var drives = new List<Drive>();

            // Get all standard supported drive types
            try
            {
                drives = DriveInfo.GetDrives()
                    .Where(d => desiredDriveTypes.Contains(d.DriveType))
                    .Select(d => Create(EnumConverter.ToInternalDriveType(d.DriveType), d.Name) ?? new Drive())
                    .ToList();
            }
            catch
            {
                return drives;
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
                    if (mediaType != null && ((mediaType > 0 && mediaType < 11) || (mediaType > 12 && mediaType < 22)))
                    {
                        char devId = (properties["Caption"].Value as string ?? string.Empty)[0];
                        drives.ForEach(d => { if (d?.Name != null && d.Name[0] == devId) { d.InternalDriveType = Data.InternalDriveType.Floppy; } });
                    }
                }
            }
            catch
            {
                // No-op
            }
#endif

            return drives;
        }

        #endregion
    }
}
