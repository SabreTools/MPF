using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using MPF.Core.Data;
using MPF.UserControls;

namespace MPF.UI.ViewModels
{
    public class LogViewModel
    {
        #region Fields

        /// <summary>
        /// Parent OptionsWindow object
        /// </summary>
        public LogOutput Parent { get; private set; }

        #endregion

        #region Private State Variables

        /// <summary>
        /// Paragraph backing the log
        /// </summary>
        private readonly Paragraph _paragraph;

        /// <summary>
        /// Cached value of the last line written
        /// </summary>
        private Run lastLine = null;

        /// <summary>
        /// Queue of items that need to be logged
        /// </summary>
        private readonly ProcessingQueue<LogLine> logQueue;

        /// <summary>
        /// List of Matchers for progress tracking
        /// </summary>
        private readonly List<Matcher?> _matchers;

        /// <summary>
        /// Cached value of the last matcher used
        /// </summary>
        private Matcher? lastUsedMatcher = null;

        /// <summary>
        /// Regex pattern to find DiscImageCreator progress messages
        /// </summary>
        private const string DiscImageCreatorProgressPattern = @"\s*(\d+)\/\s*(\d+)$";

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public LogViewModel(LogOutput parent)
        {
            Parent = parent;

            // Add handlers
            Parent.OutputViewer.SizeChanged += OutputViewerSizeChanged;
            Parent.Output.TextChanged += OnTextChanged;
            Parent.ClearButton.Click += OnClearButton;
            Parent.SaveButton.Click += OnSaveButton;

            // Update the internal state
            var document = new FlowDocument()
            {
                Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x20, 0x20, 0x20))
            };
            _paragraph = new Paragraph();
            document.Blocks.Add(_paragraph);
            Parent.Output.Document = document;

            // TODO: Can we dynamically add matchers *only* during dumping?
            _matchers = new List<Matcher?>();
            AddAaruMatchers();
            AddDiscImageCreatorMatchers();

            logQueue = new ProcessingQueue<LogLine>(ProcessLogLine);
        }

        #region Matching

        /// <summary>
        /// Matching wrapper
        /// </summary>
        private struct Matcher
        {
            private readonly string prefix;
            private readonly Regex regex;
            private readonly int start;
            private readonly string progressBarText;
            private readonly Action<Match, string> lambda;

            public Matcher(string prefix, string regex, string progressBarText, Action<Match, string> lambda)
            {
                this.prefix = prefix;
                this.regex = new Regex(regex);
                this.start = prefix.Length;
                this.progressBarText = progressBarText;
                this.lambda = lambda;
            }

            /// <summary>
            /// Check if the text matches the prefix
            /// </summary>
            /// <param name="text">Text to check</param>
            /// <returns>True if the line starts with the prefix, false otherwise</returns>
            public bool Matches(string text) => text.StartsWith(prefix);

            /// <summary>
            /// Generate a Match and apply the lambda
            /// </summary>
            /// <param name="text">Text to match and apply from</param>
            public void Apply(string text)
            {
                Match match = regex?.Match(text, start);
                lambda?.Invoke(match, progressBarText);
            }
        }

        /// <summary>
        /// Add all Matchers for Aaru
        /// </summary>
        private void AddAaruMatchers()
        {
            // TODO: Determine matchers that can be added
        }

        /// <summary>
        /// Add all Matchers for DiscImageCreator
        /// </summary>
        private void AddDiscImageCreatorMatchers()
        {
            #region Pre-dump Checking

            _matchers.Add(new Matcher(
                "Checking EXE",
                DiscImageCreatorProgressPattern,
                "Checking executables...",
                StandardDiscImageCreatorProgress
            ));

            _matchers.Add(new Matcher(
                "Checking Pregap sync, msf, mode (LBA)",
                @"\s*-(\d+)$",
                "Checking Pregap sync, msf, mode",
                (match, text) =>
                {
                    Parent.ProgressBar.Value = 0;
                    Parent.ProgressLabel.Text = text;
                }));

            _matchers.Add(new Matcher(
                "Checking SubQ adr (Track)",
                DiscImageCreatorProgressPattern,
                "Checking SubQ adr...",
                StandardDiscImageCreatorProgress
            ));

            _matchers.Add(new Matcher(
                "Checking SubQ ctl (Track)",
                DiscImageCreatorProgressPattern,
                "Checking SubQ ctl...",
                StandardDiscImageCreatorProgress
            ));

            _matchers.Add(new Matcher(
                "Checking SubRtoW (Track)",
                DiscImageCreatorProgressPattern,
                "Checking SubRtoW...",
                StandardDiscImageCreatorProgress
            ));

            _matchers.Add(new Matcher(
                "Reading DirectoryRecord",
                DiscImageCreatorProgressPattern,
                "Reading directory records...",
                StandardDiscImageCreatorProgress
            ));

            _matchers.Add(new Matcher(
                "Scanning sector for anti-mod string (LBA)",
                DiscImageCreatorProgressPattern,
                "Scanning sectors for anti-mod string...",
                StandardDiscImageCreatorProgress
            ));

            #endregion

            #region Dumping

            _matchers.Add(new Matcher(
                @"Creating iso(LBA)",
                DiscImageCreatorProgressPattern,
                "Creating ISO...",
                StandardDiscImageCreatorProgress
            ));

            _matchers.Add(new Matcher(
                @"Creating .scm (LBA)",
                DiscImageCreatorProgressPattern,
                "Creating scrambled image...",
                StandardDiscImageCreatorProgress
            ));

            #endregion

            #region Post-Dump Processing

            _matchers.Add(new Matcher(
                "Checking sectors:",
                DiscImageCreatorProgressPattern,
                "Checking for errors...",
                StandardDiscImageCreatorProgress
            ));

            _matchers.Add(new Matcher(
                "Creating bin (Track)",
                DiscImageCreatorProgressPattern,
                "Creating BIN(s)...",
                StandardDiscImageCreatorProgress
            ));

            _matchers.Add(new Matcher(
                "Creating cue and ccd (Track)",
                DiscImageCreatorProgressPattern,
                "Creating CUE and CCD...",
                StandardDiscImageCreatorProgress
            ));

            _matchers.Add(new Matcher(
                "Descrambling data sector of img:",
                 DiscImageCreatorProgressPattern,
                "Descrambling image...",
                StandardDiscImageCreatorProgress
            ));

            _matchers.Add(new Matcher(
                "Scanning sector (LBA)",
                DiscImageCreatorProgressPattern,
                "Scanning sectors for protection...",
                StandardDiscImageCreatorProgress
            ));

            #endregion
        }

        #endregion

        #region Logging

        /// <summary>
        /// Log level for output
        /// </summary>
        public enum LogLevel
        {
            USER,
            VERBOSE,
            ERROR,
            SECRET,
        }

        /// <summary>
        /// Log line wrapper
        /// </summary>
        private struct LogLine
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
            Parent.Dispatcher.Invoke(() =>
            {
                Parent.ProgressBar.Value = 0;
                Parent.ProgressLabel.Text = string.Empty;
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
            logQueue.Enqueue(new LogLine(text, logLevel));
        }

        /// <summary>
        /// Process the log lines in the queue
        /// </summary>
        /// <param name="nextLogLine">LogLine item to process</param>
        private void ProcessLogLine(LogLine nextLogLine)
        {
            // Null text gets ignored
            string nextText = Parent.Dispatcher.Invoke(() => nextLogLine.Text);
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
            Parent.Dispatcher.Invoke(() =>
            {
                var run = logLine.GenerateRun();
                _paragraph.Inlines.Add(run);
                lastLine = run;
            });
        }

        /// <summary>
        /// Get the last line written to the log text box
        /// </summary>
        private Run GetLastLine()
        {
            return Parent.Dispatcher.Invoke(() =>
            {
                if (!_paragraph.Inlines.Any())
                    return null;

                return _paragraph.Inlines.LastInline as Run;
            });
        }

        /// <summary>
        /// Process text if it should update the progress bar
        /// </summary>
        /// <param name="text">Text to check and update with</param>
        private void ProcessStringForProgressBar(string text, Matcher? matcher)
        {
            Parent.Dispatcher.Invoke(() => { matcher?.Apply(text); });
        }

        /// <summary>
        /// Replace the last line written to the log text box
        /// </summary>
        /// <param name="logLine">LogLine value to append</param>
        private void ReplaceLastLine(LogLine logLine)
        {
            Parent.Dispatcher.Invoke(() =>
            {
                lastLine.Text = logLine.Text;
                lastLine.Foreground = logLine.GetForegroundColor();
            });
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Scroll the current view to the bottom
        /// </summary>
        public void ScrollToBottom()
        {
            Parent.OutputViewer.ScrollToBottom();
        }

        #endregion

        #region EventHandlers

        private void OnClearButton(object sender, EventArgs e)
        {
            _paragraph.Inlines.Clear();
            ResetProgressBar();
        }

        private void OnSaveButton(object sender, EventArgs e)
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

        private void OnTextChanged(object sender, TextChangedEventArgs e) =>
            ScrollToBottom();

        private void OutputViewerSizeChanged(object sender, SizeChangedEventArgs e) =>
            ScrollToBottom();

        private void StandardDiscImageCreatorProgress(Match match, string text)
        {
            if (uint.TryParse(match.Groups[1].Value, out uint current) && uint.TryParse(match.Groups[2].Value, out uint total))
            {
                float percentProgress = (current / (float)total) * 100;
                Parent.ProgressBar.Value = percentProgress;
                Parent.ProgressLabel.Text = string.Format($"{text} ({percentProgress:N2}%)");
            }
        }

        #endregion
    }
}
