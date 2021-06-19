using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using BurnOutSharp;
using MPF.Data;
#if NET_FRAMEWORK
using IMAPI2;
#endif

namespace MPF.Utilities
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

                case KnownSystem.MattelFisherPriceiXL:
                    types.Add(MediaType.CDROM);
                    break;

                // https://en.wikipedia.org/wiki/HyperScan
                case KnownSystem.MattelHyperscan:
                    types.Add(MediaType.CDROM);
                    break;

                // https://en.wikipedia.org/wiki/Xbox_(console)
                case KnownSystem.MicrosoftXBOX:
                    types.Add(MediaType.DVD);
                    types.Add(MediaType.CDROM);
                    break;

                // https://en.wikipedia.org/wiki/Xbox_360
                case KnownSystem.MicrosoftXBOX360:
                    types.Add(MediaType.DVD);
                    types.Add(MediaType.CDROM);
                    break;

                // https://en.wikipedia.org/wiki/Xbox_One
                case KnownSystem.MicrosoftXBOXOne:
                    types.Add(MediaType.BluRay);
                    break;

                // https://en.wikipedia.org/wiki/Xbox_Series_X_and_Series_S
                case KnownSystem.MicrosoftXboxSeriesXS:
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
                    types.Add(MediaType.DVD); // Only added here to help users; not strictly correct
                    types.Add(MediaType.NintendoGameCubeGameDisc);
                    break;

                // https://en.wikipedia.org/wiki/Super_NES_CD-ROM
                case KnownSystem.NintendoSonySuperNESCDROMSystem:
                    types.Add(MediaType.CDROM);
                    break;

                // https://en.wikipedia.org/wiki/Wii
                case KnownSystem.NintendoWii:
                    types.Add(MediaType.DVD); // Only added here to help users; not strictly correct
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

                // https://en.wikipedia.org/wiki/PlayStation_5
                case KnownSystem.SonyPlayStation5:
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
                    types.Add(MediaType.DataCartridge);
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
                case KnownSystem.funworldPhotoPlay:
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

                // https://en.wikipedia.org/wiki/DVD-Audio
                case KnownSystem.DVDAudio:
                    types.Add(MediaType.DVD);
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

                // https://segaretro.org/Prologue_21
                case KnownSystem.SegaPrologue21:
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
                .Select(d => new Drive(Converters.ToInternalDriveType(d.DriveType), d))
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

#if NET_FRAMEWORK
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
#else
                // TODO: This entire .NET Core path still doesn't work
                // This may honestly require an entire import of IMAPI2 stuff and then try
                // as best as possible to get it working.

                return (null, "Media detection only supported on .NET Framework");
#endif

            }
            catch (Exception ex)
            {
                return (null, ex.Message);
            }
        }

        /// <summary>
        /// Get the current system from drive
        /// </summary>
        /// <param name="drive"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static KnownSystem? GetKnownSystem(Drive drive, KnownSystem? defaultValue)
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
                return KnownSystem.IBMPCCompatible;

            // Audio CD
            if (drive.VolumeLabel.Equals("Audio CD", StringComparison.OrdinalIgnoreCase))
            {
                return KnownSystem.AudioCD;
            }

            // DVD-Audio
            if (Directory.Exists(Path.Combine(drivePath, "AUDIO_TS"))
                && Directory.EnumerateFiles(Path.Combine(drivePath, "AUDIO_TS")).Count() > 0)
            {
                return KnownSystem.DVDAudio;
            }

            // DVD-Video and Xbox
            if (Directory.Exists(Path.Combine(drivePath, "VIDEO_TS"))
                && Directory.EnumerateFiles(Path.Combine(drivePath, "VIDEO_TS")).Count() > 0)
            {
                // TODO: Maybe add video track hashes to compare for Xbox and X360?
                if (drive.VolumeLabel.StartsWith("SEP13011042", StringComparison.OrdinalIgnoreCase))
                    return KnownSystem.MicrosoftXBOX;

                return KnownSystem.DVDVideo;
            }

            // HD-DVD-Video
            if (Directory.Exists(Path.Combine(drivePath, "HVDVD_TS"))
                && Directory.EnumerateFiles(Path.Combine(drivePath, "HVDVD_TS")).Count() > 0)
            {
                return KnownSystem.HDDVDVideo;
            }

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

            // Sega Saturn
            try
            {
                byte[] sector = drive?.ReadSector(0);
                if (sector != null)
                {
                    if (sector.StartsWith(Interface.SaturnSectorZeroStart))
                        return KnownSystem.SegaSaturn;
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
                    return KnownSystem.SonyPlayStation;
                else if (systemCnf.ContainsKey("BOOT2"))
                    return KnownSystem.SonyPlayStation2;
            }
            else if (File.Exists(psxExePath))
            {
                return KnownSystem.SonyPlayStation;
            }

            // Sony PlayStation 4
            if (drive.VolumeLabel.Equals("PS4VOLUME", StringComparison.OrdinalIgnoreCase))
            {
                return KnownSystem.SonyPlayStation4;
            }

            // Sony PlayStation 5
            if (drive.VolumeLabel.Equals("PS5VOLUME", StringComparison.OrdinalIgnoreCase))
            {
                return KnownSystem.SonyPlayStation5;
            }

            // V.Tech V.Flash / V.Smile Pro
            if (File.Exists(Path.Combine(drivePath, "0SYSTEM")))
            {
                return KnownSystem.VTechVFlashVSmilePro;
            }

            // VCD
            if (Directory.Exists(Path.Combine(drivePath, "VCD"))
                && Directory.EnumerateFiles(Path.Combine(drivePath, "VCD")).Count() > 0)
            {
                return KnownSystem.VideoCD;
            }

            // Default return
            return defaultValue;
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
                        IncludePosition = options.IncludeDebugProtectionInformation,
                        ScanAllFiles = options.ForceScanningForProtection,
                        ScanArchives = options.ScanArchivesForProtection,
                        ScanPackers = options.ScanPackersForProtection,
                    };
                    return scanner.GetProtections(path);
                });

                if (found == null || found.Count == 0)
                    return (true, "None found");

                // Join the output protections for writing
                string protections = string.Join(", ", found
                    .Where(kvp => kvp.Value != null && kvp.Value.Any())
                    .SelectMany(kvp => kvp.Value)
                    .Distinct());
                return (true, protections);
            }
            catch (Exception ex)
            {
                return (false, ex.ToString());
            }
        }
    }
}
