using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DICUI
{
    public partial class LogWindow : Window
    {
        private FlowDocument _document;
        private Paragraph _paragraph;

        public LogWindow()
        {
            InitializeComponent();

            _document = new FlowDocument();
            _paragraph = new Paragraph();
            _document.Blocks.Add(_paragraph);
            output.Document = _document;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() =>
            {
                _cmd = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = @"Programs/DiscImageCreator.exe",
                        Arguments = "cd e Gam.iso 16",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                    },
                };

                StreamState stdoutState = new StreamState(false);
                StreamState stderrState = new StreamState(true);

                //_cmd.ErrorDataReceived += (process, text) => Dispatcher.Invoke(() => UpdateConsole(text.Data, Brushes.Red));
                _cmd.Start();

                var _1 = ConsumeOutput(_cmd.StandardOutput, s => Dispatcher.Invoke(() => UpdateConsole(s, stdoutState)));
                var _2 = ConsumeOutput(_cmd.StandardError, s => Dispatcher.Invoke(() => UpdateConsole(s, stderrState)));

                _cmd.EnableRaisingEvents = true;
                _cmd.Exited += OnProcessExit;
            });
        }

        private void GracefullyTerminateProcess()
        {
            if (_cmd != null)
            {
                if (!_cmd.HasExited)
                {
                    AppendToTextBox("\r\nForcefully Killing the process\r\n", Brushes.Red);
                    _cmd.Kill();
                }

                AppendToTextBox(string.Format("\r\nExit Code: {0}\r\n", _cmd.ExitCode), Brushes.Red);
                _cmd.Exited -= OnProcessExit;
                _cmd.Close();

            }

            _cmd = null;
        }

        private void ScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            outputViewer.ScrollToBottom();
        }

        async Task ConsumeOutput(TextReader reader, Action<string> callback)
        {
            char[] buffer = new char[256];
            int cch;

            while ((cch = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                callback(new string(buffer, 0, cch));
            }
        }

        private Regex createdImgRegex = new Regex(@"^Created\ img\ \(LBA\)\s+(\d+)\/(\d+)$");
        private void ProcessStringForProgressBar(string text)
        {
            if (text.StartsWith("Created img"))
            {
                var match = createdImgRegex.Match(text);

                if (match.Success && UInt32.TryParse(match.Groups[1].Value, out uint current) && UInt32.TryParse(match.Groups[2].Value, out uint total))
                {
                    float percentProgress = (current / (float)total) * 100;
                    progressBar.Value = percentProgress;
                    progressLabel.Text = string.Format("Creating IMG ({0:##.##}%)", percentProgress);
                }
            }
        }

        class StreamState
        {
            public enum State
            {
                BEGIN,
                READ_CARRIAGE,
            };

            public State state;
            public readonly StringBuilder buffer;
            public readonly bool isError;

            public StreamState(bool isError)
            {
                this.state = State.BEGIN;
                this.buffer = new StringBuilder();
                this.isError = isError;
            }

            public bool HasData() => buffer.Length > 0;
            public string Fetch() => buffer.ToString();
            public void Clear() => buffer.Clear();
            public void Append(char c) => buffer.Append(c);

            public bool Is(State state) => this.state == state;
            public void Set(State state) => this.state = state;
        }

        private void AppendToTextBox(string text, Brush color)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                Run run = new Run(text) { Foreground = color };
                _paragraph.Inlines.Add(run);
            }
            else
                Dispatcher.Invoke(() =>
                {
                    Run run = new Run(text) { Foreground = color };
                    _paragraph.Inlines.Add(run);
                });
        }

        private void UpdateConsole(string text, StreamState state)
        {
            /*if (c == '\r') { output.Inlines.Add(@"\r"); file.Write("\\r"); }
                else if (c == '\n') { output.Inlines.Add(@"\n"); file.Write("\\n\n"); }
                output.Inlines.Add(""+c);
                file.Write(c);
                file.Flush();
                continue;*/

            if (text != null)
            {
                foreach (char c in text)
                {
                    switch (c)
                    {
                        case '\r' when (state.Is(StreamState.State.BEGIN)):
                            {
                                state.Set(StreamState.State.READ_CARRIAGE);
                                break;
                            }

                        case '\n' when (state.Is(StreamState.State.READ_CARRIAGE)):
                            {
                                if (state.buffer.Length > 0)
                                {
                                    string buffer = state.Fetch();

                                    AppendToTextBox(buffer, state.isError ? Brushes.Red : Brushes.White);

                                    if (!state.isError)
                                        ProcessStringForProgressBar(buffer);
                                }
                                _paragraph.Inlines.Add(new LineBreak());
                                state.Clear();
                                state.Set(StreamState.State.BEGIN);
                                break;
                            }

                        default:
                            if (state.Is(StreamState.State.READ_CARRIAGE) && state.HasData())
                            {
                                if (!(_paragraph.Inlines.LastInline is LineBreak))
                                    _paragraph.Inlines.Remove(_paragraph.Inlines.LastInline);

                                string buffer = state.Fetch();

                                AppendToTextBox(buffer, state.isError ? Brushes.Red : Brushes.White);

                                if (!state.isError)
                                    ProcessStringForProgressBar(buffer);

                                state.Clear();
                            }

                            state.Set(StreamState.State.BEGIN);
                            state.Append(c);
                            break;

                    }
                }
            }
        }

        #region EventHandlers

        private void OnWindowClose(object sender, EventArgs e)
        {
            GracefullyTerminateProcess();
        }

        void OnProcessExit(object sender, EventArgs e)
        {
            if (_cmd.ExitCode != 0)
            {
                Dispatcher.Invoke(() =>
                {
                    progressLabel.Foreground = Brushes.White;
                    progressLabel.Text = "Error occurred! Please check log.";

                    progressBar.Value = 100;
                    progressBar.Foreground = Brushes.Red;
                });
            }

            Dispatcher.Invoke(() => btnAbort.IsEnabled = false);

            GracefullyTerminateProcess();
        }

        private void OnAbortButton(object sender, EventArgs args)
        {
            GracefullyTerminateProcess();
        }

        #endregion

        Process _cmd;
    }
}
