using System;
using System.Reflection;
using DICUI.Web;

namespace DICUI.Utilities
{
    public static class Tools
    {
        #region Byte Arrays

        /// <summary>
        /// Search for a byte array in another array
        /// </summary>
        public static bool Contains(this byte[] stack, byte[] needle, out int position, int start = 0, int end = -1)
        {
            // Initialize the found position to -1
            position = -1;

            // If either array is null or empty, we can't do anything
            if (stack == null || stack.Length == 0 || needle == null || needle.Length == 0)
                return false;

            // If the needle array is larger than the stack array, it can't be contained within
            if (needle.Length > stack.Length)
                return false;

            // If start or end are not set properly, set them to defaults
            if (start < 0)
                start = 0;
            if (end < 0)
                end = stack.Length - needle.Length;

            for (int i = start; i < end; i++)
            {
                if (stack.EqualAt(needle, i))
                {
                    position = i;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// See if a byte array starts with another
        /// </summary>
        public static bool StartsWith(this byte[] stack, byte[] needle)
        {
            return stack.Contains(needle, out int _, start: 0, end: 1);
        }

        /// <summary>
        /// Get if a stack at a certain index is equal to a needle
        /// </summary>
        private static bool EqualAt(this byte[] stack, byte[] needle, int index)
        {
            // If we're too close to the end of the stack, return false
            if (needle.Length >= stack.Length - index)
                return false;

            for (int i = 0; i < needle.Length; i++)
            {
                if (stack[i + index] != needle[i])
                    return false;
            }

            return true;
        }

        #endregion

        #region Versioning

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

        #endregion
    }
}
