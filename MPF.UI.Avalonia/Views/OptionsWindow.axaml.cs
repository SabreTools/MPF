using System;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Interactivity;
using MPF.Frontend;
using MPF.Frontend.ViewModels;
using MPF.UI.Avalonia.Services;
using SabreTools.RedumpLib.Web;

namespace MPF.UI.Avalonia.Views
{
    /// <summary>
    /// Interaction logic for OptionsWindow.axaml
    /// </summary>
    public partial class OptionsWindow : WindowBase
    {
        /// <summary>
        /// Read-only access to the current options view model
        /// </summary>
        public OptionsViewModel OptionsViewModel => (OptionsViewModel)DataContext!;

        /// <summary>
        /// Designer / XAML-compiler constructor — not for runtime use.
        /// </summary>
        public OptionsWindow() : this(new Options()) { }

        /// <summary>
        /// Constructor
        /// </summary>
        public OptionsWindow(Options options)
        {
            InitializeComponent();

            DataContext = new OptionsViewModel(options);

            // Pre-populate the password TextBox from the stored password.
            // The password box is a plain TextBox with PasswordChar so it cannot bind to
            // Options.Processing.Login.RedumpPassword directly (the binding would display
            // the raw string, but changes made by the user must be committed back manually
            // via the TextChanged handler below).
            RedumpPasswordBox.Text = options.Processing.Login.RedumpPassword;
            RedumpPasswordBox.TextChanged += OnPasswordChanged;

            // Avalonia Slider.Ticks requires AvaloniaList<double>; Constants returns List<double>.
            // Set ticks here to avoid a runtime binding-conversion error.
            DumpSpeedCDSlider.Ticks = new AvaloniaList<double>(Constants.SpeedsForCDAsCollection);
            DumpSpeedDVDSlider.Ticks = new AvaloniaList<double>(Constants.SpeedsForDVDAsCollection);
            DumpSpeedHDDVDSlider.Ticks = new AvaloniaList<double>(Constants.SpeedsForHDDVDAsCollection);
            DumpSpeedBDSlider.Ticks = new AvaloniaList<double>(Constants.SpeedsForBDAsCollection);

            // Set ViewModel title to match the Window title once the title is set by the caller.
            // OnLoaded fires after the caller has set the Title property.
        }

        /// <inheritdoc/>
        protected override void OnLoaded(RoutedEventArgs e)
        {
            base.OnLoaded(e);
            // Sync the window title into the ViewModel so the title-bar TextBlock is correct.
            OptionsViewModel.Title = Title;
        }

        // ──────────────────────────────────────────────────────────────────────
        // Helpers
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Look up a string resource by key, returning an empty string when missing.
        /// </summary>
        private string FindResourceString(string key)
            => this.TryFindResource(key, out var value) && value is string s ? s : string.Empty;

        // ──────────────────────────────────────────────────────────────────────
        // Browse button handlers
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Browse for the Aaru executable path
        /// </summary>
        private async void BrowseForAaruPathClick(object? sender, RoutedEventArgs e)
        {
            string? result = await StorageDialogs.OpenFileAsync(this,
                FindResourceString("AaruPathLabelString"),
                ("Executables", new[] { "*.exe", "*" }));
            if (!string.IsNullOrEmpty(result))
            {
                OptionsViewModel.Options.Dumping.AaruPath = result;
                AaruPathTextBox.Text = result;
            }
        }

        /// <summary>
        /// Browse for the DiscImageCreator executable path
        /// </summary>
        private async void BrowseForDiscImageCreatorPathClick(object? sender, RoutedEventArgs e)
        {
            string? result = await StorageDialogs.OpenFileAsync(this,
                FindResourceString("DiscImageCreatorPathLabelString"),
                ("Executables", new[] { "*.exe", "*" }));
            if (!string.IsNullOrEmpty(result))
            {
                OptionsViewModel.Options.Dumping.DiscImageCreatorPath = result;
                DiscImageCreatorPathTextBox.Text = result;
            }
        }

        /// <summary>
        /// Browse for the Redumper executable path
        /// </summary>
        private async void BrowseForRedumperPathClick(object? sender, RoutedEventArgs e)
        {
            string? result = await StorageDialogs.OpenFileAsync(this,
                FindResourceString("RedumperPathLabelString"),
                ("Executables", new[] { "*.exe", "*" }));
            if (!string.IsNullOrEmpty(result))
            {
                OptionsViewModel.Options.Dumping.RedumperPath = result;
                RedumperPathTextBox.Text = result;
            }
        }

        /// <summary>
        /// Browse for the default output folder
        /// </summary>
        private async void BrowseForDefaultOutputPathClick(object? sender, RoutedEventArgs e)
        {
            string? result = await StorageDialogs.PickFolderAsync(this,
                FindResourceString("DefaultOutputPathLabelString"));
            if (!string.IsNullOrEmpty(result))
            {
                OptionsViewModel.Options.Dumping.DefaultOutputPath = result;
                DefaultOutputPathTextBox.Text = result;
            }
        }

        // ──────────────────────────────────────────────────────────────────────
        // Non-redump mode
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Alert user of non-redump mode implications
        /// </summary>
        private async void NonRedumpModeClicked(object? sender, RoutedEventArgs e)
        {
            if (OptionsViewModel.Options.Dumping.Redumper.NonRedumpMode)
            {
                await MessageBoxWindow.ShowAsync(this,
                    FindResourceString("WarningMessageString"),
                    "All logs generated with these options will not be acceptable for Redump submission",
                    MessageBoxButtons.Ok);
            }
            else
            {
                OptionsViewModel.NonRedumpModeUnChecked();
            }
        }

        // ──────────────────────────────────────────────────────────────────────
        // Accept / Cancel
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Handler for AcceptButton Click event
        /// </summary>
        private void OnAcceptClick(object? sender, RoutedEventArgs e)
        {
            OptionsViewModel.SavedSettings = true;
            Close();
        }

        /// <summary>
        /// Handler for CancelButton Click event
        /// </summary>
        private void OnCancelClick(object? sender, RoutedEventArgs e)
        {
            OptionsViewModel.SavedSettings = false;
            Close();
        }

        // ──────────────────────────────────────────────────────────────────────
        // Password
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Keep the ViewModel password in sync as the user types
        /// </summary>
        private void OnPasswordChanged(object? sender, TextChangedEventArgs e)
        {
            OptionsViewModel.Options.Processing.Login.RedumpPassword = RedumpPasswordBox.Text ?? string.Empty;
        }

        // ──────────────────────────────────────────────────────────────────────
        // Redump login test
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Test Redump credentials for validity
        /// </summary>
        private async void OnRedumpTestClick(object? sender, RoutedEventArgs e)
            => await ValidateRedumpCredentials();

        /// <summary>
        /// Validate Redump credentials and display the result to the user
        /// </summary>
        private async Task<bool?> ValidateRedumpCredentials()
        {
            string username = RedumpUsernameTextBox.Text ?? string.Empty;
            string password = RedumpPasswordBox.Text ?? string.Empty;

            bool? success = await RedumpClient.ValidateCredentials(username, password);
            string message = OptionsViewModel.GetRedumpLoginResult(success);

            string title = success == true
                ? "Success"
                : FindResourceString("ErrorMessageString");

            await MessageBoxWindow.ShowAsync(this, title, message, MessageBoxButtons.Ok);
            return success;
        }
    }
}
