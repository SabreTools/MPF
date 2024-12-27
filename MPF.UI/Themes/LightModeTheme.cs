using System.Windows;
using System.Windows.Media;

namespace MPF.UI.Themes
{
    /// <summary>
    /// Default light-mode theme
    /// </summary>
    public sealed class LightModeTheme : Theme
    {
        public LightModeTheme()
        {
            // Handle application-wide resources
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
            ComboBox_Disabled_Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xF0, 0xF0, 0xF0));
            ComboBox_Disabled_Editable_Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
            ComboBox_Disabled_Editable_Button_Background = Brushes.Transparent;
            ComboBox_MouseOver_Background = new LinearGradientBrush(
                Color.FromArgb(0xFF, 0xEC, 0xF4, 0xFC),
                Color.FromArgb(0xFF, 0xDC, 0xEC, 0xFC),
                new Point(0, 0),
                new Point(0, 1));
            ComboBox_MouseOver_Editable_Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
            ComboBox_MouseOver_Editable_Button_Background = new LinearGradientBrush(
                Color.FromArgb(0xFF, 0xEB, 0xF4, 0xFC),
                Color.FromArgb(0xFF, 0xDC, 0xEC, 0xFC),
                new Point(0, 0),
                new Point(0, 1));
            ComboBox_Pressed_Background = new LinearGradientBrush(
                Color.FromArgb(0xFF, 0xDA, 0xEC, 0xFC),
                Color.FromArgb(0xFF, 0xC4, 0xE0, 0xFC),
                new Point(0, 0),
                new Point(0, 1));
            ComboBox_Pressed_Editable_Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
            ComboBox_Pressed_Editable_Button_Background = new LinearGradientBrush(
                Color.FromArgb(0xFF, 0xDA, 0xEB, 0xFC),
                Color.FromArgb(0xFF, 0xC4, 0xE0, 0xFC),
                new Point(0, 0),
                new Point(0, 1));
            ComboBox_Static_Background = new LinearGradientBrush(
                Color.FromArgb(0xFF, 0xF0, 0xF0, 0xF0),
                Color.FromArgb(0xFF, 0xE5, 0xE5, 0xE5),
                new Point(0, 0),
                new Point(0, 1));
            ComboBox_Static_Editable_Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
            ComboBox_Static_Editable_Button_Background = Brushes.Transparent;

            // Handle CustomMessageBox-specific resources
            CustomMessageBox_Static_Background = null;

            // Handle MenuItem-specific resources
            MenuItem_SubMenu_Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xF0, 0xF0, 0xF0));
            MenuItem_SubMenu_Border = Brushes.DarkGray;

            // Handle ScrollViewer-specific resources
            ScrollViewer_ScrollBar_Background = Brushes.LightGray;
            ScrollViewer_ScrollBar_Foreground = Brushes.Gainsboro;

            // Handle TabItem-specific resources
            TabItem_Selected_Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
            TabItem_Static_Background = new LinearGradientBrush(
                Color.FromArgb(0xFF, 0xF0, 0xF0, 0xF0),
                Color.FromArgb(0xFF, 0xE5, 0xE5, 0xE5),
                new Point(0, 0),
                new Point(0, 1));
            TabItem_Static_Border = Brushes.DarkGray;

            // Handle TextBox-specific resources
            TextBox_Static_Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
        }
    }
}
