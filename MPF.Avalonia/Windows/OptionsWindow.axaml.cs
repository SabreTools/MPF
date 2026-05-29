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
            Opened += (_, _) =>
            {
                OptionsViewModel.Title = Title;
                WireEvents();
            };
        }

        #region UI Functionality

        /// <summary>
        /// Add all event handlers
        /// </summary>
        private void WireEvents()
        {
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
            if (!System.OperatingSystem.IsWindows())
            {
                return await DialogService.OpenFileAsync(this, title, new List<FilePickerFileType>
                {
                    new("All files") { Patterns = new[] { "*", "*.*" } },
                });
            }

            return await DialogService.OpenFileAsync(this, title, new List<FilePickerFileType>
            {
                new("Executables") { Patterns = new[] { "*.exe" } },
                new("All files") { Patterns = new[] { "*", "*.*" } },
            });
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
                OptionsViewModel.Options.Processing.Login.RedumpUsername,
                OptionsViewModel.Options.Processing.Login.RedumpPassword);

            _ = MessageBoxWindow.ShowAsync(this, "Redump", OptionsViewModel.GetRedumpLoginResult(success), 1, success == true);
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
