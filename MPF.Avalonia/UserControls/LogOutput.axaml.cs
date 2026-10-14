using System.Collections.ObjectModel;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
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

                Scroller?.ScrollToEnd();
            });
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Set the height of the console surface
        /// </summary>
        public void SetConsoleHeight(double height)
        {
            Border? logSurface = LogSurface;
            logSurface?.Height = height;
        }

        /// <summary>
        /// Get the foreground Brush for the current LogLevel
        /// </summary>
        /// <returns>Brush representing the color</returns>
        private static IBrush GetBrush(LogLevel logLevel)
        {
            bool darkMode = Application.Current?.ActualThemeVariant == ThemeVariant.Dark;

            return logLevel switch
            {
                LogLevel.SECRET => Brushes.DarkGray,
                LogLevel.ERROR => Brushes.IndianRed,
                LogLevel.VERBOSE => darkMode ? new SolidColorBrush(Color.Parse("#FFCBBF00")) : Brushes.Yellow,
                LogLevel.USER_SUCCESS => Brushes.ForestGreen,
                LogLevel.USER_GENERIC => Brushes.White,

                // Make unmatched log levels obvious
                _ => Brushes.Pink,
            };
        }

        #endregion

        #region EventHandlers

        private void OnClearButton(object? sender, RoutedEventArgs e)
            => Entries.Clear();

        private async void OnSaveButton(object? sender, RoutedEventArgs e)
        {
            try
            {
                string logPath = "console.log";
                if (!SystemEnvironment.IsWindows() && this.GetVisualRoot() is Window owner)
                {
                    var folders = await owner.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                    {
                        Title = "Save Log",
                        AllowMultiple = false,
                    });

                    string? directory = folders.Count > 0 ? folders[0].TryGetLocalPath() : null;
                    if (string.IsNullOrWhiteSpace(directory))
                        return;

                    logPath = Path.Combine(directory, "console.log");
                }

                // Ensure the output directory has been created
                string? parentDirectory = Path.GetDirectoryName(logPath);
                if (parentDirectory is not null)
                    Directory.CreateDirectory(parentDirectory);

                using var writer = new StreamWriter(File.Open(logPath, FileMode.Create, FileAccess.Write, FileShare.Read));
                foreach (LogEntry entry in Entries)
                {
                    writer.WriteLine(entry.Text);
                }
            }
            catch
            {
                // TODO: Report to user that the writing failed
            }
        }

        #endregion

        /// <summary>
        /// Log line wrapper
        /// </summary>
        public sealed record LogEntry(string Text, IBrush Foreground);
    }
}
