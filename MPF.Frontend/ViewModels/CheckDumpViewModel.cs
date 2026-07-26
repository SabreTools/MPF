#if !(NET20 || NET35 || NET40)
using System;
#endif
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using BinaryObjectScanner;
using MPF.Frontend.ComboBoxItems;
using MPF.Frontend.Tools;
using SabreTools.RedumpLib.Data;

namespace MPF.Frontend.ViewModels
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
        /// Currently selected system value
        /// </summary>
        public PhysicalSystem? CurrentSystem
        {
            get;
            set
            {
                field = value;
                TriggerPropertyChanged(nameof(CurrentSystem));
            }
        }

        /// <summary>
        /// Indicates the status of the system type combo box
        /// </summary>
        public bool SystemTypeComboBoxEnabled
        {
            get;
            set
            {
                field = value;
                TriggerPropertyChanged(nameof(SystemTypeComboBoxEnabled));
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
        /// Currently selected dumping program
        /// </summary>
        public InternalProgram CurrentProgram
        {
            get;
            set
            {
                field = value;
                TriggerPropertyChanged(nameof(CurrentProgram));
            }
        }

        /// <summary>
        /// Indicates the status of the dumping program combo box
        /// </summary>
        public bool DumpingProgramComboBoxEnabled
        {
            get;
            set
            {
                field = value;
                TriggerPropertyChanged(nameof(DumpingProgramComboBoxEnabled));
            }
        } = true;

        /// <summary>
        /// Currently displayed status
        /// </summary>
        public string Status
        {
            get;
            set
            {
                field = value;
                TriggerPropertyChanged(nameof(Status));
                TriggerPropertyChanged(nameof(StatusFirstLine));
            }
        } = string.Empty;

        /// <summary>
        /// Currently displayed status trimmed to one line
        /// </summary>
        public string StatusFirstLine
        {
            get
            {
                if (string.IsNullOrEmpty(Status))
                    return string.Empty;

                var statusLines = Status.Split('\n');
                if (statusLines.Length > 1)
                    return statusLines[0] + " (...)";

                return statusLines[0];
            }
        }

        /// <summary>
        /// Indicates the status of the check dump button
        /// </summary>
        public bool CheckDumpButtonEnabled
        {
            get;
            set
            {
                field = value;
                TriggerPropertyChanged(nameof(CheckDumpButtonEnabled));
            }
        } = false;

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

        #endregion

        #region List Properties

        /// <summary>
        /// Current list of supported system profiles
        /// </summary>
        public List<PhysicalSystemComboBoxItem> Systems
        {
            get;
            set
            {
                field = value;
                TriggerPropertyChanged(nameof(Systems));
            }
        } = PhysicalSystemComboBoxItem.GenerateElements();

        /// <summary>
        /// List of available internal programs
        /// </summary>
        public List<Element<InternalProgram>> AvailableInternalPrograms
        {
            get;
            set
            {
                field = value;
                TriggerPropertyChanged(nameof(AvailableInternalPrograms));
            }
        } = [];

        #endregion

        #region Constants

        /// <summary>
        /// Set of check-supported programs
        /// </summary>
        private static readonly List<InternalProgram> CheckablePrograms =
        [
            InternalProgram.Redumper,
            InternalProgram.Aaru,
            InternalProgram.DiscImageCreator,
            InternalProgram.Dreamdump,
            InternalProgram.CleanRip,
            InternalProgram.PS3CFW,
            InternalProgram.UmdImageCreator,
            InternalProgram.XboxBackupCreator,
            InternalProgram.Generic,
        ];

        #endregion

        /// <summary>
        /// Constructor for pure view model
        /// </summary>
        public CheckDumpViewModel()
        {
            _options = OptionsLoader.LoadFromConfig(out _);

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
            if (cachedCanExecuteSelectionChanged)
                EnableEventHandlers();
        }

        #endregion

        #region UI Commands

        /// <summary>
        /// Change the currently selected system
        /// </summary>
        public void ChangeSystem()
        {
            CheckDumpButtonEnabled = ShouldEnableCheckDumpButton();
        }

        /// <summary>
        /// Change the currently selected dumping program
        /// </summary>
        public void ChangeDumpingProgram()
        {
            CheckDumpButtonEnabled = ShouldEnableCheckDumpButton();
        }

        /// <summary>
        /// Change the currently selected input path
        /// </summary>
        public void ChangeInputPath()
        {
            CheckDumpButtonEnabled = ShouldEnableCheckDumpButton();
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
            DumpingProgramComboBoxEnabled = false;
            CheckDumpButtonEnabled = false;
            CancelButtonEnabled = false;
        }

        #endregion

        #region Population

        /// <summary>
        /// Populate media type according to system type
        /// </summary>
        private void PopulateInternalPrograms()
        {
            // Disable other UI updates
            bool cachedCanExecuteSelectionChanged = CanExecuteSelectionChanged;
            DisableEventHandlers();

            // Get the current internal program
            InternalProgram internalProgram = Options.InternalProgram;

            // Create a static list of supported Check programs, not everything
            AvailableInternalPrograms = CheckablePrograms.ConvertAll(ip => new Element<InternalProgram>(ip));

            // Select the current default dumping program
            int currentIndex = AvailableInternalPrograms.FindIndex(m => m == internalProgram);
            CurrentProgram = currentIndex > -1 ? AvailableInternalPrograms[currentIndex].Value : AvailableInternalPrograms[0].Value;

            // Reenable event handlers, if necessary
            if (cachedCanExecuteSelectionChanged)
                EnableEventHandlers();
        }

        #endregion

        #region UI Functionality

        private bool ShouldEnableCheckDumpButton()
        {
            return CurrentSystem is not null && !string.IsNullOrEmpty(InputPath);
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
        public async Task<ResultEventArgs> CheckDump(ProcessUserInfoDelegate processUserInfo)
        {
            if (string.IsNullOrEmpty(InputPath))
                return ResultEventArgs.Failure("Invalid Input path");

            if (!File.Exists(InputPath!.Trim('"')))
                return ResultEventArgs.Failure("Input Path is not a valid file");

            // Disable UI while Check is running
            DisableUIElements();
            bool cachedCanExecuteSelectionChanged = CanExecuteSelectionChanged;
            DisableEventHandlers();

            // Get progress indicators
            var resultProgress = new Progress<ResultEventArgs>();
            resultProgress.ProgressChanged += ProgressUpdated;
            var protectionProgress = new Progress<ProtectionProgress>();
            protectionProgress.ProgressChanged += ProgressUpdated;

            // Populate an environment
            var env = new DumpEnvironment(Options,
                Path.GetFullPath(InputPath.Trim('"')),
                null,
                CurrentSystem,
                CurrentProgram);
            env.SetProcessor();

            // Finally, attempt to do the output dance
            var result = await env.VerifyAndSaveDumpOutput(
                resultProgress: resultProgress,
                protectionProgress: protectionProgress,
                processUserInfo: processUserInfo);

            // Reenable UI and event handlers, if necessary
            EnableUIElements();
            if (cachedCanExecuteSelectionChanged)
                EnableEventHandlers();

            return result;
        }

        /// <summary>
        /// Handler for Result ProgressChanged event
        /// </summary>
        private void ProgressUpdated(object? sender, ResultEventArgs value)
        {
            Status = value?.Message ?? string.Empty;
        }

        /// <summary>
        /// Handler for ProtectionProgress ProgressChanged event
        /// </summary>
        private void ProgressUpdated(object? sender, ProtectionProgress value)
        {
            string message = $"{value.Percentage * 100:N2}%: {value.Filename} - {value.Protection}";
            Status = message;
        }

        #endregion
    }
}
