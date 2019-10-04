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

                // https://en.wikipedia.org/wiki/Atari_Jaguar_CD
                case KnownSystem.AtariJaguarCD:
                    types.Add(MediaType.CDROM);
                    break;

                // https://en.wikipedia.org/wiki/Playdia
                case KnownSystem.BandaiPlaydiaQuickInteractiveSystem:
                    types.Add(MediaType.CDROM);
                    break;

                // https://en.wikipedia.org/wiki/Apple_Bandai_Pippin
                case KnownSystem.BandaiApplePippin:
                    types.Add(MediaType.CDROM);
                    break;

                // https://en.wikipedia.org/wiki/Amiga_CD32
                case KnownSystem.CommodoreAmigaCD32:
                    types.Add(MediaType.CDROM);
                    break;

                // https://en.wikipedia.org/wiki/Commodore_CDTV
                case KnownSystem.CommodoreAmigaCDTV:
                    types.Add(MediaType.CDROM);
                    break;

                // https://en.wikipedia.org/wiki/EVO_Smart_Console
                case KnownSystem.EnvizionsEVOSmartConsole:
                    types.Add(MediaType.CDROM);
                    types.Add(MediaType.DVD);
                    break;

                // https://en.wikipedia.org/wiki/FM_Towns_Marty
                case KnownSystem.FujitsuFMTownsMarty:
                    types.Add(MediaType.CDROM);
                    types.Add(MediaType.FloppyDisk);
                    break;

                // https://en.wikipedia.org/wiki/VideoNow
                case KnownSystem.HasbroVideoNow:
                    types.Add(MediaType.CDROM);
                    break;

                // https://en.wikipedia.org/wiki/VideoNow
                case KnownSystem.HasbroVideoNowColor:
                    types.Add(MediaType.CDROM);
                    break;

                // https://en.wikipedia.org/wiki/VideoNow
                case KnownSystem.HasbroVideoNowJr:
                    types.Add(MediaType.CDROM);
                    break;

                // https://en.wikipedia.org/wiki/VideoNow
                case KnownSystem.HasbroVideoNowXP:
                    types.Add(MediaType.CDROM);
                    break;

                // https://en.wikipedia.org/wiki/HyperScan
                case KnownSystem.MattelHyperscan:
                    types.Add(MediaType.CDROM);
                    break;

                // https://en.wikipedia.org/wiki/Xbox_(console)
                case KnownSystem.MicrosoftXBOX:
                    types.Add(MediaType.CDROM);
                    types.Add(MediaType.DVD);
                    break;

                // https://en.wikipedia.org/wiki/Xbox_360
                case KnownSystem.MicrosoftXBOX360:
                    types.Add(MediaType.CDROM);
                    types.Add(MediaType.DVD);
                    break;

                // https://en.wikipedia.org/wiki/Xbox_One
                case KnownSystem.MicrosoftXBOXOne:
                    types.Add(MediaType.BluRay);
                    break;

                // https://en.wikipedia.org/wiki/TurboGrafx-16
                case KnownSystem.NECPCEngineTurboGrafxCD:
                    types.Add(MediaType.CDROM);
                    break;

                // https://en.wikipedia.org/wiki/PC-FX
                case KnownSystem.NECPCFX:
                    types.Add(MediaType.CDROM);
                    break;

                // https://en.wikipedia.org/wiki/GameCube
                case KnownSystem.NintendoGameCube:
                    types.Add(MediaType.NintendoGameCubeGameDisc);
                    break;

                // https://en.wikipedia.org/wiki/Super_NES_CD-ROM
                case KnownSystem.NintendoSonySuperNESCDROMSystem:
                    types.Add(MediaType.CDROM);
                    break;

                // https://en.wikipedia.org/wiki/Wii
                case KnownSystem.NintendoWii:
                    types.Add(MediaType.NintendoWiiOpticalDisc);
                    break;

                // https://en.wikipedia.org/wiki/Wii_U
                case KnownSystem.NintendoWiiU:
                    types.Add(MediaType.NintendoWiiUOpticalDisc);
                    break;

                // https://en.wikipedia.org/wiki/3DO_Interactive_Multiplayer
                case KnownSystem.Panasonic3DOInteractiveMultiplayer:
                    types.Add(MediaType.CDROM);
                    break;

                // https://en.wikipedia.org/wiki/Philips_CD-i
                case KnownSystem.PhilipsCDi:
                    types.Add(MediaType.CDROM);
                    break;

                // https://en.wikipedia.org/wiki/LaserActive
                case KnownSystem.PioneerLaserActive:
                    types.Add(MediaType.CDROM);
                    types.Add(MediaType.LaserDisc);
                    break;

                // https://en.wikipedia.org/wiki/Sega_CD
                case KnownSystem.SegaCDMegaCD:
                    types.Add(MediaType.CDROM);
                    break;

                // https://en.wikipedia.org/wiki/Dreamcast
                case KnownSystem.SegaDreamcast:
                    types.Add(MediaType.CDROM); // Low density partition, MIL-CD
                    types.Add(MediaType.GDROM); // High density partition
                    break;

                // https://en.wikipedia.org/wiki/Sega_Saturn
                case KnownSystem.SegaSaturn:
                    types.Add(MediaType.CDROM);
                    break;

                // https://en.wikipedia.org/wiki/Neo_Geo_CD
                case KnownSystem.SNKNeoGeoCD:
                    types.Add(MediaType.CDROM);
                    break;

                // https://en.wikipedia.org/wiki/PlayStation_(console)
                case KnownSystem.SonyPlayStation:
                    types.Add(MediaType.CDROM);
                    break;

                // https://en.wikipedia.org/wiki/PlayStation_2
                case KnownSystem.SonyPlayStation2:
                    types.Add(MediaType.CDROM);
                    types.Add(MediaType.DVD);
                    break;

                // https://en.wikipedia.org/wiki/PlayStation_3
                case KnownSystem.SonyPlayStation3:
                    types.Add(MediaType.BluRay);
                    types.Add(MediaType.CDROM);
                    types.Add(MediaType.DVD);
                    break;

                // https://en.wikipedia.org/wiki/PlayStation_4
                case KnownSystem.SonyPlayStation4:
                    types.Add(MediaType.BluRay);
                    break;

                // https://en.wikipedia.org/wiki/PlayStation_Portable
                case KnownSystem.SonyPlayStationPortable:
                    types.Add(MediaType.UMD);
                    types.Add(MediaType.CDROM); // Development discs only
                    types.Add(MediaType.DVD); // Development discs only
                    break;

                // https://en.wikipedia.org/wiki/Tandy_Video_Information_System
                case KnownSystem.TandyMemorexVisualInformationSystem:
                    types.Add(MediaType.CDROM);
                    break;

                // https://en.wikipedia.org/wiki/Nuon_(DVD_technology)
                case KnownSystem.VMLabsNuon:
                    types.Add(MediaType.DVD);
                    break;

                // https://en.wikipedia.org/wiki/V.Flash
                case KnownSystem.VTechVFlashVSmilePro:
                    types.Add(MediaType.CDROM);
                    break;

                // https://en.wikipedia.org/wiki/Game_Wave_Family_Entertainment_System
                case KnownSystem.ZAPiTGamesGameWaveFamilyEntertainmentSystem:
                    types.Add(MediaType.DVD);
                    break;

                #endregion

                #region Computers

                // https://en.wikipedia.org/wiki/Acorn_Archimedes
                case KnownSystem.AcornArchimedes:
                    types.Add(MediaType.CDROM);
                    types.Add(MediaType.FloppyDisk);
                    break;

                // https://en.wikipedia.org/wiki/Macintosh
                case KnownSystem.AppleMacintosh:
                    types.Add(MediaType.CDROM);
                    types.Add(MediaType.DVD);
                    types.Add(MediaType.FloppyDisk);
                    types.Add(MediaType.HardDisk);
                    break;

                // https://en.wikipedia.org/wiki/Amiga
                case KnownSystem.CommodoreAmiga:
                    types.Add(MediaType.CDROM);
                    types.Add(MediaType.FloppyDisk);
                    break;

                // https://en.wikipedia.org/wiki/FM_Towns
                case KnownSystem.FujitsuFMTowns:
                    types.Add(MediaType.CDROM);
                    break;

                // https://en.wikipedia.org/wiki/IBM_PC_compatible
                case KnownSystem.IBMPCCompatible:
                    types.Add(MediaType.CDROM);
                    types.Add(MediaType.DVD);
                    types.Add(MediaType.FloppyDisk);
                    types.Add(MediaType.HardDisk);
                    break;

                // https://en.wikipedia.org/wiki/PC-8800_series
                case KnownSystem.NECPC88:
                    types.Add(MediaType.CDROM);
                    types.Add(MediaType.FloppyDisk);
                    break;

                // https://en.wikipedia.org/wiki/PC-9800_series
                case KnownSystem.NECPC98:
                    types.Add(MediaType.CDROM);
                    types.Add(MediaType.DVD);
                    types.Add(MediaType.FloppyDisk);
                    break;

                // https://en.wikipedia.org/wiki/X68000
                case KnownSystem.SharpX68000:
                    types.Add(MediaType.CDROM);
                    types.Add(MediaType.FloppyDisk);
                    break;

                #endregion

                #region Arcade

                // https://www.bigbookofamigahardware.com/bboah/product.aspx?id=36
                case KnownSystem.AmigaCUBOCD32:
                    types.Add(MediaType.CDROM);
                    break;

                // https://en.wikipedia.org/wiki/Orbatak
                case KnownSystem.AmericanLaserGames3DO:
                    types.Add(MediaType.CDROM);
                    break;

                // http://system16.com/hardware.php?id=779
                case KnownSystem.Atari3DO:
                    types.Add(MediaType.CDROM);
                    break;

                // http://newlifegames.net/nlg/index.php?topic=22003.0
                // http://newlifegames.net/nlg/index.php?topic=5486.msg119440
                case KnownSystem.Atronic:
                    types.Add(MediaType.CDROM);
                    types.Add(MediaType.DVD);
                    break;

                // https://www.arcade-museum.com/members/member_detail.php?member_id=406530
                case KnownSystem.AUSCOMSystem1:
                    types.Add(MediaType.CDROM);
                    break;

                // http://newlifegames.net/nlg/index.php?topic=285.0
                case KnownSystem.BallyGameMagic:
                    types.Add(MediaType.CDROM);
                    break;

                // https://en.wikipedia.org/wiki/CP_System_III
                case KnownSystem.CapcomCPSystemIII:
                    types.Add(MediaType.CDROM);
                    break;

                // UNKNOWN
                case KnownSystem.GlobalVRVarious:
                    types.Add(MediaType.CDROM);
                    break;

                // https://service.globalvr.com/troubleshooting/vortek.html
                case KnownSystem.GlobalVRVortek:
                    types.Add(MediaType.CDROM);
                    break;

                // https://service.globalvr.com/downloads/v3/040-1001-01c-V3-System-Manual.pdf
                case KnownSystem.GlobalVRVortekV3:
                    types.Add(MediaType.CDROM);
                    break;

                // https://www.icegame.com/games
                case KnownSystem.ICEPCHardware:
                    types.Add(MediaType.DVD);
                    break;

                // https://github.com/mamedev/mame/blob/master/src/mame/drivers/iteagle.cpp
                case KnownSystem.IncredibleTechnologiesEagle:
                    types.Add(MediaType.CDROM);
                    break;

                // UNKNOWN
                case KnownSystem.IncredibleTechnologiesVarious:
                    types.Add(MediaType.CDROM);
                    types.Add(MediaType.DVD);
                    break;

                // https://en.wikipedia.org/wiki/E-Amusement
                case KnownSystem.KonamieAmusement:
                    types.Add(MediaType.CDROM);
                    types.Add(MediaType.DVD);
                    break;

                // http://system16.com/hardware.php?id=828
                case KnownSystem.KonamiFirebeat:
                    types.Add(MediaType.CDROM);
                    types.Add(MediaType.DVD);
                    break;

                // http://system16.com/hardware.php?id=577
                case KnownSystem.KonamiGVSystem:
                    types.Add(MediaType.CDROM);
                    break;

                // http://system16.com/hardware.php?id=575
                case KnownSystem.KonamiM2:
                    types.Add(MediaType.CDROM);
                    break;

                // http://system16.com/hardware.php?id=586
                // http://system16.com/hardware.php?id=977
                case KnownSystem.KonamiPython:
                    types.Add(MediaType.CDROM);
                    types.Add(MediaType.DVD);
                    break;

                // http://system16.com/hardware.php?id=976
                // http://system16.com/hardware.php?id=831
                case KnownSystem.KonamiPython2:
                    types.Add(MediaType.CDROM);
                    types.Add(MediaType.DVD);
                    break;

                // http://system16.com/hardware.php?id=582
                // http://system16.com/hardware.php?id=822
                // http://system16.com/hardware.php?id=823
                case KnownSystem.KonamiSystem573:
                    types.Add(MediaType.CDROM);
                    break;

                // http://system16.com/hardware.php?id=827
                case KnownSystem.KonamiTwinkle:
                    types.Add(MediaType.CDROM);
                    break;

                // UNKNOWN
                case KnownSystem.KonamiVarious:
                    types.Add(MediaType.CDROM);
                    types.Add(MediaType.DVD);
                    break;

                // http://www.meritgames.com/Support_Center/manuals/PM0591-01.pdf
                case KnownSystem.MeritIndustriesBoardwalk:
                    types.Add(MediaType.CDROM);
                    types.Add(MediaType.DVD);
                    break;

                // http://www.meritgames.com/Support_Center/Force%20Elite/PM0380-09.pdf
                // http://www.meritgames.com/Support_Center/Force%20Upright/PM0382-07%20FORCE%20Upright%20manual.pdf
                // http://www.meritgames.com/Support_Center/Force%20Upright/PM0383-07%20FORCE%20Upright%20manual.pdf
                case KnownSystem.MeritIndustriesMegaTouchForce:
                    types.Add(MediaType.CDROM);
                    break;

                // http://www.meritgames.com/Service%20Center/Ion%20Troubleshooting.pdf
                case KnownSystem.MeritIndustriesMegaTouchION:
                    types.Add(MediaType.CDROM);
                    types.Add(MediaType.DVD);
                    break;

                // http://www.meritgames.com/Support_Center/EZ%20Maxx/Manuals/MAXX%20Elite%20with%20coin.pdf
                // http://www.meritgames.com/Support_Center/EZ%20Maxx/Manuals/MAXX%20Elite.pdf
                // http://www.meritgames.com/Support_Center/manuals/90003010%20Maxx%20TSM_Rev%20C.pdf
                case KnownSystem.MeritIndustriesMegaTouchMaxx:
                    types.Add(MediaType.CDROM);
                    break;

                // http://www.meritgames.com/Support_Center/manuals/pm0076_OA_Megatouch%20XL%20Trouble%20Shooting%20Manual.pdf
                // http://www.meritgames.com/Support_Center/MEGA%20XL/manuals/Megatouch_XL_pm0109-0D.pdf
                // http://www.meritgames.com/Support_Center/MEGA%20XL/manuals/Megatouch_XL_Super_5000_manual.pdf
                case KnownSystem.MeritIndustriesMegaTouchXL:
                    types.Add(MediaType.CDROM);
                    break;

                // http://system16.com/hardware.php?id=546
                // http://system16.com/hardware.php?id=872
                case KnownSystem.NamcoCapcomSystem256:
                    types.Add(MediaType.CDROM);
                    types.Add(MediaType.DVD);
                    break;

                // http://system16.com/hardware.php?id=543
                case KnownSystem.NamcoCapcomTaitoSystem246:
                    types.Add(MediaType.CDROM);
                    types.Add(MediaType.DVD);
                    break;

                // http://system16.com/hardware.php?id=545
                case KnownSystem.NamcoSegaNintendoTriforce:
                    types.Add(MediaType.CDROM); // Low density partition
                    types.Add(MediaType.GDROM); // High density partition
                    break;

                // http://system16.com/hardware.php?id=535
                case KnownSystem.NamcoSystem12:
                    types.Add(MediaType.CDROM);
                    break;

                // http://system16.com/hardware.php?id=900
                case KnownSystem.NamcoSystem357:
                    types.Add(MediaType.CDROM);
                    types.Add(MediaType.DVD);
                    types.Add(MediaType.BluRay);
                    break;

                // https://www.arcade-history.com/?n=the-yakyuuken-part-1&page=detail&id=33049
                case KnownSystem.NewJatreCDi:
                    types.Add(MediaType.CDROM);
                    break;

                // http://blog.system11.org/?p=2499
                case KnownSystem.NichibutsuHighRateSystem:
                    types.Add(MediaType.DVD);
                    break;

                // http://blog.system11.org/?p=2514
                case KnownSystem.NichibutsuSuperCD:
                    types.Add(MediaType.CDROM);
                    break;

                // http://collectedit.com/collectors/shou-time-213/arcade-pcbs-281/x-rate-dvd-series-17-newlywed-life-japan-by-nichibutsu-32245
                case KnownSystem.NichibutsuXRateSystem:
                    types.Add(MediaType.DVD);
                    break;

                // https://en.wikipedia.org/wiki/Panasonic_M2
                case KnownSystem.PanasonicM2:
                    types.Add(MediaType.CDROM);
                    types.Add(MediaType.DVD);
                    break;

                // https://github.com/mamedev/mame/blob/master/src/mame/drivers/photoply.cpp
                case KnownSystem.PhotoPlayVarious:
                    types.Add(MediaType.CDROM);
                    break;

                // UNKNOWN
                case KnownSystem.RawThrillsVarious:
                    types.Add(MediaType.DVD);
                    break;

                // http://system16.com/hardware.php?id=729
                case KnownSystem.SegaChihiro:
                    types.Add(MediaType.CDROM); // Low density partition
                    types.Add(MediaType.GDROM); // High density partition
                    break;

                // http://system16.com/hardware.php?id=907
                case KnownSystem.SegaEuropaR:
                    types.Add(MediaType.DVD);
                    break;

                // http://system16.com/hardware.php?id=985
                // http://system16.com/hardware.php?id=731
                // http://system16.com/hardware.php?id=984
                // http://system16.com/hardware.php?id=986
                case KnownSystem.SegaLindbergh:
                    types.Add(MediaType.DVD);
                    break;

                // http://system16.com/hardware.php?id=721
                // http://system16.com/hardware.php?id=723
                // http://system16.com/hardware.php?id=906
                // http://system16.com/hardware.php?id=722
                case KnownSystem.SegaNaomi:
                    types.Add(MediaType.CDROM); // Low density partition
                    types.Add(MediaType.GDROM); // High density partition
                    break;

                // http://system16.com/hardware.php?id=725
                // http://system16.com/hardware.php?id=726
                // http://system16.com/hardware.php?id=727
                case KnownSystem.SegaNaomi2:
                    types.Add(MediaType.CDROM); // Low density partition
                    types.Add(MediaType.GDROM); // High density partition
                    break;

                // http://system16.com/hardware.php?id=975
                // https://en.wikipedia.org/wiki/List_of_Sega_arcade_system_boards#Sega_Nu
                case KnownSystem.SegaNu:
                    types.Add(MediaType.BluRay);
                    break;

                // http://system16.com/hardware.php?id=910
                // https://en.wikipedia.org/wiki/List_of_Sega_arcade_system_boards#Sega_Ring_series
                case KnownSystem.SegaRingEdge:
                    types.Add(MediaType.DVD);
                    break;

                // http://system16.com/hardware.php?id=982
                // https://en.wikipedia.org/wiki/List_of_Sega_arcade_system_boards#Sega_Ring_series
                case KnownSystem.SegaRingEdge2:
                    types.Add(MediaType.DVD);
                    break;

                // http://system16.com/hardware.php?id=911
                // https://en.wikipedia.org/wiki/List_of_Sega_arcade_system_boards#Sega_Ring_series
                case KnownSystem.SegaRingWide:
                    types.Add(MediaType.DVD);
                    break;

                // http://system16.com/hardware.php?id=711
                case KnownSystem.SegaTitanVideo:
                    types.Add(MediaType.CDROM);
                    break;

                // http://system16.com/hardware.php?id=709
                // http://system16.com/hardware.php?id=710
                case KnownSystem.SegaSystem32:
                    types.Add(MediaType.CDROM);
                    break;

                // https://github.com/mamedev/mame/blob/master/src/mame/drivers/seibucats.cpp
                case KnownSystem.SeibuCATSSystem:
                    types.Add(MediaType.DVD);
                    break;

                // https://www.tab.at/en/support/support/downloads
                case KnownSystem.TABAustriaQuizard:
                    types.Add(MediaType.CDROM);
                    break;

                // https://primetimeamusements.com/product/tsumo-multi-game-motion-system/
                // https://www.highwaygames.com/arcade-machines/tsumo-tsunami-motion-8117/
                case KnownSystem.TsunamiTsuMoMultiGameMotionSystem:
                    types.Add(MediaType.CDROM);
                    break;

                #endregion

                #region Others

                // https://en.wikipedia.org/wiki/Audio_CD
                case KnownSystem.AudioCD:
                    types.Add(MediaType.CDROM);
                    break;

                // https://en.wikipedia.org/wiki/Blu-ray#Player_profiles
                case KnownSystem.BDVideo:
                    types.Add(MediaType.BluRay);
                    break;

                // https://en.wikipedia.org/wiki/DVD-Video
                case KnownSystem.DVDVideo:
                    types.Add(MediaType.DVD);
                    break;

                // https://en.wikipedia.org/wiki/Blue_Book_(CD_standard)
                case KnownSystem.EnhancedCD:
                    types.Add(MediaType.CDROM);
                    break;

                // https://en.wikipedia.org/wiki/HD_DVD
                case KnownSystem.HDDVDVideo:
                    types.Add(MediaType.HDDVD);
                    break;

                // UNKNOWN
                case KnownSystem.NavisoftNaviken21:
                    types.Add(MediaType.CDROM);
                    break;

                // UNKNOWN
                case KnownSystem.PalmOS:
                    types.Add(MediaType.CDROM);
                    break;

                // UNKNOWN
                case KnownSystem.PhilipsCDiDigitalVideo:
                    types.Add(MediaType.CDROM);
                    break;

                // https://en.wikipedia.org/wiki/Photo_CD
                case KnownSystem.PhotoCD:
                    types.Add(MediaType.CDROM);
                    break;

                // UNKNOWN
                case KnownSystem.PlayStationGameSharkUpdates:
                    types.Add(MediaType.CDROM);
                    break;

                // https://en.wikipedia.org/wiki/Doors_and_Windows_(EP)
                case KnownSystem.RainbowDisc:
                    types.Add(MediaType.CDROM);
                    break;

                // https://en.wikipedia.org/wiki/Super_Audio_CD
                case KnownSystem.SuperAudioCD:
                    types.Add(MediaType.CDROM);
                    break;

                // https://www.cnet.com/products/tao-music-iktv-karaoke-station-karaoke-system-series/
                case KnownSystem.TaoiKTV:
                    types.Add(MediaType.CDROM);
                    break;

                // http://ultimateconsoledatabase.com/golden/kiss_site.htm
                case KnownSystem.TomyKissSite:
                    types.Add(MediaType.CDROM);
                    break;

                // https://en.wikipedia.org/wiki/Video_CD
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
                .Select(d => Drive.Optical(d.Name[0], (d.IsReady ? (string.IsNullOrWhiteSpace(d.VolumeLabel) ? "disc" : d.VolumeLabel) : Template.DiscNotDetected), d.IsReady))                
                .ToList();

            // Get the hard disk drives
            // TODO: Ensure DICUI isn't running on drive
            // TODO: Ensure output directory is not on drive
            List<Drive> hardDiskDrives = DriveInfo.GetDrives()
                .Where(d => d.DriveType == DriveType.Fixed)
                .Select(d => Drive.HardDisk(d.Name[0], string.IsNullOrWhiteSpace(d.VolumeLabel) ? "hdd" : d.VolumeLabel))
                .ToList();

            // Get the removable disk drives
            // TODO: Ensure DICUI isn't running on drive
            // TODO: Ensure output directory is not on drive
            List<Drive> removableDrives = DriveInfo.GetDrives()
                .Where(d => d.DriveType == DriveType.Removable)
                .Select(d => Drive.Removable(d.Name[0], (d.IsReady ? (string.IsNullOrWhiteSpace(d.VolumeLabel) ? "disc" : d.VolumeLabel) : Template.DiscNotDetected), d.IsReady))
                .ToList();

            // Add the lists together and order
            drives.AddRange(discDrives);
            drives.AddRange(hardDiskDrives);
            drives.AddRange(removableDrives);
            drives = drives.OrderBy(i => i.Letter).ToList();

            return drives;
        }

        /// <summary>
        /// Get the current media type from drive letter
        /// </summary>
        /// <param name="drive"></param>
        /// <returns></returns>
        /// <remarks>
        /// https://stackoverflow.com/questions/11420365/detecting-if-disc-is-in-dvd-drive
        /// </remarks>
        public static MediaType? GetMediaType(Drive drive)
        {
            // Take care of the non-optical stuff first
            if (drive.DriveType == InternalDriveType.Floppy)
                return MediaType.FloppyDisk;
            else if (drive.DriveType == InternalDriveType.HardDisk)
                return MediaType.HardDisk;
            else if (drive.DriveType == InternalDriveType.Removable)
                return MediaType.FlashDrive;

            // Get the DeviceID from the current drive letter
            string deviceId = null;
            try
            {
                ManagementObjectSearcher searcher =
                    new ManagementObjectSearcher("root\\CIMV2",
                    "SELECT * FROM Win32_CDROMDrive WHERE Id = '" + drive.Letter + ":\'");

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
                    return Converters.ToMediaType(media);
            }
            catch
            {
                // We don't care what the error is
            }

            return null;
        }

        /// <summary>
        /// Get the current system from drive
        /// </summary>
        /// <param name="drive"></param>
        /// <returns></returns>
        public static KnownSystem? GetKnownSystem(Drive drive)
        {
            // If drive or drive letter are provided, we can't do anything
            if (drive?.Letter == null)
                return null;

            string drivePath = $"{drive.Letter}:\\";

            // If we can't read the media in that drive, we can't do anything
            if (!Directory.Exists(drivePath))
                return null;

            // We're going to assume for floppies, HDDs, and removable drives
            // TODO: Try to be smarter about this
            if (drive.DriveType != InternalDriveType.Optical)
                return KnownSystem.IBMPCCompatible;

            // Sega Dreamcast
            if (File.Exists(Path.Combine(drivePath, "IP.BIN")))
            {
                return KnownSystem.SegaDreamcast;
            }

            // Sega Mega-CD / Sega-CD
            if (File.Exists(Path.Combine(drivePath, "_BOOT", "IP.BIN"))
                || File.Exists(Path.Combine(drivePath, "_BOOT", "SP.BIN"))
                || File.Exists(Path.Combine(drivePath, "_BOOT", "SP_AS.BIN"))
                || File.Exists(Path.Combine(drivePath, "FILESYSTEM.BIN")))
            {
                return KnownSystem.SegaCDMegaCD;
            }

            // Sony PlayStation and Sony PlayStation 2
            if (File.Exists(Path.Combine(drivePath, "SYSTEM.CNF")))
            {
                // Check for either BOOT or BOOT2
                using (StreamReader reader = File.OpenText(Path.Combine(drivePath, "SYSTEM.CNF")))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (line.Contains("BOOT2"))
                            return KnownSystem.SonyPlayStation2;
                        else if (line.Contains("BOOT"))
                            return KnownSystem.SonyPlayStation;
                    }
                }

                // If we have a weird disc, just assume PS1
                return KnownSystem.SonyPlayStation;
            }
            
            // Sony PlayStation 4
            if (drive.VolumeLabel.Equals("PS4VOLUME", StringComparison.OrdinalIgnoreCase))
            {
                return KnownSystem.SonyPlayStation4;
            }

            // V.Tech V.Flash / V.Smile Pro
            if (File.Exists(Path.Combine(drivePath, "0SYSTEM")))
            {
                return KnownSystem.VTechVFlashVSmilePro;
            }

            // Default return
            return KnownSystem.NONE;
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
                case MediaType.HardDisk:
                case MediaType.CompactFlash:
                case MediaType.SDCard:
                case MediaType.FlashDrive:
                case MediaType.HDDVD:
                    return Result.Success("{0} ready to dump", type.LongName());

                // Partially supported types
                case MediaType.GDROM:
                case MediaType.NintendoGameCubeGameDisc:
                case MediaType.NintendoWiiOpticalDisc:
                    return Result.Success("{0} partially supported for dumping", type.LongName());

                // Special case for other supported tools
                case MediaType.UMD:
                    return Result.Success("{0} supported for submission info parsing", type.LongName());

                // Specifically unknown type
                case MediaType.NONE:
                    return Result.Failure("Please select a valid media type");

                // Undumpable but recognized types
                default:
                    return Result.Failure("{0} media are not supported for dumping", type.LongName());
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
                return "Path could not be scanned!";
            }
        }
    }
}
