using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace MPF.UserControls
{
    public partial class LogOutput : UserControl
    {
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
        private readonly ConcurrentQueue<LogLine> logQueue;

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

        public LogOutput()
        {
            InitializeComponent();
            DataContext = this;

            var document = new FlowDocument();
            _paragraph = new Paragraph();
            document.Blocks.Add(_paragraph);
            Output.Document = document;

            _matchers = new List<Matcher?>();
            AddAaruMatchers();
            AddDiscImageCreatorMatchers();

            logQueue = new ConcurrentQueue<LogLine>();
            Task.Run(() => ProcessLogLines());
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
                    ProgressBar.Value = 0;
                    ProgressLabel.Text = text;
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
        private enum LogLevel
        {
            USER,
            VERBOSE,
            ERROR,
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
        public void Log(string text) => LogInternal(text);

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
        /// <param name="verbose">True if the log is verbose output, false otherwise</param>
        /// <param name="error">True if the log is error output, false otherwise</param>
        private void LogInternal(string text, LogLevel logLevel = LogLevel.USER)
        {
            // Null text gets ignored
            if (text == null)
                return;

            // If we have verbose logs but not enabled, ignore
            if (logLevel == LogLevel.VERBOSE && !ViewModels.OptionsViewModel.VerboseLogging)
                return;            

            // Enqueue the text
            logQueue.Enqueue(new LogLine(text, logLevel));
        }

        /// <summary>
        /// Process the log lines in the queue
        /// </summary>
        private void ProcessLogLines()
        {
            while (true)
            {
                // Nothing in the queue means we get to idle
                if (logQueue.Count == 0)
                    continue;

                // Get the next item from the queue
                if (!logQueue.TryDequeue(out LogLine nextLogLine))
                    continue;

                // Null text gets ignored
                string nextText = Dispatcher.Invoke(() => nextLogLine.Text);
                if (nextText == null)
                    continue;

                try
                {
                    // Get last line
                    lastLine = lastLine ?? GetLastLine();

                    // Always append if there's no previous line
                    if (lastLine == null)
                    {
                        AppendToTextBox(nextLogLine);
                        lastUsedMatcher = _matchers.FirstOrDefault(m => m.HasValue && m.Value.Matches(nextText));
                    }
                    // Return always means overwrite
                    else if (nextText.StartsWith("\r"))
                    {
                        ReplaceLastLine(nextLogLine);
                    }
                    // If we have a cached matcher and we match
                    else if (lastUsedMatcher?.Matches(nextText) == true)
                    {
                        ReplaceLastLine(nextLogLine);
                    }
                    else
                    {
                        // Get the first matching Matcher
                        var firstMatcher = _matchers.FirstOrDefault(m => m.HasValue && m.Value.Matches(nextText));
                        if (firstMatcher.HasValue)
                        {
                            string lastText = Dispatcher.Invoke(() => { return lastLine.Text; });
                            if (firstMatcher.Value.Matches(lastText))
                                ReplaceLastLine(nextLogLine);
                            else if (string.IsNullOrWhiteSpace(lastText))
                                ReplaceLastLine(nextLogLine);
                            else
                                AppendToTextBox(nextLogLine);

                            // Cache the last used Matcher
                            lastUsedMatcher = firstMatcher;
                        }
                        // Default case for all other text
                        else
                        {
                            AppendToTextBox(nextLogLine);
                            lastUsedMatcher = null;
                        }
                    }

                    // Update the bar if needed
                    ProcessStringForProgressBar(nextText, lastUsedMatcher);
                }
                catch (Exception ex)
                {
                    // In the event that something fails horribly, we want to log
                    AppendToTextBox(new LogLine(ex.ToString(), LogLevel.ERROR));
                }
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
        /// Get the last line written to the log text box
        /// </summary>
        private Run GetLastLine()
        {
            return Dispatcher.Invoke(() =>
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
            Dispatcher.Invoke(() => { matcher?.Apply(text); });
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

        #region EventHandlers

        private void OnClearButton(object sender, EventArgs e)
        {
            _paragraph.Inlines.Clear();
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

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            OutputViewer.ScrollToBottom();
        }

        private void OutputViewerSizeChanged(object sender, SizeChangedEventArgs e)
        {
            OutputViewer.ScrollToBottom();
        }

        private void StandardDiscImageCreatorProgress(Match match, string text)
        {
            if (uint.TryParse(match.Groups[1].Value, out uint current) && uint.TryParse(match.Groups[2].Value, out uint total))
            {
                float percentProgress = (current / (float)total) * 100;
                ProgressBar.Value = percentProgress;
                ProgressLabel.Text = string.Format($"{text} ({percentProgress:N2}%)");
            }
        }

        #endregion
    }
}
