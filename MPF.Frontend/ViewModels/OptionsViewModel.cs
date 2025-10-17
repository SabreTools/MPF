using System.Collections.Generic;
using System.ComponentModel;
using MPF.Frontend.ComboBoxItems;
using LogCompression = MPF.Processors.LogCompression;
using RedumperDriveType = MPF.ExecutionContexts.Redumper.DriveType;
using RedumperReadMethod = MPF.ExecutionContexts.Redumper.ReadMethod;
using RedumperSectorOrder = MPF.ExecutionContexts.Redumper.SectorOrder;

namespace MPF.Frontend.ViewModels
{
    /// <summary>
    /// Constructor
    /// </summary>
    public class OptionsViewModel : INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        /// Title for the window
        /// </summary>
        public string? Title
        {
            get => _title;
            set
            {
                _title = value;
                TriggerPropertyChanged(nameof(Title));
            }
        }
        private string? _title;

        /// <summary>
        /// Current set of options
        /// </summary>
        public Options Options { get; }

        /// <summary>
        /// Flag for if settings were saved or not
        /// </summary>
        public bool SavedSettings { get; set; }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region Lists

        /// <summary>
        /// List of available internal programs
        /// </summary>
        public static List<Element<InternalProgram>> InternalPrograms => PopulateInternalPrograms();

        /// <summary>
        /// List of available UI languages
        /// </summary>
        public static List<Element<InterfaceLanguage>> UILanguages => Element<InterfaceLanguage>.GenerateElements();

        /// <summary>
        /// List of available log compression methods
        /// </summary>
        public static List<Element<LogCompression>> LogCompressions => Element<LogCompression>.GenerateElements();

        /// <summary>
        /// Current list of supported Redumper read methods
        /// </summary>
        public static List<Element<RedumperReadMethod>> RedumperReadMethods => Element<RedumperReadMethod>.GenerateElements();

        /// <summary>
        /// Current list of supported Redumper sector orders
        /// </summary>
        public static List<Element<RedumperSectorOrder>> RedumperSectorOrders => Element<RedumperSectorOrder>.GenerateElements();

        /// <summary>
        /// Current list of supported Redumper drive types
        /// </summary>
        public static List<Element<RedumperDriveType>> RedumperDriveTypes => Element<RedumperDriveType>.GenerateElements();

        /// <summary>
        /// Current list of supported system profiles
        /// </summary>
        public static List<RedumpSystemComboBoxItem> Systems => RedumpSystemComboBoxItem.GenerateElements();

        #endregion

        /// <summary>
        /// Constructor for pure view model
        /// </summary>
        public OptionsViewModel()
        {
            Options = new Options();
        }

        /// <summary>
        /// Constructor for in-code
        /// </summary>
        public OptionsViewModel(Options baseOptions)
        {
            Options = new Options(baseOptions);
        }

        #region Population

        /// <summary>
        /// Get a complete list of supported internal programs
        /// </summary>
        private static List<Element<InternalProgram>> PopulateInternalPrograms()
        {
            var internalPrograms = new List<InternalProgram> { InternalProgram.Redumper, InternalProgram.DiscImageCreator, InternalProgram.Aaru };
            return internalPrograms.ConvertAll(ip => new Element<InternalProgram>(ip));
        }

        #endregion

        #region UI Commands

        /// <summary>
        /// Get the human-readable result for a Redump login result
        /// </summary>
        public static string GetRedumpLoginResult(bool? success)
        {
            return success switch
            {
                true => "Redump username and password accepted!",
                false => "Redump username and password denied!",
                null => "An error occurred validating your credentials!",
            };
        }

        /// <summary>
        /// Reset Redumper non-redump options (Read Method, Sector Order, Drive Type)
        /// </summary>
        public void NonRedumpModeUnChecked()
        {
            Options.RedumperReadMethod = RedumperReadMethod.NONE;
            Options.RedumperSectorOrder = RedumperSectorOrder.NONE;
            Options.RedumperDriveType = RedumperDriveType.NONE;
            TriggerPropertyChanged(nameof(Options));
        }

        #endregion

        #region Property Updates

        /// <summary>
        /// Trigger a property changed event
        /// </summary>
        private void TriggerPropertyChanged(string propertyName)
        {
            // If the property change event is initialized
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
