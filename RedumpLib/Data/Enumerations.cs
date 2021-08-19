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
    /// List of all disc types
    /// </summary>
    public enum DiscType
    {
        NONE = 0,

        [HumanReadable(LongName = "BD-25")]
        BD25,

        [HumanReadable(LongName = "BD-50")]
        BD50,

        [HumanReadable(LongName = "CD")]
        CD,

        [HumanReadable(LongName = "DVD-5")]
        DVD5,

        [HumanReadable(LongName = "DVD-9")]
        DVD9,
        
        [HumanReadable(LongName = "GD-ROM")]
        GDROM,
        
        [HumanReadable(LongName = "HD-DVD SL")]
        HDDVDSL,
        
        [HumanReadable(LongName = "MIL-CD")]
        MILCD,
        
        [HumanReadable(LongName = "Nintendo GameCube Game Disc")]
        NintendoGameCubeGameDisc,
        
        [HumanReadable(LongName = "Nintendo Wii Optical Disc SL")]
        NintendoWiiOpticalDiscSL,

        [HumanReadable(LongName = "Nintendo Wii Optical Disc DL")]
        NintendoWiiOpticalDiscDL,
        
        [HumanReadable(LongName = "Nintendo Wii U Optical Disc SL")]
        NintendoWiiUOpticalDiscSL,
        
        [HumanReadable(LongName = "UMD SL")]
        UMDSL,
        
        [HumanReadable(LongName = "UMD DL")]
        UMDDL,
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
    /// TODO: Remove marker items
    public enum RedumpSystem
    {
        #region BIOS Sets

        [System(LongName = "Microsoft Xbox (BIOS)", ShortName = "xbox-bios", HasDat = true)]
        MicrosoftXboxBIOS,

        [System(LongName = "Nintendo GameCube (BIOS)", ShortName = "gc-bios", HasDat = true)]
        NintendoGameCubeBIOS,

        [System(LongName = "Sony PlayStation (BIOS)", ShortName = "psx-bios", HasDat = true)]
        SonyPlayStationBIOS,

        [System(LongName = "Sony PlayStation 2 (BIOS)", ShortName = "ps2-bios", HasDat = true)]
        SonyPlayStation2BIOS,

        #endregion

        #region Disc-Based Consoles

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Atari Jaguar CD Interactive Multimedia System", ShortName = "ajcd", HasCues = true, HasDat = true)]
        AtariJaguarCDInteractiveMultimediaSystem,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Bandai Playdia Quick Interactive System", ShortName = "qis", HasCues = true, HasDat = true)]
        BandaiPlaydiaQuickInteractiveSystem,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Bandai Pippin", ShortName = "pippin", HasCues = true, HasDat = true)]
        BandaiPippin,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Commodore Amiga CD32", ShortName = "cd32", HasCues = true, HasDat = true)]
        CommodoreAmigaCD32,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Commodore Amiga CDTV", ShortName = "cdtv", HasCues = true, HasDat = true)]
        CommodoreAmigaCDTV,

        [System(Category = SystemCategory.DiscBasedConsole, Available = false, LongName = "Envizions EVO Smart Console")]
        EnvizionsEVOSmartConsole,

        [System(Category = SystemCategory.DiscBasedConsole, Available = false, LongName = "Fujitsu FM Towns Marty")]
        FujitsuFMTownsMarty,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Hasbro VideoNow", ShortName = "hvn", IsBanned = true, HasCues = true, HasDat = true)]
        HasbroVideoNow,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Hasbro VideoNow Color", ShortName = "hvnc", IsBanned = true, HasCues = true, HasDat = true)]
        HasbroVideoNowColor,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Hasbro VideoNow Jr.", ShortName = "hvnjr", IsBanned = true, HasCues = true, HasDat = true)]
        HasbroVideoNowJr,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Hasbro VideoNow XP", ShortName = "hvnxp", IsBanned = true, HasCues = true, HasDat = true)]
        HasbroVideoNowXP,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Mattel Fisher-Price iXL", ShortName = "ixl", HasCues = true, HasDat = true)]
        MattelFisherPriceiXL,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Mattel HyperScan", ShortName = "hs", HasCues = true, HasDat = true)]
        MattelHyperScan,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Microsoft Xbox", ShortName = "xbox", HasCues = true, HasDat = true)]
        MicrosoftXbox,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Microsoft Xbox 360", ShortName = "xbox360", IsBanned = true, HasCues = true, HasDat = true)]
        MicrosoftXbox360,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Microsoft Xbox One", ShortName = "xboxone", IsBanned = true, HasDat = true)]
        MicrosoftXboxOne,

        [System(Category = SystemCategory.DiscBasedConsole, Available = false, LongName = "Microsoft Xbox Series X/S")]
        MicrosoftXboxSeriesXS,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Memorex Visual Information System", ShortName = "vis", HasCues = true, HasDat = true)]
        MemorexVisualInformationSystem,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "NEC PC Engine CD & TurboGrafx CD", ShortName = "pce", HasCues = true, HasDat = true)]
        NECPCEngineCDTurboGrafxCD,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "NEC PC-FX & PC-FXGA", ShortName = "pc-fx", HasCues = true, HasDat = true)]
        NECPCFXPCFXGA,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Nintendo GameCube", ShortName = "gc", HasDat = true)]
        NintendoGameCube,

        [System(Category = SystemCategory.DiscBasedConsole, Available = false, LongName = "Nintendo-Sony Super NES CD-ROM System")]
        NintendoSonySuperNESCDROMSystem,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Nintendo Wii", ShortName = "wii", IsBanned = true, HasDat = true)]
        NintendoWii,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Nintendo Wii U", ShortName = "wiiu", IsBanned = true, HasDat = true, HasKeys = true)]
        NintendoWiiU,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Panasonic 3DO Interactive Multiplayer", ShortName = "3do", HasCues = true, HasDat = true)]
        Panasonic3DOInteractiveMultiplayer,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Philips CD-i", ShortName = "cdi", HasCues = true, HasDat = true)]
        PhilipsCDi,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Philips CD-i Digital Video", ShortName = "cdi-video", IsBanned = true)]
        PhilipsCDiDigitalVideo,

        [System(Category = SystemCategory.DiscBasedConsole, Available = false, LongName = "Pioneer LaserActive")]
        PioneerLaserActive,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Sega Dreamcast", ShortName = "dc", HasCues = true, HasDat = true, HasGdi = true)]
        SegaDreamcast,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Sega Mega CD & Sega CD", ShortName = "mcd", HasCues = true, HasDat = true)]
        SegaMegaCDSegaCD,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Sega Saturn", ShortName = "ss", HasCues = true, HasDat = true)]
        SegaSaturn,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Neo Geo CD", ShortName = "ngcd", HasCues = true, HasDat = true)]
        SNKNeoGeoCD,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Sony PlayStation", ShortName = "psx", HasCues = true, HasDat = true, HasLsd = true, HasSbi = true)]
        SonyPlayStation,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Sony PlayStation 2", ShortName = "ps2", HasCues = true, HasDat = true)]
        SonyPlayStation2,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Sony PlayStation 3", ShortName = "ps3", IsBanned = true, HasCues = true, HasDat = true, HasDkeys = true, HasKeys = true)]
        SonyPlayStation3,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Sony PlayStation 4", ShortName = "ps4", IsBanned = true, HasDat = true)]
        SonyPlayStation4,

        [System(Category = SystemCategory.DiscBasedConsole, Available = false, LongName = "Sony PlayStation 5")]
        SonyPlayStation5,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "Sony PlayStation Portable", ShortName = "psp", HasDat = true)]
        SonyPlayStationPortable,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "VM Labs NUON", ShortName = "nuon", HasDat = true)]
        VMLabsNUON,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "VTech V.Flash & V.Smile Pro", ShortName = "vflash", HasCues = true, HasDat = true)]
        VTechVFlashVSmilePro,

        [System(Category = SystemCategory.DiscBasedConsole, LongName = "ZAPiT Games Game Wave Family Entertainment System", ShortName = "gamewave", HasDat = true)]
        ZAPiTGamesGameWaveFamilyEntertainmentSystem,

        // End of console section delimiter
        MarkerDiscBasedConsoleEnd,

        #endregion

        #region Cartridge-Based and Other Consoles

        /*
        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Amstrad GX-4000")]
        AmstradGX4000,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "APF Microcomputer System")]
        APFMicrocomputerSystem,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Atari 2600 & VCS")]
        Atari2600VCS,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Atari 5200")]
        Atari5200,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Atari 7800")]
        Atari7800,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Atari Jaguar")]
        AtariJaguar,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Atari XEGS")]
        AtariXEGS,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Audiosonic 1292 Advanced Programmable Video System")]
        Audiosonic1292AdvancedProgrammableVideoSystem,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Bally Astrocade")]
        BallyAstrocade,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Bit Corporation Dina")]
        BitCorporationDina,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Casio Loopy")]
        CasioLoopy,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Casio PV-1000")]
        CasioPV1000,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Commodore 64 Games System")]
        Commodore64GamesSystem,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Daewoo Electronics Zemmix")]
        DaewooElectronicsZemmix,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Emerson Arcadia 2001")]
        EmersonArcadia2001,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Epoch Cassette Vision")]
        EpochCassetteVision,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Epoch Super Cassette Vision")]
        EpochSuperCassetteVision,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Fairchild Channel F")]
        FairchildChannelF,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Funtech Super A'Can")]
        FuntechSuperACan,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "GCE Vectrex")]
        GCEVectrex,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Heber BBC Bridge Companion")]
        HeberBBCBridgeCompanion,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Interton VC-4000")]
        IntertonVC4000,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "JungleTac Vii")]
        JungleTacVii,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "LeapFrog ClickStart")]
        LeapFrogClickStart,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "LJN VideoArt")]
        LJNVideoArt,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Magnavox Odyssey 2")]
        MagnavoxOdyssey2,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Mattel Intellivision")]
        MattelIntellivision,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "NEC PC Engine & TurboGrafx-16")]
        NECPCEngineTurboGrafx16,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Nichibutsu MyVision")]
        NichibutsuMyVision,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Nintendo 64")]
        Nintendo64,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Nintendo 64DD")]
        Nintendo64DD,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Nintendo Famicom & Nintendo Entertainment System")]
        NintendoFamicomNintendoEntertainmentSystem,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Nintendo Famicom Disk System")]
        NintendoFamicomDiskSystem,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Nintendo Super Famicom & Super Nintendo Entertainment System")]
        NintendoSuperFamicomSuperNintendoEntertainmentSystem,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Nintendo Switch")]
        NintendoSwitch,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Philips Videopac+ & G7400")]
        PhilipsVideopacPlusG7400,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "RCA Studio-II")]
        RCAStudioII,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Sega 32X")]
        Sega32X,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Sega Mark III & Master System")]
        SegaMarkIIIMasterSystem,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Sega MegaDrive & Genesis")]
        SegaMegaDriveGenesis,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Sega SG-1000")]
        SegaSG1000,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "SNK NeoGeo")]
        SNKNeoGeo,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "SSD COMPANY LIMITED XaviXPORT")]
        SSDCOMPANYLIMITEDXaviXPORT,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "ViewMaster Interactive Vision")]
        ViewMasterInteractiveVision,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "V.Tech CreatiVision")]
        VTechCreatiVision,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "V.Tech V.Smile")]
        VTechVSmile,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "V.Tech Socrates")]
        VTechSocrates,

        [System(Category = SystemCategory.OtherConsole, Available = false, LongName = "Worlds of Wonder ActionMax")]
        WorldsOfWonderActionMax,

        // End of other console delimiter
        MarkerOtherConsoleEnd,
        */

        #endregion

        #region Computers

        [System(Category = SystemCategory.Computer, LongName = "Acorn Archimedes", ShortName = "archcd", HasCues = true, HasDat = true)]
        AcornArchimedes,

        [System(Category = SystemCategory.Computer, LongName = "Apple Macintosh", ShortName = "mac", HasCues = true, HasDat = true)]
        AppleMacintosh,

        [System(Category = SystemCategory.Computer, LongName = "Commodore Amiga CD", ShortName = "acd", HasCues = true, HasDat = true)]
        CommodoreAmigaCD,

        [System(Category = SystemCategory.Computer, LongName = "Fujitsu FM Towns series", ShortName = "fmt", HasCues = true, HasDat = true)]
        FujitsuFMTownsseries,

        [System(Category = SystemCategory.Computer, LongName = "IBM PC compatible", ShortName = "pc", HasCues = true, HasDat = true, HasLsd = true, HasSbi = true)]
        IBMPCcompatible,
        
        [System(Category = SystemCategory.Computer, LongName = "NEC PC-88 series", ShortName = "pc-88", HasCues = true, HasDat = true)]
        NECPC88series,

        [System(Category = SystemCategory.Computer, LongName = "NEC PC-98 series", ShortName = "pc-98", HasCues = true, HasDat = true)]
        NECPC98series,

        [System(Category = SystemCategory.Computer, LongName = "Sharp X68000", ShortName = "x86kcd", HasCues = true, HasDat = true)]
        SharpX68000,

        // End of computer section delimiter
        MarkerComputerEnd,

        #endregion

        #region Arcade

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Amiga CUBO CD32")]
        AmigaCUBOCD32,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "American Laser Games 3DO")]
        AmericanLaserGames3DO,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Atari 3DO")]
        Atari3DO,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Atronic")]
        Atronic,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "AUSCOM System 1")]
        AUSCOMSystem1,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Bally Game Magic")]
        BallyGameMagic,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Capcom CP System III")]
        CapcomCPSystemIII,

        [System(Category = SystemCategory.Arcade, LongName = "funworld Photo Play", ShortName = "fpp", HasCues = true, HasDat = true)]
        funworldPhotoPlay,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Global VR PC-based Systems")]
        GlobalVRVarious,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Global VR Vortek")]
        GlobalVRVortek,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Global VR Vortek V3")]
        GlobalVRVortekV3,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "ICE PC-based Hardware")]
        ICEPCHardware,

        [System(Category = SystemCategory.Arcade, LongName = "Incredible Technologies Eagle", ShortName = "ite", HasCues = true, HasDat = true)]
        IncredibleTechnologiesEagle,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Incredible Technologies PC-based Systems")]
        IncredibleTechnologiesVarious,

        [System(Category = SystemCategory.Arcade, LongName = "Konami e-Amusement", ShortName = "kea", HasCues = true, HasDat = true)]
        KonamieAmusement,

        [System(Category = SystemCategory.Arcade, LongName = "Konami FireBeat", ShortName = "kfb", HasCues = true, HasDat = true)]
        KonamiFireBeat,

        [System(Category = SystemCategory.Arcade, LongName = "Konami M2", ShortName = "km2", IsBanned = true, HasCues = true, HasDat = true)]
        KonamiM2,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Konami Python")]
        KonamiPython,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Konami Python 2")]
        KonamiPython2,

        [System(Category = SystemCategory.Arcade, LongName = "Konami System 573", ShortName = "ks573")]
        KonamiSystem573,

        [System(Category = SystemCategory.Arcade, LongName = "Konami System GV", ShortName = "ksgv", HasCues = true, HasDat = true)]
        KonamiSystemGV,

        [System(Category = SystemCategory.Arcade, LongName = "Konami Twinkle", ShortName = "kt")]
        KonamiTwinkle,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Konami PC-based Systems")]
        KonamiVarious,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Merit Industries Boardwalk")]
        MeritIndustriesBoardwalk,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Merit Industries MegaTouch Force")]
        MeritIndustriesMegaTouchForce,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Merit Industries MegaTouch ION")]
        MeritIndustriesMegaTouchION,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Merit Industries MegaTouch Maxx")]
        MeritIndustriesMegaTouchMaxx,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Merit Industries MegaTouch XL")]
        MeritIndustriesMegaTouchXL,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Namco · Capcom System 256/Super System 256")]
        NamcoCapcomSystem256,

        [System(Category = SystemCategory.Arcade, LongName = "Namco · Sega · Nintendo Triforce", ShortName = "triforce", HasCues = true, HasDat = true, HasGdi = true)]
        NamcoSegaNintendoTriforce,

        [System(Category = SystemCategory.Arcade, LongName = "Namco System 12", ShortName = "ns12")]
        NamcoSystem12,

        [System(Category = SystemCategory.Arcade, LongName = "Namco System 246", ShortName = "ns246", HasCues = true, HasDat = true)]
        NamcoSystem246,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Namco System 357")]
        NamcoSystem357,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "New Jatre CD-i")]
        NewJatreCDi,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Nichibutsu High Rate System")]
        NichibutsuHighRateSystem,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Nichibutsu Super CD")]
        NichibutsuSuperCD,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Nichibutsu X-Rate System")]
        NichibutsuXRateSystem,

        [System(Category = SystemCategory.Arcade, LongName = "Panasonic M2", ShortName = "m2", IsBanned = true, HasCues = true, HasDat = true)]
        PanasonicM2,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "PhotoPlay PC-based Systems")]
        PhotoPlayVarious,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Raw Thrills PC-based Systems")]
        RawThrillsVarious,

        [System(Category = SystemCategory.Arcade, LongName = "Sega Chihiro", ShortName = "chihiro", HasCues = true, HasDat = true, HasGdi = true)]
        SegaChihiro,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Sega Europa-R")]
        SegaEuropaR,

        [System(Category = SystemCategory.Arcade, LongName = "Sega Lindbergh", ShortName = "lindbergh", HasDat = true)]
        SegaLindbergh,

        [System(Category = SystemCategory.Arcade, LongName = "Sega Naomi", ShortName = "naomi", HasCues = true, HasDat = true, HasGdi = true)]
        SegaNaomi,

        [System(Category = SystemCategory.Arcade, LongName = "Sega Naomi 2", ShortName = "naomi2", HasCues = true, HasDat = true, HasGdi = true)]
        SegaNaomi2,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Sega Nu")]
        SegaNu,

        [System(Category = SystemCategory.Arcade, LongName = "Sega RingEdge", ShortName = "sre", IsBanned = true, HasDat = true)]
        SegaRingEdge,

        [System(Category = SystemCategory.Arcade, LongName = "Sega RingEdge 2", ShortName = "sre2", IsBanned = true, HasDat = true)]
        SegaRingEdge2,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Sega RingWide")]
        SegaRingWide,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Sega System 32")]
        SegaSystem32,

        [System(Category = SystemCategory.Arcade, LongName = "Sega Titan Video", ShortName = "stv")]
        SegaTitanVideo,

        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Seibu CATS System")]
        SeibuCATSSystem,

        [System(Category = SystemCategory.Arcade, LongName = "TAB-Austria Quizard", ShortName = "quizard", HasCues = true, HasDat = true)]
        TABAustriaQuizard,
        
        [System(Category = SystemCategory.Arcade, Available = false, LongName = "Tsunami TsuMo Multi-Game Motion System")]
        TsunamiTsuMoMultiGameMotionSystem,

        // End of arcade section delimiter
        MarkerArcadeEnd,

        #endregion

        #region Other

        [System(Category = SystemCategory.Other, LongName = "Audio CD", ShortName = "audio-cd", IsBanned = true, HasCues = true, HasDat = true)]
        AudioCD,

        [System(Category = SystemCategory.Other, LongName = "BD-Video", ShortName = "bd-video", IsBanned = true, HasDat = true)]
        BDVideo,

        [System(Category = SystemCategory.Other, Available = false, LongName = "DVD-Audio")]
        DVDAudio,

        [System(Category = SystemCategory.Other, LongName = "DVD-Video", ShortName = "dvd-video", IsBanned = true, HasDat = true)]
        DVDVideo,

        [System(Category = SystemCategory.Other, LongName = "Enhanced CD", ShortName = "enhanced-cd", IsBanned = true)]
        EnhancedCD,

        [System(Category = SystemCategory.Other, LongName = "HD DVD-Video", ShortName = "hddvd-video", IsBanned = true, HasDat = true)]
        HDDVDVideo,

        [System(Category = SystemCategory.Other, LongName = "Navisoft Naviken 2.1", ShortName = "navi21", IsBanned = true, HasCues = true, HasDat = true)]
        NavisoftNaviken21,

        [System(Category = SystemCategory.Other, LongName = "Palm OS", ShortName = "palm", HasCues = true, HasDat = true)]
        PalmOS,

        [System(Category = SystemCategory.Other, LongName = "Photo CD", ShortName = "photo-cd", HasCues = true, HasDat = true)]
        PhotoCD,

        [System(Category = SystemCategory.Other, LongName = "PlayStation GameShark Updates", ShortName = "psxgs", HasCues = true, HasDat = true)]
        PlayStationGameSharkUpdates,

        [System(Category = SystemCategory.Other, LongName = "Pocket PC", ShortName = "ppc", HasCues = true, HasDat = true)]
        PocketPC,

        [System(Category = SystemCategory.Other, Available = false, LongName = "Rainbow Disc")]
        RainbowDisc,

        [System(Category = SystemCategory.Other, LongName = "Sega Prologue 21 Multimedia Karaoke System", ShortName = "sp21", HasCues = true, HasDat = true)]
        SegaPrologue21MultimediaKaraokeSystem,

        [System(Category = SystemCategory.Other, Available = false, LongName = "Super Audio CD")]
        SuperAudioCD,

        [System(Category = SystemCategory.Other, LongName = "Tao iKTV", ShortName = "iktv")]
        TaoiKTV,

        [System(Category = SystemCategory.Other, LongName = "Tomy Kiss-Site", ShortName = "ksite", HasCues = true, HasDat = true)]
        TomyKissSite,

        [System(Category = SystemCategory.Other, LongName = "Video CD", ShortName = "vcd", IsBanned = true, HasCues = true, HasDat = true)]
        VideoCD,

        // End of other section delimiter
        MarkerOtherEnd,

        #endregion
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

    /// <summary>
    /// List of system categories
    /// </summary>
    public enum SystemCategory
    {
        NONE = 0,

        [HumanReadable(LongName = "Disc-Based Consoles")]
        DiscBasedConsole,

        [HumanReadable(LongName = "Other Consoles")]
        OtherConsole,

        [HumanReadable(LongName = "Computers")]
        Computer,

        [HumanReadable(LongName = "Arcade")]
        Arcade,

        [HumanReadable(LongName = "Other")]
        Other,
    };

    /// <summary>
    /// Generic yes/no values
    /// </summary>
    public enum YesNo
    {
        [HumanReadable(LongName = "Yes/No")]
        NULL = 0,

        [HumanReadable(LongName = "No")]
        No = 1,

        [HumanReadable(LongName = "Yes")]
        Yes = 2,
    }
}
