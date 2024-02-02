using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
        /// Currently provided ManaGunZ log path
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
        /// Indicates the status of the ManaGunZ log path text box
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
        /// Indicates the status of the ManaGunZ log path browse button
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
        /// Indicates the status of the key text box
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
            _keyStatus = "Will attempt to pull key from redump.org";

            InputPathTextBoxEnabled = true;
            InputPathBrowseButtonEnabled = true;
            LogPathTextBoxEnabled = true;
            LogPathBrowseButtonEnabled = true;
            KeyPathTextBoxEnabled = true;
            KeyPathBrowseButtonEnabled = true;
            HexKeyTextBoxEnabled = true;
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
        /// Change the currently selected input path
        /// </summary>
        public void ChangeInputPath()
        {
            this.CreateIRDButtonEnabled = !string.IsNullOrEmpty(this.InputPath);
        }

        /// <summary>
        /// Change the currently selected ManaGunZ log path
        /// </summary>
        public void ChangeLogPath()
        {
            if (ParseLog(this.LogPath, out byte[]? key))
            {
                Key = key;
                KeyStatus = "Getting key from ManaGunZ Log";
                KeyPathTextBoxEnabled = false;
                KeyPathBrowseButtonEnabled = false;
                HexKeyTextBoxEnabled = false;
            }
            else
            {
                Key = null;
                KeyStatus = "Will attempt to pull key from redump.org";
                KeyPathTextBoxEnabled = true;
                KeyPathBrowseButtonEnabled = true;
                HexKeyTextBoxEnabled = true;
            }
        }

        /// <summary>
        /// Change the currently selected key file path
        /// </summary>
        public void ChangeKeyPath()
        {
            if (ParseKeyFile(this.KeyPath, out byte[]? key))
            {
                Key = key;
                KeyStatus = "Getting key from Key File";
                LogPathTextBoxEnabled = false;
                LogPathBrowseButtonEnabled = false;
                HexKeyTextBoxEnabled = false;
            }
            else
            {
                Key = null;
                KeyStatus = "Will attempt to pull key from redump.org";
                LogPathTextBoxEnabled = true;
                LogPathBrowseButtonEnabled = true;
                HexKeyTextBoxEnabled = true;
            }
        }

        /// <summary>
        /// Change the currently selected hexadecimal key
        /// </summary>
        public void ChangeKey()
        {
            if (ParseHexKey(this.HexKey, out byte[]? key))
            {
                Key = key;
                KeyStatus = "Getting key from Hexadecimal Key";
                LogPathTextBoxEnabled = false;
                LogPathBrowseButtonEnabled = false;
                KeyPathTextBoxEnabled = false;
                KeyPathBrowseButtonEnabled = false;
            }
            else
            {
                Key = null;
                KeyStatus = "Will attempt to pull key from redump.org";
                LogPathTextBoxEnabled = true;
                LogPathBrowseButtonEnabled = true;
                KeyPathTextBoxEnabled = true;
                KeyPathBrowseButtonEnabled = true;
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

        #endregion

        #region LibIRD

        /// <summary>
        /// Validates a ManaGunZ log to check for presence of valid PS3 key
        /// </summary>
        /// <param name="logPath">Path to ManaGunZ log file</param>
        /// <returns>True if path to log file contains valid key</returns>
        internal bool ParseLog(string? logPath, out byte[]? key)
        {
            key = null;
            // TODO: Parse ManaGunZ log
            return !string.IsNullOrEmpty(logPath);
        }

        /// <summary>
        /// Validates a key file to check for presence of valid PS3 key
        /// </summary>
        /// <param name="keyPath">Path to key file</param>
        /// <returns>True if path contains valid key</returns>
        internal bool ParseKeyFile(string? keyPath, out byte[]? key)
        {
            key = null;
            // TODO: Parse key file
            return !string.IsNullOrEmpty(keyPath);
        }

        /// <summary>
        /// Validates a hexadecimal key
        /// </summary>
        /// <param name="key">String representing hexadecimal key</param>
        /// <returns>True if string is a valid key</returns>
        internal bool ParseHexKey(string? hexKey, out byte[]? key)
        {
            key = null;
            // TODO: Parse hex key
            return !string.IsNullOrEmpty(hexKey);
        }

        /// <summary>
        /// Performs LibIRD functionality
        /// </summary>
        /// <returns>An error message if failed, otherwise string.Empty/null</returns>
        public string? CreateIRD()
        {
            if (string.IsNullOrEmpty(InputPath))
                return "Invalid Input path";

            if (!File.Exists(this.InputPath!.Trim('"')))
                return $"{this.InputPath!.Trim('"')} is not a valid file";

            if (Key == null)
            {
                // TODO: Pull key from redump
                Key = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
            }

            // TODO: Create IRD using LibIRD

            return "";
        }

        #endregion
    }
}
