using MPF.UI.Core.Windows;
using MPF.UI.ViewModels;

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

#if NET6_0_OR_GREATER
            this.SkipMediaTypeDetectionCheckBox.IsEnabled = false;
            this.SkipMediaTypeDetectionCheckBox.IsChecked = false;
            this.SkipMediaTypeDetectionCheckBox.ToolTip = "This feature is not enabled for .NET 6";
#endif
        }
    }
}
