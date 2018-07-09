using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DICUI.UI
{
    public class OptionsViewModel
    {
        private Options _options;

        public OptionsViewModel(Options options)
        {
            this._options = options;
        }

        public bool QuietMode
        {
            get { return _options.QuietMode; }
            set { _options.QuietMode = value; }
        }

        public bool ParanoidMode
        {
            get { return _options.ParanoidMode; }
            set { _options.ParanoidMode = value; }
        }

        public bool SkipMediaTypeDetection
        {
            get { return _options.SkipMediaTypeDetection; }
            set { _options.SkipMediaTypeDetection = value; }
        }

        public string RereadAmountForC2
        {
            get { return Convert.ToString(_options.RereadAmountForC2); }
            set
            {
                if (Int32.TryParse(value, out int result))
                    _options.RereadAmountForC2 = result;
            }
        }
    }

    public static class ViewModels
    {
        public static OptionsViewModel OptionsViewModel { get; set; }
    }

}
