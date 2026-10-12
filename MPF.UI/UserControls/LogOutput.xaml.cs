using System;
using System.Collections;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using MPF.Frontend;

namespace MPF.UI.UserControls
{
    public partial class LogOutput : UserControl
    {
        /// <summary>
        /// Document representing the text
        /// </summary>
        private readonly FlowDocument _document;

        /// <summary>
        /// Paragraph backing the log
        /// </summary>
        private readonly Paragraph _paragraph;

        /// <summary>
        /// Maximum number of inlines before trimming
        /// </summary>
        private const int MaxInlineCount = 5000;

#if NET35

        private RichTextBox? Output => ItemHelper.FindChild<RichTextBox>(this, "Output");
        private ScrollViewer? OutputViewer => ItemHelper.FindChild<ScrollViewer>(this, "OutputViewer");

#endif

        public LogOutput()
        {
#if NET40_OR_GREATER || NETCOREAPP
            InitializeComponent();
#endif

            // Update the internal state
            _document = new FlowDocument()
            {
                Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x20, 0x20, 0x20))
            };
            _paragraph = new Paragraph();
            _document.Blocks.Add(_paragraph);

            // Update the internal state
            Output.Document = _document;
        }

        #region Logging

        /// <summary>
        /// Enqueue text to the log with formatting
        /// </summary>
        /// <param name="logLevel">LogLevel for the log</param>
        /// <param name="text">Text to write to the log</param>
        public void EnqueueLog(LogLevel logLevel, string text)
        {
            // Create a new log line
            var logLine = new LogLine(text, logLevel);

#if NET40
            Dispatcher.Invoke(() =>
#else
            Dispatcher.InvokeAsync(() =>
#endif
            {
                var run = logLine.GenerateRun();
                _paragraph.Inlines.Add(run);
                if (_paragraph.Inlines.Count > MaxInlineCount)
                    ((IList)_paragraph.Inlines).RemoveAt(0);
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
            /// <returns>Brush representing the color</returns>
            public Brush GetForegroundColor()
            {
                return LogLevel switch
                {
                    LogLevel.SECRET => Brushes.DarkGray,
                    LogLevel.ERROR => Brushes.Red,
                    LogLevel.VERBOSE => Brushes.Yellow,
                    LogLevel.USER_SUCCESS => Brushes.ForestGreen,
                    LogLevel.USER_GENERIC => Brushes.White,

                    // Make unmatched log levels obvious
                    _ => Brushes.Pink,
                };
            }

            /// <summary>
            /// Generate a Run object from the current LogLine
            /// </summary>
            /// <returns>Run object based on internal values</returns>
            public Run GenerateRun()
            {
                return new Run { Text = Text, Foreground = GetForegroundColor() };
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Clear all inlines of the paragraph
        /// </summary>
        private void ClearInlines() => _paragraph.Inlines.Clear();

        /// <summary>
        /// Save all inlines to console.log
        /// </summary>
        private void SaveInlines()
        {
            try
            {
                string logPath = Path.Combine(Environment.CurrentDirectory, "console.log");

                // Ensure the output directory has been created
                string? parentDirectory = Path.GetDirectoryName(logPath);
                if (parentDirectory is not null)
                    Directory.CreateDirectory(parentDirectory);

                using var sw = new StreamWriter(File.Open(logPath, FileMode.Create, FileAccess.Write, FileShare.Read));
                foreach (var inline in _paragraph.Inlines)
                {
                    if (inline is Run run)
                        sw.Write(run.Text);
                }
            }
            catch
            {
                // TODO: Report to user that the writing failed
            }
        }

        /// <summary>
        /// Scroll the current view to the bottom
        /// </summary>
        public void ScrollToBottom() => OutputViewer!.ScrollToBottom();

        #endregion

        #region EventHandlers

        private void OnClearButton(object sender, EventArgs e)
            => ClearInlines();

        private void OnSaveButton(object sender, EventArgs e)
            => SaveInlines();

        private void OnTextChanged(object sender, TextChangedEventArgs e)
            => ScrollToBottom();

        private void OutputViewerSizeChanged(object sender, SizeChangedEventArgs e)
            => ScrollToBottom();

        #endregion
    }
}
