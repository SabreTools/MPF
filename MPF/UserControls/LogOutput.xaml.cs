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
        private FlowDocument _document;
        private Paragraph _paragraph;
        private readonly List<Matcher> _matchers;

        public LogOutput()
        {
            InitializeComponent();
            DataContext = this;

            _document = new FlowDocument();
            _paragraph = new Paragraph();
            _document.Blocks.Add(_paragraph);
            Output.Document = _document;

            _matchers = new List<Matcher>();
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
        /// Add all Matchers for DiscImageCreator
        /// </summary>
        private void AddDiscImageCreatorMatchers()
        {
            _matchers.Add(new Matcher(
                   "Descrambling data sector of img (LBA)",
                   @"\s*(\d+)\/\s*(\d+)$",
                   match => {
                       if (UInt32.TryParse(match.Groups[1].Value, out uint current) && UInt32.TryParse(match.Groups[2].Value, out uint total))
                       {
                           float percentProgress = (current / (float)total) * 100;
                           ProgressBar.Value = percentProgress;
                           ProgressLabel.Text = string.Format("Descrambling image... ({0:##.##}%)", percentProgress);
                       }
                   }));

            _matchers.Add(new Matcher(
                @"Creating .scm (LBA)",
                @"\s*(\d+)\/\s*(\d+)$",
                match => {
                    if (UInt32.TryParse(match.Groups[1].Value, out uint current) && UInt32.TryParse(match.Groups[2].Value, out uint total))
                    {
                        float percentProgress = (current / (float)total) * 100;
                        ProgressBar.Value = percentProgress;
                        ProgressLabel.Text = string.Format("Creating scrambled image... ({0:##.##}%)", percentProgress);
                    }
                }));

            _matchers.Add(new Matcher(
                @"Creating iso(LBA)",
                @"\s*(\d+)\/\s*(\d+)$",
                match => {
                    if (UInt32.TryParse(match.Groups[1].Value, out uint current) && UInt32.TryParse(match.Groups[2].Value, out uint total))
                    {
                        float percentProgress = (current / (float)total) * 100;
                        ProgressBar.Value = percentProgress;
                        ProgressLabel.Text = string.Format("Creating ISO... ({0:##.##}%)", percentProgress);
                    }
                }));

            _matchers.Add(new Matcher(
                "Checking sectors (LBA)",
                @"\s*(\d+)\/\s*(\d+)$",
                match => {
                    if (UInt32.TryParse(match.Groups[1].Value, out uint current) && UInt32.TryParse(match.Groups[2].Value, out uint total))
                    {
                        float percentProgress = (current / (float)total) * 100;
                        ProgressBar.Value = percentProgress;
                        ProgressLabel.Text = string.Format("Checking for errors... ({0:##.##}%)", percentProgress);
                    }
                }));

            _matchers.Add(new Matcher(
                "Scanning sector (LBA)",
                @"\s*(\d+)\/\s*(\d+)$",
                match => {
                    if (UInt32.TryParse(match.Groups[1].Value, out uint current) && UInt32.TryParse(match.Groups[2].Value, out uint total))
                    {
                        float percentProgress = (current / (float)total) * 100;
                        ProgressBar.Value = percentProgress;
                        ProgressLabel.Text = string.Format("Scanning sectors for protection... ({0:##.##}%)", percentProgress);
                    }
                }));

            _matchers.Add(new Matcher(
                "Checking SubQ ctl (Track)",
                @"\s*(\d+)\/\s*(\d+)$",
                match => {
                    if (UInt32.TryParse(match.Groups[1].Value, out uint current) && UInt32.TryParse(match.Groups[2].Value, out uint total))
                    {
                        float percentProgress = (current / (float)total) * 100;
                        ProgressBar.Value = percentProgress;
                        ProgressLabel.Text = string.Format("Checking subchannels... ({0:##.##}%)", percentProgress);
                    }
                }));
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

            // Print according to the text
            if (text.StartsWith("\r"))
                ReplaceLastLine(text, brush);
            else if (_matchers.Any(m => m.Matches(ref text)))
                ReplaceLastLine(text, brush);
            else
                AppendToTextBox(text, brush);

            // Update the bar if needed
            ProcessStringForProgressBar(text);
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
                var last = _paragraph.Inlines.Last();
                _paragraph.Inlines.Remove(last);
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    var last = _paragraph.Inlines.Last();
                    _paragraph.Inlines.Remove(last);
                });
            }

            AppendToTextBox(text, color);
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
