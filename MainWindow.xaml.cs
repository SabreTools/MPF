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
        private const string dicPath = "Programs\\DiscImageCreator.exe"; // TODO: Make configurable in UI or in Settings
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
            string outputFileName = txt_OutputFilename.Text;
            int driveSpeed = (int)cmb_DriveSpeed.SelectedItem;
            btn_Start.IsEnabled = false;

            // Get the discType and processArguments from a given system and disc combo
            var selected = cmb_DiscType.SelectedValue as Tuple<string, KnownSystem?, DiscType?>;
            string discType = Utilities.GetBaseCommand(selected.Item3);
            string defaultArguments = string.Join(" ", Utilities.GetDefaultParameters(selected.Item2, selected.Item3));

            await Task.Run(
                () =>
                {
                    Process process = new Process();
                    process.StartInfo.FileName = dicPath;
                    process.StartInfo.Arguments = discType + " " + driveLetter + " \"" + Path.Combine(outputDirectory, outputFileName) + "\" " + driveSpeed + " " + defaultArguments;
                    process.Start();
                    process.WaitForExit();
                });

            // Special cases
            string guid = Guid.NewGuid().ToString();
            switch (selected.Item2)
            {
                case KnownSystem.MicrosoftXBOXOne:
                case KnownSystem.SonyPlayStation4:
                    Process sgraw = new Process()
                    {
                        StartInfo = new ProcessStartInfo()
                        {
                            FileName = "sg_raw.exe", // TODO: Make this configurable
                            Arguments = "-v -r 4100 -R " + driveLetter + ": " + "ad 01 00 00 00 00 00 00 10 04 00 00 -o \"PIC.bin\""
                        },
                    };
                    sgraw.Start();
                    sgraw.WaitForExit();
                    break;
                case KnownSystem.SonyPlayStation:
                    // TODO: Direct invocation of program instead of via Batch File
                    using (StreamWriter writetext = new StreamWriter("PSX" + guid + ".bat"))
                    {
                        writetext.WriteLine("psxt001z" + " " + "\"" + outputDirectory + "\\" + outputFileName + ".bin" + "\" > " + "\"" + outputDirectory + "\\" + "psxt001z1.txt");
                        writetext.WriteLine("psxt001z" + " " + "\"" + outputDirectory + "\\" + outputFileName + " (Track 1).bin" + "\" > " + "\"" + outputDirectory + "\\" + "psxt001z2.txt");
                        writetext.WriteLine("psxt001z" + " " + "\"" + outputDirectory + "\\" + outputFileName + " (Track 01).bin" + "\" > " + "\"" + outputDirectory + "\\" + "psxt001z3.txt");
                        writetext.WriteLine("psxt001z" + " " + "--libcrypt " + "\"" + outputDirectory + "\\" + outputFileName + ".sub\" > " + "\"" + outputDirectory + "\\" + "libcrypt.txt");
                        writetext.WriteLine("psxt001z" + " " + "--libcryptdrvfast " + driveLetter + " > " + "\"" + outputDirectory + "\\" + "libcryptdrv.log");
                    }
                    Process psxt = new Process();
                    psxt.StartInfo.FileName = "PSX" + guid + ".bat";
                    psxt.Start();
                    psxt.WaitForExit();
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
        /// <remarks>TODO: Make the "ISO" part of the default configurable in UI or Settings</remarks>
        private void GetOutputNames()
        {
            var driveTuple = cmb_DriveLetter.SelectedItem as Tuple<char, string>;
            var discTuple = cmb_DiscType.SelectedItem as Tuple<string, KnownSystem?, DiscType?>;

            if (driveTuple != null)
            {
                if (String.IsNullOrWhiteSpace(txt_OutputDirectory.Text))
                {
                    txt_OutputDirectory.Text = "ISO";
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
