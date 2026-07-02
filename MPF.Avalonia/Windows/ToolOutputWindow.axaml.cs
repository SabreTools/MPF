using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MPF.Avalonia.Helpers;

namespace MPF.Avalonia.Windows
{
    /// <summary>
    /// Separate window that streams a dumping tool's live standard output and error.
    /// </summary>
    /// <remarks>
    /// Shown modeless (via <c>Show(owner)</c>) so the MPF main window never freezes and
    /// its Stop button stays reachable. Raw output chunks arrive on <see cref="Append"/>
    /// from background reader threads; a bounded <see cref="OutputPump"/> batches them and
    /// a <see cref="TerminalBuffer"/> applies terminal line-discipline so a tool's bare
    /// <c>\r</c> progress redraws collapse onto a single live line instead of flooding the
    /// scrollback. Committed lines feed a virtualized list with a CMD-style fixed cap.
    /// </remarks>
    public partial class ToolOutputWindow : Window
    {
        /// <summary>
        /// CMD-style fixed scrollback: oldest lines are trimmed past this cap
        /// </summary>
        private const int MaxLines = 10_000;

        /// <summary>
        /// One batched UI dispatch per ~60 fps frame
        /// </summary>
        private const int FlushIntervalMs = 16;

        /// <summary>
        ///
        /// </summary>
        private readonly ObservableCollection<string> _lines = [];

        /// <summary>
        ///
        /// </summary>
        private readonly TerminalBuffer _buffer = new();

        /// <summary>
        ///
        /// </summary>
        private readonly OutputPump _pump = new(capacity: 8192);

        /// <summary>
        ///
        /// </summary>
        private readonly CancellationTokenSource _cts = new();

        /// <summary>
        ///
        /// </summary>
        private readonly Task _pumpTask;

        /// <summary>
        /// Committed lines already pushed to the UI
        /// </summary>
        private int _committedIndex;

        /// <summary>
        /// Follow the tail; toggled only by user input
        /// </summary>
        private bool _autoScroll = true;

        /// <summary>
        /// Coalesces deferred ScrollToEnd posts
        /// </summary>
        private bool _scrollQueued;

        /// <summary>
        /// Vertical scrollbar Scroll event subscribed
        /// </summary>
        private bool _scrollBarHooked;

        /// <summary>
        /// ListBox's inner scroll viewer (found lazily)
        /// </summary>
        private ScrollViewer? _scroller;

        /// <summary>
        /// Raised when the user toggles the auto-close checkbox, so the host can persist it
        /// </summary>
        public event Action<bool>? AutoCloseChanged;

        public ToolOutputWindow() : this(autoClose: true) { }

        public ToolOutputWindow(bool autoClose)
        {
            InitializeComponent();

            LinesList.ItemsSource = _lines;

            CopyMenuItem.Click += (_, _) => CopySelectionToClipboard();
            SelectAllMenuItem.Click += (_, _) => LinesList.SelectAll();
            // Window-level so Ctrl+C / Ctrl+A work without giving the (deliberately
            // non-focusable) log rows keyboard focus.
            KeyDown += OnKeyDown;

            AutoCloseCheck.IsChecked = autoClose;
            AutoCloseCheck.IsCheckedChanged += (_, _) => AutoCloseChanged?.Invoke(AutoCloseCheck.IsChecked == true);

            // The pump runs for the lifetime of the window, draining whatever Append posts.
            _pumpTask = _pump.RunAsync(_buffer, OnBatch, FlushIntervalMs, _cts.Token);
        }

        /// <summary>
        /// Whether the window should close itself when the tool exits
        /// </summary>
        public bool AutoClose
        {
            get => AutoCloseCheck.IsChecked == true;
            set => AutoCloseCheck.IsChecked = value;
        }

        /// <summary>
        /// Push a raw chunk of tool output. Thread-safe: called from reader threads.
        /// </summary>
        public void Append(string chunk) => _pump.Post(chunk);

        /// <summary>
        /// The tool process exited: flush the remaining output, then close if the user
        /// opted in. Safe to call on the UI thread.
        /// </summary>
        public async void NotifyToolExited()
        {
            // Complete the pump so its final drain flushes the tail to the UI, then wait
            // for that drain to finish before deciding whether to close.
            _pump.Complete();
            try
            {
                await _pumpTask;
            }
            catch
            {
                // shutdown races are fine
            }

            if (AutoCloseCheck.IsChecked == true)
                Close();
        }

        /// <summary>
        /// Ctrl+C / context-menu Copy: copy the selected log lines as plain text.
        /// Ctrl+A selects every committed line.
        /// </summary>
        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyModifiers.HasFlag(KeyModifiers.Control) && e.Key == Key.C)
            {
                CopySelectionToClipboard();
                e.Handled = true;
            }
            else if (e.KeyModifiers.HasFlag(KeyModifiers.Control) && e.Key == Key.A)
            {
                LinesList.SelectAll();
                e.Handled = true;
            }
        }

        /// <summary>
        /// Copies the selected rows to the clipboard, newline-joined and in top-to-bottom
        /// (source) order regardless of the order they were clicked.
        /// </summary>
        private async void CopySelectionToClipboard()
        {
            IReadOnlyList<int> selected = LinesList.Selection?.SelectedIndexes ?? Array.Empty<int>();
            if (selected.Count == 0)
                return;

            var sb = new StringBuilder(selected.Count * 48);
            foreach (int idx in selected.OrderBy(i => i))
            {
                if (idx >= 0 && idx < _lines.Count)
                    sb.Append(_lines[idx]);

                sb.Append('\n');
            }

            if (sb.Length > 0 && sb[^1] == '\n')
                sb.Length -= 1; // drop the trailing newline

            IClipboard? clipboard = GetTopLevel(this)?.Clipboard;
            if (clipboard is not null)
                await clipboard.SetTextAsync(sb.ToString());
        }

        /// <summary>
        /// Runs on the consumer thread after a batch was fed to the buffer. Snapshot the
        /// new committed lines + the current live line, then do exactly ONE dispatch to
        /// the UI thread for the whole batch.
        /// </summary>
        private void OnBatch(int consumed)
        {
            List<string>? newLines = null;
            List<string> committed = _buffer.CommittedLines;
            if (_committedIndex < committed.Count)
            {
                newLines = new List<string>(committed.Count - _committedIndex);
                for (; _committedIndex < committed.Count; _committedIndex++)
                {
                    newLines.Add(committed[_committedIndex]);
                }
            }

            string live = _buffer.CurrentLine;

            Dispatcher.UIThread.Post(() =>
            {
                EnsureScroller();

                if (newLines is not null)
                {
                    foreach (string line in newLines)
                    {
                        _lines.Add(line);
                    }
                }

                // CMD-style fixed scrollback: always drop the oldest lines past the cap.
                while (_lines.Count > MaxLines)
                {
                    _lines.RemoveAt(0);
                }

                LiveLine.Text = live;

                if (_autoScroll && _scroller is not null && !_scrollQueued)
                {
                    _scrollQueued = true;
                    // Defer past the layout pass so the extent is fresh and we land on the bottom.
                    Dispatcher.UIThread.Post(() =>
                    {
                        _scrollQueued = false;
                        if (_autoScroll)
                            _scroller!.ScrollToEnd();
                    }, DispatcherPriority.Background);
                }
            });
        }

        /// <summary>
        /// Lazily grab the ListBox's inner ScrollViewer (only exists after the control is
        /// templated) and subscribe to its scroll changes for tail-follow tracking.
        /// </summary>
        private void EnsureScroller()
        {
            if (_scroller is null)
            {
                _scroller = LinesList.FindDescendantOfType<ScrollViewer>();
                if (_scroller is not null)
                    LinesList.PointerWheelChanged += OnWheel;
            }

            // The vertical ScrollBar realizes slightly later than the ScrollViewer.
            if (_scroller is not null && !_scrollBarHooked)
            {
                ScrollBar? vbar = _scroller.GetVisualDescendants()
                    .OfType<ScrollBar>()
                    .FirstOrDefault(sb => sb.Orientation == Orientation.Vertical);
                if (vbar is not null)
                {
                    vbar.Scroll += OnUserScroll;
                    _scrollBarHooked = true;
                }
            }
        }

        /// <summary>
        /// Tail-follow is toggled ONLY by genuine user input: the scrollbar's Scroll event
        /// (thumb drag / track / page) and the mouse wheel. Neither fires for the
        /// programmatic ScrollToEnd or for the ring-buffer trim -- so following never drops
        /// on its own under fast output. Position then decides direction: at/near the
        /// bottom = follow, else not.
        /// </summary>
        private void OnUserScroll(object? sender, ScrollEventArgs e)
            => SyncFollowToPosition();

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWheel(object? sender, PointerWheelEventArgs e)
            // The wheel scroll is applied after this event fires; check once it has.
            => Dispatcher.UIThread.Post(SyncFollowToPosition, DispatcherPriority.Background);

        /// <summary>
        ///
        /// </summary>
        private void SyncFollowToPosition()
        {
            if (_scroller is null)
                return;

            double distanceFromBottom = _scroller.Extent.Height - _scroller.Viewport.Height - _scroller.Offset.Y;
            _autoScroll = distanceFromBottom <= 4;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            // Best-effort shutdown; if the tool is still running its output is simply
            // dropped from here on (Post on a completed channel is a no-op).
            _cts.Cancel();
            _pump.Complete();
            base.OnClosed(e);
        }
    }
}
