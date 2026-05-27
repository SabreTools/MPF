// MPF cross-platform (Avalonia) UI — contributed by Knutwurst (https://github.com/knutwurst)
using Avalonia;
using Avalonia.Media;

namespace MPF.UI.Avalonia
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
        /// <remarks>
        /// WPF used SystemColors keys (e.g. SystemColors.ActiveBorderBrushKey) as resource keys.
        /// Avalonia has no SystemColors class; we use plain string keys matching the resource
        /// dictionary entries defined in the AXAML styles (Task 5). If system-colour defaults
        /// are needed in a concrete subclass, map them to explicit Avalonia Color values
        /// (e.g. Colors.Gray for GrayText).
        /// </remarks>
        public void Apply()
        {
            var resources = global::Avalonia.Application.Current?.Resources;
            if (resources is null)
                return;

            // Application-wide (WPF used SystemColors.XxxBrushKey; we use string keys)
            resources["ActiveBorderBrush"] = ActiveBorderBrush;
            resources["ControlBrush"] = ControlBrush;
            resources["ControlTextBrush"] = ControlTextBrush;
            resources["GrayTextBrush"] = GrayTextBrush;
            resources["WindowBrush"] = WindowBrush;
            resources["WindowTextBrush"] = WindowTextBrush;

            // Button-specific resources
            resources["Button.Disabled.Background"] = Button_Disabled_Background;
            resources["Button.MouseOver.Background"] = Button_MouseOver_Background;
            resources["Button.Pressed.Background"] = Button_Pressed_Background;
            resources["Button.Static.Background"] = Button_Static_Background;

            // ComboBox-specific resources
            resources["ComboBox.Disabled.Background"] = ComboBox_Disabled_Background;
            resources["ComboBox.Disabled.Editable.Background"] = ComboBox_Disabled_Editable_Background;
            resources["ComboBox.Disabled.Editable.Button.Background"] = ComboBox_Disabled_Editable_Button_Background;
            resources["ComboBox.MouseOver.Background"] = ComboBox_MouseOver_Background;
            resources["ComboBox.MouseOver.Editable.Background"] = ComboBox_MouseOver_Editable_Background;
            resources["ComboBox.MouseOver.Editable.Button.Background"] = ComboBox_MouseOver_Editable_Button_Background;
            resources["ComboBox.Pressed.Background"] = ComboBox_Pressed_Background;
            resources["ComboBox.Pressed.Editable.Background"] = ComboBox_Pressed_Editable_Background;
            resources["ComboBox.Pressed.Editable.Button.Background"] = ComboBox_Pressed_Editable_Button_Background;
            resources["ComboBox.Static.Background"] = ComboBox_Static_Background;
            resources["ComboBox.Static.Editable.Background"] = ComboBox_Static_Editable_Background;
            resources["ComboBox.Static.Editable.Button.Background"] = ComboBox_Static_Editable_Button_Background;

            // CustomMessageBox-specific resources
            resources["CustomMessageBox.Static.Background"] = CustomMessageBox_Static_Background;

            // MenuItem-specific resources
            resources["MenuItem.SubMenu.Background"] = MenuItem_SubMenu_Background;
            resources["MenuItem.SubMenu.Border"] = MenuItem_SubMenu_Border;

            // ScrollViewer-specific resources
            resources["ScrollViewer.ScrollBar.Background"] = ScrollViewer_ScrollBar_Background;

            // TabItem-specific resources
            resources["TabItem.Selected.Background"] = TabItem_Selected_Background;
            resources["TabItem.Static.Background"] = TabItem_Static_Background;
            resources["TabItem.Static.Border"] = TabItem_Static_Border;

            // TextBox-specific resources
            resources["TextBox.Static.Background"] = TextBox_Static_Background;
        }
    }
}
