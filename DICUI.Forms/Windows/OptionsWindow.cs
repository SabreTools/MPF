using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;

namespace DICUI.Forms.Windows
{
    /// <summary>
    /// Interaction logic for OptionsWindow
    /// </summary>
    public partial class OptionsWindow : Form
    {
        private readonly MainWindow _mainWindow;
        private readonly Options _options;

        public OptionsWindow(MainWindow mainWindow, Options options)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            _options = options;

            // Create data sets
            DumpSpeedCDSlider.Minimum = (int)Constants.SpeedsForCDAsCollection.First();
            DumpSpeedCDSlider.Maximum = (int)Constants.SpeedsForCDAsCollection.Last();
            DumpSpeedDVDSlider.Minimum = (int)Constants.SpeedsForDVDAsCollection.First();
            DumpSpeedDVDSlider.Maximum = (int)Constants.SpeedsForDVDAsCollection.Last();
            DumpSpeedBDSlider.Minimum = (int)Constants.SpeedsForBDAsCollection.First();
            DumpSpeedBDSlider.Maximum = (int)Constants.SpeedsForBDAsCollection.Last();

            // Select the current values
            DumpSpeedCDSlider.Value = _options.PreferredDumpSpeedCD;
            DumpSpeedDVDSlider.Value = _options.PreferredDumpSpeedDVD;
            DumpSpeedBDSlider.Value = _options.PreferredDumpSpeedBD;

            // Create textbox outputs
            DumpSpeedCDTextBox.Text = DumpSpeedCDSlider.Value.ToString();
            DumpSpeedDVDTextBox.Text = DumpSpeedDVDSlider.Value.ToString();
            DumpSpeedBDTextBox.Text = DumpSpeedBDSlider.Value.ToString();

            // Set options
            QuietModeCheckBox.Checked = _options.QuietMode;
            ParanoidModeCheckBox.Checked = _options.ParanoidMode;
            C2RereadTimesTextBox.Text = _options.RereadAmountForC2.ToString();
            AutoScanCheckBox.Checked = _options.ScanForProtection;
            SkipDetectionCheckBox.Checked = _options.SkipMediaTypeDetection;
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

        private string[] PathSettings()
        {
            string[] pathSettings = { "DefaultOutputPath", "DICPath", "SubDumpPath" };
            return pathSettings;
        }

        private TextBox TextBoxForPathSetting(string name)
        {
            switch (name)
            {
                case "DICPath":
                    return DICPathTextBox;
                case "SubDumpPath":
                    return SubDumpPathTextBox;
                case "DefaultOutputPath":
                    return DefaultOutputPathTextBox;
                default:
                    return null;
            }
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

        public void RefreshSettings()
        {
            Array.ForEach(PathSettings(), setting => TextBoxForPathSetting(setting).Text = _options.Get(setting));

            DumpSpeedCDSlider.Value = _options.PreferredDumpSpeedCD;
            DumpSpeedDVDSlider.Value = _options.PreferredDumpSpeedDVD;
            DumpSpeedBDSlider.Value = _options.PreferredDumpSpeedBD;
        }

        #region Event Handlers

        private void SliderChanged(object sender, EventArgs e)
        {
            DumpSpeedCDTextBox.Text = DumpSpeedCDSlider.Value.ToString();
            DumpSpeedDVDTextBox.Text = DumpSpeedDVDSlider.Value.ToString();
            DumpSpeedBDTextBox.Text = DumpSpeedBDSlider.Value.ToString();
        }

        private void OnAcceptClick(object sender, EventArgs e)
        {
            Array.ForEach(PathSettings(), setting => _options.Set(setting, TextBoxForPathSetting(setting).Text));

            _options.PreferredDumpSpeedCD = Convert.ToInt32(DumpSpeedCDSlider.Value);
            _options.PreferredDumpSpeedDVD = Convert.ToInt32(DumpSpeedDVDSlider.Value);
            _options.PreferredDumpSpeedBD = Convert.ToInt32(DumpSpeedBDSlider.Value);

            _options.QuietMode = QuietModeCheckBox.Checked;
            _options.ParanoidMode = ParanoidModeCheckBox.Checked;
            _options.RereadAmountForC2 = Convert.ToInt32(C2RereadTimesTextBox.Text);
            _options.ScanForProtection = AutoScanCheckBox.Checked;
            _options.SkipMediaTypeDetection = SkipDetectionCheckBox.Checked;

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
