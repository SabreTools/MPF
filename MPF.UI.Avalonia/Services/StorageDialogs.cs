using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace MPF.UI.Avalonia.Services
{
    /// <summary>
    /// Async helpers wrapping Avalonia <see cref="IStorageProvider"/> for file and folder pickers.
    /// These replace the WinForms/WPF dialog calls used by the original MPF.UI windows.
    /// </summary>
    public static class StorageDialogs
    {
        /// <summary>
        /// Show a save-file picker and return the chosen local path, or null if cancelled.
        /// </summary>
        /// <param name="owner">Owning window.</param>
        /// <param name="title">Picker title.</param>
        /// <param name="suggestedName">Pre-filled file name (optional).</param>
        /// <param name="filters">File type filters as (display name, pattern array) tuples.</param>
        public static async Task<string?> SaveFileAsync(
            Window owner,
            string title,
            string? suggestedName,
            params (string name, string[] patterns)[] filters)
        {
            var options = new FilePickerSaveOptions
            {
                Title = title,
                SuggestedFileName = suggestedName,
                FileTypeChoices = BuildFileTypes(filters),
            };

            var result = await owner.StorageProvider.SaveFilePickerAsync(options);
            return result?.TryGetLocalPath();
        }

        /// <summary>
        /// Show a folder picker and return the chosen local path, or null if cancelled.
        /// </summary>
        /// <param name="owner">Owning window.</param>
        /// <param name="title">Picker title.</param>
        public static async Task<string?> PickFolderAsync(Window owner, string title)
        {
            var options = new FolderPickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
            };

            var results = await owner.StorageProvider.OpenFolderPickerAsync(options);
            return results.Count > 0 ? results[0].TryGetLocalPath() : null;
        }

        /// <summary>
        /// Show an open-file picker and return the chosen local path, or null if cancelled.
        /// </summary>
        /// <param name="owner">Owning window.</param>
        /// <param name="title">Picker title.</param>
        /// <param name="filters">File type filters as (display name, pattern array) tuples.</param>
        public static async Task<string?> OpenFileAsync(
            Window owner,
            string title,
            params (string name, string[] patterns)[] filters)
        {
            var options = new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
                FileTypeFilter = BuildFileTypes(filters),
            };

            var results = await owner.StorageProvider.OpenFilePickerAsync(options);
            return results.Count > 0 ? results[0].TryGetLocalPath() : null;
        }

        // ------------------------------------------------------------------ helpers

        private static List<FilePickerFileType> BuildFileTypes((string name, string[] patterns)[] filters)
        {
            var list = new List<FilePickerFileType>(filters.Length);
            foreach (var (name, patterns) in filters)
                list.Add(new FilePickerFileType(name) { Patterns = patterns });
            return list;
        }
    }
}
