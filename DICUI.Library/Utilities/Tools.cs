using System;
using System.Reflection;
using DICUI.Web;

namespace DICUI.Utilities
{
    public class Tools
    {
        /// <summary>
        /// Check for a new DICUI version
        /// </summary>
        /// <returns>
        /// Bool representing if the values are different.
        /// String representing the message to display the the user.
        /// String representing the new release URL.
        /// </returns>
        public static (bool different, string message, string url) CheckForNewVersion()
        {
            // Get current assembly version
            string version = GetCurrentVersion();

            // Get the latest tag from GitHub
            using (var client = new RedumpWebClient())
            {
                (string tag, string url) = client.GetRemoteVersionAndUrl();
                bool different = version != tag;

                string message = $"Local version: {version}"
                    + $"{Environment.NewLine}Remote version: {tag}"
                    + (different
                        ? $"{Environment.NewLine}The update URL has been added copied to your clipboard"
                        : $"{Environment.NewLine}You have the newest version!");

                return (different, message, url);
            }
        }

        /// <summary>
        /// Get the current assembly version formatted as a string
        /// </summary>
        private static string GetCurrentVersion()
        {
            var assemblyVersion = Assembly.GetEntryAssembly().GetName().Version;
            return $"{assemblyVersion.Major}.{assemblyVersion.Minor}" + (assemblyVersion.Build != 0 ? $".{assemblyVersion.Build}" : string.Empty);
        }

    }
}
