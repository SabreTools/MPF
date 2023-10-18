using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using MPF.Core.Data;
using MPF.Core.UI.ComboBoxItems;
using SabreTools.RedumpLib.Web;

namespace MPF.Core.UI.ViewModels
{
    public class OptionsViewModel : INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        /// Title for the window
        /// </summary>
#if NET48
        public string Title
#else
        public string? Title
#endif
        {
            get => _title;
            set
            {
                _title = value;
                TriggerPropertyChanged(nameof(Title));
            }
        }
#if NET48
        private string _title;
#else
        private string? _title;
#endif

        /// <summary>
        /// Current set of options
        /// </summary>
        public Options Options { get; }

        /// <summary>
        /// Flag for if settings were saved or not
        /// </summary>
        public bool SavedSettings { get; set; }

        /// <inheritdoc/>
#if NET48
        public event PropertyChangedEventHandler PropertyChanged;
#else
        public event PropertyChangedEventHandler? PropertyChanged;
#endif

#endregion

        #region Lists

        /// <summary>
        /// List of available internal programs
        /// </summary>
        public List<Element<InternalProgram>> InternalPrograms => PopulateInternalPrograms();

        /// <summary>
        /// Current list of supported system profiles
        /// </summary>
        public List<RedumpSystemComboBoxItem> Systems => RedumpSystemComboBoxItem.GenerateElements().ToList();

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public OptionsViewModel(Options baseOptions)
        {
            Options = new Options(baseOptions);
        }

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
#if NET48
        public (bool?, string) TestRedumpLogin(string username, string password)
#else
        public async Task<(bool?, string?)> TestRedumpLogin(string username, string password)
#endif
        {
#if NET48
            return RedumpWebClient.ValidateCredentials(username, password);
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
