using System.Collections.Generic;
using System.Linq;
using System.Windows;
using MPF.Core.Data;
using MPF.Core.Utilities;
using MPF.UI.Core.ComboBoxItems;
using MPF.UI.Core.Windows;
using RedumpLib.Data;

namespace MPF.UI.Core.ViewModels
{
    public class DiscInformationViewModel
    {
        #region Fields

        /// <summary>
        /// Parent DiscInformationWindow object
        /// </summary>
        public DiscInformationWindow Parent { get; private set; }

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
        public DiscInformationViewModel(DiscInformationWindow parent, Options options, SubmissionInfo submissionInfo)
        {
            Parent = parent;
            Options = options;
            SubmissionInfo = submissionInfo.Clone() as SubmissionInfo ?? new SubmissionInfo();

            // Limit lists, if necessary
            if (this.Options.EnableRedumpCompatibility)
            {
                SetRedumpRegions();
                SetRedumpLanguages();
            }

            // Add handlers
            Parent.AcceptButton.Click += OnAcceptClick;
            Parent.CancelButton.Click += OnCancelClick;
            Parent.RingCodeGuideButton.Click += OnRingCodeGuideClick;

            // Update UI with new values
            ManipulateFields();
            Load();
        }

        #region Helpers

        /// <summary>
        /// Enable tab entry on supported fields
        /// </summary>
        private void EnableTabsInInputFields()
        {
            // Additional Information
            Parent.CommentsTextBox.Tab = true;

            // Contents
            Parent.GeneralContent.Tab = true;
            Parent.GamesTextBox.Tab = true;
            Parent.NetYarozeGamesTextBox.Tab = true;
            Parent.PlayableDemosTextBox.Tab = true;
            Parent.RollingDemosTextBox.Tab = true;
            Parent.TechDemosTextBox.Tab = true;
            Parent.GameFootageTextBox.Tab = true;
            Parent.VideosTextBox.Tab = true;
            Parent.PatchesTextBox.Tab = true;
            Parent.SavegamesTextBox.Tab = true;
            Parent.ExtrasTextBox.Tab = true;

            // L0
            Parent.L0MasteringRing.Tab = true;
            Parent.L0MasteringSID.Tab = true;
            Parent.L0Toolstamp.Tab = true;
            Parent.L0MouldSID.Tab = true;
            Parent.L0AdditionalMould.Tab = true;

            // L1
            Parent.L1MasteringRing.Tab = true;
            Parent.L1MasteringSID.Tab = true;
            Parent.L1Toolstamp.Tab = true;
            Parent.L1MouldSID.Tab = true;
            Parent.L1AdditionalMould.Tab = true;

            // L2
            Parent.L2MasteringRing.Tab = true;
            Parent.L2MasteringSID.Tab = true;
            Parent.L2Toolstamp.Tab = true;

            // L3
            Parent.L3MasteringRing.Tab = true;
            Parent.L3MasteringSID.Tab = true;
            Parent.L3Toolstamp.Tab = true;
        }

        /// <summary>
        /// Hide any optional, read-only fields if they don't have a value
        /// </summary>
        private void HideReadOnlyFields()
        {
            if (SubmissionInfo?.FullyMatchedID == null)
                Parent.FullyMatchedID.Visibility = Visibility.Collapsed;
            else
                Parent.FullyMatchedID.Text = SubmissionInfo.FullyMatchedID.ToString();
            if (SubmissionInfo?.PartiallyMatchedIDs == null)
                Parent.PartiallyMatchedIDs.Visibility = Visibility.Collapsed;
            else
                Parent.PartiallyMatchedIDs.Text = string.Join(", ", SubmissionInfo.PartiallyMatchedIDs);
            if (SubmissionInfo?.CopyProtection?.AntiModchip == null)
                Parent.AntiModchip.Visibility = Visibility.Collapsed;
            else
                Parent.AntiModchip.Text = SubmissionInfo.CopyProtection.AntiModchip.LongName();
            if (SubmissionInfo?.TracksAndWriteOffsets?.OtherWriteOffsets == null)
                Parent.DiscOffset.Visibility = Visibility.Collapsed;
            else
                Parent.DiscOffset.Text = SubmissionInfo.TracksAndWriteOffsets.OtherWriteOffsets;
            if (SubmissionInfo?.CommonDiscInfo?.CommentsSpecialFields.Keys.Contains(SiteCode.DMIHash) != true)
                Parent.DMIHash.Visibility = Visibility.Collapsed;
            if (string.IsNullOrWhiteSpace(SubmissionInfo?.CommonDiscInfo?.ErrorsCount))
                Parent.ErrorsCount.Visibility = Visibility.Collapsed;
            if (string.IsNullOrWhiteSpace(SubmissionInfo?.CommonDiscInfo?.EXEDateBuildDate))
                Parent.EXEDateBuildDate.Visibility = Visibility.Collapsed;
            if (SubmissionInfo?.EDC?.EDC == null)
                Parent.EDC.Visibility = Visibility.Collapsed;
            else
                Parent.EDC.Text = SubmissionInfo.EDC.EDC.LongName();
            if (SubmissionInfo?.CommonDiscInfo?.CommentsSpecialFields.Keys.Contains(SiteCode.Filename) != true)
                Parent.Filename.Visibility = Visibility.Collapsed;
            if (string.IsNullOrWhiteSpace(SubmissionInfo?.Extras?.Header))
                Parent.Header.Visibility = Visibility.Collapsed;
            if (SubmissionInfo?.CommonDiscInfo?.CommentsSpecialFields.Keys.Contains(SiteCode.InternalName) != true)
                Parent.InternalName.Visibility = Visibility.Collapsed;
            if (SubmissionInfo?.CommonDiscInfo?.CommentsSpecialFields.Keys.Contains(SiteCode.InternalSerialName) != true)
                Parent.InternalSerialName.Visibility = Visibility.Collapsed;
            if (SubmissionInfo?.CopyProtection?.LibCrypt == null)
                Parent.LibCrypt.Visibility = Visibility.Collapsed;
            else
                Parent.LibCrypt.Text = SubmissionInfo.CopyProtection.LibCrypt.LongName();
            if (string.IsNullOrWhiteSpace(SubmissionInfo?.CopyProtection?.LibCryptData))
                Parent.LibCryptData.Visibility = Visibility.Collapsed;
            if (SubmissionInfo?.CommonDiscInfo?.CommentsSpecialFields.Keys.Contains(SiteCode.PFIHash) != true)
                Parent.PFIHash.Visibility = Visibility.Collapsed;
            if (string.IsNullOrWhiteSpace(SubmissionInfo?.Extras?.PIC))
                Parent.PIC.Visibility = Visibility.Collapsed;
            if (string.IsNullOrWhiteSpace(SubmissionInfo?.Extras?.PVD))
                Parent.PVD.Visibility = Visibility.Collapsed;
            if (SubmissionInfo?.CommonDiscInfo?.CommentsSpecialFields.Keys.Contains(SiteCode.Multisession) != true)
                Parent.Multisession.Visibility = Visibility.Collapsed;
            if (SubmissionInfo?.CommonDiscInfo?.CommentsSpecialFields.Keys.Contains(SiteCode.RingNonZeroDataStart) != true)
                Parent.RingNonZeroDataStart.Visibility = Visibility.Collapsed;
            if (string.IsNullOrWhiteSpace(SubmissionInfo?.CopyProtection?.SecuROMData))
                Parent.SecuROMData.Visibility = Visibility.Collapsed;
            if (SubmissionInfo?.CommonDiscInfo?.CommentsSpecialFields.Keys.Contains(SiteCode.SSHash) != true)
                Parent.SSHash.Visibility = Visibility.Collapsed;
            if (string.IsNullOrWhiteSpace(SubmissionInfo?.Extras?.SecuritySectorRanges))
                Parent.SecuritySectorRanges.Visibility = Visibility.Collapsed;
            if (SubmissionInfo?.CommonDiscInfo?.CommentsSpecialFields.Keys.Contains(SiteCode.SSVersion) != true)
                Parent.SSVersion.Visibility = Visibility.Collapsed;
            if (SubmissionInfo?.CommonDiscInfo?.CommentsSpecialFields.Keys.Contains(SiteCode.UniversalHash) != true)
                Parent.UniversalHash.Visibility = Visibility.Collapsed;
            if (SubmissionInfo?.CommonDiscInfo?.CommentsSpecialFields.Keys.Contains(SiteCode.VolumeLabel) != true)
                Parent.VolumeLabel.Visibility = Visibility.Collapsed;
            if (SubmissionInfo?.CommonDiscInfo?.CommentsSpecialFields.Keys.Contains(SiteCode.XeMID) != true)
                Parent.XeMID.Visibility = Visibility.Collapsed;
            if (SubmissionInfo?.CommonDiscInfo?.CommentsSpecialFields.Keys.Contains(SiteCode.XMID) != true)
                Parent.XMID.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Manipulate fields based on the current disc
        /// </summary>
        private void ManipulateFields()
        {
            // Enable tabs in all fields, if required
            if (this.Options.EnableTabsInInputFields)
                EnableTabsInInputFields();

            // Hide read-only fields that don't have values set
            HideReadOnlyFields();

            // Different media types mean different fields available
            UpdateFromDiscType();

            // Different systems mean different fields available
            UpdateFromSystemType();
        }

        /// <summary>
        /// Load the current contents of the base SubmissionInfo to the UI
        /// </summary>
        private void Load()
        {
            Parent.CategoryComboBox.SelectedIndex = Categories.FindIndex(r => r == SubmissionInfo.CommonDiscInfo.Category);
            Parent.RegionComboBox.SelectedIndex = Regions.FindIndex(r => r == SubmissionInfo.CommonDiscInfo.Region);
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
                    Parent.AlternativeTitleTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.AlternativeTitle];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.AlternativeForeignTitle))
                    Parent.AlternativeForeignTitleTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.AlternativeForeignTitle];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.Genre))
                    Parent.GenreTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.Genre];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.PostgapType))
                    Parent.PostgapTypeCheckBox.IsChecked = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.PostgapType] != null;
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.Series))
                    Parent.SeriesTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.Series];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.VCD))
                    Parent.VCDCheckBox.IsChecked = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.VCD] != null;

                // Physical Identifiers
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.BBFCRegistrationNumber))
                    Parent.BBFCRegistrationNumberTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.BBFCRegistrationNumber];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.CDProjektID))
                    Parent.CDProjektIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.CDProjektID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.DiscHologramID))
                    Parent.DiscHologramIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.DiscHologramID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.DNASDiscID))
                    Parent.DNASDiscIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.DNASDiscID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.ISBN))
                    Parent.ISBNTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.ISBN];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.ISSN))
                    Parent.ISSNTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.ISSN];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.PPN))
                    Parent.PPNTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.PPN];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.VFCCode))
                    Parent.VFCCodeTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.VFCCode];

                // Publisher Identifiers
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.AcclaimID))
                    Parent.AcclaimIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.AcclaimID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.ActivisionID))
                    Parent.ActivisionIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.ActivisionID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.BandaiID))
                    Parent.BandaiIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.BandaiID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.ElectronicArtsID))
                    Parent.ElectronicArtsIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.ElectronicArtsID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.FoxInteractiveID))
                    Parent.FoxInteractiveIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.FoxInteractiveID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.GTInteractiveID))
                    Parent.GTInteractiveIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.GTInteractiveID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.JASRACID))
                    Parent.JASRACIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.JASRACID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.KingRecordsID))
                    Parent.KingRecordsIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.KingRecordsID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.KoeiID))
                    Parent.KoeiIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.KoeiID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.KonamiID))
                    Parent.KonamiIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.KonamiID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.LucasArtsID))
                    Parent.LucasArtsIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.LucasArtsID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.MicrosoftID))
                    Parent.MicrosoftIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.MicrosoftID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.NaganoID))
                    Parent.NaganoIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.NaganoID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.NamcoID))
                    Parent.NamcoIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.NamcoID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.NipponIchiSoftwareID))
                    Parent.NipponIchiSoftwareIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.NipponIchiSoftwareID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.OriginID))
                    Parent.OriginIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.OriginID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.PonyCanyonID))
                    Parent.PonyCanyonIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.PonyCanyonID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.SegaID))
                    Parent.SegaIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.SegaID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.SelenID))
                    Parent.SelenIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.SelenID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.SierraID))
                    Parent.SierraIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.SierraID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.TaitoID))
                    Parent.TaitoIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.TaitoID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.UbisoftID))
                    Parent.UbisoftIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.UbisoftID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.ValveID))
                    Parent.ValveIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.ValveID];

                // Read-Only Information
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.DMIHash))
                    Parent.DMIHash.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.DMIHash];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.Filename))
                    Parent.Filename.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.Filename];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.InternalName))
                    Parent.InternalName.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.InternalName];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.InternalSerialName))
                    Parent.InternalSerialName.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.InternalSerialName];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.Multisession))
                    Parent.Multisession.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.Multisession];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.PFIHash))
                    Parent.PFIHash.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.PFIHash];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.RingNonZeroDataStart))
                    Parent.RingNonZeroDataStart.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.RingNonZeroDataStart];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.SSHash))
                    Parent.SSHash.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.SSHash];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.SSVersion))
                    Parent.SSVersion.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.SSVersion];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.UniversalHash))
                    Parent.UniversalHash.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.UniversalHash];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.VolumeLabel))
                    Parent.VolumeLabel.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.VolumeLabel];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.XeMID))
                    Parent.XeMID.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.XeMID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.XMID))
                    Parent.XMID.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.XMID];
            }

            // Content Fields
            if (SubmissionInfo.CommonDiscInfo.ContentsSpecialFields != null)
            {
                // Games
                if (SubmissionInfo.CommonDiscInfo.ContentsSpecialFields.ContainsKey(SiteCode.Games))
                    Parent.GamesTextBox.Text = SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.Games];
                if (SubmissionInfo.CommonDiscInfo.ContentsSpecialFields.ContainsKey(SiteCode.NetYarozeGames))
                    Parent.NetYarozeGamesTextBox.Text = SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.NetYarozeGames];

                // Demos
                if (SubmissionInfo.CommonDiscInfo.ContentsSpecialFields.ContainsKey(SiteCode.PlayableDemos))
                    Parent.PlayableDemosTextBox.Text = SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.PlayableDemos];
                if (SubmissionInfo.CommonDiscInfo.ContentsSpecialFields.ContainsKey(SiteCode.RollingDemos))
                    Parent.RollingDemosTextBox.Text = SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.RollingDemos];
                if (SubmissionInfo.CommonDiscInfo.ContentsSpecialFields.ContainsKey(SiteCode.TechDemos))
                    Parent.TechDemosTextBox.Text = SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.TechDemos];

                // Video
                if (SubmissionInfo.CommonDiscInfo.ContentsSpecialFields.ContainsKey(SiteCode.GameFootage))
                    Parent.GameFootageTextBox.Text = SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.GameFootage];
                if (SubmissionInfo.CommonDiscInfo.ContentsSpecialFields.ContainsKey(SiteCode.Videos))
                    Parent.VideosTextBox.Text = SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.Videos];

                // Miscellaneous
                if (SubmissionInfo.CommonDiscInfo.ContentsSpecialFields.ContainsKey(SiteCode.Patches))
                    Parent.PatchesTextBox.Text = SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.Patches];
                if (SubmissionInfo.CommonDiscInfo.ContentsSpecialFields.ContainsKey(SiteCode.Savegames))
                    Parent.SavegamesTextBox.Text = SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.Savegames];
                if (SubmissionInfo.CommonDiscInfo.ContentsSpecialFields.ContainsKey(SiteCode.Extras))
                    Parent.ExtrasTextBox.Text = SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.Extras];
            }
        }

        /// <summary>
        /// Save the current contents of the UI to the base SubmissionInfo
        /// </summary>
        public void Save()
        {
            SubmissionInfo.CommonDiscInfo.Category = (Parent.CategoryComboBox.SelectedItem as Element<DiscCategory>)?.Value ?? DiscCategory.Games;
            SubmissionInfo.CommonDiscInfo.Region = (Parent.RegionComboBox.SelectedItem as Element<Region>)?.Value ?? Region.World;
            SubmissionInfo.CommonDiscInfo.Languages = Languages.Where(l => l.IsChecked).Select(l => l?.Value).ToArray();
            if (!SubmissionInfo.CommonDiscInfo.Languages.Any())
                SubmissionInfo.CommonDiscInfo.Languages = new Language?[] { null };
            SubmissionInfo.CommonDiscInfo.LanguageSelection = LanguageSelections.Where(ls => ls.IsChecked).Select(ls => ls?.Value).ToArray();

            // TODO: Figure out if this can be automatically mapped instead

            // Initialize the dictionaries, if needed
            if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields == null)
                SubmissionInfo.CommonDiscInfo.CommentsSpecialFields = new Dictionary<SiteCode?, string>();
            if (SubmissionInfo.CommonDiscInfo.ContentsSpecialFields == null)
                SubmissionInfo.CommonDiscInfo.ContentsSpecialFields = new Dictionary<SiteCode?, string>();

            #region Comment Fields

            // Additional Information
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.AlternativeTitle] = Parent.AlternativeTitleTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.AlternativeForeignTitle] = Parent.AlternativeForeignTitleTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.Genre] = Parent.GenreTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.PostgapType] = Parent.PostgapTypeCheckBox.IsChecked?.ToString();
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.Series] = Parent.SeriesTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.VCD] = Parent.VCDCheckBox.IsChecked?.ToString();

            // Physical Identifiers
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.BBFCRegistrationNumber] = Parent.BBFCRegistrationNumberTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.CDProjektID] = Parent.CDProjektIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.DiscHologramID] = Parent.DiscHologramIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.DNASDiscID] = Parent.DNASDiscIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.ISBN] = Parent.ISBNTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.ISSN] = Parent.ISSNTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.PPN] = Parent.PPNTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.VFCCode] = Parent.VFCCodeTextBox.Text;

            // Publisher Identifiers
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.AcclaimID] = Parent.AcclaimIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.ActivisionID] = Parent.ActivisionIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.BandaiID] = Parent.BandaiIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.ElectronicArtsID] = Parent.ElectronicArtsIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.FoxInteractiveID] = Parent.FoxInteractiveIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.GTInteractiveID] = Parent.GTInteractiveIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.JASRACID] = Parent.JASRACIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.KingRecordsID] = Parent.KingRecordsIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.KoeiID] = Parent.KoeiIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.KonamiID] = Parent.KonamiIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.LucasArtsID] = Parent.LucasArtsIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.MicrosoftID] = Parent.MicrosoftIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.NaganoID] = Parent.NaganoIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.NamcoID] = Parent.NamcoIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.NipponIchiSoftwareID] = Parent.NipponIchiSoftwareIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.OriginID] = Parent.OriginIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.PonyCanyonID] = Parent.PonyCanyonIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.SegaID] = Parent.SegaIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.SelenID] = Parent.SelenIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.SierraID] = Parent.SierraIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.TaitoID] = Parent.TaitoIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.UbisoftID] = Parent.UbisoftIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.ValveID] = Parent.ValveIDTextBox.Text;

            // Read-Only Information
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.DMIHash] = Parent.DMIHash.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.Filename] = Parent.Filename.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.InternalName] = Parent.InternalName.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.InternalSerialName] = Parent.InternalSerialName.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.Multisession] = Parent.Multisession.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.PFIHash] = Parent.PFIHash.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.RingNonZeroDataStart] = Parent.RingNonZeroDataStart.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.SSHash] = Parent.SSHash.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.SSVersion] = Parent.SSVersion.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.UniversalHash] = Parent.UniversalHash.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.VolumeLabel] = Parent.VolumeLabel.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.XeMID] = Parent.XeMID.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.XMID] = Parent.XMID.Text;

            #endregion

            #region Content Fields

            // Games
            SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.Games] = Parent.GamesTextBox.Text;
            SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.NetYarozeGames] = Parent.NetYarozeGamesTextBox.Text;

            // Demos
            SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.PlayableDemos] = Parent.PlayableDemosTextBox.Text;
            SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.RollingDemos] = Parent.RollingDemosTextBox.Text;
            SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.TechDemos] = Parent.TechDemosTextBox.Text;

            // Video
            SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.GameFootage] = Parent.GameFootageTextBox.Text;
            SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.Videos] = Parent.VideosTextBox.Text;

            // Miscellaneous
            SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.Patches] = Parent.PatchesTextBox.Text;
            SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.Savegames] = Parent.SavegamesTextBox.Text;
            SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.Extras] = Parent.ExtrasTextBox.Text;

            #endregion
        }

        /// <summary>
        /// Repopulate the list of Languages based on Redump support
        /// </summary>
        private void SetRedumpLanguages()
        {
            this.Languages = RedumpLanguages.Select(l => new Element<Language>(l)).ToList();
        }

        /// <summary>
        /// Repopulate the list of Regions based on Redump support
        /// </summary>
        private void SetRedumpRegions()
        {
            this.Regions = RedumpRegions.Select(r => new Element<Region>(r)).ToList();
        }

        /// <summary>
        /// Update visible fields and sections based on the media type
        /// </summary>
        private void UpdateFromDiscType()
        {
            // Sony-printed discs have layers in the opposite order
            var system = SubmissionInfo?.CommonDiscInfo?.System;
            bool reverseOrder = system.HasReversedRingcodes();

            switch (SubmissionInfo?.CommonDiscInfo?.Media)
            {
                case DiscType.CD:
                case DiscType.GDROM:
                    Parent.L0Info.Header = "Data Side";
                    Parent.L0MasteringRing.Label = "Mastering Ring";
                    Parent.L0MasteringSID.Label = "Mastering SID";
                    Parent.L0Toolstamp.Label = "Toolstamp/Mastering Code";
                    Parent.L0MouldSID.Label = "Mould SID";
                    Parent.L0AdditionalMould.Label = "Additional Mould";

                    Parent.L1Info.Header = "Label Side";
                    Parent.L1MasteringRing.Visibility = Visibility.Collapsed;
                    Parent.L1MasteringSID.Visibility = Visibility.Collapsed;
                    Parent.L1Toolstamp.Visibility = Visibility.Collapsed;
                    Parent.L1MouldSID.Label = "Mould SID";
                    Parent.L1AdditionalMould.Label = "Additional Mould";
                    break;

                case DiscType.DVD5:
                case DiscType.DVD9:
                case DiscType.HDDVDSL:
                case DiscType.HDDVDDL:
                case DiscType.BD25:
                case DiscType.BD33:
                case DiscType.BD50:
                case DiscType.BD66:
                case DiscType.BD100:
                case DiscType.BD128:
                case DiscType.NintendoGameCubeGameDisc:
                case DiscType.NintendoWiiOpticalDiscSL:
                case DiscType.NintendoWiiOpticalDiscDL:
                case DiscType.NintendoWiiUOpticalDiscSL:
                    // Quad-layer discs
                    if (SubmissionInfo?.SizeAndChecksums?.Layerbreak3 != default(long))
                    {
                        Parent.L2Info.Visibility = Visibility.Visible;
                        Parent.L3Info.Visibility = Visibility.Visible;

                        Parent.L0Info.Header = reverseOrder ? "Layer 0 (Outer)" : "Layer 0 (Inner)";
                        Parent.L0MasteringRing.Label = "Mastering Ring";
                        Parent.L0MasteringSID.Label = "Mastering SID";
                        Parent.L0Toolstamp.Label = "Toolstamp/Mastering Code";
                        Parent.L0MouldSID.Label = "Data Side Mould SID";
                        Parent.L0AdditionalMould.Label = "Data Side Additional Mould";

                        Parent.L1Info.Header = "Layer 1";
                        Parent.L1MasteringRing.Label = "Mastering Ring";
                        Parent.L1MasteringSID.Label = "Mastering SID";
                        Parent.L1Toolstamp.Label = "Toolstamp/Mastering Code";
                        Parent.L1MouldSID.Label = "Label Side Mould SID";
                        Parent.L1AdditionalMould.Label = "Label Side Additional Mould";

                        Parent.L2Info.Header = "Layer 2";
                        Parent.L2MasteringRing.Label = "Mastering Ring";
                        Parent.L2MasteringSID.Label = "Mastering SID";
                        Parent.L2Toolstamp.Label = "Toolstamp/Mastering Code";

                        Parent.L3Info.Header = reverseOrder ? "Layer 3 (Inner)" : "Layer 3 (Outer)";
                        Parent.L3MasteringRing.Label = "Mastering Ring";
                        Parent.L3MasteringSID.Label = "Mastering SID";
                        Parent.L3Toolstamp.Label = "Toolstamp/Mastering Code";
                    }

                    // Triple-layer discs
                    else if (SubmissionInfo?.SizeAndChecksums?.Layerbreak2 != default(long))
                    {
                        Parent.L2Info.Visibility = Visibility.Visible;

                        Parent.L0Info.Header = reverseOrder ? "Layer 0 (Outer)" : "Layer 0 (Inner)";
                        Parent.L0MasteringRing.Label = "Mastering Ring";
                        Parent.L0MasteringSID.Label = "Mastering SID";
                        Parent.L0Toolstamp.Label = "Toolstamp/Mastering Code";
                        Parent.L0MouldSID.Label = "Data Side Mould SID";
                        Parent.L0AdditionalMould.Label = "Data Side Additional Mould";

                        Parent.L1Info.Header = "Layer 1";
                        Parent.L1MasteringRing.Label = "Mastering Ring";
                        Parent.L1MasteringSID.Label = "Mastering SID";
                        Parent.L1Toolstamp.Label = "Toolstamp/Mastering Code";
                        Parent.L1MouldSID.Label = "Label Side Mould SID";
                        Parent.L1AdditionalMould.Label = "Label Side Additional Mould";

                        Parent.L2Info.Header = reverseOrder ? "Layer 2 (Inner)" : "Layer 2 (Outer)";
                        Parent.L2MasteringRing.Label = "Mastering Ring";
                        Parent.L2MasteringSID.Label = "Mastering SID";
                        Parent.L2Toolstamp.Label = "Toolstamp/Mastering Code";
                    }

                    // Double-layer discs
                    else if (SubmissionInfo?.SizeAndChecksums?.Layerbreak != default(long))
                    {
                        Parent.L0Info.Header = reverseOrder ? "Layer 0 (Outer)" : "Layer 0 (Inner)";
                        Parent.L0MasteringRing.Label = "Mastering Ring";
                        Parent.L0MasteringSID.Label = "Mastering SID";
                        Parent.L0Toolstamp.Label = "Toolstamp/Mastering Code";
                        Parent.L0MouldSID.Label = "Data Side Mould SID";
                        Parent.L0AdditionalMould.Label = "Data Side Additional Mould";

                        Parent.L1Info.Header = reverseOrder ? "Layer 1 (Inner)" : "Layer 1 (Outer)";
                        Parent.L1MasteringRing.Label = "Mastering Ring";
                        Parent.L1MasteringSID.Label = "Mastering SID";
                        Parent.L1Toolstamp.Label = "Toolstamp/Mastering Code";
                        Parent.L1MouldSID.Label = "Label Side Mould SID";
                        Parent.L1AdditionalMould.Label = "Label Side Additional Mould";
                    }

                    // Single-layer discs
                    else
                    {
                        Parent.L0Info.Header = "Data Side";
                        Parent.L0MasteringRing.Label = "Mastering Ring";
                        Parent.L0MasteringSID.Label = "Mastering SID";
                        Parent.L0Toolstamp.Label = "Toolstamp/Mastering Code";
                        Parent.L0MouldSID.Label = "Mould SID";
                        Parent.L0AdditionalMould.Label = "Additional Mould";

                        Parent.L1Info.Header = "Label Side";
                        Parent.L1MasteringRing.Label = "Mastering Ring";
                        Parent.L1MasteringSID.Label = "Mastering SID";
                        Parent.L1Toolstamp.Label = "Toolstamp/Mastering Code";
                        Parent.L1MouldSID.Label = "Mould SID";
                        Parent.L1AdditionalMould.Label = "Additional Mould";
                    }

                    break;

                // All other media we assume to have no rings
                default:
                    Parent.L0Info.Visibility = Visibility.Collapsed;
                    Parent.L1Info.Visibility = Visibility.Collapsed;
                    Parent.L2Info.Visibility = Visibility.Collapsed;
                    Parent.L3Info.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        /// <summary>
        /// Update visible fields and sections based on the system type
        /// </summary>
        private void UpdateFromSystemType()
        {
            var system = SubmissionInfo?.CommonDiscInfo?.System;
            switch (system)
            {
                case RedumpSystem.SonyPlayStation2:
                    Parent.LanguageSelectionGrid.Visibility = Visibility.Visible;
                    break;
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handler for AcceptButton Click event
        /// </summary>
        private void OnAcceptClick(object sender, RoutedEventArgs e)
        {
            Save();
            Parent.DialogResult = true;
            Parent.Close();
        }

        /// <summary>
        /// Handler for CancelButton Click event
        /// </summary>
        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Parent.DialogResult = false;
            Parent.Close();
        }

        /// <summary>
        /// Handler for RingCodeGuideButton Click event
        /// </summary>
        private void OnRingCodeGuideClick(object sender, RoutedEventArgs e)
        {
            var ringCodeGuideWindow = new RingCodeGuideWindow()
            {
                Owner = Parent,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };
            ringCodeGuideWindow.Show();
        }

        #endregion
    }
}
