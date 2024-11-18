using System;
using System.Windows.Media;

namespace MPF.UI.Themes
{
    /// <summary>
    /// Custom colour theme
    /// </summary>
    public sealed class CustomTheme : Theme
    {
        public CustomTheme(string? backgroundColor, string? foregroundColor)
        {
            // Convert hex colors to byte values
            byte backgroundR;
            byte backgroundG;
            byte backgroundB;
            byte foregroundR;
            byte foregroundG;
            byte foregroundB;
            try
            {
                backgroundR = Convert.ToByte(backgroundColor!.Substring(0, 2), 16);
                backgroundG = Convert.ToByte(backgroundColor!.Substring(2, 2), 16);
                backgroundB = Convert.ToByte(backgroundColor!.Substring(4, 2), 16);
                foregroundR = Convert.ToByte(foregroundColor!.Substring(0, 2), 16);
                foregroundG = Convert.ToByte(foregroundColor!.Substring(2, 2), 16);
                foregroundB = Convert.ToByte(foregroundColor!.Substring(4, 2), 16);
            }
            catch
            {
                return;
            }

            // Setup needed brushes
            var backgroundBrush = new SolidColorBrush { Color = Color.FromArgb(0xff, backgroundR, backgroundG, backgroundB) };
            var foregroundBrush = new SolidColorBrush { Color = Color.FromArgb(0xff, foregroundR, foregroundG, foregroundB) };

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

            // Handle ProgressBar-specific resources
            ProgressBar_Background = backgroundBrush;

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
