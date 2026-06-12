using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

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

            var files = await owner.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                FileTypeFilter = fileTypes,
            });

            return files.Count > 0 ? files[0].TryGetLocalPath() : null;
        }

        /// <summary>
        /// Show a folder picker dialog and return the selected folder path, if any
        /// </summary>
        public static async Task<string?> OpenFolderAsync(Window owner, string title)
        {
            if (owner.StorageProvider is null)
                return null;

            var folders = await owner.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
            });

            return folders.Count > 0 ? folders[0].TryGetLocalPath() : null;
        }

        /// <summary>
        /// Show a save-file dialog and return the chosen file path, if any
        /// </summary>
        public static async Task<string?> SaveFileAsync(Window owner, string title, string suggestedName, IReadOnlyList<FilePickerFileType> fileTypes)
        {
            if (owner.StorageProvider is null)
                return null;

            var file = await owner.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = title,
                SuggestedFileName = suggestedName,
                FileTypeChoices = fileTypes,
            });

            return file?.TryGetLocalPath();
        }
    }
}
