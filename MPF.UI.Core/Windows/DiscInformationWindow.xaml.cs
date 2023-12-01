using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MPF.Core.Data;
using MPF.Core.UI.ViewModels;
using MPF.Core.Utilities;
using MPF.UI.Core.UserControls;
using SabreTools.RedumpLib.Data;

namespace MPF.UI.Core.Windows
{
    /// <summary>
    /// Interaction logic for DiscInformationWindow.xaml
    /// </summary>
    public partial class DiscInformationWindow : WindowBase
    {
#if NET35

        #region Common Info

        private Grid? _LanguageSelectionGrid => ItemHelper.FindChild<Grid>(this, "LanguageSelectionGrid");

        #endregion

        #region Additional Info

        private UserInput? _CommentsTextBox => ItemHelper.FindChild<UserInput>(this, "CommentsTextBox");
        private UserInput? _DiscIDTextBox => ItemHelper.FindChild<UserInput>(this, "DiscIDTextBox");
        private UserInput? _DiscKeyTextBox => ItemHelper.FindChild<UserInput>(this, "DiscKeyTextBox");

        #endregion

        #region Contents

        private UserInput? _ExtrasTextBox => ItemHelper.FindChild<UserInput>(this, "ExtrasTextBox");
        private UserInput? _GameFootageTextBox => ItemHelper.FindChild<UserInput>(this, "GameFootageTextBox");
        private UserInput? _GamesTextBox => ItemHelper.FindChild<UserInput>(this, "GamesTextBox");
        private UserInput? _GeneralContent => ItemHelper.FindChild<UserInput>(this, "GeneralContent");
        private UserInput? _NetYarozeGamesTextBox => ItemHelper.FindChild<UserInput>(this, "NetYarozeGamesTextBox");
        private UserInput? _PatchesTextBox => ItemHelper.FindChild<UserInput>(this, "PatchesTextBox");
        private UserInput? _PlayableDemosTextBox => ItemHelper.FindChild<UserInput>(this, "PlayableDemosTextBox");
        private UserInput? _RollingDemosTextBox => ItemHelper.FindChild<UserInput>(this, "RollingDemosTextBox");
        private UserInput? _SavegamesTextBox => ItemHelper.FindChild<UserInput>(this, "SavegamesTextBox");
        private UserInput? _TechDemosTextBox => ItemHelper.FindChild<UserInput>(this, "TechDemosTextBox");
        private UserInput? _VideosTextBox => ItemHelper.FindChild<UserInput>(this, "VideosTextBox");

        #endregion

        #region Ringcodes

        private GroupBox? _L0Info => ItemHelper.FindChild<GroupBox>(this, "L0Info");
        private UserInput? _L0MasteringRing => ItemHelper.FindChild<UserInput>(this, "L0MasteringRing");
        private UserInput? _L0MasteringSID => ItemHelper.FindChild<UserInput>(this, "L0MasteringSID");
        private UserInput? _L0Toolstamp => ItemHelper.FindChild<UserInput>(this, "L0Toolstamp");
        private UserInput? _L0MouldSID => ItemHelper.FindChild<UserInput>(this, "L0MouldSID");
        private UserInput? _L0AdditionalMould => ItemHelper.FindChild<UserInput>(this, "L0AdditionalMould");
        private GroupBox? _L1Info => ItemHelper.FindChild<GroupBox>(this, "L1Info");
        private UserInput? _L1MasteringRing => ItemHelper.FindChild<UserInput>(this, "L1MasteringRing");
        private UserInput? _L1MasteringSID => ItemHelper.FindChild<UserInput>(this, "L1MasteringSID");
        private UserInput? _L1Toolstamp => ItemHelper.FindChild<UserInput>(this, "L1Toolstamp");
        private UserInput? _L1MouldSID => ItemHelper.FindChild<UserInput>(this, "L1MouldSID");
        private UserInput? _L1AdditionalMould => ItemHelper.FindChild<UserInput>(this, "L1AdditionalMould");
        private GroupBox? _L2Info => ItemHelper.FindChild<GroupBox>(this, "L2Info");
        private UserInput? _L2MasteringRing => ItemHelper.FindChild<UserInput>(this, "L2MasteringRing");
        private UserInput? _L2MasteringSID => ItemHelper.FindChild<UserInput>(this, "L2MasteringSID");
        private UserInput? _L2Toolstamp => ItemHelper.FindChild<UserInput>(this, "L2Toolstamp");
        private GroupBox? _L3Info => ItemHelper.FindChild<GroupBox>(this, "L3Info");
        private UserInput? _L3MasteringRing => ItemHelper.FindChild<UserInput>(this, "L3MasteringRing");
        private UserInput? _L3MasteringSID => ItemHelper.FindChild<UserInput>(this, "L3MasteringSID");
        private UserInput? _L3Toolstamp => ItemHelper.FindChild<UserInput>(this, "L3Toolstamp");

        #endregion

        #region Read-Only Info

        private UserInput? _FullyMatchedID => ItemHelper.FindChild<UserInput>(this, "FullyMatchedID");
        private UserInput? _PartiallyMatchedIDs => ItemHelper.FindChild<UserInput>(this, "PartiallyMatchedIDs");
        private UserInput? _AntiModchip => ItemHelper.FindChild<UserInput>(this, "AntiModchip");
        private UserInput? _DiscOffset => ItemHelper.FindChild<UserInput>(this, "DiscOffset");
        private UserInput? _DMIHash => ItemHelper.FindChild<UserInput>(this, "DMIHash");
        private UserInput? _EDC => ItemHelper.FindChild<UserInput>(this, "EDC");
        private UserInput? _ErrorsCount => ItemHelper.FindChild<UserInput>(this, "ErrorsCount");
        private UserInput? _EXEDateBuildDate => ItemHelper.FindChild<UserInput>(this, "EXEDateBuildDate");
        private UserInput? _Filename => ItemHelper.FindChild<UserInput>(this, "Filename");
        private UserInput? _Header => ItemHelper.FindChild<UserInput>(this, "Header");
        private UserInput? _InternalName => ItemHelper.FindChild<UserInput>(this, "InternalName");
        private UserInput? _InternalSerialName => ItemHelper.FindChild<UserInput>(this, "InternalSerialName");
        private UserInput? _Multisession => ItemHelper.FindChild<UserInput>(this, "Multisession");
        private UserInput? _LibCrypt => ItemHelper.FindChild<UserInput>(this, "LibCrypt");
        private UserInput? _LibCryptData => ItemHelper.FindChild<UserInput>(this, "LibCryptData");
        private UserInput? _PFIHash => ItemHelper.FindChild<UserInput>(this, "PFIHash");
        private UserInput? _PIC => ItemHelper.FindChild<UserInput>(this, "PIC");
        private UserInput? _PVD => ItemHelper.FindChild<UserInput>(this, "PVD");
        private UserInput? _RingNonZeroDataStart => ItemHelper.FindChild<UserInput>(this, "RingNonZeroDataStart");
        private UserInput? _SecuROMData => ItemHelper.FindChild<UserInput>(this, "SecuROMData");
        private UserInput? _SSHash => ItemHelper.FindChild<UserInput>(this, "SSHash");
        private UserInput? _SecuritySectorRanges => ItemHelper.FindChild<UserInput>(this, "SecuritySectorRanges");
        private UserInput? _SSVersion => ItemHelper.FindChild<UserInput>(this, "SSVersion");
        private UserInput? _UniversalHash => ItemHelper.FindChild<UserInput>(this, "UniversalHash");
        private UserInput? _VolumeLabel => ItemHelper.FindChild<UserInput>(this, "VolumeLabel");
        private UserInput? _XeMID => ItemHelper.FindChild<UserInput>(this, "XeMID");
        private UserInput? _XMID => ItemHelper.FindChild<UserInput>(this, "XMID");

        #endregion

        #region Accept / Cancel

        private Button? _AcceptButton => ItemHelper.FindChild<Button>(this, "AcceptButton");
        private Button? _CancelButton => ItemHelper.FindChild<Button>(this, "CancelButton");
        private Button? _RingCodeGuideButton => ItemHelper.FindChild<Button>(this, "RingCodeGuideButton");

        #endregion

#endif

        /// <summary>
        /// Read-only access to the current disc information view model
        /// </summary>
        public DiscInformationViewModel DiscInformationViewModel => DataContext as DiscInformationViewModel ?? new DiscInformationViewModel(new Options(), new SubmissionInfo());

        /// <summary>
        /// Constructor
        /// </summary>
        public DiscInformationWindow(Options options, SubmissionInfo? submissionInfo)
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

            DataContext = new DiscInformationViewModel(options, submissionInfo);
            DiscInformationViewModel.Load();

            // Limit lists, if necessary
            if (options.EnableRedumpCompatibility)
            {
                DiscInformationViewModel.SetRedumpRegions();
                DiscInformationViewModel.SetRedumpLanguages();
            }

            // Add handlers
#if NET35
            _AcceptButton!.Click += OnAcceptClick;
            _CancelButton!.Click += OnCancelClick;
            _RingCodeGuideButton!.Click += OnRingCodeGuideClick;
#else
            AcceptButton.Click += OnAcceptClick;
            CancelButton.Click += OnCancelClick;
            RingCodeGuideButton.Click += OnRingCodeGuideClick;
#endif

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
#if NET35
            _CommentsTextBox!.Tab = true;
#else
            CommentsTextBox.Tab = true;
#endif

            // Contents
#if NET35
            _GeneralContent!.Tab = true;
            _GamesTextBox!.Tab = true;
            _NetYarozeGamesTextBox!.Tab = true;
            _PlayableDemosTextBox!.Tab = true;
            _RollingDemosTextBox!.Tab = true;
            _TechDemosTextBox!.Tab = true;
            _GameFootageTextBox!.Tab = true;
            _VideosTextBox!.Tab = true;
            _PatchesTextBox!.Tab = true;
            _SavegamesTextBox!.Tab = true;
            _ExtrasTextBox!.Tab = true;
#else
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
#endif

            // L0
#if NET35
            _L0MasteringRing!.Tab = true;
            _L0MasteringSID!.Tab = true;
            _L0Toolstamp!.Tab = true;
            _L0MouldSID!.Tab = true;
            _L0AdditionalMould!.Tab = true;
#else
            L0MasteringRing.Tab = true;
            L0MasteringSID.Tab = true;
            L0Toolstamp.Tab = true;
            L0MouldSID.Tab = true;
            L0AdditionalMould.Tab = true;
#endif

            // L1
#if NET35
            _L1MasteringRing!.Tab = true;
            _L1MasteringSID!.Tab = true;
            _L1Toolstamp!.Tab = true;
            _L1MouldSID!.Tab = true;
            _L1AdditionalMould!.Tab = true;
#else
            L1MasteringRing.Tab = true;
            L1MasteringSID.Tab = true;
            L1Toolstamp.Tab = true;
            L1MouldSID.Tab = true;
            L1AdditionalMould.Tab = true;
#endif

            // L2
#if NET35
            _L2MasteringRing!.Tab = true;
            _L2MasteringSID!.Tab = true;
            _L2Toolstamp!.Tab = true;
#else
            L2MasteringRing.Tab = true;
            L2MasteringSID.Tab = true;
            L2Toolstamp.Tab = true;
#endif

            // L3
#if NET35
            _L3MasteringRing!.Tab = true;
            _L3MasteringSID!.Tab = true;
            _L3Toolstamp!.Tab = true;
#else
            L3MasteringRing.Tab = true;
            L3MasteringSID.Tab = true;
            L3Toolstamp.Tab = true;
#endif
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

#if NET35
            if (submissionInfo.FullyMatchedID == null)
                _FullyMatchedID!.Visibility = Visibility.Collapsed;
            if (submissionInfo.PartiallyMatchedIDs == null)
                _PartiallyMatchedIDs!.Visibility = Visibility.Collapsed;
            else
                _PartiallyMatchedIDs!.Text = string.Join(", ", submissionInfo.PartiallyMatchedIDs.Select(i => i.ToString()).ToArray());
            if (submissionInfo.CopyProtection?.AntiModchip == null)
                _AntiModchip!.Visibility = Visibility.Collapsed;
            if (submissionInfo.TracksAndWriteOffsets?.OtherWriteOffsets == null)
                _DiscOffset!.Visibility = Visibility.Collapsed;
            if (submissionInfo.CommonDiscInfo?.CommentsSpecialFields?.Keys?.Contains(SiteCode.DMIHash) != true)
                _DMIHash!.Visibility = Visibility.Collapsed;
            if (submissionInfo.EDC?.EDC == null)
                _EDC!.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(submissionInfo.CommonDiscInfo?.ErrorsCount))
                _ErrorsCount!.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(submissionInfo.CommonDiscInfo?.EXEDateBuildDate))
                _EXEDateBuildDate!.Visibility = Visibility.Collapsed;
            if (submissionInfo.CommonDiscInfo?.CommentsSpecialFields?.ContainsKey(SiteCode.Filename) != true)
                _Filename!.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(submissionInfo.Extras?.Header))
                _Header!.Visibility = Visibility.Collapsed;
            if (submissionInfo.CommonDiscInfo?.CommentsSpecialFields?.ContainsKey(SiteCode.InternalName) != true)
                _InternalName!.Visibility = Visibility.Collapsed;
            if (submissionInfo.CommonDiscInfo?.CommentsSpecialFields?.ContainsKey(SiteCode.InternalSerialName) != true)
                _InternalSerialName!.Visibility = Visibility.Collapsed;
            if (submissionInfo.CommonDiscInfo?.CommentsSpecialFields?.ContainsKey(SiteCode.Multisession) != true)
                _Multisession!.Visibility = Visibility.Collapsed;
            if (submissionInfo.CopyProtection?.LibCrypt == null)
                _LibCrypt!.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(submissionInfo.CopyProtection?.LibCryptData))
                _LibCryptData!.Visibility = Visibility.Collapsed;
            if (submissionInfo.CommonDiscInfo?.CommentsSpecialFields?.ContainsKey(SiteCode.PFIHash) != true)
                _PFIHash!.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(submissionInfo.Extras?.PIC))
                _PIC!.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(submissionInfo.Extras?.PVD))
                _PVD!.Visibility = Visibility.Collapsed;
            if (submissionInfo.CommonDiscInfo?.CommentsSpecialFields?.ContainsKey(SiteCode.RingNonZeroDataStart) != true)
                _RingNonZeroDataStart!.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(submissionInfo.CopyProtection?.SecuROMData))
                _SecuROMData!.Visibility = Visibility.Collapsed;
            if (submissionInfo.CommonDiscInfo?.CommentsSpecialFields?.ContainsKey(SiteCode.SSHash) != true)
                _SSHash!.Visibility = Visibility.Collapsed;
            if (string.IsNullOrEmpty(submissionInfo.Extras?.SecuritySectorRanges))
                _SecuritySectorRanges!.Visibility = Visibility.Collapsed;
            if (submissionInfo.CommonDiscInfo?.CommentsSpecialFields?.ContainsKey(SiteCode.SSVersion) != true)
                _SSVersion!.Visibility = Visibility.Collapsed;
            if (submissionInfo.CommonDiscInfo?.CommentsSpecialFields?.ContainsKey(SiteCode.UniversalHash) != true)
                _UniversalHash!.Visibility = Visibility.Collapsed;
            if (submissionInfo.CommonDiscInfo?.CommentsSpecialFields?.ContainsKey(SiteCode.VolumeLabel) != true)
                _VolumeLabel!.Visibility = Visibility.Collapsed;
            if (submissionInfo.CommonDiscInfo?.CommentsSpecialFields?.ContainsKey(SiteCode.XeMID) != true)
                _XeMID!.Visibility = Visibility.Collapsed;
            if (submissionInfo.CommonDiscInfo?.CommentsSpecialFields?.ContainsKey(SiteCode.XMID) != true)
                _XMID!.Visibility = Visibility.Collapsed;
#else
            if (submissionInfo.FullyMatchedID == null)
                FullyMatchedID.Visibility = Visibility.Collapsed;
            if (submissionInfo.PartiallyMatchedIDs == null)
                PartiallyMatchedIDs.Visibility = Visibility.Collapsed;
            else
                PartiallyMatchedIDs.Text = string.Join(", ", submissionInfo.PartiallyMatchedIDs.Select(i => i.ToString()).ToArray());
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
#endif
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
#if NET35
                    _L0Info!.Header = "Data Side";
                    _L0MasteringRing!.Label = "Mastering Ring";
                    _L0MasteringSID!.Label = "Mastering SID";
                    _L0Toolstamp!.Label = "Toolstamp/Mastering Code";
                    _L0MouldSID!.Label = "Mould SID";
                    _L0AdditionalMould!.Label = "Additional Mould";
#else
                    L0Info.Header = "Data Side";
                    L0MasteringRing.Label = "Mastering Ring";
                    L0MasteringSID.Label = "Mastering SID";
                    L0Toolstamp.Label = "Toolstamp/Mastering Code";
                    L0MouldSID.Label = "Mould SID";
                    L0AdditionalMould.Label = "Additional Mould";
#endif

#if NET35
                    _L1Info!.Header = "Label Side";
                    _L1MasteringRing!.Visibility = Visibility.Collapsed;
                    _L1MasteringSID!.Visibility = Visibility.Collapsed;
                    _L1Toolstamp!.Visibility = Visibility.Collapsed;
                    _L1MouldSID!.Label = "Mould SID";
                    _L1AdditionalMould!.Label = "Additional Mould";
#else
                    L1Info.Header = "Label Side";
                    L1MasteringRing.Visibility = Visibility.Collapsed;
                    L1MasteringSID.Visibility = Visibility.Collapsed;
                    L1Toolstamp.Visibility = Visibility.Collapsed;
                    L1MouldSID.Label = "Mould SID";
                    L1AdditionalMould.Label = "Additional Mould";
#endif
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
#if NET35
                        _L2Info!.Visibility = Visibility.Visible;
                        _L3Info!.Visibility = Visibility.Visible;
#else
                        L2Info.Visibility = Visibility.Visible;
                        L3Info.Visibility = Visibility.Visible;
#endif

#if NET35
                        _L0Info!.Header = reverseOrder ? "Layer 0 (Outer)" : "Layer 0 (Inner)";
                        _L0MasteringRing!.Label = "Mastering Ring";
                        _L0MasteringSID!.Label = "Mastering SID";
                        _L0Toolstamp!.Label = "Toolstamp/Mastering Code";
                        _L0MouldSID!.Label = "Data Side Mould SID";
                        _L0AdditionalMould!.Label = "Data Side Additional Mould";
#else
                        L0Info.Header = reverseOrder ? "Layer 0 (Outer)" : "Layer 0 (Inner)";
                        L0MasteringRing.Label = "Mastering Ring";
                        L0MasteringSID.Label = "Mastering SID";
                        L0Toolstamp.Label = "Toolstamp/Mastering Code";
                        L0MouldSID.Label = "Data Side Mould SID";
                        L0AdditionalMould.Label = "Data Side Additional Mould";
#endif

#if NET35
                        _L1Info!.Header = "Layer 1";
                        _L1MasteringRing!.Label = "Mastering Ring";
                        _L1MasteringSID!.Label = "Mastering SID";
                        _L1Toolstamp!.Label = "Toolstamp/Mastering Code";
                        _L1MouldSID!.Label = "Label Side Mould SID";
                        _L1AdditionalMould!.Label = "Label Side Additional Mould";
#else
                        L1Info.Header = "Layer 1";
                        L1MasteringRing.Label = "Mastering Ring";
                        L1MasteringSID.Label = "Mastering SID";
                        L1Toolstamp.Label = "Toolstamp/Mastering Code";
                        L1MouldSID.Label = "Label Side Mould SID";
                        L1AdditionalMould.Label = "Label Side Additional Mould";
#endif

#if NET35
                        _L2Info!.Header = "Layer 2";
                        _L2MasteringRing!.Label = "Mastering Ring";
                        _L2MasteringSID!.Label = "Mastering SID";
                        _L2Toolstamp!.Label = "Toolstamp/Mastering Code";
#else
                        L2Info.Header = "Layer 2";
                        L2MasteringRing.Label = "Mastering Ring";
                        L2MasteringSID.Label = "Mastering SID";
                        L2Toolstamp.Label = "Toolstamp/Mastering Code";
#endif

#if NET35
                        _L3Info!.Header = reverseOrder ? "Layer 3 (Inner)" : "Layer 3 (Outer)";
                        _L3MasteringRing!.Label = "Mastering Ring";
                        _L3MasteringSID!.Label = "Mastering SID";
                        _L3Toolstamp!.Label = "Toolstamp/Mastering Code";
#else
                        L3Info.Header = reverseOrder ? "Layer 3 (Inner)" : "Layer 3 (Outer)";
                        L3MasteringRing.Label = "Mastering Ring";
                        L3MasteringSID.Label = "Mastering SID";
                        L3Toolstamp.Label = "Toolstamp/Mastering Code";
#endif
                    }

                    // Triple-layer discs
                    else if (submissionInfo?.SizeAndChecksums?.Layerbreak2 != default(long))
                    {
#if NET35
                        _L2Info!.Visibility = Visibility.Visible;
#else
                        L2Info.Visibility = Visibility.Visible;
#endif

#if NET35
                        _L0Info!.Header = reverseOrder ? "Layer 0 (Outer)" : "Layer 0 (Inner)";
                        _L0MasteringRing!.Label = "Mastering Ring";
                        _L0MasteringSID!.Label = "Mastering SID";
                        _L0Toolstamp!.Label = "Toolstamp/Mastering Code";
                        _L0MouldSID!.Label = "Data Side Mould SID";
                        _L0AdditionalMould!.Label = "Data Side Additional Mould";
#else
                        L0Info.Header = reverseOrder ? "Layer 0 (Outer)" : "Layer 0 (Inner)";
                        L0MasteringRing.Label = "Mastering Ring";
                        L0MasteringSID.Label = "Mastering SID";
                        L0Toolstamp.Label = "Toolstamp/Mastering Code";
                        L0MouldSID.Label = "Data Side Mould SID";
                        L0AdditionalMould.Label = "Data Side Additional Mould";
#endif

#if NET35
                        _L1Info!.Header = "Layer 1";
                        _L1MasteringRing!.Label = "Mastering Ring";
                        _L1MasteringSID!.Label = "Mastering SID";
                        _L1Toolstamp!.Label = "Toolstamp/Mastering Code";
                        _L1MouldSID!.Label = "Label Side Mould SID";
                        _L1AdditionalMould!.Label = "Label Side Additional Mould";
#else
                        L1Info.Header = "Layer 1";
                        L1MasteringRing.Label = "Mastering Ring";
                        L1MasteringSID.Label = "Mastering SID";
                        L1Toolstamp.Label = "Toolstamp/Mastering Code";
                        L1MouldSID.Label = "Label Side Mould SID";
                        L1AdditionalMould.Label = "Label Side Additional Mould";
#endif

#if NET35
                        _L2Info!.Header = reverseOrder ? "Layer 2 (Inner)" : "Layer 2 (Outer)";
                        _L2MasteringRing!.Label = "Mastering Ring";
                        _L2MasteringSID!.Label = "Mastering SID";
                        _L2Toolstamp!.Label = "Toolstamp/Mastering Code";
#else
                        L2Info.Header = reverseOrder ? "Layer 2 (Inner)" : "Layer 2 (Outer)";
                        L2MasteringRing.Label = "Mastering Ring";
                        L2MasteringSID.Label = "Mastering SID";
                        L2Toolstamp.Label = "Toolstamp/Mastering Code";
#endif
                    }

                    // Double-layer discs
                    else if (submissionInfo?.SizeAndChecksums?.Layerbreak != default(long))
                    {
#if NET35
                        _L0Info!.Header = reverseOrder ? "Layer 0 (Outer)" : "Layer 0 (Inner)";
                        _L0MasteringRing!.Label = "Mastering Ring";
                        _L0MasteringSID!.Label = "Mastering SID";
                        _L0Toolstamp!.Label = "Toolstamp/Mastering Code";
                        _L0MouldSID!.Label = "Data Side Mould SID";
                        _L0AdditionalMould!.Label = "Data Side Additional Mould";
#else
                        L0Info.Header = reverseOrder ? "Layer 0 (Outer)" : "Layer 0 (Inner)";
                        L0MasteringRing.Label = "Mastering Ring";
                        L0MasteringSID.Label = "Mastering SID";
                        L0Toolstamp.Label = "Toolstamp/Mastering Code";
                        L0MouldSID.Label = "Data Side Mould SID";
                        L0AdditionalMould.Label = "Data Side Additional Mould";
#endif

#if NET35
                        _L1Info!.Header = reverseOrder ? "Layer 1 (Inner)" : "Layer 1 (Outer)";
                        _L1MasteringRing!.Label = "Mastering Ring";
                        _L1MasteringSID!.Label = "Mastering SID";
                        _L1Toolstamp!.Label = "Toolstamp/Mastering Code";
                        _L1MouldSID!.Label = "Label Side Mould SID";
                        _L1AdditionalMould!.Label = "Label Side Additional Mould";
#else
                        L1Info.Header = reverseOrder ? "Layer 1 (Inner)" : "Layer 1 (Outer)";
                        L1MasteringRing.Label = "Mastering Ring";
                        L1MasteringSID.Label = "Mastering SID";
                        L1Toolstamp.Label = "Toolstamp/Mastering Code";
                        L1MouldSID.Label = "Label Side Mould SID";
                        L1AdditionalMould.Label = "Label Side Additional Mould";
#endif
                    }

                    // Single-layer discs
                    else
                    {
#if NET35
                        _L0Info!.Header = "Data Side";
                        _L0MasteringRing!.Label = "Mastering Ring";
                        _L0MasteringSID!.Label = "Mastering SID";
                        _L0Toolstamp!.Label = "Toolstamp/Mastering Code";
                        _L0MouldSID!.Label = "Mould SID";
                        _L0AdditionalMould!.Label = "Additional Mould";
#else
                        L0Info.Header = "Data Side";
                        L0MasteringRing.Label = "Mastering Ring";
                        L0MasteringSID.Label = "Mastering SID";
                        L0Toolstamp.Label = "Toolstamp/Mastering Code";
                        L0MouldSID.Label = "Mould SID";
                        L0AdditionalMould.Label = "Additional Mould";
#endif

#if NET35
                        _L1Info!.Header = "Label Side";
                        _L1MasteringRing!.Visibility = Visibility.Collapsed;
                        _L1MasteringSID!.Visibility = Visibility.Collapsed;
                        _L1Toolstamp!.Visibility = Visibility.Collapsed;
                        _L1MouldSID!.Label = "Mould SID";
                        _L1AdditionalMould!.Label = "Additional Mould";
#else
                        L1Info.Header = "Label Side";
                        L1MasteringRing.Visibility = Visibility.Collapsed;
                        L1MasteringSID.Visibility = Visibility.Collapsed;
                        L1Toolstamp.Visibility = Visibility.Collapsed;
                        L1MouldSID.Label = "Mould SID";
                        L1AdditionalMould.Label = "Additional Mould";
#endif
                    }

                    break;

                // All other media we assume to have no rings
                default:
#if NET35
                    _L0Info!.Visibility = Visibility.Collapsed;
                    _L1Info!.Visibility = Visibility.Collapsed;
                    _L2Info!.Visibility = Visibility.Collapsed;
                    _L3Info!.Visibility = Visibility.Collapsed;
#else
                    L0Info.Visibility = Visibility.Collapsed;
                    L1Info.Visibility = Visibility.Collapsed;
                    L2Info.Visibility = Visibility.Collapsed;
                    L3Info.Visibility = Visibility.Collapsed;
#endif
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
#if NET35
                    _DiscKeyTextBox!.Visibility = Visibility.Visible;
#else
                    DiscKeyTextBox.Visibility = Visibility.Visible;
#endif
                    break;

                case RedumpSystem.SonyPlayStation2:
#if NET35
                    _LanguageSelectionGrid!.Visibility = Visibility.Visible;
#else
                    LanguageSelectionGrid.Visibility = Visibility.Visible;
#endif
                    break;

                case RedumpSystem.SonyPlayStation3:
#if NET35
                    _DiscKeyTextBox!.Visibility = Visibility.Visible;
                    _DiscIDTextBox!.Visibility = Visibility.Visible;
#else
                    DiscKeyTextBox.Visibility = Visibility.Visible;
                    DiscIDTextBox.Visibility = Visibility.Visible;
#endif
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
