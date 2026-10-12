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
using Avalonia.Interactivity;
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
    /// a <see cref="TerminalOutputProcessor"/> applies terminal line-discipline so a tool's bare
    /// <c>\r</c> progress redraws collapse onto a single live line instead of flooding the
    /// scrollback. Committed lines feed a virtualized list with a CMD-style fixed cap.
    /// </remarks>
    public partial class ToolOutputWindow : Window
    {
        #region Constants

        /// <summary>
        /// CMD-style fixed scrollback: oldest lines are trimmed past this cap
        /// </summary>
        private const int MaxLines = 10_000;

        /// <summary>
        /// One batched UI dispatch per ~60 fps frame
        /// </summary>
        private const int FlushIntervalMs = 16;

        #endregion

        #region Properties

        /// <summary>
        /// Whether the window should close itself when the tool exits
        /// </summary>
        public bool AutoClose
        {
            get => AutoCloseCheck.IsChecked == true;
            set => AutoCloseCheck.IsChecked = value;
        }

        #endregion

        #region Events

        /// <summary>
        /// Raised when the user toggles the auto-close checkbox, so the host can persist it
        /// </summary>
        public event Action<bool>? AutoCloseChanged;

        #endregion

        #region Private Fields

        /// <summary>
        ///
        /// </summary>
        private readonly ObservableCollection<string> _lines = [];

        /// <summary>
        ///
        /// </summary>
        private readonly TerminalOutputProcessor _buffer = new();

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

        #endregion

        public ToolOutputWindow() : this(autoClose: true) { }

        public ToolOutputWindow(bool autoClose)
        {
            InitializeComponent();

            LinesList.ItemsSource = _lines;
            AutoCloseCheck.IsChecked = autoClose;

            // The pump runs for the lifetime of the window
            _pumpTask = RunPumpAsync(_cts.Token);
        }

        /// <summary>
        /// Push a raw chunk of tool output. Thread-safe: called from reader threads.
        /// </summary>
        public void Append(string chunk) => _pump.TryWrite(chunk);

        /// <summary>
        /// The tool process exited: flush the remaining output, then close if the user
        /// opted in. Safe to call on the UI thread.
        /// </summary>
        public async void NotifyToolExited()
        {
            // Complete the pump so its final drain flushes the tail to the UI, then wait
            // for that drain to finish before deciding whether to close.
            _pump.TryComplete();
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

        #region Event Handlers

        /// <summary>
        /// Changed handler for AutoCloseCheck
        /// </summary>
        private void AutoCloseCheckChanged(object? sender, RoutedEventArgs e)
            => AutoCloseChanged?.Invoke(AutoCloseCheck.IsChecked == true);

        /// <summary>
        /// Click handler for CopyMenuItem
        /// </summary>
        private void CopyMenuItemClick(object? sender, RoutedEventArgs e)
            => CopySelectionToClipboard();

        /// <summary>
        /// Runs on the consumer thread after a batch was fed to the buffer. Snapshot the
        /// new committed lines + the current live line, then do exactly ONE dispatch to
        /// the UI thread for the whole batch.
        /// </summary>
        private void OnBatch()
        {
            List<string> newLines = [];
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

                foreach (string line in newLines)
                {
                    _lines.Add(line);
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
        /// Best-effort shutdown; if the tool is still running its output is simply
        /// dropped from here on (Post on a completed channel is a no-op)
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            _cts.Cancel();
            _pump.TryComplete();
            base.OnClosed(e);
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
        /// Tail-follow is toggled ONLY by genuine user input: the scrollbar's Scroll event
        /// (thumb drag / track / page) and the mouse wheel. Neither fires for the
        /// programmatic ScrollToEnd or for the ring-buffer trim -- so following never drops
        /// on its own under fast output. Position then decides direction: at/near the
        /// bottom = follow, else not.
        /// </summary>
        private void OnUserScroll(object? sender, ScrollEventArgs e)
            => SyncFollowToPosition();

        /// <summary>
        ///The wheel scroll is applied after this event fires; check once it has
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWheel(object? sender, PointerWheelEventArgs e)
            => Dispatcher.UIThread.Post(SyncFollowToPosition, DispatcherPriority.Background);

        /// <summary>
        /// Click handler for SelectAllMenuItem
        /// </summary>
        private void SelectAllMenuItemClick(object? sender, RoutedEventArgs e)
            => LinesList.SelectAll();

        #endregion

        #region Helpers

        /// <summary>
        /// Copies the selected rows to the clipboard, newline-joined and in top-to-bottom
        /// (source) order regardless of the order they were clicked.
        /// </summary>
        private async void CopySelectionToClipboard()
        {
            IReadOnlyList<int> selected = LinesList.Selection?.SelectedIndexes ?? [];
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
        /// Run message pump async until cancelled
        /// </summary>
        private async Task RunPumpAsync(CancellationToken ct)
        {
            try
            {
                while (await _pump.WaitToReadAsync(ct).ConfigureAwait(false))
                {
                    List<string> batch = _pump.DrainAvailable();
                    if (batch.Count > 0)
                    {
                        foreach (string chunk in batch)
                        {
                            _buffer.Process(chunk);
                        }

                        OnBatch();
                    }

                    if (FlushIntervalMs > 0)
                        await Task.Delay(FlushIntervalMs, ct).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected on shutdown
            }

            // Final drain after completion so the tail is not lost.
            List<string> tail = _pump.DrainAvailable();
            foreach (string chunk in tail)
            {
                _buffer.Process(chunk);
            }

            _buffer.Flush();
            OnBatch();
        }

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

        #endregion
    }
}
