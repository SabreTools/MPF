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
        public Options Options
        {
            get => _options;
        }
        private readonly Options _options;

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
            get;
            set
            {
                field = value;
                TriggerPropertyChanged(nameof(CancelButtonEnabled));
            }
        } = true;

        /// <summary>
        /// Indicates the status of the check dump button
        /// </summary>
        public bool CreateIRDButtonEnabled
        {
            get;
            set
            {
                field = value;
                TriggerPropertyChanged(nameof(CreateIRDButtonEnabled));
            }
        } = false;

        /// <summary>
        /// Current Create IRD status message
        /// </summary>
        public string CreateIRDStatus
        {
            get;
            set
            {
                field = value;
                TriggerPropertyChanged(nameof(CreateIRDStatus));
            }
        } = "Please provide an ISO";

        /// <summary>
        /// Currently provided Disc ID
        /// </summary>
        public byte[]? DiscID
        {
            get;
            set
            {
                field = value;
                TriggerPropertyChanged(nameof(DiscID));
            }
        } = null;

        /// <summary>
        /// Current disc ID status message
        /// </summary>
        public string DiscIDStatus
        {
            get;
            set
            {
                field = value;
                TriggerPropertyChanged(nameof(DiscIDStatus));
            }
        } = "Unknown Disc ID, generating ID using Region: NONE";

        /// <summary>
        /// Currently provided Disc ID string
        /// </summary>
        public string? DiscIDString
        {
            get;
            set
            {
                field = value;
                TriggerPropertyChanged(nameof(DiscIDString));
            }
        } = string.Empty;

        /// <summary>
        /// Indicates the status of the disc ID text box
        /// </summary>
        public bool DiscIDTextBoxEnabled
        {
            get;
            set
            {
                field = value;
                TriggerPropertyChanged(nameof(DiscIDTextBoxEnabled));
            }
        } = true;

        /// <summary>
        /// Currently provided hexadecimal key
        /// </summary>
        public string? HexKey
        {
            get;
            set
            {
                field = value;
                TriggerPropertyChanged(nameof(HexKey));
            }
        } = string.Empty;

        /// <summary>
        /// Indicates the status of the hex key text box
        /// </summary>
        public bool HexKeyTextBoxEnabled
        {
            get;
            set
            {
                field = value;
                TriggerPropertyChanged(nameof(HexKeyTextBoxEnabled));
            }
        } = true;

        /// <summary>
        /// Currently provided input path
        /// </summary>
        public string? InputPath
        {
            get;
            set
            {
                field = value;
                TriggerPropertyChanged(nameof(InputPath));
            }
        } = string.Empty;

        /// <summary>
        /// Indicates the status of the input path browse button
        /// </summary>
        public bool InputPathBrowseButtonEnabled
        {
            get;
            set
            {
                field = value;
                TriggerPropertyChanged(nameof(InputPathBrowseButtonEnabled));
            }
        } = true;

        /// <summary>
        /// Indicates the status of the input path text box
        /// </summary>
        public bool InputPathTextBoxEnabled
        {
            get;
            set
            {
                field = value;
                TriggerPropertyChanged(nameof(InputPathTextBoxEnabled));
            }
        } = true;

        /// <summary>
        /// Currently provided key
        /// </summary>
        public byte[]? Key
        {
            get;
            set
            {
                field = value;
                TriggerPropertyChanged(nameof(Key));
            }
        } = null;

        /// <summary>
        /// Currently provided key path
        /// </summary>
        public string? KeyPath
        {
            get;
            set
            {
                field = value;
                TriggerPropertyChanged(nameof(KeyPath));
            }
        } = string.Empty;

        /// <summary>
        /// Indicates the status of the key path browse button
        /// </summary>
        public bool KeyPathBrowseButtonEnabled
        {
            get;
            set
            {
                field = value;
                TriggerPropertyChanged(nameof(KeyPathBrowseButtonEnabled));
            }
        } = true;

        /// <summary>
        /// Indicates the status of the key path text box
        /// </summary>
        public bool KeyPathTextBoxEnabled
        {
            get;
            set
            {
                field = value;
                TriggerPropertyChanged(nameof(KeyPathTextBoxEnabled));
            }
        } = true;

        /// <summary>
        /// Current key status message
        /// </summary>
        public string KeyStatus
        {
            get;
            set
            {
                field = value;
                TriggerPropertyChanged(nameof(KeyStatus));
            }
        } = "Cannot create an IRD without a key"; // "Will attempt to pull Encryption Key from redump.info"

        /// <summary>
        /// Currently provided layerbreak
        /// </summary>
        public long? Layerbreak
        {
            get;
            set
            {
                field = value;
                TriggerPropertyChanged(nameof(Layerbreak));
            }
        }

        /// <summary>
        /// Currently provided layerbreak string
        /// </summary>
        public string? LayerbreakString
        {
            get;
            set
            {
                field = value;
                TriggerPropertyChanged(nameof(LayerbreakString));
            }
        } = string.Empty;

        /// <summary>
        /// Indicates the status of the layerbreak text box
        /// </summary>
        public bool LayerbreakTextBoxEnabled
        {
            get;
            set
            {
                field = value;
                TriggerPropertyChanged(nameof(LayerbreakTextBoxEnabled));
            }
        } = true;

        /// <summary>
        /// Currently provided .getkey.log path
        /// </summary>
        public string? LogPath
        {
            get;
            set
            {
                field = value;
                TriggerPropertyChanged(nameof(LogPath));
            }
        } = string.Empty;

        /// <summary>
        /// Indicates the status of the .getkey.log path browse button
        /// </summary>
        public bool LogPathBrowseButtonEnabled
        {
            get;
            set
            {
                field = value;
                TriggerPropertyChanged(nameof(LogPathBrowseButtonEnabled));
            }
        } = true;

        /// <summary>
        /// Indicates whether a .getkey.log path is not provided
        /// </summary>
        public bool LogPathNotProvided
        {
            get;
            set
            {
                field = value;
                TriggerPropertyChanged(nameof(LogPathNotProvided));
            }
        } = true;

        /// <summary>
        /// Indicates the status of the .getkey.log path text box
        /// </summary>
        public bool LogPathTextBoxEnabled
        {
            get;
            set
            {
                field = value;
                TriggerPropertyChanged(nameof(LogPathTextBoxEnabled));
            }
        } = true;

        /// <summary>
        /// Currently provided PIC file path
        /// </summary>
        public string? PICPath
        {
            get;
            set
            {
                field = value;
                TriggerPropertyChanged(nameof(PICPath));
            }
        } = string.Empty;

        /// <summary>
        /// Indicates the status of the PIC file path browse button
        /// </summary>
        public bool PICPathBrowseButtonEnabled
        {
            get;
            set
            {
                field = value;
                TriggerPropertyChanged(nameof(PICPathBrowseButtonEnabled));
            }
        } = true;

        /// <summary>
        /// Indicates the status of the PIC file path text box
        /// </summary>
        public bool PICPathTextBoxEnabled
        {
            get;
            set
            {
                field = value;
                TriggerPropertyChanged(nameof(PICPathTextBoxEnabled));
            }
        } = true;

        /// <summary>
        /// Currently provided PIC
        /// </summary>
        public byte[]? PIC
        {
            get;
            set
            {
                field = value;
                TriggerPropertyChanged(nameof(PIC));
            }
        } = null;

        /// <summary>
        /// Current PIC status message
        /// </summary>
        public string PICStatus
        {
            get;
            set
            {
                field = value;
                TriggerPropertyChanged(nameof(PICStatus));
            }
        } = "Will generate a PIC assuming a Layerbreak of 12219392";

        /// <summary>
        /// Currently provided PIC string
        /// </summary>
        public string? PICString
        {
            get;
            set
            {
                field = value;
                TriggerPropertyChanged(nameof(PICString));
            }
        } = string.Empty;

        /// <summary>
        /// Indicates the status of the PIC text box
        /// </summary>
        public bool PICTextBoxEnabled
        {
            get;
            set
            {
                field = value;
                TriggerPropertyChanged(nameof(PICTextBoxEnabled));
            }
        } = true;

        #endregion

        /// <summary>
        /// Constructor for pure view model
        /// </summary>
        public CreateIRDViewModel()
        {
            _options = OptionsLoader.LoadFromConfig(out _);

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
                //KeyStatus = "Will attempt to pull Encryption Key from redump.info"; // Use this when redump key pulling is implemented
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
                //KeyStatus = "Will attempt to pull Encryption Key from redump.info"; // Use this when redump key pulling is implemented
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
                //KeyStatus = "Will attempt to pull Encryption Key from redump.info"; // Use this when redump key pulling is implemented
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
            //_keyStatus = "Will attempt to pull Encryption Key from redump.info";
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

            // TODO: Implement pulling key from redump.info
            if (Key is null)
                return "Pulling key from redump.info is currently not implemented.";

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
