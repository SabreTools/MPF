using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using MPF.Frontend;
using MPF.Frontend.ViewModels;

namespace MPF.Avalonia.Windows
{
    /// <summary>
    /// Interaction logic for OptionsWindow.axaml
    /// </summary>
    public partial class OptionsWindow : WindowBase
    {
        /// <summary>
        /// Read-only access to the current options view model
        /// </summary>
        public OptionsViewModel OptionsViewModel => DataContext as OptionsViewModel ?? new OptionsViewModel();

        public OptionsWindow()
            : this(new Options())
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public OptionsWindow(Options options)
        {
            InitializeComponent();
            DataContext = new OptionsViewModel(options);

            // Set the window title
            OptionsViewModel.Title = Title;

            // Elsewhere drives are device paths, whose files are scanned in the dump image after dumping anyway
            ScanDumpImageFilesCheckBox!.IsVisible = SystemEnvironment.IsWindows();
        }

        #region UI Functionality

        /// <summary>
        /// Browse for an executable file, filtering to *.exe on Windows
        /// </summary>
        private async Task<string?> BrowseForExecutableAsync(string title)
        {
            List<FilePickerFileType> fileTypes;
            if (SystemEnvironment.IsWindows())
            {
                fileTypes = [
                    new FilePickerFileType("Executables") { Patterns = ["*.exe"] },
                    new FilePickerFileType("All files") { Patterns = ["*", "*.*"] },
                ];
            }
            else
            {
                fileTypes = [
                    new FilePickerFileType("All files") { Patterns = ["*", "*.*"] },
                ];
            }

            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                FileTypeFilter = fileTypes,
            });

            return files.Count > 0 ? files[0].TryGetLocalPath() : null;
        }

        /// <summary>
        /// Browse for a folder path
        /// </summary>
        private async Task<string?> BrowseForFolderAsync(string title)
        {
            var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
            });

            return folders.Count > 0 ? folders[0].TryGetLocalPath() : null;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handler for generic Click event
        /// </summary>
        private async void BrowseForAaruPathClick(object? sender, RoutedEventArgs e)
        {
            string? result = await BrowseForExecutableAsync("Aaru");
            if (!string.IsNullOrWhiteSpace(result))
                OptionsViewModel.AaruPath = result;
        }

        /// <summary>
        /// Handler for generic Click event
        /// </summary>
        private async void BrowseForDiscImageCreatorPathClick(object? sender, RoutedEventArgs e)
        {
            string? result = await BrowseForExecutableAsync("DiscImageCreator");
            if (!string.IsNullOrWhiteSpace(result))
                OptionsViewModel.DiscImageCreatorPath = result;
        }

        /// <summary>
        /// Handler for generic Click event
        /// </summary>
        private async void BrowseForDreamdumpPathClick(object? sender, RoutedEventArgs e)
        {
            string? result = await BrowseForExecutableAsync("Dreamdump");
            if (!string.IsNullOrWhiteSpace(result))
                OptionsViewModel.DreamdumpPath = result;
        }

        /// <summary>
        /// Handler for generic Click event
        /// </summary>
        private async void BrowseForRedumperPathClick(object? sender, RoutedEventArgs e)
        {
            string? result = await BrowseForExecutableAsync("Redumper");
            if (!string.IsNullOrWhiteSpace(result))
                OptionsViewModel.RedumperPath = result;
        }

        /// <summary>
        /// Handler for generic Click event
        /// </summary>
        private async void BrowseForDefaultOutputPathClick(object? sender, RoutedEventArgs e)
        {
            string? result = await BrowseForFolderAsync("Default Output Path");
            if (!string.IsNullOrWhiteSpace(result))
                OptionsViewModel.DefaultOutputPath = result;
        }

        /// <summary>
        /// Alert user of non-redump mode implications
        /// </summary>
        private void DreamdumpNonRedumpModeClicked(object? sender, RoutedEventArgs e)
        {
            if (OptionsViewModel.DreamdumpNonRedumpMode)
                _ = MessageBoxWindow.ShowAsync(this, StringResource("WarningMessageString", "Warning"),
            "All logs generated with these options will not be acceptable for Redump submission", 1, true);
            else
                OptionsViewModel.DreamdumpNonRedumpModeUnChecked();
        }

        /// <summary>
        /// Alert user of non-redump mode implications
        /// </summary>
        private void RedumperNonRedumpModeClicked(object? sender, RoutedEventArgs e)
        {
            if (OptionsViewModel.RedumperNonRedumpMode)
                _ = MessageBoxWindow.ShowAsync(this, StringResource("WarningMessageString", "Warning"),
            "All logs generated with these options will not be acceptable for Redump submission", 1, true);
            else
                OptionsViewModel.RedumperNonRedumpModeUnChecked();
        }

        /// <summary>
        /// Handler for AcceptButton Click event
        /// </summary>
        private void OnAcceptClick(object? sender, RoutedEventArgs e)
        {
            OptionsViewModel.SavedSettings = true;
            Close(true);
        }

        /// <summary>
        /// Handler for CancelButton Click event
        /// </summary>
        private void OnCancelClick(object? sender, RoutedEventArgs e)
        {
            OptionsViewModel.SavedSettings = false;
            Close(false);
        }

        #endregion
    }
}
