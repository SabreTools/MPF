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

        #endregion
    }
}
