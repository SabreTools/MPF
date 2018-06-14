namespace DICUI
{
    /// <summary>
    /// Known disc types
    /// </summary>
    public enum DiscType
    {
        NONE = 0,
        CD,
        DVD5,
        DVD9,
        GDROM,
        HDDVD,
        BD25,
        BD50,

        // Special Formats
        GameCubeGameDisc,
        WiiOpticalDisc,
        WiiUOpticalDisc,
        UMD,
        
        // Keeping this separate since it's currently unsupported in the UI
        Floppy = 99,
    }

    /// <summary>
    /// Known systems
    /// </summary>
    /// <remarks>Ensure that Utilities methods are updated as well</remarks>
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
        SegaSTV,
        SegaSystem32,
        SeibuCATSSystem,
        TABAustriaQuizard,
        TandyMemorexVisualInformationSystem,
        TsunamiTsuMoMultiGameMotionSystem,

        #endregion

        #region Other

        AudioCD,
        BDVideo,
        DVDVideo,
        EnhancedCD,
        PalmOS,
        PhilipsCDiDigitalVideo,
        PhotoCD,
        PlayStationGameSharkUpdates,
        TaoiKTV,
        TomyKissSite,
        VideoCD,

        #endregion
    }
}
