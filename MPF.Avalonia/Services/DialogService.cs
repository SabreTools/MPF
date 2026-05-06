using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace MPF.Avalonia.Services
{
    internal static class DialogService
    {
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

            return files.FirstOrDefault()?.TryGetLocalPath();
        }

        public static async Task<string?> OpenFolderAsync(Window owner, string title)
        {
            if (owner.StorageProvider is null)
                return null;

            var folders = await owner.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = title,
                AllowMultiple = false,
            });

            return folders.FirstOrDefault()?.TryGetLocalPath();
        }

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
