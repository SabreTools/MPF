using System.Linq;
using System.Windows;
using MPF.Core.Data;
using MPF.Core.UI.ViewModels;
using MPF.Core.Utilities;
using SabreTools.RedumpLib.Data;

namespace MPF.UI.Core.Windows
{
    /// <summary>
    /// Interaction logic for DiscInformationWindow.xaml
    /// </summary>
    public partial class DiscInformationWindow : WindowBase
    {
        /// <summary>
        /// Read-only access to the current disc information view model
        /// </summary>
        public DiscInformationViewModel DiscInformationViewModel => DataContext as DiscInformationViewModel ?? new DiscInformationViewModel(new Options(), new SubmissionInfo());

        /// <summary>
        /// Constructor
        /// </summary>
        public DiscInformationWindow(Options options, SubmissionInfo? submissionInfo)
        {
#if NET452_OR_GREATER || NETCOREAPP
            var chrome = new System.Windows.Shell.WindowChrome
            {
                CaptionHeight = 0,
                ResizeBorderThickness = new Thickness(0),
            };
            System.Windows.Shell.WindowChrome.SetWindowChrome(this, chrome);
#endif

            InitializeComponent();
            DataContext = new DiscInformationViewModel(options, submissionInfo);
            DiscInformationViewModel.Load();

            // Limit lists, if necessary
            if (options.EnableRedumpCompatibility)
            {
                DiscInformationViewModel.SetRedumpRegions();
                DiscInformationViewModel.SetRedumpLanguages();
            }

            // Add handlers
            AcceptButton.Click += OnAcceptClick;
            CancelButton.Click += OnCancelClick;
            RingCodeGuideButton.Click += OnRingCodeGuideClick;

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
            if (options.EnableTabsInInputFields)
                EnableTabsInInputFields();

            // Hide read-only fields that don't have values set
            HideReadOnlyFields(submissionInfo);

            // Different media types mean different fields available
            UpdateFromDiscType(submissionInfo);

            // Different systems mean different fields available
            UpdateFromSystemType(submissionInfo);
        }

        /// <summary>
        /// Enable tab entry on supported fields
        /// </summary>
        /// TODO: See if these can be done by binding
        private void EnableTabsInInputFields()
        {
            // Additional Information
            CommentsTextBox.Tab = true;

            // Contents
            GeneralContent.Tab = true;
            GamesTextBox.Tab = true;
            NetYarozeGamesTextBox.Tab = true;
            PlayableDemosTextBox.Tab = true;
            RollingDemosTextBox.Tab = true;
            TechDemosTextBox.Tab = true;
            GameFootageTextBox.Tab = true;
            VideosTextBox.Tab = true;
            PatchesTextBox.Tab = true;
            SavegamesTextBox.Tab = true;
            ExtrasTextBox.Tab = true;

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
            if (submissionInfo == null)
                return;

            if (submissionInfo.FullyMatchedID == null)
                FullyMatchedID.Visibility = Visibility.Collapsed;
            if (submissionInfo.PartiallyMatchedIDs == null)
                PartiallyMatchedIDs.Visibility = Visibility.Collapsed;
            else
                PartiallyMatchedIDs.Text = string.Join(", ", submissionInfo.PartiallyMatchedIDs);
            if (submissionInfo.CopyProtection?.AntiModchip == null)
                AntiModchip.Visibility = Visibility.Collapsed;
            if (submissionInfo.TracksAndWriteOffsets?.OtherWriteOffsets == null)
                DiscOffset.Visibility = Visibility.Collapsed;
            if (submissionInfo.CommonDiscInfo?.CommentsSpecialFields?.Keys?.Contains(SiteCode.DMIHash) != true)
                DMIHash.Visibility = Visibility.Collapsed;
            if (submissionInfo.EDC?.EDC == null)
                EDC.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(submissionInfo.CommonDiscInfo?.ErrorsCount))
                ErrorsCount.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(submissionInfo.CommonDiscInfo?.EXEDateBuildDate))
                EXEDateBuildDate.Visibility = Visibility.Collapsed;
            if (submissionInfo.CommonDiscInfo?.CommentsSpecialFields?.ContainsKey(SiteCode.Filename) != true)
                Filename.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(submissionInfo.Extras?.Header))
                Header.Visibility = Visibility.Collapsed;
            if (submissionInfo.CommonDiscInfo?.CommentsSpecialFields?.ContainsKey(SiteCode.InternalName) != true)
                InternalName.Visibility = Visibility.Collapsed;
            if (submissionInfo.CommonDiscInfo?.CommentsSpecialFields?.ContainsKey(SiteCode.InternalSerialName) != true)
                InternalSerialName.Visibility = Visibility.Collapsed;
            if (submissionInfo.CommonDiscInfo?.CommentsSpecialFields?.ContainsKey(SiteCode.Multisession) != true)
                Multisession.Visibility = Visibility.Collapsed;
            if (submissionInfo.CopyProtection?.LibCrypt == null)
                LibCrypt.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(submissionInfo.CopyProtection?.LibCryptData))
                LibCryptData.Visibility = Visibility.Collapsed;
            if (submissionInfo.CommonDiscInfo?.CommentsSpecialFields?.ContainsKey(SiteCode.PFIHash) != true)
                PFIHash.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(submissionInfo.Extras?.PIC))
                PIC.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(submissionInfo.Extras?.PVD))
                PVD.Visibility = Visibility.Collapsed;
            if (submissionInfo.CommonDiscInfo?.CommentsSpecialFields?.ContainsKey(SiteCode.RingNonZeroDataStart) != true)
                RingNonZeroDataStart.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(submissionInfo.CopyProtection?.SecuROMData))
                SecuROMData.Visibility = Visibility.Collapsed;
            if (submissionInfo.CommonDiscInfo?.CommentsSpecialFields?.ContainsKey(SiteCode.SSHash) != true)
                SSHash.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(submissionInfo.Extras?.SecuritySectorRanges))
                SecuritySectorRanges.Visibility = Visibility.Collapsed;
            if (submissionInfo.CommonDiscInfo?.CommentsSpecialFields?.ContainsKey(SiteCode.SSVersion) != true)
                SSVersion.Visibility = Visibility.Collapsed;
            if (submissionInfo.CommonDiscInfo?.CommentsSpecialFields?.ContainsKey(SiteCode.UniversalHash) != true)
                UniversalHash.Visibility = Visibility.Collapsed;
            if (submissionInfo.CommonDiscInfo?.CommentsSpecialFields?.ContainsKey(SiteCode.VolumeLabel) != true)
                VolumeLabel.Visibility = Visibility.Collapsed;
            if (submissionInfo.CommonDiscInfo?.CommentsSpecialFields?.ContainsKey(SiteCode.XeMID) != true)
                XeMID.Visibility = Visibility.Collapsed;
            if (submissionInfo.CommonDiscInfo?.CommentsSpecialFields?.ContainsKey(SiteCode.XMID) != true)
                XMID.Visibility = Visibility.Collapsed;
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
                    L1MasteringRing.Visibility = Visibility.Collapsed;
                    L1MasteringSID.Visibility = Visibility.Collapsed;
                    L1Toolstamp.Visibility = Visibility.Collapsed;
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
                    if (submissionInfo?.SizeAndChecksums?.Layerbreak3 != default(long))
                    {
                        L2Info.Visibility = Visibility.Visible;
                        L3Info.Visibility = Visibility.Visible;

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
                    else if (submissionInfo?.SizeAndChecksums?.Layerbreak2 != default(long))
                    {
                        L2Info.Visibility = Visibility.Visible;

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
                    else if (submissionInfo?.SizeAndChecksums?.Layerbreak != default(long))
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

                // All other media we assume to have no rings
                default:
                    L0Info.Visibility = Visibility.Collapsed;
                    L1Info.Visibility = Visibility.Collapsed;
                    L2Info.Visibility = Visibility.Collapsed;
                    L3Info.Visibility = Visibility.Collapsed;
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
                case RedumpSystem.SonyPlayStation2:
                    LanguageSelectionGrid.Visibility = Visibility.Visible;
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
            DiscInformationViewModel.Save();
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
