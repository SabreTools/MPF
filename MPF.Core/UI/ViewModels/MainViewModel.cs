using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BinaryObjectScanner;
using MPF.Core.Converters;
using MPF.Core.Data;
using MPF.Core.UI.ComboBoxItems;
using MPF.Core.Utilities;
using SabreTools.RedumpLib.Data;

namespace MPF.Core.UI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        /// Access to the current options
        /// </summary>
        public Data.Options Options
        {
            get => _options;
            set
            {
                _options = value;
                OptionsLoader.SaveToConfig(_options);
            }
        }
        private Data.Options _options;

        /// <summary>
        /// Indicates if SelectionChanged events can be executed
        /// </summary>
        public bool CanExecuteSelectionChanged { get; private set; } = false;

        /// <inheritdoc/>
#if NET48
        public event PropertyChangedEventHandler PropertyChanged;
#else
        public event PropertyChangedEventHandler? PropertyChanged;
#endif

        /// <summary>
        /// Action to process logging statements
        /// </summary>
#if NET48
        private Action<LogLevel, string> _logger;
#else
        private Action<LogLevel, string>? _logger;
#endif

        /// <summary>
        /// Display a message to a user
        /// </summary>
        /// <remarks>
        /// T1 - Title to display to the user
        /// T1 - Message to display to the user
        /// T3 - Number of default options to display
        /// T4 - true for inquiry, false otherwise
        /// TResult - true for positive, false for negative, null for neutral
        /// </remarks>
#if NET48
        private Func<string, string, int, bool, bool?> _displayUserMessage;
#else
        private Func<string, string, int, bool, bool?>? _displayUserMessage;
#endif

        /// <summary>
        /// Detected media type, distinct from the selected one
        /// </summary>
        private MediaType? _detectedMediaType;

        /// <summary>
        /// Current dumping environment
        /// </summary>
#if NET48
        private DumpEnvironment _environment;
#else
        private DumpEnvironment? _environment;
#endif

        /// <summary>
        /// Function to process user information
        /// </summary>
#if NET48
        private Func<SubmissionInfo, (bool?, SubmissionInfo)> _processUserInfo;
#else
        private Func<SubmissionInfo?, (bool?, SubmissionInfo?)>? _processUserInfo;
#endif

        #endregion

        #region Properties

        /// <summary>
        /// Indicates the status of the options menu item
        /// </summary>
        public bool OptionsMenuItemEnabled
        {
            get => _optionsMenuItemEnabled;
            set
            {
                _optionsMenuItemEnabled = value;
                TriggerPropertyChanged(nameof(OptionsMenuItemEnabled));
            }
        }
        private bool _optionsMenuItemEnabled;

        /// <summary>
        /// Currently selected system value
        /// </summary>
        public RedumpSystem? CurrentSystem
        {
            get => _currentSystem;
            set
            {
                _currentSystem = value;
                TriggerPropertyChanged(nameof(CurrentSystem));
            }
        }
        private RedumpSystem? _currentSystem;

        /// <summary>
        /// Indicates the status of the system type combo box
        /// </summary>
        public bool SystemTypeComboBoxEnabled
        {
            get => _systemTypeComboBoxEnabled;
            set
            {
                _systemTypeComboBoxEnabled = value;
                TriggerPropertyChanged(nameof(SystemTypeComboBoxEnabled));
            }
        }
        private bool _systemTypeComboBoxEnabled;

        /// <summary>
        /// Currently selected media type value
        /// </summary>
        public MediaType? CurrentMediaType
        {
            get => _currentMediaType;
            set
            {
                _currentMediaType = value;
                TriggerPropertyChanged(nameof(CurrentMediaType));
            }
        }
        private MediaType? _currentMediaType;

        /// <summary>
        /// Indicates the status of the media type combo box
        /// </summary>
        public bool MediaTypeComboBoxEnabled
        {
            get => _mediaTypeComboBoxEnabled;
            set
            {
                _mediaTypeComboBoxEnabled = value;
                TriggerPropertyChanged(nameof(MediaTypeComboBoxEnabled));
            }
        }
        private bool _mediaTypeComboBoxEnabled;

        /// <summary>
        /// Currently provided output path
        /// </summary>
        public string OutputPath
        {
            get => _outputPath;
            set
            {
                _outputPath = value;
                TriggerPropertyChanged(nameof(OutputPath));
            }
        }
        private string _outputPath;

        /// <summary>
        /// Indicates the status of the output path text box
        /// </summary>
        public bool OutputPathTextBoxEnabled
        {
            get => _outputPathTextBoxEnabled;
            set
            {
                _outputPathTextBoxEnabled = value;
                TriggerPropertyChanged(nameof(OutputPathTextBoxEnabled));
            }
        }
        private bool _outputPathTextBoxEnabled;

        /// <summary>
        /// Indicates the status of the output path browse button
        /// </summary>
        public bool OutputPathBrowseButtonEnabled
        {
            get => _outputPathBrowseButtonEnabled;
            set
            {
                _outputPathBrowseButtonEnabled = value;
                TriggerPropertyChanged(nameof(OutputPathBrowseButtonEnabled));
            }
        }
        private bool _outputPathBrowseButtonEnabled;

        /// <summary>
        /// Currently selected drive value
        /// </summary>
#if NET48
        public Drive CurrentDrive
#else
        public Drive? CurrentDrive
#endif
        {
            get => _currentDrive;
            set
            {
                _currentDrive = value;
                TriggerPropertyChanged(nameof(CurrentDrive));
            }
        }
#if NET48
        private Drive _currentDrive;
#else
        private Drive? _currentDrive;
#endif

        /// <summary>
        /// Indicates the status of the drive combo box
        /// </summary>
        public bool DriveLetterComboBoxEnabled
        {
            get => _driveLetterComboBoxEnabled;
            set
            {
                _driveLetterComboBoxEnabled = value;
                TriggerPropertyChanged(nameof(DriveLetterComboBoxEnabled));
            }
        }
        private bool _driveLetterComboBoxEnabled;

        /// <summary>
        /// Currently selected drive speed value
        /// </summary>
        public int DriveSpeed
        {
            get => _driveSpeed;
            set
            {
                _driveSpeed = value;
                TriggerPropertyChanged(nameof(DriveSpeed));
            }
        }
        private int _driveSpeed;

        /// <summary>
        /// Indicates the status of the drive speed combo box
        /// </summary>
        public bool DriveSpeedComboBoxEnabled
        {
            get => _driveSpeedComboBoxEnabled;
            set
            {
                _driveSpeedComboBoxEnabled = value;
                TriggerPropertyChanged(nameof(DriveSpeedComboBoxEnabled));
            }
        }
        private bool _driveSpeedComboBoxEnabled;

        /// <summary>
        /// Currently selected dumping program
        /// </summary>
        public InternalProgram CurrentProgram
        {
            get => _currentProgram;
            set
            {
                _currentProgram = value;
                TriggerPropertyChanged(nameof(CurrentProgram));
            }
        }
        private InternalProgram _currentProgram;

        /// <summary>
        /// Indicates the status of the dumping program combo box
        /// </summary>
        public bool DumpingProgramComboBoxEnabled
        {
            get => _dumpingProgramComboBoxEnabled;
            set
            {
                _dumpingProgramComboBoxEnabled = value;
                TriggerPropertyChanged(nameof(DumpingProgramComboBoxEnabled));
            }
        }
        private bool _dumpingProgramComboBoxEnabled;

        /// <summary>
        /// Currently provided parameters
        /// </summary>
        public string Parameters
        {
            get => _parameters;
            set
            {
                _parameters = value;
                TriggerPropertyChanged(nameof(Parameters));
            }
        }
        private string _parameters;

        /// <summary>
        /// Indicates the status of the parameters text box
        /// </summary>
        public bool ParametersCheckBoxEnabled
        {
            get => _parametersCheckBoxEnabled;
            set
            {
                _parametersCheckBoxEnabled = value;
                TriggerPropertyChanged(nameof(ParametersCheckBoxEnabled));
            }
        }
        private bool _parametersCheckBoxEnabled;

        /// <summary>
        /// Indicates the status of the parameters check box
        /// </summary>
        public bool EnableParametersCheckBoxEnabled
        {
            get => _enableParametersCheckBoxEnabled;
            set
            {
                _enableParametersCheckBoxEnabled = value;
                TriggerPropertyChanged(nameof(EnableParametersCheckBoxEnabled));
            }
        }
        private bool _enableParametersCheckBoxEnabled;

        /// <summary>
        /// Indicates the status of the start/stop button
        /// </summary>
        public bool StartStopButtonEnabled
        {
            get => _startStopButtonEnabled;
            set
            {
                _startStopButtonEnabled = value;
                TriggerPropertyChanged(nameof(StartStopButtonEnabled));
            }
        }
        private bool _startStopButtonEnabled;

        /// <summary>
        /// Current value for the start/stop dumping button
        /// </summary>
        public object StartStopButtonText
        {
            get => _startStopButtonText;
            set
            {
                _startStopButtonText = (value as string) ?? string.Empty;
                TriggerPropertyChanged(nameof(StartStopButtonText));
            }
        }
        private string _startStopButtonText;

        /// <summary>
        /// Indicates the status of the media scan button
        /// </summary>
        public bool MediaScanButtonEnabled
        {
            get => _mediaScanButtonEnabled;
            set
            {
                _mediaScanButtonEnabled = value;
                TriggerPropertyChanged(nameof(MediaScanButtonEnabled));
            }
        }
        private bool _mediaScanButtonEnabled;

        /// <summary>
        /// Indicates the status of the update volume label button
        /// </summary>
        public bool UpdateVolumeLabelEnabled
        {
            get => _updateVolumeLabelEnabled;
            set
            {
                _updateVolumeLabelEnabled = value;
                TriggerPropertyChanged(nameof(UpdateVolumeLabelEnabled));
            }
        }
        private bool _updateVolumeLabelEnabled;

        /// <summary>
        /// Indicates the status of the copy protect scan button
        /// </summary>
        public bool CopyProtectScanButtonEnabled
        {
            get => _copyProtectScanButtonEnabled;
            set
            {
                _copyProtectScanButtonEnabled = value;
                TriggerPropertyChanged(nameof(CopyProtectScanButtonEnabled));
            }
        }
        private bool _copyProtectScanButtonEnabled;

        /// <summary>
        /// Currently displayed status
        /// </summary>
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                TriggerPropertyChanged(nameof(Status));
            }
        }
        private string _status;

        /// <summary>
        /// Indicates the status of the log panel
        /// </summary>
        public bool LogPanelExpanded
        {
            get => _logPanelExpanded;
            set
            {
                _logPanelExpanded = value;
                TriggerPropertyChanged(nameof(LogPanelExpanded));
            }
        }
        private bool _logPanelExpanded;

        #endregion

        #region List Properties

        /// <summary>
        /// Current list of drives
        /// </summary>
        public List<Drive> Drives
        {
            get => _drives;
            set
            {
                _drives = value;
                TriggerPropertyChanged(nameof(Drives));
            }
        }
        private List<Drive> _drives;

        /// <summary>
        /// Current list of drive speeds
        /// </summary>
        public List<int> DriveSpeeds
        {
            get => _driveSpeeds;
            set
            {
                _driveSpeeds = value;
                TriggerPropertyChanged(nameof(DriveSpeeds));
            }
        }
        private List<int> _driveSpeeds;

        /// <summary>
        /// Current list of supported media types
        /// </summary>
#if NET48
        public List<Element<MediaType>> MediaTypes
#else
        public List<Element<MediaType>>? MediaTypes
#endif
        {
            get => _mediaTypes;
            set
            {
                _mediaTypes = value;
                TriggerPropertyChanged(nameof(MediaTypes));
            }
        }
#if NET48
        private List<Element<MediaType>> _mediaTypes;
#else
        private List<Element<MediaType>>? _mediaTypes;
#endif

        /// <summary>
        /// Current list of supported system profiles
        /// </summary>
        public List<RedumpSystemComboBoxItem> Systems
        {
            get => _systems;
            set
            {
                _systems = value;
                TriggerPropertyChanged(nameof(Systems));
            }
        }
        private List<RedumpSystemComboBoxItem> _systems;

        /// <summary>
        /// List of available internal programs
        /// </summary>
        public List<Element<InternalProgram>> InternalPrograms
        {
            get => _internalPrograms;
            set
            {
                _internalPrograms = value;
                TriggerPropertyChanged(nameof(InternalPrograms));
            }
        }
        private List<Element<InternalProgram>> _internalPrograms;

        #endregion

        /// <summary>
        /// Generic constructor
        /// </summary>
        public MainViewModel()
        {
            _options = OptionsLoader.LoadFromConfig();

            // Added to clear warnings, all are set externally
            _drives = new List<Drive>();
            _driveSpeeds = new List<int>();
            _internalPrograms = new List<Element<InternalProgram>>();
            _outputPath = string.Empty;
            _parameters = string.Empty;
            _startStopButtonText = string.Empty;
            _status = string.Empty;
            _systems = new List<RedumpSystemComboBoxItem>();

            OptionsMenuItemEnabled = true;
            SystemTypeComboBoxEnabled = true;
            MediaTypeComboBoxEnabled = true;
            OutputPathTextBoxEnabled = true;
            OutputPathBrowseButtonEnabled = true;
            DriveLetterComboBoxEnabled = true;
            DumpingProgramComboBoxEnabled = true;
            StartStopButtonEnabled = true;
            StartStopButtonText = Interface.StartDumping;
            MediaScanButtonEnabled = true;
            EnableParametersCheckBoxEnabled = true;
            LogPanelExpanded = _options.OpenLogWindowAtStartup;

            MediaTypes = new List<Element<MediaType>>();
            Systems = RedumpSystemComboBoxItem.GenerateElements().ToList();
            InternalPrograms = new List<Element<InternalProgram>>();
        }

        /// <summary>
        /// Initialize the main window after loading
        /// </summary>
        public void Init(
            Action<LogLevel, string> loggerAction,
            Func<string, string, int, bool, bool?> displayUserMessage,
#if NET48
            Func<SubmissionInfo, (bool?, SubmissionInfo)> processUserInfo)
#else
            Func<SubmissionInfo?, (bool?, SubmissionInfo?)> processUserInfo)
#endif
        {
            // Set the callbacks
            _logger = loggerAction;
            _displayUserMessage = displayUserMessage;
            _processUserInfo = processUserInfo;

            // Finish initializing the rest of the values
            InitializeUIValues(removeEventHandlers: false, rescanDrives: true);
        }

        #region Property Updates

        /// <summary>
        /// Trigger a property changed event
        /// </summary>
        private void TriggerPropertyChanged(string propertyName)
        {
            // Disable event handlers temporarily
            bool cachedCanExecuteSelectionChanged = CanExecuteSelectionChanged;
            DisableEventHandlers();

            // If the property change event is initialized
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            // Reenable event handlers, if necessary
            if (cachedCanExecuteSelectionChanged) EnableEventHandlers();
        }

        #endregion

        #region Population

        /// <summary>
        /// Get a complete list of active disc drives and fill the combo box
        /// </summary>
        /// <remarks>TODO: Find a way for this to periodically run, or have it hook to a "drive change" event</remarks>
        private void PopulateDrives()
        {
            // Disable other UI updates
            bool cachedCanExecuteSelectionChanged = CanExecuteSelectionChanged;
            DisableEventHandlers();

            VerboseLogLn("Scanning for drives..");

            // Always enable the media scan
            this.MediaScanButtonEnabled = true;
            this.UpdateVolumeLabelEnabled = true;

            // If we have a selected drive, keep track of it
            char? lastSelectedDrive = this.CurrentDrive?.Name?[0] ?? null;

            // Populate the list of drives and add it to the combo box
            Drives = Drive.CreateListOfDrives(this.Options.IgnoreFixedDrives);

            if (Drives.Count > 0)
            {
                VerboseLogLn($"Found {Drives.Count} drives: {string.Join(", ", Drives.Select(d => d.Name))}");

                // Check for the last selected drive, if possible
                int index = -1;
                if (lastSelectedDrive != null)
                    index = Drives.FindIndex(d => d.MarkedActive && (d.Name?[0] ?? '\0') == lastSelectedDrive);

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
                CurrentDrive = (index != -1 ? Drives[index] : Drives[0]);
                this.Status = "Valid drive found! Choose your Media Type";
                this.CopyProtectScanButtonEnabled = true;

                // Get the current system type
                if (index != -1)
                    DetermineSystemType();

                // Only enable the start/stop if we don't have the default selected
                this.StartStopButtonEnabled = ShouldEnableDumpingButton();
            }
            else
            {
                VerboseLogLn("Found no drives");
                this.CurrentDrive = null;
                this.Status = "No valid drive found!";
                this.StartStopButtonEnabled = false;
                this.CopyProtectScanButtonEnabled = false;
            }

            // Reenable event handlers, if necessary
            if (cachedCanExecuteSelectionChanged) EnableEventHandlers();
        }

        /// <summary>
        /// Populate media type according to system type
        /// </summary>
        private void PopulateMediaType()
        {
            // Disable other UI updates
            bool cachedCanExecuteSelectionChanged = CanExecuteSelectionChanged;
            DisableEventHandlers();

            if (this.CurrentSystem != null)
            {
                var mediaTypeValues = this.CurrentSystem.MediaTypes();
                int index = mediaTypeValues.FindIndex(m => m == this.CurrentMediaType);
                if (this.CurrentMediaType != null && index == -1)
                    VerboseLogLn($"Disc of type '{CurrentMediaType.LongName()}' found, but the current system does not support it!");

                MediaTypes = mediaTypeValues.Select(m => new Element<MediaType>(m ?? MediaType.NONE)).ToList();
                this.MediaTypeComboBoxEnabled = MediaTypes.Count > 1;
                this.CurrentMediaType = (index > -1 ? MediaTypes[index] : MediaTypes[0]);
            }
            else
            {
                this.MediaTypeComboBoxEnabled = false;
                this.MediaTypes = null;
                this.CurrentMediaType = null;
            }

            // Reenable event handlers, if necessary
            if (cachedCanExecuteSelectionChanged) EnableEventHandlers();
        }

        /// <summary>
        /// Populate media type according to system type
        /// </summary>
        private void PopulateInternalPrograms()
        {
            // Disable other UI updates
            bool cachedCanExecuteSelectionChanged = CanExecuteSelectionChanged;
            DisableEventHandlers();

            // Get the current internal program
            InternalProgram internalProgram = this.Options.InternalProgram;

            // Create a static list of supported programs, not everything
            var internalPrograms = new List<InternalProgram> { InternalProgram.DiscImageCreator, InternalProgram.Aaru, InternalProgram.Redumper };
            InternalPrograms = internalPrograms.Select(ip => new Element<InternalProgram>(ip)).ToList();

            // Select the current default dumping program
            int currentIndex = InternalPrograms.FindIndex(m => m == internalProgram);
            this.CurrentProgram = (currentIndex > -1 ? InternalPrograms[currentIndex].Value : InternalPrograms[0].Value);

            // Reenable event handlers, if necessary
            if (cachedCanExecuteSelectionChanged) EnableEventHandlers();
        }

        #endregion

        #region UI Commands

        /// <summary>
        /// Change the currently selected dumping program
        /// </summary>
        public void ChangeDumpingProgram()
        {
            VerboseLogLn($"Changed dumping program to: {((InternalProgram?)this.CurrentProgram).LongName()}");
            EnsureDiscInformation();
            GetOutputNames(false);
        }

        /// <summary>
        /// Change the currently selected media type
        /// </summary>
        public void ChangeMediaType(System.Collections.IList removedItems, System.Collections.IList addedItems)
        {
            // Only change the media type if the selection and not the list has changed
            if ((removedItems == null || removedItems.Count == 1) && (addedItems == null || addedItems.Count == 1))
                SetSupportedDriveSpeed();

            GetOutputNames(false);
            EnsureDiscInformation();
        }

        /// <summary>
        /// Change the currently selected system
        /// </summary>
        public void ChangeSystem()
        {
            VerboseLogLn($"Changed system to: {this.CurrentSystem.LongName()}");
            PopulateMediaType();
            GetOutputNames(false);
            EnsureDiscInformation();
        }

        /// <summary>
        /// Check for available updates
        /// </summary>
#if NET48
        public (bool, string, string) CheckForUpdates()
#else
        public (bool, string, string?) CheckForUpdates()
#endif
        {
            (bool different, string message, var url) = Tools.CheckForNewVersion();

            SecretLogLn(message);
            if (url == null)
                message = "An exception occurred while checking for versions, please try again later. See the log window for more details.";

            return (different, message, url);
        }

        /// <summary>
        /// Build the about text 
        /// </summary>
        /// <returns></returns>
        public string CreateAboutText()
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
            SecretLogLn(aboutText);
            return aboutText;
        }

        /// <summary>
        /// Build a dummy SubmissionInfo
        /// </summary>
        public static SubmissionInfo CreateDebugSubmissionInfo()
        {
            return new SubmissionInfo()
            {
                SchemaVersion = 1,
                FullyMatchedID = 3,
                PartiallyMatchedIDs = new List<int> { 0, 1, 2, 3 },
                Added = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,

                CommonDiscInfo = new CommonDiscInfoSection()
                {
                    System = SabreTools.RedumpLib.Data.RedumpSystem.IBMPCcompatible,
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
        }

        /// <summary>
        /// Toggle the Start/Stop button
        /// </summary>
        public async void ToggleStartStop()
        {
            // Dump or stop the dump
            if (this.StartStopButtonText as string == Interface.StartDumping)
            {
                StartDumping();
            }
            else if (this.StartStopButtonText as string == Interface.StopDumping)
            {
                VerboseLogLn("Canceling dumping process...");
                _environment?.CancelDumping();
                this.CopyProtectScanButtonEnabled = true;

                if (_environment != null && _environment.Options.EjectAfterDump)
                {
                    VerboseLogLn($"Ejecting disc in drive {_environment.Drive?.Name}");
                    await _environment.EjectDisc();
                }

                if (_environment != null && this.Options.DICResetDriveAfterDump)
                {
                    VerboseLogLn($"Resetting drive {_environment.Drive?.Name}");
                    await _environment.ResetDrive();
                }
            }
        }

        /// <summary>
        /// Update the internal options from a closed OptionsWindow
        /// </summary>
        /// <param name="savedSettings">Indicates if the settings were saved or not</param>
        /// <param name="newOptions">Options representing the new, saved values</param>
#if NET48
        public void UpdateOptions(bool savedSettings, Data.Options newOptions)
#else
        public void UpdateOptions(bool savedSettings, Data.Options? newOptions)
#endif
        {
            // Get which options to save
            var optionsToSave = savedSettings ? newOptions : Options;

            // Ensure the first run flag is unset
            var continuingOptions = new Data.Options(optionsToSave);
            continuingOptions.FirstRun = false;
            this.Options = continuingOptions;

            // If settings were changed, reinitialize the UI
            if (savedSettings)
                InitializeUIValues(removeEventHandlers: true, rescanDrives: true);
        }

        #endregion

        #region UI Functionality

        /// <summary>
        /// Performs UI value setup end to end
        /// </summary>
        /// <param name="removeEventHandlers">Whether event handlers need to be removed first</param>
        /// <param name="rescanDrives">Whether drives should be rescanned or not</param>
        public void InitializeUIValues(bool removeEventHandlers, bool rescanDrives)
        {
            // Disable the dumping button
            StartStopButtonEnabled = false;

            // Safely uncheck the parameters box, just in case
            if (ParametersCheckBoxEnabled == true)
            {
                bool cachedCanExecuteSelectionChanged = CanExecuteSelectionChanged;
                DisableEventHandlers();
                ParametersCheckBoxEnabled = false;
                if (cachedCanExecuteSelectionChanged) EnableEventHandlers();
            }

            // Remove event handlers to ensure ordering
            if (removeEventHandlers)
                DisableEventHandlers();

            // Populate the list of drives and determine the system
            if (rescanDrives)
            {
                Status = "Creating drive list, please wait!";
                PopulateDrives();
            }
            else
            {
                DetermineSystemType();
            }

            // Determine current media type, if possible
            PopulateMediaType();
            CacheCurrentDiscType();
            SetCurrentDiscType();

            // Set the dumping program
            PopulateInternalPrograms();

            // Set the initial environment and UI values
            SetSupportedDriveSpeed();
            _environment = DetermineEnvironment();
            GetOutputNames(true);
            EnsureDiscInformation();

            // Enable event handlers
            EnableEventHandlers();

            // Enable the dumping button, if necessary
            StartStopButtonEnabled = ShouldEnableDumpingButton();
        }

        /// <summary>
        /// Performs a fast update of the output path while skipping disc checks
        /// </summary>
        /// <param name="removeEventHandlers">Whether event handlers need to be removed first</param>
        public void FastUpdateLabel(bool removeEventHandlers)
        {
            // Disable the dumping button
            this.StartStopButtonEnabled = false;

            // Safely uncheck the parameters box, just in case
            if (this.ParametersCheckBoxEnabled == true)
            {
                bool cachedCanExecuteSelectionChanged = CanExecuteSelectionChanged;
                this.DisableEventHandlers();
                this.ParametersCheckBoxEnabled = false;
                if (cachedCanExecuteSelectionChanged) EnableEventHandlers();
            }

            // Remove event handlers to ensure ordering
            if (removeEventHandlers)
                DisableEventHandlers();

            // Refresh the drive info
            this.CurrentDrive?.RefreshDrive();

            // Set the initial environment and UI values
            _environment = DetermineEnvironment();
            GetOutputNames(true);
            EnsureDiscInformation();

            // Enable event handlers
            EnableEventHandlers();

            // Enable the dumping button, if necessary
            this.StartStopButtonEnabled = ShouldEnableDumpingButton();
        }

        /// <summary>
        /// Enable all textbox and combobox event handlers
        /// </summary>
        private void EnableEventHandlers()
        {
            CanExecuteSelectionChanged = true;
        }

        /// <summary>
        /// Disable all textbox and combobox event handlers
        /// </summary>
        private void DisableEventHandlers()
        {
            CanExecuteSelectionChanged = false;
        }

        #endregion

        #region Logging

        /// <summary>
        /// Enqueue text to the log
        /// </summary>
        /// <param name="text">Text to write to the log</param>
        private void Log(string text)
        {
            _logger?.Invoke(LogLevel.USER, text);
        }

        /// <summary>
        /// Enqueue text with a newline to the log
        /// </summary>
        /// <param name="text">Text to write to the log</param>
        private void LogLn(string text) => Log(text + "\n");

        /// <summary>
        /// Enqueue error text to the log
        /// </summary>
        /// <param name="text">Text to write to the log</param>
        private void ErrorLog(string text)
        {
            _logger?.Invoke(LogLevel.ERROR, text);
        }

        /// <summary>
        /// Enqueue error text with a newline to the log
        /// </summary>
        /// <param name="text">Text to write to the log</param>
        private void ErrorLogLn(string text) => ErrorLog(text + "\n");

        /// <summary>
        /// Enqueue secret text to the log
        /// </summary>
        /// <param name="text">Text to write to the log</param>
        private void SecretLog(string text)
        {
            _logger?.Invoke(LogLevel.SECRET, text);
        }

        /// <summary>
        /// Enqueue secret text with a newline to the log
        /// </summary>
        /// <param name="text">Text to write to the log</param>
        private void SecretLogLn(string text) => SecretLog(text + "\n");

        /// <summary>
        /// Enqueue verbose text to the log
        /// </summary>
        /// <param name="text">Text to write to the log</param>
        private void VerboseLog(string text)
        {
            if (_logger != null && Options.VerboseLogging)
                _logger(LogLevel.VERBOSE, text);
        }

        /// <summary>
        /// Enqueue verbose text with a newline to the log
        /// </summary>
        /// <param name="text">Text to write to the log</param>
        private void VerboseLogLn(string text)
        {
            if (Options.VerboseLogging)
                VerboseLog(text + "\n");
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Cache the current disc type to internal variable
        /// </summary>
        private void CacheCurrentDiscType()
        {
            // If the selected item is invalid, we just skip
            if (this.CurrentDrive == null)
                return;

            // Get reasonable default values based on the current system
            MediaType? defaultMediaType = this.CurrentSystem.MediaTypes().FirstOrDefault() ?? MediaType.CDROM;
            if (defaultMediaType == MediaType.NONE)
                defaultMediaType = MediaType.CDROM;

            // If we're skipping detection, set the default value
            if (this.Options.SkipMediaTypeDetection)
            {
                VerboseLogLn($"Media type detection disabled, defaulting to {defaultMediaType.LongName()}.");
                CurrentMediaType = defaultMediaType;
            }
            // If the drive is marked active, try to read from it
            else if (this.CurrentDrive.MarkedActive)
            {
                VerboseLog($"Trying to detect media type for drive {this.CurrentDrive.Name} [{this.CurrentDrive.DriveFormat}] using size and filesystem.. ");
                (MediaType? detectedMediaType, var errorMessage) = this.CurrentDrive.GetMediaType(this.CurrentSystem);

                // If we got an error message, post it to the log
                if (errorMessage != null)
                    VerboseLogLn($"Message from detecting media type: {errorMessage}");

                // If we got either an error or no media, default to the current System default
                if (detectedMediaType == null)
                {
                    VerboseLogLn($"Unable to detect, defaulting to {defaultMediaType.LongName()}.");
                    CurrentMediaType = defaultMediaType;
                }
                else
                {
                    VerboseLogLn($"Detected {detectedMediaType.LongName()}.");
                    _detectedMediaType = detectedMediaType;
                    CurrentMediaType = detectedMediaType;
                }
            }

            // All other cases, just use the default
            else
            {
                VerboseLogLn($"Drive marked as empty, defaulting to {defaultMediaType.LongName()}.");
                CurrentMediaType = defaultMediaType;
            }
        }

        /// <summary>
        /// Create a DumpEnvironment with all current settings
        /// </summary>
        /// <returns>Filled DumpEnvironment this.Parent</returns>
        private DumpEnvironment DetermineEnvironment()
        {
            return new DumpEnvironment(
                this.Options,
                this.OutputPath,
                this.CurrentDrive,
                this.CurrentSystem,
                this.CurrentMediaType,
                this.CurrentProgram,
                this.Parameters);
        }

        /// <summary>
        /// Determine and set the current system type, if allowed
        /// </summary>
        private void DetermineSystemType()
        {
            if (Drives == null || Drives.Count == 0 || this.CurrentDrive == null)
            {
                VerboseLogLn("Skipping system type detection because no valid drives found!");
            }
            else if (this.CurrentDrive?.MarkedActive != true)
            {
                VerboseLogLn("Skipping system type detection because drive not marked as active!");
            }
            else if (!this.Options.SkipSystemDetection)
            {
                VerboseLog($"Trying to detect system for drive {this.CurrentDrive.Name}.. ");
                var currentSystem = this.CurrentDrive?.GetRedumpSystem(this.Options.DefaultSystem) ?? this.Options.DefaultSystem;
                VerboseLogLn(currentSystem == null ? "unable to detect." : ($"detected {currentSystem.LongName()}."));

                if (currentSystem != null)
                {
                    int sysIndex = Systems.FindIndex(s => s == currentSystem);
                    this.CurrentSystem = Systems[sysIndex];
                }
            }
            else if (this.Options.SkipSystemDetection && this.Options.DefaultSystem != null)
            {
                var currentSystem = this.Options.DefaultSystem;
                VerboseLogLn($"System detection disabled, setting to default of {currentSystem.LongName()}.");
                int sysIndex = Systems.FindIndex(s => s == currentSystem);
                this.CurrentSystem = Systems[sysIndex];
            }
        }

        /// <summary>
        /// Disable all UI elements during dumping
        /// </summary>
        public void DisableAllUIElements()
        {
            OptionsMenuItemEnabled = false;
            SystemTypeComboBoxEnabled = false;
            MediaTypeComboBoxEnabled = false;
            OutputPathTextBoxEnabled = false;
            OutputPathBrowseButtonEnabled = false;
            DriveLetterComboBoxEnabled = false;
            DriveSpeedComboBoxEnabled = false;
            DumpingProgramComboBoxEnabled = false;
            EnableParametersCheckBoxEnabled = false;
            StartStopButtonText = Interface.StopDumping;
            MediaScanButtonEnabled = false;
            UpdateVolumeLabelEnabled = false;
            CopyProtectScanButtonEnabled = false;
        }

        /// <summary>
        /// Enable all UI elements after dumping
        /// </summary>
        public void EnableAllUIElements()
        {
            OptionsMenuItemEnabled = true;
            SystemTypeComboBoxEnabled = true;
            MediaTypeComboBoxEnabled = true;
            OutputPathTextBoxEnabled = true;
            OutputPathBrowseButtonEnabled = true;
            DriveLetterComboBoxEnabled = true;
            DriveSpeedComboBoxEnabled = true;
            DumpingProgramComboBoxEnabled = true;
            EnableParametersCheckBoxEnabled = true;
            StartStopButtonText = Interface.StartDumping;
            MediaScanButtonEnabled = true;
            UpdateVolumeLabelEnabled = true;
            CopyProtectScanButtonEnabled = true;
        }

        /// <summary>
        /// Ensure information is consistent with the currently selected disc type
        /// </summary>
        public void EnsureDiscInformation()
        {
            // Get the current environment information
            _environment = DetermineEnvironment();

            // Get the status to write out
            Result result = Tools.GetSupportStatus(_environment.System, _environment.Type);
            this.Status = result.Message;

            // Enable or disable the button
            this.StartStopButtonEnabled = result && ShouldEnableDumpingButton();

            // If we're in a type that doesn't support drive speeds
            this.DriveSpeedComboBoxEnabled = _environment.Type.DoesSupportDriveSpeed();

            // If input params are not enabled, generate the full parameters from the environment
            if (!this.ParametersCheckBoxEnabled)
            {
                var generated = _environment.GetFullParameters(this.DriveSpeed);
                if (generated != null)
                    this.Parameters = generated;
            }
        }

        /// <summary>
        /// Get the default output directory name from the currently selected drive
        /// </summary>
        /// <param name="driveChanged">Force an updated name if the drive letter changes</param>
        public void GetOutputNames(bool driveChanged)
        {
            if (Drives == null || Drives.Count == 0 || this.CurrentDrive == null)
            {
                VerboseLogLn("Skipping output name building because no valid drives found!");
                return;
            }

            // Get the extension for the file for the next two statements
            var extension = _environment?.Parameters?.GetDefaultExtension(this.CurrentMediaType);

            // Set the output filename, if it's not already
            if (string.IsNullOrEmpty(this.OutputPath))
            {
                var label = this.CurrentDrive?.FormattedVolumeLabel ?? this.CurrentSystem.LongName();
                var directory = this.Options.DefaultOutputPath;
                string filename = $"{label}{extension ?? ".bin"}";

                // If the path ends with the label already
                if (directory != null && label != null && directory.EndsWith(label, StringComparison.OrdinalIgnoreCase))
                    directory = Path.GetDirectoryName(directory);

                if (directory != null && label != null)
                    this.OutputPath = Path.Combine(directory, label, filename);
                else
                    this.OutputPath = filename;
            }

            // Set the output filename, if we changed drives
            else if (driveChanged)
            {
                var label = this.CurrentDrive?.FormattedVolumeLabel ?? this.CurrentSystem.LongName();
                string oldPath = InfoTool.NormalizeOutputPaths(this.OutputPath, false);
                string oldFilename = Path.GetFileNameWithoutExtension(oldPath);
                var directory = Path.GetDirectoryName(oldPath);
                string filename = $"{label}{extension ?? ".bin"}";

                // If the previous path included the label
                if (directory != null && directory.EndsWith(oldFilename, StringComparison.OrdinalIgnoreCase))
                    directory = Path.GetDirectoryName(directory);

                // If the path ends with the label already
                if (directory != null && label != null && directory.EndsWith(label, StringComparison.OrdinalIgnoreCase))
                    directory = Path.GetDirectoryName(directory);

                if (directory != null && label != null)
                    this.OutputPath = Path.Combine(directory, label, filename);
                else
                    this.OutputPath = filename;
            }

            // Otherwise, reset the extension of the currently set path
            else
            {
                string oldPath = InfoTool.NormalizeOutputPaths(this.OutputPath, false);
                string filename = Path.GetFileNameWithoutExtension(oldPath);
                var directory = Path.GetDirectoryName(oldPath);
                filename = $"{filename}{extension ?? ".bin"}";

                if (directory != null)
                    this.OutputPath = Path.Combine(directory, filename);
                else
                    this.OutputPath = filename;
            }
        }

        /// <summary>
        /// Process the current custom parameters back into UI values
        /// </summary>
        public void ProcessCustomParameters()
        {
            _environment?.SetParameters(this.Parameters);
            if (_environment?.Parameters == null)
                return;

            // Catch this in case there's an input path issue
            try
            {
                int driveIndex = Drives.Select(d => d.Name?[0] ?? '\0').ToList().IndexOf(_environment.Parameters?.InputPath?[0] ?? default);
                this.CurrentDrive = (driveIndex != -1 ? Drives[driveIndex] : Drives[0]);
            }
            catch { }

            int driveSpeed = _environment.Parameters?.Speed ?? -1;
            if (driveSpeed > 0)
                this.DriveSpeed = driveSpeed;
            else if (_environment.Parameters != null)
                _environment.Parameters.Speed = this.DriveSpeed;

            // Disable change handling
            DisableEventHandlers();

            this.OutputPath = InfoTool.NormalizeOutputPaths(_environment.Parameters?.OutputPath, false);

            if (MediaTypes != null)
            {
                MediaType? mediaType = _environment.Parameters?.GetMediaType();
                int mediaTypeIndex = MediaTypes.FindIndex(m => m == mediaType);
                this.CurrentMediaType = (mediaTypeIndex > -1 ? MediaTypes[mediaTypeIndex] : MediaTypes[0]);
            }

            // Reenable change handling
            EnableEventHandlers();
        }

        /// <summary>
        /// Scan and show copy protection for the current disc
        /// </summary>
#if NET48
        public async Task<(string, string)> ScanAndShowProtection()
#else
        public async Task<(string?, string?)> ScanAndShowProtection()
#endif
        {
            // Determine current environment, just in case
#if NET48
            if (_environment == null)
                _environment = DetermineEnvironment();
#else
            _environment ??= DetermineEnvironment();
#endif

            // If we don't have a valid drive
            if (this.CurrentDrive?.Name == null)
                return (null, "No valid drive found!");

            VerboseLogLn($"Scanning for copy protection in {this.CurrentDrive.Name}");

            var tempContent = this.Status;
            this.Status = "Scanning for copy protection... this might take a while!";
            this.StartStopButtonEnabled = false;
            this.MediaScanButtonEnabled = false;
            this.UpdateVolumeLabelEnabled = false;
            this.CopyProtectScanButtonEnabled = false;

            var progress = new Progress<ProtectionProgress>();
            progress.ProgressChanged += ProgressUpdated;
            var (protections, error) = await Protection.RunProtectionScanOnPath(this.CurrentDrive.Name, this.Options, progress);
            var output = Protection.FormatProtections(protections);

            // If SmartE is detected on the current disc, remove `/sf` from the flags for DIC only -- Disabled until further notice
            //if (Env.InternalProgram == InternalProgram.DiscImageCreator && output.Contains("SmartE"))
            //{
            //    ((Modules.DiscImageCreator.Parameters)Env.Parameters)[Modules.DiscImageCreator.FlagStrings.ScanFileProtect] = false;
            //    if (this.Options.VerboseLogging)
            //        this.Logger.VerboseLogLn($"SmartE detected, removing {Modules.DiscImageCreator.FlagStrings.ScanFileProtect} from parameters");
            //}

            if (string.IsNullOrEmpty(error))
                LogLn($"Detected the following protections in {this.CurrentDrive.Name}:\r\n\r\n{output}");
            else
                ErrorLogLn($"Path could not be scanned! Exception information:\r\n\r\n{error}");

            this.Status = tempContent;
            this.StartStopButtonEnabled = ShouldEnableDumpingButton();
            this.MediaScanButtonEnabled = true;
            this.UpdateVolumeLabelEnabled = true;
            this.CopyProtectScanButtonEnabled = true;

            return (output, error);
        }

        /// <summary>
        /// Set the current disc type in the combo box
        /// </summary>
        private void SetCurrentDiscType()
        {
            // If we don't have any selected media types, we don't care and return
            if (MediaTypes == null)
                return;

            // If we have a detected media type, use that first
            if (_detectedMediaType != null)
            {
                int detectedIndex = MediaTypes.FindIndex(kvp => kvp.Value == _detectedMediaType);
                if (detectedIndex > -1)
                {
                    CurrentMediaType = _detectedMediaType;
                    return;
                }
            }

            // If we have an invalid current type, we don't care and return
            if (CurrentMediaType == null || CurrentMediaType == MediaType.NONE)
                return;

            // Now set the selected item, if possible
            int index = MediaTypes.FindIndex(kvp => kvp.Value == CurrentMediaType);
            if (this.CurrentMediaType != null && index == -1)
                VerboseLogLn($"Disc of type '{CurrentMediaType.LongName()}' found, but the current system does not support it!");

            this.CurrentMediaType = (index > -1 ? MediaTypes[index] : MediaTypes[0]);
        }

        /// <summary>
        /// Set the drive speed based on reported maximum and user-defined option
        /// </summary>
        public void SetSupportedDriveSpeed()
        {
            // Set the drive speed list that's appropriate
            this.DriveSpeeds = (List<int>)Interface.GetSpeedsForMediaType(CurrentMediaType);
            VerboseLogLn($"Supported media speeds: {string.Join(", ", this.DriveSpeeds)}");

            // Set the selected speed
            int speed;
            switch (this.CurrentMediaType)
            {
                case MediaType.CDROM:
                case MediaType.GDROM:
                    speed = this.Options.PreferredDumpSpeedCD;
                    break;
                case MediaType.DVD:
                case MediaType.NintendoGameCubeGameDisc:
                case MediaType.NintendoWiiOpticalDisc:
                    speed = this.Options.PreferredDumpSpeedDVD;
                    break;
                case MediaType.HDDVD:
                    speed = this.Options.PreferredDumpSpeedHDDVD;
                    break;
                case MediaType.BluRay:
                    speed = this.Options.PreferredDumpSpeedBD;
                    break;
                default:
                    speed = this.Options.PreferredDumpSpeedCD;
                    break;
            }

            VerboseLogLn($"Setting drive speed to: {speed}");
            this.DriveSpeed = speed;
        }

        /// <summary>
        /// Determine if the dumping button should be enabled
        /// </summary>
        private bool ShouldEnableDumpingButton()
        {
            return Drives != null
                && Drives.Count > 0
                && this.CurrentSystem != null
                && this.CurrentMediaType != null
                && Tools.ProgramSupportsMedia(this.CurrentProgram, this.CurrentMediaType);
        }

        /// <summary>
        /// Begin the dumping process using the given inputs
        /// </summary>
        public async void StartDumping()
        {
            // One last check to determine environment, just in case
            _environment = DetermineEnvironment();

            // Force an internal drive refresh in case the user entered things manually
            _environment.Drive?.RefreshDrive();

            // If still in custom parameter mode, check that users meant to continue or not
            if (this.ParametersCheckBoxEnabled == true && _displayUserMessage != null)
            {
                bool? result = _displayUserMessage("Custom Changes", "It looks like you have custom parameters that have not been saved. Would you like to apply those changes before starting to dump?", 3, true);
                if (result == true)
                {
                    this.ParametersCheckBoxEnabled = false;
                    ProcessCustomParameters();
                }
                else if (result == null)
                {
                    return;
                }
                // If false, then we continue with the current known environment
            }

            // Run path adjustments for DiscImageCreator -- Disabled until further notice
            //Env.AdjustPathsForDiscImageCreator();

            try
            {
                // Run pre-dumping validation checks
                if (!ValidateBeforeDumping())
                    return;

                // Disable all UI elements apart from dumping button
                DisableAllUIElements();

                // Refresh the drive, if it wasn't null
                _environment.Drive?.RefreshDrive();

                // Output to the label and log
                this.Status = "Starting dumping process... Please wait!";
                LogLn("Starting dumping process... Please wait!");
                if (this.Options.ToolsInSeparateWindow)
                    LogLn("Look for the separate command window for more details");
                else
                    LogLn("Program outputs may be slow to populate in the log window");

                // Get progress indicators
                var resultProgress = new Progress<Result>();
                resultProgress.ProgressChanged += ProgressUpdated;
                var protectionProgress = new Progress<ProtectionProgress>();
                protectionProgress.ProgressChanged += ProgressUpdated;
                _environment.ReportStatus += ProgressUpdated;

                // Run the program with the parameters
                Result result = await _environment.Run(resultProgress);

                // If we didn't execute a dumping command we cannot get submission output
                if (_environment.Parameters?.IsDumpingCommand() != true)
                {
                    LogLn("No dumping command was run, submission information will not be gathered.");
                    this.Status = "Execution complete!";

                    // Reset all UI elements
                    EnableAllUIElements();
                    return;
                }

                // Verify dump output and save it
                if (result)
                {
                    result = await _environment.VerifyAndSaveDumpOutput(resultProgress, protectionProgress, _processUserInfo);
                }
                else
                {
                    ErrorLogLn(result.Message);
                    this.Status = "Execution failed!";
                }
            }
            catch (Exception ex)
            {
                ErrorLogLn(ex.ToString());
                this.Status = "An exception occurred!";
            }
            finally
            {
                // Reset all UI elements
                EnableAllUIElements();
            }
        }

        /// <summary>
        /// Toggle the parameters input box
        /// </summary>
        public void ToggleParameters()
        {
            if (ParametersCheckBoxEnabled == true)
            {
                SystemTypeComboBoxEnabled = false;
                MediaTypeComboBoxEnabled = false;

                OutputPathTextBoxEnabled = false;
                OutputPathBrowseButtonEnabled = false;

                MediaScanButtonEnabled = false;
                UpdateVolumeLabelEnabled = false;
                CopyProtectScanButtonEnabled = false;
            }
            else
            {
                ProcessCustomParameters();

                SystemTypeComboBoxEnabled = true;
                MediaTypeComboBoxEnabled = true;

                OutputPathTextBoxEnabled = true;
                OutputPathBrowseButtonEnabled = true;

                MediaScanButtonEnabled = true;
                UpdateVolumeLabelEnabled = true;
                CopyProtectScanButtonEnabled = true;
            }
        }

        /// <summary>
        /// Perform validation, including user input, before attempting to start dumping
        /// </summary>
        /// <returns>True if dumping should start, false otherwise</returns>
        private bool ValidateBeforeDumping()
        {
            // Validate that we have an output path of any sort
            if (string.IsNullOrWhiteSpace(_environment?.OutputPath))
            {
                if (_displayUserMessage != null)
                    _ = _displayUserMessage("Missing Path", "No output path was provided so dumping cannot continue.", 1, false);
                LogLn("Dumping aborted!");
                return false;
            }

            // Validate that the user explicitly wants an inactive drive to be considered for dumping
            if (_environment.Drive?.MarkedActive != true && _displayUserMessage != null)
            {
                string message = "The currently selected drive does not appear to contain a disc! "
                    + (!_environment.System.DetectedByWindows() ? $"This is normal for {_environment.System.LongName()} as the discs may not be readable on Windows. " : string.Empty)
                    + "Do you want to continue?";

                bool? mbresult = _displayUserMessage("No Disc Detected", message, 2, false);
                if (mbresult != true)
                {
                    LogLn("Dumping aborted!");
                    return false;
                }
            }

            // Pre-split the output path
            var outputDirectory = Path.GetDirectoryName(_environment.OutputPath);
            string outputFilename = Path.GetFileName(_environment.OutputPath);

            // If a complete dump already exists
            (bool foundFiles, List<string> _) = InfoTool.FoundAllFiles(outputDirectory, outputFilename, _environment.Parameters, true);
            if (foundFiles && _displayUserMessage != null)
            {
                bool? mbresult = _displayUserMessage("Overwrite?", "A complete dump already exists! Are you sure you want to overwrite?", 2, true);
                if (mbresult != true)
                {
                    LogLn("Dumping aborted!");
                    return false;
                }
            }

            // Validate that at least some space exists
            // TODO: Tie this to the size of the disc, type of disc, etc.
            string fullPath;
            if (string.IsNullOrWhiteSpace(outputDirectory))
                fullPath = Path.GetFullPath(_environment.OutputPath);
            else
                fullPath = Path.GetFullPath(outputDirectory);

            var driveInfo = new DriveInfo(Path.GetPathRoot(fullPath) ?? string.Empty);
            if (driveInfo.AvailableFreeSpace < Math.Pow(2, 30) && _displayUserMessage != null)
            {
                bool? mbresult = _displayUserMessage("Low Space", "There is less than 1gb of space left on the target drive. Are you sure you want to continue?", 2, true);
                if (mbresult != true)
                {
                    LogLn("Dumping aborted!");
                    return false;
                }
            }

            // If nothing above fails, we want to continue
            return true;
        }

#endregion

        #region Progress Reporting

        /// <summary>
        /// Handler for Result ProgressChanged event
        /// </summary>
#if NET48
        private void ProgressUpdated(object sender, string value)
#else
        private void ProgressUpdated(object? sender, string value)
#endif
        {
            try
            {
#if NET48
                value = value ?? string.Empty;
#else
                value ??= string.Empty;
#endif
                LogLn(value);
            }
            catch { }
        }

        /// <summary>
        /// Handler for Result ProgressChanged event
        /// </summary>
#if NET48
        private void ProgressUpdated(object sender, Result value)
#else
        private void ProgressUpdated(object? sender, Result value)
#endif
        {
            var message = value?.Message;

            // Update the label with only the first line of output
#if NET48
            if (message != null && message.Contains("\n"))
#else
            if (message != null && message.Contains('\n'))
#endif
                this.Status = value?.Message?.Split('\n')[0] + " (See log output)";
            else
                this.Status = value?.Message ?? string.Empty;

            // Log based on success or failure
            if (value != null && value)
                VerboseLogLn(message ?? string.Empty);
            else if (value != null && !value)
                ErrorLogLn(message ?? string.Empty);
        }

        /// <summary>
        /// Handler for ProtectionProgress ProgressChanged event
        /// </summary>
#if NET48
        private void ProgressUpdated(object sender, ProtectionProgress value)
#else
        private void ProgressUpdated(object? sender, ProtectionProgress value)
#endif
        {
            string message = $"{value.Percentage * 100:N2}%: {value.Filename} - {value.Protection}";
            this.Status = message;
            VerboseLogLn(message);
        }

#endregion
    }
}
