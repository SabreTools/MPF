using System;
using System.Collections.Generic;

namespace DICUI
{
	public static class Utilities
	{
		/// <summary>
		/// Get the string representation of the System enum values
		/// </summary>
		/// <param name="sys">System value to convert</param>
		/// <returns>String representing the value, if possible</returns>
		public static string SystemToString(System sys)
		{
			switch (sys)
			{
				#region Consoles

				case System.BandaiPlaydiaQuickInteractiveSystem:
					return "Bandai Playdia Quick Interactive System";
				case System.BandaiApplePippin:
					return "Bandai / Apple Pippin";
				case System.CommodoreAmigaCD32:
					return "Commodore Amiga CD32";
				case System.CommodoreAmigaCDTV:
					return "Commodore Amiga CDTV";
				case System.MattelHyperscan:
					return "Mattel HyperScan";
				case System.MicrosoftXBOX:
					return "Microsoft XBOX";
				case System.MicrosoftXBOX360:
					return "Microsoft XBOX 360";
				case System.MicrosoftXBOXOne:
					return "Microsoft XBOX One";
				case System.NECPCEngineTurboGrafxCD:
					return "NEC PC-Engine / TurboGrafx CD";
				case System.NECPCFX:
					return "NEC PC-FX / PC-FXGA";
				case System.NintendoGameCube:
					return "Nintendo GameCube";
				case System.NintendoWii:
					return "Nintendo Wii";
				case System.NintendoWiiU:
					return "Nintendo Wii U";
				case System.Panasonic3DOInteractiveMultiplayer:
					return "Panasonic 3DO Interactive Multiplayer";
				case System.PhilipsCDi:
					return "Philips CD-i";
				case System.SegaCDMegaCD:
					return "Sega CD / Mega CD";
				case System.SegaDreamcast:
					return "Sega Dreamcast";
				case System.SegaSaturn:
					return "Sega Saturn";
				case System.SNKNeoGeoCD:
					return "SNK Neo Geo CD";
				case System.SonyPlayStation:
					return "Sony PlayStation";
				case System.SonyPlayStation2:
					return "Sony PlayStation 2";
				case System.SonyPlayStation3:
					return "Sony PlayStation 3";
				case System.SonyPlayStation4:
					return "Sony PlayStation 4";
				case System.SonyPlayStationPortable:
					return "Sony PlayStation Portable";
				case System.VMLabsNuon:
					return "VM Labs NUON";
				case System.VTechVFlashVSmilePro:
					return "VTech V.Flash - V.Smile Pro";
				case System.ZAPiTGamesGameWaveFamilyEntertainmentSystem:
					return "ZAPiT Games Game Wave Family Entertainment System";

				#endregion

				#region Computers

				case System.AcornArchimedes:
					return "Acorn Archimedes";
				case System.AppleMacintosh:
					return "Apple Macintosh";
				case System.CommodoreAmigaCD:
					return "CommodoreAmigaCD";
				case System.FujitsuFMTowns:
					return "Fujitsu FM Towns series";
				case System.IBMPCCompatible:
					return "IBM PC Compatible";
				case System.NECPC88:
					return "NEC PC-88";
				case System.NECPC98:
					return "NEC PC-98";
				case System.SharpX68000:
					return "Sharp X68000";

				#endregion

				#region Arcade

				case System.NamcoSegaNintendoTriforce:
					return "Namco / Sega / Nintendo Triforce";
				case System.NamcoSystem246:
					return "Namco System 246";
				case System.SegaChihiro:
					return "Sega Chihiro";
				case System.SegaLindbergh:
					return "Sega Lindbergh";
				case System.SegaNaomi:
					return "Sega Naomi";
				case System.SegaNaomi2:
					return "Sega Naomi 2";
				case System.TABAustriaQuizard:
					return "TAB-Austria Quizard";
				case System.TandyMemorexVisualInformationSystem:
					return "Tandy / Memorex Visual Information System";

				#endregion

				#region Others

				case System.AudioCD:
					return "Audio CD";
				case System.BDVideo:
					return "BD-Video";
				case System.DVDVideo:
					return "DVD-Video";
				case System.EnhancedCD:
					return "Enhanced CD";
				case System.PalmOS:
					return "PalmOS";
				case System.PhilipsCDiDigitalVideo:
					return "Philips CD-i Digital Video";
				case System.PhotoCD:
					return "Photo CD";
				case System.PlayStationGameSharkUpdates:
					return "PlayStation GameShark Updates";
				case System.TaoiKTV:
					return "Tao iKTV";
				case System.TomyKissSite:
					return "Tomy Kiss-Site";
				case System.VideoCD:
					return "Video CD";

				#endregion

				case System.NONE:
				default:
					return "Unknown";
			}
		}

		/// <summary>
		/// Get a list of valid DiscTypes for a given system
		/// </summary>
		/// <param name="sys">System value to check</param>
		/// <returns>List of DiscTypes</returns>
		public static List<DiscType> GetValidDiscTypes(System sys)
		{
			List<DiscType> types = new List<DiscType>();
			
			switch (sys)
			{
				#region Consoles

				case System.BandaiPlaydiaQuickInteractiveSystem:
					types.Add(DiscType.CD);
					break;
				case System.BandaiApplePippin:
					types.Add(DiscType.CD);
					break;
				case System.CommodoreAmigaCD32:
					types.Add(DiscType.CD);
					break;
				case System.CommodoreAmigaCDTV:
					types.Add(DiscType.CD);
					break;
				case System.MattelHyperscan:
					types.Add(DiscType.CD);
					break;
				case System.MicrosoftXBOX:
					types.Add(DiscType.CD);
					types.Add(DiscType.DVD5);
					break;
				case System.MicrosoftXBOX360:
					types.Add(DiscType.CD);
					types.Add(DiscType.DVD9);
					types.Add(DiscType.HDDVD);
					break;
				case System.MicrosoftXBOXOne:
					types.Add(DiscType.BD25);
					types.Add(DiscType.BD50);
					break;
				case System.NECPCEngineTurboGrafxCD:
					types.Add(DiscType.CD);
					break;
				case System.NECPCFX:
					types.Add(DiscType.CD);
					break;
				case System.NintendoGameCube:
					types.Add(DiscType.GameCubeGameDisc);
					break;
				case System.NintendoWii:
					types.Add(DiscType.DVD5); // TODO: Confirm
					types.Add(DiscType.DVD9); // TODO: Confirm
					break;
				case System.NintendoWiiU:
					types.Add(DiscType.DVD5); // TODO: Confirm
					break;
				case System.Panasonic3DOInteractiveMultiplayer:
					types.Add(DiscType.CD);
					break;
				case System.PhilipsCDi:
					types.Add(DiscType.CD);
					break;
				case System.SegaCDMegaCD:
					types.Add(DiscType.CD);
					break;
				case System.SegaDreamcast:
					types.Add(DiscType.GDROM);
					break;
				case System.SegaSaturn:
					types.Add(DiscType.CD);
					break;
				case System.SNKNeoGeoCD:
					types.Add(DiscType.CD);
					break;
				case System.SonyPlayStation:
					types.Add(DiscType.CD);
					break;
				case System.SonyPlayStation2:
					types.Add(DiscType.CD);
					types.Add(DiscType.DVD5);
					types.Add(DiscType.DVD9);
					break;
				case System.SonyPlayStation3:
					types.Add(DiscType.BD25);
					types.Add(DiscType.BD50);
					break;
				case System.SonyPlayStation4:
					types.Add(DiscType.BD25);
					types.Add(DiscType.BD50);
					break;
				case System.SonyPlayStationPortable:
					types.Add(DiscType.UMD);
					break;
				case System.VMLabsNuon:
					types.Add(DiscType.DVD5); // TODO: Confirm
					break;
				case System.VTechVFlashVSmilePro:
					types.Add(DiscType.CD);
					break;
				case System.ZAPiTGamesGameWaveFamilyEntertainmentSystem:
					types.Add(DiscType.DVD5);
					break;

				#endregion

				#region Computers

				case System.AcornArchimedes:
					types.Add(DiscType.CD);
					break;
				case System.AppleMacintosh:
					types.Add(DiscType.CD);
					types.Add(DiscType.DVD5);
					types.Add(DiscType.DVD9);
					types.Add(DiscType.Floppy);
					break;
				case System.CommodoreAmigaCD:
					types.Add(DiscType.CD);
					break;
				case System.FujitsuFMTowns:
					types.Add(DiscType.CD);
					break;
				case System.IBMPCCompatible:
					types.Add(DiscType.CD);
					types.Add(DiscType.DVD5);
					types.Add(DiscType.DVD9);
					types.Add(DiscType.Floppy);
					break;
				case System.NECPC88:
					types.Add(DiscType.CD);
					break;
				case System.NECPC98:
					types.Add(DiscType.CD);
					break;
				case System.SharpX68000:
					types.Add(DiscType.CD);
					break;

				#endregion

				#region Arcade

				case System.NamcoSegaNintendoTriforce:
					types.Add(DiscType.GDROM);
					break;
				case System.SegaChihiro:
					types.Add(DiscType.GDROM);
					break;
				case System.SegaLindbergh:
					types.Add(DiscType.DVD5); // TODO: Confirm
					break;
				case System.SegaNaomi:
					types.Add(DiscType.GDROM);
					break;
				case System.SegaNaomi2:
					types.Add(DiscType.GDROM);
					break;
				case System.TABAustriaQuizard:
					types.Add(DiscType.CD);
					break;
				case System.TandyMemorexVisualInformationSystem:
					types.Add(DiscType.CD);
					break;

				#endregion

				#region Others

				case System.AudioCD:
					types.Add(DiscType.CD);
					break;
				case System.BDVideo:
					types.Add(DiscType.BD25);
					types.Add(DiscType.BD50);
					break;
				case System.DVDVideo:
					types.Add(DiscType.DVD5);
					types.Add(DiscType.DVD9);
					break;
				case System.EnhancedCD:
					types.Add(DiscType.CD);
					break;
				case System.PalmOS:
					types.Add(DiscType.CD);
					break;
				case System.PhilipsCDiDigitalVideo:
					types.Add(DiscType.CD);
					break;
				case System.PhotoCD:
					types.Add(DiscType.CD);
					break;
				case System.PlayStationGameSharkUpdates:
					types.Add(DiscType.CD);
					break;
				case System.TaoiKTV:
					types.Add(DiscType.CD);
					break;
				case System.TomyKissSite:
					types.Add(DiscType.CD);
					break;
				case System.VideoCD:
					types.Add(DiscType.CD);
					break;

				#endregion

				case System.NONE:
				default:
					types.Add(DiscType.NONE);
					break;
			}

			return types;
		}

		/// <summary>
		/// Get the DIC command to be used for a given DiscType
		/// </summary>
		/// <param name="type">DiscType value to check</param>
		/// <returns>String containing the command, null on error</returns>
		public static string GetBaseCommand(DiscType type)
		{
			switch (type)
			{
				case DiscType.CD:
					return "cd";
				case DiscType.DVD5:
					return "dvd";
				case DiscType.DVD9:
					return "dvd";
				case DiscType.GDROM:
					return "gd"; // TODO: "swap"?
				case DiscType.HDDVD:
					Console.WriteLine("HD-DVD dumping is not supported by DIC");
					return null;
				case DiscType.BD25:
					return "bd";
				case DiscType.BD50:
					return "bd";

				// Special Formats
				case DiscType.GameCubeGameDisc:
					return "dvd";
				case DiscType.UMD:
					Console.WriteLine("UMD dumping is not supported by DIC");
					return null;

				// Non-optical
				case DiscType.Floppy:
					return "fd";

				default:
					return null;
			}
		}

		/// <summary>
		/// Get list of default parameters for a given system and disc type
		/// </summary>
		/// <param name="sys">System value to check</param>
		/// <param name="type">DiscType value to check</param>
		/// <returns>List of strings representing the parameters</returns>
		public static List<string> GetDefaultParameters(System sys, DiscType type)
		{
			// First check to see if the combination of system and disctype is valid
			List<DiscType> validTypes = GetValidDiscTypes(sys);
			if (!validTypes.Contains(type))
			{
				Console.WriteLine("Invalid DiscType '{0}' for System '{1}'", type.ToString(), SystemToString(sys));
				return null;
			}

			// Now sort based on disc type
			List<string> parameters = new List<string>();
			switch (type)
			{
				case DiscType.CD:
					parameters.Add("/c2 20");

					switch (sys)
					{
						case System.AppleMacintosh:
						case System.IBMPCCompatible:
							parameters.Add("/ns");
							parameters.Add("/sf");
							parameters.Add("/ss");
							break;
						case System.NECPCEngineTurboGrafxCD:
							parameters.Add("/m");
							break;
						case System.SonyPlayStation:
							parameters.Add("/am");
							break;
					}
					break;
				case DiscType.DVD5:
					// Currently no defaults set
					break;
				case DiscType.DVD9:
					// Currently no defaults set
					break;
				case DiscType.GDROM:
					parameters.Add("/c2 20");
					break;
				case DiscType.HDDVD:
					Console.WriteLine("HD-DVD dumping is not supported by DIC");
					break;
				case DiscType.BD25:
					// Currently no defaults set
					break;
				case DiscType.BD50:
					// Currently no defaults set
					break;

				// Special Formats
				case DiscType.GameCubeGameDisc:
					parameters.Add("/raw");
					break;
				case DiscType.UMD:
					Console.WriteLine("UMD dumping is not supported by DIC");
					break;

				// Non-optical
				case DiscType.Floppy:
					// Currently no defaults set
					break;
			}

			return parameters;
		}
	}
}
