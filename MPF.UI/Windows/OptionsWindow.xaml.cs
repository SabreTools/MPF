using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using MPF.Frontend;
using MPF.Frontend.ViewModels;
using SabreTools.RedumpLib.Web;
using WPFCustomMessageBox;

namespace MPF.UI.Windows
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : WindowBase
    {
        /// <summary>
        /// Read-only access to the current options view model
        /// </summary>
        public OptionsViewModel OptionsViewModel => DataContext as OptionsViewModel ?? new OptionsViewModel();

#if NET35

        private System.Windows.Controls.Button? AaruPathButton => ItemHelper.FindChild<System.Windows.Controls.Button>(this, "AaruPathButton");
        private System.Windows.Controls.Button? AcceptButton => ItemHelper.FindChild<System.Windows.Controls.Button>(this, "AcceptButton");
        private System.Windows.Controls.Button? CancelButton => ItemHelper.FindChild<System.Windows.Controls.Button>(this, "CancelButton");
        private System.Windows.Controls.Button? DefaultOutputPathButton => ItemHelper.FindChild<System.Windows.Controls.Button>(this, "DefaultOutputPathButton");
        private System.Windows.Controls.Button? DiscImageCreatorPathButton => ItemHelper.FindChild<System.Windows.Controls.Button>(this, "DiscImageCreatorPathButton");
        private System.Windows.Controls.Button? RedumperPathButton => ItemHelper.FindChild<System.Windows.Controls.Button>(this, "RedumperPathButton");
        private System.Windows.Controls.Button? RedumpLoginTestButton => ItemHelper.FindChild<System.Windows.Controls.Button>(this, "RedumpLoginTestButton");
        private PasswordBox? RedumpPasswordBox => ItemHelper.FindChild<PasswordBox>(this, "RedumpPasswordBox");
        private System.Windows.Controls.TextBox? RedumpUsernameTextBox => ItemHelper.FindChild<System.Windows.Controls.TextBox>(this, "RedumpUsernameTextBox");

#endif

        /// <summary>
        /// Constructor
        /// </summary>
        public OptionsWindow(SegmentedOptions options)
        {
#if NET40_OR_GREATER || NETCOREAPP
            InitializeComponent();
#endif

#if NET40_OR_GREATER || NETCOREAPP
            DumpSpeedCDTextBox.IsReadOnlyCaretVisible = false;
            DumpSpeedDVDTextBox.IsReadOnlyCaretVisible = false;
            DumpSpeedHDDVDTextBox.IsReadOnlyCaretVisible = false;
            DumpSpeedBDTextBox.IsReadOnlyCaretVisible = false;
#endif

#if NET452_OR_GREATER || NETCOREAPP
            var chrome = new System.Windows.Shell.WindowChrome
            {
                CaptionHeight = 0,
                ResizeBorderThickness = new Thickness(0),
            };
            System.Windows.Shell.WindowChrome.SetWindowChrome(this, chrome);
#endif
            DataContext = new OptionsViewModel(options);

            // Set initial value for binding
            RedumpPasswordBox!.Password = options.Processing.Login.RedumpPassword;

            // Add handlers
            AaruPathButton!.Click += BrowseForAaruPathClick;
            DiscImageCreatorPathButton!.Click += BrowseForDiscImageCreatorPathClick;
            RedumperPathButton!.Click += BrowseForRedumperPathClick;
            DefaultOutputPathButton!.Click += BrowseForDefaultOutputPathClick;

            AcceptButton!.Click += OnAcceptClick;
            CancelButton!.Click += OnCancelClick;
            RedumpPasswordBox!.PasswordChanged += OnPasswordChanged;
            RedumpLoginTestButton!.Click += OnRedumpTestClick;
        }

        /// <summary>
        /// Handler for OptionsWindow OnContentRendered event
        /// </summary>
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);

            // Set the window title
            OptionsViewModel.Title = Title;
        }

        #region UI Commands

        /// <summary>
        /// Browse and set a path based on the invoking button
        /// </summary>
        private string? BrowseForPath(Window parent, System.Windows.Controls.Button? button)
        {
            // If the button is null, we can't do anything
            if (button is null)
                return null;

            // Strips button prefix to obtain the setting name
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
            string pathSettingName = button.Name[..button.Name.IndexOf("Button")];
#else
            string pathSettingName = button.Name.Substring(0, button.Name.IndexOf("Button"));
#endif

            // TODO: hack for now, then we'll see
            bool shouldBrowseForPath = pathSettingName == "DefaultOutputPath";

            var currentPath = TextBoxForPathSetting(parent, pathSettingName)?.Text;
            var initialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (!shouldBrowseForPath && !string.IsNullOrEmpty(currentPath))
                initialDirectory = Path.GetDirectoryName(Path.GetFullPath(currentPath));

            CommonDialog dialog = shouldBrowseForPath
                ? CreateFolderBrowserDialog()
                : CreateOpenFileDialog(initialDirectory);
            using (dialog)
            {
                DialogResult result = dialog.ShowDialog();
                if (result != System.Windows.Forms.DialogResult.OK)
                    return null;

                string path = string.Empty;
                bool exists = false;

                if (shouldBrowseForPath && dialog is FolderBrowserDialog folderBrowserDialog)
                {
                    path = folderBrowserDialog.SelectedPath;
                    exists = Directory.Exists(path);
                }
                else if (dialog is OpenFileDialog openFileDialog)
                {
                    path = openFileDialog.FileName;
                    exists = File.Exists(path);
                }

                if (exists)
                {
                    var textBox = TextBoxForPathSetting(parent, pathSettingName);
                    textBox?.Text = path;
                }
                else
                {
                    CustomMessageBox.Show(
                        "Specified path doesn't exist!",
                        (string)System.Windows.Application.Current.FindResource("ErrorMessageString"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );
                }

                return path;
            }
        }

        /// <summary>
        /// Find a TextBox by setting name
        /// </summary>
        /// <param name="name">Setting name to find</param>
        /// <returns>TextBox for that setting</returns>
        private static System.Windows.Controls.TextBox? TextBoxForPathSetting(Window parent, string name)
            => parent.FindName(name + "TextBox") as System.Windows.Controls.TextBox;

        /// <summary>
        /// Create an open folder dialog box
        /// </summary>
        private static FolderBrowserDialog CreateFolderBrowserDialog() => new();

        /// <summary>
        /// Create an open file dialog box
        /// </summary>
        private static OpenFileDialog CreateOpenFileDialog(string? initialDirectory)
        {
            return new OpenFileDialog()
            {
                InitialDirectory = initialDirectory,
                Filter = "Executables (*.exe)|*.exe",
                FilterIndex = 0,
                RestoreDirectory = true,
            };
        }

        /// <summary>
        /// Test Redump credentials for validity
        /// </summary>
        private async Task<bool?> ValidateRedumpCredentials()
        {
            bool? success = await RedumpClient.ValidateCredentials(RedumpUsernameTextBox!.Text, RedumpPasswordBox!.Password);
            string message = OptionsViewModel.GetRedumpLoginResult(success);

            if (success == true)
                CustomMessageBox.Show(this, message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            else if (success == false)
                CustomMessageBox.Show(this, message, (string)System.Windows.Application.Current.FindResource("ErrorMessageString"), MessageBoxButton.OK, MessageBoxImage.Error);
            else
                CustomMessageBox.Show(this, message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            return success;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handler for generic Click event
        /// </summary>
        private void BrowseForAaruPathClick(object sender, EventArgs e)
        {
            string? result = BrowseForPath(this, sender as System.Windows.Controls.Button);
            if (result is not null)
                OptionsViewModel.Options.Dumping.AaruPath = result;
        }

        /// <summary>
        /// Handler for generic Click event
        /// </summary>
        private void BrowseForDefaultOutputPathClick(object sender, EventArgs e)
        {
            string? result = BrowseForPath(this, sender as System.Windows.Controls.Button);
            if (result is not null)
                OptionsViewModel.Options.Dumping.DefaultOutputPath = result;
        }

        /// <summary>
        /// Handler for generic Click event
        /// </summary>
        private void BrowseForDiscImageCreatorPathClick(object sender, EventArgs e)
        {
            string? result = BrowseForPath(this, sender as System.Windows.Controls.Button);
            if (result is not null)
                OptionsViewModel.Options.Dumping.DiscImageCreatorPath = result;
        }

        /// <summary>
        /// Handler for generic Click event
        /// </summary>
        private void BrowseForRedumperPathClick(object sender, EventArgs e)
        {
            string? result = BrowseForPath(this, sender as System.Windows.Controls.Button);
            if (result is not null)
                OptionsViewModel.Options.Dumping.RedumperPath = result;
        }

        /// <summary>
        /// Alert user of non-redump mode implications
        /// </summary>
        private void NonRedumpModeClicked(object sender, EventArgs e)
        {
            if (OptionsViewModel.Options.Dumping.Redumper.NonRedumpMode)
                CustomMessageBox.Show(this, "All logs generated with these options will not be acceptable for Redump submission",
                    (string)System.Windows.Application.Current.FindResource("WarningMessageString"), MessageBoxButton.OK, MessageBoxImage.Warning);
            else
                OptionsViewModel.NonRedumpModeUnChecked();
        }

        /// <summary>
        /// Handler for AcceptButton Click event
        /// </summary>
        private void OnAcceptClick(object sender, EventArgs e)
        {
            OptionsViewModel.SavedSettings = true;
            Close();
        }

        /// <summary>
        /// Handler for CancelButtom Click event
        /// </summary>
        private void OnCancelClick(object sender, EventArgs e)
        {
            OptionsViewModel.SavedSettings = false;
            Close();
        }

        /// <summary>
        /// Handler for RedumpPasswordBox PasswordChanged event
        /// </summary>
        private void OnPasswordChanged(object sender, EventArgs e)
        {
            OptionsViewModel.Options.Processing.Login.RedumpPassword = RedumpPasswordBox!.Password;
        }

        /// <summary>
        /// Test Redump credentials for validity
        /// </summary>
        private async void OnRedumpTestClick(object sender, EventArgs e)
            => await ValidateRedumpCredentials();

        #endregion
    }
}
