using System;
using System.IO;
using System.Reflection;
using SabreTools.RedumpLib.Data;

namespace MPF.Core.Utilities
{
    public static class Tools
    {
        #region Support

        /// <summary>
        /// Returns false if a given InternalProgram does not support a given MediaType
        /// </summary>
        public static bool ProgramSupportsMedia(InternalProgram program, MediaType? type)
        {
            // If the media type is not set, return false
            if (type == null || type == MediaType.NONE)
                return false;

            return (program) switch
            {
                // Aaru
                InternalProgram.Aaru when type == MediaType.BluRay => true,
                InternalProgram.Aaru when type == MediaType.CDROM => true,
                InternalProgram.Aaru when type == MediaType.CompactFlash => true,
                InternalProgram.Aaru when type == MediaType.DVD => true,
                InternalProgram.Aaru when type == MediaType.GDROM => true,
                InternalProgram.Aaru when type == MediaType.FlashDrive => true,
                InternalProgram.Aaru when type == MediaType.FloppyDisk => true,
                InternalProgram.Aaru when type == MediaType.HardDisk => true,
                InternalProgram.Aaru when type == MediaType.HDDVD => true,
                InternalProgram.Aaru when type == MediaType.NintendoGameCubeGameDisc => true,
                InternalProgram.Aaru when type == MediaType.NintendoWiiOpticalDisc => true,
                InternalProgram.Aaru when type == MediaType.SDCard => true,

                // DiscImageCreator
                InternalProgram.DiscImageCreator when type == MediaType.BluRay => true,
                InternalProgram.DiscImageCreator when type == MediaType.CDROM => true,
                InternalProgram.DiscImageCreator when type == MediaType.CompactFlash => true,
                InternalProgram.DiscImageCreator when type == MediaType.DVD => true,
                InternalProgram.DiscImageCreator when type == MediaType.GDROM => true,
                InternalProgram.DiscImageCreator when type == MediaType.FlashDrive => true,
                InternalProgram.DiscImageCreator when type == MediaType.FloppyDisk => true,
                InternalProgram.DiscImageCreator when type == MediaType.HardDisk => true,
                InternalProgram.DiscImageCreator when type == MediaType.HDDVD => true,
                InternalProgram.DiscImageCreator when type == MediaType.NintendoGameCubeGameDisc => true,
                InternalProgram.DiscImageCreator when type == MediaType.NintendoWiiOpticalDisc => true,
                InternalProgram.DiscImageCreator when type == MediaType.SDCard => true,

                // Redumper
                InternalProgram.Redumper when type == MediaType.BluRay => true,
                InternalProgram.Redumper when type == MediaType.CDROM => true,
                InternalProgram.Redumper when type == MediaType.DVD => true,
                InternalProgram.Redumper when type == MediaType.GDROM => true,
                InternalProgram.Redumper when type == MediaType.HDDVD => true,

                // Default
                _ => false,
            };
        }

        #endregion

        #region Versioning

        /// <summary>
        /// Check for a new MPF version
        /// </summary>
        /// <returns>
        /// Bool representing if the values are different.
        /// String representing the message to display the the user.
        /// String representing the new release URL.
        /// </returns>
        public static (bool different, string message, string? url) CheckForNewVersion()
        {
            try
            {
                // Get current assembly version
                var assemblyVersion = Assembly.GetEntryAssembly()?.GetName()?.Version;
                if (assemblyVersion == null)
                    return (false, "Assembly version could not be determined", null);

                string version = $"{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}";

                // Get the latest tag from GitHub
                var (tag, url) = GetRemoteVersionAndUrl();
                bool different = version != tag && tag != null;

                string message = $"Local version: {version}"
                    + $"{Environment.NewLine}Remote version: {tag}"
                    + (different
                        ? $"{Environment.NewLine}The update URL has been added copied to your clipboard"
                        : $"{Environment.NewLine}You have the newest version!");

                return (different, message, url);
            }
            catch (Exception ex)
            {
                return (false, ex.ToString(), null);
            }
        }

        /// <summary>
        /// Get the current informational version formatted as a string
        /// </summary>
        public static string? GetCurrentVersion()
        {
            try
            {
                var assembly = Assembly.GetEntryAssembly();
                if (assembly == null)
                    return null;

                var assemblyVersion = Attribute.GetCustomAttribute(assembly, typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute;
                return assemblyVersion?.InformationalVersion;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        /// <summary>
        /// Get the latest version of MPF from GitHub and the release URL
        /// </summary>
        private static (string? tag, string? url) GetRemoteVersionAndUrl()
        {
#if NET20 || NET35 || NET40
            // Not supported in .NET Frameworks 2.0, 3.5, or 4.0
            return (null, null);
#else
            using var hc = new System.Net.Http.HttpClient();
#if NET452
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
#endif

            // TODO: Figure out a better way than having this hardcoded...
            string url = "https://api.github.com/repos/SabreTools/MPF/releases/latest";
            var message = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, url);
            message.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:64.0) Gecko/20100101 Firefox/64.0");
            var latestReleaseJsonString = hc.SendAsync(message)?.ConfigureAwait(false).GetAwaiter().GetResult()
                .Content?.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            if (latestReleaseJsonString == null)
                return (null, null);

            var latestReleaseJson = Newtonsoft.Json.Linq.JObject.Parse(latestReleaseJsonString);
            if (latestReleaseJson == null)
                return (null, null);

            var latestTag = latestReleaseJson["tag_name"]?.ToString();
            var releaseUrl = latestReleaseJson["html_url"]?.ToString();

            return (latestTag, releaseUrl);
#endif
        }

        #endregion
    }
}
