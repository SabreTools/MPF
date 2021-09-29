using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using MPF.Core.Data;
using MPF.Windows;
using RedumpLib.Web;
using WPFCustomMessageBox;

namespace MPF.GUI.ViewModels
{
    public class OptionsViewModel
    {
        #region Fields

        /// <summary>
        /// Parent OptionsWindow object
        /// </summary>
        public OptionsWindow Parent { get; private set; }

        /// <summary>
        /// Current set of options
        /// </summary>
        public Options Options { get; private set; }

        /// <summary>
        /// Flag for if settings were saved or not
        /// </summary>
        public bool SavedSettings { get; private set; } = false;

        #endregion

        #region Lists

        /// <summary>
        /// List of available internal programs
        /// </summary>
        public List<Element<InternalProgram>> InternalPrograms { get; private set; } = PopulateInternalPrograms();

        /// <summary>
        /// Current list of supported system profiles
        /// </summary>
        public List<RedumpSystemComboBoxItem> Systems { get; private set; } = RedumpSystemComboBoxItem.GenerateElements().ToList();

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
            var internalPrograms = new List<InternalProgram> { InternalProgram.DiscImageCreator, InternalProgram.Aaru, InternalProgram.DD };
            return internalPrograms.Select(ip => new Element<InternalProgram>(ip)).ToList();
        }

        #endregion

        #region UI Commands

        /// <summary>
        /// Browse and set a path based on the invoking button
        /// </summary>
        private void BrowseForPath(Button button)
        {
            // If the button is null, we can't do anything
            if (button == null)
                return;

            // Strips button prefix to obtain the setting name
            string pathSettingName = button.Name.Substring(0, button.Name.IndexOf("Button"));

            // TODO: hack for now, then we'll see
            bool shouldBrowseForPath = pathSettingName == "DefaultOutputPath";

            CommonDialog dialog = shouldBrowseForPath ? (CommonDialog)CreateFolderBrowserDialog() : CreateOpenFileDialog();
            using (dialog)
            {
                DialogResult result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    string path;
                    bool exists;

                    if (shouldBrowseForPath)
                    {
                        path = (dialog as FolderBrowserDialog).SelectedPath;
                        exists = Directory.Exists(path);
                    }
                    else
                    {
                        path = (dialog as OpenFileDialog).FileName;
                        exists = File.Exists(path);
                    }

                    if (exists)
                    {
                        TextBoxForPathSetting(pathSettingName).Text = path;
                    }
                    else
                    {
                        CustomMessageBox.Show(
                            "Specified path doesn't exists!",
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
        private void TestRedumpLogin()
        {
            using (RedumpWebClient wc = new RedumpWebClient())
            {
                bool? loggedIn = wc.Login(Parent.RedumpUsernameTextBox.Text, Parent.RedumpPasswordBox.Password);
                if (loggedIn == true)
                    CustomMessageBox.Show(Parent, "Redump login credentials accepted!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                else if (loggedIn == false)
                    CustomMessageBox.Show(Parent, "Redump login credentials denied!", "Failure", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    CustomMessageBox.Show(Parent, "Error validating credentials!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
        private static OpenFileDialog CreateOpenFileDialog()
        {
            return new OpenFileDialog()
            {
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory,
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
        private TextBox TextBoxForPathSetting(string name) => Parent.FindName(name + "TextBox") as TextBox;

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handler for generic Click event
        /// </summary>
        private void BrowseForPathClick(object sender, EventArgs e) =>
            BrowseForPath(sender as Button);

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
        private void OnRedumpTestClick(object sender, EventArgs e) =>
            TestRedumpLogin();

        #endregion
    }
}
