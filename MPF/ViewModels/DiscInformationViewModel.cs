using System.Collections.Generic;
using System.Linq;
using System.Windows;
using MPF.Core.Utilities;
using MPF.Windows;
using RedumpLib.Data;

namespace MPF.GUI.ViewModels
{
    public class DiscInformationViewModel
    {
        #region Fields

        /// <summary>
        /// Parent DiscInformationWindow object
        /// </summary>
        public DiscInformationWindow Parent { get; private set; }

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
        /// List of available languages
        /// </summary>
        public List<Element<Language>> Languages { get; private set; } = Element<Language>.GenerateElements().ToList();

        /// <summary>
        /// List of available languages
        /// </summary>
        public List<Element<LanguageSelection>> LanguageSelections { get; private set; } = Element<LanguageSelection>.GenerateElements().ToList();

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public DiscInformationViewModel(DiscInformationWindow parent, SubmissionInfo submissionInfo)
        {
            Parent = parent;
            SubmissionInfo = submissionInfo.Clone() as SubmissionInfo ?? new SubmissionInfo();

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
        /// Manipulate fields based on the current disc
        /// </summary>
        private void ManipulateFields()
        {
            // Sony-printed discs have layers in the opposite order
            var system = SubmissionInfo?.CommonDiscInfo?.System;
            bool reverseOrder = system.HasReversedRingcodes();

            // Different media types mean different fields available
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
                case DiscType.BD25:
                case DiscType.BD50:
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

            // Different systems mean different fields available
            switch (system)
            {
                case RedumpSystem.SonyPlayStation2:
                    Parent.LanguageSelectionGrid.Visibility = Visibility.Visible;
                    break;
            }
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
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.InternalSerialName))
                    Parent.InternalSerialName.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.InternalSerialName];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.PostgapType))
                    Parent.PostgapTypeTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.PostgapType];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.Series))
                    Parent.SeriesTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.Series];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.VolumeLabel))
                    Parent.VolumeLabelTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.VolumeLabel];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.VCD))
                    Parent.VCDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.VCD];

                // Media Identifiers
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.AcclaimID))
                    Parent.AcclaimIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.AcclaimID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.ActivisionID))
                    Parent.ActivisionIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.ActivisionID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.BandaiID))
                    Parent.BandaiIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.BandaiID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.BBFCRegistrationNumber))
                    Parent.BBFCRegistrationNumberTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.BBFCRegistrationNumber];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.DNASDiscID))
                    Parent.DNASDiscIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.DNASDiscID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.ElectronicArtsID))
                    Parent.ElectronicArtsIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.ElectronicArtsID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.FoxInteractiveID))
                    Parent.FoxInteractiveIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.FoxInteractiveID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.GTInteractiveID))
                    Parent.GTInteractiveIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.GTInteractiveID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.ISBN))
                    Parent.ISBNTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.ISBN];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.ISSN))
                    Parent.ISSNTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.ISSN];
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
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.PPN))
                    Parent.PPNTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.PPN];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.SegaID))
                    Parent.SegaIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.SegaID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.SelenID))
                    Parent.SelenIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.SelenID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.TaitoID))
                    Parent.TaitoIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.TaitoID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.UbisoftID))
                    Parent.UbisoftIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.UbisoftID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.ValveID))
                    Parent.ValveIDTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.ValveID];
                if (SubmissionInfo.CommonDiscInfo.CommentsSpecialFields.ContainsKey(SiteCode.VFCCode))
                    Parent.VFCCodeTextBox.Text = SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.VFCCode];
            }

            // Content Fields
            if (SubmissionInfo.CommonDiscInfo.ContentsSpecialFields != null)
            {
                if (SubmissionInfo.CommonDiscInfo.ContentsSpecialFields.ContainsKey(SiteCode.Extras))
                    Parent.ExtrasTextBox.Text = SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.Extras];
                if (SubmissionInfo.CommonDiscInfo.ContentsSpecialFields.ContainsKey(SiteCode.GameFootage))
                    Parent.GameFootageTextBox.Text = SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.GameFootage];
                if (SubmissionInfo.CommonDiscInfo.ContentsSpecialFields.ContainsKey(SiteCode.NetYarozeGames))
                    Parent.NetYarozeGamesTextBox.Text = SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.NetYarozeGames];
                if (SubmissionInfo.CommonDiscInfo.ContentsSpecialFields.ContainsKey(SiteCode.Patches))
                    Parent.PatchesTextBox.Text = SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.Patches];
                if (SubmissionInfo.CommonDiscInfo.ContentsSpecialFields.ContainsKey(SiteCode.PlayableDemos))
                    Parent.PlayableDemosTextBox.Text = SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.PlayableDemos];
                if (SubmissionInfo.CommonDiscInfo.ContentsSpecialFields.ContainsKey(SiteCode.RollingDemos))
                    Parent.RollingDemosTextBox.Text = SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.RollingDemos];
                if (SubmissionInfo.CommonDiscInfo.ContentsSpecialFields.ContainsKey(SiteCode.Savegames))
                    Parent.SavegamesTextBox.Text = SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.Savegames];
                if (SubmissionInfo.CommonDiscInfo.ContentsSpecialFields.ContainsKey(SiteCode.TechDemos))
                    Parent.TechDemosTextBox.Text = SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.TechDemos];
                if (SubmissionInfo.CommonDiscInfo.ContentsSpecialFields.ContainsKey(SiteCode.Videos))
                    Parent.VideosTextBox.Text = SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.Videos];
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

            // Additional Information
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.AlternativeTitle] = Parent.AlternativeTitleTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.AlternativeForeignTitle] = Parent.AlternativeForeignTitleTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.Genre] = Parent.GenreTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.InternalSerialName] = Parent.InternalSerialName.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.PostgapType] = Parent.PostgapTypeTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.Series] = Parent.SeriesTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.VolumeLabel] = Parent.VolumeLabelTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.VCD] = Parent.VCDTextBox.Text;

            // Media Identifiers
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.AcclaimID] = Parent.AcclaimIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.ActivisionID] = Parent.ActivisionIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.BandaiID] = Parent.BandaiIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.BBFCRegistrationNumber] = Parent.BBFCRegistrationNumberTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.DNASDiscID] = Parent.DNASDiscIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.ElectronicArtsID] = Parent.ElectronicArtsIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.FoxInteractiveID] = Parent.FoxInteractiveIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.GTInteractiveID] = Parent.GTInteractiveIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.ISBN] = Parent.ISBNTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.ISSN] = Parent.ISSNTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.JASRACID] = Parent.JASRACIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.KingRecordsID] = Parent.KingRecordsIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.KoeiID] = Parent.KoeiIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.KonamiID] = Parent.KonamiIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.LucasArtsID] = Parent.LucasArtsIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.NaganoID] = Parent.NaganoIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.NamcoID] = Parent.NamcoIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.NipponIchiSoftwareID] = Parent.NipponIchiSoftwareIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.OriginID] = Parent.OriginIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.PonyCanyonID] = Parent.PonyCanyonIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.PPN] = Parent.PPNTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.SegaID] = Parent.SegaIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.SelenID] = Parent.SelenIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.TaitoID] = Parent.TaitoIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.UbisoftID] = Parent.UbisoftIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.ValveID] = Parent.ValveIDTextBox.Text;
            SubmissionInfo.CommonDiscInfo.CommentsSpecialFields[SiteCode.VFCCode] = Parent.VFCCodeTextBox.Text;

            // Content Fields
            SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.Extras] = Parent.ExtrasTextBox.Text;
            SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.GameFootage] = Parent.GameFootageTextBox.Text;
            SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.NetYarozeGames] = Parent.NetYarozeGamesTextBox.Text;
            SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.Patches] = Parent.PatchesTextBox.Text;
            SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.PlayableDemos] = Parent.PlayableDemosTextBox.Text;
            SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.RollingDemos] = Parent.RollingDemosTextBox.Text;
            SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.Savegames] = Parent.SavegamesTextBox.Text;
            SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.TechDemos] = Parent.TechDemosTextBox.Text;
            SubmissionInfo.CommonDiscInfo.ContentsSpecialFields[SiteCode.Videos] = Parent.VideosTextBox.Text;
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
