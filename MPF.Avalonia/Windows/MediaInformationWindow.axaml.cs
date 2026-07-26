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

            // Handle an Avalonia-specific case
            // TODO: Do these need to be explicitly set if they're in the AXAML?
            PCMacHybridGrid!.IsVisible = _showPcMacHybridAlways
                || submissionInfo?.DiscIdentity?.Media == MediaType.CD;
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
