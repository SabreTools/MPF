using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MPF.Avalonia.UserControls;
using MPF.Frontend;
using MPF.Frontend.ComboBoxItems;
using MPF.Frontend.ViewModels;
using SabreTools.RedumpLib.Data;

namespace MPF.Avalonia.Windows
{
    /// <summary>
    /// Interaction logic for MediaInformationWindow.axaml
    /// </summary>
    public partial class MediaInformationWindow : WindowBase
    {
        /// <summary>
        /// Whether the PC/Mac hybrid grid should always be shown regardless of media type
        /// </summary>
        private readonly bool _showPcMacHybridAlways;

        #region Field Mappings

        /// <summary>
        /// Mapping of comment input control names to their associated site codes
        /// </summary>
        private static readonly (string Name, SiteCode Code)[] CommentFields =
        [
            ("AlternativeTitleTextBox", SiteCode.AlternativeTitle),
            ("AlternativeForeignTitleTextBox", SiteCode.AlternativeForeignTitle),
            ("DiscTitleNonLatinTextBox", SiteCode.DiscTitleNonLatin),
            ("EditionNonLatinTextBox", SiteCode.EditionNonLatin),
            ("CompatibleOSTextBox", SiteCode.CompatibleOS),
            ("BBFCRegistrationNumberTextBox", SiteCode.BBFCRegistrationNumber),
            ("DiscHologramIDTextBox", SiteCode.DiscHologramID),
            ("DNASDiscIDTextBox", SiteCode.DNASDiscID),
            ("CommentDiscIDTextBox", SiteCode.DiscID),
            ("CoverIDTextBox", SiteCode.CoverID),
            ("ISBNTextBox", SiteCode.ISBN),
            ("ISSNTextBox", SiteCode.ISSN),
            ("PPNTextBox", SiteCode.PPN),
            ("VFCCodeTextBox", SiteCode.VFCCode),
            ("TwoKGamesIDTextBox", SiteCode.TwoKGamesID),
            ("AcclaimIDTextBox", SiteCode.AcclaimID),
            ("AccoladeIDTextBox", SiteCode.AccoladeID),
            ("ActivisionIDTextBox", SiteCode.ActivisionID),
            ("BandaiIDTextBox", SiteCode.BandaiID),
            ("BethesdaIDTextBox", SiteCode.BethesdaID),
            ("CDProjektIDTextBox", SiteCode.CDProjektID),
            ("DisneyInteractiveIDTextBox", SiteCode.DisneyInteractiveID),
            ("EidosIDTextBox", SiteCode.EidosID),
            ("ElectronicArtsIDTextBox", SiteCode.ElectronicArtsID),
            ("FoxInteractiveIDTextBox", SiteCode.FoxInteractiveID),
            ("GTInteractiveIDTextBox", SiteCode.GTInteractiveID),
            ("InterplayIDTextBox", SiteCode.InterplayID),
            ("JASRACIDTextBox", SiteCode.JASRACID),
            ("KingRecordsIDTextBox", SiteCode.KingRecordsID),
            ("KoeiIDTextBox", SiteCode.KoeiID),
            ("KonamiIDTextBox", SiteCode.KonamiID),
            ("LucasArtsIDTextBox", SiteCode.LucasArtsID),
            ("MicrosoftIDTextBox", SiteCode.MicrosoftID),
            ("NaganoIDTextBox", SiteCode.NaganoID),
            ("NamcoIDTextBox", SiteCode.NamcoID),
            ("NipponIchiSoftwareIDTextBox", SiteCode.NipponIchiSoftwareID),
            ("OriginIDTextBox", SiteCode.OriginID),
            ("PonyCanyonIDTextBox", SiteCode.PonyCanyonID),
            ("SegaIDTextBox", SiteCode.SegaID),
            ("SelenIDTextBox", SiteCode.SelenID),
            ("SierraIDTextBox", SiteCode.SierraID),
            ("TaitoIDTextBox", SiteCode.TaitoID),
            ("UbisoftIDTextBox", SiteCode.UbisoftID),
            ("ValveIDTextBox", SiteCode.ValveID),
            ("DMIHash", SiteCode.DMIHash),
            ("Filename", SiteCode.Filename),
            ("InternalName", SiteCode.InternalName),
            ("InternalSerialName", SiteCode.InternalSerialName),
            ("Multisession", SiteCode.Multisession),
            ("PFIHash", SiteCode.PFIHash),
            ("RingNonZeroDataStart", SiteCode.RingNonZeroDataStart),
            ("RingPerfectAudioOffset", SiteCode.RingPerfectAudioOffset),
            ("SSHash", SiteCode.SSHash),
            ("SSVersion", SiteCode.SSVersion),
            ("UniversalHash", SiteCode.UniversalHash),
            ("VolumeLabel", SiteCode.VolumeLabel),
            ("XeMID", SiteCode.XeMID),
            ("XMID", SiteCode.XMID),
        ];

        /// <summary>
        /// Mapping of content input control names to their associated site codes
        /// </summary>
        private static readonly (string Name, SiteCode Code)[] ContentFields =
        [
            ("ApplicationsTextBox", SiteCode.Applications),
            ("GamesTextBox", SiteCode.Games),
            ("NetYarozeGamesTextBox", SiteCode.NetYarozeGames),
            ("PlayableDemosTextBox", SiteCode.PlayableDemos),
            ("RollingDemosTextBox", SiteCode.RollingDemos),
            ("TechDemosTextBox", SiteCode.TechDemos),
            ("GameFootageTextBox", SiteCode.GameFootage),
            ("VideosTextBox", SiteCode.Videos),
            ("PatchesTextBox", SiteCode.Patches),
            ("SavegamesTextBox", SiteCode.Savegames),
            ("ExtrasTextBox", SiteCode.Extras),
        ];

        #endregion

        /// <summary>
        /// Read-only access to the current media information view model
        /// </summary>
        public MediaInformationViewModel MediaInformationViewModel => DataContext as MediaInformationViewModel ?? new MediaInformationViewModel(new Options(), new SubmissionInfo());

        public MediaInformationWindow()
            : this(new Options(), new SubmissionInfo(), showPcMacHybridAlways: true)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public MediaInformationWindow(Options options, SubmissionInfo? submissionInfo, bool showPcMacHybridAlways = false)
        {
            _showPcMacHybridAlways = showPcMacHybridAlways;
            InitializeComponent();
            DataContext = new MediaInformationViewModel(options, submissionInfo);
            if (options.Processing.MediaInformation.EnableRedumpCompatibility)
            {
                MediaInformationViewModel.SetRedumpRegions();
                MediaInformationViewModel.SetRedumpLanguages();
            }

            MediaInformationViewModel.Load();
            PopulateCollections();
            LoadMappedFields();
            ManipulateFields(options, MediaInformationViewModel.SubmissionInfo);

            this.FindControl<Button>("AcceptButton")!.Click += OnAcceptClick;
            this.FindControl<Button>("CancelButton")!.Click += OnCancelClick;
            this.FindControl<Button>("RingCodeGuideButton")!.Click += OnRingCodeGuideClick;
        }

        #region Event Handlers

        /// <summary>
        /// Handler for AcceptButton Click event
        /// </summary>
        private void OnAcceptClick(object? sender, RoutedEventArgs e)
        {
            SaveMappedFields();
            MediaInformationViewModel.Save();
            Close(true);
        }

        /// <summary>
        /// Handler for CancelButton Click event
        /// </summary>
        private void OnCancelClick(object? sender, RoutedEventArgs e)
            => Close(false);

        /// <summary>
        /// Handler for RingCodeGuideButton Click event
        /// </summary>
        private void OnRingCodeGuideClick(object? sender, RoutedEventArgs e)
            => _ = new RingCodeGuideWindow().ShowDialog(this);

        #endregion

        #region Helpers

        /// <summary>
        /// Assign the view model collections as the data sources for the dropdown controls
        /// </summary>
        private void PopulateCollections()
        {
            this.FindControl<ComboBox>("CategoryComboBox")!.ItemsSource = MediaInformationViewModel.Categories;
            this.FindControl<ComboBox>("RegionComboBox")!.ItemsSource = MediaInformationViewModel.Regions;
            this.FindControl<MultiSelectDropDown>("LanguagesDropDown")!.ItemsSource = MediaInformationViewModel.Languages;
            this.FindControl<MultiSelectDropDown>("LanguageSelectionsDropDown")!.ItemsSource = MediaInformationViewModel.LanguageSelections;
        }

        /// <summary>
        /// Populate the input controls from the current submission info values
        /// </summary>
        private void LoadMappedFields()
        {
            foreach (var (name, code) in CommentFields)
            {
                SetControlText(name, GetComment(code));
            }

            foreach (var (name, code) in ContentFields)
            {
                SetControlText(name, GetContent(code));
            }

            SetControlText("DiscKeyTextBox", MediaInformationViewModel.SubmissionInfo.Extras?.DiscKey);
            SetControlText("DiscIDTextBox", MediaInformationViewModel.SubmissionInfo.Extras?.DiscID);

            if (MediaInformationViewModel.SubmissionInfo.PartiallyMatchedIDs?.Count > 0)
                SetControlText("PartiallyMatchedIDs", string.Join(", ", MediaInformationViewModel.SubmissionInfo.PartiallyMatchedIDs));

            this.FindControl<CheckBox>("PCMacHybridCheckBox")!.IsChecked = ParseBooleanComment(GetComment(SiteCode.PCMacHybrid));
        }

        /// <summary>
        /// Save the input control values back into the current submission info
        /// </summary>
        private void SaveMappedFields()
        {
            EnsureSpecialFields();

            foreach (var (name, code) in CommentFields)
            {
                SetComment(code, GetControlText(name));
            }

            foreach (var (name, code) in ContentFields)
            {
                SetContent(code, GetControlText(name));
            }

            if (MediaInformationViewModel.SubmissionInfo.Extras is not null)
            {
                MediaInformationViewModel.SubmissionInfo.Extras.DiscKey = GetControlText("DiscKeyTextBox");
                MediaInformationViewModel.SubmissionInfo.Extras.DiscID = GetControlText("DiscIDTextBox");
            }

            SetComment(SiteCode.PCMacHybrid, this.FindControl<CheckBox>("PCMacHybridCheckBox")!.IsChecked == true ? "true" : string.Empty);
        }

        /// <summary>
        /// Manipulate fields based on the current disc
        /// </summary>
        private void ManipulateFields(Options options, SubmissionInfo? submissionInfo)
        {
            // Enable tabs in all fields, if required
            if (options.Processing.MediaInformation.EnableTabsInInputFields)
                EnableTabsInInputFields();

            // Hide read-only fields that don't have values set
            HideReadOnlyFields(submissionInfo);

            // Different media types mean different fields available
            UpdateFromDiscType(submissionInfo);

            // Different systems mean different fields available
            UpdateFromSystemType(submissionInfo);
        }

        /// <summary>
        /// Ringcode input control names that should allow tab entry
        /// </summary>
        private static readonly string[] TabEnabledFieldNames =
        [
            "L0MasteringRing", "L0MasteringSID", "L0Toolstamp", "L0MouldSID", "L0AdditionalMould",
            "L1MasteringRing", "L1MasteringSID", "L1Toolstamp", "L1MouldSID", "L1AdditionalMould",
            "L2MasteringRing", "L2MasteringSID", "L2Toolstamp",
            "L3MasteringRing", "L3MasteringSID", "L3Toolstamp",
        ];

        /// <summary>
        /// Enable tab entry on ringcode fields
        /// </summary>
        private void EnableTabsInInputFields()
        {
            foreach (string name in TabEnabledFieldNames)
            {
                this.FindControl<UserInput>(name)!.Tab = true;
            }
        }

        /// <summary>
        /// Hide any optional, read-only fields if they don't have a value
        /// </summary>
        private void HideReadOnlyFields(SubmissionInfo? submissionInfo)
        {
            // If there's no submission information
            if (submissionInfo is null)
                return;

            CollapseWhen("FullyMatchedID", submissionInfo.FullyMatchedID is null);
            CollapseWhen("PartiallyMatchedIDs", submissionInfo.PartiallyMatchedIDs is null || submissionInfo.PartiallyMatchedIDs.Count == 0);
            CollapseWhen("HashData", string.IsNullOrEmpty(submissionInfo.TracksAndWriteOffsets.ClrMameProData));
            CollapseWhen("HashDataSize", submissionInfo.SizeAndChecksums.Size == 0);
            CollapseWhen("HashDataCRC", string.IsNullOrEmpty(submissionInfo.SizeAndChecksums.CRC32));
            CollapseWhen("HashDataMD5", string.IsNullOrEmpty(submissionInfo.SizeAndChecksums.MD5));
            CollapseWhen("HashDataSHA1", string.IsNullOrEmpty(submissionInfo.SizeAndChecksums.SHA1));
            CollapseWhen("HashDataLayerbreak1", submissionInfo.SizeAndChecksums.Layerbreak == 0);
            CollapseWhen("HashDataLayerbreak2", submissionInfo.SizeAndChecksums.Layerbreak2 == 0);
            CollapseWhen("HashDataLayerbreak3", submissionInfo.SizeAndChecksums.Layerbreak3 == 0);
            CollapseWhen("AntiModchip", submissionInfo.CopyProtection?.AntiModchip is null);
            CollapseWhen("DiscOffset", submissionInfo.TracksAndWriteOffsets.OtherWriteOffsets is null);
            CollapseWhen("DMIHash", ShouldCollapseComment(submissionInfo, SiteCode.DMIHash));
            CollapseWhen("EDC", submissionInfo.EDC?.EDC is null);
            CollapseWhen("ErrorsCount", string.IsNullOrEmpty(submissionInfo.CommonDiscInfo?.ErrorsCount));
            CollapseWhen("EXEDateBuildDate", string.IsNullOrEmpty(submissionInfo.CommonDiscInfo?.EXEDateBuildDate));
            CollapseWhen("Filename", ShouldCollapseComment(submissionInfo, SiteCode.Filename));
            CollapseWhen("Header", string.IsNullOrEmpty(submissionInfo.Extras?.Header));
            CollapseWhen("InternalName", ShouldCollapseComment(submissionInfo, SiteCode.InternalName));
            CollapseWhen("InternalSerialName", ShouldCollapseComment(submissionInfo, SiteCode.InternalSerialName));
            CollapseWhen("Multisession", ShouldCollapseComment(submissionInfo, SiteCode.Multisession));
            CollapseWhen("LibCrypt", submissionInfo.CopyProtection?.LibCrypt is null);
            CollapseWhen("LibCryptData", string.IsNullOrEmpty(submissionInfo.CopyProtection?.LibCryptData));
            CollapseWhen("PFIHash", ShouldCollapseComment(submissionInfo, SiteCode.PFIHash));
            CollapseWhen("PIC", string.IsNullOrEmpty(submissionInfo.Extras?.PIC));
            CollapseWhen("PVD", string.IsNullOrEmpty(submissionInfo.Extras?.PVD));
            CollapseWhen("RingNonZeroDataStart", ShouldCollapseComment(submissionInfo, SiteCode.RingNonZeroDataStart));
            CollapseWhen("RingPerfectAudioOffset", ShouldCollapseComment(submissionInfo, SiteCode.RingPerfectAudioOffset));
            CollapseWhen("SecuROMData", string.IsNullOrEmpty(submissionInfo.CopyProtection?.SecuROMData));
            CollapseWhen("SSHash", ShouldCollapseComment(submissionInfo, SiteCode.SSHash));
            CollapseWhen("SecuritySectorRanges", string.IsNullOrEmpty(submissionInfo.Extras?.SecuritySectorRanges));
            CollapseWhen("SSVersion", ShouldCollapseComment(submissionInfo, SiteCode.SSVersion));
            CollapseWhen("UniversalHash", ShouldCollapseComment(submissionInfo, SiteCode.UniversalHash));
            CollapseWhen("VolumeLabel", ShouldCollapseComment(submissionInfo, SiteCode.VolumeLabel));
            CollapseWhen("XeMID", ShouldCollapseComment(submissionInfo, SiteCode.XeMID));
            CollapseWhen("XMID", ShouldCollapseComment(submissionInfo, SiteCode.XMID));
        }

        /// <summary>
        /// Update visible fields and sections based on the media type
        /// </summary>
        private void UpdateFromDiscType(SubmissionInfo? submissionInfo)
        {
            // Sony-printed discs have layers in the opposite order
            var system = submissionInfo?.CommonDiscInfo?.System;
            bool reverseOrder = system.HasReversedRingcodes();

            this.FindControl<Grid>("PCMacHybridGrid")!.IsVisible = _showPcMacHybridAlways
                || submissionInfo?.CommonDiscInfo?.Media == DiscType.CD;
            this.FindControl<Border>("L0InfoPanel")!.IsVisible = true;
            this.FindControl<Border>("L1InfoPanel")!.IsVisible = true;
            this.FindControl<Border>("L2InfoPanel")!.IsVisible = false;
            this.FindControl<Border>("L3InfoPanel")!.IsVisible = false;

            switch (submissionInfo?.CommonDiscInfo?.Media)
            {
                case DiscType.CD:
                case DiscType.GDROM:
                    SetLayerLabels("Data Side", "Label Side", false, false, reverseOrder, submissionInfo);
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
                    SetLayerLabels(null, null, true, true, reverseOrder, submissionInfo);
                    break;

                case DiscType.UMDSL:
                case DiscType.UMDDL:
                    SetLayerLabels(null, null, true, false, reverseOrder, submissionInfo);
                    break;

                default:
                    this.FindControl<Border>("L0InfoPanel")!.IsVisible = false;
                    this.FindControl<Border>("L1InfoPanel")!.IsVisible = false;
                    this.FindControl<Border>("L2InfoPanel")!.IsVisible = false;
                    this.FindControl<Border>("L3InfoPanel")!.IsVisible = false;
                    break;
            }
        }

        /// <summary>
        /// Set the layer panel headers and ring field labels based on the disc's layer count
        /// </summary>
        private void SetLayerLabels(string? singleLayerDataHeader, string? singleLayerLabelHeader, bool supportExtraLayers, bool supportQuadLayers, bool reverseOrder, SubmissionInfo? submissionInfo)
        {
            if (!supportExtraLayers)
            {
                SetLayerHeader("L0HeaderText", singleLayerDataHeader ?? "Data Side");
                SetLayerHeader("L1HeaderText", singleLayerLabelHeader ?? "Label Side");
                SetRingFieldLabels(false);
                return;
            }

            long layerbreak = submissionInfo?.SizeAndChecksums.Layerbreak ?? 0;
            long layerbreak2 = submissionInfo?.SizeAndChecksums.Layerbreak2 ?? 0;
            long layerbreak3 = submissionInfo?.SizeAndChecksums.Layerbreak3 ?? 0;

            if (supportQuadLayers && layerbreak3 != 0)
            {
                this.FindControl<Border>("L2InfoPanel")!.IsVisible = true;
                this.FindControl<Border>("L3InfoPanel")!.IsVisible = true;

                SetLayerHeader("L0HeaderText", reverseOrder ? "Layer 0 (Outer)" : "Layer 0 (Inner)");
                SetLayerHeader("L1HeaderText", "Layer 1");
                SetLayerHeader("L2HeaderText", "Layer 2");
                SetLayerHeader("L3HeaderText", reverseOrder ? "Layer 3 (Inner)" : "Layer 3 (Outer)");
                SetRingFieldLabels(true);
                return;
            }

            if (layerbreak2 != 0)
            {
                this.FindControl<Border>("L2InfoPanel")!.IsVisible = true;

                SetLayerHeader("L0HeaderText", reverseOrder ? "Layer 0 (Outer)" : "Layer 0 (Inner)");
                SetLayerHeader("L1HeaderText", "Layer 1");
                SetLayerHeader("L2HeaderText", reverseOrder ? "Layer 2 (Inner)" : "Layer 2 (Outer)");
                SetRingFieldLabels(true);
                return;
            }

            if (layerbreak != 0)
            {
                SetLayerHeader("L0HeaderText", reverseOrder ? "Layer 0 (Outer)" : "Layer 0 (Inner)");
                SetLayerHeader("L1HeaderText", reverseOrder ? "Layer 1 (Inner)" : "Layer 1 (Outer)");
                SetRingFieldLabels(true);
                return;
            }

            SetLayerHeader("L0HeaderText", "Data Side");
            SetLayerHeader("L1HeaderText", "Label Side");
            SetRingFieldLabels(false);
        }

        /// <summary>
        /// Set the labels on all ring code input fields, varying mould labels by layer state
        /// </summary>
        private void SetRingFieldLabels(bool layered)
        {
            this.FindControl<UserInput>("L0MasteringRing")!.Label = "Mastering Ring";
            this.FindControl<UserInput>("L0MasteringSID")!.Label = "Mastering SID";
            this.FindControl<UserInput>("L0Toolstamp")!.Label = "Toolstamp/Mastering Code";
            this.FindControl<UserInput>("L0MouldSID")!.Label = layered ? "Data Side Mould SID" : "Mould SID";
            this.FindControl<UserInput>("L0AdditionalMould")!.Label = layered ? "Data Side Additional Mould" : "Additional Mould";

            this.FindControl<UserInput>("L1MasteringRing")!.Label = "Mastering Ring";
            this.FindControl<UserInput>("L1MasteringSID")!.Label = "Mastering SID";
            this.FindControl<UserInput>("L1Toolstamp")!.Label = "Toolstamp/Mastering Code";
            this.FindControl<UserInput>("L1MouldSID")!.Label = layered ? "Label Side Mould SID" : "Mould SID";
            this.FindControl<UserInput>("L1AdditionalMould")!.Label = layered ? "Label Side Additional Mould" : "Additional Mould";

            this.FindControl<UserInput>("L2MasteringRing")!.Label = "Mastering Ring";
            this.FindControl<UserInput>("L2MasteringSID")!.Label = "Mastering SID";
            this.FindControl<UserInput>("L2Toolstamp")!.Label = "Toolstamp/Mastering Code";

            this.FindControl<UserInput>("L3MasteringRing")!.Label = "Mastering Ring";
            this.FindControl<UserInput>("L3MasteringSID")!.Label = "Mastering SID";
            this.FindControl<UserInput>("L3Toolstamp")!.Label = "Toolstamp/Mastering Code";
        }

        /// <summary>
        /// Update visible fields and sections based on the system type
        /// </summary>
        private void UpdateFromSystemType(SubmissionInfo? submissionInfo)
        {
            var system = submissionInfo?.CommonDiscInfo?.System;

            this.FindControl<UserInput>("CompatibleOSTextBox")!.IsVisible = false;
            this.FindControl<UserInput>("DiscKeyTextBox")!.IsVisible = false;
            this.FindControl<UserInput>("DiscIDTextBox")!.IsVisible = false;
            this.FindControl<UserInput>("NetYarozeGamesTextBox")!.IsVisible = false;
            this.FindControl<MultiSelectDropDown>("LanguageSelectionsDropDown")!.IsVisible = false;

            switch (system)
            {
                case RedumpSystem.AppleMacintosh:
                case RedumpSystem.IBMPCcompatible:
                    this.FindControl<UserInput>("CompatibleOSTextBox")!.IsVisible = true;
                    break;

                case RedumpSystem.NintendoWiiU:
                    this.FindControl<UserInput>("DiscKeyTextBox")!.IsVisible = true;
                    break;

                case RedumpSystem.SonyPlayStation:
                    this.FindControl<UserInput>("NetYarozeGamesTextBox")!.IsVisible = true;
                    break;

                case RedumpSystem.SonyPlayStation2:
                    this.FindControl<MultiSelectDropDown>("LanguageSelectionsDropDown")!.IsVisible = true;
                    break;

                case RedumpSystem.SonyPlayStation3:
                    this.FindControl<UserInput>("DiscKeyTextBox")!.IsVisible = true;
                    this.FindControl<UserInput>("DiscIDTextBox")!.IsVisible = true;
                    break;
            }
        }

        /// <summary>
        /// Set the text of a layer header text block
        /// </summary>
        private void SetLayerHeader(string name, string value)
            => this.FindControl<TextBlock>(name)!.Text = value;

        /// <summary>
        /// Show or collapse a named input field based on the given condition
        /// </summary>
        private void CollapseWhen(string name, bool collapse)
            => this.FindControl<UserInput>(name)!.IsVisible = !collapse;

        /// <summary>
        /// Get the current text of a named input control
        /// </summary>
        private string GetControlText(string name)
            => this.FindControl<UserInput>(name)?.Text ?? string.Empty;

        /// <summary>
        /// Set the text of a named input control, if it exists
        /// </summary>
        private void SetControlText(string name, string? value)
        {
            var control = this.FindControl<UserInput>(name);
            if (control is not null)
                control.Text = value ?? string.Empty;
        }

        /// <summary>
        /// Get the comment special field value for the given site code
        /// </summary>
        private string GetComment(SiteCode code)
        {
            if (MediaInformationViewModel.SubmissionInfo.CommonDiscInfo?.CommentsSpecialFields?.TryGetValue(code, out string? value) == true)
                return value ?? string.Empty;

            return string.Empty;
        }

        /// <summary>
        /// Set the comment special field value for the given site code
        /// </summary>
        private void SetComment(SiteCode code, string? value)
            => MediaInformationViewModel.SubmissionInfo.CommonDiscInfo!.CommentsSpecialFields![code] = value ?? string.Empty;

        /// <summary>
        /// Get the content special field value for the given site code
        /// </summary>
        private string GetContent(SiteCode code)
        {
            if (MediaInformationViewModel.SubmissionInfo.CommonDiscInfo?.ContentsSpecialFields?.TryGetValue(code, out string? value) == true)
                return value ?? string.Empty;

            return string.Empty;
        }

        /// <summary>
        /// Set the content special field value for the given site code
        /// </summary>
        private void SetContent(SiteCode code, string? value)
            => MediaInformationViewModel.SubmissionInfo.CommonDiscInfo!.ContentsSpecialFields![code] = value ?? string.Empty;

        /// <summary>
        /// Ensure the special fields dictionaries and extras section exist before use
        /// </summary>
        private void EnsureSpecialFields()
        {
            MediaInformationViewModel.SubmissionInfo.CommonDiscInfo ??= new SabreTools.RedumpLib.Data.Sections.CommonDiscInfoSection();
            MediaInformationViewModel.SubmissionInfo.CommonDiscInfo.CommentsSpecialFields ??= [];
            MediaInformationViewModel.SubmissionInfo.CommonDiscInfo.ContentsSpecialFields ??= [];
            MediaInformationViewModel.SubmissionInfo.Extras ??= new SabreTools.RedumpLib.Data.Sections.ExtrasSection();
        }

        /// <summary>
        /// Parse a comment string into a boolean, treating "true", "yes", and "1" as true
        /// </summary>
        private static bool ParseBooleanComment(string? value)
            => value?.Equals("true", StringComparison.OrdinalIgnoreCase) == true
            || value?.Equals("yes", StringComparison.OrdinalIgnoreCase) == true
            || value == "1";

        /// <summary>
        /// Determine if a comment field should be collapsed in read-only view
        /// </summary>
        private static bool ShouldCollapseComment(SubmissionInfo? submissionInfo, SiteCode siteCode)
        {
            // If the special fields don't exist
            if (submissionInfo?.CommonDiscInfo?.CommentsSpecialFields is null)
                return true;

            // If the key doesn't exist
            if (!submissionInfo.CommonDiscInfo.CommentsSpecialFields.TryGetValue(siteCode, out string? value))
                return true;

            // Collapse if the value doesn't exist
            return string.IsNullOrEmpty(value);
        }

        #endregion
    }
}
