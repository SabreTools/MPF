using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using WinForms = System.Windows.Forms;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DICUI
{
    public partial class MainWindow : Window
    {
        // TODO: Make configurable in UI or in Settings
        private const string defaultOutputPath = "ISO";
        private const string dicPath = "Programs\\DiscImageCreator.exe";
        private const string psxtPath = "psxt001z.exe";
        private const string sgRawPath = "sg_raw.exe";

        private List<Tuple<char, string>> _drives { get; set; }
        private List<int> _driveSpeeds { get { return new List<int> { 1, 2, 3, 4, 6, 8, 12, 16, 20, 24, 32, 40, 44, 48, 52, 56, 72 }; } }
        private List<Tuple<string, KnownSystem?, DiscType?>> _systems { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            // Populate the list of systems
            PopulateSystems();

            // Populate the list of drives
            PopulateDrives();

            // Populate the list of drive speeds
            PopulateDriveSpeeds();
        }

        #region Events

        private void btn_Start_Click(object sender, RoutedEventArgs e)
        {
            StartDumping();
        }

        private void btn_OutputDirectoryBrowse_Click(object sender, RoutedEventArgs e)
        {
            BrowseFolder();
        }

        private void btn_Search_Click(object sender, RoutedEventArgs e)
        {
            PopulateDrives();
        }

        private void cmb_DiscType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EnsureDiscInformation();
            GetOutputNames();
        }

        private void cmb_DriveLetter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetOutputNames();
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Get a complete list of supported systems and fill the combo box
        /// </summary>
        private void PopulateSystems()
        {
            _systems = Utilities.CreateListOfSystems();
            cmb_DiscType.ItemsSource = _systems;
            cmb_DiscType.DisplayMemberPath = "Item1";
            cmb_DiscType.SelectedIndex = 0;
            cmb_DiscType_SelectionChanged(null, null);
        }

        /// <summary>
        /// Get a complete list of active disc drives and fill the combo box
        /// </summary>
        /// <remarks>TODO: Find a way for this to periodically run, or have it hook to a "drive change" event</remarks>
        private void PopulateDrives()
        {
            // Populate the list of drives and add it to the combo box
            _drives = Utilities.CreateListOfDrives();
            cmb_DriveLetter.ItemsSource = _drives;
            cmb_DriveLetter.DisplayMemberPath = "Item1";
            cmb_DriveLetter.SelectedIndex = 0;
            cmb_DriveLetter_SelectionChanged(null, null);

            if (cmb_DriveLetter.Items.Count > 0)
            {
                lbl_Status.Content = "Valid optical disc found! Choose your Disc Type";
                btn_Start.IsEnabled = true;
            }
            else
            {
                lbl_Status.Content = "No valid optical disc found!";
                btn_Start.IsEnabled = false;
            }
        }

        /// <summary>
        /// Get a complete list of (possible) disc drive speeds, and fill the combo box
        /// </summary>
        private void PopulateDriveSpeeds()
        {
            cmb_DriveSpeed.ItemsSource = _driveSpeeds;
            cmb_DriveSpeed.SelectedItem = 8;
        }

        /// <summary>
        /// Browse for an output folder
        /// </summary>
        private void BrowseFolder()
        {
            WinForms.FolderBrowserDialog folderDialog = new WinForms.FolderBrowserDialog { ShowNewFolderButton = false, SelectedPath = System.AppDomain.CurrentDomain.BaseDirectory };
            WinForms.DialogResult result = folderDialog.ShowDialog();

            if (result == WinForms.DialogResult.OK)
            {
                txt_OutputDirectory.Text = folderDialog.SelectedPath;
            }
        }

        /// <summary>
        /// Begin the dumping process using the given inputs
        /// </summary>
        private async void StartDumping()
        {
            // Local variables
            string driveLetter = cmb_DriveLetter.Text;
            string outputDirectory = txt_OutputDirectory.Text;
            string outputFilename = txt_OutputFilename.Text;
            int driveSpeed = (int)cmb_DriveSpeed.SelectedItem;
            btn_Start.IsEnabled = false;

            // Get the discType and processArguments from a given system and disc combo
            var selected = cmb_DiscType.SelectedValue as Tuple<string, KnownSystem?, DiscType?>;
            string discType = Utilities.GetBaseCommand(selected.Item3);
            List<string> defaultParams = Utilities.GetDefaultParameters(selected.Item2, selected.Item3);

            // Validate that everything is good
            if (discType == null || defaultParams == null)
            {
                lbl_Status.Content = "Error! Current configuration is not supported!";
                return;
            }

            // Validate that the required program exits
            if (!File.Exists(dicPath))
            {
                lbl_Status.Content = "Error! Could not find DiscImageCreator!";
                return;
            }

            await Task.Run(
                () =>
                {
                    Process process = new Process();
                    process.StartInfo.FileName = dicPath;
                    process.StartInfo.Arguments = discType
                        + " " + driveLetter
                        + " \"" + Path.Combine(outputDirectory, outputFilename) + "\" "
                        + (selected.Item3 != DiscType.BD25 && selected.Item3 != DiscType.BD50 ? driveSpeed + " " : "")
                        + string.Join(" ", defaultParams);
                    process.Start();
                    process.WaitForExit();
                });

            // Check to make sure that the output had all the correct files
            if (!Utilities.FoundAllFiles(outputDirectory, outputFilename, selected.Item3))
            {
                lbl_Status.Content = "Error! Please check output directory as dump may be incomplete!";
                return;
            }

            // TODO: UNUSED
            Dictionary<string, string> templateValues = Utilities.ExtractOutputInformation(outputDirectory, outputFilename, selected.Item2, selected.Item3);

            // Special cases
            switch (selected.Item2)
            {
                case KnownSystem.MicrosoftXBOXOne:
                case KnownSystem.SonyPlayStation4:
                    if (!File.Exists(sgRawPath))
                    {
                        lbl_Status.Content = "Error! Could not find sg-raw!";
                        return;
                    }

                    Process sgraw = new Process()
                    {
                        StartInfo = new ProcessStartInfo()
                        {
                            FileName = sgRawPath,
                            Arguments = "-v -r 4100 -R " + driveLetter + ": " + "ad 01 00 00 00 00 00 00 10 04 00 00 -o \"PIC.bin\""
                        },
                    };
                    sgraw.Start();
                    sgraw.WaitForExit();
                    break;
                case KnownSystem.SonyPlayStation:
                    if (!File.Exists(psxtPath))
                    {
                        lbl_Status.Content = "Error! Could not find psxt001z!";
                        return;
                    }

                    // TODO: Direct invocation of program instead of via Batch File
                    string batchname = "PSX" + Guid.NewGuid() + ".bat";
                    using (StreamWriter writetext = new StreamWriter(batchname))
                    {
                        writetext.WriteLine(psxtPath + " " + "\"" + Utilities.GetFirstTrack(outputDirectory, outputFilename) + "\" > " + "\"" + Path.Combine(outputDirectory, "psxt001z.txt"));
                        writetext.WriteLine(psxtPath + " " + "--libcrypt " + "\"" + Path.Combine(outputDirectory, outputFilename + ".sub") + "\" > " + "\"" + Path.Combine(outputDirectory, "libcrypt.txt"));
                        writetext.WriteLine(psxtPath + " " + "--libcryptdrvfast " + driveLetter + " > " + "\"" + Path.Combine(outputDirectory, "libcryptdrv.log"));
                    }

                    Process psxt = new Process();
                    psxt.StartInfo.FileName = batchname;
                    psxt.Start();
                    psxt.WaitForExit();

                    // Now try to delete the batch file
                    try
                    {
                        File.Delete(batchname);
                    }
                    catch
                    {
                        // Right now, we don't care if the batch file can't be deleted
                    }
                    break;
            }

            btn_Start.IsEnabled = true;
        }

        /// <summary>
        /// Ensure information is consistent with the currently selected disc type
        /// </summary>
        private void EnsureDiscInformation()
        {
            // If we're on a separator, go to the next item
            var tuple = cmb_DiscType.SelectedItem as Tuple<string, KnownSystem?, DiscType?>;
            if (tuple.Item2 == null && tuple.Item3 == null)
            {
                cmb_DiscType.SelectedIndex++;
                tuple = cmb_DiscType.SelectedItem as Tuple<string, KnownSystem?, DiscType?>;
            }

            // If we're on an unsupported type, update the status accordingly
            switch (tuple.Item3)
            {
                case DiscType.NONE:
                    lbl_Status.Content = "Please select a valid disc type";
                    break;
                case DiscType.GameCubeGameDisc:
                case DiscType.GDROM:
                    lbl_Status.Content = string.Format("{0} discs are partially supported by DIC", Utilities.DiscTypeToString(tuple.Item3));
                    break;
                case DiscType.HDDVD:
                case DiscType.UMD:
                    lbl_Status.Content = string.Format("{0} discs are not currently supported by DIC", Utilities.DiscTypeToString(tuple.Item3));
                    break;
                default:
                    lbl_Status.Content = string.Format("{0} ready to dump", Utilities.DiscTypeToString(tuple.Item3));
                    break;
            }

            // If we're in a type that doesn't support drive speeds
            switch (tuple.Item3)
            {
                case DiscType.BD25:
                case DiscType.BD50:
                    cmb_DriveSpeed.IsEnabled = false;
                    break;
                default:
                    cmb_DriveSpeed.IsEnabled = true;
                    break;
            }
        }

        /// <summary>
        /// Get the default output directory name from the currently selected drive
        /// </summary>
        private void GetOutputNames()
        {
            var driveTuple = cmb_DriveLetter.SelectedItem as Tuple<char, string>;
            var discTuple = cmb_DiscType.SelectedItem as Tuple<string, KnownSystem?, DiscType?>;

            if (driveTuple != null)
            {
                if (String.IsNullOrWhiteSpace(txt_OutputDirectory.Text))
                {
                    txt_OutputDirectory.Text = defaultOutputPath;
                }

                if (discTuple != null)
                {
                    txt_OutputFilename.Text = driveTuple.Item2 + Utilities.GetDefaultExtension(discTuple.Item3);
                }
            }
        }

        #endregion
    }
}
