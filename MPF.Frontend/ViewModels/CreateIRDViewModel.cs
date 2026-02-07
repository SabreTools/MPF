using System;
using System.ComponentModel;
using System.IO;
using MPF.Frontend.Tools;
using MPF.Processors;

namespace MPF.Frontend.ViewModels
{
    /// <summary>
    /// Constructor
    /// </summary>
    public class CreateIRDViewModel : INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        /// Access to the current options
        /// </summary>
        public SegmentedOptions Options
        {
            get => _options;
        }
        private readonly SegmentedOptions _options;

        /// <summary>
        /// Indicates if SelectionChanged events can be executed
        /// </summary>
        public bool CanExecuteSelectionChanged { get; private set; } = false;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region Properties

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

        /// <summary>
        /// Indicates the status of the check dump button
        /// </summary>
        public bool CreateIRDButtonEnabled
        {
            get => _createIRDButtonEnabled;
            set
            {
                _createIRDButtonEnabled = value;
                TriggerPropertyChanged(nameof(CreateIRDButtonEnabled));
            }
        }
        private bool _createIRDButtonEnabled;

        /// <summary>
        /// Current Create IRD status message
        /// </summary>
        public string CreateIRDStatus
        {
            get => _createIRDStatus;
            set
            {
                _createIRDStatus = value;
                TriggerPropertyChanged(nameof(CreateIRDStatus));
            }
        }
        private string _createIRDStatus;

        /// <summary>
        /// Currently provided Disc ID
        /// </summary>
        public byte[]? DiscID
        {
            get => _discID;
            set
            {
                _discID = value;
                TriggerPropertyChanged(nameof(DiscID));
            }
        }
        private byte[]? _discID;

        /// <summary>
        /// Current disc ID status message
        /// </summary>
        public string DiscIDStatus
        {
            get => _discIDStatus;
            set
            {
                _discIDStatus = value;
                TriggerPropertyChanged(nameof(DiscIDStatus));
            }
        }
        private string _discIDStatus;

        /// <summary>
        /// Currently provided Disc ID string
        /// </summary>
        public string? DiscIDString
        {
            get => _discIDString;
            set
            {
                _discIDString = value;
                TriggerPropertyChanged(nameof(DiscIDString));
            }
        }
        private string? _discIDString;

        /// <summary>
        /// Indicates the status of the disc ID text box
        /// </summary>
        public bool DiscIDTextBoxEnabled
        {
            get => _discIDTextBoxEnabled;
            set
            {
                _discIDTextBoxEnabled = value;
                TriggerPropertyChanged(nameof(DiscIDTextBoxEnabled));
            }
        }
        private bool _discIDTextBoxEnabled;

        /// <summary>
        /// Currently provided hexadecimal key
        /// </summary>
        public string? HexKey
        {
            get => _hexKey;
            set
            {
                _hexKey = value;
                TriggerPropertyChanged(nameof(HexKey));
            }
        }
        private string? _hexKey;

        /// <summary>
        /// Indicates the status of the hex key text box
        /// </summary>
        public bool HexKeyTextBoxEnabled
        {
            get => _hexKeyTextBoxEnabled;
            set
            {
                _hexKeyTextBoxEnabled = value;
                TriggerPropertyChanged(nameof(HexKeyTextBoxEnabled));
            }
        }
        private bool _hexKeyTextBoxEnabled;

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
        /// Currently provided key
        /// </summary>
        public byte[]? Key
        {
            get => _key;
            set
            {
                _key = value;
                TriggerPropertyChanged(nameof(Key));
            }
        }
        private byte[]? _key;

        /// <summary>
        /// Currently provided key path
        /// </summary>
        public string? KeyPath
        {
            get => _keyPath;
            set
            {
                _keyPath = value;
                TriggerPropertyChanged(nameof(KeyPath));
            }
        }
        private string? _keyPath;

        /// <summary>
        /// Indicates the status of the key path browse button
        /// </summary>
        public bool KeyPathBrowseButtonEnabled
        {
            get => _keyPathBrowseButtonEnabled;
            set
            {
                _keyPathBrowseButtonEnabled = value;
                TriggerPropertyChanged(nameof(KeyPathBrowseButtonEnabled));
            }
        }
        private bool _keyPathBrowseButtonEnabled;

        /// <summary>
        /// Indicates the status of the key path text box
        /// </summary>
        public bool KeyPathTextBoxEnabled
        {
            get => _keyPathTextBoxEnabled;
            set
            {
                _keyPathTextBoxEnabled = value;
                TriggerPropertyChanged(nameof(KeyPathTextBoxEnabled));
            }
        }
        private bool _keyPathTextBoxEnabled;

        /// <summary>
        /// Current key status message
        /// </summary>
        public string KeyStatus
        {
            get => _keyStatus;
            set
            {
                _keyStatus = value;
                TriggerPropertyChanged(nameof(KeyStatus));
            }
        }
        private string _keyStatus;

        /// <summary>
        /// Currently provided layerbreak
        /// </summary>
        public long? Layerbreak
        {
            get => _layerbreak;
            set
            {
                _layerbreak = value;
                TriggerPropertyChanged(nameof(Layerbreak));
            }
        }
        private long? _layerbreak;

        /// <summary>
        /// Currently provided layerbreak string
        /// </summary>
        public string? LayerbreakString
        {
            get => _layerbreakString;
            set
            {
                _layerbreakString = value;
                TriggerPropertyChanged(nameof(LayerbreakString));
            }
        }
        private string? _layerbreakString;

        /// <summary>
        /// Indicates the status of the layerbreak text box
        /// </summary>
        public bool LayerbreakTextBoxEnabled
        {
            get => _layerbreakTextBoxEnabled;
            set
            {
                _layerbreakTextBoxEnabled = value;
                TriggerPropertyChanged(nameof(LayerbreakTextBoxEnabled));
            }
        }
        private bool _layerbreakTextBoxEnabled;

        /// <summary>
        /// Currently provided .getkey.log path
        /// </summary>
        public string? LogPath
        {
            get => _logPath;
            set
            {
                _logPath = value;
                TriggerPropertyChanged(nameof(LogPath));
            }
        }
        private string? _logPath;

        /// <summary>
        /// Indicates the status of the .getkey.log path browse button
        /// </summary>
        public bool LogPathBrowseButtonEnabled
        {
            get => _logPathBrowseButtonEnabled;
            set
            {
                _logPathBrowseButtonEnabled = value;
                TriggerPropertyChanged(nameof(LogPathBrowseButtonEnabled));
            }
        }
        private bool _logPathBrowseButtonEnabled;

        /// <summary>
        /// Indicates whether a .getkey.log path is not provided
        /// </summary>
        public bool LogPathNotProvided
        {
            get => _logPathNotProvided;
            set
            {
                _logPathNotProvided = value;
                TriggerPropertyChanged(nameof(LogPathNotProvided));
            }
        }
        private bool _logPathNotProvided;

        /// <summary>
        /// Indicates the status of the .getkey.log path text box
        /// </summary>
        public bool LogPathTextBoxEnabled
        {
            get => _logPathTextBoxEnabled;
            set
            {
                _logPathTextBoxEnabled = value;
                TriggerPropertyChanged(nameof(LogPathTextBoxEnabled));
            }
        }
        private bool _logPathTextBoxEnabled;

        /// <summary>
        /// Currently provided PIC file path
        /// </summary>
        public string? PICPath
        {
            get => _picPath;
            set
            {
                _picPath = value;
                TriggerPropertyChanged(nameof(PICPath));
            }
        }
        private string? _picPath;

        /// <summary>
        /// Indicates the status of the PIC file path browse button
        /// </summary>
        public bool PICPathBrowseButtonEnabled
        {
            get => _picPathBrowseButtonEnabled;
            set
            {
                _picPathBrowseButtonEnabled = value;
                TriggerPropertyChanged(nameof(PICPathBrowseButtonEnabled));
            }
        }
        private bool _picPathBrowseButtonEnabled;

        /// <summary>
        /// Indicates the status of the PIC file path text box
        /// </summary>
        public bool PICPathTextBoxEnabled
        {
            get => _picPathTextBoxEnabled;
            set
            {
                _picPathTextBoxEnabled = value;
                TriggerPropertyChanged(nameof(PICPathTextBoxEnabled));
            }
        }
        private bool _picPathTextBoxEnabled;

        /// <summary>
        /// Currently provided PIC
        /// </summary>
        public byte[]? PIC
        {
            get => _pic;
            set
            {
                _pic = value;
                TriggerPropertyChanged(nameof(PIC));
            }
        }
        private byte[]? _pic;

        /// <summary>
        /// Current PIC status message
        /// </summary>
        public string PICStatus
        {
            get => _picStatus;
            set
            {
                _picStatus = value;
                TriggerPropertyChanged(nameof(PICStatus));
            }
        }
        private string _picStatus;

        /// <summary>
        /// Currently provided PIC string
        /// </summary>
        public string? PICString
        {
            get => _picString;
            set
            {
                _picString = value;
                TriggerPropertyChanged(nameof(PICString));
            }
        }
        private string? _picString;

        /// <summary>
        /// Indicates the status of the PIC text box
        /// </summary>
        public bool PICTextBoxEnabled
        {
            get => _picTextBoxEnabled;
            set
            {
                _picTextBoxEnabled = value;
                TriggerPropertyChanged(nameof(PICTextBoxEnabled));
            }
        }
        private bool _picTextBoxEnabled;

        #endregion

        /// <summary>
        /// Constructor for pure view model
        /// </summary>
        public CreateIRDViewModel()
        {
            _options = OptionsLoader.LoadFromConfig();

            _inputPath = string.Empty;
            _logPath = string.Empty;

            _keyPath = string.Empty;
            _hexKey = string.Empty;
            _key = null;
            //_keyStatus = "Will attempt to pull Encryption Key from redump.org";
            _keyStatus = "Cannot create an IRD without a key";

            _discID = null;
            _discIDString = string.Empty;
            _discIDStatus = "Unknown Disc ID, generating ID using Region: NONE";

            _picPath = string.Empty;
            _layerbreakString = string.Empty;
            _picString = string.Empty;
            _pic = null;
            _picStatus = "Will generate a PIC assuming a Layerbreak of 12219392";

            _createIRDStatus = "Please provide an ISO";

            _inputPathTextBoxEnabled = true;
            _inputPathBrowseButtonEnabled = true;
            _logPathTextBoxEnabled = true;
            _logPathNotProvided = true;
            _logPathBrowseButtonEnabled = true;
            _discIDTextBoxEnabled = true;
            _keyPathTextBoxEnabled = true;
            _keyPathBrowseButtonEnabled = true;
            _hexKeyTextBoxEnabled = true;
            _picPathTextBoxEnabled = true;
            _picPathBrowseButtonEnabled = true;
            _picTextBoxEnabled = true;
            _layerbreakTextBoxEnabled = true;
            _createIRDButtonEnabled = false;
            _cancelButtonEnabled = true;

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
        /// Determine if the Create IRD button should be enabled
        /// </summary>
        private bool ShouldEnableCreateIRDButton()
        {
            if (string.IsNullOrEmpty(InputPath) || !File.Exists(InputPath))
            {
                CreateIRDStatus = "Please provide an ISO";
                return false;
            }

            if (string.IsNullOrEmpty(LogPath) && string.IsNullOrEmpty(HexKey) && string.IsNullOrEmpty(KeyPath))
            {
                CreateIRDStatus = "Please provide a GetKey log or Disc Key";
                return false;
            }

            CreateIRDStatus = "Ready to create IRD";
            return true;
        }

        /// <summary>
        /// Determine if the Log Path TextBox and Browse Button should be enabled
        /// </summary>
        /// <returns></returns>
        private bool ShouldEnableLogPath()
        {
            return string.IsNullOrEmpty(LogPath)
                && string.IsNullOrEmpty(HexKey)
                && string.IsNullOrEmpty(KeyPath)
                && string.IsNullOrEmpty(DiscIDString)
                && string.IsNullOrEmpty(PICString)
                && string.IsNullOrEmpty(PICPath)
                && Layerbreak is null;
        }

        /// <summary>
        /// Change the currently selected input path
        /// </summary>
        public void ChangeInputPath()
        {
            CreateIRDButtonEnabled = ShouldEnableCreateIRDButton();
        }

        /// <summary>
        /// Change the currently selected .getkey.log path
        /// </summary>
        public void ChangeLogPath()
        {
            if (string.IsNullOrEmpty(LogPath))
            {
                // No .getkey.log file provided: Reset Key and PIC sections
                LogPathNotProvided = true;

                Key = null;
                //KeyStatus = "Will attempt to pull Encryption Key from redump.org"; // Use this when redump key pulling is implemented
                KeyStatus = "Cannot create an IRD without a key";
                KeyPathTextBoxEnabled = true;
                KeyPathBrowseButtonEnabled = true;
                HexKeyTextBoxEnabled = true;

                DiscID = null;
                DiscIDStatus = "Unknown Disc ID, using Region: NONE";
                DiscIDTextBoxEnabled = true;

                PIC = null;
                PICStatus = "Will generate a PIC assuming a Layerbreak of 12219392";
                PICPathTextBoxEnabled = true;
                PICPathBrowseButtonEnabled = true;
                PICTextBoxEnabled = true;
                LayerbreakTextBoxEnabled = true;
                //CreateIRDButtonEnabled = ShouldEnableCreateIRDButton(); // Use this when redump key pulling is implemented
                CreateIRDStatus = "Please provide a GetKey log or Disc Key";
                CreateIRDButtonEnabled = false;

                return;
            }

            // A .getkey.log path is provided: Disable Key and PIC sections
            LogPathNotProvided = false;
            KeyPathTextBoxEnabled = false;
            KeyPathBrowseButtonEnabled = false;
            HexKeyTextBoxEnabled = false;
            DiscIDTextBoxEnabled = false;
            PICPathTextBoxEnabled = false;
            PICPathBrowseButtonEnabled = false;
            PICTextBoxEnabled = false;
            LayerbreakTextBoxEnabled = false;

            if (ProcessingTool.ParseGetKeyLog(LogPath, out byte[]? key, out byte[]? id, out byte[]? pic))
            {
                Key = key;
                DiscID = id;
                PIC = pic;
                KeyStatus = $"Using key from file: {Path.GetFileName(LogPath)}";
                DiscIDStatus = $"Using ID from file: {Path.GetFileName(LogPath)}";
                PICStatus = $"Using PIC from file: {Path.GetFileName(LogPath)}";
                CreateIRDButtonEnabled = ShouldEnableCreateIRDButton();
            }
            else
            {
                Key = null;
                DiscID = null;
                PIC = null;
                CreateIRDStatus = "Please provide a valid GetKey log file path";
                if (File.Exists(LogPath))
                {
                    KeyStatus = "ERROR: Invalid *.getkey.log file";
                    DiscIDStatus = "ERROR: Invalid *.getkey.log file";
                    PICStatus = "ERROR: Invalid *.getkey.log file";
                }
                else
                {
                    KeyStatus = "ERROR: Invalid *.getkey.log path";
                    DiscIDStatus = "ERROR: Invalid *.getkey.log path";
                    PICStatus = "ERROR: Invalid *.getkey.log path";
                }

                CreateIRDButtonEnabled = false;
            }
        }

        /// <summary>
        /// Change the currently selected disc ID
        /// </summary>
        public void ChangeDiscID()
        {
            if (string.IsNullOrEmpty(DiscIDString))
            {
                DiscID = null;
                DiscIDStatus = "Unknown Disc ID, generating ID using Region: NONE";
                LogPathTextBoxEnabled = ShouldEnableLogPath();
                LogPathBrowseButtonEnabled = ShouldEnableLogPath();
                CreateIRDButtonEnabled = ShouldEnableCreateIRDButton();
                return;
            }

            LogPathTextBoxEnabled = false;
            LogPathBrowseButtonEnabled = false;

            byte[]? id = ProcessingTool.ParseDiscID(DiscIDString);
            if (id is not null)
            {
                DiscID = id;
                DiscIDStatus = $"Using provided ID: {DiscIDString}";
                CreateIRDButtonEnabled = ShouldEnableCreateIRDButton();
            }
            else
            {
                DiscID = null;
                DiscIDStatus = "ERROR: Invalid Disc ID";
                CreateIRDStatus = "Please provide a valid Disc ID";
                CreateIRDButtonEnabled = false;
            }
        }

        /// <summary>
        /// Change the currently selected key file path
        /// </summary>
        public void ChangeKeyPath()
        {
            if (string.IsNullOrEmpty(KeyPath))
            {
                Key = null;
                //KeyStatus = "Will attempt to pull Encryption Key from redump.org"; // Use this when redump key pulling is implemented
                KeyStatus = "Cannot create an IRD without a key";
                LogPathTextBoxEnabled = ShouldEnableLogPath();
                LogPathBrowseButtonEnabled = ShouldEnableLogPath();
                HexKeyTextBoxEnabled = true;
                //CreateIRDButtonEnabled = ShouldEnableCreateIRDButton(); // Use this when redump key pulling is implemented
                CreateIRDButtonEnabled = false;
                CreateIRDStatus = "Please provide a GetKey log or Disc Key"; // Remove this when redump key pulling is implemented
                return;
            }

            LogPathTextBoxEnabled = false;
            LogPathBrowseButtonEnabled = false;
            HexKeyTextBoxEnabled = false;

            byte[]? key = ProcessingTool.ParseKeyFile(KeyPath);
            if (key is not null)
            {
                Key = key;
                KeyStatus = $"Using key from file: {Path.GetFileName(KeyPath)}";
                CreateIRDButtonEnabled = ShouldEnableCreateIRDButton();
            }
            else
            {
                Key = null;
                if (File.Exists(KeyPath))
                    KeyStatus = "ERROR: Invalid *.key file";
                else
                    KeyStatus = "ERROR: Invalid *.key path";
                CreateIRDButtonEnabled = false;
                CreateIRDStatus = "Please provide a valid key file path";
            }
        }

        /// <summary>
        /// Change the currently selected hexadecimal key
        /// </summary>
        public void ChangeKey()
        {
            if (string.IsNullOrEmpty(HexKey))
            {
                Key = null;
                //KeyStatus = "Will attempt to pull Encryption Key from redump.org"; // Use this when redump key pulling is implemented
                KeyStatus = "Cannot create an IRD without a key";
                LogPathTextBoxEnabled = ShouldEnableLogPath();
                LogPathBrowseButtonEnabled = ShouldEnableLogPath();
                KeyPathTextBoxEnabled = true;
                KeyPathBrowseButtonEnabled = true;
                //CreateIRDButtonEnabled = ShouldEnableCreateIRDButton(); // Use this when redump key pulling is implemented
                CreateIRDButtonEnabled = false;
                CreateIRDStatus = "Please provide a GetKey log or Disc Key"; // Remove this when redump key pulling is implemented
                return;
            }

            LogPathTextBoxEnabled = false;
            LogPathBrowseButtonEnabled = false;
            KeyPathTextBoxEnabled = false;
            KeyPathBrowseButtonEnabled = false;

            byte[]? key = ProcessingTool.ParseHexKey(HexKey);
            if (key is not null)
            {
                Key = key;
                KeyStatus = $"Using provided Key: {HexKey}";
                CreateIRDButtonEnabled = ShouldEnableCreateIRDButton();
            }
            else
            {
                Key = null;
                KeyStatus = "ERROR: Invalid Key";
                CreateIRDButtonEnabled = false;
                CreateIRDStatus = "Please provide a valid key";
            }
        }

        /// <summary>
        /// Change the currently selected PIC file path
        /// </summary>
        public void ChangePICPath()
        {
            Layerbreak = null;

            if (string.IsNullOrEmpty(PICPath))
            {
                PIC = null;
                PICStatus = "Will generate a PIC assuming a Layerbreak of 12219392";
                PICTextBoxEnabled = true;
                LayerbreakTextBoxEnabled = true;
                LogPathTextBoxEnabled = ShouldEnableLogPath();
                LogPathBrowseButtonEnabled = ShouldEnableLogPath();
                CreateIRDButtonEnabled = ShouldEnableCreateIRDButton();
                return;
            }

            LogPathTextBoxEnabled = false;
            LogPathBrowseButtonEnabled = false;
            PICTextBoxEnabled = false;
            LayerbreakTextBoxEnabled = false;

            PIC = ProcessingTool.ParsePICFile(PICPath);
            if (PIC is not null)
            {
                PICStatus = $"Using PIC from file: {Path.GetFileName(PICPath)}";
                CreateIRDButtonEnabled = ShouldEnableCreateIRDButton();
            }
            else
            {
                if (File.Exists(PICPath))
                    PICStatus = "ERROR: Invalid PIC file";
                else
                    PICStatus = "ERROR: Invalid PIC path";
                CreateIRDButtonEnabled = false;
                CreateIRDStatus = "Please provide a valid PIC";
            }
        }

        /// <summary>
        /// Change the currently selected PIC
        /// </summary>
        public void ChangePIC()
        {
            Layerbreak = null;

            if (string.IsNullOrEmpty(PICString))
            {
                PIC = null;
                PICStatus = "Will generate a PIC assuming a Layerbreak of 12219392";
                PICPathTextBoxEnabled = true;
                PICPathBrowseButtonEnabled = true;
                LayerbreakTextBoxEnabled = true;
                LogPathTextBoxEnabled = ShouldEnableLogPath();
                LogPathBrowseButtonEnabled = ShouldEnableLogPath();
                CreateIRDButtonEnabled = ShouldEnableCreateIRDButton();
                return;
            }

            LogPathTextBoxEnabled = false;
            LogPathBrowseButtonEnabled = false;
            PICPathTextBoxEnabled = false;
            PICPathBrowseButtonEnabled = false;
            LayerbreakTextBoxEnabled = false;

            PIC = ProcessingTool.ParsePIC(PICString);
            if (PIC is not null)
            {
                PICStatus = "Using provided PIC";
                CreateIRDButtonEnabled = ShouldEnableCreateIRDButton();
            }
            else
            {
                PICStatus = "ERROR: Invalid PIC";
                CreateIRDButtonEnabled = false;
                CreateIRDStatus = "Please provide a valid PIC";
            }
        }

        /// <summary>
        /// Change the currently selected layerbreak
        /// </summary>
        public void ChangeLayerbreak()
        {
            PIC = null;

            if (string.IsNullOrEmpty(LayerbreakString))
            {
                Layerbreak = null;
                PICStatus = "Will generate a PIC assuming a Layerbreak of 12219392";
                PICPathTextBoxEnabled = true;
                PICPathBrowseButtonEnabled = true;
                PICTextBoxEnabled = true;
                LogPathTextBoxEnabled = ShouldEnableLogPath();
                LogPathBrowseButtonEnabled = ShouldEnableLogPath();
                CreateIRDButtonEnabled = ShouldEnableCreateIRDButton();
                return;
            }

            LogPathTextBoxEnabled = false;
            LogPathBrowseButtonEnabled = false;
            PICPathTextBoxEnabled = false;
            PICPathBrowseButtonEnabled = false;
            PICTextBoxEnabled = false;

            Layerbreak = ProcessingTool.ParseLayerbreak(LayerbreakString);
            if (Layerbreak is not null)
            {
                PICStatus = $"Will generate a PIC using a Layerbreak of {Layerbreak}";
                CreateIRDButtonEnabled = ShouldEnableCreateIRDButton();
            }
            else
            {
                PICStatus = "ERROR: Invalid Layerbreak";
                CreateIRDButtonEnabled = false;
                CreateIRDStatus = "Please provide a valid Layerbreak value";
            }
        }

        #endregion

        #region UI Functionality

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

        /// <summary>
        /// Resets all UI fields
        /// </summary>
        public void ResetFields()
        {
            InputPath = string.Empty;
            LogPath = string.Empty;

            KeyPath = string.Empty;
            HexKey = string.Empty;
            Key = null;
            //_keyStatus = "Will attempt to pull Encryption Key from redump.org";
            KeyStatus = "Cannot create an IRD without a key";

            DiscID = null;
            DiscIDString = string.Empty;
            DiscIDStatus = "Unknown Disc ID, generating ID using Region: NONE";

            PICPath = string.Empty;
            LayerbreakString = string.Empty;
            PICString = string.Empty;
            PIC = null;
            PICStatus = "Will generate a PIC assuming a Layerbreak of 12219392";

            CreateIRDStatus = "Please provide an ISO";

            InputPathTextBoxEnabled = true;
            InputPathBrowseButtonEnabled = true;
            LogPathTextBoxEnabled = true;
            LogPathNotProvided = true;
            LogPathBrowseButtonEnabled = true;
            DiscIDTextBoxEnabled = true;
            KeyPathTextBoxEnabled = true;
            KeyPathBrowseButtonEnabled = true;
            HexKeyTextBoxEnabled = true;
            PICPathTextBoxEnabled = true;
            PICPathBrowseButtonEnabled = true;
            PICTextBoxEnabled = true;
            LayerbreakTextBoxEnabled = true;
            CreateIRDButtonEnabled = false;
            CancelButtonEnabled = true;
        }

        /// <summary>
        /// Disables all UI fields and returns a list of all their previous states
        /// </summary>
        /// <returns></returns>
        public bool[] DisableUIFields()
        {
            bool[] oldValues =
            [
                InputPathTextBoxEnabled,
                InputPathBrowseButtonEnabled,
                LogPathTextBoxEnabled,
                LogPathNotProvided,
                LogPathBrowseButtonEnabled,
                DiscIDTextBoxEnabled,
                KeyPathTextBoxEnabled,
                KeyPathBrowseButtonEnabled,
                HexKeyTextBoxEnabled,
                PICPathTextBoxEnabled,
                PICPathBrowseButtonEnabled,
                PICTextBoxEnabled,
                LayerbreakTextBoxEnabled,
                CreateIRDButtonEnabled,
                CancelButtonEnabled,
            ];
            InputPathTextBoxEnabled = false;
            InputPathBrowseButtonEnabled = false;
            LogPathTextBoxEnabled = false;
            LogPathNotProvided = false;
            LogPathBrowseButtonEnabled = false;
            DiscIDTextBoxEnabled = false;
            KeyPathTextBoxEnabled = false;
            KeyPathBrowseButtonEnabled = false;
            HexKeyTextBoxEnabled = false;
            PICPathTextBoxEnabled = false;
            PICPathBrowseButtonEnabled = false;
            PICTextBoxEnabled = false;
            LayerbreakTextBoxEnabled = false;
            CreateIRDButtonEnabled = false;
            CancelButtonEnabled = false;

            return oldValues;
        }

        /// <summary>
        /// Re-enables all UI fields to their previous states
        /// </summary>
        /// <param name="oldValues"></param>
        public void ReenableUIFields(bool[] oldValues)
        {
            InputPathTextBoxEnabled = oldValues[0];
            InputPathBrowseButtonEnabled = oldValues[1];
            LogPathTextBoxEnabled = oldValues[2];
            LogPathNotProvided = oldValues[3];
            LogPathBrowseButtonEnabled = oldValues[4];
            DiscIDTextBoxEnabled = oldValues[5];
            KeyPathTextBoxEnabled = oldValues[6];
            KeyPathBrowseButtonEnabled = oldValues[7];
            HexKeyTextBoxEnabled = oldValues[8];
            PICPathTextBoxEnabled = oldValues[9];
            PICPathBrowseButtonEnabled = oldValues[10];
            PICTextBoxEnabled = oldValues[11];
            LayerbreakTextBoxEnabled = oldValues[12];
            CreateIRDButtonEnabled = oldValues[13];
            CancelButtonEnabled = oldValues[14];
        }

        #endregion

        #region LibIRD

        /// <summary>
        /// Performs LibIRD functionality
        /// </summary>
        /// <returns>An error message if failed, otherwise string.Empty/null</returns>
        public string? CreateIRD(string outputPath)
        {
            if (string.IsNullOrEmpty(InputPath))
                return "Invalid ISO path.";

            if (!File.Exists(InputPath!.Trim('"')))
                return $"{InputPath!.Trim('"')} is not a valid ISO path.";

            // TODO: Implement pulling key from redump.org
            if (Key is null)
                return "Pulling key from redump.org is currently not implemented.";

            try
            {
                // Create Redump-style reproducible IRD
                LibIRD.ReIRD ird = new(InputPath, Key, Layerbreak);
                if (PIC is not null)
                    ird.PIC = PIC;
                if (DiscID is not null && ird.DiscID[15] != 0x00)
                    ird.DiscID = DiscID;
                ird.Write(outputPath);
                CreateIRDStatus = "IRD Created Successfully";
                return string.Empty;
            }
            catch (Exception e)
            {
                // Failed to create IRD, return error message
                CreateIRDStatus = "Failed to create IRD";
                return e.Message;
            }
        }

        #endregion
    }
}
