using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using BinaryObjectScanner;
using MPF.Frontend.ComboBoxItems;
using MPF.Frontend.Tools;
using SabreTools.IO.Extensions;
using SabreTools.RedumpLib.Data;
using SabreTools.RedumpLib.Data.Sections;
using SabreTools.Text.INI;

namespace MPF.Frontend.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        /// Access to the current options
        /// </summary>
        public Options Options
        {
            get => _options;
            set
            {
                _options = value;
                OptionsLoader.SaveToConfig(_options);
            }
        }
        private Options _options;

        /// <summary>
        /// Indicates if SelectionChanged events can be executed
        /// </summary>
        public bool CanExecuteSelectionChanged { get; private set; } = false;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Action to process logging statements
        /// </summary>
        private Action<LogLevel, string>? _logger;

        /// <summary>
        /// Display a message to a user
        /// </summary>
        /// <remarks>
        /// T1 - Title to display to the user
        /// T2 - Message to display to the user
        /// T3 - Number of default options to display
        /// T4 - true for inquiry, false otherwise
        /// TResult - true for positive, false for negative, null for neutral
        /// </remarks>
        private Func<string, string, int, bool, bool?>? _displayUserMessage;

        /// <summary>
        /// Detected media type, distinct from the selected one
        /// </summary>
        private PhysicalMediaType? _detectedPhysicalMediaType;

        /// <summary>
        /// Current dumping environment
        /// </summary>
        private DumpEnvironment? _environment;

        /// <summary>
        /// Function to process user information
        /// </summary>
        private ProcessUserInfoDelegate? _processUserInfo;

        #endregion

        #region Properties

        /// <summary>
        /// Indicates the status of the check dump menu item
        /// </summary>
        public bool AskBeforeQuit
        {
            get => _askBeforeQuit;
            set
            {
                _askBeforeQuit = value;
                TriggerPropertyChanged(nameof(AskBeforeQuit));
            }
        }
        private bool _askBeforeQuit;

        /// <summary>
        /// Indicates the status of the check dump menu item
        /// </summary>
        public bool CheckDumpMenuItemEnabled
        {
            get => _checkDumpMenuItemEnabled;
            set
            {
                _checkDumpMenuItemEnabled = value;
                TriggerPropertyChanged(nameof(CheckDumpMenuItemEnabled));
            }
        }
        private bool _checkDumpMenuItemEnabled;

        /// <summary>
        /// Indicates the status of the create IRD menu item
        /// </summary>
        public bool CreateIRDMenuItemEnabled
        {
            get => _createIRDMenuItemEnabled;
            set
            {
                _createIRDMenuItemEnabled = value;
                TriggerPropertyChanged(nameof(CreateIRDMenuItemEnabled));
            }
        }
        private bool _createIRDMenuItemEnabled;

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
        public PhysicalSystem? CurrentSystem
        {
            get => _currentSystem;
            set
            {
                _currentSystem = value;
                TriggerPropertyChanged(nameof(CurrentSystem));
            }
        }
        private PhysicalSystem? _currentSystem;

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
        public PhysicalMediaType? CurrentPhysicalMediaType
        {
            get => _currentPhysicalMediaType;
            set
            {
                _currentPhysicalMediaType = value;
                TriggerPropertyChanged(nameof(CurrentPhysicalMediaType));
            }
        }
        private PhysicalMediaType? _currentPhysicalMediaType;

        /// <summary>
        /// Indicates the status of the media type combo box
        /// </summary>
        public bool PhysicalMediaTypeComboBoxEnabled
        {
            get => _mediaTypeComboBoxEnabled;
            set
            {
                _mediaTypeComboBoxEnabled = value;
                TriggerPropertyChanged(nameof(PhysicalMediaTypeComboBoxEnabled));
            }
        }
        private bool _mediaTypeComboBoxEnabled;

        /// <summary>
        /// Currently provided output path
        /// Not guaranteed to be a valid path
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
        public Drive? CurrentDrive
        {
            get => _currentDrive;
            set
            {
                _currentDrive = value;
                TriggerPropertyChanged(nameof(CurrentDrive));
            }
        }
        private Drive? _currentDrive;

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
        public bool ParametersTextBoxEnabled
        {
            get => _parametersTextBoxEnabled;
            set
            {
                _parametersTextBoxEnabled = value;
                TriggerPropertyChanged(nameof(ParametersTextBoxEnabled));
            }
        }
        private bool _parametersTextBoxEnabled;

        /// <summary>
        /// Indicates the status of the parameters check box
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
        public List<Element<PhysicalMediaType>>? MediaTypes
        {
            get => _mediaTypes;
            set
            {
                _mediaTypes = value;
                TriggerPropertyChanged(nameof(MediaTypes));
            }
        }
        private List<Element<PhysicalMediaType>>? _mediaTypes;

        /// <summary>
        /// Current list of supported system profiles
        /// </summary>
        public List<PhysicalSystemComboBoxItem> Systems
        {
            get => _systems;
            set
            {
                _systems = value;
                TriggerPropertyChanged(nameof(Systems));
            }
        }
        private List<PhysicalSystemComboBoxItem> _systems;

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

        #region Strings

        private const string DiscNotDetectedValue = "Disc Not Detected";
        private string StartDumpingValue = "Start Dumping";
        private string StopDumpingValue = "Stop Dumping";

        #endregion

        /// <summary>
        /// Generic constructor
        /// </summary>
        public MainViewModel()
        {
            _options = OptionsLoader.LoadFromConfig(out _);

            // Added to clear warnings, all are set externally
            _drives = [];
            _driveSpeeds = [];
            _internalPrograms = [];
            _outputPath = string.Empty;
            _parameters = string.Empty;
            _startStopButtonText = string.Empty;
            _status = string.Empty;
            _systems = [];

            AskBeforeQuit = false;
            OptionsMenuItemEnabled = true;
            CheckDumpMenuItemEnabled = true;
            CreateIRDMenuItemEnabled = true;
            SystemTypeComboBoxEnabled = true;
            PhysicalMediaTypeComboBoxEnabled = true;
            OutputPathTextBoxEnabled = true;
            OutputPathBrowseButtonEnabled = true;
            DriveLetterComboBoxEnabled = true;
            DumpingProgramComboBoxEnabled = true;
            StartStopButtonEnabled = true;
            StartStopButtonText = StartDumpingValue;
            MediaScanButtonEnabled = true;
            ParametersCheckBoxEnabled = true;
            EnableParametersCheckBoxEnabled = true;
            LogPanelExpanded = _options.GUI.OpenLogWindowAtStartup;

            MediaTypes = [];
            Systems = PhysicalSystemComboBoxItem.GenerateElements();
            InternalPrograms = [];
        }

        /// <summary>
        /// Initialize the main window after loading
        /// </summary>
        public void Init(
            Action<LogLevel, string> loggerAction,
            Func<string, string, int, bool, bool?> displayUserMessage,
            ProcessUserInfoDelegate processUserInfo)
        {
            // Set the callbacks
            _logger = loggerAction;
            _displayUserMessage = displayUserMessage;
            _processUserInfo = processUserInfo;

            // Finish initializing the rest of the values
            InitializeUIValues(removeEventHandlers: false, rebuildPrograms: true, rescanDrives: true);
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
            MediaScanButtonEnabled = true;
            UpdateVolumeLabelEnabled = true;

            // If we have a selected drive, keep track of it
            char? lastSelectedDrive = CurrentDrive?.Name?[0] ?? null;

            // Populate the list of drives and add it to the combo box
            Drives = Drive.CreateListOfDrives(Options.GUI.IgnoreFixedDrives);

            if (Drives.Count > 0)
            {
                VerboseLogLn($"Found {Drives.Count} drives: {string.Join(", ", [.. Drives.ConvertAll(d => d.Name)])}");

                // Check for the last selected drive, if possible
                int index = -1;
                if (lastSelectedDrive is not null)
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
                CurrentDrive = index != -1 ? Drives[index] : Drives[0];
                Status = "Valid drive found!";
                CopyProtectScanButtonEnabled = true;

                // Get the current system type
                DetermineSystemType();

                // Only enable the start/stop if we don't have the default selected
                StartStopButtonEnabled = ShouldEnableDumpingButton();
            }
            else
            {
                VerboseLogLn("Found no drives");
                CurrentDrive = null;
                Status = "No valid drive found!";
                StartStopButtonEnabled = false;
                CopyProtectScanButtonEnabled = false;
            }

            // Reenable event handlers, if necessary
            if (cachedCanExecuteSelectionChanged) EnableEventHandlers();
        }

        /// <summary>
        /// Populate media type according to system type
        /// </summary>
        private void PopulatePhysicalMediaType()
        {
            // Disable other UI updates
            bool cachedCanExecuteSelectionChanged = CanExecuteSelectionChanged;
            DisableEventHandlers();

            if (CurrentSystem is not null)
            {
                var mediaTypeValues = CurrentSystem.MediaTypes();
                int index = mediaTypeValues.FindIndex(m => m == CurrentPhysicalMediaType);
                if (CurrentPhysicalMediaType is not null && index == -1)
                    VerboseLogLn($"Disc of type '{CurrentPhysicalMediaType.LongName()}' found, but the current system does not support it!");

                MediaTypes = mediaTypeValues.ConvertAll(m => new Element<PhysicalMediaType>(m ?? PhysicalMediaType.NONE));
                PhysicalMediaTypeComboBoxEnabled = MediaTypes.Count > 1;
                CurrentPhysicalMediaType = index > -1 ? MediaTypes[index] : MediaTypes[0];
            }
            else
            {
                PhysicalMediaTypeComboBoxEnabled = false;
                MediaTypes = null;
                CurrentPhysicalMediaType = null;
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

            // Create a static list of supported programs, not everything
#if NET5_0_OR_GREATER
            var ipArr = Enum.GetValues<InternalProgram>();
#else
            var ipArr = (InternalProgram[])Enum.GetValues(typeof(InternalProgram));
#endif
            ipArr = Array.FindAll(ipArr, ip => InternalProgramExists(ip));
            InternalPrograms = [.. Array.ConvertAll(ipArr, ip => new Element<InternalProgram>(ip))];

            // Get the current internal program
            InternalProgram internalProgram = Options.InternalProgram;

            // Select the current default dumping program
            if (InternalPrograms.Count == 0)
            {
                // If no programs are found, default to InternalProgram.NONE
                CurrentProgram = InternalProgram.NONE;
            }
            else
            {
                int currentIndex = InternalPrograms.FindIndex(m => m == internalProgram);
                CurrentProgram = currentIndex > -1 ? InternalPrograms[currentIndex].Value : InternalPrograms[0].Value;
            }

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
            VerboseLogLn($"Changed dumping program to: {((InternalProgram?)CurrentProgram).LongName()}");
            EnsureMediaInformation();
            // New output name depends on new environment
            GetOutputNames(false);
            // New environment depends on new output name
            EnsureMediaInformation();
        }

        /// <summary>
        /// Change the currently selected media type
        /// </summary>
        public void ChangePhysicalMediaType(System.Collections.IList removedItems, System.Collections.IList addedItems)
        {
            // Only change the media type if the selection and not the list has changed
            if ((removedItems is null || removedItems.Count == 1) && (addedItems is null || addedItems.Count == 1))
                SetSupportedDriveSpeed();

            GetOutputNames(false);
            EnsureMediaInformation();
        }

        /// <summary>
        /// Change the currently selected system
        /// </summary>
        public void ChangeSystem()
        {
            VerboseLogLn($"Changed system to: {CurrentSystem.LongName()}");
            PopulatePhysicalMediaType();
            GetOutputNames(false);
            EnsureMediaInformation();
        }

        /// <summary>
        /// Check for available updates
        /// </summary>
        public void CheckForUpdates(out bool different, out string message, out string? url)
        {
            FrontendTool.CheckForNewVersion(out different, out message, out url);

            SecretLogLn(message);
            if (url is null)
                message = "An exception occurred while checking for remote versions. See the log window for more details.";
        }

        /// <summary>
        /// Build a dummy SubmissionInfo
        /// </summary>
        public static SubmissionInfo CreateDebugSubmissionInfo()
        {
            return new SubmissionInfo()
            {
                SchemaVersion = 1,
                FullyMatchedIDs = [3],
                PartiallyMatchedIDs = [0, 1, 2, 3],
                Added = DateTime.UtcNow,
                LastModified = DateTime.UtcNow,

                DiscIdentity = new DiscIdentitySection()
                {
                    System = PhysicalSystem.IBMPCcompatible,
                    Media = MediaType.BD128,
                    Category = DiscCategory.Games,
                    Title = "Game Title",
                    ForeignTitle = "Foreign Game Title",
                    DiscNumber = "1",
                    DiscTitle = "Install Disc",
                    FilenameSuffix = "Alt",
                },

                RegionsAndLanguages = new RegionsAndLanguagesSection()
                {
                    Regions = [Region.World],
                    Languages = [Language.English, Language.Spanish, Language.French],
                },

                DiscIdentifiers = new DiscIdentifiersSection()
                {
                    DiscSerials = "Disc Serial",
                    Editions = "Rerelease",
                    Barcodes = "UPC Barcode",
                    Version = "Original",
                    ErrorCount = "0",
                    EXEDate = "19xx-xx-xx",
                    EDC = YesNo.Yes,
                    Layerbreak = 0,
                    Layerbreak2 = 1,
                    Layerbreak3 = 2,
                    DiscID = "Disc ID",
                    DiscKey = "Disc key",
                    UniversalHash = "Universal Hash",
                },

                RingCodes = new RingCodesSection()
                {
                    Layer0MasteringCode = "L0 Mastering Code",
                    Layer0MasteringSID = "L0 Mastering SID",
                    Layer0Toolstamps = "L0 Toolstamp",
                    Layer0MouldSIDs = "L0 Mould SIDs",
                    Layer0AdditionalMoulds = "L0 Additional Moulds",

                    Layer1MasteringCode = "L1 Mastering Code",
                    Layer1MasteringSID = "L1 Mastering SID",
                    Layer1Toolstamps = "L1 Toolstamp",
                    Layer1MouldSIDs = "L1 Mould SIDs",
                    Layer1AdditionalMoulds = "L1 Additional Moulds",

                    Layer2MasteringCode = "L2 Mastering Code",
                    Layer2MasteringSID = "L2 Mastering SID",
                    Layer2Toolstamps = "L2 Toolstamp",
                    Layer2MouldSIDs = "L2 Mould SIDs",
                    Layer2AdditionalMoulds = "L2 Additional Moulds",

                    Layer3MasteringCode = "L3 Mastering Code",
                    Layer3MasteringSID = "L3 Mastering SID",
                    Layer3Toolstamps = "L3 Toolstamp",
                    Layer3MouldSIDs = "L3 Mould SIDs",
                    Layer3AdditionalMoulds = "L3 Additional Moulds",

                    LabelSideMasteringCode = "Label Side Mastering Code",
                    LabelSideMasteringSID = "Label Side Mastering SID",
                    LabelSideToolstamps = "Label Side Toolstamp",
                    LabelSideMouldSIDs = "Label Side Mould SIDs",
                    LabelSideAdditionalMoulds = "Label Side Additional Moulds",

                    WriteOffset = "+12",
                    SampleStart = "+123",
                },

                DumpMetadata = new DumpMetadataSection()
                {
                    Comments = "Comment data line 1\r\nComment data line 2",
                    CommentsSpecialFields = new Dictionary<SiteCode, string>()
                    {
                        [SiteCode.ISBN] = "ISBN",
                        [SiteCode.RingPerfectAudioOffset] = "+0"
                    },
                    Contents = "Special contents 1\r\nSpecial contents 2",
                    ContentsSpecialFields = new Dictionary<SiteCode, string>()
                    {
                        [SiteCode.PlayableDemos] = "Game Demo 1",
                    },
                    Protection = "List of protections",
                    SectorRanges = "SSv1 Ranges",
                    SBI = "SecuROM data",
                    PVD = "0320 : 20 20 20 20 20 20 20 20  20 20 20 20 20 32 30 31                201\n0330 : 30 31 30 32 35 31 36 31  39 30 30 30 00 04 32 30   010251619000 .20\n0340 : 31 30 31 30 32 35 31 36  31 39 30 30 30 00 04 30   1010251619000 .0\n0350 : 30 30 30 30 30 30 30 30  30 30 30 30 30 30 30 00   000000000000000 \n0360 : 30 30 30 30 30 30 30 30  30 30 30 30 30 30 30 30   0000000000000000\n0370 : 00 01 00 00 00 00 00 00  00 00 00 00 00 00 00 00    .              ",
                    Header = "Header",
                    BCA = "BCA",
                    PIC = "10020000444901080000200042444F01\n1101010001000000004F947F00100000\n004F947E000000000000000000000000\n00000000000000000000000000000000\n00000000000000000000000000000000\n00000000000000000000000000000000\n00000000000000000000000000000000\n00000000000000000000000000000000\n00000000",
                    Cuesheet = "Cuesheet",
                    Dat = "Datfile",
                },

                SubmissionControls = new SubmissionControlsSection()
                {
                    DumpLog = "dump log",
                    LogsArchiveURL = "http://some.url",
                    ReviewComment = "Denied",
                    SubmissionComment = "This was a bootleg",
                    SubmitAs = "Dumper1",
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
        public void ToggleStartStop()
        {
            // Dump or stop the dump
            if ((StartStopButtonText as string) == StartDumpingValue)
            {
                StartDumping();
            }
            else if ((StartStopButtonText as string) == StopDumpingValue)
            {
                VerboseLogLn("Canceling dumping process...");
                _environment?.CancelDumping();
                CopyProtectScanButtonEnabled = true;
            }
        }

        /// <summary>
        /// Update the internal options from a closed OptionsWindow
        /// </summary>
        /// <param name="savedSettings">Indicates if the settings were saved or not</param>
        /// <param name="newOptions">Options representing the new, saved values</param>
        public void UpdateOptions(bool savedSettings, Options? newOptions)
        {
            // Get which options to save
            var optionsToSave = savedSettings ? newOptions : Options;

            // Ensure the first run flag is unset
            var continuingOptions = new Options(optionsToSave) { FirstRun = false };
            Options = new Options(continuingOptions);

            // If settings were changed, reinitialize the UI
            if (savedSettings)
                InitializeUIValues(removeEventHandlers: true, rebuildPrograms: true, rescanDrives: true);
        }

        #endregion

        #region UI Functionality

        /// <summary>
        /// Performs UI value setup end to end
        /// </summary>
        /// <param name="removeEventHandlers">Whether event handlers need to be removed first</param>
        /// <param name="rebuildPrograms">Whether the available program list should be rebuilt</param>
        /// <param name="rescanDrives">Whether drives should be rescanned or not</param>
        public void InitializeUIValues(bool removeEventHandlers, bool rebuildPrograms, bool rescanDrives)
        {
            // Disable the dumping button
            StartStopButtonEnabled = false;

            // Safely check the parameters box, just in case
            if (!ParametersCheckBoxEnabled)
            {
                bool cachedCanExecuteSelectionChanged = CanExecuteSelectionChanged;
                DisableEventHandlers();
                ParametersCheckBoxEnabled = true;
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
            PopulatePhysicalMediaType();
            CacheCurrentMediaType();
            SetCurrentMediaType();

            // Set the dumping program
            if (rebuildPrograms)
                PopulateInternalPrograms();

            // Set the initial environment and UI values
            SetSupportedDriveSpeed();
            _environment = DetermineEnvironment();
            GetOutputNames(true);
            EnsureMediaInformation();

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
            StartStopButtonEnabled = false;

            // Safely check the parameters box, just in case
            if (!ParametersCheckBoxEnabled)
            {
                bool cachedCanExecuteSelectionChanged = CanExecuteSelectionChanged;
                DisableEventHandlers();
                ParametersCheckBoxEnabled = true;
                if (cachedCanExecuteSelectionChanged) EnableEventHandlers();
            }

            // Remove event handlers to ensure ordering
            if (removeEventHandlers)
                DisableEventHandlers();

            // Refresh the drive info
            CurrentDrive?.RefreshDrive();

            // Set the initial environment and UI values
            _environment = DetermineEnvironment();
            GetOutputNames(true);
            EnsureMediaInformation();

            // Enable event handlers
            EnableEventHandlers();

            // Enable the dumping button, if necessary
            StartStopButtonEnabled = ShouldEnableDumpingButton();
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
            _logger?.Invoke(LogLevel.USER_GENERIC, text);
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
        /// Enqueue success text to the log
        /// </summary>
        /// <param name="text">Text to write to the log</param>
        private void SuccessLog(string text)
        {
            _logger?.Invoke(LogLevel.USER_SUCCESS, text);
        }

        /// <summary>
        /// Enqueue text with a newline to the log
        /// </summary>
        /// <param name="text">Text to write to the log</param>
        private void SuccessLogLn(string text) => SuccessLog(text + "\n");

        /// <summary>
        /// Enqueue verbose text to the log
        /// </summary>
        /// <param name="text">Text to write to the log</param>
        private void VerboseLog(string text)
        {
            if (_logger is not null && Options.VerboseLogging)
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
        private void CacheCurrentMediaType()
        {
            // If the selected item is invalid, we just skip
            if (CurrentDrive is null)
                return;

            // Get reasonable default values based on the current system
            var mediaTypes = CurrentSystem.MediaTypes();
            PhysicalMediaType? defaultPhysicalMediaType = mediaTypes.Count > 0 ? mediaTypes[0] : PhysicalMediaType.CDROM;
            if (defaultPhysicalMediaType == PhysicalMediaType.NONE)
                defaultPhysicalMediaType = PhysicalMediaType.CDROM;

            // If the drive is marked active, try to read from it
            if (CurrentDrive.MarkedActive)
            {
                VerboseLog($"Trying to detect media type for drive {CurrentDrive.Name} [{CurrentDrive.DriveFormat}] using size and filesystem.. ");
                PhysicalMediaType? detectedPhysicalMediaType = CurrentDrive.GetPhysicalMediaType(CurrentSystem);

                // If we got either an error or no media, default to the current System default
                if (detectedPhysicalMediaType is null)
                {
                    VerboseLogLn($"Could not detect media type, defaulting to {defaultPhysicalMediaType.LongName()}.");
                    CurrentPhysicalMediaType = defaultPhysicalMediaType;
                }
                else
                {
                    VerboseLogLn($"Detected {detectedPhysicalMediaType.LongName()}.");
                    _detectedPhysicalMediaType = detectedPhysicalMediaType;
                    CurrentPhysicalMediaType = detectedPhysicalMediaType;
                }
            }
            // Otherwise just use the default
            else
            {
                VerboseLogLn($"Drive marked as empty, defaulting to {defaultPhysicalMediaType.LongName()}.");
                CurrentPhysicalMediaType = defaultPhysicalMediaType;
            }
        }

        /// <summary>
        /// Create a DumpEnvironment with all current settings
        /// </summary>
        /// <returns>Filled DumpEnvironment Parent</returns>
        private DumpEnvironment DetermineEnvironment()
        {
            // Resolve the program paths temporarily
#pragma warning disable IDE0010 // Add missing cases
            switch (CurrentProgram)
            {
                case InternalProgram.Aaru:
                    string? aaruPath = FrontendTool.ResolveBinaryPath(Options.Dumping.AaruPath);
                    if (aaruPath != null)
                        Options.Dumping.AaruPath = aaruPath;

                    break;

                case InternalProgram.DiscImageCreator:
                    string? dicPath = FrontendTool.ResolveBinaryPath(Options.Dumping.DiscImageCreatorPath);
                    if (dicPath != null)
                        Options.Dumping.DiscImageCreatorPath = dicPath;

                    break;

                case InternalProgram.Redumper:
                    string? redumperPath = FrontendTool.ResolveBinaryPath(Options.Dumping.RedumperPath);
                    if (redumperPath != null)
                        Options.Dumping.RedumperPath = redumperPath;

                    break;
            }
#pragma warning restore IDE0010 // Add missing cases

            var env = new DumpEnvironment(
                Options,
                EvaluateOutputPath(OutputPath),
                CurrentDrive,
                CurrentSystem,
                CurrentProgram);
            env.SetExecutionContext(CurrentPhysicalMediaType, Parameters);
            env.SetProcessor();
            return env;
        }

        /// <summary>
        /// Determine and set the current system type, if allowed
        /// </summary>
        private void DetermineSystemType()
        {
            if (Drives is null || Drives.Count == 0 || CurrentDrive is null)
            {
                VerboseLogLn("Skipping system type detection because no valid drives found!");
            }
            else if (!Options.GUI.SkipSystemDetection)
            {
                VerboseLog($"Trying to detect system for drive {CurrentDrive.Name}.. ");
                var currentSystem = GetPhysicalSystem(CurrentDrive);
                if (currentSystem is not null)
                    VerboseLogLn($"detected {currentSystem.LongName()}.");

                // If undetected system on inactive drive, and PC is the default system, check for potential Mac disc
                if (currentSystem is null && !CurrentDrive.MarkedActive && Options.Dumping.DefaultSystem == PhysicalSystem.IBMPCcompatible)
                {
                    try
                    {
                        // If disc is readable on inactive drive, assume it is a Mac disc
                        if (PhysicalTool.GetFirstBytes(CurrentDrive, 1) is not null)
                        {
                            currentSystem = PhysicalSystem.AppleMacintosh;
                            VerboseLogLn($"unable to detect, defaulting to {currentSystem.LongName()}.");
                        }
                    }
                    catch { }
                }

                // Fallback to default system only if drive is active
                if (currentSystem is null && CurrentDrive.MarkedActive)
                {
                    currentSystem = Options.Dumping.DefaultSystem;
                    VerboseLogLn($"unable to detect, defaulting to {currentSystem.LongName()}.");
                }

                if (currentSystem is not null)
                {
                    int sysIndex = Systems.FindIndex(s => s == currentSystem);
                    CurrentSystem = Systems[sysIndex];
                }
            }
            else if (Options.GUI.SkipSystemDetection && Options.Dumping.DefaultSystem is not null)
            {
                var currentSystem = Options.Dumping.DefaultSystem;
                VerboseLogLn($"System detection disabled, defaulting to {currentSystem.LongName()}.");
                int sysIndex = Systems.FindIndex(s => s == currentSystem);
                CurrentSystem = Systems[sysIndex];
            }
        }

        /// <summary>
        /// Disable all UI elements during dumping
        /// </summary>
        public void DisableAllUIElements()
        {
            OptionsMenuItemEnabled = false;
            CheckDumpMenuItemEnabled = false;
            CreateIRDMenuItemEnabled = false;
            SystemTypeComboBoxEnabled = false;
            PhysicalMediaTypeComboBoxEnabled = false;
            OutputPathTextBoxEnabled = false;
            OutputPathBrowseButtonEnabled = false;
            DriveLetterComboBoxEnabled = false;
            DriveSpeedComboBoxEnabled = false;
            DumpingProgramComboBoxEnabled = false;
            EnableParametersCheckBoxEnabled = false;
            StartStopButtonText = StopDumpingValue;
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
            CheckDumpMenuItemEnabled = true;
            CreateIRDMenuItemEnabled = true;
            SystemTypeComboBoxEnabled = true;
            PhysicalMediaTypeComboBoxEnabled = true;
            OutputPathTextBoxEnabled = true;
            OutputPathBrowseButtonEnabled = true;
            DriveLetterComboBoxEnabled = true;
            DriveSpeedComboBoxEnabled = true;
            DumpingProgramComboBoxEnabled = true;
            EnableParametersCheckBoxEnabled = true;
            StartStopButtonText = StartDumpingValue;
            MediaScanButtonEnabled = true;
            UpdateVolumeLabelEnabled = true;
            CopyProtectScanButtonEnabled = true;
        }

        /// <summary>
        /// Ensure information is consistent with the currently selected media type
        /// </summary>
        public void EnsureMediaInformation()
        {
            // If the drive list is empty, ignore updates
            if (Drives.Count == 0)
                return;

            // Get the current environment information
            _environment = DetermineEnvironment();

            // Get the status to write out
            ResultEventArgs result = _environment.GetSupportStatus(CurrentPhysicalMediaType);
            if (CurrentProgram == InternalProgram.NONE)
                Status = "No dumping program found";
            else
                Status = result.Message;

            // Enable or disable the button
            StartStopButtonEnabled = result == true && ShouldEnableDumpingButton();

            // If we're in a type that doesn't support drive speeds
            DriveSpeedComboBoxEnabled = DumpEnvironment.DoesSupportDriveSpeed(CurrentPhysicalMediaType);

            // If input params are enabled, generate the full parameters from the environment
            if (ParametersCheckBoxEnabled)
            {
                var generated = _environment.GetFullParameters(CurrentPhysicalMediaType, DriveSpeed);
                if (generated is not null)
                    Parameters = generated;
            }
        }

        /// <summary>
        /// Replaces %-delimited variables inside a path string with their values
        /// </summary>
        /// <param name="outputPath">Path to be evaluated</param>
        /// <returns>String with %-delimited variables evaluated</returns>
        public string EvaluateOutputPath(string outputPath)
        {
            string systemLong = _currentSystem.LongName() ?? "Unknown System";
            if (string.IsNullOrEmpty(systemLong))
                systemLong = "Unknown System";
            string systemShort = _currentSystem.ShortName() ?? "unknown";
            if (string.IsNullOrEmpty(systemShort))
                systemShort = "unknown";
            string mediaLong = _currentPhysicalMediaType.LongName() ?? "Unknown Media";
            if (string.IsNullOrEmpty(mediaLong))
                mediaLong = "Unknown Media";
            string program = _currentProgram.ToString() ?? "Unknown Program";
            if (string.IsNullOrEmpty(program))
                program = "Unknown Program";
            string programShort = program == "DiscImageCreator" ? "DIC" : program;
            if (string.IsNullOrEmpty(programShort))
                programShort = "Unknown Program";
            string label = GetFormattedVolumeLabel(_currentDrive) ?? $"track_{DateTime.Now:yyyyMMdd-HHmm}";
            if (string.IsNullOrEmpty(label))
                label = $"track_{DateTime.Now:yyyyMMdd-HHmm}";
            string date = DateTime.Today.ToString("yyyyMMdd");
            if (string.IsNullOrEmpty(date))
                date = "UNKNOWN";
            string datetime = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            if (string.IsNullOrEmpty(datetime))
                datetime = "UNKNOWN";

            return outputPath
                .Replace("%SYSTEM%", systemLong)
                .Replace("%SYS%", systemShort.ToUpperInvariant())
                .Replace("%sys%", systemShort.ToLowerInvariant())
                .Replace("%MEDIA%", mediaLong)
                .Replace("%PROGRAM%", program)
                .Replace("%PROG%", programShort)
                .Replace("%LABEL%", label)
                .Replace("%DATE%", date)
                .Replace("%DATETIME%", datetime);
        }

        /// <summary>
        /// Get the default output directory name from the currently selected drive
        /// </summary>
        /// <param name="driveChanged">Force an updated name if the drive letter changes</param>
        public void GetOutputNames(bool driveChanged)
        {
            if (Drives is null || Drives.Count == 0 || CurrentDrive is null)
            {
                VerboseLogLn("Skipping output name building because no valid drives found!");
                return;
            }

            // Get path pieces that are used in all branches
            string defaultOutputPath = Options.Dumping.DefaultOutputPath ?? "ISO";
            string extension = _environment?.GetDefaultExtension(CurrentPhysicalMediaType) ?? ".bin";
            string label = GetFormattedVolumeLabel(CurrentDrive) ?? CurrentSystem.LongName() ?? $"track_{DateTime.Now:yyyyMMdd-HHmm}";
            string defaultFilename = $"{label}{extension}";

            // If no path exists, set one using default values
            if (string.IsNullOrEmpty(OutputPath))
            {
#if NET20 || NET35
                OutputPath = Path.Combine(Path.Combine(defaultOutputPath, label), defaultFilename);
#else
                OutputPath = Path.Combine(defaultOutputPath, label, defaultFilename);
#endif
                return;
            }

            // For all other cases, separate the last path
            string lastPath = IOExtensions.NormalizeFilePath(OutputPath, fullPath: false);
            string lastDirectory = Path.GetDirectoryName(lastPath) ?? string.Empty;
            string lastFilename = Path.GetFileNameWithoutExtension(lastPath);

            // Set the output filename, if we changed drives
            if (driveChanged)
            {
                // If the previous path is exactly the default path and last filename
                if (lastDirectory.EndsWith(Path.Combine(defaultOutputPath, lastFilename)))
                    lastDirectory = Path.GetDirectoryName(lastDirectory) ?? string.Empty;

                // Create the output path
                if (lastDirectory == defaultOutputPath)
#if NET20 || NET35
                    OutputPath = Path.Combine(Path.Combine(lastDirectory, label), defaultFilename);
#else
                    OutputPath = Path.Combine(lastDirectory, label, defaultFilename);
#endif
                else
                    OutputPath = Path.Combine(lastDirectory, defaultFilename);
            }

            // Otherwise, reset the extension of the currently set path
            else
            {
                lastFilename = $"{lastFilename}{extension}";
                OutputPath = Path.Combine(lastDirectory, lastFilename);
            }
        }

        /// <summary>
        /// Get the current system from drive
        /// </summary>
        private static PhysicalSystem? GetPhysicalSystem(Drive? drive)
        {
            // If the drive does not exist, we can't do anything
            if (drive is null || string.IsNullOrEmpty(drive.Name))
                return null;

            // If we can't read the files in the drive, we can only perform physical checks
            if (!drive.MarkedActive || !Directory.Exists(drive.Name))
            {
                try
                {
                    // Check for Panasonic 3DO - filesystem not readable on Windows
                    PhysicalSystem? detected3DOSystem = PhysicalTool.Detect3DOSystem(drive);
                    if (detected3DOSystem is not null)
                    {
                        return detected3DOSystem;
                    }

                    // Sega Saturn / Sega Dreamcast / Sega Mega-CD / Sega-CD
                    PhysicalSystem? detectedSegaSystem = PhysicalTool.DetectSegaSystem(drive);
                    if (detectedSegaSystem is not null)
                    {
                        return detectedSegaSystem;
                    }
                }
                catch { }

                // Otherwise, return null
                return null;
            }

            // Floppies, HDDs, and removable drives are assumed
            if (drive.InternalDriveType != InternalDriveType.Optical)
                return PhysicalSystem.IBMPCcompatible;

            // Check volume labels first
            PhysicalSystem? systemFromLabel = FrontendTool.GetPhysicalSystemFromVolumeLabel(drive.VolumeLabel);
            if (systemFromLabel is not null)
                return systemFromLabel;

            // Get a list of files for quicker checking
            #region Arcade

            // Funworld Photo Play
            if (File.Exists(Path.Combine(drive.Name, "PP.INF"))
                && Directory.Exists(Path.Combine(drive.Name, "PPINC")))
            {
                return PhysicalSystem.FunworldPhotoPlay;
            }

            // Konami Python 2
            if (Directory.Exists(Path.Combine(drive.Name, "PY2.D")))
            {
                return PhysicalSystem.KonamiPython2;
            }

            #endregion

            #region Consoles

            // Apple/Bandai Pippin
            if (File.Exists(Path.Combine(drive.Name, "PippinAuthenticationFile")))
            {
                return PhysicalSystem.AppleBandaiPippin;
            }

            // Bandai Playdia Quick Interactive System
            try
            {
                List<string> files = [.. Directory.GetFiles(drive.Name, "*", SearchOption.TopDirectoryOnly)];

                if (files.Exists(f => f.EndsWith(".AJS", StringComparison.OrdinalIgnoreCase))
                    && files.Exists(f => f.EndsWith(".GLB", StringComparison.OrdinalIgnoreCase)))
                {
                    return PhysicalSystem.BandaiPlaydiaQuickInteractiveSystem;
                }
            }
            catch { }

            // Commodore CDTV/CD32
#if NET20 || NET35
            if (File.Exists(Path.Combine(Path.Combine(drive.Name, "S"), "STARTUP-SEQUENCE")))
#else
            if (File.Exists(Path.Combine(drive.Name, "S", "STARTUP-SEQUENCE")))
#endif
            {
                if (File.Exists(Path.Combine(drive.Name, "CDTV.TM")))
                    return PhysicalSystem.CommodoreAmigaCDTV;
                else
                    return PhysicalSystem.CommodoreAmigaCD32;
            }

            // Mattel HyperScan -- TODO: May need case-insensitivity added
            if (File.Exists(Path.Combine(drive.Name, "hyper.exe")))
            {
                return PhysicalSystem.MattelHyperScan;
            }

            // Mattel Fisher-Price iXL
#if NET20 || NET35
            if (File.Exists(Path.Combine(Path.Combine(drive.Name, "iXL"), "iXLUpdater.exe")))
#else
            if (File.Exists(Path.Combine(drive.Name, "iXL", "iXLUpdater.exe")))
#endif
            {
                return PhysicalSystem.MattelFisherPriceiXL;
            }

            // Memorex - Visual Information System
            if (File.Exists(Path.Combine(drive.Name, "CONTROL.TAT")))
            {
                return PhysicalSystem.MemorexVisualInformationSystem;
            }

            // Microsoft Xbox 360
            try
            {
                if (Directory.Exists(Path.Combine(drive.Name, "$SystemUpdate"))
                    && Path.Combine(drive.Name, "$SystemUpdate").SafeGetFiles().Length > 0
                    && drive.TotalSize <= 500_000_000)
                {
                    return PhysicalSystem.MicrosoftXbox360;
                }
            }
            catch { }

            // Microsoft Xbox One and Series X
            try
            {
                if (Directory.Exists(Path.Combine(drive.Name, "MSXC")))
                {
                    try
                    {
#if NET20 || NET35
                        string catalogjs = Path.Combine(drive.Name, Path.Combine("MSXC", Path.Combine("Metadata", "catalog.js")));
#else
                        string catalogjs = Path.Combine(drive.Name, "MSXC", "Metadata", "catalog.js");
#endif
                        if (!File.Exists(catalogjs))
                            return PhysicalSystem.MicrosoftXboxOne;

                        var catalog = new SabreTools.Serialization.Readers.Catalog().Deserialize(catalogjs);
                        if (catalog is not null && catalog.Version is not null && catalog.Packages is not null)
                        {
                            if (!double.TryParse(catalog.Version, out double version))
                                return PhysicalSystem.MicrosoftXboxOne;

                            if (version < 4)
                                return PhysicalSystem.MicrosoftXboxOne;

                            foreach (var package in catalog.Packages)
                            {
                                if (package.Generation != "9")
                                    return PhysicalSystem.MicrosoftXboxOne;
                            }

                            return PhysicalSystem.MicrosoftXboxSeriesXS;
                        }
                    }
                    catch
                    {
                        return PhysicalSystem.MicrosoftXboxOne;
                    }
                }
            }
            catch { }

            // Playmaji Polymega
            if (File.Exists(Path.Combine(drive.Name, "Get Polymega App.url")))
            {
                return PhysicalSystem.PlaymajiPolymega;
            }

            try
            {
                // Sega Saturn / Sega Dreamcast / Sega Mega-CD / Sega-CD
                PhysicalSystem? segaSystem = PhysicalTool.DetectSegaSystem(drive);
                if (segaSystem is not null)
                {
                    return segaSystem;
                }
            }
            catch { }

            // Sega Dreamcast
            if (File.Exists(Path.Combine(drive.Name, "IP.BIN")))
            {
                return PhysicalSystem.SegaDreamcast;
            }

            // Sega Mega-CD / Sega-CD
#if NET20 || NET35
            if (File.Exists(Path.Combine(Path.Combine(drive.Name, "_BOOT"), "IP.BIN"))
                || File.Exists(Path.Combine(Path.Combine(drive.Name, "_BOOT"), "SP.BIN"))
                || File.Exists(Path.Combine(Path.Combine(drive.Name, "_BOOT"), "SP_AS.BIN"))
                || File.Exists(Path.Combine(drive.Name, "FILESYSTEM.BIN")))
#else
            if (File.Exists(Path.Combine(drive.Name, "_BOOT", "IP.BIN"))
                || File.Exists(Path.Combine(drive.Name, "_BOOT", "SP.BIN"))
                || File.Exists(Path.Combine(drive.Name, "_BOOT", "SP_AS.BIN"))
                || File.Exists(Path.Combine(drive.Name, "FILESYSTEM.BIN")))
#endif
            {
                return PhysicalSystem.SegaMegaCDSegaCD;
            }

            // SNK Neo-Geo CD
            try
            {
                if (File.Exists(Path.Combine(drive.Name, "ABS.TXT"))
                    || File.Exists(Path.Combine(drive.Name, "BIB.TXT"))
                    || File.Exists(Path.Combine(drive.Name, "CPY.TXT"))
                    || File.Exists(Path.Combine(drive.Name, "IPL.TXT")))
                {
                    return PhysicalSystem.SNKNeoGeoCD;
                }
            }
            catch { }

            // Sony PlayStation and Sony PlayStation 2
            string psxExePath = Path.Combine(drive.Name, "PSX.EXE");
            string systemCnfPath = Path.Combine(drive.Name, "SYSTEM.CNF");
            if (File.Exists(systemCnfPath))
            {
                // Check for either BOOT or BOOT2
                var systemCnf = new IniFile(systemCnfPath);
                if (systemCnf.ContainsKey("BOOT"))
                    return PhysicalSystem.SonyPlayStation;
                else if (systemCnf.ContainsKey("BOOT2"))
                    return PhysicalSystem.SonyPlayStation2;
            }
            else if (File.Exists(psxExePath))
            {
                return PhysicalSystem.SonyPlayStation;
            }

            // Sony PlayStation 3
            try
            {
                if (Directory.Exists(Path.Combine(drive.Name, "PS3_GAME"))
                    || Directory.Exists(Path.Combine(drive.Name, "PS3_UPDATE"))
                    || File.Exists(Path.Combine(drive.Name, "PS3_DISC.SFB")))
                {
                    return PhysicalSystem.SonyPlayStation3;
                }
            }
            catch { }

            // Sony PlayStation 4
#if NET20 || NET35
            if (File.Exists(Path.Combine(Path.Combine(Path.Combine(drive.Name, "PS4"), "UPDATE"), "PS4UPDATE.PUP")))
#else
            if (File.Exists(Path.Combine(drive.Name, "PS4", "UPDATE", "PS4UPDATE.PUP")))
#endif
            {
                return PhysicalSystem.SonyPlayStation4;
            }

            // Sony PlayStation 5
#if NET20 || NET35
            if (File.Exists(Path.Combine(Path.Combine(Path.Combine(drive.Name, "PS5"), "UPDATE"), "PS5UPDATE.PUP")))
#else
            if (File.Exists(Path.Combine(drive.Name, "PS5", "UPDATE", "PS5UPDATE.PUP")))
#endif
            {
                return PhysicalSystem.SonyPlayStation5;
            }

            // V.Tech V.Flash / V.Smile Pro
            if (File.Exists(Path.Combine(drive.Name, "0SYSTEM")))
            {
                return PhysicalSystem.VTechVFlashVSmilePro;
            }

            // VM Labs NUON
#if NET20 || NET35
            if (File.Exists(Path.Combine(Path.Combine(drive.Name, "NUON"), "nuon.run")))
#else
            if (File.Exists(Path.Combine(drive.Name, "NUON", "nuon.run")))
#endif
            {
                return PhysicalSystem.VMLabsNUON;
            }

            // ZAPit Games - GameWave
            if (File.Exists(Path.Combine(drive.Name, "gamewave.diz")))
            {
                return PhysicalSystem.ZAPiTGamesGameWaveFamilyEntertainmentSystem;
            }

            #endregion

            #region Computers

            // Amiga CD (Do this check AFTER CD32/CDTV)
            if (File.Exists(Path.Combine(drive.Name, "Disk.info")))
            {
                return PhysicalSystem.CommodoreAmigaCD;
            }

            // Fujitsu FM Towns
            try
            {
                if (File.Exists(Path.Combine(drive.Name, "TMENU.EXP"))
                    || File.Exists(Path.Combine(drive.Name, "TBIOS.SYS"))
                    || File.Exists(Path.Combine(drive.Name, "TBIOS.BIN")))
                {
                    return PhysicalSystem.FujitsuFMTownsSeries;
                }
            }
            catch { }

            // Sharp X68000
            if (File.Exists(Path.Combine(drive.Name, "COMMAND.X")))
            {
                return PhysicalSystem.SharpX68000;
            }

            #endregion

            #region Video Formats

            // BD-Video
            if (Directory.Exists(Path.Combine(drive.Name, "BDMV")))
            {
                // Technically BD-Audio has this as well, but it's hard to split that out right now
                return PhysicalSystem.BDVideo;
            }

            // DVD-Audio and DVD-Video
            try
            {
                if (Directory.Exists(Path.Combine(drive.Name, "AUDIO_TS"))
                    && Path.Combine(drive.Name, "AUDIO_TS").SafeGetFiles().Length > 0)
                {
                    return PhysicalSystem.DVDAudio;
                }

                else if (Directory.Exists(Path.Combine(drive.Name, "VIDEO_TS"))
                    && Path.Combine(drive.Name, "VIDEO_TS").SafeGetFiles().Length > 0)
                {
                    return PhysicalSystem.DVDVideo;
                }
            }
            catch { }

            // HD-DVD-Video
            try
            {
                if (Directory.Exists(Path.Combine(drive.Name, "HVDVD_TS"))
                    && Path.Combine(drive.Name, "HVDVD_TS").SafeGetFiles().Length > 0)
                {
                    return PhysicalSystem.HDDVDVideo;
                }
            }
            catch { }

            // Photo CD
            try
            {
                if (Directory.Exists(Path.Combine(drive.Name, "PHOTO_CD"))
                    && Path.Combine(drive.Name, "PHOTO_CD").SafeGetFiles().Length > 0)
                {
                    return PhysicalSystem.PhotoCD;
                }
            }
            catch { }

            // VCD
            try
            {
                if (Directory.Exists(Path.Combine(drive.Name, "VCD"))
                    && Path.Combine(drive.Name, "VCD").SafeGetFiles().Length > 0)
                {
                    return PhysicalSystem.VideoCD;
                }
            }
            catch { }

            #endregion

            // Default return
            return null;
        }

        /// <summary>
        /// Logs the About text
        /// </summary>
        public void LogAboutText(string message)
        {
            SecretLogLn(message);
        }

        /// <summary>
        /// Process the current custom parameters back into UI values
        /// </summary>
        public void ProcessCustomParameters()
        {
            // Set the execution context and processor
            if (_environment?.SetExecutionContext(CurrentPhysicalMediaType, Parameters) != true)
                return;
            if (_environment?.SetProcessor() != true)
                return;

            // Catch this in case there's an input path issue
            try
            {
                int driveIndex = Drives.ConvertAll(d => d.Name?[0] ?? '\0')
                    .IndexOf(_environment.ContextInputPath?[0] ?? default);
                CurrentDrive = driveIndex != -1 ? Drives[driveIndex] : Drives[0];
            }
            catch { }

            int driveSpeed = _environment.Speed ?? -1;
            if (driveSpeed > 0)
                DriveSpeed = driveSpeed;
            else
                _environment.Speed = DriveSpeed;

            // Disable change handling
            DisableEventHandlers();

            OutputPath = IOExtensions.NormalizeFilePath(_environment.ContextOutputPath, fullPath: false);

            if (MediaTypes is not null)
            {
                PhysicalMediaType? mediaType = _environment.GetPhysicalMediaType();
                if (mediaType is not null)
                {
                    int mediaTypeIndex = MediaTypes.FindIndex(m => m == mediaType);
                    CurrentPhysicalMediaType = mediaTypeIndex > -1 ? MediaTypes[mediaTypeIndex] : MediaTypes[0];
                }
            }

            // Reenable change handling
            EnableEventHandlers();
        }

        /// <summary>
        /// Scan and show copy protection for the current disc
        /// </summary>
        public async Task<string?> ScanAndShowProtection()
        {
            // Determine current environment, just in case
            _environment ??= DetermineEnvironment();

            // If we don't have a valid drive
            if (CurrentDrive?.Name is null)
            {
                ErrorLogLn("No valid drive found!");
                return null;
            }

            VerboseLogLn($"Scanning for copy protection in {CurrentDrive.Name}");

            var tempContent = Status;
            Status = "Scanning for copy protection... this might take a while!";

            // Disable UI elements
            OptionsMenuItemEnabled = false;

            SystemTypeComboBoxEnabled = false;
            PhysicalMediaTypeComboBoxEnabled = false;

            OutputPathTextBoxEnabled = false;
            OutputPathBrowseButtonEnabled = false;
            DriveLetterComboBoxEnabled = false;
            DriveSpeedComboBoxEnabled = false;
            DumpingProgramComboBoxEnabled = false;
            EnableParametersCheckBoxEnabled = false;

            StartStopButtonEnabled = false;
            MediaScanButtonEnabled = false;
            UpdateVolumeLabelEnabled = false;
            CopyProtectScanButtonEnabled = false;

            var progress = new Progress<ProtectionProgress>();
            progress.ProgressChanged += ProgressUpdated;

            try
            {
                var protections = await ProtectionTool.RunProtectionScanOnPath(CurrentDrive.Name, Options, progress);
                var output = ProtectionTool.FormatProtections(protections, CurrentDrive);

                LogLn($"Detected the following protections in {CurrentDrive.Name}:\r\n\r\n{output}");
                return output;
            }
            catch (Exception ex)
            {
                ErrorLogLn($"Path could not be scanned! Exception information:\r\n\r\n{ex}");
                return null;
            }
            finally
            {
                // Reset the status
                Status = tempContent;

                // Enable UI elements
                OptionsMenuItemEnabled = true;

                SystemTypeComboBoxEnabled = true;
                PhysicalMediaTypeComboBoxEnabled = true;

                OutputPathTextBoxEnabled = true;
                OutputPathBrowseButtonEnabled = true;
                DriveLetterComboBoxEnabled = true;
                DriveSpeedComboBoxEnabled = true;
                DumpingProgramComboBoxEnabled = true;
                EnableParametersCheckBoxEnabled = true;

                StartStopButtonEnabled = ShouldEnableDumpingButton();
                MediaScanButtonEnabled = true;
                UpdateVolumeLabelEnabled = true;
                CopyProtectScanButtonEnabled = true;
            }
        }

        /// <summary>
        /// Media label as read by Windows, formatted to avoid odd outputs
        /// If no volume label present, use PSX or PS2 serial if valid
        /// Otherwise, use "track" with current datetime as volume label
        /// </summary>
        private static string? GetFormattedVolumeLabel(Drive? drive)
        {
            // If the drive is invalid
            if (drive is null)
                return null;

            // If the drive is marked as inactive
            if (!drive.MarkedActive)
                return DiscNotDetectedValue;

            // Use internal serials where appropriate
            string? volumeLabel = string.IsNullOrEmpty(drive.VolumeLabel) ? null : drive.VolumeLabel!.Trim();
#pragma warning disable IDE0010
            switch (GetPhysicalSystem(drive))
            {
                case PhysicalSystem.SonyPlayStation:
                case PhysicalSystem.SonyPlayStation2:
                    string? ps12Serial = PhysicalTool.GetPlayStationSerial(drive);
                    volumeLabel ??= ps12Serial ?? $"track_{DateTime.Now:yyyyMMdd-HHmm}";
                    break;

                case PhysicalSystem.SonyPlayStation3:
                    string? ps3Serial = PhysicalTool.GetPlayStation3Serial(drive);
                    if (volumeLabel == "PS3VOLUME")
                        volumeLabel = ps3Serial ?? volumeLabel;
                    else
                        volumeLabel ??= ps3Serial ?? $"track_{DateTime.Now:yyyyMMdd-HHmm}";
                    break;

                case PhysicalSystem.SonyPlayStation4:
                    string? ps4Serial = PhysicalTool.GetPlayStation4Serial(drive);
                    if (volumeLabel == "PS4VOLUME")
                        volumeLabel = ps4Serial ?? volumeLabel;
                    else
                        volumeLabel ??= ps4Serial ?? $"track_{DateTime.Now:yyyyMMdd-HHmm}";
                    break;

                case PhysicalSystem.SonyPlayStation5:
                    string? ps5Serial = PhysicalTool.GetPlayStation5Serial(drive);
                    if (volumeLabel == "PS5VOLUME")
                        volumeLabel = ps5Serial ?? volumeLabel;
                    else
                        volumeLabel ??= ps5Serial ?? $"track_{DateTime.Now:yyyyMMdd-HHmm}";
                    break;

                default:
                    volumeLabel ??= $"track_{DateTime.Now:yyyyMMdd-HHmm}";
                    break;
            }
#pragma warning restore IDE0010

            foreach (char c in Path.GetInvalidFileNameChars())
            {
                volumeLabel = volumeLabel?.Replace(c, '_');
            }

            return volumeLabel;
        }

        /// <summary>
        /// Set the current disc type in the combo box
        /// </summary>
        private void SetCurrentMediaType()
        {
            // If we don't have any selected media types, we don't care and return
            if (MediaTypes is null)
                return;

            // If we have a detected media type, use that first
            if (_detectedPhysicalMediaType is not null)
            {
                int detectedIndex = MediaTypes.FindIndex(kvp => kvp.Value == _detectedPhysicalMediaType);
                if (detectedIndex > -1)
                {
                    CurrentPhysicalMediaType = _detectedPhysicalMediaType;
                    return;
                }
            }

            // If we have an invalid current type, we don't care and return
            if (CurrentPhysicalMediaType is null || CurrentPhysicalMediaType == PhysicalMediaType.NONE)
                return;

            // Now set the selected item, if possible
            int index = MediaTypes.FindIndex(kvp => kvp.Value == CurrentPhysicalMediaType);
            if (CurrentPhysicalMediaType is not null && index == -1)
                VerboseLogLn($"Disc of type '{CurrentPhysicalMediaType.LongName()}' found, but the current system does not support it!");

            CurrentPhysicalMediaType = index > -1 ? MediaTypes[index] : MediaTypes[0];
        }

        /// <summary>
        /// Set the drive speed based on reported maximum and user-defined option
        /// </summary>
        public void SetSupportedDriveSpeed()
        {
            // Skip trying to set speeds if no drives
            if (Drives.Count == 0)
                return;

            // Set the drive speed list that's appropriate
            DriveSpeeds = InterfaceConstants.GetSpeedsForPhysicalMediaType(CurrentPhysicalMediaType);
            VerboseLogLn($"Supported media speeds: {string.Join(", ", [.. DriveSpeeds.ConvertAll(ds => ds.ToString())])}");

            // Set the selected speed
            DriveSpeed = FrontendTool.GetDefaultSpeedForPhysicalMediaType(CurrentPhysicalMediaType, Options);
        }

        /// <summary>
        /// Determine if the dumping button should be enabled
        /// </summary>
        private bool ShouldEnableDumpingButton()
        {
            return Drives.Count > 0
                && CurrentSystem is not null
                && CurrentPhysicalMediaType is not null
                && ProgramSupportsMedia();
        }

        /// <summary>
        /// Returns false if a given InternalProgram does not support a given PhysicalMediaType
        /// </summary>
        private bool ProgramSupportsMedia()
        {
            // If the media type is not set, return false
            if (CurrentPhysicalMediaType is null || CurrentPhysicalMediaType == PhysicalMediaType.NONE)
                return false;

#pragma warning disable IDE0072
            return CurrentProgram switch
            {
                // Aaru
                InternalProgram.Aaru when CurrentPhysicalMediaType == PhysicalMediaType.BluRay => true,
                InternalProgram.Aaru when CurrentPhysicalMediaType == PhysicalMediaType.CDROM => true,
                InternalProgram.Aaru when CurrentPhysicalMediaType == PhysicalMediaType.CompactFlash => true,
                InternalProgram.Aaru when CurrentPhysicalMediaType == PhysicalMediaType.DVD => true,
                InternalProgram.Aaru when CurrentPhysicalMediaType == PhysicalMediaType.GDROM => true,
                InternalProgram.Aaru when CurrentPhysicalMediaType == PhysicalMediaType.FlashDrive => true,
                InternalProgram.Aaru when CurrentPhysicalMediaType == PhysicalMediaType.FloppyDisk => true,
                InternalProgram.Aaru when CurrentPhysicalMediaType == PhysicalMediaType.HardDisk => true,
                InternalProgram.Aaru when CurrentPhysicalMediaType == PhysicalMediaType.HDDVD => true,
                InternalProgram.Aaru when CurrentPhysicalMediaType == PhysicalMediaType.NintendoGameCubeGameDisc => true,
                InternalProgram.Aaru when CurrentPhysicalMediaType == PhysicalMediaType.NintendoWiiOpticalDisc => true,
                InternalProgram.Aaru when CurrentPhysicalMediaType == PhysicalMediaType.NintendoWiiUOpticalDisc => true,
                InternalProgram.Aaru when CurrentPhysicalMediaType == PhysicalMediaType.SDCard => true,

                // DiscImageCreator
                InternalProgram.DiscImageCreator when CurrentPhysicalMediaType == PhysicalMediaType.BluRay => true,
                InternalProgram.DiscImageCreator when CurrentPhysicalMediaType == PhysicalMediaType.CDROM => true,
                InternalProgram.DiscImageCreator when CurrentPhysicalMediaType == PhysicalMediaType.CompactFlash => true,
                InternalProgram.DiscImageCreator when CurrentPhysicalMediaType == PhysicalMediaType.DVD => true,
                InternalProgram.DiscImageCreator when CurrentPhysicalMediaType == PhysicalMediaType.GDROM => true,
                InternalProgram.DiscImageCreator when CurrentPhysicalMediaType == PhysicalMediaType.FlashDrive => true,
                InternalProgram.DiscImageCreator when CurrentPhysicalMediaType == PhysicalMediaType.FloppyDisk => true,
                InternalProgram.DiscImageCreator when CurrentPhysicalMediaType == PhysicalMediaType.HardDisk => true,
                InternalProgram.DiscImageCreator when CurrentPhysicalMediaType == PhysicalMediaType.HDDVD => true,
                InternalProgram.DiscImageCreator when CurrentPhysicalMediaType == PhysicalMediaType.NintendoGameCubeGameDisc => true,
                InternalProgram.DiscImageCreator when CurrentPhysicalMediaType == PhysicalMediaType.NintendoWiiOpticalDisc => true,
                InternalProgram.DiscImageCreator when CurrentPhysicalMediaType == PhysicalMediaType.NintendoWiiUOpticalDisc => true,
                InternalProgram.DiscImageCreator when CurrentPhysicalMediaType == PhysicalMediaType.SDCard => true,

                // Dreamdump
                // InternalProgram.Dreamdump when CurrentPhysicalMediaType == PhysicalMediaType.GDROM => true,

                // Redumper
                InternalProgram.Redumper when CurrentPhysicalMediaType == PhysicalMediaType.BluRay => true,
                InternalProgram.Redumper when CurrentPhysicalMediaType == PhysicalMediaType.CDROM => true,
                InternalProgram.Redumper when CurrentPhysicalMediaType == PhysicalMediaType.DVD => true,
                InternalProgram.Redumper when CurrentPhysicalMediaType == PhysicalMediaType.GDROM => true,
                InternalProgram.Redumper when CurrentPhysicalMediaType == PhysicalMediaType.HDDVD => true,
                InternalProgram.Redumper when CurrentPhysicalMediaType == PhysicalMediaType.NintendoGameCubeGameDisc => true,
                InternalProgram.Redumper when CurrentPhysicalMediaType == PhysicalMediaType.NintendoWiiOpticalDisc => true,
                InternalProgram.Redumper when CurrentPhysicalMediaType == PhysicalMediaType.NintendoWiiUOpticalDisc => true,

                // Default
                _ => false,
            };
#pragma warning restore IDE0072
        }

        /// <summary>
        /// Begin the dumping process using the given inputs
        /// </summary>
        public async void StartDumping()
        {
            // Ask user to confirm before exiting application during a dump
            AskBeforeQuit = true;

            // One last check to determine environment, just in case
            _environment = DetermineEnvironment();

            // Force an internal drive refresh in case the user entered things manually
            _environment.RefreshDrive();

            try
            {
                // Run pre-dumping validation checks
                if (!ValidateBeforeDumping())
                {
                    // Re-allow quick exiting
                    AskBeforeQuit = false;
                    return;
                }

                // Disable all UI elements apart from dumping button
                DisableAllUIElements();

                // Refresh the drive, if it wasn't null
                _environment.RefreshDrive();

                // Output to the label and log
                Status = "Starting dumping process... please wait!";
                LogLn("Starting dumping process... please wait!");
                LogLn("Look for the separate command window for more details");

                // Get progress indicators
                var resultProgress = new Progress<ResultEventArgs>();
                resultProgress.ProgressChanged += ProgressUpdated;
                var protectionProgress = new Progress<ProtectionProgress>();
                protectionProgress.ProgressChanged += ProgressUpdated;
                _environment.ReportStatus += ProgressUpdated;

                // Run the program with the parameters
                ResultEventArgs result = await _environment.Run(CurrentPhysicalMediaType, resultProgress);

                // If we didn't execute a dumping command we cannot get submission output
                if (!_environment.IsDumpingCommand())
                {
                    SuccessLogLn("No dumping command was run, submission information will not be gathered.");
                    Status = "Execution complete!";

                    // Re-allow quick exiting
                    AskBeforeQuit = false;

                    // Reset all UI elements
                    EnableAllUIElements();
                    return;
                }

                // Verify dump output and save it
                if (result == true)
                {
                    result = await _environment.VerifyAndSaveDumpOutput(
                        resultProgress: resultProgress,
                        protectionProgress: protectionProgress,
                        processUserInfo: _processUserInfo);

                    if (result == false)
                        ErrorLogLn(result.Message);
                }
                else
                {
                    ErrorLogLn(result.Message);
                    Status = "Execution failed!";
                }
            }
            catch (Exception ex)
            {
                ErrorLogLn(ex.ToString());
                Status = "An exception occurred!";
            }
            finally
            {
                // Re-allow quick exiting
                AskBeforeQuit = false;

                // Reset all UI elements
                EnableAllUIElements();
            }
        }

        /// <summary>
        /// Toggle the parameters input box
        /// </summary>
        public void ToggleParameters()
        {
            if (!ParametersCheckBoxEnabled)
            {
                OptionsMenuItemEnabled = false;

                SystemTypeComboBoxEnabled = false;
                PhysicalMediaTypeComboBoxEnabled = false;

                OutputPathTextBoxEnabled = false;
                OutputPathBrowseButtonEnabled = false;
                DriveLetterComboBoxEnabled = false;
                DriveSpeedComboBoxEnabled = false;
                DumpingProgramComboBoxEnabled = false;
                ParametersTextBoxEnabled = true;

                MediaScanButtonEnabled = false;
                UpdateVolumeLabelEnabled = false;
                CopyProtectScanButtonEnabled = false;
                StartStopButtonEnabled = false;
            }
            else
            {
                ProcessCustomParameters();

                OptionsMenuItemEnabled = true;

                SystemTypeComboBoxEnabled = true;
                PhysicalMediaTypeComboBoxEnabled = true;

                OutputPathTextBoxEnabled = true;
                OutputPathBrowseButtonEnabled = true;
                DriveLetterComboBoxEnabled = true;
                DriveSpeedComboBoxEnabled = true;
                DumpingProgramComboBoxEnabled = true;
                ParametersTextBoxEnabled = false;

                MediaScanButtonEnabled = true;
                UpdateVolumeLabelEnabled = true;
                CopyProtectScanButtonEnabled = true;
                StartStopButtonEnabled = true;
            }
        }

        /// <summary>
        /// Perform validation, including user input, before attempting to start dumping
        /// </summary>
        /// <returns>True if dumping should start, false otherwise</returns>
        private bool ValidateBeforeDumping()
        {
            if (Parameters is null || _environment is null)
                return false;

            // Validate that we have an output path of any sort
            if (string.IsNullOrEmpty(_environment.OutputPath))
            {
                if (_displayUserMessage is not null)
                    _ = _displayUserMessage("Missing Path", "No output path was provided so dumping cannot continue.", 1, false);
                LogLn("Dumping aborted!");
                return false;
            }

            // Validate that the user explicitly wants an inactive drive to be considered for dumping
            if (!_environment.DriveMarkedActive && _displayUserMessage is not null)
            {
                string message = "The currently selected drive does not appear to contain a disc! "
                    + (!_environment!.DetectedByWindows() ? $"This is normal for {_environment.SystemName} as the discs may not be readable on Windows. " : string.Empty)
                    + "Do you want to continue?";

                bool? mbresult = _displayUserMessage("No Disc Detected", message, 2, false);
                if (mbresult != true)
                {
                    LogLn("Dumping aborted!");
                    return false;
                }
            }

            // Pre-split the output path
            var outputDirectory = Path.GetDirectoryName(_environment!.OutputPath);
            string outputFilename = Path.GetFileName(_environment.OutputPath);

            // If a complete or partial dump already exists
            bool foundAllFiles = _environment.FoundAllFiles(CurrentPhysicalMediaType, outputDirectory, outputFilename);
            if (foundAllFiles && _displayUserMessage is not null)
            {
                bool? mbresult = _displayUserMessage("Overwrite?", "A complete dump already exists! Are you sure you want to overwrite?", 2, true);
                if (mbresult != true)
                {
                    LogLn("Dumping aborted!");
                    return false;
                }
            }
            else
            {
                // If a partial dump exists
                bool foundAnyFiles = _environment.FoundAnyFiles(CurrentPhysicalMediaType, outputDirectory, outputFilename);
                if (foundAnyFiles && _displayUserMessage is not null)
                {
                    bool? mbresult = _displayUserMessage("Overwrite?", $"A partial dump already exists! Dumping here may cause issues. Are you sure you want to overwrite?", 2, true);
                    if (mbresult != true)
                    {
                        LogLn("Dumping aborted!");
                        return false;
                    }
                }
                else
                {
                    // If a complete dump exists from a different program
                    InternalProgram? completeProgramFound = _environment.CheckForMatchingProgram(CurrentPhysicalMediaType, outputDirectory, outputFilename);
                    if (completeProgramFound is not null && _displayUserMessage is not null)
                    {
                        bool? mbresult = _displayUserMessage("Overwrite?", $"A complete dump from {completeProgramFound} already exists! Dumping here may cause issues. Are you sure you want to overwrite?", 2, true);
                        if (mbresult != true)
                        {
                            LogLn("Dumping aborted!");
                            return false;
                        }
                    }
                    else
                    {
                        // If a partial dump exists from a different program
                        InternalProgram? partialProgramFound = _environment.CheckForPartialProgram(CurrentPhysicalMediaType, outputDirectory, outputFilename);
                        if (partialProgramFound is not null && _displayUserMessage is not null)
                        {
                            bool? mbresult = _displayUserMessage("Overwrite?", $"A partial dump from {partialProgramFound} already exists! Dumping here may cause issues. Are you sure you want to overwrite?", 2, true);
                            if (mbresult != true)
                            {
                                LogLn("Dumping aborted!");
                                return false;
                            }
                        }
                    }
                }
            }

            // Validate that at least some space exists
            // TODO: Tie this to the size of the disc, type of disc, etc.
            string fullPath;
            if (string.IsNullOrEmpty(outputDirectory))
                fullPath = Path.GetFullPath(_environment.OutputPath);
            else
                fullPath = Path.GetFullPath(outputDirectory);

            var driveInfo = new DriveInfo(Path.GetPathRoot(fullPath) ?? string.Empty);
            if (driveInfo.AvailableFreeSpace < Math.Pow(2, 30) && _displayUserMessage is not null)
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

        /// <summary>
        /// Checks whether a internal program is found in its path
        /// </summary>
        /// <param name="program">Program to check for</param>
        /// <returns>True if the program is found, false otherwise</returns>
        private bool InternalProgramExists(InternalProgram program)
        {
            try
            {
#pragma warning disable IDE0072
                return program switch
                {
                    InternalProgram.Aaru => FrontendTool.ResolveBinaryPath(Options.Dumping.AaruPath) != null,
                    InternalProgram.DiscImageCreator => FrontendTool.ResolveBinaryPath(Options.Dumping.DiscImageCreatorPath) != null,
                    // InternalProgram.Dreamdump => FrontendTool.ResolveBinaryPath(Options.Dumping.DreamdumpPath) != null,
                    InternalProgram.Redumper => FrontendTool.ResolveBinaryPath(Options.Dumping.RedumperPath) != null,
                    _ => false,
                };
#pragma warning restore IDE0072
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Translates strings in MainModelView
        /// </summary>
        /// <param name="translationStrings">Dictionary of keys and their translated string</param>
        public void TranslateStrings(Dictionary<string, string>? translationStrings)
        {
            if (translationStrings is not null)
            {
                // Cache current start dumping string
                var oldStartDumpingValue = StartDumpingValue;

                // Get translated strings
                if (translationStrings.TryGetValue("StartDumpingButtonString", out string? startDumpingButtonString))
                    StartDumpingValue = startDumpingButtonString ?? StartDumpingValue;
                if (translationStrings.TryGetValue("StopDumpingButtonString", out string? stopDumpingValue))
                    StopDumpingValue = stopDumpingValue ?? StopDumpingValue;

                // Set button text
                if ((StartStopButtonText as string) == oldStartDumpingValue)
                    StartStopButtonText = StartDumpingValue;
                else
                    StartStopButtonText = StopDumpingValue;
            }
        }

        #endregion

        #region Progress Reporting

        /// <summary>
        /// Handler for Result ProgressChanged event
        /// </summary>
        private void ProgressUpdated(object? sender, StringEventArgs value)
        {
            try
            {
                LogLn(value);
            }
            catch { }
        }

        /// <summary>
        /// Handler for Result ProgressChanged event
        /// </summary>
        private void ProgressUpdated(object? sender, ResultEventArgs value)
        {
            var message = value.Message;

            // Update the label with only the first line of output
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
            if (message is not null && message.Contains('\n'))
#else
            if (message is not null && message.Contains("\n"))
#endif
                Status = message.Split('\n')[0] + " (See log output)";
            else
                Status = message ?? string.Empty;

            // Log based on success or failure
            if ((bool?)value is null)
                LogLn(message ?? string.Empty);
            else if (value == true)
                SuccessLogLn(message ?? string.Empty);
            else if (value == false)
                ErrorLogLn(message ?? string.Empty);
        }

        /// <summary>
        /// Handler for ProtectionProgress ProgressChanged event
        /// </summary>
        private void ProgressUpdated(object? sender, ProtectionProgress value)
        {
            string message = $"{value.Percentage * 100:N2}%: {value.Filename} - {value.Protection}";
            Status = message;
            VerboseLogLn(message);
        }

        #endregion
    }
}
