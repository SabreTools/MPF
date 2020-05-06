using System;
using System.Windows.Media;
using DICUI.Windows;

namespace DICUI
{
    public class OptionsViewModel
    {
        private UIOptions _uiOptions;

        public OptionsViewModel(UIOptions uiOptions)
        {
            this._uiOptions = uiOptions;
        }

        public string InternalProgram
        {
            get { return _uiOptions.Options.InternalProgram; }
            set { _uiOptions.Options.InternalProgram = value; }
        }

        public bool QuietMode
        {
            get { return _uiOptions.Options.QuietMode; }
            set { _uiOptions.Options.QuietMode = value; }
        }

        public bool ParanoidMode
        {
            get { return _uiOptions.Options.ParanoidMode; }
            set { _uiOptions.Options.ParanoidMode = value; }
        }

        public bool ScanForProtection
        {
            get { return _uiOptions.Options.ScanForProtection; }
            set { _uiOptions.Options.ScanForProtection = value; }
        }

        public string RereadAmountForC2
        {
            get { return Convert.ToString(_uiOptions.Options.RereadAmountForC2); }
            set
            {
                if (Int32.TryParse(value, out int result))
                    _uiOptions.Options.RereadAmountForC2 = result;
            }
        }

        public bool AddPlaceholders
        {
            get { return _uiOptions.Options.AddPlaceholders; }
            set { _uiOptions.Options.AddPlaceholders = value; }
        }

        public bool PromptForDiscInformation
        {
            get { return _uiOptions.Options.PromptForDiscInformation; }
            set { _uiOptions.Options.PromptForDiscInformation = value; }
        }

        public bool IgnoreFixedDrives
        {
            get { return _uiOptions.Options.IgnoreFixedDrives; }
            set { _uiOptions.Options.IgnoreFixedDrives = value; }
        }

        public bool ResetDriveAfterDump
        {
            get { return _uiOptions.Options.ResetDriveAfterDump; }
            set { _uiOptions.Options.ResetDriveAfterDump = value; }
        }

        public bool SkipMediaTypeDetection
        {
            get { return _uiOptions.Options.SkipMediaTypeDetection; }
            set { _uiOptions.Options.SkipMediaTypeDetection = value; }
        }

        public bool SkipSystemDetection
        {
            get { return _uiOptions.Options.SkipSystemDetection; }
            set { _uiOptions.Options.SkipSystemDetection = value; }
        }

        public bool VerboseLogging
        {
            get { return _uiOptions.Options.VerboseLogging; }
            set
            {
                _uiOptions.Options.VerboseLogging = value;
                _uiOptions.Save();
            }
        }

        public bool OpenLogWindowAtStartup
        {
            get { return _uiOptions.Options.OpenLogWindowAtStartup; }
            set
            {
                _uiOptions.Options.OpenLogWindowAtStartup = value;
                _uiOptions.Save();
            }
        }

        public string Username
        {
            get { return _uiOptions.Options.Username; }
            set { _uiOptions.Options.Username = value; }
        }

        public string Password
        {
            get { return _uiOptions.Options.Password; }
            set { _uiOptions.Options.Password = value; }
        }
    }

    public class LoggerViewModel
    {
        private LogWindow _logWindow;

        public void SetWindow(LogWindow logWindow) => _logWindow = logWindow;

        public bool WindowVisible
        {
            get => _logWindow != null ? _logWindow.IsVisible : false;
            set
            {
                if (value)
                {
                    _logWindow.AdjustPositionToMainWindow();
                    _logWindow.Show();
                }
                else
                    _logWindow.Hide();
            }
        }

        public void VerboseLog(string text)
        {
            if (ViewModels.OptionsViewModel.VerboseLogging)
                _logWindow.AppendToTextBox(text, Brushes.Yellow);
        }

        public void VerboseLog(string format, params object[] args) => VerboseLog(string.Format(format, args));
        public void VerboseLogLn(string format, params object[] args) => VerboseLog(string.Format(format, args) + "\n");
    }

    public static class ViewModels
    {
        public static OptionsViewModel OptionsViewModel { get; set; }
        public static LoggerViewModel LoggerViewModel { get; set; } = new LoggerViewModel();
    }
}
