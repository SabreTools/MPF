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
            RegionDropDown!.ItemsSource = MediaInformationViewModel.Regions;
            LanguagesDropDown!.ItemsSource = MediaInformationViewModel.Languages;
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
            UpdateFromMediaType(submissionInfo);

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
            L0MasteringCode!.Tab = true;
            L0MasteringSID!.Tab = true;
            L0Toolstamp!.Tab = true;
            L0MouldSIDs!.Tab = true;
            L0AdditionalMoulds!.Tab = true;

            // L1
            L1MasteringCode!.Tab = true;
            L1MasteringSID!.Tab = true;
            L1Toolstamp!.Tab = true;
            L1MouldSIDs!.Tab = true;
            L1AdditionalMoulds!.Tab = true;

            // L2
            L2MasteringCode!.Tab = true;
            L2MasteringSID!.Tab = true;
            L2Toolstamp!.Tab = true;

            // L3
            L3MasteringCode!.Tab = true;
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

            if (submissionInfo.FullyMatchedIDs is null || submissionInfo.FullyMatchedIDs.Count == 0)
                FullyMatchedIDs!.IsVisible = false;
            if (submissionInfo.PartiallyMatchedIDs is null || submissionInfo.PartiallyMatchedIDs.Count == 0)
                PartiallyMatchedIDs!.IsVisible = false;
            else
                PartiallyMatchedIDs!.Text = string.Join(", ", [.. submissionInfo.PartiallyMatchedIDs.ConvertAll(i => i.ToString())]);
            if (string.IsNullOrEmpty(submissionInfo.DumpMetadata.Dat))
                HashData!.IsVisible = false;
            if (submissionInfo.DiscIdentifiers.Layerbreak == 0)
                HashDataLayerbreak1!.IsVisible = false;
            if (submissionInfo.DiscIdentifiers.Layerbreak2 == 0)
                HashDataLayerbreak2!.IsVisible = false;
            if (submissionInfo.DiscIdentifiers.Layerbreak3 == 0)
                HashDataLayerbreak3!.IsVisible = false;
            if (submissionInfo.RingCodes.WriteOffset is null)
                DiscOffset!.IsVisible = false;
            if (ShouldCollapseComment(submissionInfo, SiteCode.DMIHash))
                DMIHash!.IsVisible = false;
            if (submissionInfo.DiscIdentifiers?.EDC is null)
                EDC!.IsVisible = false;
            if (string.IsNullOrEmpty(submissionInfo.DiscIdentifiers?.ErrorCount))
                ErrorsCount!.IsVisible = false;
            if (string.IsNullOrEmpty(submissionInfo.DiscIdentifiers?.EXEDate))
                EXEDateBuildDate!.IsVisible = false;
            if (ShouldCollapseComment(submissionInfo, SiteCode.Filename))
                Filename!.IsVisible = false;
            if (string.IsNullOrEmpty(submissionInfo.DumpMetadata?.Header))
                Header!.IsVisible = false;
            if (ShouldCollapseComment(submissionInfo, SiteCode.InternalName))
                InternalName!.IsVisible = false;
            if (ShouldCollapseComment(submissionInfo, SiteCode.InternalSerialName))
                InternalSerialName!.IsVisible = false;
            if (ShouldCollapseComment(submissionInfo, SiteCode.Multisession))
                Multisession!.IsVisible = false;
            if (ShouldCollapseComment(submissionInfo, SiteCode.PFIHash))
                PFIHash!.IsVisible = false;
            if (string.IsNullOrEmpty(submissionInfo.DumpMetadata?.PIC))
                PIC!.IsVisible = false;
            if (string.IsNullOrEmpty(submissionInfo.DumpMetadata?.PVD))
                PVD!.IsVisible = false;
            if (ShouldCollapseComment(submissionInfo, SiteCode.RingNonZeroDataStart))
                RingNonZeroDataStart!.IsVisible = false;
            if (ShouldCollapseComment(submissionInfo, SiteCode.RingPerfectAudioOffset))
                RingPerfectAudioOffset!.IsVisible = false;
            if (string.IsNullOrEmpty(submissionInfo.DumpMetadata?.SBI))
                SecuROMData!.IsVisible = false;
            if (ShouldCollapseComment(submissionInfo, SiteCode.SSHash))
                SSHash!.IsVisible = false;
            if (string.IsNullOrEmpty(submissionInfo.DumpMetadata?.SectorRanges))
                SecuritySectorRanges!.IsVisible = false;
            if (ShouldCollapseComment(submissionInfo, SiteCode.SSVersion))
                SSVersion!.IsVisible = false;
            if (string.IsNullOrEmpty(submissionInfo.DiscIdentifiers?.UniversalHash))
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
        private void UpdateFromMediaType(SubmissionInfo? submissionInfo)
        {
            // Sony-printed discs have layers in the opposite order
            var system = submissionInfo?.DiscIdentity?.System;
            bool reverseOrder = system.HasReversedRingcodes();

            // TODO: Do these need to be explicitly set if they're in the AXAML?
            PCMacHybridGrid!.IsVisible = _showPcMacHybridAlways
                || submissionInfo?.DiscIdentity?.Media == MediaType.CD;
            L0InfoPanel!.IsVisible = true;
            L1InfoPanel!.IsVisible = true;
            L2InfoPanel!.IsVisible = false;
            L3InfoPanel!.IsVisible = false;

#pragma warning disable IDE0010
            switch (submissionInfo?.DiscIdentity?.Media)
            {
                case MediaType.CD:
                case MediaType.GDROM:
                    L0HeaderText!.Text = "Data Side";
                    L0MasteringCode!.Label = "Mastering Code";
                    L0MasteringSID!.Label = "Mastering SID";
                    L0Toolstamp!.Label = "Toolstamps";
                    L0MouldSIDs!.Label = "Mould SIDs";
                    L0AdditionalMoulds!.Label = "Additional Moulds";

                    L1HeaderText!.Text = "Label Side";
                    L1MasteringCode!.Label = "Mastering Code";
                    L1MasteringSID!.Label = "Mastering SID";
                    L1Toolstamp!.Label = "Toolstamps";
                    L1MouldSIDs!.Label = "Mould SIDs";
                    L1AdditionalMoulds!.Label = "Additional Moulds";
                    break;

                case MediaType.DVD5:
                case MediaType.DVD9:
                case MediaType.HDDVDSL:
                case MediaType.HDDVDDL:
                case MediaType.BD25:
                case MediaType.BD33:
                case MediaType.BD50:
                case MediaType.BD66:
                case MediaType.BD100:
                case MediaType.BD128:
                case MediaType.NintendoGameCubeGameDisc:
                case MediaType.NintendoWiiOpticalDiscSL:
                case MediaType.NintendoWiiOpticalDiscDL:
                case MediaType.NintendoWiiUOpticalDiscSL:
                    // Quad-layer discs
                    if (submissionInfo?.DiscIdentifiers.Layerbreak3 != default(long))
                    {
                        L2InfoPanel!.IsVisible = true;
                        L3InfoPanel!.IsVisible = true;

                        L0HeaderText!.Text = reverseOrder ? "Layer 0 (Outer)" : "Layer 0 (Inner)";
                        L0MasteringCode!.Label = "Mastering Code";
                        L0MasteringSID!.Label = "Mastering SID";
                        L0Toolstamp!.Label = "Toolstamps";
                        L0MouldSIDs!.Label = "Data Side Mould SIDs";
                        L0AdditionalMoulds!.Label = "Data Side Additional Moulds";

                        L1HeaderText!.Text = "Layer 1";
                        L1MasteringCode!.Label = "Mastering Code";
                        L1MasteringSID!.Label = "Mastering SID";
                        L1Toolstamp!.Label = "Toolstamps";
                        L1MouldSIDs!.Label = "Label Side Mould SIDs";
                        L1AdditionalMoulds!.Label = "Label Side Additional Moulds";

                        L2HeaderText!.Text = "Layer 2";
                        L2MasteringCode!.Label = "Mastering Code";
                        L2MasteringSID!.Label = "Mastering SID";
                        L2Toolstamp!.Label = "Toolstamps";

                        L3HeaderText!.Text = reverseOrder ? "Layer 3 (Inner)" : "Layer 3 (Outer)";
                        L3MasteringCode!.Label = "Mastering Code";
                        L3MasteringSID!.Label = "Mastering SID";
                        L3Toolstamp!.Label = "Toolstamps";
                    }

                    // Triple-layer discs
                    else if (submissionInfo?.DiscIdentifiers.Layerbreak2 != default(long))
                    {
                        L2InfoPanel!.IsVisible = true;

                        L0HeaderText!.Text = reverseOrder ? "Layer 0 (Outer)" : "Layer 0 (Inner)";
                        L0MasteringCode!.Label = "Mastering Code";
                        L0MasteringSID!.Label = "Mastering SID";
                        L0Toolstamp!.Label = "Toolstamps";
                        L0MouldSIDs!.Label = "Data Side Mould SIDs";
                        L0AdditionalMoulds!.Label = "Data Side Additional Moulds";

                        L1HeaderText!.Text = "Layer 1";
                        L1MasteringCode!.Label = "Mastering Code";
                        L1MasteringSID!.Label = "Mastering SID";
                        L1Toolstamp!.Label = "Toolstamps";
                        L1MouldSIDs!.Label = "Label Side Mould SIDs";
                        L1AdditionalMoulds!.Label = "Label Side Additional Moulds";

                        L2HeaderText!.Text = reverseOrder ? "Layer 2 (Inner)" : "Layer 2 (Outer)";
                        L2MasteringCode!.Label = "Mastering Code";
                        L2MasteringSID!.Label = "Mastering SID";
                        L2Toolstamp!.Label = "Toolstamps";
                    }

                    // Double-layer discs
                    else if (submissionInfo?.DiscIdentifiers.Layerbreak != default(long))
                    {
                        L0HeaderText!.Text = reverseOrder ? "Layer 0 (Outer)" : "Layer 0 (Inner)";
                        L0MasteringCode!.Label = "Mastering Code";
                        L0MasteringSID!.Label = "Mastering SID";
                        L0Toolstamp!.Label = "Toolstamps";
                        L0MouldSIDs!.Label = "Data Side Mould SIDs";
                        L0AdditionalMoulds!.Label = "Data Side Additional Moulds";

                        L1HeaderText!.Text = reverseOrder ? "Layer 1 (Inner)" : "Layer 1 (Outer)";
                        L1MasteringCode!.Label = "Mastering Code";
                        L1MasteringSID!.Label = "Mastering SID";
                        L1Toolstamp!.Label = "Toolstamps";
                        L1MouldSIDs!.Label = "Label Side Mould SIDs";
                        L1AdditionalMoulds!.Label = "Label Side Additional Moulds";
                    }

                    // Single-layer discs
                    else
                    {
                        L0HeaderText!.Text = "Data Side";
                        L0MasteringCode!.Label = "Mastering Code";
                        L0MasteringSID!.Label = "Mastering SID";
                        L0Toolstamp!.Label = "Toolstamps";
                        L0MouldSIDs!.Label = "Mould SIDs";
                        L0AdditionalMoulds!.Label = "Additional Moulds";

                        L1HeaderText!.Text = "Label Side";
                        L1MasteringCode!.Label = "Mastering Code";
                        L1MasteringSID!.Label = "Mastering SID";
                        L1Toolstamp!.Label = "Toolstamps";
                        L1MouldSIDs!.Label = "Mould SIDs";
                        L1AdditionalMoulds!.Label = "Additional Moulds";
                    }

                    break;

                case MediaType.UMDSL:
                case MediaType.UMDDL:
                    L0HeaderText!.Text = reverseOrder ? "Layer 0 (Outer)" : "Layer 0 (Inner)";
                    L0MasteringCode!.Label = "Mastering Code";
                    L0MasteringSID!.Label = "Mastering SID";
                    L0Toolstamp!.Label = "Toolstamps";
                    L0MouldSIDs!.Label = "Data Side Mould SIDs";
                    L0AdditionalMoulds!.Label = "Data Side Additional Moulds";

                    L1HeaderText!.Text = reverseOrder ? "Layer 1 (Inner)" : "Layer 1 (Outer)";
                    L1MasteringCode!.Label = "Mastering Code";
                    L1MasteringSID!.Label = "Mastering SID";
                    L1Toolstamp!.Label = "Toolstamps";
                    L1MouldSIDs!.Label = "Label Side Mould SIDs";
                    L1AdditionalMoulds!.Label = "Label Side Additional Moulds";
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
            var system = submissionInfo?.DiscIdentity?.System;

            // TODO: Do these need to be explicitly set if they're in the AXAML?
            CompatibleOSTextBox!.IsVisible = false;
            DiscKeyTextBox!.IsVisible = false;
            DiscIDTextBox!.IsVisible = false;
            NetYarozeGamesTextBox!.IsVisible = false;

#pragma warning disable IDE0010
            switch (system)
            {
                case PhysicalSystem.AppleMacintosh:
                    PCMacHybridGrid!.IsVisible = true;
                    CompatibleOSTextBox!.IsVisible = true;
                    break;

                case PhysicalSystem.IBMPCcompatible:
                    PCMacHybridGrid!.IsVisible = true;
                    CompatibleOSTextBox!.IsVisible = true;
                    break;

                case PhysicalSystem.NintendoWiiU:
                    DiscKeyTextBox!.IsVisible = true;
                    break;

                case PhysicalSystem.SonyPlayStation:
                    NetYarozeGamesTextBox!.IsVisible = true;
                    break;

                case PhysicalSystem.SonyPlayStation3:
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
            if (submissionInfo?.DumpMetadata?.CommentsSpecialFields is null)
                return true;

            // If the key doesn't exist
            if (!submissionInfo.DumpMetadata.CommentsSpecialFields.TryGetValue(siteCode, out string? value))
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
