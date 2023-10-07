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
        internal void Load(DiscInformationWindow parent)
        {
            parent.CategoryComboBox.SelectedIndex = Categories.FindIndex(r => r == SubmissionInfo.CommonDiscInfo.Category);
            parent.RegionComboBox.SelectedIndex = Regions.FindIndex(r => r == SubmissionInfo.CommonDiscInfo.Region);
            if (SubmissionInfo.CommonDiscInfo.Languages != null)
                Languages.ForEach(l => l.IsChecked = SubmissionInfo.CommonDiscInfo.Languages.Contains(l));
            if (SubmissionInfo.CommonDiscInfo.LanguageSelection != null)
                LanguageSelections.ForEach(ls => ls.IsChecked = SubmissionInfo.CommonDiscInfo.LanguageSelection.Contains(ls));

            // TODO: Figure out if this can be automatically mapped instead

            // Comment Fields
            if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields != null)
            {
                // Additional Information
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.AlternativeTitle))
                    parent.AlternativeTitleTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.AlternativeTitle];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.AlternativeForeignTitle))
                    parent.AlternativeForeignTitleTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.AlternativeForeignTitle];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.Genre))
                    parent.GenreTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.Genre];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.PostgapType))
                    parent.PostgapTypeCheckBox.IsChecked = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.PostgapType] != null;
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.Series))
                    parent.SeriesTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.Series];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.VCD))
                    parent.VCDCheckBox.IsChecked = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.VCD] != null;

                // Physical Identifiers
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.BBFCRegistrationNumber))
                    parent.BBFCRegistrationNumberTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.BBFCRegistrationNumber];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.CDProjektID))
                    parent.CDProjektIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.CDProjektID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.DiscHologramID))
                    parent.DiscHologramIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.DiscHologramID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.DNASDiscID))
                    parent.DNASDiscIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.DNASDiscID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.ISBN))
                    parent.ISBNTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.ISBN];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.ISSN))
                    parent.ISSNTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.ISSN];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.PPN))
                    parent.PPNTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.PPN];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.VFCCode))
                    parent.VFCCodeTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.VFCCode];

                // Publisher Identifiers
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.AcclaimID))
                    parent.AcclaimIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.AcclaimID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.ActivisionID))
                    parent.ActivisionIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.ActivisionID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.BandaiID))
                    parent.BandaiIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.BandaiID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.ElectronicArtsID))
                    parent.ElectronicArtsIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.ElectronicArtsID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.FoxInteractiveID))
                    parent.FoxInteractiveIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.FoxInteractiveID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.GTInteractiveID))
                    parent.GTInteractiveIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.GTInteractiveID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.JASRACID))
                    parent.JASRACIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.JASRACID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.KingRecordsID))
                    parent.KingRecordsIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.KingRecordsID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.KoeiID))
                    parent.KoeiIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.KoeiID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.KonamiID))
                    parent.KonamiIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.KonamiID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.LucasArtsID))
                    parent.LucasArtsIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.LucasArtsID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.MicrosoftID))
                    parent.MicrosoftIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.MicrosoftID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.NaganoID))
                    parent.NaganoIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.NaganoID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.NamcoID))
                    parent.NamcoIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.NamcoID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.NipponIchiSoftwareID))
                    parent.NipponIchiSoftwareIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.NipponIchiSoftwareID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.OriginID))
                    parent.OriginIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.OriginID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.PonyCanyonID))
                    parent.PonyCanyonIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.PonyCanyonID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.SegaID))
                    parent.SegaIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.SegaID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.SelenID))
                    parent.SelenIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.SelenID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.SierraID))
                    parent.SierraIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.SierraID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.TaitoID))
                    parent.TaitoIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.TaitoID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.UbisoftID))
                    parent.UbisoftIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.UbisoftID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.ValveID))
                    parent.ValveIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.ValveID];

                // Read-Only Information
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.DMIHash))
                    parent.DMIHash.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.DMIHash];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.Filename))
                    parent.Filename.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.Filename];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.InternalName))
                    parent.InternalName.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.InternalName];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.InternalSerialName))
                    parent.InternalSerialName.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.InternalSerialName];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.Multisession))
                    parent.Multisession.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.Multisession];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.PFIHash))
                    parent.PFIHash.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.PFIHash];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.RingNonZeroDataStart))
                    parent.RingNonZeroDataStart.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.RingNonZeroDataStart];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.SSHash))
                    parent.SSHash.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.SSHash];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.SSVersion))
                    parent.SSVersion.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.SSVersion];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.UniversalHash))
                    parent.UniversalHash.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.UniversalHash];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.VolumeLabel))
                    parent.VolumeLabel.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.VolumeLabel];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.XeMID))
                    parent.XeMID.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.XeMID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.XMID))
                    parent.XMID.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.XMID];
            }

            // Content Fields
            if (SubmissionInfo.CommonDiscInfo.ContentsSpecialFields != null)
            {
                // Games
                if (SubmissionInfo.CommonDiscInfo.ContentsSpecialFields.ContainsKey(SiteCode.Games))
                    parent.GamesTextBox.Text = SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.Games];
                if (SubmissionInfo.CommonDiscInfo.ContentsSpecialFields.ContainsKey(SiteCode.NetYarozeGames))
                    parent.NetYarozeGamesTextBox.Text = SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.NetYarozeGames];

                // Demos
                if (SubmissionInfo.CommonDiscInfo.ContentsSpecialFields.ContainsKey(SiteCode.PlayableDemos))
                    parent.PlayableDemosTextBox.Text = SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.PlayableDemos];
                if (SubmissionInfo.CommonDiscInfo.ContentsSpecialFields.ContainsKey(SiteCode.RollingDemos))
                    parent.RollingDemosTextBox.Text = SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.RollingDemos];
                if (SubmissionInfo.CommonDiscInfo.ContentsSpecialFields.ContainsKey(SiteCode.TechDemos))
                    parent.TechDemosTextBox.Text = SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.TechDemos];

                // Video
                if (SubmissionInfo.CommonDiscInfo.ContentsSpecialFields.ContainsKey(SiteCode.GameFootage))
                    parent.GameFootageTextBox.Text = SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.GameFootage];
                if (SubmissionInfo.CommonDiscInfo.ContentsSpecialFields.ContainsKey(SiteCode.Videos))
                    parent.VideosTextBox.Text = SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.Videos];

                // Miscellaneous
                if (SubmissionInfo.CommonDiscInfo.ContentsSpecialFields.ContainsKey(SiteCode.Patches))
                    parent.PatchesTextBox.Text = SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.Patches];
                if (SubmissionInfo.CommonDiscInfo.ContentsSpecialFields.ContainsKey(SiteCode.Savegames))
                    parent.SavegamesTextBox.Text = SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.Savegames];
                if (SubmissionInfo.CommonDiscInfo.ContentsSpecialFields.ContainsKey(SiteCode.Extras))
                    parent.ExtrasTextBox.Text = SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.Extras];
            }
        }

        /// <summary>
        /// Save the current contents of the UI to the base SubmissionInfo
        /// </summary>
        internal void Save(DiscInformationWindow parent)
        {
            SubmissionInfo.CommonDiscInfo.Category = (parent.CategoryComboBox.SelectedItem as Element<DiscCategory>)?.Value ?? DiscCategory.Games;
            SubmissionInfo.CommonDiscInfo.Region = (parent.RegionComboBox.SelectedItem as Element<Region>)?.Value ?? Region.World;
            SubmissionInfo.CommonDiscInfo.Languages = Languages.Where(l => l.IsChecked).Select(l => l?.Value).ToArray();
            if (!SubmissionInfo.CommonDiscInfo.Languages.Any())
                SubmissionInfo.CommonDiscInfo.Languages = new Language?[] { null };
            SubmissionInfo.CommonDiscInfo.LanguageSelection = LanguageSelections.Where(ls => ls.IsChecked).Select(ls => ls?.Value).ToArray();

            // TODO: Figure out if this can be automatically mapped instead

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
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.AlternativeTitle] = parent.AlternativeTitleTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.AlternativeForeignTitle] = parent.AlternativeForeignTitleTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.Genre] = parent.GenreTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.PostgapType] = parent.PostgapTypeCheckBox.IsChecked?.ToString();
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.Series] = parent.SeriesTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.VCD] = parent.VCDCheckBox.IsChecked?.ToString();

            // Physical Identifiers
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.BBFCRegistrationNumber] = parent.BBFCRegistrationNumberTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.CDProjektID] = parent.CDProjektIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.DiscHologramID] = parent.DiscHologramIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.DNASDiscID] = parent.DNASDiscIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.ISBN] = parent.ISBNTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.ISSN] = parent.ISSNTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.PPN] = parent.PPNTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.VFCCode] = parent.VFCCodeTextBox.Text;

            // Publisher Identifiers
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.AcclaimID] = parent.AcclaimIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.ActivisionID] = parent.ActivisionIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.BandaiID] = parent.BandaiIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.ElectronicArtsID] = parent.ElectronicArtsIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.FoxInteractiveID] = parent.FoxInteractiveIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.GTInteractiveID] = parent.GTInteractiveIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.JASRACID] = parent.JASRACIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.KingRecordsID] = parent.KingRecordsIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.KoeiID] = parent.KoeiIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.KonamiID] = parent.KonamiIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.LucasArtsID] = parent.LucasArtsIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.MicrosoftID] = parent.MicrosoftIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.NaganoID] = parent.NaganoIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.NamcoID] = parent.NamcoIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.NipponIchiSoftwareID] = parent.NipponIchiSoftwareIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.OriginID] = parent.OriginIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.PonyCanyonID] = parent.PonyCanyonIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.SegaID] = parent.SegaIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.SelenID] = parent.SelenIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.SierraID] = parent.SierraIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.TaitoID] = parent.TaitoIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.UbisoftID] = parent.UbisoftIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.ValveID] = parent.ValveIDTextBox.Text;

            // Read-Only Information
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.DMIHash] = parent.DMIHash.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.Filename] = parent.Filename.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.InternalName] = parent.InternalName.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.InternalSerialName] = parent.InternalSerialName.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.Multisession] = parent.Multisession.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.PFIHash] = parent.PFIHash.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.RingNonZeroDataStart] = parent.RingNonZeroDataStart.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.SSHash] = parent.SSHash.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.SSVersion] = parent.SSVersion.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.UniversalHash] = parent.UniversalHash.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.VolumeLabel] = parent.VolumeLabel.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.XeMID] = parent.XeMID.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.XMID] = parent.XMID.Text;

            #endregion

            #region Content Fields

            // Games
            SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.Games] = parent.GamesTextBox.Text;
            SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.NetYarozeGames] = parent.NetYarozeGamesTextBox.Text;

            // Demos
            SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.PlayableDemos] = parent.PlayableDemosTextBox.Text;
            SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.RollingDemos] = parent.RollingDemosTextBox.Text;
            SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.TechDemos] = parent.TechDemosTextBox.Text;

            // Video
            SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.GameFootage] = parent.GameFootageTextBox.Text;
            SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.Videos] = parent.VideosTextBox.Text;

            // Miscellaneous
            SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.Patches] = parent.PatchesTextBox.Text;
            SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.Savegames] = parent.SavegamesTextBox.Text;
            SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.Extras] = parent.ExtrasTextBox.Text;

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
