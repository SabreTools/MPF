using IMAPI2;
using DICUI.Data;

namespace DICUI.Utilities
{
    public static class Converters
    {
        #region Cross-enumeration conversions

        /// <summary>
        /// Get the most common known system for a given MediaType
        /// </summary>
        /// <param name="baseCommand">DICCommand value to check</param>
        /// <returns>KnownSystem if possible, null on error</returns>
        public static KnownSystem? ToKnownSystem(this DICCommand baseCommand)
        {
            switch (baseCommand)
            {
                case DICCommand.Audio:
                    return KnownSystem.AudioCD;
                case DICCommand.CompactDisc:
                case DICCommand.Data:
                case DICCommand.DigitalVideoDisc:
                case DICCommand.Floppy:
                    return KnownSystem.IBMPCCompatible;
                case DICCommand.GDROM:
                case DICCommand.Swap:
                    return KnownSystem.SegaDreamcast;
                case DICCommand.BluRay:
                    return KnownSystem.SonyPlayStation3;
                case DICCommand.XBOX:
                case DICCommand.XBOXSwap:
                    return KnownSystem.MicrosoftXBOX;
                case DICCommand.XGD2Swap:
                case DICCommand.XGD3Swap:
                    return KnownSystem.MicrosoftXBOX360;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Get the MediaType associated with a given base command
        /// </summary>
        /// <param name="baseCommand">DICCommand value to check</param>
        /// <returns>MediaType if possible, null on error</returns>
        /// <remarks>This takes the "safe" route by assuming the larger of any given format</remarks>
        public static MediaType? ToMediaType(this DICCommand baseCommand)
        {
            switch (baseCommand)
            {
                case DICCommand.Audio:
                case DICCommand.CompactDisc:
                case DICCommand.Data:
                    return MediaType.CDROM;
                case DICCommand.GDROM:
                case DICCommand.Swap:
                    return MediaType.GDROM;
                case DICCommand.DigitalVideoDisc:
                case DICCommand.XBOX:
                case DICCommand.XBOXSwap:
                case DICCommand.XGD2Swap:
                case DICCommand.XGD3Swap:
                    return MediaType.DVD;
                case DICCommand.BluRay:
                    return MediaType.BluRay;

                // Non-optical
                case DICCommand.Floppy:
                    return MediaType.FloppyDisk;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Convert IMAPI physical media type to a MediaType
        /// </summary>
        /// <param name="type">IMAPI_MEDIA_PHYSICAL_TYPE value to check</param>
        /// <returns>MediaType if possible, null on error</returns>
        public static MediaType? ToMediaType(IMAPI_MEDIA_PHYSICAL_TYPE type)
        {
            switch (type)
            {
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_UNKNOWN:
                    return MediaType.NONE;
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_CDROM:
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_CDR:
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_CDRW:
                    return MediaType.CDROM;
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDROM:
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDRAM:
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDPLUSR:
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDPLUSRW:
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDPLUSR_DUALLAYER:
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDDASHR:
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDDASHRW:
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDDASHR_DUALLAYER:
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DISK:
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_DVDPLUSRW_DUALLAYER:
                    return MediaType.DVD;
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_HDDVDROM:
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_HDDVDR:
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_HDDVDRAM:
                    return MediaType.HDDVD;
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_BDROM:
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_BDR:
                case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_BDRE:
                    return MediaType.BluRay;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Get the default extension for a given disc type
        /// </summary>
        /// <param name="type">MediaType value to check</param>
        /// <returns>Valid extension (with leading '.'), null on error</returns>
        public static string Extension(this MediaType? type)
        {
            switch (type)
            {
                case MediaType.CDROM:
                case MediaType.GDROM:
                case MediaType.Cartridge:
                    return ".bin";
                case MediaType.DVD:
                case MediaType.HDDVD:
                case MediaType.BluRay:
                case MediaType.NintendoWiiOpticalDisc:
                case MediaType.UMD:
                    return ".iso";
                case MediaType.LaserDisc:
                case MediaType.NintendoGameCubeGameDisc:
                    return ".raw";
                case MediaType.NintendoWiiUOpticalDisc:
                    return ".wud";
                case MediaType.FloppyDisk:
                    return ".img";
                case MediaType.Cassette:
                    return ".wav";
                case MediaType.NONE:
                default:
                    return null;
            }
        }

        #endregion

        #region Convert to Long Name

        /// <summary>
        /// Get the string representation of the Category enum values
        /// </summary>
        /// <param name="category">Category value to convert</param>
        /// <returns>Short string representing the value, if possible</returns>
        public static string LongName(this Category? category)
        {
            switch (category)
            {
                case Category.Games:
                    return "Games";
                case Category.Demos:
                    return "Demos";
                case Category.Video:
                    return "Video";
                case Category.Audio:
                    return "Audio";
                case Category.Multimedia:
                    return "Multimedia";
                case Category.Applications:
                    return "Applications";
                case Category.Coverdiscs:
                    return "Coverdiscs";
                case Category.Educational:
                    return "Educational";
                case Category.BonusDiscs:
                    return "Bonus Discs";
                case Category.Preproduction:
                    return "Preproduction";
                case Category.AddOns:
                    return "Add-Ons";
                default:
                    return null;
            }
        }

        /// <summary>
        /// Get the string representation of the DICCommand enum values
        /// </summary>
        /// <param name="command">DICCommand value to convert</param>
        /// <returns>String representing the value, if possible</returns>
        public static string LongName(this DICCommand command)
        {
            switch (command)
            {
                case DICCommand.Audio:
                    return DICCommandStrings.Audio;
                case DICCommand.BluRay:
                    return DICCommandStrings.BluRay;
                case DICCommand.Close:
                    return DICCommandStrings.Close;
                case DICCommand.CompactDisc:
                    return DICCommandStrings.CompactDisc;
                case DICCommand.Data:
                    return DICCommandStrings.Data;
                case DICCommand.DigitalVideoDisc:
                    return DICCommandStrings.DigitalVideoDisc;
                case DICCommand.DriveSpeed:
                    return DICCommandStrings.DriveSpeed;
                case DICCommand.Eject:
                    return DICCommandStrings.Eject;
                case DICCommand.Floppy:
                    return DICCommandStrings.Floppy;
                case DICCommand.GDROM:
                    return DICCommandStrings.GDROM;
                case DICCommand.MDS:
                    return DICCommandStrings.MDS;
                case DICCommand.Merge:
                    return DICCommandStrings.Merge;
                case DICCommand.Reset:
                    return DICCommandStrings.Reset;
                case DICCommand.Start:
                    return DICCommandStrings.Start;
                case DICCommand.Stop:
                    return DICCommandStrings.Stop;
                case DICCommand.Sub:
                    return DICCommandStrings.Sub;
                case DICCommand.Swap:
                    return DICCommandStrings.Swap;
                case DICCommand.XBOX:
                    return DICCommandStrings.XBOX;
                case DICCommand.XBOXSwap:
                    return DICCommandStrings.XBOXSwap;
                case DICCommand.XGD2Swap:
                    return DICCommandStrings.XGD2Swap;
                case DICCommand.XGD3Swap:
                    return DICCommandStrings.XGD3Swap;

                case DICCommand.NONE:
                default:
                    return "";
            }
        }

        /// <summary>
        /// Get the string representation of the DICFlag enum values
        /// </summary>
        /// <param name="command">DICFlag value to convert</param>
        /// <returns>String representing the value, if possible</returns>
        public static string LongName(this DICFlag flag)
        {
            switch (flag)
            {
                case DICFlag.AddOffset:
                    return DICFlagStrings.AddOffset;
                case DICFlag.AMSF:
                    return DICFlagStrings.AMSF;
                case DICFlag.BEOpcode:
                    return DICFlagStrings.BEOpcode;
                case DICFlag.C2Opcode:
                    return DICFlagStrings.C2Opcode;
                case DICFlag.CopyrightManagementInformation:
                    return DICFlagStrings.CopyrightManagementInformation;
                case DICFlag.D8Opcode:
                    return DICFlagStrings.D8Opcode;
                case DICFlag.DisableBeep:
                    return DICFlagStrings.DisableBeep;
                case DICFlag.ForceUnitAccess:
                    return DICFlagStrings.ForceUnitAccess;
                case DICFlag.MCN:
                    return DICFlagStrings.MCN;
                case DICFlag.MultiSession:
                    return DICFlagStrings.MultiSession;
                case DICFlag.NoFixSubP:
                    return DICFlagStrings.NoFixSubP;
                case DICFlag.NoFixSubQ:
                    return DICFlagStrings.NoFixSubQ;
                case DICFlag.NoFixSubQLibCrypt:
                    return DICFlagStrings.NoFixSubQLibCrypt;
                case DICFlag.NoFixSubQSecuROM:
                    return DICFlagStrings.NoFixSubQSecuROM;
                case DICFlag.NoFixSubRtoW:
                    return DICFlagStrings.NoFixSubRtoW;
                case DICFlag.Raw:
                    return DICFlagStrings.Raw;
                case DICFlag.Reverse:
                    return DICFlagStrings.Reverse;
                case DICFlag.ScanAntiMod:
                    return DICFlagStrings.ScanAntiMod;
                case DICFlag.ScanFileProtect:
                    return DICFlagStrings.ScanFileProtect;
                case DICFlag.ScanSectorProtect:
                    return DICFlagStrings.ScanSectorProtect;
                case DICFlag.SeventyFour:
                    return DICFlagStrings.SeventyFour;
                case DICFlag.SkipSector:
                    return DICFlagStrings.SkipSector;
                case DICFlag.SubchannelReadLevel:
                    return DICFlagStrings.SubchannelReadLevel;
                case DICFlag.VideoNow:
                    return DICFlagStrings.VideoNow;

                case DICFlag.NONE:
                default:
                    return "";
            }
        }

        /// <summary>
        /// Get the string representation of the KnownSystem enum values
        /// </summary>
        /// <param name="sys">KnownSystem value to convert</param>
        /// <returns>String representing the value, if possible</returns>
        public static string LongName(this KnownSystem? sys)
        {
            switch (sys)
            {
                #region Consoles

                case KnownSystem.AtariJaguarCD:
                    return "Atari Jaguar CD";
                case KnownSystem.BandaiPlaydiaQuickInteractiveSystem:
                    return "Bandai Playdia Quick Interactive System";
                case KnownSystem.BandaiApplePippin:
                    return "Bandai / Apple Pippin";
                case KnownSystem.CommodoreAmigaCD32:
                    return "Commodore Amiga CD32";
                case KnownSystem.CommodoreAmigaCDTV:
                    return "Commodore Amiga CDTV";
                case KnownSystem.EnvizionsEVOSmartConsole:
                    return "Envizions EVO Smart Console";
                case KnownSystem.FujitsuFMTownsMarty:
                    return "Fujitsu FM Towns Marty";
                case KnownSystem.HasbroVideoNow:
                    return "Hasbro VideoNow";
                case KnownSystem.HasbroVideoNowColor:
                    return "Hasbro VideoNow Color";
                case KnownSystem.HasbroVideoNowJr:
                    return "Hasbro VideoNow Jr.";
                case KnownSystem.HasbroVideoNowXP:
                    return "Hasbro VideoNow XP";
                case KnownSystem.MattelHyperscan:
                    return "Mattel HyperScan";
                case KnownSystem.MicrosoftXBOX:
                    return "Microsoft XBOX";
                case KnownSystem.MicrosoftXBOX360:
                    return "Microsoft XBOX 360";
                case KnownSystem.MicrosoftXBOXOne:
                    return "Microsoft XBOX One";
                case KnownSystem.NECPCEngineTurboGrafxCD:
                    return "NEC PC-Engine / TurboGrafx CD";
                case KnownSystem.NECPCFX:
                    return "NEC PC-FX / PC-FXGA";
                case KnownSystem.NintendoGameCube:
                    return "Nintendo GameCube";
                case KnownSystem.NintendoSonySuperNESCDROMSystem:
                    return "Nintendo-Sony Super NES CD-ROM System";
                case KnownSystem.NintendoWii:
                    return "Nintendo Wii";
                case KnownSystem.NintendoWiiU:
                    return "Nintendo Wii U";
                case KnownSystem.Panasonic3DOInteractiveMultiplayer:
                    return "Panasonic 3DO Interactive Multiplayer";
                case KnownSystem.PhilipsCDi:
                    return "Philips CD-i";
                case KnownSystem.PioneerLaserActive:
                    return "Pioneer LaserActive";
                case KnownSystem.SegaCDMegaCD:
                    return "Sega CD / Mega CD";
                case KnownSystem.SegaDreamcast:
                    return "Sega Dreamcast";
                case KnownSystem.SegaSaturn:
                    return "Sega Saturn";
                case KnownSystem.SNKNeoGeoCD:
                    return "SNK Neo Geo CD";
                case KnownSystem.SonyPlayStation:
                    return "Sony PlayStation";
                case KnownSystem.SonyPlayStation2:
                    return "Sony PlayStation 2";
                case KnownSystem.SonyPlayStation3:
                    return "Sony PlayStation 3";
                case KnownSystem.SonyPlayStation4:
                    return "Sony PlayStation 4";
                case KnownSystem.SonyPlayStationPortable:
                    return "Sony PlayStation Portable";
                case KnownSystem.TandyMemorexVisualInformationSystem:
                    return "Tandy / Memorex Visual Information System";
                case KnownSystem.VMLabsNuon:
                    return "VM Labs NUON";
                case KnownSystem.VTechVFlashVSmilePro:
                    return "VTech V.Flash - V.Smile Pro";
                case KnownSystem.ZAPiTGamesGameWaveFamilyEntertainmentSystem:
                    return "ZAPiT Games Game Wave Family Entertainment System";

                #endregion

                #region Computers

                case KnownSystem.AcornArchimedes:
                    return "Acorn Archimedes";
                case KnownSystem.AppleMacintosh:
                    return "Apple Macintosh";
                case KnownSystem.CommodoreAmiga:
                    return "Commodore Amiga";
                case KnownSystem.FujitsuFMTowns:
                    return "Fujitsu FM Towns series";
                case KnownSystem.IBMPCCompatible:
                    return "IBM PC Compatible";
                case KnownSystem.NECPC88:
                    return "NEC PC-88";
                case KnownSystem.NECPC98:
                    return "NEC PC-98";
                case KnownSystem.SharpX68000:
                    return "Sharp X68000";

                #endregion

                #region Arcade

                case KnownSystem.AmigaCUBOCD32:
                    return "Amiga CUBO CD32";
                case KnownSystem.AmericanLaserGames3DO:
                    return "American Laser Games 3DO";
                case KnownSystem.Atari3DO:
                    return "Atari 3DO";
                case KnownSystem.Atronic:
                    return "Atronic";
                case KnownSystem.AUSCOMSystem1:
                    return "AUSCOM System 1";
                case KnownSystem.BallyGameMagic:
                    return "Bally Game Magic";
                case KnownSystem.CapcomCPSystemIII:
                    return "Capcom CP System III";
                case KnownSystem.GlobalVRVarious:
                    return "Global VR PC-based Systems";
                case KnownSystem.GlobalVRVortek:
                    return "Global VR Vortek";
                case KnownSystem.GlobalVRVortekV3:
                    return "Global VR Vortek V3";
                case KnownSystem.ICEPCHardware:
                    return "ICE PC-based Hardware";
                case KnownSystem.IncredibleTechnologiesEagle:
                    return "Incredible Technologies Eagle";
                case KnownSystem.IncredibleTechnologiesVarious:
                    return "Incredible Technologies PC-based Systems";
                case KnownSystem.KonamieAmusement:
                    return "Konami e-Amusement";
                case KnownSystem.KonamiFirebeat:
                    return "Konami Firebeat";
                case KnownSystem.KonamiGVSystem:
                    return "Konami GV System";
                case KnownSystem.KonamiM2:
                    return "Konami M2";
                case KnownSystem.KonamiPython:
                    return "Konami Python";
                case KnownSystem.KonamiPython2:
                    return "Konami Python 2";
                case KnownSystem.KonamiSystem573:
                    return "Konami System 573";
                case KnownSystem.KonamiTwinkle:
                    return "Konami Twinkle";
                case KnownSystem.KonamiVarious:
                    return "Konami PC-based Systems";
                case KnownSystem.MeritIndustriesBoardwalk:
                    return "Merit Industries Boardwalk";
                case KnownSystem.MeritIndustriesMegaTouchForce:
                    return "Merit Industries MegaTouch Force";
                case KnownSystem.MeritIndustriesMegaTouchION:
                    return "Merit Industries MegaTouch ION";
                case KnownSystem.MeritIndustriesMegaTouchMaxx:
                    return "Merit Industries MegaTouch Maxx";
                case KnownSystem.MeritIndustriesMegaTouchXL:
                    return "Merit Industries MegaTouch XL";
                case KnownSystem.NamcoCapcomSystem256:
                    return "Namco / Capcom System 256/Super System 256";
                case KnownSystem.NamcoCapcomTaitoSystem246:
                    return "Namco / Capcom / Taito System 246";
                case KnownSystem.NamcoSegaNintendoTriforce:
                    return "Namco / Sega / Nintendo Triforce";
                case KnownSystem.NamcoSystem12:
                    return "Namco System 12";
                case KnownSystem.NamcoSystem357:
                    return "Namco System 357";
                case KnownSystem.NewJatreCDi:
                    return "New Jatre CD-i";
                case KnownSystem.NichibutsuHighRateSystem:
                    return "Nichibutsu High Rate System";
                case KnownSystem.NichibutsuSuperCD:
                    return "Nichibutsu Super CD";
                case KnownSystem.NichibutsuXRateSystem:
                    return "Nichibutsu X-Rate System";
                case KnownSystem.PanasonicM2:
                    return "Panasonic M2";
                case KnownSystem.PhotoPlayVarious:
                    return "PhotoPlay PC-based Systems";
                case KnownSystem.RawThrillsVarious:
                    return "Raw Thrills PC-based Systems";
                case KnownSystem.SegaChihiro:
                    return "Sega Chihiro";
                case KnownSystem.SegaEuropaR:
                    return "Sega Europa-R";
                case KnownSystem.SegaLindbergh:
                    return "Sega Lindbergh";
                case KnownSystem.SegaNaomi:
                    return "Sega Naomi";
                case KnownSystem.SegaNaomi2:
                    return "Sega Naomi 2";
                case KnownSystem.SegaNu:
                    return "Sega Nu";
                case KnownSystem.SegaRingEdge:
                    return "Sega RingEdge";
                case KnownSystem.SegaRingEdge2:
                    return "Sega RingEdge 2";
                case KnownSystem.SegaRingWide:
                    return "Sega RingWide";
                case KnownSystem.SegaTitanVideo:
                    return "Sega Titan Video";
                case KnownSystem.SegaSystem32:
                    return "Sega System 32";
                case KnownSystem.SeibuCATSSystem:
                    return "Seibu CATS System";
                case KnownSystem.TABAustriaQuizard:
                    return "TAB-Austria Quizard";
                case KnownSystem.TsunamiTsuMoMultiGameMotionSystem:
                    return "Tsunami TsuMo Multi-Game Motion System";

                #endregion

                #region Others

                case KnownSystem.AudioCD:
                    return "Audio CD";
                case KnownSystem.BDVideo:
                    return "BD-Video";
                case KnownSystem.DVDVideo:
                    return "DVD-Video";
                case KnownSystem.EnhancedCD:
                    return "Enhanced CD";
                case KnownSystem.HDDVDVideo:
                    return "HD-DVD-Video";
                case KnownSystem.NavisoftNaviken21:
                    return "Navisoft Naviken 2.1";
                case KnownSystem.PalmOS:
                    return "PalmOS";
                case KnownSystem.PhilipsCDiDigitalVideo:
                    return "Philips CD-i Digital Video";
                case KnownSystem.PhotoCD:
                    return "Photo CD";
                case KnownSystem.PlayStationGameSharkUpdates:
                    return "PlayStation GameShark Updates";
                case KnownSystem.RainbowDisc:
                    return "Rainbow Disc";
                case KnownSystem.TaoiKTV:
                    return "Tao iKTV";
                case KnownSystem.TomyKissSite:
                    return "Tomy Kiss-Site";
                case KnownSystem.VideoCD:
                    return "Video CD";

                #endregion

                case KnownSystem.NONE:
                default:
                    return "Unknown";
            }
        }

        /// <summary>
        /// Get the string representation of the KnownSystemCategory enum values
        /// </summary>
        /// <param name="category">KnownSystemCategory value to convert</param>
        /// <returns>String representing the value, if possible</returns>
        public static string LongName(this KnownSystemCategory? category)
        {
            switch (category)
            {
                case KnownSystemCategory.Arcade:
                    return "Arcade";
                case KnownSystemCategory.Computer:
                    return "Computers";
                case KnownSystemCategory.DiscBasedConsole:
                    return "Disc-Based Consoles";
                case KnownSystemCategory.OtherConsole:
                    return "Other Consoles";
                case KnownSystemCategory.Other:
                    return "Other";
                case KnownSystemCategory.Custom:
                    return "Custom";
                default:
                    return "";
            }
        }

        /// <summary>
        /// Get the string representation of the Language enum values
        /// </summary>
        /// <param name="lang">Language value to convert</param>
        /// <returns>String representing the value, if possible</returns>
        public static string LongName(this Language? lang)
        {
            switch (lang)
            {
                case Language.Afrikaans:
                    return "Afrikaans";
                case Language.Arabic:
                    return "Arabic";
                case Language.Basque:
                    return "Basque";
                case Language.Bulgarian:
                    return "Bulgarian";
                case Language.Catalan:
                    return "Catalan";
                case Language.Chinese:
                    return "Chinese";
                case Language.Croatian:
                    return "Croatian";
                case Language.Czech:
                    return "Czech";
                case Language.Danish:
                    return "Danish";
                case Language.Dutch:
                    return "Dutch";
                case Language.English:
                    return "English";
                case Language.Finnish:
                    return "Finnish";
                case Language.French:
                    return "French";
                case Language.Gaelic:
                    return "Gaelic";
                case Language.German:
                    return "German";
                case Language.Greek:
                    return "Greek";
                case Language.Hebrew:
                    return "Hebrew";
                case Language.Hindi:
                    return "Hindi";
                case Language.Hungarian:
                    return "Hungarian";
                case Language.Italian:
                    return "Italian";
                case Language.Japanese:
                    return "Japanese";
                case Language.Korean:
                    return "Korean";
                case Language.Norwegian:
                    return "Norwegian";
                case Language.Polish:
                    return "Polish";
                case Language.Portuguese:
                    return "Portuguese";
                case Language.Punjabi:
                    return "Punjabi";
                case Language.Romanian:
                    return "Romanian";
                case Language.Russian:
                    return "Russian";
                case Language.Slovak:
                    return "Slovak";
                case Language.Slovenian:
                    return "Slovenian";
                case Language.Spanish:
                    return "Spanish";
                case Language.Swedish:
                    return "Swedish";
                case Language.Tamil:
                    return "Tamil";
                case Language.Thai:
                    return "Thai";
                case Language.Turkish:
                    return "Turkish";
                case Language.Ukrainian:
                    return "Ukrainian";
                default:
                    return "Klingon (CHANGE THIS)";
            }
        }

        /// <summary>
        /// Get the string representation of the MediaType enum values
        /// </summary>
        /// <param name="type">MediaType value to convert</param>
        /// <returns>String representing the value, if possible</returns>
        public static string LongName(this MediaType? type)
        {
            switch (type)
            {
                #region Punched Media

                case MediaType.ApertureCard:
                    return "Aperture card";
                case MediaType.JacquardLoomCard:
                    return "Jacquard Loom card";
                case MediaType.MagneticStripeCard:
                    return "Magnetic stripe card";
                case MediaType.OpticalPhonecard:
                    return "Optical phonecard";
                case MediaType.PunchedCard:
                    return "Punched card";
                case MediaType.PunchedTape:
                    return "Punched tape";

                #endregion

                #region Tape

                case MediaType.OpenReel:
                    return "Open Reel Tape";
                case MediaType.DataCartridge:
                    return "Data Tape Cartridge";
                case MediaType.Cassette:
                    return "Cassette Tape";

                #endregion

                #region Disc / Disc

                case MediaType.BluRay:
                    return "BD-ROM";
                case MediaType.CDROM:
                    return "CD-ROM";
                case MediaType.DVD:
                    return "DVD-ROM";
                case MediaType.FloppyDisk:
                    return "Floppy Disk";
                case MediaType.Floptical:
                    return "Floptical";
                case MediaType.GDROM:
                    return "GD-ROM";
                case MediaType.HDDVD:
                    return "HD-DVD-ROM";
                case MediaType.HardDisk:
                    return "Hard Disk";
                case MediaType.IomegaBernoulliDisk:
                    return "Iomega Bernoulli Disk";
                case MediaType.IomegaJaz:
                    return "Iomega Jaz";
                case MediaType.IomegaZip:
                    return "Iomega Zip";
                case MediaType.LaserDisc:
                    return "LD-ROM / LV-ROM";
                case MediaType.Nintendo64DD:
                    return "64DD Disk";
                case MediaType.NintendoFamicomDiskSystem:
                    return "Famicom Disk System Disk";
                case MediaType.NintendoGameCubeGameDisc:
                    return "GameCube Game Disc";
                case MediaType.NintendoWiiOpticalDisc:
                    return "Wii Optical Disc";
                case MediaType.NintendoWiiUOpticalDisc:
                    return "Wii U Optical Disc";
                case MediaType.UMD:
                    return "UMD";

                #endregion

                // Unsorted Formats
                case MediaType.Cartridge:
                    return "Cartridge";
                case MediaType.CED:
                    return "CED";

                case MediaType.NONE:
                default:
                    return "Unknown";
            }
        }

        /// <summary>
        /// Get the string representation of the Region enum values
        /// </summary>
        /// <param name="region">Region value to convert</param>
        /// <returns>String representing the value, if possible</returns>
        public static string LongName(this Region? region)
        {
            switch (region)
            {
                case Region.Argentina:
                    return "Argentina";
                case Region.Asia:
                    return "Asia";
                case Region.AsiaEurope:
                    return "Asia, Europe";
                case Region.AsiaUSA:
                    return "Asia, USA";
                case Region.Australia:
                    return "Australia";
                case Region.Austria:
                    return "Austria";
                case Region.AustriaSwitzerland:
                    return "Austria, Switzerland";
                case Region.Belgium:
                    return "Belgium";
                case Region.BelgiumNetherlands:
                    return "Belgium, Netherlands";
                case Region.Brazil:
                    return "Brazil";
                case Region.Canada:
                    return "Canada";
                case Region.China:
                    return "China";
                case Region.Croatia:
                    return "Croatia";
                case Region.Czech:
                    return "Czech";
                case Region.Denmark:
                    return "Denmark";
                case Region.Europe:
                    return "Europe";
                case Region.EuropeAsia:
                    return "Europe, Asia";
                case Region.EuropeAustralia:
                    return "Europe, Australia";
                case Region.Finland:
                    return "Finland";
                case Region.France:
                    return "France";
                case Region.FranceSpain:
                    return "France, Spain";
                case Region.Germany:
                    return "Germany";
                case Region.Greece:
                    return "Greece";
                case Region.Hungary:
                    return "Hungary";
                case Region.India:
                    return "India";
                case Region.Ireland:
                    return "Ireland";
                case Region.Israel:
                    return "Israel";
                case Region.Italy:
                    return "Italy";
                case Region.Japan:
                    return "Japan";
                case Region.JapanAsia:
                    return "Japan, Asia";
                case Region.JapanEurope:
                    return "Japan, Europe";
                case Region.JapanKorea:
                    return "Japan, Korea";
                case Region.JapanUSA:
                    return "Japan, USA";
                case Region.Korea:
                    return "Korea";
                case Region.LatinAmerica:
                    return "Latin America";
                case Region.Netherlands:
                    return "Netherlands";
                case Region.Norway:
                    return "Norway";
                case Region.Poland:
                    return "Poland";
                case Region.Portugal:
                    return "Portugal";
                case Region.Russia:
                    return "Russia";
                case Region.Scandinavia:
                    return "Scandinavia";
                case Region.Singapore:
                    return "Singapore";
                case Region.Slovakia:
                    return "Slovakia";
                case Region.SouthAfrica:
                    return "South Africa";
                case Region.Spain:
                    return "Spain";
                case Region.Sweden:
                    return "Sweden";
                case Region.Switzerland:
                    return "Switzerland";
                case Region.Taiwan:
                    return "Taiwan";
                case Region.Thailand:
                    return "Thailand";
                case Region.Turkey:
                    return "Turkey";
                case Region.UnitedArabEmirates:
                    return "United Arab Emirates";
                case Region.UK:
                    return "UK";
                case Region.Ukraine:
                    return "Ukraine";
                case Region.USA:
                    return "USA";
                case Region.USAAsia:
                    return "USA, Asia";
                case Region.USABrazil:
                    return "USA, Brazil";
                case Region.USAEurope:
                    return "USA, Europe";
                case Region.USAJapan:
                    return "USA, Japan";
                case Region.World:
                    return "World";
                default:
                    return "World (CHANGE THIS)";
            }
        }

        /// <summary>
        /// Get the string representation of the YesNo enum values
        /// </summary>
        /// <param name="yesno">YesNo value to convert</param>
        /// <returns>String representing the value, if possible</returns>
        public static string LongName(this YesNo yesno)
        {
            switch(yesno)
            {
                case YesNo.No:
                    return "No";
                case YesNo.Yes:
                    return "Yes";
                default:
                case YesNo.NULL:
                    return "Yes/No";
            }
        }

        #endregion

        #region Convert to Short Name

        /// <summary>
        /// Get the short string representation of the KnownSystem enum values
        /// </summary>
        /// <param name="sys">KnownSystem value to convert</param>
        /// <returns>Short string representing the value, if possible</returns>
        public static string ShortName(this KnownSystem? sys)
        {
            switch (sys)
            {
                #region Consoles

                case KnownSystem.AtariJaguarCD:
                    return "jaguar";
                case KnownSystem.BandaiPlaydiaQuickInteractiveSystem:
                    return "playdia";
                case KnownSystem.BandaiApplePippin:
                    return "pippin";
                case KnownSystem.CommodoreAmigaCD32:
                    return "cd32";
                case KnownSystem.EnvizionsEVOSmartConsole:
                    return "evosc";
                case KnownSystem.FujitsuFMTownsMarty:
                    return "fmtm";
                case KnownSystem.CommodoreAmigaCDTV:
                    return "cdtv";
                case KnownSystem.HasbroVideoNow:
                    return "videonow";
                case KnownSystem.HasbroVideoNowColor:
                    return "videonowcolor";
                case KnownSystem.HasbroVideoNowJr:
                    return "videonowjr";
                case KnownSystem.HasbroVideoNowXP:
                    return "videonowxp";
                case KnownSystem.MattelHyperscan:
                    return "hyperscan";
                case KnownSystem.MicrosoftXBOX:
                    return "xbox";
                case KnownSystem.MicrosoftXBOX360:
                    return "x360";
                case KnownSystem.MicrosoftXBOXOne:
                    return "xbone";
                case KnownSystem.NECPCEngineTurboGrafxCD:
                    return "pcecd";
                case KnownSystem.NECPCFX:
                    return "pcfx";
                case KnownSystem.NintendoGameCube:
                    return "gc";
                case KnownSystem.NintendoSonySuperNESCDROMSystem:
                    return "snescd";
                case KnownSystem.NintendoWii:
                    return "wii";
                case KnownSystem.NintendoWiiU:
                    return "wiiu";
                case KnownSystem.Panasonic3DOInteractiveMultiplayer:
                    return "3do";
                case KnownSystem.PhilipsCDi:
                    return "cdi";
                case KnownSystem.PioneerLaserActive:
                    return "laseractive";
                case KnownSystem.SegaCDMegaCD:
                    return "mcd";
                case KnownSystem.SegaDreamcast:
                    return "dc";
                case KnownSystem.SegaSaturn:
                    return "saturn";
                case KnownSystem.SNKNeoGeoCD:
                    return "ngcd";
                case KnownSystem.SonyPlayStation:
                    return "psx";
                case KnownSystem.SonyPlayStation2:
                    return "ps2";
                case KnownSystem.SonyPlayStation3:
                    return "ps3";
                case KnownSystem.SonyPlayStation4:
                    return "ps4";
                case KnownSystem.SonyPlayStationPortable:
                    return "psp";
                case KnownSystem.TandyMemorexVisualInformationSystem:
                    return "vis";
                case KnownSystem.VMLabsNuon:
                    return "nuon";
                case KnownSystem.VTechVFlashVSmilePro:
                    return "vflash";
                case KnownSystem.ZAPiTGamesGameWaveFamilyEntertainmentSystem:
                    return "gamewave";

                #endregion

                #region Computers

                case KnownSystem.AcornArchimedes:
                    return "archimedes";
                case KnownSystem.AppleMacintosh:
                    return "mac";
                case KnownSystem.CommodoreAmiga:
                    return "amiga";
                case KnownSystem.FujitsuFMTowns:
                    return "fmtowns";
                case KnownSystem.IBMPCCompatible:
                    return "ibm";
                case KnownSystem.NECPC88:
                    return "pc88";
                case KnownSystem.NECPC98:
                    return "pc98";
                case KnownSystem.SharpX68000:
                    return "x68k";

                #endregion

                #region Arcade

                case KnownSystem.AmigaCUBOCD32:
                    return "cubo";
                case KnownSystem.AmericanLaserGames3DO:
                    return "alg 3do";
                case KnownSystem.Atari3DO:
                    return "atari 3do";
                case KnownSystem.Atronic:
                    return "atronic";
                case KnownSystem.AUSCOMSystem1:
                    return "auscom";
                case KnownSystem.BallyGameMagic:
                    return "game magic";
                case KnownSystem.CapcomCPSystemIII:
                    return "cps3";
                case KnownSystem.GlobalVRVarious:
                    return "globalvr";
                case KnownSystem.GlobalVRVortek:
                    return "vortek";
                case KnownSystem.GlobalVRVortekV3:
                    return "vortek v3";
                case KnownSystem.ICEPCHardware:
                    return "ice";
                case KnownSystem.IncredibleTechnologiesEagle:
                    return "eagle";
                case KnownSystem.IncredibleTechnologiesVarious:
                    return "itpc";
                case KnownSystem.KonamieAmusement:
                    return "e-amusement";
                case KnownSystem.KonamiFirebeat:
                    return "firebeat";
                case KnownSystem.KonamiGVSystem:
                    return "gv system";
                case KnownSystem.KonamiM2:
                    return "konami m2";
                case KnownSystem.KonamiPython:
                    return "python";
                case KnownSystem.KonamiPython2:
                    return "python 2";
                case KnownSystem.KonamiSystem573:
                    return "system 573";
                case KnownSystem.KonamiTwinkle:
                    return "twinkle";
                case KnownSystem.KonamiVarious:
                    return "konami pc";
                case KnownSystem.MeritIndustriesBoardwalk:
                    return "boardwalk";
                case KnownSystem.MeritIndustriesMegaTouchForce:
                    return "megatouch force";
                case KnownSystem.MeritIndustriesMegaTouchION:
                    return "megatouch ion";
                case KnownSystem.MeritIndustriesMegaTouchMaxx:
                    return "megatouch maxx";
                case KnownSystem.MeritIndustriesMegaTouchXL:
                    return "megatouch xl";
                case KnownSystem.NamcoCapcomSystem256:
                    return "system 256";
                case KnownSystem.NamcoCapcomTaitoSystem246:
                    return "system 246";
                case KnownSystem.NamcoSegaNintendoTriforce:
                    return "triforce";
                case KnownSystem.NamcoSystem12:
                    return "system 12";
                case KnownSystem.NamcoSystem357:
                    return "system 357";
                case KnownSystem.NewJatreCDi:
                    return "new jatre cdi";
                case KnownSystem.NichibutsuHighRateSystem:
                    return "nichibutsu hrs";
                case KnownSystem.NichibutsuSuperCD:
                    return "nichibutsu scd";
                case KnownSystem.NichibutsuXRateSystem:
                    return "nichibutsu xrs";
                case KnownSystem.PanasonicM2:
                    return "panasonic m2";
                case KnownSystem.PhotoPlayVarious:
                    return "photoplay";
                case KnownSystem.RawThrillsVarious:
                    return "raw thrills";
                case KnownSystem.SegaChihiro:
                    return "chihiro";
                case KnownSystem.SegaEuropaR:
                    return "europar";
                case KnownSystem.SegaLindbergh:
                    return "lindbergh";
                case KnownSystem.SegaNaomi:
                    return "naomi";
                case KnownSystem.SegaNaomi2:
                    return "naomi 2";
                case KnownSystem.SegaNu:
                    return "nu";
                case KnownSystem.SegaRingEdge:
                    return "ringedge";
                case KnownSystem.SegaRingEdge2:
                    return "ringedge 2";
                case KnownSystem.SegaRingWide:
                    return "ringwide";
                case KnownSystem.SegaTitanVideo:
                    return "stv";
                case KnownSystem.SegaSystem32:
                    return "system 32";
                case KnownSystem.SeibuCATSSystem:
                    return "seibu cats";
                case KnownSystem.TABAustriaQuizard:
                    return "quizard";
                case KnownSystem.TsunamiTsuMoMultiGameMotionSystem:
                    return "tsumo";

                #endregion

                #region Others

                case KnownSystem.AudioCD:
                    return "audio";
                case KnownSystem.BDVideo:
                    return "bd-video";
                case KnownSystem.DVDVideo:
                    return "dvd-video";
                case KnownSystem.EnhancedCD:
                    return "enhanced cd";
                case KnownSystem.HDDVDVideo:
                    return "hddvd-video";
                case KnownSystem.NavisoftNaviken21:
                    return "naviken";
                case KnownSystem.PalmOS:
                    return "palmos";
                case KnownSystem.PhilipsCDiDigitalVideo:
                    return "cdi digital video";
                case KnownSystem.PhotoCD:
                    return "photo cd";
                case KnownSystem.PlayStationGameSharkUpdates:
                    return "gameshark";
                case KnownSystem.RainbowDisc:
                    return "rainbow";
                case KnownSystem.TaoiKTV:
                    return "iktv";
                case KnownSystem.TomyKissSite:
                    return "kiss-site";
                case KnownSystem.VideoCD:
                    return "vcd";

                #endregion

                case KnownSystem.NONE:
                default:
                    return "unknown";
            }
        }

        /// <summary>
        /// Get the short string representation of the Language enum values
        /// </summary>
        /// <param name="lang">Language value to convert</param>
        /// <returns>Short string representing the value, if possible</returns>
        public static string ShortName(this Language? lang)
        {
            switch (lang)
            {
                case Language.Afrikaans:
                    return "afr";
                case Language.Arabic:
                    return "ara";
                case Language.Basque:
                    return "baq";
                case Language.Bulgarian:
                    return "bul";
                case Language.Catalan:
                    return "cat";
                case Language.Chinese:
                    return "chi";
                case Language.Croatian:
                    return "hrv";
                case Language.Czech:
                    return "cze";
                case Language.Danish:
                    return "dan";
                case Language.Dutch:
                    return "dut";
                case Language.English:
                    return "eng";
                case Language.Finnish:
                    return "fin";
                case Language.French:
                    return "fre";
                case Language.Gaelic:
                    return "gla";
                case Language.German:
                    return "ger";
                case Language.Greek:
                    return "gre";
                case Language.Hebrew:
                    return "heb";
                case Language.Hindi:
                    return "hin";
                case Language.Hungarian:
                    return "hun";
                case Language.Italian:
                    return "ita";
                case Language.Japanese:
                    return "jap";
                case Language.Korean:
                    return "kor";
                case Language.Norwegian:
                    return "nor";
                case Language.Polish:
                    return "pol";
                case Language.Portuguese:
                    return "por";
                case Language.Punjabi:
                    return "pan";
                case Language.Romanian:
                    return "ron";
                case Language.Russian:
                    return "rus";
                case Language.Slovak:
                    return "slk";
                case Language.Slovenian:
                    return "slv";
                case Language.Spanish:
                    return "spa";
                case Language.Swedish:
                    return "swe";
                case Language.Tamil:
                    return "tam";
                case Language.Thai:
                    return "tha";
                case Language.Turkish:
                    return "tur";
                case Language.Ukrainian:
                    return "ukr";
                default:
                    return "unk";
            }
        }

        /// <summary>
        /// Get the short string representation of the MediaType enum values
        /// </summary>
        /// <param name="type">MediaType value to convert</param>
        /// <returns>Short string representing the value, if possible</returns>
        public static string ShortName(this MediaType? type)
        {
            switch (type)
            {
                #region Punched Media

                case MediaType.ApertureCard:
                    return "aperture";
                case MediaType.JacquardLoomCard:
                    return "jacquard loom card";
                case MediaType.MagneticStripeCard:
                    return "magnetic stripe";
                case MediaType.OpticalPhonecard:
                    return "optical phonecard";
                case MediaType.PunchedCard:
                    return "punchcard";
                case MediaType.PunchedTape:
                    return "punchtape";

                #endregion

                #region Tape

                case MediaType.OpenReel:
                    return "open reel";
                case MediaType.DataCartridge:
                    return "data cartridge";
                case MediaType.Cassette:
                    return "cassette";

                #endregion

                #region Disc / Disc

                case MediaType.BluRay:
                    return "bdrom";
                case MediaType.CDROM:
                    return "cdrom";
                case MediaType.DVD:
                    return "dvd";
                case MediaType.FloppyDisk:
                    return "fd";
                case MediaType.Floptical:
                    return "floptical";
                case MediaType.GDROM:
                    return "gdrom";
                case MediaType.HDDVD:
                    return "hddvd";
                case MediaType.HardDisk:
                    return "hdd";
                case MediaType.IomegaBernoulliDisk:
                    return "bernoulli";
                case MediaType.IomegaJaz:
                    return "jaz";
                case MediaType.IomegaZip:
                    return "zip";
                case MediaType.LaserDisc:
                    return "ldrom";
                case MediaType.Nintendo64DD:
                    return "64dd";
                case MediaType.NintendoFamicomDiskSystem:
                    return "fds";
                case MediaType.NintendoGameCubeGameDisc:
                    return "gc";
                case MediaType.NintendoWiiOpticalDisc:
                    return "wii";
                case MediaType.NintendoWiiUOpticalDisc:
                    return "wiiu";
                case MediaType.UMD:
                    return "umd";

                #endregion

                // Unsorted Formats
                case MediaType.Cartridge:
                    return "cart";
                case MediaType.CED:
                    return "ced";

                case MediaType.NONE:
                default:
                    return "unknown";
            }
        }

        /// <summary>
        /// Get the short string representation of the Region enum values
        /// </summary>
        /// <param name="region">Region value to convert</param>
        /// <returns>Short string representing the value, if possible</returns>
        public static string ShortName(this Region? region)
        {
            switch (region)
            {
                case Region.Argentina:
                    return "Ar";
                case Region.Asia:
                    return "A";
                case Region.AsiaEurope:
                    return "A,E";
                case Region.AsiaUSA:
                    return "A,U";
                case Region.Australia:
                    return "Au";
                case Region.Austria:
                    return "At";
                case Region.AustriaSwitzerland:
                    return "At,Ch";
                case Region.Belgium:
                    return "Be";
                case Region.BelgiumNetherlands:
                    return "Be,N";
                case Region.Brazil:
                    return "B";
                case Region.Canada:
                    return "Ca";
                case Region.China:
                    return "C";
                case Region.Croatia:
                    return "Hr";
                case Region.Czech:
                    return "Cz";
                case Region.Denmark:
                    return "Dk";
                case Region.Europe:
                    return "E";
                case Region.EuropeAsia:
                    return "E,A";
                case Region.EuropeAustralia:
                    return "E,Au";
                case Region.Finland:
                    return "Fi";
                case Region.France:
                    return "F";
                case Region.FranceSpain:
                    return "F,S";
                case Region.Germany:
                    return "G";
                case Region.Greece:
                    return "Gr";
                case Region.Hungary:
                    return "H";
                case Region.India:
                    return "In";
                case Region.Ireland:
                    return "Ie";
                case Region.Israel:
                    return "Il";
                case Region.Italy:
                    return "I";
                case Region.Japan:
                    return "J";
                case Region.JapanAsia:
                    return "J,A";
                case Region.JapanEurope:
                    return "J,E";
                case Region.JapanKorea:
                    return "J,K";
                case Region.JapanUSA:
                    return "J,U";
                case Region.Korea:
                    return "K";
                case Region.LatinAmerica:
                    return "LAm";
                case Region.Netherlands:
                    return "N";
                case Region.Norway:
                    return "No";
                case Region.Poland:
                    return "P";
                case Region.Portugal:
                    return "Pt";
                case Region.Russia:
                    return "R";
                case Region.Scandinavia:
                    return "Sca";
                case Region.Singapore:
                    return "Sg";
                case Region.Slovakia:
                    return "Sk";
                case Region.SouthAfrica:
                    return "Za";
                case Region.Spain:
                    return "S";
                case Region.Sweden:
                    return "Sw";
                case Region.Switzerland:
                    return "Ch";
                case Region.Taiwan:
                    return "Tw";
                case Region.Thailand:
                    return "Th";
                case Region.Turkey:
                    return "Tr";
                case Region.UnitedArabEmirates:
                    return "Ae";
                case Region.UK:
                    return "Uk";
                case Region.Ukraine:
                    return "Ue";
                case Region.USA:
                    return "U";
                case Region.USAAsia:
                    return "U,A";
                case Region.USABrazil:
                    return "U,B";
                case Region.USAEurope:
                    return "U,E";
                case Region.USAJapan:
                    return "U,J";
                case Region.World:
                    return "W";
                default:
                    return null;
            }
        }

        #endregion

        #region Convert From String

        /// <summary>
        /// Get the Category enum value for a given string
        /// </summary>
        /// <param name="sys">String value to convert</param>
        /// <returns>Category represented by the string, if possible</returns>
        public static Category StringToCategory(string category)
        {
            switch (category.ToLowerInvariant())
            {
                case "games":
                    return Category.Games;
                case "demos":
                    return Category.Demos;
                case "video":
                    return Category.Video;
                case "audio":
                    return Category.Audio;
                case "multimedia":
                    return Category.Multimedia;
                case "applications":
                    return Category.Applications;
                case "coverdiscs":
                    return Category.Coverdiscs;
                case "educational":
                    return Category.Educational;
                case "bonusdiscs":
                case "bonus discs":
                    return Category.BonusDiscs;
                case "preproduction":
                    return Category.Preproduction;
                case "addons":
                case "add-ons":
                    return Category.AddOns;
                default:
                    return Category.Games;
            }
        }

        /// <summary>
        /// Get the KnownSystem enum value for a given string
        /// </summary>
        /// <param name="sys">String value to convert</param>
        /// <returns>KnownSystem represented by the string, if possible</returns>
        public static KnownSystem StringToKnownSystem(string sys)
        {
            switch (sys)
            {
                #region Consoles

                case "jaguar":
                case "jagcd":
                case "jaguarcd":
                case "jaguar cd":
                case "atarijaguar":
                case "atarijagcd":
                case "atarijaguarcd":
                case "atari jaguar cd":
                    return KnownSystem.AtariJaguarCD;
                case "playdia":
                case "playdiaqis":
                case "playdiaquickinteractivesystem":
                case "bandaiplaydia":
                case "bandaiplaydiaquickinteractivesystem":
                case "bandai playdia quick interactive system":
                    return KnownSystem.BandaiPlaydiaQuickInteractiveSystem;
                case "pippin":
                case "bandaipippin":
                case "bandai pippin":
                case "applepippin":
                case "apple pippin":
                case "bandaiapplepippin":
                case "bandai apple pippin":
                case "bandai / apple pippin":
                    return KnownSystem.BandaiApplePippin;
                case "cd32":
                case "amigacd32":
                case "amiga cd32":
                case "commodoreamigacd32":
                case "commodore amiga cd32":
                    return KnownSystem.CommodoreAmigaCD32;
                case "cdtv":
                case "amigacdtv":
                case "amiga cdtv":
                case "commodoreamigacdtv":
                case "commodore amiga cdtv":
                    return KnownSystem.CommodoreAmigaCDTV;
                case "evosc":
                case "evo sc":
                case "evosmartconsole":
                case "evo smart console":
                case "envizionsevosc":
                case "envizion evo sc":
                case "envizionevosmartconsole":
                case "envizion evo smart console":
                    return KnownSystem.EnvizionsEVOSmartConsole;
                case "fmtm":
                case "fmtownsmarty":
                case "fm towns marty":
                case "fujitsufmtownsmarty":
                case "fujitsu fm towns marty":
                    return KnownSystem.FujitsuFMTownsMarty;
                case "videonow":
                case "hasbrovideonow":
                case "hasbro videonow":
                    return KnownSystem.HasbroVideoNow;
                case "videonowcolor":
                case "videonow color":
                case "hasbrovideonowcolor":
                case "hasbro videonow color":
                    return KnownSystem.HasbroVideoNowColor;
                case "videonowjr":
                case "videonow jr":
                case "hasbrovideonowjr":
                case "hasbro videonow jr":
                    return KnownSystem.HasbroVideoNowColor;
                case "videonowxp":
                case "videonow xp":
                case "hasbrovideonowxp":
                case "hasbro videonow xp":
                    return KnownSystem.HasbroVideoNowColor;
                case "hyperscan":
                case "mattelhyperscan":
                case "mattel hyperscan":
                    return KnownSystem.MattelHyperscan;
                case "xbox":
                case "microsoftxbox":
                case "microsoft xbox":
                    return KnownSystem.MicrosoftXBOX;
                case "x360":
                case "xbox360":
                case "microsoftx360":
                case "microsoftxbox360":
                case "microsoft x360":
                case "microsoft xbox 360":
                    return KnownSystem.MicrosoftXBOX360;
                case "xb1":
                case "xbone":
                case "xboxone":
                case "microsoftxbone":
                case "microsoftxboxone":
                case "microsoft xbone":
                case "microsoft xbox one":
                    return KnownSystem.MicrosoftXBOXOne;
                case "pcecd":
                case "pce-cd":
                case "tgcd":
                case "tg-cd":
                case "necpcecd":
                case "nectgcd":
                case "nec pc-engine cd":
                case "nec turbografx cd":
                case "nec pc-engine / turbografx cd":
                    return KnownSystem.NECPCEngineTurboGrafxCD;
                case "pcfx":
                case "pc-fx":
                case "pcfxga":
                case "pc-fxga":
                case "necpcfx":
                case "necpcfxga":
                case "nec pc-fx":
                case "nec pc-fxga":
                case "nec pc-fx / pc-fxga":
                    return KnownSystem.NECPCFX;
                case "gc":
                case "gamecube":
                case "ngc":
                case "nintendogamecube":
                case "nintendo gamecube":
                    return KnownSystem.NintendoGameCube;
                case "snescd":
                case "snes cd":
                case "snes-cd":
                case "supernescd":
                case "super nes cd":
                case "super nes-cd":
                case "supernintendocd":
                case "super nintendo cd":
                case "super nintendo-cd":
                case "nintendosnescd":
                case "nintendo snes cd":
                case "nintendosnes-cd":
                case "nintendosupernescd":
                case "nintendo super nes cd":
                case "nintendo super nes-cd":
                case "nintendosupernintendocd":
                case "nintendo super nintendo cd":
                case "nintendo super nintendo-cd":
                case "sonysnescd":
                case "sony snes cd":
                case "sonysnes-cd":
                case "sonysupernescd":
                case "sony super nes cd":
                case "sony super nes-cd":
                case "sonysupernintendocd":
                case "sony super nintendo cd":
                case "sony super nintendo-cd":
                    return KnownSystem.NintendoSonySuperNESCDROMSystem;
                case "wii":
                case "nintendowii":
                case "nintendo wii":
                    return KnownSystem.NintendoWii;
                case "wiiu":
                case "wii u":
                case "nintendowiiu":
                case "nintendo wii u":
                    return KnownSystem.NintendoWiiU;
                case "3do":
                case "3do interactive multiplayer":
                case "panasonic3do":
                case "panasonic 3do":
                case "panasonic 3do interactive multiplayer":
                    return KnownSystem.Panasonic3DOInteractiveMultiplayer;
                case "cdi":
                case "cd-i":
                case "philipscdi":
                case "philips cdi":
                case "philips cd-i":
                    return KnownSystem.PhilipsCDi;
                case "laseractive":
                case "pioneerlaseractive":
                case "pioneer laseractive":
                    return KnownSystem.PioneerLaserActive;
                case "scd":
                case "mcd":
                case "smcd":
                case "segacd":
                case "megacd":
                case "segamegacd":
                case "sega cd":
                case "mega cd":
                case "sega cd / mega cd":
                    return KnownSystem.SegaCDMegaCD;
                case "dc":
                case "sdc":
                case "dreamcast":
                case "segadreamcast":
                case "sega dreamcast":
                    return KnownSystem.SegaDreamcast;
                case "saturn":
                case "segasaturn":
                case "sega saturn":
                    return KnownSystem.SegaSaturn;
                case "ngcd":
                case "neogeocd":
                case "neogeo cd":
                case "neo geo cd":
                case "snk ngcd":
                case "snk neogeo cd":
                case "snk neo geo cd":
                    return KnownSystem.SNKNeoGeoCD;
                case "ps1":
                case "psx":
                case "playstation":
                case "sonyps1":
                case "sony ps1":
                case "sonypsx":
                case "sony psx":
                case "sonyplaystation":
                case "sony playstation":
                    return KnownSystem.SonyPlayStation;
                case "ps2":
                case "playstation2":
                case "playstation 2":
                case "sonyps2":
                case "sony ps2":
                case "sonyplaystation2":
                case "sony playstation 2":
                    return KnownSystem.SonyPlayStation2;
                case "ps3":
                case "playstation3":
                case "playstation 3":
                case "sonyps3":
                case "sony ps3":
                case "sonyplaystation3":
                case "sony playstation 3":
                    return KnownSystem.SonyPlayStation3;
                case "ps4":
                case "playstation4":
                case "playstation 4":
                case "sonyps4":
                case "sony ps4":
                case "sonyplaystation4":
                case "sony playstation 4":
                    return KnownSystem.SonyPlayStation4;
                case "psp":
                case "playstationportable":
                case "playstation portable":
                case "sonypsp":
                case "sony psp":
                case "sonyplaystationportable":
                case "sony playstation portable":
                    return KnownSystem.SonyPlayStationPortable;
                case "vis":
                case "tandyvis":
                case "tandy vis":
                case "tandyvisualinformationsystem":
                case "tandy visual information system":
                case "memorexvis":
                case "memorex vis":
                case "memorexvisualinformationsystem":
                case "memorex visual information sytem":
                case "tandy / memorex visual information system":
                    return KnownSystem.TandyMemorexVisualInformationSystem;
                case "nuon":
                case "vmlabsnuon":
                case "vm labs nuon":
                    return KnownSystem.VMLabsNuon;
                case "vflash":
                case "vsmile":
                case "vsmilepro":
                case "vsmile pro":
                case "v.flash":
                case "v.smile":
                case "v.smilepro":
                case "v.smile pro":
                case "vtechvflash":
                case "vtech vflash":
                case "vtech v.flash":
                case "vtechvsmile":
                case "vtech vsmile":
                case "vtech v.smile":
                case "vtechvsmilepro":
                case "vtech vsmile pro":
                case "vtech v.smile pro":
                case "vtech v.flash - v.smile pro":
                    return KnownSystem.VTechVFlashVSmilePro;
                case "gamewave":
                case "game wave":
                case "zapit":
                case "zapitgamewave":
                case "zapit game wave":
                case "zapit games game wave family entertainment system":
                    return KnownSystem.ZAPiTGamesGameWaveFamilyEntertainmentSystem;

                #endregion

                #region Computers

                case "acorn":
                case "archimedes":
                case "acornarchimedes":
                case "acorn archimedes":
                    return KnownSystem.AcornArchimedes;
                case "apple":
                case "mac":
                case "applemac":
                case "macintosh":
                case "applemacintosh":
                case "apple mac":
                case "apple macintosh":
                    return KnownSystem.AppleMacintosh;
                case "amiga":
                case "commodoreamiga":
                case "commodore amiga":
                    return KnownSystem.CommodoreAmiga;
                case "fmtowns":
                case "fmt":
                case "fm towns":
                case "fujitsufmtowns":
                case "fujitsu fm towns":
                case "fujitsu fm towns series":
                    return KnownSystem.FujitsuFMTowns;
                case "ibm":
                case "ibmpc":
                case "pc":
                case "ibm pc":
                case "ibm pc compatible":
                    return KnownSystem.IBMPCCompatible;
                case "pc88":
                case "pc-88":
                case "necpc88":
                case "nec pc88":
                case "nec pc-88":
                    return KnownSystem.NECPC88;
                case "pc98":
                case "pc-98":
                case "necpc98":
                case "nec pc98":
                case "nec pc-98":
                    return KnownSystem.NECPC98;
                case "x68k":
                case "x68000":
                case "sharpx68k":
                case "sharp x68k":
                case "sharpx68000":
                case "sharp x68000":
                    return KnownSystem.SharpX68000;

                #endregion

                #region Arcade

                case "cubo":
                case "cubocd32":
                case "cubo cd32":
                case "amigacubocd32":
                case "amiga cubo cd32":
                    return KnownSystem.AmigaCUBOCD32;
                case "alg3do":
                case "alg 3do":
                case "americanlasergames3do":
                case "american laser games 3do":
                    return KnownSystem.AmericanLaserGames3DO;
                case "atari3do":
                case "atari 3do":
                    return KnownSystem.Atari3DO;
                case "atronic":
                    return KnownSystem.Atronic;
                case "auscom":
                case "auscomsystem1":
                case "auscom system 1":
                    return KnownSystem.AUSCOMSystem1;
                case "gamemagic":
                case "game magic":
                case "ballygamemagic":
                case "bally game magic":
                    return KnownSystem.BallyGameMagic;
                case "cps3":
                case "cpsiii":
                case "cps 3":
                case "cp system 3":
                case "cp system iii":
                case "capcomcps3":
                case "capcomcpsiii":
                case "capcom cps 3":
                case "capcom cps iii":
                case "capcom cp system 3":
                case "capcom cp system iii":
                    return KnownSystem.CapcomCPSystemIII;
                case "globalvr":
                case "global vr":
                case "global vr pc-based systems":
                    return KnownSystem.GlobalVRVarious;
                case "vortek":
                case "globalvrvortek":
                case "global vr vortek":
                    return KnownSystem.GlobalVRVortek;
                case "vortekv3":
                case "vortek v3":
                case "globalvrvortekv3":
                case "global vr vortek v3":
                    return KnownSystem.GlobalVRVortekV3;
                case "ice":
                case "icepc":
                case "ice pc":
                case "ice pc-based hardware":
                    return KnownSystem.ICEPCHardware;
                case "iteagle":
                case "eagle":
                case "incredible technologies eagle":
                    return KnownSystem.IncredibleTechnologiesEagle;
                case "itpc":
                case "incredible technologies pc-based systems":
                    return KnownSystem.IncredibleTechnologiesVarious;
                case "eamusement":
                case "e-amusement":
                case "konamieamusement":
                case "konami eamusement":
                case "konamie-amusement":
                case "konami e-amusement":
                    return KnownSystem.KonamieAmusement;
                case "firebeat":
                case "konamifirebeat":
                case "konami firebeat":
                    return KnownSystem.KonamiFirebeat;
                case "gvsystem":
                case "gv system":
                case "konamigvsystem":
                case "konami gv system":
                    return KnownSystem.KonamiGVSystem;
                case "konamim2":
                case "konami m2":
                    return KnownSystem.KonamiM2;
                case "python":
                case "konamipython":
                case "konami python":
                    return KnownSystem.KonamiPython;
                case "python2":
                case "python 2":
                case "konamipython2":
                case "konami python 2":
                    return KnownSystem.KonamiPython2;
                case "system573":
                case "system 573":
                case "konamisystem573":
                case "konami system 573":
                    return KnownSystem.KonamiSystem573;
                case "twinkle":
                case "konamitwinkle":
                case "konami twinkle":
                    return KnownSystem.KonamiTwinkle;
                case "konamipc":
                case "konami pc":
                case "konami pc-based systems":
                    return KnownSystem.KonamiVarious;
                case "boardwalk":
                case "meritindustriesboardwalk":
                case "merit industries boardwalk":
                    return KnownSystem.MeritIndustriesBoardwalk;
                case "megatouchforce":
                case "megatouch force":
                case "meritindustriesmegatouchforce":
                case "merit industries megatouch force":
                    return KnownSystem.MeritIndustriesMegaTouchForce;
                case "megatouchion":
                case "megatouch ion":
                case "meritindustriesmegatouchion":
                case "merit industries megatouch ion":
                    return KnownSystem.MeritIndustriesMegaTouchION;
                case "megatouchmaxx":
                case "megatouch maxx":
                case "meritindustriesmegatouchmaxx":
                case "merit industries megatouch maxx":
                    return KnownSystem.MeritIndustriesMegaTouchMaxx;
                case "megatouchxl":
                case "megatouch xl":
                case "meritindustriesmegatouchxl":
                case "merit industries megatouch xl":
                    return KnownSystem.MeritIndustriesMegaTouchXL;
                case "system256":
                case "system 256":
                case "supersystem256":
                case "super system 256":
                case "namcosystem256":
                case "namco system 256":
                case "namcosupersystem256":
                case "namco super system 256":
                case "capcomsystem256":
                case "capcom system 256":
                case "capcomsupersystem256":
                case "capcom super system 256":
                case "namco / capcom system 256/super system 256":
                    return KnownSystem.NamcoCapcomSystem256;
                case "system246":
                case "system 246":
                case "namcosystem246":
                case "namco system 246":
                case "capcomsystem246":
                case "capcom system 246":
                case "taitosystem246":
                case "taito system 246":
                case "namco / capcom / taito system 246":
                    return KnownSystem.NamcoCapcomTaitoSystem246;
                case "triforce":
                case "namcotriforce":
                case "namco triforce":
                case "segatriforce":
                case "sega triforce":
                case "nintendotriforce":
                case "nintendo triforce":
                case "namco / sega / nintendo triforce":
                    return KnownSystem.NamcoSegaNintendoTriforce;
                case "system12":
                case "system 12":
                case "namcosystem12":
                case "namco system 12":
                    return KnownSystem.NamcoSystem12;
                case "system357":
                case "system 357":
                case "namcosystem357":
                case "namco system 357":
                    return KnownSystem.NamcoSystem357;
                case "newjatrecdi":
                case "new jatre cdi":
                case "new jatre cd-i":
                    return KnownSystem.NewJatreCDi;
                case "hrs":
                case "highratesytem":
                case "high rate system":
                case "nichibutsuhrs":
                case "nichibutsu hrs":
                case "nichibutsu high rate system":
                    return KnownSystem.NichibutsuHighRateSystem;
                case "supercd":
                case "super cd":
                case "nichibutsuscd":
                case "nichibutsu scd":
                case "nichibutsusupercd":
                case "nichibutsu supercd":
                case "nichibutsu super cd":
                    return KnownSystem.NichibutsuSuperCD;
                case "xrs":
                case "xratesystem":
                case "x-rate system":
                case "nichibutsuxrs":
                case "nichibutsu xrs":
                case "nichibutsu x-rate system":
                    return KnownSystem.NichibutsuXRateSystem;
                case "panasonicm2":
                case "panasonic m2":
                    return KnownSystem.PanasonicM2;
                case "photoplay":
                case "photoplaypc":
                case "photoplay pc":
                case "photoplay pc-based systems":
                    return KnownSystem.PhotoPlayVarious;
                case "rawthrills":
                case "raw thrills":
                case "raw thrills pc-based systems":
                    return KnownSystem.RawThrillsVarious;
                case "chihiro":
                case "segachihiro":
                case "sega chihiro":
                    return KnownSystem.SegaChihiro;
                case "europar":
                case "europa-r":
                case "segaeuropar":
                case "sega europar":
                case "sega europa-r":
                    return KnownSystem.SegaEuropaR;
                case "lindbergh":
                case "segalindbergh":
                case "sega lindbergh":
                    return KnownSystem.SegaLindbergh;
                case "naomi":
                case "seganaomi":
                case "sega naomi":
                    return KnownSystem.SegaNaomi;
                case "naomi2":
                case "naomi 2":
                case "seganaomi2":
                case "sega naomi 2":
                    return KnownSystem.SegaNaomi2;
                case "nu":
                case "seganu":
                case "sega nu":
                    return KnownSystem.SegaNu;
                case "ringedge":
                case "segaringedge":
                case "sega ringedge":
                    return KnownSystem.SegaRingEdge;
                case "ringedge2":
                case "ringedge 2":
                case "segaringedge2":
                case "sega ringedge 2":
                    return KnownSystem.SegaRingEdge2;
                case "ringwide":
                case "segaringwide":
                case "sega ringwide":
                    return KnownSystem.SegaRingWide;
                case "stv":
                case "titanvideo":
                case "titan video":
                case "segatitanvideo":
                case "sega titan video":
                    return KnownSystem.SegaTitanVideo;
                case "system32":
                case "system 32":
                case "segasystem32":
                case "sega system 32":
                    return KnownSystem.SegaSystem32;
                case "cats":
                case "seibucats":
                case "seibu cats":
                case "seibu cats system":
                    return KnownSystem.SeibuCATSSystem;
                case "quizard":
                case "tabaustriaquizard":
                case "tab-austria quizard":
                    return KnownSystem.TABAustriaQuizard;
                case "tsumo":
                case "tsunamitsumo":
                case "tsunami tsumo":
                case "tsunami tsumo multi-game motion system":
                    return KnownSystem.TsunamiTsuMoMultiGameMotionSystem;

                #endregion

                #region Others

                case "audio":
                case "audiocd":
                case "audio cd":
                    return KnownSystem.AudioCD;
                case "bdvideo":
                case "bd-video":
                case "blurayvideo":
                case "bluray video":
                    return KnownSystem.BDVideo;
                case "dvdvideo":
                case "dvd-video":
                    return KnownSystem.DVDVideo;
                case "enhancedcd":
                case "enhanced cd":
                case "enhancedcdrom":
                case "enhanced cdrom":
                case "enhanced cd-rom":
                    return KnownSystem.EnhancedCD;
                case "hddvdvideo":
                case "hddvd-video":
                case "hd-dvd-video":
                    return KnownSystem.HDDVDVideo;
                case "naviken":
                case "naviken21":
                case "naviken 2.1":
                case "navisoftnaviken":
                case "navisoft naviken":
                case "navisoftnaviken21":
                case "navisoft naviken 2.1":
                    return KnownSystem.NavisoftNaviken21;
                case "palm":
                case "palmos":
                    return KnownSystem.PalmOS;
                case "cdidv":
                case "cdidigitalvideo":
                case "cdi digital video":
                case "cd-i digital video":
                case "philipscdidigitalvideo":
                case "philips cdi digital video":
                case "philips cd-i digital video":
                    return KnownSystem.PhilipsCDiDigitalVideo;
                case "photo":
                case "photocd":
                case "photo cd":
                    return KnownSystem.PhotoCD;
                case "gameshark":
                case "psgameshark":
                case "ps gameshark":
                case "playstationgameshark":
                case "playstation gameshark":
                case "playstation gameshark updates":
                    return KnownSystem.PlayStationGameSharkUpdates;
                case "rainbow":
                case "rainbowdisc":
                case "rainbow disc":
                    return KnownSystem.RainbowDisc;
                case "iktv":
                case "taoiktv":
                case "tao iktv":
                    return KnownSystem.TaoiKTV;
                case "kisssite":
                case "kiss-site":
                case "tomykisssite":
                case "tomy kisssite":
                case "tomy kiss-site":
                    return KnownSystem.TomyKissSite;
                case "vcd":
                case "videocd":
                case "video cd":
                    return KnownSystem.VideoCD;

                #endregion

                default:
                    return KnownSystem.NONE;
            }
        }

        /// <summary>
        /// Get the Language enum value for a given string
        /// </summary>
        /// <param name="sys">String value to convert</param>
        /// <returns>Language represented by the string, if possible</returns>
        public static Language? StringToLanguage(string lang)
        {
            switch (lang)
            {
                case "afr":
                    return Language.Afrikaans;
                case "ara":
                    return Language.Arabic;
                case "baq":
                    return Language.Basque;
                case "bul":
                    return Language.Bulgarian;
                case "cat":
                    return Language.Catalan;
                case "chi":
                    return Language.Chinese;
                case "hrv":
                    return Language.Croatian;
                case "cze":
                    return Language.Czech;
                case "dan":
                    return Language.Danish;
                case "dut":
                    return Language.Dutch;
                case "eng":
                    return Language.English;
                case "fin":
                    return Language.Finnish;
                case "fre":
                    return Language.French;
                case "gla":
                    return Language.Gaelic;
                case "ger":
                    return Language.German;
                case "gre":
                    return Language.Greek;
                case "heb":
                    return Language.Hebrew;
                case "hin":
                    return Language.Hindi;
                case "hun":
                    return Language.Hungarian;
                case "ita":
                    return Language.Italian;
                case "jap":
                    return Language.Japanese;
                case "kor":
                    return Language.Korean;
                case "nor":
                    return Language.Norwegian;
                case "pol":
                    return Language.Polish;
                case "por":
                    return Language.Portuguese;
                case "pan":
                    return Language.Punjabi;
                case "ron":
                    return Language.Romanian;
                case "rus":
                    return Language.Russian;
                case "slk":
                    return Language.Slovak;
                case "slv":
                    return Language.Slovenian;
                case "spa":
                    return Language.Spanish;
                case "swe":
                    return Language.Swedish;
                case "tam":
                    return Language.Tamil;
                case "tha":
                    return Language.Thai;
                case "tur":
                    return Language.Turkish;
                case "ukr":
                    return Language.Ukrainian;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Get the MediaType enum value for a given string
        /// </summary>
        /// <param name="type">String value to convert</param>
        /// <returns>MediaType represented by the string, if possible</returns>
        public static MediaType StringToMediaType(string type)
            {
                switch (type.ToLowerInvariant())
                {
                    #region Punched Media

                    case "aperture":
                    case "aperturecard":
                    case "aperture card":
                        return MediaType.ApertureCard;
                    case "jacquardloom":
                    case "jacquardloomcard":
                    case "jacquard loom card":
                        return MediaType.JacquardLoomCard;
                    case "magneticstripe":
                    case "magneticstripecard":
                    case "magnetic stripe card":
                        return MediaType.MagneticStripeCard;
                    case "opticalphone":
                    case "opticalphonecard":
                    case "optical phonecard":
                        return MediaType.OpticalPhonecard;
                    case "punchcard":
                    case "punchedcard":
                    case "punched card":
                        return MediaType.PunchedCard;
                    case "punchtape":
                    case "punchedtape":
                    case "punched tape":
                        return MediaType.PunchedTape;

                    #endregion

                    #region Tape

                    case "openreel":
                    case "openreeltape":
                    case "open reel tape":
                        return MediaType.OpenReel;
                    case "datacart":
                    case "datacartridge":
                    case "datatapecartridge":
                    case "data tape cartridge":
                        return MediaType.DataCartridge;
                    case "cassette":
                    case "cassettetape":
                    case "cassette tape":
                        return MediaType.Cassette;

                    #endregion

                    #region Disc / Disc

                    case "bd":
                    case "bdrom":
                    case "bd-rom":
                    case "bluray":
                        return MediaType.BluRay;
                    case "cd":
                    case "cdrom":
                    case "cd-rom":
                        return MediaType.CDROM;
                    case "dvd":
                    case "dvd5":
                    case "dvd-5":
                    case "dvd9":
                    case "dvd-9":
                    case "dvdrom":
                    case "dvd-rom":
                        return MediaType.DVD;
                    case "fd":
                    case "floppy":
                    case "floppydisk":
                    case "floppy disk":
                    case "floppy diskette":
                        return MediaType.FloppyDisk;
                    case "floptical":
                        return MediaType.Floptical;
                    case "gd":
                    case "gdrom":
                    case "gd-rom":
                        return MediaType.GDROM;
                    case "hddvd":
                    case "hd-dvd":
                    case "hddvdrom":
                    case "hd-dvd-rom":
                        return MediaType.HDDVD;
                    case "hdd":
                    case "harddisk":
                    case "hard disk":
                        return MediaType.HardDisk;
                    case "bernoullidisk":
                    case "iomegabernoullidisk":
                    case "bernoulli disk":
                    case "iomega bernoulli disk":
                        return MediaType.IomegaBernoulliDisk;
                    case "jaz":
                    case "iomegajaz":
                    case "iomega jaz":
                        return MediaType.IomegaJaz;
                    case "zip":
                    case "zipdisk":
                    case "iomegazip":
                    case "iomega zip":
                        return MediaType.IomegaZip;
                    case "ldrom":
                    case "lvrom":
                    case "ld-rom":
                    case "lv-rom":
                    case "laserdisc":
                    case "laservision":
                    case "ld-rom / lv-rom":
                        return MediaType.LaserDisc;
                    case "64dd":
                    case "n64dd":
                    case "64dddisk":
                    case "n64dddisk":
                    case "64dd disk":
                    case "n64dd disk":
                        return MediaType.Nintendo64DD;
                    case "fds":
                    case "famicom":
                    case "nfds":
                    case "nintendofamicom":
                    case "famicomdisksystem":
                    case "famicom disk system":
                    case "famicom disk system disk":
                        return MediaType.NintendoFamicomDiskSystem;
                    case "gc":
                    case "gamecube":
                    case "nintendogamecube":
                    case "nintendo gamecube":
                    case "gamecube disc":
                    case "gamecube game disc":
                        return MediaType.NintendoGameCubeGameDisc;
                    case "wii":
                    case "nintendowii":
                    case "nintendo wii":
                    case "nintendo wii disc":
                    case "wii optical disc":
                        return MediaType.NintendoWiiOpticalDisc;
                    case "wiiu":
                    case "nintendowiiu":
                    case "nintendo wiiu":
                    case "nintendo wiiu disc":
                    case "wiiu optical disc":
                    case "wii u optical disc":
                        return MediaType.NintendoWiiUOpticalDisc;
                    case "umd":
                        return MediaType.UMD;

                    #endregion

                    // Unsorted Formats
                    case "cartridge":
                        return MediaType.Cartridge;
                    case "ced":
                    case "rcaced":
                    case "rca ced":
                    case "videodisc":
                    case "rca videodisc":
                        return MediaType.CED;

                    default:
                        return MediaType.NONE;
                }
            }

        /// <summary>
        /// Get the Region enum value for a given string
        /// </summary>
        /// <param name="type">String value to convert</param>
        /// <returns>Region represented by the string, if possible</returns>
        public static Region? StringToRegion(string region)
        {
            switch (region)
            {
                case "Ar":
                    return Region.Argentina;
                case "A":
                    return Region.Asia;
                case "A,E":
                    return Region.AsiaEurope;
                case "A,U":
                    return Region.AsiaUSA;
                case "Au":
                    return Region.Australia;
                case "At":
                    return Region.Austria;
                case "At,Ch":
                    return Region.AustriaSwitzerland;
                case "Be":
                    return Region.Belgium;
                case "Be,N":
                    return Region.BelgiumNetherlands;
                case "B":
                    return Region.Brazil;
                case "Ca":
                    return Region.Canada;
                case "C":
                    return Region.China;
                case "Hr":
                    return Region.Croatia;
                case "Cz":
                    return Region.Czech;
                case "Dk":
                    return Region.Denmark;
                case "E":
                    return Region.Europe;
                case "E,A":
                    return Region.EuropeAsia;
                case "E,Au":
                    return Region.EuropeAustralia;
                case "Fi":
                    return Region.Finland;
                case "F":
                    return Region.France;
                case "F,S":
                    return Region.FranceSpain;
                case "G":
                    return Region.Germany;
                case "Gr":
                    return Region.Greece;
                case "H":
                    return Region.Hungary;
                case "In":
                    return Region.India;
                case "Ie":
                    return Region.Ireland;
                case "Il":
                    return Region.Israel;
                case "I":
                    return Region.Italy;
                case "J":
                    return Region.Japan;
                case "J,A":
                    return Region.JapanAsia;
                case "J,E":
                    return Region.JapanEurope;
                case "J,K":
                    return Region.JapanKorea;
                case "J,U":
                    return Region.JapanUSA;
                case "K":
                    return Region.Korea;
                case "LAm":
                    return Region.LatinAmerica;
                case "N":
                    return Region.Netherlands;
                case "No":
                    return Region.Norway;
                case "P":
                    return Region.Poland;
                case "Pt":
                    return Region.Portugal;
                case "R":
                    return Region.Russia;
                case "Sca":
                    return Region.Scandinavia;
                case "Sg":
                    return Region.Singapore;
                case "Sk":
                    return Region.Slovakia;
                case "Za":
                    return Region.SouthAfrica;
                case "S":
                    return Region.Spain;
                case "Sw":
                    return Region.Sweden;
                case "Ch":
                    return Region.Switzerland;
                case "Tw":
                    return Region.Taiwan;
                case "Th":
                    return Region.Thailand;
                case "Tr":
                    return Region.Turkey;
                case "Ae":
                    return Region.UnitedArabEmirates;
                case "Uk":
                    return Region.UK;
                case "Ue":
                    return Region.Ukraine;
                case "U":
                    return Region.USA;
                case "U,A":
                    return Region.USAAsia;
                case "U,B":
                    return Region.USABrazil;
                case "U,E":
                    return Region.USAEurope;
                case "U,J":
                    return Region.USAJapan;
                case "W":
                    return Region.World;
                default:
                    return null;
            }
        }

        #endregion
    }
}