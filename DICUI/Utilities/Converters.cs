using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using IMAPI2;
using DICUI.Data;

namespace DICUI.Utilities
{
    /// <summary>
    /// Used to provide a converter to XAML files to render comboboxes with enum values
    /// </summary>
    public class EnumDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MediaType?)
                return ((MediaType?)value).Name();
            else if (value is KnownSystem?)
                return ((KnownSystem?)value).Name();
            else
                return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }
    }

    public static class Converters
    {
        /// <summary>
        /// Get the MediaType associated with a given base command
        /// </summary>
        /// <param name="baseCommand">String value to check</param>
        /// <returns>MediaType if possible, null on error</returns>
        /// <remarks>This takes the "safe" route by assuming the larger of any given format</remarks>
        public static MediaType? BaseCommmandToMediaType(string baseCommand)
        {
            switch (baseCommand)
            {
                case DICCommandStrings.Audio:
                case DICCommandStrings.CompactDisc:
                case DICCommandStrings.Data:
                    return MediaType.CD;
                case DICCommandStrings.GDROM:
                case DICCommandStrings.Swap:
                    return MediaType.GDROM;
                case DICCommandStrings.DigitalVideoDisc:
                case DICCommandStrings.XBOX:
                    return MediaType.DVD;
                case DICCommandStrings.BluRay:
                    return MediaType.BluRay;

                // Non-optical
                case DICCommandStrings.Floppy:
                    return MediaType.Floppy;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Get the most common known system for a given MediaType
        /// </summary>
        /// <param name="baseCommand">String value to check</param>
        /// <returns>KnownSystem if possible, null on error</returns>
        public static KnownSystem? BaseCommandToKnownSystem(string baseCommand)
        {
            switch (baseCommand)
            {
                case DICCommandStrings.Audio:
                    return KnownSystem.AudioCD;
                case DICCommandStrings.CompactDisc:
                case DICCommandStrings.Data:
                case DICCommandStrings.DigitalVideoDisc:
                case DICCommandStrings.Floppy:
                    return KnownSystem.IBMPCCompatible;
                case DICCommandStrings.GDROM:
                case DICCommandStrings.Swap:
                    return KnownSystem.SegaDreamcast;
                case DICCommandStrings.BluRay:
                    return KnownSystem.SonyPlayStation3;
                case DICCommandStrings.XBOX:
                    return KnownSystem.MicrosoftXBOX;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Get the string representation of the DICCommand enum values
        /// </summary>
        /// <param name="command">DICCommand value to convert</param>
        /// <returns>String representing the value, if possible</returns>
        public static string DICCommandToString(DICCommand command)
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
        public static string DICFlagToString(DICFlag flag)
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
                case DICFlag.SubchannelReadLevel:
                    return DICFlagStrings.SubchannelReadLevel;

                case DICFlag.NONE:
                default:
                    return "";
            }
        }

        /// <summary>
        /// Convert IMAPI physical media type to a MediaType
        /// </summary>
        /// <param name="type">IMAPI_MEDIA_PHYSICAL_TYPE value to check</param>
        /// <returns>MediaType if possible, null on error</returns>
        public static MediaType? IMAPIDiskTypeToMediaType(IMAPI_MEDIA_PHYSICAL_TYPE type)
		{
			switch (type)
			{
				case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_UNKNOWN:
					return MediaType.NONE;
				case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_CDROM:
				case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_CDR:
				case IMAPI_MEDIA_PHYSICAL_TYPE.IMAPI_MEDIA_TYPE_CDRW:
					return MediaType.CD;
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
        public static string MediaTypeToExtension(MediaType? type)
        {
            switch (type)
            {
                case MediaType.CD:
                case MediaType.GDROM:
                case MediaType.Cartridge:
                    return ".bin";
                case MediaType.DVD:
                case MediaType.HDDVD:
                case MediaType.BluRay:
                case MediaType.WiiOpticalDisc:
                case MediaType.UMD:
                    return ".iso";
                case MediaType.LaserDisc:
                case MediaType.GameCubeGameDisc:
                    return ".raw";
                case MediaType.WiiUOpticalDisc:
                    return ".wud";
                case MediaType.Floppy:
                    return ".img";
                case MediaType.Cassette:
                    return ".wav";
                case MediaType.NONE:
                case MediaType.CED:
                default:
                    return null;
            }
        }

        /// <summary>
        /// Get the string representation of the MediaType enum values
        /// </summary>
        /// <param name="type">MediaType value to convert</param>
        /// <returns>String representing the value, if possible</returns>
        public static string MediaTypeToString(MediaType? type)
        {
            switch (type)
            {
                case MediaType.CD:
                    return "CD-ROM";
                case MediaType.DVD:
                    return "DVD";
                case MediaType.GDROM:
                    return "GD-ROM";
                case MediaType.HDDVD:
                    return "HD-DVD";
                case MediaType.BluRay:
                    return "BluRay";
                case MediaType.LaserDisc:
                    return "LaserDisc";

                case MediaType.CED:
                    return "CED";
                case MediaType.GameCubeGameDisc:
                    return "GameCube Game";
                case MediaType.WiiOpticalDisc:
                    return "Wii Optical";
                case MediaType.WiiUOpticalDisc:
                    return "Wii U Optical";
                case MediaType.UMD:
                    return "UMD";

                case MediaType.Cartridge:
                    return "Cartridge";
                case MediaType.Cassette:
                    return "Cassette Tape";
                case MediaType.Floppy:
                    return "Floppy Disk";

                case MediaType.NONE:
                default:
                    return "Unknown";
            }
        }

        /// <summary>
        /// Get the DIC command to be used for a given MediaType
        /// </summary>
        /// <param name="type">MediaType value to check</param>
        /// <returns>String containing the command, null on error</returns>
        public static string KnownSystemAndMediaTypeToBaseCommand(KnownSystem? sys, MediaType? type)
        {
            // If we have an invalid combination, we should return null
            if (!Validators.GetValidMediaTypes(sys).Contains(type))
            {
                return null;
            }

            switch (type)
            {
                case MediaType.CD:
                    if (sys == KnownSystem.MicrosoftXBOX)
                    {
                        return DICCommandStrings.XBOX;
                    }
                    return DICCommandStrings.CompactDisc;
                case MediaType.DVD:
                    if (sys == KnownSystem.MicrosoftXBOX
                        || sys == KnownSystem.MicrosoftXBOX360XDG2
                        || sys == KnownSystem.MicrosoftXBOX360XDG3)
                    {
                        return DICCommandStrings.XBOX;
                    }
                    return DICCommandStrings.DigitalVideoDisc;
                case MediaType.GDROM:
                    return DICCommandStrings.GDROM;
                case MediaType.HDDVD:
                    return DICCommandStrings.DigitalVideoDisc;
                case MediaType.BluRay:
                    return DICCommandStrings.BluRay;
                case MediaType.GameCubeGameDisc:
                    return DICCommandStrings.DigitalVideoDisc;
                case MediaType.WiiOpticalDisc:
                    return DICCommandStrings.DigitalVideoDisc;
                case MediaType.Floppy:
                    return DICCommandStrings.Floppy;

                default:
                    return null;
            }
        }

        /// <summary>
        /// Get list of default parameters for a given system and disc type
        /// </summary>
        /// <param name="sys">KnownSystem value to check</param>
        /// <param name="type">MediaType value to check</param>
        /// <returns>List of strings representing the parameters</returns>
        public static List<string> KnownSystemAndMediaTypeToParameters(KnownSystem? sys, MediaType? type)
        {
            // First check to see if the combination of system and MediaType is valid
            var validTypes = Validators.GetValidMediaTypes(sys);
            if (!validTypes.Contains(type))
            {
                return null;
            }

            // Now sort based on disc type
            List<string> parameters = new List<string>();
            switch (type)
            {
                case MediaType.CD:
                    parameters.Add(DICFlagStrings.C2Opcode); parameters.Add("20");

                    switch (sys)
                    {
                        case KnownSystem.AppleMacintosh:
                        case KnownSystem.IBMPCCompatible:
                            parameters.Add(DICFlagStrings.NoFixSubQSecuROM);
                            parameters.Add(DICFlagStrings.ScanFileProtect);
                            parameters.Add(DICFlagStrings.ScanSectorProtect);
                            break;
                        case KnownSystem.NECPCEngineTurboGrafxCD:
                            parameters.Add(DICFlagStrings.MCN);
                            break;
                        case KnownSystem.SonyPlayStation:
                            parameters.Add(DICFlagStrings.ScanAntiMod);
                            parameters.Add(DICFlagStrings.NoFixSubQLibCrypt);
                            break;
                    }
                    break;
                case MediaType.DVD:
                    // Currently no defaults set
                    break;
                case MediaType.GDROM:
                    parameters.Add(DICFlagStrings.C2Opcode); parameters.Add("20");
                    break;
                case MediaType.HDDVD:
                    // Currently no defaults set
                    break;
                case MediaType.BluRay:
                    // Currently no defaults set
                    break;

                // Special Formats
                case MediaType.GameCubeGameDisc:
                    parameters.Add(DICFlagStrings.Raw);
                    break;
                case MediaType.WiiOpticalDisc:
                    parameters.Add(DICFlagStrings.Raw);
                    break;

                // Non-optical
                case MediaType.Floppy:
                    // Currently no defaults set
                    break;
            }

            return parameters;
        }

        /// <summary>
        /// Get the string representation of the KnownSystem enum values
        /// </summary>
        /// <param name="sys">KnownSystem value to convert</param>
        /// <returns>String representing the value, if possible</returns>
        public static string KnownSystemToString(KnownSystem? sys)
        {
            switch (sys)
            {
                #region Consoles

                case KnownSystem.BandaiPlaydiaQuickInteractiveSystem:
                    return "Bandai Playdia Quick Interactive System";
                case KnownSystem.BandaiApplePippin:
                    return "Bandai / Apple Pippin";
                case KnownSystem.CommodoreAmigaCD32:
                    return "Commodore Amiga CD32";
                case KnownSystem.CommodoreAmigaCDTV:
                    return "Commodore Amiga CDTV";
                case KnownSystem.MattelHyperscan:
                    return "Mattel HyperScan";
                case KnownSystem.MicrosoftXBOX:
                    return "Microsoft XBOX";
                case KnownSystem.MicrosoftXBOX360XDG2:
                    return "Microsoft XBOX 360 (XDG2)";
                case KnownSystem.MicrosoftXBOX360XDG3:
                    return "Microsoft XBOX 360 (XDG3)";
                case KnownSystem.MicrosoftXBOXOne:
                    return "Microsoft XBOX One";
                case KnownSystem.NECPCEngineTurboGrafxCD:
                    return "NEC PC-Engine / TurboGrafx CD";
                case KnownSystem.NECPCFX:
                    return "NEC PC-FX / PC-FXGA";
                case KnownSystem.NintendoGameCube:
                    return "Nintendo GameCube";
                case KnownSystem.NintendoWii:
                    return "Nintendo Wii";
                case KnownSystem.NintendoWiiU:
                    return "Nintendo Wii U";
                case KnownSystem.Panasonic3DOInteractiveMultiplayer:
                    return "Panasonic 3DO Interactive Multiplayer";
                case KnownSystem.PhilipsCDi:
                    return "Philips CD-i";
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
                case KnownSystem.CommodoreAmigaCD:
                    return "Commodore Amiga CD";
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
                case KnownSystem.MeritIndustriesMegaTouchAurora:
                    return "Merit Industries MegaTouch Aurora";
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
                    return "NichibutsuX-Rate System";
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
                case KnownSystem.SegaSTV:
                    return "Sega STV";
                case KnownSystem.SegaSystem32:
                    return "Sega System 32";
                case KnownSystem.SeibuCATSSystem:
                    return "Seibu CATS System";
                case KnownSystem.TABAustriaQuizard:
                    return "TAB-Austria Quizard";
                case KnownSystem.TandyMemorexVisualInformationSystem:
                    return "Tandy / Memorex Visual Information System";
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
                case KnownSystem.PalmOS:
                    return "PalmOS";
                case KnownSystem.PhilipsCDiDigitalVideo:
                    return "Philips CD-i Digital Video";
                case KnownSystem.PhotoCD:
                    return "Photo CD";
                case KnownSystem.PlayStationGameSharkUpdates:
                    return "PlayStation GameShark Updates";
                case KnownSystem.TaoiKTV:
                    return "Tao iKTV";
                case KnownSystem.TomyKissSite:
                    return "Tomy Kiss-Site";
                case KnownSystem.VideoCD:
                    return "Video CD";

                #endregion

                case KnownSystem.Custom:
                    return "Custom Input";

                case KnownSystem.NONE:
                default:
                    return "Unknown";
            }
        }
    }
}