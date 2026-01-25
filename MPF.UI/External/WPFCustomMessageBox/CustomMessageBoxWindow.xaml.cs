using System;
using System.Drawing;
using System.Windows;
using System.Windows.Input;

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
                return TextBlock_Message!.Text;
            }
            set
            {
                TextBlock_Message!.Text = value;
            }
        }

        public string? OkButtonText
        {
            get
            {
                return Label_Ok!.Content.ToString();
            }
            set
            {
                Label_Ok!.Content = value.TryAddKeyboardAccellerator();
            }
        }

        public string? CancelButtonText
        {
            get
            {
                return Label_Cancel!.Content.ToString();
            }
            set
            {
                Label_Cancel!.Content = value.TryAddKeyboardAccellerator();
            }
        }

        public string? YesButtonText
        {
            get
            {
                return Label_Yes!.Content.ToString();
            }
            set
            {
                Label_Yes!.Content = value.TryAddKeyboardAccellerator();
            }
        }

        public string? NoButtonText
        {
            get
            {
                return Label_No!.Content.ToString();
            }
            set
            {
                Label_No!.Content = value.TryAddKeyboardAccellerator();
            }
        }

        public MessageBoxResult Result { get; set; }

#if NET35

        private Button? Button_Cancel => ItemHelper.FindChild<Button>(this, "Button_Cancel");
        private Button? Button_No => ItemHelper.FindChild<Button>(this, "Button_No");
        private Button? Button_OK => ItemHelper.FindChild<Button>(this, "Button_OK");
        private Button? Button_Yes => ItemHelper.FindChild<Button>(this, "Button_Yes");
        private System.Windows.Controls.Image? _Image_MessageBox => ItemHelper.FindChild<System.Windows.Controls.Image>(this, "Image_MessageBox");
        private Label? Label_Cancel => ItemHelper.FindChild<Label>(this, "Label_Cancel");
        private Label? Label_No => ItemHelper.FindChild<Label>(this, "Label_No");
        private Label? Label_Ok => ItemHelper.FindChild<Label>(this, "Label_Ok");
        private Label? Label_Yes => ItemHelper.FindChild<Label>(this, "Label_Yes");
        private TextBlock? TextBlock_Message => ItemHelper.FindChild<TextBlock>(this, "TextBlock_Message");

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

            if (owner is not null && owner.IsLoaded)
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
                Image_MessageBox!.Visibility = Visibility.Collapsed;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            if (_removeTitleBarIcon)
                Util.RemoveIcon(this);

            base.OnSourceInitialized(e);
        }

        private void DisplayButtons(MessageBoxButton button)
        {
#pragma warning disable IDE0010
            switch (button)
            {
                case MessageBoxButton.OKCancel:
                    // Hide all but OK, Cancel
                    Button_OK!.Visibility = Visibility.Visible;
                    Button_OK.Focus();
                    Button_Cancel!.Visibility = Visibility.Visible;

                    Button_Yes!.Visibility = Visibility.Collapsed;
                    Button_No!.Visibility = Visibility.Collapsed;
                    break;
                case MessageBoxButton.YesNo:
                    // Hide all but Yes, No
                    Button_Yes!.Visibility = Visibility.Visible;
                    Button_Yes.Focus();
                    Button_No!.Visibility = Visibility.Visible;

                    Button_OK!.Visibility = Visibility.Collapsed;
                    Button_Cancel!.Visibility = Visibility.Collapsed;
                    break;
                case MessageBoxButton.YesNoCancel:
                    // Hide only OK
                    Button_Yes!.Visibility = Visibility.Visible;
                    Button_Yes.Focus();
                    Button_No!.Visibility = Visibility.Visible;
                    Button_Cancel!.Visibility = Visibility.Visible;

                    Button_OK!.Visibility = Visibility.Collapsed;
                    break;
                default:
                    // Hide all but OK
                    Button_OK!.Visibility = Visibility.Visible;
                    Button_OK.Focus();

                    Button_Yes!.Visibility = Visibility.Collapsed;
                    Button_No!.Visibility = Visibility.Collapsed;
                    Button_Cancel!.Visibility = Visibility.Collapsed;
                    break;
            }
#pragma warning restore IDE0010
        }

        private void DisplayImage(MessageBoxImage image)
        {
#pragma warning disable IDE0072
            Icon icon = image switch
            {
                // Enumeration value 48 - also covers "Warning"
                MessageBoxImage.Exclamation => SystemIcons.Exclamation,

                // Enumeration value 16, also covers "Hand" and "Stop"
                MessageBoxImage.Error => SystemIcons.Hand,

                // Enumeration value 64 - also covers "Asterisk"
                MessageBoxImage.Information => SystemIcons.Information,

                MessageBoxImage.Question => SystemIcons.Question,
                _ => SystemIcons.Information,
            };
#pragma warning restore IDE0072

            Image_MessageBox!.Source = icon.ToImageSource();
            Image_MessageBox.Visibility = Visibility.Visible;
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
