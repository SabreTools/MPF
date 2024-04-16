using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using MPF.Core.Data;
using MPF.Core.UI.ComboBoxItems;
using SabreTools.RedumpLib.Web;

namespace MPF.Core.UI.ViewModels
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
        /// Current list of supported Redumper read methods
        /// </summary>
        public static List<Element<RedumperReadMethod>> RedumperReadMethods => PopulateRedumperReadMethods();

        /// <summary>
        /// Current list of supported Redumper sector orders
        /// </summary>
        public static List<Element<RedumperSectorOrder>> RedumperSectorOrders => PopulateRedumperSectorOrders();

        /// <summary>
        /// Current list of supported system profiles
        /// </summary>
        public static List<RedumpSystemComboBoxItem> Systems => RedumpSystemComboBoxItem.GenerateElements().ToList();

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
            return internalPrograms.Select(ip => new Element<InternalProgram>(ip)).ToList();
        }

        /// <summary>
        /// Get a complete list of supported redumper drive read methods
        /// </summary>
        private static List<Element<RedumperReadMethod>> PopulateRedumperReadMethods()
        {
            var readMethods = new List<RedumperReadMethod> { RedumperReadMethod.NONE, RedumperReadMethod.D8, RedumperReadMethod.BE, RedumperReadMethod.BE_CDDA };
            return readMethods.Select(rm => new Element<RedumperReadMethod>(rm)).ToList();
        }

        /// <summary>
        /// Get a complete list of supported redumper drive sector orders
        /// </summary>
        private static List<Element<RedumperSectorOrder>> PopulateRedumperSectorOrders()
        {
            var sectorOrders = new List<RedumperSectorOrder> { RedumperSectorOrder.NONE, RedumperSectorOrder.DATA_C2_SUB, RedumperSectorOrder.DATA_SUB_C2, RedumperSectorOrder.DATA_SUB, RedumperSectorOrder.DATA_C2 };
            return sectorOrders.Select(so => new Element<RedumperSectorOrder>(so)).ToList();
        }

        #endregion

        #region UI Commands

        /// <summary>
        /// Test Redump login credentials
        /// </summary>
#if NET40
        public static Task<(bool?, string?)> TestRedumpLogin(string username, string password)
#else
        public static async Task<(bool?, string?)> TestRedumpLogin(string username, string password)
#endif
        {
#if NET40
            return Task.Factory.StartNew(() => RedumpWebClient.ValidateCredentials(username, password));
#elif NETFRAMEWORK
            return await Task.Run(() => RedumpWebClient.ValidateCredentials(username, password));
#else
            return await RedumpHttpClient.ValidateCredentials(username, password);
#endif
        }

        /// <summary>
        /// Reset Redumper non-redump options (Read Method, Sector Order, Drive Type)
        /// </summary>
        public void NonRedumpModeUnChecked()
        {
            Options.RedumperReadMethod = RedumperReadMethod.NONE;
            Options.RedumperSectorOrder = RedumperSectorOrder.NONE;
            Options.RedumperUseGenericDriveType = false;
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
