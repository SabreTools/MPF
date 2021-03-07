using MPF.Data;

namespace MPF.Redump
{
    /// <summary>
    /// Information pertaining to Redump systems
    /// </summary>
    public static class Extras
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
            //RedumpSystem.MicrosoftXboxSeriesXS,
            RedumpSystem.NavisoftNaviken21,
            RedumpSystem.NintendoWii,
            RedumpSystem.NintendoWiiU,
            RedumpSystem.PanasonicM2,
            RedumpSystem.SegaPrologue21,
            RedumpSystem.SegaRingEdge,
            RedumpSystem.SegaRingEdge2,
            RedumpSystem.SonyPlayStation3,
            RedumpSystem.SonyPlayStation4,
            //RedumpSystem.SonyPlayStation5,
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
            //RedumpSystem.MicrosoftXboxSeriesXS,
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
            //RedumpSystem.SonyPlayStation5,
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
    }
}
