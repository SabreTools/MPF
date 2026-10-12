using System;
using System.Globalization;
using System.Windows;
using MPF.Frontend;

namespace MPF.UI
{
    /// <summary>
    /// Loads localized string resource dictionaries into the application based on interface language
    /// </summary>
    internal static class StringResourceLoader
    {
        /// <summary>
        /// Reset the merged dictionaries to the default English strings, then overlay the
        /// requested language (auto-detecting from the current culture when set to AutoDetect)
        /// </summary>
        public static void SetInterfaceLanguage(InterfaceLanguage lang)
        {
            // Reset the resources dictionary
            Application.Current.Resources.MergedDictionaries.Clear();

            // Auto detect language
            if (lang == InterfaceLanguage.AutoDetect)
                lang = FromCurrentCulture();

            // Set baseline language (English), required as some translations may not translate all strings
            if (lang != InterfaceLanguage.English)
            {
                var baselineDictionary = new ResourceDictionary
                {
                    Source = new Uri("../Resources/Strings.xaml", UriKind.Relative)
                };
                Application.Current.Resources.MergedDictionaries.Add(baselineDictionary);
            }

            var dictionary = new ResourceDictionary
            {
#pragma warning disable IDE0072
                Source = lang switch
                {
                    InterfaceLanguage.English => new Uri("../Resources/Strings.xaml", UriKind.Relative),
                    InterfaceLanguage.French => new Uri("../Resources/Strings.fr.xaml", UriKind.Relative),
                    InterfaceLanguage.German => new Uri("../Resources/Strings.de.xaml", UriKind.Relative),
                    InterfaceLanguage.Italian => new Uri("../Resources/Strings.it.xaml", UriKind.Relative),
                    InterfaceLanguage.Japanese => new Uri("../Resources/Strings.ja.xaml", UriKind.Relative),
                    InterfaceLanguage.Korean => new Uri("../Resources/Strings.ko.xaml", UriKind.Relative),
                    InterfaceLanguage.Polish => new Uri("../Resources/Strings.pl.xaml", UriKind.Relative),
                    InterfaceLanguage.Portuguese => new Uri("../Resources/Strings.pt.xaml", UriKind.Relative),
                    InterfaceLanguage.Russian => new Uri("../Resources/Strings.ru.xaml", UriKind.Relative),
                    InterfaceLanguage.Spanish => new Uri("../Resources/Strings.es.xaml", UriKind.Relative),
                    InterfaceLanguage.Swedish => new Uri("../Resources/Strings.sv.xaml", UriKind.Relative),
                    InterfaceLanguage.Ukrainian => new Uri("../Resources/Strings.uk.xaml", UriKind.Relative),
                    InterfaceLanguage.L337 => new Uri("../Resources/Strings.37.xaml", UriKind.Relative),
                    _ => new Uri("../Resources/Strings.xaml", UriKind.Relative),
                }
#pragma warning restore IDE0072
            };
            Application.Current.Resources.MergedDictionaries.Add(dictionary);
        }

        /// <summary>
        /// Map the current UI culture's two-letter ISO language name to a supported interface language
        /// </summary>
        /// TODO: Merge with Avalonia version in Frontend
        private static InterfaceLanguage FromCurrentCulture()
        {
            // Get current region code to distinguish regional variants of languages
            string region = string.Empty;
            try
            {
                // Can throw exception depending on current locale
                region = new RegionInfo(CultureInfo.CurrentUICulture.Name).TwoLetterISORegionName;
            }
            catch { }

            return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName switch
            {
                "en" => InterfaceLanguage.English,
                "fr" => InterfaceLanguage.French,
                "de" => InterfaceLanguage.German,
                "it" => InterfaceLanguage.Italian,
                "ja" => InterfaceLanguage.Japanese,
                "ko" => InterfaceLanguage.Korean,
                "pl" => InterfaceLanguage.Polish,
                "pt" => InterfaceLanguage.Portuguese,
                "ru" => InterfaceLanguage.Russian,
                "es" => InterfaceLanguage.Spanish,
                "sv" => InterfaceLanguage.Swedish,
                "uk" => InterfaceLanguage.Ukrainian,

                // TODO: Translate UI elements to Traditional Chinese
                "zh" when Array.Exists(["TW", "HK", "MO"], s => s == region) => InterfaceLanguage.English,

                // TODO: Translate UI elements to Simplified Chinese
                "zh" => InterfaceLanguage.English,

                _ => InterfaceLanguage.English,
            };
        }
    }
}
