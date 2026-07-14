using System;
using System.Reflection;
using System.Text;
using SabreTools.RedumpLib.Data;

namespace MPF.Frontend.Tools
{
    public static class FrontendTool
    {
        #region Information Extraction

        /// <summary>
        /// Get the default speed for a given media type from the supplied options
        /// </summary>
        public static int GetDefaultSpeedForPhysicalMediaType(PhysicalMediaType? mediaType, Options options)
        {
#pragma warning disable IDE0072
            return mediaType switch
            {
                // CD dump speed
                PhysicalMediaType.CDROM => options.Dumping.DumpSpeeds.CD,
                PhysicalMediaType.GDROM => options.Dumping.DumpSpeeds.CD,

                // DVD dump speed
                PhysicalMediaType.DVD => options.Dumping.DumpSpeeds.DVD,
                PhysicalMediaType.NintendoGameCubeGameDisc => options.Dumping.DumpSpeeds.DVD,
                PhysicalMediaType.NintendoWiiOpticalDisc => options.Dumping.DumpSpeeds.DVD,

                // HD-DVD dump speed
                PhysicalMediaType.HDDVD => options.Dumping.DumpSpeeds.HDDVD,

                // BD dump speed
                PhysicalMediaType.BluRay => options.Dumping.DumpSpeeds.Bluray,
                PhysicalMediaType.NintendoWiiUOpticalDisc => options.Dumping.DumpSpeeds.Bluray,

                // Default
                _ => options.Dumping.DumpSpeeds.CD,
            };
#pragma warning restore IDE0072
        }

        /// <summary>
        /// Get the current system from the drive volume label
        /// </summary>
        /// <returns>The system based on volume label, null if none detected</returns>
        public static PhysicalSystem? GetPhysicalSystemFromVolumeLabel(string? volumeLabel)
        {
            // If the volume label is empty, we can't do anything
            if (string.IsNullOrEmpty(volumeLabel))
                return null;

            // Trim the volume label
            volumeLabel = volumeLabel!.Trim();

            // Audio CD
            if (volumeLabel!.Equals("Audio CD", StringComparison.OrdinalIgnoreCase))
                return PhysicalSystem.AudioCD;

            // Microsoft Xbox
            if (volumeLabel.Equals("SEP13011042", StringComparison.OrdinalIgnoreCase))
                return PhysicalSystem.MicrosoftXbox;
            else if (volumeLabel.Equals("SEP13011042072", StringComparison.OrdinalIgnoreCase))
                return PhysicalSystem.MicrosoftXbox;

            // Microsoft Xbox 360
            if (volumeLabel.Equals("XBOX360"))
                return PhysicalSystem.MicrosoftXbox360;
            else if (volumeLabel.Equals("XGD2DVD_NTSC"))
                return PhysicalSystem.MicrosoftXbox360;
            else if (volumeLabel.Equals("XBOX_TINYTEST"))
                return PhysicalSystem.MicrosoftXbox360;
            else if (volumeLabel.Equals("13599"))
                return PhysicalSystem.MicrosoftXbox360;
            else if (volumeLabel.Equals("14719"))
                return PhysicalSystem.MicrosoftXbox360;
            else if (volumeLabel.Equals("15574"))
                return PhysicalSystem.MicrosoftXbox360;
            else if (volumeLabel.Equals("16197"))
                return PhysicalSystem.MicrosoftXbox360;
            else if (volumeLabel.Equals("16197"))
                return PhysicalSystem.MicrosoftXbox360;
            else if (volumeLabel.Equals("17349"))
                return PhysicalSystem.MicrosoftXbox360;
            // DVD_ROM and CD_ROM have too many false positives
            //else if (volumeLabel.Equals("DVD_ROM"))
            //    return PhysicalSystem.MicrosoftXbox360;

            // Sega Mega-CD / Sega-CD
            if (volumeLabel.Equals("Sega_CD", StringComparison.OrdinalIgnoreCase))
                return PhysicalSystem.SegaMegaCDSegaCD;

            // Sony PlayStation 3
            if (volumeLabel.Equals("PS3VOLUME", StringComparison.OrdinalIgnoreCase))
                return PhysicalSystem.SonyPlayStation3;

            // Sony PlayStation 4
            if (volumeLabel.Equals("PS4VOLUME", StringComparison.OrdinalIgnoreCase))
                return PhysicalSystem.SonyPlayStation4;

            // Sony PlayStation 5
            if (volumeLabel.Equals("PS5VOLUME", StringComparison.OrdinalIgnoreCase))
                return PhysicalSystem.SonyPlayStation5;

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
        public static string? NormalizeDiscTitle(string? title, LanguageCode?[]? languages)
        {
            // If we have no set languages, then assume English
            if (languages is null || languages.Length == 0)
                languages = [LanguageCode.English];

            // Loop through all of the given languages
            foreach (var language in languages)
            {
                // If the new title is different, assume it was normalized and return it
                string? newTitle = NormalizeDiscTitle(title, language);
                if (newTitle != title)
                    return newTitle;
            }

            // If we didn't already try English, try it now
            if (!Array.Exists(languages, l => l == LanguageCode.English))
                return NormalizeDiscTitle(title, LanguageCode.English);

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
        /// pass in LanguageCode.English for standardized coverage.
        /// </remarks>
        public static string? NormalizeDiscTitle(string? title, LanguageCode? language)
        {
            // If we have an invalid title, just return it as-is
            if (string.IsNullOrEmpty(title))
                return title;

            // If we have an invalid language, assume LanguageCode.English
            language ??= LanguageCode.English;

            // Get the title split into parts
            string[] splitTitle = Array.FindAll(title!.Split(' '), s => !string.IsNullOrEmpty(s));

            // If we only have one part, we can't do anything
            if (splitTitle.Length <= 1)
                return title;

            // Determine if we have a definite or indefinite article as the first item
            string firstItem = splitTitle[0];
            switch (firstItem.ToLowerInvariant())
            {
                // Latin script articles
                case "'n"
                    when language == LanguageCode.Manx:
                case "a"
                    when language == LanguageCode.English
                        || language == LanguageCode.Hungarian
                        || language == LanguageCode.Portuguese
                        || language == LanguageCode.Scots:
                case "a'"
                    when language == LanguageCode.English
                        || language == LanguageCode.Hungarian
                        || language == LanguageCode.Irish
                        || language == LanguageCode.Gaelic:     // Scottish Gaelic
                case "al"
                    when language == LanguageCode.Breton:
                case "am"
                    when language == LanguageCode.Gaelic:       // Scottish Gaelic
                case "an"
                    when language == LanguageCode.Breton
                        || language == LanguageCode.Cornish
                        || language == LanguageCode.English
                        || language == LanguageCode.Irish
                        || language == LanguageCode.Gaelic:     // Scottish Gaelic
                case "anek"
                    when language == LanguageCode.Nepali:
                case "ar"
                    when language == LanguageCode.Breton:
                case "az"
                    when language == LanguageCode.Hungarian:
                case "ān"
                    when language == LanguageCode.Persian:
                case "as"
                    when language == LanguageCode.Portuguese:
                case "d'"
                    when language == LanguageCode.Luxembourgish:
                case "das"
                    when language == LanguageCode.German:
                case "dat"
                    when language == LanguageCode.Luxembourgish:
                case "de"
                    when language == LanguageCode.Dutch:
                case "déi"
                    when language == LanguageCode.Luxembourgish:
                case "dem"
                    when language == LanguageCode.German
                        || language == LanguageCode.Luxembourgish:
                case "den"
                    when language == LanguageCode.Dutch
                        || language == LanguageCode.German
                        || language == LanguageCode.Luxembourgish:
                case "der"
                    when language == LanguageCode.Dutch
                        || language == LanguageCode.German
                        || language == LanguageCode.Luxembourgish:
                case "des"
                    when language == LanguageCode.Dutch
                        || language == LanguageCode.French
                        || language == LanguageCode.German:
                case "die"
                    when language == LanguageCode.Afrikaans
                        || language == LanguageCode.German:
                case "du"
                    when language == LanguageCode.French:
                case "e"
                    when language == LanguageCode.Papiamento:
                case "een"
                    when language == LanguageCode.Dutch:
                case "egy"
                    when language == LanguageCode.Hungarian:
                case "ei"
                    when language == LanguageCode.Norwegian:
                case "ein"
                    when language == LanguageCode.German
                        || language == LanguageCode.Norwegian:
                case "eine"
                    when language == LanguageCode.German:
                case "einem"
                    when language == LanguageCode.German:
                case "einen"
                    when language == LanguageCode.German:
                case "einer"
                    when language == LanguageCode.German:
                case "eines"
                    when language == LanguageCode.German:
                case "eit"
                    when language == LanguageCode.Norwegian:
                case "ek"
                    when language == LanguageCode.Nepali:
                case "el"
                    when language == LanguageCode.Arabic
                        || language == LanguageCode.Catalan
                        || language == LanguageCode.Spanish:
                case "els"
                    when language == LanguageCode.Catalan:
                case "en"
                    when language == LanguageCode.Danish
                        || language == LanguageCode.Luxembourgish
                        || language == LanguageCode.Norwegian
                        || language == LanguageCode.Swedish:
                case "eng"
                    when language == LanguageCode.Luxembourgish:
                case "engem"
                    when language == LanguageCode.Luxembourgish:
                case "enger"
                    when language == LanguageCode.Luxembourgish:
                case "es"
                    when language == LanguageCode.Catalan:
                case "et"
                    when language == LanguageCode.Danish
                        || language == LanguageCode.Norwegian:
                case "ett"
                    when language == LanguageCode.Swedish:
                case "euta"
                    when language == LanguageCode.Nepali:
                case "euti"
                    when language == LanguageCode.Nepali:
                case "gli"
                    when language == LanguageCode.Italian:
                case "he"
                    when language == LanguageCode.Hawaiian
                        || language == LanguageCode.Maori:
                case "het"
                    when language == LanguageCode.Dutch:
                case "i"
                    when language == LanguageCode.Italian
                        || language == LanguageCode.Khasi:
                case "il"
                    when language == LanguageCode.Italian:
                case "in"
                    when language == LanguageCode.Persian:
                case "ka"
                    when language == LanguageCode.Hawaiian
                        || language == LanguageCode.Khasi:
                case "ke"
                    when language == LanguageCode.Hawaiian:
                case "ki"
                    when language == LanguageCode.Khasi:
                case "kunai"
                    when language == LanguageCode.Nepali:
                case "l'"
                    when language == LanguageCode.Catalan
                        || language == LanguageCode.French
                        || language == LanguageCode.Italian:
                case "la"
                    when language == LanguageCode.Catalan
                        || language == LanguageCode.Esperanto
                        || language == LanguageCode.French
                        || language == LanguageCode.Italian
                        || language == LanguageCode.Spanish:
                case "las"
                    when language == LanguageCode.Spanish:
                case "le"
                    when language == LanguageCode.French
                        || language == LanguageCode.Interlingua
                        || language == LanguageCode.Italian:
                case "les"
                    when language == LanguageCode.Catalan
                        || language == LanguageCode.French:
                case "lo"
                    when language == LanguageCode.Catalan
                        || language == LanguageCode.Italian
                        || language == LanguageCode.Spanish:
                case "los"
                    when language == LanguageCode.Catalan
                        || language == LanguageCode.Spanish:
                case "na"
                    when language == LanguageCode.Irish
                        || language == LanguageCode.Gaelic:     // Scottish Gaelic
                case "nam"
                    when language == LanguageCode.Gaelic:       // Scottish Gaelic
                case "nan"
                    when language == LanguageCode.Gaelic:       // Scottish Gaelic
                case "nā"
                    when language == LanguageCode.Hawaiian:
                case "ngā"
                    when language == LanguageCode.Maori:
                case "niște"
                    when language == LanguageCode.Romanian:
                case "ny"
                    when language == LanguageCode.Manx:
                case "o"
                    when language == LanguageCode.Portuguese
                        || language == LanguageCode.Romanian:
                case "os"
                    when language == LanguageCode.Portuguese:
                case "sa"
                    when language == LanguageCode.Catalan:
                case "sang"
                    when language == LanguageCode.Malay:
                case "se"
                    when language == LanguageCode.Finnish:
                case "ses"
                    when language == LanguageCode.Catalan:
                case "si"
                    when language == LanguageCode.Malay:
                case "te"
                    when language == LanguageCode.Maori:
                case "the"
                    when language == LanguageCode.English
                        || language == LanguageCode.Scots:
                case "u"
                    when language == LanguageCode.Khasi:
                case "ul"
                    when language == LanguageCode.Breton:
                case "um"
                    when language == LanguageCode.Portuguese:
                case "uma"
                    when language == LanguageCode.Portuguese:
                case "umas"
                    when language == LanguageCode.Portuguese:
                case "un"
                    when language == LanguageCode.Breton
                        || language == LanguageCode.Catalan
                        || language == LanguageCode.French
                        || language == LanguageCode.Interlingua
                        || language == LanguageCode.Italian
                        || language == LanguageCode.Papiamento
                        || language == LanguageCode.Romanian
                        || language == LanguageCode.Spanish:
                case "un'"
                    when language == LanguageCode.Italian:
                case "una"
                    when language == LanguageCode.Catalan
                        || language == LanguageCode.Italian:
                case "unas"
                    when language == LanguageCode.Spanish:
                case "une"
                    when language == LanguageCode.French:
                case "uno"
                    when language == LanguageCode.Italian:
                case "unos"
                    when language == LanguageCode.Spanish:
                case "uns"
                    when language == LanguageCode.Catalan
                        || language == LanguageCode.Portuguese:
                case "unei"
                    when language == LanguageCode.Romanian:
                case "unes"
                    when language == LanguageCode.Catalan:
                case "unor"
                    when language == LanguageCode.Romanian:
                case "unui"
                    when language == LanguageCode.Romanian:
                case "ur"
                    when language == LanguageCode.Breton:
                case "y"
                    when language == LanguageCode.Manx
                        || language == LanguageCode.Welsh:
                case "ye"
                    when language == LanguageCode.Persian:
                case "yek"
                    when language == LanguageCode.Persian:
                case "yn"
                    when language == LanguageCode.Manx:
                case "yr"
                    when language == LanguageCode.Welsh:

                // Non-latin script articles
                case "ο"
                    when language == LanguageCode.Greek:
                case "η"
                    when language == LanguageCode.Greek:
                case "το"
                    when language == LanguageCode.Greek:
                case "οι"
                    when language == LanguageCode.Greek:
                case "τα"
                    when language == LanguageCode.Greek:
                case "ένας"
                    when language == LanguageCode.Greek:
                case "μια"
                    when language == LanguageCode.Greek:
                case "ένα"
                    when language == LanguageCode.Greek:
                case "еден"
                    when language == LanguageCode.Macedonian:
                case "една"
                    when language == LanguageCode.Macedonian:
                case "едно"
                    when language == LanguageCode.Macedonian:
                case "едни"
                    when language == LanguageCode.Macedonian:
                case "एउटा"
                    when language == LanguageCode.Nepali:
                case "एउटी"
                    when language == LanguageCode.Nepali:
                case "एक"
                    when language == LanguageCode.Nepali:
                case "अनेक"
                    when language == LanguageCode.Nepali:
                case "कुनै"
                    when language == LanguageCode.Nepali:
                case "דער"
                    when language == LanguageCode.Yiddish:
                case "די"
                    when language == LanguageCode.Yiddish:
                case "דאָס"
                    when language == LanguageCode.Yiddish:
                case "דעם"
                    when language == LanguageCode.Yiddish:
                case "אַ"
                    when language == LanguageCode.Yiddish:
                case "אַן"
                    when language == LanguageCode.Yiddish:

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
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
                    newTitleBuilder.Append($" {segment[..^1]}, {firstItem}:");
#else
                    newTitleBuilder.Append($" {segment.Substring(0, segment.Length - 1)}, {firstItem}:");
#endif
                }
                else if (!itemInserted && segment.EndsWith("-"))
                {
                    itemInserted = true;
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER
                    newTitleBuilder.Append($" {segment[..^1]}, {firstItem}-");
#else
                    newTitleBuilder.Append($" {segment.Substring(0, segment.Length - 1)}, {firstItem}-");
#endif
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
        public static void CheckForNewVersion(out bool different, out string message, out string? url)
        {
            try
            {
                // Get current assembly version
                var assemblyVersion = Assembly.GetEntryAssembly()?.GetName()?.Version;
                if (assemblyVersion is null)
                {
                    different = false;
                    message = "Assembly version could not be determined";
                    url = null;
                    return;
                }

                string version = $"{assemblyVersion.Major}.{assemblyVersion.Minor}.{assemblyVersion.Build}";

                // Get the latest tag from GitHub
                _ = GetRemoteVersionAndUrl(out string? tag, out url);
                different = version != tag && tag is not null;
                message = $"Local version: {version}{Environment.NewLine}Remote version: {tag}";
            }
            catch
            {
                different = false;
                message = "An error occurred while checking for versions";  // ex.ToString();
                url = null;
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
                if (assembly is null)
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
        private static bool GetRemoteVersionAndUrl(out string? tag, out string? url)
        {
            tag = null; url = null;
#if NET20 || NET35 || NET40
            // Not supported in .NET Frameworks 2.0, 3.5, or 4.0
            return false;
#else
            using var hc = new System.Net.Http.HttpClient();
#if NET452
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
#endif

            // TODO: Figure out a better way than having this hardcoded...
            string releaseUrl = "https://api.github.com/repos/SabreTools/MPF/releases/latest";
            var message = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, releaseUrl);
            message.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:64.0) Gecko/20100101 Firefox/64.0");
            var latestReleaseJsonString = hc.SendAsync(message)?.ConfigureAwait(false).GetAwaiter().GetResult()
                .Content?.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            if (latestReleaseJsonString is null)
                return false;

            var latestReleaseJson = Newtonsoft.Json.Linq.JObject.Parse(latestReleaseJsonString);
            if (latestReleaseJson is null)
                return false;

            tag = latestReleaseJson["tag_name"]?.ToString();
            url = latestReleaseJson["html_url"]?.ToString();

            return true;
#endif
        }

        #endregion
    }
}
