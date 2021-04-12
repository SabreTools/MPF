using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using MPF.Data;
using MPF.Redump;
using Button = System.Windows.Controls.Button;
using TextBox = System.Windows.Controls.TextBox;

namespace MPF.Windows
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {
        #region Fields

        /// <summary>
        /// Current set of options
        /// </summary>
        public Options Options { get; set; }

        /// <summary>
        /// List of available internal programs
        /// </summary>
        public List<Element<InternalProgram>> InternalPrograms { get; private set; } = PopulateInternalPrograms();

        /// <summary>
        /// Current list of supported system profiles
        /// </summary>
        public List<KnownSystemComboBoxItem> Systems { get; private set; } = KnownSystemComboBoxItem.GenerateElements().ToList();

        /// <summary>
        /// Flag for if settings were saved or not
        /// </summary>
        public bool SavedSettings { get; private set; } = false;

        #endregion

        public OptionsWindow(Options options)
        {
            InitializeComponent();
            DataContext = this;

            Options = options.Clone() as Options;
            Load();
        }

        #region Helpers

        private FolderBrowserDialog CreateFolderBrowserDialog()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            return dialog;
        }

        private OpenFileDialog CreateOpenFileDialog()
        {
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            dialog.Filter = "Executables (*.exe)|*.exe";
            dialog.FilterIndex = 0;
            dialog.RestoreDirectory = true;

            return dialog;
        }

        /// <summary>
        /// Load any options-related elements
        /// </summary>
        private void Load()
        {
            InternalProgramComboBox.SelectedIndex = InternalPrograms.FindIndex(r => r == Options.InternalProgram);
            DefaultSystemComboBox.SelectedIndex = Systems.FindIndex(r => r == Options.DefaultSystem);
            RedumpPasswordBox.Password = Options.RedumpPassword;
        }

        /// <summary>
        /// Get a complete list of  supported internal programs
        /// </summary>
        private static List<Element<InternalProgram>> PopulateInternalPrograms()
        {
            var internalPrograms = new List<InternalProgram> { InternalProgram.DiscImageCreator, InternalProgram.Aaru, InternalProgram.DD };
            return internalPrograms.Select(ip => new Element<InternalProgram>(ip)).ToList();
        }

        /// <summary>
        /// Save any options-related elements
        /// </summary>
        private void Save()
        {
            var selectedInternalProgram = InternalProgramComboBox.SelectedItem as Element<InternalProgram>;
            Options.InternalProgram = selectedInternalProgram?.Value ?? InternalProgram.DiscImageCreator;
            var selectedDefaultSystem = DefaultSystemComboBox.SelectedItem as KnownSystemComboBoxItem;
            Options.DefaultSystem = selectedDefaultSystem?.Value ?? KnownSystem.NONE;
            Options.RedumpPassword = RedumpPasswordBox.Password;

            SavedSettings = true;
        }

        /// <summary>
        /// Find a TextBox by setting name
        /// </summary>
        /// <param name="name">Setting name to find</param>
        /// <returns>TextBox for that setting</returns>
        private TextBox TextBoxForPathSetting(string name)
        {
            return FindName(name + "TextBox") as TextBox;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handler for generic Click event
        /// </summary>
        private void BrowseForPathClick(object sender, EventArgs e)
        {
            Button button = sender as Button;
            // strips button prefix to obtain the setting name
            string pathSettingName = button.Name.Substring(0, button.Name.IndexOf("Button"));

            // TODO: hack for now, then we'll see
            bool shouldBrowseForPath = pathSettingName == "DefaultOutputPath";

            CommonDialog dialog = shouldBrowseForPath ? (CommonDialog)CreateFolderBrowserDialog() : CreateOpenFileDialog();
            using (dialog)
            {
                DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
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
                        System.Windows.MessageBox.Show(
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
        /// Handler for AcceptButton Click event
        /// </summary>
        private void OnAcceptClick(object sender, EventArgs e)
        {
            Save();
            Close();
        }

        /// <summary>
        /// Handler for CancelButtom Click event
        /// </summary>
        private void OnCancelClick(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Test Redump credentials for validity
        /// </summary>
        private void OnRedumpTestClick(object sender, EventArgs e)
        {
            using (RedumpWebClient wc = new RedumpWebClient())
            {
                bool? loggedIn = wc.Login(RedumpUsernameTextBox.Text, RedumpPasswordBox.Password);
                if (loggedIn == true)
                    System.Windows.MessageBox.Show("Redump login credentials accepted!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                else if (loggedIn == false)
                    System.Windows.MessageBox.Show("Redump login credentials denied!", "Failure", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                    System.Windows.MessageBox.Show("Error validating credentials!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}
