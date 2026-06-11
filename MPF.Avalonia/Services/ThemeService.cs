using System;
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
            => options.GUI.Theming.EnableDarkMode = IsSystemDarkMode();

        /// <summary>
        /// Sync the dark mode flag with the system theme, then apply the resolved theme
        /// </summary>
        public static void ApplySystemTheme(IResourceDictionary resources, Options options)
        {
            SyncWithSystemTheme(options);
            Apply(resources, options);
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

            if (global::Avalonia.Application.Current is { } application)
            {
                application.RequestedThemeVariant = darkMode ? ThemeVariant.Dark : ThemeVariant.Light;
                ApplyWindowsTitleBarTheme(darkMode);
            }
        }

        /// <summary>
        /// Determine whether the system is currently using a dark theme
        /// </summary>
        private static bool IsSystemDarkMode()
        {
            PlatformThemeVariant? themeVariant = global::Avalonia.Application.Current?
                .PlatformSettings?
                .GetColorValues()
                .ThemeVariant;

            if (themeVariant == PlatformThemeVariant.Dark)
                return true;

            if (!OperatingSystem.IsLinux())
                return false;

            return IsLinuxDarkMode();
        }
    }
}
