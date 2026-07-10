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

        private GroupBox? Layer0Info => ItemHelper.FindChild<GroupBox>(this, "Layer0Info");
        private UserInput? Layer0MasteringCode => ItemHelper.FindChild<UserInput>(this, "Layer0MasteringCode");
        private UserInput? Layer0MasteringSID => ItemHelper.FindChild<UserInput>(this, "Layer0MasteringSID");
        private UserInput? Layer0Toolstamp => ItemHelper.FindChild<UserInput>(this, "Layer0Toolstamp");
        private UserInput? Layer0MouldSIDs => ItemHelper.FindChild<UserInput>(this, "Layer0MouldSIDs");
        private UserInput? Layer0AdditionalMoulds => ItemHelper.FindChild<UserInput>(this, "Layer0AdditionalMoulds");

        private GroupBox? Layer1Info => ItemHelper.FindChild<GroupBox>(this, "Layer1Info");
        private UserInput? Layer1MasteringCode => ItemHelper.FindChild<UserInput>(this, "Layer1MasteringCode");
        private UserInput? Layer1MasteringSID => ItemHelper.FindChild<UserInput>(this, "Layer1MasteringSID");
        private UserInput? Layer1Toolstamp => ItemHelper.FindChild<UserInput>(this, "Layer1Toolstamp");
        private UserInput? Layer1MouldSIDs => ItemHelper.FindChild<UserInput>(this, "Layer1MouldSIDs");
        private UserInput? Layer1AdditionalMoulds => ItemHelper.FindChild<UserInput>(this, "Layer1AdditionalMoulds");

        private GroupBox? Layer2Info => ItemHelper.FindChild<GroupBox>(this, "Layer2Info");
        private UserInput? Layer2MasteringCode => ItemHelper.FindChild<UserInput>(this, "Layer2MasteringCode");
        private UserInput? Layer2MasteringSID => ItemHelper.FindChild<UserInput>(this, "Layer2MasteringSID");
        private UserInput? Layer2Toolstamp => ItemHelper.FindChild<UserInput>(this, "Layer2Toolstamp");
        private UserInput? Layer2MouldSIDs => ItemHelper.FindChild<UserInput>(this, "Layer2MouldSIDs");
        private UserInput? Layer2AdditionalMoulds => ItemHelper.FindChild<UserInput>(this, "Layer2AdditionalMoulds");

        private GroupBox? Layer3Info => ItemHelper.FindChild<GroupBox>(this, "Layer3Info");
        private UserInput? Layer3MasteringCode => ItemHelper.FindChild<UserInput>(this, "Layer3MasteringCode");
        private UserInput? Layer3MasteringSID => ItemHelper.FindChild<UserInput>(this, "Layer3MasteringSID");
        private UserInput? Layer3Toolstamp => ItemHelper.FindChild<UserInput>(this, "Layer3Toolstamp");
        private UserInput? Layer3MouldSIDs => ItemHelper.FindChild<UserInput>(this, "Layer3MouldSIDs");
        private UserInput? Layer3AdditionalMoulds => ItemHelper.FindChild<UserInput>(this, "Layer3AdditionalMoulds");

        private GroupBox? LabelSideInfo => ItemHelper.FindChild<GroupBox>(this, "LabelSideInfo");
        private UserInput? LabelSideMasteringCode => ItemHelper.FindChild<UserInput>(this, "LabelSideMasteringCode");
        private UserInput? LabelSideMasteringSID => ItemHelper.FindChild<UserInput>(this, "LabelSideMasteringSID");
        private UserInput? LabelSideToolstamp => ItemHelper.FindChild<UserInput>(this, "LabelSideToolstamp");
        private UserInput? LabelSideMouldSIDs => ItemHelper.FindChild<UserInput>(this, "LabelSideMouldSIDs");
        private UserInput? LabelSideAdditionalMoulds => ItemHelper.FindChild<UserInput>(this, "LabelSideAdditionalMoulds");

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
            Layer0MasteringCode!.Tab = true;
            Layer0MasteringSID!.Tab = true;
            Layer0Toolstamp!.Tab = true;
            Layer0MouldSIDs!.Tab = true;
            Layer0AdditionalMoulds!.Tab = true;

            Layer1MasteringCode!.Tab = true;
            Layer1MasteringSID!.Tab = true;
            Layer1Toolstamp!.Tab = true;
            Layer1MouldSIDs!.Tab = true;
            Layer1AdditionalMoulds!.Tab = true;

            Layer2MasteringCode!.Tab = true;
            Layer2MasteringSID!.Tab = true;
            Layer2Toolstamp!.Tab = true;
            Layer2MouldSIDs!.Tab = true;
            Layer2AdditionalMoulds!.Tab = true;

            Layer3MasteringCode!.Tab = true;
            Layer3MasteringSID!.Tab = true;
            Layer3Toolstamp!.Tab = true;
            Layer3MouldSIDs!.Tab = true;
            Layer3AdditionalMoulds!.Tab = true;

            LabelSideMasteringCode!.Tab = true;
            LabelSideMasteringSID!.Tab = true;
            LabelSideToolstamp!.Tab = true;
            LabelSideMouldSIDs!.Tab = true;
            LabelSideAdditionalMoulds!.Tab = true;
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
            if (string.IsNullOrEmpty(submissionInfo.DiscIdentifiers?.UniversalHash))
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
            // Reset the visibility state for all panels
            Layer0Info!.Visibility = Visibility.Collapsed;
            Layer1Info!.Visibility = Visibility.Collapsed;
            Layer2Info!.Visibility = Visibility.Collapsed;
            Layer3Info!.Visibility = Visibility.Collapsed;
            LabelSideInfo!.Visibility = Visibility.Collapsed;

#pragma warning disable IDE0010
            switch (submissionInfo?.DiscIdentity?.Media)
            {
                case MediaType.CD:
                case MediaType.GDROM:
                    Layer0Info!.Visibility = Visibility.Visible;
                    LabelSideInfo!.Visibility = Visibility.Visible;
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
                        Layer0Info!.Visibility = Visibility.Visible;
                        Layer1Info!.Visibility = Visibility.Visible;
                        Layer2Info!.Visibility = Visibility.Visible;
                        Layer3Info!.Visibility = Visibility.Visible;
                        LabelSideInfo!.Visibility = Visibility.Visible;
                    }

                    // Triple-layer discs
                    else if (submissionInfo?.DiscIdentifiers.Layerbreak2 != default(long))
                    {
                        Layer0Info!.Visibility = Visibility.Visible;
                        Layer1Info!.Visibility = Visibility.Visible;
                        Layer2Info!.Visibility = Visibility.Visible;
                        LabelSideInfo!.Visibility = Visibility.Visible;
                    }

                    // Double-layer discs
                    else if (submissionInfo?.DiscIdentifiers.Layerbreak != default(long))
                    {
                        Layer0Info!.Visibility = Visibility.Visible;
                        Layer1Info!.Visibility = Visibility.Visible;
                        LabelSideInfo!.Visibility = Visibility.Visible;
                    }

                    // Single-layer discs
                    else
                    {
                        Layer0Info!.Visibility = Visibility.Visible;
                        LabelSideInfo!.Visibility = Visibility.Visible;
                    }

                    break;

                case MediaType.UMDSL:
                case MediaType.UMDDL:
                    Layer0Info!.Visibility = Visibility.Visible;
                    Layer1Info!.Visibility = Visibility.Visible;
                    LabelSideInfo!.Visibility = Visibility.Visible;
                    break;

                // Defaults are set above
                default:
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
