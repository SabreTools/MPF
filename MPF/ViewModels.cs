namespace MPF
{
    /// <summary>
    /// Globally referencable options
    /// </summary>
    public class OptionsViewModel
    {
        private readonly UIOptions _uiOptions;

        public bool VerboseLogging
        {
            get { return _uiOptions.Options.VerboseLogging; }
            set
            {
                _uiOptions.Options.VerboseLogging = value;
                _uiOptions.Save(); // TODO: Why does this save here?
            }
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
