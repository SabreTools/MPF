using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using BurnOutSharp;
using IMAPI2;
using DICUI.Data;

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
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.BandaiApplePippin:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.CommodoreAmigaCD32:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.CommodoreAmigaCDTV:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.MattelHyperscan:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.MicrosoftXBOX:
                    types.Add(MediaType.CDROM);
                    types.Add(MediaType.DVD);
                    break;
                case KnownSystem.MicrosoftXBOX360:
                    types.Add(MediaType.CDROM);
                    types.Add(MediaType.DVD);
                    break;
                case KnownSystem.MicrosoftXBOXOne:
                    types.Add(MediaType.BluRay);
                    break;
                case KnownSystem.NECPCEngineTurboGrafxCD:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.NECPCFX:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.NintendoGameCube:
                    types.Add(MediaType.NintendoGameCube);
                    break;
                case KnownSystem.NintendoWii:
                    types.Add(MediaType.NintendoWiiOpticalDisc);
                    break;
                case KnownSystem.NintendoWiiU:
                    types.Add(MediaType.NintendoWiiUOpticalDisc);
                    break;
                case KnownSystem.Panasonic3DOInteractiveMultiplayer:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.PhilipsCDi:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.SegaCDMegaCD:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.SegaDreamcast:
                    types.Add(MediaType.CDROM);    // Low density partition
                    types.Add(MediaType.GDROM); // Hight density partition
                    break;
                case KnownSystem.SegaSaturn:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.SNKNeoGeoCD:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.SonyPlayStation:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.SonyPlayStation2:
                    types.Add(MediaType.CDROM);
                    types.Add(MediaType.DVD);
                    break;
                case KnownSystem.SonyPlayStation3:
                    types.Add(MediaType.BluRay);
                    types.Add(MediaType.CDROM);
                    types.Add(MediaType.DVD);
                    break;
                case KnownSystem.SonyPlayStation4:
                    types.Add(MediaType.BluRay);
                    break;
                case KnownSystem.SonyPlayStationPortable:
                    types.Add(MediaType.UMD);
                    types.Add(MediaType.DVD); // TODO: Confirm this
                    break;
                case KnownSystem.VMLabsNuon:
                    types.Add(MediaType.DVD);
                    break;
                case KnownSystem.VTechVFlashVSmilePro:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.ZAPiTGamesGameWaveFamilyEntertainmentSystem:
                    types.Add(MediaType.DVD);
                    break;

                #endregion

                #region Computers

                case KnownSystem.AcornArchimedes:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.AppleMacintosh:
                    types.Add(MediaType.CDROM);
                    types.Add(MediaType.DVD);
                    types.Add(MediaType.FloppyDisk);
                    break;
                case KnownSystem.CommodoreAmigaCD:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.FujitsuFMTowns:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.IBMPCCompatible:
                    types.Add(MediaType.CDROM);
                    types.Add(MediaType.DVD);
                    types.Add(MediaType.FloppyDisk);
                    break;
                case KnownSystem.NECPC88:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.NECPC98:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.SharpX68000:
                    types.Add(MediaType.CDROM);
                    break;

                #endregion

                #region Arcade

                case KnownSystem.AmigaCUBOCD32:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.AmericanLaserGames3DO:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.Atari3DO:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.Atronic:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.AUSCOMSystem1:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.BallyGameMagic:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.CapcomCPSystemIII:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.GlobalVRVarious:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.GlobalVRVortek:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.GlobalVRVortekV3:
                    types.Add(MediaType.DVD); // TODO: Confirm
                    break;
                case KnownSystem.ICEPCHardware:
                    types.Add(MediaType.DVD);
                    break;
                case KnownSystem.IncredibleTechnologiesEagle:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.IncredibleTechnologiesVarious:
                    types.Add(MediaType.CDROM);
                    types.Add(MediaType.DVD);
                    break;
                case KnownSystem.KonamieAmusement:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.KonamiFirebeat:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.KonamiGVSystem:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.KonamiM2:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.KonamiPython:
                    types.Add(MediaType.DVD); // TODO: Confirm
                    break;
                case KnownSystem.KonamiPython2:
                    types.Add(MediaType.DVD); // TODO: Confirm
                    break;
                case KnownSystem.KonamiSystem573:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.KonamiTwinkle:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.KonamiVarious:
                    types.Add(MediaType.CDROM);
                    types.Add(MediaType.DVD);
                    break;
                case KnownSystem.MeritIndustriesBoardwalk:
                    types.Add(MediaType.CDROM); // TODO: Confirm
                    break;
                case KnownSystem.MeritIndustriesMegaTouchAurora:
                    types.Add(MediaType.CDROM); // TODO: Confirm
                    break;
                case KnownSystem.MeritIndustriesMegaTouchForce:
                    types.Add(MediaType.CDROM);
                    types.Add(MediaType.DVD);
                    break;
                case KnownSystem.MeritIndustriesMegaTouchION:
                    types.Add(MediaType.CDROM);
                    types.Add(MediaType.DVD);
                    break;
                case KnownSystem.MeritIndustriesMegaTouchMaxx:
                    types.Add(MediaType.DVD);
                    break;
                case KnownSystem.MeritIndustriesMegaTouchXL:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.NamcoCapcomSystem256:
                    types.Add(MediaType.DVD);
                    break;
                case KnownSystem.NamcoCapcomTaitoSystem246:
                    types.Add(MediaType.CDROM);
                    types.Add(MediaType.DVD);
                    break;
                case KnownSystem.NamcoSegaNintendoTriforce:
                    types.Add(MediaType.GDROM);
                    break;
                case KnownSystem.NamcoSystem12:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.NamcoSystem357:
                    types.Add(MediaType.DVD);
                    types.Add(MediaType.BluRay);
                    break;
                case KnownSystem.NewJatreCDi:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.NichibutsuHighRateSystem:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.NichibutsuSuperCD:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.NichibutsuXRateSystem:
                    types.Add(MediaType.DVD);
                    break;
                case KnownSystem.PanasonicM2:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.PhotoPlayVarious:
                    types.Add(MediaType.CDROM);
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
                case KnownSystem.SegaTitanVideo:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.SegaSystem32:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.SeibuCATSSystem:
                    types.Add(MediaType.DVD);
                    break;
                case KnownSystem.TABAustriaQuizard:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.TandyMemorexVisualInformationSystem:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.TsunamiTsuMoMultiGameMotionSystem:
                    types.Add(MediaType.CDROM);
                    break;

                #endregion

                #region Others

                case KnownSystem.AudioCD:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.BDVideo:
                    types.Add(MediaType.BluRay);
                    break;
                case KnownSystem.DVDVideo:
                    types.Add(MediaType.DVD);
                    break;
                case KnownSystem.EnhancedCD:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.EnhancedDVD:
                    types.Add(MediaType.DVD);
                    break;
                case KnownSystem.EnhancedBD:
                    types.Add(MediaType.BluRay);
                    break;
                case KnownSystem.HasbroVideoNow:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.HDDVDVideo:
                    types.Add(MediaType.HDDVD);
                    break;
                case KnownSystem.PalmOS:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.PhilipsCDiDigitalVideo:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.PhotoCD:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.PlayStationGameSharkUpdates:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.RainbowDisc:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.TaoiKTV:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.TomyKissSite:
                    types.Add(MediaType.CDROM);
                    break;
                case KnownSystem.VideoCD:
                    types.Add(MediaType.CDROM);
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
                .Select(d => Drive.Optical(d.Name[0], (d.IsReady ? d.VolumeLabel : Template.DiscNotDetected), d.IsReady))                
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
                    deviceId = (string)queryObj["DeviceID"];
            }
            catch
            {
                // We don't care what the error was
                return null;
            }

            // If we got no valid device, we don't care and just return
            if (deviceId == null)
                return null;

            // Get all relevant disc information
            try
            {
                MsftDiscMaster2 discMaster = new MsftDiscMaster2();
                deviceId = deviceId.ToLower().Replace('\\', '#');
                string id = null;
                foreach (var disc in discMaster)
                {
                    if (disc.ToString().Contains(deviceId))
                        id = disc.ToString();
                }

                // If we couldn't find the drive, we don't care and return
                if (id == null)
                    return null;

                // Otherwise, we get the media type, if any
                MsftDiscRecorder2 recorder = new MsftDiscRecorder2();
                recorder.InitializeDiscRecorder(id);
                MsftDiscFormat2Data dataWriter = new MsftDiscFormat2Data();
                dataWriter.Recorder = recorder;
                var media = dataWriter.CurrentPhysicalMediaType;
                if (media != IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_UNKNOWN)
                    return Converters.IMAPIDiskTypeToMediaType(media);
            }
            catch
            {
                // We don't care what the error is
            }

            return null;
        }

        /// <summary>
        /// Verify that, given a system and a media type, they are correct
        /// </summary>
        public static Result GetSupportStatus(KnownSystem? system, MediaType? type)
        {
            // No system chosen, update status
            if (system == KnownSystem.NONE)
                return Result.Failure("Please select a valid system");

            // If we're on an unsupported type, update the status accordingly
            switch (type)
            {
                // Fully supported types
                case MediaType.BluRay:
                case MediaType.CDROM:
                case MediaType.DVD:
                case MediaType.FloppyDisk:
                case MediaType.HDDVD:
                    return Result.Success("{0} ready to dump", type.Name());

                // Partially supported types
                case MediaType.GDROM:
                case MediaType.NintendoGameCube:
                case MediaType.NintendoWiiOpticalDisc:
                    return Result.Success("{0} partially supported for dumping", type.Name());

                // Special case for other supported tools
                case MediaType.UMD:
                    return Result.Success("{0} supported for submission info parsing", type.Name());

                // Specifically unknown type
                case MediaType.NONE:
                    return Result.Failure("Please select a valid disc type");

                // Undumpable but recognized types
                default:
                    return Result.Failure("{0} discs are not supported for dumping", type.Name());
            }
        }

        /// <summary>
        /// Run protection scan on a given dump environment
        /// </summary>
        /// <param name="env">DumpEnvirionment containing all required information</param>
        /// <returns>Copy protection detected in the envirionment, if any</returns>
        public static async Task<string> RunProtectionScanOnPath(string path)
        {
            try
            {
                var found = await Task.Run(() =>
                {
                    return ProtectionFind.Scan(path);
                });

                if (found == null || found.Count == 0)
                    return "None found";

                return string.Join("\n", found.Select(kvp => kvp.Key + ": " + kvp.Value).ToArray());
            }
            catch
            {
                return "Disc could not be scanned!";
            }
        }
    }
}
