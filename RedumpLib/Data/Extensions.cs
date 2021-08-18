using RedumpLib.Attributes;

namespace RedumpLib.Data
{
    /// <summary>
    /// Information pertaining to Redump systems
    /// </summary>
    public static class Extensions
    {
        #region Category

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
                case "hddvd-video":
                case "hd dvd video":
                case "hd-dvd video":
                case "hd dvd-video":
                case "hd-dvd-video":
                    return RedumpSystem.HDDVDVideo;
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
                case "cdi-video":
                case "cdi video":
                case "cd-i-video":
                case "cd-i video":
                case "cdidigitalvideo":
                case "cdi digital video":
                case "cd-i digital video":
                case "philipscdivideo":
                case "philips cdi-video":
                case "philips cdi video":
                case "philips cd-ivideo":
                case "philips cd-i-video":
                case "philips cd-i video":
                case "philipscdidigitalvideo":
                case "philips cdi digital video":
                case "philips cd-idigitalvideo":
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
                case "ppc":
                case "pocketpc":
                case "pocket pc":
                    return RedumpSystem.PocketPC;
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
                case "sp21":
                case "prologue21":
                case "prologue 21":
                case "segaprologue21":
                case "sega prologue21":
                case "sega prologue 21":
                case "segaprologue21multimediakaraokesystem":
                case "sega prologue21 multimedia karaoke system":
                case "sega prologue 21 multimedia karaoke system":
                    return RedumpSystem.SegaPrologue21MultimediaKaraokeSystem;
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
