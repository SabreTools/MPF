using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

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

    public class LoggerViewModel
    {
        private readonly LogWindow _logWindow;

        public LoggerViewModel(LogWindow logWindow) => _logWindow = logWindow;

        public bool Verbose { get; set; } = true;

        public void VerboseLog(string text) => _logWindow.AppendToTextBox(text, Brushes.Yellow);
        public void VerboseLog(string format, params object[] args) => VerboseLog(string.Format(format, args));
        public void VerboseLogLn(string format, params object[] args) => VerboseLog(string.Format(format, args) + "\n");

    }

    public static class ViewModels
    {
        public static OptionsViewModel OptionsViewModel { get; set; }
        public static LoggerViewModel LoggerViewModel { get; set; }
    }

}
