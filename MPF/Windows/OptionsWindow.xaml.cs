using MPF.GUI.ViewModels;
using MPF.UI.Core.Windows;

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
        public OptionsViewModel OptionsViewModel => DataContext as OptionsViewModel;

        /// <summary>
        /// Constructor
        /// </summary>
        public OptionsWindow()
        {
            InitializeComponent();
            DataContext = new OptionsViewModel(this);
        }
    }
}
