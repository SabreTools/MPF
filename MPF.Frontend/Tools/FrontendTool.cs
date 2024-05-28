using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using SabreTools.RedumpLib.Data;

namespace MPF.Frontend.Tools
{
    public static class FrontendTool
    {
        #region Information Extraction

        /// <summary>
        /// Get the current system from the drive volume label
        /// </summary>
        /// <returns>The system based on volume label, null if none detected</returns>
        public static RedumpSystem? GetRedumpSystemFromVolumeLabel(string? volumeLabel)
        {
            // If the volume label is empty, we can't do anything
            if (string.IsNullOrEmpty(volumeLabel))
                return null;

            // Audio CD
            if (volumeLabel!.Equals("Audio CD", StringComparison.OrdinalIgnoreCase))
                return RedumpSystem.AudioCD;

            // Microsoft Xbox
            if (volumeLabel.Equals("SEP13011042", StringComparison.OrdinalIgnoreCase))
                return RedumpSystem.MicrosoftXbox;
            else if (volumeLabel.Equals("SEP13011042072", StringComparison.OrdinalIgnoreCase))
                return RedumpSystem.MicrosoftXbox;

            // Microsoft Xbox 360
            if (volumeLabel.Equals("XBOX360", StringComparison.OrdinalIgnoreCase))
                return RedumpSystem.MicrosoftXbox360;
            else if (volumeLabel.Equals("XGD2DVD_NTSC", StringComparison.OrdinalIgnoreCase))
                return RedumpSystem.MicrosoftXbox360;

            // Microsoft Xbox 360 - Too overly broad even if a lot of discs use this
            //if (volumeLabel.Equals("CD_ROM", StringComparison.OrdinalIgnoreCase))
            //    return RedumpSystem.MicrosoftXbox360; // Also for Xbox One?
            //if (volumeLabel.Equals("DVD_ROM", StringComparison.OrdinalIgnoreCase))
            //    return RedumpSystem.MicrosoftXbox360;

            // Sega Mega-CD / Sega-CD
            if (volumeLabel.Equals("Sega_CD", StringComparison.OrdinalIgnoreCase))
                return RedumpSystem.SegaMegaCDSegaCD;

            // Sony PlayStation 3
            if (volumeLabel.Equals("PS3VOLUME", StringComparison.OrdinalIgnoreCase))
                return RedumpSystem.SonyPlayStation3;

            // Sony PlayStation 4
            if (volumeLabel.Equals("PS4VOLUME", StringComparison.OrdinalIgnoreCase))
                return RedumpSystem.SonyPlayStation4;

            // Sony PlayStation 5
            if (volumeLabel.Equals("PS5VOLUME", StringComparison.OrdinalIgnoreCase))
                return RedumpSystem.SonyPlayStation5;

            return null;
        }

        #endregion

        #region Normalization

        /// <summary>
        /// Adjust a disc title so that it will be processed correctly by Redump
        /// </summary>
        /// <param name="title">Existing title to potentially reformat</param>
        /// <param name="languages">Array of languages to use for assuming articles</param>
        /// <returns>The reformatted title</returns>
        public static string? NormalizeDiscTitle(string? title, Language?[]? languages)
        {
            // If we have no set languages, then assume English
            if (languages == null || languages.Length == 0)
                languages = [Language.English];

            // Loop through all of the given languages
            foreach (var language in languages)
            {
                // If the new title is different, assume it was normalized and return it
                string? newTitle = NormalizeDiscTitle(title, language);
                if (newTitle != title)
                    return newTitle;
            }

            // If we didn't already try English, try it now
            if (!languages.Contains(Language.English))
                return NormalizeDiscTitle(title, Language.English);

            // If all fails, then the title didn't need normalization
            return title;
        }

        /// <summary>
        /// Adjust a disc title so that it will be processed correctly by Redump
        /// </summary>
        /// <param name="title">Existing title to potentially reformat</param>
        /// <param name="language">Language to use for assuming articles</param>
        /// <returns>The reformatted title</returns>
        /// <remarks>
        /// If the language of the title is unknown or if it's multilingual,
        /// pass in Language.English for standardized coverage.
        /// </remarks>
        public static string? NormalizeDiscTitle(string? title, Language? language)
        {
            // If we have an invalid title, just return it as-is
            if (string.IsNullOrEmpty(title))
                return title;

            // If we have an invalid language, assume Language.English
            if (language == null)
                language = Language.English;

            // Get the title split into parts
            string[] splitTitle = title!.Split(' ').Where(s => !string.IsNullOrEmpty(s)).ToArray();

            // If we only have one part, we can't do anything
            if (splitTitle.Length <= 1)
                return title;

            // Determine if we have a definite or indefinite article as the first item
            string firstItem = splitTitle[0];
            switch (firstItem.ToLowerInvariant())
            {
                // Latin script articles
                case "'n"
                    when language is Language.Manx:
                case "a"
                    when language is Language.English
                        || language is Language.Hungarian
                        || language is Language.Portuguese
                        || language is Language.Scots:
                case "a'"
                    when language is Language.English
                        || language is Language.Hungarian
                        || language is Language.Irish
                        || language is Language.Gaelic:     // Scottish Gaelic
                case "al"
                    when language is Language.Breton:
                case "am"
                    when language is Language.Gaelic:       // Scottish Gaelic
                case "an"
                    when language is Language.Breton
                        || language is Language.Cornish
                        || language is Language.English
                        || language is Language.Irish
                        || language is Language.Gaelic:     // Scottish Gaelic
                case "anek"
                    when language is Language.Nepali:
                case "ar"
                    when language is Language.Breton:
                case "az"
                    when language is Language.Hungarian:
                case "ān"
                    when language is Language.Persian:
                case "as"
                    when language is Language.Portuguese:
                case "d'"
                    when language is Language.Luxembourgish:
                case "das"
                    when language is Language.German:
                case "dat"
                    when language is Language.Luxembourgish:
                case "de"
                    when language is Language.Dutch:
                case "déi"
                    when language is Language.Luxembourgish:
                case "dem"
                    when language is Language.German
                        || language is Language.Luxembourgish:
                case "den"
                    when language is Language.Dutch
                        || language is Language.German
                        || language is Language.Luxembourgish:
                case "der"
                    when language is Language.Dutch
                        || language is Language.German
                        || language is Language.Luxembourgish:
                case "des"
                    when language is Language.Dutch
                        || language is Language.French
                        || language is Language.German:
                case "die"
                    when language is Language.Afrikaans
                        || language is Language.German:
                case "du"
                    when language is Language.French:
                case "e"
                    when language is Language.Papiamento:
                case "een"
                    when language is Language.Dutch:
                case "egy"
                    when language is Language.Hungarian:
                case "ei"
                    when language is Language.Norwegian:
                case "ein"
                    when language is Language.German
                        || language is Language.Norwegian:
                case "eine"
                    when language is Language.German:
                case "einem"
                    when language is Language.German:
                case "einen"
                    when language is Language.German:
                case "einer"
                    when language is Language.German:
                case "eines"
                    when language is Language.German:
                case "eit"
                    when language is Language.Norwegian:
                case "ek"
                    when language is Language.Nepali:
                case "el"
                    when language is Language.Arabic
                        || language is Language.Catalan
                        || language is Language.Spanish:
                case "els"
                    when language is Language.Catalan:
                case "en"
                    when language is Language.Danish
                        || language is Language.Luxembourgish
                        || language is Language.Norwegian
                        || language is Language.Swedish:
                case "eng"
                    when language is Language.Luxembourgish:
                case "engem"
                    when language is Language.Luxembourgish:
                case "enger"
                    when language is Language.Luxembourgish:
                case "es"
                    when language is Language.Catalan:
                case "et"
                    when language is Language.Danish
                        || language is Language.Norwegian:
                case "ett"
                    when language is Language.Swedish:
                case "euta"
                    when language is Language.Nepali:
                case "euti"
                    when language is Language.Nepali:
                case "gli"
                    when language is Language.Italian:
                case "he"
                    when language is Language.Hawaiian
                        || language is Language.Maori:
                case "het"
                    when language is Language.Dutch:
                case "i"
                    when language is Language.Italian
                        || language is Language.Khasi:
                case "il"
                    when language is Language.Italian:
                case "in"
                    when language is Language.Persian:
                case "ka"
                    when language is Language.Hawaiian
                        || language is Language.Khasi:
                case "ke"
                    when language is Language.Hawaiian:
                case "ki"
                    when language is Language.Khasi:
                case "kunai"
                    when language is Language.Nepali:
                case "l'"
                    when language is Language.Catalan
                        || language is Language.French
                        || language is Language.Italian:
                case "la"
                    when language is Language.Catalan
                        || language is Language.Esperanto
                        || language is Language.French
                        || language is Language.Italian
                        || language is Language.Spanish:
                case "las"
                    when language is Language.Spanish:
                case "le"
                    when language is Language.French
                        || language is Language.Interlingua
                        || language is Language.Italian:
                case "les"
                    when language is Language.Catalan
                        || language is Language.French:
                case "lo"
                    when language is Language.Catalan
                        || language is Language.Italian
                        || language is Language.Spanish:
                case "los"
                    when language is Language.Catalan
                        || language is Language.Spanish:
                case "na"
                    when language is Language.Irish
                        || language is Language.Gaelic:     // Scottish Gaelic
                case "nam"
                    when language is Language.Gaelic:       // Scottish Gaelic
                case "nan"
                    when language is Language.Gaelic:       // Scottish Gaelic
                case "nā"
                    when language is Language.Hawaiian:
                case "ngā"
                    when language is Language.Maori:
                case "niște"
                    when language is Language.Romanian:
                case "ny"
                    when language is Language.Manx:
                case "o"
                    when language is Language.Portuguese
                        || language is Language.Romanian:
                case "os"
                    when language is Language.Portuguese:
                case "sa"
                    when language is Language.Catalan:
                case "sang"
                    when language is Language.Malay:
                case "se"
                    when language is Language.Finnish:
                case "ses"
                    when language is Language.Catalan:
                case "si"
                    when language is Language.Malay:
                case "te"
                    when language is Language.Maori:
                case "the"
                    when language is Language.English
                        || language is Language.Scots:
                case "u"
                    when language is Language.Khasi:
                case "ul"
                    when language is Language.Breton:
                case "um"
                    when language is Language.Portuguese:
                case "uma"
                    when language is Language.Portuguese:
                case "umas"
                    when language is Language.Portuguese:
                case "un"
                    when language is Language.Breton
                        || language is Language.Catalan
                        || language is Language.French
                        || language is Language.Interlingua
                        || language is Language.Italian
                        || language is Language.Papiamento
                        || language is Language.Romanian
                        || language is Language.Spanish:
                case "un'"
                    when language is Language.Italian:
                case "una"
                    when language is Language.Catalan
                        || language is Language.Italian:
                case "unas"
                    when language is Language.Spanish:
                case "une"
                    when language is Language.French:
                case "uno"
                    when language is Language.Italian:
                case "unos"
                    when language is Language.Spanish:
                case "uns"
                    when language is Language.Catalan
                        || language is Language.Portuguese:
                case "unei"
                    when language is Language.Romanian:
                case "unes"
                    when language is Language.Catalan:
                case "unor"
                    when language is Language.Romanian:
                case "unui"
                    when language is Language.Romanian:
                case "ur"
                    when language is Language.Breton:
                case "y"
                    when language is Language.Manx
                        || language is Language.Welsh:
                case "ye"
                    when language is Language.Persian:
                case "yek"
                    when language is Language.Persian:
                case "yn"
                    when language is Language.Manx:
                case "yr"
                    when language is Language.Welsh:

                // Non-latin script articles
                case "ο"
                    when language is Language.Greek:
                case "η"
                    when language is Language.Greek:
                case "το"
                    when language is Language.Greek:
                case "οι"
                    when language is Language.Greek:
                case "τα"
                    when language is Language.Greek:
                case "ένας"
                    when language is Language.Greek:
                case "μια"
                    when language is Language.Greek:
                case "ένα"
                    when language is Language.Greek:
                case "еден"
                    when language is Language.Macedonian:
                case "една"
                    when language is Language.Macedonian:
                case "едно"
                    when language is Language.Macedonian:
                case "едни"
                    when language is Language.Macedonian:
                case "एउटा"
                    when language is Language.Nepali:
                case "एउटी"
                    when language is Language.Nepali:
                case "एक"
                    when language is Language.Nepali:
                case "अनेक"
                    when language is Language.Nepali:
                case "कुनै"
                    when language is Language.Nepali:
                case "דער"
                    when language is Language.Yiddish:
                case "די"
                    when language is Language.Yiddish:
                case "דאָס"
                    when language is Language.Yiddish:
                case "דעם"
                    when language is Language.Yiddish:
                case "אַ"
                    when language is Language.Yiddish:
                case "אַן"
                    when language is Language.Yiddish:

                    break;

                // Otherwise, just return it as-is
                default:
                    return title;
            }

            // Insert the first item if we have a `:` or `-`
            bool itemInserted = false;
            var newTitleBuilder = new StringBuilder();
            for (int i = 1; i < splitTitle.Length; i++)
            {
                string segment = splitTitle[i];
                if (!itemInserted && segment == ":")
                {
                    itemInserted = true;
                    newTitleBuilder.Append($", {firstItem} :");
                }
                else if (!itemInserted && segment == "-")
                {
                    itemInserted = true;
                    newTitleBuilder.Append($", {firstItem} -");
                }
                else if (!itemInserted && segment.EndsWith(":"))
                {
                    itemInserted = true;
                    newTitleBuilder.Append($" {segment.Substring(0, segment.Length - 1)}, {firstItem}:");
                }
                else if (!itemInserted && segment.EndsWith("-"))
                {
                    itemInserted = true;
                    newTitleBuilder.Append($" {segment.Substring(0, segment.Length - 1)}, {firstItem}-");
                }
                else
                {
                    newTitleBuilder.Append($" {segment}");
                }
            }

            // If we didn't insert the item yet, add it to the end
            string newTitle = newTitleBuilder.ToString().Trim();
            if (!itemInserted)
                newTitle = $"{newTitle}, {firstItem}";

            return newTitle;
        }

        /// <summary>
        /// Normalize a split set of paths
        /// </summary>
        /// <param name="path">Path value to normalize</param>
        public static string NormalizeOutputPaths(string? path, bool getFullPath)
        {
            // The easy way
            try
            {
                // If we have an invalid path
                if (string.IsNullOrEmpty(path))
                    return string.Empty;

                // Remove quotes and angle brackets from path
                path = path!.Replace("\"", string.Empty);
                path = path!.Replace("<", string.Empty);
                path = path!.Replace(">", string.Empty);

                // Try getting the combined path and returning that directly
                string fullPath = getFullPath ? Path.GetFullPath(path) : path;
                var fullDirectory = Path.GetDirectoryName(fullPath);
                string fullFile = Path.GetFileName(fullPath);

                // Remove invalid path characters
                if (fullDirectory != null)
                {
                    foreach (char c in Path.GetInvalidPathChars())
                        fullDirectory = fullDirectory.Replace(c, '_');
                }

                // Remove invalid filename characters
                foreach (char c in Path.GetInvalidFileNameChars())
                    fullFile = fullFile.Replace(c, '_');

                if (string.IsNullOrEmpty(fullDirectory))
                    return fullFile;
                else
                    return Path.Combine(fullDirectory, fullFile);
            }
            catch { }

            return path ?? string.Empty;
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