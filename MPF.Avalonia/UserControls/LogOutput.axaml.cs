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
        public const double DefaultConsoleHeight = 180;
        private const int MaxEntryCount = 5000;

        public ObservableCollection<LogEntry> Entries { get; } = [];

        public LogOutput()
        {
            InitializeComponent();
            DataContext = this;

            this.FindControl<Button>("ClearButton")!.Click += OnClearButton;
            this.FindControl<Button>("SaveButton")!.Click += OnSaveButton;
        }

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

        private void AddLine(string line, IBrush brush)
        {
            Entries.Add(new LogEntry(line, brush));
            if (Entries.Count > MaxEntryCount)
                Entries.RemoveAt(0);
        }

        public void SetConsoleHeight(double height)
        {
            Border? logSurface = this.FindControl<Border>("LogSurface");
            if (logSurface is not null)
                logSurface.Height = DefaultConsoleHeight;
        }

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
                writer.WriteLine(entry.Text);
        }

        public sealed record LogEntry(string Text, IBrush Foreground);
    }
}
