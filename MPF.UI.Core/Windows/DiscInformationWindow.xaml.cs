using MPF.Core.Data;
using MPF.UI.Core.ViewModels;
using RedumpLib.Data;

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
        public DiscInformationViewModel DiscInformationViewModel => DataContext as DiscInformationViewModel;

        /// <summary>
        /// Constructor
        /// </summary>
        public DiscInformationWindow(Options options, SubmissionInfo submissionInfo)
        {
            InitializeComponent();
            DataContext = new DiscInformationViewModel(this, options, submissionInfo);
        }
    }
}
