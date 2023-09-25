using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BurnOutSharp;
using MPF.Core.Data;
using MPF.Core.Utilities;
using MPF.Library;
using MPF.UI.Core.ComboBoxItems;
using MPF.UI.Core.ViewModels;
using MPF.UI.Core.Windows;
using MPF.Windows;
using SabreTools.RedumpLib.Data;
using WPFCustomMessageBox;
using WinForms = System.Windows.Forms;
using MPF.UI.Core;

namespace MPF.UI.ViewModels
{
    public class MainViewModel
    {
        #region Fields

        /// <summary>
        /// Currently selected or detected media type
        /// </summary>
        public MediaType? CurrentMediaType { get; set; }

        /// <summary>
        /// Current list of drives
        /// </summary>
        public List<Drive> Drives { get; set; }

        /// <summary>
        /// Current dumping environment
        /// </summary>
        public DumpEnvironment Env { get; set; }

        #endregion

        #region Lists

        /// <summary>
        /// Current list of supported media types
        /// </summary>
        public List<Element<MediaType>> MediaTypes { get; set; } = new List<Element<MediaType>>();

        /// <summary>
        /// Current list of supported system profiles
        /// </summary>
        public List<RedumpSystemComboBoxItem> Systems { get; set; } = RedumpSystemComboBoxItem.GenerateElements().ToList();

        /// <summary>
        /// List of available internal programs
        /// </summary>
        public List<Element<InternalProgram>> InternalPrograms { get; set; } = new List<Element<InternalProgram>>();

        #endregion

        #region Private Event Flags

        /// <summary>
        /// Indicates if SelectionChanged events can be executed
        /// </summary>
        private bool _canExecuteSelectionChanged = false;

        #endregion

        /// <summary>
        /// Initialize the main window after loading
        /// </summary>
        public void Init()
        {
            // Load the log output
            App.Instance.LogPanel.IsExpanded = App.Options.OpenLogWindowAtStartup;

            // Disable buttons until we load fully
            App.Instance.StartStopButton.IsEnabled = false;
            App.Instance.MediaScanButton.IsEnabled = false;
            App.Instance.UpdateVolumeLabel.IsEnabled = false;
            App.Instance.CopyProtectScanButton.IsEnabled = false;

            // Add the click handlers to the UI
            AddEventHandlers(App.Instance);

            // Display the debug option in the menu, if necessary
            if (App.Options.ShowDebugViewMenuItem)
                App.Instance.DebugViewMenuItem.Visibility = Visibility.Visible;

            // Finish initializing the rest of the values
            InitializeUIValues(App.Instance, App.Logger, App.Options, removeEventHandlers: false, rescanDrives: true);

            // Check for updates, if necessary
            if (App.Options.CheckForUpdatesOnStartup)
                CheckForUpdates(App.Instance, App.Logger, showIfSame: false);
        }

        #region Population

        /// <summary>
        /// Get a complete list of active disc drives and fill the combo box
        /// </summary>
        /// <remarks>TODO: Find a way for this to periodically run, or have it hook to a "drive change" event</remarks>
        private void PopulateDrives(MainWindow instance, LogViewModel logger, MPF.Core.Data.Options options)
        {
            if (options.VerboseLogging)
                logger.VerboseLogLn("Scanning for drives..");

            // Always enable the media scan
            instance.MediaScanButton.IsEnabled = true;
            instance.UpdateVolumeLabel.IsEnabled = true;

            // If we have a selected drive, keep track of it
            char? lastSelectedDrive = (instance.DriveLetterComboBox.SelectedValue as Drive)?.Letter;

            // Populate the list of drives and add it to the combo box
            Drives = Drive.CreateListOfDrives(options.IgnoreFixedDrives);
            instance.DriveLetterComboBox.ItemsSource = Drives;

            if (instance.DriveLetterComboBox.Items.Count > 0)
            {
                if (options.VerboseLogging)
                    logger.VerboseLogLn($"Found {Drives.Count} drives: {string.Join(", ", Drives.Select(d => d.Letter))}");

                // Check for the last selected drive, if possible
                int index = -1;
                if (lastSelectedDrive != null)
                    index = Drives.FindIndex(d => d.MarkedActive && d.Letter == lastSelectedDrive);

                // Check for active optical drives
                if (index == -1)
                    index = Drives.FindIndex(d => d.MarkedActive && d.InternalDriveType == InternalDriveType.Optical);

                // Check for active floppy drives
                if (index == -1)
                    index = Drives.FindIndex(d => d.MarkedActive && d.InternalDriveType == InternalDriveType.Floppy);

                // Check for any active drives
                if (index == -1)
                    index = Drives.FindIndex(d => d.MarkedActive);

                // Set the selected index
                instance.DriveLetterComboBox.SelectedIndex = (index != -1 ? index : 0);
                instance.StatusLabel.Text = "Valid drive found! Choose your Media Type";
                instance.CopyProtectScanButton.IsEnabled = true;

                // Get the current system type
                if (index != -1)
                    DetermineSystemType(instance, logger, options);

                // Only enable the start/stop if we don't have the default selected
                instance.StartStopButton.IsEnabled = ShouldEnableDumpingButton(instance);
            }
            else
            {
                if (options.VerboseLogging)
                    logger.VerboseLogLn("Found no drives");
                instance.DriveLetterComboBox.SelectedIndex = -1;
                instance.StatusLabel.Text = "No valid drive found!";
                instance.StartStopButton.IsEnabled = false;
                instance.CopyProtectScanButton.IsEnabled = false;
            }

            // Ensure the UI gets updated
            instance.UpdateLayout();
        }

        /// <summary>
        /// Populate media type according to system type
        /// </summary>
        private void PopulateMediaType(MainWindow instance)
        {
            RedumpSystem? currentSystem = instance.SystemTypeComboBox.SelectedItem as RedumpSystemComboBoxItem;

            if (currentSystem != null)
            {
                var mediaTypeValues = currentSystem.MediaTypes();
                MediaTypes = Element<MediaType>.GenerateElements().Where(m => mediaTypeValues.Contains(m.Value)).ToList();
                instance.MediaTypeComboBox.ItemsSource = MediaTypes;

                instance.MediaTypeComboBox.IsEnabled = MediaTypes.Count > 1;
                int currentIndex = MediaTypes.FindIndex(m => m == CurrentMediaType);
                instance.MediaTypeComboBox.SelectedIndex = (currentIndex > -1 ? currentIndex : 0);
            }
            else
            {
                instance.MediaTypeComboBox.IsEnabled = false;
                instance.MediaTypeComboBox.ItemsSource = null;
                instance.MediaTypeComboBox.SelectedIndex = -1;
            }

            // Ensure the UI gets updated
            instance.UpdateLayout();
        }

        /// <summary>
        /// Populate media type according to system type
        /// </summary>
        private void PopulateInternalPrograms(MainWindow instance, MPF.Core.Data.Options options)
        {
            // Get the current internal program
            InternalProgram internalProgram = options.InternalProgram;

            // Create a static list of supported programs, not everything
            var internalPrograms = new List<InternalProgram> { InternalProgram.DiscImageCreator, InternalProgram.Aaru, InternalProgram.Redumper };
            InternalPrograms = internalPrograms.Select(ip => new Element<InternalProgram>(ip)).ToList();
            instance.DumpingProgramComboBox.ItemsSource = InternalPrograms;

            // Select the current default dumping program
            int currentIndex = InternalPrograms.FindIndex(m => m == internalProgram);
            instance.DumpingProgramComboBox.SelectedIndex = (currentIndex > -1 ? currentIndex : 0);

            // Ensure the UI gets updated
            instance.UpdateLayout();
        }

        #endregion

        #region UI Commands

        /// <summary>
        /// Change the currently selected dumping program
        /// </summary>
        public void ChangeDumpingProgram(MainWindow instance, LogViewModel logger, MPF.Core.Data.Options options)
        {
            if (options.VerboseLogging)
                logger.VerboseLogLn($"Changed dumping program to: {(instance.DumpingProgramComboBox.SelectedItem as Element<InternalProgram>).Name}");
            EnsureDiscInformation(instance, options);
            GetOutputNames(instance, logger, options, false);
        }

        /// <summary>
        /// Change the currently selected media type
        /// </summary>
        public void ChangeMediaType(MainWindow instance, LogViewModel logger, MPF.Core.Data.Options options, SelectionChangedEventArgs e)
        {
            // Only change the media type if the selection and not the list has changed
            if (e.RemovedItems.Count == 1 && e.AddedItems.Count == 1)
            {
                var selectedMediaType = instance.MediaTypeComboBox.SelectedItem as Element<MediaType>;
                CurrentMediaType = selectedMediaType.Value;
                SetSupportedDriveSpeed(instance, logger, options);
            }

            GetOutputNames(instance, logger, options, false);
            EnsureDiscInformation(instance, options);
        }

        /// <summary>
        /// Change the currently selected system
        /// </summary>
        public void ChangeSystem(MainWindow instance, LogViewModel logger, MPF.Core.Data.Options options)
        {
            if (options.VerboseLogging)
                logger.VerboseLogLn($"Changed system to: {(instance.SystemTypeComboBox.SelectedItem as RedumpSystemComboBoxItem).Name}");
            PopulateMediaType(instance);
            GetOutputNames(instance, logger, options, false);
            EnsureDiscInformation(instance, options);
        }

        /// <summary>
        /// Check for available updates
        /// </summary>
        /// <param name="showIfSame">True to show the box even if it's the same, false to only show if it's different</param>
        public void CheckForUpdates(MainWindow instance, LogViewModel logger, bool showIfSame)
        {
            (bool different, string message, string url) = Tools.CheckForNewVersion();

            // If we have a new version, put it in the clipboard
            if (different)
                Clipboard.SetText(url);

            logger.SecretLogLn(message);
            if (url == null)
                message = "An exception occurred while checking for versions, please try again later. See the log window for more details.";

            if (showIfSame || different)
                CustomMessageBox.Show(instance, message, "Version Update Check", MessageBoxButton.OK, different ? MessageBoxImage.Exclamation : MessageBoxImage.Information);
        }

        /// <summary>
        /// Shutdown the current application
        /// </summary>
        public void ExitApplication() => Application.Current.Shutdown();

        /// <summary>
        /// Set the output path from a dialog box
        /// </summary>
        public void SetOutputPath(MainWindow instance, MPF.Core.Data.Options options)
        {
            BrowseFile(instance, options);
            EnsureDiscInformation(instance, options);
        }

        /// <summary>
        /// Show the About text popup
        /// </summary>
        public void ShowAboutText(MainWindow instance, LogViewModel logger)
        {
            string aboutText = $"Media Preservation Frontend (MPF)"
                + $"{Environment.NewLine}"
                + $"{Environment.NewLine}A community preservation frontend developed in C#."
                + $"{Environment.NewLine}Supports Redumper, Aaru, and DiscImageCreator."
                + $"{Environment.NewLine}Originally created to help the Redump project."
                + $"{Environment.NewLine}"
                + $"{Environment.NewLine}Thanks to everyone who has supported this project!"
                + $"{Environment.NewLine}"
                + $"{Environment.NewLine}Version {Tools.GetCurrentVersion()}";

            logger.SecretLogLn(aboutText);
            CustomMessageBox.Show(instance, aboutText, "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Build a dummy SubmissionInfo and display it for testing
        /// </summary>
        public void ShowDebugDiscInfoWindow()
        {
            var submissionInfo = new SubmissionInfo()
            {
                SchemaVersion = 1,
                FullyMatchedID = 3,
                PartiallyMatchedIDs = new List<int> { 0, 1, 2, 3 },
                Added = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,

                CommonDiscInfo = new CommonDiscInfoSection()
                {
                    System = RedumpSystem.IBMPCcompatible,
                    Media = DiscType.BD128,
                    Title = "Game Title",
                    ForeignTitleNonLatin = "Foreign Game Title",
                    DiscNumberLetter = "1",
                    DiscTitle = "Install Disc",
                    Category = DiscCategory.Games,
                    Region = Region.World,
                    Languages = new Language?[] { Language.English, Language.Spanish, Language.French },
                    LanguageSelection = new LanguageSelection?[] { LanguageSelection.BiosSettings },
                    Serial = "Disc Serial",
                    Layer0MasteringRing = "L0 Mastering Ring",
                    Layer0MasteringSID = "L0 Mastering SID",
                    Layer0ToolstampMasteringCode = "L0 Toolstamp",
                    Layer0MouldSID = "L0 Mould SID",
                    Layer0AdditionalMould = "L0 Additional Mould",
                    Layer1MasteringRing = "L1 Mastering Ring",
                    Layer1MasteringSID = "L1 Mastering SID",
                    Layer1ToolstampMasteringCode = "L1 Toolstamp",
                    Layer1MouldSID = "L1 Mould SID",
                    Layer1AdditionalMould = "L1 Additional Mould",
                    Layer2MasteringRing = "L2 Mastering Ring",
                    Layer2MasteringSID = "L2 Mastering SID",
                    Layer2ToolstampMasteringCode = "L2 Toolstamp",
                    Layer3MasteringRing = "L3 Mastering Ring",
                    Layer3MasteringSID = "L3 Mastering SID",
                    Layer3ToolstampMasteringCode = "L3 Toolstamp",
                    RingWriteOffset = "+12",
                    Barcode = "UPC Barcode",
                    EXEDateBuildDate = "19xx-xx-xx",
                    ErrorsCount = "0",
                    Comments = "Comment data line 1\r\nComment data line 2",
#if NET48
                    CommentsSpecialFields = new Dictionary<SiteCode?, string>()
#else
                    CommentsSpecialFields = new Dictionary<SiteCode, string>()
#endif
                    {
                        [SiteCode.ISBN] = "ISBN",
                    },
                    Contents = "Special contents 1\r\nSpecial contents 2",
#if NET48
                    ContentsSpecialFields = new Dictionary<SiteCode?, string>()
#else
                    ContentsSpecialFields = new Dictionary<SiteCode, string>()
#endif
                    {
                        [SiteCode.PlayableDemos] = "Game Demo 1",
                    },
                },

                VersionAndEditions = new VersionAndEditionsSection()
                {
                    Version = "Original",
                    VersionDatfile = "Alt",
                    CommonEditions = new string[] { "Taikenban" },
                    OtherEditions = "Rerelease",
                },

                EDC = new EDCSection()
                {
                    EDC = YesNo.Yes,
                },

                ParentCloneRelationship = new ParentCloneRelationshipSection()
                {
                    ParentID = "12345",
                    RegionalParent = false,
                },

                Extras = new ExtrasSection()
                {
                    PVD = "PVD with a stupidly long line and nothing else but a little more\nPVD with a stupidly long line and nothing else but a little more\nPVD with a stupidly long line and nothing else but a little more\nPVD with a stupidly long line and nothing else but a little more\nPVD with a stupidly long line and nothing else but a little more\nPVD with a stupidly long line and nothing else but a little more\n",
                    DiscKey = "Disc key",
                    DiscID = "Disc ID",
                    PIC = "PIC",
                    Header = "Header",
                    BCA = "BCA",
                    SecuritySectorRanges = "SSv1 Ranges",
                },

                CopyProtection = new CopyProtectionSection()
                {
                    AntiModchip = YesNo.Yes,
                    LibCrypt = YesNo.No,
                    LibCryptData = "LibCrypt data",
                    Protection = "List of protections",
                    SecuROMData = "SecuROM data",
                },

                DumpersAndStatus = new DumpersAndStatusSection()
                {
                    Status = DumpStatus.TwoOrMoreGreen,
                    Dumpers = new string[] { "Dumper1", "Dumper2" },
                    OtherDumpers = "Dumper3",
                },

                TracksAndWriteOffsets = new TracksAndWriteOffsetsSection()
                {
                    ClrMameProData = "Datfile",
                    Cuesheet = "Cuesheet",
                    CommonWriteOffsets = new int[] { 0, 12, -12 },
                    OtherWriteOffsets = "-2",
                },

                SizeAndChecksums = new SizeAndChecksumsSection()
                {
                    Layerbreak = 0,
                    Layerbreak2 = 1,
                    Layerbreak3 = 2,
                    Size = 12345,
                    CRC32 = "CRC32",
                    MD5 = "MD5",
                    SHA1 = "SHA1",
                },

                DumpingInfo = new DumpingInfoSection()
                {
                    DumpingProgram = "DiscImageCreator 20500101",
                    DumpingDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                    Manufacturer = "ATAPI",
                    Model = "Optical Drive",
                    Firmware = "1.23",
                    ReportedDiscType = "CD-R",
                },

                Artifacts = new Dictionary<string, string>()
                {
                    ["Sample Artifact"] = "Sample Data",
                },
            };

            var result = ShowDiscInformationWindow(submissionInfo);
            InfoTool.ProcessSpecialFields(result.Item2);
        }

        /// <summary>
        /// Show the Options window
        /// </summary>
        public void ShowOptionsWindow(MPF.Core.Data.Options options)
        {
            var optionsWindow = new OptionsWindow(options) { Owner = App.Instance };
            optionsWindow.Closed += OnOptionsUpdated;
            optionsWindow.Show();
        }

        /// <summary>
        /// Toggle the parameters input box
        /// </summary>
        public void ToggleParameters(MainWindow instance)
        {
            if (instance.EnableParametersCheckBox.IsChecked == true)
            {
                instance.SystemTypeComboBox.IsEnabled = false;
                instance.MediaTypeComboBox.IsEnabled = false;

                instance.OutputPathTextBox.IsEnabled = false;
                instance.OutputPathBrowseButton.IsEnabled = false;

                instance.MediaScanButton.IsEnabled = false;
                instance.UpdateVolumeLabel.IsEnabled = false;
                instance.CopyProtectScanButton.IsEnabled = false;

                instance.ParametersTextBox.IsEnabled = true;
            }
            else
            {
                instance.ParametersTextBox.IsEnabled = false;
                ProcessCustomParameters(instance);

                instance.SystemTypeComboBox.IsEnabled = true;
                instance.MediaTypeComboBox.IsEnabled = true;

                instance.OutputPathTextBox.IsEnabled = true;
                instance.OutputPathBrowseButton.IsEnabled = true;

                instance.MediaScanButton.IsEnabled = true;
                instance.UpdateVolumeLabel.IsEnabled = true;
                instance.CopyProtectScanButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// Toggle the Start/Stop button
        /// </summary>
        public async void ToggleStartStop(MainWindow instance, LogViewModel logger, MPF.Core.Data.Options options)
        {
            // Dump or stop the dump
            if ((string)instance.StartStopButton.Content == Interface.StartDumping)
            {
                StartDumping(instance, logger, options);
            }
            else if ((string)instance.StartStopButton.Content == Interface.StopDumping)
            {
                if (options.VerboseLogging)
                    logger.VerboseLogLn("Canceling dumping process...");
                Env.CancelDumping();
                instance.CopyProtectScanButton.IsEnabled = true;

                if (Env.Options.EjectAfterDump == true)
                {
                    if (options.VerboseLogging)
                        logger.VerboseLogLn($"Ejecting disc in drive {Env.Drive.Letter}");
                    await Env.EjectDisc();
                }

                if (options.DICResetDriveAfterDump)
                {
                    if (options.VerboseLogging)
                        logger.VerboseLogLn($"Resetting drive {Env.Drive.Letter}");
                    await Env.ResetDrive();
                }
            }

            // Reset the progress bar
            logger.ResetProgressBar();
        }

        /// <summary>
        /// Update the internal options from a closed OptionsWindow
        /// </summary>
        /// <param name="optionsWindow">OptionsWindow to copy back data from</param>
        public void UpdateOptions(OptionsWindow optionsWindow)
        {
            if (optionsWindow?.OptionsViewModel.SavedSettings == true)
            {
                App.Options = optionsWindow.OptionsViewModel.Options.Clone() as MPF.Core.Data.Options;
                InitializeUIValues(App.Instance, App.Logger, App.Options, removeEventHandlers: true, rescanDrives: true);
            }
        }

        #endregion

        #region UI Functionality

        /// <summary>
        /// Performs UI value setup end to end
        /// </summary>
        /// <param name="removeEventHandlers">Whether event handlers need to be removed first</param>
        /// <param name="rescanDrives">Whether drives should be rescanned or not</param>
        public async void InitializeUIValues(MainWindow instance, LogViewModel logger, MPF.Core.Data.Options options, bool removeEventHandlers, bool rescanDrives)
        {
            // Disable the dumping button
            instance.StartStopButton.IsEnabled = false;

            // Safely uncheck the parameters box, just in case
            if (instance.EnableParametersCheckBox.IsChecked == true)
            {
                instance.EnableParametersCheckBox.Checked -= EnableParametersCheckBoxClick;
                instance.EnableParametersCheckBox.IsChecked = false;
                instance.ParametersTextBox.IsEnabled = false;
                instance.EnableParametersCheckBox.Checked += EnableParametersCheckBoxClick;
            }

            // Set the UI color scheme according to the options
            if (options.EnableDarkMode)
                EnableDarkMode();
            else
                EnableLightMode();

            // Force the UI to reload after applying the theme
            instance.UpdateLayout();

            // Remove event handlers to ensure ordering
            if (removeEventHandlers)
                DisableEventHandlers();

            // Populate the list of drives and determine the system
            if (rescanDrives)
            {
                instance.StatusLabel.Text = "Creating drive list, please wait!";
                await instance.Dispatcher.InvokeAsync(() => PopulateDrives(instance, logger, options));
            }
            else
            {
                await instance.Dispatcher.InvokeAsync(() => DetermineSystemType(instance, logger, options));
            }

            // Determine current media type, if possible
            await instance.Dispatcher.InvokeAsync(() => PopulateMediaType(instance));
            CacheCurrentDiscType(instance, logger, options);
            SetCurrentDiscType(instance);

            // Set the dumping program
            await instance.Dispatcher.InvokeAsync(() => PopulateInternalPrograms(instance, options));

            // Set the initial environment and UI values
            SetSupportedDriveSpeed(instance, logger, options);
            Env = DetermineEnvironment(instance, options);
            GetOutputNames(instance, logger, options, true);
            EnsureDiscInformation(instance, options);

            // Enable event handlers
            EnableEventHandlers();

            // Enable the dumping button, if necessary
            instance.StartStopButton.IsEnabled = ShouldEnableDumpingButton(instance);
        }

        /// <summary>
        /// Performs a fast update of the output path while skipping disc checks
        /// </summary>
        /// <param name="removeEventHandlers">Whether event handlers need to be removed first</param>
        private void FastUpdateLabel(MainWindow instance, LogViewModel logger, MPF.Core.Data.Options options, bool removeEventHandlers)
        {
            // Disable the dumping button
            instance.StartStopButton.IsEnabled = false;

            // Safely uncheck the parameters box, just in case
            if (instance.EnableParametersCheckBox.IsChecked == true)
            {
                instance.EnableParametersCheckBox.Checked -= EnableParametersCheckBoxClick;
                instance.EnableParametersCheckBox.IsChecked = false;
                instance.ParametersTextBox.IsEnabled = false;
                instance.EnableParametersCheckBox.Checked += EnableParametersCheckBoxClick;
            }

            // Remove event handlers to ensure ordering
            if (removeEventHandlers)
                DisableEventHandlers();

            // Refresh the drive info
            (instance.DriveLetterComboBox.SelectedItem as Drive)?.RefreshDrive();

            // Set the initial environment and UI values
            Env = DetermineEnvironment(instance, options);
            GetOutputNames(instance, logger, options, true);
            EnsureDiscInformation(instance, options);

            // Enable event handlers
            EnableEventHandlers();

            // Enable the dumping button, if necessary
            instance.StartStopButton.IsEnabled = ShouldEnableDumpingButton(instance);
        }

        /// <summary>
        /// Add all event handlers
        /// </summary>
        private void AddEventHandlers(MainWindow instance)
        {
            // Menu Bar Click
            instance.AboutMenuItem.Click += AboutClick;
            instance.AppExitMenuItem.Click += AppExitClick;
            instance.CheckForUpdatesMenuItem.Click += CheckForUpdatesClick;
            instance.DebugViewMenuItem.Click += DebugViewClick;
            instance.OptionsMenuItem.Click += OptionsMenuItemClick;

            // User Area Click
            instance.CopyProtectScanButton.Click += CopyProtectScanButtonClick;
            instance.EnableParametersCheckBox.Click += EnableParametersCheckBoxClick;
            instance.MediaScanButton.Click += MediaScanButtonClick;
            instance.UpdateVolumeLabel.Click += UpdateVolumeLabelClick;
            instance.OutputPathBrowseButton.Click += OutputPathBrowseButtonClick;
            instance.StartStopButton.Click += StartStopButtonClick;

            // User Area SelectionChanged
            instance.SystemTypeComboBox.SelectionChanged += SystemTypeComboBoxSelectionChanged;
            instance.MediaTypeComboBox.SelectionChanged += MediaTypeComboBoxSelectionChanged;
            instance.DriveLetterComboBox.SelectionChanged += DriveLetterComboBoxSelectionChanged;
            instance.DriveSpeedComboBox.SelectionChanged += DriveSpeedComboBoxSelectionChanged;
            instance.DumpingProgramComboBox.SelectionChanged += DumpingProgramComboBoxSelectionChanged;

            // User Area TextChanged
            instance.OutputPathTextBox.TextChanged += OutputPathTextBoxTextChanged;
        }

        /// <summary>
        /// Enable all textbox and combobox event handlers
        /// </summary>
        private void EnableEventHandlers()
        {
            _canExecuteSelectionChanged = true;
        }

        /// <summary>
        /// Disable all textbox and combobox event handlers
        /// </summary>
        private void DisableEventHandlers()
        {
            _canExecuteSelectionChanged = false;
        }

        /// <summary>
        /// Disable all UI elements during dumping
        /// </summary>
        private void DisableAllUIElements(MainWindow instance)
        {
            instance.OptionsMenuItem.IsEnabled = false;
            instance.SystemTypeComboBox.IsEnabled = false;
            instance.MediaTypeComboBox.IsEnabled = false;
            instance.OutputPathTextBox.IsEnabled = false;
            instance.OutputPathBrowseButton.IsEnabled = false;
            instance.DriveLetterComboBox.IsEnabled = false;
            instance.DriveSpeedComboBox.IsEnabled = false;
            instance.DumpingProgramComboBox.IsEnabled = false;
            instance.EnableParametersCheckBox.IsEnabled = false;
            instance.StartStopButton.Content = Interface.StopDumping;
            instance.MediaScanButton.IsEnabled = false;
            instance.UpdateVolumeLabel.IsEnabled = false;
            instance.CopyProtectScanButton.IsEnabled = false;
        }

        /// <summary>
        /// Enable all UI elements after dumping
        /// </summary>
        private void EnableAllUIElements(MainWindow instance)
        {
            instance.OptionsMenuItem.IsEnabled = true;
            instance.SystemTypeComboBox.IsEnabled = true;
            instance.MediaTypeComboBox.IsEnabled = true;
            instance.OutputPathTextBox.IsEnabled = true;
            instance.OutputPathBrowseButton.IsEnabled = true;
            instance.DriveLetterComboBox.IsEnabled = true;
            instance.DriveSpeedComboBox.IsEnabled = true;
            instance.DumpingProgramComboBox.IsEnabled = true;
            instance.EnableParametersCheckBox.IsEnabled = true;
            instance.StartStopButton.Content = Interface.StartDumping;
            instance.MediaScanButton.IsEnabled = true;
            instance.UpdateVolumeLabel.IsEnabled = true;
            instance.CopyProtectScanButton.IsEnabled = true;
        }

        /// <summary>
        /// Recolor all UI elements for light mode
        /// </summary>
        private void EnableLightMode()
        {
            var theme = new LightModeTheme();
            theme.Apply();
        }

        /// <summary>
        /// Recolor all UI elements for dark mode
        /// </summary>
        private void EnableDarkMode()
        {
            var theme = new DarkModeTheme();
            theme.Apply();
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Browse for an output file path
        /// </summary>
        private void BrowseFile(MainWindow instance, MPF.Core.Data.Options options)
        {
            // Get the current path, if possible
            string currentPath = instance.OutputPathTextBox.Text;
            if (string.IsNullOrWhiteSpace(currentPath))
                currentPath = Path.Combine(options.DefaultOutputPath, "track.bin");
            if (string.IsNullOrWhiteSpace(currentPath))
                currentPath = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "track.bin");

            // Get the full path
            currentPath = Path.GetFullPath(currentPath);

            // Get the directory
            string directory = Path.GetDirectoryName(currentPath);

            // Get the filename
            string filename = Path.GetFileName(currentPath);

            WinForms.FileDialog fileDialog = new WinForms.SaveFileDialog
            {
                FileName = filename,
                InitialDirectory = directory,
            };
            WinForms.DialogResult result = fileDialog.ShowDialog();

            if (result == WinForms.DialogResult.OK)
            {
                instance.OutputPathTextBox.Text = fileDialog.FileName;
            }
        }

        /// <summary>
        /// Cache the current disc type to internal variable
        /// </summary>
        private void CacheCurrentDiscType(MainWindow instance, LogViewModel logger, MPF.Core.Data.Options options)
        {
            // If the selected item is invalid, we just skip
            if (!(instance.DriveLetterComboBox.SelectedItem is Drive drive))
                return;

            // Get reasonable default values based on the current system
            RedumpSystem? currentSystem = Systems[instance.SystemTypeComboBox.SelectedIndex];
            MediaType? defaultMediaType = currentSystem.MediaTypes().FirstOrDefault() ?? MediaType.CDROM;
            if (defaultMediaType == MediaType.NONE)
                defaultMediaType = MediaType.CDROM;

#if NET48 || NETSTANDARD2_1
            // If we're skipping detection, set the default value
            if (options.SkipMediaTypeDetection)
            {
                if (options.VerboseLogging)
                    logger.VerboseLogLn($"Media type detection disabled, defaulting to {defaultMediaType.LongName()}.");
                CurrentMediaType = defaultMediaType;
            }
            // If the drive is marked active, try to read from it
            else if (drive.MarkedActive)
            {
                if (options.VerboseLogging)
                    logger.VerboseLog($"Trying to detect media type for drive {drive.Letter} [{drive.DriveFormat}].. ");
                (MediaType? detectedMediaType, string errorMessage) = drive.GetMediaType();

                // If we got an error message, post it to the log
                if (errorMessage != null && options.VerboseLogging)
                    logger.VerboseLogLn($"Message from detecting media type: {errorMessage}");

                // If we got either an error or no media, default to the current System default
                if (detectedMediaType == null)
                {
                    if (options.VerboseLogging)
                        logger.VerboseLogLn($"Unable to detect, defaulting to {defaultMediaType.LongName()}.");
                    CurrentMediaType = defaultMediaType;
                }
                else
                {
                    if (options.VerboseLogging)
                        logger.VerboseLogLn($"Detected {CurrentMediaType.LongName()}.");
                    CurrentMediaType = detectedMediaType;
                }
            }

            // All other cases, just use the default
            else
            {
                if (options.VerboseLogging)
                    logger.VerboseLogLn($"Drive marked as empty, defaulting to {defaultMediaType.LongName()}.");
                CurrentMediaType = defaultMediaType;
            }
#else
            // Media type detection on initialize is always disabled
            if (options.VerboseLogging)
                logger.VerboseLogLn($"Media type detection not available, defaulting to {defaultMediaType.LongName()}.");
            CurrentMediaType = defaultMediaType;
#endif

            // Ensure the UI gets updated
            instance.UpdateLayout();
        }

        /// <summary>
        /// Create a DumpEnvironment with all current settings
        /// </summary>
        /// <returns>Filled DumpEnvironment instance</returns>
        private static DumpEnvironment DetermineEnvironment(MainWindow instance, MPF.Core.Data.Options options)
        {
            return new DumpEnvironment(options,
                instance.OutputPathTextBox.Text,
                instance.DriveLetterComboBox.SelectedItem as Drive,
                instance.SystemTypeComboBox.SelectedItem as RedumpSystemComboBoxItem,
                instance.MediaTypeComboBox.SelectedItem as Element<MediaType>,
                instance.DumpingProgramComboBox.SelectedItem as Element<InternalProgram>,
                instance.ParametersTextBox.Text);
        }

        /// <summary>
        /// Determine and set the current system type, if allowed
        /// </summary>
        private void DetermineSystemType(MainWindow instance, LogViewModel logger, MPF.Core.Data.Options options)
        {
            if (Drives == null || Drives.Count == 0 || instance.DriveLetterComboBox.SelectedIndex == -1)
            {
                if (options.VerboseLogging)
                    logger.VerboseLog("Skipping system type detection because no valid drives found!");
            }
            else if (!options.SkipSystemDetection)
            {
                if (options.VerboseLogging)
                    logger.VerboseLog($"Trying to detect system for drive {Drives[instance.DriveLetterComboBox.SelectedIndex].Letter}.. ");
                var currentSystem = Drives[instance.DriveLetterComboBox.SelectedIndex]?.GetRedumpSystem(options.DefaultSystem) ?? options.DefaultSystem;
                if (options.VerboseLogging)
                    logger.VerboseLogLn(currentSystem == null ? "unable to detect." : ($"detected {currentSystem.LongName()}."));

                if (currentSystem != null)
                {
                    int sysIndex = Systems.FindIndex(s => s == currentSystem);
                    instance.SystemTypeComboBox.SelectedIndex = sysIndex;
                }
            }
            else if (options.SkipSystemDetection && options.DefaultSystem != null)
            {
                var currentSystem = options.DefaultSystem;
                if (options.VerboseLogging)
                    logger.VerboseLog($"System detection disabled, setting to default of {currentSystem.LongName()}.");
                int sysIndex = Systems.FindIndex(s => s == currentSystem);
                instance.SystemTypeComboBox.SelectedIndex = sysIndex;
            }

            // Ensure the UI gets updated
            instance.UpdateLayout();
        }

        /// <summary>
        /// Ensure information is consistent with the currently selected disc type
        /// </summary>
        public void EnsureDiscInformation(MainWindow instance, MPF.Core.Data.Options options)
        {
            // Get the current environment information
            Env = DetermineEnvironment(instance, options);

            // Get the status to write out
            Result result = Tools.GetSupportStatus(Env.System, Env.Type);
            instance.StatusLabel.Text = result.Message;

            // Set the index for the current disc type
            SetCurrentDiscType(instance);

            // Enable or disable the button
            instance.StartStopButton.IsEnabled = result && ShouldEnableDumpingButton(instance);

            // If we're in a type that doesn't support drive speeds
            instance.DriveSpeedComboBox.IsEnabled = Env.Type.DoesSupportDriveSpeed();

            // If input params are not enabled, generate the full parameters from the environment
            if (!instance.ParametersTextBox.IsEnabled)
            {
                string generated = Env.GetFullParameters((int?)instance.DriveSpeedComboBox.SelectedItem);
                if (generated != null)
                    instance.ParametersTextBox.Text = generated;
            }

            // Ensure the UI gets updated
            instance.UpdateLayout();
        }

        /// <summary>
        /// Get the default output directory name from the currently selected drive
        /// </summary>
        /// <param name="driveChanged">Force an updated name if the drive letter changes</param>
        public void GetOutputNames(MainWindow instance, LogViewModel logger, MPF.Core.Data.Options options, bool driveChanged)
        {
            if (Drives == null || Drives.Count == 0 || instance.DriveLetterComboBox.SelectedIndex == -1)
            {
                if (options.VerboseLogging)
                    logger.VerboseLog("Skipping output name building because no valid drives found!");
                return;
            }

            Drive drive = instance.DriveLetterComboBox.SelectedItem as Drive;
            RedumpSystem? systemType = instance.SystemTypeComboBox.SelectedItem as RedumpSystemComboBoxItem;
            MediaType? mediaType = instance.MediaTypeComboBox.SelectedItem as Element<MediaType>;

            // Get the extension for the file for the next two statements
            string extension = Env.Parameters?.GetDefaultExtension(mediaType);

            // Set the output filename, if it's not already
            if (string.IsNullOrEmpty(instance.OutputPathTextBox.Text))
            {
                string label = drive?.FormattedVolumeLabel ?? systemType.LongName();
                string directory = options.DefaultOutputPath;
                string filename = $"{label}{extension ?? ".bin"}";

                // If the path ends with the label already
                if (directory.EndsWith(label, StringComparison.OrdinalIgnoreCase))
                    directory = Path.GetDirectoryName(directory);

                instance.OutputPathTextBox.Text = Path.Combine(directory, label, filename);
            }

            // Set the output filename, if we changed drives
            else if (driveChanged)
            {
                string label = drive?.FormattedVolumeLabel ?? systemType.LongName();
                string oldFilename = Path.GetFileNameWithoutExtension(instance.OutputPathTextBox.Text);
                string directory = Path.GetDirectoryName(instance.OutputPathTextBox.Text);
                string filename = $"{label}{extension ?? ".bin"}";

                // If the previous path included the label
                if (directory.EndsWith(oldFilename, StringComparison.OrdinalIgnoreCase))
                    directory = Path.GetDirectoryName(directory);

                // If the path ends with the label already
                if (directory.EndsWith(label, StringComparison.OrdinalIgnoreCase))
                    directory = Path.GetDirectoryName(directory);

                instance.OutputPathTextBox.Text = Path.Combine(directory, label, filename);
            }

            // Otherwise, reset the extension of the currently set path
            else
            {
                string filename = Path.GetFileNameWithoutExtension(instance.OutputPathTextBox.Text);
                string directory = Path.GetDirectoryName(instance.OutputPathTextBox.Text);
                filename = $"{filename}{extension ?? ".bin"}";

                instance.OutputPathTextBox.Text = Path.Combine(directory, filename);
            }

            // Ensure the UI gets updated
            instance.UpdateLayout();
        }

        /// <summary>
        /// Process the current custom parameters back into UI values
        /// </summary>
        public void ProcessCustomParameters(MainWindow instance)
        {
            Env.SetParameters(instance.ParametersTextBox.Text);
            if (Env.Parameters == null)
                return;

            // Catch this in case there's an input path issue
            try
            {
                int driveIndex = Drives.Select(d => d.Letter).ToList().IndexOf(Env.Parameters.InputPath[0]);
                if (driveIndex > -1)
                    instance.DriveLetterComboBox.SelectedIndex = driveIndex;
            }
            catch { }

            int driveSpeed = Env.Parameters.Speed ?? -1;
            if (driveSpeed > 0)
                instance.DriveSpeedComboBox.SelectedValue = driveSpeed;
            else
                Env.Parameters.Speed = instance.DriveSpeedComboBox.SelectedValue as int?;

            // Disable change handling
            DisableEventHandlers();

            string trimmedPath = Env.Parameters.OutputPath?.Trim('"') ?? string.Empty;
            trimmedPath = InfoTool.NormalizeOutputPaths(trimmedPath);
            instance.OutputPathTextBox.Text = trimmedPath;

            MediaType? mediaType = Env.Parameters.GetMediaType();
            int mediaTypeIndex = MediaTypes.FindIndex(m => m == mediaType);
            if (mediaTypeIndex > -1)
                instance.MediaTypeComboBox.SelectedIndex = mediaTypeIndex;

            // Reenable change handling
            EnableEventHandlers();
        }

        /// <summary>
        /// Scan and show copy protection for the current disc
        /// </summary>
        public async void ScanAndShowProtection(MainWindow instance, LogViewModel logger, MPF.Core.Data.Options options)
        {
            // Determine current environment, just in case
            if (Env == null)
                Env = DetermineEnvironment(instance, options);

            // Pull the drive letter from the UI directly, just in case
            var drive = instance.DriveLetterComboBox.SelectedItem as Drive;
            if (drive.Letter != default(char))
            {
                if (options.VerboseLogging)
                    logger.VerboseLogLn($"Scanning for copy protection in {drive.Letter}");

                var tempContent = instance.StatusLabel.Text;
                instance.StatusLabel.Text = "Scanning for copy protection... this might take a while!";
                instance.StartStopButton.IsEnabled = false;
                instance.MediaScanButton.IsEnabled = false;
                instance.UpdateVolumeLabel.IsEnabled = false;
                instance.CopyProtectScanButton.IsEnabled = false;

                var progress = new Progress<ProtectionProgress>();
                progress.ProgressChanged += ProgressUpdated;
                (var protections, string error) = await Protection.RunProtectionScanOnPath(drive.Letter + ":\\", options, progress);
                string output = Protection.FormatProtections(protections);

                // If SmartE is detected on the current disc, remove `/sf` from the flags for DIC only -- Disabled until further notice
                //if (Env.InternalProgram == InternalProgram.DiscImageCreator && output.Contains("SmartE"))
                //{
                //    ((Modules.DiscImageCreator.Parameters)Env.Parameters)[Modules.DiscImageCreator.FlagStrings.ScanFileProtect] = false;
                //    if (options.VerboseLogging)
                //        logger.VerboseLogLn($"SmartE detected, removing {Modules.DiscImageCreator.FlagStrings.ScanFileProtect} from parameters");
                //}

                if (!instance.LogPanel.IsExpanded)
                {
                    if (string.IsNullOrEmpty(error))
                        CustomMessageBox.Show(output, "Detected Protection(s)", MessageBoxButton.OK, MessageBoxImage.Information);
                    else
                        CustomMessageBox.Show("An exception occurred, see the log for details", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                if (string.IsNullOrEmpty(error))
                    logger.LogLn($"Detected the following protections in {drive.Letter}:\r\n\r\n{output}");
                else
                    logger.ErrorLogLn($"Path could not be scanned! Exception information:\r\n\r\n{error}");

                instance.StatusLabel.Text = tempContent;
                instance.StartStopButton.IsEnabled = ShouldEnableDumpingButton(instance);
                instance.MediaScanButton.IsEnabled = true;
                instance.UpdateVolumeLabel.IsEnabled = true;
                instance.CopyProtectScanButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// Set the current disc type in the combo box
        /// </summary>
        private void SetCurrentDiscType(MainWindow instance)
        {
            // If we have an invalid current type, we don't care and return
            if (CurrentMediaType == null || CurrentMediaType == MediaType.NONE)
                return;

            // Now set the selected item, if possible
            int index = MediaTypes.FindIndex(kvp => kvp.Value == CurrentMediaType);
            if (index != -1)
                instance.MediaTypeComboBox.SelectedIndex = index;
            else
                instance.StatusLabel.Text = $"Disc of type '{CurrentMediaType.LongName()}' found, but the current system does not support it!";

            // Ensure the UI gets updated
            instance.UpdateLayout();
        }

        /// <summary>
        /// Set the drive speed based on reported maximum and user-defined option
        /// </summary>
        public void SetSupportedDriveSpeed(MainWindow instance, LogViewModel logger, MPF.Core.Data.Options options)
        {
            // Set the drive speed list that's appropriate
            var values = Interface.GetSpeedsForMediaType(CurrentMediaType);
            instance.DriveSpeedComboBox.ItemsSource = values;
            if (options.VerboseLogging)
                logger.VerboseLogLn($"Supported media speeds: {string.Join(", ", values)}");

            // Set the selected speed
            int speed;
            switch (CurrentMediaType)
            {
                case MediaType.CDROM:
                case MediaType.GDROM:
                    speed = options.PreferredDumpSpeedCD;
                    break;
                case MediaType.DVD:
                case MediaType.NintendoGameCubeGameDisc:
                case MediaType.NintendoWiiOpticalDisc:
                    speed = options.PreferredDumpSpeedDVD;
                    break;
                case MediaType.HDDVD:
                    speed = options.PreferredDumpSpeedHDDVD;
                    break;
                case MediaType.BluRay:
                    speed = options.PreferredDumpSpeedBD;
                    break;
                default:
                    speed = options.PreferredDumpSpeedCD;
                    break;
            }

            if (options.VerboseLogging)
                logger.VerboseLogLn($"Setting drive speed to: {speed}");
            instance.DriveSpeedComboBox.SelectedValue = speed;

            // Ensure the UI gets updated
            instance.UpdateLayout();
        }

        /// <summary>
        /// Determine if the dumping button should be enabled
        /// </summary>
        private bool ShouldEnableDumpingButton(MainWindow instance)
        {
            return Drives != null
                && Drives.Count > 0
                && instance.SystemTypeComboBox.SelectedIndex > -1
                && instance.SystemTypeComboBox.SelectedItem as RedumpSystemComboBoxItem != null
                && !string.IsNullOrEmpty(instance.ParametersTextBox.Text);
        }

        /// <summary>
        /// Show the disc information window
        /// </summary>
        /// <param name="submissionInfo">SubmissionInfo object to display and possibly change</param>
        /// <returns>Dialog open result</returns>
        private (bool?, SubmissionInfo) ShowDiscInformationWindow(SubmissionInfo submissionInfo)
        {
            if (App.Options.ShowDiscEjectReminder)
                CustomMessageBox.Show(App.Instance, "It is now safe to eject the disc", "Eject", MessageBoxButton.OK, MessageBoxImage.Information);

            var discInformationWindow = new DiscInformationWindow(App.Options, submissionInfo)
            {
                Owner = App.Instance,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
            };
            bool? result = discInformationWindow.ShowDialog();

            // Copy back the submission info changes, if necessary
            if (result == true)
                submissionInfo = discInformationWindow.DiscInformationViewModel.SubmissionInfo.Clone() as SubmissionInfo;

            return (result, submissionInfo);
        }

        /// <summary>
        /// Begin the dumping process using the given inputs
        /// </summary>
        public async void StartDumping(MainWindow instance, LogViewModel logger, MPF.Core.Data.Options options)
        {
            // One last check to determine environment, just in case
            Env = DetermineEnvironment(instance, loptions);

            // Force an internal drive refresh in case the user entered things manually
            Env.Drive.RefreshDrive();

            // If still in custom parameter mode, check that users meant to continue or not
            if (instance.EnableParametersCheckBox.IsChecked == true)
            {
                MessageBoxResult result = CustomMessageBox.Show("It looks like you have custom parameters that have not been saved. Would you like to apply those changes before starting to dump?", "Custom Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    instance.EnableParametersCheckBox.IsChecked = false;
                    instance.ParametersTextBox.IsEnabled = false;
                    ProcessCustomParameters(instance);
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
                // If "No", then we continue with the current known environment
            }

            // Run path adjustments for DiscImageCreator -- Disabled until further notice
            //Env.AdjustPathsForDiscImageCreator();

            try
            {
                // Run pre-dumping validation checks
                if (!ValidateBeforeDumping(logger))
                    return;

                // Disable all UI elements apart from dumping button
                DisableAllUIElements(instance);

                // Refresh the drive, if it wasn't null
                if (Env.Drive != null)
                    Env.Drive.RefreshDrive();

                // Output to the label and log
                instance.StatusLabel.Text = "Starting dumping process... Please wait!";
                logger.LogLn("Starting dumping process... Please wait!");
                if (loptions.ToolsInSeparateWindow)
                    logger.LogLn("Look for the separate command window for more details");
                else
                    logger.LogLn("Program outputs may be slow to populate in the log window");

                // Get progress indicators
                var resultProgress = new Progress<Result>();
                resultProgress.ProgressChanged += ProgressUpdated;
                var protectionProgress = new Progress<ProtectionProgress>();
                protectionProgress.ProgressChanged += ProgressUpdated;
                Env.ReportStatus += ProgressUpdated;

                // Run the program with the parameters
                Result result = await Env.Run(resultProgress);
                logger.ResetProgressBar();

                // If we didn't execute a dumping command we cannot get submission output
                if (!Env.Parameters.IsDumpingCommand())
                {
                    logger.LogLn("No dumping command was run, submission information will not be gathered.");
                    instance.StatusLabel.Text = "Execution complete!";

                    // Reset all UI elements
                    EnableAllUIElements(instance);
                    return;
                }

                // Verify dump output and save it
                if (result)
                {
                    result = await Env.VerifyAndSaveDumpOutput(resultProgress, protectionProgress, ShowDiscInformationWindow);
                }
                else
                {
                    logger.ErrorLogLn(result.Message);
                    instance.StatusLabel.Text = "Execution failed!";
                }
            }
            catch (Exception ex)
            {
                logger.ErrorLogLn(ex.ToString());
                instance.StatusLabel.Text = "An exception occurred!";
            }
            finally
            {
                // Reset all UI elements
                EnableAllUIElements(instance);
            }
        }

        /// <summary>
        /// Perform validation, including user input, before attempting to start dumping
        /// </summary>
        /// <returns>True if dumping should start, false otherwise</returns>
        private bool ValidateBeforeDumping(LogViewModel logger)
        {
            // Validate that we have an output path of any sort
            if (string.IsNullOrWhiteSpace(Env.OutputPath))
            {
                MessageBoxResult mbresult = CustomMessageBox.Show("No output path was provided so dumping cannot continue.", "Missing Path", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                logger.LogLn("Dumping aborted!");
                return false;
            }

            // Validate that the user explicitly wants an inactive drive to be considered for dumping
            if (!Env.Drive.MarkedActive)
            {
                string message = "The currently selected drive does not appear to contain a disc! "
                    + (!Env.System.DetectedByWindows() ? $"This is normal for {Env.System.LongName()} as the discs may not be readable on Windows. " : string.Empty)
                    + "Do you want to continue?";

                MessageBoxResult mbresult = CustomMessageBox.Show(message, "No Disc Detected", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                if (mbresult == MessageBoxResult.No || mbresult == MessageBoxResult.Cancel || mbresult == MessageBoxResult.None)
                {
                    logger.LogLn("Dumping aborted!");
                    return false;
                }
            }

            // Pre-split the output path
            string outputDirectory = Path.GetDirectoryName(Env.OutputPath);
            string outputFilename = Path.GetFileName(Env.OutputPath);

            // If a complete dump already exists
            (bool foundFiles, List<string> _) = InfoTool.FoundAllFiles(outputDirectory, outputFilename, Env.Parameters, true);
            if (foundFiles)
            {
                MessageBoxResult mbresult = CustomMessageBox.Show("A complete dump already exists! Are you sure you want to overwrite?", "Overwrite?", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                if (mbresult == MessageBoxResult.No || mbresult == MessageBoxResult.Cancel || mbresult == MessageBoxResult.None)
                {
                    logger.LogLn("Dumping aborted!");
                    return false;
                }
            }

            // Validate that at least some space exists
            // TODO: Tie this to the size of the disc, type of disc, etc.
            string fullPath = Path.GetFullPath(outputDirectory);
            var driveInfo = new DriveInfo(Path.GetPathRoot(fullPath));
            if (driveInfo.AvailableFreeSpace < Math.Pow(2, 30))
            {
                MessageBoxResult mbresult = CustomMessageBox.Show("There is less than 1gb of space left on the target drive. Are you sure you want to continue?", "Low Space", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                if (mbresult == MessageBoxResult.No || mbresult == MessageBoxResult.Cancel || mbresult == MessageBoxResult.None)
                {
                    logger.LogLn("Dumping aborted!");
                    return false;
                }
            }

            // If nothing above fails, we want to continue
            return true;
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handler for OptionsWindow OnUpdated event
        /// </summary>
        private void OnOptionsUpdated(object sender, EventArgs e) =>
            UpdateOptions(sender as OptionsWindow);

        #region Progress Reporting

        /// <summary>
        /// Handler for Result ProgressChanged event
        /// </summary>
        private void ProgressUpdated(object sender, string value)
        {
            try
            {
                value = value ?? string.Empty;
                App.Logger.LogLn(value);
            }
            catch { }
        }

        /// <summary>
        /// Handler for Result ProgressChanged event
        /// </summary>
        private void ProgressUpdated(object sender, Result value)
        {
            string message = value?.Message;

            // Update the label with only the first line of output
            if (message.Contains("\n"))
                App.Instance.StatusLabel.Text = value.Message.Split('\n')[0] + " (See log output)";
            else
                App.Instance.StatusLabel.Text = value.Message;

            // Log based on success or failure
            if (value && loptions.VerboseLogging)
                App.Logger.VerboseLogLn(message);
            else if (!value)
                App.Logger.ErrorLogLn(message);
        }

        /// <summary>
        /// Handler for ProtectionProgress ProgressChanged event
        /// </summary>
        private void ProgressUpdated(object sender, ProtectionProgress value)
        {
            string message = $"{value.Percentage * 100:N2}%: {value.Filename} - {value.Protection}";
            App.Instance.StatusLabel.Text = message;
            if (loptions.VerboseLogging)
                App.Logger.VerboseLogLn(message);
        }

        #endregion

        #region Menu Bar

        /// <summary>
        /// Handler for AboutMenuItem Click event
        /// </summary>
        private void AboutClick(object sender, RoutedEventArgs e) =>
            ShowAboutText(App.Instance, App.Logger);

        /// <summary>
        /// Handler for AppExitMenuItem Click event
        /// </summary>
        private void AppExitClick(object sender, RoutedEventArgs e) =>
            ExitApplication();

        /// <summary>
        /// Handler for CheckForUpdatesMenuItem Click event
        /// </summary>
        private void CheckForUpdatesClick(object sender, RoutedEventArgs e) =>
            CheckForUpdates(App.Instance, App.Logger, showIfSame: true);

        /// <summary>
        /// Handler for DebugViewMenuItem Click event
        /// </summary>
        private void DebugViewClick(object sender, RoutedEventArgs e) =>
            ShowDebugDiscInfoWindow();

        /// <summary>
        /// Handler for OptionsMenuItem Click event
        /// </summary>
        private void OptionsMenuItemClick(object sender, RoutedEventArgs e) =>
            ShowOptionsWindow(App.Options);

        #endregion

        #region User Area

        /// <summary>
        /// Handler for CopyProtectScanButton Click event
        /// </summary>
        private void CopyProtectScanButtonClick(object sender, RoutedEventArgs e) =>
            ScanAndShowProtection(App.Instance, App.Logger, App.Options);

        /// <summary>
        /// Handler for DriveLetterComboBox SelectionChanged event
        /// </summary>
        private void DriveLetterComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_canExecuteSelectionChanged)
                InitializeUIValues(App.Instance, App.Logger, App.Options, removeEventHandlers: true, rescanDrives: false);
        }

        /// <summary>
        /// Handler for DriveSpeedComboBox SelectionChanged event
        /// </summary>
        private void DriveSpeedComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_canExecuteSelectionChanged)
                EnsureDiscInformation(App.Instance, App.Options);
        }

        /// <summary>
        /// Handler for DumpingProgramComboBox SelectionChanged event
        /// </summary>
        private void DumpingProgramComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_canExecuteSelectionChanged)
                ChangeDumpingProgram(App.Instance, App.Logger, App.Options);
        }

        /// <summary>
        /// Handler for EnableParametersCheckBox Click event
        /// </summary>
        private void EnableParametersCheckBoxClick(object sender, RoutedEventArgs e) =>
            ToggleParameters(App.Instance);

        /// <summary>
        /// Handler for MediaScanButton Click event
        /// </summary>
        private void MediaScanButtonClick(object sender, RoutedEventArgs e) =>
            InitializeUIValues(App.Instance, App.Logger, App.Options, removeEventHandlers: true, rescanDrives: true);

        /// <summary>
        /// Handler for MediaTypeComboBox SelectionChanged event
        /// </summary>
        private void MediaTypeComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_canExecuteSelectionChanged)
                ChangeMediaType(App.Instance, App.Logger, App.Options, e);
        }

        /// <summary>
        /// Handler for OutputPathBrowseButton Click event
        /// </summary>
        private void OutputPathBrowseButtonClick(object sender, RoutedEventArgs e) =>
            SetOutputPath(App.Instance, App.Options);

        /// <summary>
        /// Handler for OutputPathTextBox TextChanged event
        /// </summary>
        private void OutputPathTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_canExecuteSelectionChanged)
                EnsureDiscInformation(App.Instance, App.Options);
        }

        /// <summary>
        /// Handler for StartStopButton Click event
        /// </summary>
        private void StartStopButtonClick(object sender, RoutedEventArgs e) =>
            ToggleStartStop(App.Instance, App.Logger, App.Options);

        /// <summary>
        /// Handler for SystemTypeComboBox SelectionChanged event
        /// </summary>
        private void SystemTypeComboBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_canExecuteSelectionChanged)
                ChangeSystem(App.Instance, App.Logger, App.Options);
        }

        /// <summary>
        /// Handler for UpdateVolumeLabel Click event
        /// </summary>
        private void UpdateVolumeLabelClick(object sender, RoutedEventArgs e)
        {
            if (_canExecuteSelectionChanged)
            {
                if (App.Options.FastUpdateLabel)
                    FastUpdateLabel(App.Instance, App.Logger, App.Options, removeEventHandlers: true);
                else
                    InitializeUIValues(App.Instance, App.Logger, App.Options, removeEventHandlers: true, rescanDrives: false);
            }
        }

        #endregion

        #endregion // Event Handlers
    }
}
