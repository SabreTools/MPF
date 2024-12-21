using System.Windows;
using System.Windows.Media;

namespace MPF.UI
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
        public SolidColorBrush? ActiveBorderBrush { get; protected set; }

        /// <summary>
        /// SolidColorBrush that paints the face of a three-dimensional display element.
        /// </summary>
        public SolidColorBrush? ControlBrush { get; protected set; }

        /// <summary>
        /// SolidColorBrush that paints text in a three-dimensional display element.
        /// </summary>
        public SolidColorBrush? ControlTextBrush { get; protected set; }

        /// <summary>
        /// SolidColorBrush that paints disabled text.
        /// </summary>
        public SolidColorBrush? GrayTextBrush { get; protected set; }

        /// <summary>
        /// SolidColorBrush that paints the background of a window's client area.
        /// </summary>
        public SolidColorBrush? WindowBrush { get; protected set; }

        /// <summary>
        /// SolidColorBrush that paints the text in the client area of a window.
        /// </summary>
        public SolidColorBrush? WindowTextBrush { get; protected set; }

        #endregion

        #region Button

        /// <summary>
        /// Brush for the Button.Disabled.Background resource
        /// </summary>
        public Brush? Button_Disabled_Background { get; protected set; }

        /// <summary>
        /// Brush for the Button.MouseOver.Background resource
        /// </summary>
        public Brush? Button_MouseOver_Background { get; protected set; }

        /// <summary>
        /// Brush for the Button.Pressed.Background resource
        /// </summary>
        public Brush? Button_Pressed_Background { get; protected set; }

        /// <summary>
        /// Brush for the Button.Static.Background resource
        /// </summary>
        public Brush? Button_Static_Background { get; protected set; }

        #endregion

        #region ComboBox

        /// <summary>
        /// Brush for the ComboBox.Disabled.Background resource
        /// </summary>
        public Brush? ComboBox_Disabled_Background { get; protected set; }

        /// <summary>
        /// Brush for the ComboBox.Disabled.Editable.Background resource
        /// </summary>
        public Brush? ComboBox_Disabled_Editable_Background { get; protected set; }

        /// <summary>
        /// Brush for the ComboBox.Disabled.Editable.Button.Background resource
        /// </summary>
        public Brush? ComboBox_Disabled_Editable_Button_Background { get; protected set; }

        /// <summary>
        /// Brush for the ComboBox.MouseOver.Background resource
        /// </summary>
        public Brush? ComboBox_MouseOver_Background { get; protected set; }

        /// <summary>
        /// Brush for the ComboBox.MouseOver.Editable.Background resource
        /// </summary>
        public Brush? ComboBox_MouseOver_Editable_Background { get; protected set; }

        /// <summary>
        /// Brush for the ComboBox.MouseOver.Editable.Button.Background resource
        /// </summary>
        public Brush? ComboBox_MouseOver_Editable_Button_Background { get; protected set; }

        /// <summary>
        /// Brush for the ComboBox.Pressed.Background resource
        /// </summary>
        public Brush? ComboBox_Pressed_Background { get; protected set; }

        /// <summary>
        /// Brush for the ComboBox.Pressed.Editable.Background resource
        /// </summary>
        public Brush? ComboBox_Pressed_Editable_Background { get; protected set; }

        /// <summary>
        /// Brush for the ComboBox.Pressed.Editable.Button.Background resource
        /// </summary>
        public Brush? ComboBox_Pressed_Editable_Button_Background { get; protected set; }

        /// <summary>
        /// Brush for the ComboBox.Static.Background resource
        /// </summary>
        public Brush? ComboBox_Static_Background { get; protected set; }

        /// <summary>
        /// Brush for the ComboBox.Static.Editable.Background resource
        /// </summary>
        public Brush? ComboBox_Static_Editable_Background { get; protected set; }

        /// <summary>
        /// Brush for the ComboBox.Static.Editable.Button.Background resource
        /// </summary>
        public Brush? ComboBox_Static_Editable_Button_Background { get; protected set; }

        #endregion

        #region CustomMessageBox

        /// <summary>
        /// Brush for the CustomMessageBox.Static.Background resource
        /// </summary>
        public Brush? CustomMessageBox_Static_Background { get; protected set; }

        #endregion

        #region MenuItem

        /// <summary>
        /// Brush for the MenuItem.SubMenu.Background resource
        /// </summary>
        public Brush? MenuItem_SubMenu_Background { get; protected set; }

        /// <summary>
        /// Brush for the MenuItem.SubMenu.Border resource
        /// </summary>
        public Brush? MenuItem_SubMenu_Border { get; protected set; }

        #endregion

        #region ScrollViewer

        /// <summary>
        /// Brush for the ScrollViewer.ScrollBar.Background resource
        /// </summary>
        public Brush? ScrollViewer_ScrollBar_Background { get; protected set; }

        #endregion

        #region TabItem

        /// <summary>
        /// Brush for the TabItem.Selected.Background resource
        /// </summary>
        public Brush? TabItem_Selected_Background { get; protected set; }

        /// <summary>
        /// Brush for the TabItem.Static.Background resource
        /// </summary>
        public Brush? TabItem_Static_Background { get; protected set; }

        /// <summary>
        /// Brush for the TabItem.Static.Border resource
        /// </summary>
        public Brush? TabItem_Static_Border { get; protected set; }

        #endregion

        #region TextBox

        /// <summary>
        /// Brush for the TextBox.Static.Background resource
        /// </summary>
        public Brush? TextBox_Static_Background { get; protected set; }

        #endregion

        /// <summary>
        /// Apply the theme to the current application
        /// </summary>
        public void Apply()
        {
            // Handle application-wide resources
            Application.Current.Resources[SystemColors.ActiveBorderBrushKey] = ActiveBorderBrush;
            Application.Current.Resources[SystemColors.ControlBrushKey] = ControlBrush;
            Application.Current.Resources[SystemColors.ControlTextBrushKey] = ControlTextBrush;
            Application.Current.Resources[SystemColors.GrayTextBrushKey] = GrayTextBrush;
            Application.Current.Resources[SystemColors.WindowBrushKey] = WindowBrush;
            Application.Current.Resources[SystemColors.WindowTextBrushKey] = WindowTextBrush;

            // Handle Button-specific resources
            Application.Current.Resources["Button.Disabled.Background"] = Button_Disabled_Background;
            Application.Current.Resources["Button.MouseOver.Background"] = Button_MouseOver_Background;
            Application.Current.Resources["Button.Pressed.Background"] = Button_Pressed_Background;
            Application.Current.Resources["Button.Static.Background"] = Button_Static_Background;

            // Handle ComboBox-specific resources
            Application.Current.Resources["ComboBox.Disabled.Background"] = ComboBox_Disabled_Background;
            Application.Current.Resources["ComboBox.Disabled.Editable.Background"] = ComboBox_Disabled_Editable_Background;
            Application.Current.Resources["ComboBox.Disabled.Editable.Button.Background"] = ComboBox_Disabled_Editable_Button_Background;
            Application.Current.Resources["ComboBox.MouseOver.Background"] = ComboBox_MouseOver_Background;
            Application.Current.Resources["ComboBox.MouseOver.Editable.Background"] = ComboBox_MouseOver_Editable_Background;
            Application.Current.Resources["ComboBox.MouseOver.Editable.Button.Background"] = ComboBox_MouseOver_Editable_Button_Background;
            Application.Current.Resources["ComboBox.Pressed.Background"] = ComboBox_Pressed_Background;
            Application.Current.Resources["ComboBox.Pressed.Editable.Background"] = ComboBox_Pressed_Editable_Background;
            Application.Current.Resources["ComboBox.Pressed.Editable.Button.Background"] = ComboBox_Pressed_Editable_Button_Background;
            Application.Current.Resources["ComboBox.Static.Background"] = ComboBox_Static_Background;
            Application.Current.Resources["ComboBox.Static.Editable.Background"] = ComboBox_Static_Editable_Background;
            Application.Current.Resources["ComboBox.Static.Editable.Button.Background"] = ComboBox_Static_Editable_Button_Background;

            // Handle CustomMessageBox-specific resources
            Application.Current.Resources["CustomMessageBox.Static.Background"] = CustomMessageBox_Static_Background;

            // Handle MenuItem-specific resources
            Application.Current.Resources["MenuItem.SubMenu.Background"] = MenuItem_SubMenu_Background;
            Application.Current.Resources["MenuItem.SubMenu.Border"] = MenuItem_SubMenu_Border;

            // Handle ScrollViewer-specific resources
            Application.Current.Resources["ScrollViewer.ScrollBar.Background"] = ScrollViewer_ScrollBar_Background;

            // Handle TabItem-specific resources
            Application.Current.Resources["TabItem.Selected.Background"] = TabItem_Selected_Background;
            Application.Current.Resources["TabItem.Static.Background"] = TabItem_Static_Background;
            Application.Current.Resources["TabItem.Static.Border"] = TabItem_Static_Border;

            // Handle TextBox-specific resources
            Application.Current.Resources["TextBox.Static.Background"] = TextBox_Static_Background;
        }
    }
}
