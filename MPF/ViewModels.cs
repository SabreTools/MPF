namespace MPF
{
    /// <summary>
    /// Globally referencable options
    /// </summary>
    public class OptionsViewModel
    {
        private readonly UIOptions _uiOptions;

        /// <summary>
        /// Access to the only setting needed cross-domain
        /// </summary>
        public bool VerboseLogging
        {
            get { return _uiOptions.Options.VerboseLogging; }
        }

        public OptionsViewModel(UIOptions uiOptions)
        {
            this._uiOptions = uiOptions;
        }
    }


    public static class ViewModels
    {
        public static OptionsViewModel OptionsViewModel { get; set; }
    }
}
