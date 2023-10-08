using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using MPF.Core.Data;
using MPF.Core.UI.ViewModels;
using WPFCustomMessageBox;

namespace MPF.UI.Core.Windows
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : WindowBase
    {
        /// <summary>
        /// Read-only access to the current options view model
        /// </summary>
        public OptionsViewModel OptionsViewModel => DataContext as OptionsViewModel;

        /// <summary>
        /// Constructor
        /// </summary>
        public OptionsWindow(Options options)
        {
            InitializeComponent();
            DataContext = new OptionsViewModel(options);

            // Set initial value for binding
            RedumpPasswordBox.Password = options.RedumpPassword;

            // Add handlers
            AaruPathButton.Click += BrowseForPathClick;
            DiscImageCreatorPathButton.Click += BrowseForPathClick;
            DefaultOutputPathButton.Click += BrowseForPathClick;

            AcceptButton.Click += OnAcceptClick;
            CancelButton.Click += OnCancelClick;
            RedumpPasswordBox.PasswordChanged += OnPasswordChanged;
            RedumpLoginTestButton.Click += OnRedumpTestClick;
        }

        #region UI Commands

        /// <summary>
        /// Browse and set a path based on the invoking button
        /// </summary>
        private void BrowseForPath(Window parent, System.Windows.Controls.Button button)
        {
            // If the button is null, we can't do anything
            if (button == null)
                return;

            // Strips button prefix to obtain the setting name
            string pathSettingName = button.Name.Substring(0, button.Name.IndexOf("Button"));

            // TODO: hack for now, then we'll see
            bool shouldBrowseForPath = pathSettingName == "DefaultOutputPath";

            string currentPath = TextBoxForPathSetting(parent, pathSettingName)?.Text;
            string initialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (!shouldBrowseForPath && !string.IsNullOrEmpty(currentPath))
                initialDirectory = Path.GetDirectoryName(Path.GetFullPath(currentPath));

            CommonDialog dialog = shouldBrowseForPath
                ? (CommonDialog)CreateFolderBrowserDialog()
                : CreateOpenFileDialog(initialDirectory);
            using (dialog)
            {
                DialogResult result = dialog.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
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
                        OptionsViewModel.Options[pathSettingName] = path;
                        TextBoxForPathSetting(parent, pathSettingName).Text = path;
                    }
                    else
                    {
                        CustomMessageBox.Show(
                            "Specified path doesn't exist!",
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error
                        );
                    }
                }
            }
        }

        /// <summary>
        /// Find a TextBox by setting name
        /// </summary>
        /// <param name="name">Setting name to find</param>
        /// <returns>TextBox for that setting</returns>
        private System.Windows.Controls.TextBox TextBoxForPathSetting(Window parent, string name) =>
            parent.FindName(name + "TextBox") as System.Windows.Controls.TextBox;

        /// <summary>
        /// Create an open folder dialog box
        /// </summary>
        private static FolderBrowserDialog CreateFolderBrowserDialog() => new FolderBrowserDialog();

        /// <summary>
        /// Create an open file dialog box
        /// </summary>
        private static OpenFileDialog CreateOpenFileDialog(string initialDirectory)
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
#if NET48
        private void ValidateRedumpCredentials()
#else
        private async Task ValidateRedumpCredentials()
#endif
        {
#if NET48
            (bool? success, string message) = OptionsViewModel.TestRedumpLogin(RedumpUsernameTextBox.Text, RedumpPasswordBox.Password);
#else
            (bool? success, string message) = await OptionsViewModel.TestRedumpLogin(RedumpUsernameTextBox.Text, RedumpPasswordBox.Password);
#endif

            if (success == true)
                CustomMessageBox.Show(this, message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            else if (success == false)
                CustomMessageBox.Show(this, message, "Failure", MessageBoxButton.OK, MessageBoxImage.Error);
            else
                CustomMessageBox.Show(this, message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

#endregion

        #region Event Handlers

        /// <summary>
        /// Handler for generic Click event
        /// </summary>
        private void BrowseForPathClick(object sender, EventArgs e) =>
            BrowseForPath(this, sender as System.Windows.Controls.Button);

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
            OptionsViewModel.Options.RedumpPassword = RedumpPasswordBox.Password;
        }

        /// <summary>
        /// Test Redump credentials for validity
        /// </summary>
#if NET48
        private void OnRedumpTestClick(object sender, EventArgs e) => ValidateRedumpCredentials();
#else
        private async void OnRedumpTestClick(object sender, EventArgs e) => await ValidateRedumpCredentials();
#endif

        #endregion
    }
}
