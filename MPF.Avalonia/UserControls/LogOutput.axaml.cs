using System;
using System.Collections.ObjectModel;
using System.IO;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;
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

        private void OnSaveButton(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
        {
            using var writer = new StreamWriter(File.Open("console.log", FileMode.Create, FileAccess.Write, FileShare.Read));
            foreach (LogEntry entry in Entries)
                writer.Write(entry.Text);
        }

        public sealed record LogEntry(string Text, IBrush Foreground);
    }
}
