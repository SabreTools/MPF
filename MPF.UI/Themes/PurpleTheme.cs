using System.Windows.Media;

namespace MPF.UI.Themes
{
    /// <summary>
    /// Special purple theme
    /// </summary>
    public sealed class PurpleTheme : Theme
    {
        public PurpleTheme()
        {
            // Setup needed brushes
            var backgroundBrush = new SolidColorBrush { Color = Color.FromArgb(0xff, 0x11, 0x11, 0x11) };
            var foregroundBrush = new SolidColorBrush { Color = Color.FromArgb(0xff, 0x9A, 0x5E, 0xC0) };

            // Handle application-wide resources
            ActiveBorderBrush = foregroundBrush;
            ControlBrush = backgroundBrush;
            ControlTextBrush = foregroundBrush;
            GrayTextBrush = foregroundBrush;
            WindowBrush = backgroundBrush;
            WindowTextBrush = foregroundBrush;

            // Handle Button-specific resources
            Button_Disabled_Background = backgroundBrush;
            Button_MouseOver_Background = backgroundBrush;
            Button_Pressed_Background = backgroundBrush;
            Button_Static_Background = backgroundBrush;

            // Handle ComboBox-specific resources
            ComboBox_Disabled_Background = backgroundBrush;
            ComboBox_Disabled_Editable_Background = backgroundBrush;
            ComboBox_Disabled_Editable_Button_Background = backgroundBrush;
            ComboBox_MouseOver_Background = backgroundBrush;
            ComboBox_MouseOver_Editable_Background = backgroundBrush;
            ComboBox_MouseOver_Editable_Button_Background = backgroundBrush;
            ComboBox_Pressed_Background = backgroundBrush;
            ComboBox_Pressed_Editable_Background = backgroundBrush;
            ComboBox_Pressed_Editable_Button_Background = backgroundBrush;
            ComboBox_Static_Background = backgroundBrush;
            ComboBox_Static_Editable_Background = backgroundBrush;
            ComboBox_Static_Editable_Button_Background = backgroundBrush;

            // Handle CustomMessageBox-specific resources
            CustomMessageBox_Static_Background = backgroundBrush;

            // Handle MenuItem-specific resources
            MenuItem_SubMenu_Background = backgroundBrush;
            MenuItem_SubMenu_Border = Brushes.DarkGray;

            // Handle ScrollViewer-specific resources
            ScrollViewer_ScrollBar_Background = backgroundBrush;

            // Handle TabItem-specific resources
            TabItem_Selected_Background = backgroundBrush;
            TabItem_Static_Background = backgroundBrush;
            TabItem_Static_Border = Brushes.DarkGray;

            // Handle TextBox-specific resources
            TextBox_Static_Background = backgroundBrush;
        }
    }
}
