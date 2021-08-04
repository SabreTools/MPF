using System.Windows;
using MPF.GUI.ViewModels;
using MPF.Data;
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
        /// Read-only access to the current log window
        /// </summary>
        public static LogViewModel Logger => Instance.LogOutput.LogViewModel;

        /// <summary>
        /// Access to the current options
        /// </summary>
        public static Options Options
        {
            get => _options;
            set
            {
                _options = value;
                OptionsLoader.SaveToConfig(_options);
            }
        }

        /// <summary>
        /// Internal reference to Options
        /// </summary>
        private static Options _options;

        /// <summary>
        /// Constructor
        /// </summary>
        public App()
        {
            _appInstance = this;
            _options = OptionsLoader.LoadFromConfig();
        }
    }
}
