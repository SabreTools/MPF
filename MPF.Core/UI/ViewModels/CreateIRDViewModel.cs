using System;
using System.ComponentModel;
using System.IO;
using MPF.Core.Utilities;
#if NET6_0_OR_GREATER
using LibIRD;
#endif

namespace MPF.Core.UI.ViewModels
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
            if(string.IsNullOrEmpty(InputPath) || !File.Exists(InputPath))
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
                && Layerbreak == null;
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

            if (ParseLog(LogPath, out byte[]? key, out byte[]? id, out byte[]? pic))
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

            byte[]? id = ParseDiscID(DiscIDString);
            if (id != null)
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

            byte[]? key = ParseKeyFile(KeyPath);
            if (key != null)
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

            byte[]? key = ParseHexKey(HexKey);
            if (key != null)
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

            PIC = ParsePICFile(PICPath);
            if (PIC != null)
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

            PIC = ParsePIC(PICString);
            if (PIC != null)
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

            Layerbreak = ParseLayerbreak(LayerbreakString);
            if (Layerbreak != null)
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
            if (Key == null)
                return "Pulling key from redump.org is currently not implemented.";

            try
            {
#if NET6_0_OR_GREATER
                // Create IRD
                ReIRD ird = new(InputPath, Key, Layerbreak);
                if (PIC != null)
                    ird.PIC = PIC;
                if (DiscID != null && ird.DiscID[15] != 0x00)
                    ird.DiscID = DiscID;
                ird.Write(outputPath);
                CreateIRDStatus = "IRD Created Successfully";
                return "";
#else
                return "LibIRD requires .NET Core 6 or greater.";
#endif
            }
            catch (Exception e)
            {
                // Failed to create IRD, return error message
                CreateIRDStatus = "Failed to create IRD";
                return e.Message;
            }
        }

        /// <summary>
        /// Validates a getkey log to check for presence of valid PS3 key
        /// </summary>
        /// <param name="logPath">Path to getkey log file</param>
        /// <param name="key">Output 16 byte key, null if not valid</param>
        /// <returns>True if path to log file contains valid key, false otherwise</returns>
        private static bool ParseLog(string? logPath, out byte[]? key, out byte[]? id, out byte[]? pic)
        {
            key = null;
            id = null;
            pic = null;

            if (string.IsNullOrEmpty(logPath))
                return false;

            try
            {
                if (!File.Exists(logPath))
                    return false;

                // Protect from attempting to read from really long files
                FileInfo logFile = new(logPath);
                if (logFile.Length > 65536)
                    return false;

                // Read from .getkey.log file
                using StreamReader sr = File.OpenText(logPath);

                // Determine whether GetKey was successful
                string? line;
                while ((line = sr.ReadLine()) != null && line.Trim().StartsWith("get_dec_key succeeded!") == false) ;
                if (line == null)
                    return false;

                // Look for Disc Key in log
                while ((line = sr.ReadLine()) != null && line.Trim().StartsWith("disc_key = ") == false) ;
                // If end of file reached, no key found
                if (line == null)
                    return false;
                // Get Disc Key from log
                string discKeyStr = line.Substring("disc_key = ".Length);
                // Validate Disc Key from log
                if (discKeyStr.Length != 32)
                    return false;
                // Convert Disc Key to byte array
                key = HexStringToByteArray(discKeyStr);
                if (key == null)
                    return false;

                // Read Disc ID
                while ((line = sr.ReadLine()) != null && line.Trim().StartsWith("disc_id = ") == false) ;
                // If end of file reached, no ID found
                if (line == null)
                    return false;
                // Get Disc ID from log
                string discIDStr = line.Substring("disc_id = ".Length);
                // Validate Disc ID from log
                if (discIDStr.Length != 32)
                    return false;
                // Replace X's in Disc ID with 00000001
                discIDStr = discIDStr.Substring(0, 24) + "00000001";
                // Convert Disc ID to byte array
                id = HexStringToByteArray(discIDStr);
                if (id == null)
                    return false;

                // Look for PIC in log
                while ((line = sr.ReadLine()) != null && line.Trim().StartsWith("PIC:") == false) ;
                // If end of file reached, no PIC found
                if (line == null)
                    return false;
                // Get PIC from log
                string discPICStr = "";
                for (int i = 0; i < 8; i++)
                    discPICStr += sr.ReadLine();
                if (discPICStr == null)
                    return false;
                // Validate PIC from log
                if (discPICStr.Length != 256)
                    return false;
                // Convert PIC to byte array
                pic = HexStringToByteArray(discPICStr.Substring(0, 230));
                if (pic == null)
                    return false;

                // Double check for warnings in .getkey.log
                while ((line = sr.ReadLine()) != null)
                {
                    string t = line.Trim();
                    if (t.StartsWith("WARNING"))
                        return false;
                    else if (t.StartsWith("SUCCESS"))
                        return true;
                }
            }
            catch
            {
                // We are not concerned with the error
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates a hexadecimal disc ID
        /// </summary>
        /// <param name="discID">String representing hexadecimal disc ID</param>
        /// <returns>True if string is a valid disc ID, false otherwise</returns>
        private static byte[]? ParseDiscID(string? discID)
        {
            if (string.IsNullOrEmpty(discID))
                return null;

            string cleandiscID = discID!.Trim().Replace("\n", string.Empty);

            if (discID!.Length != 32)
                return null;

            // Censor last 4 bytes by replacing with 0x00000001
            cleandiscID = cleandiscID.Substring(0, 24) + "00000001";

            // Convert to byte array, null if invalid hex string
            byte[]? id = HexStringToByteArray(cleandiscID);

            return id;
        }

        /// <summary>
        /// Validates a key file to check for presence of valid PS3 key
        /// </summary>
        /// <param name="keyPath">Path to key file</param>
        /// <returns>Output 16 byte key, null if not valid</returns>
        private static byte[]? ParseKeyFile(string? keyPath)
        {
            if (string.IsNullOrEmpty(keyPath))
                return null;

            // Try read from key file
            try
            {
                if (!File.Exists(keyPath))
                    return null;

                // Key file must be exactly 16 bytes long
                FileInfo keyFile = new(keyPath);
                if (keyFile.Length != 16)
                    return null;
                byte[] key = new byte[16];

                // Read 16 bytes from Key file
                using FileStream fs = new(keyPath, FileMode.Open, FileAccess.Read);
                using BinaryReader reader = new(fs);
                int numBytes = reader.Read(key, 0, 16);
                if (numBytes != 16)
                    return null;

                return key;
            }
            catch
            {
                // Not concerned with error
                return null;
            }
        }

        /// <summary>
        /// Validates a hexadecimal key
        /// </summary>
        /// <param name="hexKey">String representing hexadecimal key</param>
        /// <returns>Output 16 byte key, null if not valid</returns>
        private static byte[]? ParseHexKey(string? hexKey)
        {
            if (string.IsNullOrEmpty(hexKey))
                return null;

            string cleanHexKey = hexKey!.Trim().Replace("\n", string.Empty);

            if (hexKey!.Length != 32)
                return null;

            // Convert to byte array, null if invalid hex string
            byte[]? key = HexStringToByteArray(cleanHexKey);

            return key;
        }

        /// <summary>
        /// Validates a PIC file path
        /// </summary>
        /// <param name="picPath">Path to PIC file</param>
        /// <returns>Output PIC byte array, null if not valid</returns>
        private static byte[]? ParsePICFile(string? picPath)
        {
            if (string.IsNullOrEmpty(picPath))
                return null;

            // Try read from PIC file
            try
            {
                if (!File.Exists(picPath))
                    return null;

                // PIC file must be at least 115 bytes long
                FileInfo picFile = new(picPath);
                if (picFile.Length < 115)
                    return null;
                byte[] pic = new byte[115];

                // Read 115 bytes from PIC file
                using FileStream fs = new(picPath, FileMode.Open, FileAccess.Read);
                using BinaryReader reader = new(fs);
                int numBytes = reader.Read(pic, 0, 115);
                if (numBytes != 115)
                    return null;

                // Validate that a PIC was read by checking first 6 bytes
                if (pic[0] != 0x10 ||
                    pic[1] != 0x02 ||
                    pic[2] != 0x00 ||
                    pic[3] != 0x00 ||
                    pic[4] != 0x44 ||
                    pic[5] != 0x49)
                    return null;

                return pic;
            }
            catch
            {
                // Not concerned with error
                return null;
            }
        }

        /// <summary>
        /// Validates a PIC
        /// </summary>
        /// <param name="inputPIC">String representing PIC</param>
        /// <returns>Output PIC byte array, null if not valid</returns>
        private static byte[]? ParsePIC(string? inputPIC)
        {
            if (string.IsNullOrEmpty(inputPIC))
                return null;

            string cleanPIC = inputPIC!.Trim().Replace("\n", string.Empty);

            if (cleanPIC.Length < 230)
                return null;

            // Convert to byte array, null if invalid hex string
            byte[]? pic = HexStringToByteArray(cleanPIC);

            return pic;
        }

        /// <summary>
        /// Validates a layerbreak value (in sectors)
        /// </summary>
        /// <param name="inputLayerbreak">String representing layerbreak value</param>
        /// <param name="layerbreak">Output layerbreak value, null if not valid</param>
        /// <returns>True if layerbreak is valid, false otherwise</returns>
        private static long? ParseLayerbreak(string? inputLayerbreak)
        {
            if (string.IsNullOrEmpty(inputLayerbreak))
                return null;

            if (!long.TryParse(inputLayerbreak, out long layerbreak))
                return null;

            // Check that layerbreak is positive number and smaller than largest disc size (in sectors)
            if (layerbreak <= 0 || layerbreak > 24438784)
                return null;

            return layerbreak;
        }

        #endregion

        #region Helper Functions

        /// <summary>
        /// Converts a hex string into a byte array
        /// </summary>
        /// <param name="hex">Hex string</param>
        /// <returns>Converted byte array, or null if invalid hex string</returns>
        private static byte[]? HexStringToByteArray(string? hexString)
        {
            // Valid hex string must be an even number of characters
            if (string.IsNullOrEmpty(hexString) || hexString!.Length % 2 == 1)
                return null;

            // Convert ASCII to byte via lookup table
            int[] hexLookup = [0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, 0x00, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F];
            byte[] byteArray = new byte[hexString.Length / 2];
            for (int i = 0; i < hexString.Length; i += 2)
            {
                // Convert next two chars to ASCII value relative to '0'
                int a = Char.ToUpper(hexString[i]) - '0';
                int b = Char.ToUpper(hexString[i + 1]) - '0';
                // Ensure hex string only has '0' through '9' and 'A' through 'F' (case insensitive)
                if ((a < 0 || b < 0 || a > 22 || b > 22) || (a > 10 && a < 17) || (b > 10 && b < 17))
                    return null;
                byteArray[i / 2] = (byte)(hexLookup[a] << 4 | hexLookup[b]);
            }

            return byteArray;
        }

        #endregion
    }
}
