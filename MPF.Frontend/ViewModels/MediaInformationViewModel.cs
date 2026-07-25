using System;
using System.Collections.Generic;
using MPF.Frontend.ComboBoxItems;
using MPF.Frontend.Tools;
using SabreTools.RedumpLib.Data;

namespace MPF.Frontend.ViewModels
{
    public class MediaInformationViewModel
    {
        #region Fields

        /// <summary>
        /// Application-level Options object
        /// </summary>
        public Options Options { get; private set; }

        /// <summary>
        /// SubmissionInfo object to fill and save
        /// </summary>
        public SubmissionInfo SubmissionInfo { get; private set; }

        /// <summary>
        /// Translation layer for comment special fields
        /// </summary>
        public Dictionary<string, string> CommentsSpecialFields { get; private set; } = [];

        /// <summary>
        /// Translation layer for content special fields
        /// </summary>
        public Dictionary<string, string> ContentsSpecialFields { get; private set; } = [];

        #endregion

        #region Lists

        /// <summary>
        /// List of available disc categories
        /// </summary>
        public List<Element<DiscCategory>> Categories { get; private set; }
            = Element<DiscCategory>.GenerateElements();

        /// <summary>
        /// List of available regions
        /// </summary>
        public List<RegionCodeComboBoxItem> Regions { get; private set; }
            = RegionCodeComboBoxItem.GenerateElements();

        /// <summary>
        /// List of Redump-supported Regions
        /// </summary>
        private static readonly List<RegionCode> RedumpRegions =
        [
            RegionCode.Argentina,
            RegionCode.Asia,
            RegionCode.Australia,
            RegionCode.Austria,
            RegionCode.Azerbaijan,
            RegionCode.Belarus,
            RegionCode.Belgium,
            RegionCode.Brazil,
            RegionCode.Bulgaria,
            RegionCode.Canada,
            RegionCode.China,
            RegionCode.Croatia,
            RegionCode.Cyprus,
            RegionCode.Czechia,
            RegionCode.Denmark,
            RegionCode.Estonia,
            RegionCode.Europe,
            RegionCode.Export,
            RegionCode.Finland,
            RegionCode.France,
            RegionCode.Germany,
            RegionCode.Greece,
            RegionCode.Hungary,
            RegionCode.Iceland,
            RegionCode.India,
            RegionCode.Indonesia,
            RegionCode.Iran,
            RegionCode.Ireland,
            RegionCode.Israel,
            RegionCode.Italy,
            RegionCode.Japan,
            RegionCode.SouthKorea,
            RegionCode.LatinAmerica,
            RegionCode.Lithuania,
            RegionCode.Malaysia,
            RegionCode.Mexico,
            RegionCode.Netherlands,
            RegionCode.NewZealand,
            RegionCode.Norway,
            RegionCode.Poland,
            RegionCode.Portugal,
            RegionCode.Romania,
            RegionCode.RussianFederation,
            RegionCode.Scandinavia,
            RegionCode.Serbia,
            RegionCode.Singapore,
            RegionCode.Slovakia,
            RegionCode.SouthAfrica,
            RegionCode.Spain,
            RegionCode.Sweden,
            RegionCode.Switzerland,
            RegionCode.Taiwan,
            RegionCode.Thailand,
            RegionCode.Turkey,
            RegionCode.UnitedKingdom,
            RegionCode.Ukraine,
            RegionCode.UnitedArabEmirates,
            RegionCode.Ukraine,
            RegionCode.UnitedStatesOfAmerica,
            RegionCode.VietNam,
            RegionCode.World,
        ];

        /// <summary>
        /// List of available languages
        /// </summary>
        public List<LanguageCodeComboBoxItem> Languages { get; private set; }
            = LanguageCodeComboBoxItem.GenerateElements();

        /// <summary>
        /// List of Redump-supported Languages
        /// </summary>
        private static readonly List<LanguageCode> RedumpLanguages =
        [
            LanguageCode.Afrikaans,
            LanguageCode.Albanian,
            LanguageCode.Arabic,
            LanguageCode.Armenian,
            LanguageCode.Azerbaijani,
            LanguageCode.Basque,
            LanguageCode.Belarusian,
            LanguageCode.Bulgarian,
            LanguageCode.Catalan,
            LanguageCode.Chinese,
            LanguageCode.Croatian,
            LanguageCode.Czech,
            LanguageCode.Danish,
            LanguageCode.Dutch,
            LanguageCode.English,
            LanguageCode.Estonian,
            LanguageCode.Finnish,
            LanguageCode.French,
            LanguageCode.Gaelic,
            LanguageCode.Galician,
            LanguageCode.German,
            LanguageCode.Greek,
            LanguageCode.Hebrew,
            LanguageCode.Hindi,
            LanguageCode.Hungarian,
            LanguageCode.Icelandic,
            LanguageCode.Indonesian,
            LanguageCode.Irish,
            LanguageCode.Italian,
            LanguageCode.Japanese,
            LanguageCode.Korean,
            LanguageCode.Latin,
            LanguageCode.Latvian,
            LanguageCode.Lithuanian,
            LanguageCode.Macedonian,
            LanguageCode.Malay,
            LanguageCode.Maori,
            LanguageCode.Norwegian,
            LanguageCode.Persian,
            LanguageCode.Polish,
            LanguageCode.Portuguese,
            LanguageCode.Panjabi,
            LanguageCode.Romanian,
            LanguageCode.Russian,
            LanguageCode.Serbian,
            LanguageCode.Slovak,
            LanguageCode.Slovenian,
            LanguageCode.Spanish,
            LanguageCode.Swedish,
            LanguageCode.Tamil,
            LanguageCode.Thai,
            LanguageCode.Turkish,
            LanguageCode.Ukrainian,
            LanguageCode.Vietnamese,
            LanguageCode.Welsh,
        ];

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public MediaInformationViewModel(Options options, SubmissionInfo? submissionInfo)
        {
            Options = options;
            SubmissionInfo = submissionInfo?.Clone() as SubmissionInfo ?? new SubmissionInfo();
        }

        #region Helpers

        /// <summary>
        /// Load the current contents of the base SubmissionInfo to the UI
        /// </summary>
        /// TODO: Convert selected list item to binding
        public void Load()
        {
            if (SubmissionInfo.RegionsAndLanguages.Regions is not null)
                Regions.ForEach(l => l.IsChecked = Array.IndexOf(SubmissionInfo.RegionsAndLanguages.Regions, l) > -1);
            if (SubmissionInfo.RegionsAndLanguages.Languages is not null)
                Languages.ForEach(l => l.IsChecked = Array.IndexOf(SubmissionInfo.RegionsAndLanguages.Languages, l) > -1);

            // Convert comments special fields to string-based keys
            foreach (var kvp in SubmissionInfo.DumpMetadata.CommentsSpecialFields)
            {
                var key = ConvertSiteCodeToString(kvp.Key);
                if (key is not null)
                    CommentsSpecialFields[key] = kvp.Value;
            }

            // Convert contents special fields to string-based keys
            foreach (var kvp in SubmissionInfo.DumpMetadata.ContentsSpecialFields)
            {
                var key = ConvertSiteCodeToString(kvp.Key);
                if (key is not null)
                    ContentsSpecialFields[key] = kvp.Value;
            }
        }

        /// <summary>
        /// Save the current contents of the UI to the base SubmissionInfo
        /// </summary>
        /// TODO: Convert selected list item to binding
        public void Save()
        {
            SubmissionInfo.RegionsAndLanguages.Regions = [.. Regions.FindAll(l => l.IsChecked).ConvertAll(l => l?.Value)];
            if (SubmissionInfo.RegionsAndLanguages.Regions.Length == 0)
                SubmissionInfo.RegionsAndLanguages.Regions = [null];

            SubmissionInfo.RegionsAndLanguages.Languages = [.. Languages.FindAll(l => l.IsChecked).ConvertAll(l => l?.Value)];
            if (SubmissionInfo.RegionsAndLanguages.Languages.Length == 0)
                SubmissionInfo.RegionsAndLanguages.Languages = [null];

            SubmissionInfo.DiscIdentity.Title = FrontendTool.NormalizeDiscTitle(SubmissionInfo.DiscIdentity.Title, SubmissionInfo.RegionsAndLanguages.Languages);

            // Convert comments special fields from string-based keys
            foreach (var kvp in CommentsSpecialFields)
            {
                var siteCode = ConvertStringToSiteCode(kvp.Key);
                if (siteCode is not null)
                    SubmissionInfo.DumpMetadata.CommentsSpecialFields[siteCode] = kvp.Value;
            }

            // Convert contents special fields from string-based keys
            foreach (var kvp in ContentsSpecialFields)
            {
                var siteCode = ConvertStringToSiteCode(kvp.Key);
                if (siteCode is not null)
                    SubmissionInfo.DumpMetadata.ContentsSpecialFields[siteCode] = kvp.Value;
            }
        }

        /// <summary>
        /// Repopulate the list of Languages based on Redump support
        /// </summary>
        public void SetRedumpLanguages()
        {
            Languages = RedumpLanguages.ConvertAll(l => new LanguageCodeComboBoxItem(l));
        }

        /// <summary>
        /// Repopulate the list of Regions based on Redump support
        /// </summary>
        public void SetRedumpRegions()
        {
            Regions = RedumpRegions.ConvertAll(r => new RegionCodeComboBoxItem(r));
        }

        /// <summary>
        /// Convert a site code to the corresponding index string
        /// </summary>
        private static string? ConvertSiteCodeToString(SiteCode? siteCode)
        {
            if (siteCode == SiteCode.AcclaimID)
                return "AcclaimID";
            else if (siteCode == SiteCode.AccoladeID)
                return "AccoladeID";
            else if (siteCode == SiteCode.ActivisionID)
                return "ActivisionID";
            else if (siteCode == SiteCode.AdditionalBCAData)
                return "AdditionalBCAData";
            else if (siteCode == SiteCode.AlternativeTitle)
                return "AlternativeTitle";
            else if (siteCode == SiteCode.AlternativeForeignTitle)
                return "AlternativeForeignTitle";
            else if (siteCode == SiteCode.Applications)
                return "Applications";

            else if (siteCode == SiteCode.BandaiID)
                return "BandaiID";
            else if (siteCode == SiteCode.BBFCRegistrationNumber)
                return "BBFCRegistrationNumber";
            else if (siteCode == SiteCode.BethesdaID)
                return "BethesdaID";

            else if (siteCode == SiteCode.CDProjektID)
                return "CDProjektID";
            else if (siteCode == SiteCode.CompatibleOS)
                return "CompatibleOS";
            else if (siteCode == SiteCode.CoverID)
                return "CoverID";

            else if (siteCode == SiteCode.DiceMultimedia)
                return "DiceMultimedia";
            else if (siteCode == SiteCode.DiscHologramID)
                return "DiscHologramID";
            else if (siteCode == SiteCode.DiscID)
                return "DiscID";
            else if (siteCode == SiteCode.DiscTitleNonLatin)
                return "DiscTitleNonLatin";
            else if (siteCode == SiteCode.DisneyInteractiveID)
                return "DisneyInteractiveID";
            else if (siteCode == SiteCode.DMIHash)
                return "DMIHash";
            else if (siteCode == SiteCode.DNASDiscID)
                return "DNASDiscID";

            else if (siteCode == SiteCode.EditionNonLatin)
                return "EditionNonLatin";
            else if (siteCode == SiteCode.EidosID)
                return "EidosID";
            else if (siteCode == SiteCode.ElectronicArtsID)
                return "ElectronicArtsID";
            else if (siteCode == SiteCode.Extras)
                return "Extras";

            else if (siteCode == SiteCode.Filename)
                return "Filename";
            else if (siteCode == SiteCode.FocusMultimedia)
                return "FocusMultimedia";
            else if (siteCode == SiteCode.FoxInteractiveID)
                return "FoxInteractiveID";

            else if (siteCode == SiteCode.GameFootage)
                return "GameFootage";
            else if (siteCode == SiteCode.Games)
                return "Games";
            else if (siteCode == SiteCode.Genre)
                return "Genre";
            else if (siteCode == SiteCode.GSPSoftware)
                return "GSPSoftware";
            else if (siteCode == SiteCode.GTInteractiveID)
                return "GTInteractiveID";

            else if (siteCode == SiteCode.HighSierraVolumeDescriptor)
                return "HighSierraVolumeDescriptor";

            else if (siteCode == SiteCode.InternalName)
                return "InternalName";
            else if (siteCode == SiteCode.InternalSerialName)
                return "InternalSerialName";
            else if (siteCode == SiteCode.InterplayID)
                return "InterplayID";
            else if (siteCode == SiteCode.ISBN)
                return "ISBN";
            else if (siteCode == SiteCode.ISSN)
                return "ISSN";

            else if (siteCode == SiteCode.JASRACID)
                return "JASRACID";

            else if (siteCode == SiteCode.KingRecordsID)
                return "KingRecordsID";
            else if (siteCode == SiteCode.KoeiID)
                return "KoeiID";
            else if (siteCode == SiteCode.KonamiID)
                return "KonamiID";

            else if (siteCode == SiteCode.LucasArtsID)
                return "LucasArtsID";

            else if (siteCode == SiteCode.MicrosoftID)
                return "MicrosoftID";
            else if (siteCode == SiteCode.Multisession)
                return "Multisession";

            else if (siteCode == SiteCode.NaganoID)
                return "NaganoID";
            else if (siteCode == SiteCode.NamcoID)
                return "NamcoID";
            else if (siteCode == SiteCode.NetYarozeGames)
                return "NetYarozeGames";

            else if (siteCode == SiteCode.NipponIchiSoftwareID)
                return "NipponIchiSoftwareID";

            else if (siteCode == SiteCode.OriginID)
                return "OriginID";

            else if (siteCode == SiteCode.Patches)
                return "Patches";
            else if (siteCode == SiteCode.PCMacHybrid)
                return "PCMacHybrid";
            else if (siteCode == SiteCode.PFIHash)
                return "PFIHash";
            else if (siteCode == SiteCode.PlayableDemos)
                return "PlayableDemos";
            else if (siteCode == SiteCode.PonyCanyonID)
                return "PonyCanyonID";
            else if (siteCode == SiteCode.PostgapType)
                return "PostgapType";
            else if (siteCode == SiteCode.PPN)
                return "PPN";
            else if (siteCode == SiteCode.Protection)
                return "Protection";

            else if (siteCode == SiteCode.RingPerfectAudioOffset)
                return "RingPerfectAudioOffset";
            else if (siteCode == SiteCode.RollingDemos)
                return "RollingDemos";

            else if (siteCode == SiteCode.Savegames)
                return "Savegames";
            else if (siteCode == SiteCode.SegaID)
                return "SegaID";
            else if (siteCode == SiteCode.SelenID)
                return "SelenID";
            else if (siteCode == SiteCode.Series)
                return "Series";
            else if (siteCode == SiteCode.SierraID)
                return "SierraID";
            else if (siteCode == SiteCode.SSHash)
                return "SSHash";
            else if (siteCode == SiteCode.SSVersion)
                return "SSVersion";
            else if (siteCode == SiteCode.SteamAppID)
                return "SteamAppID";
            else if (siteCode == SiteCode.SteamCsmCsdDepotID)
                return "SteamCsmCsdDepotID";
            else if (siteCode == SiteCode.SteamSimSidDepotID)
                return "SteamSimSidDepotID";

            else if (siteCode == SiteCode.TaitoID)
                return "TaitoID";
            else if (siteCode == SiteCode.TechDemos)
                return "TechDemos";
            else if (siteCode == SiteCode.TitleID)
                return "TitleID";
            else if (siteCode == SiteCode.TwoKGamesID)
                return "TwoKGamesID";

            else if (siteCode == SiteCode.UbisoftID)
                return "UbisoftID";

            else if (siteCode == SiteCode.ValveID)
                return "ValveID";
            else if (siteCode == SiteCode.VFCCode)
                return "VFCCode";
            else if (siteCode == SiteCode.Videos)
                return "Videos";
            else if (siteCode == SiteCode.VolumeLabel)
                return "VolumeLabel";
            else if (siteCode == SiteCode.VCD)
                return "VCD";

            else if (siteCode == SiteCode.XeMID)
                return "XeMID";
            else if (siteCode == SiteCode.XMID)
                return "XMID";

            return null;
        }

        /// <summary>
        /// Convert an index string to the corresponding site code
        /// </summary>
        private static SiteCode? ConvertStringToSiteCode(string? str)
        {
            return str switch
            {
                "AcclaimID" => SiteCode.AcclaimID,
                "AccoladeID" => SiteCode.AccoladeID,
                "ActivisionID" => SiteCode.ActivisionID,
                "AdditionalBCAData" => SiteCode.AdditionalBCAData,
                "AlternativeTitle" => SiteCode.AlternativeTitle,
                "AlternativeForeignTitle" => SiteCode.AlternativeForeignTitle,
                "Applications" => SiteCode.Applications,

                "BandaiID" => SiteCode.BandaiID,
                "BBFCRegistrationNumber" => SiteCode.BBFCRegistrationNumber,
                "BethesdaID" => SiteCode.BethesdaID,

                "CDProjektID" => SiteCode.CDProjektID,
                "CompatibleOS" => SiteCode.CompatibleOS,
                "CoverID" => SiteCode.CoverID,

                "DiceMultimedia" => SiteCode.DiceMultimedia,
                "DiscHologramID" => SiteCode.DiscHologramID,
                "DiscID" => SiteCode.DiscID,
                "DiscTitleNonLatin" => SiteCode.DiscTitleNonLatin,
                "DisneyInteractiveID" => SiteCode.DisneyInteractiveID,
                "DMIHash" => SiteCode.DMIHash,
                "DNASDiscID" => SiteCode.DNASDiscID,

                "EditionNonLatin" => SiteCode.EditionNonLatin,
                "EidosID" => SiteCode.EidosID,
                "ElectronicArtsID" => SiteCode.ElectronicArtsID,
                "Extras" => SiteCode.Extras,

                "Filename" => SiteCode.Filename,
                "FocusMultimedia" => SiteCode.FocusMultimedia,
                "FoxInteractiveID" => SiteCode.FoxInteractiveID,

                "GameFootage" => SiteCode.GameFootage,
                "Games" => SiteCode.Games,
                "Genre" => SiteCode.Genre,
                "GSPSoftware" => SiteCode.GSPSoftware,
                "GTInteractiveID" => SiteCode.GTInteractiveID,

                "HighSierraVolumeDescriptor" => SiteCode.HighSierraVolumeDescriptor,

                "InternalName" => SiteCode.InternalName,
                "InternalSerialName" => SiteCode.InternalSerialName,
                "InterplayID" => SiteCode.InterplayID,
                "ISBN" => SiteCode.ISBN,
                "ISSN" => SiteCode.ISSN,

                "JASRACID" => SiteCode.JASRACID,

                "KingRecordsID" => SiteCode.KingRecordsID,
                "KoeiID" => SiteCode.KoeiID,
                "KonamiID" => SiteCode.KonamiID,

                "LucasArtsID" => SiteCode.LucasArtsID,

                "MicrosoftID" => SiteCode.MicrosoftID,
                "Multisession" => SiteCode.Multisession,

                "NaganoID" => SiteCode.NaganoID,
                "NamcoID" => SiteCode.NamcoID,
                "NetYarozeGames" => SiteCode.NetYarozeGames,

                "NipponIchiSoftwareID" => SiteCode.NipponIchiSoftwareID,

                "OriginID" => SiteCode.OriginID,

                "Patches" => SiteCode.Patches,
                "PCMacHybrid" => SiteCode.PCMacHybrid,
                "PFIHash" => SiteCode.PFIHash,
                "PlayableDemos" => SiteCode.PlayableDemos,
                "PonyCanyonID" => SiteCode.PonyCanyonID,
                "PostgapType" => SiteCode.PostgapType,
                "PPN" => SiteCode.PPN,
                "Protection" => SiteCode.Protection,

                "RingPerfectAudioOffset" => SiteCode.RingPerfectAudioOffset,
                "RollingDemos" => SiteCode.RollingDemos,

                "Savegames" => SiteCode.Savegames,
                "SegaID" => SiteCode.SegaID,
                "SelenID" => SiteCode.SelenID,
                "Series" => SiteCode.Series,
                "SierraID" => SiteCode.SierraID,
                "SSHash" => SiteCode.SSHash,
                "SSVersion" => SiteCode.SSVersion,
                "SteamAppID" => SiteCode.SteamAppID,
                "SteamCsmCsdDepotID" => SiteCode.SteamCsmCsdDepotID,
                "SteamSimSidDepotID" => SiteCode.SteamSimSidDepotID,

                "TaitoID" => SiteCode.TaitoID,
                "TechDemos" => SiteCode.TechDemos,
                "TitleID" => SiteCode.TitleID,
                "TwoKGamesID" => SiteCode.TwoKGamesID,

                "UbisoftID" => SiteCode.UbisoftID,

                "ValveID" => SiteCode.ValveID,
                "VFCCode" => SiteCode.VFCCode,
                "Videos" => SiteCode.Videos,
                "VolumeLabel" => SiteCode.VolumeLabel,
                "VCD" => SiteCode.VCD,

                "XeMID" => SiteCode.XeMID,
                "XMID" => SiteCode.XMID,

                _ => null,
            };
        }

        #endregion
    }
}
