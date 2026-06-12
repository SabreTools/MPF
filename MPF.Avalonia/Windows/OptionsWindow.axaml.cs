using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using MPF.Avalonia.Services;
using MPF.Frontend;
using MPF.Frontend.ViewModels;
using SabreTools.RedumpLib.Web;

namespace MPF.Avalonia.Windows
{
    /// <summary>
    /// Interaction logic for OptionsWindow.axaml
    /// </summary>
    public partial class OptionsWindow : WindowBase
    {
        /// <summary>
        /// File picker types for browsing executables on Windows
        /// </summary>
        private static readonly List<FilePickerFileType> WindowsExecutableFileTypes =
        [
            new("Executables") { Patterns = ["*.exe"] },
            new("All files") { Patterns = ["*", "*.*"] },
        ];

        /// <summary>
        /// File picker types for browsing executables on non-Windows platforms
        /// </summary>
        private static readonly List<FilePickerFileType> AllFilesFileTypes =
        [
            new("All files") { Patterns = ["*", "*.*"] },
        ];

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
            Opened += WireEvents;
        }

        #region UI Functionality

        /// <summary>
        /// Add all event handlers once the window has opened
        /// </summary>
        private void WireEvents(object? sender, EventArgs e)
        {
            OptionsViewModel.Title = Title;
            this.FindControl<Button>("AaruPathButton")!.Click += BrowseForAaruPathClick;
            this.FindControl<Button>("DiscImageCreatorPathButton")!.Click += BrowseForDiscImageCreatorPathClick;
            this.FindControl<Button>("RedumperPathButton")!.Click += BrowseForRedumperPathClick;
            this.FindControl<Button>("DefaultOutputPathButton")!.Click += BrowseForDefaultOutputPathClick;
            this.FindControl<Button>("AcceptButton")!.Click += OnAcceptClick;
            this.FindControl<Button>("CancelButton")!.Click += OnCancelClick;
            this.FindControl<Button>("RedumpLoginTestButton")!.Click += OnRedumpTestClick;
            this.FindControl<CheckBox>("NonRedumpModeCheckBox")!.Click += NonRedumpModeClicked;
        }

        /// <summary>
        /// Browse for an executable file, filtering to *.exe on Windows
        /// </summary>
        private async Task<string?> BrowseForExecutableAsync(string title)
        {
            List<FilePickerFileType> fileTypes = OperatingSystem.IsWindows()
                ? WindowsExecutableFileTypes
                : AllFilesFileTypes;

            return await DialogService.OpenFileAsync(this, title, fileTypes);
        }

        /// <summary>
        /// Browse for a folder path
        /// </summary>
        private async Task<string?> BrowseForFolderAsync(string title)
            => await DialogService.OpenFolderAsync(this, title);

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handler for generic Click event
        /// </summary>
        private async void BrowseForAaruPathClick(object? sender, RoutedEventArgs e)
        {
            string? result = await BrowseForExecutableAsync("Aaru");
            if (!string.IsNullOrWhiteSpace(result))
                OptionsViewModel.Options.Dumping.AaruPath = result;
        }

        /// <summary>
        /// Handler for generic Click event
        /// </summary>
        private async void BrowseForDiscImageCreatorPathClick(object? sender, RoutedEventArgs e)
        {
            string? result = await BrowseForExecutableAsync("DiscImageCreator");
            if (!string.IsNullOrWhiteSpace(result))
                OptionsViewModel.Options.Dumping.DiscImageCreatorPath = result;
        }

        /// <summary>
        /// Handler for generic Click event
        /// </summary>
        private async void BrowseForRedumperPathClick(object? sender, RoutedEventArgs e)
        {
            string? result = await BrowseForExecutableAsync("Redumper");
            if (!string.IsNullOrWhiteSpace(result))
                OptionsViewModel.Options.Dumping.RedumperPath = result;
        }

        /// <summary>
        /// Handler for generic Click event
        /// </summary>
        private async void BrowseForDefaultOutputPathClick(object? sender, RoutedEventArgs e)
        {
            string? result = await BrowseForFolderAsync("Default Output Path");
            if (!string.IsNullOrWhiteSpace(result))
                OptionsViewModel.Options.Dumping.DefaultOutputPath = result;
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
            => Close(false);

        /// <summary>
        /// Test Redump credentials for validity
        /// </summary>
        private async void OnRedumpTestClick(object? sender, RoutedEventArgs e)
        {
            bool? success = await RedumpClient.ValidateCredentials(
                OptionsViewModel.Options.Processing.Login.RedumpOrgUsername,
                OptionsViewModel.Options.Processing.Login.RedumpOrgPassword);

            _ = MessageBoxWindow.ShowAsync(this, "Redump", OptionsViewModel.GetRedumpOrgLoginResult(success), 1, success == true);
        }

        /// <summary>
        /// Alert user of non-redump mode implications
        /// </summary>
        private void NonRedumpModeClicked(object? sender, RoutedEventArgs e)
        {
            if (this.FindControl<CheckBox>("NonRedumpModeCheckBox")!.IsChecked != true)
                OptionsViewModel.NonRedumpModeUnChecked();
        }

        #endregion
    }
}
