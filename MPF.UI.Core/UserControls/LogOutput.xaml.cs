using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using MPF.Core;
using MPF.Core.Data;

#pragma warning disable IDE1006 // Naming Styles

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
        private Run? lastLine = null;

#if NET35

        private Button? _ClearButton => ItemHelper.FindChild<Button>(this, "ClearButton");
        private RichTextBox? _Output => ItemHelper.FindChild<RichTextBox>(this, "Output");
        private ScrollViewer? _OutputViewer => ItemHelper.FindChild<ScrollViewer>(this, "OutputViewer");
        private Button? _SaveButton => ItemHelper.FindChild<Button>(this, "SaveButton");

#endif

        public LogOutput()
        {
#if NET40_OR_GREATER || NETCOREAPP
            InitializeComponent();
#endif

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
#if NET35
            _OutputViewer!.SizeChanged += OutputViewerSizeChanged;
            _Output!.TextChanged += OnTextChanged;
            _ClearButton!.Click += OnClearButton;
            _SaveButton!.Click += OnSaveButton;
#else
            OutputViewer.SizeChanged += OutputViewerSizeChanged;
            Output.TextChanged += OnTextChanged;
            ClearButton.Click += OnClearButton;
            SaveButton.Click += OnSaveButton;
#endif

            // Update the internal state
#if NET35
            _Output.Document = Document;
#else
            Output.Document = Document;
#endif
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
                return this.LogLevel switch
                {
                    LogLevel.SECRET => Brushes.Blue,
                    LogLevel.ERROR => Brushes.Red,
                    LogLevel.VERBOSE => Brushes.Yellow,
                    _ => Brushes.White,
                };
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
                lastLine ??= new Run();
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
            using var sw = new StreamWriter(File.OpenWrite("console.log"));
            foreach (var inline in _paragraph.Inlines)
            {
                if (inline is Run run)
                    sw.Write(run.Text);
            }
        }

        /// <summary>
        /// Scroll the current view to the bottom
        /// </summary>
#if NET35
        public void ScrollToBottom() => _OutputViewer!.ScrollToBottom();
#else
        public void ScrollToBottom() => OutputViewer.ScrollToBottom();
#endif

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
