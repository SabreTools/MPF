using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MPF.Core.Converters;
using MPF.Core.Utilities;
using RedumpLib.Data;
#if NETFRAMEWORK
using System.Management;
using IMAPI2;
#else
using Aaru.CommonTypes.Enums;
using Aaru.Core.Media.Info;
using Aaru.Decoders.SCSI.MMC;
using Aaru.Devices;
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
            if (File.Exists(Path.Combine(drivePath, "iXL", "iXLUpdater.exe"))
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

#if NETFRAMEWORK

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

            // Get the floppy drives and set the flag from removable
            try
            {
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_LogicalDisk");

                var collection = searcher.Get();
                foreach (ManagementObject queryObj in collection)
                {
                    uint? mediaType = (uint?)queryObj["MediaType"];
                    if (mediaType != null && ((mediaType > 0 && mediaType < 11) || (mediaType > 12 && mediaType < 22)))
                    {
                        char devId = queryObj["DeviceID"].ToString()[0];
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
                var searcher = new ManagementObjectSearcher(
                    "root\\CIMV2",
                    $"SELECT * FROM Win32_CDROMDrive WHERE Id = '{driveLetter}:\'");

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    deviceId = (string)queryObj["DeviceID"];
                    loaded = (bool)queryObj["MediaLoaded"];
                }

                // If we got no valid device, we don't care and just return
                if (deviceId == null)
                    return (null, "Device could not be found");
                else if (!loaded)
                    return (null, "Device is not reporting media loaded");

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
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

#else

        /// <summary>
        /// Get all devices attached converted to Drive objects
        /// </summary>
        /// <param name="ignoreFixedDrives">True to ignore fixed drives from population, false otherwise</param>
        /// <returns>List of drives, null on error</returns>
        private static List<Drive> GetDriveList(bool ignoreFixedDrives)
        {
            DeviceInfo[] deviceInfos = Device.ListDevices();
            if (deviceInfos == null)
                return null;

            var drives = new List<Drive>();
            foreach (DeviceInfo deviceInfo in deviceInfos)
            {
                if (deviceInfo.Path == null)
                    continue;

                if (!deviceInfo.Supported)
                    continue;

                var drive = GetDriveFromDevice(deviceInfo.Path, ignoreFixedDrives);
                if (drive == null)
                    continue;

                drives.Add(drive);
            }

            return drives;
        }

        /// <summary>
        /// Generate a Drive object from a single device
        /// </summary>
        /// <param name="devicePath">Path to the device</param>
        /// <param name="ignoreFixedDrives">True to ignore fixed drives from population, false otherwise</param>
        /// <returns>Drive object for the device, null on error</returns>
        private static Drive GetDriveFromDevice(string devicePath, bool ignoreFixedDrives)
        {
            if (devicePath.Length == 2 &&
               devicePath[1] == ':' &&
               devicePath[0] != '/' &&
               char.IsLetter(devicePath[0]))
                devicePath = "\\\\.\\" + char.ToUpper(devicePath[0]) + ':';

            string windowsLocalDevicePath = devicePath;
            if (windowsLocalDevicePath.StartsWith("\\\\.\\"))
                windowsLocalDevicePath = windowsLocalDevicePath.Substring("\\\\.\\".Length);

            var dev = Device.Create(devicePath, out _);
            if (dev == null || dev.Error)
                return null;

            var devInfo = new Aaru.Core.Devices.Info.DeviceInfo(dev);
            if (devInfo.MmcConfiguration != null)
            {
                Features.SeparatedFeatures ftr = Features.Separate(devInfo.MmcConfiguration);
                if (ftr.Descriptors != null && ftr.Descriptors.Any(d => d.Code == 0x0000))
                {
                    var desc = ftr.Descriptors.First(d => d.Code == 0x0000);
                    bool isOptical = IsOptical(desc.Data);
                    if (isOptical)
                        return Create(Data.InternalDriveType.Optical, windowsLocalDevicePath);
                    else if (!ignoreFixedDrives)
                        return Create(Data.InternalDriveType.Removable, windowsLocalDevicePath);
                }
            }

            if (!ignoreFixedDrives)
            {
                switch (dev.Type)
                {
                    case DeviceType.MMC:
                        return Create(Data.InternalDriveType.Removable, windowsLocalDevicePath);

                    case DeviceType.SecureDigital:
                        return Create(Data.InternalDriveType.Removable, windowsLocalDevicePath);
                }

                if (dev.IsUsb)
                    return Create(Data.InternalDriveType.Removable, windowsLocalDevicePath);

                if (dev.IsFireWire)
                    return Create(Data.InternalDriveType.Removable, windowsLocalDevicePath);

                if (dev.IsPcmcia)
                    return Create(Data.InternalDriveType.Removable, windowsLocalDevicePath);
            }

            dev.Close();
            return null;
        }

        /// <summary>
        /// Get the media type for a device path using the Aaru libraries
        /// </summary>
        /// <param name="devicePath">Path to the device</param>
        /// <param name="internalDriveType">Current internal drive type</param>
        /// <returns>MediaType, null on error</returns>
        /// <remarks>
        /// TODO: Get the device type for devices with set media types
        /// </remarks>
        private static (MediaType?, string) GetMediaType(string devicePath, InternalDriveType? internalDriveType)
        {
            if (devicePath.Length == 2 &&
               devicePath[1] == ':' &&
               devicePath[0] != '/' &&
               char.IsLetter(devicePath[0]))
                devicePath = "\\\\.\\" + char.ToUpper(devicePath[0]) + ':';

            var dev = Device.Create(devicePath, out _);
            if (dev == null || dev.Error)
                return (null, "Device could not be accessed");

            switch (dev.Type)
            {
                case DeviceType.ATAPI:
                case DeviceType.SCSI:
                    ScsiInfo scsiInfo = new ScsiInfo(dev);
                    var mediaType = EnumConverter.MediaTypeToMediaType(scsiInfo?.MediaType);
                    if (mediaType == null)
                        return (mediaType, "Could not determine media type");
                    else
                        return (mediaType, null);
            }

            return (null, "Device does not support media type finding");
        }

        /// <summary>
        /// Determine if a drive is optical or not
        /// </summary>
        /// <param name="featureBytes">Bytes representing the field to check</param>
        /// <returns>True if the drive is optical, false otherwise</returns>
        private static bool IsOptical(byte[] featureBytes)
        {
            var supportedMediaTypes = OpticalMediaSupport(featureBytes);
            if (supportedMediaTypes == null || !supportedMediaTypes.Any())
                return false;

            if (supportedMediaTypes.Contains(MediaType.CDROM))
                return true;
            else if (supportedMediaTypes.Contains(MediaType.DVD))
                return true;
            else if (supportedMediaTypes.Contains(MediaType.BluRay))
                return true;
            else if (supportedMediaTypes.Contains(MediaType.HDDVD))
                return true;

            return false;
        }

        /// <summary>
        /// Get supported media types for a drive
        /// </summary>
        /// <param name="featureBytes">Bytes representing the field to check</param>
        /// <returns>List of supported media types, null on error</returns>
        private static List<MediaType> OpticalMediaSupport(byte[] featureBytes)
        {
            Feature_0000? feature = Features.Decode_0000(featureBytes);

            if (!feature.HasValue)
                return null;

            var supportedMediaTypes = new List<MediaType>();
            foreach (Profile prof in feature.Value.Profiles)
            {
                switch (prof.Number)
                {
                    // Values we don't care about for Optical
                    case ProfileNumber.Reserved:
                    case ProfileNumber.NonRemovable:
                    case ProfileNumber.Removable:
                    case ProfileNumber.MOErasable:
                    case ProfileNumber.OpticalWORM:
                    case ProfileNumber.ASMO:
                    case ProfileNumber.Unconforming:
                        break;

                    // Every supported optical profile
                    case ProfileNumber.CDROM:
                    case ProfileNumber.CDR:
                    case ProfileNumber.CDRW:
                        supportedMediaTypes.Add(MediaType.CDROM);
                        break;

                    case ProfileNumber.DVDROM:
                    case ProfileNumber.DVDRSeq:
                    case ProfileNumber.DVDRAM:
                    case ProfileNumber.DVDRWRes:
                    case ProfileNumber.DVDRWSeq:
                    case ProfileNumber.DVDRDLSeq:
                    case ProfileNumber.DVDRDLJump:
                    case ProfileNumber.DVDRWDL:
                    case ProfileNumber.DVDDownload:
                    case ProfileNumber.DVDRWPlus:
                    case ProfileNumber.DVDRPlus:
                    case ProfileNumber.DVDRWDLPlus:
                    case ProfileNumber.DVDRDLPlus:
                        supportedMediaTypes.Add(MediaType.DVD);
                        break;

                    // TODO: Add DDCD as media type
                    case ProfileNumber.DDCDROM:
                    case ProfileNumber.DDCDR:
                    case ProfileNumber.DDCDRW:
                        supportedMediaTypes.Add(MediaType.DVD);
                        break;

                    case ProfileNumber.BDROM:
                    case ProfileNumber.BDRSeq:
                    case ProfileNumber.BDRRdm:
                    case ProfileNumber.BDRE:
                        supportedMediaTypes.Add(MediaType.BluRay);
                        break;

                    case ProfileNumber.HDDVDROM:
                    case ProfileNumber.HDDVDR:
                    case ProfileNumber.HDDVDRAM:
                    case ProfileNumber.HDDVDRW:
                    case ProfileNumber.HDDVDRDL:
                    case ProfileNumber.HDDVDRWDL:
                    case ProfileNumber.HDBURNROM:
                    case ProfileNumber.HDBURNR:
                    case ProfileNumber.HDBURNRW:
                        supportedMediaTypes.Add(MediaType.HDDVD);
                        break;
                }
            }

            return supportedMediaTypes.Distinct().ToList();
        }

#endif

        #endregion
    }
}
