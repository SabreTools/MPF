using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Reflection;
using MPF.Core.Converters;
using MPF.Core.Data;
using Newtonsoft.Json.Linq;
using RedumpLib.Data;
#if NET_FRAMEWORK
using IMAPI2;
#else
using Aaru.CommonTypes.Enums;
using AaruDevices = Aaru.Devices;
#endif

namespace MPF.Core.Utilities
{
    public static class Tools
    {
        #region Byte Arrays

        /// <summary>
        /// Search for a byte array in another array
        /// </summary>
        public static bool Contains(this byte[] stack, byte[] needle, out int position, int start = 0, int end = -1)
        {
            // Initialize the found position to -1
            position = -1;

            // If either array is null or empty, we can't do anything
            if (stack == null || stack.Length == 0 || needle == null || needle.Length == 0)
                return false;

            // If the needle array is larger than the stack array, it can't be contained within
            if (needle.Length > stack.Length)
                return false;

            // If start or end are not set properly, set them to defaults
            if (start < 0)
                start = 0;
            if (end < 0)
                end = stack.Length - needle.Length;

            for (int i = start; i < end; i++)
            {
                if (stack.EqualAt(needle, i))
                {
                    position = i;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// See if a byte array starts with another
        /// </summary>
        public static bool StartsWith(this byte[] stack, byte[] needle)
        {
            return stack.Contains(needle, out int _, start: 0, end: 1);
        }

        /// <summary>
        /// Get if a stack at a certain index is equal to a needle
        /// </summary>
        private static bool EqualAt(this byte[] stack, byte[] needle, int index)
        {
            // If we're too close to the end of the stack, return false
            if (needle.Length >= stack.Length - index)
                return false;

            for (int i = 0; i < needle.Length; i++)
            {
                if (stack[i + index] != needle[i])
                    return false;
            }

            return true;
        }

        #endregion

        #region Physical Access

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
                        drives.ForEach(d => { if (d.Letter == devId) { d.InternalDriveType = InternalDriveType.Floppy; } });
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
        public static (MediaType?, string) GetMediaType(Drive drive)
        {
            // Take care of the non-optical stuff first
            // TODO: See if any of these can be more granular, like Optical is
            if (drive.InternalDriveType == InternalDriveType.Floppy)
                return (MediaType.FloppyDisk, null);
            else if (drive.InternalDriveType == InternalDriveType.HardDisk)
                return (MediaType.HardDisk, null);
            else if (drive.InternalDriveType == InternalDriveType.Removable)
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
                    $"SELECT * FROM Win32_CDROMDrive WHERE Id = '{drive.Letter}:\'");

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
                var device = new AaruDevices.Device(drive.Name);
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
        /// <param name="drive"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static RedumpSystem? GetRedumpSystem(Drive drive, RedumpSystem? defaultValue)
        {
            // If drive or drive letter are provided, we can't do anything
            if (drive?.Letter == null)
                return defaultValue;

            string drivePath = $"{drive.Letter}:\\";

            // If we can't read the media in that drive, we can't do anything
            if (!Directory.Exists(drivePath))
                return defaultValue;

            // We're going to assume for floppies, HDDs, and removable drives
            // TODO: Try to be smarter about this
            if (drive.InternalDriveType != InternalDriveType.Optical)
                return RedumpSystem.IBMPCcompatible;

            // Audio CD
            if (drive.VolumeLabel.Equals("Audio CD", StringComparison.OrdinalIgnoreCase))
            {
                return RedumpSystem.AudioCD;
            }

            // DVD-Audio
            if (Directory.Exists(Path.Combine(drivePath, "AUDIO_TS"))
                && Directory.EnumerateFiles(Path.Combine(drivePath, "AUDIO_TS")).Count() > 0)
            {
                return RedumpSystem.DVDAudio;
            }

            // DVD-Video and Xbox
            if (Directory.Exists(Path.Combine(drivePath, "VIDEO_TS"))
                && Directory.EnumerateFiles(Path.Combine(drivePath, "VIDEO_TS")).Count() > 0)
            {
                // TODO: Maybe add video track hashes to compare for Xbox and X360?
                if (drive.VolumeLabel.StartsWith("SEP13011042", StringComparison.OrdinalIgnoreCase))
                    return RedumpSystem.MicrosoftXbox;

                return RedumpSystem.DVDVideo;
            }

            // HD-DVD-Video
            if (Directory.Exists(Path.Combine(drivePath, "HVDVD_TS"))
                && Directory.EnumerateFiles(Path.Combine(drivePath, "HVDVD_TS")).Count() > 0)
            {
                return RedumpSystem.HDDVDVideo;
            }

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
                byte[] sector = drive?.ReadSector(0);
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
            if (drive.VolumeLabel.Equals("PS3VOLUME", StringComparison.OrdinalIgnoreCase))
            {
                return RedumpSystem.SonyPlayStation3;
            }

            // Sony PlayStation 4
            if (drive.VolumeLabel.Equals("PS4VOLUME", StringComparison.OrdinalIgnoreCase))
            {
                return RedumpSystem.SonyPlayStation4;
            }

            // Sony PlayStation 5
            if (drive.VolumeLabel.Equals("PS5VOLUME", StringComparison.OrdinalIgnoreCase))
            {
                return RedumpSystem.SonyPlayStation5;
            }

            // V.Tech V.Flash / V.Smile Pro
            if (File.Exists(Path.Combine(drivePath, "0SYSTEM")))
            {
                return RedumpSystem.VTechVFlashVSmilePro;
            }

            // VCD
            if (Directory.Exists(Path.Combine(drivePath, "VCD"))
                && Directory.EnumerateFiles(Path.Combine(drivePath, "VCD")).Count() > 0)
            {
                return RedumpSystem.VideoCD;
            }

            // Default return
            return defaultValue;
        }

        #endregion

        #region Support

        /// <summary>
        /// Verify that, given a system and a media type, they are correct
        /// </summary>
        public static Result GetSupportStatus(RedumpSystem? system, MediaType? type)
        {
            // No system chosen, update status
            if (system == null)
                return Result.Failure("Please select a valid system");

            // If we're on an unsupported type, update the status accordingly
            switch (type)
            {
                // Fully supported types
                case MediaType.BluRay:
                case MediaType.CDROM:
                case MediaType.DVD:
                case MediaType.FloppyDisk:
                case MediaType.HardDisk:
                case MediaType.CompactFlash:
                case MediaType.SDCard:
                case MediaType.FlashDrive:
                case MediaType.HDDVD:
                    return Result.Success($"{type.LongName()} ready to dump");

                // Partially supported types
                case MediaType.GDROM:
                case MediaType.NintendoGameCubeGameDisc:
                case MediaType.NintendoWiiOpticalDisc:
                    return Result.Success($"{type.LongName()} partially supported for dumping");

                // Special case for other supported tools
                case MediaType.UMD:
                    return Result.Failure($"{type.LongName()} supported for submission info parsing");

                // Specifically unknown type
                case MediaType.NONE:
                    return Result.Failure($"Please select a valid media type");

                // Undumpable but recognized types
                default:
                    return Result.Failure($"{type.LongName()} media are not supported for dumping");
            }
        }

        #endregion

        #region Versioning

        /// <summary>
        /// Check for a new MPF version
        /// </summary>
        /// <returns>
        /// Bool representing if the values are different.
        /// String representing the message to display the the user.
        /// String representing the new release URL.
        /// </returns>
        public static (bool different, string message, string url) CheckForNewVersion()
        {
            try
            {
                // Get current assembly version
                var assemblyVersion = Assembly.GetEntryAssembly().GetName().Version;
                string version = $"{assemblyVersion.Major}.{assemblyVersion.Minor}" + (assemblyVersion.Build != 0 ? $".{assemblyVersion.Build}" : string.Empty);

                // Get the latest tag from GitHub
                (string tag, string url) = GetRemoteVersionAndUrl();
                bool different = version != tag;

                string message = $"Local version: {version}"
                    + $"{Environment.NewLine}Remote version: {tag}"
                    + (different
                        ? $"{Environment.NewLine}The update URL has been added copied to your clipboard"
                        : $"{Environment.NewLine}You have the newest version!");

                return (different, message, url);
            }
            catch (Exception ex)
            {
                return (false, ex.ToString(), null);
            }
        }

        /// <summary>
        /// Get the current informational version formatted as a string
        /// </summary>
        public static string GetCurrentVersion()
        {
            var assemblyVersion = Attribute.GetCustomAttribute(Assembly.GetEntryAssembly(), typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute;
            return assemblyVersion.InformationalVersion;
        }

        /// <summary>
        /// Get the latest version of MPF from GitHub and the release URL
        /// </summary>
        private static (string tag, string url) GetRemoteVersionAndUrl()
        {
            using (WebClient wc = new WebClient())
            {
                wc.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:64.0) Gecko/20100101 Firefox/64.0";

                // TODO: Figure out a better way than having this hardcoded...
                string url = "https://api.github.com/repos/SabreTools/MPF/releases/latest";
                string latestReleaseJsonString = wc.DownloadString(url);
                var latestReleaseJson = JObject.Parse(latestReleaseJsonString);
                string latestTag = latestReleaseJson["tag_name"].ToString();
                string releaseUrl = latestReleaseJson["html_url"].ToString();

                return (latestTag, releaseUrl);
            }
        }

        #endregion
    }
}
