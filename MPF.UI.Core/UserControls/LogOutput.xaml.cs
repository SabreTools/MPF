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
            DataContext = new LogOutputViewModel(Dispatcher);

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
        /// Enqueue text to the log with formatting
        /// </summary>
        /// <param name="logLevel">LogLevel for the log</param>
        /// <param name="text">Text to write to the log</param>
        public void EnqueueLog(LogLevel logLevel, string text)
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
