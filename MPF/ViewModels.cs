using MPF.Data;

namespace MPF
{
    /// <summary>
    /// Globally referencable options
    /// </summary>
    public class OptionsViewModel
    {
        private readonly Options options;

        public bool VerboseLogging
        {
            get { return options.VerboseLogging; }
        }

        public bool EnableLogFormatting
        {
            get { return options.EnableLogFormatting; }
        }

        public bool EnableProgressProcessing
        {
            get { return options.EnableProgressProcessing; }
        }

        public OptionsViewModel(Options options)
        {
            this.options = options;
        }
    }


    public static class ViewModels
    {
        public static OptionsViewModel OptionsViewModel { get; set; }
    }
}
