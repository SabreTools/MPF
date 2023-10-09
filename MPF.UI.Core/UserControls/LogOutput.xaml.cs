using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using MPF.Core.Data;

namespace MPF.UI.Core.UserControls
{
    public partial class LogOutput : UserControl
    {
        /// <summary>
        /// Document representing the text
        /// </summary>
        internal FlowDocument Document { get; private set; }

        /// <summary>
        /// Queue of items that need to be logged
        /// </summary>
        internal ProcessingQueue<LogLine> LogQueue { get; private set; }

        /// <summary>
        /// Paragraph backing the log
        /// </summary>
        private readonly Paragraph _paragraph;

        /// <summary>
        /// Cached value of the last line written
        /// </summary>
        private Run lastLine = null;

        public LogOutput()
        {
            InitializeComponent();

            // Update the internal state
            Document = new FlowDocument()
            {
                Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x20, 0x20, 0x20))
            };
            _paragraph = new Paragraph();
            Document.Blocks.Add(_paragraph);

            // Setup the processing queue
            LogQueue = new ProcessingQueue<LogLine>(ProcessLogLine);

            // Add handlers
            OutputViewer.SizeChanged += OutputViewerSizeChanged;
            Output.TextChanged += OnTextChanged;
            ClearButton.Click += OnClearButton;
            SaveButton.Click += OnSaveButton;

            // Update the internal state
            Output.Document = Document;
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
            LogQueue.Enqueue(new LogLine(text, logLevel));
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
                this.Text = text;
                this.LogLevel = logLevel;
            }

            /// <summary>
            /// Get the foreground Brush for the current LogLevel
            /// </summary>
            /// <returns>Brush representing the color</returns>
            public Brush GetForegroundColor()
            {
                switch (this.LogLevel)
                {
                    case LogLevel.SECRET:
                        return Brushes.Blue;
                    case LogLevel.ERROR:
                        return Brushes.Red;
                    case LogLevel.VERBOSE:
                        return Brushes.Yellow;
                    case LogLevel.USER:
                    default:
                        return Brushes.White;
                }
            }

            /// <summary>
            /// Generate a Run object from the current LogLine
            /// </summary>
            /// <returns>Run object based on internal values</returns>
            public Run GenerateRun()
            {
                return new Run { Text = this.Text, Foreground = GetForegroundColor() };
            }
        }

        /// <summary>
        /// Process the log lines in the queue
        /// </summary>
        /// <param name="nextLogLine">LogLine item to process</param>
        internal void ProcessLogLine(LogLine nextLogLine)
        {
            // Null text gets ignored
            string nextText = nextLogLine.Text;
            if (nextText == null)
                return;

            try
            {
                if (nextText.StartsWith("\r"))
                    ReplaceLastLine(nextLogLine);
                else
                    AppendToTextBox(nextLogLine);
            }
            catch (Exception ex)
            {
                // In the event that something fails horribly, we want to log
                AppendToTextBox(new LogLine(ex.ToString(), LogLevel.ERROR));
            }
        }

        /// <summary>
        /// Append log line to the log text box
        /// </summary>
        /// <param name="logLine">LogLine value to append</param>
        private void AppendToTextBox(LogLine logLine)
        {
            Dispatcher.Invoke(() =>
            {
                var run = logLine.GenerateRun();
                _paragraph.Inlines.Add(run);
                lastLine = run;
            });
        }

        /// <summary>
        /// Replace the last line written to the log text box
        /// </summary>
        /// <param name="logLine">LogLine value to append</param>
        private void ReplaceLastLine(LogLine logLine)
        {
            Dispatcher.Invoke(() =>
            {
                lastLine.Text = logLine.Text;
                lastLine.Foreground = logLine.GetForegroundColor();
            });
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
            using (StreamWriter tw = new StreamWriter(File.OpenWrite("console.log")))
            {
                foreach (var inline in _paragraph.Inlines)
                {
                    if (inline is Run run)
                        tw.Write(run.Text);
                }
            }
        }

        /// <summary>
        /// Scroll the current view to the bottom
        /// </summary>
        public void ScrollToBottom() => OutputViewer.ScrollToBottom();

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
