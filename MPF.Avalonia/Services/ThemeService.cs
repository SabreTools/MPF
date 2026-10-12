using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Avalonia.Styling;
using MPF.Avalonia.Themes;
using MPF.Frontend;

namespace MPF.Avalonia.Services
{
    /// <summary>
    /// Resolves and applies the active light/dark theme, including custom color overrides
    /// and platform-specific title bar theming. OS-specific functionality lives in the
    /// matching partial class files.
    /// </summary>
    internal static partial class ThemeService
    {
        /// <summary>
        /// Update the options' dark mode flag to match the detected system theme
        /// </summary>
        public static void SyncWithSystemTheme(Options options)
        {
            // Ignore if dark mode is already set
            if (options.GUI.Theming.EnableDarkMode)
                return;

            // If the Avalonia theme variant is Dark
            PlatformThemeVariant? themeVariant = Application.Current?
                .PlatformSettings?
                .GetColorValues()
                .ThemeVariant;
            if (themeVariant == PlatformThemeVariant.Dark)
                options.GUI.Theming.EnableDarkMode = true;

            // Check Linux dark mode variants
            else if (OperatingSystem.IsLinux())
                options.GUI.Theming.EnableDarkMode = IsLinuxDarkMode();
        }

        /// <summary>
        /// Apply the mapped theme for the current options to the resource dictionary, honoring any
        /// custom background and text color overrides, and apply the matching window theme variant
        /// </summary>
        public static void Apply(IResourceDictionary resources, Options options)
        {
            bool darkMode = options.GUI.Theming.EnableDarkMode;

            Theme theme = darkMode ? new DarkModeTheme() : new LightModeTheme();
            theme.ApplyCustomColors(options.GUI.Theming.CustomBackgroundColor, options.GUI.Theming.CustomTextColor);
            theme.Apply(resources);

            if (Application.Current is Application application)
            {
                application.RequestedThemeVariant = darkMode ? ThemeVariant.Dark : ThemeVariant.Light;
                ApplyWindowsTitleBarTheme(darkMode);
            }
        }
    }
}
