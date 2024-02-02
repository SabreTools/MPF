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
        /// Currently provided .physical path
        /// </summary>
        public string? PhysicalPath
        {
            get => _physicalPath;
            set
            {
                _physicalPath = value;
                TriggerPropertyChanged(nameof(PhysicalPath));
            }
        }
        private string? _physicalPath;

        /// <summary>
        /// Indicates the status of the .physical path browse button
        /// </summary>
        public bool PhysicalPathBrowseButtonEnabled
        {
            get => _physicalPathBrowseButtonEnabled;
            set
            {
                _physicalPathBrowseButtonEnabled = value;
                TriggerPropertyChanged(nameof(PhysicalPathBrowseButtonEnabled));
            }
        }
        private bool _physicalPathBrowseButtonEnabled;

        /// <summary>
        /// Indicates the status of the .physical path text box
        /// </summary>
        public bool PhysicalPathTextBoxEnabled
        {
            get => _physicalPathTextBoxEnabled;
            set
            {
                _physicalPathTextBoxEnabled = value;
                TriggerPropertyChanged(nameof(PhysicalPathTextBoxEnabled));
            }
        }
        private bool _physicalPathTextBoxEnabled;

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
            _keyStatus = "Will attempt to pull Encryption Key from redump.org";

            _physicalPath = string.Empty;
            _layerbreakString = string.Empty;
            _picString = string.Empty;
            _pic = null;
            _picStatus = "Will generate a PIC assuming a Layerbreak of 12219392";

            InputPathTextBoxEnabled = true;
            InputPathBrowseButtonEnabled = true;
            LogPathTextBoxEnabled = true;
            LogPathBrowseButtonEnabled = true;
            KeyPathTextBoxEnabled = true;
            KeyPathBrowseButtonEnabled = true;
            HexKeyTextBoxEnabled = true;
            PhysicalPathTextBoxEnabled = true;
            PhysicalPathBrowseButtonEnabled = true;
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
            return !string.IsNullOrEmpty(this.InputPath)
                && File.Exists(this.InputPath);
        }

        /// <summary>
        /// Change the currently selected input path
        /// </summary>
        public void ChangeInputPath()
        {
            this.CreateIRDButtonEnabled = ShouldEnableCreateIRDButton();
        }

        /// <summary>
        /// Change the currently selected .getkey.log path
        /// </summary>
        public void ChangeLogPath()
        {
            if (string.IsNullOrEmpty(this.LogPath))
            {
                Key = null;
                KeyStatus = "Will attempt to pull Encryption Key from redump.org";
                PICStatus = "Will generate a PIC assuming a Layerbreak of 12219392";
                KeyPathTextBoxEnabled = true;
                KeyPathBrowseButtonEnabled = true;
                HexKeyTextBoxEnabled = true;
                PhysicalPathTextBoxEnabled = true;
                PhysicalPathBrowseButtonEnabled = true;
                PICTextBoxEnabled = true;
                LayerbreakTextBoxEnabled = true;
                CreateIRDButtonEnabled = ShouldEnableCreateIRDButton();
                return;
            }

            KeyPathTextBoxEnabled = false;
            KeyPathBrowseButtonEnabled = false;
            HexKeyTextBoxEnabled = false;
            PhysicalPathTextBoxEnabled = false;
            PhysicalPathBrowseButtonEnabled = false;
            PICTextBoxEnabled = false;
            LayerbreakTextBoxEnabled = false;

            if (ParseLog(this.LogPath, out byte[]? key, out byte[]? id, out byte[]? pic))
            {
                // TODO: Use ID
                Key = key;
                PIC = pic;
                KeyStatus = $"Using key from file: {Path.GetFileName(this.LogPath)}";
                PICStatus = $"Using PIC from file: {Path.GetFileName(this.LogPath)}";
                CreateIRDButtonEnabled = ShouldEnableCreateIRDButton();
            }
            else
            {
                Key = null;
                if (File.Exists(this.LogPath))
                {
                    KeyStatus = "ERROR: Invalid *.getkey.log file";
                    PICStatus = "ERROR: Invalid *.getkey.log file";
                }
                else
                {
                    KeyStatus = "ERROR: Invalid *.getkey.log path";
                    PICStatus = "ERROR: Invalid *.getkey.log path";
                }
                CreateIRDButtonEnabled = false;
            }
        }

        /// <summary>
        /// Change the currently selected key file path
        /// </summary>
        public void ChangeKeyPath()
        {
            if (string.IsNullOrEmpty(this.KeyPath))
            {
                Key = null;
                KeyStatus = "Will attempt to pull Encryption Key from redump.org";
                LogPathTextBoxEnabled = true;
                LogPathBrowseButtonEnabled = true;
                HexKeyTextBoxEnabled = true;
                CreateIRDButtonEnabled = ShouldEnableCreateIRDButton();
                return;
            }

            LogPathTextBoxEnabled = false;
            LogPathBrowseButtonEnabled = false;
            HexKeyTextBoxEnabled = false;

            byte[]? key = ParseKeyFile(this.KeyPath);
            if (key != null)
            {
                Key = key;
                KeyStatus = $"Using key from file: {Path.GetFileName(this.KeyPath)}";
                CreateIRDButtonEnabled = ShouldEnableCreateIRDButton();
            }
            else
            {
                Key = null;
                if (File.Exists(this.KeyPath))
                    KeyStatus = "ERROR: Invalid *.key file";
                else
                    KeyStatus = "ERROR: Invalid *.key path";
                CreateIRDButtonEnabled = false;
            }
        }

        /// <summary>
        /// Change the currently selected hexadecimal key
        /// </summary>
        public void ChangeKey()
        {
            if (string.IsNullOrEmpty(this.HexKey))
            {
                Key = null;
                KeyStatus = "Will attempt to pull Encryption Key from redump.org";
                LogPathTextBoxEnabled = true;
                LogPathBrowseButtonEnabled = true;
                KeyPathTextBoxEnabled = true;
                KeyPathBrowseButtonEnabled = true;
                CreateIRDButtonEnabled = ShouldEnableCreateIRDButton();
                return;
            }

            LogPathTextBoxEnabled = false;
            LogPathBrowseButtonEnabled = false;
            KeyPathTextBoxEnabled = false;
            KeyPathBrowseButtonEnabled = false;

            byte[]? key = ParseHexKey(this.HexKey);
            if (key != null)
            {
                Key = key;
                KeyStatus = $"Using provided Key: {this.HexKey}";
                CreateIRDButtonEnabled = ShouldEnableCreateIRDButton();
            }
            else
            {
                Key = null;
                KeyStatus = "ERROR: Invalid Key";
                CreateIRDButtonEnabled = false;
            }
        }

        /// <summary>
        /// Change the currently selected .physical path
        /// </summary>
        public void ChangePhysicalPath()
        {
            Layerbreak = null;

            if (string.IsNullOrEmpty(this.PhysicalPath))
            {
                PIC = null;
                PICStatus = "Will generate a PIC assuming a Layerbreak of 12219392";
                PICTextBoxEnabled = true;
                LayerbreakTextBoxEnabled = true;
                CreateIRDButtonEnabled = ShouldEnableCreateIRDButton();
                return;
            }

            PICTextBoxEnabled = false;
            LayerbreakTextBoxEnabled = false;

            PIC = ParsePhysical(this.PhysicalPath);
            if (PIC != null)
            {
                PICStatus = $"Using PIC from file: {Path.GetFileName(this.PhysicalPath)}";
                CreateIRDButtonEnabled = ShouldEnableCreateIRDButton();
            }
            else
            {
                if (File.Exists(this.PhysicalPath))
                    PICStatus = "ERROR: Invalid *.physical file";
                else
                    PICStatus = "ERROR: Invalid *.physical path";
                CreateIRDButtonEnabled = false;
            }
        }

        /// <summary>
        /// Change the currently selected PIC
        /// </summary>
        public void ChangePIC()
        {
            Layerbreak = null;

            if (string.IsNullOrEmpty(this.PICString))
            {
                PIC = null;
                PICStatus = "Will generate a PIC assuming a Layerbreak of 12219392";
                PhysicalPathTextBoxEnabled = true;
                PhysicalPathBrowseButtonEnabled = true;
                LayerbreakTextBoxEnabled = true;
                CreateIRDButtonEnabled = ShouldEnableCreateIRDButton();
                return;
            }
            
            PhysicalPathTextBoxEnabled = false;
            PhysicalPathBrowseButtonEnabled = false;
            LayerbreakTextBoxEnabled = false;

            PIC = ParsePIC(this.PICString);
            if (PIC != null)
            {
                PICStatus = "Using provided PIC";
                CreateIRDButtonEnabled = ShouldEnableCreateIRDButton();
            }
            else
            {
                PICStatus = "ERROR: Invalid PIC";
                CreateIRDButtonEnabled = false;
            }
        }

        /// <summary>
        /// Change the currently selected layerbreak
        /// </summary>
        public void ChangeLayerbreak()
        {
            PIC = null;

            if (string.IsNullOrEmpty(this.LayerbreakString))
            {
                Layerbreak = null;
                PICStatus = "Will generate a PIC assuming a Layerbreak of 12219392";
                PhysicalPathTextBoxEnabled = true;
                PhysicalPathBrowseButtonEnabled = true;
                PICTextBoxEnabled = true;
                CreateIRDButtonEnabled = ShouldEnableCreateIRDButton();
                return;
            }

            PhysicalPathTextBoxEnabled = false;
            PhysicalPathBrowseButtonEnabled = false;
            PICTextBoxEnabled = false;

            Layerbreak = ParseLayerbreak(this.LayerbreakString);
            if (Layerbreak != null)
            {
                PICStatus = $"Will generate a PIC using a Layerbreak of {Layerbreak}";
                CreateIRDButtonEnabled = ShouldEnableCreateIRDButton();
            }
            else
            {
                PICStatus = "ERROR: Invalid Layerbreak";
                CreateIRDButtonEnabled = false;
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
        /// <param name="key">Output 16 byte key, null if not valid</param>
        /// <returns>True if path to log file contains valid key, false otherwise</returns>
        private bool ParseLog(string? logPath, out byte[]? key, out byte[]? id, out byte[]? pic)
        {
            key = null;
            id = null;
            pic = null;

            if (string.IsNullOrEmpty(logPath))
                return false;

            if (!File.Exists(logPath))
                return false;

            // TODO: Parse log file
            //if (!LibIRD.ParseGetKeyLog(logPath, key, id, pic)
            //    return false;

            return true;
        }

        /// <summary>
        /// Validates a key file to check for presence of valid PS3 key
        /// </summary>
        /// <param name="keyPath">Path to key file</param>
        /// <param name="key">Output 16 byte key, null if not valid</param>
        /// <returns>True if path contains valid key, false otherwise</returns>
        private byte[]? ParseKeyFile(string? keyPath)
        {
            if (string.IsNullOrEmpty(keyPath))
                return null;

            if (!File.Exists(keyPath))
                return null;

            // TODO: Parse key file
            byte[] key = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

            return key;
        }

        /// <summary>
        /// Validates a hexadecimal key
        /// </summary>
        /// <param name="hexKey">String representing hexadecimal key</param>
        /// <param name="key">Output 16 byte key, null if not valid</param>
        /// <returns>True if string is a valid key, false otherwise</returns>
        private byte[]? ParseHexKey(string? hexKey)
        {
            if (string.IsNullOrEmpty(hexKey))
                return null;

            // TODO: Parse hex key
            byte[] key = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

            return key;
        }

        /// <summary>
        /// Validates a .physical path
        /// </summary>
        /// <param name="physicalPath">Path to .physical file</param>
        /// <param name="pic">Output PIC byte array, null if not valid</param>
        /// <returns>True if .physical contains a valid PIC, false otherwise</returns>
        private byte[]? ParsePhysical(string? physicalPath)
        {
            if (string.IsNullOrEmpty(physicalPath))
                return null;

            if (!File.Exists(physicalPath))
                return null;

            // TODO: Parse Physical
            byte[]? pic = [];

            return pic;
        }

        /// <summary>
        /// Validates a PIC
        /// </summary>
        /// <param name="inputPIC">String representing PIC</param>
        /// <param name="pic">Output PIC byte array, null if not valid</param>
        /// <returns>True if PIC is valid, false otherwise</returns>
        private byte[]? ParsePIC(string? inputPIC)
        {
            if (string.IsNullOrEmpty(inputPIC))
                return null;

            string cleanPIC = inputPIC!.Trim().Replace("\n", string.Empty);

            if (cleanPIC.Length < 115)
                return null;

            byte[]? pic = [];

            return pic;
        }

        /// <summary>
        /// Validates a layerbreak value (in sectors)
        /// </summary>
        /// <param name="inputLayerbreak">String representing layerbreak value</param>
        /// <param name="layerbreak">Output layerbreak value, null if not valid</param>
        /// <returns>True if layerbreak is valid, false otherwise</returns>
        private long? ParseLayerbreak(string? inputLayerbreak)
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

        /// <summary>
        /// Performs LibIRD functionality
        /// </summary>
        /// <returns>An error message if failed, otherwise string.Empty/null</returns>
        public string? CreateIRD()
        {
            if (string.IsNullOrEmpty(InputPath))
                return "Invalid ISO path.";

            if (!File.Exists(this.InputPath!.Trim('"')))
                return $"{this.InputPath!.Trim('"')} is not a valid ISO path.";

            if (Key == null)
            {
                // TODO: Pull key from redump
                Key = [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];
                // TODO: Return error if cannot pull key from redump
                //retrun "Cannot retrieve key from redump. Please manually provide a PS3 Encrypt Key."
            }

            // TODO: Create IRD using LibIRD

            // TODO: Catch exception about needing layerbreak
            //return "BD-Video hybrid disc detected. Please manually provide a PIC or Layerbreak.";

            return "";
        }

        #endregion
    }
}
