using System.Windows;
using MPF.Frontend;
using MPF.Frontend.ViewModels;
using SabreTools.RedumpLib.Data;

namespace MPF.UI.Windows
{
    /// <summary>
    /// Interaction logic for MediaInformationWindow.xaml
    /// </summary>
    public partial class MediaInformationWindow : WindowBase
    {
#if NET35

        #region Additional Info

        private UserInput? CompatibleOSTextBox => ItemHelper.FindChild<UserInput>(this, "CompatibleOSTextBox");
        private Grid? PCMacHybridGrid => ItemHelper.FindChild<Grid>(this, "PCMacHybridGrid");
        private UserInput? DiscIDTextBox => ItemHelper.FindChild<UserInput>(this, "DiscIDTextBox");
        private UserInput? DiscKeyTextBox => ItemHelper.FindChild<UserInput>(this, "DiscKeyTextBox");

        #endregion

        #region Contents

        private UserInput? NetYarozeGamesTextBox => ItemHelper.FindChild<UserInput>(this, "NetYarozeGamesTextBox");

        #endregion

        #region Ringcodes

        private GroupBox? L0Info => ItemHelper.FindChild<GroupBox>(this, "L0Info");
        private UserInput? L0MasteringCode => ItemHelper.FindChild<UserInput>(this, "L0MasteringCode");
        private UserInput? L0MasteringSID => ItemHelper.FindChild<UserInput>(this, "L0MasteringSID");
        private UserInput? L0Toolstamp => ItemHelper.FindChild<UserInput>(this, "L0Toolstamp");
        private UserInput? L0MouldSIDs => ItemHelper.FindChild<UserInput>(this, "L0MouldSIDs");
        private UserInput? L0AdditionalMoulds => ItemHelper.FindChild<UserInput>(this, "L0AdditionalMoulds");
        private GroupBox? L1Info => ItemHelper.FindChild<GroupBox>(this, "L1Info");
        private UserInput? L1MasteringCode => ItemHelper.FindChild<UserInput>(this, "L1MasteringCode");
        private UserInput? L1MasteringSID => ItemHelper.FindChild<UserInput>(this, "L1MasteringSID");
        private UserInput? L1Toolstamp => ItemHelper.FindChild<UserInput>(this, "L1Toolstamp");
        private UserInput? L1MouldSIDs => ItemHelper.FindChild<UserInput>(this, "L1MouldSIDs");
        private UserInput? L1AdditionalMoulds => ItemHelper.FindChild<UserInput>(this, "L1AdditionalMoulds");
        private GroupBox? L2Info => ItemHelper.FindChild<GroupBox>(this, "L2Info");
        private UserInput? L2MasteringCode => ItemHelper.FindChild<UserInput>(this, "L2MasteringCode");
        private UserInput? L2MasteringSID => ItemHelper.FindChild<UserInput>(this, "L2MasteringSID");
        private UserInput? L2Toolstamp => ItemHelper.FindChild<UserInput>(this, "L2Toolstamp");
        private GroupBox? L3Info => ItemHelper.FindChild<GroupBox>(this, "L3Info");
        private UserInput? L3MasteringCode => ItemHelper.FindChild<UserInput>(this, "L3MasteringCode");
        private UserInput? L3MasteringSID => ItemHelper.FindChild<UserInput>(this, "L3MasteringSID");
        private UserInput? L3Toolstamp => ItemHelper.FindChild<UserInput>(this, "L3Toolstamp");

        #endregion

        #region Read-Only Info

        private UserInput? FullyMatchedIDs => ItemHelper.FindChild<UserInput>(this, "FullyMatchedIDs");
        private UserInput? PartiallyMatchedIDs => ItemHelper.FindChild<UserInput>(this, "PartiallyMatchedIDs");
        private UserInput? HashData => ItemHelper.FindChild<UserInput>(this, "HashData");
        private UserInput? HashDataLayerbreak1 => ItemHelper.FindChild<UserInput>(this, "HashDataLayerbreak1");
        private UserInput? HashDataLayerbreak2 => ItemHelper.FindChild<UserInput>(this, "HashDataLayerbreak2");
        private UserInput? HashDataLayerbreak3 => ItemHelper.FindChild<UserInput>(this, "HashDataLayerbreak3");
        private UserInput? AntiModchip => ItemHelper.FindChild<UserInput>(this, "AntiModchip");
        private UserInput? DiscOffset => ItemHelper.FindChild<UserInput>(this, "DiscOffset");
        private UserInput? DMIHash => ItemHelper.FindChild<UserInput>(this, "DMIHash");
        private UserInput? EDC => ItemHelper.FindChild<UserInput>(this, "EDC");
        private UserInput? ErrorsCount => ItemHelper.FindChild<UserInput>(this, "ErrorsCount");
        private UserInput? EXEDateBuildDate => ItemHelper.FindChild<UserInput>(this, "EXEDateBuildDate");
        private UserInput? Filename => ItemHelper.FindChild<UserInput>(this, "Filename");
        private UserInput? Header => ItemHelper.FindChild<UserInput>(this, "Header");
        private UserInput? InternalName => ItemHelper.FindChild<UserInput>(this, "InternalName");
        private UserInput? InternalSerialName => ItemHelper.FindChild<UserInput>(this, "InternalSerialName");
        private UserInput? Multisession => ItemHelper.FindChild<UserInput>(this, "Multisession");
        private UserInput? PFIHash => ItemHelper.FindChild<UserInput>(this, "PFIHash");
        private UserInput? PIC => ItemHelper.FindChild<UserInput>(this, "PIC");
        private UserInput? PVD => ItemHelper.FindChild<UserInput>(this, "PVD");
        private UserInput? RingPerfectAudioOffset => ItemHelper.FindChild<UserInput>(this, "RingPerfectAudioOffset");
        private UserInput? SampleStart => ItemHelper.FindChild<UserInput>(this, "SampleStart");
        private UserInput? SecuROMData => ItemHelper.FindChild<UserInput>(this, "SecuROMData");
        private UserInput? SSHash => ItemHelper.FindChild<UserInput>(this, "SSHash");
        private UserInput? SecuritySectorRanges => ItemHelper.FindChild<UserInput>(this, "SecuritySectorRanges");
        private UserInput? SSVersion => ItemHelper.FindChild<UserInput>(this, "SSVersion");
        private UserInput? UniversalHash => ItemHelper.FindChild<UserInput>(this, "UniversalHash");
        private UserInput? VolumeLabel => ItemHelper.FindChild<UserInput>(this, "VolumeLabel");
        private UserInput? XeMID => ItemHelper.FindChild<UserInput>(this, "XeMID");
        private UserInput? XMID => ItemHelper.FindChild<UserInput>(this, "XMID");

        #endregion

        #region Accept / Cancel

        private Button? AcceptButton => ItemHelper.FindChild<Button>(this, "AcceptButton");
        private Button? CancelButton => ItemHelper.FindChild<Button>(this, "CancelButton");
        private Button? RingCodeGuideButton => ItemHelper.FindChild<Button>(this, "RingCodeGuideButton");

        #endregion

#endif

        /// <summary>
        /// Read-only access to the current media information view model
        /// </summary>
        public MediaInformationViewModel MediaInformationViewModel => DataContext as MediaInformationViewModel ?? new MediaInformationViewModel(new Options(), new SubmissionInfo());

        /// <summary>
        /// Constructor
        /// </summary>
        public MediaInformationWindow(Options options, SubmissionInfo? submissionInfo)
        {
#if NET40_OR_GREATER || NETCOREAPP
            InitializeComponent();
#endif

#if NET452_OR_GREATER || NETCOREAPP
            var chrome = new System.Windows.Shell.WindowChrome
            {
                CaptionHeight = 0,
                ResizeBorderThickness = new Thickness(0),
            };
            System.Windows.Shell.WindowChrome.SetWindowChrome(this, chrome);
#endif

            DataContext = new MediaInformationViewModel(options, submissionInfo);
            MediaInformationViewModel.Load();

            // Limit lists, if necessary
            if (options.Processing.MediaInformation.EnableRedumpCompatibility)
            {
                MediaInformationViewModel.SetRedumpRegions();
                MediaInformationViewModel.SetRedumpLanguages();
            }

            // Add handlers
            AcceptButton!.Click += OnAcceptClick;
            CancelButton!.Click += OnCancelClick;
            RingCodeGuideButton!.Click += OnRingCodeGuideClick;

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
                FullyMatchedIDs!.Visibility = Visibility.Collapsed;
            else
                FullyMatchedIDs!.Text = string.Join(", ", [.. submissionInfo.FullyMatchedIDs.ConvertAll(i => i.ToString())]);
            if (submissionInfo.PartiallyMatchedIDs is null || submissionInfo.PartiallyMatchedIDs.Count == 0)
                PartiallyMatchedIDs!.Visibility = Visibility.Collapsed;
            else
                PartiallyMatchedIDs!.Text = string.Join(", ", [.. submissionInfo.PartiallyMatchedIDs.ConvertAll(i => i.ToString())]);
            if (string.IsNullOrEmpty(submissionInfo.DumpMetadata.Dat))
                HashData!.Visibility = Visibility.Collapsed;
            if (submissionInfo.DiscIdentifiers.Layerbreak == 0)
                HashDataLayerbreak1!.Visibility = Visibility.Collapsed;
            if (submissionInfo.DiscIdentifiers.Layerbreak2 == 0)
                HashDataLayerbreak2!.Visibility = Visibility.Collapsed;
            if (submissionInfo.DiscIdentifiers.Layerbreak3 == 0)
                HashDataLayerbreak3!.Visibility = Visibility.Collapsed;
            if (submissionInfo.RingCodes.WriteOffset is null)
                DiscOffset!.Visibility = Visibility.Collapsed;
            if (ShouldCollapseComment(submissionInfo, SiteCode.DMIHash))
                DMIHash!.Visibility = Visibility.Collapsed;
            if (submissionInfo.DiscIdentifiers?.EDC is null)
                EDC!.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(submissionInfo.DiscIdentifiers?.ErrorCount))
                ErrorsCount!.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(submissionInfo.DiscIdentifiers?.EXEDate))
                EXEDateBuildDate!.Visibility = Visibility.Collapsed;
            if (ShouldCollapseComment(submissionInfo, SiteCode.Filename))
                Filename!.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(submissionInfo.DumpMetadata?.Header))
                Header!.Visibility = Visibility.Collapsed;
            if (ShouldCollapseComment(submissionInfo, SiteCode.InternalName))
                InternalName!.Visibility = Visibility.Collapsed;
            if (ShouldCollapseComment(submissionInfo, SiteCode.InternalSerialName))
                InternalSerialName!.Visibility = Visibility.Collapsed;
            if (ShouldCollapseComment(submissionInfo, SiteCode.Multisession))
                Multisession!.Visibility = Visibility.Collapsed;
            if (ShouldCollapseComment(submissionInfo, SiteCode.PFIHash))
                PFIHash!.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(submissionInfo.DumpMetadata?.PIC))
                PIC!.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(submissionInfo.DumpMetadata?.PVD))
                PVD!.Visibility = Visibility.Collapsed;
            if (ShouldCollapseComment(submissionInfo, SiteCode.RingPerfectAudioOffset))
                RingPerfectAudioOffset!.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(submissionInfo.RingCodes.SampleStart))
                SampleStart!.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(submissionInfo.DumpMetadata?.SBI))
                SecuROMData!.Visibility = Visibility.Collapsed;
            if (ShouldCollapseComment(submissionInfo, SiteCode.SSHash))
                SSHash!.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(submissionInfo.DumpMetadata?.SectorRanges))
                SecuritySectorRanges!.Visibility = Visibility.Collapsed;
            if (ShouldCollapseComment(submissionInfo, SiteCode.SSVersion))
                SSVersion!.Visibility = Visibility.Collapsed;
            if (ShouldCollapseComment(submissionInfo, SiteCode.UniversalHash))
                UniversalHash!.Visibility = Visibility.Collapsed;
            if (ShouldCollapseComment(submissionInfo, SiteCode.VolumeLabel))
                VolumeLabel!.Visibility = Visibility.Collapsed;
            if (ShouldCollapseComment(submissionInfo, SiteCode.XeMID))
                XeMID!.Visibility = Visibility.Collapsed;
            if (ShouldCollapseComment(submissionInfo, SiteCode.XMID))
                XMID!.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Update visible fields and sections based on the media type
        /// </summary>
        /// TODO: See if these can be done by binding
        private void UpdateFromDiscType(SubmissionInfo? submissionInfo)
        {
#pragma warning disable IDE0010
            switch (submissionInfo?.DiscIdentity?.Media)
            {
                case MediaType.CD:
                case MediaType.GDROM:
                    L0Info!.Header = "Data Side";
                    L0MasteringCode!.Label = "Mastering Code";
                    L0MasteringSID!.Label = "Mastering SID";
                    L0Toolstamp!.Label = "Toolstamps";
                    L0MouldSIDs!.Label = "Mould SIDs";
                    L0AdditionalMoulds!.Label = "Additional Moulds";

                    L1Info!.Header = "Label Side";
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
                        L2Info!.Visibility = Visibility.Visible;
                        L3Info!.Visibility = Visibility.Visible;

                        L0Info!.Header = "Layer 0";
                        L0MasteringCode!.Label = "Mastering Code";
                        L0MasteringSID!.Label = "Mastering SID";
                        L0Toolstamp!.Label = "Toolstamps";
                        L0MouldSIDs!.Label = "Data Side Mould SIDs";
                        L0AdditionalMoulds!.Label = "Data Side Additional Moulds";

                        L1Info!.Header = "Layer 1";
                        L1MasteringCode!.Label = "Mastering Code";
                        L1MasteringSID!.Label = "Mastering SID";
                        L1Toolstamp!.Label = "Toolstamps";
                        L1MouldSIDs!.Label = "Label Side Mould SIDs";
                        L1AdditionalMoulds!.Label = "Label Side Additional Moulds";

                        L2Info!.Header = "Layer 2";
                        L2MasteringCode!.Label = "Mastering Code";
                        L2MasteringSID!.Label = "Mastering SID";
                        L2Toolstamp!.Label = "Toolstamps";

                        L3Info!.Header = "Layer 3";
                        L3MasteringCode!.Label = "Mastering Code";
                        L3MasteringSID!.Label = "Mastering SID";
                        L3Toolstamp!.Label = "Toolstamps";
                    }

                    // Triple-layer discs
                    else if (submissionInfo?.DiscIdentifiers.Layerbreak2 != default(long))
                    {
                        L2Info!.Visibility = Visibility.Visible;

                        L0Info!.Header = "Layer 0";
                        L0MasteringCode!.Label = "Mastering Code";
                        L0MasteringSID!.Label = "Mastering SID";
                        L0Toolstamp!.Label = "Toolstamps";
                        L0MouldSIDs!.Label = "Data Side Mould SIDs";
                        L0AdditionalMoulds!.Label = "Data Side Additional Moulds";

                        L1Info!.Header = "Layer 1";
                        L1MasteringCode!.Label = "Mastering Code";
                        L1MasteringSID!.Label = "Mastering SID";
                        L1Toolstamp!.Label = "Toolstamps";
                        L1MouldSIDs!.Label = "Label Side Mould SIDs";
                        L1AdditionalMoulds!.Label = "Label Side Additional Moulds";

                        L2Info!.Header = "Layer 2";
                        L2MasteringCode!.Label = "Mastering Code";
                        L2MasteringSID!.Label = "Mastering SID";
                        L2Toolstamp!.Label = "Toolstamps";
                    }

                    // Double-layer discs
                    else if (submissionInfo?.DiscIdentifiers.Layerbreak != default(long))
                    {
                        L0Info!.Header = "Layer 0";
                        L0MasteringCode!.Label = "Mastering Code";
                        L0MasteringSID!.Label = "Mastering SID";
                        L0Toolstamp!.Label = "Toolstamps";
                        L0MouldSIDs!.Label = "Data Side Mould SIDs";
                        L0AdditionalMoulds!.Label = "Data Side Additional Moulds";

                        L1Info!.Header = "Layer 1";
                        L1MasteringCode!.Label = "Mastering Code";
                        L1MasteringSID!.Label = "Mastering SID";
                        L1Toolstamp!.Label = "Toolstamps";
                        L1MouldSIDs!.Label = "Label Side Mould SIDs";
                        L1AdditionalMoulds!.Label = "Label Side Additional Moulds";
                    }

                    // Single-layer discs
                    else
                    {
                        L0Info!.Header = "Data Side";
                        L0MasteringCode!.Label = "Mastering Code";
                        L0MasteringSID!.Label = "Mastering SID";
                        L0Toolstamp!.Label = "Toolstamps";
                        L0MouldSIDs!.Label = "Mould SIDs";
                        L0AdditionalMoulds!.Label = "Additional Moulds";

                        L1Info!.Header = "Label Side";
                        L1MasteringCode!.Label = "Mastering Code";
                        L1MasteringSID!.Label = "Mastering SID";
                        L1Toolstamp!.Label = "Toolstamps";
                        L1MouldSIDs!.Label = "Mould SIDs";
                        L1AdditionalMoulds!.Label = "Additional Moulds";
                    }

                    break;

                case MediaType.UMDSL:
                case MediaType.UMDDL:
                    L0Info!.Header = "Layer 0";
                    L0MasteringCode!.Label = "Mastering Code";
                    L0MasteringSID!.Label = "Mastering SID";
                    L0Toolstamp!.Label = "Toolstamps";
                    L0MouldSIDs!.Label = "Data Side Mould SIDs";
                    L0AdditionalMoulds!.Label = "Data Side Additional Moulds";

                    L1Info!.Header = "Layer 1";
                    L1MasteringCode!.Label = "Mastering Code";
                    L1MasteringSID!.Label = "Mastering SID";
                    L1Toolstamp!.Label = "Toolstamps";
                    L1MouldSIDs!.Label = "Label Side Mould SIDs";
                    L1AdditionalMoulds!.Label = "Label Side Additional Moulds";
                    break;

                // All other media we assume to have no rings
                default:
                    L0Info!.Visibility = Visibility.Collapsed;
                    L1Info!.Visibility = Visibility.Collapsed;
                    L2Info!.Visibility = Visibility.Collapsed;
                    L3Info!.Visibility = Visibility.Collapsed;
                    break;
            }
#pragma warning restore IDE0010
        }

        /// <summary>
        /// Update visible fields and sections based on the system type
        /// </summary>
        /// TODO: See if these can be done by binding
        private void UpdateFromSystemType(SubmissionInfo? submissionInfo)
        {
            var system = submissionInfo?.DiscIdentity?.System;
#pragma warning disable IDE0010
            switch (system)
            {
                case PhysicalSystem.AppleMacintosh:
                    PCMacHybridGrid!.Visibility = Visibility.Visible;
                    CompatibleOSTextBox!.Visibility = Visibility.Visible;
                    break;

                case PhysicalSystem.IBMPCcompatible:
                    PCMacHybridGrid!.Visibility = Visibility.Visible;
                    CompatibleOSTextBox!.Visibility = Visibility.Visible;
                    break;

                case PhysicalSystem.NintendoWiiU:
                    DiscKeyTextBox!.Visibility = Visibility.Visible;
                    break;

                case PhysicalSystem.SonyPlayStation:
                    NetYarozeGamesTextBox!.Visibility = Visibility.Visible;
                    break;

                case PhysicalSystem.SonyPlayStation3:
                    DiscKeyTextBox!.Visibility = Visibility.Visible;
                    DiscIDTextBox!.Visibility = Visibility.Visible;
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
        private void OnAcceptClick(object sender, RoutedEventArgs e)
        {
            MediaInformationViewModel.Save();
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Handler for CancelButton Click event
        /// </summary>
        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        /// <summary>
        /// Handler for RingCodeGuideButton Click event
        /// </summary>
        private void OnRingCodeGuideClick(object sender, RoutedEventArgs e)
        {
            var ringCodeGuideWindow = new RingCodeGuideWindow()
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };
            ringCodeGuideWindow.Show();
        }

        #endregion
    }
}
