using System;

namespace MPF.Data
{
    /// <summary>
    /// Drive type for dumping
    /// </summary>
    public enum InternalDriveType
    {
        Optical,
        Floppy,
        HardDisk,
        Removable,
    }

    /// <summary>
    /// Program that is being used to dump media
    /// </summary>
    public enum InternalProgram
    {
        NONE = 0,

        // Dumping support
        Aaru,
        DD,
        DiscImageCreator,

        // Verification support only
        CleanRip,
        DCDumper,
        UmdImageCreator,
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
        MattelFisherPriceiXL,
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
        funworldPhotoPlay,
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
        DVDAudio,
        DVDVideo,
        EnhancedCD,
        HDDVDVideo,
        NavisoftNaviken21,
        PalmOS,
        PhotoCD,
        PlayStationGameSharkUpdates,
        RainbowDisc,
        SegaPrologue21,
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
    /// Physical media types
    /// </summary>
    /// <see cref="https://docs.microsoft.com/en-us/previous-versions/windows/desktop/cimwin32a/win32-physicalmedia"/>
    public enum PhysicalMediaType : ushort
    {
        Unknown = 0,
        Other = 1,
        TapeCartridge = 2,
        QICCartridge = 3,
        AITCartridge = 4,
        DTFCartridge = 5,
        DATCartridge = 6,
        EightMillimeterTapeCartridge = 7,
        NineteenMillimeterTapeCartridge = 8,
        DLTCartridge = 9,
        HalfInchMagneticTapeCartridge = 10,
        CartridgeDisk = 11,
        JAZDisk = 12,
        ZIPDisk = 13,
        SyQuestDisk = 14,
        WinchesterRemovableDisk = 15,
        CDROM = 16,
        CDROMXA = 17,
        CDI = 18,
        CDRecordable = 19,
        WORM = 20,
        MagnetoOptical = 21,
        DVD = 22,
        DVDPlusRW = 23,
        DVDRAM = 24,
        DVDROM = 25,
        DVDVideo = 26,
        Divx = 27,
        FloppyDiskette = 28,
        HardDisk = 29,
        MemoryCard = 30,
        HardCopy = 31,
        ClikDisk = 32,
        CDRW = 33,
        CDDA = 34,
        CDPlus = 35,
        DVDRecordable = 36,
        DVDMinusRW = 37,
        DVDAudio = 38,
        DVD5 = 39,
        DVD9 = 40,
        DVD10 = 41,
        DVD18 = 42,
        MagnetoOpticalRewriteable = 43,
        MagnetoOpticalWriteOnce = 44,
        MagnetoOpticalRewriteableLIMDOW = 45,
        PhaseChangeWriteOnce = 46,
        PhaseChangeRewriteable = 47,
        PhaseChangeDualRewriteable = 48,
        AblativeWriteOnce = 49,
        NearFieldRecording = 50,
        MiniQic = 51,
        Travan = 52,
        EightMillimeterMetalParticle = 53,
        EightMillimeterAdvancedMetalEvaporate = 54,
        NCTP = 55,
        LTOUltrium = 56,
        LTOAccelis = 57,
        NineTrackTape = 58,
        EighteenTrackTape = 59,
        ThirtySixTrackTape = 60,
        Magstar3590 = 61,
        MagstarMP = 62,
        D2Tape = 63,
        TapeDSTSmall = 64,
        TapeDSTMedium = 65,
        TapeDSTLarge = 66,
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

    #region Win32_CDROMDrive

    // https://docs.microsoft.com/en-us/windows/win32/cimwin32prov/win32-cdromdrive

    /// <summary>
    /// Availability and status of the device
    /// </summary>
    public enum Availability : ushort
    {
        Other = 1,
        Unknown = 2,
        RunningFullPower = 3,
        Warning = 4,
        InTest = 5,
        NotApplicable = 6,
        PowerOff = 7,
        OffLine = 8,
        OffDuty = 9,
        Degraded = 10,
        NotInstalled = 11,
        InstallError = 12,
        PowerSaveUnknown = 13,
        PowerSaveLowPowerMode = 14,
        PowerSaveStandby = 15,
        PowerCycle = 16,
        PowerSaveWarning = 17,
        Paused = 18,
        NotReady = 19,
        NotConfigured = 20,
        Quiesced = 21,
    }

    /// <summary>
    /// Optical drive capabilities
    /// </summary>
    public enum Capabilities : ushort
    {
        Unknown = 0,
        Other = 1,
        SequentialAccess = 2,
        RandomAccess = 3,
        SupportsWriting = 4,
        Encryption = 5,
        Compression = 6,
        SupportsRemoveableMedia = 7,
        ManualCleaning = 8,
        AutomaticCleaning = 9,
        SMARTNotification = 10,
        SupportsDualSidedMedia = 11,
        PredismountEjectNotRequired = 12,
    }

    /// <summary>
    /// File system flags
    /// </summary>
    [Flags]
    public enum FileSystemFlags : uint
    {
        None = 0,
        CaseSensitiveSearch = 1,
        CasePreservedNames = 2,
        UnicodeOnDisk = 4,
        PersistentACLs = 8,
        FileCompression = 16,
        VolumeQuotas = 32,
        SupportsSparseFiles = 64,
        SupportsReparsePoints = 128,
        SupportsRemoteStorage = 256,
        SupportsLongNames = 16384,
        VolumeIsCompressed = 32768,
        ReadOnlyVolume = 524289, // TODO: Invesitgate, as this value seems wrong
        SupportsObjectIDS = 65536,
        SupportsEncryption = 131072,
        SupportsNamedStreams = 262144,
    }

    /// <summary>
    /// Specific power-related capabilities of a logical device
    /// </summary>
    public enum PowerManagementCapabilities : ushort
    {
        Unknown = 0,
        NotSupported = 1,
        Disabled = 2,
        Enabled = 3,
        PowerSavingModesEnteredAutomatically = 4,
        PowerStateSettable = 5,
        PowerCyclingSupported = 6,
        TimedPowerOnSupported = 7,
    }

    #endregion
}
