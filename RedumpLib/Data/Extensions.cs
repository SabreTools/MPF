using RedumpLib.Attributes;

namespace RedumpLib.Data
{
    /// <summary>
    /// Information pertaining to Redump systems
    /// </summary>
    public static class Extensions
    {
        #region Disc Category

        /// <summary>
        /// Get the Redump longnames for each known category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public static string LongName(this DiscCategory? category) => AttributeHelper<DiscCategory?>.GetAttribute(category)?.LongName;

        /// <summary>
        /// Get the Category enum value for a given string
        /// </summary>
        /// <param name="category">String value to convert</param>
        /// <returns>Category represented by the string, if possible</returns>
        public static DiscCategory? ToDiscCategory(string category)
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

        #region Disc Type

        /// <summary>
        /// Get the Redump longnames for each known disc type
        /// </summary>
        /// <param name="discType"></param>
        /// <returns></returns>
        public static string LongName(this DiscType? discType) => AttributeHelper<DiscType?>.GetAttribute(discType)?.LongName;

        /// <summary>
        /// Get the DiscType enum value for a given string
        /// </summary>
        /// <param name="discType">String value to convert</param>
        /// <returns>DiscType represented by the string, if possible</returns>
        public static DiscType? ToDiscType(string discType)
        {
            switch (discType.ToLowerInvariant())
            {
                case "bd25":
                case "bd-25":
                    return DiscType.BD25;
                case "bd50":
                case "bd-50":
                    return DiscType.BD50;
                case "cd":
                case "cdrom":
                case "cd-rom":
                    return DiscType.CD;
                case "dvd5":
                case "dvd-5":
                    return DiscType.DVD5;
                case "dvd9":
                case "dvd-9":
                    return DiscType.DVD9;
                case "gd":
                case "gdrom":
                case "gd-rom":
                    return DiscType.GDROM;
                case "hddvd":
                case "hddvdsl":
                case "hd-dvd sl":
                    return DiscType.HDDVDSL;
                case "milcd":
                case "mil-cd":
                    return DiscType.MILCD;
                case "nintendogamecubegamedisc":
                case "nintendo game cube game disc":
                    return DiscType.NintendoGameCubeGameDisc;
                case "nintendowiiopticaldiscsl":
                case "nintendo wii optical disc sl":
                    return DiscType.NintendoWiiOpticalDiscSL;
                case "nintendowiiopticaldiscdl":
                case "nintendo wii optical disc dl":
                    return DiscType.NintendoWiiOpticalDiscDL;
                case "nintendowiiuopticaldiscsl":
                case "nintendo wii u optical disc sl":
                    return DiscType.NintendoWiiUOpticalDiscSL;
                case "umd":
                case "umdsl":
                case "umd sl":
                    return DiscType.UMDSL;
                case "umddl":
                case "umd dl":
                    return DiscType.UMDDL;
                default:
                    return null;
            }
        }

        #endregion

        #region Language

        /// <summary>
        /// Get the Redump longnames for each known language
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public static string LongName(this Language? language) => AttributeHelper<Language?>.GetAttribute(language)?.LongName;

        /// <summary>
        /// Get the Redump shortnames for each known language
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public static string ShortName(this Language? language) => AttributeHelper<Language?>.GetAttribute(language)?.ShortName;

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
                case "sqi":
                    return Language.Albanian;
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
                case "est":
                    return Language.Estonian;
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
                case "ind":
                    return Language.Indonesian;
                case "isl":
                    return Language.Icelandic;
                case "ita":
                    return Language.Italian;
                case "jap":
                    return Language.Japanese;
                case "kor":
                    return Language.Korean;
                case "lat":
                    return Language.Latin;
                case "lav":
                    return Language.Latvian;
                case "lit":
                    return Language.Lithuanian;
                case "mkd":
                    return Language.Macedonian;
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
                case "srp":
                    return Language.Serbian;
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

        #region Language Selection

        /// <summary>
        /// Get the string representation of the LanguageSelection enum values
        /// </summary>
        /// <param name="langSelect">LanguageSelection value to convert</param>
        /// <returns>String representing the value, if possible</returns>
        public static string LongName(this LanguageSelection? langSelect) => AttributeHelper<LanguageSelection?>.GetAttribute(langSelect)?.LongName;

        #endregion

        #region Region

        /// <summary>
        /// Get the Redump longnames for each known region
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        public static string LongName(this Region? region) => AttributeHelper<Region?>.GetAttribute(region)?.LongName;

        /// <summary>
        /// Get the Redump shortnames for each known region
        /// </summary>
        /// <param name="region"></param>
        /// <returns></returns>
        public static string ShortName(this Region? region) => AttributeHelper<Region?>.GetAttribute(region)?.ShortName;

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
                case "Au,G":
                    return Region.AustraliaGermany;
                case "Au,Nz":
                    return Region.AustraliaNewZealand;
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
                case "Bg":
                    return Region.Bulgaria;
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
                case "Ee":
                    return Region.Estonia;
                case "E":
                    return Region.Europe;
                case "E,A":
                    return Region.EuropeAsia;
                case "E,Au":
                    return Region.EuropeAustralia;
                case "E,Ca":
                    return Region.EuropeCanada;
                case "E,G":
                    return Region.EuropeGermany;
                case "Ex":
                    return Region.Export;
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
                case "Is":
                    return Region.Iceland;
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
                case "Lt":
                    return Region.Lithuania;
                case "N":
                    return Region.Netherlands;
                case "Nz":
                    return Region.NewZealand;
                case "No":
                    return Region.Norway;
                case "P":
                    return Region.Poland;
                case "Pt":
                    return Region.Portugal;
                case "Ro":
                    return Region.Romania;
                case "R":
                    return Region.Russia;
                case "Sca":
                    return Region.Scandinavia;
                case "Rs":
                    return Region.Serbia;
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
                case "U,Au":
                    return Region.USAAustralia;
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
                case "U,K":
                    return Region.USAKorea;
                case "W":
                    return Region.World;
                default:
                    return null;
            }
        }

        #endregion

        #region System

        /// <summary>
        /// Get the Redump longnames for each known system
        /// </summary>
        /// <param name="system"></param>
        /// <returns></returns>
        public static string LongName(this RedumpSystem? system) => AttributeHelper<RedumpSystem?>.GetAttribute(system)?.LongName;

        /// <summary>
        /// Get the Redump shortnames for each known system
        /// </summary>
        /// <param name="system"></param>
        /// <returns></returns>
        public static string ShortName(this RedumpSystem? system) => AttributeHelper<RedumpSystem?>.GetAttribute(system)?.ShortName;

        /// <summary>
        /// Determine the category of a system
        /// </summary>
        public static SystemCategory GetCategory(this RedumpSystem? system) => ((SystemAttribute)AttributeHelper<RedumpSystem?>.GetAttribute(system))?.Category ?? SystemCategory.NONE;

        /// <summary>
        /// Determine if a system is available in Redump yet
        /// </summary>
        public static bool IsAvailable(this RedumpSystem? system) => ((SystemAttribute)AttributeHelper<RedumpSystem?>.GetAttribute(system))?.Available ?? false;

        /// <summary>
        /// Determine if a system is restricted to dumpers
        /// </summary>
        public static bool IsBanned(this RedumpSystem? system) => ((SystemAttribute)AttributeHelper<RedumpSystem?>.GetAttribute(system))?.IsBanned ?? false;

        /// <summary>
        /// Determine if a system has a CUE pack
        /// </summary>
        public static bool HasCues(this RedumpSystem? system) => ((SystemAttribute)AttributeHelper<RedumpSystem?>.GetAttribute(system))?.HasCues ?? false;

        /// <summary>
        /// Determine if a system has a DAT
        /// </summary>
        public static bool HasDat(this RedumpSystem? system) => ((SystemAttribute)AttributeHelper<RedumpSystem?>.GetAttribute(system))?.HasDat ?? false;

        /// <summary>
        /// Determine if a system has a decrypted keys pack
        /// </summary>
        public static bool HasDkeys(this RedumpSystem? system) => ((SystemAttribute)AttributeHelper<RedumpSystem?>.GetAttribute(system))?.HasDkeys ?? false;

        /// <summary>
        /// Determine if a system has a GDI pack
        /// </summary>
        public static bool HasGdi(this RedumpSystem? system) => ((SystemAttribute)AttributeHelper<RedumpSystem?>.GetAttribute(system))?.HasGdi ?? false;

        /// <summary>
        /// Determine if a system has a keys pack
        /// </summary>
        public static bool HasKeys(this RedumpSystem? system) => ((SystemAttribute)AttributeHelper<RedumpSystem?>.GetAttribute(system))?.HasKeys ?? false;

        /// <summary>
        /// Determine if a system has an LSD pack
        /// </summary>
        public static bool HasLsd(this RedumpSystem? system) => ((SystemAttribute)AttributeHelper<RedumpSystem?>.GetAttribute(system))?.HasLsd ?? false;

        /// <summary>
        /// Determine if a system has an SBI pack
        /// </summary>
        public static bool HasSbi(this RedumpSystem? system) => ((SystemAttribute)AttributeHelper<RedumpSystem?>.GetAttribute(system))?.HasSbi ?? false;

        /// <summary>
        /// Get the RedumpSystem enum value for a given string
        /// </summary>
        /// <param name="sys">String value to convert</param>
        /// <returns>RedumpSystem represented by the string, if possible</returns>
        public static RedumpSystem? ToRedumpSystem(string sys)
        {
            switch (sys)
            {
                #region BIOS Sets

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

                #endregion

                #region Consoles

                case "jaguar":
                case "jagcd":
                case "jaguarcd":
                case "jaguar cd":
                case "atarijaguar":
                case "atarijagcd":
                case "atarijaguarcd":
                case "atari jaguar cd":
                    return RedumpSystem.AtariJaguarCDInteractiveMultimediaSystem;
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
                case "evosc":
                case "evo sc":
                case "evosmartconsole":
                case "evo smart console":
                case "envizionsevosc":
                case "envizion evo sc":
                case "envizionevosmartconsole":
                case "envizion evo smart console":
                    return RedumpSystem.EnvizionsEVOSmartConsole;
                case "fmtm":
                case "fmtownsmarty":
                case "fm towns marty":
                case "fujitsufmtownsmarty":
                case "fujitsu fm towns marty":
                    return RedumpSystem.FujitsuFMTownsMarty;
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
                    return RedumpSystem.HasbroVideoNowJr;
                case "videonowxp":
                case "videonow xp":
                case "hasbrovideonowxp":
                case "hasbro videonow xp":
                    return RedumpSystem.HasbroVideoNowXP;
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
                case "xbs":
                case "xbseries":
                case "xbseriess":
                case "xbseriesx":
                case "xbseriessx":
                case "xboxseries":
                case "xboxseriess":
                case "xboxseriesx":
                case "xboxseriesxs":
                case "microsoftxboxseries":
                case "microsoftxboxseriess":
                case "microsoftxboxseriesx":
                case "microsoftxboxseriesxs":
                case "microsoft xbox series":
                case "microsoft xbox series s":
                case "microsoft xbox series x":
                case "microsoft xbox series x and s":
                    return RedumpSystem.MicrosoftXboxSeriesXS;
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
                    return RedumpSystem.NintendoSonySuperNESCDROMSystem;
                case "wii":
                case "nintendowii":
                case "nintendo wii":
                    return RedumpSystem.NintendoWii;
                case "wiiu":
                case "wii u":
                case "nintendowiiu":
                case "nintendo wii u":
                    return RedumpSystem.NintendoWiiU;
                case "3do":
                case "3do interactive multiplayer":
                case "panasonic3do":
                case "panasonic 3do":
                case "panasonic 3do interactive multiplayer":
                    return RedumpSystem.Panasonic3DOInteractiveMultiplayer;
                case "cdi":
                case "cd-i":
                case "philipscdi":
                case "philips cdi":
                case "philips cd-i":
                    return RedumpSystem.PhilipsCDi;
                case "laseractive":
                case "pioneerlaseractive":
                case "pioneer laseractive":
                    return RedumpSystem.PioneerLaserActive;
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
                case "dc":
                case "sdc":
                case "dreamcast":
                case "segadreamcast":
                case "sega dreamcast":
                    return RedumpSystem.SegaDreamcast;
                case "saturn":
                case "segasaturn":
                case "sega saturn":
                    return RedumpSystem.SegaSaturn;
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
                case "ps5":
                case "playstation5":
                case "playstation 5":
                case "sonyps5":
                case "sony ps5":
                case "sonyplaystation5":
                case "sony playstation 5":
                    return RedumpSystem.SonyPlayStation5;
                case "psp":
                case "playstationportable":
                case "playstation portable":
                case "sonypsp":
                case "sony psp":
                case "sonyplaystationportable":
                case "sony playstation portable":
                    return RedumpSystem.SonyPlayStationPortable;
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

                #endregion

                #region Computers

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
                case "amiga":
                case "commodoreamiga":
                case "commodore amiga":
                    return RedumpSystem.CommodoreAmigaCD;
                case "fmtowns":
                case "fmt":
                case "fm towns":
                case "fujitsufmtowns":
                case "fujitsu fm towns":
                case "fujitsu fm towns series":
                    return RedumpSystem.FujitsuFMTownsseries;
                case "ibm":
                case "ibmpc":
                case "pc":
                case "ibm pc":
                case "ibm pc compatible":
                    return RedumpSystem.IBMPCcompatible;
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
                case "x68k":
                case "x68000":
                case "sharpx68k":
                case "sharp x68k":
                case "sharpx68000":
                case "sharp x68000":
                    return RedumpSystem.SharpX68000;

                #endregion

                #region Arcade

                case "cubo":
                case "cubocd32":
                case "cubo cd32":
                case "amigacubocd32":
                case "amiga cubo cd32":
                    return RedumpSystem.AmigaCUBOCD32;
                case "alg3do":
                case "alg 3do":
                case "americanlasergames3do":
                case "american laser games 3do":
                    return RedumpSystem.AmericanLaserGames3DO;
                case "atari3do":
                case "atari 3do":
                    return RedumpSystem.Atari3DO;
                case "atronic":
                    return RedumpSystem.Atronic;
                case "auscom":
                case "auscomsystem1":
                case "auscom system 1":
                    return RedumpSystem.AUSCOMSystem1;
                case "gamemagic":
                case "game magic":
                case "ballygamemagic":
                case "bally game magic":
                    return RedumpSystem.BallyGameMagic;
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
                    return RedumpSystem.CapcomCPSystemIII;
                case "fpp":
                case "funworldphotoplay":
                case "funworld photoplay":
                case "funworld photo play":
                    return RedumpSystem.funworldPhotoPlay;
                case "globalvr":
                case "global vr":
                case "global vr pc-based systems":
                    return RedumpSystem.GlobalVRVarious;
                case "vortek":
                case "globalvrvortek":
                case "global vr vortek":
                    return RedumpSystem.GlobalVRVortek;
                case "vortekv3":
                case "vortek v3":
                case "globalvrvortekv3":
                case "global vr vortek v3":
                    return RedumpSystem.GlobalVRVortekV3;
                case "ice":
                case "icepc":
                case "ice pc":
                case "ice pc-based hardware":
                    return RedumpSystem.ICEPCHardware;
                case "iteagle":
                case "eagle":
                case "incredible technologies eagle":
                    return RedumpSystem.IncredibleTechnologiesEagle;
                case "itpc":
                case "incredible technologies pc-based systems":
                    return RedumpSystem.IncredibleTechnologiesVarious;
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
                case "gvsystem":
                case "gv system":
                case "konamigvsystem":
                case "konami gv system":
                case "systemgv":
                case "system gv":
                case "konamisystemgv":
                case "konami system gv":
                    return RedumpSystem.KonamiSystemGV;
                case "konamim2":
                case "konami m2":
                    return RedumpSystem.KonamiM2;
                case "python":
                case "konamipython":
                case "konami python":
                    return RedumpSystem.KonamiPython;
                case "python2":
                case "python 2":
                case "konamipython2":
                case "konami python 2":
                    return RedumpSystem.KonamiPython2;
                case "system573":
                case "system 573":
                case "konamisystem573":
                case "konami system 573":
                    return RedumpSystem.KonamiSystem573;
                case "twinkle":
                case "konamitwinkle":
                case "konami twinkle":
                    return RedumpSystem.KonamiTwinkle;
                case "konamipc":
                case "konami pc":
                case "konami pc-based systems":
                    return RedumpSystem.KonamiVarious;
                case "boardwalk":
                case "meritindustriesboardwalk":
                case "merit industries boardwalk":
                    return RedumpSystem.MeritIndustriesBoardwalk;
                case "megatouchforce":
                case "megatouch force":
                case "meritindustriesmegatouchforce":
                case "merit industries megatouch force":
                    return RedumpSystem.MeritIndustriesMegaTouchForce;
                case "megatouchion":
                case "megatouch ion":
                case "meritindustriesmegatouchion":
                case "merit industries megatouch ion":
                    return RedumpSystem.MeritIndustriesMegaTouchION;
                case "megatouchmaxx":
                case "megatouch maxx":
                case "meritindustriesmegatouchmaxx":
                case "merit industries megatouch maxx":
                    return RedumpSystem.MeritIndustriesMegaTouchMaxx;
                case "megatouchxl":
                case "megatouch xl":
                case "meritindustriesmegatouchxl":
                case "merit industries megatouch xl":
                    return RedumpSystem.MeritIndustriesMegaTouchXL;
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
                    return RedumpSystem.NamcoCapcomSystem256;
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
                case "system357":
                case "system 357":
                case "namcosystem357":
                case "namco system 357":
                    return RedumpSystem.NamcoSystem357;
                case "newjatrecdi":
                case "new jatre cdi":
                case "new jatre cd-i":
                    return RedumpSystem.NewJatreCDi;
                case "hrs":
                case "highratesytem":
                case "high rate system":
                case "nichibutsuhrs":
                case "nichibutsu hrs":
                case "nichibutsu high rate system":
                    return RedumpSystem.NichibutsuHighRateSystem;
                case "supercd":
                case "super cd":
                case "nichibutsuscd":
                case "nichibutsu scd":
                case "nichibutsusupercd":
                case "nichibutsu supercd":
                case "nichibutsu super cd":
                    return RedumpSystem.NichibutsuSuperCD;
                case "xrs":
                case "xratesystem":
                case "x-rate system":
                case "nichibutsuxrs":
                case "nichibutsu xrs":
                case "nichibutsu x-rate system":
                    return RedumpSystem.NichibutsuXRateSystem;
                case "panasonicm2":
                case "panasonic m2":
                    return RedumpSystem.PanasonicM2;
                case "photoplay":
                case "photoplaypc":
                case "photoplay pc":
                case "photoplay pc-based systems":
                    return RedumpSystem.PhotoPlayVarious;
                case "rawthrills":
                case "raw thrills":
                case "raw thrills pc-based systems":
                    return RedumpSystem.RawThrillsVarious;
                case "chihiro":
                case "segachihiro":
                case "sega chihiro":
                    return RedumpSystem.SegaChihiro;
                case "europar":
                case "europa-r":
                case "segaeuropar":
                case "sega europar":
                case "sega europa-r":
                    return RedumpSystem.SegaEuropaR;
                case "lindbergh":
                case "segalindbergh":
                case "sega lindbergh":
                    return RedumpSystem.SegaLindbergh;
                case "naomi":
                case "seganaomi":
                case "sega naomi":
                    return RedumpSystem.SegaNaomi;
                case "naomi2":
                case "naomi 2":
                case "seganaomi2":
                case "sega naomi 2":
                    return RedumpSystem.SegaNaomi2;
                case "nu":
                case "seganu":
                case "sega nu":
                    return RedumpSystem.SegaNu;
                case "ringedge":
                case "segaringedge":
                case "sega ringedge":
                    return RedumpSystem.SegaRingEdge;
                case "ringedge2":
                case "ringedge 2":
                case "segaringedge2":
                case "sega ringedge 2":
                    return RedumpSystem.SegaRingEdge2;
                case "ringwide":
                case "segaringwide":
                case "sega ringwide":
                    return RedumpSystem.SegaRingWide;
                case "stv":
                case "titanvideo":
                case "titan video":
                case "segatitanvideo":
                case "sega titan video":
                    return RedumpSystem.SegaTitanVideo;
                case "system32":
                case "system 32":
                case "segasystem32":
                case "sega system 32":
                    return RedumpSystem.SegaSystem32;
                case "cats":
                case "seibucats":
                case "seibu cats":
                case "seibu cats system":
                    return RedumpSystem.SeibuCATSSystem;
                case "quizard":
                case "tabaustriaquizard":
                case "tab-austria quizard":
                    return RedumpSystem.TABAustriaQuizard;
                case "tsumo":
                case "tsunamitsumo":
                case "tsunami tsumo":
                case "tsunami tsumo multi-game motion system":
                    return RedumpSystem.TsunamiTsuMoMultiGameMotionSystem;

                #endregion

                #region Others

                case "audio":
                case "audiocd":
                case "audio cd":
                    return RedumpSystem.AudioCD;
                case "bdvideo":
                case "bd-video":
                case "blurayvideo":
                case "bluray video":
                    return RedumpSystem.BDVideo;
                case "dvda":
                case "dvdaudio":
                case "dvd-audio":
                    return RedumpSystem.DVDAudio;
                case "dvd":
                case "dvdv":
                case "dvdvideo":
                case "dvd-video":
                    return RedumpSystem.DVDVideo;
                case "enhancedcd":
                case "enhanced cd":
                case "enhancedcdrom":
                case "enhanced cdrom":
                case "enhanced cd-rom":
                    return RedumpSystem.EnhancedCD;
                case "hddvd":
                case "hddvdv":
                case "hddvdvideo":
                case "hddvd-video":
                case "hd-dvd-video":
                    return RedumpSystem.HDDVDVideo;
                case "naviken":
                case "naviken21":
                case "naviken 2.1":
                case "navisoftnaviken":
                case "navisoft naviken":
                case "navisoftnaviken21":
                case "navisoft naviken 2.1":
                    return RedumpSystem.NavisoftNaviken21;
                case "palm":
                case "palmos":
                    return RedumpSystem.PalmOS;
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
                case "pocketpc":
                case "pocket pc":
                case "ppc":
                    return RedumpSystem.PocketPC;
                case "rainbow":
                case "rainbowdisc":
                case "rainbow disc":
                    return RedumpSystem.RainbowDisc;
                case "pl21":
                case "prologue21":
                case "prologue 21":
                case "segaprologue21":
                case "sega prologue21":
                case "sega prologue 21":
                    return RedumpSystem.SegaPrologue21MultimediaKaraokeSystem;
                case "sacd":
                case "superaudiocd":
                case "super audio cd":
                    return RedumpSystem.SuperAudioCD;
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

                #endregion

                default:
                    return null;
            }
        }

        #endregion
    
        #region System Category

        /// <summary>
        /// Get the string representation of the system category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public static string LongName(this SystemCategory? category) => AttributeHelper<SystemCategory?>.GetAttribute(category)?.LongName;

        #endregion

        #region Yes/No

        /// <summary>
        /// Get the string representation of the YesNo value
        /// </summary>
        /// <param name="yesno"></param>
        /// <returns></returns>
        public static string LongName(this YesNo yesno) => AttributeHelper<YesNo>.GetAttribute(yesno)?.LongName ?? "Yes/No";

        /// <summary>
        /// Get the YesNo enum value for a given string
        /// </summary>
        /// <param name="yesno">String value to convert</param>
        /// <returns>YesNo represented by the string, if possible</returns>
        public static YesNo? ToYesNo(string yesno)
        {
            switch (yesno.ToLowerInvariant())
            {
                case "no":
                    return YesNo.No;
                case "yes":
                    return YesNo.Yes;
                default:
                    return YesNo.NULL;
            }
        }

        #endregion
    }
}
