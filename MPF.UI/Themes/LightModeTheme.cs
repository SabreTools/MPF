using System.Windows;
using System.Windows.Media;

namespace MPF.UI.Themes
{
    /// <summary>
    /// Default light-mode theme
    /// </summary>
    public class LightModeTheme : Theme
    {
        public LightModeTheme()
        {
            // Handle application-wide resources
            this.ActiveBorderBrush = null;
            this.ControlBrush = null;
            this.ControlTextBrush = null;
            this.GrayTextBrush = null;
            this.WindowBrush = null;
            this.WindowTextBrush = null;

            // Handle Button-specific resources
            this.Button_Disabled_Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xF4, 0xF4, 0xF4));
            this.Button_MouseOver_Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xBE, 0xE6, 0xFD));
            this.Button_Pressed_Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xC4, 0xE5, 0xF6));
            this.Button_Static_Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xDD, 0xDD, 0xDD));

            // Handle ComboBox-specific resources
            this.ComboBox_Disabled_Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xF0, 0xF0, 0xF0));
            this.ComboBox_Disabled_Editable_Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
            this.ComboBox_Disabled_Editable_Button_Background = Brushes.Transparent;
            this.ComboBox_MouseOver_Background = new LinearGradientBrush(
                Color.FromArgb(0xFF, 0xEC, 0xF4, 0xFC),
                Color.FromArgb(0xFF, 0xDC, 0xEC, 0xFC),
                new Point(0, 0),
                new Point(0, 1));
            this.ComboBox_MouseOver_Editable_Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
            this.ComboBox_MouseOver_Editable_Button_Background = new LinearGradientBrush(
                Color.FromArgb(0xFF, 0xEB, 0xF4, 0xFC),
                Color.FromArgb(0xFF, 0xDC, 0xEC, 0xFC),
                new Point(0, 0),
                new Point(0, 1));
            this.ComboBox_Pressed_Background = new LinearGradientBrush(
                Color.FromArgb(0xFF, 0xDA, 0xEC, 0xFC),
                Color.FromArgb(0xFF, 0xC4, 0xE0, 0xFC),
                new Point(0, 0),
                new Point(0, 1));
            this.ComboBox_Pressed_Editable_Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
            this.ComboBox_Pressed_Editable_Button_Background = new LinearGradientBrush(
                Color.FromArgb(0xFF, 0xDA, 0xEB, 0xFC),
                Color.FromArgb(0xFF, 0xC4, 0xE0, 0xFC),
                new Point(0, 0),
                new Point(0, 1));
            this.ComboBox_Static_Background = new LinearGradientBrush(
                Color.FromArgb(0xFF, 0xF0, 0xF0, 0xF0),
                Color.FromArgb(0xFF, 0xE5, 0xE5, 0xE5),
                new Point(0, 0),
                new Point(0, 1));
            this.ComboBox_Static_Editable_Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
            this.ComboBox_Static_Editable_Button_Background = Brushes.Transparent;

            // Handle CustomMessageBox-specific resources
            this.CustomMessageBox_Static_Background = null;

            // Handle MenuItem-specific resources
            this.MenuItem_SubMenu_Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xF0, 0xF0, 0xF0));
            this.MenuItem_SubMenu_Border = Brushes.DarkGray;

            // Handle ProgressBar-specific resources
            this.ProgressBar_Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xE6, 0xE6, 0xE6));

            // Handle ScrollViewer-specific resources
            this.ScrollViewer_ScrollBar_Background = Brushes.LightGray;

            // Handle TabItem-specific resources
            this.TabItem_Selected_Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
            this.TabItem_Static_Background = new LinearGradientBrush(
                Color.FromArgb(0xFF, 0xF0, 0xF0, 0xF0),
                Color.FromArgb(0xFF, 0xE5, 0xE5, 0xE5),
                new Point(0, 0),
                new Point(0, 1));
            this.TabItem_Static_Border = Brushes.DarkGray;

            // Handle TextBox-specific resources
            this.TextBox_Static_Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
        }
    }
}
