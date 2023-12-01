using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MPF.UI.Core;

namespace WPFCustomMessageBox
{
    /// <summary>
    /// Interaction logic for ModalDialog.xaml
    /// </summary>
    internal partial class CustomMessageBoxWindow : Window
    {
        private readonly bool _removeTitleBarIcon = true;

        public string? Caption
        {
            get
            {
                return Title;
            }
            set
            {
                Title = value;
            }
        }

        public string? Message
        {
            get
            {
#if NET35
                return _TextBlock_Message!.Text;
#else
                return TextBlock_Message.Text;
#endif
            }
            set
            {
#if NET35
                _TextBlock_Message!.Text = value;
#else
                TextBlock_Message.Text = value;
#endif
            }
        }

        public string? OkButtonText
        {
            get
            {
#if NET35
                return _Label_Ok!.Content.ToString();
#else
                return Label_Ok.Content.ToString();
#endif
            }
            set
            {
#if NET35
                _Label_Ok!.Content = value.TryAddKeyboardAccellerator();
#else
                Label_Ok.Content = value.TryAddKeyboardAccellerator();
#endif
            }
        }

        public string? CancelButtonText
        {
            get
            {
#if NET35
                return _Label_Cancel!.Content.ToString();
#else
                return Label_Cancel.Content.ToString();
#endif
            }
            set
            {
#if NET35
                _Label_Cancel!.Content = value.TryAddKeyboardAccellerator();
#else
                Label_Cancel.Content = value.TryAddKeyboardAccellerator();
#endif
            }
        }

        public string? YesButtonText
        {
            get
            {
#if NET35
                return _Label_Yes!.Content.ToString();
#else
                return Label_Yes.Content.ToString();
#endif
            }
            set
            {
#if NET35
                _Label_Yes!.Content = value.TryAddKeyboardAccellerator();
#else
                Label_Yes.Content = value.TryAddKeyboardAccellerator();
#endif
            }
        }

        public string? NoButtonText
        {
            get
            {
#if NET35
                return _Label_No!.Content.ToString();
#else
                return Label_No.Content.ToString();
#endif
            }
            set
            {
#if NET35
                _Label_No!.Content = value.TryAddKeyboardAccellerator();
#else
                Label_No.Content = value.TryAddKeyboardAccellerator();
#endif
            }
        }

        public MessageBoxResult Result { get; set; }

#if NET35

        private Button? _Button_Cancel => ItemHelper.FindChild<Button>(this, "Button_Cancel");
        private Button? _Button_No => ItemHelper.FindChild<Button>(this, "Button_No");
        private Button? _Button_OK => ItemHelper.FindChild<Button>(this, "Button_OK");
        private Button? _Button_Yes => ItemHelper.FindChild<Button>(this, "Button_Yes");
        private System.Windows.Controls.Image? _Image_MessageBox => ItemHelper.FindChild<System.Windows.Controls.Image>(this, "Image_MessageBox");
        private Label? _Label_Cancel => ItemHelper.FindChild<Label>(this, "Label_Cancel");
        private Label? _Label_No => ItemHelper.FindChild<Label>(this, "Label_No");
        private Label? _Label_Ok => ItemHelper.FindChild<Label>(this, "Label_Ok");
        private Label? _Label_Yes => ItemHelper.FindChild<Label>(this, "Label_Yes");
        private TextBlock? _TextBlock_Message => ItemHelper.FindChild<TextBlock>(this, "TextBlock_Message");

#endif

        internal CustomMessageBoxWindow(Window? owner, string? message, string? caption = null, MessageBoxButton? button = null, MessageBoxImage? image = null, bool removeTitleBarIcon = true)
        {
#if NET40_OR_GREATER || NETCOREAPP
            System.Windows.Media.TextOptions.SetTextFormattingMode(this, System.Windows.Media.TextFormattingMode.Display);
            System.Windows.Media.TextOptions.SetTextRenderingMode(this, System.Windows.Media.TextRenderingMode.ClearType);
            UseLayoutRounding = true;
#endif

#if NET452_OR_GREATER || NETCOREAPP
            var chrome = new System.Windows.Shell.WindowChrome
            {
                CaptionHeight = 0,
                ResizeBorderThickness = new Thickness(0),
            };
            System.Windows.Shell.WindowChrome.SetWindowChrome(this, chrome);
#endif

#if NET40_OR_GREATER || NETCOREAPP
            InitializeComponent();
#endif

            _removeTitleBarIcon = removeTitleBarIcon;
            Focusable = true;
            ShowActivated = true;
            ShowInTaskbar = true;

            if (owner != null && owner.IsLoaded)
            {
                Owner = owner;
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }

            Message = message;
            Caption = caption;

            DisplayButtons(button ?? MessageBoxButton.OK);

            if (image.HasValue)
                DisplayImage(image.Value);
            else
#if NET35
                _Image_MessageBox!.Visibility = Visibility.Collapsed;
#else
                Image_MessageBox.Visibility = Visibility.Collapsed;
#endif
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            if (_removeTitleBarIcon)
                Util.RemoveIcon(this);

            base.OnSourceInitialized(e);
        }

        private void DisplayButtons(MessageBoxButton button)
        {
            switch (button)
            {
                case MessageBoxButton.OKCancel:
                    // Hide all but OK, Cancel
#if NET35
                    _Button_OK!.Visibility = Visibility.Visible;
                    _Button_OK.Focus();
                    _Button_Cancel!.Visibility = Visibility.Visible;

                    _Button_Yes!.Visibility = Visibility.Collapsed;
                    _Button_No!.Visibility = Visibility.Collapsed;
#else
                    Button_OK.Visibility = Visibility.Visible;
                    Button_OK.Focus();
                    Button_Cancel.Visibility = Visibility.Visible;

                    Button_Yes.Visibility = Visibility.Collapsed;
                    Button_No.Visibility = Visibility.Collapsed;
#endif
                    break;
                case MessageBoxButton.YesNo:
                    // Hide all but Yes, No
#if NET35
                    _Button_Yes!.Visibility = Visibility.Visible;
                    _Button_Yes.Focus();
                    _Button_No!.Visibility = Visibility.Visible;

                    _Button_OK!.Visibility = Visibility.Collapsed;
                    _Button_Cancel!.Visibility = Visibility.Collapsed;
#else
                    Button_Yes.Visibility = Visibility.Visible;
                    Button_Yes.Focus();
                    Button_No.Visibility = Visibility.Visible;

                    Button_OK.Visibility = Visibility.Collapsed;
                    Button_Cancel.Visibility = Visibility.Collapsed;
#endif
                    break;
                case MessageBoxButton.YesNoCancel:
                    // Hide only OK
#if NET35
                    _Button_Yes!.Visibility = Visibility.Visible;
                    _Button_Yes.Focus();
                    _Button_No!.Visibility = Visibility.Visible;
                    _Button_Cancel!.Visibility = Visibility.Visible;

                    _Button_OK!.Visibility = Visibility.Collapsed;
#else
                    Button_Yes.Visibility = Visibility.Visible;
                    Button_Yes.Focus();
                    Button_No.Visibility = Visibility.Visible;
                    Button_Cancel.Visibility = Visibility.Visible;

                    Button_OK.Visibility = Visibility.Collapsed;
#endif
                    break;
                default:
                    // Hide all but OK
#if NET35
                    _Button_OK!.Visibility = Visibility.Visible;
                    _Button_OK.Focus();

                    _Button_Yes!.Visibility = Visibility.Collapsed;
                    _Button_No!.Visibility = Visibility.Collapsed;
                    _Button_Cancel!.Visibility = Visibility.Collapsed;
#else
                    Button_OK.Visibility = Visibility.Visible;
                    Button_OK.Focus();

                    Button_Yes.Visibility = Visibility.Collapsed;
                    Button_No.Visibility = Visibility.Collapsed;
                    Button_Cancel.Visibility = Visibility.Collapsed;
#endif
                    break;
            }
        }

        private void DisplayImage(MessageBoxImage image)
        {
            Icon icon;

            switch (image)
            {
                case MessageBoxImage.Exclamation:       // Enumeration value 48 - also covers "Warning"
                    icon = SystemIcons.Exclamation;
                    break;
                case MessageBoxImage.Error:             // Enumeration value 16, also covers "Hand" and "Stop"
                    icon = SystemIcons.Hand;
                    break;
                case MessageBoxImage.Information:       // Enumeration value 64 - also covers "Asterisk"
                    icon = SystemIcons.Information;
                    break;
                case MessageBoxImage.Question:
                    icon = SystemIcons.Question;
                    break;
                default:
                    icon = SystemIcons.Information;
                    break;
            }

#if NET35
            _Image_MessageBox!.Source = icon.ToImageSource();
            _Image_MessageBox.Visibility = Visibility.Visible;
#else
            Image_MessageBox.Source = icon.ToImageSource();
            Image_MessageBox.Visibility = Visibility.Visible;
#endif
        }

        private void Button_OK_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.OK;
            Close();
        }

        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Cancel;
            Close();
        }

        private void Button_Yes_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Yes;
            Close();
        }

        private void Button_No_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.No;
            Close();
        }

        /// <summary>
        /// Handler for Title MouseDown event
        /// </summary>
        private void TitleMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}
