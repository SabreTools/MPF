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
        }

        public bool EnableLogFormatting
        {
            get { return _uiOptions.Options.EnableLogFormatting; }
        }

        public bool EnableProgressProcessing
        {
            get { return _uiOptions.Options.EnableProgressProcessing; }
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
