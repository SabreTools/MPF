using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using MPF.Avalonia.Windows;

namespace MPF.Avalonia.Services
{
    /// <summary>
    /// Wraps the Avalonia storage provider to present open, save, and folder picker dialogs
    /// </summary>
    internal static class DialogService
    {
        /// <summary>
        /// Show an open-file dialog and return the selected file path, if any
        /// </summary>
        public static async Task<string?> OpenFileAsync(Window owner, string title, IReadOnlyList<FilePickerFileType> fileTypes)
        {
            if (owner.StorageProvider is null)
                return null;

            try
            {
                var files = await owner.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {
                    Title = title,
                    AllowMultiple = false,
                    FileTypeFilter = fileTypes,
                });

                return files.Count > 0 ? files[0].TryGetLocalPath() : null;
            }
            catch (Exception ex)
            {
                await ReportFailureAsync(owner, ex);
                return null;
            }
        }

        /// <summary>
        /// Show a folder picker dialog and return the selected folder path, if any
        /// </summary>
        public static async Task<string?> OpenFolderAsync(Window owner, string title)
        {
            if (owner.StorageProvider is null)
                return null;

            try
            {
                var folders = await owner.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = title,
                    AllowMultiple = false,
                });

                return folders.Count > 0 ? folders[0].TryGetLocalPath() : null;
            }
            catch (Exception ex)
            {
                await ReportFailureAsync(owner, ex);
                return null;
            }
        }

        /// <summary>
        /// Show a save-file dialog and return the chosen file path, if any
        /// </summary>
        public static async Task<string?> SaveFileAsync(Window owner, string title, string suggestedName, IReadOnlyList<FilePickerFileType> fileTypes)
        {
            if (owner.StorageProvider is null)
                return null;

            try
            {
                var file = await owner.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = title,
                    SuggestedFileName = suggestedName,
                    FileTypeChoices = fileTypes,
                });

                return file?.TryGetLocalPath();
            }
            catch (Exception ex)
            {
                await ReportFailureAsync(owner, ex);
                return null;
            }
        }

        /// <summary>
        /// Tell the user that the dialog could not be shown
        /// </summary>
        /// <remarks>
        /// The storage provider hands the request to the platform's file chooser, which can fail:
        /// on Linux it lives out of process behind xdg-desktop-portal and refuses the call outright
        /// when it cannot identify the caller. Every caller reaches these methods from an async void
        /// event handler, so an escaping exception is unhandled and takes MPF down. Report the
        /// failure and return no path, which callers already treat as a cancelled dialog.
        /// </remarks>
        private static async Task ReportFailureAsync(Window owner, Exception ex)
        {
            string message = $"The file dialog could not be shown:{Environment.NewLine}{Environment.NewLine}{ex.Message}";
            await MessageBoxWindow.ShowAsyncResult(owner, "File Dialog Error", message, 1, false);
        }
    }
}
