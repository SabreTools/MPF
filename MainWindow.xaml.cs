using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WinForms = System.Windows.Forms;
using DICUI.Data;
using DICUI.Utilities;

namespace DICUI
{
    public partial class MainWindow : Window
    {
        // Private UI-related variables
        private List<KeyValuePair<char, string>> _drives { get; set; }
        private MediaType? _currentMediaType { get; set; }
        private List<int> _driveSpeeds { get { return new List<int> { 1, 2, 3, 4, 6, 8, 12, 16, 20, 24, 32, 40, 44, 48, 52, 56, 72 }; } }
        private List<KeyValuePair<string, KnownSystem?>> _systems { get; set; }
        private List<MediaType?> _mediaTypes { get; set; }
        //private Process childProcess { get; set; }

        private OptionsWindow _optionsWindow;
        private Options _options;

        private LogWindow _logWindow;
        private DumpEnvironment _env;

        public MainWindow()
        {
            InitializeComponent();

            _logWindow = new LogWindow();
  
            // Initializes and load Options object
            _options = new Options();
            _options.Load();

            // Populate the list of systems
            PopulateSystems();

            // Populate the list of drives
            PopulateDrives();
            SetCurrentDiscType();

            // Populate the list of drive speeds
            PopulateDriveSpeeds();
            SetSupportedDriveSpeed();
        }

        #region Events

        private void btn_StartStop_Click(object sender, RoutedEventArgs e)
        {
            // Dump or stop the dump
            if ((string)btn_StartStop.Content == UIElements.StartDumping)
            {
                StartDumping();
            }
            else if ((string)btn_StartStop.Content == UIElements.StopDumping)
            {
                Tasks.CancelDumping(_env);

                if (chk_EjectWhenDone.IsChecked == true)
                {
                    Tasks.EjectDisc(_env);
                }
            }
        }

        private void btn_OutputDirectoryBrowse_Click(object sender, RoutedEventArgs e)
        {
            BrowseFolder();
            EnsureDiscInformation();
        }

        private void btn_Search_Click(object sender, RoutedEventArgs e)
        {
            PopulateDrives();
            SetCurrentDiscType();
            SetSupportedDriveSpeed();
            EnsureDiscInformation();
        }

        private void cmb_SystemType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PopulateMediaTypeAccordingToChosenSystem();
            GetOutputNames();
            EnsureDiscInformation();
        }

        private void cmb_MediaType_SelectionChanged(object sencder, SelectionChangedEventArgs e)
        {
            // TODO: This is giving people the benefit of the doubt that their change is valid
            _currentMediaType = cmb_MediaType.SelectedItem as MediaType?;
            GetOutputNames();
            EnsureDiscInformation();
        }

        private void cmb_DriveLetter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetCurrentDiscType();
            SetSupportedDriveSpeed();
            GetOutputNames();
            EnsureDiscInformation();
        }

        private void cmb_DriveSpeed_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EnsureDiscInformation();
        }

        private void tbr_Options_Click(object sender, RoutedEventArgs e)
        {
            // lazy initialization
            if (_optionsWindow == null)
            {
                _optionsWindow = new OptionsWindow(_options);
                _optionsWindow.Closed += delegate
                {
                    _optionsWindow = null;
                };
            }

            _optionsWindow.Owner = this;
            _optionsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            _optionsWindow.Refresh();
            _optionsWindow.Show();
        }

        private void txt_OutputFilename_TextChanged(object sender, TextChangedEventArgs e)
        {
            EnsureDiscInformation();
        }

        private void txt_OutputDirectory_TextChanged(object sender, TextChangedEventArgs e)
        {
            EnsureDiscInformation();
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Populate disc type according to system type
        /// </summary>
        private void PopulateMediaTypeAccordingToChosenSystem()
        {
            var currentSystem = cmb_SystemType.SelectedItem as KeyValuePair<string, KnownSystem?>?;

            if (currentSystem != null)
            {
                _mediaTypes = Validators.GetValidMediaTypes(currentSystem?.Value).ToList();
                cmb_MediaType.ItemsSource = _mediaTypes;

                cmb_MediaType.IsEnabled = _mediaTypes.Count > 1;
                cmb_MediaType.SelectedIndex = 0;
            }
            else
            {
                cmb_MediaType.IsEnabled = false;
                cmb_MediaType.ItemsSource = null;
                cmb_MediaType.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Get a complete list of supported systems and fill the combo box
        /// </summary>
        private void PopulateSystems()
        {
            _systems = Validators.CreateListOfSystems()
                .Select(i => new KeyValuePair<string, KnownSystem?>(i.Key, i.Value))
                .ToList();
            cmb_SystemType.ItemsSource = _systems;
            cmb_SystemType.DisplayMemberPath = "Key";
            cmb_SystemType.SelectedIndex = 0;
            cmb_SystemType_SelectionChanged(null, null);

            btn_StartStop.IsEnabled = false;
        }

        /// <summary>
        /// Get a complete list of active disc drives and fill the combo box
        /// </summary>
        /// <remarks>TODO: Find a way for this to periodically run, or have it hook to a "drive change" event</remarks>
        private void PopulateDrives()
        {
            // Populate the list of drives and add it to the combo box
            _drives = Validators.CreateListOfDrives()
                .Select(i => new KeyValuePair<char, string>(i.Key, i.Value))
                .ToList();
            cmb_DriveLetter.ItemsSource = _drives;
            cmb_DriveLetter.DisplayMemberPath = "Key";
            cmb_DriveLetter.SelectedIndex = 0;
            cmb_DriveLetter_SelectionChanged(null, null);

            if (cmb_DriveLetter.Items.Count > 0)
            {
                lbl_Status.Content = "Valid optical disc found! Choose your Disc Type";
                btn_StartStop.IsEnabled = (_drives.Count > 0 ? true : false);
            }
            else
            {
                lbl_Status.Content = "No valid optical disc found!";
                btn_StartStop.IsEnabled = false;
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

        private DumpEnvironment DetermineEnvironment()
        {
            DumpEnvironment env = new DumpEnvironment();

            // Paths to tools
            env.subdumpPath = _options.subdumpPath;
            env.psxtPath = _options.psxtPath;
            env.dicPath = _options.dicPath;

            // Populate all KVPs
            var driveKvp = cmb_DriveLetter.SelectedItem as KeyValuePair<char, string>?;
            var systemKvp = cmb_SystemType.SelectedValue as KeyValuePair<string, KnownSystem?>?;

            env.outputDirectory = txt_OutputDirectory.Text;
            env.outputFilename = txt_OutputFilename.Text;

            // Get the currently selected options
            env.driveLetter = (char)driveKvp?.Key;
            env.isFloppy = (driveKvp?.Value == UIElements.FloppyDriveString);

            env.dicParameters = txt_Parameters.Text;

            env.system = systemKvp?.Value;
            env.type = cmb_MediaType.SelectedItem as MediaType?;

            return env;
        }

        /// <summary>
        /// Begin the dumping process using the given inputs
        /// </summary>
        private async void StartDumping()
        {
            _env = DetermineEnvironment();

            btn_StartStop.Content = UIElements.StopDumping;
            lbl_Status.Content = "Beginning dumping process";

            _logWindow.StartDump(_env.dicParameters);
            _logWindow.Show();
            return;

            var task = Tasks.StartDumping(_env);
            Result result = await task;

            lbl_Status.Content = result ? "Dumping complete!" : result.message;
            btn_StartStop.Content = UIElements.StartDumping;

            if (chk_EjectWhenDone.IsChecked == true)
                Tasks.EjectDisc(_env);
        }

        /// <summary>
        /// Ensure information is consistent with the currently selected disc type
        /// </summary>
        private void EnsureDiscInformation()
        {
            var systemKvp = cmb_SystemType.SelectedItem as KeyValuePair<string, KnownSystem?>?;

            // If we're on a separator, go to the next item
            if (systemKvp?.Value == null)
            {
                systemKvp = cmb_SystemType.Items[++cmb_SystemType.SelectedIndex] as KeyValuePair<string, KnownSystem?>?;
            }

            var selectedSystem = systemKvp?.Value;
            MediaType? selectedMediaType = cmb_MediaType.SelectedItem as MediaType? ?? MediaType.NONE;

            Result result = EnsureCorrectInformationForSystemAndMediaType(selectedSystem, selectedMediaType);

            lbl_Status.Content = result.message;
            btn_StartStop.IsEnabled = result && (_drives.Count > 0 ? true : false);

            // If we're in a type that doesn't support drive speeds
            cmb_DriveSpeed.IsEnabled = selectedMediaType.DoesSupportDriveSpeed() && selectedSystem.DoesSupportDriveSpeed();

            // Special case for Custom input
            if (selectedSystem == KnownSystem.Custom)
            {
                txt_Parameters.IsEnabled = true;
                txt_OutputFilename.IsEnabled = false;
                txt_OutputDirectory.IsEnabled = false;
                btn_OutputDirectoryBrowse.IsEnabled = false;
                cmb_DriveLetter.IsEnabled = false;
                cmb_DriveSpeed.IsEnabled = false;
                btn_StartStop.IsEnabled = (_drives.Count > 0 ? true : false);
                lbl_Status.Content = "User input mode";
            }
            else
            {
                txt_Parameters.IsEnabled = false;
                txt_OutputFilename.IsEnabled = true;
                txt_OutputDirectory.IsEnabled = true;
                btn_OutputDirectoryBrowse.IsEnabled = true;
                cmb_DriveLetter.IsEnabled = true;

                // Populate with the correct params for inputs (if we're not on the default option)
                if (selectedSystem != KnownSystem.NONE && selectedMediaType != MediaType.NONE)
                {
                    var driveletter = cmb_DriveLetter.SelectedValue as KeyValuePair<char, string>?;

                    // If drive letter is invalid, skip this
                    if (driveletter == null)
                        return;

                    string command = Converters.KnownSystemAndMediaTypeToBaseCommand(selectedSystem, selectedMediaType);
                    List<string> defaultParams = Converters.KnownSystemAndMediaTypeToParameters(selectedSystem, selectedMediaType);
                    txt_Parameters.Text = command
                        + " " + driveletter?.Key
                        + " \"" + Path.Combine(txt_OutputDirectory.Text, txt_OutputFilename.Text) + "\" "
                        + (selectedMediaType != MediaType.Floppy
                            && selectedMediaType != MediaType.BluRay
                            && selectedSystem != KnownSystem.MicrosoftXBOX
                            && selectedSystem != KnownSystem.MicrosoftXBOX360XDG2
                            && selectedSystem != KnownSystem.MicrosoftXBOX360XDG3
                                ? (int)cmb_DriveSpeed.SelectedItem + " " : "")
                        + string.Join(" ", defaultParams);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private Result EnsureCorrectInformationForSystemAndMediaType(KnownSystem? system, MediaType? type)
        {
            // No system chosen, update status
            if (system == KnownSystem.NONE)
                return Result.Failure("Please select a valid system");
            // custom system chosen, then don't check anything
            else if (system == KnownSystem.Custom)
                return Result.Success("{0} ready to dump", type.Name());

            // If we're on an unsupported type, update the status accordingly
            switch (type)
            {
                case MediaType.NONE:
                    return Result.Failure("Please select a valid disc type");
                case MediaType.GameCubeGameDisc:
                case MediaType.GDROM:
                    return Result.Success("{0} discs are partially supported by DIC", type.Name());
                case MediaType.HDDVD:
                case MediaType.LaserDisc:
                case MediaType.CED:
                case MediaType.UMD:
                case MediaType.WiiOpticalDisc:
                case MediaType.WiiUOpticalDisc:
                case MediaType.Cartridge:
                case MediaType.Cassette:
                    return Result.Failure("{0} discs are not currently supported by DIC", type.Name());
                default:
                    if (system == KnownSystem.MicrosoftXBOX360XDG3)
                    {
                        return Result.Failure("{0} discs are not currently supported by DIC", type.Name());
                    }
                    else
                    {
                        // Take care of the selected item
                        if (_currentMediaType != null && _currentMediaType != MediaType.NONE)
                        {
                            int index = _mediaTypes.IndexOf(_currentMediaType);
                            if (index != -1)
                            {
                                if (cmb_MediaType.SelectedIndex != index)
                                {
                                    cmb_MediaType.SelectedIndex = index;
                                }
                            }
                            else
                            {
                                return Result.Success("Disc of type {0} found, but the current system does not support it!", type.Name());
                            }
                        }

                    }
                    break;
            }

            return Result.Success("{0} ready to dump", type.Name());
        }

        /// <summary>
        /// Get the default output directory name from the currently selected drive
        /// </summary>
        private void GetOutputNames()
        {
            var driveKvp = cmb_DriveLetter.SelectedItem as KeyValuePair<char, string>?;
            var systemKvp = cmb_SystemType.SelectedItem as KeyValuePair<string, KnownSystem?>?;
            MediaType? mediaType = cmb_MediaType.SelectedItem as MediaType?;

            if (driveKvp != null && (driveKvp?.Value != UIElements.FloppyDriveString) && systemKvp != null && mediaType != null)
            {
                txt_OutputDirectory.Text = Path.Combine(_options.defaultOutputPath, driveKvp?.Value);
                txt_OutputFilename.Text = driveKvp?.Value + mediaType.Extension();
            }
            else
            {
                txt_OutputDirectory.Text = _options.defaultOutputPath;
                txt_OutputFilename.Text = "disc.bin";
            }
        }

        /// <summary>
        /// Get the highest supported drive speed as reported by DiscImageCreator
        /// </summary>
        private void SetSupportedDriveSpeed()
        {
            // Get the drive letter from the selected item
            var selected = cmb_DriveLetter.SelectedItem as KeyValuePair<char, string>?;
            if (selected == null || (selected?.Value == UIElements.FloppyDriveString))
            {
                return;
            }

            //Validators.GetDriveSpeed((char)selected?.Key);
            //Validators.GetDriveSpeedEx((char)selected?.Key, MediaType.CD);

            // Validate that the required program exists and it's not DICUI itself
            if (!File.Exists(_options.dicPath) ||
                Path.GetFullPath(_options.dicPath) == Path.GetFullPath(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName))
            {
                return;
            }

            char driveLetter = (char)selected?.Key;
            Process childProcess = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = _options.dicPath,
                    Arguments = DICCommands.DriveSpeed + " " + driveLetter,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                },
            };
            childProcess.Start();
            childProcess.WaitForExit();
            string output = childProcess.StandardOutput.ReadToEnd();

            int index = output.IndexOf("ReadSpeedMaximum:");
            string readspeed = Regex.Match(output.Substring(index), @"ReadSpeedMaximum: [0-9]+KB/sec \(([0-9]*)x\)").Groups[1].Value;
            if (!Int32.TryParse(readspeed, out int speed))
            {
                return;
            }

            cmb_DriveSpeed.SelectedValue = speed;
        }

        /// <summary>
        /// Set the current disc type in the combo box
        /// </summary>
        private void SetCurrentDiscType()
        {
            // Get the drive letter from the selected item
            var selected = cmb_DriveLetter.SelectedItem as KeyValuePair<char, string>?;
            if (selected == null || (selected?.Value == UIElements.FloppyDriveString))
            {
                return;
            }

            // Get the current optical disc type
            _currentMediaType = Validators.GetDiscType(selected?.Key);

            // If we have an invalid current type, we don't care and return
            if (_currentMediaType == null || _currentMediaType == MediaType.NONE)
            {
                return;
            }

            // Now set the selected item, if possible
            int index = _mediaTypes.FindIndex(kvp => kvp.Value == _currentMediaType);
            if (index != -1)
            {
                cmb_MediaType.SelectedIndex = index;
            }
            else
            {
                lbl_Status.Content = $"Disc of type '{Converters.MediaTypeToString(_currentMediaType)}' found, but the current system does not support it!";
            }
        }

        #endregion
    }
}
