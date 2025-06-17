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
        private static readonly List<Region> RedumpRegions = new()
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
        public List<Element<Language>> Languages { get; private set; }
            = Element<Language>.GenerateElements();

        /// <summary>
        /// List of Redump-supported Languages
        /// </summary>
        private static readonly List<Language> RedumpLanguages = new()
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
        public List<Element<LanguageSelection>> LanguageSelections { get; private set; }
            = Element<LanguageSelection>.GenerateElements();

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
            if (SubmissionInfo.CommonDiscInfo?.Languages != null)
                Languages.ForEach(l => l.IsChecked = Array.IndexOf(SubmissionInfo.CommonDiscInfo.Languages, l) > -1);
            if (SubmissionInfo.CommonDiscInfo?.LanguageSelection != null)
                LanguageSelections.ForEach(ls => ls.IsChecked = Array.IndexOf(SubmissionInfo.CommonDiscInfo.LanguageSelection, ls) > -1);
        }

        /// <summary>
        /// Save the current contents of the UI to the base SubmissionInfo
        /// </summary>
        /// TODO: Convert selected list item to binding
        public void Save()
        {
            if (SubmissionInfo.CommonDiscInfo == null)
                SubmissionInfo.CommonDiscInfo = new CommonDiscInfoSection();
            SubmissionInfo.CommonDiscInfo.Languages = [.. Languages.FindAll(l => l.IsChecked).ConvertAll(l => l?.Value)];
            if (SubmissionInfo.CommonDiscInfo.Languages.Length == 0)
                SubmissionInfo.CommonDiscInfo.Languages = [null];
            SubmissionInfo.CommonDiscInfo.LanguageSelection = [.. LanguageSelections.FindAll(ls => ls.IsChecked).ConvertAll(ls => ls?.Value)];
            SubmissionInfo.CommonDiscInfo.Title = FrontendTool.NormalizeDiscTitle(SubmissionInfo.CommonDiscInfo.Title, SubmissionInfo.CommonDiscInfo.Languages);
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
