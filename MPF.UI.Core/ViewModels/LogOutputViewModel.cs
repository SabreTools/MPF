using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
using MPF.Core.Data;

namespace MPF.UI.Core.ViewModels
{
    public class LogOutputViewModel
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

        public LogOutputViewModel()
        {
            // Update the internal state
            Document = new FlowDocument()
            {
                Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x20, 0x20, 0x20))
            };
            _paragraph = new Paragraph();
            Document.Blocks.Add(_paragraph);

            // Setup the processing queue
            LogQueue = new ProcessingQueue<LogLine>(ProcessLogLine);
        }

        /// <summary>
        /// Clear all inlines of the paragraph
        /// </summary>
        public void ClearInlines() => _paragraph.Inlines.Clear();

        /// <summary>
        /// Save all inlines to console.log
        /// </summary>
        public void SaveInlines()
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
        /// Log line wrapper
        /// </summary>
        internal struct LogLine
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
        /// Enqueue text to the log with formatting
        /// </summary>
        /// <param name="text">Text to write to the log</param>
        /// <param name="logLevel">LogLevel for the log</param>
        internal void LogInternal(string text, LogLevel logLevel)
        {
            // Null text gets ignored
            if (text == null)
                return;

            // Enqueue the text
            LogQueue.Enqueue(new LogLine(text, logLevel));
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
            Task.Run(() =>
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
            Task.Run(() =>
            {
                lastLine.Text = logLine.Text;
                lastLine.Foreground = logLine.GetForegroundColor();
            });
        }
    }
}
