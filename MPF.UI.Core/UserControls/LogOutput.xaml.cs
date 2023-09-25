using System.Windows.Controls;
using MPF.UI.Core.ViewModels;

namespace MPF.UI.Core.UserControls
{
    public partial class LogOutput : UserControl
    {
        /// <summary>
        /// Read-only access to the current log view model
        /// </summary>
        public LogViewModel LogViewModel => DataContext as LogViewModel;

        public LogOutput()
        {
            InitializeComponent();
            DataContext = new LogViewModel(this);
        }
    }
}
