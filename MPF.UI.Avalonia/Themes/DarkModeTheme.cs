// MPF cross-platform (Avalonia) UI — contributed by Knutwurst (https://github.com/knutwurst)
using Avalonia.Media;

namespace MPF.UI.Avalonia
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
            ActiveBorderBrush = new SolidColorBrush(Colors.Black);
            ControlBrush = darkModeBrush;
            ControlTextBrush = new SolidColorBrush(Colors.White);
            GrayTextBrush = new SolidColorBrush(Colors.DarkGray);
            WindowBrush = darkModeBrush;
            WindowTextBrush = new SolidColorBrush(Colors.White);

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
            MenuItem_SubMenu_Border = new SolidColorBrush(Colors.DarkGray);

            // Handle ScrollViewer-specific resources
            ScrollViewer_ScrollBar_Background = darkModeBrush;

            // Handle TabItem-specific resources
            TabItem_Selected_Background = darkModeBrush;
            TabItem_Static_Background = darkModeBrush;
            TabItem_Static_Border = new SolidColorBrush(Colors.DarkGray);

            // Handle TextBox-specific resources
            TextBox_Static_Background = darkModeBrush;
        }
    }
}
