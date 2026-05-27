using Avalonia.Controls;
using Avalonia.Interactivity;
using MPF.Frontend;
using MPF.Frontend.ViewModels;
using MPF.UI.Avalonia.Controls;
using SabreTools.RedumpLib.Data;

namespace MPF.UI.Avalonia.Views
{
    /// <summary>
    /// Interaction logic for MediaInformationWindow.axaml
    /// </summary>
    public partial class MediaInformationWindow : WindowBase
    {
        /// <summary>
        /// Read-only access to the current media information view model
        /// </summary>
        public MediaInformationViewModel MediaInformationViewModel => (MediaInformationViewModel)DataContext!;

        /// <summary>
        /// String-keyed adapter for CommentsSpecialFields so Avalonia bindings can use
        /// [EnumMemberName] indexer keys instead of WPF's unsupported (ns:Type)Member syntax.
        /// </summary>
        public SiteCodeStringDictionary CommentsFields { get; }

        /// <summary>
        /// String-keyed adapter for ContentsSpecialFields so Avalonia bindings can use
        /// [EnumMemberName] indexer keys instead of WPF's unsupported (ns:Type)Member syntax.
        /// </summary>
        public SiteCodeStringDictionary ContentsFields { get; }

        /// <summary>
        /// Bool adapter for the PCMacHybrid boolean SiteCode stored in CommentsSpecialFields.
        /// Reads "true"/"false" string values and writes back the bool as a string.
        /// </summary>
        public bool? PCMacHybridChecked
        {
            get
            {
                var v = CommentsFields[nameof(SiteCode.PCMacHybrid)];
                if (string.IsNullOrEmpty(v)) return null;
                return bool.TryParse(v, out var b) ? b : (bool?)null;
            }
            set
            {
                CommentsFields[nameof(SiteCode.PCMacHybrid)] = value?.ToString().ToLowerInvariant() ?? string.Empty;
            }
        }

        /// <summary>
        /// Designer / XAML-compiler constructor — not for runtime use.
        /// </summary>
        public MediaInformationWindow() : this(new Options(), new SubmissionInfo()) { }

        /// <summary>
        /// Constructor
        /// </summary>
        public MediaInformationWindow(Options options, SubmissionInfo? submissionInfo)
        {
            // Initialize adapters BEFORE InitializeComponent so bindings in AXAML resolve correctly.
            // The accessors are lambdas that re-fetch the live dictionary on every call, so they remain
            // valid even if the VM replaces the dictionary instance during Load().
            CommentsFields = new SiteCodeStringDictionary(
                () => MediaInformationViewModel.SubmissionInfo?.CommonDiscInfo?.CommentsSpecialFields);
            ContentsFields = new SiteCodeStringDictionary(
                () => MediaInformationViewModel.SubmissionInfo?.CommonDiscInfo?.ContentsSpecialFields);

            InitializeComponent();

            DataContext = new MediaInformationViewModel(options, submissionInfo);
            MediaInformationViewModel.Load();

            // Limit lists, if necessary
            if (options.Processing.MediaInformation.EnableRedumpCompatibility)
            {
                MediaInformationViewModel.SetRedumpRegions();
                MediaInformationViewModel.SetRedumpLanguages();
            }

            // Update UI with new values
            ManipulateFields(options, submissionInfo);
        }

        #region Helpers

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
            L0MasteringRing.Tab = true;
            L0MasteringSID.Tab = true;
            L0Toolstamp.Tab = true;
            L0MouldSID.Tab = true;
            L0AdditionalMould.Tab = true;

            // L1
            L1MasteringRing.Tab = true;
            L1MasteringSID.Tab = true;
            L1Toolstamp.Tab = true;
            L1MouldSID.Tab = true;
            L1AdditionalMould.Tab = true;

            // L2
            L2MasteringRing.Tab = true;
            L2MasteringSID.Tab = true;
            L2Toolstamp.Tab = true;

            // L3
            L3MasteringRing.Tab = true;
            L3MasteringSID.Tab = true;
            L3Toolstamp.Tab = true;
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
                FullyMatchedID.IsVisible = false;
            if (submissionInfo.PartiallyMatchedIDs is null || submissionInfo.PartiallyMatchedIDs.Count == 0)
                PartiallyMatchedIDs.IsVisible = false;
            else
                PartiallyMatchedIDs.Text = string.Join(", ", [.. submissionInfo.PartiallyMatchedIDs.ConvertAll(i => i.ToString())]);
            if (string.IsNullOrEmpty(submissionInfo.TracksAndWriteOffsets.ClrMameProData))
                HashData.IsVisible = false;
            if (submissionInfo.SizeAndChecksums.Size == 0)
                HashDataSize.IsVisible = false;
            if (string.IsNullOrEmpty(submissionInfo.SizeAndChecksums.CRC32))
                HashDataCRC.IsVisible = false;
            if (string.IsNullOrEmpty(submissionInfo.SizeAndChecksums.MD5))
                HashDataMD5.IsVisible = false;
            if (string.IsNullOrEmpty(submissionInfo.SizeAndChecksums.SHA1))
                HashDataSHA1.IsVisible = false;
            if (submissionInfo.SizeAndChecksums.Layerbreak == 0)
                HashDataLayerbreak1.IsVisible = false;
            if (submissionInfo.SizeAndChecksums.Layerbreak2 == 0)
                HashDataLayerbreak2.IsVisible = false;
            if (submissionInfo.SizeAndChecksums.Layerbreak3 == 0)
                HashDataLayerbreak3.IsVisible = false;
            if (submissionInfo.CopyProtection?.AntiModchip is null)
                AntiModchip.IsVisible = false;
            if (submissionInfo.TracksAndWriteOffsets.OtherWriteOffsets is null)
                DiscOffset.IsVisible = false;
            if (ShouldCollapseComment(submissionInfo, SiteCode.DMIHash))
                DMIHash.IsVisible = false;
            if (submissionInfo.EDC?.EDC is null)
                EDC.IsVisible = false;
            if (string.IsNullOrEmpty(submissionInfo.CommonDiscInfo?.ErrorsCount))
                ErrorsCount.IsVisible = false;
            if (string.IsNullOrEmpty(submissionInfo.CommonDiscInfo?.EXEDateBuildDate))
                EXEDateBuildDate.IsVisible = false;
            if (ShouldCollapseComment(submissionInfo, SiteCode.Filename))
                Filename.IsVisible = false;
            if (string.IsNullOrEmpty(submissionInfo.Extras?.Header))
                Header.IsVisible = false;
            if (ShouldCollapseComment(submissionInfo, SiteCode.InternalName))
                InternalName.IsVisible = false;
            if (ShouldCollapseComment(submissionInfo, SiteCode.InternalSerialName))
                InternalSerialName.IsVisible = false;
            if (ShouldCollapseComment(submissionInfo, SiteCode.Multisession))
                Multisession.IsVisible = false;
            if (submissionInfo.CopyProtection?.LibCrypt is null)
                LibCrypt.IsVisible = false;
            if (string.IsNullOrEmpty(submissionInfo.CopyProtection?.LibCryptData))
                LibCryptData.IsVisible = false;
            if (ShouldCollapseComment(submissionInfo, SiteCode.PFIHash))
                PFIHash.IsVisible = false;
            if (string.IsNullOrEmpty(submissionInfo.Extras?.PIC))
                PIC.IsVisible = false;
            if (string.IsNullOrEmpty(submissionInfo.Extras?.PVD))
                PVD.IsVisible = false;
            if (ShouldCollapseComment(submissionInfo, SiteCode.RingNonZeroDataStart))
                RingNonZeroDataStart.IsVisible = false;
            if (ShouldCollapseComment(submissionInfo, SiteCode.RingPerfectAudioOffset))
                RingPerfectAudioOffset.IsVisible = false;
            if (string.IsNullOrEmpty(submissionInfo.CopyProtection?.SecuROMData))
                SecuROMData.IsVisible = false;
            if (ShouldCollapseComment(submissionInfo, SiteCode.SSHash))
                SSHash.IsVisible = false;
            if (string.IsNullOrEmpty(submissionInfo.Extras?.SecuritySectorRanges))
                SecuritySectorRanges.IsVisible = false;
            if (ShouldCollapseComment(submissionInfo, SiteCode.SSVersion))
                SSVersion.IsVisible = false;
            if (ShouldCollapseComment(submissionInfo, SiteCode.UniversalHash))
                UniversalHash.IsVisible = false;
            if (ShouldCollapseComment(submissionInfo, SiteCode.VolumeLabel))
                VolumeLabel.IsVisible = false;
            if (ShouldCollapseComment(submissionInfo, SiteCode.XeMID))
                XeMID.IsVisible = false;
            if (ShouldCollapseComment(submissionInfo, SiteCode.XMID))
                XMID.IsVisible = false;
        }

        /// <summary>
        /// Update visible fields and sections based on the media type
        /// </summary>
        /// TODO: See if these can be done by binding
        private void UpdateFromDiscType(SubmissionInfo? submissionInfo)
        {
            // Sony-printed discs have layers in the opposite order
            var system = submissionInfo?.CommonDiscInfo?.System;
            bool reverseOrder = system.HasReversedRingcodes();

            switch (submissionInfo?.CommonDiscInfo?.Media)
            {
                case DiscType.CD:
                case DiscType.GDROM:
                    L0Info.Header = "Data Side";
                    L0MasteringRing.Label = "Mastering Ring";
                    L0MasteringSID.Label = "Mastering SID";
                    L0Toolstamp.Label = "Toolstamp/Mastering Code";
                    L0MouldSID.Label = "Mould SID";
                    L0AdditionalMould.Label = "Additional Mould";

                    L1Info.Header = "Label Side";
                    L1MasteringRing.Label = "Mastering Ring";
                    L1MasteringSID.Label = "Mastering SID";
                    L1Toolstamp.Label = "Toolstamp/Mastering Code";
                    L1MouldSID.Label = "Mould SID";
                    L1AdditionalMould.Label = "Additional Mould";
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
                        L2Info.IsVisible = true;
                        L3Info.IsVisible = true;

                        L0Info.Header = reverseOrder ? "Layer 0 (Outer)" : "Layer 0 (Inner)";
                        L0MasteringRing.Label = "Mastering Ring";
                        L0MasteringSID.Label = "Mastering SID";
                        L0Toolstamp.Label = "Toolstamp/Mastering Code";
                        L0MouldSID.Label = "Data Side Mould SID";
                        L0AdditionalMould.Label = "Data Side Additional Mould";

                        L1Info.Header = "Layer 1";
                        L1MasteringRing.Label = "Mastering Ring";
                        L1MasteringSID.Label = "Mastering SID";
                        L1Toolstamp.Label = "Toolstamp/Mastering Code";
                        L1MouldSID.Label = "Label Side Mould SID";
                        L1AdditionalMould.Label = "Label Side Additional Mould";

                        L2Info.Header = "Layer 2";
                        L2MasteringRing.Label = "Mastering Ring";
                        L2MasteringSID.Label = "Mastering SID";
                        L2Toolstamp.Label = "Toolstamp/Mastering Code";

                        L3Info.Header = reverseOrder ? "Layer 3 (Inner)" : "Layer 3 (Outer)";
                        L3MasteringRing.Label = "Mastering Ring";
                        L3MasteringSID.Label = "Mastering SID";
                        L3Toolstamp.Label = "Toolstamp/Mastering Code";
                    }

                    // Triple-layer discs
                    else if (submissionInfo?.SizeAndChecksums.Layerbreak2 != default(long))
                    {
                        L2Info.IsVisible = true;

                        L0Info.Header = reverseOrder ? "Layer 0 (Outer)" : "Layer 0 (Inner)";
                        L0MasteringRing.Label = "Mastering Ring";
                        L0MasteringSID.Label = "Mastering SID";
                        L0Toolstamp.Label = "Toolstamp/Mastering Code";
                        L0MouldSID.Label = "Data Side Mould SID";
                        L0AdditionalMould.Label = "Data Side Additional Mould";

                        L1Info.Header = "Layer 1";
                        L1MasteringRing.Label = "Mastering Ring";
                        L1MasteringSID.Label = "Mastering SID";
                        L1Toolstamp.Label = "Toolstamp/Mastering Code";
                        L1MouldSID.Label = "Label Side Mould SID";
                        L1AdditionalMould.Label = "Label Side Additional Mould";

                        L2Info.Header = reverseOrder ? "Layer 2 (Inner)" : "Layer 2 (Outer)";
                        L2MasteringRing.Label = "Mastering Ring";
                        L2MasteringSID.Label = "Mastering SID";
                        L2Toolstamp.Label = "Toolstamp/Mastering Code";
                    }

                    // Double-layer discs
                    else if (submissionInfo?.SizeAndChecksums.Layerbreak != default(long))
                    {
                        L0Info.Header = reverseOrder ? "Layer 0 (Outer)" : "Layer 0 (Inner)";
                        L0MasteringRing.Label = "Mastering Ring";
                        L0MasteringSID.Label = "Mastering SID";
                        L0Toolstamp.Label = "Toolstamp/Mastering Code";
                        L0MouldSID.Label = "Data Side Mould SID";
                        L0AdditionalMould.Label = "Data Side Additional Mould";

                        L1Info.Header = reverseOrder ? "Layer 1 (Inner)" : "Layer 1 (Outer)";
                        L1MasteringRing.Label = "Mastering Ring";
                        L1MasteringSID.Label = "Mastering SID";
                        L1Toolstamp.Label = "Toolstamp/Mastering Code";
                        L1MouldSID.Label = "Label Side Mould SID";
                        L1AdditionalMould.Label = "Label Side Additional Mould";
                    }

                    // Single-layer discs
                    else
                    {
                        L0Info.Header = "Data Side";
                        L0MasteringRing.Label = "Mastering Ring";
                        L0MasteringSID.Label = "Mastering SID";
                        L0Toolstamp.Label = "Toolstamp/Mastering Code";
                        L0MouldSID.Label = "Mould SID";
                        L0AdditionalMould.Label = "Additional Mould";

                        L1Info.Header = "Label Side";
                        L1MasteringRing.Label = "Mastering Ring";
                        L1MasteringSID.Label = "Mastering SID";
                        L1Toolstamp.Label = "Toolstamp/Mastering Code";
                        L1MouldSID.Label = "Mould SID";
                        L1AdditionalMould.Label = "Additional Mould";
                    }

                    break;

                case DiscType.UMDSL:
                case DiscType.UMDDL:
                    L0Info.Header = reverseOrder ? "Layer 0 (Outer)" : "Layer 0 (Inner)";
                    L0MasteringRing.Label = "Mastering Ring";
                    L0MasteringSID.Label = "Mastering SID";
                    L0Toolstamp.Label = "Toolstamp/Mastering Code";
                    L0MouldSID.Label = "Data Side Mould SID";
                    L0AdditionalMould.Label = "Data Side Additional Mould";

                    L1Info.Header = reverseOrder ? "Layer 1 (Inner)" : "Layer 1 (Outer)";
                    L1MasteringRing.Label = "Mastering Ring";
                    L1MasteringSID.Label = "Mastering SID";
                    L1Toolstamp.Label = "Toolstamp/Mastering Code";
                    L1MouldSID.Label = "Label Side Mould SID";
                    L1AdditionalMould.Label = "Label Side Additional Mould";
                    break;

                // All other media we assume to have no rings
                default:
                    L0Info.IsVisible = false;
                    L1Info.IsVisible = false;
                    L2Info.IsVisible = false;
                    L3Info.IsVisible = false;
                    break;
            }
        }

        /// <summary>
        /// Update visible fields and sections based on the system type
        /// </summary>
        /// TODO: See if these can be done by binding
        private void UpdateFromSystemType(SubmissionInfo? submissionInfo)
        {
            var system = submissionInfo?.CommonDiscInfo?.System;
            switch (system)
            {
                case RedumpSystem.AppleMacintosh:
                    PCMacHybridGrid.IsVisible = true;
                    CompatibleOSTextBox.IsVisible = true;
                    break;

                case RedumpSystem.IBMPCcompatible:
                    PCMacHybridGrid.IsVisible = true;
                    CompatibleOSTextBox.IsVisible = true;
                    break;

                case RedumpSystem.NintendoWiiU:
                    DiscKeyTextBox.IsVisible = true;
                    break;

                case RedumpSystem.SonyPlayStation:
                    NetYarozeGamesTextBox.IsVisible = true;
                    break;

                case RedumpSystem.SonyPlayStation2:
                    LanguageSelectionGrid.IsVisible = true;
                    break;

                case RedumpSystem.SonyPlayStation3:
                    DiscKeyTextBox.IsVisible = true;
                    DiscIDTextBox.IsVisible = true;
                    break;
            }
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
        private async void OnRingCodeGuideClick(object? sender, RoutedEventArgs e)
        {
            // TODO(Task 13): open RingCodeGuideWindow once it has been ported.
            await MessageBoxWindow.ShowAsync(this,
                FindResourceString("RingCodeGuideTitleString"),
                "This window is not yet available in the macOS UI.",
                MessageBoxButtons.Ok);
        }

        #endregion

        // ──────────────────────────────────────────────────────────────────────
        // Helpers
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Look up a string resource by key, returning an empty string when missing.
        /// </summary>
        private string FindResourceString(string key)
            => this.TryFindResource(key, out var value) && value is string s ? s : string.Empty;
    }
}
