namespace DICUI
{
    /// <summary>
    /// Known disc types
    /// </summary>
    public enum DiscType
    {
        // Generic Optical Formats
        NONE = 0,
        CD,
        DVD,
        GDROM,
        HDDVD,
        BluRay,
        LaserDisc,

        // Special Optical Formats
        CED,
        GameCubeGameDisc,
        WiiOpticalDisc,
        WiiUOpticalDisc,
        UMD,
        
        // Non-Optical Formats
        Floppy,
        Cassette,
        Cartridge,
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
        MicrosoftXBOX360XDG2,
        MicrosoftXBOX360XDG3,
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
        HDDVDVideo,
        PalmOS,
        PhilipsCDiDigitalVideo,
        PhotoCD,
        PlayStationGameSharkUpdates,
        TaoiKTV,
        TomyKissSite,
        VideoCD,

        #endregion

        Custom = -1
    }
}
