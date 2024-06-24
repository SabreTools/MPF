using System.Windows.Media;

namespace MPF.UI.Themes
{
    /// <summary>
    /// Default purple-mode theme
    /// </summary>
    public class PurpModeTheme : Theme
    {
        public PurpModeTheme()
        {
            // Setup needed brushes
            var darkPurpBrush = new SolidColorBrush { Color = Color.FromArgb(0xff, 0x11, 0x11, 0x11) };
            var lightPurpBrush = new SolidColorBrush { Color = Color.FromArgb(0xff, 0x9a, 0x5e, 0xc0) };

            // Handle application-wide resources
            this.ActiveBorderBrush = lightPurpBrush;
            this.ControlBrush = darkPurpBrush;
            this.ControlTextBrush = lightPurpBrush;
            this.GrayTextBrush = lightPurpBrush;
            this.WindowBrush = darkPurpBrush;
            this.WindowTextBrush = lightPurpBrush;

            // Handle Button-specific resources
            this.Button_Disabled_Background = darkPurpBrush;
            this.Button_MouseOver_Background = darkPurpBrush;
            this.Button_Pressed_Background = darkPurpBrush;
            this.Button_Static_Background = darkPurpBrush;

            // Handle ComboBox-specific resources
            this.ComboBox_Disabled_Background = darkPurpBrush;
            this.ComboBox_Disabled_Editable_Background = darkPurpBrush;
            this.ComboBox_Disabled_Editable_Button_Background = darkPurpBrush;
            this.ComboBox_MouseOver_Background = darkPurpBrush;
            this.ComboBox_MouseOver_Editable_Background = darkPurpBrush;
            this.ComboBox_MouseOver_Editable_Button_Background = darkPurpBrush;
            this.ComboBox_Pressed_Background = darkPurpBrush;
            this.ComboBox_Pressed_Editable_Background = darkPurpBrush;
            this.ComboBox_Pressed_Editable_Button_Background = darkPurpBrush;
            this.ComboBox_Static_Background = darkPurpBrush;
            this.ComboBox_Static_Editable_Background = darkPurpBrush;
            this.ComboBox_Static_Editable_Button_Background = darkPurpBrush;

            // Handle CustomMessageBox-specific resources
            this.CustomMessageBox_Static_Background = darkPurpBrush;

            // Handle MenuItem-specific resources
            this.MenuItem_SubMenu_Background = darkPurpBrush;
            this.MenuItem_SubMenu_Border = Brushes.DarkGray;

            // Handle ProgressBar-specific resources
            this.ProgressBar_Background = darkPurpBrush;

            // Handle ScrollViewer-specific resources
            this.ScrollViewer_ScrollBar_Background = darkPurpBrush;

            // Handle TabItem-specific resources
            this.TabItem_Selected_Background = darkPurpBrush;
            this.TabItem_Static_Background = darkPurpBrush;
            this.TabItem_Static_Border = Brushes.DarkGray;

            // Handle TextBox-specific resources
            this.TextBox_Static_Background = darkPurpBrush;
        }
    }
}
