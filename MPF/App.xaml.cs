using System.Windows;
using MPF.Core.Data;
using MPF.UI.ViewModels;
using MPF.Windows;

namespace MPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// <remarks>
    /// This application is not fully MVVM. The following steps are needed to get there:
    /// - Use commands instead of event handlers, where at all possible
    /// - Reduce the amount of manual UI adjustments needed, instead binding to the view models
    /// </remarks>
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
