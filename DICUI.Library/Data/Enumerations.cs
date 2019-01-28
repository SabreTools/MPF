namespace DICUI.Data
{
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
        DriveSpeed,
        Eject,
        Floppy,
        GDROM,
        MDS,
        Merge,
        Reset,
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
        NoFixSubQSecuROM,
        NoFixSubRtoW,
        Raw,
        Reverse,
        ScanAntiMod,
        ScanFileProtect,
        ScanSectorProtect,
        SeventyFour,
        SkipSector,
        SubchannelReadLevel,
    }

    /// <summary>
    /// Known systems
    /// </summary>
    public enum KnownSystem
    {
        NONE = 0,

        #region Consoles

        BandaiPlaydiaQuickInteractiveSystem,
        BandaiApplePippin,
        CommodoreAmigaCD32,
        CommodoreAmigaCDTV,
        MattelHyperscan,
        MicrosoftXBOX,
        MicrosoftXBOX360,
        MicrosoftXBOXOne,
        NECPCEngineTurboGrafxCD,
        NECPCFX,
        NintendoGameCube,
        NintendoWii,
        NintendoWiiU,
        Panasonic3DOInteractiveMultiplayer,
        PhilipsCDi,
        SegaCDMegaCD,
        SegaDreamcast,
        SegaSaturn,
        SNKNeoGeoCD,
        SonyPlayStation,
        SonyPlayStation2,
        SonyPlayStation3,
        SonyPlayStation4,
        SonyPlayStationPortable,
        VMLabsNuon,
        VTechVFlashVSmilePro,
        ZAPiTGamesGameWaveFamilyEntertainmentSystem,

        MarkerConsoleEnd,

        #endregion

        #region Computers

        AcornArchimedes,
        AppleMacintosh,
        CommodoreAmigaCD,
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
        MeritIndustriesMegaTouchAurora,
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
        TandyMemorexVisualInformationSystem,
        TsunamiTsuMoMultiGameMotionSystem,

        MarkerArcadeEnd,

        #endregion

        #region Other

        AudioCD,
        BDVideo,
        DVDVideo,
        EnhancedCD,
        EnhancedDVD,
        EnhancedBD,
        HasbroVideoNow,
        HDDVDVideo,
        PalmOS,
        PhilipsCDiDigitalVideo,
        PhotoCD,
        PlayStationGameSharkUpdates,
        RainbowDisc,
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
        Console = 0,
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
        NintendoGameCube,
        NintendoWiiOpticalDisc,
        NintendoWiiUOpticalDisc,
        UMD,

        #endregion

        // Unsorted Formats
        Cartridge,
        CED,
    }
}
