using System;
using System.Collections.ObjectModel;
using System.IO;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using MPF.Avalonia.Services;
using MPF.Frontend;

namespace MPF.Avalonia.UserControls
{
    public partial class LogOutput : UserControl
    {
        /// <summary>
        /// Default height of the console surface
        /// </summary>
        public const double DefaultConsoleHeight = 180;

        /// <summary>
        /// Maximum number of entries before trimming
        /// </summary>
        private const int MaxEntryCount = 5000;

        /// <summary>
        /// Collection of log entries bound to the output list
        /// </summary>
        public ObservableCollection<LogEntry> Entries { get; } = [];

        public LogOutput()
        {
            InitializeComponent();
            DataContext = this;

            // Add handlers
            this.FindControl<Button>("ClearButton")!.Click += OnClearButton;
            this.FindControl<Button>("SaveButton")!.Click += OnSaveButton;
        }

        #region Logging

        /// <summary>
        /// Enqueue text to the log with formatting
        /// </summary>
        /// <param name="logLevel">LogLevel for the log</param>
        /// <param name="text">Text to write to the log</param>
        public void EnqueueLog(LogLevel logLevel, string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            Dispatcher.UIThread.Post(() =>
            {
                IBrush brush = GetBrush(logLevel);
                foreach (string line in text.Replace("\r\n", "\n").TrimEnd('\n').Split('\n'))
                {
                    Entries.Add(new LogEntry(line, brush));
                    if (Entries.Count > MaxEntryCount)
                        Entries.RemoveAt(0);
                }

                this.FindControl<ScrollViewer>("Scroller")?.ScrollToEnd();
            });
        }

        /// <summary>
        /// Add a single line to the log, trimming the oldest entry past the maximum count
        /// </summary>
        private void AddLine(string line, IBrush brush)
        {
            Entries.Add(new LogEntry(line, brush));
            if (Entries.Count > MaxEntryCount)
                Entries.RemoveAt(0);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Set the height of the console surface
        /// </summary>
        public void SetConsoleHeight(double height)
        {
            Border? logSurface = this.FindControl<Border>("LogSurface");
            if (logSurface is not null)
                logSurface.Height = DefaultConsoleHeight;
        }

        /// <summary>
        /// Get the foreground Brush for the current LogLevel
        /// </summary>
        /// <returns>Brush representing the color</returns>
        private static IBrush GetBrush(LogLevel logLevel)
        {
            bool darkMode = global::Avalonia.Application.Current?.ActualThemeVariant == ThemeVariant.Dark;

            return logLevel switch
            {
                LogLevel.SECRET => Brushes.DarkGray,
                LogLevel.ERROR => Brushes.IndianRed,
                LogLevel.VERBOSE => darkMode ? new SolidColorBrush(Color.Parse("#FFCBBF00")) : Brushes.Yellow,
                LogLevel.USER_SUCCESS => Brushes.ForestGreen,
                _ => Brushes.White,
            };
        }

        #endregion

        #region EventHandlers

        private void OnClearButton(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
            => Entries.Clear();

        private async void OnSaveButton(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
        {
            string logPath = "console.log";
            if (!OperatingSystem.IsWindows() && this.GetVisualRoot() is Window owner)
            {
                string? directory = await DialogService.OpenFolderAsync(owner, "Save Log");
                if (string.IsNullOrWhiteSpace(directory))
                    return;

                logPath = Path.Combine(directory, "console.log");
            }

            using var writer = new StreamWriter(File.Open(logPath, FileMode.Create, FileAccess.Write, FileShare.Read));
            foreach (LogEntry entry in Entries)
            {
                writer.WriteLine(entry.Text);
            }
        }

        #endregion

        /// <summary>
        /// Log line wrapper
        /// </summary>
        public sealed record LogEntry(string Text, IBrush Foreground);
    }
}
