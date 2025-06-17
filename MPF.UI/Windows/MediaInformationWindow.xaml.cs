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

        #region Common Info

        private Grid? LanguageSelectionGrid => ItemHelper.FindChild<Grid>(this, "LanguageSelectionGrid");

        #endregion

        #region Additional Info
        
        private UserInput? DiscIDTextBox => ItemHelper.FindChild<UserInput>(this, "DiscIDTextBox");
        private UserInput? DiscKeyTextBox => ItemHelper.FindChild<UserInput>(this, "DiscKeyTextBox");

        #endregion

        #region Ringcodes

        private GroupBox? L0Info => ItemHelper.FindChild<GroupBox>(this, "L0Info");
        private UserInput? L0MasteringRing => ItemHelper.FindChild<UserInput>(this, "L0MasteringRing");
        private UserInput? L0MasteringSID => ItemHelper.FindChild<UserInput>(this, "L0MasteringSID");
        private UserInput? L0Toolstamp => ItemHelper.FindChild<UserInput>(this, "L0Toolstamp");
        private UserInput? L0MouldSID => ItemHelper.FindChild<UserInput>(this, "L0MouldSID");
        private UserInput? L0AdditionalMould => ItemHelper.FindChild<UserInput>(this, "L0AdditionalMould");
        private GroupBox? L1Info => ItemHelper.FindChild<GroupBox>(this, "L1Info");
        private UserInput? L1MasteringRing => ItemHelper.FindChild<UserInput>(this, "L1MasteringRing");
        private UserInput? L1MasteringSID => ItemHelper.FindChild<UserInput>(this, "L1MasteringSID");
        private UserInput? L1Toolstamp => ItemHelper.FindChild<UserInput>(this, "L1Toolstamp");
        private UserInput? L1MouldSID => ItemHelper.FindChild<UserInput>(this, "L1MouldSID");
        private UserInput? L1AdditionalMould => ItemHelper.FindChild<UserInput>(this, "L1AdditionalMould");
        private GroupBox? L2Info => ItemHelper.FindChild<GroupBox>(this, "L2Info");
        private UserInput? L2MasteringRing => ItemHelper.FindChild<UserInput>(this, "L2MasteringRing");
        private UserInput? L2MasteringSID => ItemHelper.FindChild<UserInput>(this, "L2MasteringSID");
        private UserInput? L2Toolstamp => ItemHelper.FindChild<UserInput>(this, "L2Toolstamp");
        private GroupBox? L3Info => ItemHelper.FindChild<GroupBox>(this, "L3Info");
        private UserInput? L3MasteringRing => ItemHelper.FindChild<UserInput>(this, "L3MasteringRing");
        private UserInput? L3MasteringSID => ItemHelper.FindChild<UserInput>(this, "L3MasteringSID");
        private UserInput? L3Toolstamp => ItemHelper.FindChild<UserInput>(this, "L3Toolstamp");

        #endregion

        #region Read-Only Info

        private UserInput? FullyMatchedID => ItemHelper.FindChild<UserInput>(this, "FullyMatchedID");
        private UserInput? PartiallyMatchedIDs => ItemHelper.FindChild<UserInput>(this, "PartiallyMatchedIDs");
        private UserInput? HashData => ItemHelper.FindChild<UserInput>(this, "HashData");
        private UserInput? HashDataSize => ItemHelper.FindChild<UserInput>(this, "HashDataSize");
        private UserInput? HashDataCRC => ItemHelper.FindChild<UserInput>(this, "HashDataCRC");
        private UserInput? HashDataMD5 => ItemHelper.FindChild<UserInput>(this, "HashDataMD5");
        private UserInput? HashDataSHA1 => ItemHelper.FindChild<UserInput>(this, "HashDataSHA1");
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
        private UserInput? LibCrypt => ItemHelper.FindChild<UserInput>(this, "LibCrypt");
        private UserInput? LibCryptData => ItemHelper.FindChild<UserInput>(this, "LibCryptData");
        private UserInput? PFIHash => ItemHelper.FindChild<UserInput>(this, "PFIHash");
        private UserInput? PIC => ItemHelper.FindChild<UserInput>(this, "PIC");
        private UserInput? PVD => ItemHelper.FindChild<UserInput>(this, "PVD");
        private UserInput? RingNonZeroDataStart => ItemHelper.FindChild<UserInput>(this, "RingNonZeroDataStart");
        private UserInput? RingPerfectAudioOffset => ItemHelper.FindChild<UserInput>(this, "RingPerfectAudioOffset");
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
            if (options.EnableRedumpCompatibility)
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
            if (submissionInfo == null)
                return;

            if (submissionInfo.FullyMatchedID == null)
                FullyMatchedID!.Visibility = Visibility.Collapsed;
            if (submissionInfo.PartiallyMatchedIDs == null || submissionInfo.PartiallyMatchedIDs.Count == 0)
                PartiallyMatchedIDs!.Visibility = Visibility.Collapsed;
            else
                PartiallyMatchedIDs!.Text = string.Join(", ", [.. submissionInfo.PartiallyMatchedIDs.ConvertAll(i => i.ToString())]);
            if (string.IsNullOrEmpty(submissionInfo.TracksAndWriteOffsets?.ClrMameProData))
                HashData!.Visibility = Visibility.Collapsed;
            if (submissionInfo.SizeAndChecksums?.Size == null || submissionInfo.SizeAndChecksums.Size == 0)
                HashDataSize!.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(submissionInfo.SizeAndChecksums?.CRC32))
                HashDataCRC!.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(submissionInfo.SizeAndChecksums?.MD5))
                HashDataMD5!.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(submissionInfo.SizeAndChecksums?.SHA1))
                HashDataSHA1!.Visibility = Visibility.Collapsed;
            if (submissionInfo.SizeAndChecksums?.Layerbreak == null || submissionInfo.SizeAndChecksums.Layerbreak == 0)
                HashDataLayerbreak1!.Visibility = Visibility.Collapsed;
            if (submissionInfo.SizeAndChecksums?.Layerbreak2 == null || submissionInfo.SizeAndChecksums.Layerbreak2 == 0)
                HashDataLayerbreak2!.Visibility = Visibility.Collapsed;
            if (submissionInfo.SizeAndChecksums?.Layerbreak3 == null || submissionInfo.SizeAndChecksums.Layerbreak3 == 0)
                HashDataLayerbreak3!.Visibility = Visibility.Collapsed;
            if (submissionInfo.CopyProtection?.AntiModchip == null)
                AntiModchip!.Visibility = Visibility.Collapsed;
            if (submissionInfo.TracksAndWriteOffsets?.OtherWriteOffsets == null)
                DiscOffset!.Visibility = Visibility.Collapsed;
            if (ShouldCollapseComment(submissionInfo, SiteCode.DMIHash))
                DMIHash!.Visibility = Visibility.Collapsed;
            if (submissionInfo.EDC?.EDC == null)
                EDC!.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(submissionInfo.CommonDiscInfo?.ErrorsCount))
                ErrorsCount!.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(submissionInfo.CommonDiscInfo?.EXEDateBuildDate))
                EXEDateBuildDate!.Visibility = Visibility.Collapsed;
            if (ShouldCollapseComment(submissionInfo, SiteCode.Filename))
                Filename!.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(submissionInfo.Extras?.Header))
                Header!.Visibility = Visibility.Collapsed;
            if (ShouldCollapseComment(submissionInfo, SiteCode.InternalName))
                InternalName!.Visibility = Visibility.Collapsed;
            if (ShouldCollapseComment(submissionInfo, SiteCode.InternalSerialName))
                InternalSerialName!.Visibility = Visibility.Collapsed;
            if (ShouldCollapseComment(submissionInfo, SiteCode.Multisession))
                Multisession!.Visibility = Visibility.Collapsed;
            if (submissionInfo.CopyProtection?.LibCrypt == null)
                LibCrypt!.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(submissionInfo.CopyProtection?.LibCryptData))
                LibCryptData!.Visibility = Visibility.Collapsed;
            if (ShouldCollapseComment(submissionInfo, SiteCode.PFIHash))
                PFIHash!.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(submissionInfo.Extras?.PIC))
                PIC!.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(submissionInfo.Extras?.PVD))
                PVD!.Visibility = Visibility.Collapsed;
            if (ShouldCollapseComment(submissionInfo, SiteCode.RingNonZeroDataStart))
                RingNonZeroDataStart!.Visibility = Visibility.Collapsed;
            if (ShouldCollapseComment(submissionInfo, SiteCode.RingPerfectAudioOffset))
                RingPerfectAudioOffset!.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(submissionInfo.CopyProtection?.SecuROMData))
                SecuROMData!.Visibility = Visibility.Collapsed;
            if (ShouldCollapseComment(submissionInfo, SiteCode.SSHash))
                SSHash!.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(submissionInfo.Extras?.SecuritySectorRanges))
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
            // Sony-printed discs have layers in the opposite order
            var system = submissionInfo?.CommonDiscInfo?.System;
            bool reverseOrder = system.HasReversedRingcodes();

            switch (submissionInfo?.CommonDiscInfo?.Media)
            {
                case DiscType.CD:
                case DiscType.GDROM:
                    L0Info!.Header = "Data Side";
                    L0MasteringRing!.Label = "Mastering Ring";
                    L0MasteringSID!.Label = "Mastering SID";
                    L0Toolstamp!.Label = "Toolstamp/Mastering Code";
                    L0MouldSID!.Label = "Mould SID";
                    L0AdditionalMould!.Label = "Additional Mould";

                    L1Info!.Header = "Label Side";
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
                case DiscType.UMDSL:
                case DiscType.UMDDL:
                    // Quad-layer discs
                    if (submissionInfo?.SizeAndChecksums?.Layerbreak3 != default(long))
                    {
                        L2Info!.Visibility = Visibility.Visible;
                        L3Info!.Visibility = Visibility.Visible;

                        L0Info!.Header = reverseOrder ? "Layer 0 (Outer)" : "Layer 0 (Inner)";
                        L0MasteringRing!.Label = "Mastering Ring";
                        L0MasteringSID!.Label = "Mastering SID";
                        L0Toolstamp!.Label = "Toolstamp/Mastering Code";
                        L0MouldSID!.Label = "Data Side Mould SID";
                        L0AdditionalMould!.Label = "Data Side Additional Mould";

                        L1Info!.Header = "Layer 1";
                        L1MasteringRing!.Label = "Mastering Ring";
                        L1MasteringSID!.Label = "Mastering SID";
                        L1Toolstamp!.Label = "Toolstamp/Mastering Code";
                        L1MouldSID!.Label = "Label Side Mould SID";
                        L1AdditionalMould!.Label = "Label Side Additional Mould";

                        L2Info!.Header = "Layer 2";
                        L2MasteringRing!.Label = "Mastering Ring";
                        L2MasteringSID!.Label = "Mastering SID";
                        L2Toolstamp!.Label = "Toolstamp/Mastering Code";

                        L3Info!.Header = reverseOrder ? "Layer 3 (Inner)" : "Layer 3 (Outer)";
                        L3MasteringRing!.Label = "Mastering Ring";
                        L3MasteringSID!.Label = "Mastering SID";
                        L3Toolstamp!.Label = "Toolstamp/Mastering Code";
                    }

                    // Triple-layer discs
                    else if (submissionInfo?.SizeAndChecksums?.Layerbreak2 != default(long))
                    {
                        L2Info!.Visibility = Visibility.Visible;

                        L0Info!.Header = reverseOrder ? "Layer 0 (Outer)" : "Layer 0 (Inner)";
                        L0MasteringRing!.Label = "Mastering Ring";
                        L0MasteringSID!.Label = "Mastering SID";
                        L0Toolstamp!.Label = "Toolstamp/Mastering Code";
                        L0MouldSID!.Label = "Data Side Mould SID";
                        L0AdditionalMould!.Label = "Data Side Additional Mould";

                        L1Info!.Header = "Layer 1";
                        L1MasteringRing!.Label = "Mastering Ring";
                        L1MasteringSID!.Label = "Mastering SID";
                        L1Toolstamp!.Label = "Toolstamp/Mastering Code";
                        L1MouldSID!.Label = "Label Side Mould SID";
                        L1AdditionalMould!.Label = "Label Side Additional Mould";

                        L2Info!.Header = reverseOrder ? "Layer 2 (Inner)" : "Layer 2 (Outer)";
                        L2MasteringRing!.Label = "Mastering Ring";
                        L2MasteringSID!.Label = "Mastering SID";
                        L2Toolstamp!.Label = "Toolstamp/Mastering Code";
                    }

                    // Double-layer discs
                    else if (submissionInfo?.SizeAndChecksums?.Layerbreak != default(long))
                    {
                        L0Info!.Header = reverseOrder ? "Layer 0 (Outer)" : "Layer 0 (Inner)";
                        L0MasteringRing!.Label = "Mastering Ring";
                        L0MasteringSID!.Label = "Mastering SID";
                        L0Toolstamp!.Label = "Toolstamp/Mastering Code";
                        L0MouldSID!.Label = "Data Side Mould SID";
                        L0AdditionalMould!.Label = "Data Side Additional Mould";

                        L1Info!.Header = reverseOrder ? "Layer 1 (Inner)" : "Layer 1 (Outer)";
                        L1MasteringRing!.Label = "Mastering Ring";
                        L1MasteringSID!.Label = "Mastering SID";
                        L1Toolstamp!.Label = "Toolstamp/Mastering Code";
                        L1MouldSID!.Label = "Label Side Mould SID";
                        L1AdditionalMould!.Label = "Label Side Additional Mould";
                    }

                    // Single-layer discs
                    else
                    {
                        L0Info!.Header = "Data Side";
                        L0MasteringRing!.Label = "Mastering Ring";
                        L0MasteringSID!.Label = "Mastering SID";
                        L0Toolstamp!.Label = "Toolstamp/Mastering Code";
                        L0MouldSID!.Label = "Mould SID";
                        L0AdditionalMould!.Label = "Additional Mould";

                        L1Info!.Header = "Label Side";
                        L1MasteringRing!.Label = "Mastering Ring";
                        L1MasteringSID!.Label = "Mastering SID";
                        L1Toolstamp!.Label = "Toolstamp/Mastering Code";
                        L1MouldSID!.Label = "Mould SID";
                        L1AdditionalMould!.Label = "Additional Mould";
                    }

                    break;

                // All other media we assume to have no rings
                default:
                    L0Info!.Visibility = Visibility.Collapsed;
                    L1Info!.Visibility = Visibility.Collapsed;
                    L2Info!.Visibility = Visibility.Collapsed;
                    L3Info!.Visibility = Visibility.Collapsed;
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
                case RedumpSystem.NintendoWiiU:
                    DiscKeyTextBox!.Visibility = Visibility.Visible;
                    break;

                case RedumpSystem.SonyPlayStation2:
                    LanguageSelectionGrid!.Visibility = Visibility.Visible;
                    break;

                case RedumpSystem.SonyPlayStation3:
                    DiscKeyTextBox!.Visibility = Visibility.Visible;
                    DiscIDTextBox!.Visibility = Visibility.Visible;
                    break;
            }
        }

        /// <summary>
        /// Determine if a comment field should be collapsed in read-only view
        /// </summary>
        private static bool ShouldCollapseComment(SubmissionInfo? submissionInfo, SiteCode siteCode)
        {
            // If the special fields don't exist
            if (submissionInfo?.CommonDiscInfo?.CommentsSpecialFields == null)
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
