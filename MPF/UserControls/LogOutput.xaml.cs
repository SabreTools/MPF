using System.Windows.Controls;
using MPF.GUI.ViewModels;

namespace MPF.UserControls
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

        #region Logging

        /// <summary>
        /// Enqueue text to the log
        /// </summary>
        /// <param name="text">Text to write to the log</param>
        public void Log(string text) => LogViewModel.LogInternal(text);

        /// <summary>
        /// Enqueue text with a newline to the log
        /// </summary>
        /// <param name="text">Text to write to the log</param>
        public void LogLn(string text) => Log(text + "\n");

        /// <summary>
        /// Enqueue error text to the log
        /// </summary>
        /// <param name="text">Text to write to the log</param>
        public void ErrorLog(string text) => LogViewModel.LogInternal(text, LogViewModel.LogLevel.ERROR);

        /// <summary>
        /// Enqueue error text with a newline to the log
        /// </summary>
        /// <param name="text">Text to write to the log</param>
        public void ErrorLogLn(string text) => ErrorLog(text + "\n");

        /// <summary>
        /// Enqueue secret text to the log
        /// </summary>
        /// <param name="text">Text to write to the log</param>
        public void SecretLog(string text) => LogViewModel.LogInternal(text, LogViewModel.LogLevel.SECRET);

        /// <summary>
        /// Enqueue secret text with a newline to the log
        /// </summary>
        /// <param name="text">Text to write to the log</param>
        public void SecretLogLn(string text) => SecretLog(text + "\n");

        /// <summary>
        /// Enqueue verbose text to the log
        /// </summary>
        /// <param name="text">Text to write to the log</param>
        public void VerboseLog(string text) => LogViewModel.LogInternal(text, LogViewModel.LogLevel.VERBOSE);

        /// <summary>
        /// Enqueue verbose text with a newline to the log
        /// </summary>
        /// <param name="text">Text to write to the log</param>
        public void VerboseLogLn(string text) => VerboseLog(text + "\n");

        /// <summary>
        /// Reset the progress bar state
        /// </summary>
        public void ResetProgressBar() =>
            LogViewModel.ResetProgressBar();

        #endregion
    }
}
