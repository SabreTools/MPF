using MPF.Core.Data;
using MPF.UI.Core.ViewModels;

namespace MPF.UI.Core.Windows
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : WindowBase
    {
        /// <summary>
        /// Read-only access to the current options view model
        /// </summary>
        public OptionsViewModel OptionsViewModel => DataContext as OptionsViewModel;

        /// <summary>
        /// Constructor
        /// </summary>
        public OptionsWindow(Options options)
        {
            InitializeComponent();
            DataContext = new OptionsViewModel(this, options);
        }
    }
}
