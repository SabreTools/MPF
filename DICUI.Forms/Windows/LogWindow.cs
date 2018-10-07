using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;

namespace DICUI.Forms.Windows
{
    public partial class LogWindow : Form
    {
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private MainWindow _mainWindow;

        private List<Matcher> _matchers;

        volatile Process _process;

        public LogWindow(MainWindow mainWindow)
        {
            InitializeComponent();

            this._mainWindow = mainWindow;

            _matchers = new List<Matcher>();

            _matchers.Add(new Matcher(
                "Descrambling data sector of img (LBA)",
                @"\s*(\d+)\/\s*(\d+)$",
                match => {
                    if (UInt32.TryParse(match.Groups[1].Value, out uint current) && UInt32.TryParse(match.Groups[2].Value, out uint total))
                    {
                        float percentProgress = (current / (float)total) * 100;
                        progressBar.Value = (int)percentProgress;
                        //progressBar.Text = string.Format("Descrambling image.. ({0:##.##}%)", percentProgress);
                    }
                }));

            _matchers.Add(new Matcher(
                @"Creating .scm (LBA)",
                @"\s*(\d+)\/\s*(\d+)$",
                match => {
                    if (UInt32.TryParse(match.Groups[1].Value, out uint current) && UInt32.TryParse(match.Groups[2].Value, out uint total))
                    {
                        float percentProgress = (current / (float)total) * 100;
                        progressBar.Value = (int)percentProgress;
                        //progressBar.Text = string.Format("Creating scrambled image.. ({0:##.##}%)", percentProgress);
                    }
                }));

            _matchers.Add(new Matcher(
                "Checking sectors (LBA)",
                @"\s*(\d+)\/\s*(\d+)$",
                match => {
                    if (UInt32.TryParse(match.Groups[1].Value, out uint current) && UInt32.TryParse(match.Groups[2].Value, out uint total))
                    {
                        float percentProgress = (current / (float)total) * 100;
                        progressBar.Value = (int)percentProgress;
                        //progressBar.Text = string.Format("Checking for errors.. ({0:##.##}%)", percentProgress);
                    }
                }));

            _matchers.Add(new Matcher(
                "Scanning sector (LBA)",
                @"\s*(\d+)\/\s*(\d+)$",
                match => {
                    if (UInt32.TryParse(match.Groups[1].Value, out uint current) && UInt32.TryParse(match.Groups[2].Value, out uint total))
                    {
                        float percentProgress = (current / (float)total) * 100;
                        progressBar.Value = (int)percentProgress;
                        //progressBar.Text = string.Format("Scanning sectors for protection.. ({0:##.##}%)", percentProgress);
                    }
                }));
        }

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        public void StartDump(string args)
        {
            AppendToTextBox(string.Format("Launching DIC with args: {0}\r\n", args), System.Drawing.Color.Orange);

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

                //_cmd.ErrorDataReceived += (process, text) => Dispatcher.CurrentDispatcher.Invoke(() => UpdateConsole(text.Data, Brushes.Red));
                _process.Start();

                var _1 = ConsumeOutput(_process.StandardOutput, s => Dispatcher.CurrentDispatcher.Invoke(() => UpdateConsole(s, stdoutState)));
                var _2 = ConsumeOutput(_process.StandardError, s => Dispatcher.CurrentDispatcher.Invoke(() => UpdateConsole(s, stderrState)));

                _process.EnableRaisingEvents = true;
                _process.Exited += OnProcessExit;
            });
        }

        public void AdjustPositionToMainWindow()
        {
            this.Left = _mainWindow.Left;
            this.Top = _mainWindow.Top + _mainWindow.Height + Constants.LogWindowMarginFromMainWindow;
            this.BringToFront();
        }

        private void GracefullyTerminateProcess()
        {
            if (_process != null)
            {
                _process.Exited -= OnProcessExit;
                bool isForced = !_process.HasExited;

                if (isForced)
                {
                    AppendToTextBox("\r\nForcefully Killing the process\r\n", System.Drawing.Color.Red);
                    _process.Kill();
                    _process.WaitForExit();
                }

                AppendToTextBox(string.Format("\r\nExit Code: {0}\r\n", _process.ExitCode), _process.ExitCode == 0 ? System.Drawing.Color.Green : System.Drawing.Color.Red);
                if (_process.ExitCode == 0)
                {
                    Dispatcher.CurrentDispatcher.Invoke(() =>
                    {
                        //progressBar.Text = "Done!";
                        progressBar.Value = 100;
                        progressBar.ForeColor = System.Drawing.Color.Green;
                    });
                }
                else
                {
                    Dispatcher.CurrentDispatcher.Invoke(() =>
                    {
                        //progressBar.Text = isForced ? "Aborted by user" : "Error, please check log!";
                        progressBar.Value = 100;
                        progressBar.ForeColor = System.Drawing.Color.Red;
                    });
                }

                _process.Close();
            }

            _process = null;
        }

        private void ScrollViewer_SizeChanged(object sender, EventArgs e)
        {
            output.ScrollToCaret();
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

        public void AppendToTextBox(string text, System.Drawing.Color color)
        {
            if (Dispatcher.CurrentDispatcher.CheckAccess())
            {
                output.ForeColor = color;
                output.AppendText(text);
            }
            else
                Dispatcher.CurrentDispatcher.Invoke(() =>
                {
                    output.ForeColor = color;
                    output.AppendText(text);
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

                                    AppendToTextBox(buffer, state.isError ? System.Drawing.Color.Red : System.Drawing.Color.White);

                                    if (!state.isError)
                                        ProcessStringForProgressBar(buffer);
                                }
                                output.AppendText(Environment.NewLine);
                                state.Clear();
                                state.Set(StreamState.State.BEGIN);
                                break;
                            }

                        default:
                            if (state.Is(StreamState.State.READ_CARRIAGE) && state.HasData())
                            {
                                if (!String.IsNullOrEmpty(output.Lines.Last()))
                                    output.Lines[output.Lines.Length] = string.Empty;

                                string buffer = state.Fetch();

                                AppendToTextBox(buffer, state.isError ? System.Drawing.Color.Red : System.Drawing.Color.White);

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

        private void OnWindowClosed(object sender, FormClosedEventArgs e)
        {
            GracefullyTerminateProcess();
        }

        private void OnWindowLoaded(object sender, EventArgs e)
        {
            var hwnd = this.Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }

        void OnProcessExit(object sender, EventArgs e)
        {
            //Dispatcher.CurrentDispatcher.Invoke(() => AbortButton.IsEnabled = false);
            GracefullyTerminateProcess();
        }

        private void OnHideButton(object sender, EventArgs e)
        {
            ViewModels.LoggerViewModel.WindowVisible = false;
            //TODO: this should be bound directly to WindowVisible property in two way fashion
            // we need to study how to properly do it in XAML
            _mainWindow.showLogWindowToolStripMenuItem.Checked = false;
        }

        private void OnClearButton(object sender, EventArgs e)
        {
            output.Clear();
        }

        private void OnAbortButton(object sender, EventArgs args)
        {
            GracefullyTerminateProcess();
        }

        private void OnStartButton(object sender, EventArgs args)
        {
            StartDump("cd e Gam.iso 16");
        }

        private void OnTextChanged(object sender, EventArgs e)
        {
            output.ScrollToCaret();
        }

        #endregion
    }
}
