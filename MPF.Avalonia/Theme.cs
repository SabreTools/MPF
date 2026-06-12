using Avalonia.Controls;
using Avalonia.Media;

namespace MPF.Avalonia
{
    /// <summary>
    /// Represents all required brush mapping values for the UI
    /// </summary>
    internal abstract class Theme
    {
        #region Application-Wide

        /// <summary>
        /// Brush for the AppBackgroundBrush resource
        /// </summary>
        public SolidColorBrush? AppBackgroundBrush { get; protected set; }

        /// <summary>
        /// Brush for the AppForegroundBrush resource
        /// </summary>
        public SolidColorBrush? AppForegroundBrush { get; protected set; }

        /// <summary>
        /// Brush for the HeadingForegroundBrush resource
        /// </summary>
        public SolidColorBrush? HeadingForegroundBrush { get; protected set; }

        /// <summary>
        /// Brush for the DisabledForegroundBrush resource
        /// </summary>
        public SolidColorBrush? DisabledForegroundBrush { get; protected set; }

        #endregion

        #region Panel

        /// <summary>
        /// Brush for the PanelBackgroundBrush resource
        /// </summary>
        public SolidColorBrush? PanelBackgroundBrush { get; protected set; }

        /// <summary>
        /// Brush for the PanelBorderBrush resource
        /// </summary>
        public SolidColorBrush? PanelBorderBrush { get; protected set; }

        #endregion

        #region Input

        /// <summary>
        /// Brush for the InputBackgroundBrush resource
        /// </summary>
        public SolidColorBrush? InputBackgroundBrush { get; protected set; }

        /// <summary>
        /// Brush for the DisabledInputBackgroundBrush resource
        /// </summary>
        public SolidColorBrush? DisabledInputBackgroundBrush { get; protected set; }

        /// <summary>
        /// Brush for the DisabledInputForegroundBrush resource
        /// </summary>
        public SolidColorBrush? DisabledInputForegroundBrush { get; protected set; }

        #endregion

        #region Button

        /// <summary>
        /// Brush for the ButtonBackgroundBrush resource
        /// </summary>
        public SolidColorBrush? ButtonBackgroundBrush { get; protected set; }

        /// <summary>
        /// Brush for the ButtonHoverBackgroundBrush resource
        /// </summary>
        public SolidColorBrush? ButtonHoverBackgroundBrush { get; protected set; }

        /// <summary>
        /// Brush for the ButtonPressedBackgroundBrush resource
        /// </summary>
        public SolidColorBrush? ButtonPressedBackgroundBrush { get; protected set; }

        /// <summary>
        /// Brush for the ButtonDisabledBackgroundBrush resource
        /// </summary>
        public SolidColorBrush? ButtonDisabledBackgroundBrush { get; protected set; }

        /// <summary>
        /// Brush for the ButtonBorderBrush resource
        /// </summary>
        public SolidColorBrush? ButtonBorderBrush { get; protected set; }

        /// <summary>
        /// Brush for the ButtonHoverBorderBrush resource
        /// </summary>
        public SolidColorBrush? ButtonHoverBorderBrush { get; protected set; }

        /// <summary>
        /// Brush for the ButtonPressedBorderBrush resource
        /// </summary>
        public SolidColorBrush? ButtonPressedBorderBrush { get; protected set; }

        /// <summary>
        /// Brush for the ButtonDisabledBorderBrush resource
        /// </summary>
        public SolidColorBrush? ButtonDisabledBorderBrush { get; protected set; }

        #endregion

        #region Menu

        /// <summary>
        /// Brush for the MenuBackgroundBrush resource
        /// </summary>
        public SolidColorBrush? MenuBackgroundBrush { get; protected set; }

        /// <summary>
        /// Brush for the MenuSubMenuBackgroundBrush resource
        /// </summary>
        public SolidColorBrush? MenuSubMenuBackgroundBrush { get; protected set; }

        /// <summary>
        /// Brush for the MenuSubMenuBorderBrush resource
        /// </summary>
        public SolidColorBrush? MenuSubMenuBorderBrush { get; protected set; }

        /// <summary>
        /// Brush for the MenuItemHoverBackgroundBrush resource
        /// </summary>
        public SolidColorBrush? MenuItemHoverBackgroundBrush { get; protected set; }

        /// <summary>
        /// Brush for the MenuItemPressedBackgroundBrush resource
        /// </summary>
        public SolidColorBrush? MenuItemPressedBackgroundBrush { get; protected set; }

        #endregion

        #region Title Bar

        /// <summary>
        /// Brush for the TitleBarButtonHoverBrush resource
        /// </summary>
        public SolidColorBrush? TitleBarButtonHoverBrush { get; protected set; }

        /// <summary>
        /// Brush for the TitleBarButtonPressedBrush resource
        /// </summary>
        public SolidColorBrush? TitleBarButtonPressedBrush { get; protected set; }

        #endregion

        #region Log

        /// <summary>
        /// Brush for the LogBackgroundBrush resource
        /// </summary>
        public SolidColorBrush? LogBackgroundBrush { get; protected set; }

        /// <summary>
        /// Brush for the LogForegroundBrush resource
        /// </summary>
        public SolidColorBrush? LogForegroundBrush { get; protected set; }

        #endregion

        /// <summary>
        /// Override the application background and text brushes with custom colors, ignoring
        /// any value that is missing or fails to parse
        /// </summary>
        public void ApplyCustomColors(string? backgroundColor, string? textColor)
        {
            if (!string.IsNullOrWhiteSpace(backgroundColor) && Color.TryParse(backgroundColor, out Color customBackground))
                AppBackgroundBrush = new SolidColorBrush(customBackground);

            if (!string.IsNullOrWhiteSpace(textColor) && Color.TryParse(textColor, out Color customForeground))
                AppForegroundBrush = new SolidColorBrush(customForeground);
        }

        /// <summary>
        /// Apply the theme to the given resource dictionary
        /// </summary>
        public void Apply(IResourceDictionary resources)
        {
            // Handle application-wide resources
            resources["AppBackgroundBrush"] = AppBackgroundBrush;
            resources["AppForegroundBrush"] = AppForegroundBrush;
            resources["HeadingForegroundBrush"] = HeadingForegroundBrush;
            resources["DisabledForegroundBrush"] = DisabledForegroundBrush;

            // Handle Panel-specific resources
            resources["PanelBackgroundBrush"] = PanelBackgroundBrush;
            resources["PanelBorderBrush"] = PanelBorderBrush;

            // Handle Input-specific resources
            resources["InputBackgroundBrush"] = InputBackgroundBrush;
            resources["DisabledInputBackgroundBrush"] = DisabledInputBackgroundBrush;
            resources["DisabledInputForegroundBrush"] = DisabledInputForegroundBrush;

            // Handle Button-specific resources
            resources["ButtonBackgroundBrush"] = ButtonBackgroundBrush;
            resources["ButtonHoverBackgroundBrush"] = ButtonHoverBackgroundBrush;
            resources["ButtonPressedBackgroundBrush"] = ButtonPressedBackgroundBrush;
            resources["ButtonDisabledBackgroundBrush"] = ButtonDisabledBackgroundBrush;
            resources["ButtonBorderBrush"] = ButtonBorderBrush;
            resources["ButtonHoverBorderBrush"] = ButtonHoverBorderBrush;
            resources["ButtonPressedBorderBrush"] = ButtonPressedBorderBrush;
            resources["ButtonDisabledBorderBrush"] = ButtonDisabledBorderBrush;

            // Handle Menu-specific resources
            resources["MenuBackgroundBrush"] = MenuBackgroundBrush;
            resources["MenuSubMenuBackgroundBrush"] = MenuSubMenuBackgroundBrush;
            resources["MenuSubMenuBorderBrush"] = MenuSubMenuBorderBrush;
            resources["MenuItemHoverBackgroundBrush"] = MenuItemHoverBackgroundBrush;
            resources["MenuItemPressedBackgroundBrush"] = MenuItemPressedBackgroundBrush;

            // Handle Title Bar-specific resources
            resources["TitleBarButtonHoverBrush"] = TitleBarButtonHoverBrush;
            resources["TitleBarButtonPressedBrush"] = TitleBarButtonPressedBrush;

            // Handle Log-specific resources
            resources["LogBackgroundBrush"] = LogBackgroundBrush;
            resources["LogForegroundBrush"] = LogForegroundBrush;
        }
    }
}
