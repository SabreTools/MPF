using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using IMAPI2;
using DICUI.Data;
using DICUI.External;

namespace DICUI.Utilities
{
    public static class Validators
    {
        /// <summary>
        /// Get a list of valid MediaTypes for a given system matched to their respective names
        /// </summary>
        /// <param name="sys">KnownSystem value to check</param>
        /// <returns>MediaTypes matched to enums, if possible</returns>
        /// <remarks>
        ///	If something has a "string, null" value, it should be assumed that it is a separator
        /// </remarks>
        public static OrderedDictionary<string, MediaType?> GetValidMediaTypes(KnownSystem? sys)
        {
            var types = new List<MediaType?>();
            var typesDict = new OrderedDictionary<string, MediaType?>();

            switch (sys)
            {
                #region Consoles

                case KnownSystem.BandaiPlaydiaQuickInteractiveSystem:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.BandaiApplePippin:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.CommodoreAmigaCD32:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.CommodoreAmigaCDTV:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.MattelHyperscan:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.MicrosoftXBOX:
                    types.Add(MediaType.CD);
                    types.Add(MediaType.DVD);
                    break;
                case KnownSystem.MicrosoftXBOX360XDG2:
                    types.Add(MediaType.CD);
                    types.Add(MediaType.DVD);
                    break;
                case KnownSystem.MicrosoftXBOX360XDG3:
                    types.Add(MediaType.CD);
                    types.Add(MediaType.DVD);
                    types.Add(MediaType.HDDVD);
                    break;
                case KnownSystem.MicrosoftXBOXOne:
                    types.Add(MediaType.BluRay);
                    break;
                case KnownSystem.NECPCEngineTurboGrafxCD:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.NECPCFX:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.NintendoGameCube:
                    types.Add(MediaType.GameCubeGameDisc);
                    break;
                case KnownSystem.NintendoWii:
                    types.Add(MediaType.WiiOpticalDisc);
                    break;
                case KnownSystem.NintendoWiiU:
                    types.Add(MediaType.WiiUOpticalDisc);
                    break;
                case KnownSystem.Panasonic3DOInteractiveMultiplayer:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.PhilipsCDi:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.SegaCDMegaCD:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.SegaDreamcast:
                    types.Add(MediaType.GDROM);
                    break;
                case KnownSystem.SegaSaturn:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.SNKNeoGeoCD:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.SonyPlayStation:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.SonyPlayStation2:
                    types.Add(MediaType.CD);
                    types.Add(MediaType.DVD);
                    break;
                case KnownSystem.SonyPlayStation3:
                    types.Add(MediaType.BluRay);
                    break;
                case KnownSystem.SonyPlayStation4:
                    types.Add(MediaType.BluRay);
                    break;
                case KnownSystem.SonyPlayStationPortable:
                    types.Add(MediaType.UMD);
                    break;
                case KnownSystem.VMLabsNuon:
                    types.Add(MediaType.DVD);
                    break;
                case KnownSystem.VTechVFlashVSmilePro:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.ZAPiTGamesGameWaveFamilyEntertainmentSystem:
                    types.Add(MediaType.DVD);
                    break;

                #endregion

                #region Computers

                case KnownSystem.AcornArchimedes:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.AppleMacintosh:
                    types.Add(MediaType.CD);
                    types.Add(MediaType.DVD);
                    types.Add(MediaType.Floppy);
                    break;
                case KnownSystem.CommodoreAmigaCD:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.FujitsuFMTowns:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.IBMPCCompatible:
                    types.Add(MediaType.CD);
                    types.Add(MediaType.DVD);
                    types.Add(MediaType.Floppy);
                    break;
                case KnownSystem.NECPC88:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.NECPC98:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.SharpX68000:
                    types.Add(MediaType.CD);
                    break;

                #endregion

                #region Arcade

                case KnownSystem.AmigaCUBOCD32:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.AmericanLaserGames3DO:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.Atari3DO:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.Atronic:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.AUSCOMSystem1:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.BallyGameMagic:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.CapcomCPSystemIII:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.GlobalVRVarious:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.GlobalVRVortek:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.GlobalVRVortekV3:
                    types.Add(MediaType.DVD); // TODO: Confirm
                    break;
                case KnownSystem.ICEPCHardware:
                    types.Add(MediaType.DVD);
                    break;
                case KnownSystem.IncredibleTechnologiesEagle:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.IncredibleTechnologiesVarious:
                    types.Add(MediaType.CD);
                    types.Add(MediaType.DVD);
                    break;
                case KnownSystem.KonamiFirebeat:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.KonamiGVSystem:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.KonamiM2:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.KonamiPython:
                    types.Add(MediaType.DVD); // TODO: Confirm
                    break;
                case KnownSystem.KonamiPython2:
                    types.Add(MediaType.DVD); // TODO: Confirm
                    break;
                case KnownSystem.KonamiSystem573:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.KonamiTwinkle:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.KonamiVarious:
                    types.Add(MediaType.CD);
                    types.Add(MediaType.DVD);
                    break;
                case KnownSystem.MeritIndustriesBoardwalk:
                    types.Add(MediaType.CD); // TODO: Confirm
                    break;
                case KnownSystem.MeritIndustriesMegaTouchAurora:
                    types.Add(MediaType.CD); // TODO: Confirm
                    break;
                case KnownSystem.MeritIndustriesMegaTouchForce:
                    types.Add(MediaType.CD);
                    types.Add(MediaType.DVD);
                    break;
                case KnownSystem.MeritIndustriesMegaTouchION:
                    types.Add(MediaType.CD);
                    types.Add(MediaType.DVD);
                    break;
                case KnownSystem.MeritIndustriesMegaTouchMaxx:
                    types.Add(MediaType.DVD);
                    break;
                case KnownSystem.MeritIndustriesMegaTouchXL:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.NamcoCapcomSystem256:
                    types.Add(MediaType.DVD);
                    break;
                case KnownSystem.NamcoCapcomTaitoSystem246:
                    types.Add(MediaType.DVD);
                    break;
                case KnownSystem.NamcoSegaNintendoTriforce:
                    types.Add(MediaType.GDROM);
                    break;
                case KnownSystem.NamcoSystem12:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.NamcoSystem357:
                    types.Add(MediaType.DVD);
                    types.Add(MediaType.BluRay);
                    break;
                case KnownSystem.NewJatreCDi:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.NichibutsuHighRateSystem:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.NichibutsuSuperCD:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.NichibutsuXRateSystem:
                    types.Add(MediaType.DVD);
                    break;
                case KnownSystem.PhotoPlayVarious:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.RawThrillsVarious:
                    types.Add(MediaType.DVD);
                    break;
                case KnownSystem.SegaChihiro:
                    types.Add(MediaType.GDROM);
                    break;
                case KnownSystem.SegaEuropaR:
                    types.Add(MediaType.DVD); // TODO: Confirm
                    break;
                case KnownSystem.SegaLindbergh:
                    types.Add(MediaType.DVD);
                    break;
                case KnownSystem.SegaNaomi:
                    types.Add(MediaType.GDROM);
                    break;
                case KnownSystem.SegaNaomi2:
                    types.Add(MediaType.GDROM);
                    break;
                case KnownSystem.SegaNu:
                    types.Add(MediaType.DVD);
                    types.Add(MediaType.BluRay);
                    break;
                case KnownSystem.SegaRingEdge:
                    types.Add(MediaType.DVD);
                    break;
                case KnownSystem.SegaRingEdge2:
                    types.Add(MediaType.DVD);
                    break;
                case KnownSystem.SegaRingWide:
                    types.Add(MediaType.DVD);
                    break;
                case KnownSystem.SegaSTV:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.SegaSystem32:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.SeibuCATSSystem:
                    types.Add(MediaType.DVD);
                    break;
                case KnownSystem.TABAustriaQuizard:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.TandyMemorexVisualInformationSystem:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.TsunamiTsuMoMultiGameMotionSystem:
                    types.Add(MediaType.CD);
                    break;

                #endregion

                #region Others

                case KnownSystem.AudioCD:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.BDVideo:
                    types.Add(MediaType.BluRay);
                    break;
                case KnownSystem.DVDVideo:
                    types.Add(MediaType.DVD);
                    break;
                case KnownSystem.EnhancedCD:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.HDDVDVideo:
                    types.Add(MediaType.HDDVD);
                    break;
                case KnownSystem.PalmOS:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.PhilipsCDiDigitalVideo:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.PhotoCD:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.PlayStationGameSharkUpdates:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.TaoiKTV:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.TomyKissSite:
                    types.Add(MediaType.CD);
                    break;
                case KnownSystem.VideoCD:
                    types.Add(MediaType.CD);
                    break;

                #endregion

                case KnownSystem.NONE:
                default:
                    types.Add(MediaType.NONE);
                    break;
            }

            // Populate the dictionary
            foreach (var type in types)
            {
                typesDict.Add(Converters.MediaTypeToString(type), type);
            }

            return typesDict;
        }

        /// <summary>
        /// Create a list of systems matched to their respective enums
        /// </summary>
        /// <returns>Systems matched to enums, if possible</returns>
        /// <remarks>
        ///	If something has a "string, null" value, it should be assumed that it is a separator
        /// </remarks>
        /// TODO: Figure out a way that the sections can be generated more automatically
        public static OrderedDictionary<string, KnownSystem?> CreateListOfSystems()
        {
            var systemsDict = new OrderedDictionary<string, KnownSystem?>();

            foreach (KnownSystem system in Enum.GetValues(typeof(KnownSystem)))
            {
                // In the special cases of breaks, we want to add the proper mappings for sections
                switch (system)
                {
                    // Consoles section
                    case KnownSystem.BandaiPlaydiaQuickInteractiveSystem:
                        systemsDict.Add("---------- Consoles ----------", null);
                        break;

                    // Computers section
                    case KnownSystem.AcornArchimedes:
                        systemsDict.Add("---------- Computers ----------", null);
                        break;

                    // Arcade section
                    case KnownSystem.AmigaCUBOCD32:
                        systemsDict.Add("---------- Arcade ----------", null);
                        break;

                    // Other section
                    case KnownSystem.AudioCD:
                        systemsDict.Add("---------- Others ----------", null);
                        break;
                }

                systemsDict.Add(Converters.KnownSystemToString(system), system);
            }

            return systemsDict;
        }

        /// <summary>
        /// Create a list of active optical drives matched to their volume labels
        /// </summary>
        /// <returns>Active drives, matched to labels, if possible</returns>
        /// <remarks>
        /// https://stackoverflow.com/questions/3060796/how-to-distinguish-between-usb-and-floppy-devices?utm_medium=organic&utm_source=google_rich_qa&utm_campaign=google_rich_qa
        /// https://msdn.microsoft.com/en-us/library/aa394173(v=vs.85).aspx
        /// </remarks>
        public static OrderedDictionary<char, string> CreateListOfDrives()
        {
            // Get the floppy drives
            var floppyDrives = new List<KeyValuePair<char, string>>();
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
                        floppyDrives.Add(new KeyValuePair<char, string>(devId, UIElements.FloppyDriveString));
                    }
                }
            }
            catch
            {
                // No-op
            }

            // Get the optical disc drives
            List<KeyValuePair<char, string>> discDrives = DriveInfo.GetDrives()
                .Where(d => d.DriveType == DriveType.CDRom && d.IsReady)
                .Select(d => new KeyValuePair<char, string>(d.Name[0], d.VolumeLabel))
                .ToList();

            // Add the two lists together and order
            floppyDrives.AddRange(discDrives);
            floppyDrives = floppyDrives.OrderBy(i => i.Key).ToList();

            // Add to the ordered dictionary and return
            var drivesDict = new OrderedDictionary<char, string>();
            foreach (var drive in floppyDrives)
            {
                drivesDict.Add(drive.Key, drive.Value);
            }

            return drivesDict;
        }

        /// <summary>
        /// Get the current disc type from drive letter
        /// </summary>
        /// <param name="driveLetter"></param>
        /// <returns></returns>
        /// <remarks>
        /// https://stackoverflow.com/questions/11420365/detecting-if-disc-is-in-dvd-drive
        /// </remarks>
        public static MediaType? GetDiscType(char? driveLetter)
        {
            // Get the DeviceID from the current drive letter
            string deviceId = null;
            try
            {
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_CDROMDrive WHERE Id = '" + driveLetter + ":\'");

                var collection = searcher.Get();
                foreach (ManagementObject queryObj in collection)
                {
                    deviceId = (string)queryObj["DeviceID"];
                }
            }
            catch
            {
                // We don't care what the error was
                return null;
            }

            // If we got no valid device, we don't care and just return
            if (deviceId == null)
            {
                return null;
            }

            // Get all relevant disc information
            MsftDiscMaster2 discMaster = new MsftDiscMaster2();
            deviceId = deviceId.ToLower().Replace('\\', '#');
            string id = null;
            foreach (var disc in discMaster)
            {
                if (disc.ToString().Contains(deviceId))
                {
                    id = disc.ToString();
                }
            }

            // If we couldn't find the drive, we don't care and return
            if (id == null)
            {
                return null;
            }

            // Otherwise, we get the media type, if any
            MsftDiscRecorder2 recorder = new MsftDiscRecorder2();
            recorder.InitializeDiscRecorder(id);
            MsftDiscFormat2Data dataWriter = new MsftDiscFormat2Data();
            dataWriter.Recorder = recorder;
            var media = dataWriter.CurrentPhysicalMediaType;
            if (media != IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_UNKNOWN)
            {
                return Converters.IMAPIDiskTypeToMediaType(media);
            }

            return null;
        }

        /// <summary>
        /// Get the drive speed of the currently selected drive
        /// </summary>
        /// <returns>Speed of the drive converted from kbps</returns>
        /// <remarks>
        /// DIC uses the SCSI_MODE_SENSE command to check this, so does QPXTool (a different one, but still)
        /// See if SCSI_MODE_SENSE can be used here
        /// Currently, the calculations get something that is technically accurate, but is different than the advertisised
        /// capabilities of the drives (according to QPXTool)
        /// </remarks>
        public static int GetDriveSpeed(char driveLetter)
        {
            ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_CDROMDrive WHERE Id = '" + driveLetter + ":\'");

            var collection = searcher.Get();
            double? transferRate = -1;
            foreach (ManagementObject queryObj in collection)
            {
                var obj = queryObj["TransferRate"];
                transferRate = (double?)queryObj["TransferRate"];
            }

            // Transfer Rates (bps)
            double cdTransfer = 150 * 1024;
            double dvdTransfer = 1353 * 1024;

            double cdTransferTest = ((transferRate ?? -1) * 1024) / cdTransfer;
            double cdTransferTestKilo = ((transferRate ?? -1) * 1000) / cdTransfer;
            double dvdTransferTest = ((transferRate ?? -1) * 1024) / dvdTransfer;
            double dvdTransferTestKilo = ((transferRate ?? -1) * 1000) / dvdTransfer;

            return 0;
        }

        public static int GetDriveSpeedEx(char driveLetter, MediaType? mediaType)
        {
            // Get the DeviceID from the current drive letter
            string deviceId = null;
            try
            {
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_CDROMDrive WHERE Id = '" + driveLetter + ":\'");

                var collection = searcher.Get();
                foreach (ManagementObject queryObj in collection)
                {
                    deviceId = (string)queryObj["DeviceID"];
                }
            }
            catch
            {
                // We don't care what the error was
                return -1;
            }

            // If we got no valid device, we don't care and just return
            if (deviceId == null)
            {
                return -1;
            }

            // Get all relevant disc information
            MsftDiscMaster2 discMaster = new MsftDiscMaster2();
            deviceId = deviceId.ToLower().Replace('\\', '#');
            string id = null;
            foreach (var disc in discMaster)
            {
                if (disc.ToString().Contains(deviceId))
                {
                    id = disc.ToString();
                }
            }

            // If we couldn't find the drive, we don't care and return
            if (id == null)
            {
                return -1;
            }

            // Now we initialize the recorder to get disc info
            MsftDiscRecorder2 recorder = new MsftDiscRecorder2();
            recorder.InitializeDiscRecorder(id);
            IDiscRecorder2Ex recorderEx = recorder as IDiscRecorder2Ex;
            IMAPI_FEATURE_PAGE_TYPE ifpt = IMAPI_FEATURE_PAGE_TYPE.IMAPI_FEATURE_PAGE_TYPE_PROFILE_LIST;
            switch(mediaType)
            {
                case MediaType.CD:
                    ifpt = IMAPI_FEATURE_PAGE_TYPE.IMAPI_FEATURE_PAGE_TYPE_CD_READ;
                    break;
                case MediaType.DVD:
                    ifpt = IMAPI_FEATURE_PAGE_TYPE.IMAPI_FEATURE_PAGE_TYPE_DVD_READ;
                    break;
                case MediaType.HDDVD:
                    ifpt = IMAPI_FEATURE_PAGE_TYPE.IMAPI_FEATURE_PAGE_TYPE_HD_DVD_READ;
                    break;
                case MediaType.BluRay:
                    ifpt = IMAPI_FEATURE_PAGE_TYPE.IMAPI_FEATURE_PAGE_TYPE_BD_READ;
                    break;
            }

            // If we couldn't determine the media type properly, we don't care and return
            if (ifpt == IMAPI_FEATURE_PAGE_TYPE.IMAPI_FEATURE_PAGE_TYPE_PROFILE_LIST)
            {
                return -1;
            }

            // Now we get the requested mode data
            IntPtr modeData = Marshal.AllocHGlobal(256 * sizeof(byte));
            recorderEx.GetModePage(
                IMAPI_MODE_PAGE_TYPE.IMAPI_MODE_PAGE_TYPE_LEGACY_CAPABILITIES,
                IMAPI_MODE_PAGE_REQUEST_TYPE.IMAPI_MODE_PAGE_REQUEST_TYPE_CURRENT_VALUES,
                modeData,
                out uint modeDataSize);
            byte[] outModeArray = new byte[modeDataSize];
            Marshal.Copy(modeData, outModeArray, 0, (int)modeDataSize);

            // Now we get the requested feature page
            IntPtr featureData = Marshal.AllocHGlobal(32 * sizeof(byte));
            recorderEx.GetFeaturePage(
                ifpt,
                (sbyte)1,
                featureData,
                out uint byteSize);
            byte[] outArray = new byte[byteSize];
            Marshal.Copy(featureData, outArray, 0, (int)byteSize);

            return -1;
        }

        /// <summary>
        /// Validate that at string would be valid as input to DiscImageCreator
        /// </summary>
        /// <param name="parameters">String representing all parameters</param>
        /// <returns>True if it would be valid, false otherwise</returns>
        public static bool ValidateParameters(string parameters)
        {
            // The string has to be valid by itself first
            if (String.IsNullOrWhiteSpace(parameters))
            {
                return false;
            }

            // Now split the string into parts for easier validation
            // https://stackoverflow.com/questions/14655023/split-a-string-that-has-white-spaces-unless-they-are-enclosed-within-quotes
            parameters = parameters.Trim();
            List<string> parts = Regex.Matches(parameters, @"[\""].+?[\""]|[^ ]+")
                .Cast<Match>()
                .Select(m => m.Value)
                .ToList();

            // Determine what the commandline should look like given the first item
            int index = -1;
            switch (parts[0])
            {
                case DICCommands.CompactDisc:
                case DICCommands.GDROM:
                case DICCommands.Swap:
                case DICCommands.Data:
                    if (!IsValidDriveLetter(parts[1]))
                    {
                        return false;
                    }
                    else if (IsFlag(parts[2]))
                    {
                        return false;
                    }
                    else if (!IsValidNumber(parts[3], lowerBound: 0, upperBound: 72))
                    {
                        return false;
                    }

                    if (parts[0] == DICCommands.Swap)
                    {
                        if (parts.Count > 5)
                        {
                            return false;
                        }
                    }
                    else if (parts[0] == DICCommands.Data || parts[0] == DICCommands.Audio)
                    {
                        if (!IsValidNumber(parts[4]) || !IsValidNumber(parts[5]))
                        {
                            return false;
                        }

                        index = 6;
                    }
                    else
                    {
                        index = 4;
                    }

                    break;
                case DICCommands.DigitalVideoDisc:
                    if (!IsValidDriveLetter(parts[1]))
                    {
                        return false;
                    }
                    else if (IsFlag(parts[2]))
                    {
                        return false;
                    }
                    else if (!IsValidNumber(parts[3], lowerBound: 0, upperBound: 72)) // Officially 0-16
                    {
                        return false;
                    }

                    // Loop through all auxilary flags
                    for (int i = 4; i < parts.Count; i++)
                    {
                        switch (parts[i])
                        {
                            case DICFlags.DisableBeep:
                            case DICFlags.CMI:
                            case DICFlags.Raw:
                                // No-op, all of these are single flags
                                break;
                            case DICFlags.ForceUnitAccess:
                                // If the next item doesn't exist, it's good
                                if (!DoesNextExist(parts, i))
                                {
                                    break;
                                }
                                // If the next item is a flag, it's good
                                if (IsFlag(parts[i + 1]))
                                {
                                    break;
                                }
                                // If the next item isn't a valid number
                                else if (!IsValidNumber(parts[i + 1], lowerBound: 0))
                                {
                                    return false;
                                }
                                i++;
                                break;
                            default:
                                return false;
                        }
                    }
                    break;
                case DICCommands.BluRay:
                case DICCommands.XBOX:
                    if (!IsValidDriveLetter(parts[1]))
                    {
                        return false;
                    }
                    else if (IsFlag(parts[2]))
                    {
                        return false;
                    }

                    // Loop through all auxilary flags
                    for (int i = 3; i < parts.Count; i++)
                    {
                        switch (parts[i])
                        {
                            case DICFlags.DisableBeep:
                                // No-op, this is a single flag
                                break;
                            case DICFlags.ForceUnitAccess:
                                // If the next item doesn't exist, it's good
                                if (!DoesNextExist(parts, i))
                                {
                                    break;
                                }
                                // If the next item is a flag, it's good
                                if (IsFlag(parts[i + 1]))
                                {
                                    break;
                                }
                                // If the next item isn't a valid number
                                else if (!IsValidNumber(parts[i + 1], lowerBound: 0))
                                {
                                    return false;
                                }
                                i++;
                                break;
                            default:
                                return false;
                        }
                    }
                    break;
                case DICCommands.Floppy:
                    if (!IsValidDriveLetter(parts[1]))
                    {
                        return false;
                    }
                    else if (IsFlag(parts[2]))
                    {
                        return false;
                    }
                    else if (parts.Count > 3)
                    {
                        return false;
                    }
                    break;
                case DICCommands.Stop:
                case DICCommands.Start:
                case DICCommands.Eject:
                case DICCommands.Close:
                case DICCommands.Reset:
                case DICCommands.DriveSpeed:
                    if (!IsValidDriveLetter(parts[1]))
                    {
                        return false;
                    }
                    else if (parts.Count > 2)
                    {
                        return false;
                    }
                    break;
                case DICCommands.Sub:
                case DICCommands.MDS:
                    if (IsFlag(parts[1]))
                    {
                        return false;
                    }
                    else if (parts.Count > 2)
                    {
                        return false;
                    }
                    break;
                default:
                    return false;
            }

            // Loop through all auxilary flags, if necessary
            if (index > 0)
            {
                for (int i = index; i < parts.Count; i++)
                {
                    switch (parts[i])
                    {
                        case DICFlags.DisableBeep:
                            if (parts[0] != DICCommands.CompactDisc
                                && parts[0] != DICCommands.GDROM
                                && parts[0] != DICCommands.Data
                                && parts[0] != DICCommands.Audio)
                            {
                                return false;
                            }
                            break;
                        case DICFlags.D8Opcode:
                            if (parts[0] != DICCommands.CompactDisc
                                && parts[0] != DICCommands.GDROM
                                && parts[0] != DICCommands.Data
                                && parts[0] != DICCommands.Audio)
                            {
                                return false;
                            }
                            break;
                        case DICFlags.MCN:
                            if (parts[0] != DICCommands.CompactDisc)
                            {
                                return false;
                            }
                            break;
                        case DICFlags.AMSF:
                            if (parts[0] != DICCommands.CompactDisc)
                            {
                                return false;
                            }
                            break;
                        case DICFlags.Reverse:
                            if (parts[0] != DICCommands.CompactDisc
                                && parts[0] != DICCommands.Data)
                            {
                                return false;
                            }
                            break;
                        case DICFlags.MultiSession:
                            if (parts[0] != DICCommands.CompactDisc)
                            {
                                return false;
                            }
                            break;
                        case DICFlags.ScanSectorProtect:
                            if (parts[0] != DICCommands.CompactDisc
                                && parts[0] != DICCommands.Data)
                            {
                                return false;
                            }
                            break;
                        case DICFlags.ScanAntiMod:
                            if (parts[0] != DICCommands.CompactDisc)
                            {
                                return false;
                            }
                            break;
                        case DICFlags.NoFixSubP:
                            if (parts[0] != DICCommands.CompactDisc
                                && parts[0] != DICCommands.GDROM
                                && parts[0] != DICCommands.Data
                                && parts[0] != DICCommands.Audio)
                            {
                                return false;
                            }
                            break;
                        case DICFlags.NoFixSubQ:
                            if (parts[0] != DICCommands.CompactDisc
                                && parts[0] != DICCommands.GDROM
                                && parts[0] != DICCommands.Data
                                && parts[0] != DICCommands.Audio)
                            {
                                return false;
                            }
                            break;
                        case DICFlags.NoFixSubRtoW:
                            if (parts[0] != DICCommands.CompactDisc
                                && parts[0] != DICCommands.GDROM
                                && parts[0] != DICCommands.Data
                                && parts[0] != DICCommands.Audio)
                            {
                                return false;
                            }
                            break;
                        case DICFlags.NoFixSubQLibCrypt:
                            if (parts[0] != DICCommands.CompactDisc)
                            {
                                return false;
                            }
                            break;
                        case DICFlags.NoFixSubQSecuROM:
                            if (parts[0] != DICCommands.CompactDisc
                                && parts[0] != DICCommands.GDROM
                                && parts[0] != DICCommands.Data
                                && parts[0] != DICCommands.Audio)
                            {
                                return false;
                            }
                            break;
                        case DICFlags.ScanFileProtect:
                            if (parts[0] != DICCommands.CompactDisc
                                && parts[0] != DICCommands.Data)
                            {
                                return false;
                            }

                            // If the next item doesn't exist, it's good
                            if (!DoesNextExist(parts, i))
                            {
                                break;
                            }
                            // If the next item is a flag, it's good
                            if (IsFlag(parts[i + 1]))
                            {
                                break;
                            }
                            // If the next item isn't a valid number
                            else if (!IsValidNumber(parts[i + 1], lowerBound: 0))
                            {
                                return false;
                            }
                            i++;
                            break;
                        case DICFlags.ForceUnitAccess:
                            if (parts[0] != DICCommands.CompactDisc
                                && parts[0] != DICCommands.GDROM
                                && parts[0] != DICCommands.Data
                                && parts[0] != DICCommands.Audio)
                            {
                                return false;
                            }

                            // If the next item doesn't exist, it's good
                            if (!DoesNextExist(parts, i))
                            {
                                break;
                            }
                            // If the next item is a flag, it's good
                            if (IsFlag(parts[i + 1]))
                            {
                                break;
                            }
                            // If the next item isn't a valid number
                            else if (!IsValidNumber(parts[i + 1], lowerBound: 0))
                            {
                                return false;
                            }
                            i++;
                            break;
                        case DICFlags.AddOffset:
                            if (parts[0] != DICCommands.CompactDisc
                                && parts[0] != DICCommands.Audio)
                            {
                                return false;
                            }

                            // If the next item doesn't exist, it's not good
                            if (parts.Count == i + 1)
                            {
                                return false;
                            }
                            // If the next item isn't a valid number
                            else if (IsValidNumber(parts[i + 1]))
                            {
                                return false;
                            }
                            break;
                        case DICFlags.BEOpcode:
                            if (parts[0] != DICCommands.CompactDisc
                                && parts[0] != DICCommands.GDROM
                                && parts[0] != DICCommands.Data
                                && parts[0] != DICCommands.Audio)
                            {
                                return false;
                            }

                            // If the next item doesn't exist, it's good
                            if (!DoesNextExist(parts, i))
                            {
                                break;
                            }
                            // If the next item is a flag, it's good
                            if (IsFlag(parts[i + 1]))
                            {
                                break;
                            }
                            else if (parts[i + 1] != "raw"
                                && (parts[i + 1] != "pack"))
                            {
                                return false;
                            }
                            i++;
                            break;
                        case DICFlags.C2Opcode:
                            if (parts[0] != DICCommands.CompactDisc
                                && parts[0] != DICCommands.GDROM
                                && parts[0] != DICCommands.Data
                                && parts[0] != DICCommands.Audio)
                            {
                                return false;
                            }

                            for (int j = 1; j < 4; j++)
                            {
                                // If the next item doesn't exist, it's good
                                if (!DoesNextExist(parts, i + 1))
                                {
                                    i++;
                                    break;
                                }
                                // If the next item is a flag, it's good
                                if (IsFlag(parts[i + 1]))
                                {
                                    i++;
                                    break;
                                }
                                // If the next item isn't a valid number
                                else if (!IsValidNumber(parts[i + 1], lowerBound: 0))
                                {
                                    return false;
                                }
                                i++;
                            }
                            break;
                        case DICFlags.SubchannelReadLevel:
                            if (parts[0] != DICCommands.CompactDisc
                                && parts[0] != DICCommands.GDROM
                                && parts[0] != DICCommands.Data
                                && parts[0] != DICCommands.Audio)
                            {
                                return false;
                            }

                            // If the next item doesn't exist, it's good
                            if (!DoesNextExist(parts, i))
                            {
                                break;
                            }
                            // If the next item is a flag, it's good
                            if (IsFlag(parts[i + 1]))
                            {
                                break;
                            }
                            // If the next item isn't a valid number
                            else if (!IsValidNumber(parts[3], lowerBound: 0, upperBound: 2))
                            {
                                return false;
                            }
                            break;
                        case DICFlags.SeventyFour:
                            if (parts[0] != DICCommands.Swap)
                            {
                                return false;
                            }
                            break;
                        default:
                            return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Returns whether a string is a valid drive letter
        /// </summary>
        /// <param name="parameter">String value to check</param>
        /// <returns>True if it's a valid drive letter, false otherwise</returns>
        private static bool IsValidDriveLetter(string parameter)
        {
            if (!Regex.IsMatch(parameter, @"^[A-Z]:?\\?$"))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns whether a string is a flag (starts with '/')
        /// </summary>
        /// <param name="parameter">String value to check</param>
        /// <returns>True if it's a flag, false otherwise</returns>
        private static bool IsFlag(string parameter)
        {
            if (parameter.Trim('\"').StartsWith("/"))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns whether or not the next item exists
        /// </summary>
        /// <param name="parameters">List of parameters to check against</param>
        /// <param name="index">Current index</param>
        /// <returns>True if the next item exists, false otherwise</returns>
        private static bool DoesNextExist(List<string> parameters, int index)
        {
            if (index >= parameters.Count - 1)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns whether a string is a valid number
        /// </summary>
        /// <param name="parameter">String value to check</param>
        /// <param name="lowerBound">Lower bound (>=)</param>
        /// <param name="upperBound">Upper bound (<=)</param>
        /// <returns>True if it's a valid number, false otherwise</returns>
        private static bool IsValidNumber(string parameter, int lowerBound = -1, int upperBound = -1)
        {
            if (!Int32.TryParse(parameter, out int temp))
            {
                return false;
            }
            else if (lowerBound != -1 && temp < lowerBound)
            {
                return false;
            }
            else if (upperBound != -1 && temp > upperBound)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determine the base flags to use for checking a commandline
        /// </summary>
        /// <param name="parameters">Parameters as a string to check</param>
        /// <param name="type">Output nullable MediaType containing the found MediaType, if possible</param>
        /// <param name="system">Output nullable KnownSystem containing the found KnownSystem, if possible</param>
        /// <param name="letter">Output string containing the found drive letter</param>
        /// <param name="path">Output string containing the found path</param>
        /// <returns>False on error (and all outputs set to null), true otherwise</returns>
        public static bool DetermineFlags(string parameters, out MediaType? type, out KnownSystem? system, out string letter, out string path)
        {
            // Populate all output variables with null
            type = null; system = null; letter = null; path = null;

            // The string has to be valid by itself first
            if (String.IsNullOrWhiteSpace(parameters))
            {
                return false;
            }

            // Now split the string into parts for easier validation
            // https://stackoverflow.com/questions/14655023/split-a-string-that-has-white-spaces-unless-they-are-enclosed-within-quotes
            parameters = parameters.Trim();
            List<string> parts = Regex.Matches(parameters, @"[\""].+?[\""]|[^ ]+")
                .Cast<Match>()
                .Select(m => m.Value)
                .ToList();

            type = Converters.BaseCommmandToMediaType(parts[0]);
            system = Converters.BaseCommandToKnownSystem(parts[0]);

            // Determine what the commandline should look like given the first item
            switch (parts[0])
            {
                case DICCommands.CompactDisc:
                case DICCommands.GDROM:
                case DICCommands.Swap:
                case DICCommands.Data:
                case DICCommands.Audio:
                case DICCommands.DigitalVideoDisc:
                case DICCommands.BluRay:
                case DICCommands.XBOX:
                case DICCommands.Floppy:
                    if (!IsValidDriveLetter(parts[1]))
                    {
                        return false;
                    }
                    letter = parts[1];

                    if (IsFlag(parts[2]))
                    {
                        return false;
                    }
                    path = parts[2].Trim('\"');

                    // Special case for GameCube/Wii
                    if (parts.Contains(DICFlags.Raw))
                    {
                        type = MediaType.GameCubeGameDisc;
                        system = KnownSystem.NintendoGameCube;
                    }
                    // Special case for PlayStation
                    else if (parts.Contains(DICFlags.NoFixSubQLibCrypt)
                        || parts.Contains(DICFlags.ScanAntiMod))
                    {
                        type = MediaType.CD;
                        system = KnownSystem.SonyPlayStation;
                    }
                    // Special case for Saturn
                    else if (parts.Contains(DICFlags.SeventyFour))
                    {
                        type = MediaType.CD;
                        system = KnownSystem.SegaSaturn;
                    }

                    break;
                case DICCommands.Stop:
                case DICCommands.Start:
                case DICCommands.Eject:
                case DICCommands.Close:
                case DICCommands.Reset:
                case DICCommands.DriveSpeed:
                    if (!IsValidDriveLetter(parts[1]))
                    {
                        return false;
                    }
                    letter = parts[1];

                    break;
                case DICCommands.Sub:
                case DICCommands.MDS:
                    if (IsFlag(parts[1]))
                    {
                        return false;
                    }
                    path = parts[1].Trim('\"');

                    break;
                default:
                    return false;
            }

            return true;
        }
    }
}
