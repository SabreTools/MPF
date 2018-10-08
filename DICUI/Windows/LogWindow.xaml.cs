using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Media;

namespace DICUI.Windows
{
    public partial class LogWindow : Window
    {
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private MainWindow _mainWindow;

        private FlowDocument _document;
        private Paragraph _paragraph;
        private List<Matcher> _matchers;

        volatile Process _process;

        public LogWindow(MainWindow mainWindow)
        {
            InitializeComponent();

            this._mainWindow = mainWindow;

            _document = new FlowDocument();
            _paragraph = new Paragraph();
            _document.Blocks.Add(_paragraph);
            output.Document = _document;

            _matchers = new List<Matcher>();

            _matchers.Add(new Matcher(
                "Descrambling data sector of img (LBA)",
                @"\s*(\d+)\/\s*(\d+)$",
                match => {
                    if (UInt32.TryParse(match.Groups[1].Value, out uint current) && UInt32.TryParse(match.Groups[2].Value, out uint total))
                    {
                        float percentProgress = (current / (float)total) * 100;
                        progressBar.Value = percentProgress;
                        progressLabel.Text = string.Format("Descrambling image.. ({0:##.##}%)", percentProgress);
                    }
            }));

            _matchers.Add(new Matcher(
                @"Creating .scm (LBA)",
                @"\s*(\d+)\/\s*(\d+)$",
                match => {
                    if (UInt32.TryParse(match.Groups[1].Value, out uint current) && UInt32.TryParse(match.Groups[2].Value, out uint total))
                    {
                        float percentProgress = (current / (float)total) * 100;
                        progressBar.Value = percentProgress;
                        progressLabel.Text = string.Format("Creating scrambled image.. ({0:##.##}%)", percentProgress);
                    }
            }));

            _matchers.Add(new Matcher(
                "Checking sectors (LBA)",
                @"\s*(\d+)\/\s*(\d+)$",
                match => {
                    if (UInt32.TryParse(match.Groups[1].Value, out uint current) && UInt32.TryParse(match.Groups[2].Value, out uint total))
                    {
                        float percentProgress = (current / (float)total) * 100;
                        progressBar.Value = percentProgress;
                        progressLabel.Text = string.Format("Checking for errors.. ({0:##.##}%)", percentProgress);
                    }
            }));

            _matchers.Add(new Matcher(
                "Scanning sector (LBA)",
                @"\s*(\d+)\/\s*(\d+)$",
                match => {
                    if (UInt32.TryParse(match.Groups[1].Value, out uint current) && UInt32.TryParse(match.Groups[2].Value, out uint total))
                    {
                        float percentProgress = (current / (float)total) * 100;
                        progressBar.Value = percentProgress;
                        progressLabel.Text = string.Format("Scanning sectors for protection.. ({0:##.##}%)", percentProgress);
                    }
                }));
        }

        public void StartDump(string args)
        {
            AppendToTextBox(string.Format("Launching DIC with args: {0}\r\n", args), Brushes.Orange);

            Task.Run(() =>
            {
                _process = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = @"Programs/DiscImageCreator.exe",
                        Arguments = args,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                    },
                };

                StreamState stdoutState = new StreamState(false);
                StreamState stderrState = new StreamState(true);

                //_cmd.ErrorDataReceived += (process, text) => Dispatcher.Invoke(() => UpdateConsole(text.Data, Brushes.Red));
                _process.Start();

                var _1 = ConsumeOutput(_process.StandardOutput, s => Dispatcher.Invoke(() => UpdateConsole(s, stdoutState)));
                var _2 = ConsumeOutput(_process.StandardError, s => Dispatcher.Invoke(() => UpdateConsole(s, stderrState)));

                _process.EnableRaisingEvents = true;
                _process.Exited += OnProcessExit;
            });
        }

        public void AdjustPositionToMainWindow()
        {
            this.Left = _mainWindow.Left;
            this.Top = _mainWindow.Top + _mainWindow.Height + Constants.LogWindowMarginFromMainWindow;
        }

        private void GracefullyTerminateProcess()
        {
            if (_process != null)
            {
                _process.Exited -= OnProcessExit;
                bool isForced = !_process.HasExited;

                if (isForced)
                {
                    AppendToTextBox("\r\nForcefully Killing the process\r\n", Brushes.Red);
                    _process.Kill();
                    _process.WaitForExit();
                }

                AppendToTextBox(string.Format("\r\nExit Code: {0}\r\n", _process.ExitCode), _process.ExitCode == 0 ? Brushes.Green : Brushes.Red);

                if (_process.ExitCode == 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        progressLabel.Text = "Done!";
                        progressBar.Value = 100;
                        progressBar.Foreground = Brushes.Green;
                    });
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        progressLabel.Text = isForced ? "Aborted by user" : "Error, please check log!";
                        progressBar.Value = 100;
                        progressBar.Foreground = Brushes.Red;
                    });
                }

                _process.Close();
            }

            _process = null;
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

        // this is used to optimize the work since we need to process A LOT of text
        struct Matcher
        {
            private readonly String prefix;
            private readonly Regex regex;
            private readonly int start;
            private readonly Action<Match> lambda;

            public Matcher(String prefix, String regex, Action<Match> lambda)
            {
                this.prefix = prefix;
                this.regex = new Regex(regex);
                this.start = prefix.Length;
                this.lambda = lambda;
            }

            public bool Matches(ref string text) => text.StartsWith(prefix);

            public void Apply(ref string text)
            {
                Match match = regex.Match(text, start);
                lambda.Invoke(match);
            }
        }

        private void ProcessStringForProgressBar(string text)
        {
            foreach (Matcher matcher in _matchers)
            {
                if (matcher.Matches(ref text))
                {
                    matcher.Apply(ref text);
                    return;
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

        public void AppendToTextBox(string text, Brush color)
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

        private void OnWindowClosed(object sender, EventArgs e)
        {
            GracefullyTerminateProcess();
        }

        private void OnWindowLoaded(object sender, EventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }

        void OnProcessExit(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => AbortButton.IsEnabled = false);
            GracefullyTerminateProcess();
        }

        private void OnHideButton(object sender, EventArgs e)
        {
            ViewModels.LoggerViewModel.WindowVisible = false;
            //TODO: this should be bound directly to WindowVisible property in two way fashion
            // we need to study how to properly do it in XAML
            _mainWindow.ShowLogMenuItem.IsChecked = false;
        }

        private void OnClearButton(object sender, EventArgs e)
        {
            output.Document.Blocks.Clear();
        }

        private void OnAbortButton(object sender, EventArgs args)
        {
            GracefullyTerminateProcess();
        }

        private void OnStartButton(object sender, EventArgs args)
        {
            StartDump("cd e Gam.iso 16");
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            outputViewer.ScrollToBottom();
        }

        #endregion

    }
}
