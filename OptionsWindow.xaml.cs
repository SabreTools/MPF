using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Button = System.Windows.Controls.Button;
using TextBox = System.Windows.Controls.TextBox;

namespace DICUI
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {
        private readonly MainWindow _mainWindow;
        private readonly Options _options;

        public OptionsWindow(MainWindow mainWindow, Options options)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _options = options;      
        }

        private OpenFileDialog CreateOpenFileDialog()
        {
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.InitialDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
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

        private string[] PathSettings()
        {
            string[] pathSettings = { "defaultOutputPath", "dicPath", "subdumpPath" };
            return pathSettings;
        }

        private TextBox TextBoxForPathSetting(string name)
        {
            return FindName("txt_" + name) as TextBox;
        }

        private void btn_BrowseForPath_Click(object sender, EventArgs e)
        {
            Button button = sender as Button;
            // strips button prefix to obtain the setting name
            string pathSettingName = button.Name.Substring("btn_".Length);

            // TODO: hack for now, then we'll see
            bool shouldBrowseForPath = pathSettingName == "defaultOutputPath";

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
                        TextBoxForPathSetting(pathSettingName).Text = path;
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
            Array.ForEach(PathSettings(), setting => TextBoxForPathSetting(setting).Text = _options.Get(setting));

            slider_DumpSpeedCD.Value = _options.preferredDumpSpeedCD;
            slider_DumpSpeedDVD.Value = _options.preferredDumpSpeedDVD;
        }

        #region Event Handlers

        private void OnAcceptClick(object sender, EventArgs e)
        {
            Array.ForEach(PathSettings(), setting => _options.Set(setting, TextBoxForPathSetting(setting).Text));

            _options.preferredDumpSpeedCD = Convert.ToInt32(slider_DumpSpeedCD.Value);
            _options.preferredDumpSpeedDVD = Convert.ToInt32(slider_DumpSpeedDVD.Value);

            _options.Save();
            Hide();

            _mainWindow.OnOptionsUpdated();
        }

        private void OnCancelClick(object sender, EventArgs e)
        {
            // just hide the window and don't care
            Hide();
        }

        #endregion
    }
}
