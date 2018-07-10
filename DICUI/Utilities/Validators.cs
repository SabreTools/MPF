using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using IMAPI2;
using DICUI.Data;
using DICUI.External.BurnOut;

namespace DICUI.Utilities
{
    public static class Validators
    {
        /// <summary>
        /// Get a list of valid MediaTypes for a given KnownSystem
        /// </summary>
        /// <param name="sys">KnownSystem value to check</param>
        /// <returns>MediaTypes, if possible</returns>
        public static List<MediaType?> GetValidMediaTypes(KnownSystem? sys)
        {
            var types = new List<MediaType?>();

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

            return types;
        }

        /// <summary>
        /// Create a list of systems
        /// </summary>
        /// <returns>KnownSystems, if possible</returns>
        public static List<KnownSystem?> CreateListOfSystems()
        {
            return Enum.GetValues(typeof(KnownSystem))
                .OfType<KnownSystem?>()
                .Where(s => !s.IsMarker() && s != KnownSystem.NONE)
                .ToList();
        }

        /// <summary>
        /// Create a list of active drives matched to their volume labels
        /// </summary>
        /// <returns>Active drives, matched to labels, if possible</returns>
        /// <remarks>
        /// https://stackoverflow.com/questions/3060796/how-to-distinguish-between-usb-and-floppy-devices?utm_medium=organic&utm_source=google_rich_qa&utm_campaign=google_rich_qa
        /// https://msdn.microsoft.com/en-us/library/aa394173(v=vs.85).aspx
        /// </remarks>
        public static List<Drive> CreateListOfDrives()
        {
            var drives = new List<Drive>();

            // Get the floppy drives
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
                        drives.Add(Drive.Floppy(devId));
                    }
                }
            }
            catch
            {
                // No-op
            }

            // Get the optical disc drives
            List<Drive> discDrives = DriveInfo.GetDrives()
                .Where(d => d.DriveType == DriveType.CDRom)
                .Select(d => Drive.Optical(d.Name[0], (d.IsReady ? d.VolumeLabel : UIElements.DiscNotDetected), d.IsReady))                
                .ToList();

            // Add the two lists together and order
            drives.AddRange(discDrives);
            drives = drives.OrderBy(i => i.Letter).ToList();

            return drives;
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
            try
            {
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
            }
            catch
            {
                // We don't care what the error is
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
        /// TransferRate appears to be the CURRENT transfer rate, not the maximum... basically making that flag useless
        /// </remarks>
        public static int GetDriveSpeed(Drive drive)
        {
            // If the current drive is not active or optical
            if (drive.IsFloppy || !drive.MarkedActive)
                return -1;

            ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_CDROMDrive WHERE Id = '" + drive.Letter + ":\'");

            var collection = searcher.Get();
            double? transferRate = -1;
            foreach (ManagementObject queryObj in collection)
            {
                var obj = queryObj["TransferRate"];
                transferRate = (double?)queryObj["TransferRate"];
            }

            // Transfer Rates (kBps)
            double cdTransfer = 153.6;
            double dvdTransfer = 1385;

            double cdTransferTest = ((transferRate ?? -1)) / cdTransfer;
            double dvdTransferTest = ((transferRate ?? -1)) / dvdTransfer;

            return 0;
        }

        public unsafe static int GetDriveSpeedEx(Drive drive, MediaType? mediaType)
        {
            // If the current drive is not active or optical
            if (drive.IsFloppy || !drive.MarkedActive)
                return -1;

            // Get the DeviceID from the current drive letter
            string deviceId = null;
            try
            {
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_CDROMDrive WHERE Id = '" + drive.Letter + ":\'");

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

            // Ones that haven't worked:
            // recorderEx.GetAdapterDescriptor
            // recorderEx.GetDeviceDescriptor
            // recorderEx.GetDiscInformation
            // recorderEx.GetTrackInformation

            // Now we get the requested feature page
            // TODO: Figure out structure of returned data
            IntPtr featureData = Marshal.AllocHGlobal(32 * sizeof(byte));
            recorderEx.GetFeaturePage(
                ifpt,
                (sbyte)0,
                featureData,
                out uint byteSize);
            byte[] outFeatureArray = new byte[byteSize];
            Marshal.Copy(featureData, outFeatureArray, 0, (int)byteSize);

            // Now we get the requested mode data
            // TODO: Figure out structure of returned data
            IntPtr modeData = Marshal.AllocHGlobal(256 * sizeof(byte));
            recorderEx.GetModePage(
                (IMAPI_MODE_PAGE_TYPE)0x2A,
                IMAPI_MODE_PAGE_REQUEST_TYPE.IMAPI_MODE_PAGE_REQUEST_TYPE_CURRENT_VALUES,
                modeData,
                out uint modeDataSize);
            byte[] outModeArray = new byte[modeDataSize];
            Marshal.Copy(modeData, outModeArray, 0, (int)modeDataSize);

            // Now we send the command to get sense data from the device
            // TODO: This seems like the best option, but how is this data structured properly?
            byte[] cdbArray = new byte[] { 0x5a, 0x0, 0x2a, 0x00, 0xff, 0x0 };
            byte[] senseBuffer = new byte[256];
            byte[] buffer = new byte[256];
            uint bufferSize = 256;

            recorderEx.SendCommandGetDataFromDevice(
                ref cdbArray[0],
                (uint)6,
                senseBuffer,
                (uint)60,
                out buffer[0],
                bufferSize,
                out uint BufferFetched);

            return -1;
        }

        /// <summary>
        /// Verify that, given a system and a media type, they are correct
        /// </summary>
        public static Result GetSupportStatus(KnownSystem? system, MediaType? type)
        {
            // No system chosen, update status
            if (system == KnownSystem.NONE)
                return Result.Failure("Please select a valid system");
            // custom system chosen, then don't check anything
            else if (system == KnownSystem.Custom)
                return Result.Success("{0} ready to dump", type.Name());

            // If we're on an unsupported type, update the status accordingly
            switch (type)
            {
                // Fully supported types
                case MediaType.CD:
                case MediaType.DVD:
                case MediaType.HDDVD:
                case MediaType.BluRay:
                    if (system == KnownSystem.MicrosoftXBOX360XDG3)
                    {
                        return Result.Failure("{0} discs are not currently supported by DIC", type.Name());
                    }
                    return Result.Success("{0} ready to dump", type.Name());

                // Partially supported types
                case MediaType.GDROM:
                case MediaType.GameCubeGameDisc:
                case MediaType.WiiOpticalDisc:
                    return Result.Success("{0} discs are partially supported by DIC", type.Name());

                // Undumpable but recognized types
                case MediaType.LaserDisc:
                case MediaType.WiiUOpticalDisc:
                case MediaType.CED:
                case MediaType.UMD:
                case MediaType.Cartridge:
                case MediaType.Cassette:
                    return Result.Failure("{0} discs are not currently supported by DIC", type.Name());

                // Invalid or unknown types
                case MediaType.NONE:
                default:
                    return Result.Failure("Please select a valid disc type");
            }
        }

        /// <summary>
        /// Run protection scan on a given dump environment
        /// </summary>
        /// <param name="env">DumpEnvirionment containing all required information</param>
        /// <returns>Copy protection detected in the envirionment, if any</returns>
        public static async Task<string> RunProtectionScanOnPath(string path)
        {
            var found = await Task.Run(() =>
            {
                return ProtectionFind.Scan(path);
            });

            if (found == null)
                return "None found";

            return string.Join("\n", found.Select(kvp => kvp.Key + ": " + kvp.Value).ToArray());
        }
    }
}
