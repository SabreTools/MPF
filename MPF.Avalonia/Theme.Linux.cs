using System;
using System.Diagnostics;

namespace MPF.Avalonia
{
    /// <summary>
    /// Linux-specific functionality
    /// </summary>
    internal abstract partial class Theme
    {
        /// <summary>
        /// Detect a dark theme on Linux by inspecting common GTK/Qt environment variables,
        /// GNOME color-scheme settings, and the KDE color scheme
        /// </summary>
        public static bool IsLinuxDarkMode()
        {
            // GTK theme
            string? colorScheme = Environment.GetEnvironmentVariable("GTK_THEME");
            if (IsDarkThemeName(colorScheme))
                return true;

            // QT style
            colorScheme = Environment.GetEnvironmentVariable("QT_STYLE_OVERRIDE");
            if (IsDarkThemeName(colorScheme))
                return true;

            // GNOME color scheme
            colorScheme = RunCommand("gsettings", "get org.gnome.desktop.interface color-scheme");
            if (IsDarkThemeName(colorScheme))
                return true;

            // GNOME GTK theme
            colorScheme = RunCommand("gsettings", "get org.gnome.desktop.interface gtk-theme");
            if (IsDarkThemeName(colorScheme))
                return true;

            // Plasma 6
            colorScheme = RunCommand("kreadconfig6", "--group General --key ColorScheme");
            if (colorScheme is not null)
                return IsDarkThemeName(colorScheme);

            // Plasma 5
            colorScheme = RunCommand("kreadconfig5", "--group General --key ColorScheme");
            if (colorScheme is not null)
                return IsDarkThemeName(colorScheme);

            return false;
        }

        /// <summary>
        /// Test whether a theme or color scheme name indicates a dark variant
        /// </summary>
        private static bool IsDarkThemeName(string? value)
        {
            // Invalid values are ignored
            if (value is null)
                return false;

            // Known exact dark themes
            if (string.Equals(value, "BlackMATE"))
                return true;

            // Search for "dark" or "prefer-dark"
            return value.Contains("dark", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Run a command line tool and return its trimmed standard output, or null on failure or timeout
        /// </summary>
        /// TODO: Figure out a way to centralize this and ensure safety
        private static string? RunCommand(string fileName, string arguments)
        {
            try
            {
                using var process = Process.Start(new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                });
                if (process is null)
                    return null;

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
