﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using MPF.Core.Data;
using MPF.UI.Core.ComboBoxItems;
using MPF.Windows;
using RedumpLib.Web;
using WPFCustomMessageBox;

namespace MPF.UI.ViewModels
{
    public class OptionsViewModel
    {
        #region Fields

        /// <summary>
        /// Parent OptionsWindow object
        /// </summary>
        public OptionsWindow Parent { get; }

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
        public OptionsViewModel(OptionsWindow parent)
        {
            Parent = parent;
            Options = App.Options.Clone() as Options;

            // Add handlers
            Parent.AaruPathButton.Click += BrowseForPathClick;
            Parent.DiscImageCreatorPathButton.Click += BrowseForPathClick;
            Parent.DDPathButton.Click += BrowseForPathClick;
            Parent.DefaultOutputPathButton.Click += BrowseForPathClick;

            Parent.AcceptButton.Click += OnAcceptClick;
            Parent.CancelButton.Click += OnCancelClick;
            Parent.RedumpLoginTestButton.Click += OnRedumpTestClick;

            // Update UI with new values
            Load();
        }

        #region Load and Save

        /// <summary>
        /// Load any options-related elements
        /// </summary>
        private void Load()
        {
            Parent.InternalProgramComboBox.SelectedIndex = InternalPrograms.FindIndex(r => r == Options.InternalProgram);
            Parent.DefaultSystemComboBox.SelectedIndex = Systems.FindIndex(r => r == Options.DefaultSystem);
            Parent.RedumpPasswordBox.Password = Options.RedumpPassword;
        }

        /// <summary>
        /// Save any options-related elements
        /// </summary>
        private void Save()
        {
            var selectedInternalProgram = Parent.InternalProgramComboBox.SelectedItem as Element<InternalProgram>;
            Options.InternalProgram = selectedInternalProgram?.Value ?? InternalProgram.DiscImageCreator;
            var selectedDefaultSystem = Parent.DefaultSystemComboBox.SelectedItem as RedumpSystemComboBoxItem;
            Options.DefaultSystem = selectedDefaultSystem?.Value ?? null;
            Options.RedumpPassword = Parent.RedumpPasswordBox.Password;

            SavedSettings = true;
        }

        #endregion

        #region Population

        /// <summary>
        /// Get a complete list of  supported internal programs
        /// </summary>
        private static List<Element<InternalProgram>> PopulateInternalPrograms()
        {
            var internalPrograms = new List<InternalProgram> { InternalProgram.DiscImageCreator, InternalProgram.Aaru, InternalProgram.Redumper, InternalProgram.DD };
            return internalPrograms.Select(ip => new Element<InternalProgram>(ip)).ToList();
        }

        #endregion

        #region UI Commands

        /// <summary>
        /// Browse and set a path based on the invoking button
        /// </summary>
        private void BrowseForPath(System.Windows.Controls.Button button)
        {
            // If the button is null, we can't do anything
            if (button == null)
                return;

            // Strips button prefix to obtain the setting name
            string pathSettingName = button.Name.Substring(0, button.Name.IndexOf("Button"));

            // TODO: hack for now, then we'll see
            bool shouldBrowseForPath = pathSettingName == "DefaultOutputPath";

            string currentPath = TextBoxForPathSetting(pathSettingName)?.Text;
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
                        TextBoxForPathSetting(pathSettingName).Text = path;
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
        /// Optionally save the current options and close the parent window
        /// </summary>
        private void OptionalSaveAndClose(bool save)
        {
            // Save if we're supposed to
            if (save)
                Save();

            Parent.Close();
        }

        /// <summary>
        /// Test Redump login credentials
        /// </summary>
#if NET48 || NETSTANDARD2_1
        private bool? TestRedumpLogin()
#else
        private async Task<bool?> TestRedumpLogin()
#endif
        {
#if NET48 || NETSTANDARD2_1
            (bool? success, string message) = RedumpWebClient.ValidateCredentials(Parent.RedumpUsernameTextBox.Text, Parent.RedumpPasswordBox.Password);
#else
            (bool? success, string message) = await RedumpHttpClient.ValidateCredentials(Parent.RedumpUsernameTextBox.Text, Parent.RedumpPasswordBox.Password);
#endif
            if (success == true)
                CustomMessageBox.Show(Parent, message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            else if (success == false)
                CustomMessageBox.Show(Parent, message, "Failure", MessageBoxButton.OK, MessageBoxImage.Error);
            else
                CustomMessageBox.Show(Parent, message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

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
        private System.Windows.Controls.TextBox TextBoxForPathSetting(string name) =>
            Parent.FindName(name + "TextBox") as System.Windows.Controls.TextBox;

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handler for generic Click event
        /// </summary>
        private void BrowseForPathClick(object sender, EventArgs e) =>
            BrowseForPath(sender as System.Windows.Controls.Button);

        /// <summary>
        /// Handler for AcceptButton Click event
        /// </summary>
        private void OnAcceptClick(object sender, EventArgs e) =>
            OptionalSaveAndClose(true);

        /// <summary>
        /// Handler for CancelButtom Click event
        /// </summary>
        private void OnCancelClick(object sender, EventArgs e) =>
            OptionalSaveAndClose(false);

        /// <summary>
        /// Test Redump credentials for validity
        /// </summary>
#if NET48 || NETSTANDARD2_1
        private void OnRedumpTestClick(object sender, EventArgs e) =>
            TestRedumpLogin();
#else
        private async void OnRedumpTestClick(object sender, EventArgs e) =>
            _ = await TestRedumpLogin();
#endif

        #endregion
    }
}
