using MPF.Data;

namespace MPF.Windows
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : WindowBase
    {
        /// <summary>
        /// Read-only access to the current options view model
        /// </summary>
        public GUI.ViewModels.OptionsViewModel OptionsViewModel => DataContext as GUI.ViewModels.OptionsViewModel;

        /// <summary>
        /// Constructor
        /// </summary>
        public OptionsWindow(Options options)
        {
            InitializeComponent();
            DataContext = new GUI.ViewModels.OptionsViewModel(this, options);
        }
    }
}
