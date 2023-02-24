using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Generic;
using MPF.Core.Converters;
using MPF.Core.Utilities;
using RedumpLib.Data;
#if NETFRAMEWORK
using IMAPI2;
#endif

namespace MPF.Core.Data
{
    /// <summary>
    /// Represents information for a single drive
    /// </summary>
    /// <remarks>
    /// TODO: This needs to be less Windows-centric. Devices do not always have a single letter that can be used.
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
        public string DriveFormat { get; private set; } = null;

        /// <summary>
        /// Windows drive path
        /// </summary>
        public string Name { get; private set; } = null;

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
        public string VolumeLabel { get; private set; } = null;

        #endregion

        #region Derived Fields

        /// <summary>
        /// Media label as read by Windows, formatted to avoid odd outputs
        /// </summary>
        public string FormattedVolumeLabel
        {
            get
            {
                string volumeLabel = Template.DiscNotDetected;
                if (this.MarkedActive)
                {
                    if (string.IsNullOrWhiteSpace(this.VolumeLabel))
                        volumeLabel = "track";
                    else
                        volumeLabel = this.VolumeLabel;
                }

                foreach (char c in Path.GetInvalidFileNameChars())
                    volumeLabel = volumeLabel.Replace(c, '_');

                return volumeLabel;
            }
        }

        /// <summary>
        /// Windows drive letter
        /// </summary>
        public char Letter => this.Name == null || this.Name.Length == 0 ? '\0' : this.Name[0];

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
        public static Drive Create(InternalDriveType? driveType, string devicePath)
        {
            // Create a new, empty drive object
            var drive = new Drive()
            {
                InternalDriveType = driveType,
            };

            // If we have an invalid device path, return null
            if (string.IsNullOrWhiteSpace(devicePath))
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
        private void PopulateFromDriveInfo(DriveInfo driveInfo)
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
            drives = drives?.OrderBy(i => i.Letter)?.ToList();
            return drives;
        }

        /// <summary>
        /// Get the current media type from drive letter
        /// </summary>
        /// <returns></returns>
        public (MediaType?, string) GetMediaType()
            => GetMediaType(this.Name, this.InternalDriveType);

        /// <summary>
        /// Get the current system from drive
        /// </summary>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public RedumpSystem? GetRedumpSystem(RedumpSystem? defaultValue)
        {
            string drivePath = $"{this.Letter}:\\";

            // If we can't read the media in that drive, we can't do anything
            if (!Directory.Exists(drivePath))
                return defaultValue;

            // We're going to assume for floppies, HDDs, and removable drives
            // TODO: Try to be smarter about this
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
                List<string> files = Directory.EnumerateFiles(drivePath, "*", SearchOption.TopDirectoryOnly).ToList();

                if (files.Any(f => f.EndsWith(".AJS", StringComparison.OrdinalIgnoreCase))
                    && files.Any(f => f.EndsWith(".GLB", StringComparison.OrdinalIgnoreCase)))
                {
                    return RedumpSystem.BandaiPlaydiaQuickInteractiveSystem;
                }
            }
            catch { }

            // Mattel Fisher-Price iXL
            if (File.Exists(Path.Combine(drivePath, "iXL", "iXLUpdater.exe")))
            {
                return RedumpSystem.MattelFisherPriceiXL;
            }

            // Microsoft Xbox 360
            try
            {
                if (Directory.Exists(Path.Combine(drivePath, "$SystemUpdate"))
                    && Directory.EnumerateFiles(Path.Combine(drivePath, "$SystemUpdate")).Any()
                    && this.TotalSize <= 500_000_000)
                {
                    return RedumpSystem.MicrosoftXbox360;
                }
            }
            catch { }

            // Microsoft Xbox One
            try
            {
                if (Directory.Exists(Path.Combine(drivePath, "MSXC"))
                    && Directory.EnumerateFiles(Path.Combine(drivePath, "MSXC")).Any())
                {
                    return RedumpSystem.MicrosoftXboxOne;
                }
            }
            catch { }

            // Sega Dreamcast
            if (File.Exists(Path.Combine(drivePath, "IP.BIN")))
            {
                return RedumpSystem.SegaDreamcast;
            }

            // Sega Mega-CD / Sega-CD
            if (File.Exists(Path.Combine(drivePath, "_BOOT", "IP.BIN"))
                || File.Exists(Path.Combine(drivePath, "_BOOT", "SP.BIN"))
                || File.Exists(Path.Combine(drivePath, "_BOOT", "SP_AS.BIN"))
                || File.Exists(Path.Combine(drivePath, "FILESYSTEM.BIN")))
            {
                return RedumpSystem.SegaMegaCDSegaCD;
            }

            // Sega Saturn
            try
            {
                byte[] sector = ReadSector(0);
                if (sector != null)
                {
                    if (sector.StartsWith(Interface.SaturnSectorZeroStart))
                        return RedumpSystem.SegaSaturn;
                }
            }
            catch { }

            // Sony PlayStation and Sony PlayStation 2
            string psxExePath = Path.Combine(drivePath, "PSX.EXE");
            string systemCnfPath = Path.Combine(drivePath, "SYSTEM.CNF");
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
                if (Directory.Exists(Path.Combine(drivePath, "PS3_GAME"))
                    || Directory.Exists(Path.Combine(drivePath, "PS3_UPDATE"))
                    || File.Exists(Path.Combine(drivePath, "PS3_DISC.SFB")))
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
            if (File.Exists(Path.Combine(drivePath, "PS4", "UPDATE", "PS4UPDATE.PUP")))
            {
                return RedumpSystem.SonyPlayStation4;
            }

            // V.Tech V.Flash / V.Smile Pro
            if (File.Exists(Path.Combine(drivePath, "0SYSTEM")))
            {
                return RedumpSystem.VTechVFlashVSmilePro;
            }

            #endregion

            #region Computers

            // Sharp X68000
            if (File.Exists(Path.Combine(drivePath, "COMMAND.X")))
            {
                return RedumpSystem.SharpX68000;
            }

            #endregion

            #region Video Formats

            // BD-Video
            if (Directory.Exists(Path.Combine(drivePath, "BDMV")))
            {
                // Technically BD-Audio has this as well, but it's hard to split that out right now
                return RedumpSystem.BDVideo;
            }

            // DVD-Audio and DVD-Video
            try
            {
                if (Directory.Exists(Path.Combine(drivePath, "AUDIO_TS"))
                    && Directory.EnumerateFiles(Path.Combine(drivePath, "AUDIO_TS")).Any())
                {
                    return RedumpSystem.DVDAudio;
                }

                else if (Directory.Exists(Path.Combine(drivePath, "VIDEO_TS"))
                    && Directory.EnumerateFiles(Path.Combine(drivePath, "VIDEO_TS")).Any())
                {
                    return RedumpSystem.DVDVideo;
                }
            }
            catch { }

            // HD-DVD-Video
            try
            {
                if (Directory.Exists(Path.Combine(drivePath, "HVDVD_TS"))
                    && Directory.EnumerateFiles(Path.Combine(drivePath, "HVDVD_TS")).Any())
                {
                    return RedumpSystem.HDDVDVideo;
                }
            }
            catch { }

            // VCD
            try
            {
                if (Directory.Exists(Path.Combine(drivePath, "VCD"))
                    && Directory.EnumerateFiles(Path.Combine(drivePath, "VCD")).Any())
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
            if (string.IsNullOrWhiteSpace(this.VolumeLabel))
                return null;

            // Audio CD
            if (this.VolumeLabel.Equals("Audio CD", StringComparison.OrdinalIgnoreCase))
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
        /// Read a sector with a specified size from the drive
        /// </summary>
        /// <param name="num">Sector number, non-negative</param>
        /// <param name="size">Size of a sector in bytes</param>
        /// <returns>Byte array representing the sector, null on error</returns>
        public byte[] ReadSector(long num, int size = 2048)
        {
            // Missing drive leter is not supported
            if (string.IsNullOrEmpty(this.Name))
                return null;

            // We don't support negative sectors
            if (num < 0)
                return null;

            // Wrap the following in case of device access errors
            Stream fs = null;
            try
            {
                // Open the drive as a device
                fs = File.OpenRead($"\\\\?\\{this.Letter}:");

                // Seek to the start of the sector, if possible
                long start = num * size;
                fs.Seek(start, SeekOrigin.Begin);

                // Read and return the sector
                byte[] buffer = new byte[size];
                fs.Read(buffer, 0, size);
                return buffer;
            }
            catch
            {
                return null;
            }
            finally
            {
                fs?.Dispose();
            }
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
        /// Get the media type for a device path
        /// </summary>
        /// <returns>MediaType, null on error</returns>
        private (MediaType?, string) GetMediaTypeFromFilesystemName()
        {
            switch (this.DriveFormat)
            {
                default: return (null, $"Unrecognized format: {this.DriveFormat}");
            }
        }

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

            // Get all supported drive types
            var drives = DriveInfo.GetDrives()
                .Where(d => desiredDriveTypes.Contains(d.DriveType))
                .Select(d => Create(EnumConverter.ToInternalDriveType(d.DriveType), d.Name))
                .ToList();

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
                        char devId = (properties["Caption"].Value as string)[0];
                        drives.ForEach(d => { if (d.Letter == devId) { d.InternalDriveType = Data.InternalDriveType.Floppy; } });
                    }
                }
            }
            catch
            {
                // No-op
            }

            return drives;
        }

        /// <summary>
        /// Get the media type for a device path using the Aaru libraries
        /// </summary>
        /// <param name="devicePath">Path to the device</param>
        /// <param name="internalDriveType">Current internal drive type</param>
        /// <returns>MediaType, null on error</returns>
        private static (MediaType?, string) GetMediaType(string devicePath, InternalDriveType? internalDriveType)
        {
            char driveLetter = devicePath == null || !devicePath.Any() ? '\0' : devicePath[0];

            // Take care of the non-optical stuff first
            // TODO: See if any of these can be more granular, like Optical is
            if (internalDriveType == Data.InternalDriveType.Floppy)
                return (MediaType.FloppyDisk, null);
            else if (internalDriveType == Data.InternalDriveType.HardDisk)
                return (MediaType.HardDisk, null);
            else if (internalDriveType == Data.InternalDriveType.Removable)
                return (MediaType.FlashDrive, null);

            // Get the current drive information
            string deviceId = null;
            bool loaded = false;
            try
            {
                // Get the device ID first
                CimSession session = CimSession.Create(null);
                var collection = session.QueryInstances("root\\CIMV2", "WQL", $"SELECT * FROM Win32_CDROMDrive WHERE Id = '{driveLetter}:\'");

                foreach (CimInstance instance in collection)
                {
                    CimKeyedCollection<CimProperty> properties = instance.CimInstanceProperties;
                    deviceId = (string)properties["DeviceID"]?.Value;
                    loaded = (bool)properties["MediaLoaded"]?.Value;
                }

                // If we got no valid device, we don't care and just return
                if (deviceId == null)
                    return (null, "Device could not be found");
                else if (!loaded)
                    return (null, "Device is not reporting media loaded");

                #if NETFRAMEWORK

                MsftDiscMaster2 discMaster = new MsftDiscMaster2();
                deviceId = deviceId.ToLower().Replace('\\', '#').Replace('/', '#');
                string id = null;
                foreach (var disc in discMaster)
                {
                    if (disc.ToString().Contains(deviceId))
                        id = disc.ToString();
                }

                // If we couldn't find the drive, we don't care and return
                if (id == null)
                    return (null, "Device ID could not be found");

                // Create the required objects for reading from the drive
                MsftDiscRecorder2 recorder = new MsftDiscRecorder2();
                recorder.InitializeDiscRecorder(id);
                MsftDiscFormat2Data dataWriter = new MsftDiscFormat2Data();

                // If the recorder is not supported, just return
                if (!dataWriter.IsRecorderSupported(recorder))
                    return (null, "IMAPI2 recorder not supported");

                // Otherwise, set the recorder to get information from
                dataWriter.Recorder = recorder;

                var media = dataWriter.CurrentPhysicalMediaType;
                return (media.IMAPIToMediaType(), null);

                #endif

                return (null, "IMAPI2 recorder not supported");
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        #endregion
    }
}
