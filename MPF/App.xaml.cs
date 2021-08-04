using System.Windows;
using MPF.Windows;

namespace MPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Static application instance for reference
        /// </summary>
        private static App _appInstance;

        /// <summary>
        /// Read-only access to the current main window
        /// </summary>
        public static MainWindow Instance => _appInstance.MainWindow as MainWindow;

        /// <summary>
        /// Constructor
        /// </summary>
        public App() => _appInstance = this;
    }
}
