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
    public class OptionsViewModel(Options baseOptions) : INotifyPropertyChanged
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
        public Options Options { get; } = new Options(baseOptions);

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
        /// Current list of supported system profiles
        /// </summary>
        public static List<RedumpSystemComboBoxItem> Systems => RedumpSystemComboBoxItem.GenerateElements().ToList();

        #endregion

        #region Population

        /// <summary>
        /// Get a complete list of  supported internal programs
        /// </summary>
        private static List<Element<InternalProgram>> PopulateInternalPrograms()
        {
            var internalPrograms = new List<InternalProgram> { InternalProgram.DiscImageCreator, InternalProgram.Aaru, InternalProgram.Redumper };
            return internalPrograms.Select(ip => new Element<InternalProgram>(ip)).ToList();
        }

        #endregion

        #region UI Commands

        /// <summary>
        /// Test Redump login credentials
        /// </summary>
        public static async Task<(bool?, string?)> TestRedumpLogin(string username, string password)
        {
#if NET40
            return await Task.Factory.StartNew(() => RedumpWebClient.ValidateCredentials(username, password));
#elif NETFRAMEWORK
            return await Task.Run(() => RedumpWebClient.ValidateCredentials(username, password));
#else
            return await RedumpHttpClient.ValidateCredentials(username, password);
#endif
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
