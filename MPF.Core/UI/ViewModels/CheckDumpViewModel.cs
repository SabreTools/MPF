using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BinaryObjectScanner;
using MPF.Core.Data;
using MPF.Core.UI.ComboBoxItems;
using MPF.Core.Utilities;
using SabreTools.RedumpLib.Data;

namespace MPF.Core.UI.ViewModels
{
    /// <summary>
    /// Constructor
    /// </summary>
    public class CheckDumpViewModel : INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        /// Access to the current options
        /// </summary>
        public Data.Options Options
        {
            get => _options;
        }
        private readonly Data.Options _options;

        /// <summary>
        /// Indicates if SelectionChanged events can be executed
        /// </summary>
        public bool CanExecuteSelectionChanged { get; private set; } = false;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region Properties

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
        /// Currently provided input path
        /// </summary>
        public string? InputPath
        {
            get => _inputPath;
            set
            {
                _inputPath = value;
                TriggerPropertyChanged(nameof(InputPath));
            }
        }
        private string? _inputPath;

        /// <summary>
        /// Indicates the status of the input path text box
        /// </summary>
        public bool InputPathTextBoxEnabled
        {
            get => _inputPathTextBoxEnabled;
            set
            {
                _inputPathTextBoxEnabled = value;
                TriggerPropertyChanged(nameof(InputPathTextBoxEnabled));
            }
        }
        private bool _inputPathTextBoxEnabled;

        /// <summary>
        /// Indicates the status of the input path browse button
        /// </summary>
        public bool InputPathBrowseButtonEnabled
        {
            get => _inputPathBrowseButtonEnabled;
            set
            {
                _inputPathBrowseButtonEnabled = value;
                TriggerPropertyChanged(nameof(InputPathBrowseButtonEnabled));
            }
        }
        private bool _inputPathBrowseButtonEnabled;

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
        /// Indicates the status of the check dump button
        /// </summary>
        public bool CheckDumpButtonEnabled
        {
            get => _checkDumpButtonEnabled;
            set
            {
                _checkDumpButtonEnabled = value;
                TriggerPropertyChanged(nameof(CheckDumpButtonEnabled));
            }
        }
        private bool _checkDumpButtonEnabled;

        /// <summary>
        /// Indicates the status of the cancel button
        /// </summary>
        public bool CancelButtonEnabled
        {
            get => _cancelButtonEnabled;
            set
            {
                _cancelButtonEnabled = value;
                TriggerPropertyChanged(nameof(CancelButtonEnabled));
            }
        }
        private bool _cancelButtonEnabled;

        #endregion

        #region List Properties

        /// <summary>
        /// Current list of supported media types
        /// </summary>
        public List<Element<MediaType>>? MediaTypes
        {
            get => _mediaTypes;
            set
            {
                _mediaTypes = value;
                TriggerPropertyChanged(nameof(MediaTypes));
            }
        }
        private List<Element<MediaType>>? _mediaTypes;

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
        /// Constructor for pure view model
        /// </summary>
        public CheckDumpViewModel()
        {
            _options = OptionsLoader.LoadFromConfig();
            _internalPrograms = [];
            _inputPath = string.Empty;
            _systems = [];

            SystemTypeComboBoxEnabled = true;
            InputPathTextBoxEnabled = true;
            InputPathBrowseButtonEnabled = true;
            MediaTypeComboBoxEnabled = true;
            DumpingProgramComboBoxEnabled = true;
            CheckDumpButtonEnabled = false;
            CancelButtonEnabled = true;

            MediaTypes = [];
            Systems = RedumpSystemComboBoxItem.GenerateElements().ToList();
            InternalPrograms = [];

            PopulateMediaType();
            PopulateInternalPrograms();
            EnableEventHandlers();
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

        #region UI Commands

        /// <summary>
        /// Change the currently selected system
        /// </summary>
        public void ChangeSystem()
        {
            PopulateMediaType();
            this.CheckDumpButtonEnabled = ShouldEnableCheckDumpButton();
        }

        /// <summary>
        /// Change the currently selected media type
        /// </summary>
        public void ChangeMediaType()
        {
            this.CheckDumpButtonEnabled = ShouldEnableCheckDumpButton();
        }

        /// <summary>
        /// Change the currently selected dumping program
        /// </summary>
        public void ChangeDumpingProgram()
        {
            this.CheckDumpButtonEnabled = ShouldEnableCheckDumpButton();
        }

        /// <summary>
        /// Change the currently selected input path
        /// </summary>
        public void ChangeInputPath()
        {
            this.CheckDumpButtonEnabled = ShouldEnableCheckDumpButton();
        }

        #endregion

        #region UI Control

        /// <summary>
        /// Enables all UI elements that should be enabled
        /// </summary>
        private void EnableUIElements()
        {
            SystemTypeComboBoxEnabled = true;
            InputPathTextBoxEnabled = true;
            InputPathBrowseButtonEnabled = true;
            DumpingProgramComboBoxEnabled = true;
            PopulateMediaType();
            CheckDumpButtonEnabled = ShouldEnableCheckDumpButton();
            CancelButtonEnabled = true;
        }

        /// <summary>
        /// Disables all UI elements
        /// </summary>
        private void DisableUIElements()
        {
            SystemTypeComboBoxEnabled = false;
            InputPathTextBoxEnabled = false;
            InputPathBrowseButtonEnabled = false;
            MediaTypeComboBoxEnabled = false;
            DumpingProgramComboBoxEnabled = false;
            CheckDumpButtonEnabled = false;
            CancelButtonEnabled = false;
        }

        #endregion

        #region Population

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

            // Create a static list of supported Check programs, not everything
            var internalPrograms = new List<InternalProgram> { InternalProgram.Redumper, InternalProgram.Aaru, InternalProgram.DiscImageCreator, InternalProgram.CleanRip, InternalProgram.PS3CFW, InternalProgram.UmdImageCreator, InternalProgram.XboxBackupCreator };
            InternalPrograms = internalPrograms.Select(ip => new Element<InternalProgram>(ip)).ToList();

            // Select the current default dumping program
            int currentIndex = InternalPrograms.FindIndex(m => m == internalProgram);
            this.CurrentProgram = (currentIndex > -1 ? InternalPrograms[currentIndex].Value : InternalPrograms[0].Value);

            // Reenable event handlers, if necessary
            if (cachedCanExecuteSelectionChanged) EnableEventHandlers();
        }

        #endregion

        #region UI Functionality

        private bool ShouldEnableCheckDumpButton()
        {
            return this.CurrentSystem != null && this.CurrentMediaType != null && !string.IsNullOrEmpty(this.InputPath);
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

        #region MPF.Check

        /// <summary>
        /// Performs MPF.Check functionality
        /// </summary>
        /// <returns>An error message if failed, otherwise string.Empty/null</returns>
        public async Task<string?> CheckDump(Func<SubmissionInfo?, (bool?, SubmissionInfo?)> processUserInfo)
        {
            if (string.IsNullOrEmpty(InputPath))
                return "Invalid Input path";

            if (!File.Exists(this.InputPath!.Trim('"')))
                return "Input Path is not a valid file";

            // Disable UI while Check is running
            DisableUIElements();
            bool cachedCanExecuteSelectionChanged = CanExecuteSelectionChanged;
            DisableEventHandlers();

            // Populate an environment
            var env = new DumpEnvironment(Options, Path.GetFullPath(this.InputPath.Trim('"')), null, this.CurrentSystem, this.CurrentMediaType, this.CurrentProgram, parameters: null);

            // Make new Progress objects
            var resultProgress = new Progress<Result>();
            resultProgress.ProgressChanged += ConsoleLogger.ProgressUpdated;
            var protectionProgress = new Progress<ProtectionProgress>();
            protectionProgress.ProgressChanged += ConsoleLogger.ProgressUpdated;

            // Finally, attempt to do the output dance
            var result = await env.VerifyAndSaveDumpOutput(resultProgress, protectionProgress, processUserInfo);

            // Reenable UI and event handlers, if necessary
            EnableUIElements();
            if (cachedCanExecuteSelectionChanged)
                EnableEventHandlers();

            return result.Message;
        }

        #endregion
    }
}
