using System;
using System.Windows;
using System.Windows.Controls;
using MPF.Core.Data;
using MPF.UI.Core.ViewModels;

namespace MPF.UI.Core.UserControls
{
    public partial class LogOutput : UserControl
    {
        /// <summary>
        /// Read-only access to the current log output view model
        /// </summary>
        public LogOutputViewModel LogOutputViewModel => DataContext as LogOutputViewModel;

        public LogOutput()
        {
            InitializeComponent();
            DataContext = new LogOutputViewModel();

            // Add handlers
            OutputViewer.SizeChanged += OutputViewerSizeChanged;
            Output.TextChanged += OnTextChanged;
            ClearButton.Click += OnClearButton;
            SaveButton.Click += OnSaveButton;

            // Update the internal state
            Output.Document = LogOutputViewModel.Document;
        }

        #region Logging

        /// <summary>
        /// Enqueue text to the log
        /// </summary>
        /// <param name="text">Text to write to the log</param>
        public void Log(string text) => LogInternal(text, LogLevel.USER);

        /// <summary>
        /// Enqueue text with a newline to the log
        /// </summary>
        /// <param name="text">Text to write to the log</param>
        public void LogLn(string text) => Log(text + "\n");

        /// <summary>
        /// Enqueue error text to the log
        /// </summary>
        /// <param name="text">Text to write to the log</param>
        public void ErrorLog(string text) => LogInternal(text, LogLevel.ERROR);

        /// <summary>
        /// Enqueue error text with a newline to the log
        /// </summary>
        /// <param name="text">Text to write to the log</param>
        public void ErrorLogLn(string text) => ErrorLog(text + "\n");

        /// <summary>
        /// Enqueue secret text to the log
        /// </summary>
        /// <param name="text">Text to write to the log</param>
        public void SecretLog(string text) => LogInternal(text, LogLevel.SECRET);

        /// <summary>
        /// Enqueue secret text with a newline to the log
        /// </summary>
        /// <param name="text">Text to write to the log</param>
        public void SecretLogLn(string text) => SecretLog(text + "\n");

        /// <summary>
        /// Enqueue verbose text to the log
        /// </summary>
        /// <param name="text">Text to write to the log</param>
        public void VerboseLog(string text) => LogInternal(text, LogLevel.VERBOSE);

        /// <summary>
        /// Enqueue verbose text with a newline to the log
        /// </summary>
        /// <param name="text">Text to write to the log</param>
        public void VerboseLogLn(string text) => VerboseLog(text + "\n");

        /// <summary>
        /// Reset the progress bar state
        /// </summary>
        public void ResetProgressBar()
        {
            Dispatcher.Invoke(() =>
            {
                ProgressBar.Value = 0;
                ProgressLabel.Text = string.Empty;
            });
        }

        /// <summary>
        /// Enqueue text to the log with formatting
        /// </summary>
        /// <param name="text">Text to write to the log</param>
        /// <param name="logLevel">LogLevel for the log</param>
        private void LogInternal(string text, LogLevel logLevel)
        {
            // Null text gets ignored
            if (text == null)
                return;

            // Enqueue the text
            LogOutputViewModel.LogQueue.Enqueue(new LogOutputViewModel.LogLine(text, logLevel));
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Scroll the current view to the bottom
        /// </summary>
        public void ScrollToBottom() => OutputViewer.ScrollToBottom();

        #endregion

        #region EventHandlers

        private void OnClearButton(object sender, EventArgs e)
        {
            LogOutputViewModel.ClearInlines();
            ResetProgressBar();
        }

        private void OnSaveButton(object sender, EventArgs e)
            => LogOutputViewModel.SaveInlines();

        private void OnTextChanged(object sender, TextChangedEventArgs e)
            => ScrollToBottom();

        private void OutputViewerSizeChanged(object sender, SizeChangedEventArgs e)
            => ScrollToBottom();

        #endregion
    }
}
