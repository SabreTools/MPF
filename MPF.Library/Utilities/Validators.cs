using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BurnOutSharp;
using MPF.Converters;
using MPF.Data;
using RedumpLib.Data;
#if NET_FRAMEWORK
using IMAPI2;
#else
using Aaru.CommonTypes.Enums;
using AaruDevices = Aaru.Devices;
#endif

namespace MPF.Utilities
{
    public static class Validators
    {
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

        /// <summary>
        /// Run protection scan on a given dump environment
        /// </summary>
        /// <param name="path">Path to scan for protection</param>
        /// <param name="options">Options object that determines what to scan</param>
        /// <param name="progress">Optional progress callback</param>
        /// <returns>TCopy protection detected in the envirionment, if any</returns>
        public static async Task<(bool, string)> RunProtectionScanOnPath(string path, Options options, IProgress<ProtectionProgress> progress = null)
        {
            try
            {
                var found = await Task.Run(() =>
                {
                    var scanner = new Scanner(progress)
                    {
                        IncludeDebug = options.IncludeDebugProtectionInformation,
                        ScanAllFiles = options.ForceScanningForProtection,
                        ScanArchives = options.ScanArchivesForProtection,
                        ScanPackers = options.ScanPackersForProtection,
                    };
                    return scanner.GetProtections(path);
                });

                if (found == null || found.Count() == 0)
                    return (true, "None found");

                // Get an ordered list of distinct found protections
                var orderedDistinctProtections = found
                    .Where(kvp => kvp.Value != null && kvp.Value.Any())
                    .SelectMany(kvp => kvp.Value)
                    .Distinct()
                    .OrderBy(p => p);

                // Sanitize and join protections for writing
                string protections = SanitizeFoundProtections(orderedDistinctProtections);
                return (true, protections);
            }
            catch (Exception ex)
            {
                return (false, ex.ToString());
            }
        }

        /// <summary>
        /// Sanitize unnecessary protection duplication from output
        /// </summary>
        /// <param name="foundProtections">Enumerable of found protections</param>
        private static string SanitizeFoundProtections(IEnumerable<string> foundProtections)
        {
            // ActiveMARK
            if (foundProtections.Any(p => p == "ActiveMARK 5") && foundProtections.Any(p => p == "ActiveMARK"))
                foundProtections = foundProtections.Where(p => p != "ActiveMARK");

            // Cactus Data Shield
            if (foundProtections.Any(p => Regex.IsMatch(p, @"Cactus Data Shield [0-9]{3} .+")) && foundProtections.Any(p => p == "Cactus Data Shield 200"))
                foundProtections = foundProtections.Where(p => p != "Cactus Data Shield 200");

            // CD-Check
            foundProtections = foundProtections.Where(p => p != "Executable-Based CD Check");

            // CD-Cops
            if (foundProtections.Any(p => p == "CD-Cops") && foundProtections.Any(p => p.StartsWith("CD-Cops") && p.Length > "CD-Cops".Length))
                foundProtections = foundProtections.Where(p => p != "CD-Cops");

            // CD-Key / Serial
            foundProtections = foundProtections.Where(p => p != "CD-Key / Serial");

            // Electronic Arts
            if (foundProtections.Any(p => p == "EA CdKey Registration Module") && foundProtections.Any(p => p.StartsWith("EA CdKey Registration Module") && p.Length > "EA CdKey Registration Module".Length))
                foundProtections = foundProtections.Where(p => p != "EA CdKey Registration Module");
            if (foundProtections.Any(p => p == "EA DRM Protection") && foundProtections.Any(p => p.StartsWith("EA DRM Protection") && p.Length > "EA DRM Protection".Length))
                foundProtections = foundProtections.Where(p => p != "EA DRM Protection");

            // Games for Windows LIVE
            if (foundProtections.Any(p => p == "Games for Windows LIVE") && foundProtections.Any(p => p.StartsWith("Games for Windows LIVE") && !p.Contains("Zero Day Piracy Protection") && p.Length > "Games for Windows LIVE".Length))
                foundProtections = foundProtections.Where(p => p != "Games for Windows LIVE");

            // Impulse Reactor
            if (foundProtections.Any(p => p.StartsWith("Impulse Reactor Core Module")) && foundProtections.Any(p => p == "Impulse Reactor"))
                foundProtections = foundProtections.Where(p => p != "Impulse Reactor");

            // JoWood X-Prot
            if (foundProtections.Any(p => p.StartsWith("JoWood X-Prot")))
            {
                if (foundProtections.Any(p => Regex.IsMatch(p, @"JoWood X-Prot [0-9]\.[0-9]\.[0-9]\.[0-9]{2}")))
                {
                    foundProtections = foundProtections.Where(p => p != "JoWood X-Prot");
                    foundProtections = foundProtections.Where(p => p != "JoWood X-Prot v1.0-v1.3");
                    foundProtections = foundProtections.Where(p => p != "JoWood X-Prot v1.4+");
                    foundProtections = foundProtections.Where(p => p != "JoWood X-Prot v2");
                }
                else if (foundProtections.Any(p => p == "JoWood X-Prot v2"))
                {
                    foundProtections = foundProtections.Where(p => p != "JoWood X-Prot");
                    foundProtections = foundProtections.Where(p => p != "JoWood X-Prot v1.0-v1.3");
                    foundProtections = foundProtections.Where(p => p != "JoWood X-Prot v1.4+");
                }
                else if (foundProtections.Any(p => p == "JoWood X-Prot v1.4+"))
                {
                    foundProtections = foundProtections.Where(p => p != "JoWood X-Prot");
                    foundProtections = foundProtections.Where(p => p != "JoWood X-Prot v1.0-v1.3");
                }
                else if (foundProtections.Any(p => p == "JoWood X-Prot v1.0-v1.3"))
                {
                    foundProtections = foundProtections.Where(p => p != "JoWood X-Prot");
                }
            }

            // LaserLok
            // TODO: Figure this one out

            // Online RegistrationBu
            foundProtections = foundProtections.Where(p => p.StartsWith("Executable-Based Online Registration"));

            // ProtectDISC / VOB ProtectCD/DVD
            // TODO: Figure this one out

            // SafeCast
            // TODO: Figure this one out

            // SafeDisc
            if (foundProtections.Any(p => p.StartsWith("SafeDisc")))
            {
                if (foundProtections.Any(p => Regex.IsMatch(p, @"SafeDisc [0-9]\.[0-9]{2}\.[0-9]{3}")))
                {
                    foundProtections = foundProtections.Where(p => p != "SafeDisc");
                    foundProtections = foundProtections.Where(p => p != "SafeDisc 1/Lite");
                    foundProtections = foundProtections.Where(p => p != "SafeDisc 1-3");
                    foundProtections = foundProtections.Where(p => p != "SafeDisc 2");
                    foundProtections = foundProtections.Where(p => p != "SafeDisc 3.20-4.xx (version removed)");
                    foundProtections = foundProtections.Where(p => !p.StartsWith("SafeDisc (dplayerx.dll)"));
                    foundProtections = foundProtections.Where(p => !p.StartsWith("SafeDisc (drvmgt.dll)"));
                    foundProtections = foundProtections.Where(p => !p.StartsWith("SafeDisc (secdrv.sys)"));
                    foundProtections = foundProtections.Where(p => p != "SafeDisc Lite");
                }
                else if (foundProtections.Any(p => p.StartsWith("SafeDisc (drvmgt.dll)")))
                {
                    foundProtections = foundProtections.Where(p => p != "SafeDisc");
                    foundProtections = foundProtections.Where(p => p != "SafeDisc 1/Lite");
                    foundProtections = foundProtections.Where(p => p != "SafeDisc 1-3");
                    foundProtections = foundProtections.Where(p => p != "SafeDisc 2");
                    foundProtections = foundProtections.Where(p => p != "SafeDisc 3.20-4.xx (version removed)");
                    foundProtections = foundProtections.Where(p => !p.StartsWith("SafeDisc (dplayerx.dll)"));
                    foundProtections = foundProtections.Where(p => !p.StartsWith("SafeDisc (secdrv.sys)"));
                    foundProtections = foundProtections.Where(p => p != "SafeDisc Lite");
                }
                else if (foundProtections.Any(p => p.StartsWith("SafeDisc (secdrv.sys)")))
                {
                    foundProtections = foundProtections.Where(p => p != "SafeDisc");
                    foundProtections = foundProtections.Where(p => p != "SafeDisc 1/Lite");
                    foundProtections = foundProtections.Where(p => p != "SafeDisc 1-3");
                    foundProtections = foundProtections.Where(p => p != "SafeDisc 2");
                    foundProtections = foundProtections.Where(p => p != "SafeDisc 3.20-4.xx (version removed)");
                    foundProtections = foundProtections.Where(p => !p.StartsWith("SafeDisc (dplayerx.dll)"));
                    foundProtections = foundProtections.Where(p => p != "SafeDisc Lite");
                }
                else if (foundProtections.Any(p => p.StartsWith("SafeDisc (dplayerx.dll)")))
                {
                    foundProtections = foundProtections.Where(p => p != "SafeDisc");
                    foundProtections = foundProtections.Where(p => p != "SafeDisc 1/Lite");
                    foundProtections = foundProtections.Where(p => p != "SafeDisc 1-3");
                    foundProtections = foundProtections.Where(p => p != "SafeDisc 2");
                    foundProtections = foundProtections.Where(p => p != "SafeDisc 3.20-4.xx (version removed)");
                    foundProtections = foundProtections.Where(p => p != "SafeDisc Lite");
                }
                else if (foundProtections.Any(p => p == "SafeDisc 3.20-4.xx (version removed)"))
                {
                    foundProtections = foundProtections.Where(p => p != "SafeDisc");
                    foundProtections = foundProtections.Where(p => p != "SafeDisc 1/Lite");
                    foundProtections = foundProtections.Where(p => p != "SafeDisc 1-3");
                    foundProtections = foundProtections.Where(p => p != "SafeDisc 2");
                    foundProtections = foundProtections.Where(p => p != "SafeDisc Lite");
                }
                else if (foundProtections.Any(p => p == "SafeDisc 2"))
                {
                    foundProtections = foundProtections.Where(p => p != "SafeDisc");
                    foundProtections = foundProtections.Where(p => p != "SafeDisc 1/Lite");
                    foundProtections = foundProtections.Where(p => p != "SafeDisc 1-3");
                    foundProtections = foundProtections.Where(p => p != "SafeDisc Lite");
                }
                else if (foundProtections.Any(p => p == "SafeDisc 1/Lite"))
                {
                    foundProtections = foundProtections.Where(p => p != "SafeDisc");
                    foundProtections = foundProtections.Where(p => p != "SafeDisc 1-3");
                    foundProtections = foundProtections.Where(p => p != "SafeDisc Lite");
                }
                else if (foundProtections.Any(p => p == "SafeDisc Lite"))
                {
                    foundProtections = foundProtections.Where(p => p != "SafeDisc");
                    foundProtections = foundProtections.Where(p => p != "SafeDisc 1-3");
                }
                else if (foundProtections.Any(p => p == "SafeDisc 1-3"))
                {
                    foundProtections = foundProtections.Where(p => p != "SafeDisc");
                }
            }

            // SecuROM
            // TODO: Figure this one out

            // SolidShield
            // TODO: Figure this one out

            // StarForce
            // TODO: Figure this one out

            // Sysiphus
            if (foundProtections.Any(p => p == "Sysiphus") && foundProtections.Any(p => p.StartsWith("Sysiphus") && p.Length > "Sysiphus".Length))
                foundProtections = foundProtections.Where(p => p != "Sysiphus");

            // TAGES
            // TODO: Figure this one out

            // XCP
            if (foundProtections.Any(p => p == "XCP") && foundProtections.Any(p => p.StartsWith("XCP") && p.Length > "XCP".Length))
                foundProtections = foundProtections.Where(p => p != "XCP");

            return string.Join(", ", foundProtections);
        }
    }
}
