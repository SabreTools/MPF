using System.Windows;
using System.Windows.Media;

namespace MPF.UI.Core
{
    /// <summary>
    /// Represents all required mapping values for the UI
    /// </summary>
    public abstract class Theme
    {
        #region Application-Wide

        /// <summary>
        /// SolidColorBrush used to paint the active window's border.
        /// </summary>
#if NET48
        public SolidColorBrush ActiveBorderBrush { get; protected set; }
#else
        public SolidColorBrush? ActiveBorderBrush { get; protected set; }
#endif

        /// <summary>
        /// SolidColorBrush that paints the face of a three-dimensional display element.
        /// </summary>
#if NET48
        public SolidColorBrush ControlBrush { get; protected set; }
#else
        public SolidColorBrush? ControlBrush { get; protected set; }
#endif

        /// <summary>
        /// SolidColorBrush that paints text in a three-dimensional display element.
        /// </summary>
#if NET48
        public SolidColorBrush ControlTextBrush { get; protected set; }
#else
        public SolidColorBrush? ControlTextBrush { get; protected set; }
#endif

        /// <summary>
        /// SolidColorBrush that paints disabled text.
        /// </summary>
#if NET48
        public SolidColorBrush GrayTextBrush { get; protected set; }
#else
        public SolidColorBrush? GrayTextBrush { get; protected set; }
#endif

        /// <summary>
        /// SolidColorBrush that paints the background of a window's client area.
        /// </summary>
#if NET48
        public SolidColorBrush WindowBrush { get; protected set; }
#else
        public SolidColorBrush? WindowBrush { get; protected set; }
#endif

        /// <summary>
        /// SolidColorBrush that paints the text in the client area of a window.
        /// </summary>
#if NET48
        public SolidColorBrush WindowTextBrush { get; protected set; }
#else
        public SolidColorBrush? WindowTextBrush { get; protected set; }
#endif

        #endregion

        #region Button

        /// <summary>
        /// Brush for the Button.Disabled.Background resource
        /// </summary>
#if NET48
        public Brush Button_Disabled_Background { get; protected set; }
#else
        public Brush? Button_Disabled_Background { get; protected set; }
#endif

        /// <summary>
        /// Brush for the Button.MouseOver.Background resource
        /// </summary>
#if NET48
        public Brush Button_MouseOver_Background { get; protected set; }
#else
        public Brush? Button_MouseOver_Background { get; protected set; }
#endif

        /// <summary>
        /// Brush for the Button.Pressed.Background resource
        /// </summary>
#if NET48
        public Brush Button_Pressed_Background { get; protected set; }
#else
        public Brush? Button_Pressed_Background { get; protected set; }
#endif

        /// <summary>
        /// Brush for the Button.Static.Background resource
        /// </summary>
#if NET48
        public Brush Button_Static_Background { get; protected set; }
#else
        public Brush? Button_Static_Background { get; protected set; }
#endif

        #endregion

        #region ComboBox

        /// <summary>
        /// Brush for the ComboBox.Disabled.Background resource
        /// </summary>
#if NET48
        public Brush ComboBox_Disabled_Background { get; protected set; }
#else
        public Brush? ComboBox_Disabled_Background { get; protected set; }
#endif

        /// <summary>
        /// Brush for the ComboBox.Disabled.Editable.Background resource
        /// </summary>
#if NET48
        public Brush ComboBox_Disabled_Editable_Background { get; protected set; }
#else
        public Brush? ComboBox_Disabled_Editable_Background { get; protected set; }
#endif

        /// <summary>
        /// Brush for the ComboBox.Disabled.Editable.Button.Background resource
        /// </summary>
#if NET48
        public Brush ComboBox_Disabled_Editable_Button_Background { get; protected set; }
#else
        public Brush? ComboBox_Disabled_Editable_Button_Background { get; protected set; }
#endif

        /// <summary>
        /// Brush for the ComboBox.MouseOver.Background resource
        /// </summary>
#if NET48
        public Brush ComboBox_MouseOver_Background { get; protected set; }
#else
        public Brush? ComboBox_MouseOver_Background { get; protected set; }
#endif

        /// <summary>
        /// Brush for the ComboBox.MouseOver.Editable.Background resource
        /// </summary>
#if NET48
        public Brush ComboBox_MouseOver_Editable_Background { get; protected set; }
#else
        public Brush? ComboBox_MouseOver_Editable_Background { get; protected set; }
#endif

        /// <summary>
        /// Brush for the ComboBox.MouseOver.Editable.Button.Background resource
        /// </summary>
#if NET48
        public Brush ComboBox_MouseOver_Editable_Button_Background { get; protected set; }
#else
        public Brush? ComboBox_MouseOver_Editable_Button_Background { get; protected set; }
#endif

        /// <summary>
        /// Brush for the ComboBox.Pressed.Background resource
        /// </summary>
#if NET48
        public Brush ComboBox_Pressed_Background { get; protected set; }
#else
        public Brush? ComboBox_Pressed_Background { get; protected set; }
#endif

        /// <summary>
        /// Brush for the ComboBox.Pressed.Editable.Background resource
        /// </summary>
#if NET48
        public Brush ComboBox_Pressed_Editable_Background { get; protected set; }
#else
        public Brush? ComboBox_Pressed_Editable_Background { get; protected set; }
#endif

        /// <summary>
        /// Brush for the ComboBox.Pressed.Editable.Button.Background resource
        /// </summary>
#if NET48
        public Brush ComboBox_Pressed_Editable_Button_Background { get; protected set; }
#else
        public Brush? ComboBox_Pressed_Editable_Button_Background { get; protected set; }
#endif

        /// <summary>
        /// Brush for the ComboBox.Static.Background resource
        /// </summary>
#if NET48
        public Brush ComboBox_Static_Background { get; protected set; }
#else
        public Brush? ComboBox_Static_Background { get; protected set; }
#endif

        /// <summary>
        /// Brush for the ComboBox.Static.Editable.Background resource
        /// </summary>
#if NET48
        public Brush ComboBox_Static_Editable_Background { get; protected set; }
#else
        public Brush? ComboBox_Static_Editable_Background { get; protected set; }
#endif

        /// <summary>
        /// Brush for the ComboBox.Static.Editable.Button.Background resource
        /// </summary>
#if NET48
        public Brush ComboBox_Static_Editable_Button_Background { get; protected set; }
#else
        public Brush? ComboBox_Static_Editable_Button_Background { get; protected set; }
#endif

        #endregion

        #region CustomMessageBox

        /// <summary>
        /// Brush for the CustomMessageBox.Static.Background resource
        /// </summary>
#if NET48
        public Brush CustomMessageBox_Static_Background { get; protected set; }
#else
        public Brush? CustomMessageBox_Static_Background { get; protected set; }
#endif

        #endregion

        #region MenuItem

        /// <summary>
        /// Brush for the MenuItem.SubMenu.Background resource
        /// </summary>
#if NET48
        public Brush MenuItem_SubMenu_Background { get; protected set; }
#else
        public Brush? MenuItem_SubMenu_Background { get; protected set; }
#endif

        /// <summary>
        /// Brush for the MenuItem.SubMenu.Border resource
        /// </summary>
#if NET48
        public Brush MenuItem_SubMenu_Border { get; protected set; }
#else
        public Brush? MenuItem_SubMenu_Border { get; protected set; }
#endif

        #endregion

        #region ProgressBar

        /// <summary>
        /// Brush for the ProgressBar.Background resource
        /// </summary>
#if NET48
        public Brush ProgressBar_Background { get; protected set; }
#else
        public Brush? ProgressBar_Background { get; protected set; }
#endif

        #endregion

        #region ScrollViewer

        /// <summary>
        /// Brush for the ScrollViewer.ScrollBar.Background resource
        /// </summary>
#if NET48
        public Brush ScrollViewer_ScrollBar_Background { get; protected set; }
#else
        public Brush? ScrollViewer_ScrollBar_Background { get; protected set; }
#endif

        #endregion

        #region TabItem

        /// <summary>
        /// Brush for the TabItem.Selected.Background resource
        /// </summary>
#if NET48
        public Brush TabItem_Selected_Background { get; protected set; }
#else
        public Brush? TabItem_Selected_Background { get; protected set; }
#endif

        /// <summary>
        /// Brush for the TabItem.Static.Background resource
        /// </summary>
#if NET48
        public Brush TabItem_Static_Background { get; protected set; }
#else
        public Brush? TabItem_Static_Background { get; protected set; }
#endif

        /// <summary>
        /// Brush for the TabItem.Static.Border resource
        /// </summary>
#if NET48
        public Brush TabItem_Static_Border { get; protected set; }
#else
        public Brush? TabItem_Static_Border { get; protected set; }
#endif

        #endregion

        #region TextBox

        /// <summary>
        /// Brush for the TextBox.Static.Background resource
        /// </summary>
#if NET48
        public Brush TextBox_Static_Background { get; protected set; }
#else
        public Brush? TextBox_Static_Background { get; protected set; }
#endif

        #endregion

        /// <summary>
        /// Apply the theme to the current application
        /// </summary>
        public void Apply()
        {
            // Handle application-wide resources
            Application.Current.Resources[SystemColors.ActiveBorderBrushKey] = this.ActiveBorderBrush;
            Application.Current.Resources[SystemColors.ControlBrushKey] = this.ControlBrush;
            Application.Current.Resources[SystemColors.ControlTextBrushKey] = this.ControlTextBrush;
            Application.Current.Resources[SystemColors.GrayTextBrushKey] = this.GrayTextBrush;
            Application.Current.Resources[SystemColors.WindowBrushKey] = this.WindowBrush;
            Application.Current.Resources[SystemColors.WindowTextBrushKey] = this.WindowTextBrush;

            // Handle Button-specific resources
            Application.Current.Resources["Button.Disabled.Background"] = this.Button_Disabled_Background;
            Application.Current.Resources["Button.MouseOver.Background"] = this.Button_MouseOver_Background;
            Application.Current.Resources["Button.Pressed.Background"] = this.Button_Pressed_Background;
            Application.Current.Resources["Button.Static.Background"] = this.Button_Static_Background;

            // Handle ComboBox-specific resources
            Application.Current.Resources["ComboBox.Disabled.Background"] = this.ComboBox_Disabled_Background;
            Application.Current.Resources["ComboBox.Disabled.Editable.Background"] = this.ComboBox_Disabled_Editable_Background;
            Application.Current.Resources["ComboBox.Disabled.Editable.Button.Background"] = this.ComboBox_Disabled_Editable_Button_Background;
            Application.Current.Resources["ComboBox.MouseOver.Background"] = this.ComboBox_MouseOver_Background;
            Application.Current.Resources["ComboBox.MouseOver.Editable.Background"] = this.ComboBox_MouseOver_Editable_Background;
            Application.Current.Resources["ComboBox.MouseOver.Editable.Button.Background"] = this.ComboBox_MouseOver_Editable_Button_Background;
            Application.Current.Resources["ComboBox.Pressed.Background"] = this.ComboBox_Pressed_Background;
            Application.Current.Resources["ComboBox.Pressed.Editable.Background"] = this.ComboBox_Pressed_Editable_Background;
            Application.Current.Resources["ComboBox.Pressed.Editable.Button.Background"] = this.ComboBox_Pressed_Editable_Button_Background;
            Application.Current.Resources["ComboBox.Static.Background"] = this.ComboBox_Static_Background;
            Application.Current.Resources["ComboBox.Static.Editable.Background"] = this.ComboBox_Static_Editable_Background;
            Application.Current.Resources["ComboBox.Static.Editable.Button.Background"] = this.ComboBox_Static_Editable_Button_Background;

            // Handle CustomMessageBox-specific resources
            Application.Current.Resources["CustomMessageBox.Static.Background"] = this.CustomMessageBox_Static_Background;

            // Handle MenuItem-specific resources
            Application.Current.Resources["MenuItem.SubMenu.Background"] = this.MenuItem_SubMenu_Background;
            Application.Current.Resources["MenuItem.SubMenu.Border"] = this.MenuItem_SubMenu_Border;

            // Handle ProgressBar-specific resources
            Application.Current.Resources["ProgressBar.Background"] = this.ProgressBar_Background;

            // Handle ScrollViewer-specific resources
            Application.Current.Resources["ScrollViewer.ScrollBar.Background"] = this.ScrollViewer_ScrollBar_Background;

            // Handle TabItem-specific resources
            Application.Current.Resources["TabItem.Selected.Background"] = this.TabItem_Selected_Background;
            Application.Current.Resources["TabItem.Static.Background"] = this.TabItem_Static_Background;
            Application.Current.Resources["TabItem.Static.Border"] = this.TabItem_Static_Border;

            // Handle TextBox-specific resources
            Application.Current.Resources["TextBox.Static.Background"] = this.TextBox_Static_Background;
        }
    }

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
