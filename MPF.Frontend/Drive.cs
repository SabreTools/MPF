using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
#if NET462_OR_GREATER || NETCOREAPP
using Microsoft.Management.Infrastructure;
using Microsoft.Management.Infrastructure.Generic;
#endif
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MPF.Processors;
using SabreTools.IO;
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
            switch (InternalDriveType)
            {
                case Frontend.InternalDriveType.Floppy:
                    return (MediaType.FloppyDisk, null);
                case Frontend.InternalDriveType.HardDisk:
                    return (MediaType.HardDisk, null);
                case Frontend.InternalDriveType.Removable:
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
            if (TotalSize >= 0 && TotalSize <= 800_000_000 && (DriveFormat == "CDFS" || DriveFormat == "UDF"))
                return (MediaType.CDROM, null);
            else if (TotalSize > 800_000_000 && TotalSize <= 8_540_000_000 && (DriveFormat == "CDFS" || DriveFormat == "UDF"))
                return (MediaType.DVD, null);
            else if (TotalSize > 8_540_000_000)
                return (MediaType.BluRay, null);

            return (null, "Could not determine media type!");
        }

        /// <summary>
        /// Refresh the current drive information based on path
        /// </summary>
        public void RefreshDrive()
        {
            var driveInfo = DriveInfo.GetDrives().FirstOrDefault(d => d?.Name == Name);
            PopulateFromDriveInfo(driveInfo);
        }

        #endregion

        #region Information Extraction

        /// <summary>
        /// Get the EXE name from a PlayStation disc, if possible
        /// </summary>
        /// <returns>Executable name on success, null otherwise</returns>
        public string? GetPlayStationExecutableName()
        {
            // If there's no drive path, we can't get exe name
            if (string.IsNullOrEmpty(Name))
                return null;

            // If the folder no longer exists, we can't get exe name
            if (!Directory.Exists(Name))
                return null;

            // Get the two paths that we will need to check
            string psxExePath = Path.Combine(Name, "PSX.EXE");
            string systemCnfPath = Path.Combine(Name, "SYSTEM.CNF");

            // Read the CNF file as an INI file
            var systemCnf = new IniFile(systemCnfPath);
            string? bootValue = string.Empty;

            // PlayStation uses "BOOT" as the key
            if (systemCnf.ContainsKey("BOOT"))
                bootValue = systemCnf["BOOT"];

            // PlayStation 2 uses "BOOT2" as the key
            if (systemCnf.ContainsKey("BOOT2"))
                bootValue = systemCnf["BOOT2"];

            // If we had any boot value, parse it and get the executable name
            if (!string.IsNullOrEmpty(bootValue))
            {
                var match = Regex.Match(bootValue, @"cdrom.?:\\?(.*)", RegexOptions.Compiled);
                if (match.Groups.Count > 1)
                {
                    string? serial = match.Groups[1].Value;

                    // Some games may have the EXE in a subfolder
                    serial = Path.GetFileName(serial);

                    return serial;
                }
            }

            // If the SYSTEM.CNF value can't be found, try PSX.EXE
            if (File.Exists(psxExePath))
                return "PSX.EXE";

            // If neither can be found, we return null
            return null;
        }

        /// <summary>
        /// Get the EXE date from a PlayStation disc, if possible
        /// </summary>
        /// <param name="serial">Internal disc serial, if possible</param>
        /// <param name="region">Output region, if possible</param>
        /// <param name="date">Output EXE date in "yyyy-mm-dd" format if possible, null on error</param>
        /// <returns>True if information could be determined, false otherwise</returns>
        public bool GetPlayStationExecutableInfo(out string? serial, out Region? region, out string? date)
        {
            serial = null; region = null; date = null;

            // If there's no drive path, we can't do this part
            if (string.IsNullOrEmpty(Name))
                return false;

            // If the folder no longer exists, we can't do this part
            if (!Directory.Exists(Name))
                return false;

            // Get the executable name
            string? exeName = GetPlayStationExecutableName();

            // If no executable found, we can't do this part
            if (exeName == null)
                return false;

            // EXE name may have a trailing `;` after
            // EXE name should always be in all caps
            exeName = exeName
                .Split(';')[0]
                .ToUpperInvariant();

            // Serial is most of the EXE name normalized
            serial = exeName
                .Replace('_', '-')
                .Replace(".", string.Empty);

            // Get the region, if possible
            region = ProcessingTool.GetPlayStationRegion(exeName);

            // Now that we have the EXE name, try to get the fileinfo for it
            string exePath = Path.Combine(Name, exeName);
            if (!File.Exists(exePath))
                return false;

            // Fix the Y2K timestamp issue
            var fi = new FileInfo(exePath);
            var dt = new DateTime(fi.LastWriteTimeUtc.Year >= 1900 && fi.LastWriteTimeUtc.Year < 1920 ? 2000 + fi.LastWriteTimeUtc.Year % 100 : fi.LastWriteTimeUtc.Year,
                fi.LastWriteTimeUtc.Month, fi.LastWriteTimeUtc.Day);
            date = dt.ToString("yyyy-MM-dd");

            return true;
        }

        /// <summary>
        /// Get the version from a PlayStation 2 disc, if possible
        /// </summary>
        /// <returns>Game version if possible, null on error</returns>
        public string? GetPlayStation2Version()
        {
            // If there's no drive path, we can't do this part
            if (string.IsNullOrEmpty(Name))
                return null;

            // If the folder no longer exists, we can't do this part
            if (!Directory.Exists(Name))
                return null;

            // Get the SYSTEM.CNF path to check
            string systemCnfPath = Path.Combine(Name, "SYSTEM.CNF");

            // Try to parse the SYSTEM.CNF file
            var systemCnf = new IniFile(systemCnfPath);
            if (systemCnf.ContainsKey("VER"))
                return systemCnf["VER"];

            // If "VER" can't be found, we can't do much
            return null;
        }

        /// <summary>
        /// Get the internal serial from a PlayStation 3 disc, if possible
        /// </summary>
        /// <returns>Internal disc serial if possible, null on error</returns>
        public string? GetPlayStation3Serial()
        {
            // If there's no drive path, we can't do this part
            if (string.IsNullOrEmpty(Name))
                return null;

            // If the folder no longer exists, we can't do this part
            if (!Directory.Exists(Name))
                return null;

            // Attempt to use PS3_DISC.SFB
            string sfbPath = Path.Combine(Name, "PS3_DISC.SFB");
            if (File.Exists(sfbPath))
            {
                try
                {
                    using var br = new BinaryReader(File.OpenRead(sfbPath));
                    br.BaseStream.Seek(0x220, SeekOrigin.Begin);
                    return new string(br.ReadChars(0x10)).TrimEnd('\0');
                }
                catch
                {
                    // We don't care what the error was
                    return null;
                }
            }

            // Attempt to use PARAM.SFO
#if NET20 || NET35
            string sfoPath = Path.Combine(Path.Combine(Name, "PS3_GAME"), "PARAM.SFO");
#else
            string sfoPath = Path.Combine(Name, "PS3_GAME", "PARAM.SFO");
#endif
            if (File.Exists(sfoPath))
            {
                try
                {
                    using var br = new BinaryReader(File.OpenRead(sfoPath));
                    br.BaseStream.Seek(-0x18, SeekOrigin.End);
                    return new string(br.ReadChars(9)).TrimEnd('\0').Insert(4, "-");
                }
                catch
                {
                    // We don't care what the error was
                    return null;
                }
            }

            return null;
        }

        /// <summary>
        /// Get the version from a PlayStation 3 disc, if possible
        /// </summary>
        /// <returns>Game version if possible, null on error</returns>
        public string? GetPlayStation3Version()
        {
            // If there's no drive path, we can't do this part
            if (string.IsNullOrEmpty(Name))
                return null;

            // If the folder no longer exists, we can't do this part
            if (!Directory.Exists(Name))
                return null;

            // Attempt to use PS3_DISC.SFB
            string sfbPath = Path.Combine(Name, "PS3_DISC.SFB");
            if (File.Exists(sfbPath))
            {
                try
                {
                    using var br = new BinaryReader(File.OpenRead(sfbPath));
                    br.BaseStream.Seek(0x230, SeekOrigin.Begin);
                    var discVersion = new string(br.ReadChars(0x10)).TrimEnd('\0');
                    if (!string.IsNullOrEmpty(discVersion))
                        return discVersion;
                }
                catch
                {
                    // We don't care what the error was
                    return null;
                }
            }

            // Attempt to use PARAM.SFO
#if NET20 || NET35
            string sfoPath = Path.Combine(Path.Combine(Name, "PS3_GAME"), "PARAM.SFO");
#else
            string sfoPath = Path.Combine(Name, "PS3_GAME", "PARAM.SFO");
#endif
            if (File.Exists(sfoPath))
            {
                try
                {
                    using var br = new BinaryReader(File.OpenRead(sfoPath));
                    br.BaseStream.Seek(-0x08, SeekOrigin.End);
                    return new string(br.ReadChars(5)).TrimEnd('\0');
                }
                catch
                {
                    // We don't care what the error was
                    return null;
                }
            }

            return null;
        }

        /// <summary>
        /// Get the firmware version from a PlayStation 3 disc, if possible
        /// </summary>
        /// <returns>Firmware version if possible, null on error</returns>
        public string? GetPlayStation3FirmwareVersion()
        {
            // If there's no drive path, we can't do this part
            if (string.IsNullOrEmpty(Name))
                return null;

            // If the folder no longer exists, we can't do this part
            if (!Directory.Exists(Name))
                return null;

            // Attempt to read from /PS3_UPDATE/PS3UPDAT.PUP
#if NET20 || NET35
            string pupPath = Path.Combine(Path.Combine(Name, "PS3_UPDATE"), "PS3UPDAT.PUP");
#else
            string pupPath = Path.Combine(Name, "PS3_UPDATE", "PS3UPDAT.PUP");
#endif
            if (!File.Exists(pupPath))
                return null;

            try
            {
                using var br = new BinaryReader(File.OpenRead(pupPath));
                br.BaseStream.Seek(0x3E, SeekOrigin.Begin);
                byte[] buf = new byte[2];
                br.Read(buf, 0, 2);
                Array.Reverse(buf);
                short location = BitConverter.ToInt16(buf, 0);
                br.BaseStream.Seek(location, SeekOrigin.Begin);
                return new string(br.ReadChars(4));
            }
            catch
            {
                // We don't care what the error was
                return null;
            }
        }

        /// <summary>
        /// Get the internal serial from a PlayStation 4 disc, if possible
        /// </summary>
        /// <returns>Internal disc serial if possible, null on error</returns>
        public string? GetPlayStation4Serial()
        {
            // If there's no drive path, we can't do this part
            if (string.IsNullOrEmpty(Name))
                return null;

            // If the folder no longer exists, we can't do this part
            if (!Directory.Exists(Name))
                return null;

            // If we can't find param.sfo, we don't have a PlayStation 4 disc
#if NET20 || NET35
            string paramSfoPath = Path.Combine(Path.Combine(Name, "bd"), "param.sfo");
#else
            string paramSfoPath = Path.Combine(Name, "bd", "param.sfo");
#endif
            if (!File.Exists(paramSfoPath))
                return null;

            // Let's try reading param.sfo to find the serial at the end of the file
            try
            {
                using var br = new BinaryReader(File.OpenRead(paramSfoPath));
                br.BaseStream.Seek(-0x14, SeekOrigin.End);
                return new string(br.ReadChars(9)).Insert(4, "-");
            }
            catch
            {
                // We don't care what the error was
                return null;
            }
        }

        /// <summary>
        /// Get the version from a PlayStation 4 disc, if possible
        /// </summary>
        /// <returns>Game version if possible, null on error</returns>
        public string? GetPlayStation4Version()
        {
            // If there's no drive path, we can't do this part
            if (string.IsNullOrEmpty(Name))
                return null;

            // If the folder no longer exists, we can't do this part
            if (!Directory.Exists(Name))
                return null;

            // If we can't find param.sfo, we don't have a PlayStation 4 disc
#if NET20 || NET35
            string paramSfoPath = Path.Combine(Path.Combine(Name, "bd"), "param.sfo");
#else
            string paramSfoPath = Path.Combine(Name, "bd", "param.sfo");
#endif
            if (!File.Exists(paramSfoPath))
                return null;

            // Let's try reading param.sfo to find the version at the end of the file
            try
            {
                using var br = new BinaryReader(File.OpenRead(paramSfoPath));
                br.BaseStream.Seek(-0x08, SeekOrigin.End);
                return new string(br.ReadChars(5));
            }
            catch
            {
                // We don't care what the error was
                return null;
            }
        }

        /// <summary>
        /// Get the internal serial from a PlayStation 5 disc, if possible
        /// </summary>
        /// <returns>Internal disc serial if possible, null on error</returns>
        public string? GetPlayStation5Serial()
        {
            // Attempt to get the param.json file
            var json = GetPlayStation5ParamsJsonFromDrive();
            if (json == null)
                return null;

            try
            {
                return json["disc"]?[0]?["masterDataId"]?.Value<string>()?.Insert(4, "-");
            }
            catch
            {
                // We don't care what the error was
                return null;
            }
        }

        // <summary>
        /// Get the version from a PlayStation 5 disc, if possible
        /// </summary>
        /// <returns>Game version if possible, null on error</returns>
        public string? GetPlayStation5Version()
        {
            // Attempt to get the param.json file
            var json = GetPlayStation5ParamsJsonFromDrive();
            if (json == null)
                return null;

            try
            {
                return json["masterVersion"]?.Value<string>();
            }
            catch
            {
                // We don't care what the error was
                return null;
            }
        }

        /// <summary>
        /// Get the params.json file from a drive path, if possible
        /// </summary>
        /// <returns>JObject representing the JSON on success, null on error</returns>
        private JObject? GetPlayStation5ParamsJsonFromDrive()
        {
            // If there's no drive path, we can't do this part
            if (string.IsNullOrEmpty(Name))
                return null;

            // If the folder no longer exists, we can't do this part
            if (!Directory.Exists(Name))
                return null;

            // If we can't find param.json, we don't have a PlayStation 5 disc
#if NET20 || NET35
            string paramJsonPath = Path.Combine(Path.Combine(Name, "bd"), "param.json");
#else
            string paramJsonPath = Path.Combine(Name, "bd", "param.json");
#endif
            return GetPlayStation5ParamsJsonFromFile(paramJsonPath);
        }

        /// <summary>
        /// Get the params.json file from a filename, if possible
        /// </summary>
        /// <param name="filename">Filename to check</param>
        /// <returns>JObject representing the JSON on success, null on error</returns>
        private static JObject? GetPlayStation5ParamsJsonFromFile(string? filename)
        {
            // If the file doesn't exist
            if (string.IsNullOrEmpty(filename) || !File.Exists(filename))
                return null;

            // Let's try reading param.json to find the version in the unencrypted JSON
            try
            {
                using var br = new BinaryReader(File.OpenRead(filename));
                br.BaseStream.Seek(0x800, SeekOrigin.Begin);
                byte[] jsonBytes = br.ReadBytes((int)(br.BaseStream.Length - 0x800));
                return JsonConvert.DeserializeObject(Encoding.ASCII.GetString(jsonBytes)) as JObject;
            }
            catch
            {
                // We don't care what the error was
                return null;
            }
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
                    .Select(d => Create(ToInternalDriveType(d.DriveType), d.Name) ?? new Drive())
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
                        drives.ForEach(d => { if (d?.Name != null && d.Name[0] == devId) { d.InternalDriveType = Frontend.InternalDriveType.Floppy; } });
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

        /// <summary>
        /// Convert drive type to internal version, if possible
        /// </summary>
        /// <param name="driveType">DriveType value to check</param>
        /// <returns>InternalDriveType, if possible, null on error</returns>
        internal static InternalDriveType? ToInternalDriveType(DriveType driveType)
        {
            return driveType switch
            {
                DriveType.CDRom => (InternalDriveType?)Frontend.InternalDriveType.Optical,
                DriveType.Fixed => (InternalDriveType?)Frontend.InternalDriveType.HardDisk,
                DriveType.Removable => (InternalDriveType?)Frontend.InternalDriveType.Removable,
                _ => null,
            };
        }

        #endregion
    }
}
