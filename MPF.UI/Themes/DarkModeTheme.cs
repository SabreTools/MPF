using System.Windows.Media;

namespace MPF.UI.Themes
{
    /// <summary>
    /// Default dark-mode theme
    /// </summary>
    public class DarkModeTheme : Theme
    {
        public DarkModeTheme()
        {
            // Setup needed brushes
            var darkModeBrush = new SolidColorBrush { Color = Color.FromArgb(0xff, 0x20, 0x20, 0x20) };

            // Handle application-wide resources
            this.ActiveBorderBrush = Brushes.Black;
            this.ControlBrush = darkModeBrush;
            this.ControlTextBrush = Brushes.White;
            this.GrayTextBrush = Brushes.DarkGray;
            this.WindowBrush = darkModeBrush;
            this.WindowTextBrush = Brushes.White;

            // Handle Button-specific resources
            this.Button_Disabled_Background = darkModeBrush;
            this.Button_MouseOver_Background = darkModeBrush;
            this.Button_Pressed_Background = darkModeBrush;
            this.Button_Static_Background = darkModeBrush;

            // Handle ComboBox-specific resources
            this.ComboBox_Disabled_Background = darkModeBrush;
            this.ComboBox_Disabled_Editable_Background = darkModeBrush;
            this.ComboBox_Disabled_Editable_Button_Background = darkModeBrush;
            this.ComboBox_MouseOver_Background = darkModeBrush;
            this.ComboBox_MouseOver_Editable_Background = darkModeBrush;
            this.ComboBox_MouseOver_Editable_Button_Background = darkModeBrush;
            this.ComboBox_Pressed_Background = darkModeBrush;
            this.ComboBox_Pressed_Editable_Background = darkModeBrush;
            this.ComboBox_Pressed_Editable_Button_Background = darkModeBrush;
            this.ComboBox_Static_Background = darkModeBrush;
            this.ComboBox_Static_Editable_Background = darkModeBrush;
            this.ComboBox_Static_Editable_Button_Background = darkModeBrush;

            // Handle CustomMessageBox-specific resources
            this.CustomMessageBox_Static_Background = darkModeBrush;

            // Handle MenuItem-specific resources
            this.MenuItem_SubMenu_Background = darkModeBrush;
            this.MenuItem_SubMenu_Border = Brushes.DarkGray;

            // Handle ProgressBar-specific resources
            this.ProgressBar_Background = darkModeBrush;

            // Handle ScrollViewer-specific resources
            this.ScrollViewer_ScrollBar_Background = darkModeBrush;

            // Handle TabItem-specific resources
            this.TabItem_Selected_Background = darkModeBrush;
            this.TabItem_Static_Background = darkModeBrush;
            this.TabItem_Static_Border = Brushes.DarkGray;

            // Handle TextBox-specific resources
            this.TextBox_Static_Background = darkModeBrush;
        }
    }
}
