using Avalonia.Interactivity;
using MPF.Frontend;
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

        /// <summary>
        /// Read-only access to the current media information view model
        /// </summary>
        public MediaInformationViewModel MediaInformationViewModel
            => DataContext as MediaInformationViewModel ?? new MediaInformationViewModel(new Options(), new SubmissionInfo());

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
            MediaInformationViewModel.Load();

            if (options.Processing.MediaInformation.EnableRedumpCompatibility)
            {
                MediaInformationViewModel.SetRedumpRegions();
                MediaInformationViewModel.SetRedumpLanguages();
            }

            // TODO: Determine why these need to be here
            PopulateCollections();

            // Add handlers
            AcceptButton!.Click += OnAcceptClick;
            CancelButton!.Click += OnCancelClick;
            RingCodeGuideButton!.Click += OnRingCodeGuideClick;

            // Update UI with new values
            ManipulateFields(options, MediaInformationViewModel.SubmissionInfo);
        }

        #region Helpers

        /// <summary>
        /// Assign the view model collections as the data sources for the dropdown controls
        /// </summary>
        /// TODO: Can this be avoided by binding?
        private void PopulateCollections()
        {
            CategoryComboBox!.ItemsSource = MediaInformationViewModel.Categories;
            RegionComboBox!.ItemsSource = MediaInformationViewModel.Regions;
            LanguagesDropDown!.ItemsSource = MediaInformationViewModel.Languages;
            LanguageSelectionsDropDown!.ItemsSource = MediaInformationViewModel.LanguageSelections;
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
        /// Enable tab entry on ringcode fields
        /// </summary>
        /// TODO: See if these can be done by binding
        private void EnableTabsInInputFields()
        {
            // L0
            L0MasteringRing!.Tab = true;
            L0MasteringSID!.Tab = true;
            L0Toolstamp!.Tab = true;
            L0MouldSID!.Tab = true;
            L0AdditionalMould!.Tab = true;

            // L1
            L1MasteringRing!.Tab = true;
            L1MasteringSID!.Tab = true;
            L1Toolstamp!.Tab = true;
            L1MouldSID!.Tab = true;
            L1AdditionalMould!.Tab = true;

            // L2
            L2MasteringRing!.Tab = true;
            L2MasteringSID!.Tab = true;
            L2Toolstamp!.Tab = true;

            // L3
            L3MasteringRing!.Tab = true;
            L3MasteringSID!.Tab = true;
            L3Toolstamp!.Tab = true;
        }

        /// <summary>
        /// Hide any optional, read-only fields if they don't have a value
        /// </summary>
        /// TODO: Figure out how to bind the PartiallyMatchedIDs array to a text box
        /// TODO: Convert visibility to a binding
        private void HideReadOnlyFields(SubmissionInfo? submissionInfo)
        {
            // If there's no submission information
            if (submissionInfo is null)
                return;

            if (submissionInfo.FullyMatchedID is null)
                FullyMatchedID!.IsVisible = false;
            if (submissionInfo.PartiallyMatchedIDs is null || submissionInfo.PartiallyMatchedIDs.Count == 0)
                PartiallyMatchedIDs!.IsVisible = false;
            else
                PartiallyMatchedIDs!.Text = string.Join(", ", [.. submissionInfo.PartiallyMatchedIDs.ConvertAll(i => i.ToString())]);
            if (string.IsNullOrEmpty(submissionInfo.TracksAndWriteOffsets.ClrMameProData))
                HashData!.IsVisible = false;
            if (submissionInfo.SizeAndChecksums.Layerbreak == 0)
                HashDataLayerbreak1!.IsVisible = false;
            if (submissionInfo.SizeAndChecksums.Layerbreak2 == 0)
                HashDataLayerbreak2!.IsVisible = false;
            if (submissionInfo.SizeAndChecksums.Layerbreak3 == 0)
                HashDataLayerbreak3!.IsVisible = false;
            if (submissionInfo.CopyProtection?.AntiModchip is null)
                AntiModchip!.IsVisible = false;
            if (submissionInfo.TracksAndWriteOffsets.OtherWriteOffsets is null)
                DiscOffset!.IsVisible = false;
            if (ShouldCollapseComment(submissionInfo, SiteCode.DMIHash))
                DMIHash!.IsVisible = false;
            if (submissionInfo.EDC?.EDC is null)
                EDC!.IsVisible = false;
            if (string.IsNullOrEmpty(submissionInfo.CommonDiscInfo?.ErrorsCount))
                ErrorsCount!.IsVisible = false;
            if (string.IsNullOrEmpty(submissionInfo.CommonDiscInfo?.EXEDateBuildDate))
                EXEDateBuildDate!.IsVisible = false;
            if (ShouldCollapseComment(submissionInfo, SiteCode.Filename))
                Filename!.IsVisible = false;
            if (string.IsNullOrEmpty(submissionInfo.Extras?.Header))
                Header!.IsVisible = false;
            if (ShouldCollapseComment(submissionInfo, SiteCode.InternalName))
                InternalName!.IsVisible = false;
            if (ShouldCollapseComment(submissionInfo, SiteCode.InternalSerialName))
                InternalSerialName!.IsVisible = false;
            if (ShouldCollapseComment(submissionInfo, SiteCode.Multisession))
                Multisession!.IsVisible = false;
            if (submissionInfo.CopyProtection?.LibCrypt is null)
                LibCrypt!.IsVisible = false;
            if (string.IsNullOrEmpty(submissionInfo.CopyProtection?.LibCryptData))
                LibCryptData!.IsVisible = false;
            if (ShouldCollapseComment(submissionInfo, SiteCode.PFIHash))
                PFIHash!.IsVisible = false;
            if (string.IsNullOrEmpty(submissionInfo.Extras?.PIC))
                PIC!.IsVisible = false;
            if (string.IsNullOrEmpty(submissionInfo.Extras?.PVD))
                PVD!.IsVisible = false;
            if (ShouldCollapseComment(submissionInfo, SiteCode.RingNonZeroDataStart))
                RingNonZeroDataStart!.IsVisible = false;
            if (ShouldCollapseComment(submissionInfo, SiteCode.RingPerfectAudioOffset))
                RingPerfectAudioOffset!.IsVisible = false;
            if (string.IsNullOrEmpty(submissionInfo.CopyProtection?.SecuROMData))
                SecuROMData!.IsVisible = false;
            if (ShouldCollapseComment(submissionInfo, SiteCode.SSHash))
                SSHash!.IsVisible = false;
            if (string.IsNullOrEmpty(submissionInfo.Extras?.SecuritySectorRanges))
                SecuritySectorRanges!.IsVisible = false;
            if (ShouldCollapseComment(submissionInfo, SiteCode.SSVersion))
                SSVersion!.IsVisible = false;
            if (ShouldCollapseComment(submissionInfo, SiteCode.UniversalHash))
                UniversalHash!.IsVisible = false;
            if (ShouldCollapseComment(submissionInfo, SiteCode.VolumeLabel))
                VolumeLabel!.IsVisible = false;
            if (ShouldCollapseComment(submissionInfo, SiteCode.XeMID))
                XeMID!.IsVisible = false;
            if (ShouldCollapseComment(submissionInfo, SiteCode.XMID))
                XMID!.IsVisible = false;
        }

        /// <summary>
        /// Update visible fields and sections based on the media type
        /// </summary>
        private void UpdateFromDiscType(SubmissionInfo? submissionInfo)
        {
            // Sony-printed discs have layers in the opposite order
            var system = submissionInfo?.CommonDiscInfo?.System;
            bool reverseOrder = system.HasReversedRingcodes();

            // TODO: Do these need to be explicitly set if they're in the AXAML?
            PCMacHybridGrid!.IsVisible = _showPcMacHybridAlways
                || submissionInfo?.CommonDiscInfo?.Media == DiscType.CD;
            L0InfoPanel!.IsVisible = true;
            L1InfoPanel!.IsVisible = true;
            L2InfoPanel!.IsVisible = false;
            L3InfoPanel!.IsVisible = false;

#pragma warning disable IDE0010
            switch (submissionInfo?.CommonDiscInfo?.Media)
            {
                case DiscType.CD:
                case DiscType.GDROM:
                    L0HeaderText!.Text = "Data Side";
                    L0MasteringRing!.Label = "Mastering Ring";
                    L0MasteringSID!.Label = "Mastering SID";
                    L0Toolstamp!.Label = "Toolstamp/Mastering Code";
                    L0MouldSID!.Label = "Mould SID";
                    L0AdditionalMould!.Label = "Additional Mould";

                    L1HeaderText!.Text = "Label Side";
                    L1MasteringRing!.Label = "Mastering Ring";
                    L1MasteringSID!.Label = "Mastering SID";
                    L1Toolstamp!.Label = "Toolstamp/Mastering Code";
                    L1MouldSID!.Label = "Mould SID";
                    L1AdditionalMould!.Label = "Additional Mould";
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
                    if (submissionInfo?.SizeAndChecksums.Layerbreak3 != default(long))
                    {
                        L2InfoPanel!.IsVisible = true;
                        L3InfoPanel!.IsVisible = true;

                        L0HeaderText!.Text = reverseOrder ? "Layer 0 (Outer)" : "Layer 0 (Inner)";
                        L0MasteringRing!.Label = "Mastering Ring";
                        L0MasteringSID!.Label = "Mastering SID";
                        L0Toolstamp!.Label = "Toolstamp/Mastering Code";
                        L0MouldSID!.Label = "Data Side Mould SID";
                        L0AdditionalMould!.Label = "Data Side Additional Mould";

                        L1HeaderText!.Text = "Layer 1";
                        L1MasteringRing!.Label = "Mastering Ring";
                        L1MasteringSID!.Label = "Mastering SID";
                        L1Toolstamp!.Label = "Toolstamp/Mastering Code";
                        L1MouldSID!.Label = "Label Side Mould SID";
                        L1AdditionalMould!.Label = "Label Side Additional Mould";

                        L2HeaderText!.Text = "Layer 2";
                        L2MasteringRing!.Label = "Mastering Ring";
                        L2MasteringSID!.Label = "Mastering SID";
                        L2Toolstamp!.Label = "Toolstamp/Mastering Code";

                        L3HeaderText!.Text = reverseOrder ? "Layer 3 (Inner)" : "Layer 3 (Outer)";
                        L3MasteringRing!.Label = "Mastering Ring";
                        L3MasteringSID!.Label = "Mastering SID";
                        L3Toolstamp!.Label = "Toolstamp/Mastering Code";
                    }

                    // Triple-layer discs
                    else if (submissionInfo?.SizeAndChecksums.Layerbreak2 != default(long))
                    {
                        L2InfoPanel!.IsVisible = true;

                        L0HeaderText!.Text = reverseOrder ? "Layer 0 (Outer)" : "Layer 0 (Inner)";
                        L0MasteringRing!.Label = "Mastering Ring";
                        L0MasteringSID!.Label = "Mastering SID";
                        L0Toolstamp!.Label = "Toolstamp/Mastering Code";
                        L0MouldSID!.Label = "Data Side Mould SID";
                        L0AdditionalMould!.Label = "Data Side Additional Mould";

                        L1HeaderText!.Text = "Layer 1";
                        L1MasteringRing!.Label = "Mastering Ring";
                        L1MasteringSID!.Label = "Mastering SID";
                        L1Toolstamp!.Label = "Toolstamp/Mastering Code";
                        L1MouldSID!.Label = "Label Side Mould SID";
                        L1AdditionalMould!.Label = "Label Side Additional Mould";

                        L2HeaderText!.Text = reverseOrder ? "Layer 2 (Inner)" : "Layer 2 (Outer)";
                        L2MasteringRing!.Label = "Mastering Ring";
                        L2MasteringSID!.Label = "Mastering SID";
                        L2Toolstamp!.Label = "Toolstamp/Mastering Code";
                    }

                    // Double-layer discs
                    else if (submissionInfo?.SizeAndChecksums.Layerbreak != default(long))
                    {
                        L0HeaderText!.Text = reverseOrder ? "Layer 0 (Outer)" : "Layer 0 (Inner)";
                        L0MasteringRing!.Label = "Mastering Ring";
                        L0MasteringSID!.Label = "Mastering SID";
                        L0Toolstamp!.Label = "Toolstamp/Mastering Code";
                        L0MouldSID!.Label = "Data Side Mould SID";
                        L0AdditionalMould!.Label = "Data Side Additional Mould";

                        L1HeaderText!.Text = reverseOrder ? "Layer 1 (Inner)" : "Layer 1 (Outer)";
                        L1MasteringRing!.Label = "Mastering Ring";
                        L1MasteringSID!.Label = "Mastering SID";
                        L1Toolstamp!.Label = "Toolstamp/Mastering Code";
                        L1MouldSID!.Label = "Label Side Mould SID";
                        L1AdditionalMould!.Label = "Label Side Additional Mould";
                    }

                    // Single-layer discs
                    else
                    {
                        L0HeaderText!.Text = "Data Side";
                        L0MasteringRing!.Label = "Mastering Ring";
                        L0MasteringSID!.Label = "Mastering SID";
                        L0Toolstamp!.Label = "Toolstamp/Mastering Code";
                        L0MouldSID!.Label = "Mould SID";
                        L0AdditionalMould!.Label = "Additional Mould";

                        L1HeaderText!.Text = "Label Side";
                        L1MasteringRing!.Label = "Mastering Ring";
                        L1MasteringSID!.Label = "Mastering SID";
                        L1Toolstamp!.Label = "Toolstamp/Mastering Code";
                        L1MouldSID!.Label = "Mould SID";
                        L1AdditionalMould!.Label = "Additional Mould";
                    }

                    break;

                case DiscType.UMDSL:
                case DiscType.UMDDL:
                    L0HeaderText!.Text = reverseOrder ? "Layer 0 (Outer)" : "Layer 0 (Inner)";
                    L0MasteringRing!.Label = "Mastering Ring";
                    L0MasteringSID!.Label = "Mastering SID";
                    L0Toolstamp!.Label = "Toolstamp/Mastering Code";
                    L0MouldSID!.Label = "Data Side Mould SID";
                    L0AdditionalMould!.Label = "Data Side Additional Mould";

                    L1HeaderText!.Text = reverseOrder ? "Layer 1 (Inner)" : "Layer 1 (Outer)";
                    L1MasteringRing!.Label = "Mastering Ring";
                    L1MasteringSID!.Label = "Mastering SID";
                    L1Toolstamp!.Label = "Toolstamp/Mastering Code";
                    L1MouldSID!.Label = "Label Side Mould SID";
                    L1AdditionalMould!.Label = "Label Side Additional Mould";
                    break;

                default:
                    L0InfoPanel!.IsVisible = false;
                    L1InfoPanel!.IsVisible = false;
                    L2InfoPanel!.IsVisible = false;
                    L3InfoPanel!.IsVisible = false;
                    break;
            }
#pragma warning restore IDE0010
        }

        /// <summary>
        /// Update visible fields and sections based on the system type
        /// </summary>
        private void UpdateFromSystemType(SubmissionInfo? submissionInfo)
        {
            var system = submissionInfo?.CommonDiscInfo?.System;

            // TODO: Do these need to be explicitly set if they're in the AXAML?
            CompatibleOSTextBox!.IsVisible = false;
            DiscKeyTextBox!.IsVisible = false;
            DiscIDTextBox!.IsVisible = false;
            NetYarozeGamesTextBox!.IsVisible = false;
            LanguageSelectionsDropDown!.IsVisible = false;

#pragma warning disable IDE0010
            switch (system)
            {
                case RedumpSystem.AppleMacintosh:
                    PCMacHybridGrid!.IsVisible = true;
                    CompatibleOSTextBox!.IsVisible = true;
                    break;

                case RedumpSystem.IBMPCcompatible:
                    PCMacHybridGrid!.IsVisible = true;
                    CompatibleOSTextBox!.IsVisible = true;
                    break;

                case RedumpSystem.NintendoWiiU:
                    DiscKeyTextBox!.IsVisible = true;
                    break;

                case RedumpSystem.SonyPlayStation:
                    NetYarozeGamesTextBox!.IsVisible = true;
                    break;

                case RedumpSystem.SonyPlayStation2:
                    LanguageSelectionsDropDown!.IsVisible = true;
                    break;

                case RedumpSystem.SonyPlayStation3:
                    DiscKeyTextBox!.IsVisible = true;
                    DiscIDTextBox!.IsVisible = true;
                    break;
            }
#pragma warning restore IDE0010
        }

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

        #region Event Handlers

        /// <summary>
        /// Handler for AcceptButton Click event
        /// </summary>
        private void OnAcceptClick(object? sender, RoutedEventArgs e)
        {
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
        {
            var ringCodeGuideWindow = new RingCodeGuideWindow();
            _ = ringCodeGuideWindow.ShowDialog(this);
        }

        #endregion
    }
}
