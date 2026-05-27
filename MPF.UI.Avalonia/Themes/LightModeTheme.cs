using Avalonia.Media;

namespace MPF.UI.Avalonia
{
    /// <summary>
    /// Default light-mode theme
    /// </summary>
    public sealed class LightModeTheme : Theme
    {
        public LightModeTheme()
        {
            // Handle application-wide resources
            // Null resets to the defaults defined in Brushes.axaml
            ActiveBorderBrush = null;
            ControlBrush = null;
            ControlTextBrush = null;
            GrayTextBrush = null;
            WindowBrush = null;
            WindowTextBrush = null;

            // Handle Button-specific resources
            Button_Disabled_Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xF4, 0xF4, 0xF4));
            Button_MouseOver_Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xBE, 0xE6, 0xFD));
            Button_Pressed_Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xC4, 0xE5, 0xF6));
            Button_Static_Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xDD, 0xDD, 0xDD));

            // Handle ComboBox-specific resources
            // WPF used LinearGradientBrush for several of these; Brushes.axaml already
            // flattened them to SolidColorBrush using the gradient start colour, so we
            // match that approach here for consistency.
            ComboBox_Disabled_Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xF0, 0xF0, 0xF0));
            ComboBox_Disabled_Editable_Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
            ComboBox_Disabled_Editable_Button_Background = new SolidColorBrush(Colors.Transparent);
            ComboBox_MouseOver_Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xEC, 0xF4, 0xFC));
            ComboBox_MouseOver_Editable_Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
            ComboBox_MouseOver_Editable_Button_Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xEB, 0xF4, 0xFC));
            ComboBox_Pressed_Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xDA, 0xEC, 0xFC));
            ComboBox_Pressed_Editable_Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
            ComboBox_Pressed_Editable_Button_Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xDA, 0xEB, 0xFC));
            ComboBox_Static_Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xF0, 0xF0, 0xF0));
            ComboBox_Static_Editable_Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
            ComboBox_Static_Editable_Button_Background = new SolidColorBrush(Colors.Transparent);

            // Handle CustomMessageBox-specific resources
            CustomMessageBox_Static_Background = null;

            // Handle MenuItem-specific resources
            MenuItem_SubMenu_Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xF0, 0xF0, 0xF0));
            MenuItem_SubMenu_Border = new SolidColorBrush(Colors.DarkGray);

            // Handle ScrollViewer-specific resources
            ScrollViewer_ScrollBar_Background = new SolidColorBrush(Colors.LightGray);

            // Handle TabItem-specific resources
            TabItem_Selected_Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
            // WPF used a LinearGradientBrush (#FFF0F0F0 → #FFE5E5E5); use start colour
            TabItem_Static_Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xF0, 0xF0, 0xF0));
            TabItem_Static_Border = new SolidColorBrush(Colors.DarkGray);

            // Handle TextBox-specific resources
            TextBox_Static_Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
        }
    }
}
