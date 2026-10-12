using System;
using System.Windows;
using MPF.Frontend;
using MPF.Frontend.Tools;

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
        public static void SetInterfaceLanguage(ResourceDictionary resources, InterfaceLanguage lang)
        {
            // Reset the resources dictionary
            resources.MergedDictionaries.Clear();

            // Auto detect language
            if (lang == InterfaceLanguage.AutoDetect)
                lang = FrontendTool.FromCurrentCulture();

            // Set baseline language (English), required as some translations may not translate all strings
            if (lang != InterfaceLanguage.English)
            {
                var baselineDictionary = new ResourceDictionary
                {
                    Source = new Uri("../Resources/Strings.xaml", UriKind.Relative)
                };
                resources.MergedDictionaries.Add(baselineDictionary);
            }

            // Determine the XAML path and load
            string xamlPath = FrontendTool.GetLanguageXamlFilename(lang);
            var uri = new Uri($"../Resources/{xamlPath}", UriKind.Relative);
            var dictionary = new ResourceDictionary { Source = uri };

            // Merge the new language dictionary
            resources.MergedDictionaries.Add(dictionary);
        }
    }
}
