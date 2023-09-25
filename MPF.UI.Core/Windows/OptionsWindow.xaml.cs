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

#if NET6_0_OR_GREATER
            this.SkipMediaTypeDetectionCheckBox.IsEnabled = false;
            this.SkipMediaTypeDetectionCheckBox.IsChecked = false;
            this.SkipMediaTypeDetectionCheckBox.ToolTip = "This feature is not enabled for .NET 6";
#endif
        }
    }
}
