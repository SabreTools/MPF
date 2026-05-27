using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using MPF.Frontend;

namespace MPF.UI.Avalonia.Controls
{
    public partial class LogOutput : UserControl
    {
        /// <summary>
        /// Maximum number of inlines before trimming
        /// </summary>
        private const int MaxInlineCount = 5000;

        public LogOutput()
        {
            AvaloniaXamlLoader.Load(this);

            ClearButton.Click += OnClearButton;
            SaveButton.Click += OnSaveButton;
            Output.SizeChanged += OnOutputSizeChanged;
        }

        #region Logging

        /// <summary>
        /// Enqueue text to the log with formatting.
        /// Matches the WPF signature used as the <c>Action&lt;LogLevel, string&gt;</c> logger
        /// delegate passed to <c>MainViewModel.Init</c>.
        /// Safe to call from any thread.
        /// </summary>
        /// <param name="logLevel">LogLevel for the log</param>
        /// <param name="text">Text to write to the log</param>
        public void EnqueueLog(LogLevel logLevel, string text)
        {
            // Null text gets ignored
            if (text is null)
                return;

            var logLine = new LogLine(text, logLevel);

            Dispatcher.UIThread.Post(() =>
            {
                var inlines = Output.Inlines;
                if (inlines is null)
                    return;

                var run = logLine.GenerateRun();
                inlines.Add(run);

                while (inlines.Count > MaxInlineCount)
                    inlines.RemoveAt(0);

                ScrollToBottom();
            });
        }

        /// <summary>
        /// Log line wrapper
        /// </summary>
        internal readonly struct LogLine
        {
            public readonly string Text;
            public readonly LogLevel LogLevel;

            public LogLine(string text, LogLevel logLevel)
            {
                Text = text;
                LogLevel = logLevel;
            }

            /// <summary>
            /// Get the foreground Brush for the current LogLevel
            /// </summary>
            public IBrush GetForegroundColor()
            {
                return LogLevel switch
                {
                    LogLevel.SECRET => Brushes.DarkGray,
                    LogLevel.ERROR => Brushes.Red,
                    LogLevel.VERBOSE => Brushes.Yellow,
                    LogLevel.USER_SUCCESS => Brushes.ForestGreen,
                    LogLevel.USER_GENERIC => Brushes.White,
                    _ => Brushes.Pink,
                };
            }

            /// <summary>
            /// Generate a Run object from the current LogLine
            /// </summary>
            public Run GenerateRun()
            {
                return new Run { Text = Text, Foreground = GetForegroundColor() };
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Clear all inlines of the output block
        /// </summary>
        private void ClearInlines() => Output.Inlines?.Clear();

        /// <summary>
        /// Save all inlines to console.log
        /// </summary>
        private void SaveInlines()
        {
            var inlines = Output.Inlines;
            if (inlines is null)
                return;

            using var sw = new StreamWriter(File.OpenWrite("console.log"));
            foreach (var inline in inlines)
            {
                if (inline is Run run)
                    sw.Write(run.Text);
            }
        }

        /// <summary>
        /// Scroll the current view to the bottom
        /// </summary>
        public void ScrollToBottom() => OutputViewer.ScrollToEnd();

        #endregion

        #region Event Handlers

        private void OnClearButton(object? sender, EventArgs e)
            => ClearInlines();

        private void OnSaveButton(object? sender, EventArgs e)
            => SaveInlines();

        private void OnOutputSizeChanged(object? sender, EventArgs e)
            => ScrollToBottom();

        #endregion
    }
}
