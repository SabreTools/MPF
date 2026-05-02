using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Styling;
using MPF.Frontend;

namespace MPF.Avalonia.Services
{
    internal static class ThemeService
    {
        public static void SyncWithSystemTheme(Options options)
            => options.GUI.Theming.EnableDarkMode = IsSystemDarkMode();

        public static void ApplySystemTheme(IResourceDictionary resources, Options options)
        {
            SyncWithSystemTheme(options);
            Apply(resources, options);
        }

        public static void Apply(IResourceDictionary resources, Options options)
        {
            bool darkMode = options.GUI.Theming.EnableDarkMode;
            Color background = darkMode ? Color.Parse("#FF1F1F1F") : Color.Parse("#FFF5F5F5");
            Color foreground = darkMode ? Color.Parse("#FFD8D8D8") : Color.Parse("#FF111111");

            if (!string.IsNullOrWhiteSpace(options.GUI.Theming.CustomBackgroundColor)
                && Color.TryParse(options.GUI.Theming.CustomBackgroundColor, out Color customBackground))
            {
                background = customBackground;
            }

            if (!string.IsNullOrWhiteSpace(options.GUI.Theming.CustomTextColor)
                && Color.TryParse(options.GUI.Theming.CustomTextColor, out Color customForeground))
            {
                foreground = customForeground;
            }

            resources["AppBackgroundBrush"] = new SolidColorBrush(background);
            resources["AppForegroundBrush"] = new SolidColorBrush(foreground);
            resources["HeadingForegroundBrush"] = new SolidColorBrush(darkMode ? Color.Parse("#FFCFCFCF") : Color.Parse("#FF111111"));
            resources["PanelBackgroundBrush"] = new SolidColorBrush(darkMode ? Color.Parse("#FF292929") : Colors.White);
            resources["PanelBorderBrush"] = new SolidColorBrush(darkMode ? Color.Parse("#FF424242") : Color.Parse("#FFD0D0D0"));
            resources["ButtonBackgroundBrush"] = new SolidColorBrush(darkMode ? Color.Parse("#FF333333") : Color.Parse("#FFE5E5E5"));
            resources["ButtonHoverBackgroundBrush"] = new SolidColorBrush(darkMode ? Color.Parse("#FF3D3D3D") : Color.Parse("#FFEDEDED"));
            resources["ButtonPressedBackgroundBrush"] = new SolidColorBrush(darkMode ? Color.Parse("#FF2A2A2A") : Color.Parse("#FFD8D8D8"));
            resources["ButtonDisabledBackgroundBrush"] = new SolidColorBrush(darkMode ? Color.Parse("#FF252525") : Color.Parse("#FFE2E2E2"));
            resources["ButtonBorderBrush"] = new SolidColorBrush(darkMode ? Color.Parse("#FF424242") : Color.Parse("#FFB8B8B8"));
            resources["ButtonHoverBorderBrush"] = new SolidColorBrush(darkMode ? Color.Parse("#FF4A4A4A") : Color.Parse("#FF9F9F9F"));
            resources["ButtonPressedBorderBrush"] = new SolidColorBrush(darkMode ? Color.Parse("#FF505050") : Color.Parse("#FF8E8E8E"));
            resources["ButtonDisabledBorderBrush"] = new SolidColorBrush(darkMode ? Color.Parse("#FF333333") : Color.Parse("#FFD2D2D2"));
            resources["DisabledForegroundBrush"] = new SolidColorBrush(darkMode ? Color.Parse("#FF5C5C5C") : Color.Parse("#FF8A8A8A"));
            resources["TitleBarButtonHoverBrush"] = new SolidColorBrush(darkMode ? Color.Parse("#FF333333") : Color.Parse("#FFE5E5E5"));
            resources["TitleBarButtonPressedBrush"] = new SolidColorBrush(darkMode ? Color.Parse("#FF2A2A2A") : Color.Parse("#FFD0D0D0"));
            resources["LogBackgroundBrush"] = new SolidColorBrush(darkMode ? Color.Parse("#FF151515") : Color.Parse("#FF202020"));
            resources["LogForegroundBrush"] = new SolidColorBrush(Color.Parse("#FFF0F0F0"));

            if (global::Avalonia.Application.Current is { } application)
            {
                application.RequestedThemeVariant = darkMode ? ThemeVariant.Dark : ThemeVariant.Light;
                ApplyWindowsTitleBarTheme(darkMode);
            }
        }

        public static void ApplyWindowTitleBarTheme(Window window)
        {
            if (global::Avalonia.Application.Current is null)
                return;

            ApplyWindowTitleBarTheme(window, global::Avalonia.Application.Current.RequestedThemeVariant == ThemeVariant.Dark);
        }

        private static void ApplyWindowsTitleBarTheme(bool darkMode)
        {
            if (!OperatingSystem.IsWindows())
                return;

            if (global::Avalonia.Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
                return;

            foreach (Window window in desktop.Windows)
                ApplyWindowTitleBarTheme(window, darkMode);
        }

        private static void ApplyWindowTitleBarTheme(Window window, bool darkMode)
        {
            if (!OperatingSystem.IsWindows())
                return;

            IntPtr hwnd = window.TryGetPlatformHandle()?.Handle ?? IntPtr.Zero;
            if (hwnd == IntPtr.Zero)
                return;

            int enabled = darkMode ? 1 : 0;
            if (DwmSetWindowAttribute(hwnd, DwmWindowAttributeUseImmersiveDarkMode, ref enabled, sizeof(int)) != 0)
                _ = DwmSetWindowAttribute(hwnd, DwmWindowAttributeUseImmersiveDarkModeBefore20H1, ref enabled, sizeof(int));
        }

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

        private static bool IsLinuxDarkMode()
            => IsDarkThemeName(Environment.GetEnvironmentVariable("GTK_THEME"))
                || IsDarkThemeName(Environment.GetEnvironmentVariable("QT_STYLE_OVERRIDE"))
                || IsDarkThemeName(RunCommand("gsettings", "get org.gnome.desktop.interface color-scheme"))
                || IsDarkThemeName(RunCommand("gsettings", "get org.gnome.desktop.interface gtk-theme"))
                || IsKdeColorSchemeDark();

        private static bool IsKdeColorSchemeDark()
        {
            string? colorScheme = RunCommand("kreadconfig6", "--group General --key ColorScheme")
                ?? RunCommand("kreadconfig5", "--group General --key ColorScheme");

            return IsDarkThemeName(colorScheme);
        }

        private static bool IsDarkThemeName(string? value)
            => value?.IndexOf("dark", StringComparison.OrdinalIgnoreCase) >= 0
                || value?.IndexOf("prefer-dark", StringComparison.OrdinalIgnoreCase) >= 0;

        private static string? RunCommand(string fileName, string arguments)
        {
            try
            {
                using Process process = Process.Start(new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                })!;

                if (!process.WaitForExit(1000))
                {
                    process.Kill(true);
                    return null;
                }

                return process.ExitCode == 0 ? process.StandardOutput.ReadToEnd().Trim() : null;
            }
            catch
            {
                return null;
            }
        }

        private const int DwmWindowAttributeUseImmersiveDarkModeBefore20H1 = 19;
        private const int DwmWindowAttributeUseImmersiveDarkMode = 20;

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int attributeValue, int attributeSize);
    }
}
