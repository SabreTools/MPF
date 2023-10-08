using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MPF.Core.Data;
using MPF.Core.UI.ComboBoxItems;
using SabreTools.RedumpLib.Web;

namespace MPF.Core.UI.ViewModels
{
    public class OptionsViewModel
    {
        #region Fields

        /// <summary>
        /// Current set of options
        /// </summary>
        public Options Options { get; }

        /// <summary>
        /// Flag for if settings were saved or not
        /// </summary>
        public bool SavedSettings { get; set; }

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
    }
}
