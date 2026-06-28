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
        public List<Element<Region>> Regions { get; private set; }
            = Element<Region>.GenerateElements();

        /// <summary>
        /// List of Redump-supported Regions
        /// </summary>
        private static readonly List<Region> RedumpRegions =
        [
            Region.Argentina,
            Region.Asia,
            Region.Australia,
            Region.Austria,
            Region.Azerbaijan,
            Region.Belarus,
            Region.Belgium,
            Region.Brazil,
            Region.Bulgaria,
            Region.Canada,
            Region.China,
            Region.Croatia,
            Region.Czechia,
            Region.Denmark,
            Region.Estonia,
            Region.Europe,
            Region.Export,
            Region.Finland,
            Region.France,
            Region.Germany,
            Region.Greece,
            Region.Hungary,
            Region.Iceland,
            Region.India,
            Region.Iran,
            Region.Ireland,
            Region.Israel,
            Region.Italy,
            Region.Japan,
            Region.SouthKorea,
            Region.LatinAmerica,
            Region.Lithuania,
            Region.Mexico,
            Region.Netherlands,
            Region.NewZealand,
            Region.Norway,
            Region.Poland,
            Region.Portugal,
            Region.Romania,
            Region.RussianFederation,
            Region.Scandinavia,
            Region.Serbia,
            Region.Singapore,
            Region.Slovakia,
            Region.SouthAfrica,
            Region.Spain,
            Region.Sweden,
            Region.Switzerland,
            Region.Taiwan,
            Region.Thailand,
            Region.Turkey,
            Region.UnitedKingdom,
            Region.Ukraine,
            Region.UnitedArabEmirates,
            Region.Ukraine,
            Region.UnitedStatesOfAmerica,
            Region.World,
        ];

        /// <summary>
        /// List of available languages
        /// </summary>
        public List<Element<Language>> Languages { get; private set; }
            = Element<Language>.GenerateElements();

        /// <summary>
        /// List of Redump-supported Languages
        /// </summary>
        private static readonly List<Language> RedumpLanguages =
        [
            Language.Afrikaans,
            Language.Albanian,
            Language.Arabic,
            Language.Armenian,
            Language.Azerbaijani,
            Language.Basque,
            Language.Belarusian,
            Language.Bulgarian,
            Language.Catalan,
            Language.Chinese,
            Language.Croatian,
            Language.Czech,
            Language.Danish,
            Language.Dutch,
            Language.English,
            Language.Estonian,
            Language.Finnish,
            Language.French,
            Language.Galician,
            Language.German,
            Language.Greek,
            Language.Hebrew,
            Language.Hindi,
            Language.Hungarian,
            Language.Icelandic,
            Language.Indonesian,
            Language.Irish,
            Language.Italian,
            Language.Japanese,
            Language.Korean,
            Language.Latin,
            Language.Latvian,
            Language.Lithuanian,
            Language.Macedonian,
            Language.Malay,
            Language.Maori,
            Language.Norwegian,
            Language.Persian,
            Language.Polish,
            Language.Portuguese,
            Language.Panjabi,
            Language.Romanian,
            Language.Russian,
            Language.Scots,
            Language.Serbian,
            Language.Slovak,
            Language.Slovenian,
            Language.Spanish,
            Language.Swedish,
            Language.Tamil,
            Language.Thai,
            Language.Turkish,
            Language.Ukrainian,
            Language.Vietnamese,
            Language.Welsh,
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
            Languages = RedumpLanguages.ConvertAll(l => new Element<Language>(l));
        }

        /// <summary>
        /// Repopulate the list of Regions based on Redump support
        /// </summary>
        public void SetRedumpRegions()
        {
            Regions = RedumpRegions.ConvertAll(r => new Element<Region>(r));
        }

        #endregion
    }
}
