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

            logQueue = new ProcessingQueue<LogLine>(ProcessLogLine);
        }

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

        #endregion
    }
}
