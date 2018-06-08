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
		UMD,
		
		// Keeping this separate since it's currently unsupported in the UI
		Floppy = 99,
	}

	/// <summary>
	/// Known systems
	/// </summary>
	/// <remarks>Ensure that Utilities methods are updated as well</remarks>
	public enum System
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

		NamcoSegaNintendoTriforce,
		NamcoSystem246,
		SegaChihiro,
		SegaLindbergh,
		SegaNaomi,
		SegaNaomi2,
		TABAustriaQuizard,
		TandyMemorexVisualInformationSystem,

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
