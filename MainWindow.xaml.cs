using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WinForms = System.Windows.Forms;
using DICUI.Utilities;

namespace DICUI
{
    public partial class MainWindow : Window
    {
        // Private UI-related variables
        private List<Tuple<char, string, bool>> _drives { get; set; }
        private List<int> _driveSpeeds { get { return new List<int> { 1, 2, 3, 4, 6, 8, 12, 16, 20, 24, 32, 40, 44, 48, 52, 56, 72 }; } }
        private List<Tuple<string, KnownSystem?>> _systems { get; set; }
        private List<Tuple<string, DiscType?>> _discTypes { get; set; }
        private Process childProcess { get; set; }

        private OptionsWindow _optionsWindow;
        private Options _options;

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
                CancelDumping();

                if (chk_EjectWhenDone.IsChecked == true)
                {
                    EjectDisc();
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
            SetSupportedDriveSpeed();
            EnsureDiscInformation();
        }

        private void cmb_SystemType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GetOutputNames();
            PopulateDiscTypeAccordingToChosenSystem();
            EnsureDiscInformation();
        }

        private void cmb_DiscType_SelectionChanged(object sencder, SelectionChangedEventArgs e)
        {
            GetOutputNames();
            EnsureDiscInformation();
        }

        private void cmb_DriveLetter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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
        private void PopulateDiscTypeAccordingToChosenSystem()
        {
            var currentSystem = cmb_SystemType.SelectedItem as Tuple<string, KnownSystem?>;

            if (currentSystem != null)
            {
                _discTypes = Utilities.Validation.GetValidDiscTypes(currentSystem.Item2);
                cmb_DiscType.ItemsSource = _discTypes;
                cmb_DiscType.DisplayMemberPath = "Item1";

                cmb_DiscType.IsEnabled = _discTypes.Count > 1;
                cmb_DiscType.SelectedIndex = 0;
            }
            else
            {
                cmb_DiscType.IsEnabled = false;
                cmb_DiscType.ItemsSource = null;
                cmb_DiscType.SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Get a complete list of supported systems and fill the combo box
        /// </summary>
        private void PopulateSystems()
        {
            _systems = Utilities.Validation.CreateListOfSystems();
            cmb_SystemType.ItemsSource = _systems;
            cmb_SystemType.DisplayMemberPath = "Item1";
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
            _drives = Utilities.Validation.CreateListOfDrives();
            cmb_DriveLetter.ItemsSource = _drives;
            cmb_DriveLetter.DisplayMemberPath = "Item1";
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

        /// <summary>
        /// Begin the dumping process using the given inputs
        /// </summary>
        private async void StartDumping()
        {
            btn_StartStop.Content = UIElements.StopDumping;

            // Populate all tuples
            var driveLetterTuple = cmb_DriveLetter.SelectedItem as Tuple<char, string, bool>;
            var systemTypeTuple = cmb_SystemType.SelectedValue as Tuple<string, KnownSystem?>;
            var discTypeTuple = cmb_DiscType.SelectedValue as Tuple<string, DiscType?>;

            // Get the currently selected options
            string dicPath = _options.dicPath;
            string psxtPath = _options.psxtPath;
            string subdumpPath = _options.subdumpPath;
            char driveLetter = driveLetterTuple.Item1;
            bool isFloppy = driveLetterTuple.Item3;
            string outputDirectory = txt_OutputDirectory.Text;
            string outputFilename = txt_OutputFilename.Text;
            string systemName = systemTypeTuple.Item1;
            KnownSystem? system = systemTypeTuple.Item2;
            DiscType? type = discTypeTuple.Item2;

            string customParameters = txt_Parameters.Text;

            // Validate that everything is good
            if (string.IsNullOrWhiteSpace(customParameters)
                || !Utilities.Validation.ValidateParameters(customParameters)
                || (isFloppy ^ type == DiscType.Floppy))
            {
                lbl_Status.Content = "Error! Current configuration is not supported!";
                btn_StartStop.Content = UIElements.StartDumping;
                return;
            }

            // If we have a custom configuration, we need to extract the best possible information from it
            if (system == KnownSystem.Custom)
            {
                Utilities.Validation.DetermineFlags(customParameters, out type, out system, out string letter, out string path);
                driveLetter = letter[0];
                outputDirectory = Path.GetDirectoryName(path);
                outputFilename = Path.GetFileName(path);
            }

            // Validate that the required program exits
            if (!File.Exists(dicPath))
            {
                lbl_Status.Content = "Error! Could not find DiscImageCreator!";
                btn_StartStop.Content = UIElements.StartDumping;
                return;
            }

            // If a complete dump already exists
            if (DumpInformation.FoundAllFiles(outputDirectory, outputFilename, type))
            {
                MessageBoxResult result = MessageBox.Show("A complete dump already exists! Are you sure you want to overwrite?", "Overwrite?", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                if (result == MessageBoxResult.No || result == MessageBoxResult.Cancel || result == MessageBoxResult.None)
                {
                    lbl_Status.Content = "Dumping aborted!";
                    btn_StartStop.Content = UIElements.StartDumping;
                    return;
                }
            }

            lbl_Status.Content = "Beginning dumping process";
            string parameters = txt_Parameters.Text;

            await Task.Run(() =>
            {
                childProcess = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = dicPath,
                        Arguments = parameters,
                    },
                };
                childProcess.Start();
                childProcess.WaitForExit();
            });

            // Special cases
            switch (system)
            {
                case KnownSystem.SegaSaturn:
                    if (!File.Exists(subdumpPath))
                    {
                        lbl_Status.Content = "Error! Could not find subdump!";
                        break;
                    }

                    await Task.Run(() =>
                    {
                        childProcess = new Process()
                        {
                            StartInfo = new ProcessStartInfo()
                            {
                                FileName = subdumpPath,
                                Arguments = "-i " + driveLetter + ": -f " + Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(outputFilename) + "_subdump.sub") + "-mode 6 -rereadnum 25 -fix 2",
                            },
                        };
                        childProcess.Start();
                        childProcess.WaitForExit();
                    });
                    break;
                case KnownSystem.SonyPlayStation:
                    if (!File.Exists(psxtPath))
                    {
                        lbl_Status.Content = "Error! Could not find psxt001z!";
                        break;
                    }

                    // Invoke the program with all 3 configurations
                    // TODO: Use these outputs for PSX information
                    await Task.Run(() =>
                    {
                        childProcess = new Process()
                        {
                            StartInfo = new ProcessStartInfo()
                            {
                                FileName = psxtPath,
                                Arguments = "\"" + DumpInformation.GetFirstTrack(outputDirectory, outputFilename) + "\" > " + "\"" + Path.Combine(outputDirectory, "psxt001z.txt"),
                            },
                        };
                        childProcess.Start();
                        childProcess.WaitForExit();

                        childProcess = new Process()
                        {
                            StartInfo = new ProcessStartInfo()
                            {
                                FileName = psxtPath,
                                Arguments = "--libcrypt \"" + Path.Combine(outputDirectory, Path.GetFileNameWithoutExtension(outputFilename) + ".sub") + "\" > \"" + Path.Combine(outputDirectory, "libcrypt.txt"),
                            },
                        };
                        childProcess.Start();
                        childProcess.WaitForExit();

                        childProcess = new Process()
                        {
                            StartInfo = new ProcessStartInfo()
                            {
                                FileName = psxtPath,
                                Arguments = "--libcryptdrvfast " + driveLetter + " > " + "\"" + Path.Combine(outputDirectory, "libcryptdrv.log"),
                            },
                        };
                        childProcess.Start();
                        childProcess.WaitForExit();
                    });
                    break;
            }

            // Check to make sure that the output had all the correct files
            if (!DumpInformation.FoundAllFiles(outputDirectory, outputFilename, type))
            {
                lbl_Status.Content = "Error! Please check output directory as dump may be incomplete!";
                btn_StartStop.Content = UIElements.StartDumping;
                if (chk_EjectWhenDone.IsChecked == true)
                {
                    EjectDisc();
                }
                return;
            }

            Dictionary<string, string> templateValues = DumpInformation.ExtractOutputInformation(outputDirectory, outputFilename, system, type, driveLetter);
            List<string> formattedValues = DumpInformation.FormatOutputData(templateValues, system, type);
            bool success = DumpInformation.WriteOutputData(outputDirectory, outputFilename, formattedValues);

            lbl_Status.Content = "Dumping complete!";
            btn_StartStop.Content = UIElements.StartDumping;
            if (chk_EjectWhenDone.IsChecked == true)
            {
                EjectDisc();
            }
        }

        /// <summary>
        /// Cancel an in-progress dumping process
        /// </summary>
        private void CancelDumping()
        {
            try
            {
                childProcess.Kill();
            }
            catch
            { }
        }

        /// <summary>
        /// Eject the disc using DIC
        /// </summary>
        private async void EjectDisc()
        {
            // Validate that the required program exits
            if (!File.Exists(_options.dicPath))
            {
                return;
            }

            CancelDumping();

            var driveTuple = cmb_DriveLetter.SelectedItem as Tuple<char, string, bool>;
            if (driveTuple.Item3)
            {
                return;
            }

            await Task.Run(() =>
            {
                childProcess = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = _options.dicPath,
                        Arguments = DICCommands.Eject + " " + driveTuple.Item1,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                    },
                };
                childProcess.Start();
                childProcess.WaitForExit();
            });
        }

        /// <summary>
        /// Ensure information is consistent with the currently selected disc type
        /// </summary>
        private void EnsureDiscInformation()
        {
            var systemTuple = cmb_SystemType.SelectedItem as Tuple<string, KnownSystem?>;
            var discTypeTuple = cmb_DiscType.SelectedItem as Tuple<string, DiscType?>;

            // If we're on a separator, go to the next item
            if (systemTuple.Item2 == null)
                systemTuple = cmb_SystemType.Items[++cmb_SystemType.SelectedIndex] as Tuple<string, KnownSystem?>;

            var selectedSystem = systemTuple.Item2;
            var selectedDiscType = discTypeTuple != null ? discTypeTuple.Item2 : DiscType.NONE;

            // No system chosen, update status
            if (selectedSystem == KnownSystem.NONE)
            {
                lbl_Status.Content = "Please select a valid system";
                btn_StartStop.IsEnabled = false;
            }
            else if (selectedSystem != KnownSystem.Custom)
            {
                // If we're on an unsupported type, update the status accordingly
                switch (selectedDiscType)
                {
                    case DiscType.NONE:
                        lbl_Status.Content = "Please select a valid disc type";
                        btn_StartStop.IsEnabled = false;
                        break;
                    case DiscType.GameCubeGameDisc:
                    case DiscType.GDROM:
                        lbl_Status.Content = string.Format("{0} discs are partially supported by DIC", discTypeTuple.Item1);
                        btn_StartStop.IsEnabled = (_drives.Count > 0 ? true : false);
                        break;
                    case DiscType.HDDVD:
                    case DiscType.LaserDisc:
                    case DiscType.CED:
                    case DiscType.UMD:
                    case DiscType.WiiOpticalDisc:
                    case DiscType.WiiUOpticalDisc:
                    case DiscType.Cartridge:
                    case DiscType.Cassette:
                        lbl_Status.Content = string.Format("{0} discs are not currently supported by DIC", discTypeTuple.Item1);
                        btn_StartStop.IsEnabled = false;
                        break;
                    case DiscType.DVD:
                        if (selectedSystem == KnownSystem.MicrosoftXBOX360XDG3)
                        {
                            lbl_Status.Content = string.Format("{0} discs are not currently supported by DIC", discTypeTuple.Item1);
                            btn_StartStop.IsEnabled = false;
                        }
                        else
                        {
                            lbl_Status.Content = string.Format("{0} ready to dump", discTypeTuple.Item1);
                            btn_StartStop.IsEnabled = (_drives.Count > 0 ? true : false);
                        }
                        break;
                    default:
                        lbl_Status.Content = string.Format("{0} ready to dump", discTypeTuple.Item1);
                        btn_StartStop.IsEnabled = (_drives.Count > 0 ? true : false);
                        break;
                }
            }

            // If we're in a type that doesn't support drive speeds
            switch (selectedDiscType)
            {
                case DiscType.Floppy:
                case DiscType.BluRay:
                    cmb_DriveSpeed.IsEnabled = false;
                    break;
                default:
                    if (selectedSystem == KnownSystem.MicrosoftXBOX
                        || selectedSystem == KnownSystem.MicrosoftXBOX360XDG2
                        || selectedSystem == KnownSystem.MicrosoftXBOX360XDG3)
                    {
                        cmb_DriveSpeed.IsEnabled = false;
                    }
                    else
                    {
                        cmb_DriveSpeed.IsEnabled = true;
                    }
                    break;
            }

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
                if (selectedSystem != KnownSystem.NONE && selectedDiscType != DiscType.NONE)
                {
                    var driveletter = cmb_DriveLetter.SelectedValue as Tuple<char, string, bool>;

                    // If drive letter is invalid, skip this
                    if (driveletter == null)
                        return;

                    string discType = Converters.KnownSystemAndDiscTypeToBaseCommand(selectedSystem, selectedDiscType);
                    List<string> defaultParams = Converters.KnownSystemAndDiscTypeToParameters(selectedSystem, selectedDiscType);
                    txt_Parameters.Text = discType
                        + " " + driveletter.Item1
                        + " \"" + Path.Combine(txt_OutputDirectory.Text, txt_OutputFilename.Text) + "\" "
                        + (selectedDiscType != DiscType.Floppy
                            && selectedDiscType != DiscType.BluRay
                            && selectedSystem != KnownSystem.MicrosoftXBOX
                            && selectedSystem != KnownSystem.MicrosoftXBOX360XDG2
                            && selectedSystem != KnownSystem.MicrosoftXBOX360XDG3
                                ? (int)cmb_DriveSpeed.SelectedItem + " " : "")
                        + string.Join(" ", defaultParams);
                }
            }
        }

        /// <summary>
        /// Get the default output directory name from the currently selected drive
        /// </summary>
        private void GetOutputNames()
        {
            var driveTuple = cmb_DriveLetter.SelectedItem as Tuple<char, string, bool>;
            var systemTuple = cmb_SystemType.SelectedItem as Tuple<string, KnownSystem?>;
            var discTuple = cmb_DiscType.SelectedItem as Tuple<string, DiscType?>;

            if (driveTuple != null && systemTuple != null && discTuple != null)
            {
                txt_OutputDirectory.Text = Path.Combine(_options.defaultOutputPath, driveTuple.Item2);
                txt_OutputFilename.Text = driveTuple.Item2 + Converters.DiscTypeToExtension(discTuple.Item2);
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
            var selected = cmb_DriveLetter.SelectedItem as Tuple<char, string, bool>;
            if (selected == null || selected.Item3)
            {
                return;
            }

            // Validate that the required program exists and it's not DICUI itself
            if (!File.Exists(_options.dicPath) || 
                Path.GetFullPath(_options.dicPath) == Path.GetFullPath(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName))
            {
                return;
            }

            char driveLetter = selected.Item1;
            childProcess = new Process()
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

        #endregion
    }
}
