using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using DICUI.Web;
using Button = System.Windows.Controls.Button;
using TextBox = System.Windows.Controls.TextBox;

namespace DICUI.Windows
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

        #endregion

        public OptionsWindow()
        {
            InitializeComponent();
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

        private FolderBrowserDialog CreateFolderBrowserDialog()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            return dialog;
        }

        private TextBox TextBoxForPathSetting(string name)
        {
            return FindName(name + "TextBox") as TextBox;
        }

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

        public void Refresh()
        {
            // Handle non-bindable fields
            RedumpPasswordBox.Password = UIOptions.Options.Password;
        }

        #region Event Handlers

        private void OnAcceptClick(object sender, EventArgs e)
        {
            // Handle non-bindable fields
            UIOptions.Options.Password = RedumpPasswordBox.Password;

            UIOptions.Save();
            Hide();

            (Owner as MainWindow).OnOptionsUpdated();
        }

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
                if (wc.Login(RedumpUsernameTextBox.Text, RedumpPasswordBox.Password))
                    System.Windows.MessageBox.Show("Redump login credentials accepted!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    System.Windows.MessageBox.Show("Redump login credentials denied!", "Failure", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}
