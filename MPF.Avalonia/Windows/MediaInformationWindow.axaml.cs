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
        /// Read-only access to the current media information view model
        /// </summary>
        public MediaInformationViewModel MediaInformationViewModel
            => DataContext as MediaInformationViewModel ?? new MediaInformationViewModel(new Options(), new SubmissionInfo());

        /// <summary>
        /// Constructor required for runtime loader
        /// </summary>
        public MediaInformationWindow()
            : this(new Options(), new SubmissionInfo())
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public MediaInformationWindow(Options options, SubmissionInfo? submissionInfo)
        {
            InitializeComponent();

            DataContext = new MediaInformationViewModel(options, submissionInfo);

            // Add handlers
            AcceptButton!.Click += OnAcceptClick;
            CancelButton!.Click += OnCancelClick;
            RingCodeGuideButton!.Click += OnRingCodeGuideClick;
        }

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
