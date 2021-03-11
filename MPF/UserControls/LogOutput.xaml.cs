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
        private readonly List<Matcher> _matchers;

        /// <summary>
        /// Cached value of the last line written
        /// </summary>
        private string lastLineText = null;

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

            _matchers = new List<Matcher>();
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
            private readonly Action<Match> lambda;

            public Matcher(string prefix, string regex, Action<Match> lambda)
            {
                this.prefix = prefix;
                this.regex = new Regex(regex);
                this.start = prefix.Length;
                this.lambda = lambda;
            }

            public bool Matches(ref string text) => text.StartsWith(prefix);

            public void Apply(ref string text)
            {
                Match match = regex?.Match(text, start);
                lambda?.Invoke(match);
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
                match =>
                {
                    if (UInt32.TryParse(match.Groups[1].Value, out uint current) && UInt32.TryParse(match.Groups[2].Value, out uint total))
                    {
                        float percentProgress = (current / (float)total) * 100;
                        ProgressBar.Value = percentProgress;
                        ProgressLabel.Text = string.Format("Checking executables... ({0:##.##}%)", percentProgress);
                    }
                }));

            _matchers.Add(new Matcher(
                "Checking Pregap sync, msf, mode (LBA)",
                @"\s*-(\d+)$",
                match =>
                {
                    ProgressBar.Value = 0;
                    ProgressLabel.Text = "Checking Pregap sync, msf, mode";
                }));

            _matchers.Add(new Matcher(
                "Checking SubQ adr (Track)",
                DiscImageCreatorProgressPattern,
                match =>
                {
                    if (UInt32.TryParse(match.Groups[1].Value, out uint current) && UInt32.TryParse(match.Groups[2].Value, out uint total))
                    {
                        float percentProgress = (current / (float)total) * 100;
                        ProgressBar.Value = percentProgress;
                        ProgressLabel.Text = string.Format("Checking SubQ adr... ({0:##.##}%)", percentProgress);
                    }
                }));

            _matchers.Add(new Matcher(
                "Checking SubQ ctl (Track)",
                DiscImageCreatorProgressPattern,
                match =>
                {
                    if (UInt32.TryParse(match.Groups[1].Value, out uint current) && UInt32.TryParse(match.Groups[2].Value, out uint total))
                    {
                        float percentProgress = (current / (float)total) * 100;
                        ProgressBar.Value = percentProgress;
                        ProgressLabel.Text = string.Format("Checking SubQ ctl... ({0:##.##}%)", percentProgress);
                    }
                }));

            _matchers.Add(new Matcher(
                "Checking SubRtoW (Track)",
                DiscImageCreatorProgressPattern,
                match =>
                {
                    if (UInt32.TryParse(match.Groups[1].Value, out uint current) && UInt32.TryParse(match.Groups[2].Value, out uint total))
                    {
                        float percentProgress = (current / (float)total) * 100;
                        ProgressBar.Value = percentProgress;
                        ProgressLabel.Text = string.Format("Checking SubRtoW... ({0:##.##}%)", percentProgress);
                    }
                }));

            _matchers.Add(new Matcher(
                "Reading DirectoryRecord",
                DiscImageCreatorProgressPattern,
                match =>
                {
                    if (UInt32.TryParse(match.Groups[1].Value, out uint current) && UInt32.TryParse(match.Groups[2].Value, out uint total))
                    {
                        float percentProgress = (current / (float)total) * 100;
                        ProgressBar.Value = percentProgress;
                        ProgressLabel.Text = string.Format("Reading directory records... ({0:##.##}%)", percentProgress);
                    }
                }));

            #endregion

            #region Dumping

            _matchers.Add(new Matcher(
                @"Creating iso(LBA)",
                DiscImageCreatorProgressPattern,
                match =>
                {
                    if (UInt32.TryParse(match.Groups[1].Value, out uint current) && UInt32.TryParse(match.Groups[2].Value, out uint total))
                    {
                        float percentProgress = (current / (float)total) * 100;
                        ProgressBar.Value = percentProgress;
                        ProgressLabel.Text = string.Format("Creating ISO... ({0:##.##}%)", percentProgress);
                    }
                }));

            _matchers.Add(new Matcher(
                @"Creating .scm (LBA)",
                DiscImageCreatorProgressPattern,
                match =>
                {
                    if (UInt32.TryParse(match.Groups[1].Value, out uint current) && UInt32.TryParse(match.Groups[2].Value, out uint total))
                    {
                        float percentProgress = (current / (float)total) * 100;
                        ProgressBar.Value = percentProgress;
                        ProgressLabel.Text = string.Format("Creating scrambled image... ({0:##.##}%)", percentProgress);
                    }
                }));

            #endregion

            #region Post-Dump Processing

            _matchers.Add(new Matcher(
               "Checking sectors:",
               DiscImageCreatorProgressPattern,
               match =>
               {
                   if (UInt32.TryParse(match.Groups[1].Value, out uint current) && UInt32.TryParse(match.Groups[2].Value, out uint total))
                   {
                       float percentProgress = (current / (float)total) * 100;
                       ProgressBar.Value = percentProgress;
                       ProgressLabel.Text = string.Format("Checking for errors... ({0:##.##}%)", percentProgress);
                   }
               }));

            _matchers.Add(new Matcher(
               "Creating bin (Track)",
               DiscImageCreatorProgressPattern,
               match =>
               {
                   if (UInt32.TryParse(match.Groups[1].Value, out uint current) && UInt32.TryParse(match.Groups[2].Value, out uint total))
                   {
                       float percentProgress = (current / (float)total) * 100;
                       ProgressBar.Value = percentProgress;
                       ProgressLabel.Text = string.Format("Creating BIN(s)... ({0:##.##}%)", percentProgress);
                   }
               }));

            _matchers.Add(new Matcher(
                "Creating cue and ccd (Track)",
                DiscImageCreatorProgressPattern,
                match =>
                {
                    if (UInt32.TryParse(match.Groups[1].Value, out uint current) && UInt32.TryParse(match.Groups[2].Value, out uint total))
                    {
                        float percentProgress = (current / (float)total) * 100;
                        ProgressBar.Value = percentProgress;
                        ProgressLabel.Text = string.Format("Creating CUE and CCD... ({0:##.##}%)", percentProgress);
                    }
                }));

            _matchers.Add(new Matcher(
                   "Descrambling data sector of img:",
                   DiscImageCreatorProgressPattern,
                   match =>
                   {
                       if (UInt32.TryParse(match.Groups[1].Value, out uint current) && UInt32.TryParse(match.Groups[2].Value, out uint total))
                       {
                           float percentProgress = (current / (float)total) * 100;
                           ProgressBar.Value = percentProgress;
                           ProgressLabel.Text = string.Format("Descrambling image... ({0:##.##}%)", percentProgress);
                       }
                   }));

            _matchers.Add(new Matcher(
                "Scanning sector (LBA)",
                DiscImageCreatorProgressPattern,
                match =>
                {
                    if (UInt32.TryParse(match.Groups[1].Value, out uint current) && UInt32.TryParse(match.Groups[2].Value, out uint total))
                    {
                        float percentProgress = (current / (float)total) * 100;
                        ProgressBar.Value = percentProgress;
                        ProgressLabel.Text = string.Format("Scanning sectors for protection... ({0:##.##}%)", percentProgress);
                    }
                }));

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
            if (Application.Current.Dispatcher.CheckAccess())
            {
                Run run = new Run(text) { Foreground = color };
                _paragraph.Inlines.Add(run);
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    Run run = new Run(text) { Foreground = color };
                    _paragraph.Inlines.Add(run);
                });
            }
        }

        /// <summary>
        /// Get the last line written to the log text box
        /// </summary>
        /// <param name="text">Text to append</param>
        private string GetLastLine()
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                if (!_paragraph.Inlines.Any())
                    return null;

                var last = _paragraph.Inlines.LastInline as Run;
                return last.Text;
            }
            else
            {
                return Dispatcher.Invoke(() =>
                {
                    if (!_paragraph.Inlines.Any())
                        return null;

                    var last = _paragraph.Inlines.LastInline as Run;
                    return last.Text;
                });
            }
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

            // Get thr brush color from the flags
            Brush brush = Brushes.White;
            if (error)
                brush = Brushes.Red;
            else if (verbose)
                brush = Brushes.Yellow;

            // Get last line
            if (lastLineText == null)
                lastLineText = GetLastLine();

            // Always append if there's no previous line
            if (lastLineText == null)
            {
                AppendToTextBox(text, brush);
                lastUsedMatcher = null;
            }
            // Return always means overwrite
            else if (text.StartsWith("\r"))
            {
                ReplaceLastLine(text, brush);
                lastUsedMatcher = null;
            }
            else
            {
                // If we have a cached matcher, try it first
                if (lastUsedMatcher?.Matches(ref text) == true)
                {
                    ReplaceLastLine(text, brush);
                }
                else
                {
                    // Get all matching Matchers
                    var matches = _matchers.Where(m => m.Matches(ref text));
                    if (matches.Any())
                    {
                        // Use the first Matcher
                        var firstMatcher = matches.First();
                        if (firstMatcher.Matches(ref lastLineText))
                            ReplaceLastLine(text, brush);
                        else if (string.IsNullOrWhiteSpace(lastLineText))
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
            }

            // Update the bar if needed
            ProcessStringForProgressBar(text);

            // Cache the current text as the last line
            lastLineText = text;
        }

        /// <summary>
        /// Process text if it should update the progress bar
        /// </summary>
        /// <param name="text">Text to check and update with</param>
        private void ProcessStringForProgressBar(string text)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                var matcher = _matchers.FirstOrDefault(m => m.Matches(ref text));
                matcher.Apply(ref text);
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    var matcher = _matchers.FirstOrDefault(m => m.Matches(ref text));
                    matcher.Apply(ref text);
                });
            }
        }

        /// <summary>
        /// Replace the last line written to the log text box
        /// </summary>
        /// <param name="text">Text to append</param>
        /// <param name="color">Color to print text in</param>
        private void ReplaceLastLine(string text, Brush color)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                var last = _paragraph.Inlines.LastInline as Run;
                last.Text = text;
                last.Foreground = color;
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    var last = _paragraph.Inlines.LastInline as Run;
                    last.Text = text;
                    last.Foreground = color;
                });
            }
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

        #endregion
    }
}
