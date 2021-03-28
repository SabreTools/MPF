using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace MPF.UserControls
{
    public partial class LogOutput : UserControl
    {
        /// <summary>
        /// Document backing the log
        /// </summary>
        private FlowDocument _document;

        /// <summary>
        /// Paragraph backing the log
        /// </summary>
        private Paragraph _paragraph;

        /// <summary>
        /// List of Matchers for progress tracking
        /// </summary>
        private readonly List<Matcher?> _matchers;

        /// <summary>
        /// Cached value of the last line written
        /// </summary>
        private Run lastLine = null;

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

            _document = new FlowDocument();
            _paragraph = new Paragraph();
            _document.Blocks.Add(_paragraph);
            Output.Document = _document;

            _matchers = new List<Matcher?>();
            AddAaruMatchers();
            AddDiscImageCreatorMatchers();
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

            public bool Matches(string text) => text.StartsWith(prefix);

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

        #region Writing

        /// <summary>
        /// Write text to the log
        /// </summary>
        /// <param name="text">Text to write to the log</param>
        public void Log(string text) => LogInternal(text, verbose: false);

        /// <summary>
        /// Write text with a newline to the log
        /// </summary>
        /// <param name="text">Text to write to the log</param>
        public void LogLn(string text) => Log(text + "\n");

        /// <summary>
        /// Write error text to the log
        /// </summary>
        /// <param name="text">Text to write to the log</param>
        public void ErrorLog(string text) => LogInternal(text, error: true);

        /// <summary>
        /// Write error text with a newline to the log
        /// </summary>
        /// <param name="text">Text to write to the log</param>
        public void ErrorLogLn(string text) => ErrorLog(text + "\n");

        /// <summary>
        /// Reset the progress bar state
        /// </summary>
        public void ResetProgressBar()
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                ProgressBar.Value = 0;
                ProgressLabel.Text = string.Empty;
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    ProgressBar.Value = 0;
                    ProgressLabel.Text = string.Empty;
                });
            }
        }

        /// <summary>
        /// Write verbose text to the log
        /// </summary>
        /// <param name="text">Text to write to the log</param>
        public void VerboseLog(string text) => LogInternal(text, verbose: true);

        /// <summary>
        /// Write verbose text with a newline to the log
        /// </summary>
        /// <param name="text">Text to write to the log</param>
        public void VerboseLogLn(string text) => VerboseLog(text + "\n");

        /// <summary>
        /// Append text to the log text box
        /// </summary>
        /// <param name="text">Text to append</param>
        /// <param name="color">Color to print text in</param>
        private void AppendToTextBox(string text, Brush color)
        {
            Dispatcher.Invoke(() =>
            {
                var run = new Run(text) { Foreground = color };
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
        /// Write text to the log with formatting
        /// </summary>
        /// <param name="text">Text to write to the log</param>
        /// <param name="verbose">True if the log is verbose output, false otherwise</param>
        /// <param name="error">True if the log is error output, false otherwise</param>
        private void LogInternal(string text, bool verbose = false, bool error = false)
        {
            // Null text gets ignored
            if (text == null)
                return;

            // If we have verbose logs but not enabled, ignore
            if (verbose && !ViewModels.OptionsViewModel.VerboseLogging)
                return;

            // Get the brush color from the flags
            Brush brush = Brushes.White;
            if (error)
                brush = Brushes.Red;
            else if (verbose)
                brush = Brushes.Yellow;

            try
            {
                // Get last line
                lastLine = lastLine ?? GetLastLine();

                // Always append if there's no previous line
                if (lastLine == null)
                {
                    AppendToTextBox(text, brush);
                    lastUsedMatcher = _matchers.FirstOrDefault(m => m.HasValue && m.Value.Matches(text));
                }
                // Return always means overwrite
                else if (text.StartsWith("\r"))
                {
                    ReplaceLastLine(text, brush);
                }
                // If we have a cached matcher and we match
                else if (lastUsedMatcher?.Matches(text) == true)
                {
                    ReplaceLastLine(text, brush);
                }
                else
                {
                    // Get the first matching Matcher
                    var firstMatcher = _matchers.FirstOrDefault(m => m.HasValue && m.Value.Matches(text));
                    if (firstMatcher.HasValue)
                    {
                        string lastText = Dispatcher.Invoke(() => { return lastLine.Text; });
                        if (firstMatcher.Value.Matches(lastText))
                            ReplaceLastLine(text, brush);
                        else if (string.IsNullOrWhiteSpace(lastText))
                            ReplaceLastLine(text, brush);
                        else
                            AppendToTextBox(text, brush);

                        // Cache the last used Matcher
                        lastUsedMatcher = firstMatcher;
                    }
                    // Default case for all other text
                    else
                    {
                        AppendToTextBox(text, brush);
                        lastUsedMatcher = null;
                    }
                }

                // Update the bar if needed
                ProcessStringForProgressBar(text, lastUsedMatcher);
            }
            catch (Exception ex)
            {
                // In the event that something fails horribly, we want to log
                AppendToTextBox(ex.ToString(), Brushes.Red);
            }
        }

        /// <summary>
        /// Process text if it should update the progress bar
        /// </summary>
        /// <param name="text">Text to check and update with</param>
        private void ProcessStringForProgressBar(string text, Matcher? matcher)
        {
            Dispatcher.Invoke(() =>
            {
                matcher?.Apply(text);
            });
        }

        /// <summary>
        /// Replace the last line written to the log text box
        /// </summary>
        /// <param name="text">Text to append</param>
        /// <param name="color">Color to print text in</param>
        private void ReplaceLastLine(string text, Brush color)
        {
            Dispatcher.Invoke(() =>
            {
                lastLine.Text = text;
                lastLine.Foreground = color;
            });
        }

        #endregion

        #region EventHandlers

        private void OnClearButton(object sender, EventArgs e)
        {
            Output.Document.Blocks.Clear();
            _paragraph = new Paragraph();
            _document.Blocks.Add(_paragraph);
        }
        
        private void OnSaveButton(object sender, EventArgs e)
        {
            using (StreamWriter tw = new StreamWriter(File.OpenWrite("console.log")))
            {
                foreach (var inline in _paragraph.Inlines)
                {
                    if (inline is Run run)
                    {
                        tw.Write(run.Text);
                    }
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
