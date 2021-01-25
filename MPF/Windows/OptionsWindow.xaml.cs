using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using MPF.Data;
using MPF.Utilities;
using MPF.Web;
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
        /// Current UI options
        /// </summary>
        public UIOptions UIOptions { get; set; }

        /// <summary>
        /// List of available internal programs
        /// </summary>
        public List<InternalProgramComboBoxItem> InternalPrograms { get; private set; }

        #endregion

        public OptionsWindow()
        {
            InitializeComponent();

            PopulateInternalPrograms();
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
        /// Get a complete list of internal programs and fill the combo box
        /// </summary>
        private void PopulateInternalPrograms()
        {
            // We only support certain programs for dumping
            var internalPrograms = new List<InternalProgram> { InternalProgram.DiscImageCreator, InternalProgram.Aaru, InternalProgram.DD };
            ViewModels.LoggerViewModel.VerboseLogLn($"Populating internal programs, {internalPrograms.Count} internal programs found.");

            InternalPrograms = new List<InternalProgramComboBoxItem>();
            foreach (var internalProgram in internalPrograms)
            {
                InternalPrograms.Add(new InternalProgramComboBoxItem(internalProgram));
            }

            InternalProgramComboBox.ItemsSource = InternalPrograms;
            InternalProgramComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// Refresh any options-related elements
        /// </summary>
        public void Refresh()
        {
            // Handle non-bindable fields
            InternalProgramComboBox.SelectedIndex = InternalPrograms.FindIndex(r => r == Converters.ToInternalProgram(UIOptions.Options.InternalProgram));
            RedumpPasswordBox.Password = UIOptions.Options.Password;
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
            // Handle non-bindable fields
            UIOptions.Options.InternalProgram = (InternalProgramComboBox.SelectedItem as InternalProgramComboBoxItem)?.Name ?? InternalProgram.DiscImageCreator.ToString();
            UIOptions.Options.Password = RedumpPasswordBox.Password;

            UIOptions.Save();
            Hide();

            (Owner as MainWindow).OnOptionsUpdated();
        }

        /// <summary>
        /// Handler for CancelButton Click event
        /// </summary>
        private void OnCancelClick(object sender, EventArgs e)
        {
            // just hide the window and don't care
            Hide();
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
