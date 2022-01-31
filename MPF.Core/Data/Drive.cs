using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using MPF.Core.Converters;
using MPF.Core.Utilities;
using RedumpLib.Data;
#if NET_FRAMEWORK
using IMAPI2;
#else
using Aaru.CommonTypes.Enums;
using AaruDevices = Aaru.Devices;
#endif

namespace MPF.Core.Data
{
    /// <summary>
    /// Represents information for a single drive
    /// </summary>
    public class Drive
    {
        /// <summary>
        /// Represents drive type
        /// </summary>
        public InternalDriveType? InternalDriveType { get; set; }

        /// <summary>
        /// Drive partition format
        /// </summary>
        public string DriveFormat => driveInfo?.DriveFormat;

        /// <summary>
        /// Windows drive letter
        /// </summary>
        public char Letter => driveInfo?.Name[0] ?? '\0';

        /// <summary>
        /// Windows drive path
        /// </summary>
        public string Name => driveInfo?.Name;

        /// <summary>
        /// Represents if Windows has marked the drive as active
        /// </summary>
        public bool MarkedActive => driveInfo?.IsReady ?? false;

        /// <summary>
        /// Media label as read by Windows
        /// </summary>
        /// <remarks>The try/catch is needed because Windows will throw an exception if the drive is not marked as active</remarks>
        public string VolumeLabel
        {
            get
            {
                try
                {
                    return driveInfo?.VolumeLabel;
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Media label as read by Windows, formatted to avoid odd outputs
        /// </summary>
        public string FormattedVolumeLabel
        {
            get
            {
                string volumeLabel = Template.DiscNotDetected;
                if (driveInfo.IsReady)
                {
                    if (string.IsNullOrWhiteSpace(driveInfo.VolumeLabel))
                        volumeLabel = "track";
                    else
                        volumeLabel = driveInfo.VolumeLabel;
                }

                foreach (char c in Path.GetInvalidFileNameChars())
                    volumeLabel = volumeLabel.Replace(c, '_');

                return volumeLabel;
            }
        }

        /// <summary>
        /// DriveInfo object representing the drive, if possible
        /// </summary>
        private readonly DriveInfo driveInfo;

        public Drive(InternalDriveType? driveType, DriveInfo driveInfo)
        {
            this.InternalDriveType = driveType;
            this.driveInfo = driveInfo;
        }

        /// <summary>
        /// Create a list of active drives matched to their volume labels
        /// </summary>
        /// <param name="ignoreFixedDrives">Ture to ignore fixed drives from population, false otherwise</param>
        /// <returns>Active drives, matched to labels, if possible</returns>
        /// <remarks>
        /// https://stackoverflow.com/questions/3060796/how-to-distinguish-between-usb-and-floppy-devices?utm_medium=organic&utm_source=google_rich_qa&utm_campaign=google_rich_qa
        /// https://msdn.microsoft.com/en-us/library/aa394173(v=vs.85).aspx
        /// </remarks>
        public static List<Drive> CreateListOfDrives(bool ignoreFixedDrives)
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
                .Select(d => new Drive(EnumConverter.ToInternalDriveType(d.DriveType), d))
                .ToList();

            // TODO: Management searcher stuff is not supported on other platforms
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

            // Order the drives by drive letter
            drives = drives.OrderBy(i => i.Letter).ToList();

            return drives;
        }

        /// <summary>
        /// Get the current media type from drive letter
        /// </summary>
        /// <param name="drive"></param>
        /// <returns></returns>
        /// <remarks>
        /// This may eventually be replaced by Aaru.Devices being able to be about 10x more accurate.
        /// This will also end up making it so that IMAPI2 is no longer necessary. Unfortunately, that
        /// will only work for .NET Core 3.1 and beyond.
        /// </remarks>
        public (MediaType?, string) GetMediaType()
        {
            // Take care of the non-optical stuff first
            // TODO: See if any of these can be more granular, like Optical is
            if (this.InternalDriveType == Data.InternalDriveType.Floppy)
                return (MediaType.FloppyDisk, null);
            else if (this.InternalDriveType == Data.InternalDriveType.HardDisk)
                return (MediaType.HardDisk, null);
            else if (this.InternalDriveType == Data.InternalDriveType.Removable)
                return (MediaType.FlashDrive, null);

#if NET_FRAMEWORK
            // Get the current drive information
            string deviceId = null;
            bool loaded = false;
            try
            {
                // Get the device ID first
                var searcher = new ManagementObjectSearcher(
                    "root\\CIMV2",
                    $"SELECT * FROM Win32_CDROMDrive WHERE Id = '{this.Letter}:\'");

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
#else
            try
            {
                var device = new AaruDevices.Device(this.Name);
                if (device.Error)
                    return (null, "Could not open device");
                else if (device.Type != DeviceType.ATAPI && device.Type != DeviceType.SCSI)
                    return (null, "Device does not support media type detection");

                // TODO: In order to get the disc type, Aaru.Core will need to be included as a
                // package. Unfortunately, it currently has a conflict with one of the required libraries:
                // System.Text.Encoding.CodePages (BOS uses >= 5.0.0, DotNetZip uses >= 4.5.0 && < 5.0.0)
            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }

            return (null, "Media detection only supported on .NET Framework");
#endif
        }

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

            #region Consoles

            // Microsoft Xbox 360
            try
            {
                if (Directory.Exists(Path.Combine(drivePath, "$SystemUpdate"))
                    && Directory.EnumerateFiles(Path.Combine(drivePath, "$SystemUpdate")).Any())
                {
                    return RedumpSystem.MicrosoftXbox360;
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
            if (string.IsNullOrEmpty(this.driveInfo?.Name))
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
    }
}
