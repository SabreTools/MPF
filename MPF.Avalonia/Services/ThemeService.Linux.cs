using System;
using System.Diagnostics;

namespace MPF.Avalonia.Services
{
    /// <summary>
    /// Linux-specific functionality: detecting a dark system theme through common
    /// desktop environment settings
    /// </summary>
    internal static partial class ThemeService
    {
        /// <summary>
        /// Detect a dark theme on Linux by inspecting common GTK/Qt environment variables,
        /// GNOME color-scheme settings, and the KDE color scheme
        /// </summary>
        private static bool IsLinuxDarkMode()
            => IsDarkThemeName(Environment.GetEnvironmentVariable("GTK_THEME"))
                || IsDarkThemeName(Environment.GetEnvironmentVariable("QT_STYLE_OVERRIDE"))
                || IsDarkThemeName(RunCommand("gsettings", "get org.gnome.desktop.interface color-scheme"))
                || IsDarkThemeName(RunCommand("gsettings", "get org.gnome.desktop.interface gtk-theme"))
                || IsKdeColorSchemeDark();

        /// <summary>
        /// Read the active KDE color scheme (Plasma 6, then Plasma 5) and test it for a dark name
        /// </summary>
        private static bool IsKdeColorSchemeDark()
        {
            string? colorScheme = RunCommand("kreadconfig6", "--group General --key ColorScheme")
                ?? RunCommand("kreadconfig5", "--group General --key ColorScheme");

            return IsDarkThemeName(colorScheme);
        }

        /// <summary>
        /// Test whether a theme or color scheme name indicates a dark variant
        /// </summary>
        private static bool IsDarkThemeName(string? value)
            => value?.IndexOf("dark", StringComparison.OrdinalIgnoreCase) >= 0
                || value?.IndexOf("prefer-dark", StringComparison.OrdinalIgnoreCase) >= 0;

        /// <summary>
        /// Run a command line tool and return its trimmed standard output, or null on failure or timeout
        /// </summary>
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
    }
}
