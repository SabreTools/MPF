using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
        private List<KeyValuePair<string, KnownSystem?>> _systems { get; set; }
        private List<MediaType?> _mediaTypes { get; set; }

        private DumpEnvironment _env;

        // Option related
        private Options _options;
        private OptionsWindow _optionsWindow;

        public MainWindow()
        {
            InitializeComponent();

            // Initializes and load Options object
            _options = new Options();
            _options.Load();

            // Populate the list of systems
            PopulateSystems();

            // Populate the list of drives
            PopulateDrives();
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
        }

        private void cmb_SystemType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PopulateMediaTypeAccordingToChosenSystem();
        }

        private void cmb_MediaType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Only change the media type if the selection and not the list has changed
            if (e.RemovedItems.Count == 1 && e.AddedItems.Count == 1)
            {
                _currentMediaType = cmb_MediaType.SelectedItem as MediaType?;
            }

            // TODO: This is giving people the benefit of the doubt that their change is valid
            SetSupportedDriveSpeed();
            GetOutputNames();
        }

        private void cmb_DriveLetter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetCurrentDiscType();
            GetOutputNames();
            SetSupportedDriveSpeed();
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
                _optionsWindow = new OptionsWindow(this, _options);
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

        public void OnOptionsUpdated()
        {
            GetOutputNames();
            //TODO: here we should adjust maximum speed if it changed in options
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
                cmb_MediaType.SelectedIndex = (_mediaTypes.IndexOf(_currentMediaType) >= 0 ? _mediaTypes.IndexOf(_currentMediaType) : 0);
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

            if (cmb_DriveLetter.Items.Count > 0)
            {
                cmb_DriveLetter.SelectedIndex = 0;
                lbl_Status.Content = "Valid media found! Choose your Media Type";
                btn_StartStop.IsEnabled = true;
            }
            else
            {
                cmb_DriveLetter.SelectedIndex = -1;
                lbl_Status.Content = "No valid media found!";
                btn_StartStop.IsEnabled = false;
            }
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
        /// Create a DumpEnvironment with all current settings
        /// </summary>
        /// <returns>Filled DumpEnvironment instance</returns>
        private DumpEnvironment DetermineEnvironment()
        {
            // Populate all KVPs
            var driveKvp = cmb_DriveLetter.SelectedItem as KeyValuePair<char, string>?;
            var systemKvp = cmb_SystemType.SelectedValue as KeyValuePair<string, KnownSystem?>?;

            return new DumpEnvironment()
            {
                // Paths to tools
                SubdumpPath = _options.subdumpPath,
                DICPath = _options.dicPath,

                OutputDirectory = txt_OutputDirectory.Text,
                OutputFilename = txt_OutputFilename.Text,

                // Get the currently selected options
                DriveLetter = (char)driveKvp?.Key,
                IsFloppy = (driveKvp?.Value == UIElements.FloppyDriveString),

                DICParameters = txt_Parameters.Text,

                System = systemKvp?.Value,
                Type = cmb_MediaType.SelectedItem as MediaType?
            };
        }

        /// <summary>
        /// Begin the dumping process using the given inputs
        /// </summary>
        private async void StartDumping()
        {
            _env = DetermineEnvironment();

            btn_StartStop.Content = UIElements.StopDumping;
            lbl_Status.Content = "Beginning dumping process";

            var task = Tasks.StartDumping(_env);
            Result result = await task;

            lbl_Status.Content = result ? "Dumping complete!" : result.Message;
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

            // If we're on a separator, go to the next item and return
            if (systemKvp?.Value == null)
            {
                cmb_SystemType.SelectedIndex++;
                return;
            }

            // Get the selected system info
            var selectedSystem = systemKvp?.Value;
            MediaType? selectedMediaType = cmb_MediaType.SelectedItem as MediaType? ?? MediaType.NONE;

            Result result = GetSupportStatus(selectedSystem, selectedMediaType);

            lbl_Status.Content = result.Message;
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
                                ? (int?)cmb_DriveSpeed.SelectedItem + " " : "")
                        + string.Join(" ", defaultParams);
                }
            }
        }

        /// <summary>
        /// Verify that, given a system and a media type, they are correct
        /// </summary>
        private Result GetSupportStatus(KnownSystem? system, MediaType? type)
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
                // Fully supported types
                case MediaType.CD:
                case MediaType.DVD:
                case MediaType.HDDVD:
                case MediaType.BluRay:
                    if (system == KnownSystem.MicrosoftXBOX360XDG3)
                    {
                        return Result.Failure("{0} discs are not currently supported by DIC", type.Name());
                    }
                    else
                    {
                        // TODO: this code should adjust things in a method which is meant to verify values so maybe we can find a better fit 
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
                                return Result.Success("Disc of type {0} found, but the current system does not support it!", _currentMediaType.Name());
                            }
                        }
                    }
                    return Result.Success("{0} ready to dump", type.Name());

                // Partially supported types
                case MediaType.GDROM:
                case MediaType.GameCubeGameDisc:
                case MediaType.WiiOpticalDisc:
                case MediaType.WiiUOpticalDisc:
                    return Result.Success("{0} discs are partially supported by DIC", type.Name());

                // Undumpable but recognized types
                case MediaType.LaserDisc:
                case MediaType.CED:
                case MediaType.UMD:
                case MediaType.Cartridge:
                case MediaType.Cassette:
                    return Result.Failure("{0} discs are not currently supported by DIC", type.Name());

                // Invalid or unknown types
                case MediaType.NONE:
                default:
                    return Result.Failure("Please select a valid disc type");
            }
        }

        /// <summary>
        /// Get the default output directory name from the currently selected drive
        /// </summary>
        private void GetOutputNames()
        {
            var driveKvp = cmb_DriveLetter.SelectedItem as KeyValuePair<char, string>?;
            var systemKvp = cmb_SystemType.SelectedItem as KeyValuePair<string, KnownSystem?>?;
            MediaType? mediaType = cmb_MediaType.SelectedItem as MediaType?;

            if (driveKvp != null
                && !String.IsNullOrWhiteSpace(driveKvp?.Value)
                && driveKvp?.Value != UIElements.FloppyDriveString
                && systemKvp != null
                && mediaType != null)
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
            // Set the drive speed list that's appropriate
            var values = UIElements.GetAllowedDriveSpeedsForMediaType(_currentMediaType);
            cmb_DriveSpeed.ItemsSource = values;
            cmb_DriveSpeed.SelectedIndex = values.Count / 2;

            // Get the drive letter from the selected item
            var selected = cmb_DriveLetter.SelectedItem as KeyValuePair<char, string>?;
            if (selected == null || (selected?.Value == UIElements.FloppyDriveString))
            {
                return;
            }

            //Validators.GetDriveSpeed((char)selected?.Key);
            //Validators.GetDriveSpeedEx((char)selected?.Key, _currentMediaType);

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
            if (!Int32.TryParse(readspeed, out int speed) || speed <= 0)
            {
                return;
            }

            // choose speed value according to maximum value reported by DIC adjusted to a precise speed from list
            // and the one choosen in options
            int chosenSpeed = Math.Min(
                UIElements.GetAllowedDriveSpeedsForMediaType(_currentMediaType).Where(s => s <= speed).Last(),
                _options.preferredDumpSpeedCD
            );

            cmb_DriveSpeed.SelectedValue = chosenSpeed;
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
