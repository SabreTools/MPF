using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using MPF.Core.Data;
using MPF.Core.UI.ComboBoxItems;
using MPF.UI.Core.Windows;
using SabreTools.RedumpLib.Web;
using WPFCustomMessageBox;

namespace MPF.UI.Core.ViewModels
{
    public class OptionsViewModel
    {
        #region Fields

        /// <summary>
        /// Current set of options
        /// </summary>
        public Options Options { get; }

        /// <summary>
        /// Flag for if settings were saved or not
        /// </summary>
        public bool SavedSettings { get; private set; }

        #endregion

        #region Lists

        /// <summary>
        /// List of available internal programs
        /// </summary>
        public List<Element<InternalProgram>> InternalPrograms => PopulateInternalPrograms();

        /// <summary>
        /// Current list of supported system profiles
        /// </summary>
        public List<RedumpSystemComboBoxItem> Systems => RedumpSystemComboBoxItem.GenerateElements().ToList();

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public OptionsViewModel(Options baseOptions)
        {
            Options = new Options(baseOptions);
        }

        #region Load and Save

        /// <summary>
        /// Load any options-related elements
        /// </summary>
        /// TODO: Convert selected list item to binding
        internal void Load(OptionsWindow parent)
        {
            parent.InternalProgramComboBox.SelectedIndex = InternalPrograms.FindIndex(r => r == Options.InternalProgram);
            parent.DefaultSystemComboBox.SelectedIndex = Systems.FindIndex(r => r == Options.DefaultSystem);
        }

        /// <summary>
        /// Save any options-related elements
        /// </summary>
        /// TODO: Convert selected list item to binding
        internal void Save(OptionsWindow parent)
        {
            var selectedInternalProgram = parent.InternalProgramComboBox.SelectedItem as Element<InternalProgram>;
            Options.InternalProgram = selectedInternalProgram?.Value ?? InternalProgram.DiscImageCreator;
            var selectedDefaultSystem = parent.DefaultSystemComboBox.SelectedItem as RedumpSystemComboBoxItem;
            Options.DefaultSystem = selectedDefaultSystem?.Value ?? null;

            SavedSettings = true;
        }

        #endregion

        #region Population

        /// <summary>
        /// Get a complete list of  supported internal programs
        /// </summary>
        private static List<Element<InternalProgram>> PopulateInternalPrograms()
        {
            var internalPrograms = new List<InternalProgram> { InternalProgram.DiscImageCreator, InternalProgram.Aaru, InternalProgram.Redumper };
            return internalPrograms.Select(ip => new Element<InternalProgram>(ip)).ToList();
        }

        #endregion

        #region UI Commands

        /// <summary>
        /// Browse and set a path based on the invoking button
        /// </summary>
        internal void BrowseForPath(Window parent, System.Windows.Controls.Button button)
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
                if (result == DialogResult.OK)
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
                        Options[pathSettingName] = path;
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
        /// Test Redump login credentials
        /// </summary>
#if NET48
        public bool? TestRedumpLogin(Window parent, string username, string password)
#else
        public async Task<bool?> TestRedumpLogin(Window parent, string username, string password)
#endif
        {
#if NET48
            (bool? success, string message) = RedumpWebClient.ValidateCredentials(username, password);
#else
            (bool? success, string message) = await RedumpHttpClient.ValidateCredentials(username, password);
#endif
            if (success == true)
                CustomMessageBox.Show(parent, message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            else if (success == false)
                CustomMessageBox.Show(parent, message, "Failure", MessageBoxButton.OK, MessageBoxImage.Error);
            else
                CustomMessageBox.Show(parent, message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            return success;
        }

        #endregion

        #region UI Functionality

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
        /// Find a TextBox by setting name
        /// </summary>
        /// <param name="name">Setting name to find</param>
        /// <returns>TextBox for that setting</returns>
        private System.Windows.Controls.TextBox TextBoxForPathSetting(Window parent, string name) =>
            parent.FindName(name + "TextBox") as System.Windows.Controls.TextBox;

        #endregion
    }
}
