using RedumpLib.Attributes;

namespace RedumpLib.Data
{
    /// <summary>
    /// List of all disc categories
    /// </summary>
    public enum DiscCategory
    {
        [HumanReadable(LongName = "Games")]
        Games = 1,

        [HumanReadable(LongName = "Demos")]
        Demos = 2,

        [HumanReadable(LongName = "Video")]
        Video = 3,

        [HumanReadable(LongName = "Audio")]
        Audio = 4,

        [HumanReadable(LongName = "Multimedia")]
        Multimedia = 5,

        [HumanReadable(LongName = "Applications")]
        Applications = 6,

        [HumanReadable(LongName = "Coverdiscs")]
        Coverdiscs = 7,

        [HumanReadable(LongName = "Educational")]
        Educational = 8,

        [HumanReadable(LongName = "Bonus Discs")]
        BonusDiscs = 9,

        [HumanReadable(LongName = "Preproduction")]
        Preproduction = 10,

        [HumanReadable(LongName = "Add-Ons")]
        AddOns = 11,
    }

    /// <summary>
    /// Dump status
    /// </summary>
    public enum DumpStatus
    {
        BadDumpRed = 2,
        PossibleBadDumpYellow = 3,
        OriginalMediaBlue = 4,
        TwoOrMoHumanReadablesGreen = 5,
    }

    /// <summary>
    /// Determines what download type to initate
    /// </summary>
    public enum Feature
    {
        NONE,
        Site,
        WIP,
        Packs,
        User,
        Quicksearch,
    }

    /// <summary>
    /// List of all disc langauges
    /// </summary>
    public enum Language
    {
        [HumanReadable(LongName = "Afrikaans", ShortName = "afr")]
        Afrikaans,

        [HumanReadable(LongName = "Albanian", ShortName = "sqi")]
        Albanian,

        [HumanReadable(LongName = "Arabic", ShortName = "ara")]
        Arabic,

        [HumanReadable(LongName = "Basque", ShortName = "baq")]
        Basque,

        [HumanReadable(LongName = "Bulgarian", ShortName = "bul")]
        Bulgarian,

        [HumanReadable(LongName = "Catalan", ShortName = "cat")]
        Catalan,

        [HumanReadable(LongName = "Chinese", ShortName = "chi")]
        Chinese,

        [HumanReadable(LongName = "Croatian", ShortName = "hrv")]
        Croatian,

        [HumanReadable(LongName = "Czech", ShortName = "cze")]
        Czech,

        [HumanReadable(LongName = "Danish", ShortName = "dan")]
        Danish,

        [HumanReadable(LongName = "Dutch", ShortName = "dut")]
        Dutch,

        [HumanReadable(LongName = "English", ShortName = "eng")]
        English,

        [HumanReadable(LongName = "Estonian", ShortName = "est")]
        Estonian,

        [HumanReadable(LongName = "Finnish", ShortName = "fin")]
        Finnish,

        [HumanReadable(LongName = "French", ShortName = "fre")]
        French,

        [HumanReadable(LongName = "Gaelic", ShortName = "gla")]
        Gaelic,

        [HumanReadable(LongName = "German", ShortName = "ger")]
        German,

        [HumanReadable(LongName = "Greek", ShortName = "gre")]
        Greek,

        [HumanReadable(LongName = "Hebrew", ShortName = "heb")]
        Hebrew,

        [HumanReadable(LongName = "Hindi", ShortName = "hin")]
        Hindi,

        [HumanReadable(LongName = "Hungarian", ShortName = "hun")]
        Hungarian,

        [HumanReadable(LongName = "Indonesian", ShortName = "ind")]
        Indonesian,

        [HumanReadable(LongName = "Icelandic", ShortName = "isl")]
        Icelandic,

        [HumanReadable(LongName = "Italian", ShortName = "ita")]
        Italian,

        [HumanReadable(LongName = "Japanese", ShortName = "jap")]
        Japanese,

        [HumanReadable(LongName = "Korean", ShortName = "kor")]
        Korean,

        [HumanReadable(LongName = "Latin", ShortName = "lat")]
        Latin,

        [HumanReadable(LongName = "Latvian", ShortName = "lav")]
        Latvian,

        [HumanReadable(LongName = "Lithuanian", ShortName = "lit")]
        Lithuanian,

        [HumanReadable(LongName = "Macedonian", ShortName = "mkd")]
        Macedonian,

        [HumanReadable(LongName = "Norwegian", ShortName = "nor")]
        Norwegian,

        [HumanReadable(LongName = "Polish", ShortName = "pol")]
        Polish,

        [HumanReadable(LongName = "Portuguese", ShortName = "por")]
        Portuguese,

        [HumanReadable(LongName = "Punjabi", ShortName = "pan")]
        Punjabi,

        [HumanReadable(LongName = "Romanian", ShortName = "ron")]
        Romanian,

        [HumanReadable(LongName = "Russian", ShortName = "rus")]
        Russian,

        [HumanReadable(LongName = "Serbian", ShortName = "srp")]
        Serbian,

        [HumanReadable(LongName = "Slovak", ShortName = "slk")]
        Slovak,

        [HumanReadable(LongName = "Slovenian", ShortName = "slv")]
        Slovenian,

        [HumanReadable(LongName = "Spanish", ShortName = "spa")]
        Spanish,

        [HumanReadable(LongName = "Swedish", ShortName = "swe")]
        Swedish,

        [HumanReadable(LongName = "Tamil", ShortName = "tam")]
        Tamil,

        [HumanReadable(LongName = "Thai", ShortName = "tha")]
        Thai,

        [HumanReadable(LongName = "Turkish", ShortName = "tur")]
        Turkish,

        [HumanReadable(LongName = "Ukrainian", ShortName = "ukr")]
        Ukrainian,
    }

    /// <summary>
    /// All possible language selections
    /// </summary>
    public enum LanguageSelection
    {
        [HumanReadable(LongName = "Bios settings")]
        BiosSettings,

        [HumanReadable(LongName = "Language selector")]
        LanguageSelector,
        
        [HumanReadable(LongName = "Options menu")]
        OptionsMenu,
    }

    /// <summary>
    /// List of all known systems
    /// </summary>
    public enum RedumpSystem
    {
        // Special BIOS sets
        [System(LongName = "Microsoft Xbox (BIOS)", ShortName = "xbox-bios", HasDat = true)]
        MicrosoftXboxBIOS,

        [System(LongName = "Nintendo GameCube (BIOS)", ShortName = "gc-bios", HasDat = true)]
        NintendoGameCubeBIOS,

        [System(LongName = "Sony PlayStation (BIOS)", ShortName = "psx-bios", HasDat = true)]
        SonyPlayStationBIOS,

        [System(LongName = "Sony PlayStation 2 (BIOS)", ShortName = "ps2-bios", HasDat = true)]
        SonyPlayStation2BIOS,

        // Regular systems
        [System(LongName = "Acorn Archimedes", ShortName = "archcd", HasCues = true, HasDat = true)]
        AcornArchimedes,

        [System(LongName = "Apple Macintosh", ShortName = "mac", HasCues = true, HasDat = true)]
        AppleMacintosh,

        [System(LongName = "Atari Jaguar CD Interactive Multimedia System", ShortName = "ajcd", HasCues = true, HasDat = true)]
        AtariJaguarCDInteractiveMultimediaSystem,

        [System(LongName = "Audio CD", ShortName = "audio-cd", IsBanned = true, HasCues = true, HasDat = true)]
        AudioCD,

        [System(LongName = "Bandai Pippin", ShortName = "pippin", HasCues = true, HasDat = true)]
        BandaiPippin,

        [System(LongName = "Bandai Playdia Quick Interactive System", ShortName = "qis", HasCues = true, HasDat = true)]
        BandaiPlaydiaQuickInteractiveSystem,

        [System(LongName = "BD-Video", ShortName = "bd-video", IsBanned = true, HasDat = true)]
        BDVideo,

        [System(LongName = "Commodore Amiga CD", ShortName = "acd", HasCues = true, HasDat = true)]
        CommodoreAmigaCD,

        [System(LongName = "Commodore Amiga CD32", ShortName = "cd32", HasCues = true, HasDat = true)]
        CommodoreAmigaCD32,

        [System(LongName = "Commodore Amiga CDTV", ShortName = "cdtv", HasCues = true, HasDat = true)]
        CommodoreAmigaCDTV,

        [System(LongName = "DVD-Video", ShortName = "dvd-video", IsBanned = true, HasDat = true)]
        DVDVideo,

        [System(LongName = "Enhanced CD", ShortName = "enhanced-cd", IsBanned = true)]
        EnhancedCD,

        [System(LongName = "Fujitsu FM Towns series", ShortName = "fmt", HasCues = true, HasDat = true)]
        FujitsuFMTownsseries,

        [System(LongName = "funworld Photo Play", ShortName = "fpp", HasCues = true, HasDat = true)]
        funworldPhotoPlay,

        [System(LongName = "Hasbro VideoNow", ShortName = "hvn", IsBanned = true, HasCues = true, HasDat = true)]
        HasbroVideoNow,

        [System(LongName = "Hasbro VideoNow Color", ShortName = "hvnc", IsBanned = true, HasCues = true, HasDat = true)]
        HasbroVideoNowColor,

        [System(LongName = "Hasbro VideoNow Jr.", ShortName = "hvnjr", IsBanned = true, HasCues = true, HasDat = true)]
        HasbroVideoNowJr,

        [System(LongName = "Hasbro VideoNow XP", ShortName = "hvnxp", IsBanned = true, HasCues = true, HasDat = true)]
        HasbroVideoNowXP,

        [System(LongName = "HD DVD-Video", ShortName = "hddvd-video", IsBanned = true, HasDat = true)]
        HDDVDVideo,

        [System(LongName = "IBM PC compatible", ShortName = "pc", HasCues = true, HasDat = true, HasLsd = true, HasSbi = true)]
        IBMPCcompatible,

        [System(LongName = "Incredible Technologies Eagle", ShortName = "ite", HasCues = true, HasDat = true)]
        IncredibleTechnologiesEagle,

        [System(LongName = "Konami e-Amusement", ShortName = "kea", HasCues = true, HasDat = true)]
        KonamieAmusement,

        [System(LongName = "Konami FireBeat", ShortName = "kfb", HasCues = true, HasDat = true)]
        KonamiFireBeat,

        [System(LongName = "Konami M2", ShortName = "km2", IsBanned = true, HasCues = true, HasDat = true)]
        KonamiM2,

        [System(LongName = "Konami System 573", ShortName = "ks573")]
        KonamiSystem573,

        [System(LongName = "Konami System GV", ShortName = "ksgv", HasCues = true, HasDat = true)]
        KonamiSystemGV,

        [System(LongName = "Konami Twinkle", ShortName = "kt")]
        KonamiTwinkle,

        [System(LongName = "Mattel Fisher-Price iXL", ShortName = "ixl", HasCues = true, HasDat = true)]
        MattelFisherPriceiXL,

        [System(LongName = "Mattel HyperScan", ShortName = "hs", HasCues = true, HasDat = true)]
        MattelHyperScan,

        [System(LongName = "Memorex Visual Information System", ShortName = "vis", HasCues = true, HasDat = true)]
        MemorexVisualInformationSystem,

        [System(LongName = "Microsoft Xbox", ShortName = "xbox", HasCues = true, HasDat = true)]
        MicrosoftXbox,

        [System(LongName = "Microsoft Xbox 360", ShortName = "xbox360", IsBanned = true, HasCues = true, HasDat = true)]
        MicrosoftXbox360,

        [System(LongName = "Microsoft Xbox One", ShortName = "xboxone", IsBanned = true, HasDat = true)]
        MicrosoftXboxOne,

        //[System(LongName = "Microsoft Xbox Series X/S", ShortName = "xboxxs", IsBanned = true)]
        //MicrosoftXboxSeriesXandS, // TODO: Not available yet

        [System(LongName = "Namco · Sega · Nintendo Triforce", ShortName = "triforce", HasCues = true, HasDat = true, HasGdi = true)]
        NamcoSegaNintendoTriforce,

        [System(LongName = "Namco System 12", ShortName = "ns12")]
        NamcoSystem12,

        [System(LongName = "Namco System 246", ShortName = "ns246", HasCues = true, HasDat = true)]
        NamcoSystem246,

        [System(LongName = "Navisoft Naviken 2.1", ShortName = "navi21", IsBanned = true, HasCues = true, HasDat = true)]
        NavisoftNaviken21,

        [System(LongName = "NEC PC Engine CD & TurboGrafx CD", ShortName = "pce", HasCues = true, HasDat = true)]
        NECPCEngineCDTurboGrafxCD,

        [System(LongName = "NEC PC-88 series", ShortName = "pc-88", HasCues = true, HasDat = true)]
        NECPC88series,

        [System(LongName = "NEC PC-98 series", ShortName = "pc-98", HasCues = true, HasDat = true)]
        NECPC98series,

        [System(LongName = "NEC PC-FX & PC-FXGA", ShortName = "pc-fx", HasCues = true, HasDat = true)]
        NECPCFXPCFXGA,

        [System(LongName = "Nintendo GameCube", ShortName = "gc", HasDat = true)]
        NintendoGameCube,

        [System(LongName = "Nintendo Wii", ShortName = "wii", IsBanned = true, HasDat = true)]
        NintendoWii,

        [System(LongName = "Nintendo Wii U", ShortName = "wiiu", IsBanned = true, HasDat = true, HasKeys = true)]
        NintendoWiiU,

        [System(LongName = "Palm OS", ShortName = "palm", HasCues = true, HasDat = true)]
        PalmOS,

        [System(LongName = "Panasonic 3DO Interactive Multiplayer", ShortName = "3do", HasCues = true, HasDat = true)]
        Panasonic3DOInteractiveMultiplayer,

        [System(LongName = "Panasonic M2", ShortName = "m2", IsBanned = true, HasCues = true, HasDat = true)]
        PanasonicM2,

        [System(LongName = "Philips CD-i", ShortName = "cdi", HasCues = true, HasDat = true)]
        PhilipsCDi,

        [System(LongName = "Philips CD-i Digital Video", ShortName = "cdi-video", IsBanned = true)]
        PhilipsCDiDigitalVideo,

        [System(LongName = "Photo CD", ShortName = "photo-cd", HasCues = true, HasDat = true)]
        PhotoCD,

        [System(LongName = "PlayStation GameShark Updates", ShortName = "psxgs", HasCues = true, HasDat = true)]
        PlayStationGameSharkUpdates,

        [System(LongName = "Pocket PC", ShortName = "ppc", HasCues = true, HasDat = true)]
        PocketPC,

        [System(LongName = "Sega Chihiro", ShortName = "chihiro", HasCues = true, HasDat = true, HasGdi = true)]
        SegaChihiro,

        [System(LongName = "Sega Dreamcast", ShortName = "dc", HasCues = true, HasDat = true, HasGdi = true)]
        SegaDreamcast,

        [System(LongName = "Sega Lindbergh", ShortName = "lindbergh", HasDat = true)]
        SegaLindbergh,

        [System(LongName = "Sega Mega CD & Sega CD", ShortName = "mcd", HasCues = true, HasDat = true)]
        SegaMegaCDSegaCD,

        [System(LongName = "Sega Naomi", ShortName = "naomi", HasCues = true, HasDat = true, HasGdi = true)]
        SegaNaomi,

        [System(LongName = "Sega Naomi 2", ShortName = "naomi2", HasCues = true, HasDat = true, HasGdi = true)]
        SegaNaomi2,

        [System(LongName = "Sega Prologue 21 Multimedia Karaoke System", ShortName = "sp21", HasCues = true, HasDat = true)]
        SegaPrologue21MultimediaKaraokeSystem,

        [System(LongName = "Sega RingEdge", ShortName = "sre", IsBanned = true, HasDat = true)]
        SegaRingEdge,

        [System(LongName = "Sega RingEdge 2", ShortName = "sre2", IsBanned = true, HasDat = true)]
        SegaRingEdge2,

        [System(LongName = "Sega Saturn", ShortName = "ss", HasCues = true, HasDat = true)]
        SegaSaturn,

        [System(LongName = "Sega Titan Video", ShortName = "stv")]
        SegaTitanVideo,

        [System(LongName = "Sharp X68000", ShortName = "x86kcd", HasCues = true, HasDat = true)]
        SharpX68000,

        [System(LongName = "Neo Geo CD", ShortName = "ngcd", HasCues = true, HasDat = true)]
        SNKNeoGeoCD,

        [System(LongName = "Sony PlayStation", ShortName = "psx", HasCues = true, HasDat = true, HasLsd = true, HasSbi = true)]
        SonyPlayStation,

        [System(LongName = "Sony PlayStation 2", ShortName = "ps2", HasCues = true, HasDat = true)]
        SonyPlayStation2,

        [System(LongName = "Sony PlayStation 3", ShortName = "ps3", IsBanned = true, HasCues = true, HasDat = true, HasDkeys = true, HasKeys = true)]
        SonyPlayStation3,

        [System(LongName = "Sony PlayStation 4", ShortName = "ps4", IsBanned = true, HasDat = true)]
        SonyPlayStation4,

        //[System(LongName = "Sony PlayStation 5", ShortName = "ps5", IsBanned = true)]
        //SonyPlayStation5, // TODO: Not available yet

        [System(LongName = "Sony PlayStation Portable", ShortName = "psp", HasDat = true)]
        SonyPlayStationPortable,

        [System(LongName = "TAB-Austria Quizard", ShortName = "quizard", HasCues = true, HasDat = true)]
        TABAustriaQuizard,

        [System(LongName = "Tao iKTV", ShortName = "iktv")]
        TaoiKTV,

        [System(LongName = "Tomy Kiss-Site", ShortName = "ksite", HasCues = true, HasDat = true)]
        TomyKissSite,

        [System(LongName = "Video CD", ShortName = "vcd", IsBanned = true, HasCues = true, HasDat = true)]
        VideoCD,

        [System(LongName = "VM Labs NUON", ShortName = "nuon", HasDat = true)]
        VMLabsNUON,

        [System(LongName = "VTech V.Flash & V.Smile Pro", ShortName = "vflash", HasCues = true, HasDat = true)]
        VTechVFlashVSmilePro,

        [System(LongName = "ZAPiT Games Game Wave Family Entertainment System", ShortName = "gamewave", HasDat = true)]
        ZAPiTGamesGameWaveFamilyEntertainmentSystem,
    }

    /// <summary>
    /// List of all known regions
    /// </summary>
    public enum Region
    {
        [HumanReadable(LongName = "Argentina", ShortName = "Ar")]
        Argentina,

        [HumanReadable(LongName = "Asia", ShortName = "A")]
        Asia,

        [HumanReadable(LongName = "Asia, Europe", ShortName = "A,E")]
        AsiaEurope,

        [HumanReadable(LongName = "Asia, USA", ShortName = "A,U")]
        AsiaUSA,

        [HumanReadable(LongName = "Australia", ShortName = "Au")]
        Australia,

        [HumanReadable(LongName = "Australia, Germany", ShortName = "Au,G")]
        AustraliaGermany,

        [HumanReadable(LongName = "Australia, New Zealand", ShortName = "Au,Nz")]
        AustraliaNewZealand,

        [HumanReadable(LongName = "Austria", ShortName = "At")]
        Austria,

        [HumanReadable(LongName = "Austria, Switzerland", ShortName = "At,Ch")]
        AustriaSwitzerland,

        [HumanReadable(LongName = "Belgium", ShortName = "Be")]
        Belgium,

        [HumanReadable(LongName = "Belgium, Netherlands", ShortName = "Be,N")]
        BelgiumNetherlands,

        [HumanReadable(LongName = "Brazil", ShortName = "B")]
        Brazil,

        [HumanReadable(LongName = "Bulgaria", ShortName = "Bg")]
        Bulgaria,

        [HumanReadable(LongName = "Canada", ShortName = "Ca")]
        Canada,

        [HumanReadable(LongName = "China", ShortName = "C")]
        China,

        [HumanReadable(LongName = "Croatia", ShortName = "Hr")]
        Croatia,

        [HumanReadable(LongName = "Czech", ShortName = "Cz")]
        Czech,

        [HumanReadable(LongName = "Denmark", ShortName = "Dk")]
        Denmark,

        [HumanReadable(LongName = "Estonia", ShortName = "Ee")]
        Estonia,

        [HumanReadable(LongName = "Europe", ShortName = "E")]
        Europe,

        [HumanReadable(LongName = "Europe, Asia", ShortName = "E,A")]
        EuropeAsia,

        [HumanReadable(LongName = "Europe, Australia", ShortName = "E,Au")]
        EuropeAustralia,

        [HumanReadable(LongName = "Europe, Canada", ShortName = "E,Ca")]
        EuropeCanada,

        [HumanReadable(LongName = "Europe, Germany", ShortName = "E,G")]
        EuropeGermany,

        [HumanReadable(LongName = "Export", ShortName = "Ex")]
        Export,

        [HumanReadable(LongName = "Finland", ShortName = "Fi")]
        Finland,

        [HumanReadable(LongName = "France", ShortName = "F")]
        France,

        [HumanReadable(LongName = "France, Spain", ShortName = "F,S")]
        FranceSpain,

        [HumanReadable(LongName = "Germany", ShortName = "G")]
        Germany,

        [HumanReadable(LongName = "Greater China", ShortName = "GC")]
        GreaterChina,

        [HumanReadable(LongName = "Greece", ShortName = "Gr")]
        Greece,

        [HumanReadable(LongName = "Hungary", ShortName = "H")]
        Hungary,

        [HumanReadable(LongName = "Iceland", ShortName = "Is")]
        Iceland,

        [HumanReadable(LongName = "India", ShortName = "In")]
        India,

        [HumanReadable(LongName = "Ireland", ShortName = "Ie")]
        Ireland,

        [HumanReadable(LongName = "Israel", ShortName = "Il")]
        Israel,

        [HumanReadable(LongName = "Italy", ShortName = "I")]
        Italy,

        [HumanReadable(LongName = "Japan", ShortName = "J")]
        Japan,

        [HumanReadable(LongName = "Japan, Asia", ShortName = "J,A")]
        JapanAsia,

        [HumanReadable(LongName = "Japan, Europe", ShortName = "J,E")]
        JapanEurope,

        [HumanReadable(LongName = "Japan, Korea", ShortName = "J,K")]
        JapanKorea,

        [HumanReadable(LongName = "Japan, USA", ShortName = "J,U")]
        JapanUSA,

        [HumanReadable(LongName = "Korea", ShortName = "K")]
        Korea,

        [HumanReadable(LongName = "Latin America", ShortName = "LAm")]
        LatinAmerica,

        [HumanReadable(LongName = "Lithuania", ShortName = "Lt")]
        Lithuania,

        [HumanReadable(LongName = "Netherlands", ShortName = "N")]
        Netherlands,

        [HumanReadable(LongName = "New Zealand", ShortName = "Nz")]
        NewZealand,

        [HumanReadable(LongName = "Norway", ShortName = "No")]
        Norway,

        [HumanReadable(LongName = "Poland", ShortName = "P")]
        Poland,

        [HumanReadable(LongName = "Portugal", ShortName = "Pt")]
        Portugal,

        [HumanReadable(LongName = "Romania", ShortName = "Ro")]
        Romania,

        [HumanReadable(LongName = "Russia", ShortName = "R")]
        Russia,

        [HumanReadable(LongName = "Scandinavia", ShortName = "Sca")]
        Scandinavia,

        [HumanReadable(LongName = "Serbia", ShortName = "Rs")]
        Serbia,

        [HumanReadable(LongName = "Singapore", ShortName = "Sg")]
        Singapore,

        [HumanReadable(LongName = "Slovakia", ShortName = "Sk")]
        Slovakia,

        [HumanReadable(LongName = "South Africa", ShortName = "Za")]
        SouthAfrica,

        [HumanReadable(LongName = "Spain", ShortName = "S")]
        Spain,

        [HumanReadable(LongName = "Spain, Portugal", ShortName = "S,Pt")]
        SpainPortugal,

        [HumanReadable(LongName = "Sweden", ShortName = "Sw")]
        Sweden,

        [HumanReadable(LongName = "Switzerland", ShortName = "Ch")]
        Switzerland,

        [HumanReadable(LongName = "Taiwan", ShortName = "Tw")]
        Taiwan,

        [HumanReadable(LongName = "Thailand", ShortName = "Th")]
        Thailand,

        [HumanReadable(LongName = "Turkey", ShortName = "Tr")]
        Turkey,

        [HumanReadable(LongName = "United Arab Emirates", ShortName = "Ae")]
        UnitedArabEmirates,

        [HumanReadable(LongName = "UK", ShortName = "Uk")]
        UK,

        [HumanReadable(LongName = "UK, Australia", ShortName = "Uk,Au")]
        UKAustralia,

        [HumanReadable(LongName = "Ukraine", ShortName = "Ue")]
        Ukraine,

        [HumanReadable(LongName = "USA", ShortName = "U")]
        USA,

        [HumanReadable(LongName = "USA, Asia", ShortName = "U,A")]
        USAAsia,

        [HumanReadable(LongName = "USA, Australia", ShortName = "U,Au")]
        USAAustralia,

        [HumanReadable(LongName = "USA, Brazil", ShortName = "U,B")]
        USABrazil,

        [HumanReadable(LongName = "USA, Canada", ShortName = "U,Ca")]
        USACanada,

        [HumanReadable(LongName = "USA, Europe", ShortName = "U,E")]
        USAEurope,

        [HumanReadable(LongName = "USA, Germany", ShortName = "U,G")]
        USAGermany,

        [HumanReadable(LongName = "USA, Japan", ShortName = "U,J")]
        USAJapan,

        [HumanReadable(LongName = "USA, Korea", ShortName = "U,K")]
        USAKorea,

        [HumanReadable(LongName = "World", ShortName = "W")]
        World,
    }
}
