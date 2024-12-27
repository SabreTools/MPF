using System.Windows.Media;

namespace MPF.UI.Themes
{
    /// <summary>
    /// Default dark-mode theme
    /// </summary>
    public sealed class DarkModeTheme : Theme
    {
        public DarkModeTheme()
        {
            // Setup needed brushes
            var darkModeBrush = new SolidColorBrush { Color = Color.FromArgb(0xff, 0x20, 0x20, 0x20) };

            // Handle application-wide resources
            ActiveBorderBrush = Brushes.Black;
            ControlBrush = darkModeBrush;
            ControlTextBrush = Brushes.White;
            GrayTextBrush = Brushes.DarkGray;
            WindowBrush = darkModeBrush;
            WindowTextBrush = Brushes.White;

            // Handle Button-specific resources
            Button_Disabled_Background = darkModeBrush;
            Button_MouseOver_Background = darkModeBrush;
            Button_Pressed_Background = darkModeBrush;
            Button_Static_Background = darkModeBrush;

            // Handle ComboBox-specific resources
            ComboBox_Disabled_Background = darkModeBrush;
            ComboBox_Disabled_Editable_Background = darkModeBrush;
            ComboBox_Disabled_Editable_Button_Background = darkModeBrush;
            ComboBox_MouseOver_Background = darkModeBrush;
            ComboBox_MouseOver_Editable_Background = darkModeBrush;
            ComboBox_MouseOver_Editable_Button_Background = darkModeBrush;
            ComboBox_Pressed_Background = darkModeBrush;
            ComboBox_Pressed_Editable_Background = darkModeBrush;
            ComboBox_Pressed_Editable_Button_Background = darkModeBrush;
            ComboBox_Static_Background = darkModeBrush;
            ComboBox_Static_Editable_Background = darkModeBrush;
            ComboBox_Static_Editable_Button_Background = darkModeBrush;

            // Handle CustomMessageBox-specific resources
            CustomMessageBox_Static_Background = darkModeBrush;

            // Handle MenuItem-specific resources
            MenuItem_SubMenu_Background = darkModeBrush;
            MenuItem_SubMenu_Border = Brushes.DarkGray;

            // Handle ScrollViewer-specific resources
            ScrollViewer_ScrollBar_Background = darkModeBrush;
            ScrollViewer_ScrollBar_Foreground = Brushes.Gray;

            // Handle TabItem-specific resources
            TabItem_Selected_Background = darkModeBrush;
            TabItem_Static_Background = darkModeBrush;
            TabItem_Static_Border = Brushes.DarkGray;

            // Handle TextBox-specific resources
            TextBox_Static_Background = darkModeBrush;
        }
    }
}
