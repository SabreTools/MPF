using System;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;

namespace MPF.Avalonia.Services
{
    /// <summary>
    /// Windows-specific functionality: theming the native title bar through the
    /// DWM immersive dark mode attribute
    /// </summary>
    internal static partial class ThemeService
    {
        private const int DwmWindowAttributeUseImmersiveDarkModeBefore20H1 = 19;
        private const int DwmWindowAttributeUseImmersiveDarkMode = 20;

        /// <summary>
        /// Apply the current application theme variant to the given window's title bar
        /// </summary>
        public static void ApplyWindowTitleBarTheme(Window window)
        {
            if (global::Avalonia.Application.Current is null)
                return;

            ApplyWindowTitleBarTheme(window, global::Avalonia.Application.Current.RequestedThemeVariant == ThemeVariant.Dark);
        }

        /// <summary>
        /// Apply the given dark/light mode to the title bar of every open desktop window
        /// </summary>
        private static void ApplyWindowsTitleBarTheme(bool darkMode)
        {
            if (!OperatingSystem.IsWindows())
                return;

            if (global::Avalonia.Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
                return;

            foreach (Window window in desktop.Windows)
            {
                ApplyWindowTitleBarTheme(window, darkMode);
            }
        }

        /// <summary>
        /// Toggle the immersive dark mode DWM attribute on the given window's native handle,
        /// falling back to the pre-20H1 attribute identifier when the newer one is not supported
        /// </summary>
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

        #region Native Interop

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int attributeValue, int attributeSize);

        #endregion
    }
}
