namespace DICUI.Data
{
    /// <summary>
    /// Category for Redump
    /// </summary>
    public enum Category
    {
        Games = 1,
        Demos = 2,
        Video = 3,
        Audio = 4,
        Multimedia = 5,
        Applications = 6,
        Coverdiscs = 7,
        Educational = 8,
        BonusDiscs = 9,
        Preproduction = 10,
        AddOns = 11,
    }

    /// <summary>
    /// Supported DIC commands
    /// </summary>
    public enum DICCommand
    {
        NONE = 0,
        Audio,
        BluRay,
        Close,
        CompactDisc,
        Data,
        DigitalVideoDisc,
        Disk,
        DriveSpeed,
        Eject,
        Floppy,
        GDROM,
        MDS,
        Merge,
        Reset,
        SACD,
        Start,
        Stop,
        Sub,
        Swap,
        XBOX,
        XBOXSwap,
        XGD2Swap,
        XGD3Swap,
    }

    /// <summary>
    /// Supported DIC flags
    /// </summary>
    public enum DICFlag
    {
        NONE = 0,
        AddOffset,
        AMSF,
        AtariJaguar,
        BEOpcode,
        C2Opcode,
        CopyrightManagementInformation,
        D8Opcode,
        DisableBeep,
        ForceUnitAccess,
        MCN,
        MultiSession,
        NoFixSubP,
        NoFixSubQ,
        NoFixSubQLibCrypt,
        NoFixSubRtoW,
        NoFixSubQSecuROM,
        NoSkipSS,
        Raw,
        Reverse,
        ScanAntiMod,
        ScanFileProtect,
        ScanSectorProtect,
        SeventyFour,
        SkipSector,
        SubchannelReadLevel,
        VideoNow,
        VideoNowColor,
    }

    /// <summary>
    /// Drive type for dumpign
    /// </summary>
    public enum InternalDriveType
    {
        Optical,
        Floppy,
        HardDisk,
        Removable,
    }

    /// <summary>
    /// Dump status for Redump
    /// </summary>
    public enum DumpStatus
    {
        BadDumpRed = 2,
        PossibleBadDumpYellow = 3,
        OriginalMediaBlue = 4,
        TwoOrMoreDumpsGreen = 5,
    }

    /// <summary>
    /// Known systems
    /// </summary>
    public enum KnownSystem
    {
        NONE = 0,

        #region Disc-Based Consoles

        AtariJaguarCD,
        BandaiPlaydiaQuickInteractiveSystem,
        BandaiApplePippin,
        CommodoreAmigaCD32,
        CommodoreAmigaCDTV,
        EnvizionsEVOSmartConsole,
        FujitsuFMTownsMarty,
        HasbroVideoNow,
        HasbroVideoNowColor,
        HasbroVideoNowJr,
        HasbroVideoNowXP,
        MattelHyperscan,
        MicrosoftXBOX,
        MicrosoftXBOX360,
        MicrosoftXBOXOne,
        NECPCEngineTurboGrafxCD,
        NECPCFX,
        NintendoGameCube,
        NintendoSonySuperNESCDROMSystem,
        NintendoWii,
        NintendoWiiU,
        Panasonic3DOInteractiveMultiplayer, // The 3DO Company 3DO Interactive Multiplayer
        PhilipsCDi,
        PioneerLaserActive,
        SegaCDMegaCD,
        SegaDreamcast,
        SegaSaturn,
        SNKNeoGeoCD,
        SonyPlayStation,
        SonyPlayStation2,
        SonyPlayStation3,
        SonyPlayStation4,
        SonyPlayStationPortable,
        TandyMemorexVisualInformationSystem,
        VMLabsNuon,
        VTechVFlashVSmilePro,
        ZAPiTGamesGameWaveFamilyEntertainmentSystem,

        MarkerDiscBasedConsoleEnd,

        #endregion

        #region Cartridge-Based and Other Consoles

        /*
        AmstradGX4000,
        APFMicrocomputerSystem,
        Atari2600VCS,
        Atari5200,
        Atari7800,
        AtariJaguar,
        AtariXEVideoGameSystem,
        Audiosonic1292AdvancedProgrammableVideoSystem,
        BallyAstrocade,
        BitCorporationDina,
        CasioLoopy,
        CasioPV1000,
        Commodore64GamesSystem,
        DaewooElectronicsZemmix,
        EmersonArcadia2001,
        EpochCassetteVision,
        EpochSuperCassetteVision,
        FairchildChannelF,
        FuntechSuperACan,
        GeneralConsumerElectricVectrex,
        HeberBBCBridgeCompanion,
        IntertonVC4000,
        JungleTacVii,
        LeapFrogClickStart,
        LJNVideoArt,
        MagnavoxOdyssey2,
        MattelIntellivision,
        NECPCEngineTurboGrafx16,
        NichibutsuMyVision,
        Nintendo64,
        Nintendo64DD,
        NintendoFamilyComputerNintendoEntertainmentSystem,
        NintendoFamilyComputerDiskSystem,
        NintendoSuperFamicomSuperNintendoEntertainmentSystem,
        NintendoSwitch,
        PhilipsVideopacPlusG7400,
        RCAStudioII,
        Sega32X,
        SegaMarkIIIMasterSystem,
        SegaMegaDriveGenesis,
        SegaSG1000,
        SNKNeoGeo,
        SSDCOMPANYLIMITEDXaviXPORT,
        ViewMasterInteractiveVision,
        VTechCreatiVision,
        VTechVSmile,
        VTechSocrates,
        WorldsOfWonderActionMax,

        MarkerOtherConsoleEnd,
        */

        #endregion

        #region Computers

        AcornArchimedes,
        AppleMacintosh,
        CommodoreAmiga,
        FujitsuFMTowns,
        IBMPCCompatible,
        NECPC88,
        NECPC98,
        SharpX68000,

        MarkerComputerEnd,

        #endregion

        #region Arcade

        AmigaCUBOCD32,
        AmericanLaserGames3DO,
        Atari3DO,
        Atronic,
        AUSCOMSystem1,
        BallyGameMagic,
        CapcomCPSystemIII,
        GlobalVRVarious,
        GlobalVRVortek,
        GlobalVRVortekV3,
        ICEPCHardware,
        IncredibleTechnologiesEagle,
        IncredibleTechnologiesVarious,
        KonamieAmusement,
        KonamiFirebeat,
        KonamiGVSystem,
        KonamiM2,
        KonamiPython,
        KonamiPython2,
        KonamiSystem573,
        KonamiTwinkle,
        KonamiVarious,
        MeritIndustriesBoardwalk,
        MeritIndustriesMegaTouchForce,
        MeritIndustriesMegaTouchION,
        MeritIndustriesMegaTouchMaxx,
        MeritIndustriesMegaTouchXL,
        NamcoCapcomSystem256,
        NamcoCapcomTaitoSystem246,
        NamcoSegaNintendoTriforce,
        NamcoSystem12,
        NamcoSystem357,
        NewJatreCDi,
        NichibutsuHighRateSystem,
        NichibutsuSuperCD,
        NichibutsuXRateSystem,
        PanasonicM2,
        PhotoPlayVarious,
        RawThrillsVarious,
        SegaChihiro,
        SegaEuropaR,
        SegaLindbergh,
        SegaNaomi,
        SegaNaomi2,
        SegaNu,
        SegaRingEdge,
        SegaRingEdge2,
        SegaRingWide,
        SegaTitanVideo,
        SegaSystem32,
        SeibuCATSSystem,
        TABAustriaQuizard,
        TsunamiTsuMoMultiGameMotionSystem,

        MarkerArcadeEnd,

        #endregion

        #region Other

        AudioCD,
        BDVideo,
        DVDVideo,
        EnhancedCD,
        HDDVDVideo,
        NavisoftNaviken21,
        PalmOS,
        PhilipsCDiDigitalVideo,
        PhotoCD,
        PlayStationGameSharkUpdates,
        RainbowDisc,
        SuperAudioCD,
        TaoiKTV,
        TomyKissSite,
        VideoCD,

        MarkerOtherEnd,

        #endregion
    }

    /// <summary>
    /// Known system category
    /// </summary>
    public enum KnownSystemCategory
    {
        DiscBasedConsole = 0,
        OtherConsole,
        Computer,
        Arcade,
        Other,
        Custom
    };

    /// <summary>
    /// List of all disc langauges
    /// </summary>
    public enum Language
    {
        Afrikaans,
        Arabic,
        Basque,
        Bulgarian,
        Catalan,
        Chinese,
        Croatian,
        Czech,
        Danish,
        Dutch,
        English,
        Finnish,
        French,
        Gaelic,
        German,
        Greek,
        Hebrew,
        Hindi,
        Hungarian,
        Italian,
        Japanese,
        Korean,
        Norwegian,
        Polish,
        Portuguese,
        Punjabi,
        Romanian,
        Russian,
        Slovak,
        Slovenian,
        Spanish,
        Swedish,
        Tamil,
        Thai,
        Turkish,
        Ukrainian,
    }

    /// <summary>
    /// All possible language selections
    /// </summary>
    public enum LanguageSelection
    {
        BiosSettings,
        LanguageSelector,
        OptionsMenu,
    }

    /// <summary>
    /// Known media types
    /// </summary>
    public enum MediaType
    {
        NONE = 0,

        #region Punched Media

        ApertureCard,
        JacquardLoomCard,
        MagneticStripeCard,
        OpticalPhonecard,
        PunchedCard,
        PunchedTape,

        #endregion

        #region Tape

        Cassette,
        DataCartridge,
        OpenReel,

        #endregion

        #region Disc / Disc

        BluRay,
        CDROM,
        DVD,
        FloppyDisk,
        Floptical,
        GDROM,
        HDDVD,
        HardDisk,
        IomegaBernoulliDisk,
        IomegaJaz,
        IomegaZip,
        LaserDisc, // LD-ROM and LV-ROM variants
        Nintendo64DD,
        NintendoFamicomDiskSystem,
        NintendoGameCubeGameDisc,
        NintendoWiiOpticalDisc,
        NintendoWiiUOpticalDisc,
        UMD,

        #endregion

        // Unsorted Formats
        Cartridge,
        CED,
        CompactFlash,
        MMC,
        SDCard,
        FlashDrive,
    }

    /// <summary>
    /// List of all known Redump systems
    /// </summary>
    public enum RedumpSystem
    {
        // Special BIOS sets
        MicrosoftXboxBIOS,
        NintendoGameCubeBIOS,
        SonyPlayStationBIOS,
        SonyPlayStation2BIOS,

        // Regular systems
        AcornArchimedes,
        AppleMacintosh,
        AudioCD,
        BDVideo,
        BandaiPippin,
        BandaiPlaydiaQuickInteractiveSystem,
        CommodoreAmigaCD,
        CommodoreAmigaCD32,
        CommodoreAmigaCDTV,
        DVDVideo,
        FujitsuFMTownsseries,
        HasbroVideoNow,
        HasbroVideoNowColor,
        HasbroVideoNowJr,
        HasbroVideoNowXP,
        IBMPCcompatible,
        IncredibleTechnologiesEagle,
        KonamiFireBeat,
        KonamiM2,
        KonamiSystem573,
        KonamiSystemGV,
        KonamiTwinkle,
        KonamieAmusement,
        MattelHyperScan,
        MemorexVisualInformationSystem,
        MicrosoftXbox,
        MicrosoftXbox360,
        MicrosoftXboxOne,
        NECPC88series,
        NECPC98series,
        NECPCEngineCDTurboGrafxCD,
        NECPCFXPCFXGA,
        NamcoSystem12,
        NamcoSystem246,
        NavisoftNaviken21,
        NinendoGameCube,
        NintendoWii,
        NintendoWiiU,
        PalmOS,
        Panasonic3DOInteractiveMultiplayer,
        PanasonicM2,
        PhilipsCDi,
        PhilipsCDiDigitalVideo,
        PhotoCD,
        PlayStationGameSharkUpdates,
        SegaChihiro,
        SegaDreamcast,
        SegaLindbergh,
        SegaMegaCDSegaCD,
        SegaNaomi,
        SegaNaomi2,
        SegaRingEdge,
        SegaRingEdge2,
        SegaSaturn,
        SegaTitanVideo,
        SegaTriforce,
        SharpX68000,
        SNKNeoGeoCD,
        SonyPlayStation,
        SonyPlayStation2,
        SonyPlayStation3,
        SonyPlayStation4,
        SonyPlayStationPortable,
        TABAustriaQuizard,
        TaoiKTV,
        TomyKissSite,
        VideoCD,
        VMLabsNUON,
        VTechVFlashVSmilePro,
        ZAPiTGamesGameWaveFamilyEntertainmentSystem,
    }

    /// <summary>
    /// List of all known Redump regions
    /// </summary>
    public enum Region
    {
        Argentina,
        Asia,
        AsiaEurope,
        AsiaUSA,
        Australia,
        Austria,
        AustriaSwitzerland,
        Belgium,
        BelgiumNetherlands,
        Brazil,
        Canada,
        China,
        Croatia,
        Czech,
        Denmark,
        Europe,
        EuropeAsia,
        EuropeAustralia,
        Finland,
        France,
        FranceSpain,
        Germany,
        GreaterChina,
        Greece,
        Hungary,
        India,
        Ireland,
        Israel,
        Italy,
        Japan,
        JapanAsia,
        JapanEurope,
        JapanKorea,
        JapanUSA,
        Korea,
        LatinAmerica,
        Netherlands,
        Norway,
        Poland,
        Portugal,
        Russia,
        Scandinavia,
        Singapore,
        Slovakia,
        SouthAfrica,
        Spain,
        Sweden,
        Switzerland,
        Taiwan,
        Thailand,
        Turkey,
        UnitedArabEmirates,
        UK,
        UKAustralia,
        Ukraine,
        USA,
        USAAsia,
        USABrazil,
        USACanada,
        USAEurope,
        USAJapan,
        World,
    }

    /// <summary>
    /// Generic yes/no values for Redump
    /// </summary>
    public enum YesNo
    {
        NULL = 0,
        No = 1,
        Yes = 2,
    }
}
