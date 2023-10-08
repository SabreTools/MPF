using System.Collections.Generic;
using System.Linq;
using MPF.Core.Data;
using MPF.Core.UI.ComboBoxItems;
using MPF.Core.Utilities;
using MPF.UI.Core.Windows;
using SabreTools.RedumpLib.Data;

namespace MPF.UI.Core.ViewModels
{
    public class DiscInformationViewModel
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
        public List<Element<DiscCategory>> Categories { get; private set; } = Element<DiscCategory>.GenerateElements().ToList();

        /// <summary>
        /// List of available regions
        /// </summary>
        public List<Element<Region>> Regions { get; private set; } = Element<Region>.GenerateElements().ToList();

        /// <summary>
        /// List of Redump-supported Regions
        /// </summary>
        private static readonly List<Region> RedumpRegions = new List<Region>
        {
            Region.Argentina,
            Region.Asia,
            Region.AsiaEurope,
            Region.AsiaUSA,
            Region.Australia,
            Region.AustraliaGermany,
            Region.AustraliaNewZealand,
            Region.Austria,
            Region.AustriaSwitzerland,
            Region.Belarus,
            Region.Belgium,
            Region.BelgiumNetherlands,
            Region.Brazil,
            Region.Bulgaria,
            Region.Canada,
            Region.China,
            Region.Croatia,
            Region.Czechia,
            Region.Denmark,
            Region.Estonia,
            Region.Europe,
            Region.EuropeAsia,
            Region.EuropeAustralia,
            Region.EuropeCanada,
            Region.EuropeGermany,
            Region.Export,
            Region.Finland,
            Region.France,
            Region.FranceSpain,
            Region.Germany,
            Region.GreaterChina,
            Region.Greece,
            Region.Hungary,
            Region.Iceland,
            Region.India,
            Region.Ireland,
            Region.Israel,
            Region.Italy,
            Region.Japan,
            Region.JapanAsia,
            Region.JapanEurope,
            Region.JapanKorea,
            Region.JapanUSA,
            Region.SouthKorea,
            Region.LatinAmerica,
            Region.Lithuania,
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
            Region.SpainPortugal,
            Region.Sweden,
            Region.Switzerland,
            Region.Taiwan,
            Region.Thailand,
            Region.Turkey,
            Region.UnitedArabEmirates,
            Region.UnitedKingdom,
            Region.UKAustralia,
            Region.Ukraine,
            Region.UnitedStatesOfAmerica,
            Region.USAAsia,
            Region.USAAustralia,
            Region.USABrazil,
            Region.USACanada,
            Region.USAEurope,
            Region.USAGermany,
            Region.USAJapan,
            Region.USAKorea,
            Region.World,
        };

        /// <summary>
        /// List of available languages
        /// </summary>
        public List<Element<Language>> Languages { get; private set; } = Element<Language>.GenerateElements().ToList();

        /// <summary>
        /// List of Redump-supported Languages
        /// </summary>
        private static readonly List<Language> RedumpLanguages = new List<Language>
        {
            Language.Afrikaans,
            Language.Albanian,
            Language.Arabic,
            Language.Armenian,
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
            Language.Gaelic,
            Language.German,
            Language.Greek,
            Language.Hebrew,
            Language.Hindi,
            Language.Hungarian,
            Language.Icelandic,
            Language.Indonesian,
            Language.Italian,
            Language.Japanese,
            Language.Korean,
            Language.Latin,
            Language.Latvian,
            Language.Lithuanian,
            Language.Macedonian,
            Language.Norwegian,
            Language.Polish,
            Language.Portuguese,
            Language.Panjabi,
            Language.Romanian,
            Language.Russian,
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
        };

        /// <summary>
        /// List of available languages
        /// </summary>
        public List<Element<LanguageSelection>> LanguageSelections { get; private set; } = Element<LanguageSelection>.GenerateElements().ToList();

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public DiscInformationViewModel(Options options, SubmissionInfo submissionInfo)
        {
            Options = options;
            SubmissionInfo = submissionInfo.Clone() as SubmissionInfo ?? new SubmissionInfo();
        }

        #region Helpers

        /// <summary>
        /// Load the current contents of the base SubmissionInfo to the UI
        /// </summary>
        /// TODO: Convert selected list item to binding
        internal void Load(DiscInformationWindow parent)
        {
            parent.CategoryComboBox.SelectedIndex = Categories.FindIndex(r => r == SubmissionInfo.CommonDiscInfo.Category);
            parent.RegionComboBox.SelectedIndex = Regions.FindIndex(r => r == SubmissionInfo.CommonDiscInfo.Region);
            if (SubmissionInfo.CommonDiscInfo.Languages != null)
                Languages.ForEach(l => l.IsChecked = SubmissionInfo.CommonDiscInfo.Languages.Contains(l));
            if (SubmissionInfo.CommonDiscInfo.LanguageSelection != null)
                LanguageSelections.ForEach(ls => ls.IsChecked = SubmissionInfo.CommonDiscInfo.LanguageSelection.Contains(ls));

            // Comment Fields
            if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields != null)
            {
                // Additional Information
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.PostgapType))
                    parent.PostgapTypeCheckBox.IsChecked = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.PostgapType] != null;
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.VCD))
                    parent.VCDCheckBox.IsChecked = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.VCD] != null;
            }
        }

        /// <summary>
        /// Save the current contents of the UI to the base SubmissionInfo
        /// </summary>
        /// TODO: Convert selected list item to binding
        internal void Save(DiscInformationWindow parent)
        {
            SubmissionInfo.CommonDiscInfo.Category = (parent.CategoryComboBox.SelectedItem as Element<DiscCategory>)?.Value ?? DiscCategory.Games;
            SubmissionInfo.CommonDiscInfo.Region = (parent.RegionComboBox.SelectedItem as Element<Region>)?.Value ?? Region.World;
            SubmissionInfo.CommonDiscInfo.Languages = Languages.Where(l => l.IsChecked).Select(l => l?.Value).ToArray();
            if (!SubmissionInfo.CommonDiscInfo.Languages.Any())
                SubmissionInfo.CommonDiscInfo.Languages = new Language?[] { null };
            SubmissionInfo.CommonDiscInfo.LanguageSelection = LanguageSelections.Where(ls => ls.IsChecked).Select(ls => ls?.Value).ToArray();

            // Initialize the dictionaries, if needed
            if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields == null)
#if NET48
                SubmissionInfo.CommonDiscInfo.CommentsSpecialFields = new Dictionary<SiteCode?, string>();
#else
                SubmissionInfo.CommonDiscInfo.CommentsSpecialFields = new Dictionary<SiteCode, string>();
#endif
            if (SubmissionInfo.CommonDiscInfo.ContentsSpecialFields == null)
#if NET48
                SubmissionInfo.CommonDiscInfo.ContentsSpecialFields = new Dictionary<SiteCode?, string>();
#else
                SubmissionInfo.CommonDiscInfo.ContentsSpecialFields = new Dictionary<SiteCode, string>();
#endif

            #region Comment Fields

            // Additional Information
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.PostgapType] = parent.PostgapTypeCheckBox.IsChecked?.ToString();
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.VCD] = parent.VCDCheckBox.IsChecked?.ToString();

            #endregion
        }

        /// <summary>
        /// Repopulate the list of Languages based on Redump support
        /// </summary>
        internal void SetRedumpLanguages()
        {
            this.Languages = RedumpLanguages.Select(l => new Element<Language>(l)).ToList();
        }

        /// <summary>
        /// Repopulate the list of Regions based on Redump support
        /// </summary>
        internal void SetRedumpRegions()
        {
            this.Regions = RedumpRegions.Select(r => new Element<Region>(r)).ToList();
        }

        #endregion
    }
}
