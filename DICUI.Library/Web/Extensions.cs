namespace DICUI.Web
{
    /// <summary>
    /// Information pertaining to Redump systems
    /// </summary>
    public static class Extensions
    {
        #region Special lists

        /// <summary>
        /// List of systems that are not publically accessible
        /// </summary>
        public static readonly RedumpSystem?[] BannedSystems = new RedumpSystem?[]
        {
            RedumpSystem.AudioCD,
            RedumpSystem.BDVideo,
            RedumpSystem.DVDVideo,
            RedumpSystem.HasbroVideoNow,
            RedumpSystem.HasbroVideoNowColor,
            RedumpSystem.HasbroVideoNowJr,
            RedumpSystem.HasbroVideoNowXP,
            RedumpSystem.KonamiM2,
            RedumpSystem.MicrosoftXbox360,
            RedumpSystem.MicrosoftXboxOne,
            RedumpSystem.NavisoftNaviken21,
            RedumpSystem.NintendoWii,
            RedumpSystem.NintendoWiiU,
            RedumpSystem.PanasonicM2,
            RedumpSystem.PhilipsCDiDigitalVideo,
            RedumpSystem.SegaPrologue21,
            RedumpSystem.SegaRingEdge,
            RedumpSystem.SegaRingEdge2,
            RedumpSystem.SonyPlayStation3,
            RedumpSystem.SonyPlayStation4,
            RedumpSystem.VideoCD,
        };

        /// <summary>
        /// List of systems that have a Cues pack
        /// </summary>
        public static readonly RedumpSystem?[] HasCues = new RedumpSystem?[]
        {
            RedumpSystem.AppleMacintosh,
            RedumpSystem.AtariJaguarCDInteractiveMultimediaSystem,
            RedumpSystem.AudioCD,
            RedumpSystem.BandaiPippin,
            RedumpSystem.BandaiPlaydiaQuickInteractiveSystem,
            RedumpSystem.CommodoreAmigaCD,
            RedumpSystem.CommodoreAmigaCD32,
            RedumpSystem.CommodoreAmigaCDTV,
            RedumpSystem.FujitsuFMTownsseries,
            RedumpSystem.funworldPhotoPlay,
            RedumpSystem.HasbroVideoNow,
            RedumpSystem.HasbroVideoNowColor,
            RedumpSystem.HasbroVideoNowJr,
            RedumpSystem.HasbroVideoNowXP,
            RedumpSystem.IBMPCcompatible,
            RedumpSystem.IncredibleTechnologiesEagle,
            RedumpSystem.KonamieAmusement,
            RedumpSystem.KonamiFireBeat,
            RedumpSystem.KonamiM2,
            RedumpSystem.KonamiSystemGV,
            RedumpSystem.MattelFisherPriceiXL,
            RedumpSystem.MattelHyperScan,
            RedumpSystem.MemorexVisualInformationSystem,
            RedumpSystem.MicrosoftXbox,
            RedumpSystem.MicrosoftXbox360,
            RedumpSystem.NamcoSegaNintendoTriforce,
            RedumpSystem.NamcoSystem246,
            RedumpSystem.NavisoftNaviken21,
            RedumpSystem.NECPCEngineCDTurboGrafxCD,
            RedumpSystem.NECPC88series,
            RedumpSystem.NECPC98series,
            RedumpSystem.NECPCFXPCFXGA,
            RedumpSystem.PalmOS,
            RedumpSystem.Panasonic3DOInteractiveMultiplayer,
            RedumpSystem.PanasonicM2,
            RedumpSystem.PhilipsCDi,
            RedumpSystem.PhilipsCDiDigitalVideo,
            RedumpSystem.PhotoCD,
            RedumpSystem.PlayStationGameSharkUpdates,
            RedumpSystem.SegaChihiro,
            RedumpSystem.SegaDreamcast,
            RedumpSystem.SegaMegaCDSegaCD,
            RedumpSystem.SegaNaomi,
            RedumpSystem.SegaNaomi2,
            RedumpSystem.SegaPrologue21,
            RedumpSystem.SegaSaturn,
            RedumpSystem.SNKNeoGeoCD,
            RedumpSystem.SonyPlayStation,
            RedumpSystem.SonyPlayStation2,
            RedumpSystem.SonyPlayStation3,
            RedumpSystem.TABAustriaQuizard,
            RedumpSystem.TomyKissSite,
            RedumpSystem.VideoCD,
            RedumpSystem.VTechVFlashVSmilePro,
};

        /// <summary>
        /// List of systems that has a Dat pack
        /// </summary>
        public static readonly RedumpSystem?[] HasDat = new RedumpSystem?[]
        {
            RedumpSystem.MicrosoftXboxBIOS,
            RedumpSystem.NintendoGameCubeBIOS,
            RedumpSystem.SonyPlayStationBIOS,
            RedumpSystem.SonyPlayStation2BIOS,

            RedumpSystem.AppleMacintosh,
            RedumpSystem.AtariJaguarCDInteractiveMultimediaSystem,
            RedumpSystem.AudioCD,
            RedumpSystem.BandaiPippin,
            RedumpSystem.BandaiPlaydiaQuickInteractiveSystem,
            RedumpSystem.BDVideo,
            RedumpSystem.CommodoreAmigaCD,
            RedumpSystem.CommodoreAmigaCD32,
            RedumpSystem.CommodoreAmigaCDTV,
            RedumpSystem.DVDVideo,
            RedumpSystem.FujitsuFMTownsseries,
            RedumpSystem.funworldPhotoPlay,
            RedumpSystem.HasbroVideoNow,
            RedumpSystem.HasbroVideoNowColor,
            RedumpSystem.HasbroVideoNowJr,
            RedumpSystem.HasbroVideoNowXP,
            RedumpSystem.IBMPCcompatible,
            RedumpSystem.IncredibleTechnologiesEagle,
            RedumpSystem.KonamieAmusement,
            RedumpSystem.KonamiFireBeat,
            RedumpSystem.KonamiM2,
            RedumpSystem.KonamiSystemGV,
            RedumpSystem.MattelFisherPriceiXL,
            RedumpSystem.MattelHyperScan,
            RedumpSystem.MemorexVisualInformationSystem,
            RedumpSystem.MicrosoftXbox,
            RedumpSystem.MicrosoftXbox360,
            RedumpSystem.MicrosoftXboxOne,
            RedumpSystem.NamcoSegaNintendoTriforce,
            RedumpSystem.NamcoSystem246,
            RedumpSystem.NavisoftNaviken21,
            RedumpSystem.NECPCEngineCDTurboGrafxCD,
            RedumpSystem.NECPC88series,
            RedumpSystem.NECPC98series,
            RedumpSystem.NECPCFXPCFXGA,
            RedumpSystem.NintendoGameCube,
            RedumpSystem.NintendoWii,
            RedumpSystem.NintendoWiiU,
            RedumpSystem.PalmOS,
            RedumpSystem.Panasonic3DOInteractiveMultiplayer,
            RedumpSystem.PanasonicM2,
            RedumpSystem.PhilipsCDi,
            RedumpSystem.PhilipsCDiDigitalVideo,
            RedumpSystem.PhotoCD,
            RedumpSystem.PlayStationGameSharkUpdates,
            RedumpSystem.SegaChihiro,
            RedumpSystem.SegaDreamcast,
            RedumpSystem.SegaLindbergh,
            RedumpSystem.SegaMegaCDSegaCD,
            RedumpSystem.SegaNaomi,
            RedumpSystem.SegaNaomi2,
            RedumpSystem.SegaRingEdge,
            RedumpSystem.SegaRingEdge2,
            RedumpSystem.SegaSaturn,
            RedumpSystem.SNKNeoGeoCD,
            RedumpSystem.SonyPlayStation,
            RedumpSystem.SonyPlayStation2,
            RedumpSystem.SonyPlayStation3,
            RedumpSystem.SonyPlayStation4,
            RedumpSystem.SonyPlayStationPortable,
            RedumpSystem.TABAustriaQuizard,
            RedumpSystem.TomyKissSite,
            RedumpSystem.VideoCD,
            RedumpSystem.VMLabsNUON,
            RedumpSystem.VTechVFlashVSmilePro,
            RedumpSystem.ZAPiTGamesGameWaveFamilyEntertainmentSystem,
        };

        /// <summary>
        /// List of systems that has a Decrypted Keys pack
        /// </summary>
        public static readonly RedumpSystem?[] HasDkeys = new RedumpSystem?[]
        {
            RedumpSystem.SonyPlayStation3,
        };

        /// <summary>
        /// List of systems that has a GDI pack
        /// </summary>
        public static readonly RedumpSystem?[] HasGdi = new RedumpSystem?[]
        {
            RedumpSystem.NamcoSegaNintendoTriforce,
            RedumpSystem.SegaChihiro,
            RedumpSystem.SegaDreamcast,
            RedumpSystem.SegaNaomi,
            RedumpSystem.SegaNaomi2,
        };

        /// <summary>
        /// List of systems that has a Keys pack
        /// </summary>
        public static readonly RedumpSystem?[] HasKeys = new RedumpSystem?[]
        {
            RedumpSystem.NintendoWiiU,
            RedumpSystem.SonyPlayStation3,
        };

        /// <summary>
        /// List of systems that has an LSD pack
        /// </summary>
        public static readonly RedumpSystem?[] HasLsd = new RedumpSystem?[]
        {
            RedumpSystem.IBMPCcompatible,
            RedumpSystem.SonyPlayStation,
        };

        /// <summary>
        /// List of systems that has an SBI pack
        /// </summary>
        public static readonly RedumpSystem?[] HasSbi = new RedumpSystem?[]
        {
            RedumpSystem.IBMPCcompatible,
            RedumpSystem.SonyPlayStation,
        };

        #endregion

        #region Redump Category

        /// <summary>
        /// Get the Redump longnames for each known category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public static string LongName(this DiscCategory? category)
        {
            switch (category)
            {
                case DiscCategory.Games:
                    return "Games";
                case DiscCategory.Demos:
                    return "Demos";
                case DiscCategory.Video:
                    return "Video";
                case DiscCategory.Audio:
                    return "Audio";
                case DiscCategory.Multimedia:
                    return "Multimedia";
                case DiscCategory.Applications:
                    return "Applications";
                case DiscCategory.Coverdiscs:
                    return "Coverdiscs";
                case DiscCategory.Educational:
                    return "Educational";
                case DiscCategory.BonusDiscs:
                    return "Bonus Discs";
                case DiscCategory.Preproduction:
                    return "Preproduction";
                case DiscCategory.AddOns:
                    return "Add-Ons";
                default:
                    return null;
            }
        }

        /// <summary>
        /// Get the Category enum value for a given string
        /// </summary>
        /// <param name="category">String value to convert</param>
        /// <returns>Category represented by the string, if possible</returns>
        public static DiscCategory? ToCategory(string category)
        {
            switch (category.ToLowerInvariant())
            {
                case "games":
                    return DiscCategory.Games;
                case "demos":
                    return DiscCategory.Demos;
                case "video":
                    return DiscCategory.Video;
                case "audio":
                    return DiscCategory.Audio;
                case "multimedia":
                    return DiscCategory.Multimedia;
                case "applications":
                    return DiscCategory.Applications;
                case "coverdiscs":
                    return DiscCategory.Coverdiscs;
                case "educational":
                    return DiscCategory.Educational;
                case "bonusdiscs":
                case "bonus discs":
                    return DiscCategory.BonusDiscs;
                case "preproduction":
                    return DiscCategory.Preproduction;
                case "addons":
                case "add-ons":
                    return DiscCategory.AddOns;
                default:
                    return DiscCategory.Games;
            }
        }

        #endregion

        #region Redump Language

        /// <summary>
        /// Get the Redump longnames for each known language
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public static string LongName(this Language? language)
        {
            switch (language)
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
                    return null;
            }
        }

        /// <summary>
        /// Get the Redump shortnames for each known language
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public static string ShortName(this Language? language)
        {
            switch (language)
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
                    return null;
            }
        }

        /// <summary>
        /// Get the Language enum value for a given string
        /// </summary>
        /// <param name="lang">String value to convert</param>
        /// <returns>Language represented by the string, if possible</returns>
        public static Language? ToLanguage(string lang)
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

        #endregion

        #region Redump Language Selection

        /// <summary>
        /// Get the string representation of the LanguageSelection enum values
        /// </summary>
        /// <param name="lang">LanguageSelection value to convert</param>
        /// <returns>String representing the value, if possible</returns>
        public static string LongName(this LanguageSelection? langSelect)
        {
            switch (langSelect)
            {
                case LanguageSelection.BiosSettings:
                    return "Bios settings";
                case LanguageSelection.LanguageSelector:
                    return "Language selector";
                case LanguageSelection.OptionsMenu:
                    return "Options menu";
                default:
                    return string.Empty;
            }
        }

        #endregion

        #region Redump Region

        /// <summary>
        /// Get the Redump longnames for each known region
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
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
                case Region.GreaterChina:
                    return "Greater China";
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
                case Region.SpainPortugal:
                    return "Spain, Portugal";
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
                case Region.UKAustralia:
                    return "UK, Australia";
                case Region.Ukraine:
                    return "Ukraine";
                case Region.USA:
                    return "USA";
                case Region.USAAsia:
                    return "USA, Asia";
                case Region.USABrazil:
                    return "USA, Brazil";
                case Region.USACanada:
                    return "USA, Canada";
                case Region.USAEurope:
                    return "USA, Europe";
                case Region.USAGermany:
                    return "USA, Germany";
                case Region.USAJapan:
                    return "USA, Japan";
                case Region.World:
                    return "World";
                default:
                    return null;
            }
        }

        /// <summary>
        /// Get the Redump shortnames for each known region
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
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
                case Region.GreaterChina:
                    return "GC";
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
                case Region.SpainPortugal:
                    return "S,Pt";
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
                case Region.UKAustralia:
                    return "Uk,Au";
                case Region.Ukraine:
                    return "Ue";
                case Region.USA:
                    return "U";
                case Region.USAAsia:
                    return "U,A";
                case Region.USABrazil:
                    return "U,B";
                case Region.USACanada:
                    return "U,Ca";
                case Region.USAEurope:
                    return "U,E";
                case Region.USAGermany:
                    return "U,G";
                case Region.USAJapan:
                    return "U,J";
                case Region.World:
                    return "W";
                default:
                    return null;
            }
        }

        /// <summary>
        /// Get the Region enum value for a given string
        /// </summary>
        /// <param name="region">String value to convert</param>
        /// <returns>Region represented by the string, if possible</returns>
        public static Region? ToRegion(string region)
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
                case "GC":
                    return Region.GreaterChina;
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
                case "S,Pt":
                    return Region.SpainPortugal;
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
                case "Uk,Au":
                    return Region.UKAustralia;
                case "Ue":
                    return Region.Ukraine;
                case "U":
                    return Region.USA;
                case "U,A":
                    return Region.USAAsia;
                case "U,B":
                    return Region.USABrazil;
                case "U,Ca":
                    return Region.USACanada;
                case "U,E":
                    return Region.USAEurope;
                case "U,G":
                    return Region.USAGermany;
                case "U,J":
                    return Region.USAJapan;
                case "W":
                    return Region.World;
                default:
                    return null;
            }
        }

        #endregion

        #region Redump System

        /// <summary>
        /// Get the Redump longnames for each known system
        /// </summary>
        /// <param name="system"></param>
        /// <returns></returns>
        public static string LongName(this RedumpSystem? system)
        {
            switch (system)
            {
                // Special BIOS sets
                case RedumpSystem.MicrosoftXboxBIOS:
                    return "Microsoft Xbox (BIOS)";
                case RedumpSystem.NintendoGameCubeBIOS:
                    return "Nintendo GameCube (BIOS)";
                case RedumpSystem.SonyPlayStationBIOS:
                    return "Sony PlayStation (BIOS)";
                case RedumpSystem.SonyPlayStation2BIOS:
                    return "Sony PlayStation 2 (BIOS)";

                // Regular systems
                case RedumpSystem.AcornArchimedes:
                    return "Acorn Archimedes";
                case RedumpSystem.AppleMacintosh:
                    return "Apple Macintosh";
                case RedumpSystem.AtariJaguarCDInteractiveMultimediaSystem:
                    return "Atari Jaguar CD Interactive Multimedia System";
                case RedumpSystem.AudioCD:
                    return "Audio CD";
                case RedumpSystem.BandaiPippin:
                    return "Bandai Pippin";
                case RedumpSystem.BandaiPlaydiaQuickInteractiveSystem:
                    return "Bandai Playdia Quick Interactive System";
                case RedumpSystem.BDVideo:
                    return "BD-Video";
                case RedumpSystem.CommodoreAmigaCD:
                    return "Commodore Amiga CD";
                case RedumpSystem.CommodoreAmigaCD32:
                    return "Commodore Amiga CD32";
                case RedumpSystem.CommodoreAmigaCDTV:
                    return "Commodore Amiga CDTV";
                case RedumpSystem.DVDVideo:
                    return "DVD-Video";
                case RedumpSystem.EnhancedCD:
                    return "Enhanced CD";
                case RedumpSystem.FujitsuFMTownsseries:
                    return "Fujitsu FM Towns series";
                case RedumpSystem.funworldPhotoPlay:
                    return "funworld Photo Play";
                case RedumpSystem.HasbroVideoNow:
                    return "Hasbro VideoNow";
                case RedumpSystem.HasbroVideoNowColor:
                    return "Hasbro VideoNow Color";
                case RedumpSystem.HasbroVideoNowJr:
                    return "Hasbro VideoNow Jr.";
                case RedumpSystem.HasbroVideoNowXP:
                    return "Hasbro VideoNow XP";
                case RedumpSystem.IBMPCcompatible:
                    return "IBM PC compatible";
                case RedumpSystem.IncredibleTechnologiesEagle:
                    return "Incredible Technologies Eagle";
                case RedumpSystem.KonamieAmusement:
                    return "Konami e-Amusement";
                case RedumpSystem.KonamiFireBeat:
                    return "Konami FireBeat";
                case RedumpSystem.KonamiM2:
                    return "Konami M2";
                case RedumpSystem.KonamiSystem573:
                    return "Konami System 573";
                case RedumpSystem.KonamiSystemGV:
                    return "Konami System GV";
                case RedumpSystem.KonamiTwinkle:
                    return "Konami Twinkle";
                case RedumpSystem.MattelFisherPriceiXL:
                    return "Mattel Fisher-Price iXL";
                case RedumpSystem.MattelHyperScan:
                    return "Mattel HyperScan";
                case RedumpSystem.MemorexVisualInformationSystem:
                    return "Memorex Visual Information System";
                case RedumpSystem.MicrosoftXbox:
                    return "Microsoft Xbox";
                case RedumpSystem.MicrosoftXbox360:
                    return "Microsoft Xbox 360";
                case RedumpSystem.MicrosoftXboxOne:
                    return "Microsoft Xbox One";
                case RedumpSystem.NamcoSegaNintendoTriforce:
                    return "Namco · Sega · Nintendo Triforce";
                case RedumpSystem.NamcoSystem12:
                    return "Namco System 12";
                case RedumpSystem.NamcoSystem246:
                    return "Namco System 246";
                case RedumpSystem.NavisoftNaviken21:
                    return "Navisoft Naviken 2.1";
                case RedumpSystem.NECPCEngineCDTurboGrafxCD:
                    return "NEC PC Engine CD & TurboGrafx CD";
                case RedumpSystem.NECPC88series:
                    return "NEC PC-88 series";
                case RedumpSystem.NECPC98series:
                    return "NEC PC-98 series";
                case RedumpSystem.NECPCFXPCFXGA:
                    return "NEC PC-FX & PC-FXGA";
                case RedumpSystem.NintendoGameCube:
                    return "Nintendo GameCube";
                case RedumpSystem.NintendoWii:
                    return "Nintendo Wii";
                case RedumpSystem.NintendoWiiU:
                    return "Nintendo Wii U";
                case RedumpSystem.PalmOS:
                    return "Palm OS";
                case RedumpSystem.Panasonic3DOInteractiveMultiplayer:
                    return "Panasonic 3DO Interactive Multiplayer";
                case RedumpSystem.PanasonicM2:
                    return "Panasonic M2";
                case RedumpSystem.PhilipsCDi:
                    return "Philips CD-i";
                case RedumpSystem.PhilipsCDiDigitalVideo:
                    return "Philips CD-i Digital Video";
                case RedumpSystem.PhotoCD:
                    return "Photo CD";
                case RedumpSystem.PlayStationGameSharkUpdates:
                    return "PlayStation GameShark Updates";
                case RedumpSystem.SegaChihiro:
                    return "Sega Chihiro";
                case RedumpSystem.SegaDreamcast:
                    return "Sega Dreamcast";
                case RedumpSystem.SegaLindbergh:
                    return "Sega Lindbergh";
                case RedumpSystem.SegaMegaCDSegaCD:
                    return "Sega Mega CD & Sega CD";
                case RedumpSystem.SegaNaomi:
                    return "Sega Naomi";
                case RedumpSystem.SegaNaomi2:
                    return "Sega Naomi 2";
                case RedumpSystem.SegaPrologue21:
                    return "Prologue 21";
                case RedumpSystem.SegaRingEdge:
                    return "Sega RingEdge";
                case RedumpSystem.SegaRingEdge2:
                    return "Sega RingEdge 2";
                case RedumpSystem.SegaSaturn:
                    return "Sega Saturn";
                case RedumpSystem.SegaTitanVideo:
                    return "Sega Titan Video";
                case RedumpSystem.SharpX68000:
                    return "Sharp X68000";
                case RedumpSystem.SNKNeoGeoCD:
                    return "Neo Geo CD";
                case RedumpSystem.SonyPlayStation:
                    return "Sony PlayStation";
                case RedumpSystem.SonyPlayStation2:
                    return "Sony PlayStation 2";
                case RedumpSystem.SonyPlayStation3:
                    return "Sony PlayStation 3";
                case RedumpSystem.SonyPlayStation4:
                    return "Sony PlayStation 4";
                case RedumpSystem.SonyPlayStationPortable:
                    return "Sony PlayStation Portable";
                case RedumpSystem.TABAustriaQuizard:
                    return "TAB-Austria Quizard";
                case RedumpSystem.TaoiKTV:
                    return "Tao iKTV";
                case RedumpSystem.TomyKissSite:
                    return "Tomy Kiss-Site";
                case RedumpSystem.VideoCD:
                    return "Video CD";
                case RedumpSystem.VMLabsNUON:
                    return "VM Labs NUON";
                case RedumpSystem.VTechVFlashVSmilePro:
                    return "VTech V.Flash & V.Smile Pro";
                case RedumpSystem.ZAPiTGamesGameWaveFamilyEntertainmentSystem:
                    return "ZAPiT Games Game Wave Family Entertainment System";
                default:
                    return null;
            }
        }

        /// <summary>
        /// Get the Redump shortnames for each known system
        /// </summary>
        /// <param name="system"></param>
        /// <returns></returns>
        public static string ShortName(this RedumpSystem? system)
        {
            switch (system)
            {
                // Special BIOS sets
                case RedumpSystem.MicrosoftXboxBIOS:
                    return "xbox-bios";
                case RedumpSystem.NintendoGameCubeBIOS:
                    return "gc-bios";
                case RedumpSystem.SonyPlayStationBIOS:
                    return "psx-bios";
                case RedumpSystem.SonyPlayStation2BIOS:
                    return "ps2-bios";

                // Regular systems
                case RedumpSystem.AcornArchimedes:
                    return "archcd";
                case RedumpSystem.AppleMacintosh:
                    return "mac";
                case RedumpSystem.AtariJaguarCDInteractiveMultimediaSystem:
                    return "ajcd";
                case RedumpSystem.AudioCD:
                    return "audio-cd";
                case RedumpSystem.BandaiPippin:
                    return "pippin";
                case RedumpSystem.BandaiPlaydiaQuickInteractiveSystem:
                    return "qis";
                case RedumpSystem.BDVideo:
                    return "bd-video";
                case RedumpSystem.CommodoreAmigaCD:
                    return "acd";
                case RedumpSystem.CommodoreAmigaCD32:
                    return "cd32";
                case RedumpSystem.CommodoreAmigaCDTV:
                    return "cdtv";
                case RedumpSystem.DVDVideo:
                    return "dvd-video";
                case RedumpSystem.EnhancedCD:
                    return "enhanced-cd";
                case RedumpSystem.FujitsuFMTownsseries:
                    return "fmt";
                case RedumpSystem.funworldPhotoPlay:
                    return "fpp";
                case RedumpSystem.HasbroVideoNow:
                    return "hvn";
                case RedumpSystem.HasbroVideoNowColor:
                    return "hvnc";
                case RedumpSystem.HasbroVideoNowJr:
                    return "hvnjr";
                case RedumpSystem.HasbroVideoNowXP:
                    return "hvnxp";
                case RedumpSystem.IBMPCcompatible:
                    return "pc";
                case RedumpSystem.IncredibleTechnologiesEagle:
                    return "ite";
                case RedumpSystem.KonamieAmusement:
                    return "kea";
                case RedumpSystem.KonamiFireBeat:
                    return "kfb";
                case RedumpSystem.KonamiM2:
                    return "km2";
                case RedumpSystem.KonamiSystem573:
                    return "ks573";
                case RedumpSystem.KonamiSystemGV:
                    return "ksgv";
                case RedumpSystem.KonamiTwinkle:
                    return "kt";
                case RedumpSystem.MattelFisherPriceiXL:
                    return "ixl";
                case RedumpSystem.MattelHyperScan:
                    return "hs";
                case RedumpSystem.MemorexVisualInformationSystem:
                    return "vis";
                case RedumpSystem.MicrosoftXbox:
                    return "xbox";
                case RedumpSystem.MicrosoftXbox360:
                    return "xbox360";
                case RedumpSystem.MicrosoftXboxOne:
                    return "xboxone";
                case RedumpSystem.NamcoSegaNintendoTriforce:
                    return "triforce";
                case RedumpSystem.NamcoSystem12:
                    return "ns12";
                case RedumpSystem.NamcoSystem246:
                    return "ns246";
                case RedumpSystem.NavisoftNaviken21:
                    return "navi21";
                case RedumpSystem.NECPCEngineCDTurboGrafxCD:
                    return "pce";
                case RedumpSystem.NECPC88series:
                    return "pc-88";
                case RedumpSystem.NECPC98series:
                    return "pc-98";
                case RedumpSystem.NECPCFXPCFXGA:
                    return "pc-fx";
                case RedumpSystem.NintendoGameCube:
                    return "gc";
                case RedumpSystem.NintendoWii:
                    return "wii";
                case RedumpSystem.NintendoWiiU:
                    return "wiiu";
                case RedumpSystem.PalmOS:
                    return "palm";
                case RedumpSystem.Panasonic3DOInteractiveMultiplayer:
                    return "3do";
                case RedumpSystem.PanasonicM2:
                    return "m2";
                case RedumpSystem.PhilipsCDi:
                    return "cdi";
                case RedumpSystem.PhilipsCDiDigitalVideo:
                    return "cdi-video";
                case RedumpSystem.PhotoCD:
                    return "photo-cd";
                case RedumpSystem.PlayStationGameSharkUpdates:
                    return "psxgs";
                case RedumpSystem.SegaChihiro:
                    return "chihiro";
                case RedumpSystem.SegaDreamcast:
                    return "dc";
                case RedumpSystem.SegaLindbergh:
                    return "lindbergh";
                case RedumpSystem.SegaMegaCDSegaCD:
                    return "mcd";
                case RedumpSystem.SegaNaomi:
                    return "naomi";
                case RedumpSystem.SegaNaomi2:
                    return "naomi2";
                case RedumpSystem.SegaPrologue21:
                    return "pl21";
                case RedumpSystem.SegaRingEdge:
                    return "sre";
                case RedumpSystem.SegaRingEdge2:
                    return "sre2";
                case RedumpSystem.SegaSaturn:
                    return "ss";
                case RedumpSystem.SegaTitanVideo:
                    return "stv";
                case RedumpSystem.SharpX68000:
                    return "x86kcd";
                case RedumpSystem.SNKNeoGeoCD:
                    return "ngcd";
                case RedumpSystem.SonyPlayStation:
                    return "psx";
                case RedumpSystem.SonyPlayStation2:
                    return "ps2";
                case RedumpSystem.SonyPlayStation3:
                    return "ps3";
                case RedumpSystem.SonyPlayStation4:
                    return "ps4";
                case RedumpSystem.SonyPlayStationPortable:
                    return "psp";
                case RedumpSystem.TABAustriaQuizard:
                    return "quizard";
                case RedumpSystem.TaoiKTV:
                    return "iktv";
                case RedumpSystem.TomyKissSite:
                    return "ksite";
                case RedumpSystem.VideoCD:
                    return "vcd";
                case RedumpSystem.VMLabsNUON:
                    return "nuon";
                case RedumpSystem.VTechVFlashVSmilePro:
                    return "vflash";
                case RedumpSystem.ZAPiTGamesGameWaveFamilyEntertainmentSystem:
                    return "gamewave";
                default:
                    return null;
            }
        }

        /// <summary>
        /// Get the RedumpSystem enum value for a given string
        /// </summary>
        /// <param name="sys">String value to convert</param>
        /// <returns>RedumpSystem represented by the string, if possible</returns>
        public static RedumpSystem? ToRedumpSystem(string sys)
        {
            switch (sys)
            {
                // Special BIOS Sets
                case "xboxbios":
                case "xbox bios":
                case "microsoftxboxbios":
                case "microsoftxbox bios":
                case "microsoft xbox bios":
                    return RedumpSystem.MicrosoftXboxBIOS;
                case "gcbios":
                case "gc bios":
                case "gamecubebios":
                case "ngcbios":
                case "ngc bios":
                case "nintendogamecubebios":
                case "nintendo gamecube bios":
                    return RedumpSystem.NintendoGameCubeBIOS;
                case "ps1bios":
                case "ps1 bios":
                case "psxbios":
                case "psx bios":
                case "playstationbios":
                case "playstation bios":
                case "sonyps1bios":
                case "sonyps1 bios":
                case "sony ps1 bios":
                case "sonypsxbios":
                case "sonypsx bios":
                case "sony psx bios":
                case "sonyplaystationbios":
                case "sonyplaystation bios":
                case "sony playstation bios":
                    return RedumpSystem.SonyPlayStationBIOS;
                case "ps2bios":
                case "ps2 bios":
                case "playstation2bios":
                case "playstation2 bios":
                case "playstation 2 bios":
                case "sonyps2bios":
                case "sonyps2 bios":
                case "sony ps2 bios":
                case "sonyplaystation2bios":
                case "sonyplaystation2 bios":
                case "sony playstation 2 bios":
                    return RedumpSystem.SonyPlayStation2BIOS;

                // Regular systems
                case "acorn":
                case "archimedes":
                case "acornarchimedes":
                case "acorn archimedes":
                    return RedumpSystem.AcornArchimedes;
                case "apple":
                case "mac":
                case "applemac":
                case "macintosh":
                case "applemacintosh":
                case "apple mac":
                case "apple macintosh":
                    return RedumpSystem.AppleMacintosh;
                case "jaguar":
                case "jagcd":
                case "jaguarcd":
                case "jaguar cd":
                case "atarijaguar":
                case "atarijagcd":
                case "atarijaguarcd":
                case "atari jaguar cd":
                    return RedumpSystem.AtariJaguarCDInteractiveMultimediaSystem;
                case "audio":
                case "audiocd":
                case "audio cd":
                    return RedumpSystem.AudioCD;
                case "playdia":
                case "playdiaqis":
                case "playdiaquickinteractivesystem":
                case "bandaiplaydia":
                case "bandaiplaydiaquickinteractivesystem":
                case "bandai playdia quick interactive system":
                    return RedumpSystem.BandaiPlaydiaQuickInteractiveSystem;
                case "pippin":
                case "bandaipippin":
                case "bandai pippin":
                case "applepippin":
                case "apple pippin":
                case "bandaiapplepippin":
                case "bandai apple pippin":
                case "bandai / apple pippin":
                    return RedumpSystem.BandaiPippin;
                case "bdvideo":
                case "bd-video":
                case "blurayvideo":
                case "bluray video":
                    return RedumpSystem.BDVideo;
                case "amiga":
                case "amigacd":
                case "amiga cd":
                case "commodoreamiga":
                case "commodoreamigacd":
                case "commodoreamiga cd":
                case "commodore amiga":
                case "commodore amiga cd":
                    return RedumpSystem.CommodoreAmigaCD;
                case "cd32":
                case "amigacd32":
                case "amiga cd32":
                case "commodoreamigacd32":
                case "commodore amiga cd32":
                    return RedumpSystem.CommodoreAmigaCD32;
                case "cdtv":
                case "amigacdtv":
                case "amiga cdtv":
                case "commodoreamigacdtv":
                case "commodore amiga cdtv":
                    return RedumpSystem.CommodoreAmigaCDTV;
                case "dvdvideo":
                case "dvd-video":
                    return RedumpSystem.DVDVideo;
                case "enhancedcd":
                case "enhanced cd":
                case "enhancedcdrom":
                case "enhanced cdrom":
                case "enhanced cd-rom":
                    return RedumpSystem.EnhancedCD;
                case "fmtowns":
                case "fmt":
                case "fm towns":
                case "fujitsufmtowns":
                case "fujitsu fm towns":
                case "fujitsu fm towns series":
                    return RedumpSystem.FujitsuFMTownsseries;
                case "fpp":
                case "funworldphotoplay":
                case "funworld photoplay":
                case "funworld photo play":
                    return RedumpSystem.funworldPhotoPlay;
                case "videonow":
                case "hasbrovideonow":
                case "hasbro videonow":
                    return RedumpSystem.HasbroVideoNow;
                case "videonowcolor":
                case "videonow color":
                case "hasbrovideonowcolor":
                case "hasbro videonow color":
                    return RedumpSystem.HasbroVideoNowColor;
                case "videonowjr":
                case "videonow jr":
                case "hasbrovideonowjr":
                case "hasbro videonow jr":
                    return RedumpSystem.HasbroVideoNowColor;
                case "videonowxp":
                case "videonow xp":
                case "hasbrovideonowxp":
                case "hasbro videonow xp":
                    return RedumpSystem.HasbroVideoNowColor;
                case "ibm":
                case "ibmpc":
                case "pc":
                case "ibm pc":
                case "ibm pc compatible":
                    return RedumpSystem.IBMPCcompatible;
                case "iteagle":
                case "eagle":
                case "incredible technologies eagle":
                    return RedumpSystem.IncredibleTechnologiesEagle;
                case "eamusement":
                case "e-amusement":
                case "konamieamusement":
                case "konami eamusement":
                case "konamie-amusement":
                case "konami e-amusement":
                    return RedumpSystem.KonamieAmusement;
                case "firebeat":
                case "konamifirebeat":
                case "konami firebeat":
                    return RedumpSystem.KonamiFireBeat;
                case "konamim2":
                case "konami m2":
                    return RedumpSystem.KonamiM2;
                case "system573":
                case "system 573":
                case "konamisystem573":
                case "konami system 573":
                    return RedumpSystem.KonamiSystem573;
                case "gvsystem":
                case "systemgv":
                case "gv system":
                case "system gv":
                case "konamigvsystem":
                case "konamisystemgv":
                case "konami gv system":
                case "konami system gv":
                    return RedumpSystem.KonamiSystemGV;
                case "twinkle":
                case "konamitwinkle":
                case "konami twinkle":
                    return RedumpSystem.KonamiTwinkle;
                case "ixl":
                case "mattelixl":
                case "mattel ixl":
                case "fisherpriceixl":
                case "fisher price ixl":
                case "fisher-price ixl":
                case "fisherprice ixl":
                case "mattelfisherpriceixl":
                case "mattel fisher price ixl":
                case "mattelfisherprice ixl":
                case "mattel fisherprice ixl":
                case "mattel fisher-price ixl":
                    return RedumpSystem.MattelFisherPriceiXL;
                case "hyperscan":
                case "mattelhyperscan":
                case "mattel hyperscan":
                    return RedumpSystem.MattelHyperScan;
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
                    return RedumpSystem.MemorexVisualInformationSystem;
                case "xbox":
                case "microsoftxbox":
                case "microsoft xbox":
                    return RedumpSystem.MicrosoftXbox;
                case "x360":
                case "xbox360":
                case "microsoftx360":
                case "microsoftxbox360":
                case "microsoft x360":
                case "microsoft xbox 360":
                    return RedumpSystem.MicrosoftXbox360;
                case "xb1":
                case "xbone":
                case "xboxone":
                case "microsoftxbone":
                case "microsoftxboxone":
                case "microsoft xbone":
                case "microsoft xbox one":
                    return RedumpSystem.MicrosoftXboxOne;
                case "triforce":
                case "namcotriforce":
                case "namco triforce":
                case "segatriforce":
                case "sega triforce":
                case "nintendotriforce":
                case "nintendo triforce":
                case "namco / sega / nintendo triforce":
                    return RedumpSystem.NamcoSegaNintendoTriforce;
                case "system12":
                case "system 12":
                case "namcosystem12":
                case "namco system 12":
                    return RedumpSystem.NamcoSystem12;
                case "system246":
                case "system 246":
                case "namcosystem246":
                case "namco system 246":
                case "capcomsystem246":
                case "capcom system 246":
                case "taitosystem246":
                case "taito system 246":
                case "namco / capcom / taito system 246":
                    return RedumpSystem.NamcoSystem246;
                case "naviken":
                case "naviken21":
                case "naviken 2.1":
                case "navisoftnaviken":
                case "navisoft naviken":
                case "navisoftnaviken21":
                case "navisoft naviken 2.1":
                    return RedumpSystem.NavisoftNaviken21;
                case "pcecd":
                case "pce-cd":
                case "tgcd":
                case "tg-cd":
                case "necpcecd":
                case "nectgcd":
                case "nec pc-engine cd":
                case "nec turbografx cd":
                case "nec pc-engine / turbografx cd":
                    return RedumpSystem.NECPCEngineCDTurboGrafxCD;
                case "pc88":
                case "pc-88":
                case "necpc88":
                case "nec pc88":
                case "nec pc-88":
                    return RedumpSystem.NECPC88series;
                case "pc98":
                case "pc-98":
                case "necpc98":
                case "nec pc98":
                case "nec pc-98":
                    return RedumpSystem.NECPC98series;
                case "pcfx":
                case "pc-fx":
                case "pcfxga":
                case "pc-fxga":
                case "necpcfx":
                case "necpcfxga":
                case "nec pc-fx":
                case "nec pc-fxga":
                case "nec pc-fx / pc-fxga":
                    return RedumpSystem.NECPCFXPCFXGA;
                case "gc":
                case "gamecube":
                case "ngc":
                case "nintendogamecube":
                case "nintendo gamecube":
                    return RedumpSystem.NintendoGameCube;
                case "wii":
                case "nintendowii":
                case "nintendo wii":
                    return RedumpSystem.NintendoWii;
                case "wiiu":
                case "wii u":
                case "nintendowiiu":
                case "nintendo wii u":
                    return RedumpSystem.NintendoWiiU;
                case "palm":
                case "palmos":
                    return RedumpSystem.PalmOS;
                case "3do":
                case "3do interactive multiplayer":
                case "panasonic3do":
                case "panasonic 3do":
                case "panasonic 3do interactive multiplayer":
                    return RedumpSystem.Panasonic3DOInteractiveMultiplayer;
                case "panasonicm2":
                case "panasonic m2":
                    return RedumpSystem.PanasonicM2;
                case "cdi":
                case "cd-i":
                case "philipscdi":
                case "philips cdi":
                case "philips cd-i":
                    return RedumpSystem.PhilipsCDi;
                case "cdidv":
                case "cdidigitalvideo":
                case "cdi digital video":
                case "cd-i digital video":
                case "philipscdidigitalvideo":
                case "philips cdi digital video":
                case "philips cd-i digital video":
                    return RedumpSystem.PhilipsCDiDigitalVideo;
                case "photo":
                case "photocd":
                case "photo cd":
                    return RedumpSystem.PhotoCD;
                case "gameshark":
                case "psgameshark":
                case "ps gameshark":
                case "playstationgameshark":
                case "playstation gameshark":
                case "playstation gameshark updates":
                    return RedumpSystem.PlayStationGameSharkUpdates;
                case "chihiro":
                case "segachihiro":
                case "sega chihiro":
                    return RedumpSystem.SegaChihiro;
                case "dc":
                case "sdc":
                case "dreamcast":
                case "segadreamcast":
                case "sega dreamcast":
                    return RedumpSystem.SegaDreamcast;
                case "lindbergh":
                case "segalindbergh":
                case "sega lindbergh":
                    return RedumpSystem.SegaLindbergh;
                case "scd":
                case "mcd":
                case "smcd":
                case "segacd":
                case "megacd":
                case "segamegacd":
                case "sega cd":
                case "mega cd":
                case "sega cd / mega cd":
                    return RedumpSystem.SegaMegaCDSegaCD;
                case "naomi":
                case "seganaomi":
                case "sega naomi":
                    return RedumpSystem.SegaNaomi;
                case "naomi2":
                case "naomi 2":
                case "seganaomi2":
                case "sega naomi 2":
                    return RedumpSystem.SegaNaomi2;
                case "pl21":
                case "prologue21":
                case "prologue 21":
                case "segaprologue21":
                case "sega prologue21":
                case "sega prologue 21":
                    return RedumpSystem.SegaPrologue21;
                case "ringedge":
                case "segaringedge":
                case "sega ringedge":
                    return RedumpSystem.SegaRingEdge;
                case "ringedge2":
                case "ringedge 2":
                case "segaringedge2":
                case "sega ringedge 2":
                    return RedumpSystem.SegaRingEdge2;
                case "saturn":
                case "segasaturn":
                case "sega saturn":
                    return RedumpSystem.SegaSaturn;
                case "stv":
                case "titanvideo":
                case "titan video":
                case "segatitanvideo":
                case "sega titan video":
                    return RedumpSystem.SegaTitanVideo;
                case "x68k":
                case "x68000":
                case "sharpx68k":
                case "sharp x68k":
                case "sharpx68000":
                case "sharp x68000":
                    return RedumpSystem.SharpX68000;
                case "ngcd":
                case "neogeocd":
                case "neogeo cd":
                case "neo geo cd":
                case "snk ngcd":
                case "snk neogeo cd":
                case "snk neo geo cd":
                    return RedumpSystem.SNKNeoGeoCD;
                case "ps1":
                case "psx":
                case "playstation":
                case "sonyps1":
                case "sony ps1":
                case "sonypsx":
                case "sony psx":
                case "sonyplaystation":
                case "sony playstation":
                    return RedumpSystem.SonyPlayStation;
                case "ps2":
                case "playstation2":
                case "playstation 2":
                case "sonyps2":
                case "sony ps2":
                case "sonyplaystation2":
                case "sony playstation 2":
                    return RedumpSystem.SonyPlayStation2;
                case "ps3":
                case "playstation3":
                case "playstation 3":
                case "sonyps3":
                case "sony ps3":
                case "sonyplaystation3":
                case "sony playstation 3":
                    return RedumpSystem.SonyPlayStation3;
                case "ps4":
                case "playstation4":
                case "playstation 4":
                case "sonyps4":
                case "sony ps4":
                case "sonyplaystation4":
                case "sony playstation 4":
                    return RedumpSystem.SonyPlayStation4;
                case "psp":
                case "playstationportable":
                case "playstation portable":
                case "sonypsp":
                case "sony psp":
                case "sonyplaystationportable":
                case "sony playstation portable":
                    return RedumpSystem.SonyPlayStationPortable;
                case "quizard":
                case "tabaustriaquizard":
                case "tab-austria quizard":
                    return RedumpSystem.TABAustriaQuizard;
                case "iktv":
                case "taoiktv":
                case "tao iktv":
                    return RedumpSystem.TaoiKTV;
                case "kisssite":
                case "kiss-site":
                case "tomykisssite":
                case "tomy kisssite":
                case "tomy kiss-site":
                    return RedumpSystem.TomyKissSite;
                case "vcd":
                case "videocd":
                case "video cd":
                    return RedumpSystem.VideoCD;
                case "nuon":
                case "vmlabsnuon":
                case "vm labs nuon":
                    return RedumpSystem.VMLabsNUON;
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
                    return RedumpSystem.VTechVFlashVSmilePro;
                case "gamewave":
                case "game wave":
                case "zapit":
                case "zapitgamewave":
                case "zapit game wave":
                case "zapit games game wave family entertainment system":
                    return RedumpSystem.ZAPiTGamesGameWaveFamilyEntertainmentSystem;

                default:
                    return null;
            }
        }

        #endregion
    }
}
